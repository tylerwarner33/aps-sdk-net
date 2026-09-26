using System.Net;
using Autodesk.Aps.TestCommon;
using Autodesk.Oss.Model;
using Autodesk.SDKManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Autodesk.Oss.Test;

[TestClass]
public class OssClientTests
{
    private const string Scopes = "bucket:create bucket:read bucket:delete data:read data:write data:create";
    private const int ChunkSizeInBytes = 5 * 1024 * 1024;
    private const string FixturePath = "fixtures/sample.txt";

    private static OssClient _ossClient = null!;
    private static string _accessToken = null!;
    private static Region _region;
    private static string _sharedBucketKey = null!;

    /// <summary>
    /// 	Creates the Tier 1 fixture: one bucket shared by every object test in this class.
    /// </summary>
    /// <remarks>
    /// 	The bucket persists between runs and is never deleted.
    /// 	OSS can keep a deleted key unavailable for some time, so a bucket that is deleted at the
    /// 	end of one run can be missing at the start of the next.
    /// 	The key is derived, so the bucket count stays at one.
    /// 	Every object test deletes its own object, and the transient policy expires any object
    /// 	that an aborted run leaves.
    /// </remarks>
    [ClassInitialize]
    public static async Task ClassInitialize(TestContext testContext)
    {
        Autodesk.SDKManager.SDKManager sdkManager = SdkManagerBuilder
            .Create()
            .Add(new ApsConfiguration())
            .Add(ResiliencyConfiguration.CreateDefault())
            .Build();

        _ossClient = new OssClient(sdkManager);
        _accessToken = await ApsTestTokens.GetTwoLeggedAsync(Scopes);
        _region = Enum.Parse<Region>(ApsTestConfig.Region, ignoreCase: true);
        _sharedBucketKey = ApsTestNaming.NewBucketKey("SharedFixture");

        await EnsureBucketAsync(_sharedBucketKey);
    }

    #region Buckets

    [TestMethod]
    public async Task CreateBucketAsync_WithNewBucketKey_ReturnsBucketWithSameKey()
    {
        string bucketKey = ApsTestNaming.NewRunScopedBucketKey();

        try
        {
            Bucket bucket = await _ossClient.CreateBucketAsync(
                xAdsRegion: _region,
                bucketsPayload: new CreateBucketsPayload
                {
                    BucketKey = bucketKey,
                    PolicyKey = PolicyKey.Transient
                },
                accessToken: _accessToken);

            Assert.AreEqual(bucketKey, bucket.BucketKey);
        }
        finally
        {
            await DeleteBucketIfPresentAsync(bucketKey);
        }
    }

    [TestMethod]
    public async Task GetBucketDetailsAsync_ForExistingBucket_ReturnsMatchingBucketKey()
    {
        Bucket bucket = await _ossClient.GetBucketDetailsAsync(
            bucketKey: _sharedBucketKey,
            accessToken: _accessToken);

        Assert.AreEqual(_sharedBucketKey, bucket.BucketKey);
    }

    [TestMethod]
    public async Task GetBucketsAsync_WhenCalled_ReturnsBucketItemsList()
    {
        Buckets buckets = await _ossClient.GetBucketsAsync(
            region: _region,
            accessToken: _accessToken);

        Assert.IsNotNull(buckets.Items);
    }

