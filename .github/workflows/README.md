# GitHub Actions Workflows

This directory contains GitHub Actions workflows for the Fancy Container Console project.

## Workflows

### 1. CI - Build and Test (`ci.yml`)

**Trigger:** Runs on push and pull requests to `main` and `develop` branches

**Purpose:** Continuous Integration - ensures code quality by building and testing the project

**Steps:**
1. Checkout code
2. Setup .NET 10.0 (preview)
3. Restore dependencies
4. Build the project in Release configuration
5. Run all tests
6. Generate test summary

**When it runs:**
- Every push to `main` or `develop` branches
- Every pull request targeting `main` or `develop` branches

---

### 2. Release - Publish Executables (`release.yml`)

**Trigger:** Runs on release publication or version tags (v*.*.*)

**Purpose:** Build and publish platform-specific executables for distribution

**Steps:**
1. Checkout code
2. Setup .NET 10.0 (preview)
3. Extract version from tag
4. Restore dependencies
5. Build self-contained executables for:
   - Windows x64
   - Linux x64
6. Create archives (ZIP for Windows, tar.gz for Linux)
7. Generate SHA256 checksums
8. Upload to GitHub Release (if triggered by tag)
9. Upload as artifacts (if triggered by release event)

**Output artifacts:**
- `FancyContainerConsole-win-x64.zip` - Windows executable
- `FancyContainerConsole-linux-x64.tar.gz` - Linux executable
- `checksums.txt` - SHA256 checksums for verification

**How to trigger a release:**
```bash
# Create and push a version tag
git tag v1.0.0
git push origin v1.0.0

# Or create a release through GitHub UI
```

---

## Requirements

- Repository must have Actions enabled
- For release workflow: Repository needs write permissions for releases
- Both workflows use:
  - .NET 10.0 (preview quality)
  - Ubuntu latest runner

## Troubleshooting

### CI workflow fails on test step
- Check that all tests pass locally: `dotnet test`
- Ensure test project path is correct

### Release workflow fails to upload assets
- Verify the tag follows the format `v*.*.*` (e.g., v1.0.0)
- Check repository permissions allow Actions to create releases

### Build fails with .NET version error
- Update the `dotnet-version` in workflows if using a different .NET version
- Remove `dotnet-quality: 'preview'` if using a stable release
