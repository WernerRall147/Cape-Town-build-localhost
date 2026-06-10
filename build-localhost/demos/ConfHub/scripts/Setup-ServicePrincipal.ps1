<#
.SYNOPSIS
    Creates a service principal for the ConfHub demo and grants it the roles
    needed to provision and deploy the application.
.DESCRIPTION
    The session .env file contains the marker "SP_needs_to_beCreated". This script
    creates a service principal scoped to the subscription from .azure/.env, grants
    it Contributor (to create resources) and Role Based Access Control Administrator
    (so the deployment can assign data-plane roles), and writes the credentials to a
    git-ignored file. You must already be signed in to the target tenant.
.NOTES
    Run interactively first:
      az login --tenant <tenant_id>
      az account set --subscription <subscription_id>
#>
[CmdletBinding()]
param(
    [string] $DisplayName = 'sp-confhub-demo'
)

. "$PSScriptRoot/_Common.ps1"

$demoEnv = Get-DemoEnv
$account = Assert-AzureContext -DemoEnv $demoEnv
Assert-SubscriptionAccess -DemoEnv $demoEnv

if ($account.user.type -eq 'servicePrincipal') {
    Write-Host ''
    Write-Host 'You are signed in as a service principal. Creating a new app registration' -ForegroundColor Yellow
    Write-Host 'usually requires a user account with directory privileges (or a service' -ForegroundColor Yellow
    Write-Host 'principal granted Microsoft Graph Application.ReadWrite). If creation fails,' -ForegroundColor Yellow
    Write-Host 'sign in interactively as a user first:' -ForegroundColor Yellow
    Write-Host "  az login --tenant $($demoEnv['tenant_id'])" -ForegroundColor Cyan
    Write-Host ''
}

$subscriptionId = $demoEnv['subscription_id']
$scope = "/subscriptions/$subscriptionId"

Write-Host "Creating service principal '$DisplayName' with Contributor on $scope ..." -ForegroundColor Cyan
$sp = az ad sp create-for-rbac `
    --name $DisplayName `
    --role 'Contributor' `
    --scopes $scope `
    --only-show-errors | ConvertFrom-Json

if (-not $sp) {
    throw 'Service principal creation failed.'
}

# Grant the ability to assign roles (the IaC creates RBAC role assignments).
Write-Host 'Granting Role Based Access Control Administrator ...' -ForegroundColor Cyan
az role assignment create `
    --assignee $sp.appId `
    --role 'Role Based Access Control Administrator' `
    --scope $scope `
    --only-show-errors | Out-Null

# Persist credentials to a git-ignored file (never commit these).
$credPath = Join-Path (Get-RepoRoot) '.azure\confhub.sp.json'
$credential = [ordered]@{
    appId          = $sp.appId
    password       = $sp.password
    tenant         = $sp.tenant
    subscriptionId = $subscriptionId
    displayName    = $DisplayName
}
$credential | ConvertTo-Json | Set-Content -Path $credPath -Encoding utf8

Write-Host ''
Write-Host "Service principal created. Credentials written to: $credPath" -ForegroundColor Green
Write-Host 'This file is git-ignored. Keep it secret.' -ForegroundColor Yellow
Write-Host ''
Write-Host 'To sign in as the service principal (e.g. in CI):' -ForegroundColor Cyan
Write-Host "  az login --service-principal -u $($sp.appId) -p <password> --tenant $($sp.tenant)" -ForegroundColor Cyan
