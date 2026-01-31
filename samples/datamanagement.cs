using Autodesk.DataManagement;
using Autodesk.DataManagement.Model;
using Autodesk.SDKManager;
using Newtonsoft.Json;

namespace Samples;

public class DataManagement
{
    private readonly string? _token = Environment.GetEnvironmentVariable("TOKEN");
    private readonly string? _folderId = Environment.GetEnvironmentVariable("FOLDER_ID");
    private readonly string? _hubId = Environment.GetEnvironmentVariable("HUB_ID");
    private readonly string? _projectId = Environment.GetEnvironmentVariable("PROJECT_ID");
    private readonly string? _downloadId = Environment.GetEnvironmentVariable("DOWNLOAD_ID");
    private readonly string? _jobId = Environment.GetEnvironmentVariable("JOB_ID");
    private readonly string? _itemId = Environment.GetEnvironmentVariable("ITEM_ID");
    private readonly string? _versionId = Environment.GetEnvironmentVariable("VERSION_ID");
    private readonly string? _storageUrn = Environment.GetEnvironmentVariable("STORAGE_URN");

    private DataManagementClient _dataManagementClient = null!;

    public void Initialize()
    {
        if (string.IsNullOrEmpty(_token))
            throw new InvalidOperationException(
                $"The access token is required to initialize the {nameof(DataManagementClient)}.");

        StaticAuthenticationProvider staticAuthenticationProvider = new(_token);
        _dataManagementClient = new DataManagementClient(authenticationProvider: staticAuthenticationProvider);
    }

    #region Hubs

    public async Task GetHubsAsync()
    {
        List<string> filter_id = ["b.a4f95080-84fe-4281-8d0a-bd8c885695e0"];
        List<string> filter_name = ["Autodesk Forge Partner Development"];
        List<string> filter_extension_type = ["hubs:autodesk.bim360:Account"];

        Hubs hubs = await _dataManagementClient.GetHubsAsync(filterId: filter_id, filterName: filter_name, filterExtensionType: filter_extension_type);

        List<HubData> hubsData = hubs.Data;
        foreach (var hub in hubsData)
        {
            TypeHub hubsType = hub.Type;
            string HubsId = hub.Id;

            Console.WriteLine(hubsType);

            Console.WriteLine(HubsId);
            Console.WriteLine(hub.Attributes.Name);
            Region region = hub.Attributes.Region;
            Console.WriteLine(hub.Attributes.Extension.Type);
        }
    }

    public async Task GetHubAsync()
    {
        Hub hub = await _dataManagementClient.GetHubAsync(hubId: _hubId);

        HubData hubData = hub.Data;
        TypeHub hubType = hubData.Type;
        string hubId = hubData.Id;

        Console.WriteLine(hubType);
        Console.WriteLine(hubId);
        Console.WriteLine(hubData.Attributes.Name);
    }

    #endregion Hubs

    #region Projects

    public async Task GetHubProjectsAsync()
    {
        List<string> filter_id = ["b.180e1bc8-6687-4029-a069-319f611de8a9"];
        List<string> filter_extension_type = ["projects:autodesk.bim360:Project"];

        Projects projects = await _dataManagementClient.GetHubProjectsAsync(hubId: _hubId, filterId: filter_id, filterExtensionType: filter_extension_type, pageNumber: 0, pageLimit: 1);

        List<ProjectData> projectsData = projects.Data;
        foreach (var current in projectsData)
        {
            TypeProject hubProjectsType = current.Type;
            string hubProjectsId = current.Id;

            Console.WriteLine(hubProjectsType);
            Console.WriteLine(hubProjectsId);
            Console.WriteLine(current.Attributes.Extension.Type);
        }
    }

    public async Task GetProjectAsync()
    {
        Project project = await _dataManagementClient.GetProjectAsync(hubId: _hubId, projectId: _projectId);

        ProjectData projectData = project.Data;
        TypeProject hubProjectDataType = projectData.Type;
        string hubProjectDataId = projectData.Id;

        Console.WriteLine(hubProjectDataType);
        Console.WriteLine(hubProjectDataId);
    }

    public async Task GetProjectHubAsync()
    {
        Hub hub = await _dataManagementClient.GetProjectHubAsync(hubId: _hubId, projectId: _projectId);

        HubData hubData = hub.Data;
        TypeHub hubType = hubData.Type;
        string hubId = hubData.Id;

        Console.WriteLine(hubType);
        Console.WriteLine(hubId);
    }

    public async Task GetProjectTopFoldersAsync()
    {
        TopFolders topFolders = await _dataManagementClient.GetProjectTopFoldersAsync(hubId: _hubId, projectId: _projectId, excludeDeleted: true, projectFilesOnly: false);

        List<TopFolderData> topFolderData = topFolders.Data;
        foreach (var topFolder in topFolderData)
        {
            TypeFolder folderType = topFolder.Type;
            string folderId = topFolder.Id;

            Console.WriteLine(folderType);
            Console.WriteLine(folderId);
        }
    }

    public async Task GetDownloadAsync()
    {
        Download download = await _dataManagementClient.GetDownloadAsync(projectId: _projectId, downloadId: _downloadId);

        DownloadData downloadData = download.Data;
        TypeDownloads downloadType = downloadData.Type;
        string downloadId = downloadData.Id;

        Console.WriteLine(downloadType);
        Console.WriteLine(downloadId);
    }

