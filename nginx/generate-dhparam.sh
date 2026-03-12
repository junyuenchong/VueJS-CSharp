#!/bin/bash
# Generate Diffie-Hellman parameters for enhanced SSL security
# This script generates strong DH parameters for perfect forward secrecy

set -e

echo "Generating Diffie-Hellman parameters..."
echo "This will take 5-10 minutes for 4096-bit parameters..."
echo ""

# Create ssl directory if it doesn't exist
mkdir -p ssl

# Generate 4096-bit DH parameters (recommended for production)
# For faster testing, use 2048: openssl dhparam -out ssl/dhparam.pem 2048
openssl dhparam -out ssl/dhparam.pem 4096

echo ""
echo "✅ Diffie-Hellman parameters generated successfully!"
echo "📁 Location: ssl/dhparam.pem"
echo ""
echo "Next steps:"
echo "1. Uncomment 'ssl_dhparam /etc/nginx/ssl/dhparam.pem;' in nginx configs"
echo "2. Rebuild nginx containers: docker compose -p crudapp up -d --build api-nginx client"
echo ""

