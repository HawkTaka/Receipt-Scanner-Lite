# CI/CD Documentation

This document describes the Continuous Integration and Continuous Deployment (CI/CD) setup for Receipt Scanner Lite.

## Overview

The project uses GitHub Actions for automated builds, testing, and releases. The CI/CD pipeline ensures code quality, runs tests, and produces platform-specific builds for Android and Windows.

## Workflows

### 1. CI Build and Test (`ci.yml`)

**Triggers:**
- Push to `main`, `develop`, or `claude/**` branches
- Pull requests to `main` or `develop`
- Manual trigger via GitHub Actions UI

**Jobs:**

#### `build-and-test`
- Runs on: Windows (for MAUI compatibility)
- Steps:
  1. Checkout code
  2. Setup .NET 8
  3. Install .NET MAUI workload
  4. Restore NuGet packages
  5. Build solution in Release configuration
  6. Run tests (with test result reporting)
  7. Upload build artifacts

**Artifacts**: Build outputs (7 day retention)

#### `code-quality`
- Runs on: Ubuntu (faster for analysis)
- Steps:
  1. Format checking with `dotnet format`
  2. Code analysis with compiler warnings
  3. Style enforcement

**Note**: Quality checks don't fail the build, they provide warnings

#### `build-android`
- Runs on: Windows
- Depends on: `build-and-test` passing
- Only runs: On push or manual trigger (not on PR)
- Produces: Android APK
- Artifacts: APK file (30 day retention)

#### `build-windows`
- Runs on: Windows
- Depends on: `build-and-test` passing
- Only runs: On push or manual trigger (not on PR)
- Produces: Windows MSIX package
- Artifacts: MSIX/publish folder (30 day retention)

### 2. Release Build (`release.yml`)

**Triggers:**
- Git tags matching `v*.*.*` (e.g., `v1.0.0`, `v2.1.3`)
- Manual trigger with version input

**Jobs:**

#### `create-release`
- Creates a GitHub Release with release notes
- Extracts version from tag or manual input
- Provides upload URL for release assets

#### `build-android-release`
- Builds production-ready Android APK
- Sets version metadata
- Uploads APK to GitHub Release
- **TODO**: Add APK signing when keystore configured

#### `build-windows-release`
- Builds production-ready Windows MSIX
- Sets version metadata
- Uploads MSIX (or ZIP if MSIX not generated) to GitHub Release

## Creating a Release

### Method 1: Using Git Tags (Recommended)

```bash
# 1. Ensure you're on main branch with latest code
git checkout main
git pull

# 2. Create and push a version tag
git tag v1.0.0
git push origin v1.0.0

# 3. GitHub Actions will automatically:
#    - Create a GitHub Release
#    - Build Android APK
#    - Build Windows MSIX
#    - Upload both to the release
```

### Method 2: Manual Trigger

1. Go to **Actions** tab in GitHub
2. Select **Release Build** workflow
3. Click **Run workflow**
4. Enter version number (e.g., `1.0.0`)
5. Click **Run workflow**

## Viewing Build Results

### CI Builds
1. Go to **Actions** tab
2. Select **CI Build and Test** workflow
3. Click on a specific run to see:
   - Build logs
   - Test results
   - Code quality reports
   - Download artifacts

### Releases
1. Go to **Releases** section
2. View published releases
3. Download platform-specific installers:
   - `ReceiptScannerLite-v1.0.0-android.apk`
   - `ReceiptScannerLite-v1.0.0-windows.msix`

## Automated Dependency Updates

Dependabot is configured to automatically check for updates weekly (Mondays at 9 AM).

**What gets updated:**
- NuGet packages (.NET dependencies)
- GitHub Actions versions

**Package Groups:**
- **microsoft**: All Microsoft.* packages grouped together
- **communitytoolkit**: CommunityToolkit.* packages
- **testing**: Test-related packages

**Pull Request Limits:**
- Maximum 5 NuGet PRs at once
- Maximum 3 GitHub Actions PRs at once

**Review Process:**
1. Dependabot creates PR with update
2. CI runs automatically on PR
3. Review changes and test results
4. Merge if tests pass

## Build Artifacts

