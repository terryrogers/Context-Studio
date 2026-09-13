namespace ContextStudio;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Contains("--self-test", StringComparer.OrdinalIgnoreCase))
            return SelfTest.Run();

        ApplicationConfiguration.Initialize();
        if (args.Contains("--ui-smoke-test", StringComparer.OrdinalIgnoreCase))
            return SelfTest.RunUiSmoke();
        Application.Run(new MainForm());
        return 0;
    }
}
