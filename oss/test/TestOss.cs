using Autodesk.Forge.Core;
using Autodesk.Oss.Http;
using Autodesk.Oss.Model;
using Autodesk.SDKManager;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;

namespace Autodesk.Oss.Test;

[TestClass]
public class TestOss
{
	private static OssClient _ossClient = null!;

	private static ForgeService _forgeService = null!;

	string? token = "eyJhbGciOiJSUzI1NiIsImtpZCI6IlZiakZvUzhQU3lYODQyMV95dndvRUdRdFJEa19SUzI1NiIsInBpLmF0bSI6ImFzc2MifQ.eyJzY29wZSI6WyJidWNrZXQ6Y3JlYXRlIiwiYnVja2V0OnJlYWQiLCJkYXRhOmNyZWF0ZSIsImRhdGE6cmVhZCIsImRhdGE6d3JpdGUiLCJ2aWV3YWJsZXM6cmVhZCJdLCJjbGllbnRfaWQiOiJvd0dnM2FCOERBMUllUFdWQ0VJcGN0WnZWZUhKR2o2NUl4YVJCSExZbVdreVBzbGgiLCJpc3MiOiJodHRwczovL2RldmVsb3Blci5hcGkuYXV0b2Rlc2suY29tIiwiYXVkIjoiaHR0cHM6Ly9hdXRvZGVzay5jb20iLCJqdGkiOiIzOWVCUVozVlJaWXpRSFowSHVzZTB0bzF3eDdDVXE2dDJVTWdQakprQm1CWDBpc2tTTkRUWWdoWW9zUmRpNjRqIiwiZXhwIjoxNzU2MjA0ODQ1fQ.MrYp8e_WO1ztM_9k9RUfe3P8d_jEHmFOAZLbRIZNVikAGWpvlZAHi2yaWcfLQT6C80x7g0MiHKPOesXot9_l_Bpb5FVu9IJ-0G_Mcsqf_4-S0-IV5HW5Y32zkMOx1YyqI_1Zb4RIWhbCI_Ej-4r6cmXrR6WsGCV9bvGato8xDccwRWQ84oy41CohUvYgtdUWhMphHPW-lhjc7N1uSI4olDNsX2sVlDNRP3ZrpHzkLwep_W5xUgUYvXXdNEXo4I2jIBI_GfghPa5G3JnTa9OFr0aLqmBEolHD-cyKHw8ENoysTcgwKn-H6sfHf5e3PTuCEUSRRdhSDnYAEM5jJ7XULQ";
	//string? token = Environment.GetEnvironmentVariable("TWO_LEGGED_ACCESS_TOKEN");
	string? bucketKey = "test-owgg3ab8da1iepwvceipctzvvehjgj65ixarbhlymwkypslh";
	//string? bucketKey = Environment.GetEnvironmentVariable("BUCKET_KEY");
	string? objectKey = "Red_Box.ipt";
	//string? objectKey = Environment.GetEnvironmentVariable("OBJECT_KEY");
	string? newObjName = Environment.GetEnvironmentVariable("NEW_OBJ_NAME");
	string? sourceToUpload = "C:\\temp\\Red_Box.ipt";
	//string? sourceToUpload = Environment.GetEnvironmentVariable("SOURCE_TO_UPLOAD");
	string? filePath = Environment.GetEnvironmentVariable("FILE_PATH");
	// Signed Url Format: "https://developer.api.autodesk.com/oss/v2/signedresources/<hash>?region=US"
	string? signedUrl = Environment.GetEnvironmentVariable("SIGNED_URL");

	[ClassInitialize]
	public static void ClassInitialize(TestContext testContext)
	{
		var sdkManager = SdkManagerBuilder
			.Create()
			.Add(new ApsConfiguration())
			.Add(ResiliencyConfiguration.CreateDefault())
			.Build();

		_ossClient = new OssClient(sdkManager);
		_forgeService = ForgeService.CreateDefault();
	}

	#region Buckets

	[TestMethod]
	public async Task TestCreateBucketAsync()
	{
		Bucket bucket = await _ossClient.CreateBucketAsync(
			accessToken: token,
			xAdsRegion: Region.US,
			bucketsPayload: new CreateBucketsPayload()
			{
				BucketKey = bucketKey,
				PolicyKey = PolicyKey.Temporary
			});
		Assert.IsTrue(bucket.BucketKey.Equals(bucketKey));
	}

	[TestMethod]
	public async Task TestGetBucketDetailsAsync()
	{
		Bucket bucket = await _ossClient.GetBucketDetailsAsync(
			 accessToken: token,
			 bucketKey: bucketKey);
		Assert.IsTrue(bucket.BucketKey.Equals(bucketKey));
	}

