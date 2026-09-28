<!-- repository-standard: schema=1; standard=Repository Standards; version=1.0.0; owner=terryrogers; source=local; scope=local-override; override=local-file; overrides=https://github.com/terryrogers/.github/blob/main/SECURITY.md -->
# Security Policy

## Supported Versions

| Release | Security Maintenance |
| --- | --- |
| Latest Published Release | Eligible for security fixes where a safe and maintainable correction is available. |
| Earlier Releases | Not routinely supported; upgrade to the latest published release before requesting a fix. |
| Unreleased Code | Accepted for early reporting but is not a supported release. |

## Reporting A Vulnerability

Use the repository's **Security** tab to submit a private vulnerability report when that option is available. If it is unavailable, email `support@cloudhub.digital` with the subject prefix `[SECURITY REPORT]`.

Do not open a public issue, discussion, or pull request containing vulnerability details, credentials, personal data, internal infrastructure, sensitive reproduction data, or exploit material.

Include:

- the affected product and version or commit;
- the affected component and environment;
- a concise impact assessment;
- reproducible steps or a minimal proof of concept;
- relevant logs or screenshots with secrets and personal data removed; and
- any known workaround or suggested remediation.

## Response Process

Maintainers will acknowledge and triage reports as soon as reasonably practicable. They may request more evidence, attempt to reproduce the issue, assess affected versions, prepare and validate a correction, and coordinate publication of an advisory or fixed release. No response or remediation deadline is promised unless a project-specific agreement states one.

## Coordinated Disclosure

Keep the report and supporting material private until maintainers confirm that disclosure is safe. Allow reasonable time for triage, correction, validation, and affected-user communication. Maintainers will credit reporters when requested and appropriate, subject to confidentiality and safety constraints.

## Security Updates

Security corrections are published through the repository's normal release or advisory channels. Release notes will describe user action where disclosure is safe. Users should run the latest supported release and apply security updates promptly.

## Scope

Reports are in scope when they demonstrate a security impact in source, packaged artifacts, supported integrations, authentication or authorization, data handling, update or installation behavior, or documented deployment defaults maintained by this repository.

Reports are normally out of scope when they concern unsupported versions, social engineering, denial-of-service testing against systems without authorization, automated findings without a reproducible impact, third-party services outside this project's control, or configuration that contradicts the documented security requirements.

## Safe Harbour

Good-faith research should avoid privacy violations, data loss, service disruption, persistence, lateral movement, and access beyond what is necessary to demonstrate the issue. Follow applicable law and test only systems you own or are explicitly authorized to assess.

## Confidentiality

Never include credentials, tokens, private keys, personal data, internal addresses, private paths, or confidential infrastructure details in a public report or artifact. Share only the minimum necessary evidence through the approved private route.

## Project-Specific Security Guidance

### Security Model

Context Studio is a local editor that operates with the current user's permissions. It has no runtime network or account integration and does not request elevation. Its primary risk is unintended modification or disclosure of sensitive instruction, memory, prompt, or context files.

### Existing Controls

- SHA-256 fingerprint comparison before saving a loaded file.
- Explicit user decision before overwriting an externally changed file.
- Timestamped backup before replacing an existing file.
- Temporary sibling write followed by replacement.
- Warning before saving recognised generated memory files.
- No credential storage in application settings.
- No third-party package dependencies in the project manifest.

### Operational Guidance

- Review the selected path before editing.
- Treat context and memory files as potentially sensitive.
- Keep backups protected by appropriate filesystem permissions and retention practices.
- Verify restored content before deleting any backup.
- Do not add secrets, credentials, private keys, tokens, logs, databases, personal data, or private infrastructure details to the repository.

### Known Gaps

- Reparse-point and symbolic-link behaviour is not explicitly constrained or tested.
- Malformed or unwritable settings are silently ignored.
- Backup retention and secure deletion are not implemented.
- No formal threat model, external audit, or code-signing process exists.

### Reporting

Use the private reporting route defined above. Include the affected Context Studio version or commit, the relevant file-handling path, reproducible steps, impact, & sanitized evidence.