    [TestMethod]
    public async Task DeleteBucketAsync_ForExistingBucket_ReturnsOk()
    {
        string bucketKey = ApsTestNaming.NewRunScopedBucketKey();
        await EnsureBucketAsync(bucketKey);

        HttpResponseMessage response = await _ossClient.DeleteBucketAsync(
            bucketKey: bucketKey,
            accessToken: _accessToken);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Objects

    [TestMethod]
    public async Task UploadObjectAsync_WithLocalFile_ReturnsObjectWithSameKey()
    {
        string objectKey = ApsTestNaming.NewObjectKey();

        try
        {
            ObjectDetails objectDetails = await _ossClient.UploadObjectAsync(
                bucketKey: _sharedBucketKey,
                objectKey: objectKey,
                sourceToUpload: FixturePath,
                accessToken: _accessToken,
                cancellationToken: CancellationToken.None);

            Assert.AreEqual(objectKey, objectDetails.ObjectKey);
        }
        finally
        {
            await DeleteObjectIfPresentAsync(objectKey);
        }
    }

    [TestMethod]
    public async Task CopyToAsync_ToNewObjectName_ReturnsCopiedObject()
    {
        string objectKey = ApsTestNaming.NewObjectKey("source");
        string copyKey = ApsTestNaming.NewObjectKey("copy");

        try
        {
            await UploadFixtureAsync(objectKey);

            ObjectDetails objectDetails = await _ossClient.CopyToAsync(
                bucketKey: _sharedBucketKey,
                objectKey: objectKey,
                newObjName: copyKey,
                accessToken: _accessToken);

            Assert.AreEqual(copyKey, objectDetails.ObjectKey);
        }
        finally
        {
            await DeleteObjectIfPresentAsync(objectKey);
            await DeleteObjectIfPresentAsync(copyKey);
        }
    }

    [TestMethod]
    public async Task DownloadObjectAsync_ForExistingObject_WritesFileWithMatchingContent()
    {
        string objectKey = ApsTestNaming.NewObjectKey();
        using TempFile target = new(".txt");

        try
        {
            await UploadFixtureAsync(objectKey);

            await _ossClient.DownloadObjectAsync(
                bucketKey: _sharedBucketKey,
                objectKey: objectKey,
                filePath: target.Path,
                accessToken: _accessToken,
                cancellationToken: CancellationToken.None);

            CollectionAssert.AreEqual(
                await File.ReadAllBytesAsync(FixturePath),
                await File.ReadAllBytesAsync(target.Path));
        }
        finally
        {
            await DeleteObjectIfPresentAsync(objectKey);
        }
    }

    [TestMethod]
    public async Task GetObjectDetailsAsync_ForExistingObject_ReturnsMatchingObjectKey()
    {
        string objectKey = ApsTestNaming.NewObjectKey();

        try
        {
            await UploadFixtureAsync(objectKey);

            ObjectFullDetails objectFullDetails = await _ossClient.GetObjectDetailsAsync(
                bucketKey: _sharedBucketKey,
                objectKey: objectKey,
                accessToken: _accessToken);

            Assert.AreEqual(objectKey, objectFullDetails.ObjectKey);
        }
        finally
        {
            await DeleteObjectIfPresentAsync(objectKey);
        }
    }

    [TestMethod]
    public async Task GetObjectsAsync_ForExistingBucket_ReturnsObjectDetailsList()
    {
        string objectKey = ApsTestNaming.NewObjectKey();

        try
        {
            await UploadFixtureAsync(objectKey);

            BucketObjects bucketObjects = await _ossClient.GetObjectsAsync(
                bucketKey: _sharedBucketKey,
                accessToken: _accessToken);

            Assert.IsTrue(bucketObjects.Items.Count > 0);
        }
        finally
        {
            await DeleteObjectIfPresentAsync(objectKey);
        }
    }

    [TestMethod]
    public async Task DeleteObjectAsync_ForExistingObject_ReturnsOk()
    {
        string objectKey = ApsTestNaming.NewObjectKey();

        try
        {
            await UploadFixtureAsync(objectKey);

            HttpResponseMessage response = await _ossClient.DeleteObjectAsync(
                bucketKey: _sharedBucketKey,
                objectKey: objectKey,
                accessToken: _accessToken);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
        finally
        {
            await DeleteObjectIfPresentAsync(objectKey);
        }
    }

    #endregion

    #region Signed Resources

    [TestMethod]
    public async Task CreateSignedResourceAsync_ForExistingObject_ReturnsSignedUrl()
    {
        string objectKey = ApsTestNaming.NewObjectKey();

        try
        {
            await UploadFixtureAsync(objectKey);

            CreateObjectSigned signedObject = await _ossClient.CreateSignedResourceAsync(
                bucketKey: _sharedBucketKey,
                objectKey: objectKey,
                createSignedResource: new CreateSignedResource
                {
                    MinutesExpiration = 3,
                    SingleUse = true
                },
                accessToken: _accessToken);

            Assert.IsTrue(signedObject.SignedUrl.StartsWith(
                "https://developer.api.autodesk.com/oss/v2/signedresources/",
                StringComparison.Ordinal));
        }
        finally
        {
            await DeleteObjectIfPresentAsync(objectKey);
        }
    }

    /// <summary>
    /// 	Uploads a file larger than one chunk through a write signed resource.
    /// </summary>
    /// <remarks>
    /// 	The source file is generated rather than committed.
    /// 	A 5 MB binary does not belong in a git repository.
    /// </remarks>
    [TestMethod]
    public async Task UploadSignedResourcesChunkAsync_WithChunkedFile_ReturnsObjectDetails()
    {
        string objectKey = ApsTestNaming.NewObjectKey();
        string sessionId = Guid.NewGuid().ToString();
        using TempFile source = TempFile.OfSize(ChunkSizeInBytes + (1024 * 1024));
        long fileSize = new FileInfo(source.Path).Length;

        try
        {
            CreateObjectSigned signedObject = await _ossClient.CreateSignedResourceAsync(
                bucketKey: _sharedBucketKey,
                objectKey: objectKey,
                createSignedResource: new CreateSignedResource
                {
                    MinutesExpiration = 60,
                    SingleUse = false
                },
                access: Access.Write,
                accessToken: _accessToken);
            string hash = new Uri(signedObject.SignedUrl).Segments.Last();
            int chunkCount = (int)Math.Ceiling((double)fileSize / ChunkSizeInBytes);

            ObjectDetails? objectDetails = null;

            using (FileStream fileStream = File.OpenRead(source.Path))
            {
                byte[] buffer = new byte[ChunkSizeInBytes];

                for (int index = 0; index < chunkCount; index++)
                {
                    long startByte = index * (long)ChunkSizeInBytes;
                    long endByte = Math.Min(startByte + ChunkSizeInBytes - 1, fileSize - 1);
                    int contentLength = (int)(endByte - startByte + 1);

                    await fileStream.ReadExactlyAsync(buffer.AsMemory(0, contentLength));
                    using MemoryStream chunkStream = new(buffer, 0, contentLength);

                    objectDetails = await _ossClient.UploadSignedResourcesChunkAsync(
                        hash: hash,
                        contentRange: $"bytes {startByte}-{endByte}/{fileSize}",
                        sessionId: sessionId,
                        body: chunkStream,
                        contentType: "application/octet-stream",
                        accessToken: _accessToken);
                }
            }

            Assert.IsNotNull(objectDetails);
            Assert.AreEqual(fileSize, objectDetails.Size);
        }
        finally
        {
            await DeleteObjectIfPresentAsync(objectKey);
        }
    }

    /// <summary>
    /// 	Reads an object back through a signed resource.
    /// </summary>
    /// <remarks>
    /// 	The signed resource is created here, not read from a secret.
    /// 	A signed resource expires in minutes, so a stored value is dead before the next run.
    /// </remarks>
    [TestMethod]
    public async Task GetSignedResourceAsync_WithValidHash_ReturnsObjectContent()
    {
        string objectKey = ApsTestNaming.NewObjectKey();

        try
        {
            await UploadFixtureAsync(objectKey);
            string hash = await CreateSignedResourceHashAsync(objectKey);

            using Stream signedResource = await _ossClient.GetSignedResourceAsync(
                hash: hash,
                region: _region,
                accessToken: _accessToken);
            using MemoryStream content = new();
            await signedResource.CopyToAsync(content);

            CollectionAssert.AreEqual(await File.ReadAllBytesAsync(FixturePath), content.ToArray());
        }
        finally
        {
            await DeleteObjectIfPresentAsync(objectKey);
        }
    }

    [TestMethod]
    public async Task DeleteSignedResourceAsync_WithValidHash_ReturnsOk()
    {
        string objectKey = ApsTestNaming.NewObjectKey();

        try
        {
            await UploadFixtureAsync(objectKey);
            string hash = await CreateSignedResourceHashAsync(objectKey);

            HttpResponseMessage response = await _ossClient.DeleteSignedResourceAsync(
                hash: hash,
                region: _region,
                accessToken: _accessToken);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
        finally
        {
            await DeleteObjectIfPresentAsync(objectKey);
        }
    }

    #endregion

    #region Arrange and cleanup helpers

    /// <summary>
    /// 	Creates the bucket, or confirms that this application already owns it.
    /// </summary>
    /// <remarks>
    /// 	Bucket keys are global across every APS tenant, so a 409 alone does not prove ownership.
    /// 	The details call fails for a bucket that another application owns, which turns a
    /// 	confusing object-level failure later into a clear failure here.
    /// </remarks>
    private static async Task EnsureBucketAsync(string bucketKey)
    {
        try
        {
            await _ossClient.CreateBucketAsync(
                xAdsRegion: _region,
                bucketsPayload: new CreateBucketsPayload
                {
                    BucketKey = bucketKey,
                    PolicyKey = PolicyKey.Transient
                },
                accessToken: _accessToken);
        }
        catch (OssApiException exception) when (exception.StatusCode == HttpStatusCode.Conflict)
        {
            await _ossClient.GetBucketDetailsAsync(bucketKey: bucketKey, accessToken: _accessToken);
        }
    }

    private static async Task DeleteBucketIfPresentAsync(string bucketKey) =>
        await _ossClient.DeleteBucketAsync(
            bucketKey: bucketKey,
            accessToken: _accessToken,
            throwOnError: false);

    private static async Task DeleteObjectIfPresentAsync(string objectKey) =>
        await _ossClient.DeleteObjectAsync(
            bucketKey: _sharedBucketKey,
            objectKey: objectKey,
            accessToken: _accessToken,
            throwOnError: false);

    private static async Task UploadFixtureAsync(string objectKey) =>
        await _ossClient.UploadObjectAsync(
            bucketKey: _sharedBucketKey,
            objectKey: objectKey,
            sourceToUpload: FixturePath,
            accessToken: _accessToken,
            cancellationToken: CancellationToken.None);

    private static async Task<string> CreateSignedResourceHashAsync(string objectKey)
    {
        CreateObjectSigned signedObject = await _ossClient.CreateSignedResourceAsync(
            bucketKey: _sharedBucketKey,
            objectKey: objectKey,
            createSignedResource: new CreateSignedResource
            {
                MinutesExpiration = 10,
                SingleUse = false
            },
            accessToken: _accessToken);

        return new Uri(signedObject.SignedUrl).Segments.Last();
    }

    #endregion
}
