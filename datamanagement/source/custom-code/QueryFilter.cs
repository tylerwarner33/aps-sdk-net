using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace Autodesk.DataManagement.Model;

/// <summary>
/// 	A single filter clause: a field name, a comparison operator, and one or more match values.
/// 	Renders to a <c>filter[field]-op=value,value</c> query parameter.
/// </summary>
/// <remarks>
/// 	Combining clauses on different fields is an AND.
/// 	Supplying multiple values to a single clause is an OR within that field.
/// 	Two clauses on the same field with different operators also AND, which is how the documented
/// 	date-range query is expressed.
/// 	Field names may use either the regular or the abbreviated form - the <c>attributes.</c> and
/// 	<c>meta.</c> prefixes are optional.
/// 	Both <c>attributes.fileType</c> and <c>fileType</c> are accepted by the service; names are
/// 	passed through verbatim.
/// 	Field names and match values are case sensitive, and comparisons use lexicographic ordering.
/// 	See <see cref="DataManagementFields"/> for the common ones.
/// 	See https://aps.autodesk.com/en/docs/data/v2/developers_guide/filtering/
/// </remarks>
public sealed class QueryFilter
{
    private static readonly char[] ReservedFieldNameCharacters = ['[', ']', '=', '&', '?', '#', ','];

    /// <summary>
    /// 	The field being filtered, ex. <c>lastModifiedTime</c>, <c>type</c>, <c>fileType</c>,
    /// 	<c>attributes.displayName</c>, <c>extension.type</c>.
    /// 	Do not include the surrounding <c>filter[]</c>.
    /// </summary>
    public string FieldName { get; }

    /// <summary>The comparison operator applied to <see cref="Values"/>.</summary>
    public ComparisonType Comparison { get; }

    /// <summary>
    /// 	One or more match values, comma-joined on render.
    /// 	Multiple values behave as a logical OR within this field.
    /// </summary>
    public IReadOnlyList<string> Values { get; }

    /// <summary>The rendered query key, ex. <c>filter[lastModifiedTime]-ge</c>.</summary>
    public string QueryKey => $"filter[{FieldName}]{Comparison.ToOperatorSuffix()}";

    /// <summary>The rendered query value, ex. <c>2016-10-15</c>.</summary>
    public string QueryValue => string.Join(",", Values);

    /// <summary>
    /// 	Creates a filter clause.
    /// </summary>
    /// <param name="fieldName">
    /// 	Field to filter on, regular or abbreviated form.
    /// </param>
    /// <param name="comparison">
    /// 	Comparison operator.
    /// </param>
    /// <param name="values">
    /// 	One or more match values.
    /// 	<see cref="string"/>, <see cref="DateTime"/>, <see cref="DateTimeOffset"/>,
    /// 	<see cref="bool"/>, numeric types, and <c>[EnumMember]</c>-decorated enums such as
    /// 	<see cref="FilterType"/> are accepted.
    /// 	A collection passed as a single value is expanded into its elements, so
    /// 	<c>Eq(fieldName, listOfIds)</c> and <c>Eq(fieldName, id1, id2)</c> are equivalent.
    /// 	Duplicate values are collapsed.
    /// 	The service separates values with a comma and has no escape for it, so a value that contains
    /// 	a comma is read as two values.
    /// </param>
    /// <exception cref="ArgumentException">
    /// 	<paramref name="fieldName"/> is empty or contains a bracket, a query delimiter, a comma, or white space,
    /// 	or no non-null value remains after conversion.
    /// </exception>
    public QueryFilter(string fieldName, ComparisonType comparison, params object[] values)
        : this(fieldName, comparison, (IEnumerable<object>)values)
    {
    }