	[TestMethod]
	public async Task TestGetBucketsAsync()
	{
		Buckets buckets = await _ossClient.GetBucketsAsync(accessToken: token);
		Assert.IsInstanceOfType(buckets.Items, typeof(List<BucketsItems>));
	}

	[TestMethod]
	public async Task TestDeleteBucketAsync()
	{
		HttpResponseMessage httpResponseMessage = await _ossClient.DeleteBucketAsync(
			 accessToken: token,
			 bucketKey: bucketKey);
		Assert.IsTrue(httpResponseMessage.StatusCode == HttpStatusCode.OK);
	}

	#endregion

	#region Objects

	[TestMethod]
	public async Task TestUploadObjectAsync()
	{
		ObjectDetails objectDetails = await _ossClient.UploadObjectAsync(
			accessToken: token,
			bucketKey: bucketKey,
			objectKey: objectKey,
			sourceToUpload: sourceToUpload,
			cancellationToken: CancellationToken.None);
		Assert.IsTrue(objectDetails.ObjectId.Equals($"urn:adsk.objects:os.object:{bucketKey}/{objectKey}"));
	}

	[TestMethod]
	public async Task TestCopyToAsync()
	{
		ObjectDetails objectDetails = await _ossClient.CopyToAsync(
			accessToken: token,
			bucketKey: bucketKey,
			objectKey: objectKey,
			newObjName: newObjName);
		Assert.IsTrue(objectDetails.ObjectId.Equals($"urn:adsk.objects:os.object:{bucketKey}/{newObjName}"));
	}

	[TestMethod]
	public async Task TestDownloadObjectAsync()
	{
		await _ossClient.DownloadObjectAsync(
			accessToken: token,
			bucketKey: bucketKey,
			objectKey: objectKey,
			filePath: filePath,
			cancellationToken: CancellationToken.None);
	}

	[TestMethod]
	public async Task TestGetObjectDetailsAsync()
	{
		ObjectFullDetails objectFullDetails = await _ossClient.GetObjectDetailsAsync(
			accessToken: token,
			bucketKey: bucketKey,
			objectKey: objectKey);
		Assert.IsTrue(objectFullDetails.ObjectId.Equals($"urn:adsk.objects:os.object:{bucketKey}/{objectKey}"));
	}

	[TestMethod]
	public async Task TestGetObjectsAsync()
	{
		BucketObjects bucketObjects = await _ossClient.GetObjectsAsync(
			accessToken: token,
			bucketKey: bucketKey);
		Assert.IsInstanceOfType(bucketObjects.Items, typeof(List<ObjectDetails>));
	}

	[TestMethod]
	public async Task TestDeleteObjectAsync()
	{
		HttpResponseMessage httpResponseMessage = await _ossClient.DeleteObjectAsync(
			accessToken: token,
			bucketKey: bucketKey,
			objectKey: objectKey);
		Assert.IsTrue(httpResponseMessage.StatusCode == HttpStatusCode.OK);
	}

	#endregion

	#region Signed Resources

	[TestMethod]
	public async Task TestCreateSignedResourceAsync()
	{
		CreateObjectSigned? signedObject = await _ossClient.CreateSignedResourceAsync(
			accessToken: token,
			bucketKey: bucketKey,
			objectKey: objectKey,
			createSignedResource: new()
			{
				MinutesExpiration = 3,
				SingleUse = true
			});
		Assert.IsTrue(signedObject.SignedUrl.StartsWith($"https://developer.api.autodesk.com/oss/v2/signedresources/"));
	}

	[TestMethod]
	public async Task TestUploadSignedResourcesChunkAsync()
	{
		string sessionId = Guid.NewGuid().ToString();
		var fileInfo = new FileInfo(sourceToUpload!);
		long fileSize = fileInfo.Length;
		const int chunkSize = 5 * 1024 * 1024; // 5MB

		CreateObjectSigned? signedObject = await _ossClient.CreateSignedResourceAsync(
			accessToken: token,
			access: Access.Write,
			bucketKey: bucketKey,
			objectKey: objectKey,
			createSignedResource: new()
			{
				MinutesExpiration = 60,
				SingleUse = false,
			});

		string signedUrl = signedObject.SignedUrl;
		string hash = new Uri(signedUrl).Segments.Last();

		ObjectDetails? objectDetails = null;
		int numChunks = (int)Math.Ceiling((double)fileSize / chunkSize);

		using var fileStream = File.OpenRead(sourceToUpload!);
		byte[] buffer = new byte[chunkSize];

		for (int index = 0; index < numChunks; index++)
		{
			long startByte = index * (long)chunkSize;
			long endByte = Math.Min(startByte + chunkSize - 1, fileSize - 1);
			int contentLength = (int)(endByte - startByte + 1);

			int bytesRead = await fileStream.ReadAsync(buffer.AsMemory(0, contentLength));
			using var chunkStream = new MemoryStream(buffer, 0, bytesRead);

			string contentRange = $"bytes {startByte}-{endByte}/{fileSize}";
			string contentType = $"application/octet-stream";

			objectDetails = await _ossClient.UploadSignedResourcesChunkAsync(
					accessToken: token,
					hash: hash,
					contentType: contentType,
					contentRange: contentRange,
					sessionId: sessionId,
					body: chunkStream);
		}

		Assert.IsNotNull(objectDetails);
		Assert.IsInstanceOfType<ObjectDetails>(objectDetails);
	}

