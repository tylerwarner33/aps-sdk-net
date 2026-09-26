using System.Net;
using Autodesk.Aps.TestCommon;
using Autodesk.Construction.Issues.Model;
using Autodesk.SDKManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Autodesk.Construction.Issues.Test;

[TestClass]
public class IssuesClientTests
{
    private const string Scopes = "data:read data:write data:create account:read account:write";

    private static IssuesClient _issuesClient = null!;
    private static string _accessToken = null!;
    private static Region _region;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext testContext)
    {
        Autodesk.SDKManager.SDKManager sdkManager = SdkManagerBuilder
            .Create()
            .Add(new ApsConfiguration())
            .Add(ResiliencyConfiguration.CreateDefault())
            .Build();

        _issuesClient = new IssuesClient(sdkManager);
        _accessToken = await ApsTestTokens.GetServiceAccountAsync(Scopes);
        _region = Enum.Parse<Region>(ApsTestConfig.Region, ignoreCase: true);
    }

    [TestMethod]
    public async Task GetUserProfileAsync_ForProject_ReturnsUserId()
    {
        User userProfile = await _issuesClient.GetUserProfileAsync(
            projectId: ApsTestConfig.IssuesProjectId,
            xAdsRegion: _region,
            accessToken: _accessToken);

        Assert.IsNotNull(userProfile.Id);
    }

    [TestMethod]
    public async Task GetIssuesTypesAsync_ForProject_ReturnsPagination()
    {
        IssueTypesPage issueTypesPage = await _issuesClient.GetIssuesTypesAsync(
            projectId: ApsTestConfig.IssuesProjectId,
            xAdsRegion: _region,
            accessToken: _accessToken);

        Assert.IsNotNull(issueTypesPage.Pagination);
    }

    [TestMethod]
    public async Task GetIssuesAsync_ForProject_ReturnsPagination()
    {
        IssuesPage issuesPage = await _issuesClient.GetIssuesAsync(
            projectId: ApsTestConfig.IssuesProjectId,
            xAdsRegion: _region,
            accessToken: _accessToken);

        Assert.IsNotNull(issuesPage.Pagination);
    }

    [TestMethod]
    public async Task GetIssueDetailsAsync_ForExistingIssue_ReturnsId()
    {
        Issue createdIssue = await CreateIssueAsync();

        try
        {
            Issue issue = await _issuesClient.GetIssueDetailsAsync(
                projectId: ApsTestConfig.IssuesProjectId,
                issueId: createdIssue.Id,
                xAdsRegion: _region,
                accessToken: _accessToken);

            Assert.IsNotNull(issue.Id);
        }
        finally
        {
            await CloseIssueAsync(createdIssue.Id);
        }
    }

    [TestMethod]
    public async Task CreateIssueAsync_WithValidPayload_ReturnsCreatedIssueId()
    {
        IssuePayload newIssue = BuildIssuePayload();

        Issue createdIssue = await _issuesClient.CreateIssueAsync(
            projectId: ApsTestConfig.IssuesProjectId,
            issuePayload: newIssue,
            xAdsRegion: _region,
            accessToken: _accessToken);

        try
        {
            Assert.IsNotNull(createdIssue.Id);
        }
        finally
        {
            await CloseIssueAsync(createdIssue.Id);
        }
    }

    [TestMethod]
    public async Task CreateCommentsAsync_WithValidPayload_ReturnsCommentId()
    {
        Issue createdIssue = await CreateIssueAsync();

        try
        {
            CommentsPayload newComment = new()
            {
                Body = "CI comment for automated test coverage"
            };

            Comment createdComment = await _issuesClient.CreateCommentsAsync(
                projectId: ApsTestConfig.IssuesProjectId,
                issueId: createdIssue.Id,
                xAdsRegion: _region,
                commentsPayload: newComment,
                accessToken: _accessToken);

            Assert.IsNotNull(createdComment.Id);
        }
        finally
        {
            await CloseIssueAsync(createdIssue.Id);
        }
    }

    [TestMethod]
    public async Task GetCommentsAsync_ForExistingIssue_ReturnsPagination()
    {
        Issue createdIssue = await CreateIssueAsync();

        try
        {
            await CreateCommentAsync(createdIssue.Id);

            CommentsPage commentsPage = await _issuesClient.GetCommentsAsync(
                projectId: ApsTestConfig.IssuesProjectId,
                issueId: createdIssue.Id,
                xAdsRegion: _region,
                accessToken: _accessToken);

            Assert.IsNotNull(commentsPage.Pagination);
        }
        finally
        {
            await CloseIssueAsync(createdIssue.Id);
        }
    }

    [TestMethod]
    public async Task GetAttributeDefinitionsAsync_ForProject_ReturnsPagination()
    {
        AttrDefinitionPage attrDefinitionPage = await _issuesClient.GetAttributeDefinitionsAsync(
            projectId: ApsTestConfig.IssuesProjectId,
            xAdsRegion: _region,
            accessToken: _accessToken);

        Assert.IsNotNull(attrDefinitionPage.Pagination);
    }

    [TestMethod]
    public async Task GetAttachmentsAsync_ForExistingIssue_ReturnsAttachments()
    {
        Issue createdIssue = await CreateIssueAsync();

        try
        {
            await CreateAttachmentAsync(createdIssue.Id);

            Attachments attachments = await _issuesClient.GetAttachmentsAsync(
                projectId: ApsTestConfig.IssuesProjectId,
                issueId: createdIssue.Id,
                accessToken: _accessToken);

            Assert.IsNotNull(attachments);
        }
        finally
        {
            await CloseIssueAsync(createdIssue.Id);
        }
    }

    [TestMethod]
    public async Task DeleteAttachmentAsync_ForExistingAttachment_ReturnsNoContent()
    {
        Issue createdIssue = await CreateIssueAsync();

        try
        {
            string attachmentId = await CreateAttachmentAsync(createdIssue.Id);

            HttpResponseMessage response = await _issuesClient.DeleteAttachmentAsync(
                projectId: ApsTestConfig.IssuesProjectId,
                issueId: createdIssue.Id,
                attachmentId: attachmentId,
                accessToken: _accessToken);

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }
        finally
        {
            await CloseIssueAsync(createdIssue.Id);
        }
    }

    [TestMethod]
    public async Task AddAttachmentsAsync_WithValidPayload_ReturnsAttachments()
    {
        Issue createdIssue = await CreateIssueAsync();

        try
        {
            AttachmentsPayload newAttachment = BuildAttachmentsPayload(createdIssue.Id);

            Attachments createdAttachments = await _issuesClient.AddAttachmentsAsync(
                projectId: ApsTestConfig.IssuesProjectId,
                attachmentsPayload: newAttachment,
                accessToken: _accessToken);

            Assert.IsNotNull(createdAttachments);
        }
        finally
        {
            await CloseIssueAsync(createdIssue.Id);
        }
    }

    #region Arrange and cleanup helpers

    private static IssuePayload BuildIssuePayload() => new()
    {
        Title = ApsTestNaming.NewIssueTitle(),
        Description = "Created by an automated test run.",
        Status = Status.Open,
        AssignedTo = ApsTestConfig.IssuesAssigneeUserId,
        AssignedToType = AssignedToType.User,
        IssueSubtypeId = ApsTestConfig.IssuesIssueSubtypeId
    };

    private static AttachmentsPayload BuildAttachmentsPayload(string issueId)
    {
        string attachmentId = Guid.NewGuid().ToString();

        return new AttachmentsPayload
        {
            DomainEntityId = issueId,
            Attachments =
            [
                new AttachmentObject
                {
                    AttachmentId = attachmentId,
                    FileName = $"{attachmentId}.txt",
                    DisplayName = "ci-attachment.txt",
                    AttachmentType = "issue-attachment",
                    StorageUrn = $"urn:adsk.objects:os.object:ci-attachments/{attachmentId}.txt"
                }
            ]
        };
    }

    private static async Task<Issue> CreateIssueAsync() =>
        await _issuesClient.CreateIssueAsync(
            projectId: ApsTestConfig.IssuesProjectId,
            issuePayload: BuildIssuePayload(),
            xAdsRegion: _region,
            accessToken: _accessToken);

    private static async Task<Comment> CreateCommentAsync(string issueId) =>
        await _issuesClient.CreateCommentsAsync(
            projectId: ApsTestConfig.IssuesProjectId,
            issueId: issueId,
            xAdsRegion: _region,
            commentsPayload: new CommentsPayload { Body = "CI comment for automated test coverage" },
            accessToken: _accessToken);

    /// <summary>
    /// 	Adds one attachment to the supplied issue and returns the attachment id.
    /// </summary>
    /// <remarks>
    /// 	The attachment id is client-generated, so the caller already knows it. It is returned only
    /// 	for readability at the call site.
    /// </remarks>
    private static async Task<string> CreateAttachmentAsync(string issueId)
    {
        AttachmentsPayload payload = BuildAttachmentsPayload(issueId);

        await _issuesClient.AddAttachmentsAsync(
            projectId: ApsTestConfig.IssuesProjectId,
            attachmentsPayload: payload,
            accessToken: _accessToken);

        return payload.Attachments[0].AttachmentId;
    }

    /// <summary>
    /// 	Closes the issue created by a test, instead of deleting it.
    /// </summary>
    /// <remarks>
    /// 	An ACC issue cannot be deleted through the API, only closed. Every issue created by this
    /// 	class is titled through <see cref="ApsTestNaming.NewIssueTitle"/>, which prefixes the title
    /// 	with <c>CI</c> so a project owner can recognise the residue.
    /// </remarks>
    private static async Task CloseIssueAsync(string issueId) =>
        await _issuesClient.PatchIssueDetailsAsync(
            projectId: ApsTestConfig.IssuesProjectId,
            issueId: issueId,
            issuePayload: new IssuePayload { Status = Status.Closed },
            xAdsRegion: _region,
            accessToken: _accessToken);

    #endregion
}
