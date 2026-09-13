namespace ContextStudio;

internal sealed class FileDiscoveryService
{
    private static readonly string[] OptionalNames = ["AGENTS.md", "AGENTS.override.md", "MEMORY.md", "memory.md", "PROMPT.md", "prompt.md", "CONTEXT.md", "context.md"];

    public IReadOnlyList<ContextFile> Discover(ContextScope scope, string? selectedRoot)
    {
        return scope switch
        {
            ContextScope.Global => DiscoverGlobal(),
            ContextScope.Project => DiscoverFolder(selectedRoot, false),
            ContextScope.Conversation => DiscoverFolder(selectedRoot, true),
            _ => []
        };
    }

    private static IReadOnlyList<ContextFile> DiscoverGlobal()
    {
        var home = AppPaths.CodexHome;
        var memories = Path.Combine(home, "memories");
        var files = new List<ContextFile>
        {
            Known("Global instructions", Path.Combine(home, "AGENTS.md"), ContextFileKind.Instructions, "Used when AGENTS.override.md is absent."),
            Known("Global override", Path.Combine(home, "AGENTS.override.md"), ContextFileKind.Instructions, "Takes precedence over global AGENTS.md."),
            Known("Memory registry", Path.Combine(memories, "MEMORY.md"), ContextFileKind.Memory, "Long-form memory registry."),
            Known("Memory summary", Path.Combine(memories, "memory_summary.md"), ContextFileKind.Memory, "Generated summary; edit with care.", true),
            Known("Raw memories", Path.Combine(memories, "raw_memories.md"), ContextFileKind.Memory, "Generated source material; edit with care.", true)
        };
        return files;
    }

    private static IReadOnlyList<ContextFile> DiscoverFolder(string? root, bool conversation)
    {
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return [];
        root = Path.GetFullPath(root);
        var results = new Dictionary<string, ContextFile>(StringComparer.OrdinalIgnoreCase);

        var primary = conversation ? "AGENTS.override.md" : "AGENTS.md";
        Add(results, Known(conversation ? "Conversation instructions" : "Project instructions", Path.Combine(root, primary), ContextFileKind.Instructions,
            conversation ? "Applies at this conversation folder and below." : "Applies at this project root and below."));

        foreach (var name in OptionalNames)
        {
            var path = Path.Combine(root, name);
            if (File.Exists(path)) Add(results, Known(name, path, KindFor(name)));
        }

        var localCodex = Path.Combine(root, ".codex");
        if (Directory.Exists(localCodex))
        {
            foreach (var path in Directory.EnumerateFiles(localCodex, "*.md", SearchOption.TopDirectoryOnly))
                Add(results, Known($".codex / {Path.GetFileName(path)}", path, KindFor(path)));
        }

        return results.Values.OrderBy(f => f.Kind).ThenBy(f => f.DisplayName, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public IReadOnlyList<(string Name, string Path)> DiscoverChatGptProjects()
    {
        if (!Directory.Exists(AppPaths.ChatGptProjects)) return [];
        return Directory.EnumerateDirectories(AppPaths.ChatGptProjects, "g-p-*")
            .Select(path => (FriendlyProjectName(path), path))
            .OrderBy(p => p.Item1, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string FriendlyProjectName(string path)
    {
        var id = Path.GetFileName(path);
        var agents = Path.Combine(path, "AGENTS.md");
        if (!File.Exists(agents)) return id;
        try
        {
            var heading = File.ReadLines(agents).Select(l => l.Trim()).FirstOrDefault(l => l.StartsWith("# "));
            return string.IsNullOrWhiteSpace(heading) ? id : $"{heading[2..].Trim()}  ·  {id}";
        }
        catch { return id; }
    }

    private static void Add(Dictionary<string, ContextFile> files, ContextFile file) => files.TryAdd(Path.GetFullPath(file.Path), file);
    private static ContextFile Known(string name, string path, ContextFileKind kind, string? description = null, bool generated = false) =>
        new(name, path, kind, File.Exists(path), generated, description);

    private static ContextFileKind KindFor(string name) =>
        name.Contains("AGENT", StringComparison.OrdinalIgnoreCase) || name.Contains("PROMPT", StringComparison.OrdinalIgnoreCase)
            ? ContextFileKind.Instructions
            : name.Contains("MEMOR", StringComparison.OrdinalIgnoreCase) ? ContextFileKind.Memory : ContextFileKind.Notes;
}
