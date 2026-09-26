using Autodesk.Aps.TestCommon;
using Autodesk.ModelDerivative.Model;
using Autodesk.SDKManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Autodesk.ModelDerivative.Test;

[TestClass]
public class ModelDerivativeClientTests
{
    private const string Scopes = "data:read data:write data:create";

    private static ModelDerivativeClient _modelDerivativeClient = null!;
    private static string _accessToken = null!;
    private static Region _region;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext testContext)
    {
        Autodesk.SDKManager.SDKManager sdkManager = SdkManagerBuilder
            .Create()
            .Add(new ApsConfiguration())
            .Add(ResiliencyConfiguration.CreateDefault())
            .Build();

        _modelDerivativeClient = new ModelDerivativeClient(sdkManager);
        _accessToken = await ApsTestTokens.GetTwoLeggedAsync(Scopes);
        _region = Enum.Parse<Region>(ApsTestConfig.Region, ignoreCase: true);
    }

    [TestMethod]
    public async Task GetFormatsAsync_WhenCalled_ReturnsFormatDictionary()
    {
        SupportedFormats supportedFormats = await _modelDerivativeClient.GetFormatsAsync(accessToken: _accessToken);

        Assert.IsInstanceOfType(supportedFormats.Formats, typeof(Dictionary<string, List<string>>));
    }

    /// <summary>
    /// 	Starts a translation job for an untranslated source model.
    /// </summary>
    /// <remarks>
    /// 	This is the only billed call in the whole repository. Translation consumes cloud credits.
    /// </remarks>
    [TestMethod]
    public async Task StartJobAsync_WithSvf2AndThumbnailOutputs_ReturnsCreatedResult()
    {
        if (ApsTestConfig.RunTranslationTest is false)
        {
            Assert.Inconclusive("The translation test is switched off by MD_RUN_TRANSLATION_TEST.");
        }

        List<IJobPayloadFormat> outputFormats =
        [
            new JobPayloadFormatSVF2
            {
                Views = [View._2d, View._3d],
                Advanced = new JobPayloadFormatSVF2AdvancedRVT
                {
                    GenerateMasterViews = true
                }
            },
            new JobPayloadFormatThumbnail
            {
                Advanced = new JobPayloadFormatAdvancedThumbnail
                {
                    Width = Width.NUMBER_100,
                    Height = Height.NUMBER_100
                }
            }
        ];
        JobPayload jobPayload = new()
        {
            Input = new JobPayloadInput
            {
                Urn = ApsTestConfig.ModelDerivativeSourceUrn,
                CompressedUrn = false
            },
            Output = new JobPayloadOutput
            {
                Formats = outputFormats
            }
        };

        Job jobResponse = await _modelDerivativeClient.StartJobAsync(
            jobPayload: jobPayload,
            accessToken: _accessToken,
            region: _region);

        Assert.AreEqual("created", jobResponse.Result);
    }

    [TestMethod]
    public async Task GetManifestAsync_ForTranslatedUrn_ReturnsCompleteProgress()
    {
        Manifest manifestResponse = await _modelDerivativeClient.GetManifestAsync(
            ApsTestConfig.ModelDerivativeTranslatedUrn,
            region: _region,
            accessToken: _accessToken);

        Assert.AreEqual("complete", manifestResponse.Progress);
    }

    [TestMethod]
    public async Task GetModelViewsAsync_ForTranslatedUrn_ReturnsMetadata()
    {
        ModelViews modelViewsResponse = await _modelDerivativeClient.GetModelViewsAsync(
            ApsTestConfig.ModelDerivativeTranslatedUrn,
            region: _region,
            accessToken: _accessToken);

        Assert.IsTrue(modelViewsResponse.Data.Metadata.Count > 0);
    }

    [TestMethod]
    public async Task GetThumbnailAsync_ForTranslatedUrn_ReturnsImageContent()
    {
        using Stream thumbnail = await _modelDerivativeClient.GetThumbnailAsync(
            ApsTestConfig.ModelDerivativeTranslatedUrn,
            Width.NUMBER_100,
            Height.NUMBER_100,
            _region,
            accessToken: _accessToken);
        using MemoryStream content = new();
        await thumbnail.CopyToAsync(content);

        Assert.IsTrue(content.Length > 0);
    }
}
