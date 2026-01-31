using Autodesk.Oss;
using Autodesk.Oss.Model;
using Autodesk.SDKManager;

namespace Samples;

public class Oss
{
    private readonly string? _token = Environment.GetEnvironmentVariable("TOKEN");
    private readonly string? _bucketKey = Environment.GetEnvironmentVariable("BUCKET_KEY");
    private readonly string? _objectKey = Environment.GetEnvironmentVariable("OBJECT_KEY");
    private readonly string? _sourceToUpload = Environment.GetEnvironmentVariable("SOURCE_TO_UPLOAD");//sourceToUpload can also be a stream object
    private readonly string? _filePath = Environment.GetEnvironmentVariable("FILE_PATH");
    private readonly string? _hash = Environment.GetEnvironmentVariable("HASH");
    private readonly string? _newObjName = Environment.GetEnvironmentVariable("NEW_OBJ_NAME");

    private OssClient _ossClient = null!;

    public void Initialize()
    {
        if (string.IsNullOrEmpty(_token))
            throw new InvalidOperationException(
                $"The access token is required to initialize the {nameof(OssClient)}.");

        // Optionally initialize SDKManager to pass custom configurations, logger, etc. 
        SdkManagerBuilder.Create().Build();

        // Instantiate OssClient using the auth provider
        StaticAuthenticationProvider staticAuthenticationProvider = new(_token);
        _ossClient = new OssClient(authenticationProvider: staticAuthenticationProvider);
    }

    #region Buckets

    public async Task CreateBucketAsync()
    {
        Bucket response = await _ossClient.CreateBucketAsync(Region.US, new()
        {
            BucketKey = _bucketKey,
            PolicyKey = PolicyKey.Temporary
        });
        Console.WriteLine(response);
    }

    public async Task DeleteBucketAsync()
    {
        var response = await _ossClient.DeleteBucketAsync(_bucketKey);

        Console.WriteLine(response);
    }

    public async Task GetBucketsAsync()
    {
        Buckets response = await _ossClient.GetBucketsAsync();
        Console.WriteLine(response);
    }

    public async Task GetBucketDetailsAsync()
    {
        Bucket bucket = await _ossClient.GetBucketDetailsAsync(_bucketKey);
        string bucketkey = bucket.BucketKey;
        string bucketOwner = bucket.BucketOwner;
        Console.WriteLine(bucket);
    }

    #endregion

    #region Objects

    public async Task BatchSignedS3UploadAsync()
    {
        var uploadObject = new Batchsigneds3uploadObject
        {
            Requests = new List<Batchsigneds3uploadObjectRequests>
            {
                new Batchsigneds3uploadObjectRequests
                {
                    ObjectKey = _objectKey,
                    FirstPart = 1,
                    Parts = 5,
                    UploadKey = "" // Start a new upload
                }
            }
        };

        Batchsigneds3uploadResponse response = await _ossClient.BatchSignedS3UploadAsync(_bucketKey, uploadObject);
        Console.WriteLine(response);
    }

    public async Task CopyToAsync()
    {
        ObjectDetails response = await _ossClient.CopyToAsync(_bucketKey, _objectKey, _newObjName);
        Console.WriteLine(response);
    }

    public async Task DeleteObjectAsync()
    {
        var response = await _ossClient.DeleteObjectAsync(_bucketKey, _objectKey);
        Console.WriteLine(response);
    }

    public async Task DownloadObjectAsync()
    {
        //The below helper method takes care of the complete Download process, i.e.
        await _ossClient.DownloadObjectAsync(_bucketKey, _objectKey, _filePath);

        //we can also download the file as stream of files.
        Stream fileStream = await _ossClient.DownloadObjectAsync(_bucketKey, _objectKey);
    }

    public async Task GetObjectDetailsAsync()
    {
        ObjectFullDetails objectFullDetails =
            await _ossClient.GetObjectDetailsAsync(
                _bucketKey,
                _objectKey,
                with: With.UserDefinedMetadata);
    }

    public async Task GetObjectsAsync()
    {
        BucketObjects response = await _ossClient.GetObjectsAsync(_bucketKey);
        Console.WriteLine(response);
    }

    public async Task SignedS3DownloadAsync()
    {
        Signeds3downloadResponse response = await _ossClient.SignedS3DownloadAsync(_bucketKey, _objectKey);
        Console.WriteLine(response);
    }

    public async Task UploadObjectAsync()
    {
        string xAdsMetaContentType = "application/json";
        string xAdsUserDefinedMetadata =
            System.Text.Json.JsonSerializer.Serialize(new
            {
                id = "123ABC",
                name = "Test Example",
                building = new
                {
                    level = new
                    {
                        id = 1,
                        height = 10
                    }
                }
            });

    //The below helper method takes care of the complete upload process, i.e. 
    // the steps 2 to 4 in this link (https://aps.autodesk.com/en/docs/data/v2/tutorials/app-managed-bucket/)
        ObjectDetails objectDetails = await _ossClient.UploadObjectAsync(
            _bucketKey,
            _objectKey,
            _sourceToUpload,
            xAdsMetaContentType: xAdsMetaContentType,
            xAdsUserDefinedMetadata: xAdsUserDefinedMetadata);

        //sourceToUpload can be either file path or stream of the object 
        // query for required properties
        string objectId = objectDetails.ObjectId;
        string objectkey = objectDetails.ObjectKey;
        Console.WriteLine(objectDetails);
    }

    #endregion

    #region Signed Resources

    public async Task CreateSignedResourceAsync()
    {
        CreateObjectSigned response = await _ossClient.CreateSignedResourceAsync(_bucketKey, _objectKey, new()
        {
            MinutesExpiration = 3,
            SingleUse = true
        });
        Console.WriteLine(response);
    }

    public async Task DeleteSignedResourceAsync()
    {
        var response = await _ossClient.DeleteSignedResourceAsync(_hash);
        Console.WriteLine(response);
    }

    public async Task GetSignedResourceAsync()
    {
        Stream response = await _ossClient.GetSignedResourceAsync(_hash);
        Console.WriteLine(response);
    }

    #endregion

}
