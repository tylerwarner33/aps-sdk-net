using System.Reflection;
using Autodesk.DataManagement.Http;
using Autodesk.DataManagement.Model;
using Autodesk.Forge.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Autodesk.DataManagement.Test;

/// <summary>
/// 	Credential-free unit tests for <see cref="ComparisonType"/>, <see cref="QueryFilter"/>, and
/// 	<see cref="QueryFilterExtensions"/>.
/// 	These do not require ACCESS_TOKEN/PROJECT_ID/FOLDER_ID and run in CI without live credentials.
/// </summary>
[TestClass]
public class TestQueryFilter
{
	[TestMethod]
	public void ToOperatorSuffix_CoversAllDocumentedComparisons()
	{
		Assert.AreEqual("", ComparisonType.Equal.ToOperatorSuffix());
		Assert.AreEqual("-eq", ComparisonType.EqualTo.ToOperatorSuffix());
		Assert.AreEqual("-lt", ComparisonType.LessThan.ToOperatorSuffix());
		Assert.AreEqual("-le", ComparisonType.LessThanOrEqual.ToOperatorSuffix());
		Assert.AreEqual("-ge", ComparisonType.GreaterThanOrEqual.ToOperatorSuffix());
		Assert.AreEqual("-gt", ComparisonType.GreaterThan.ToOperatorSuffix());
		Assert.AreEqual("-starts", ComparisonType.StartsWith.ToOperatorSuffix());
		Assert.AreEqual("-ends", ComparisonType.EndsWith.ToOperatorSuffix());
		Assert.AreEqual("-contains", ComparisonType.Contains.ToOperatorSuffix());
	}

	[TestMethod]
	public void QueryFilter_RendersDocumentedRangeExample()
	{
		QueryFilter lower = QueryFilter.Ge(DataManagementFields.LastModifiedTime, "2016-10-15T08:00");
		QueryFilter upper = QueryFilter.Le(DataManagementFields.LastModifiedTime, "2016-10-15T22:00");

		Assert.AreEqual("filter[lastModifiedTime]-ge", lower.QueryKey);
		Assert.AreEqual("filter[lastModifiedTime]-le", upper.QueryKey);
		Assert.AreEqual("2016-10-15T08:00", lower.QueryValue);
		Assert.AreEqual("2016-10-15T22:00", upper.QueryValue);
		Assert.AreEqual("filter[lastModifiedTime]-ge=2016-10-15T08:00", lower.ToString());
	}

	[TestMethod]
	public void QueryFilter_NormalizesDateTimeToGmtZero()
	{
		DateTime utc = new(2016, 10, 15, 13, 11, 36, DateTimeKind.Utc);
		Assert.AreEqual(
			"2016-10-15T13:11:36.0000000Z",
			QueryFilter.Ge(DataManagementFields.CreateTime, utc).QueryValue);
	}

	[TestMethod]
	public void QueryFilter_TreatsUnspecifiedKindAsUtc()
	{
		DateTime unspecified = new(2016, 10, 15, 13, 11, 36, DateTimeKind.Unspecified);
		Assert.AreEqual(
			"2016-10-15T13:11:36.0000000Z",
			QueryFilter.Ge(DataManagementFields.CreateTime, unspecified).QueryValue);
	}

	[TestMethod]
	public void QueryFilter_AcceptsAbbreviatedAndRegularFieldNames()
	{
		Assert.AreEqual("filter[fileType]", QueryFilter.Eq("fileType", "rvt").QueryKey);
		Assert.AreEqual("filter[attributes.fileType]", QueryFilter.Eq("attributes.fileType", "rvt").QueryKey);
		Assert.AreEqual("filter[refType]", QueryFilter.Eq("refType", "derived").QueryKey);
	}

	[TestMethod]
	public void QueryFilter_ResolvesEnumMemberValues()
	{
		QueryFilter filter = QueryFilter.Eq(DataManagementFields.Type, FilterType.Folders);
		Assert.AreEqual("filter[type]", filter.QueryKey); // Equal -> no suffix, matches legacy
		Assert.AreEqual("folders", filter.QueryValue);
	}

	[TestMethod]
	public void SetFilterParameters_MergesClausesOnTheSameKey()
	{
		Dictionary<string, object> dictionary = new();
		dictionary.SetFilterParameters(new[]
		{
			QueryFilter.Eq(DataManagementFields.FileType, "rvt"),
			QueryFilter.Eq(DataManagementFields.FileType, "jpg"),
		});
		Assert.AreEqual("rvt,jpg", dictionary["filter[fileType]"]);
	}

