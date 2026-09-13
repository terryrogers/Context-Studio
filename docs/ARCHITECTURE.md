# Architecture

## System Context

Context Studio is a single-user Windows desktop application. It reads and writes local files selected by the current user and does not connect to Codex, ChatGPT, OpenProject, GitHub, or any other network service at runtime.

## Components

### `Program`

Selects normal UI, self-test, or UI smoke-test execution and starts the Windows Forms message loop.

### `MainForm`

Owns the user interface, selected scope, navigation, editor state, unsaved-change workflow, search, and save/reload actions.

### `FileDiscoveryService`

Builds the recognised file list for Global, Project, and Conversation scopes. It searches only the selected root and the root's top-level `.codex` directory rather than recursively traversing an arbitrary project.

### `SafeFileService`

Loads UTF-8 content, calculates SHA-256 fingerprints, detects external changes, creates timestamped backups, writes new content to a temporary sibling file, and atomically replaces the destination.

### `AppPaths` And `AppSettings`

`AppPaths` resolves `CODEX_HOME`, ChatGPT project storage, and the default conversation directory. `AppSettings` persists selected folders, word wrapping, and font size under Local Application Data.

## Data Flow

1. The user selects a scope and, where required, a folder.
2. `FileDiscoveryService` returns recognised file records.
3. `SafeFileService` loads the selected file and records its fingerprint.
4. The user edits content in memory.
5. On save, the service checks the current fingerprint, backs up an existing destination, writes a temporary file, and replaces the destination.

## Dependencies And Boundaries

- .NET 8 Windows Desktop runtime and Windows Forms.
- No third-party NuGet packages are declared.
- Filesystem permissions, available disk space, filesystem semantics, and user decisions form the primary operational boundary.
- Context file syntax and interpretation belong to the consuming application; Context Studio is a text editor and does not validate semantic correctness.

## Known Limitations

- Automated coverage is intentionally small and does not exercise every UI workflow or filesystem edge case.
- The UI smoke test verifies startup and shutdown, not complete user acceptance.
- Settings load/save errors are suppressed, so persistence failures are not currently surfaced to the user.
- Supported Windows editions and versions have not been formally tested or guaranteed.
- No installer, updater, code signing, telemetry, or network integration exists.
