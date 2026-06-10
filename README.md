# Build //localhost Cape Town — Ship It Demo

**Ship It: Build, Test & Deploy AI-Powered Apps with GitHub Copilot and Azure AI Foundry**

A fully **repeatable**, end-to-end demo that goes from a blank VS Code window to a
production-ready, AI-powered cloud application — showcasing the latest Microsoft
developer stack from Build.

➡️ **Start here:** [build-localhost/demos/ConfHub/README.md](build-localhost/demos/ConfHub/README.md)

## What it showcases

- **GitHub Copilot agent mode** in VS Code
- **.NET 10** Minimal API
- **Azure Cosmos DB** (serverless, keyless via Microsoft Entra ID)
- **Azure AI Foundry** — GA Persistent Agents SDK
- **Model Context Protocol (MCP)** — grounding Copilot in live app data
- **Azure Developer CLI (azd) + Bicep** — one command up, one command down

## Map

| | |
|---|---|
| Session details | [build-localhost/session-details.md](build-localhost/session-details.md) |
| Demo app (ConfHub) | [build-localhost/demos/ConfHub](build-localhost/demos/ConfHub) |
| Repeatable scripts | [build-localhost/demos/ConfHub/scripts](build-localhost/demos/ConfHub/scripts) |
| Infrastructure (Bicep) | [build-localhost/demos/ConfHub/infra](build-localhost/demos/ConfHub/infra) |

## Azure environment

Copy [.env.sample](.env.sample) to `.azure/.env` and fill in your subscription and
tenant IDs. The `.azure/` folder is git-ignored, so your IDs and any service
principal credentials are never committed.

```powershell
cp .env.sample .azure/.env
az login --tenant <tenant_id>
az account set --subscription <subscription_id>
./build-localhost/demos/ConfHub/scripts/Provision.ps1
```

## Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Legal Notices

Microsoft and any contributors grant you a license to the Microsoft documentation and other content
in this repository under the [Creative Commons Attribution 4.0 International Public License](https://creativecommons.org/licenses/by/4.0/legalcode),
see the [LICENSE](LICENSE) file, and grant you a license to any code in the repository under the [MIT License](https://opensource.org/licenses/MIT), see the
[LICENSE-CODE](LICENSE-CODE) file.

Microsoft, Windows, Microsoft Azure and/or other Microsoft products and services referenced in the documentation
may be either trademarks or registered trademarks of Microsoft in the United States and/or other countries.
The licenses for this project do not grant you rights to use any Microsoft names, logos, or trademarks.
