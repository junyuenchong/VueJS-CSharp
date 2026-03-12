# Generate Diffie-Hellman parameters for enhanced SSL security
# This script generates strong DH parameters for perfect forward secrecy

Write-Host "Generating Diffie-Hellman parameters..." -ForegroundColor Green
Write-Host "This will take 5-10 minutes for 4096-bit parameters..." -ForegroundColor Yellow
Write-Host ""

# Create ssl directory if it doesn't exist
if (-not (Test-Path "ssl")) {
    New-Item -ItemType Directory -Path "ssl" | Out-Null
}

# Generate 4096-bit DH parameters (recommended for production)
# For faster testing, use 2048: openssl dhparam -out ssl/dhparam.pem 2048
Write-Host "Running openssl dhparam (this takes a while)..." -ForegroundColor Cyan
openssl dhparam -out ssl/dhparam.pem 4096

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✅ Diffie-Hellman parameters generated successfully!" -ForegroundColor Green
    Write-Host "📁 Location: ssl/dhparam.pem" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Uncomment 'ssl_dhparam /etc/nginx/ssl/dhparam.pem;' in nginx configs"
    Write-Host "2. Rebuild nginx containers: docker compose -p crudapp up -d --build api-nginx client"
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "❌ Error generating DH parameters. Make sure OpenSSL is installed." -ForegroundColor Red
    Write-Host ""
}

