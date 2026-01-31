using Autodesk.Construction.Issues;
using Autodesk.Construction.Issues.Model;
using Autodesk.SDKManager;

namespace Samples;

public class Issues
{
    private readonly string? _token = Environment.GetEnvironmentVariable("TOKEN");
    private readonly string? _projectId = Environment.GetEnvironmentVariable("PROJECT_ID");
    private readonly string? _issueId = Environment.GetEnvironmentVariable("ISSUE_ID");

    private IssuesClient _issuesClient = null!;

    public void Initialize()
    {
        if (string.IsNullOrEmpty(_token))
            throw new InvalidOperationException(
                $"The access token is required to initialize the {nameof(IssuesClient)}.");

        // Instantiate SDK manager as below.  
        SDKManager sdkManager = SdkManagerBuilder.Create().Build();
        // Instantiate AuthenticationClient using the created SDK manager
        //_issuesClient = new IssuesClient(sdkManager);

        StaticAuthenticationProvider staticAuthenticationProvider = new(_token);
        // Instantiate IssueClient using the auth provider
        _issuesClient = new IssuesClient(authenticationProvider: staticAuthenticationProvider);
    }

    /// <summary>
    /// Get user permission.
    /// </summary>
    public async Task GetUserInfo()
    {
        User userProfile = await _issuesClient.GetUserProfileAsync(projectId: _projectId);
    }

    /// <summary>
    /// Get issue type.
    /// </summary>
    public async Task GetIssueType()
    {
        IssueTypesPage Type = await _issuesClient.GetIssuesTypesAsync(projectId: _projectId);
    }

    /// <summary>
    /// Get issues.
    /// </summary>
    public async Task GetIssues()
    {
        IssuesPage issues = await _issuesClient.GetIssuesAsync(projectId: _projectId);
    }

    /// <summary>
    /// Get details of an issue.
    /// </summary>
    public async Task GetIssueDetail()
    {
        Issue issuedetail = await _issuesClient.GetIssueDetailsAsync(projectId: _projectId, issueId: _issueId);
    }

    /// <summary>
    /// Create issue.
    /// </summary>
    public async Task createIssue()
    {
        IssuePayload newIssue = new()
        {
            Title = "Issue Created By using SDK ",
            Description = "Created for test",
            Status = Status.Open,
            AssignedTo = "<AssignedTo>",
            AssignedToType = AssignedToType.User,
            IssueSubtypeId = "<IssueSubtypeId>",
            DueDate = "2023-10-10"
        };
        Issue createissue = await _issuesClient.CreateIssueAsync(projectId: _projectId, newIssue);
    }

    /// <summary>
    /// Create comment.
    /// </summary>
    public async Task CreateComment()
    {
        CommentsPayload newcomment = new CommentsPayload();
        newcomment.Body = "Created a Comment for testing SDK";
        Comment createComment = await _issuesClient.CreateCommentsAsync(projectId: _projectId, issueId: _issueId, commentsPayload: newcomment);
    }

    /// <summary>
    /// Get comments.
    /// </summary>
    /// <returns></returns>
    public async Task GetComments()
    {
        CommentsPage getComments = await _issuesClient.GetCommentsAsync(projectId: _projectId, issueId: _issueId);
        Console.WriteLine(getComments);
    }

    /// <summary>
    /// Get attribute definitions.
    /// </summary>
    public async Task GetAttributeDefinitions()
    {
        AttrDefinitionPage attrDefinition = await _issuesClient.GetAttributeDefinitionsAsync(projectId: _projectId);
        Console.WriteLine(attrDefinition);
    }
}
