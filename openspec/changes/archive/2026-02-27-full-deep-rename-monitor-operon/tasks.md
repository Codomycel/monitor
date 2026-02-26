## 1. Solution and Project Rename

- [x] 1.1 Rename `Monitor.sln` to `Operon.sln` and update its contents to reference `Operon\\Operon.csproj`
- [x] 1.2 Rename `Monitor` directory to `Operon` on disk
- [x] 1.3 Update `.gitignore` paths from `/Monitor/` to `/Operon/`

## 2. Update Project Files

- [x] 2.1 Edit `Operon.csproj` to set `<AssemblyName>Operon</AssemblyName>` and adjust namespace root if needed
- [x] 2.2 Update any `.wapproj` or package project references (monitormsix) to point to the new path

## 3. Search-and-Replace in Source

- [x] 3.1 Replace namespace declarations (`namespace Monitor` -> `namespace Operon`) across code and XAML
- [x] 3.2 Replace any literal strings or comments with the project name where appropriate (excluding unrelated words containing "monitor")
- [x] 3.3 Update `App.xaml` and other resource references if they embed the old name
- [x] 3.4 Replace all user-facing text "System Activity Tracker" with "Operon"
      - Update MainWindow title (Title property)
      - Update header display text in UI
      - Update system tray tooltip text
      - Update any About/help screen references
      - Verify no UI still displays "System Activity Tracker"
## 4. Update Manifests and Documentation

- [x] 4.1 Edit `monitormsix/Package.appxmanifest` DisplayName and Description fields
- [x] 4.2 Update README.md and README.dev.md with the new project name
- [x] 4.3 Update openspec/project.md title and description to mention Operon

## 5. CI and Packaging

- [x] 5.1 Modify `.github/workflows/Release (WPF).yml` to publish `Operon.csproj` and produce `Operon-win-x64.zip`
- [x] 5.2 Adjust any other build scripts or workflow steps referencing Monitor

## 6. Final Verification

- [x] 6.1 Build solution locally and run any existing tests (if available)
- [x] 6.2 Confirm no remaining occurrences of `Monitor` in repo (case-sensitive search)
- [x] 6.3 Commit changes in logical groups and update git history as needed
