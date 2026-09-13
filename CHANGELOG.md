# Changelog

This file records evidence-supported project changes. No formal release or Git tag is implied by an entry.

## Unreleased

### Added

- Project governance documentation and reciprocal GitHub/OpenProject references.
- Publication exclusions for build output, local settings, backups, logs, databases, archives, and common secret-bearing files.

### Changed

- Expanded build, verification, packaging, recovery, security, and project-status guidance.

## 1.0.0 Build Candidate - 2026-09-05

### Added

- Windows Forms interface with distinct Global, Project, and Conversation scopes.
- Discovery of Codex instruction and memory files and ChatGPT project folders.
- Search, word wrapping, font controls, folder selection, and persisted local preferences.
- SHA-256 external-change detection, temporary-file replacement, timestamped backups, and generated-file warnings.
- Six non-interactive file-safety and discovery self-tests.
- Windows UI startup and close smoke test.
- Self-contained Windows x64 publish configuration.

### Verification

- The original build record reports a Release build with no warnings or errors, six passing self-tests, a passing UI smoke test, and a locally published executable.
- The migrated source was hash-verified against the retained original source set.
- The current source was independently rebuilt and retested successfully on 2026-09-14.

### Status

This is confirmed as a built version candidate. No available evidence establishes a formal public or private release, immutable tag, GitHub Release, or deployment.
