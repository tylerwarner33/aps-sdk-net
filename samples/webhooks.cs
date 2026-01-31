using Autodesk.Oss;
using Autodesk.SDKManager;
using Autodesk.Webhooks;
using Autodesk.Webhooks.Http;
using Autodesk.Webhooks.Model;

namespace Samples;

public class Webhooks
{
    private readonly string? _token = Environment.GetEnvironmentVariable("TOKEN");
    private readonly string? _projectId = Environment.GetEnvironmentVariable("PROJECT_ID");
    private readonly string? _folderId = Environment.GetEnvironmentVariable("FOLDER_ID");
    private readonly string? _hookId = Environment.GetEnvironmentVariable("HOOK_ID");

    private WebhooksClient _webhooksClient = null!;

    public void Initialize()
    {
        if (string.IsNullOrEmpty(_token))
            throw new InvalidOperationException(
                $"The access token is required to initialize the {nameof(WebhooksClient)}.");

        // Optionally initialize SDKManager to pass custom configurations, logger, etc. 
        //SDKManager sdkManager = SdkManagerBuilder.Create().Build();

        StaticAuthenticationProvider staticAuthenticationProvider = new(_token);
        // Instantiate WebhooksClient using the auth provider
        _webhooksClient = new WebhooksClient(authenticationProvider: staticAuthenticationProvider);
    }

    /// <summary>
    /// Create post request body to receive the notification on a specified event.
    /// </summary>
    public async Task CreateSystemEventHookAsync()
    {
        HookPayload createSpecifiedEventHook = new();
        createSpecifiedEventHook.CallbackUrl = "https://example.com/callback_fifth_newest";
        createSpecifiedEventHook.Scope = new
        {
            folder = _folderId
        };

        createSpecifiedEventHook.HookExpiry = "2025-12-12T17:04:10.444Z";
        createSpecifiedEventHook.HookAttribute = new
        {
            // /* Custom metadata */
            myfoo = 76,
            projectId = _projectId,
            myobject = new
            {
                abc = true,
            }
        };

        // Add new webhook to receive the notification on a specified event.
        HttpResponseMessage createSpecifiedEventHookResponse = await _webhooksClient.CreateSystemEventHookAsync(system: Systems.Data, _event: Events.DmFolderCopied, hookPayload: createSpecifiedEventHook, region: Region.AUS);

        Console.WriteLine(createSpecifiedEventHookResponse.StatusCode);
        Console.WriteLine(createSpecifiedEventHookResponse.Content);
    }

    /// <summary>
    /// Retrieves a paginated list of all the webhooks. If the pageState query string is not specified, the first page is returned.
    /// </summary>
    public async Task GetHooksAsync()
    {
        Hooks getHooks = await _webhooksClient.GetHooksAsync(region: Region.US);
        Console.WriteLine(getHooks);
        // get hooks next link meant for pagination
        string getHooksLink = getHooks.Links.Next;
        // Get list of hooks data
        List<HookDetails> getHooksData = getHooks.Data;
        foreach (var currentHook in getHooksData)
        {
            string hook_Id = currentHook.HookId;
            string tenant = currentHook.Tenant;
            string callback_Url = currentHook.CallbackUrl;
            string created_by = currentHook.CreatedBy;
            string hook_event = currentHook.Event;
            string created_date = currentHook.CreatedDate;
            string last_updated_date = currentHook.LastUpdatedDate;
            string system_hook = currentHook.System;
            string creator_type = currentHook.CreatorType;
            Status status = currentHook.Status;
            bool? auto_reactivate_hook = currentHook.AutoReactivateHook;
            string hook_expiry = currentHook.HookExpiry;

            Console.WriteLine(hook_Id);
            Console.WriteLine(hook_event);
        }
    }

    /// <summary>
    /// Retrieves a paginated list of webhooks created in the context of a Client or Application.
    /// This API accepts 2-legged token of the application only.
    /// If the pageState query string is not specified, the first page is returned.
    /// </summary>
    public async Task GetAppHooksAsync()
    {
        Hooks getAppHooks = await _webhooksClient.GetAppHooksAsync();
        string getAppHooksLink = getAppHooks.Links.Next;
        List<HookDetails> appHooksData = getAppHooks.Data;
        foreach (var currentHook in appHooksData)
        {
            string hook_Id = currentHook.HookId;
            string tenant = currentHook.Tenant;
            string callback_Url = currentHook.CallbackUrl;
            string created_by = currentHook.CreatedBy;
            string hook_event = currentHook.Event;
            string created_date = currentHook.CreatedDate;
            string last_updated_date = currentHook.LastUpdatedDate;
            string system_hook = currentHook.System;
            string creator_type = currentHook.CreatorType;
            Status status = currentHook.Status;
            bool? auto_reactivate_hook = currentHook.AutoReactivateHook;
            string hook_expiry = currentHook.HookExpiry;
        }
    }

    /// <summary>
    /// Retrieves a paginated list of all the webhooks for a specified system.
    /// If the pageState query string is not specified, the first page is returned.
    /// </summary>
    public async Task GetSystemHooksAsync()
    {
        Hooks getSystemHooks = await _webhooksClient.GetSystemHooksAsync(system: "data", status: StatusFilter.Active);
        Console.WriteLine(getSystemHooks);
        string getSystemHooksLink = getSystemHooks.Links.Next;
        List<HookDetails> getSystemHooksData = getSystemHooks.Data;
        foreach (var currentHook in getSystemHooksData)
        {
            string hook_Id = currentHook.HookId;
            string tenant = currentHook.Tenant;
            string callback_Url = currentHook.CallbackUrl;
            string created_by = currentHook.CreatedBy;
            string hook_event = currentHook.Event;
            string created_date = currentHook.CreatedDate;
            string last_updated_date = currentHook.LastUpdatedDate;
            string system_hook = currentHook.System;
            string creator_type = currentHook.CreatorType;
            Status status = currentHook.Status;
            bool? auto_reactivate_hook = currentHook.AutoReactivateHook;
            string hook_expiry = currentHook.HookExpiry;
        }
    }