	[TestMethod]
	public async Task TestGetSignedResourceAsync()
	{
		string hash = new Uri(signedUrl).Segments.Last();
		Stream? signedResource = await _ossClient.GetSignedResourceAsync(
			accessToken: token,
			hash: hash);
		Assert.IsNotNull(signedResource);
	}

	[TestMethod]
	public async Task TestDeleteSignedResourceAsync()
	{
		string hash = new Uri(signedUrl).Segments.Last();
		HttpResponseMessage httpResponseMessage = await _ossClient.DeleteSignedResourceAsync(
			accessToken: token,
			hash: hash);
		Assert.IsTrue(httpResponseMessage.StatusCode == HttpStatusCode.OK);
	}

	#endregion














	[TestMethod]
	public async Task UploadChunk()
	{
		try
		{
			const int ChunkSize = 5 * 1024 * 1024; // 5 MB (all but last part must be >= 5MB)

			using var fileStream = File.OpenRead(sourceToUpload!);
			long fileSize = fileStream.Length;
			int totalParts = (int)Math.Ceiling(fileSize / (double)ChunkSize);

			// 1) Get a first batch of signed URLs.
			//    Tip: ask for up to ~25 at a time; reuse uploadKey for subsequent batches.
			int nextPart = 1;
			string? uploadKey = null;
			var http = new HttpClient();

			ObjectDetails? lastPartResult = null;

			while (nextPart <= totalParts)
			{
				int remaining = totalParts - nextPart + 1;
				int batch = Math.Min(25, remaining);

				// SDK call: GET signed S3 upload URLs (returns { uploadKey, urls[] })
				var signed = await _ossClient.SignedS3UploadAsync(
					 accessToken: token,
					 bucketKey: bucketKey,
					 objectKey: objectKey,
					 parts: batch,
					 firstPart: nextPart,
					 uploadKey: uploadKey,          // null for the first call; reuse afterwards
					 minutesExpiration: 10          // optional: up to 60
				);

				uploadKey ??= signed.UploadKey;     // keep the key returned by the first GET
				var urls = signed.Urls;             // one URL per part in this batch

				// 2) PUT each chunk to its URL
				for (int i = 0; i < urls.Count; i++, nextPart++)
				{
					long start = (nextPart - 1L) * ChunkSize;
					long end = Math.Min(start + ChunkSize, fileSize);
					int len = (int)(end - start);

					byte[] buffer = new byte[len];
					fileStream.Seek(start, SeekOrigin.Begin);
					int read = await fileStream.ReadAsync(buffer.AsMemory(0, len));

					using var content = new ByteArrayContent(buffer, 0, read);
					content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
					content.Headers.ContentLength = read;

					using var req = new HttpRequestMessage(HttpMethod.Put, urls[i]) { Content = content };
					using var resp = await http.SendAsync(req);

					if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
					{
						// URLs expired — break out to fetch a fresh batch (same uploadKey)
						nextPart -= i; // rewind to re-upload current part in the next loop
						break;
					}
					resp.EnsureSuccessStatusCode();
				}
			}

			// 3) Complete the upload + attach metadata
			var userMetadata = new
			{
				first = "Tyler",
				last = "Warner",
				building = new { level = new { height = 10 } }
			};
			string userMetadataJson = JsonSerializer.Serialize(userMetadata);

			// You can optionally set other x-ads-meta-* headers here (e.g., Content-Type).
			// Only the accessToken and uploadKey go in the body; metadata goes in headers.
			var response = await _ossClient.CompleteSignedS3UploadAsync(
				 bucketKey: bucketKey,
				 objectKey: objectKey,
				 contentType: "application/json", // the endpoint itself expects JSON
				 body: new Completes3uploadBody { UploadKey = uploadKey! },
				 xAdsUserDefinedMetadata: userMetadataJson,
				 // Optional extra metadata that OSS will store with the object:
				 // xAdsMetaContentType: "application/octet-stream",
				 // xAdsMetaContentDisposition: "attachment; filename=\"myfile.bin\"",
				 // xAdsMetaContentEncoding: "gzip",
				 // xAdsMetaCacheControl: "max-age=3600",
				 accessToken: token
			);


			Assert.IsNotNull(response);
		}
		catch (Exception ex)
		{
			throw;
		}
	}

