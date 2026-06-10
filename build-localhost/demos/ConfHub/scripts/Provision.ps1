<#
.SYNOPSIS
    Provisions all Azure resources and deploys the ConfHub API using azd.
.DESCRIPTION
    Repeatable, one-command provisioning. Reads the subscription/tenant from
    .azure/.env, initialises the azd environment, and runs `azd up` (provision
    infrastructure from infra/*.bicep + build & deploy the container image).

    After it completes, it writes a local appsettings.Development.json so you can
    also run the API locally against the same Azure backing services for the
    "localhost + MCP" part of the demo.
.PARAMETER Location
    Azure region to deploy to. Defaults to eastus2 (broad model availability).
.PARAMETER EnvName
    azd environment name. Defaults to "confhub".
.PARAMETER SkipDeploy
    Only provision infrastructure (azd provision) without building/deploying the app.
#>
[CmdletBinding()]
param(
    [string] $Location = 'eastus2',
    [string] $EnvName = 'confhub',
    [switch] $SkipDeploy
)

. "$PSScriptRoot/_Common.ps1"

$demoEnv = Get-DemoEnv
Assert-AzureContext -DemoEnv $demoEnv
Assert-Command -Name azd -InstallHint 'Install from https://aka.ms/azd'

$demoRoot = Get-DemoRoot
Push-Location $demoRoot
try {
    # Create the azd environment if it does not exist yet.
    $existing = azd env list --output json 2>$null | ConvertFrom-Json
    if (-not ($existing | Where-Object { $_.Name -eq $EnvName })) {
        Write-Host "Creating azd environment '$EnvName' ..." -ForegroundColor Cyan
        azd env new $EnvName --subscription $demoEnv['subscription_id'] --location $Location | Out-Null
    }

    azd env select $EnvName | Out-Null
    azd env set AZURE_SUBSCRIPTION_ID $demoEnv['subscription_id'] | Out-Null
    azd env set AZURE_LOCATION $Location | Out-Null

    if ($SkipDeploy) {
        Write-Host 'Provisioning infrastructure (azd provision) ...' -ForegroundColor Cyan
        azd provision --no-prompt
    }
    else {
        Write-Host 'Provisioning + deploying (azd up) ...' -ForegroundColor Cyan
        azd up --no-prompt
    }
    if ($LASTEXITCODE -ne 0) { throw "azd failed with exit code $LASTEXITCODE" }

    # Capture outputs and write a local dev settings file.
    $values = azd env get-values --output json | ConvertFrom-Json
    $settings = [ordered]@{
        CosmosDb       = [ordered]@{
            AccountEndpoint = $values.COSMOSDB__ACCOUNTENDPOINT
            AuthKey         = ''
            DatabaseName    = $values.COSMOSDB__DATABASENAME
            ContainerName   = $values.COSMOSDB__CONTAINERNAME
        }
        AzureAIFoundry = [ordered]@{
            ProjectEndpoint     = $values.AZUREAIFOUNDRY__PROJECTENDPOINT
            ModelDeploymentName = $values.AZUREAIFOUNDRY__MODELDEPLOYMENTNAME
            AgentId             = ''
        }
    }
    $settingsPath = Join-Path $demoRoot 'src\ConfHub.Api\appsettings.Development.json'
    $settings | ConvertTo-Json -Depth 5 | Set-Content -Path $settingsPath -Encoding utf8

    Write-Host ''
    Write-Host 'Provisioning complete.' -ForegroundColor Green
    Write-Host "Local dev settings written to: $settingsPath (git-ignored)" -ForegroundColor Green
    if ($values.PSObject.Properties.Name -contains 'AZURE_CONTAINER_APP_ENDPOINT') {
        Write-Host "Deployed API: $($values.AZURE_CONTAINER_APP_ENDPOINT)" -ForegroundColor Green
    }
    Write-Host ''
    Write-Host 'Next: seed sample data with  ./scripts/Seed-Cosmos.ps1' -ForegroundColor Cyan
}
finally {
    Pop-Location
}
