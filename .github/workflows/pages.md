# Workflow: Deploy DocFX to GitHub Pages

Builds the MCP Server API reference with [DocFX](https://dotnet.github.io/docfx/) and deploys it to GitHub Pages.

## Trigger

| Event | Condition |
|:--|:--|
| `release: published` | Runs automatically on every published release |
| `workflow_dispatch` | Manual trigger via GitHub Actions UI |

## Steps

| Step | Description |
|:--|:--|
| Checkout | Checks out the repository |
| Setup .NET | Installs .NET 10 SDK |
| Configure GitHub Packages | Adds the Payroll-Engine NuGet feed using `PAT_DISPATCH` |
| Publish tools assembly | `dotnet publish Tools/PayrollEngine.Mcp.Tools.csproj -c Release -o publish` |
| Install DocFX | Installs the latest DocFX global tool |
| Build DocFX | Runs `docfx docfx/docfx.pages.json` — generates HTML into `docfx/_site/` |
| Upload Pages artifact | Uploads `docfx/_site/` as the GitHub Pages artifact |
| Deploy to GitHub Pages | Deploys the artifact to the `github-pages` environment |

## Output

👉 https://payroll-engine.github.io/PayrollEngine.Mcp.Server/

## Permissions

| Permission | Reason |
|:--|:--|
| `contents: read` | Checkout |
| `pages: write` | Deploy to GitHub Pages |
| `id-token: write` | OIDC token for Pages deployment |

## Secrets

| Secret | Usage |
|:--|:--|
| `PAT_DISPATCH` | Read access to the Payroll-Engine GitHub Packages NuGet feed |

## Local Build

```cmd
cd docfx
Static.Build.cmd   # publish assembly + build static site
Static.Start.cmd   # open _site/index.html in browser
Server.Start.cmd   # publish assembly + serve with live reload on port 5866
```
