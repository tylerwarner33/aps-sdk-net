using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace Autodesk.Aps.TestCommon;

/// <summary>
/// 	Derives the names of every cloud resource the tests create.
/// </summary>
/// <remarks>
/// 	OSS bucket keys share one global namespace across every APS tenant, so a naive key such as
/// 	<c>apssdk-ci-{clientId}</c> would publish the client id into a namespace anyone can probe.
/// 	Every name here is a hash of the test name plus a tenant salt.
/// 	A persistent name is deterministic, which caps the count of shared fixtures at one per fixture.
/// 	A name for a resource that a test deletes also carries a run id, so it cannot conflict with
/// 	a deleted resource that the service still holds.
/// 	In replay the run id is constant, so a recording still matches the request URI exactly.
/// </remarks>
public static class ApsTestNaming
{
    private static string Prefix => ApsTestConfig.GetString("OSS_BUCKET_PREFIX", "apssdk-ci");

    private static string Salt => ApsTestConfig.RequireString("APS_TEST_BUCKET_SALT");

    /// <summary>
    /// 	Constant in CI and in replay, so recordings match and the bucket count stays at one per test.
    /// </summary>
    /// <remarks>
    /// 	Per machine only for a live run on a developer box, which no concurrency group can reach.
    /// </remarks>
    private static string Scope =>
        ApsTestConfig.IsReplay || ApsTestConfig.IsCi
            ? "ci"
            : $"dev-{ShortHash(Environment.MachineName)}";

    /// <summary>
    /// 	Identifies one test run.
    /// </summary>
    /// <remarks>
    /// 	The GitHub run id and attempt in CI, a constant in replay, and a random value on a developer machine.
    /// 	Lowercase letters and digits only, so it is valid in every resource name derived here.
    /// </remarks>
    private static string RunId => _runId.Value;

    private static readonly Lazy<string> _runId = new(() =>
    {
        if (ApsTestConfig.IsReplay)
        {
            return "replay";
        }

        string runNumber = ApsTestConfig.GetString("GITHUB_RUN_ID", "");

        if (ApsTestConfig.IsCi && runNumber.Length > 0)
        {
            return $"r{runNumber}a{ApsTestConfig.GetString("GITHUB_RUN_ATTEMPT", "1")}";
        }

        return Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
    });

    /// <summary>
    /// 	Derives the OSS bucket key for the calling test.
    /// </summary>
    /// <remarks>
    /// 	Use this key for a bucket that persists between runs, ex. a shared read-only fixture.
    /// 	Do not use it for a bucket that a test deletes. See <see cref="NewRunScopedBucketKey"/>.
    /// </remarks>
    public static string NewBucketKey([CallerMemberName] string testName = "") =>
        $"{Prefix}-{Scope}-{ShortHash(testName + Salt)}".ToLowerInvariant();

    /// <summary>
    /// 	Derives an OSS bucket key for the calling test that is unique to this run.
    /// </summary>
    /// <remarks>
    /// 	Use this key for a bucket that a test creates and deletes.
    /// 	OSS can keep a deleted key unavailable for some time, so a constant key can fail the next run.
    /// 	The bucket uses the transient policy, so a bucket left by an aborted run holds no data for long.
    /// </remarks>
    public static string NewRunScopedBucketKey([CallerMemberName] string testName = "") =>
        $"{NewBucketKey(testName)}-{RunId}";

    /// <summary>
    /// 	Derives an OSS object key for the calling test.
    /// </summary>
    public static string NewObjectKey(string suffix = "object", [CallerMemberName] string testName = "") =>
        $"{ShortHash(testName + Salt)}-{suffix}.dat".ToLowerInvariant();

    /// <summary>
    /// 	Derives the title of an ACC issue created by the calling test.
    /// </summary>
    /// <remarks>
    /// 	An ACC issue cannot be deleted through the API, only closed.
    /// 	The <c>CI</c> prefix marks the residue so a project owner can recognise it.
    /// </remarks>
    public static string NewIssueTitle([CallerMemberName] string testName = "") =>
        $"CI {testName} {ShortHash(testName + Salt)}";

    /// <summary>
    /// 	Derives the name of a service account created by the calling test.
    /// </summary>
    /// <remarks>
    /// 	Unique to this run.
    /// 	The service account list does not return the name, so an account left by an aborted run
    /// 	cannot be found by name and removed first.
    /// 	A run-scoped name cannot conflict with that account.
    /// </remarks>
    public static string NewServiceAccountName([CallerMemberName] string testName = "") =>
        $"ci{ShortHash(testName + Salt)}{RunId}";

    private static string ShortHash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))[..8].ToLowerInvariant();
}