    /// <inheritdoc cref="QueryFilter(string, ComparisonType, object[])"/>
    public QueryFilter(string fieldName, ComparisonType comparison, IEnumerable<object> values)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            throw new ArgumentException("A filter field name is required.", nameof(fieldName));
        }

        // A field name is a dotted path, ex. attributes.extension.type. A bracket, a query delimiter,
        // or white space is always a mistake, most often a name passed as filter[type] rather than type.
        // The key is URL-encoded, so this is not an injection defense. It turns a filter the service
        // would silently ignore into an error at the call site.
        string trimmedFieldName = fieldName.Trim();

        if (trimmedFieldName.IndexOfAny(ReservedFieldNameCharacters) >= 0
            || trimmedFieldName.Any(character => char.IsWhiteSpace(character) || char.IsControl(character)))
        {
            throw new ArgumentException(
                $"The filter field name '{trimmedFieldName}' is not valid. " +
                "Pass the field name alone, ex. 'type', without 'filter[]', operators, or white space.",
                nameof(fieldName));
        }

        // Resolve the suffix now so an undefined operator fails at the call site rather than
        // later, from inside request building.
        comparison.ToOperatorSuffix();

        List<string> converted = Flatten(values)
            .Select(ConvertValue)
            .Where(value => !string.IsNullOrEmpty(value))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (converted.Count == 0)
        {
            throw new ArgumentException(
                $"At least one non-null match value is required for filter '{fieldName}'.",
                nameof(values));
        }

        this.FieldName = trimmedFieldName;
        this.Comparison = comparison;
        this.Values = converted.AsReadOnly();
    }

    /// <summary>
    /// 	Expands any nested collection into its elements, so a list handed to the
    /// 	<c>params</c> factories counts as many values rather than one stringified object.
    /// 	Strings are values, never collections of characters.
    /// </summary>
    private static IEnumerable<object> Flatten(IEnumerable<object> values)
    {
        if (values is null)
        {
            yield break;
        }

        foreach (object value in values)
        {
            if (value is null)
            {
                continue;
            }

            if (value is string || value is not System.Collections.IEnumerable nested)
            {
                yield return value;
                continue;
            }

            foreach (object item in nested)
            {
                if (item is not null)
                {
                    yield return item;
                }
            }
        }
    }

    // --- Convenience factories -------------------------------------------------------

    /// <summary>Implicit equality: <c>filter[field]=value</c>.</summary>
    public static QueryFilter Eq(string fieldName, params object[] values)
    {
        return new QueryFilter(fieldName, ComparisonType.Equal, values);
    }

    /// <summary>Explicit equality: <c>filter[field]-eq=value</c>.</summary>
    public static QueryFilter EqualTo(string fieldName, params object[] values)
    {
        return new QueryFilter(fieldName, ComparisonType.EqualTo, values);
    }

    /// <summary>Less than: <c>filter[field]-lt=value</c>.</summary>
    public static QueryFilter Lt(string fieldName, params object[] values)
    {
        return new QueryFilter(fieldName, ComparisonType.LessThan, values);
    }

    /// <summary>Less than or equal: <c>filter[field]-le=value</c>.</summary>
    public static QueryFilter Le(string fieldName, params object[] values)
    {
        return new QueryFilter(fieldName, ComparisonType.LessThanOrEqual, values);
    }

    /// <summary>Greater than or equal: <c>filter[field]-ge=value</c>.</summary>
    public static QueryFilter Ge(string fieldName, params object[] values)
    {
        return new QueryFilter(fieldName, ComparisonType.GreaterThanOrEqual, values);
    }

    /// <summary>Greater than: <c>filter[field]-gt=value</c>.</summary>
    public static QueryFilter Gt(string fieldName, params object[] values)
    {
        return new QueryFilter(fieldName, ComparisonType.GreaterThan, values);
    }

    /// <summary>String starts with: <c>filter[field]-starts=value</c>.</summary>
    public static QueryFilter StartsWith(string fieldName, params object[] values)
    {
        return new QueryFilter(fieldName, ComparisonType.StartsWith, values);
    }

    /// <summary>String ends with: <c>filter[field]-ends=value</c>.</summary>
    public static QueryFilter EndsWith(string fieldName, params object[] values)
    {
        return new QueryFilter(fieldName, ComparisonType.EndsWith, values);
    }

    /// <summary>String contains: <c>filter[field]-contains=value</c>.</summary>
    public static QueryFilter Contains(string fieldName, params object[] values)
    {
        return new QueryFilter(fieldName, ComparisonType.Contains, values);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{QueryKey}={QueryValue}";
    }

    // --- Value conversion ------------------------------------------------------------

    /// <summary>
    /// 	Follows the conversion rules used by the generated <c>SetQueryParameter</c> and
    /// 	<c>LocalMarshalling.ParameterToString</c> helpers, so migrating a string or enum filter
    /// 	to <see cref="QueryFilter"/> produces an identical URL.
    /// </summary>
    /// <remarks>
    /// 	Three deliberate differences from the generated helper.
    /// 	Date-times are normalized to GMT-0, as the Filtering guide requires.
    /// 	A <see cref="bool"/> renders lower-case, where the generated helper would render
    /// 	<c>True</c>.
    /// 	An <see cref="int"/> of zero or less is sent, where the generated helper drops it.
    /// </remarks>
    private static string ConvertValue(object value)
    {
        switch (value)
        {
            case null:
                return null;
            case string stringValue:
                return stringValue;
            case DateTime dateTime:
                return FormatUtc(dateTime.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
                    : dateTime.ToUniversalTime());
            case DateTimeOffset dateTimeOffset:
                return FormatUtc(dateTimeOffset.UtcDateTime);
            case bool boolValue:
                return boolValue ? "true" : "false";
            case Enum enumValue:
                return ToEnumMemberValue(enumValue);
            default:
                return Convert.ToString(value, CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// 	Formats as ISO 8601 normalized to GMT-0, ex. <c>2016-10-15T13:11:36.0000000Z</c>.
    /// 	The service assumes UTC for values with no timezone suffix; emitting an explicit <c>Z</c>
    /// 	removes the ambiguity.
    /// </summary>
    private static string FormatUtc(DateTime utc)
    {
        return utc.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// 	Resolves an <c>[EnumMember]</c> value, falling back to the member name.
    /// </summary>

    private static string ToEnumMemberValue(Enum value)
    {
        Type type = value.GetType();
        System.Reflection.MemberInfo member = type
            .GetMember(value.ToString())
            .FirstOrDefault(memberInfo => memberInfo.DeclaringType == type);

        EnumMemberAttribute attribute = member?
            .GetCustomAttributes(typeof(EnumMemberAttribute), false)
            .OfType<EnumMemberAttribute>()
            .FirstOrDefault();

        return attribute?.Value ?? value.ToString();
    }
}
