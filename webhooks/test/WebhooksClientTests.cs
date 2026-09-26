using System.Net;
using Autodesk.Aps.TestCommon;
using Autodesk.SDKManager;
using Autodesk.Webhooks.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Autodesk.Webhooks.Test;

/// <summary>
/// 	Integration tests for <see cref="WebhooksClient"/>.
/// </summary>
/// <remarks>
/// 	Every hook here is a Model Derivative <c>extraction.finished</c> hook scoped by
/// 	<c>WEBHOOKS_WORKFLOW_ID</c>. A workflow scope is valid only on Model Derivative events.
/// 	A Data Management event needs a folder scope instead.
/// 	The service rejects a second hook with the same event, scope, and callback URL, so every
/// 	hook gets its own callback URL through a unique query value.
/// 	Every hook points at <c>APS_WEBHOOK_CALLBACK_URL</c>, which only CI uses, so a hook left by
/// 	an aborted run is recognised by that prefix and removed before the class runs.
/// </remarks>
[TestClass]
public class WebhooksClientTests
{
    private const string CallbackMarker = "ci-hook";

    private static WebhooksClient _webhooksClient = null!;
    private static string _accessToken = null!;
    private static Region _region;
    private static string _sharedHookId = null!;

    /// <summary>
    /// 	Removes residue, then creates the Tier 1 fixture: one hook shared by every read-only test.
    /// </summary>
    [ClassInitialize]
    public static async Task ClassInitialize(TestContext testContext)
    {
        Autodesk.SDKManager.SDKManager sdkManager = SdkManagerBuilder
            .Create()
            .Add(new ApsConfiguration())
            .Add(ResiliencyConfiguration.CreateDefault())
            .Build();

        _webhooksClient = new WebhooksClient(sdkManager);
        _accessToken = await ApsTestTokens.GetTwoLeggedAsync();
        _region = Enum.Parse<Region>(ApsTestConfig.Region, ignoreCase: true);

        await DeleteLeftoverHooksAsync();

        _sharedHookId = await CreateHookAsync();
    }

