namespace ContextStudio;

internal enum ContextScope
{
    Global,
    Project,
    Conversation
}

internal enum ContextFileKind
{
    Instructions,
    Memory,
    Notes
}

internal sealed record ContextFile(
    string DisplayName,
    string Path,
    ContextFileKind Kind,
    bool Exists,
    bool IsGenerated = false,
    string? Description = null)
{
    public override string ToString() => DisplayName;
}

internal sealed record LoadedDocument(string Path, string Content, string Fingerprint, DateTime LastWriteUtc);

internal sealed record SaveResult(bool Success, string Message, string? BackupPath = null, LoadedDocument? Document = null);