    public async Task GetDownloadJobAsync()
    {
        Job job = await _dataManagementClient.GetDownloadJobAsync(projectId: _projectId, jobId: _jobId);

        JobData jobData = job.Data;
        TypeJob jobDataType = jobData.Type;
        string jobDataId = jobData.Id;

        Console.WriteLine(jobDataType);
        Console.WriteLine(jobDataId);
    }

    public async Task CreateDownloadAsync()
    {
        DownloadPayload downloadPayload = new()
        {
            Jsonapi = new JsonApiVersion()
            {
                VarVersion = JsonApiVersionValue._10
            },
            Data = new DownloadPayloadData()
            {
                Type = TypeDownloads.Downloads,
                Attributes = new DownloadPayloadDataAttributes()
                {
                    Format = new DownloadPayloadDataAttributesFormat()
                    {
                        FileType = "dwg"
                    }
                },
                Relationships = new DownloadPayloadDataRelationships()
                {
                    Source = new DownloadPayloadDataRelationshipsSource()
                    {
                        Data = new DownloadPayloadDataRelationshipsSourceData()
                        {
                            Type = TypeVersion.Versions,
                            Id = _versionId
                        }
                    }
                }
            }
        };

        CreatedDownload createdDownload = await _dataManagementClient.CreateDownloadAsync(projectId: _projectId, downloadPayload: downloadPayload);

        List<CreatedDownloadData> createdDownloadData = createdDownload.Data;
        foreach (var downloadData in createdDownloadData)
        {
            TypeJob downloadDataType = downloadData.Type;
            string downloadDataId = downloadData.Id;

            Console.WriteLine(downloadDataType);
            Console.WriteLine(downloadDataId);
        }
    }

    public async Task CreateStorageAsync()
    {
        StoragePayload storagePayload = new()
        {
            Jsonapi = new JsonApiVersion()
            {
                VarVersion = JsonApiVersionValue._10
            },
            Data = new StoragePayloadData()
            {
                Type = TypeObject.Objects,
                Attributes = new StoragePayloadDataAttributes()
                {
                    Name = "drawing.dwg",
                },
                Relationships = new StoragePayloadDataRelationships()
                {
                    Target = new StoragePayloadDataRelationshipsTarget()
                    {
                        Data = new StoragePayloadDataRelationshipsTargetData()
                        {
                            Type = TypeFolderItemsForStorage.Folders,
                            Id = _folderId
                        }
                    }
                }
            }
        };

        Storage storage = await _dataManagementClient.CreateStorageAsync(projectId: _projectId, storagePayload: storagePayload);

        StorageData storageData = storage.Data;
        TypeObject storageDataType = storageData.Type;
        string storageDataId = storageData.Id;

        Console.WriteLine(storageDataType);
        Console.WriteLine(storageDataId);
    }

    #endregion Projects

    #region Folders

    public async Task GetFolderAsync()
    {
        Folder folder = await _dataManagementClient.GetFolderAsync(projectId: _projectId, folderId: _folderId);

        FolderData folderData = folder.Data;
        TypeFolder folderDataType = folderData.Type;
        string folderDataId = folderData.Id;

        Console.WriteLine(folderDataType);
        Console.WriteLine(folderDataId);
    }

    public async Task GetFolderContentsAsync()
    {
        List<FilterType> filter_type = [FilterType.Items, FilterType.Folders];

        FolderContents folderContents = await _dataManagementClient.GetFolderContentsAsync(projectId: _projectId, folderId: _folderId, filterType: filter_type);
        Console.WriteLine(folderContents);
        List<IFolderContentsData> folderContentsData = folderContents.Data;

        var converter = new FolderContentsDataConverter();
        var serializer = new JsonSerializer();

        using (var stringWriter = new StringWriter())
        using (var jsonWriter = new JsonTextWriter(stringWriter))
        {
            converter.WriteJson(jsonWriter, folderContents.Data, serializer);
            string jsonOutput = stringWriter.ToString();
            Console.WriteLine(jsonOutput);
        }

        foreach (var current in folderContentsData)
        {
            if (current is FolderData folder)
            {
                Console.WriteLine(folder.Id);
                Console.WriteLine(folder.Type);
                Console.WriteLine(folder.Relationships.Parent.Data.Type);
            }
            else if (current is ItemData item)
            {
                Console.WriteLine(item.Id);
                Console.WriteLine(item.Type);
            }
        }
    }

    public async Task GetFolderParentAsync()
    {
        Folder folder = await _dataManagementClient.GetFolderParentAsync(projectId: _projectId, folderId: _folderId);

        FolderData folderData = folder.Data;
        TypeFolder folderDataType = folderData.Type;
        string folderDataId = folderData.Id;

        Console.WriteLine(folderDataType);
        Console.WriteLine(folderDataId);
    }

    public async Task GetFolderRefsAsync()
    {
        FolderRefs folderRefs = await _dataManagementClient.GetFolderRefsAsync(projectId: _projectId, folderId: _folderId);
        List<IFolderRefsData> folderRefsData = folderRefs.Data;
        foreach (var current in folderRefsData)
        {
            if (current is FolderData folder)
            {
                Console.WriteLine(folder.Id);
                Console.WriteLine(folder.Type);
                Console.WriteLine(folder.Relationships.Parent.Data.Type);
            }
            else if (current is ItemData item)
            {
                Console.WriteLine(item.Id);
                Console.WriteLine(item.Type);
            }
            else if (current is VersionData version)
            {
                Console.WriteLine(version.Id);
                Console.WriteLine(version.Type);
            }
        }
    }

