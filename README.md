# Ship It! — Build //localhost: Cape Town

### Build, test & deploy AI-powered apps with GitHub Copilot and Azure AI Foundry

> *From a blank VS Code window to a production-ready, AI-powered cloud app — live, on stage.*
> No slides after the intro. No pre-built answers. Just real, end-to-end developer productivity.

**Speaker:** Werner Rall — Senior Cloud Solution Architect, Microsoft
[GitHub](https://github.com/WernerRall147) · [LinkedIn](https://www.linkedin.com/in/werner-rall/)
**Event:** Build //localhost: Cape Town · 13 June 2026 · `#MSBuild` `#localhost`
**Level:** 200 — Intermediate, technical · **Duration:** 60 min (45 min live build + 15 min Q&A)

➡️ **Presenter walkthrough (exact steps):** [build-localhost/demos/ConfHub/README.md](build-localhost/demos/ConfHub/README.md)

---

## What you'll walk away knowing

Six things, one workflow:

| | |
|---|---|
| **Set up VS Code & Copilot** — tuned for maximum productivity | **Build with agent mode** — a cloud app on Azure Cosmos DB, from natural language |
| **Generate tests & coverage** — AI-guided xUnit + Coverlet, not an afterthought | **Review PRs with Copilot** — AI summaries, inline suggestions, agent-mode fixes |
| **Build an AI Agent** — code-first with Azure AI Foundry in ~20 lines | **Ground Copilot in your data** — expose the app as an MCP server it can query live |

---

## What we build — ConfHub

A conference session tracker: a REST API backed by Azure Cosmos DB, AI-powered
session recommendations via an Azure AI Foundry agent, and a built-in MCP server
that GitHub Copilot can query for live data — deployed to Azure Container Apps.

```
┌─────────────────────────────────────────────────────────────────┐
│  VS Code + GitHub Copilot (agent mode)                          │
│                                                                 │
│  ┌─────────────────┐     ┌──────────────────┐                  │
│  │  ConfHub API    │────▶│  Azure Cosmos DB  │  (keyless / MI) │
│  │  (.NET 10)      │     │  (Sessions data)  │                  │
│  └────────┬────────┘     └──────────────────┘                  │
│           │  Azure AI Foundry (Persistent Agents)              │
│           ▼                                                     │
│  ┌─────────────────┐     ┌──────────────────┐                  │
│  │  MCP Server     │◀────│  GitHub Copilot  │                  │
│  │  (built-in)     │     │  (MCP client)    │                  │
│  └─────────────────┘     └──────────────────┘                  │
│                                                                 │
│  Deployed with azd → Azure Container Apps                       │
└─────────────────────────────────────────────────────────────────┘
```

---

## The live build — 45 minutes

| Time | Segment | What happens |
|------|---------|--------------|
| 0–5 min | **Setup** | Install extensions, sign in to GitHub Copilot |
| 5–15 min | **Scaffold** | .NET 10 Minimal API + Azure Cosmos DB, via agent mode |
| 15–25 min | **Test** | Unit tests & code coverage with Copilot |
| 25–35 min | **Ship** | Push, open a PR, let Copilot summarise & review it |
| 35–50 min | **Extend** | AI Agent in Azure AI Foundry, exposed via MCP |
| 50–60 min | **Q&A** | Your questions, live |

---

## Take this home — five things that stick

1. **Agent mode** can scaffold a full cloud app in minutes — with the right prompts.
2. **Tests and coverage** aren't an afterthought; Copilot makes them as easy as the code.
3. **PRs are better with AI:** automatic summaries, inline suggestions, agent-mode fixes.
4. **Azure AI Foundry** lets you build a code-first AI Agent in ~20 lines of .NET.
5. **MCP closes the loop** — your app becomes a live data source Copilot can query.

---

## Built with — the stack on stage

| AI & Copilot | Azure | Platform |
|--------------|-------|----------|
| GitHub Copilot (agent mode) | Azure AI Foundry | .NET 10 Minimal API |
| Copilot Chat & Edits | AI Agent Service (Persistent Agents) | xUnit · Coverlet |
| PR summaries & review | Azure Cosmos DB (serverless, keyless) | Model Context Protocol (MCP) |
| | Azure Container Apps · azd · Bicep | VS Code |

---

## Run it yourself

The demo is fully repeatable — one command up, one command down. Everything is
keyless (Microsoft Entra ID / managed identity); no secrets in source control.

```powershell
# Point the scripts at your subscription + tenant (git-ignored)
cp .env.sample .azure/.env

az login --tenant <tenant_id>
az account set --subscription <subscription_id>

./build-localhost/demos/ConfHub/scripts/Provision.ps1   # azd up — infra + deploy
./build-localhost/demos/ConfHub/scripts/Seed-Cosmos.ps1 # seed sample sessions
./build-localhost/demos/ConfHub/scripts/Cleanup.ps1     # azd down --force --purge
```

Full prerequisites, exact presenter steps, configuration and architecture details
are in the **[presenter walkthrough](build-localhost/demos/ConfHub/README.md)**.

| | |
|---|---|
| Demo app (ConfHub) | [build-localhost/demos/ConfHub](build-localhost/demos/ConfHub) |
| Repeatable scripts | [build-localhost/demos/ConfHub/scripts](build-localhost/demos/ConfHub/scripts) |
| Infrastructure (Bicep) | [build-localhost/demos/ConfHub/infra](build-localhost/demos/ConfHub/infra) |
| Speaker deck | [build-localhost/Ship It - Speaker Deck.pptx](build-localhost/Ship%20It%20-%20Speaker%20Deck.pptx) |

---

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
