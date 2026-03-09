# Script PowerShell para criar migrations

$migrationName = Read-Host "Enter migration name"

if ([string]::IsNullOrWhiteSpace($migrationName)) {
    Write-Host "Migration name is required!" -ForegroundColor Red
    exit
}

Write-Host "Creating migration: $migrationName" -ForegroundColor Green

Set-Location -Path "AGE.SignatureHub.Infrastructure"

dotnet ef migrations add $migrationName `
    --startup-project ../AGE.SignatureHub.API/AGE.SignatureHub.API.csproj `
    --context ApplicationDbContext `
    --output-dir Persistence/Migrations

if ($LASTEXITCODE -eq 0) {
    Write-Host "Migration created successfully!" -ForegroundColor Green
} else {
    Write-Host "Failed to create migration!" -ForegroundColor Red
}

Set-Location -Path ..