    public async Task GetFolderRelationshipsLinksAsync()
    {
        RelationshipLinks relationshipLinks = await _dataManagementClient.GetFolderRelationshipsLinksAsync(projectId: _projectId, folderId: _folderId);

        List<RelationshipLinksData> relationshipLinksData = relationshipLinks.Data;
        foreach (var relationshipLinkData in relationshipLinksData)
        {
            TypeLink relationshipLinkDataType = relationshipLinkData.Type;
            string relationshipLinkDataId = relationshipLinkData.Id;

            Console.WriteLine(relationshipLinkDataType);
            Console.WriteLine(relationshipLinkDataId);
        }
    }

    public async Task GetFolderRelationshipsRefsAsync()
    {
        RelationshipRefs relationshipRefs = await _dataManagementClient.GetFolderRelationshipsRefsAsync(folderId: _folderId, projectId: _projectId);

        IRelationshipRefsLinks links = relationshipRefs.Links;
        Console.WriteLine(links);

        List<RelationshipRefsData> relationshipRefsData = relationshipRefs.Data;
        foreach (var relationshipRefData in relationshipRefsData)
        {
            TypeEntity relationshipRefDataType = relationshipRefData.Type;
            string relationshipRefDataId = relationshipRefData.Id;

            Console.WriteLine(relationshipRefDataType);
            Console.WriteLine(relationshipRefDataId);
        }

        List<IRelationshipRefsIncluded> relationshipRefsIncluded = relationshipRefs.Included;
        foreach (var current in relationshipRefsIncluded)
        {
            if (current is FolderData folder)
            {
                Console.WriteLine(folder.Id);
                Console.WriteLine(folder.Type);
                Console.WriteLine(folder.Relationships.Parent.Data.Type);
            }
            else if (current is ItemData item)
            {
                Console.WriteLine(item.Id);
                Console.WriteLine(item.Type);
            }
            else if (current is VersionData version)
            {
                Console.WriteLine(version.Id);
                Console.WriteLine(version.Type);
            }
        }
    }

    public async Task GetFolderSearchAsync()
    {
        List<string> filter = ["John Doe"];
        Search search = await _dataManagementClient.GetFolderSearchAsync(projectId: _projectId, folderId: _folderId, filterFieldName: "createUserName", filterValue: filter, pageNumber: 0);

        List<VersionData> searchData = search.Data;
        foreach (var currentSearchData in searchData)
        {
            TypeVersion currentSearchDataType = currentSearchData.Type;
            string currentSearchDataId = currentSearchData.Id;

            Console.WriteLine(currentSearchDataType);
            Console.WriteLine(currentSearchDataId);
        }
    }

    public async Task CreateFolderAsync()
    {
        FolderPayload folderPayload = new()
        {
            Jsonapi = new JsonApiVersion()
            {
                VarVersion = JsonApiVersionValue._10
            },
            Data = new FolderPayloadData()
            {
                Type = TypeFolder.Folders,
                Attributes = new FolderPayloadDataAttributes()
                {
                    Name = "Preject 2030",
                    Extension = new FolderPayloadDataAttributesExtension()
                    {
                        Type = "folders:autodesk.bim360:Folder",
                        VarVersion = "1.0"
                    }
                },
                Relationships = new FolderPayloadDataRelationships()
                {
                    Parent = new FolderPayloadDataRelationshipsParent()
                    {
                        Data = new FolderPayloadDataRelationshipsParentData()
                        {
                            Type = TypeFolder.Folders,
                            Id = _folderId,
                        }
                    }
                }
            },
        };

        Console.WriteLine(folderPayload);

        Folder folder = await _dataManagementClient.CreateFolderAsync(projectId: _projectId, folderPayload: folderPayload);

        FolderData folderData = folder.Data;
        TypeFolder folderDataType = folderData.Type;
        string folderDataId = folderData.Id;

        Console.WriteLine(folderDataType);
        Console.WriteLine(folderDataId);
    }

    public async Task CreateFolderRelationshipsRefAsync()
    {
        RelationshipRefsPayload relationshipRefsPayload = new()
        {
            Jsonapi = new JsonApiVersion()
            {
                VarVersion = JsonApiVersionValue._10
            },
            Data = new RelationshipRefsPayloadData()
            {
                Type = TypeEntity.Versions,
                Id = _versionId,
                Meta = new RelationshipRefsPayloadDataMeta()
                {
                    Extension = new BaseAttributesExtensionObjectWithoutSchemaLink()
                    {
                        Type = "auxiliary:autodesk.core:Attachment",
                        VarVersion = "1.0"
                    }
                }
            }
        };

        HttpResponseMessage relationship = await _dataManagementClient.CreateFolderRelationshipsRefAsync(folderId: _folderId, projectId: _projectId, relationshipRefsPayload: relationshipRefsPayload);
        var statusCode = relationship.StatusCode;
        string statusCodeString = statusCode.ToString();

        Console.WriteLine(statusCodeString);
    }

