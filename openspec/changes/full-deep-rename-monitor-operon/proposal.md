## Why

The project name `Monitor` is too generic and conflicts with other systems. The new brand, "Operon," better reflects the product direction. A full rename ensures all code, resources, and artifacts align with the new identity and avoids confusion moving forward.

## What Changes

- **BREAKING**: Rename solution, projects, namespaces, folders, and all string references from `Monitor` to `Operon`.
- Update packaging, manifests, and CI workflows to use the new name.
- Adjust documentation, README, and open‑spec project context accordingly.

## Capabilities

### New Capabilities
- `rename-project`: Comprehensive search-and-replace across the codebase to adopt the `Operon` name.

### Modified Capabilities
- None: this is purely a refactor/rename without behavioral changes.

## Impact

- Solution and project files (`.sln`, `.csproj`, `.wapproj`) will be modified and/or renamed.
- Source code namespaces and resource paths.
- Build scripts, CI workflows, and packaging commands.
- Documentation and README files.
- External references (Package.appxmanifest, gitignore, etc.).
