using Autodesk.Authentication;
using Autodesk.Authentication.Model;
using Autodesk.SDKManager;

namespace Samples;

public class Authentication
{
    private readonly string? _clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
    private readonly string? _clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
    private readonly string? _redirectUri = Environment.GetEnvironmentVariable("REDIRECT_URI");
    private readonly string? _accessToken = Environment.GetEnvironmentVariable("ACCESS_TOKEN");
    private readonly string? _authorizationCode = Environment.GetEnvironmentVariable("AUTHORIZATION_CODE");
    private readonly string? _refreshToken = Environment.GetEnvironmentVariable("REFRESH_TOKEN");

    private AuthenticationClient _authenticationClient = null!;

    public void Initialize()
    {
        // Instantiate SDK manager as below.  
        SDKManager sdkManager = SdkManagerBuilder.Create().Build();

        // Instantiate AuthenticationClient using the created SDK manager
        _authenticationClient = new AuthenticationClient(sdkManager);
    }

    public async Task Get2LeggedTokenAsync()
    {
        // Get 2Legged token.
        // Pass the  client Id and secret as in your app. The method 
        // will convert it in '${Base64(<client_id>:<client_secret>)}' format
        try
        {
            TwoLeggedToken twoLeggedToken = await _authenticationClient.GetTwoLeggedTokenAsync(_clientId, _clientSecret, [Scopes.DataRead, Scopes.BucketRead]);
            string accessToken = twoLeggedToken.AccessToken;
            long? expiresAt = twoLeggedToken.ExpiresAt; // Returns the token expiry time in Unix seconds
            DateTime expiryLocalTime = DateTimeOffset.FromUnixTimeSeconds(expiresAt!.Value).LocalDateTime; // Convert Unix seconds to local time
        }
        catch (AuthenticationApiException ex)
        {
            Console.Write(ex.Message);
        }
    }

    /// <summary>
    /// Get authorize url.
    /// </summary>
    public void GetAuthorizeURL()
    {
        string url = _authenticationClient.Authorize(_clientId, ResponseType.Code, redirectUri: _redirectUri, scopes: [Scopes.DataRead, Scopes.BucketRead]);
    }

    /// <summary>
    /// Get 3Legged token. Pass the client Id and secret as in your app. The method will convert it in Basic ${Base64(<client_id>:<client_secret>)} format.
    /// </summary>
    public async Task Get3LeggedTokenAsync()
    {
        try
        {
            ThreeLeggedToken threeLeggedToken = await _authenticationClient.GetThreeLeggedTokenAsync(_clientId, _authorizationCode, _redirectUri, clientSecret: _clientSecret);
            string accessToken = threeLeggedToken.AccessToken;
        }
        catch (AuthenticationApiException ex)
        {
            Console.Write(ex.Message);
        }
    }

    /// <summary>
    /// Get refresh token.
    /// </summary>
    public async Task RefreshTokenAsync()
    {
        try
        {
            ThreeLeggedToken newToken = await _authenticationClient.RefreshTokenAsync(_refreshToken, _clientId, _clientSecret);
            string accessToken = newToken.AccessToken;

        }
        catch (AuthenticationApiException ex)
        {
            Console.Write(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves the list of public keys in the JWKS format (JSON Web Key Set).
    /// </summary>
    public async Task GetKeysAsync()
    {
        try
        {
            Jwks jwks = await _authenticationClient.GetKeysAsync();
            JwksKey jwksKey = jwks.Keys[1];
        }
        catch (AuthenticationApiException ex)
        {
            Console.Write(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves the metadata as a JSON listing of OpenID/OAuth endpoints.
    /// </summary>
    public async Task GetOidcSpecAsync()
    {
        try
        {
            OidcSpec oidcSpec = await _authenticationClient.GetOidcSpecAsync();
            string issuer = oidcSpec.Issuer;
        }
        catch (AuthenticationApiException ex)
        {
            Console.Write(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves basic information for the given authenticated user.
    /// </summary>
    public async Task GetUserInfoAsync()
    {
        try
        {
            UserInfo userInfo = await _authenticationClient.GetUserInfoAsync(_accessToken);
            string userEmail = userInfo.Email;
        }
        catch (AuthenticationApiException ex)
        {
            Console.Write(ex.Message);
        }
    }

    /// <summary>
    /// Returns the status information of the tokens.
    /// </summary>
    public async Task IntrospectTokenAsync()
    {
        try
        {
            IntrospectToken introspectToken = await _authenticationClient.IntrospectTokenAsync(_accessToken, _clientId, _clientSecret);
        }
        catch (AuthenticationApiException ex)
        {
            Console.Write(ex.Message);
        }
    }

    /// <summary>
    /// Revokes an existing access token or refresh token.
    /// </summary>
    public async Task RevokeTokenAsync()
    {
        try
        {
            HttpResponseMessage response = await _authenticationClient.RevokeAsync(_accessToken, _clientId, _clientSecret);
        }
        catch (AuthenticationApiException ex)
        {
            Console.Write(ex.Message);
        }
    }

    public void GetLogoutUrl()
    {
        string logoutUrl = _authenticationClient.Logout();
    }
}
