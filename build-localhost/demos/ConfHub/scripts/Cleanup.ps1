<#
.SYNOPSIS
    Tears down ALL Azure resources created for the ConfHub demo.
.DESCRIPTION
    Runs `azd down --force --purge` to delete the resource group and purge
    soft-deleted resources (Azure AI Foundry / Cognitive Services accounts are
    soft-deleted by default, so --purge frees the name for the next run). This makes
    the demo fully repeatable.

    Optionally also deletes the demo service principal created by
    Setup-ServicePrincipal.ps1 and removes local secrets/dev settings.
.PARAMETER EnvName
    azd environment name. Defaults to "confhub".
.PARAMETER DeleteServicePrincipal
    Also delete the sp-confhub-demo service principal and its credential file.
#>
[CmdletBinding()]
param(
    [string] $EnvName = 'confhub',
    [switch] $DeleteServicePrincipal
)

. "$PSScriptRoot/_Common.ps1"

$demoEnv = Get-DemoEnv
Assert-AzureContext -DemoEnv $demoEnv
Assert-Command -Name azd -InstallHint 'Install from https://aka.ms/azd'

$demoRoot = Get-DemoRoot
Push-Location $demoRoot
try {
    $existing = azd env list --output json 2>$null | ConvertFrom-Json
    if ($existing | Where-Object { $_.Name -eq $EnvName }) {
        azd env select $EnvName | Out-Null
        Write-Host "Deleting all resources for azd environment '$EnvName' ..." -ForegroundColor Yellow
        azd down --force --purge
        if ($LASTEXITCODE -ne 0) { throw "azd down failed with exit code $LASTEXITCODE" }
    }
    else {
        Write-Host "No azd environment named '$EnvName' found — nothing to tear down." -ForegroundColor DarkGray
    }
}
finally {
    Pop-Location
}

# Remove local generated dev settings (regenerated on next provision).
$settingsPath = Join-Path $demoRoot 'src\ConfHub.Api\appsettings.Development.json'
if (Test-Path $settingsPath) {
    Remove-Item $settingsPath -Force
    Write-Host "Removed $settingsPath" -ForegroundColor DarkGray
}

if ($DeleteServicePrincipal) {
    $credPath = Join-Path (Get-RepoRoot) '.azure\confhub.sp.json'
    if (Test-Path $credPath) {
        $cred = Get-Content $credPath -Raw | ConvertFrom-Json
        Write-Host "Deleting service principal '$($cred.displayName)' ..." -ForegroundColor Yellow
        az ad sp delete --id $cred.appId --only-show-errors
        Remove-Item $credPath -Force
        Write-Host 'Service principal and credential file removed.' -ForegroundColor DarkGray
    }
    else {
        Write-Host 'No service principal credential file found.' -ForegroundColor DarkGray
    }
}

Write-Host ''
Write-Host 'Cleanup complete.' -ForegroundColor Green