    public async Task PatchFolderAsync()
    {
        ModifyFolderPayload modifyFolderPayload = new()
        {
            Jsonapi = new JsonApiVersion()
            {
                VarVersion = JsonApiVersionValue._10
            },
            Data = new ModifyFolderPayloadData()
            {
                Type = TypeFolder.Folders,
                Id = _folderId,
                Attributes = new ModifyFolderPayloadDataAttributes()
                {
                    Name = "Project 3096"
                }
            }
        };

        Console.WriteLine(modifyFolderPayload);

        Folder folder = await _dataManagementClient.PatchFolderAsync(projectId: _projectId, folderId: _folderId, modifyFolderPayload: modifyFolderPayload);

        FolderData folderData = folder.Data;
        TypeFolder folderDataType = folderData.Type;
        string folderDataId = folderData.Id;

        Console.WriteLine(folderDataType);
        Console.WriteLine(folderDataId);
    }

    #endregion Folders

    #region Items

    public async Task GetItemAsync()
    {
        Item item = await _dataManagementClient.GetItemAsync(projectId: _projectId, itemId: _itemId);

        ItemData itemData = item.Data;
        TypeItem itemDataType = itemData.Type;
        string itemDataId = itemData.Id;

        Console.WriteLine(itemDataType);
        Console.WriteLine(itemDataId);
    }

    public async Task GetItemParentFolderAsync()
    {
        Folder folder = await _dataManagementClient.GetItemParentFolderAsync(projectId: _projectId, itemId: _itemId);

        FolderData folderData = folder.Data;
        TypeFolder folderDataType = folderData.Type;
        string folderDataId = folderData.Id;

        Console.WriteLine(folderDataType);
        Console.WriteLine(folderDataId);
    }

    public async Task GetItemRefsAsync()
    {
        Refs refs = await _dataManagementClient.GetItemRefsAsync(projectId: _projectId, itemId: _itemId);

        List<IRefsData> refsData = refs.Data;
        foreach (var current in refsData)
        {
            if (current is FolderData folder)
            {
                Console.WriteLine(folder.Id);
                Console.WriteLine(folder.Type);
                Console.WriteLine(folder.Relationships.Parent.Data.Type);
            }
            else if (current is ItemData item)
            {
                Console.WriteLine(item.Id);
                Console.WriteLine(item.Type);
            }
            else if (current is VersionData version)
            {
                Console.WriteLine(version.Id);
                Console.WriteLine(version.Type);
            }
        }
    }

    public async Task GetItemRelationshipsLinksAsync()
    {
        RelationshipLinks relationshipLinks = await _dataManagementClient.GetItemRelationshipsLinksAsync(projectId: _projectId, itemId: _itemId);

        List<RelationshipLinksData> relationshipLinksData = relationshipLinks.Data;
        foreach (var relationshipLinkData in relationshipLinksData)
        {
            TypeLink relationshipLinkDataType = relationshipLinkData.Type;
            string relationshipLinkDataId = relationshipLinkData.Id;

            Console.WriteLine(relationshipLinkDataType);
            Console.WriteLine(relationshipLinkDataId);
        }
    }

    public async Task GetItemRelationshipsRefsAsync()
    {
        RelationshipRefs relationshipRefs = await _dataManagementClient.GetItemRelationshipsRefsAsync(projectId: _projectId, itemId: _itemId);

        IRelationshipRefsLinks links = relationshipRefs.Links;
        Console.WriteLine(links);

        List<RelationshipRefsData> relationshipRefsData = relationshipRefs.Data;
        foreach (var relationshipRefData in relationshipRefsData)
        {
            TypeEntity relationshipRefDataType = relationshipRefData.Type;
            string relationshipRefDataId = relationshipRefData.Id;

            Console.WriteLine(relationshipRefDataType);
            Console.WriteLine(relationshipRefDataId);
        }

        List<IRelationshipRefsIncluded> relationshipRefsIncluded = relationshipRefs.Included;
        foreach (var current in relationshipRefsIncluded)
        {
            if (current is FolderData folder)
            {
                Console.WriteLine(folder.Id);
                Console.WriteLine(folder.Type);
                Console.WriteLine(folder.Relationships.Parent.Data.Type);
            }
            else if (current is ItemData item)
            {
                Console.WriteLine(item.Id);
                Console.WriteLine(item.Type);
            }
            else if (current is VersionData version)
            {
                Console.WriteLine(version.Id);
                Console.WriteLine(version.Type);
            }
        }
    }

    public async Task GetItemTipAsync()
    {
        ItemTip itemTip = await _dataManagementClient.GetItemTipAsync(projectId: _projectId, itemId: _itemId);

        VersionData itemTipData = itemTip.Data;
        TypeVersion itemTipDataType = itemTipData.Type;
        string itemTipDataId = itemTipData.Id;

        Console.WriteLine(itemTipDataType);
        Console.WriteLine(itemTipDataId);
    }

    public async Task GetItemVersionsAsync()
    {
        Versions versions = await _dataManagementClient.GetItemVersionsAsync(projectId: _projectId, itemId: _itemId);

        List<VersionData> versionsData = versions.Data;
        foreach (var versionData in versionsData)
        {
            TypeVersion versionDataType = versionData.Type;
            string versionDataId = versionData.Id;

            Console.WriteLine(versionDataType);
            Console.WriteLine(versionDataId);
        }
    }

