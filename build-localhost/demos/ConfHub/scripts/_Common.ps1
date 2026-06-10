<#
.SYNOPSIS
    Shared helpers for the ConfHub demo scripts.
.DESCRIPTION
    Loads the Azure environment (subscription / tenant) from the repo-root
    .azure/.env file so every script targets the SAME subscription and tenant.
    Dot-source this file from the other scripts:  . "$PSScriptRoot/_Common.ps1"
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-RepoRoot {
    # scripts/ lives at build-localhost/demos/ConfHub/scripts → repo root is 4 levels up
    return (Resolve-Path (Join-Path $PSScriptRoot '..\..\..\..')).Path
}

function Get-DemoRoot {
    return (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
}

<#
    Reads .azure/.env (key=value lines) and returns a hashtable.
    Also tolerates bare lines (e.g. a placeholder marker) by ignoring them.
#>
function Get-DemoEnv {
    $envPath = Join-Path (Get-RepoRoot) '.azure\.env'
    if (-not (Test-Path $envPath)) {
        throw "Environment file not found at '$envPath'. Create it with 'subscription_id' and 'tenant_id' entries."
    }

    $map = @{}
    foreach ($line in Get-Content $envPath) {
        $trimmed = $line.Trim()
        if ([string]::IsNullOrWhiteSpace($trimmed) -or -not $trimmed.Contains('=')) {
            continue
        }
        $parts = $trimmed.Split('=', 2)
        $map[$parts[0].Trim()] = $parts[1].Trim().Trim('"')
    }

    if (-not $map.ContainsKey('subscription_id') -or [string]::IsNullOrWhiteSpace($map['subscription_id'])) {
        throw "subscription_id is missing from .azure/.env"
    }
    if (-not $map.ContainsKey('tenant_id') -or [string]::IsNullOrWhiteSpace($map['tenant_id'])) {
        throw "tenant_id is missing from .azure/.env"
    }
    return $map
}

function Assert-Command {
    param([Parameter(Mandatory)][string] $Name, [string] $InstallHint)
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' was not found on PATH. $InstallHint"
    }
}

<#
    Confirms the Azure CLI is logged in to the tenant + subscription from .env.
    This script never logs in for you — if the context is wrong it tells you the
    exact command to run.
#>
function Assert-AzureContext {
    param([Parameter(Mandatory)][hashtable] $DemoEnv)

    Assert-Command -Name az -InstallHint 'Install from https://aka.ms/az-cli'

    $account = az account show 2>$null | ConvertFrom-Json
    $wantSub = $DemoEnv['subscription_id']
    $wantTenant = $DemoEnv['tenant_id']

    if (-not $account -or $account.tenantId -ne $wantTenant -or $account.id -ne $wantSub) {
        Write-Host ''
        Write-Host 'Azure CLI is not signed in to the expected tenant/subscription.' -ForegroundColor Yellow
        Write-Host 'Run these commands yourself (interactive sign-in required):' -ForegroundColor Yellow
        Write-Host "  az login --tenant $wantTenant" -ForegroundColor Cyan
        Write-Host "  az account set --subscription $wantSub" -ForegroundColor Cyan
        throw 'Wrong Azure context. Sign in and re-run this script.'
    }

    $identityType = $account.user.type
    Write-Host "Azure context OK — subscription '$($account.name)' ($wantSub), identity '$($account.user.name)' [$identityType]" -ForegroundColor Green
    return $account
}

<#
    Verifies the active identity can actually read the target subscription. Catches
    the common case where the CLI default subscription was switched but the identity
    has no RBAC on it (e.g. a cached service principal with no role assignments).
#>
function Assert-SubscriptionAccess {
    param([Parameter(Mandatory)][hashtable] $DemoEnv)

    $sub = $DemoEnv['subscription_id']
    az group list --subscription $sub --query "[0].name" --output tsv 2>$null | Out-Null
    if ($LASTEXITCODE -ne 0) {
        $account = az account show 2>$null | ConvertFrom-Json
        Write-Host ''
        Write-Host "The active identity '$($account.user.name)' [$($account.user.type)] cannot read subscription $sub." -ForegroundColor Yellow
        Write-Host 'It has no RBAC permissions there, so it cannot provision resources or assign roles.' -ForegroundColor Yellow
        Write-Host 'Sign in interactively as a user with Owner (or Contributor + User Access Administrator):' -ForegroundColor Yellow
        Write-Host "  az login --tenant $($DemoEnv['tenant_id'])" -ForegroundColor Cyan
        Write-Host "  az account set --subscription $sub" -ForegroundColor Cyan
        throw 'Active identity lacks access to the target subscription.'
    }
}

