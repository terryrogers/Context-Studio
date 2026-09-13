using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace ContextStudio;

internal sealed class MainForm : Form
{
    private readonly Color _nav = Color.FromArgb(22, 27, 38);
    private readonly Color _accent = Color.FromArgb(79, 111, 255);
    private readonly Color _muted = Color.FromArgb(103, 112, 130);
    private readonly FileDiscoveryService _discovery = new();
    private readonly SafeFileService _files = new();
    private readonly AppSettings _settings = AppSettings.Load();

    private readonly Button _globalButton = new();
    private readonly Button _projectButton = new();
    private readonly Button _conversationButton = new();
    private readonly Label _scopeDescription = new();
    private readonly TextBox _locationBox = new();
    private readonly Button _browseButton = new();
    private readonly Button _refreshButton = new();
    private readonly ListView _fileList = new();
    private readonly Label _emptyLabel = new();
    private readonly Label _fileTitle = new();
    private readonly Label _filePath = new();
    private readonly Label _badge = new();
    private readonly RichTextBox _editor = new();
    private readonly Button _saveButton = new();
    private readonly Button _reloadButton = new();
    private readonly Button _folderButton = new();
    private readonly TextBox _findBox = new();
    private readonly Panel _findPanel = new();
    private readonly Label _status = new();
    private readonly Label _stats = new();
    private readonly CheckBox _wordWrap = new();
    private readonly NumericUpDown _fontSize = new();

    private ContextScope _scope = ContextScope.Global;
    private string? _projectRoot;
    private string? _conversationRoot;
    private LoadedDocument? _document;
    private ContextFile? _selectedFile;
    private bool _loading;
    private bool _dirty;

