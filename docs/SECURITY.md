# Security And Privacy

## Security Model

Context Studio is a local editor that operates with the current user's permissions. It has no runtime network or account integration and does not request elevation. Its primary risk is unintended modification or disclosure of sensitive instruction, memory, prompt, or context files.

## Existing Controls

- SHA-256 fingerprint comparison before saving a loaded file.
- Explicit user decision before overwriting an externally changed file.
- Timestamped backup before replacing an existing file.
- Temporary sibling write followed by replacement.
- Warning before saving recognised generated memory files.
- No credential storage in application settings.
- No third-party package dependencies in the project manifest.

## Operational Guidance

- Review the selected path before editing.
- Treat context and memory files as potentially sensitive.
- Keep backups protected by appropriate filesystem permissions and retention practices.
- Verify restored content before deleting any backup.
- Do not add secrets, credentials, private keys, tokens, logs, databases, personal data, or private infrastructure details to the repository.

## Known Gaps

- Reparse-point and symbolic-link behaviour is not explicitly constrained or tested.
- Malformed or unwritable settings are silently ignored.
- Backup retention and secure deletion are not implemented.
- No formal threat model, external audit, code-signing process, or security disclosure channel exists.

## Reporting

No security-reporting address or response commitment has been established. Record a decision in OpenProject before publishing contact or support expectations.