	[TestMethod]
	[ExpectedException(typeof(ArgumentException))]
	public void SetFilterParameters_ThrowsWhenCollidingWithLegacyParameter()
	{
		Dictionary<string, object> dictionary = new() { { "filter[type]", "folders" } };
		dictionary.SetFilterParameters(new[] { QueryFilter.Eq(DataManagementFields.Type, "items") });
	}

	[TestMethod]
	public void SetFilterParameters_AllowsSameFieldWithDifferentOperators()
	{
		Dictionary<string, object> dictionary = new();
		dictionary.SetFilterParameters(new[]
		{
			QueryFilter.Ge(DataManagementFields.CreateTime, "2016"),
			QueryFilter.Le(DataManagementFields.CreateTime, "2017"),
		});

		Assert.AreEqual(2, dictionary.Count);
		Assert.AreEqual("2016", dictionary["filter[createTime]-ge"]);
		Assert.AreEqual("2017", dictionary["filter[createTime]-le"]);
	}

	[TestMethod]
	public void BuildRequestUri_EncodesComparisonKeys()
	{
		Dictionary<string, object> queryParam = new();
		queryParam.SetFilterParameters(new[]
		{
			QueryFilter.Ge(DataManagementFields.LastModifiedTime, "2016-10-15T08:00"),
			QueryFilter.Le(DataManagementFields.LastModifiedTime, "2016-10-15T22:00"),
		});

		string uri = Marshalling.BuildRequestUri(
			"/data/v1/projects/{project_id}/folders/{folder_id}/contents",
			new Dictionary<string, object> { { "project_id", "p" }, { "folder_id", "f" } },
			queryParam).ToString();

		StringAssert.Contains(uri, "filter%5blastModifiedTime%5d-ge=");
		StringAssert.Contains(uri, "filter%5blastModifiedTime%5d-le=");
	}

	[TestMethod]
	public void EqualComparison_MatchesLegacyRendering()
	{
		Dictionary<string, object> legacy = new() { { "filter[type]", "folders" } };

		Dictionary<string, object> modern = new();
		modern.SetFilterParameters(new[] { QueryFilter.Eq(DataManagementFields.Type, FilterType.Folders) });

		CollectionAssert.AreEquivalent(legacy.Keys.ToList(), modern.Keys.ToList());
		Assert.AreEqual(legacy["filter[type]"], modern["filter[type]"]);
	}

	[TestMethod]
	[ExpectedException(typeof(ArgumentException))]
	public void QueryFilter_RejectsEmptyFieldName() => QueryFilter.Eq("  ", "x");

	[TestMethod]
	[ExpectedException(typeof(ArgumentException))]
	public void QueryFilter_RejectsNoValues() => QueryFilter.Eq(DataManagementFields.Type);

	[TestMethod]
	public void ComparisonType_ExposesExactlyTheDocumentedOperators()
	{
		// Eight documented operators plus the implicit-equality member. The API documents no
		// not-equal operator; this guards against one being invented.
		string[] suffixes = Enum.GetValues<ComparisonType>()
			.Select(comparison => comparison.ToOperatorSuffix())
			.ToArray();

		CollectionAssert.AreEquivalent(
			new[] { "", "-eq", "-lt", "-le", "-ge", "-gt", "-starts", "-ends", "-contains" },
			suffixes);
	}

	[TestMethod]
	public void QueryFilter_JoinsMultipleValuesAsOrWithinTheField()
	{
		// filter[fileType]=rvt,jpg - the documented OR-within-a-field form.
		Assert.AreEqual("rvt,jpg", QueryFilter.Eq(DataManagementFields.FileType, "rvt", "jpg").QueryValue);
	}

	[TestMethod]
	public void QueryFilter_ConvertsDateTimeOffsetToUtc()
	{
		DateTimeOffset offset = new(2016, 10, 15, 21, 11, 36, TimeSpan.FromHours(8));
		Assert.AreEqual(
			"2016-10-15T13:11:36.0000000Z",
			QueryFilter.Ge(DataManagementFields.CreateTime, offset).QueryValue);
	}

	[TestMethod]
	public void QueryFilter_RendersDocumentedCombinedExample()
	{
		// filter[fileType]=rvt,jpg&filter[attributes.fileName]-contains=Floor
		Dictionary<string, object> queryParam = new();
		queryParam.SetFilterParameters(new[]
		{
			QueryFilter.Eq(DataManagementFields.FileType, "rvt", "jpg"),
			QueryFilter.Contains("attributes.fileName", "Floor"),
		});

		Assert.AreEqual("rvt,jpg", queryParam["filter[fileType]"]);
		Assert.AreEqual("Floor", queryParam["filter[attributes.fileName]-contains"]);
	}