    public async Task CreateItemAsync()
    {
        ItemPayload itemPayload = new()
        {
            Jsonapi = new JsonApiVersion()
            {
                VarVersion = JsonApiVersionValue._10
            },
            Data = new ItemPayloadData()
            {
                Type = TypeItem.Items,
                Attributes = new ItemPayloadDataAttributes()
                {
                    DisplayName = "drawingmytmz.rvt",
                    Extension = new ItemPayloadDataAttributesExtension()
                    {
                        Type = "items:autodesk.bim360:File",
                        VarVersion = "1.0"
                    }
                },
                Relationships = new ItemPayloadDataRelationships()
                {
                    Tip = new ItemPayloadDataRelationshipsTip()
                    {
                        Data = new ItemPayloadDataRelationshipsTipData()
                        {
                            Type = TypeVersion.Versions,
                            Id = "1"
                        }
                    },
                    Parent = new ItemPayloadDataRelationshipsParent()
                    {
                        Data = new ItemPayloadDataRelationshipsParentData()
                        {
                            Type = TypeFolder.Folders,
                            Id = _folderId,
                        }
                    }
                }
            },
            Included = new List<ItemPayloadIncluded>()
            {
                new ItemPayloadIncluded()
                {
                    Type = TypeVersion.Versions,
                    Id = "1",
                    Attributes = new ItemPayloadIncludedAttributes()
                    {
                        Name = "drawingmytmz.rvt",
                        Extension = new ItemPayloadIncludedAttributesExtension()
                        {
                            Type = "versions:autodesk.bim360:File",
                            VarVersion = "1.0"
                        }
                    },
                    Relationships = new ItemPayloadIncludedRelationships()
                    {
                        Storage = new ItemPayloadIncludedRelationshipsStorage()
                        {
                            Data = new ItemPayloadIncludedRelationshipsStorageData()
                            {
                                Type = TypeObject.Objects,
                                Id = "urn:adsk.objects:os.object:wip.dm.prod/462aed7c-8d5d-47a1-ae5f-5c930a35931c.rvt"
                            }
                        }
                    }
                }
            }
        };

        CreatedItem item = await _dataManagementClient.CreateItemAsync(projectId: _projectId, itemPayload: itemPayload);

        ItemData itemData = item.Data;
        TypeItem itemDataType = itemData.Type;
        string itemDataId = itemData.Id;

        Console.WriteLine(itemDataType);
        Console.WriteLine(itemDataId);
    }

    public async Task CreateItemRelationshipsRefAsync()
    {
        RelationshipRefsPayload relationshipRefsPayload = new()
        {
            Jsonapi = new JsonApiVersion()
            {
                VarVersion = JsonApiVersionValue._10
            },
            Data = new RelationshipRefsPayloadData()
            {
                Type = TypeEntity.Versions,
                Id = _versionId,
                Meta = new RelationshipRefsPayloadDataMeta()
                {
                    Extension = new BaseAttributesExtensionObjectWithoutSchemaLink()
                    {
                        Type = "auxiliary:autodesk.core:Attachment",
                        VarVersion = "1.0"
                    }
                }
            }
        };

        HttpResponseMessage responseMessage = await _dataManagementClient.CreateItemRelationshipsRefAsync(projectId: _projectId, itemId: _itemId, relationshipRefsPayload: relationshipRefsPayload);
        var statusCode = responseMessage.StatusCode;
        string statusCodeString = statusCode.ToString();

        Console.WriteLine(statusCodeString);
    }

    public async Task PatchItemAsync()
    {
        ModifyItemPayload modifyItemPayload = new ModifyItemPayload()
        {
            Jsonapi = new JsonApiVersion()
            {
                VarVersion = JsonApiVersionValue._10
            },
            Data = new ModifyItemPayloadData()
            {
                Type = TypeItem.Items,
                Id = _itemId,
                Attributes = new ModifyItemPayloadDataAttributes()
                {
                    DisplayName = "newDrawing.rvt"
                }
            }
        };

        Item item = await _dataManagementClient.PatchItemAsync(projectId: _projectId, itemId: _itemId, modifyItemPayload: modifyItemPayload);

        ItemData itemData = item.Data;
        TypeItem itemDataType = itemData.Type;
        string itemDataId = itemData.Id;

        Console.WriteLine(itemDataType);
        Console.WriteLine(itemDataId);
    }

    #endregion Items

    #region Versions

    public async Task GetVersionAsync()
    {
        ModelVersion versionDetails = await _dataManagementClient.GetVersionAsync(projectId: _projectId, versionId: _versionId);

        VersionData versionDetailsData = versionDetails.Data;
        TypeVersion versionDetailsDataType = versionDetailsData.Type;
        string versionDetailsDataId = versionDetailsData.Id;

        Console.WriteLine(versionDetailsDataType);
        Console.WriteLine(versionDetailsDataId);
    }

    public async Task GetVersionDownloadFormatsAsync()
    {
        DownloadFormats downloadFormats = await _dataManagementClient.GetVersionDownloadFormatsAsync(projectId: _projectId, versionId: _versionId);

        DownloadFormatsData downloadFormatsData = downloadFormats.Data;
        TypeDownloadformats downloadFormatsDataType = downloadFormatsData.Type;
        string downloadFormatsDataId = downloadFormatsData.Id;

        Console.WriteLine(downloadFormatsDataType);
        Console.WriteLine(downloadFormatsDataId);
    }

