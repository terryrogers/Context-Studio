using System.Security.Cryptography;
using System.Text;

namespace ContextStudio;

internal sealed class SafeFileService
{
    public LoadedDocument Load(string path)
    {
        path = Path.GetFullPath(path);
        if (!File.Exists(path)) return new(path, string.Empty, Fingerprint([]), DateTime.MinValue);
        var bytes = File.ReadAllBytes(path);
        return new(path, Decode(bytes), Fingerprint(bytes), File.GetLastWriteTimeUtc(path));
    }

    public bool HasChangedExternally(LoadedDocument loaded)
    {
        if (!File.Exists(loaded.Path)) return loaded.LastWriteUtc != DateTime.MinValue;
        return !string.Equals(Fingerprint(File.ReadAllBytes(loaded.Path)), loaded.Fingerprint, StringComparison.Ordinal);
    }

    public SaveResult Save(LoadedDocument loaded, string content, bool overwriteExternalChange = false)
    {
        try
        {
            if (!overwriteExternalChange && HasChangedExternally(loaded))
                return new(false, "The file changed outside Context Studio. Reload it or explicitly overwrite the newer version.");

            var directory = Path.GetDirectoryName(loaded.Path)!;
            Directory.CreateDirectory(directory);
            string? backup = null;
            if (File.Exists(loaded.Path))
            {
                var backupDirectory = Path.Combine(directory, ".context-studio-backups");
                Directory.CreateDirectory(backupDirectory);
                backup = UniqueBackupPath(backupDirectory, Path.GetFileName(loaded.Path));
                File.Copy(loaded.Path, backup, false);
            }

            var utf8 = new UTF8Encoding(false);
            var bytes = utf8.GetBytes(content);
            var temporary = Path.Combine(directory, $".{Path.GetFileName(loaded.Path)}.{Guid.NewGuid():N}.tmp");
            File.WriteAllBytes(temporary, bytes);
            if (File.Exists(loaded.Path)) File.Move(temporary, loaded.Path, true);
            else File.Move(temporary, loaded.Path);

            var document = new LoadedDocument(loaded.Path, content, Fingerprint(bytes), File.GetLastWriteTimeUtc(loaded.Path));
            return new(true, backup is null ? "File created." : "Saved. A timestamped backup was created.", backup, document);
        }
        catch (Exception ex)
        {
            return new(false, $"Save failed: {ex.Message}");
        }
    }

    private static string Decode(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
        return Encoding.UTF8.GetString(bytes);
    }

    private static string Fingerprint(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));

    private static string UniqueBackupPath(string directory, string fileName)
    {
        var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var candidate = Path.Combine(directory, $"{fileName}.{stamp}.bak");
        var suffix = 1;
        while (File.Exists(candidate)) candidate = Path.Combine(directory, $"{fileName}.{stamp}-{suffix++}.bak");
        return candidate;
    }
}
