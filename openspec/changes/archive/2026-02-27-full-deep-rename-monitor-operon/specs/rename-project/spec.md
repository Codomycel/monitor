## ADDED Requirements

### Requirement: Codebase renamed to Operon
All references, namespaces, filenames, and artifacts using the name `Monitor` SHALL be updated to `Operon`.

#### Scenario: Rename project directory and files
- **WHEN** developer runs the rename procedure
- **THEN** directory `Monitor/` becomes `Operon/` and all contained files reflect the new name

#### Scenario: Update namespaces and assembly names
- **WHEN** source files are edited
- **THEN** namespaces beginning with `Monitor` transition to `Operon` and `AssemblyName` in project files is `Operon`

#### Scenario: CI workflow uses Operon
- **WHEN** the build pipeline runs
- **THEN** it references `Operon` paths/filenames and produces `Operon-win-x64.zip`

#### Scenario: Manifest displays new name
- **WHEN** the Package.appxmanifest is read
- **THEN** `<DisplayName>` is `operon` and descriptions updated accordingly
