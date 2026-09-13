# Development And Verification

## Prerequisites

- Windows.
- .NET 8 SDK or a later SDK capable of targeting .NET 8 Windows Desktop.

## Restore And Build

```powershell
dotnet restore .\ContextStudio.slnx
dotnet build .\ContextStudio.slnx -c Release
```

## Automated Checks

```powershell
dotnet run --project .\ContextStudio\ContextStudio.csproj -c Release -- --self-test
dotnet run --project .\ContextStudio\ContextStudio.csproj -c Release -- --ui-smoke-test
```

Expected self-test output is `PASS: 6 Context Studio self-tests`. The UI smoke test normally exits without console output and must return exit code zero.

## Manual Acceptance Checks

Manual acceptance remains outstanding. At minimum, verify each scope, folder selection, ChatGPT project discovery, file creation, save and backup behaviour, external-change blocking, explicit overwrite, reload, unsaved-change prompts, search wrapping, word-wrap persistence, font-size persistence, and generated-file warning.

Use non-sensitive test files in a disposable folder. Do not use production instructions or memory registries for destructive test cases.

## Publication Checks

Before any push, review the complete staged tree and outgoing history for secrets, personal data, private infrastructure details, logs, databases, local settings, backups, build output, archives, and unintended files. Run local-only checks; do not upload source to external scanners without approval.

## Project Records

Planning, work-package status, acceptance evidence, and version governance are maintained separately from this repository.
