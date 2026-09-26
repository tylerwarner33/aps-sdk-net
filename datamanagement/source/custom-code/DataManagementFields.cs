namespace Autodesk.DataManagement.Model;

/// <summary>
/// 	Common Data Management filter field names, in abbreviated form.
/// 	The <c>attributes.</c> and <c>meta.</c> prefixes may be omitted - <c>filter[fileType]</c> and
/// 	<c>filter[attributes.fileType]</c> are equivalent, as are <c>filter[refType]</c> and
/// 	<c>filter[meta.refType]</c>.
/// </summary>
/// <remarks>
/// 	Field names and match values are case sensitive.
/// 	Comparisons use lexicographic ordering, so relational operators on numeric fields such as
/// 	<see cref="StorageSize"/> and <see cref="VersionNumber"/> order as text, not as numbers.
/// 	Not every field is filterable on every operation; see the reference page for the operation.
/// 	See https://aps.autodesk.com/en/docs/data/v2/developers_guide/filtering/
/// </remarks>
public static class DataManagementFields
{
    /// <summary>Resource type, ex. <c>folders</c> or <c>items</c>.</summary>
    public const string Type = "type";

    /// <summary>Resource identifier.</summary>
    public const string Id = "id";

    /// <summary>
    /// 	Extension type, ex. <c>items:autodesk.core:File</c>.
    /// </summary>
    /// <remarks>
    /// 	This abbreviated name is ambiguous on the <c>refs</c> and <c>relationships/refs</c>
    /// 	operations, which carry both <c>attributes.extension</c> and <c>meta.extension</c>.
    /// 	Write the qualified name on those operations.
    /// </remarks>
    public const string ExtensionType = "extension.type";

    /// <summary>
    /// 	Extension version.
    /// </summary>
    /// <remarks>
    /// 	Ambiguous on the <c>refs</c> operations, for the reason given on <see cref="ExtensionType"/>.
    /// </remarks>
    public const string ExtensionVersion = "extension.version";

    /// <summary>Creation timestamp. Date-time comparisons apply.</summary>
    public const string CreateTime = "createTime";

    /// <summary>Identifier of the user who created the resource.</summary>
    public const string CreateUserId = "createUserId";

    /// <summary>Name of the user who created the resource.</summary>
    public const string CreateUserName = "createUserName";

    /// <summary>Last modification timestamp. Date-time comparisons apply.</summary>
    public const string LastModifiedTime = "lastModifiedTime";

    /// <summary>Identifier of the user who last modified the resource.</summary>
    public const string LastModifiedUserId = "lastModifiedUserId";

    /// <summary>Name of the user who last modified the resource.</summary>
    public const string LastModifiedUserName = "lastModifiedUserName";

    /// <summary>Roll-up of last modification across an item's versions.</summary>
    public const string LastModifiedTimeRollup = "lastModifiedTimeRollup";

    /// <summary>Display name.</summary>
    public const string DisplayName = "displayName";

    /// <summary>File name.</summary>
    public const string FileName = "fileName";

    /// <summary>File type, ex. <c>rvt</c>, <c>jpg</c>.</summary>
    public const string FileType = "fileType";

    /// <summary>MIME type.</summary>
    public const string MimeType = "mimeType";

    /// <summary>
    /// 	Stored size in bytes.
    /// </summary>
    /// <remarks>
    /// 	Compares lexicographically, not numerically.
    /// </remarks>
    public const string StorageSize = "storageSize";

    /// <summary>
    /// 	Version number.
    /// </summary>
    /// <remarks>
    /// 	Compares lexicographically, not numerically.
    /// </remarks>
    public const string VersionNumber = "versionNumber";

    /// <summary>Hub or project name.</summary>
    public const string Name = "name";

    /// <summary>Download format file type.</summary>
    public const string FormatFileType = "format.fileType";

    /// <summary>Reference type (abbreviated from <c>meta.refType</c>).</summary>
    public const string RefType = "refType";

    /// <summary>Reference direction (abbreviated from <c>meta.direction</c>).</summary>
    public const string Direction = "direction";

    /// <summary>
    /// 	Hidden state.
    /// </summary>
    /// <remarks>
    /// 	Hidden items and folders are omitted by default.
    /// 	Pass <c>true</c> for hidden only, or <c>true</c> and <c>false</c> together for both.
    /// </remarks>
    public const string Hidden = "hidden";
}
