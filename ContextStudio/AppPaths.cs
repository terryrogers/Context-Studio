namespace ContextStudio;

internal static class AppPaths
{
    public static string CodexHome
    {
        get
        {
            var configured = Environment.GetEnvironmentVariable("CODEX_HOME");
            return string.IsNullOrWhiteSpace(configured)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".codex")
                : Path.GetFullPath(configured);
        }
    }

    public static string ChatGptProjects => Path.Combine(CodexHome, ".chatgpt-projects");
    public static string DefaultConversations => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Codex");
}
