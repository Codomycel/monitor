# Releasing Operon

Operon releases are built automatically by GitHub Actions when you push a version tag.

## Prerequisites

- All changes for the release are merged on the default branch.
- The WPF project builds successfully (`Operon/Operon.csproj`).

## Create a new release

1. Choose the next semantic version (for example `v1.0.0`).
2. Create and push the tag:

```bash
git tag v1.0.0
git push origin v1.0.0
```

3. GitHub Actions workflow [`.github/workflows/release.yml`](.github/workflows/release.yml) runs automatically.

## What the workflow does

On tag push matching `v*.*.*`, the workflow:

1. Builds `Operon/Operon.csproj` in **Release** mode.
2. Publishes a portable **Windows x64** self-contained single-file EXE:
   - Runtime: `win-x64`
   - `PublishSingleFile=true`
   - `IncludeNativeLibrariesForSelfExtract=true`
3. Packages the EXE into a zip named like `Operon-v1.0.0-portable-win-x64.zip`.
4. Creates (or updates) the GitHub Release for the same tag and uploads the zip as a release asset.

## Runtime data

Operon stores settings, logs, manual tasks, and leave data under:

`%LocalAppData%\SystemActivityTracker\`

No extra config files need to be shipped with the portable EXE; the app creates data files on first run.

## Manual local publish (optional)

```powershell
dotnet publish .\Operon\Operon.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  /p:PublishSingleFile=true `
  /p:IncludeNativeLibrariesForSelfExtract=true `
  -o .\publish
```

The portable executable is `publish\Operon.exe`.
