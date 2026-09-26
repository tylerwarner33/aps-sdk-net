using Autodesk.Aps.TestCommon;
using Autodesk.Authentication.Model;
using Autodesk.SDKManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Autodesk.Authentication.Test;

[TestClass]
public class AuthenticationClientTests
{
    private const string OfflineClientId = "offline-test-client-id";

    private static readonly List<Scopes> DefaultScopes = [Scopes.DataRead, Scopes.BucketRead];

    private static AuthenticationClient _authClient = null!;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext testContext)
    {
        Autodesk.SDKManager.SDKManager sdkManager = SdkManagerBuilder
            .Create()
            .Build();

        _authClient = new AuthenticationClient(sdkManager);

        await Task.CompletedTask;
    }

    [TestMethod]
    public async Task GetTwoLeggedTokenAsync_WithValidClientCredentials_ReturnsToken()
    {
        TwoLeggedToken twoLeggedToken = await _authClient.GetTwoLeggedTokenAsync(
            ApsTestConfig.ClientId,
            ApsTestConfig.ClientSecret,
            DefaultScopes);

        Assert.IsFalse(string.IsNullOrEmpty(twoLeggedToken.AccessToken));
        Assert.IsTrue(twoLeggedToken.ExpiresIn > 0);
    }

    /// <summary>
    /// 	Builds the authorization URL.
    /// </summary>
    /// <remarks>
    /// 	No network call is made, so a fixed client id is used and the test needs no credentials.
    /// </remarks>
    [TestMethod]
    public void Authorize_WithValidClientAndScopes_ReturnsAuthorizationUrl()
    {
        string url = _authClient.Authorize(
            OfflineClientId,
            ResponseType.Code,
            ApsTestConfig.ThreeLeggedRedirectUri,
            DefaultScopes);

        Uri authorizationUri = new(url);

        Assert.AreEqual("developer.api.autodesk.com", authorizationUri.Host);
        StringAssert.Contains(authorizationUri.Query, $"client_id={OfflineClientId}");
        StringAssert.Contains(authorizationUri.Query, "response_type=code");
    }

    /// <summary>
    /// 	Cannot run in CI.
    /// </summary>
    /// <remarks>
    /// 	Acquiring an authorization code requires a browser redirect through a signed-in user,
    /// 	which no automated runner can perform. The code is supplied out of band when a
    /// 	developer wants to exercise this path locally.
    /// </remarks>
    [TestMethod]
    public async Task GetThreeLeggedTokenAsync_WithValidAuthorizationCode_ReturnsAccessToken()
    {
        string authorizationCode = ApsTestConfig.GetString("APS_THREE_LEGGED_CODE", "");

        if (string.IsNullOrEmpty(authorizationCode))
        {
            Assert.Inconclusive(
                "No authorization code supplied. Set APS_THREE_LEGGED_CODE to exercise this test locally.");
        }

        ThreeLeggedToken threeLeggedToken = await _authClient.GetThreeLeggedTokenAsync(
            ApsTestConfig.ClientId,
            authorizationCode,
            ApsTestConfig.ThreeLeggedRedirectUri,
            ApsTestConfig.ClientSecret);

        Assert.IsNotNull(threeLeggedToken.AccessToken);
    }

    /// <summary>
    /// 	Cannot run in CI.
    /// </summary>
    /// <remarks>
    /// 	A refresh token can only be minted by completing the three-legged browser flow.
    /// 	One is supplied out of band when a developer wants to exercise this path locally.
    /// </remarks>
    [TestMethod]
    public async Task RefreshTokenAsync_WithValidRefreshToken_ReturnsNewAccessToken()
    {
        string refreshToken = ApsTestConfig.GetString("APS_THREE_LEGGED_REFRESH_TOKEN", "");

        if (string.IsNullOrEmpty(refreshToken))
        {
            Assert.Inconclusive(
                "No refresh token supplied. Set APS_THREE_LEGGED_REFRESH_TOKEN to exercise this test locally.");
        }

        ThreeLeggedToken newToken = await _authClient.RefreshTokenAsync(
            refreshToken,
            ApsTestConfig.ClientId,
            ApsTestConfig.ClientSecret);

        Assert.IsNotNull(newToken.AccessToken);
    }

    [TestMethod]
    public async Task GetKeysAsync_WhenCalled_ReturnsNonEmptyKeySet()
    {
        Jwks jwks = await _authClient.GetKeysAsync();

        Assert.IsTrue(jwks.Keys.Count > 0);
    }

    [TestMethod]
    public async Task GetOidcSpecAsync_WhenCalled_ReturnsSpec()
    {
        OidcSpec oidcSpec = await _authClient.GetOidcSpecAsync();

        Assert.IsNotNull(oidcSpec);
    }

    /// <summary>
    /// 	Cannot run in CI.
    /// </summary>
    /// <remarks>
    /// 	A three-legged access token can only be minted by completing the browser flow.
    /// 	One is supplied out of band when a developer wants to exercise this path locally.
    /// </remarks>
    [TestMethod]
    public async Task GetUserInfoAsync_WithThreeLeggedToken_ReturnsUserInfo()
    {
        string threeLeggedAccessToken = ApsTestConfig.GetString("APS_THREE_LEGGED_ACCESS_TOKEN", "");

        if (string.IsNullOrEmpty(threeLeggedAccessToken))
        {
            Assert.Inconclusive(
                "No three-legged token supplied. Set APS_THREE_LEGGED_ACCESS_TOKEN to exercise this test locally.");
        }

        UserInfo userInfo = await _authClient.GetUserInfoAsync(threeLeggedAccessToken);

        Assert.IsNotNull(userInfo);
    }

    [TestMethod]
    public async Task IntrospectTokenAsync_WithValidToken_ReturnsExpiry()
    {
        TwoLeggedToken twoLeggedToken = await _authClient.GetTwoLeggedTokenAsync(
            ApsTestConfig.ClientId,
            ApsTestConfig.ClientSecret,
            DefaultScopes);

        IntrospectToken introspectToken = await _authClient.IntrospectTokenAsync(
            twoLeggedToken.AccessToken,
            ApsTestConfig.ClientId,
            ApsTestConfig.ClientSecret);

        Assert.IsNotNull(introspectToken.Exp);
    }

    [TestMethod]
    public async Task RevokeAsync_WithValidToken_ReturnsSuccessStatus()
    {
        TwoLeggedToken twoLeggedToken = await _authClient.GetTwoLeggedTokenAsync(
            ApsTestConfig.ClientId,
            ApsTestConfig.ClientSecret,
            DefaultScopes);

        HttpResponseMessage response = await _authClient.RevokeAsync(
            twoLeggedToken.AccessToken,
            ApsTestConfig.ClientId,
            ApsTestConfig.ClientSecret);

        Assert.IsTrue(response.IsSuccessStatusCode);
    }
}
