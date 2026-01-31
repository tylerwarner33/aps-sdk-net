using Samples;

// Uncomment the initializer and desired methods in a region to run samples.

Console.WriteLine("Starting Autodesk SDK Samples...");

DotNetEnv.Env.Load();

#region Account Admin Samples

AccountAdmin admin = new();
admin.Initialize();

await admin.GetProjects();
await admin.GetProject();
await admin.UpdateProjectImage();
await admin.CreateProject();
await admin.GetCompanies();
await admin.GetCompany();
await admin.SearchCompany();
await admin.GetProjectCompanies();
await admin.CreateCompany();
await admin.GetCompaniesWithPagination();
await admin.ImportCompanies();
await admin.UpdateCompany();
await admin.UpdateCompanyImage();
await admin.GetUsers();
await admin.GetUser();
await admin.CreateUser();
await admin.ImportUsers();
await admin.UpdateUser();
await admin.getUserProducts();
await admin.GetUserRoles();
await admin.GetProjectUsers();
await admin.GetUserProjects();
await admin.GetProjectUser();
await admin.AssignProjectUser();
await admin.ImportProjectUsers();
await admin.UpdateProjectUser();
await admin.DeleteProjectUser();
await admin.GetBusinessUnits();
await admin.PutBusinessUnits();

#endregion

#region Authentication Samples

Authentication authentication = new();
authentication.Initialize();

await authentication.Get2LeggedTokenAsync();
authentication.GetAuthorizeURL();
await authentication.Get3LeggedTokenAsync();
await authentication.RefreshTokenAsync();
await authentication.GetOidcSpecAsync();
await authentication.GetKeysAsync();
authentication.GetLogoutUrl();

#endregion

#region Data Management Samples

DataManagement dataManagement = new();
dataManagement.Initialize();

/* Hubs */
await dataManagement.GetHubsAsync();
await dataManagement.GetHubAsync();

/* Projects */
await dataManagement.GetHubProjectsAsync();
await dataManagement.GetProjectAsync();
await dataManagement.GetProjectHubAsync();
await dataManagement.GetProjectTopFoldersAsync();
await dataManagement.GetDownloadAsync();
await dataManagement.GetDownloadJobAsync();
await dataManagement.CreateDownloadAsync();
await dataManagement.CreateStorageAsync();

/* Folders */
await dataManagement.GetFolderAsync();
await dataManagement.GetFolderContentsAsync();
await dataManagement.GetFolderParentAsync();
await dataManagement.GetFolderRefsAsync();
await dataManagement.GetFolderRelationshipsLinksAsync();
await dataManagement.GetFolderRelationshipsRefsAsync();
await dataManagement.GetFolderSearchAsync();
await dataManagement.CreateFolderAsync();
await dataManagement.CreateFolderRelationshipsRefAsync();
await dataManagement.PatchFolderAsync();

/* Items */
await dataManagement.GetItemAsync();
await dataManagement.GetItemParentFolderAsync();
await dataManagement.GetItemRefsAsync();
await dataManagement.GetItemRelationshipsLinksAsync();
await dataManagement.GetItemRelationshipsRefsAsync();
await dataManagement.GetItemTipAsync();
await dataManagement.GetItemVersionsAsync();
await dataManagement.CreateItemAsync();
await dataManagement.CreateItemRelationshipsRefAsync();
await dataManagement.PatchItemAsync();

/* Versions */
await dataManagement.GetVersionAsync();
await dataManagement.GetVersionDownloadFormatsAsync();
await dataManagement.GetVersionDownloadsAsync();
await dataManagement.GetVersionItemAsync();
await dataManagement.GetVersionRefsAsync();
await dataManagement.GetVersionRelationshipsLinksAsync();
await dataManagement.GetVersionRelationshipsRefsAsync();
await dataManagement.CreateVersionAsync();
await dataManagement.CreateVersionRelationshipsRefAsync();
await dataManagement.PatchVersionAsync();

/* Commands */
await dataManagement.ExecuteCheckPermissionCommandAsync();
await dataManagement.ExecuteListRefsCommandAsync();
await dataManagement.ExecuteListItemsCommandAsync();
await dataManagement.ExecuteGetPublishModelJobAsync();
await dataManagement.ExecutePublishModelAsync();
await dataManagement.ExecutePublishWithoutLinksAsync();

#endregion

#region Issues Samples

Issues issues = new();
issues.Initialize();

await issues.GetUserInfo();
await issues.GetIssueType();
await issues.GetIssues();
await issues.GetIssueDetail();
await issues.CreateComment();
await issues.GetComments();
await issues.GetAttributeDefinitions();

#endregion

#region Model Derivative Samples

ModelDerivative modelDerivative = new();
modelDerivative.Initialize();

/* Informational */
await modelDerivative.GetFormatsAsync();

/* Jobs */
await modelDerivative.StartJobAsync();

/* Manifest */
await modelDerivative.DeleteManifestAsync();
await modelDerivative.GetManifestAsync();

/* Dervivatives */
await modelDerivative.DownloadDerivativeURLAsync();
await modelDerivative.GetDerivativeHeadersAsync();

/* Thumbnails */
await modelDerivative.GetThumbnailAsync();

/* Metadata */
await modelDerivative.GetModelViewsAsync();
await modelDerivative.GetObjectTreeAsync();
await modelDerivative.GetAllPropertiesAsync();
await modelDerivative.GetSpecificPropertiesAsync();

#endregion

#region Oss Samples

Oss oss = new();
oss.Initialize();

/* Buckets */
await oss.CreateBucketAsync();
await oss.DeleteBucketAsync();
await oss.GetBucketsAsync();
await oss.GetBucketDetailsAsync();

/* Objects */
await oss.BatchSignedS3UploadAsync();
await oss.CopyToAsync();
await oss.DeleteObjectAsync();
await oss.UploadObjectAsync();
await oss.GetObjectDetailsAsync();
await oss.GetObjectsAsync();
await oss.SignedS3DownloadAsync();
await oss.DownloadObjectAsync();

/* Signed Resources */
await oss.GetSignedResourceAsync();
await oss.CreateSignedResourceAsync();

#endregion

#region Webhooks Samples

Webhooks webhooks = new();
webhooks.Initialize();

/* Hooks */
await webhooks.CreateSystemEventHookAsync();
await webhooks.CreateSystemHookAsync();
await webhooks.DeleteSystemEventHookAsync();
await webhooks.GetAppHooksAsync();
await webhooks.GetHookDetailsAsync();
await webhooks.GetHooksAsync();
await webhooks.GetSystemEventHooksAsync();
await webhooks.GetSystemHooksAsync();
await webhooks.PatchSystemEventHookAsync();

/* Tokens */
await webhooks.CreateTokenAsync();
await webhooks.DeleteTokenAsync();
await webhooks.PutTokenAsync();

#endregion

Console.WriteLine("Please uncomment the sample you want to run in Program.cs");
Console.ReadLine();