    public async Task GetVersionDownloadsAsync()
    {
        Downloads downloads = await _dataManagementClient.GetVersionDownloadsAsync(projectId: _projectId, versionId: _versionId);

        List<DownloadData> downloadsData = downloads.Data;
        foreach (var downloadData in downloadsData)
        {
            TypeDownloads downloadDataType = downloadData.Type;
            string downloadDataId = downloadData.Id;

            Console.WriteLine(downloadDataType);
            Console.WriteLine(downloadDataId);
        }
    }

    public async Task GetVersionItemAsync()
    {
        Item item = await _dataManagementClient.GetVersionItemAsync(projectId: _projectId, versionId: _versionId);

        ItemData itemData = item.Data;
        TypeItem itemDataType = itemData.Type;
        string itemDataId = itemData.Id;

        Console.WriteLine(itemDataType);
        Console.WriteLine(itemDataId);
    }

    public async Task GetVersionRefsAsync()
    {
        Refs refs = await _dataManagementClient.GetVersionRefsAsync(projectId: _projectId, versionId: _versionId);

        List<IRefsData> refsData = refs.Data;
        foreach (var current in refsData)
        {
            if (current is FolderData folder)
            {
                Console.WriteLine(folder.Id);
                Console.WriteLine(folder.Type);
                Console.WriteLine(folder.Relationships.Parent.Data.Type);
            }
            else if (current is ItemData item)
            {
                Console.WriteLine(item.Id);
                Console.WriteLine(item.Type);
            }
            else if (current is VersionData version)
            {
                Console.WriteLine(version.Id);
                Console.WriteLine(version.Type);
            }
        }
    }

    public async Task GetVersionRelationshipsLinksAsync()
    {
        RelationshipLinks relationshipLinks = await _dataManagementClient.GetVersionRelationshipsLinksAsync(projectId: _projectId, versionId: _versionId);

        List<RelationshipLinksData> relationshipLinksData = relationshipLinks.Data;
        foreach (var relationshipLinkData in relationshipLinksData)
        {
            TypeLink relationshipLinkDataType = relationshipLinkData.Type;
            string relationshipLinkDataId = relationshipLinkData.Id;

            Console.WriteLine(relationshipLinkDataType);
            Console.WriteLine(relationshipLinkDataId);
        }
    }

    public async Task GetVersionRelationshipsRefsAsync()
    {
        RelationshipRefs relationshipRefs = await _dataManagementClient.GetVersionRelationshipsRefsAsync(projectId: _projectId, versionId: _versionId);

        IRelationshipRefsLinks links = relationshipRefs.Links;
        Console.WriteLine(links);

        List<RelationshipRefsData> relationshipRefsData = relationshipRefs.Data;
        foreach (var relationshipRefData in relationshipRefsData)
        {
            TypeEntity relationshipRefDataType = relationshipRefData.Type;
            string relationshipRefDataId = relationshipRefData.Id;

            Console.WriteLine(relationshipRefDataType);
            Console.WriteLine(relationshipRefDataId);
        }

        List<IRelationshipRefsIncluded> relationshipRefsIncluded = relationshipRefs.Included;
        foreach (var current in relationshipRefsIncluded)
        {
            if (current is FolderData folder)
            {
                Console.WriteLine(folder.Id);
                Console.WriteLine(folder.Type);
                Console.WriteLine(folder.Relationships.Parent.Data.Type);
            }
            else if (current is ItemData item)
            {
                Console.WriteLine(item.Id);
                Console.WriteLine(item.Type);
            }
            else if (current is VersionData version)
            {
                Console.WriteLine(version.Id);
                Console.WriteLine(version.Type);
            }
        }
    }

    public async Task CreateVersionAsync()
    {
        VersionPayload versionPayload = new VersionPayload()
        {
            Jsonapi = new JsonApiVersion()
            {
                VarVersion = JsonApiVersionValue._10
            },
            Data = new VersionPayloadData()
            {
                Type = TypeVersion.Versions,
                Attributes = new VersionPayloadDataAttributes()
                {
                    Name = "racteded.rvt",
                    Extension = new VersionPayloadDataAttributesExtension()
                    {
                        Type = "versions:autodesk.bim360:File",
                        VarVersion = "1.0"
                    }
                },
                Relationships = new VersionPayloadDataRelationships()
                {
                    Item = new VersionPayloadDataRelationshipsItem()
                    {
                        Data = new VersionPayloadDataRelationshipsItemData()
                        {
                            Type = TypeItem.Items,
                            Id = _itemId
                        }
                    },
                    Storage = new VersionPayloadDataRelationshipsStorage()
                    {
                        Data = new VersionPayloadDataRelationshipsStorageData()
                        {
                            Type = TypeObject.Objects,
                            Id = _storageUrn
                        }
                    }
                }
            }
        };

        CreatedVersion createdVersion = await _dataManagementClient.CreateVersionAsync(projectId: _projectId, versionPayload: versionPayload);

        CreatedVersionData createdVersionData = createdVersion.Data;
        TypeVersion createdVersionDataType = createdVersionData.Type;
        string createdVersionDataId = createdVersionData.Id;

        Console.WriteLine(createdVersionDataType);
        Console.WriteLine(createdVersionDataId);
    }

