# Contributing to AWS Lambda Host

Thank you for your interest in contributing to the AWS Lambda Host project! This document provides guidelines and instructions for contributing.

## Getting Started

1. Fork the repository
2. Clone your fork locally
3. Create a feature branch from `main`
4. Make your changes
5. Submit a pull request

## Setting Up Your Development Environment

### Prerequisites
- .NET 8.0 or later
- Node.js (for commitlint and husky)
- Git

### Installation

```bash
# Clone the repository
git clone https://github.com/j-d-ha/aws-lambda-host.git
cd aws-lambda-host

# Install Node dependencies (for commit hooks)
npm install

# Restore .NET dependencies
dotnet restore

# Build the project
dotnet build
```

## Commit Message Guidelines

This project enforces **conventional commits** format for all commit messages.

### Format

```
<type>(scope): <description>

[optional body]

[optional footer(s)]
```

### Types

- **feat**: A new feature
- **fix**: A bug fix
- **docs**: Documentation changes only
- **refactor**: Code changes that don't add features or fix bugs
- **test**: Adding or updating tests
- **chore**: Changes to build process, dependencies, or tooling
- **ci**: Changes to CI/CD configuration

### Scope (Optional)

Recommended scopes for this project:
- `host` - Changes to AwsLambda.Host package
- `abstractions` - Changes to AwsLambda.Host.Abstractions package
- `opentelemetry` - Changes to AwsLambda.Host.OpenTelemetry package
- `deps` - Dependency updates
- `ci` - CI/CD changes
- `github` - GitHub-specific changes

### Examples

```
feat(host): add new Lambda handler support

fix(abstractions): resolve dependency issue

docs: update README with examples

chore(deps): bump package version

feat(host): redesign handler pipeline

BREAKING CHANGE: Handler API has changed
```

### Breaking Changes

If your commit introduces a breaking change, include it in the footer:

```
feat(host): redesign handler pipeline

BREAKING CHANGE: The Handler interface has changed. See migration guide in BREAKING_CHANGES.md
```

## Pull Request Guidelines

### PR Title Format

Pull request titles **MUST** follow the conventional commits format (same rules as commit messages).

**Note:** If your PR will be squashed on merge, the PR title becomes the commit message.

**Valid PR titles:**
- `feat(host): add new Lambda handler support`
- `fix(abstractions): resolve dependency issue`
- `docs: update README with examples`

**Invalid PR titles:**
- `Fixed a bug` ❌
- `Updates` ❌
- `WIP: New feature` ❌

### PR Checklist

- [ ] Title follows conventional commits format
- [ ] Branch is up to date with `main`
- [ ] All tests pass locally (`dotnet test`)
- [ ] Code follows project style guidelines
- [ ] Documentation has been updated if needed
- [ ] No breaking changes, or documented in PR body
- [ ] Commits are atomic and well-described

## Code Style

### C# XML Documentation

Use only XML tags supported by C#. Avoid tags like `<strong>` - use **markdown** in comments instead.

```csharp
/// <summary>
/// Initializes a new instance of the Handler class.
/// </summary>
/// <param name="configuration">The configuration to use</param>
/// <exception cref="ArgumentNullException">Thrown when configuration is null</exception>
public Handler(IConfiguration configuration) { }
```

### General Guidelines

- Follow C# naming conventions
- Use meaningful variable and method names
- Write self-documenting code
- Add comments for complex logic
- Keep methods focused and small

## Testing

All contributions should include tests:

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true

# Run specific test project
dotnet test src/AwsLambda.Host.Tests
```

## Building & Packaging

```bash
# Build in Release configuration
dotnet build --configuration Release

# Create NuGet packages
dotnet pack --configuration Release --output ./nupkg
```

## Submitting Changes

1. Push your changes to your fork
2. Create a pull request against the `main` branch
3. Ensure all CI checks pass
4. Address any review comments
5. A maintainer will merge your PR

## Release Process

The release process is automated and involves:

1. **Release Drafter** automatically prepares a draft release with changelog
2. Maintainers manually publish the release on GitHub
3. **GitHub Actions** automatically publishes packages to NuGet.org

For more details, see [RELEASE_PROCESS.md](../docs/RELEASE_PROCESS.md).

## Code of Conduct

Please note that this project is released with a [Contributor Code of Conduct](CODE_OF_CONDUCT.md). By participating in this project you agree to abide by its terms.

## Questions or Issues?

- Open an issue for bug reports
- Use discussions for questions
- Check existing issues before creating new ones

Thank you for contributing! 🎉
