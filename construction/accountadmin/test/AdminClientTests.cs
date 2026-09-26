using Autodesk.Aps.TestCommon;
using Autodesk.Construction.AccountAdmin.Model;
using Autodesk.SDKManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Autodesk.Construction.AccountAdmin.Test;

[TestClass]
public class AdminClientTests
{
    private const string Scopes = "account:read";

    private static AdminClient _adminClient = null!;
    private static string _accessToken = null!;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext testContext)
    {
        Autodesk.SDKManager.SDKManager sdkManager = SdkManagerBuilder
            .Create()
            .Add(new ApsConfiguration())
            .Add(ResiliencyConfiguration.CreateDefault())
            .Build();

        _adminClient = new AdminClient(sdkManager);
        _accessToken = await ApsTestTokens.GetServiceAccountAsync(Scopes);
    }

    [TestMethod]
    public async Task GetProjectAsync_ForExistingProject_ReturnsMatchingId()
    {
        Project project = await _adminClient.GetProjectAsync(
            projectId: ApsTestConfig.AccountProjectId,
            accessToken: _accessToken);

        Assert.AreEqual(ApsTestConfig.AccountProjectId, project.Id);
    }

    [TestMethod]
    public async Task GetProjectsAsync_ForAccount_ReturnsProjectList()
    {
        ProjectsPage projectsPage = await _adminClient.GetProjectsAsync(
            accountId: ApsTestConfig.AccountId,
            accessToken: _accessToken);

        Assert.IsInstanceOfType(projectsPage.Results, typeof(List<Project>));
    }

    [TestMethod]
    public async Task GetUserProjectsAsync_ForUser_ReturnsProjectList()
    {
        UserProjectsPage userProjectsPage = await _adminClient.GetUserProjectsAsync(
            accountId: ApsTestConfig.AccountId,
            userId: ApsTestConfig.AccountUserId,
            accessToken: _accessToken);

        Assert.IsInstanceOfType(userProjectsPage.Results, typeof(List<UserProject>));
    }

    [TestMethod]
    public async Task GetUserProductsAsync_ForUser_ReturnsProductList()
    {
        ProductsPage productsPage = await _adminClient.GetUserProductsAsync(
            accountId: ApsTestConfig.AccountId,
            userId: ApsTestConfig.AccountUserId,
            accessToken: _accessToken);

        Assert.IsInstanceOfType(productsPage.Results, typeof(List<Product>));
    }

    [TestMethod]
    public async Task GetUserRolesAsync_ForUser_ReturnsRoleList()
    {
        RolesPage rolesPage = await _adminClient.GetUserRolesAsync(
            accountId: ApsTestConfig.AccountId,
            userId: ApsTestConfig.AccountUserId,
            accessToken: _accessToken);

        Assert.IsInstanceOfType(rolesPage.Results, typeof(List<Role>));
    }
}