    public async Task CreateVersionRelationshipsRefAsync()
    {
        RelationshipRefsPayload relationshipRefsPayload = new()
        {
            Jsonapi = new JsonApiVersion()
            {
                VarVersion = JsonApiVersionValue._10
            },
            Data = new RelationshipRefsPayloadData()
            {
                Type = TypeEntity.Versions,
                Id = _versionId,
                Meta = new RelationshipRefsPayloadDataMeta()
                {
                    Extension = new BaseAttributesExtensionObjectWithoutSchemaLink()
                    {
                        Type = "auxiliary:autodesk.core:Attachment",
                        VarVersion = "1.0"
                    }
                }
            }
        };

        HttpResponseMessage responseMessage = await _dataManagementClient.CreateVersionRelationshipsRefAsync(projectId: _projectId, versionId: _versionId, relationshipRefsPayload: relationshipRefsPayload);

        var statusCode = responseMessage.StatusCode;
        string statusCodeString = statusCode.ToString();

        Console.WriteLine(statusCodeString);
    }

    public async Task PatchVersionAsync()
    {
        ModifyVersionPayload modifyVersionPayload = new ModifyVersionPayload()
        {
            Jsonapi = new JsonApiVersion()
            {
                VarVersion = JsonApiVersionValue._10
            },
            Data = new ModifyVersionPayloadData()
            {
                Type = TypeVersion.Versions,
                Id = _versionId,
                Attributes = new ModifyVersionPayloadDataAttributes()
                {
                    Name = "project2624.rvt"
                }
            }
        };

        ModelVersion versionDetails = await _dataManagementClient.PatchVersionAsync(projectId: _projectId, versionId: _versionId, modifyVersionPayload: modifyVersionPayload);

        VersionData versionDetailsData = versionDetails.Data;
        TypeVersion versionDetailsDataType = versionDetailsData.Type;
        string versionDetailsDataId = versionDetailsData.Id;

        Console.WriteLine(versionDetailsDataType);
        Console.WriteLine(versionDetailsDataId);
    }

    #endregion Versions

    #region Commands

    public async Task ExecuteCheckPermissionCommandAsync()
    {
        CheckPermissionPayload checkPermissionPayload = new()
        {
            Type = TypeCommands.Commands,
            Attributes = new CheckPermissionPayloadAttributes()
            {
                Extension = new CheckPermissionPayloadAttributesExtension()
                {
                    Type = TypeCommandtypeCheckPermission.CommandsautodeskCoreCheckPermission,
                    VarVersion = "1.0.0",
                    Data = new CheckPermissionPayloadAttributesExtensionData()
                    {
                        RequiredActions = new List<string>
                        {
                            "download",
                            "view",
                        }
                    }
                }
            },
            Relationships = new CheckPermissionPayloadRelationships()
            {
                Resources = new CheckPermissionPayloadRelationshipsResources()
                {
                    Data = new List<CheckPermissionPayloadRelationshipsResourcesData>
                    {
                        new CheckPermissionPayloadRelationshipsResourcesData
                        {
                            Type = TypeEntity.Folders,
                            Id = "urn:adsk.wipprod:fs.folder:co.-tmPjozvRFC-q0MiANsZew"
                        },
                    }
                }
            }
        };

        CheckPermission checkPermission = await _dataManagementClient.ExecuteCheckPermissionAsync(projectId: _projectId, checkPermissionPayload: checkPermissionPayload);

        TypeCommands checkPermissionType = checkPermission.Type;
        string checkPermissionId = checkPermission.Id;


        Console.WriteLine(checkPermission);
        Console.WriteLine(checkPermissionType);
        Console.WriteLine(checkPermissionId);
    }

    public async Task ExecuteListRefsCommandAsync()
    {
        ListRefsPayload listRefsPayload = new()
        {
            Type = TypeCommands.Commands,
            Attributes = new ListRefsPayloadAttributes()
            {
                Extension = new ListRefsPayloadAttributesExtension()
                {
                    Type = TypeCommandtypeListRefs.CommandsautodeskCoreListRefs,
                    VarVersion = "1.0.0"
                }
            },
            Relationships = new ListRefsPayloadRelationships()
            {
                Resources = new ListRefsPayloadRelationshipsResources()
                {
                    Data = new List<ListRefsPayloadRelationshipsResourcesData>
                    {
                        new ListRefsPayloadRelationshipsResourcesData
                        {
                            Type = TypeVersion.Versions,
                            Id = _versionId
                        },
                    }
                }
            }
        };

        ListRefs listRefs = await _dataManagementClient.ExecuteListRefsAsync(projectId: _projectId, listRefsPayload: listRefsPayload);

        TypeCommands listRefsType = listRefs.Type;
        string listRefsId = listRefs.Id;

        Console.WriteLine(listRefsType);
        Console.WriteLine(listRefsId);
        Console.WriteLine(listRefs);

        var converter = new ListRefsIncludedConverter();
        var serializer = new JsonSerializer();

        using (var stringWriter = new StringWriter())
        using (var jsonWriter = new JsonTextWriter(stringWriter))
        {
            converter.WriteJson(jsonWriter, listRefs.Included, serializer);
            string jsonOutput = stringWriter.ToString();
            Console.WriteLine(jsonOutput);
        }

        List<IListRefsIncluded> listRefsIncluded = listRefs.Included;
        foreach (var current in listRefsIncluded)
        {
            if (current is ItemData item)
            {
                Console.WriteLine(item.Id);
                Console.WriteLine(item.Type);
            }
            else if (current is VersionData version)
            {
                Console.WriteLine(version.Id);
                Console.WriteLine(version.Type);
            }
        }
    }