	[TestMethod]
	public void SetFilterParameters_TreatsFieldNamesAsCaseSensitive()
	{
		// The service is case sensitive, so differing case must stay two distinct keys.
		Dictionary<string, object> queryParam = new();
		queryParam.SetFilterParameters(new[]
		{
			QueryFilter.Eq("fileType", "rvt"),
			QueryFilter.Eq("filetype", "jpg"),
		});

		Assert.AreEqual(2, queryParam.Count);
		Assert.AreEqual("rvt", queryParam["filter[fileType]"]);
		Assert.AreEqual("jpg", queryParam["filter[filetype]"]);
	}

	[TestMethod]
	public void SetFilterParameters_IgnoresNullCollectionAndNullClauses()
	{
		Dictionary<string, object> queryParam = new();
		queryParam.SetFilterParameters(null);
		Assert.AreEqual(0, queryParam.Count);

		queryParam.SetFilterParameters(new QueryFilter[] { null! });
		Assert.AreEqual(0, queryParam.Count);
	}

	[TestMethod]
	[ExpectedException(typeof(ArgumentNullException))]
	public void SetFilterParameters_RejectsANullDictionary()
	{
		IDictionary<string, object> queryParam = null!;
		queryParam.SetFilterParameters(new[] { QueryFilter.Eq(DataManagementFields.Type, "folders") });
	}

	[TestMethod]
	public void QueryFilter_ExpandsACollectionPassedAsASingleValue()
	{
		// Eq(field, list) must mean the same as Eq(field, item, item), not one stringified
		// object. Binding a List<string> to `params object[]` would otherwise render
		// "System.Collections.Generic.List`1[System.String]" and silently match nothing.
		List<string> ids = new() { "urn:one", "urn:two" };

		Assert.AreEqual("urn:one,urn:two", QueryFilter.Eq(DataManagementFields.Id, ids).QueryValue);
		Assert.AreEqual(
			QueryFilter.Eq(DataManagementFields.Id, "urn:one", "urn:two").QueryValue,
			QueryFilter.Eq(DataManagementFields.Id, ids).QueryValue);
	}

	[TestMethod]
	public void QueryFilter_CollapsesDuplicateValuesSoQueryValueMatchesTheWire()
	{
		QueryFilter filter = QueryFilter.Eq(DataManagementFields.FileType, "rvt", "rvt", "jpg");
		Assert.AreEqual("rvt,jpg", filter.QueryValue);

		Dictionary<string, object> queryParam = new();
		queryParam.SetFilterParameters(new[] { filter });

		// What ToString() reports must be what is actually sent.
		Assert.AreEqual(filter.QueryValue, queryParam[filter.QueryKey]);
	}

	[TestMethod]
	[ExpectedException(typeof(ArgumentOutOfRangeException))]
	public void QueryFilter_RejectsAnUndefinedComparisonAtTheCallSite()
	{
		// Must fail here, not later from inside request building.
		_ = new QueryFilter(DataManagementFields.Type, (ComparisonType)99, "folders");
	}

	[TestMethod]
	public void QueryFilter_ConvertsLocalDateTimeToUtc()
	{
		DateTime local = new DateTime(2016, 10, 15, 13, 11, 36, DateTimeKind.Utc).ToLocalTime();
		Assert.AreEqual(
			"2016-10-15T13:11:36.0000000Z",
			QueryFilter.Ge(DataManagementFields.CreateTime, local).QueryValue);
	}

	[TestMethod]
	public void QueryFilter_ConvertsNumbersInvariantlyAndBooleansLowerCase()
	{
		// A comma-decimal locale must not produce "1,5".
		Assert.AreEqual("1.5", QueryFilter.Gt(DataManagementFields.StorageSize, 1.5d).QueryValue);
		Assert.AreEqual("0", QueryFilter.Ge(DataManagementFields.VersionNumber, 0).QueryValue);
		Assert.AreEqual("true", QueryFilter.Eq(DataManagementFields.Hidden, true).QueryValue);
		Assert.AreEqual("false", QueryFilter.Eq(DataManagementFields.Hidden, false).QueryValue);
	}

