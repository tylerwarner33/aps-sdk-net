using System.Net.Http;
using System.Runtime.CompilerServices;
using Autodesk.Aps.TestCommon;
using Autodesk.SDKManager;
using Autodesk.SecureServiceAccount.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Autodesk.SecureServiceAccount.Test;

[TestClass]
public class SecureServiceAccountClientTests
{
    private const string ManagementScopes =
        "application:service_account:write application:service_account:read " +
        "application:service_account_key:write application:service_account_key:read";

    private static SecureServiceAccountClient _ssaClient = null!;
    private static string _accessToken = null!;

    private string? _createdServiceAccountId;
    private string? _createdKeyId;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext testContext)
    {
        Autodesk.SDKManager.SDKManager sdkManager = SdkManagerBuilder
            .Create()
            .Add(new ApsConfiguration())
            .Add(ResiliencyConfiguration.CreateDefault())
            .Build();

        _ssaClient = new SecureServiceAccountClient(sdkManager);
        _accessToken = await ApsTestTokens.GetTwoLeggedAsync(ManagementScopes);
    }

    /// <summary>
    /// 	Releases the resources the test created, one test at a time.
    /// </summary>
    /// <remarks>
    /// 	An application may hold at most 10 service accounts, and 3 keys per account.
    /// 	Deferring cleanup to the end of the class would exhaust that quota mid-run.
    /// 	Deleting the service account also deletes any key still attached to it, so the key
    /// 	delete here is a best-effort call, not the only path to a clean key.
    /// </remarks>
    [TestCleanup]
    public async Task TestCleanup()
    {
        if (_createdServiceAccountId is not null && _createdKeyId is not null)
        {
            await _ssaClient.DeleteServiceAccountKeyAsync(
                _createdServiceAccountId,
                _createdKeyId,
                accessToken: _accessToken,
                throwOnError: false);
        }

        if (_createdServiceAccountId is not null)
        {
            await _ssaClient.DeleteServiceAccountAsync(
                _createdServiceAccountId,
                accessToken: _accessToken,
                throwOnError: false);
        }
    }

    #region AccountManagementApi Tests

    [TestMethod]
    public async Task CreateServiceAccountAsync_WithValidPayload_ReturnsServiceAccount()
    {
        string serviceAccountName = ApsTestNaming.NewServiceAccountName();

        ServiceAccount serviceAccount = await _ssaClient.CreateServiceAccountAsync(
            new CreateServiceAccountPayload { Name = serviceAccountName },
            accessToken: _accessToken);
        _createdServiceAccountId = serviceAccount.ServiceAccountId;

        Assert.IsNotNull(serviceAccount.ServiceAccountId);
    }

    [TestMethod]
    public async Task DeleteServiceAccountAsync_ForExistingAccount_ReturnsSuccessStatus()
    {
        string serviceAccountId = await CreateTestServiceAccountAsync();

        HttpResponseMessage response = await _ssaClient.DeleteServiceAccountAsync(
            serviceAccountId,
            accessToken: _accessToken);

        Assert.IsTrue(response.IsSuccessStatusCode);
    }

    [TestMethod]
    public async Task EnableServiceAccountAsync_ForExistingAccount_ReturnsDetails()
    {
        string serviceAccountId = await CreateTestServiceAccountAsync();
        await _ssaClient.DisableServiceAccountAsync(serviceAccountId, accessToken: _accessToken);

        ServiceAccountDetails details = await _ssaClient.EnableServiceAccountAsync(
            serviceAccountId,
            accessToken: _accessToken);

        Assert.AreEqual("ENABLED", details.Status, ignoreCase: true);
    }

    [TestMethod]
    public async Task DisableServiceAccountAsync_ForExistingAccount_ReturnsDetails()
    {
        string serviceAccountId = await CreateTestServiceAccountAsync();

        ServiceAccountDetails details = await _ssaClient.DisableServiceAccountAsync(
            serviceAccountId,
            accessToken: _accessToken);

        Assert.AreEqual("DISABLED", details.Status, ignoreCase: true);
    }

    [TestMethod]
    public async Task GetServiceAccountAsync_ForExistingAccount_ReturnsDetails()
    {
        string serviceAccountId = await CreateTestServiceAccountAsync();

        ServiceAccountDetails details = await _ssaClient.GetServiceAccountAsync(
            serviceAccountId,
            accessToken: _accessToken);

        Assert.AreEqual(serviceAccountId, details.ServiceAccountId);
    }

    [TestMethod]
    public async Task GetAllServiceAccountsAsync_WhenAccountsExist_ReturnsAccounts()
    {
        await CreateTestServiceAccountAsync();

        ServiceAccounts serviceAccounts = await _ssaClient.GetAllServiceAccountsAsync(accessToken: _accessToken);

        Assert.IsTrue(serviceAccounts.ServiceAccountsList.Count > 0);
    }

    #endregion AccountManagementApi Tests

    #region ExchangeTokenApi Tests

    /// <summary>
    /// 	Builds the assertion with the shared <c>Utils.GenerateJwtAssertion</c> helper.
    /// </summary>
    /// <remarks>
    /// 	The claim set has to match what the token endpoint expects for this service account,
    /// 	so the reference implementation is reused rather than reassembled here.
    /// </remarks>
    [TestMethod]
    public async Task ExchangeJwtAssertionAsync_WithValidAssertion_ReturnsAccessToken()
    {
        string serviceAccountId = await CreateTestServiceAccountAsync();
        ServiceAccountPrivateKey key = await CreateTestServiceAccountKeyAsync(serviceAccountId);
        string assertion = Utils.GenerateJwtAssertion(
            key.Kid,
            key.PrivateKey,
            ApsTestConfig.ClientId,
            serviceAccountId,
            [Scopes.DataRead]);

        ExchangeJwtToken exchangeJwtToken = await _ssaClient.ExchangeJwtAssertionAsync(
            assertion: assertion,
            clientId: ApsTestConfig.ClientId,
            clientSecret: ApsTestConfig.ClientSecret,
            scope: [Scopes.DataRead]);

        Assert.IsNotNull(exchangeJwtToken.AccessToken);
    }

    #endregion ExchangeTokenApi Tests

    #region KeyManagementApi Tests

    [TestMethod]
    public async Task CreateServiceAccountKeyAsync_ForExistingAccount_ReturnsPrivateKey()
    {
        string serviceAccountId = await CreateTestServiceAccountAsync();

        ServiceAccountPrivateKey privateKey = await _ssaClient.CreateServiceAccountKeyAsync(
            serviceAccountId,
            accessToken: _accessToken);
        _createdKeyId = privateKey.Kid;

        Assert.IsFalse(string.IsNullOrEmpty(privateKey.PrivateKey));
    }

    [TestMethod]
    public async Task DeleteServiceAccountKeyAsync_ForExistingKey_ReturnsSuccessStatus()
    {
        string serviceAccountId = await CreateTestServiceAccountAsync();
        ServiceAccountPrivateKey key = await CreateTestServiceAccountKeyAsync(serviceAccountId);

        HttpResponseMessage response = await _ssaClient.DeleteServiceAccountKeyAsync(
            serviceAccountId,
            key.Kid,
            accessToken: _accessToken);

        Assert.IsTrue(response.IsSuccessStatusCode);
    }

    [TestMethod]
    public async Task EnableServiceAccountKeyAsync_ForExistingKey_ReturnsSuccessStatus()
    {
        string serviceAccountId = await CreateTestServiceAccountAsync();
        ServiceAccountPrivateKey key = await CreateTestServiceAccountKeyAsync(serviceAccountId);
        await _ssaClient.DisableServiceAccountKeyAsync(serviceAccountId, key.Kid, accessToken: _accessToken);

        HttpResponseMessage response = await _ssaClient.EnableServiceAccountKeyAsync(
            serviceAccountId,
            key.Kid,
            accessToken: _accessToken);

        Assert.IsTrue(response.IsSuccessStatusCode);
    }

    [TestMethod]
    public async Task DisableServiceAccountKeyAsync_ForExistingKey_ReturnsSuccessStatus()
    {
        string serviceAccountId = await CreateTestServiceAccountAsync();
        ServiceAccountPrivateKey key = await CreateTestServiceAccountKeyAsync(serviceAccountId);

        HttpResponseMessage response = await _ssaClient.DisableServiceAccountKeyAsync(
            serviceAccountId,
            key.Kid,
            accessToken: _accessToken);

        Assert.IsTrue(response.IsSuccessStatusCode);
    }

    [TestMethod]
    public async Task GetAllServiceAccountKeysAsync_ForExistingAccount_ReturnsKeys()
    {
        string serviceAccountId = await CreateTestServiceAccountAsync();
        await CreateTestServiceAccountKeyAsync(serviceAccountId);

        ServiceAccountKeys keys = await _ssaClient.GetAllServiceAccountKeysAsync(
            serviceAccountId,
            accessToken: _accessToken);

        Assert.IsTrue(keys.Keys.Count > 0);
    }

    #endregion KeyManagementApi Tests

    #region Arrange helpers

    private async Task<string> CreateTestServiceAccountAsync([CallerMemberName] string testName = "")
    {
        ServiceAccount serviceAccount = await _ssaClient.CreateServiceAccountAsync(
            new CreateServiceAccountPayload { Name = ApsTestNaming.NewServiceAccountName(testName) },
            accessToken: _accessToken);
        _createdServiceAccountId = serviceAccount.ServiceAccountId;

        return _createdServiceAccountId;
    }

    private async Task<ServiceAccountPrivateKey> CreateTestServiceAccountKeyAsync(string serviceAccountId)
    {
        ServiceAccountPrivateKey key = await _ssaClient.CreateServiceAccountKeyAsync(
            serviceAccountId,
            accessToken: _accessToken);
        _createdKeyId = key.Kid;

        return key;
    }

    #endregion Arrange helpers
}
