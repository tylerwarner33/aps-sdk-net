using System;

namespace Autodesk.DataManagement.Model;

/// <summary>
/// 	Comparison operators supported by the Data Management <c>filter</c> query parameter.
/// 	The operator is appended to the query key, not the value -
/// 	ex. <c>filter[lastModifiedTime]-ge=2016-10-15</c>.
/// </summary>
/// <remarks>
/// 	Deliberately not decorated with <c>[EnumMember]</c>.
/// 	The generated <c>SetQueryParameter</c> helpers resolve <c>[EnumMember]</c> into the query value;
/// 	this enum must be resolved into the query key instead, via
/// 	<see cref="ComparisonTypeExtensions.ToOperatorSuffix"/>.
/// 	These eight operators are the complete documented set.
/// 	The API has no not-equal operator, so none is offered here.
/// 	Comparisons use lexicographic ordering, and field names and match values are case sensitive.
/// 	See https://aps.autodesk.com/en/docs/data/v2/developers_guide/filtering/#comparison-types
/// </remarks>
public enum ComparisonType
{
	/// <summary>Implicit equality - emits no suffix (<c>filter[field]=value</c>).</summary>
	Equal = 0,

	/// <summary>Explicit equality - <c>-eq</c>.</summary>
	EqualTo,

	/// <summary>Less than - <c>-lt</c>.</summary>
	LessThan,

	/// <summary>Less than or equal to - <c>-le</c>.</summary>
	LessThanOrEqual,

	/// <summary>Greater than or equal to - <c>-ge</c>.</summary>
	GreaterThanOrEqual,

	/// <summary>Greater than - <c>-gt</c>.</summary>
	GreaterThan,

	/// <summary>String starts with - <c>-starts</c>.</summary>
	StartsWith,

	/// <summary>String ends with - <c>-ends</c>.</summary>
	EndsWith,

	/// <summary>String contains - <c>-contains</c>.</summary>
	Contains
}

/// <summary>
/// 	Extension helpers for <see cref="ComparisonType"/>.
/// </summary>
public static class ComparisonTypeExtensions
{
	/// <summary>
	/// 	Returns the suffix appended to the <c>filter[field]</c> query key, or an empty string for
	/// 	<see cref="ComparisonType.Equal"/>.
	/// </summary>
	/// <param name="comparison">
	/// 	Comparison operator to resolve.
	/// </param>
	/// <returns>
	/// 	The operator suffix, ex. <c>-ge</c>, or an empty string for <see cref="ComparisonType.Equal"/>.
	/// </returns>
	public static string ToOperatorSuffix(this ComparisonType comparison)
	{
		switch (comparison)
		{
			case ComparisonType.Equal: return string.Empty;
			case ComparisonType.EqualTo: return "-eq";
			case ComparisonType.LessThan: return "-lt";
			case ComparisonType.LessThanOrEqual: return "-le";
			case ComparisonType.GreaterThanOrEqual: return "-ge";
			case ComparisonType.GreaterThan: return "-gt";
			case ComparisonType.StartsWith: return "-starts";
			case ComparisonType.EndsWith: return "-ends";
			case ComparisonType.Contains: return "-contains";
			default:
				throw new ArgumentOutOfRangeException(
					nameof(comparison), comparison, "Unsupported comparison type.");
		}
	}
}