	/// <summary>
	/// 	Every operation that gained filtering, as contract type and method name.
	/// </summary>
	private static IEnumerable<(Type Contract, string Method)> FilterOperations()
	{
		yield return (typeof(IHubsApi), nameof(IHubsApi.GetHubsAsync));
		yield return (typeof(IProjectsApi), nameof(IProjectsApi.GetHubProjectsAsync));
		yield return (typeof(IFoldersApi), nameof(IFoldersApi.GetFolderContentsAsync));
		yield return (typeof(IFoldersApi), nameof(IFoldersApi.GetFolderRefsAsync));
		yield return (typeof(IFoldersApi), nameof(IFoldersApi.GetFolderRelationshipsRefsAsync));
		yield return (typeof(IFoldersApi), nameof(IFoldersApi.GetFolderSearchAsync));
		yield return (typeof(IItemsApi), nameof(IItemsApi.GetItemRefsAsync));
		yield return (typeof(IItemsApi), nameof(IItemsApi.GetItemRelationshipsRefsAsync));
		yield return (typeof(IItemsApi), nameof(IItemsApi.GetItemVersionsAsync));
		yield return (typeof(IVersionsApi), nameof(IVersionsApi.GetVersionDownloadsAsync));
		yield return (typeof(IVersionsApi), nameof(IVersionsApi.GetVersionRefsAsync));
		yield return (typeof(IVersionsApi), nameof(IVersionsApi.GetVersionRelationshipsRefsAsync));
	}

	[TestMethod]
	public void EveryFilterOperation_KeepsItsOriginalSignatureAlongsideTheFiltersOverload()
	{
		// The original overload must survive untouched. Extending it instead would throw
		// MissingMethodException in callers already compiled against the previous release.
		foreach ((Type contract, string method) in FilterOperations())
		{
			MethodInfo[] overloads = contract.GetMethods()
				.Where(candidate => candidate.Name == method)
				.ToArray();

			Assert.AreEqual(2, overloads.Length, $"{contract.Name}.{method} overload count");

			Assert.AreEqual(
				1,
				overloads.Count(candidate => candidate.GetParameters().Any(IsFilters)),
				$"{contract.Name}.{method} should have exactly one filters overload");
		}
	}

	[TestMethod]
	public void FiltersParameter_IsRequiredAndFollowsTheRequiredParameters()
	{
		// A required `filters` placed just past the required parameters is what keeps an
		// existing call from matching both overloads (CS0121).
		foreach ((Type contract, string method) in FilterOperations())
		{
			MethodInfo overload = contract.GetMethods()
				.Single(candidate => candidate.Name == method
					&& candidate.GetParameters().Any(IsFilters));

			ParameterInfo[] parameters = overload.GetParameters();
			int index = Array.FindIndex(parameters, IsFilters);

			Assert.IsFalse(
				parameters[index].IsOptional,
				$"{contract.Name}.{method} filters must be required");

			Assert.IsTrue(
				parameters.Take(index).All(parameter => !parameter.IsOptional),
				$"{contract.Name}.{method} filters must follow the required parameters");

			Assert.IsTrue(
				parameters.Skip(index + 1).All(parameter => parameter.IsOptional),
				$"{contract.Name}.{method} every parameter after filters must stay optional");
		}
	}

	private static bool IsFilters(ParameterInfo parameter)
	{
		return parameter.ParameterType == typeof(IEnumerable<QueryFilter>);
	}

	[TestMethod]
	public void EveryDataManagementClientWrapper_AlsoGainsTheFiltersOverload()
	{
		// The generated interfaces are covered above; the hand-written client wrappers are the
		// surface most callers actually use, so they need the same guarantee.
		foreach ((Type _, string method) in FilterOperations())
		{
			MethodInfo[] overloads = typeof(DataManagementClient).GetMethods()
				.Where(candidate => candidate.Name == method)
				.ToArray();

			Assert.AreEqual(2, overloads.Length, $"DataManagementClient.{method} overload count");

			MethodInfo overload = overloads.Single(candidate =>
				candidate.GetParameters().Any(IsFilters));

			ParameterInfo[] parameters = overload.GetParameters();
			int index = Array.FindIndex(parameters, IsFilters);

			Assert.IsFalse(
				parameters[index].IsOptional,
				$"DataManagementClient.{method} filters must be required");

			Assert.IsTrue(
				parameters.Take(index).All(parameter => !parameter.IsOptional),
				$"DataManagementClient.{method} filters must follow the required parameters");
		}
	}

