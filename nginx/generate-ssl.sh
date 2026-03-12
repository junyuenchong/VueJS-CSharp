#!/bin/bash

# Generate SSL certificates for development
# For production, use Let's Encrypt or proper certificates

mkdir -p ssl

# Generate private key
openssl genrsa -out ssl/key.pem 2048

# Generate certificate signing request
openssl req -new -key ssl/key.pem -out ssl/cert.csr -subj "/C=US/ST=State/L=City/O=Organization/CN=localhost"

# Generate self-signed certificate (valid for 365 days)
openssl x509 -req -days 365 -in ssl/cert.csr -signkey ssl/key.pem -out ssl/cert.pem

# Clean up CSR
rm ssl/cert.csr

echo "SSL certificates generated in ssl/ directory"
echo "Note: These are self-signed certificates for development only"

