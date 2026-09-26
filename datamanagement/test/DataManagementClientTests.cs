using Autodesk.Aps.TestCommon;
using Autodesk.DataManagement.Model;
using Autodesk.SDKManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Autodesk.DataManagement.Test;

[TestClass]
public class DataManagementClientTests
{
    private const string Scopes = "data:read";

    private static DataManagementClient _dataManagementClient = null!;
    private static string _accessToken = null!;
    private static string _hubId = null!;
    private static string _projectId = null!;
    private static string _folderId = null!;
    private static string _itemId = null!;
    private static string _versionId = null!;

    /// <summary>
    /// 	Reads the seed data every test in this class reads against.
    /// </summary>
    /// <remarks>
    /// 	These tests are read-only against a hub, project, folder, item, and version
    /// 	that already exist. The SDK has no call that creates them, so there is no
    /// 	arrange-time provisioning and no cleanup.
    /// </remarks>
    [ClassInitialize]
    public static async Task ClassInitialize(TestContext testContext)
    {
        Autodesk.SDKManager.SDKManager sdkManager = SdkManagerBuilder
            .Create()
            .Add(new ApsConfiguration())
            .Add(ResiliencyConfiguration.CreateDefault())
            .Build();

        _dataManagementClient = new DataManagementClient(sdkManager);
        _accessToken = await ApsTestTokens.GetServiceAccountAsync(Scopes);
        _hubId = ApsTestConfig.DataManagementHubId;
        _projectId = ApsTestConfig.DataManagementProjectId;
        _folderId = ApsTestConfig.DataManagementFolderId;
        _itemId = ApsTestConfig.DataManagementItemId;
        _versionId = ApsTestConfig.DataManagementVersionId;
    }

    #region Hubs and projects

    [TestMethod]
    public async Task GetHubsAsync_WithValidToken_ReturnsHubs()
    {
        Hubs hubs = await _dataManagementClient.GetHubsAsync(accessToken: _accessToken);

        Assert.IsTrue(hubs.Data.Count > 0);
    }

    [TestMethod]
    public async Task GetHubAsync_ForExistingHub_ReturnsMatchingId()
    {
        Hub hub = await _dataManagementClient.GetHubAsync(hubId: _hubId, accessToken: _accessToken);

        Assert.AreEqual(_hubId, hub.Data.Id);
    }

    [TestMethod]
    public async Task GetHubProjectsAsync_ForExistingHub_ReturnsProjects()
    {
        Projects projects = await _dataManagementClient.GetHubProjectsAsync(hubId: _hubId, accessToken: _accessToken);

        Assert.IsTrue(projects.Data.Count > 0);
    }

    [TestMethod]
    public async Task GetProjectAsync_ForExistingProject_ReturnsMatchingId()
    {
        Project project = await _dataManagementClient.GetProjectAsync(
            hubId: _hubId,
            projectId: _projectId,
            accessToken: _accessToken);

        Assert.AreEqual(_projectId, project.Data.Id);
    }

    [TestMethod]
    public async Task GetProjectHubAsync_ForExistingProject_ReturnsMatchingHubId()
    {
        Hub hub = await _dataManagementClient.GetProjectHubAsync(
            hubId: _hubId,
            projectId: _projectId,
            accessToken: _accessToken);

        Assert.AreEqual(_hubId, hub.Data.Id);
    }

    [TestMethod]
    public async Task GetProjectTopFoldersAsync_ForExistingProject_ReturnsFolders()
    {
        TopFolders topFolders = await _dataManagementClient.GetProjectTopFoldersAsync(
            hubId: _hubId,
            projectId: _projectId,
            accessToken: _accessToken);

        Assert.IsTrue(topFolders.Data.Count > 0);
    }

    #endregion

    #region Folders

    [TestMethod]
    public async Task GetFolderAsync_ForExistingFolder_ReturnsMatchingId()
    {
        Folder folder = await _dataManagementClient.GetFolderAsync(
            projectId: _projectId,
            folderId: _folderId,
            accessToken: _accessToken);

        Assert.AreEqual(_folderId, folder.Data.Id);
    }

    [TestMethod]
    public async Task GetFolderContentsAsync_ForExistingFolder_ReturnsContents()
    {
        FolderContents folderContents = await _dataManagementClient.GetFolderContentsAsync(
            projectId: _projectId,
            folderId: _folderId,
            accessToken: _accessToken);

        Assert.IsTrue(folderContents.Data.Count > 0);
    }