	/// <summary>
	/// 	Compile-time guard. This method is never called - its purpose is to fail the build if
	/// 	adding the overloads ever makes one of these call shapes ambiguous (CS0121) or
	/// 	unresolvable.
	/// 	Every call below is one that compiled against the previous release.
	/// </summary>
	/// <remarks>
	/// 	This guard does NOT cover every previously valid call.
	/// 	A call that passed a bare <c>null</c> literal positionally where <c>filters</c> now sits
	/// 	- <c>GetFolderContentsAsync("p", "f", null)</c> - is genuinely ambiguous now and cannot
	/// 	be written here, because it would not compile.
	/// 	That is the one accepted source-level break; a named argument or a cast resolves it.
	/// 	See <see cref="NullLiteralAtTheFiltersPosition_IsTheOneAcceptedSourceBreak"/>.
	/// </remarks>
	private static void ExistingCallShapesMustKeepCompiling(
		IFoldersApi folders,
		IHubsApi hubs,
		IVersionsApi versions)
	{
		_ = folders.GetFolderContentsAsync("p", "f");
		_ = folders.GetFolderContentsAsync("p", "f", "user");
		_ = folders.GetFolderContentsAsync("p", "f", xUserId: "user");
		_ = folders.GetFolderContentsAsync("p", "f", accessToken: "token");
		_ = folders.GetFolderContentsAsync("p", "f", "user", null, null, null, null, 0, 200, false, "token", true);
		_ = folders.GetFolderSearchAsync("p", "f", "createUserName", new List<string> { "Ada" }, 0);
		_ = hubs.GetHubsAsync();
		_ = hubs.GetHubsAsync("user");
		_ = versions.GetVersionDownloadsAsync("p", "v");

		// The documented workarounds for the ambiguous null-literal shape must both compile.
		_ = folders.GetFolderContentsAsync("p", "f", (string)null!);
		_ = folders.GetFolderContentsAsync("p", "f", xUserId: null);

		// And the new shapes must reach the filters overload.
		QueryFilter[] filters = { QueryFilter.Ge(DataManagementFields.CreateTime, "2016") };
		_ = folders.GetFolderContentsAsync("p", "f", filters);
		_ = folders.GetFolderContentsAsync("p", "f", filters, accessToken: "token");
		_ = hubs.GetHubsAsync(filters);
		_ = versions.GetVersionDownloadsAsync("p", "v", filters);
	}

	[TestMethod]
	public void NullLiteralAtTheFiltersPosition_IsTheOneAcceptedSourceBreak()
	{
		// Documents the single accepted source-level break, so it is a recorded decision rather
		// than a surprise. `GetFolderContentsAsync("p", "f", null)` compiled before and is now
		// CS0121, because the literal converts to both `string xUserId` and
		// `IEnumerable<QueryFilter> filters`. Nothing here can assert a compile error, so assert
		// the shape that causes it: the parameter displaced by `filters` is a reference type,
		// which is what makes a bare null convertible to both.
		foreach ((Type contract, string method) in FilterOperations())
		{
			MethodInfo overload = contract.GetMethods()
				.Single(candidate => candidate.Name == method
					&& candidate.GetParameters().Any(IsFilters));

			ParameterInfo[] parameters = overload.GetParameters();
			int index = Array.FindIndex(parameters, IsFilters);

			MethodInfo original = contract.GetMethods()
				.Single(candidate => candidate.Name == method
					&& !candidate.GetParameters().Any(IsFilters));

			ParameterInfo displaced = original.GetParameters()[index];

			Assert.IsTrue(
				!displaced.ParameterType.IsValueType,
				$"{contract.Name}.{method} displaced parameter '{displaced.Name}' is a reference "
					+ "type, so a bare null literal at this position is ambiguous. Documented.");
		}
	}

	[TestMethod]
	public void SetFilterParameters_LeavesLegacyParametersUntouchedWhenFieldsDiffer()
	{
		// The legacy parameter and the clause target different fields, so both must survive.
		Dictionary<string, object> queryParam = new() { { "filter[extension.type]", "items:autodesk.core:File" } };
		queryParam.SetFilterParameters(new[]
		{
			QueryFilter.Ge(DataManagementFields.LastModifiedTime, "2016-10-15T08:00"),
		});

		Assert.AreEqual(2, queryParam.Count);
		Assert.AreEqual("items:autodesk.core:File", queryParam["filter[extension.type]"]);
		Assert.AreEqual("2016-10-15T08:00", queryParam["filter[lastModifiedTime]-ge"]);
	}
}
