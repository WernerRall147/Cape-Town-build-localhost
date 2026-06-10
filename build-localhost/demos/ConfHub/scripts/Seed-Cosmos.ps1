<#
.SYNOPSIS
    Seeds the ConfHub catalogue with sample sessions by POSTing to the running API.
.DESCRIPTION
    Posts each session in scripts/seed-data.json to {BaseUrl}/sessions. This works
    against either a local instance (dotnet run → http://localhost:5000) or the
    deployed Container App. Using the API keeps Cosmos access keyless — the API's
    managed identity (or your signed-in identity locally) handles data-plane auth.
.PARAMETER BaseUrl
    Base URL of the ConfHub API. If omitted, the deployed Container App endpoint is
    read from the azd environment; falls back to http://localhost:5000.
#>
[CmdletBinding()]
param(
    [string] $BaseUrl
)

. "$PSScriptRoot/_Common.ps1"

if (-not $BaseUrl) {
    Push-Location (Get-DemoRoot)
    try {
        $values = azd env get-values --output json 2>$null | ConvertFrom-Json
        if ($values -and ($values.PSObject.Properties.Name -contains 'AZURE_CONTAINER_APP_ENDPOINT')) {
            $BaseUrl = $values.AZURE_CONTAINER_APP_ENDPOINT
        }
    }
    catch { }
    finally { Pop-Location }
}

if (-not $BaseUrl) {
    $BaseUrl = 'http://localhost:5000'
}
$BaseUrl = $BaseUrl.TrimEnd('/')

$dataPath = Join-Path $PSScriptRoot 'seed-data.json'
$sessions = Get-Content $dataPath -Raw | ConvertFrom-Json

Write-Host "Seeding $($sessions.Count) sessions to $BaseUrl/sessions ..." -ForegroundColor Cyan
$created = 0
foreach ($session in $sessions) {
    $body = $session | ConvertTo-Json -Depth 5
    try {
        Invoke-RestMethod -Method Post -Uri "$BaseUrl/sessions" -ContentType 'application/json' -Body $body | Out-Null
        $created++
        Write-Host "  + $($session.title)" -ForegroundColor DarkGray
    }
    catch {
        Write-Warning "Failed to create '$($session.title)': $($_.Exception.Message)"
    }
}

Write-Host ''
Write-Host "Seeded $created / $($sessions.Count) sessions." -ForegroundColor Green