    public MainForm()
    {
        _projectRoot = ExistingOrDefault(_settings.ProjectRoot, Environment.CurrentDirectory);
        _conversationRoot = ExistingOrDefault(_settings.ConversationRoot, Environment.CurrentDirectory);

        Text = "Context Studio";
        MinimumSize = new Size(1020, 680);
        Size = new Size(1360, 840);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(246, 247, 250);
        Font = new Font("Segoe UI", 9F);
        KeyPreview = true;
        Icon = CreateAppIcon();

        BuildLayout();
        WireEvents();
        SelectScope(ContextScope.Global, false);
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 244));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(root);
        root.Controls.Add(BuildNavigation(), 0, 0);
        root.Controls.Add(BuildWorkspace(), 1, 0);
    }

    private Control BuildNavigation()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = _nav, Padding = new Padding(18, 22, 18, 18) };
        var title = new Label { Text = "Context Studio", ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 16F), AutoSize = true, Location = new Point(18, 22) };
        var subtitle = new Label { Text = "MEMORY & PROMPT EDITOR", ForeColor = Color.FromArgb(128, 140, 164), Font = new Font("Segoe UI Semibold", 7.5F), AutoSize = true, Location = new Point(20, 58) };
        panel.Controls.Add(title);
        panel.Controls.Add(subtitle);

        ConfigureScopeButton(_globalButton, "◉   Global", 102);
        ConfigureScopeButton(_projectButton, "◇   Project", 150);
        ConfigureScopeButton(_conversationButton, "▤   Conversation", 198);
        panel.Controls.AddRange([_globalButton, _projectButton, _conversationButton]);

        var about = new Panel { Dock = DockStyle.Bottom, Height = 122, BackColor = Color.FromArgb(28, 34, 47), Padding = new Padding(13) };
        var aboutTitle = new Label { Text = "Safe local editing", Dock = DockStyle.Top, Height = 23, ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 9F) };
        var aboutText = new Label { Text = "Files stay on this PC. Every update creates a timestamped backup and checks for outside changes.", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(160, 170, 190), Font = new Font("Segoe UI", 8.25F) };
        about.Controls.Add(aboutText);
        about.Controls.Add(aboutTitle);
        panel.Controls.Add(about);
        return panel;
    }

    private void ConfigureScopeButton(Button button, string text, int top)
    {
        button.Text = text;
        button.Location = new Point(12, top);
        button.Size = new Size(220, 42);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.TextAlign = ContentAlignment.MiddleLeft;
        button.Padding = new Padding(14, 0, 0, 0);
        button.ForeColor = Color.FromArgb(193, 201, 216);
        button.BackColor = _nav;
        button.Cursor = Cursors.Hand;
        button.Font = new Font("Segoe UI Semibold", 9.5F);
    }

    private Control BuildWorkspace()
    {
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = Padding.Empty };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        layout.Controls.Add(BuildTopBar(), 0, 0);
        layout.Controls.Add(BuildContent(), 0, 1);
        layout.Controls.Add(BuildStatusBar(), 0, 2);
        return layout;
    }

    private Control BuildTopBar()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(24, 14, 24, 12) };
        var title = new Label { Text = "Global context", Font = new Font("Segoe UI Semibold", 14F), ForeColor = Color.FromArgb(30, 36, 49), AutoSize = true, Location = new Point(24, 12), Name = "ScopeTitle" };
        _scopeDescription.Text = "Defaults available to Codex across workspaces";
        _scopeDescription.ForeColor = _muted;
        _scopeDescription.AutoSize = true;
        _scopeDescription.Location = new Point(26, 43);

        _locationBox.Location = new Point(24, 68);
        _locationBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        _locationBox.Width = panel.Width - 180;
        _locationBox.ReadOnly = true;
        _locationBox.BackColor = Color.FromArgb(247, 248, 251);
        _locationBox.BorderStyle = BorderStyle.FixedSingle;
        _locationBox.Font = new Font("Cascadia Mono", 8.5F);

        ConfigureSmallButton(_browseButton, "Choose…", 78);
        _browseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _browseButton.Location = new Point(panel.Width - 150, 66);
        ConfigureSmallButton(_refreshButton, "↻", 38);
        _refreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _refreshButton.Location = new Point(panel.Width - 62, 66);

        panel.Controls.AddRange([title, _scopeDescription, _locationBox, _browseButton, _refreshButton]);
        panel.Resize += (_, _) =>
        {
            _locationBox.Width = Math.Max(200, panel.ClientSize.Width - 198);
            _browseButton.Left = panel.ClientSize.Width - 158;
            _refreshButton.Left = panel.ClientSize.Width - 66;
        };
        return panel;
    }

    private Control BuildContent()
    {
        var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 300, FixedPanel = FixedPanel.Panel1, BackColor = Color.FromArgb(224, 227, 233), SplitterWidth = 1 };
        split.Panel1.BackColor = Color.FromArgb(249, 250, 252);
        split.Panel2.BackColor = Color.White;
        split.Panel1.Padding = new Padding(16, 14, 12, 12);
        split.Panel2.Padding = new Padding(0);

        var fileHeader = new Label { Text = "FILES", Dock = DockStyle.Top, Height = 30, ForeColor = _muted, Font = new Font("Segoe UI Semibold", 8F) };
        _fileList.Dock = DockStyle.Fill;
        _fileList.View = View.Details;
        _fileList.HeaderStyle = ColumnHeaderStyle.None;
        _fileList.FullRowSelect = true;
        _fileList.HideSelection = false;
        _fileList.BorderStyle = BorderStyle.None;
        _fileList.BackColor = Color.FromArgb(249, 250, 252);
        _fileList.Columns.Add("File", 260);
        _fileList.ShowGroups = true;
        _fileList.Font = new Font("Segoe UI", 9F);
        _emptyLabel.Text = "Choose a folder to view its context files.";
        _emptyLabel.ForeColor = _muted;
        _emptyLabel.TextAlign = ContentAlignment.MiddleCenter;
        _emptyLabel.Dock = DockStyle.Fill;
        _emptyLabel.Visible = false;
        split.Panel1.Controls.Add(_fileList);
        split.Panel1.Controls.Add(_emptyLabel);
        split.Panel1.Controls.Add(fileHeader);
        split.Panel2.Controls.Add(BuildEditor());
        return split;
    }

    private Control BuildEditor()
    {
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Color.White };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 0));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(BuildEditorHeader(), 0, 0);

        _findPanel.Dock = DockStyle.Fill;
        _findPanel.Padding = new Padding(18, 7, 18, 6);
        _findPanel.BackColor = Color.FromArgb(245, 247, 252);
        _findBox.PlaceholderText = "Find in file";
        _findBox.Width = 260;
        _findBox.Location = new Point(18, 7);
        var findNext = new Button { Text = "Next", FlatStyle = FlatStyle.Flat, Size = new Size(58, 26), Location = new Point(286, 6) };
        findNext.FlatAppearance.BorderColor = Color.FromArgb(214, 218, 228);
        var closeFind = new Button { Text = "×", FlatStyle = FlatStyle.Flat, Size = new Size(30, 26), Location = new Point(350, 6) };
        closeFind.FlatAppearance.BorderSize = 0;
        findNext.Click += (_, _) => FindNext();
        closeFind.Click += (_, _) => ToggleFind(false);
        _findBox.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { FindNext(); e.SuppressKeyPress = true; } else if (e.KeyCode == Keys.Escape) ToggleFind(false); };
        _findPanel.Controls.AddRange([_findBox, findNext, closeFind]);
        layout.Controls.Add(_findPanel, 0, 1);

        _editor.Dock = DockStyle.Fill;
        _editor.BorderStyle = BorderStyle.None;
        _editor.AcceptsTab = true;
        _editor.DetectUrls = false;
        _editor.HideSelection = false;
        _editor.Font = new Font("Cascadia Mono", _settings.FontSize);
        _editor.BackColor = Color.White;
        _editor.ForeColor = Color.FromArgb(33, 39, 52);
        _editor.Margin = new Padding(0);
        _editor.WordWrap = _settings.WordWrap;
        _editor.ZoomFactor = 1F;
        layout.Controls.Add(_editor, 0, 2);
        return layout;
    }

    private Control BuildEditorHeader()
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 11, 18, 8), BackColor = Color.White };
        _fileTitle.Text = "Select a file";
        _fileTitle.Font = new Font("Segoe UI Semibold", 13F);
        _fileTitle.ForeColor = Color.FromArgb(30, 36, 49);
        _fileTitle.AutoSize = true;
        _fileTitle.Location = new Point(20, 10);
        _filePath.Text = "Choose a file from the list to view or edit it.";
        _filePath.Font = new Font("Cascadia Mono", 8F);
        _filePath.ForeColor = _muted;
        _filePath.AutoEllipsis = true;
        _filePath.Location = new Point(21, 43);
        _filePath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _filePath.Width = 400;
        _filePath.Height = 20;
        _badge.AutoSize = true;
        _badge.Padding = new Padding(7, 3, 7, 3);
        _badge.Font = new Font("Segoe UI Semibold", 7.5F);
        _badge.Location = new Point(21, 61);
        _badge.Visible = false;

        ConfigureSmallButton(_folderButton, "Folder", 62);
        ConfigureSmallButton(_reloadButton, "Reload", 62);
        ConfigurePrimaryButton(_saveButton, "Save", 76);
        _folderButton.Anchor = _reloadButton.Anchor = _saveButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        panel.Controls.AddRange([_fileTitle, _filePath, _badge, _folderButton, _reloadButton, _saveButton]);
        panel.Resize += (_, _) =>
        {
            _saveButton.Left = panel.ClientSize.Width - 94;
            _reloadButton.Left = panel.ClientSize.Width - 164;
            _folderButton.Left = panel.ClientSize.Width - 234;
            _filePath.Width = Math.Max(160, panel.ClientSize.Width - 280);
        };
        return panel;
    }

    private Control BuildStatusBar()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(242, 244, 248), Padding = new Padding(14, 5, 14, 4) };
        _status.Text = "Ready";
        _status.Dock = DockStyle.Left;
        _status.Width = 480;
        _status.ForeColor = _muted;
        _stats.Dock = DockStyle.Right;
        _stats.Width = 140;
        _stats.TextAlign = ContentAlignment.MiddleRight;
        _stats.ForeColor = _muted;
        _wordWrap.Text = "Word wrap";
        _wordWrap.Checked = _settings.WordWrap;
        _wordWrap.Dock = DockStyle.Right;
        _wordWrap.Width = 88;
        _fontSize.Minimum = 8;
        _fontSize.Maximum = 24;
        _fontSize.Value = Math.Clamp(_settings.FontSize, 8, 24);
        _fontSize.Dock = DockStyle.Right;
        _fontSize.Width = 48;
        _fontSize.BorderStyle = BorderStyle.None;
        panel.Controls.AddRange([_status, _stats, _wordWrap, _fontSize]);
        return panel;
    }

    private void WireEvents()
    {
        _globalButton.Click += (_, _) => SelectScope(ContextScope.Global);
        _projectButton.Click += (_, _) => SelectScope(ContextScope.Project);
        _conversationButton.Click += (_, _) => SelectScope(ContextScope.Conversation);
        _browseButton.Click += (_, _) => ChooseLocation();
        _refreshButton.Click += (_, _) => RefreshFiles(true);
        _fileList.SelectedIndexChanged += (_, _) => { if (_fileList.SelectedItems.Count > 0 && _fileList.SelectedItems[0].Tag is ContextFile file) OpenFile(file); };
        _editor.TextChanged += (_, _) => { if (!_loading && _document is not null) { SetDirty(true); UpdateStats(); } };
        _editor.SelectionChanged += (_, _) => UpdateStats();
        _saveButton.Click += (_, _) => SaveCurrent();
        _reloadButton.Click += (_, _) => ReloadCurrent();
        _folderButton.Click += (_, _) => OpenContainingFolder();
        _wordWrap.CheckedChanged += (_, _) => { _editor.WordWrap = _wordWrap.Checked; _settings.WordWrap = _wordWrap.Checked; _settings.Save(); };
        _fontSize.ValueChanged += (_, _) => { _editor.Font = new Font("Cascadia Mono", (float)_fontSize.Value); _settings.FontSize = (int)_fontSize.Value; _settings.Save(); };
        KeyDown += OnFormKeyDown;
        FormClosing += OnFormClosing;
    }

    private void SelectScope(ContextScope scope, bool askToSave = true)
    {
        if (askToSave && !ConfirmDiscardOrSave()) return;
        _scope = scope;
        var title = Controls.Find("ScopeTitle", true).OfType<Label>().First();
        title.Text = scope switch { ContextScope.Global => "Global context", ContextScope.Project => "Project context", _ => "Conversation context" };
        _scopeDescription.Text = scope switch
        {
            ContextScope.Global => "Defaults available to Codex across workspaces",
            ContextScope.Project => "Instructions and memory for one project root",
            _ => "A focused override for one conversation folder"
        };
        _browseButton.Visible = scope != ContextScope.Global;
        UpdateScopeButtons();
        RefreshFiles(false);
    }

    private void UpdateScopeButtons()
    {
        foreach (var pair in new[] { (_globalButton, ContextScope.Global), (_projectButton, ContextScope.Project), (_conversationButton, ContextScope.Conversation) })
        {
            var active = pair.Item2 == _scope;
            pair.Item1.BackColor = active ? Color.FromArgb(48, 57, 78) : _nav;
            pair.Item1.ForeColor = active ? Color.White : Color.FromArgb(193, 201, 216);
            pair.Item1.FlatAppearance.BorderColor = active ? _accent : _nav;
            pair.Item1.FlatAppearance.BorderSize = active ? 1 : 0;
        }
    }

    private void RefreshFiles(bool preserveSelection)
    {
        var root = CurrentRoot();
        _locationBox.Text = root ?? "No folder selected";
        var selectedPath = preserveSelection ? _selectedFile?.Path : null;
        var items = _discovery.Discover(_scope, root);
        _fileList.BeginUpdate();
        _fileList.Items.Clear();
        _fileList.Groups.Clear();
        var groups = Enum.GetValues<ContextFileKind>().ToDictionary(k => k, k => new ListViewGroup(k.ToString().ToUpperInvariant()));
        foreach (var group in groups.Values) _fileList.Groups.Add(group);
        foreach (var file in items)
        {
            var suffix = file.Exists ? string.Empty : "  (create)";
            var item = new ListViewItem(file.DisplayName + suffix, groups[file.Kind]) { Tag = file, ToolTipText = file.Path };
            item.ForeColor = file.Exists ? Color.FromArgb(45, 52, 66) : _muted;
            _fileList.Items.Add(item);
        }
        _fileList.EndUpdate();
        _emptyLabel.Visible = items.Count == 0;
        _fileList.Visible = items.Count > 0;
        if (items.Count == 0) ClearEditor();
        else
        {
            var match = _fileList.Items.Cast<ListViewItem>().FirstOrDefault(i => i.Tag is ContextFile file && string.Equals(file.Path, selectedPath, StringComparison.OrdinalIgnoreCase));
            (match ?? _fileList.Items[0]).Selected = true;
        }
        _status.Text = $"{items.Count} context file{(items.Count == 1 ? string.Empty : "s")} in this scope";
    }

    private void OpenFile(ContextFile file)
    {
        if (_selectedFile?.Path.Equals(file.Path, StringComparison.OrdinalIgnoreCase) == true) return;
        if (!ConfirmDiscardOrSave())
        {
            ReselectCurrent();
            return;
        }
        _selectedFile = file;
        _document = _files.Load(file.Path);
        _loading = true;
        _editor.Text = _document.Content;
        _editor.SelectionStart = 0;
        _editor.SelectionLength = 0;
        _loading = false;
        _editor.ReadOnly = false;
        _fileTitle.Text = file.DisplayName;
        _filePath.Text = file.Path;
        _badge.Text = file.IsGenerated ? "GENERATED · EDIT WITH CARE" : file.Exists ? file.Kind.ToString().ToUpperInvariant() : "NEW FILE";
        _badge.BackColor = file.IsGenerated ? Color.FromArgb(255, 239, 204) : Color.FromArgb(230, 235, 255);
        _badge.ForeColor = file.IsGenerated ? Color.FromArgb(133, 84, 0) : Color.FromArgb(51, 76, 184);
        _badge.Visible = true;
        _saveButton.Text = file.Exists ? "Save" : "Create";
        _saveButton.Enabled = true;
        _reloadButton.Enabled = file.Exists;
        _folderButton.Enabled = true;
        SetDirty(false);
        UpdateStats();
        _status.Text = file.Description ?? (file.Exists ? "Loaded" : "This file will be created when saved.");
    }

    private bool SaveCurrent()
    {
        if (_document is null || _selectedFile is null) return false;
        if (_selectedFile.IsGenerated && _dirty)
        {
            var confirm = MessageBox.Show(this,
                "This is a generated memory file and may be rebuilt by Codex. Save your edit anyway?",
                "Edit generated memory", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return false;
        }
        var result = _files.Save(_document, _editor.Text);
        if (!result.Success && result.Message.StartsWith("The file changed", StringComparison.Ordinal))
        {
            var choice = MessageBox.Show(this, result.Message + "\n\nChoose Yes to overwrite it (a backup will still be made), or No to cancel and review.", "File changed", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (choice == DialogResult.Yes) result = _files.Save(_document, _editor.Text, true);
        }
        if (!result.Success)
        {
            MessageBox.Show(this, result.Message, "Could not save", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _status.Text = result.Message;
            return false;
        }
        _document = result.Document;
        _selectedFile = _selectedFile with { Exists = true };
        SetDirty(false);
        _saveButton.Text = "Save";
        _reloadButton.Enabled = true;
        _badge.Text = _selectedFile.IsGenerated ? "GENERATED · EDIT WITH CARE" : _selectedFile.Kind.ToString().ToUpperInvariant();
        _status.Text = result.BackupPath is null ? "Created successfully" : $"Saved · backup: {Path.GetFileName(result.BackupPath)}";
        RefreshListLabelsOnly();
        return true;
    }

    private void ReloadCurrent()
    {
        if (_selectedFile is null || !ConfirmDiscard()) return;
        var path = _selectedFile.Path;
        SetDirty(false);
        _selectedFile = null;
        var item = _fileList.Items.Cast<ListViewItem>().FirstOrDefault(i => i.Tag is ContextFile file && string.Equals(file.Path, path, StringComparison.OrdinalIgnoreCase));
        if (item?.Tag is ContextFile file) OpenFile(file);
    }

    private bool ConfirmDiscardOrSave()
    {
        if (!_dirty) return true;
        var choice = MessageBox.Show(this, "Save changes before switching?", "Unsaved changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        if (choice == DialogResult.Yes) return SaveCurrent();
        if (choice == DialogResult.No) { SetDirty(false); return true; }
        return false;
    }

    private bool ConfirmDiscard()
    {
        if (!_dirty) return true;
        return MessageBox.Show(this, "Discard the unsaved changes and reload from disk?", "Reload file", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
    }

    private void ChooseLocation()
    {
        if (_scope == ContextScope.Project)
        {
            var projects = _discovery.DiscoverChatGptProjects();
            if (projects.Count > 0)
            {
                var menu = new ContextMenuStrip { Font = Font, ShowImageMargin = false };
                var browse = new ToolStripMenuItem("Browse for a folder…");
                browse.Click += (_, _) => BrowseLocation();
                menu.Items.Add(browse);
                menu.Items.Add(new ToolStripSeparator());
                foreach (var project in projects)
                {
                    var item = new ToolStripMenuItem(project.Name) { ToolTipText = project.Path };
                    item.Click += (_, _) => SetProjectRoot(project.Path);
                    menu.Items.Add(item);
                }
                menu.Show(_browseButton, new Point(0, _browseButton.Height));
                return;
            }
        }
        BrowseLocation();
    }

    private void BrowseLocation()
    {
        if (!ConfirmDiscardOrSave()) return;
        using var dialog = new FolderBrowserDialog
        {
            Description = _scope == ContextScope.Project ? "Choose a project root" : "Choose a conversation folder",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true,
            InitialDirectory = CurrentRoot() ?? Environment.CurrentDirectory
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        if (_scope == ContextScope.Project) { SetProjectRoot(dialog.SelectedPath); return; }
        else { _conversationRoot = dialog.SelectedPath; _settings.ConversationRoot = dialog.SelectedPath; }
        _settings.Save();
        RefreshFiles(false);
    }

    private void SetProjectRoot(string path)
    {
        if (!ConfirmDiscardOrSave()) return;
        _projectRoot = path;
        _settings.ProjectRoot = path;
        _settings.Save();
        RefreshFiles(false);
    }

    private void OnFormKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.S) { SaveCurrent(); e.SuppressKeyPress = true; }
        else if (e.Control && e.KeyCode == Keys.F) { ToggleFind(true); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.F5) { RefreshFiles(true); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.Escape && _findPanel.Visible) ToggleFind(false);
    }

    private void ToggleFind(bool visible)
    {
        var layout = (TableLayoutPanel)_findPanel.Parent!;
        layout.RowStyles[1].Height = visible ? 40 : 0;
        _findPanel.Visible = visible;
        if (visible) { _findBox.Focus(); _findBox.SelectAll(); }
        else _editor.Focus();
    }

    private void FindNext()
    {
        if (string.IsNullOrEmpty(_findBox.Text)) return;
        var start = Math.Min(_editor.TextLength, _editor.SelectionStart + _editor.SelectionLength);
        var index = _editor.Find(_findBox.Text, start, RichTextBoxFinds.None);
        if (index < 0 && start > 0) index = _editor.Find(_findBox.Text, 0, RichTextBoxFinds.None);
        _status.Text = index < 0 ? $"No match for “{_findBox.Text}”" : "Match found";
    }

    private void OpenContainingFolder()
    {
        if (_selectedFile is null) return;
        var directory = Path.GetDirectoryName(_selectedFile.Path)!;
        Directory.CreateDirectory(directory);
        var arguments = File.Exists(_selectedFile.Path) ? $"/select,\"{_selectedFile.Path}\"" : $"\"{directory}\"";
        Process.Start(new ProcessStartInfo("explorer.exe", arguments) { UseShellExecute = true });
    }

    private void SetDirty(bool dirty)
    {
        _dirty = dirty;
        _saveButton.Enabled = _document is not null && (dirty || _selectedFile?.Exists == false);
        Text = dirty ? "Context Studio  •  Unsaved" : "Context Studio";
        if (dirty) _status.Text = "Unsaved changes";
    }

    private void UpdateStats()
    {
        if (_document is null) { _stats.Text = string.Empty; return; }
        var line = _editor.GetLineFromCharIndex(_editor.SelectionStart) + 1;
        var column = _editor.SelectionStart - _editor.GetFirstCharIndexFromLine(line - 1) + 1;
        _stats.Text = $"Ln {line}, Col {column}  ·  {_editor.TextLength:N0} chars";
    }

    private void ClearEditor()
    {
        _selectedFile = null;
        _document = null;
        _loading = true;
        _editor.Clear();
        _loading = false;
        _editor.ReadOnly = true;
        _fileTitle.Text = "No context files";
        _filePath.Text = "Choose a valid folder or create a supported context file.";
        _badge.Visible = false;
        _saveButton.Enabled = _reloadButton.Enabled = _folderButton.Enabled = false;
        SetDirty(false);
    }

    private void RefreshListLabelsOnly()
    {
        foreach (ListViewItem item in _fileList.Items)
        {
            if (item.Tag is ContextFile file && _selectedFile is not null && file.Path.Equals(_selectedFile.Path, StringComparison.OrdinalIgnoreCase))
            {
                item.Tag = _selectedFile;
                item.Text = _selectedFile.DisplayName;
                item.ForeColor = Color.FromArgb(45, 52, 66);
            }
        }
    }

    private void ReselectCurrent()
    {
        if (_selectedFile is null) return;
        foreach (ListViewItem item in _fileList.Items)
            item.Selected = item.Tag is ContextFile file && file.Path.Equals(_selectedFile.Path, StringComparison.OrdinalIgnoreCase);
    }

    private string? CurrentRoot() => _scope switch { ContextScope.Global => AppPaths.CodexHome, ContextScope.Project => _projectRoot, _ => _conversationRoot };
    private static string ExistingOrDefault(string? configured, string fallback) => !string.IsNullOrWhiteSpace(configured) && Directory.Exists(configured) ? configured : fallback;

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!ConfirmDiscardOrSave()) e.Cancel = true;
    }

    private static void ConfigureSmallButton(Button button, string text, int width)
    {
        button.Text = text;
        button.Size = new Size(width, 28);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Color.FromArgb(211, 216, 226);
        button.BackColor = Color.White;
        button.ForeColor = Color.FromArgb(55, 63, 78);
        button.Cursor = Cursors.Hand;
    }

    private void ConfigurePrimaryButton(Button button, string text, int width)
    {
        button.Text = text;
        button.Size = new Size(width, 30);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = _accent;
        button.ForeColor = Color.White;
        button.Font = new Font("Segoe UI Semibold", 9F);
        button.Cursor = Cursors.Hand;
    }

    private Icon CreateAppIcon()
    {
        using var bitmap = new Bitmap(32, 32);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(_nav);
        using var pen = new Pen(Color.White, 2.2F);
        graphics.DrawLine(pen, 8, 9, 24, 9);
        graphics.DrawLine(pen, 8, 16, 21, 16);
        graphics.DrawLine(pen, 8, 23, 17, 23);
        using var accentBrush = new SolidBrush(_accent);
        graphics.FillEllipse(accentBrush, 21, 20, 6, 6);
        return Icon.FromHandle(bitmap.GetHicon());
    }
}
