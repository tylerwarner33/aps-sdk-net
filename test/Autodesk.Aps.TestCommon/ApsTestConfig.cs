using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Autodesk.Aps.TestCommon;

/// <summary>
/// 	Reads every secret and variable the test suites need.
/// </summary>
/// <remarks>
/// 	A missing value gives an inconclusive result on a developer machine and in an unconfigured
/// 	CI environment, because the real cause is missing configuration, not a defect in the code
/// 	under test.
/// 	A missing value in a configured CI environment fails the test instead.
/// 	See <see cref="IsConfiguredCi"/>.
/// </remarks>
public static class ApsTestConfig
{
    static ApsTestConfig() => EnvBootstrap.Ensure();

    /// <summary>
    /// 	True when the tests run on a GitHub Actions runner.
    /// </summary>
    public static bool IsCi =>
        string.Equals(Environment.GetEnvironmentVariable("CI"), "true", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 	True when the tests run in CI and the GitHub environment carries client credentials.
    /// </summary>
    /// <remarks>
    /// 	A configured environment that is missing one value is a configuration defect.
    /// 	An inconclusive result there would give a green build that did not run its tests.
    /// 	So in this state a missing value fails the test.
    /// </remarks>
    public static bool IsConfiguredCi =>
        IsCi && (HasValue("APS_CLIENT_ID") || HasValue("APS_SSA_CLIENT_ID"));

    /// <summary>
    /// 	True when the tests replay recorded HTTP traffic instead of calling the live service.
    /// </summary>
    /// <remarks>
    /// 	Reserved for the replay lane. Nothing sets it yet.
    /// </remarks>
    public static bool IsReplay =>
        string.Equals(Environment.GetEnvironmentVariable("APS_TEST_REPLAY"), "true", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 	Reads an optional value and falls back to the supplied default.
    /// </summary>
    public static string GetString(string name, string fallback)
    {
        string? value = Environment.GetEnvironmentVariable(name);

        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    /// <summary>
    /// 	Reads a required value.
    /// </summary>
    /// <exception cref="AssertInconclusiveException">
    /// 	Thrown when the value is missing or empty, outside a configured CI environment.
    /// </exception>
    /// <exception cref="AssertFailedException">
    /// 	Thrown when the value is missing or empty, in a configured CI environment.
    /// </exception>
    public static string RequireString(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);

        if (string.IsNullOrWhiteSpace(value))
        {
            string message =
                $"The environment value '{name}' is not set. " +
                "Set it in the aps-integration-tests GitHub environment, or in a local .env file. " +
                "See .github/CI-SECRETS.md.";

            if (IsConfiguredCi)
            {
                throw new AssertFailedException(message);
            }

            throw new AssertInconclusiveException(message);
        }

        return value;
    }

    private static bool HasValue(string name) =>
        string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name)) is false;

    // Two-legged client credentials.
    public static string ClientId => RequireString("APS_CLIENT_ID");
    public static string ClientSecret => RequireString("APS_CLIENT_SECRET");

    // Service account credentials.
    public static string ServiceAccountClientId => RequireString("APS_SSA_CLIENT_ID");
    public static string ServiceAccountClientSecret => RequireString("APS_SSA_CLIENT_SECRET");
    public static string ServiceAccountId => RequireString("APS_SSA_SERVICE_ACCOUNT_ID");
    public static string ServiceAccountKeyId => RequireString("APS_SSA_KEY_ID");
    public static string ServiceAccountPrivateKey => RequireString("APS_SSA_PRIVATE_KEY");

    // Shared variables.
    public static string Region => GetString("APS_REGION", "US");

    // Three-legged flow. Must match a callback URL registered on the APS app.
    public static string ThreeLeggedRedirectUri => GetString("APS_THREE_LEGGED_REDIRECT_URI", "https://localhost/callback");

    // Webhooks.
    public static string WebhookCallbackUrl => RequireString("APS_WEBHOOK_CALLBACK_URL");
    public static string WebhookSecretToken => RequireString("APS_WEBHOOK_SECRET_TOKEN");
    public static string WebhooksWorkflowId => RequireString("WEBHOOKS_WORKFLOW_ID");

    // Model Derivative.
    public static string ModelDerivativeSourceUrn => RequireString("MD_SOURCE_URN");
    public static string ModelDerivativeTranslatedUrn => RequireString("MD_TRANSLATED_URN");

    /// <summary>
    /// 	Switches off the one test that consumes cloud credits.
    /// </summary>
    /// <remarks>
    /// 	Translation is the only billed call in the whole suite.
    /// </remarks>
    public static bool RunTranslationTest =>
        string.Equals(GetString("MD_RUN_TRANSLATION_TEST", "true"), "true", StringComparison.OrdinalIgnoreCase);

    // Data Management seed data.
    public static string DataManagementHubId => RequireString("DM_HUB_ID");
    public static string DataManagementProjectId => RequireString("DM_PROJECT_ID");
    public static string DataManagementFolderId => RequireString("DM_FOLDER_ID");
    public static string DataManagementItemId => RequireString("DM_ITEM_ID");
    public static string DataManagementVersionId => RequireString("DM_VERSION_ID");

    // Account Admin seed data.
    public static string AccountId => RequireString("ACC_ACCOUNT_ID");
    public static string AccountProjectId => RequireString("ACC_PROJECT_ID");
    public static string AccountUserId => RequireString("ACC_USER_ID");

    // Issues seed data.
    public static string IssuesProjectId => RequireString("ISSUES_PROJECT_ID");
    public static string IssuesIssueSubtypeId => RequireString("ISSUES_ISSUE_SUBTYPE_ID");
    public static string IssuesAssigneeUserId => RequireString("ISSUES_ASSIGNEE_USER_ID");
}
