using Autodesk.ModelDerivative;
using Autodesk.ModelDerivative.Model;
using Autodesk.SDKManager;

namespace Samples;

public class ModelDerivative
{
    private readonly string? _token = Environment.GetEnvironmentVariable("TOKEN");
    private readonly string? _urn = Environment.GetEnvironmentVariable("URN");
    private readonly string? _modelGuid = Environment.GetEnvironmentVariable("MODEL_GUID");
    private readonly string? _derivativeUrn = Environment.GetEnvironmentVariable("DERIVATIVE_URN");

    private ModelDerivativeClient _modelDerivativeClient = null!;

    public void Initialize()
    {
        if (string.IsNullOrEmpty(_token))
            throw new InvalidOperationException(
                $"The access token is required to initialize the {nameof(ModelDerivativeClient)}.");

        // Optionally initialize SDKManager to pass custom configurations, logger, etc. 
        // SDKManager sdkManager = SdkManagerBuilder.Create().Build();

        // Instantiate ModelDerivativeClient using the auth provider
        StaticAuthenticationProvider staticAuthenticationProvider = new(_token);
        _modelDerivativeClient = new ModelDerivativeClient(authenticationProvider: staticAuthenticationProvider);
    }

    #region Jobs

    /// <summary>
    /// Post job.
    /// </summary>
    public async Task StartJobAsync()
    {
        List<IJobPayloadFormat> payloadFormats = 
        [
            new JobPayloadFormatSVF2()
            {
                Views = new List<View>()
                {
                    View._2d,
                    View._3d
                },
                Advanced = new JobPayloadFormatSVF2AdvancedRVT()
                {
                    GenerateMasterViews = true
                }
            },
            new JobPayloadFormatThumbnail()
            {
                Advanced = new JobPayloadFormatAdvancedThumbnail()
                {
                    Width = Width.NUMBER_100, // enum change to only 100
                    Height = Height.NUMBER_100
                }
            }
        ];

        JobPayload Job = new()
        {
            Input = new JobPayloadInput()
            {
                Urn = _urn,
                CompressedUrn = false,
                RootFilename = "<fileName>",
            },
            Output = new JobPayloadOutput()
            {
                Formats = payloadFormats,
                // Destination is obsolete. Use the region header instead.
                // Destination = new JobPayloadOutputDestination() { Region = Region.US } // This will call the respective endpoint - Either US or EMEA. Defaults to US.
            },
        };

        try
        {
            Job jobResponse = await _modelDerivativeClient.StartJobAsync(jobPayload: Job, region: Region.US);
            string jobUrn = jobResponse.Urn;
            string jobResult = jobResponse.Result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    #endregion

    #region Manifest

    /// <summary>
    /// Get manifest.
    /// </summary>
    public async Task GetManifestAsync()
    {
        try
        {
            Manifest manifestResponse = await _modelDerivativeClient.GetManifestAsync(_urn, region: Region.US);
            string manifestUrn = manifestResponse.Urn;
            string progress = manifestResponse.Progress;
            List<ManifestDerivative> derivatives = manifestResponse.Derivatives;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Delete manifest.
    /// </summary>
    public async Task DeleteManifestAsync()
    {
        try
        {
            DeleteManifest deleteManifest = await _modelDerivativeClient.DeleteManifestAsync(_urn, Region.US);
            var result = deleteManifest.Result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    #endregion

    #region Informational

    /// <summary>
    /// Get list of supported formats.
    /// </summary>
    public async Task GetFormatsAsync()
    {
        try
        {
            SupportedFormats formatsResponse = await _modelDerivativeClient.GetFormatsAsync();
            Dictionary<string, List<string>> supportedformats = formatsResponse.Formats;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    #endregion

    #region MetaData

    /// <summary>
    /// Get list of model views.
    /// </summary>
    public async Task GetModelViewsAsync()
    {
        try
        {
            ModelViews modelViewsResponse = await _modelDerivativeClient.GetModelViewsAsync(_urn, region: Region.US);
            string modelGuid = modelViewsResponse.Data.Metadata.First().Guid;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Get object tree.
    /// </summary>
    public async Task GetObjectTreeAsync()
    {
        try
        {
            ObjectTree objectTree = await _modelDerivativeClient.GetObjectTreeAsync(_urn, _modelGuid, Region.US);
            if (objectTree.IsProcessing)
            {
                // 202 response. Call the endpoint again or iteratively to get 200 OK.
            }
            List<ObjectTreeDataObjects> treeObjects = objectTree.Data.Objects;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Get specific properties.
    /// </summary>
    public async Task GetSpecificPropertiesAsync()
    {
        string modelGuid = Environment.GetEnvironmentVariable("modelGuid")!;
        SpecificPropertiesPayload payload = new()
        {
            Query = new MatchId()
            {
                In = new List<object> { MatchIdType.ObjectId, 4088 }
            }
        };

        try
        {
            SpecificProperties specificProperties = await _modelDerivativeClient.FetchSpecificPropertiesAsync(_urn, modelGuid, specificPropertiesPayload: payload, Region.US);
            if (specificProperties.IsProcessing)
            {
                // 202 response. Call the endpoint again or iteratively to get 200 OK.
            }
            List<PropertiesDataCollection> propertiesDataCollections = specificProperties.Data.Collection;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Get all properties.
    /// </summary>
    public async Task GetAllPropertiesAsync()
    {
        string modelGuid = Environment.GetEnvironmentVariable("modelGuid")!;
        try
        {
            Properties allProperties = await _modelDerivativeClient.GetAllPropertiesAsync(_urn, modelGuid);
            if (allProperties.IsProcessing)
            {
                // 202 response. Call the endpoint again or iteratively to get 200 OK.
            }
            List<PropertiesDataCollection> propertiesDataCollections = allProperties.Data.Collection;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    #endregion

    #region Thumbnail

    /// <summary>
    /// Get thumbnail.
    /// </summary>
    /// <returns></returns>
    public async Task GetThumbnailAsync()
    {
        try
        {
            Stream thumbnail = await _modelDerivativeClient.GetThumbnailAsync(_urn, Width.NUMBER_400, Height.NUMBER_400, Region.US);
            using (FileStream fileStream = new("/full/path/including/filename", FileMode.Create, FileAccess.Write))
            {
                thumbnail.CopyTo(fileStream);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    #endregion

    #region Derivatives

    /// <summary>
    /// Returns a downloadable url including the coookies.
    /// </summary>
    /// <returns></returns>
    public async Task DownloadDerivativeURLAsync()
    {
        try
        {
            DerivativeDownload derivativeDownload = await _modelDerivativeClient.GetDerivativeUrlAsync(_derivativeUrn, _urn, Region.US);
            var url = derivativeDownload.Url;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Get derivative headers.
    /// </summary>
    public async Task GetDerivativeHeadersAsync()
    {
        try
        {
            HttpResponseMessage derivativeHeaders = await _modelDerivativeClient.HeadCheckDerivativeAsync(_urn, _derivativeUrn, Region.US);
            if (derivativeHeaders.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                // 202 response. Call the endpoint again or iteratively to get 200 OK.
            }
            var ContentLength = derivativeHeaders.Content.Headers.ContentLength;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    #endregion

}
