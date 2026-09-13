# Release And Recovery

## Version State

`1.0.0` is confirmed by the project and manifest metadata and by a retained built executable. It is not confirmed as formally released, tagged, published to GitHub Releases, or deployed.

## Build A Portable Package

```powershell
dotnet publish .\ContextStudio\ContextStudio.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\publish\win-x64
```

Package the entire publish output directory. Do not distribute only `ContextStudio.exe` when companion runtime files were emitted. A final portable archive should include extraction instructions and a SHA-256 checksum.

## Release Gate

Before a formal release:

1. Confirm the exact OpenProject version and `Versions` board representation.
2. Complete automated and manual acceptance checks.
3. Complete repository, dependency, secret, privacy, and publication reviews.
4. Confirm version metadata and release notes.
5. Obtain explicit approval for the exact commit, Git tag, release title, notes, and artifacts.
6. Create and verify the immutable tag and GitHub Release.
7. If installed or otherwise put into use, verify the deployed artifact identity against the same canonical version.

## File Recovery

For an edited context file, use the timestamped copies under the sibling `.context-studio-backups` directory. Compare the intended backup with the current file before restoring it, retain a rollback copy, and validate the consuming application's behaviour after restoration.

## Application Rollback

Portable application rollback consists of retaining the complete previously verified package, replacing the complete package directory, and verifying the executable and companion-file checksums. No automated installer, migration, or rollback mechanism exists.