    /// <summary>
    /// 	Releases the Tier 1 fixture.
    /// </summary>
    /// <remarks>
    /// 	This assembly holds one test class, so the end of the class and the end of the assembly are
    /// 	the same point on MSTest 3 and 4.
    /// 	No <c>ClassCleanupBehavior</c> is passed, because MSTest 4 removes that type.
    /// </remarks>
    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        if (_sharedHookId is not null)
        {
            await DeleteHookIfPresentAsync(_sharedHookId);
        }
    }

    #region Hooks

    [TestMethod]
    public async Task GetHookDetailsAsync_ForExistingHook_ReturnsMatchingId()
    {
        HookDetails hookDetails = await _webhooksClient.GetHookDetailsAsync(
            system: Systems.Derivative,
            _event: Events.ExtractionFinished,
            hookId: _sharedHookId,
            region: _region,
            accessToken: _accessToken);

        Assert.AreEqual(_sharedHookId, hookDetails.HookId);
    }

    [TestMethod]
    public async Task GetSystemEventHooksAsync_ForEventWithHooks_ReturnsHookList()
    {
        Hooks hooks = await _webhooksClient.GetSystemEventHooksAsync(
            system: Systems.Derivative,
            _event: Events.ExtractionFinished,
            region: _region,
            accessToken: _accessToken);

        Assert.IsTrue(hooks.Data.Count > 0);
    }

    [TestMethod]
    public async Task GetSystemHooksAsync_ForSystemWithHooks_ReturnsHookList()
    {
        Hooks hooks = await _webhooksClient.GetSystemHooksAsync(
            system: Systems.Derivative,
            region: _region,
            accessToken: _accessToken);

        Assert.IsTrue(hooks.Data.Count > 0);
    }

    [TestMethod]
    public async Task GetHooksAsync_WhenHooksExist_ReturnsHookList()
    {
        Hooks hooks = await _webhooksClient.GetHooksAsync(region: _region, accessToken: _accessToken);

        Assert.IsTrue(hooks.Data.Count > 0);
    }

    [TestMethod]
    public async Task GetAppHooksAsync_ForConfiguredRegion_ReturnsHookList()
    {
        Hooks hooks = await _webhooksClient.GetAppHooksAsync(region: _region, accessToken: _accessToken);

        Assert.IsTrue(hooks.Data.Count > 0);
    }

    [TestMethod]
    public async Task CreateSystemEventHookAsync_WithValidPayload_ReturnsCreated()
    {
        HttpResponseMessage response = await _webhooksClient.CreateSystemEventHookAsync(
            system: Systems.Derivative,
            _event: Events.ExtractionFinished,
            hookPayload: BuildHookPayload(),
            region: _region,
            accessToken: _accessToken);

        try
        {
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }
        finally
        {
            if (response.Headers.Location is not null)
            {
                await DeleteHookIfPresentAsync(ReadHookId(response.Headers.Location));
            }
        }
    }

    /// <summary>
    /// 	Creates one hook for every Model Derivative event.
    /// </summary>
    /// <remarks>
    /// 	Each created hook is deleted through its own system and event.
    /// 	Deleting them all through one event would leave every other hook behind.
    /// </remarks>
    [TestMethod]
    public async Task CreateSystemHookAsync_WithValidPayload_ReturnsHooks()
    {
        Hook hook = await _webhooksClient.CreateSystemHookAsync(
            system: Systems.Derivative,
            hookPayload: BuildHookPayload(),
            region: _region,
            accessToken: _accessToken);

        try
        {
            Assert.IsTrue(hook.Hooks.Count > 0);
        }
        finally
        {
            foreach (HookDetails createdHook in hook.Hooks)
            {
                await _webhooksClient.DeleteSystemEventHookAsync(
                    system: createdHook.System,
                    _event: createdHook.Event,
                    hookId: createdHook.HookId,
                    region: _region,
                    accessToken: _accessToken,
                    throwOnError: false);
            }
        }
    }

    [TestMethod]
    public async Task PatchSystemEventHookAsync_SettingStatusInactive_ReturnsNoContent()
    {
        string hookId = await CreateHookAsync();

        try
        {
            HttpResponseMessage response = await _webhooksClient.PatchSystemEventHookAsync(
                system: Systems.Derivative,
                _event: Events.ExtractionFinished,
                hookId: hookId,
                modifyHookPayload: new ModifyHookPayload { Status = StatusRequest.Inactive },
                region: _region,
                accessToken: _accessToken);

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }
        finally
        {
            await DeleteHookIfPresentAsync(hookId);
        }
    }

    [TestMethod]
    public async Task DeleteSystemEventHookAsync_ForExistingHook_ReturnsNoContent()
    {
        string hookId = await CreateHookAsync();

        HttpResponseMessage response = await _webhooksClient.DeleteSystemEventHookAsync(
            system: Systems.Derivative,
            _event: Events.ExtractionFinished,
            hookId: hookId,
            region: _region,
            accessToken: _accessToken);

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    #endregion

    #region Secret token

    // The secret token applies to every hook the application owns.
    // Run these tests only with an APS application that is dedicated to CI.
    // Each test removes any token first, so the order of the tests does not matter.

    [TestMethod]
    public async Task CreateTokenAsync_WithValidSecret_ReturnsStatus200()
    {
        await DeleteTokenIfPresentAsync();

        try
        {
            Token token = await _webhooksClient.CreateTokenAsync(
                tokenPayload: new TokenPayload { Token = ApsTestConfig.WebhookSecretToken },
                region: _region,
                accessToken: _accessToken);

            Assert.AreEqual(200m, token.Status);
        }
        finally
        {
            await DeleteTokenIfPresentAsync();
        }
    }

    [TestMethod]
    public async Task PutTokenAsync_WithNewSecret_ReturnsNoContent()
    {
        await DeleteTokenIfPresentAsync();

        try
        {
            await _webhooksClient.CreateTokenAsync(
                tokenPayload: new TokenPayload { Token = ApsTestConfig.WebhookSecretToken },
                region: _region,
                accessToken: _accessToken);

            HttpResponseMessage response = await _webhooksClient.PutTokenAsync(
                tokenPayload: new TokenPayload { Token = ApsTestConfig.WebhookSecretToken },
                region: _region,
                accessToken: _accessToken);

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }
        finally
        {
            await DeleteTokenIfPresentAsync();
        }
    }

    [TestMethod]
    public async Task DeleteTokenAsync_WhenTokenExists_ReturnsNoContent()
    {
        await DeleteTokenIfPresentAsync();
        await _webhooksClient.CreateTokenAsync(
            tokenPayload: new TokenPayload { Token = ApsTestConfig.WebhookSecretToken },
            region: _region,
            accessToken: _accessToken);

        HttpResponseMessage response = await _webhooksClient.DeleteTokenAsync(
            region: _region,
            accessToken: _accessToken);

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    #endregion

    #region Arrange and cleanup helpers

    /// <summary>
    /// 	Builds a hook payload with a callback URL that no other hook uses.
    /// </summary>
    private static HookPayload BuildHookPayload()
    {
        UriBuilder callback = new(ApsTestConfig.WebhookCallbackUrl);
        string marker = $"{CallbackMarker}={Guid.NewGuid():N}";
        callback.Query = string.IsNullOrEmpty(callback.Query) ? marker : $"{callback.Query.TrimStart('?')}&{marker}";

        return new HookPayload
        {
            CallbackUrl = callback.Uri.AbsoluteUri,
            Scope = new
            {
                workflow = ApsTestConfig.WebhooksWorkflowId
            }
        };
    }

    private static async Task<string> CreateHookAsync()
    {
        HttpResponseMessage response = await _webhooksClient.CreateSystemEventHookAsync(
            system: Systems.Derivative,
            _event: Events.ExtractionFinished,
            hookPayload: BuildHookPayload(),
            region: _region,
            accessToken: _accessToken);

        Uri location = response.Headers.Location
            ?? throw new AssertFailedException("The create hook response carried no Location header.");

        return ReadHookId(location);
    }

    private static string ReadHookId(Uri location)
    {
        string path = location.IsAbsoluteUri ? location.AbsolutePath : location.OriginalString;

        return path[(path.LastIndexOf('/') + 1)..];
    }

    private static async Task DeleteHookIfPresentAsync(string hookId) =>
        await _webhooksClient.DeleteSystemEventHookAsync(
            system: Systems.Derivative,
            _event: Events.ExtractionFinished,
            hookId: hookId,
            region: _region,
            accessToken: _accessToken,
            throwOnError: false);

    private static async Task DeleteTokenIfPresentAsync() =>
        await _webhooksClient.DeleteTokenAsync(
            region: _region,
            accessToken: _accessToken,
            throwOnError: false);

    /// <summary>
    /// 	Deletes every Model Derivative hook that an earlier run left on the CI callback URL.
    /// </summary>
    /// <remarks>
    /// 	Reads the first page only.
    /// 	A hook that points at any other callback URL is never touched.
    /// </remarks>
    private static async Task DeleteLeftoverHooksAsync()
    {
        string callbackPrefix = new Uri(ApsTestConfig.WebhookCallbackUrl).GetLeftPart(UriPartial.Path);

        Hooks hooks = await _webhooksClient.GetSystemHooksAsync(
            system: Systems.Derivative,
            region: _region,
            accessToken: _accessToken);

        foreach (HookDetails hook in hooks.Data ?? [])
        {
            bool isCiHook = hook.CallbackUrl is not null
                && hook.CallbackUrl.StartsWith(callbackPrefix, StringComparison.Ordinal)
                && hook.CallbackUrl.Contains($"{CallbackMarker}=", StringComparison.Ordinal);

            if (isCiHook)
            {
                await _webhooksClient.DeleteSystemEventHookAsync(
                    system: hook.System,
                    _event: hook.Event,
                    hookId: hook.HookId,
                    region: _region,
                    accessToken: _accessToken,
                    throwOnError: false);
            }
        }
    }

    #endregion
}
