using System.Security.Cryptography;

namespace Autodesk.Aps.TestCommon;

/// <summary>
/// 	A temporary file that deletes itself on dispose.
/// </summary>
/// <remarks>
/// 	The chunked upload test needs a file larger than 5 MB.
/// 	Generating it beats committing a 5 MB binary to the repository.
/// </remarks>
public sealed class TempFile : IDisposable
{
    /// <summary>
    /// 	Creates an empty temporary file.
    /// </summary>
    public TempFile(string extension = ".dat") =>
        Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"aps-sdk-test-{Guid.NewGuid():N}{extension}");

    /// <summary>
    /// 	The full path of the temporary file.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// 	Creates a temporary file filled with random bytes.
    /// </summary>
    /// <param name="sizeInBytes">
    /// 	The size of the generated file.
    /// </param>
    public static TempFile OfSize(long sizeInBytes)
    {
        TempFile file = new();

        using FileStream stream = File.Create(file.Path);
        byte[] buffer = new byte[81920];
        long written = 0;

        while (written < sizeInBytes)
        {
            RandomNumberGenerator.Fill(buffer);
            int count = (int)Math.Min(buffer.Length, sizeInBytes - written);
            stream.Write(buffer, 0, count);
            written += count;
        }

        return file;
    }

    /// <summary>
    /// 	Deletes the file. A file that is already gone is not an error.
    /// </summary>
    public void Dispose()
    {
        try
        {
            File.Delete(Path);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