    public async Task ExecuteListItemsCommandAsync()
    {
        ListItemsPayload listItemsPayload = new()
        {
            Type = TypeCommands.Commands,
            Attributes = new ListItemsPayloadAttributes()
            {
                Extension = new ListItemsPayloadAttributesExtension()
                {
                    Type = TypeCommandtypeListItems.CommandsautodeskCoreListItems,
                    VarVersion = "1.0.0"
                }
            },
            Relationships = new ListItemsPayloadRelationships()
            {
                Resources = new ListItemsPayloadRelationshipsResources()
                {
                    Data = new List<ListItemsPayloadRelationshipsResourcesData>
                    {
                        new ListItemsPayloadRelationshipsResourcesData
                        {
                            Type = TypeItem.Items,
                            Id = _itemId
                        },
                    }
                }
            }
        };

        ListItems listItems = await _dataManagementClient.ExecuteListItemsAsync(projectId: _projectId, listItemsPayload: listItemsPayload);

        TypeCommands listItemsType = listItems.Type;
        string listItemsId = listItems.Id;

        Console.WriteLine(listItemsType);
        Console.WriteLine(listItemsId);

        Console.WriteLine(listItems);
    }

    public async Task ExecuteGetPublishModelJobAsync()
    {
        PublishModelJobPayload publishModelJobPayload = new()
        {
            Type = TypeCommands.Commands,
            Attributes = new PublishModelJobPayloadAttributes()
            {
                Extension = new PublishModelJobPayloadAttributesExtension()
                {
                    Type = TypeCommandtypeGetPublishModelJob.CommandsautodeskBim360C4RModelGetPublishJob,
                    VarVersion = "1.0.0"
                }
            },
            Relationships = new PublishModelJobPayloadRelationships()
            {
                Resources = new PublishModelJobPayloadRelationshipsResources()
                {
                    Data = new List<PublishModelJobPayloadRelationshipsResourcesData>
                    {
                        new PublishModelJobPayloadRelationshipsResourcesData
                        {
                            Type = TypeItem.Items,
                            Id = _itemId
                        },
                    }
                }
            }
        };

        PublishModelJob publishModelJob = await _dataManagementClient.ExecuteGetPublishModelJobAsync(projectId: _projectId, publishModelJobPayload: publishModelJobPayload);

        TypeCommands publishModelJobType = publishModelJob.Type;
        string publishModelJobId = publishModelJob.Id;

        Console.WriteLine(publishModelJobType);
        Console.WriteLine(publishModelJobId);

        Console.WriteLine(publishModelJob);
    }

    public async Task ExecutePublishModelAsync()
    {
        PublishModelPayload publishModelPayload = new()
        {
            Type = TypeCommands.Commands,
            Attributes = new PublishModelPayloadAttributes()
            {
                Extension = new PublishModelPayloadAttributesExtension()
                {
                    Type = TypeCommandtypePublishmodel.CommandsautodeskBim360C4RModelPublish,
                    VarVersion = "1.0.0"
                }
            },
            Relationships = new PublishModelPayloadRelationships()
            {
                Resources = new PublishModelPayloadRelationshipsResources()
                {
                    Data = new List<PublishModelPayloadRelationshipsResourcesData>
                    {
                        new PublishModelPayloadRelationshipsResourcesData
                        {
                            Type = TypeItem.Items,
                            Id = _itemId
                        },
                    }
                }
            }
        };

        PublishModel publishModel = await _dataManagementClient.ExecutePublishModelAsync(projectId: _projectId, publishModelPayload: publishModelPayload);

        TypeCommands publishModelType = publishModel.Type;
        string publishModelId = publishModel.Id;

        Console.WriteLine(publishModelType);
        Console.WriteLine(publishModelId);

        Console.WriteLine(publishModel);
    }

    public async Task ExecutePublishWithoutLinksAsync()
    {
        PublishWithoutLinksPayload publishWithoutLinksPayload = new()
        {
            Type = TypeCommands.Commands,
            Attributes = new PublishWithoutLinksPayloadAttributes()
            {
                Extension = new PublishWithoutLinksPayloadAttributesExtension()
                {
                    Type = TypeCommandtypePublishWithoutLinks.CommandsautodeskBim360C4RPublishWithoutLinks,
                    VarVersion = "1.0.0"
                }
            },
            Relationships = new PublishWithoutLinksPayloadRelationships()
            {
                Resources = new PublishWithoutLinksPayloadRelationshipsResources()
                {
                    Data = new List<PublishWithoutLinksPayloadRelationshipsResourcesData>
                    {
                        new PublishWithoutLinksPayloadRelationshipsResourcesData
                        {
                            Type = TypeItem.Items,
                            Id = _itemId
                        },
                    }
                }
            }
        };

        PublishWithoutLinks publishWithoutLinks = await _dataManagementClient.ExecutePublishWithoutLinksAsync(projectId: _projectId, publishWithoutLinksPayload: publishWithoutLinksPayload);

        TypeCommands publishWithoutLinksType = publishWithoutLinks.Type;
        string publishWithoutLinksId = publishWithoutLinks.Id;

        Console.WriteLine(publishWithoutLinksType);
        Console.WriteLine(publishWithoutLinksId);

        Console.WriteLine(publishWithoutLinks);
    }

    #endregion Commands

}