    /// <summary>
    /// Retrieves a paginated list of all the webhooks for a specified event.
    /// If the pageState query string is not specified, the first page is returned.
    /// </summary>
    public async Task GetSystemEventHooksAsync()
    {
        Hooks getSystemEventHooks = await _webhooksClient.GetSystemEventHooksAsync(system: Systems.Data, _event: Events.DmFolderCopied, status: StatusFilter.Active);
        Console.WriteLine(getSystemEventHooks);
        string getSystemEventHooksLink = getSystemEventHooks.Links.Next;
        List<HookDetails> getSystemEventHooksData = getSystemEventHooks.Data;
        foreach (var currentHook in getSystemEventHooksData)
        {
            string hook_Id = currentHook.HookId;
            string tenant = currentHook.Tenant;
            string callback_Url = currentHook.CallbackUrl;
            string created_by = currentHook.CreatedBy;
            string hook_event = currentHook.Event;
            string created_date = currentHook.CreatedDate;
            string last_updated_date = currentHook.LastUpdatedDate;
            string system_hook = currentHook.System;
            string creator_type = currentHook.CreatorType;
            Status status = currentHook.Status;
            bool? auto_reactivate_hook = currentHook.AutoReactivateHook;
            string hook_expiry = currentHook.HookExpiry;
        }
    }

    /// <summary>
    /// Get details of a webhook based on its webhook ID.
    /// </summary>
    public async Task GetHookDetailsAsync()
    {
        HookDetails getSystemEventHook = await _webhooksClient.GetHookDetailsAsync(system: Systems.Derivative, _event: Events.DmVersionAdded, hookId: _hookId);
        string callbackUrl = getSystemEventHook.CallbackUrl;
        Console.WriteLine(getSystemEventHook);
    }


    /// <summary>
    /// Create update hook request body.
    /// </summary>
    public async Task PatchSystemEventHookAsync()
    {
        ModifyHookPayload updateHook = new()
        {
            Status = StatusRequest.Inactive
        };

        // Successful deactivation of a webhook:
        HttpResponseMessage updateHookResponse = await _webhooksClient.PatchSystemEventHookAsync(system: Systems.Data, _event: Events.DmVersionAdded, modifyHookPayload: updateHook, hookId: _hookId);
        Console.WriteLine(updateHookResponse.StatusCode);
        Console.WriteLine(updateHookResponse.Content);
    }

    public async Task DeleteSystemEventHookAsync()
    {
        HttpResponseMessage deleteHookResponse = await _webhooksClient.DeleteSystemEventHookAsync(system: Systems.Data, _event: Events.DmVersionAdded, hookId: _hookId);
        Console.WriteLine(deleteHookResponse.StatusCode);
    }

    /// <summary>
    /// Create post request body to receive the notification on all the events.
    /// </summary>
    public async Task CreateSystemHookAsync()
    {
        HookPayload createAllEventsHook = new()
        {
            CallbackUrl = "<callbackUrl>",
            Scope = new
            {
                workflow = "<my-workflow-id>",
            }
        };

        // Add new webhooks to receive the notification on all the events.
        Hook createAllEventsHooks = await _webhooksClient.CreateSystemHookAsync(system: Systems.Derivative, hookPayload: createAllEventsHook, accessToken: _token);
        List<HookDetails> allEventsHooks = createAllEventsHooks.Hooks;
        foreach (var currentHook in allEventsHooks)
        {
            string hook_Id = currentHook.HookId;
            string tenant = currentHook.Tenant;
            string callback_Url = currentHook.CallbackUrl;
            string created_by = currentHook.CreatedBy;
            string hook_event = currentHook.Event;
            string created_date = currentHook.CreatedDate;
            string last_updated_date = currentHook.LastUpdatedDate;
            string system_hook = currentHook.System;
            string creator_type = currentHook.CreatorType;
            Status status = currentHook.Status;
            bool? auto_reactivate_hook = currentHook.AutoReactivateHook;
            string urn = currentHook.Urn;
            string _self = currentHook.Self;

            HookDetailsScope scope = currentHook.Scope;
            string folderId = scope.Folder;

            Console.WriteLine(hook_Id);
            Console.WriteLine(hook_event);
        }
    }

    public async Task CreateTokenAsync()
    {
        TokenPayload tokenPayload = new()
        {
            Token = _token
        };

        Token createdToken = await _webhooksClient.CreateTokenAsync(tokenPayload: tokenPayload);
        Console.WriteLine(createdToken.Status);
        Console.WriteLine(createdToken);
    }

    public async Task PutTokenAsync()
    {
        TokenPayload tokenPayload = new()
        {
            Token = _token
        };

        HttpResponseMessage httpResponseMessage = await _webhooksClient.PutTokenAsync(tokenPayload: tokenPayload);
        Console.WriteLine(httpResponseMessage.StatusCode);
        Console.WriteLine(httpResponseMessage.Content);
    }


    public async Task DeleteTokenAsync()
    {
        HttpResponseMessage httpResponseMessage = await _webhooksClient.DeleteTokenAsync();
        Console.WriteLine(httpResponseMessage.StatusCode);
        Console.WriteLine(httpResponseMessage.Content);
    }
}
