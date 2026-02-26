## Context

The existing codebase, solution, and packaging all use the name `Monitor`. This includes project and folder names, namespaces, CI workflows, documentation, and manifests. A deep rename requires careful coordination to avoid broken references and ensure build continuity. The repository also contains a secondary project `monitormsix` that references the primary project by path.

## Goals / Non-Goals

**Goals:**
- Rename all visible occurrences of `Monitor` to `Operon` in filenames, namespaces, project settings, manifests, and documentation.
- Update build scripts, workflows, and packaging names accordingly.
- Preserve history by renaming files rather than deleting/recreating when possible.

**Non-Goals:**
- Change any business logic or functionality beyond renaming.
- Modify external API contracts (other than name changes where appropriate).

## Decisions

- **Filesystem vs in-place editing**: We'll perform a combination of directory renames (e.g., `Monitor/` -> `Operon/`) and search-and-replace in file contents. This keeps git history contiguous.
- **Namespace convention**: All namespaces starting with `Monitor` will become `Operon`. We'll update root namespace in project files accordingly.
- **Solution rename**: The `.sln` file will be updated to refer to the new project path and name. We'll also rename the solution file itself to `Operon.sln`.
- **CI and packaging**: Update workflow YAML to refer to `Operon` and output zip names accordingly.
- **Manifests**: The UWP/packaging manifest uses lowercase `monitor`; change to `operon` and adjust description.

## Risks / Trade-offs

- [Risk] Mistedia of string replacement causing unintended renames (e.g., variable names containing "monitor"). → Mitigation: use case-sensitive replacements and review changes.
- [Risk] Break project references during rename process. → Mitigation: perform rename in small committed steps, update references immediately.
- [Risk] CI workflows may fail until names updated. → Mitigation: test build locally before pushing.

## Migration Plan

1. Rename solution file and adjust `.sln` contents.
2. Rename `Monitor` directory to `Operon` and update project files inside.
3. Perform search-and-replace in code, XAML, manifest, and config files.
4. Rename `monitormsix` package references if needed.
5. Update CI workflow YAML, README, and documentation text.
6. Commit in a series of PRs if necessary; run full build.

## Open Questions

- Should the `monitormsix` folder be renamed as well? It may remain as `operonmsix` for consistency.
- Are there external integration tests or deployment scripts outside this repo that need manual update? Not yet identified.
