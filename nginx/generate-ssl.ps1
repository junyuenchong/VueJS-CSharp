# PowerShell script to generate SSL certificates for development
# For production, use Let's Encrypt or proper certificates

if (-not (Test-Path "ssl")) {
    New-Item -ItemType Directory -Path "ssl"
}

# Check if OpenSSL is available
$opensslPath = Get-Command openssl -ErrorAction SilentlyContinue
if (-not $opensslPath) {
    Write-Host "OpenSSL is not installed. Please install OpenSSL or use Git Bash to run generate-ssl.sh" -ForegroundColor Red
    Write-Host "Alternatively, you can install OpenSSL via Chocolatey: choco install openssl" -ForegroundColor Yellow
    exit 1
}

# Generate private key
openssl genrsa -out ssl/key.pem 2048

# Generate certificate signing request
openssl req -new -key ssl/key.pem -out ssl/cert.csr -subj "/C=US/ST=State/L=City/O=Organization/CN=localhost"

# Generate self-signed certificate (valid for 365 days)
openssl x509 -req -days 365 -in ssl/cert.csr -signkey ssl/key.pem -out ssl/cert.pem

# Clean up CSR
Remove-Item ssl/cert.csr

Write-Host "SSL certificates generated in ssl/ directory" -ForegroundColor Green
Write-Host "Note: These are self-signed certificates for development only" -ForegroundColor Yellow

