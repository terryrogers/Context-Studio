namespace ContextStudio;

internal static class SelfTest
{
    public static int RunUiSmoke()
    {
        try
        {
            using var form = new MainForm { ShowInTaskbar = false, WindowState = FormWindowState.Minimized };
            using var timer = new System.Windows.Forms.Timer { Interval = 750 };
            timer.Tick += (_, _) => { timer.Stop(); form.Close(); };
            form.Shown += (_, _) => timer.Start();
            Application.Run(form);
            return 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "Context Studio UI smoke test failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 1;
        }
    }

    public static int Run()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ContextStudio-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(root);
            var service = new SafeFileService();
            var path = Path.Combine(root, "AGENTS.md");

            var missing = service.Load(path);
            Require(missing.Content == string.Empty, "missing files load empty");
            var created = service.Save(missing, "# First\n");
            Require(created.Success && File.ReadAllText(path) == "# First\n", "creates UTF-8 document");

            var loaded = service.Load(path);
            var saved = service.Save(loaded, "# Second\n");
            Require(saved.Success && saved.BackupPath is not null && File.Exists(saved.BackupPath), "creates backup on update");
            Require(File.ReadAllText(saved.BackupPath!) == "# First\n", "backup contains prior version");

            var stale = service.Load(path);
            File.WriteAllText(path, "external");
            var blocked = service.Save(stale, "overwrite");
            Require(!blocked.Success && File.ReadAllText(path) == "external", "blocks external-change overwrite");
            var forced = service.Save(stale, "overwrite", true);
            Require(forced.Success && File.ReadAllText(path) == "overwrite", "supports explicit overwrite");

            var discovery = new FileDiscoveryService().Discover(ContextScope.Project, root);
            Require(discovery.Any(f => f.Path.Equals(path, StringComparison.OrdinalIgnoreCase)), "discovers project instructions");

            Console.WriteLine("PASS: 6 Context Studio self-tests");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"FAIL: {ex.Message}");
            return 1;
        }
        finally
        {
            try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
