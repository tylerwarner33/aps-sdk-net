using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Autodesk.Aps.TestCommon;

/// <summary>
/// 	Generates APS access tokens at run time from GitHub secrets.
/// </summary>
/// <remarks>
/// 	No test stores a token. A stored token expires in one hour and is a credential in a file.
/// 	Tokens are cached in memory and refreshed 60 seconds before expiry.
/// </remarks>
public static class ApsTestTokens
{
    private const string TokenEndpoint = "https://developer.api.autodesk.com/authentication/v2/token";
    private const string JwtBearerGrantType = "urn:ietf:params:oauth:grant-type:jwt-bearer";
    private const int AssertionLifetimeSeconds = 300;

    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };
    private static readonly SemaphoreSlim _gate = new(1, 1);
    private static readonly Dictionary<string, CachedToken> _cache = [];

    /// <summary>
    /// 	Returns a two-legged token for the supplied scopes.
    /// </summary>
    public static Task<string> GetTwoLeggedAsync(params string[] scopes)
    {
        string scope = JoinScopes(scopes);

        return GetOrCreateAsync($"two-legged:{scope}", () =>
        {
            Dictionary<string, string> form = new()
            {
                ["grant_type"] = "client_credentials",
                ["scope"] = scope
            };

            return RequestTokenAsync(ApsTestConfig.ClientId, ApsTestConfig.ClientSecret, form);
        });
    }

    /// <summary>
    /// 	Returns a service account token for the supplied scopes.
    /// </summary>
    /// <remarks>
    /// 	Builds a signed JWT assertion and exchanges it.
    /// 	The claim set matches <c>Autodesk.SecureServiceAccount.Utils.GenerateJwtAssertion</c>,
    /// 	which is the reference implementation.
    /// </remarks>
    public static Task<string> GetServiceAccountAsync(params string[] scopes)
    {
        string scope = JoinScopes(scopes);

        return GetOrCreateAsync($"service-account:{scope}", () =>
        {
            string assertion = BuildAssertion(scope);

            Dictionary<string, string> form = new()
            {
                ["grant_type"] = JwtBearerGrantType,
                ["assertion"] = assertion,
                ["scope"] = scope
            };

            return RequestTokenAsync(
                ApsTestConfig.ServiceAccountClientId,
                ApsTestConfig.ServiceAccountClientSecret,
                form);
        });
    }

    private static string JoinScopes(string[] scopes) =>
        scopes.Length == 0 ? "data:read" : string.Join(' ', scopes);

    private static async Task<string> GetOrCreateAsync(string key, Func<Task<CachedToken>> factory)
    {
        await _gate.WaitAsync();

        try
        {
            if (_cache.TryGetValue(key, out CachedToken cached) && cached.IsUsable)
            {
                return cached.AccessToken;
            }

            CachedToken issued = await factory();
            _cache[key] = issued;

            return issued.AccessToken;
        }
        finally
        {
            _gate.Release();
        }
    }

    private static string BuildAssertion(string scope)
    {
        using RSA rsa = RSA.Create();
        rsa.ImportFromPem(ApsTestConfig.ServiceAccountPrivateKey.ToCharArray());

        // The default factory caches the signature provider by key id, and that provider holds this RSA
        // instance. The instance is disposed when this method returns, so the second assertion for a
        // new scope set would fail with ObjectDisposedException. Do not cache.
        RsaSecurityKey securityKey = new(rsa)
        {
            KeyId = ApsTestConfig.ServiceAccountKeyId,
            CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
        };

        SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.RsaSha256);

        List<Claim> claims =
        [
            new Claim("iss", ApsTestConfig.ServiceAccountClientId),
            new Claim("sub", ApsTestConfig.ServiceAccountId),
            new Claim("aud", TokenEndpoint),
            new Claim(
                "scope",
                JsonSerializer.Serialize(scope.Split(' ', StringSplitOptions.RemoveEmptyEntries)),
                JsonClaimValueTypes.JsonArray)
        ];

        JwtSecurityToken token = new(
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(AssertionLifetimeSeconds),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static async Task<CachedToken> RequestTokenAsync(
        string clientId,
        string clientSecret,
        Dictionary<string, string> form)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(form)
        };

        string credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        using HttpResponseMessage response = await _httpClient.SendAsync(request);
        string body = await response.Content.ReadAsStringAsync();

        // The credentials were present, so a rejection is a real defect, not missing configuration.
        // It fails the test. An inconclusive result would hide an expired or revoked secret.
        if (response.IsSuccessStatusCode is false)
        {
            throw new AssertFailedException(
                $"The APS token endpoint returned {(int)response.StatusCode} ({DescribeError(body)}). " +
                "Check the client credentials in the aps-integration-tests environment.");
        }

        using JsonDocument document = JsonDocument.Parse(body);
        JsonElement root = document.RootElement;

        string accessToken = root.GetProperty("access_token").GetString()
            ?? throw new AssertFailedException("The APS token response carried no access_token.");

        int expiresIn = root.TryGetProperty("expires_in", out JsonElement expiry) ? expiry.GetInt32() : 3600;

        return new CachedToken(accessToken, DateTimeOffset.UtcNow.AddSeconds(expiresIn));
    }

    /// <summary>
    /// 	Reads only the documented error fields from a token endpoint error response.
    /// </summary>
    /// <remarks>
    /// 	The message goes to the CI log and to the uploaded test results.
    /// 	The raw response body is never copied there.
    /// </remarks>
    private static string DescribeError(string body)
    {
        string[] fieldNames = ["error", "errorCode", "error_description", "developerMessage"];

        try
        {
            using JsonDocument document = JsonDocument.Parse(body);

            if (document.RootElement.ValueKind is not JsonValueKind.Object)
            {
                return "no error detail";
            }

            List<string> details = [];

            foreach (string fieldName in fieldNames)
            {
                if (document.RootElement.TryGetProperty(fieldName, out JsonElement field)
                    && field.ValueKind is JsonValueKind.String)
                {
                    details.Add($"{fieldName}: {field.GetString()}");
                }
            }

            return details.Count > 0 ? string.Join(", ", details) : "no error detail";
        }
        catch (JsonException)
        {
            return "the response was not JSON";
        }
    }

    private readonly record struct CachedToken(string AccessToken, DateTimeOffset ExpiresAt)
    {
        internal bool IsUsable => DateTimeOffset.UtcNow < ExpiresAt.AddSeconds(-60);
    }
}
