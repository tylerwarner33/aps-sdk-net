using System;
using System.Collections.Generic;
using System.Linq;

namespace Autodesk.DataManagement.Model;

/// <summary>
/// 	Helpers for writing <see cref="QueryFilter"/> clauses into a query dictionary.
/// </summary>
public static class QueryFilterExtensions
{
	/// <summary>
	/// 	Writes filter clauses into a query parameter dictionary.
	/// 	Call this after all legacy <c>SetQueryParameter</c> calls for the operation.
	/// </summary>
	/// <remarks>
	/// 	Clauses within <paramref name="filters"/> that render to the same key have their values
	/// 	merged, preserving the API's OR-within-a-field semantics.
	/// 	A clause that collides with a key already populated by a legacy <c>filter*</c> parameter
	/// 	throws, rather than silently changing the query's meaning.
	/// 	Supply the field through one mechanism or the other.
	/// 	The check compares rendered keys, so it does not catch a clause that names the same
	/// 	underlying field in its other form - a legacy <c>filterExtensionType</c> renders
	/// 	<c>filter[extension.type]</c> and does not collide with a clause on
	/// 	<c>attributes.extension.type</c>. Both are then sent and ANDed by the service.
	/// </remarks>
	/// <param name="dictionary">
	/// 	Query parameter dictionary to write into.
	/// </param>
	/// <param name="filters">
	/// 	Filter clauses to render. A null value is a no-op.
	/// </param>
	/// <exception cref="ArgumentNullException">
	/// 	<paramref name="dictionary"/> is null.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// 	A clause collides with a legacy filter parameter on the same rendered key.
	/// </exception>
	public static void SetFilterParameters(
		this IDictionary<string, object> dictionary,
		IEnumerable<QueryFilter> filters)
	{
		if (dictionary is null)
		{
			throw new ArgumentNullException(nameof(dictionary));
		}

		if (filters is null)
		{
			return;
		}

		// Stage first so that merging happens only among the supplied clauses, and any collision
		// with a legacy parameter is detected rather than merged into.
		Dictionary<string, List<string>> staged = new(StringComparer.Ordinal);

		foreach (QueryFilter filter in filters)
		{
			if (filter is null)
			{
				continue;
			}

			if (!staged.TryGetValue(filter.QueryKey, out List<string> values))
			{
				values = new List<string>();
				staged[filter.QueryKey] = values;
			}

			values.AddRange(filter.Values);
		}

		foreach (KeyValuePair<string, List<string>> entry in staged)
		{
			if (dictionary.ContainsKey(entry.Key))
			{
				throw new ArgumentException(
					$"The filter '{entry.Key}' was supplied both through a legacy filter " +
					"parameter and through the filters collection. Use one or the other.",
					nameof(filters));
			}

			// Each clause already holds non-empty, de-duplicated values. De-duplicate once more
			// across merged clauses so the rendered value matches QueryFilter.QueryValue.
			dictionary[entry.Key] = string.Join(",", entry.Value.Distinct(StringComparer.Ordinal));
		}
	}
}