### CI Builds
**Location**: GitHub Actions → Select run → Artifacts section

**Available Artifacts:**
- `build-artifacts`: General build outputs (7 days)
- `android-apk`: Android APK file (30 days)
- `windows-msix`: Windows MSIX package (30 days)

### Release Builds
**Location**: GitHub Releases page

**Available Downloads:**
- Android APK (permanent)
- Windows MSIX (permanent)

## Configuration

### Environment Variables

Defined in workflow files (`.github/workflows/*.yml`):

```yaml
env:
  DOTNET_VERSION: '8.0.x'          # .NET SDK version
  SOLUTION_PATH: 'ReceiptScannerLite.sln'
  PROJECT_PATH: 'src/ReceiptScannerLite/ReceiptScannerLite.csproj'
```

### Secrets Required

Currently no secrets are required. When adding signing:

**For Android:**
- `ANDROID_KEYSTORE`: Base64-encoded keystore file
- `ANDROID_KEYSTORE_PASSWORD`: Keystore password
- `ANDROID_KEY_ALIAS`: Key alias
- `ANDROID_KEY_PASSWORD`: Key password

**For Windows:**
- `WINDOWS_PFX_FILE`: Base64-encoded PFX certificate
- `WINDOWS_PFX_PASSWORD`: Certificate password

## Troubleshooting

### Build Fails: "Workload not found"
**Solution**: The workflow installs MAUI workload automatically. If it fails, check .NET SDK version compatibility.

### Android Build Fails: "SDK not found"
**Solution**: Windows runners include Android SDK. Check if MAUI workload installation succeeded.

### Test Results Not Showing
**Solution**: Tests may not exist yet. The workflow continues even with no tests (`continue-on-error: true`).

### Format Check Fails
**Solution**: Run locally before pushing:
```bash
dotnet format ReceiptScannerLite.sln
git commit -am "Fix code formatting"
git push
```

## Adding Tests

When you add tests to the project:

1. Create test project (e.g., `tests/ReceiptScannerLite.Tests/`)
2. Add to solution: `dotnet sln add tests/ReceiptScannerLite.Tests/ReceiptScannerLite.Tests.csproj`
3. CI will automatically discover and run tests
4. Test results will appear in the Actions UI

## Performance Optimization

### Caching
- NuGet packages are automatically cached by `setup-dotnet` action
- Reduces build time by ~30-40%

### Parallel Builds
- Android and Windows builds run in parallel after main build
- Reduces total pipeline time

### Artifact Retention
- CI artifacts: 7 days (lower storage costs)
- Release artifacts: 30 days (keep recent releases available)
- GitHub Releases: Permanent (official release versions)

## Best Practices

### Branch Strategy
- `main`: Production-ready code
- `develop`: Integration branch for features
- `claude/*`: AI-assisted development branches
- Feature branches: Merge via PR to `develop`

### Commit Messages
- Use conventional commits: `feat:`, `fix:`, `docs:`, `ci:`
- Dependabot uses prefix: `deps:` for NuGet, `ci:` for Actions

### Version Tags
- Use semantic versioning: `v{major}.{minor}.{patch}`
- Tag format: `v1.0.0`, `v2.1.3`, etc.
- Don't reuse or move tags

### Release Notes
- GitHub generates automatic release notes from commits
- Edit release notes to add highlights and breaking changes
- Include installation instructions

## Future Enhancements

Potential improvements to consider:

1. **App Store Deployment**
   - Automate upload to Google Play Store
   - Automate upload to Microsoft Store

2. **Code Coverage**
   - Add code coverage reporting with Codecov or Coveralls
   - Enforce minimum coverage thresholds

3. **Performance Testing**
   - Add performance benchmarks
   - Track performance over time

4. **Security Scanning**
   - Add dependency vulnerability scanning
   - Add SAST (Static Application Security Testing)

5. **Environment Deployments**
   - Deploy to staging environment on develop branch
   - Deploy to production on release tags

6. **Notifications**
   - Slack/Discord notifications for build failures
   - Email notifications for releases

## Support

For issues with CI/CD:
1. Check workflow run logs in Actions tab
2. Review this documentation
3. Check GitHub Actions documentation
4. Open an issue with logs attached