	[TestMethod]
	public async Task Upload()
	{
		try
		{

			var fileInfo = new FileInfo(sourceToUpload!);
			using var streamSourceToUpload = File.OpenRead(sourceToUpload!);

			const int chunkSize = 5 * 1024 * 1024; // 5MB
			ulong numberOfChunks = (ulong)CalculateNumberOfChunks((ulong)streamSourceToUpload.Length);
			ulong chunksUploaded = 0;

			long start = 0;
			List<string> uploadUrls = [];
			string uploadKey = null;


			using (BinaryReader reader = new(streamSourceToUpload))
			{
				while (chunksUploaded < numberOfChunks)
				{
					long end = Math.Min((long)((chunksUploaded + 1) * chunkSize), streamSourceToUpload.Length);
					byte[] fileBytes = readFileBytes(reader, start, end);


					if (uploadUrls.Count == 0)
					{
						var uploadUrlsResponse = await GetUploadUrlsWithRetry(bucketKey, objectKey, (int)numberOfChunks, (int)chunksUploaded, uploadKey, token);

						uploadKey = uploadUrlsResponse.UploadKey;
						uploadUrls = uploadUrlsResponse.Urls;
					}

					string currentUrl = uploadUrls[0];
					uploadUrls.RemoveAt(0);
					try
					{
						var responseBuffer = await UploadToURL(currentUrl, fileBytes);

						goto NextChunk;
					}
					catch (Exception ex)
					{
						throw new S3ServiceApiException($"", 500);
					}

				NextChunk:
					chunksUploaded++;
					start = end;

					int percentCompleted = (int)(((double)chunksUploaded / (double)numberOfChunks) * 100);
				}
			}

			var metadataObject = new
			{
				first = "Tyler",
				last = "Warner",
				building = new
				{
					level = new { height = 10 }
				}
			};

			string metadataJson = JsonSerializer.Serialize(metadataObject);

			var completeResponse = await _ossClient.CompleteSignedS3UploadAsync(
				 bucketKey: bucketKey,
				 objectKey: objectKey,
				 contentType: "application/json",
				 body: new Completes3uploadBody()
				 {
					 UploadKey = uploadKey
				 },
				 xAdsMetaContentType: "application/json",
				 xAdsUserDefinedMetadata: metadataJson,
				 accessToken: token);

			Assert.IsNotNull(completeResponse);
		}
		catch (Exception ex)
		{
			throw;
		}
	}

	private byte[] readFileBytes(BinaryReader reader, long start, long end)
	{
		double numberOfBytes = end - start;
		byte[] fileBytes = new byte[(int)numberOfBytes];

		reader.BaseStream.Seek(start, SeekOrigin.Begin);
		reader.Read(fileBytes, 0, (int)numberOfBytes);

		return fileBytes;
	}

	private async Task<Signeds3uploadResponse> GetUploadUrlsWithRetry(string bucketKey, string objectKey, int numberOfChunks, int chunksUploaded, string uploadKey, string accessToken)
	{
		var parts = Math.Min(numberOfChunks - chunksUploaded, Constants.BatchSize);
		var firstPart = chunksUploaded + 1;

		try
		{
			var response = await _ossClient.SignedS3UploadAsync(
					bucketKey: bucketKey,
					objectKey: objectKey,
					parts: parts,
					firstPart: firstPart,
					uploadKey: uploadKey,
					accessToken: accessToken);

			return response;
		}
		catch (OssApiException e)
		{
			throw e;
		}

		throw new OssApiException($"");
	}

	private double CalculateNumberOfChunks(ulong fileSize)
	{
		if (fileSize == 0)
		{
			return 1;
		}

		double numberOfChunks = (int)Math.Truncate((double)(fileSize / Constants.ChunkSize));
		if (fileSize % Constants.ChunkSize != 0)
		{
			numberOfChunks++;
		}

		return numberOfChunks;
	}

	private async Task<dynamic> UploadToURL(string url, byte[] buffer)
	{
		var client = new HttpClient();
		var httpContent = new ByteArrayContent(buffer);
		return await _forgeService.Client.PutAsync(url, httpContent);
	}

	static class Constants
	{
		public const int MaxRetry = 5;
		public const ulong ChunkSize = 5 * 1024 * 1024;
		public const int BatchSize = 25;
	}
}