# Script PowerShell para aplicar migrations

Write-Host "Updating database..." -ForegroundColor Green

Set-Location -Path "AGE.SignatureHub.Infrastructure"

dotnet ef database update `
    --startup-project ../AGE.SignatureHub.API/AGE.SignatureHub.API.csproj `
    --context ApplicationDbContext

if ($LASTEXITCODE -eq 0) {
    Write-Host "Database updated successfully!" -ForegroundColor Green
} else {
    Write-Host "Failed to update database!" -ForegroundColor Red
}

Set-Location -Path ..