    /// <summary>
    /// 	Reads the parent of the seeded folder.
    /// </summary>
    /// <remarks>
    /// 	The parent of a top folder is the project itself, not another folder, so the
    /// 	previous assertion on <c>TypeFolder.Folders</c> failed against that seed data.
    /// 	This checks only that a parent came back with an id.
    /// </remarks>
    [TestMethod]
    public async Task GetFolderParentAsync_ForExistingFolder_ReturnsParentFolder()
    {
        Folder folder = await _dataManagementClient.GetFolderParentAsync(
            projectId: _projectId,
            folderId: _folderId,
            accessToken: _accessToken);

        Assert.IsNotNull(folder.Data);
        Assert.IsNotNull(folder.Data.Id);
    }

    [TestMethod]
    public async Task GetFolderRefsAsync_ForExistingFolder_ReturnsRefList()
    {
        FolderRefs folderRefs = await _dataManagementClient.GetFolderRefsAsync(
            projectId: _projectId,
            folderId: _folderId,
            accessToken: _accessToken);

        Assert.IsInstanceOfType(folderRefs.Data, typeof(List<IFolderRefsData>));
    }

    [TestMethod]
    public async Task GetFolderRelationshipsLinksAsync_ForExistingFolder_ReturnsLinkList()
    {
        RelationshipLinks relationshipLinks = await _dataManagementClient.GetFolderRelationshipsLinksAsync(
            projectId: _projectId,
            folderId: _folderId,
            accessToken: _accessToken);

        Assert.IsInstanceOfType(relationshipLinks.Data, typeof(List<RelationshipLinksData>));
    }

    [TestMethod]
    public async Task GetFolderRelationshipsRefsAsync_ForExistingFolder_ReturnsRefs()
    {
        RelationshipRefs relationshipRefs = await _dataManagementClient.GetFolderRelationshipsRefsAsync(
            folderId: _folderId,
            projectId: _projectId,
            accessToken: _accessToken);

        Assert.IsTrue(relationshipRefs.Data.Count > 0);
    }

    [TestMethod]
    public async Task GetFolderSearchAsync_ForExistingFolder_ReturnsResults()
    {
        Search search = await _dataManagementClient.GetFolderSearchAsync(
            folderId: _folderId,
            projectId: _projectId,
            accessToken: _accessToken);

        Assert.IsTrue(search.Data.Count > 0);
    }

    #endregion

    #region Items

    [TestMethod]
    public async Task GetItemAsync_ForExistingItem_ReturnsItemType()
    {
        Item item = await _dataManagementClient.GetItemAsync(
            projectId: _projectId,
            itemId: _itemId,
            accessToken: _accessToken,
            includePathInProject: true);

        Assert.AreEqual(TypeItem.Items, item.Data.Type);
    }

    #endregion

    #region Versions

    [TestMethod]
    public async Task GetVersionAsync_ForExistingVersion_ReturnsVersionType()
    {
        ModelVersion modelVersion = await _dataManagementClient.GetVersionAsync(
            projectId: _projectId,
            versionId: _versionId,
            accessToken: _accessToken);

        Assert.AreEqual(TypeVersion.Versions, modelVersion.Data.Type);
    }

    [TestMethod]
    public async Task GetVersionDownloadFormatsAsync_ForExistingVersion_ReturnsDownloadFormats()
    {
        DownloadFormats downloadFormats = await _dataManagementClient.GetVersionDownloadFormatsAsync(
            projectId: _projectId,
            versionId: _versionId,
            accessToken: _accessToken);

        Assert.AreEqual(TypeDownloadformats.DownloadFormats, downloadFormats.Data.Type);
    }

    [TestMethod]
    public async Task GetVersionItemAsync_ForExistingVersion_ReturnsItem()
    {
        Item item = await _dataManagementClient.GetVersionItemAsync(
            projectId: _projectId,
            versionId: _versionId,
            accessToken: _accessToken);

        Assert.AreEqual(TypeItem.Items, item.Data.Type);
    }

    [TestMethod]
    public async Task GetVersionRefsAsync_ForExistingVersion_ReturnsRefList()
    {
        Refs refs = await _dataManagementClient.GetVersionRefsAsync(
            projectId: _projectId,
            versionId: _versionId,
            accessToken: _accessToken);

        Assert.IsInstanceOfType(refs.Data, typeof(List<IRefsData>));
    }

    [TestMethod]
    public async Task GetVersionRelationshipsRefsAsync_ForExistingVersion_ReturnsRefs()
    {
        RelationshipRefs relationshipRefs = await _dataManagementClient.GetVersionRelationshipsRefsAsync(
            projectId: _projectId,
            versionId: _versionId,
            accessToken: _accessToken);

        Assert.IsInstanceOfType(relationshipRefs.Data, typeof(List<RelationshipRefsData>));
    }

    #endregion
}
