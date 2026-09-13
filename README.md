# Context Studio

Context Studio is a local Windows desktop editor for Codex and ChatGPT context files. It keeps global, project, and conversation context visibly separate and protects edits with backups and external-change detection.

The source repository is [terryrogers/Context-Studio](https://github.com/terryrogers/Context-Studio). Project planning and governance records are maintained separately from this repository.

## Project Status

- **Confirmed:** The application is a C#/.NET 8 Windows Forms desktop application.
- **Confirmed:** Version metadata currently identifies the build as `1.0.0`.
- **Confirmed:** The current source passes a Release build, six self-tests, and a Windows UI smoke test.
- **Confirmed:** A local self-contained Windows x64 build exists in the retained recovery workspace.
- **Inferred:** `1.0.0` is a completed build candidate. No evidence establishes a formal release, Git tag, GitHub Release, or deployment.

## Features

- Global, Project, and Conversation scopes.
- Discovery of `AGENTS.md`, `AGENTS.override.md`, memory registries, and recognised prompt/context Markdown files.
- ChatGPT project discovery under the configured Codex home.
- Search, word wrapping, font sizing, and persisted folder preferences.
- Unsaved-change prompts and SHA-256 external-change detection.
- UTF-8 writes through a temporary file and atomic replacement.
- Timestamped sibling backups in `.context-studio-backups`.
- Clear warnings before generated memory files are edited.
- No network or account integration and no elevation request.

## Requirements

- Windows with the .NET 8 SDK for development.
- The application runs with the current user's filesystem permissions.

No supported-Windows-version guarantee has yet been adopted beyond the project's `net8.0-windows` target.

## Build And Run

```powershell
dotnet build .\ContextStudio.slnx -c Release
dotnet run --project .\ContextStudio\ContextStudio.csproj -c Release
```

## Tests

```powershell
dotnet run --project .\ContextStudio\ContextStudio.csproj -c Release -- --self-test
dotnet run --project .\ContextStudio\ContextStudio.csproj -c Release -- --ui-smoke-test
```

The self-test covers file creation, backup creation and content, external-change blocking, explicit overwrite, and project instruction discovery. The smoke test verifies that the Windows message loop can create and close the main form. These checks do not replace user acceptance testing.

## Publish A Portable Windows Build

```powershell
dotnet publish .\ContextStudio\ContextStudio.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\publish\win-x64
```

Treat the complete output directory as the portable application. Even with single-file publishing enabled, runtime companion files can be emitted and must remain beside `ContextStudio.exe`. A distributable archive, checksum, formal release notes, tag, and GitHub Release have not yet been approved or produced.

## Local Data And Recovery

Application settings are stored under `%LOCALAPPDATA%\Context Studio\settings.json`. They contain selected folder paths and UI preferences, not credentials.

Before replacing an existing context file, Context Studio creates a timestamped backup in a `.context-studio-backups` directory beside that file. If a file changes after it is loaded, the application blocks the normal save and requires an explicit overwrite decision.

## Documentation

- [Architecture](docs/ARCHITECTURE.md)
- [Development And Verification](docs/DEVELOPMENT.md)
- [Security And Privacy](docs/SECURITY.md)
- [Release And Recovery](docs/RELEASES.md)
- [Changelog](CHANGELOG.md)
- [Roadmap](ROADMAP.md)

## Licence And Support

No licence, copyright policy, support commitment, security disclosure address, or compatibility guarantee has been selected. Until those decisions are made, do not infer permission to redistribute the source or binaries.
