# GitHub Configuration

This directory contains GitHub-specific configuration files for automated workflows and dependency management.

## Structure

```
.github/
├── workflows/          # GitHub Actions workflows
│   ├── ci.yml         # Main CI pipeline (builds, tests, quality checks)
│   └── release.yml    # Release pipeline (versioned deployments)
├── dependabot.yml     # Automated dependency updates
└── README.md          # This file
```

## Workflows

### CI Build and Test (`ci.yml`)
- **Triggers**: Push to main/develop/claude/* branches, PRs
- **Purpose**: Continuous integration - validate all code changes
- **Runs**: Build, test, code quality checks, platform-specific builds
- **Duration**: ~10-15 minutes

### Release Build (`release.yml`)
- **Triggers**: Version tags (v1.0.0, v2.1.3, etc.)
- **Purpose**: Create official releases with downloadable installers
- **Produces**: Android APK, Windows MSIX, GitHub Release
- **Duration**: ~15-20 minutes

## Quick Start

### To run CI on your branch:
```bash
git push origin your-branch-name
# GitHub Actions will automatically run CI
```

### To create a release:
```bash
git tag v1.0.0
git push origin v1.0.0
# GitHub Actions will build and publish the release
```

## Documentation

See [docs/CI-CD.md](../docs/CI-CD.md) for comprehensive CI/CD documentation including:
- Detailed workflow explanations
- Release process
- Troubleshooting guide
- Configuration options
- Best practices

## Status Badges

Add these to your main README.md:

```markdown
![CI](https://github.com/HawkTaka/Receipt-Scanner-Lite/workflows/CI%20Build%20and%20Test/badge.svg)
![Release](https://github.com/HawkTaka/Receipt-Scanner-Lite/workflows/Release%20Build/badge.svg)
```
