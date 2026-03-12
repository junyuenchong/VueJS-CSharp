# Nginx Configuration - Complete Guide

Complete guide for nginx configuration in this project, including architecture, best practices, security, and troubleshooting.

---

## 📋 Table of Contents

1. [Quick Start](#quick-start)
2. [Architecture Overview](#architecture-overview)
3. [Configuration Files](#configuration-files)
4. [SSL/TLS Best Practices](#ssltls-best-practices)
5. [Security Headers](#security-headers)
6. [Why HTTPS is Essential](#why-https-is-essential)
7. [Performance Optimizations](#performance-optimizations)
8. [Rate Limiting & DDoS Protection](#rate-limiting--ddos-protection)
9. [Production Checklist](#production-checklist)
10. [Testing](#testing)
11. [Troubleshooting](#troubleshooting)

---

## Quick Start

This project includes **production-ready nginx configurations** with industry best practices.

### ✅ What's Included

**Security**

- ✅ Modern TLS 1.2 & 1.3 only
- ✅ Strong cipher suites with perfect forward secrecy
- ✅ OWASP security headers (HSTS, CSP, X-Frame-Options, etc.)
- ✅ Rate limiting & DDoS protection
- ✅ Connection limits per IP
- ✅ Server version hidden

**Performance**

- ✅ HTTP/2 enabled
- ✅ Gzip compression
- ✅ Static file caching
- ✅ SSL session caching
- ✅ Optimized buffer sizes

**Monitoring**

- ✅ Detailed access logs
- ✅ Error logging
- ✅ Health check endpoints

### 🚀 Start Services

```bash
docker compose -p crudapp up -d --build
```

### Test Configuration

```bash
# Check if services are running
docker compose -p crudapp ps

# Test API
curl -k https://localhost:8443/health

# Test Client
curl -k https://localhost:5443/health

# Verify Security Headers
curl -k -I https://localhost:8443 | grep -i "strict-transport\|x-frame\|csp"
```

---

## Architecture Overview

This project uses **two nginx instances**:

1. **Frontend Nginx** (`client` service): Serves Vue.js static files with HTTPS
2. **Backend Nginx** (`api-nginx` service): Reverse proxy that adds HTTPS to the .NET API

### Architecture Diagram

```
┌─────────────────┐
│   Browser       │
│                 │
└────────┬────────┘
         │
         ├─── HTTPS:5443 ───► ┌──────────────────┐
         │                    │  client (nginx)  │  ──► Serves Vue.js static files
         │                    │  Port: 5443      │
         │                    └──────────────────┘
         │
         └─── HTTPS:8443 ───► ┌──────────────────┐      ┌──────────┐
                              │  api-nginx       │ ───► │   api    │
                              │  (reverse proxy) │      │  .NET    │
                              │  Port: 8443      │      │  Port:8080│
                              └──────────────────┘      └──────────┘
```

### Two Nginx Instances

#### 1. **Frontend Nginx** (`client` service)

- **Purpose**: Serves Vue.js static files (HTML, CSS, JS)
- **Ports**:
  - `5443` (HTTPS)
  - `5173` (HTTP - redirects to HTTPS)
- **Configuration**: `nginx/client.conf`
- **Dockerfile**: `frontend/Dockerfile`

#### 2. **Backend Nginx** (`api-nginx` service)

- **Purpose**: Reverse proxy for .NET API (adds HTTPS)
- **Ports**:
  - `8443` (HTTPS)
  - `8080` (HTTP - redirects to HTTPS)
- **Configuration**: `nginx/api.conf`
- **Dockerfile**: `nginx/Dockerfile.api`

### How It Works

#### Frontend Nginx (Client)

1. **Build Stage**: Vue.js app is built into static files (`dist/` folder)
2. **Serve Stage**: Nginx serves these static files with HTTPS

#### Backend Nginx (API Reverse Proxy)

1. **Client Request**: Browser → `https://localhost:8443/api/Products`
2. **Nginx Receives**: HTTPS request on port 8443
3. **Nginx Forwards**: HTTP request to `http://api:8080/api/Products`
4. **API Responds**: .NET API processes request
5. **Nginx Returns**: Response back to client over HTTPS

### Why Use Nginx?

#### For Frontend (Client)

1. **Production-ready**: Serves static files efficiently
2. **SPA Routing**: Handles Vue Router properly
3. **HTTPS**: Easy SSL/TLS configuration
4. **Caching**: Optimizes asset delivery
5. **Compression**: Reduces bandwidth

#### For Backend (API)

1. **HTTPS Termination**: Adds HTTPS without modifying .NET API
2. **Security**: Centralized security headers
3. **Load Balancing**: Can easily add multiple API instances
4. **SSL Offloading**: API doesn't need SSL configuration
5. **Flexibility**: Easy to add rate limiting, logging, etc.

---

## Configuration Files

### Port Mapping

| Service     | Container Port | Host Port | Protocol | Purpose            |
| ----------- | -------------- | --------- | -------- | ------------------ |
| `client`    | 443            | 5443      | HTTPS    | Vue.js frontend    |
| `client`    | 80             | 5173      | HTTP     | Redirects to HTTPS |
| `api-nginx` | 443            | 8443      | HTTPS    | API reverse proxy  |
| `api-nginx` | 80             | 8080      | HTTP     | Redirects to HTTPS |
| `api`       | 8080           | -         | HTTP     | Internal only      |

### Access URLs

- **Frontend**: `https://localhost:5443` or `http://localhost:5173` (redirects)
- **API**: `https://localhost:8443/api/Products` or `http://localhost:8080/api/Products` (redirects)
- **Swagger**: `https://localhost:8443/swagger`

### SSL Certificates

Both nginx instances use **self-signed SSL certificates** for HTTPS:

- **Location**: `ssl/` directory
- **Files**:
  - `cert.pem` - Certificate
  - `key.pem` - Private key
- **Generation**:
  - Windows: `.\nginx\generate-ssl.ps1`
  - Linux/Mac: `./nginx/generate-ssl.sh`

**Note**: Self-signed certificates will show a browser warning. Click "Advanced" → "Proceed to localhost" for development.

---

## SSL/TLS Best Practices

### ✅ Implemented Best Practices

#### **TLS Protocol Versions**

```nginx
ssl_protocols TLSv1.2 TLSv1.3;
```

- **Why**: Only allows modern, secure TLS versions
- **Impact**: Protects against known vulnerabilities in older protocols (SSLv3, TLSv1.0, TLSv1.1)
- **Compatibility**: Works with all modern browsers (2018+)

#### **Cipher Suite Selection**

```nginx
ssl_ciphers 'ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:...';
ssl_prefer_server_ciphers off;
```

- **Why**: Uses strong, modern cipher suites with perfect forward secrecy (PFS)
- **Impact**: Even if private key is compromised, past sessions remain secure
- **Priority**: TLS 1.3 ciphers first, then strong TLS 1.2 ciphers

#### **SSL Session Caching**

```nginx
ssl_session_cache shared:SSL:10m;
ssl_session_timeout 1d;
ssl_session_tickets off;
```

- **Why**: Improves performance by reusing SSL sessions
- **Impact**: Reduces SSL handshake overhead for returning visitors
- **Security**: Session tickets disabled to prevent session hijacking

#### **OCSP Stapling** (Requires CA-signed certificate)

```nginx
# ssl_stapling on;
# ssl_stapling_verify on;
# ssl_trusted_certificate /etc/nginx/ssl/chain.pem;
```

- **Why**: Validates certificate revocation status without client queries
- **Impact**: Faster page loads and improved privacy
- **Note**: Uncomment when using CA-signed certificates (Let's Encrypt, etc.)

#### **Diffie-Hellman Parameters** (Enhanced PFS)

```nginx
# ssl_dhparam /etc/nginx/ssl/dhparam.pem;
```

- **Why**: Stronger perfect forward secrecy with custom DH parameters
- **Impact**: Enhanced security for TLS 1.2 connections
- **Generation**:

  ```bash
  # Linux/Mac
  ./nginx/generate-dhparam.sh

  # Windows
  .\nginx\generate-dhparam.ps1
  ```

---

## Security Headers

All headers follow **OWASP best practices**:

### **HSTS (HTTP Strict Transport Security)**

```nginx
add_header Strict-Transport-Security "max-age=31536000; includeSubDomains; preload" always;
```

- **Purpose**: Forces browsers to use HTTPS only
- **Duration**: 1 year (31536000 seconds)
- **Preload**: Allows inclusion in browser HSTS preload lists

### **X-Frame-Options**

```nginx
add_header X-Frame-Options "SAMEORIGIN" always;
```

- **Purpose**: Prevents clickjacking attacks
- **Value**: Allows framing only from same origin

### **X-Content-Type-Options**

```nginx
add_header X-Content-Type-Options "nosniff" always;
```

- **Purpose**: Prevents MIME type sniffing
- **Impact**: Reduces risk of XSS attacks via content type confusion

### **Content-Security-Policy (CSP)**

```nginx
add_header Content-Security-Policy "default-src 'self'; ..." always;
```

- **Purpose**: Restricts which resources can be loaded
- **Impact**: Mitigates XSS attacks by controlling resource loading
- **Note**: Adjust based on your application's needs

### **Referrer-Policy**

```nginx
add_header Referrer-Policy "strict-origin-when-cross-origin" always;
```

- **Purpose**: Controls referrer information sent with requests
- **Privacy**: Limits information leakage to third-party sites

### **Permissions-Policy** (formerly Feature-Policy)

```nginx
add_header Permissions-Policy "geolocation=(), microphone=(), ..." always;
```

- **Purpose**: Disables unnecessary browser features
- **Security**: Prevents abuse of browser APIs

### **Server Tokens**

```nginx
server_tokens off;
```

- **Purpose**: Hides nginx version from responses
- **Security**: Reduces information available to attackers

---

## Why HTTPS is Essential

### 🚨 **Short Answer: YES, Backend APIs MUST Use HTTPS**

**For production business applications, HTTPS is not optional—it's mandatory.**

### 📋 **Why HTTPS for Backend APIs?**

#### 1. **🔒 Data Protection**

**Sensitive Data Transmission**

- **User credentials** (passwords, tokens, API keys)
- **Personal Information** (PII - names, emails, addresses)
- **Financial data** (payment info, transactions)
- **Business data** (customer records, internal documents)
- **Medical records** (HIPAA compliance)

**Without HTTPS**: Data is sent in **plain text** over the network—anyone can intercept and read it.

**With HTTPS**: Data is **encrypted** end-to-end.

#### 2. **🛡️ Man-in-the-Middle (MITM) Attacks**

Without HTTPS, attackers can:

- **Intercept requests** between client and server
- **Modify data** before it reaches the server
- **Inject malicious code** or responses
- **Steal authentication tokens**

**HTTPS prevents this** by encrypting all communication.

#### 3. **⚖️ Legal & Compliance Requirements**

| Regulation                | Requirement                      | Penalty for Non-Compliance      |
| ------------------------- | -------------------------------- | ------------------------------- |
| **GDPR** (EU)             | "Appropriate technical measures" | Up to €20M or 4% of revenue     |
| **HIPAA** (US Healthcare) | Encryption in transit required   | Up to $1.5M per violation       |
| **PCI DSS**               | HTTPS mandatory for payment data | Fines + loss of card processing |
| **CCPA** (California)     | "Reasonable security practices"  | $2,500-$7,500 per violation     |
| **SOC 2**                 | Encryption requirements          | Audit failure                   |

#### 4. **🔐 Modern Browser Requirements**

- **Modern browsers** block HTTP APIs from HTTPS pages (Mixed Content)
- **PWA/Service Workers** require HTTPS
- **Web APIs** (geolocation, camera) require HTTPS
- **Cookie security** (Secure flag) requires HTTPS

**Your frontend uses HTTPS → Your backend MUST use HTTPS**

#### 5. **🏢 Business Trust & Reputation**

- **Security badges** (SSL indicators) build confidence
- **Data breaches** destroy reputation
- Shows **professionalism** and **security awareness**
- Required by **enterprise clients**

### ❓ **Common Misconceptions**

#### ❌ **"My API is Internal Only - HTTPS Not Needed"**

**Reality:**

- **Internal networks** can be compromised
- **Insider threats** exist
- **Network segmentation** can fail
- **Zero Trust** model requires HTTPS everywhere

#### ❌ **"HTTPS is Slow"**

**Reality:**

- Modern TLS 1.3 is **faster than HTTP**
- HTTP/2 over HTTPS is **much faster** than HTTP/1.1
- SSL session caching reduces overhead
- Performance impact: **<1%** in most cases

#### ❌ **"Self-Signed Certificates are Fine"**

**Reality:**

- OK for **development/testing only**
- **Production** requires CA-signed certificates
- **Let's Encrypt** provides free certificates
- Self-signed causes **browser warnings** and **API client errors**

---

## Performance Optimizations

### **HTTP/2 Support**

```nginx
listen 443 ssl http2;
```

- **Why**: Multiplexing, header compression, server push
- **Impact**: Faster page loads, especially for multiple resources

### **Gzip Compression**

```nginx
gzip on;
gzip_comp_level 6;
gzip_types text/plain text/css application/json ...;
```

- **Why**: Reduces bandwidth usage
- **Impact**: 60-80% size reduction for text-based files
- **Level**: 6 provides good balance between compression and CPU usage

### **Static File Caching**

```nginx
location ~* \.(js|css|png|jpg)$ {
    expires 1y;
    add_header Cache-Control "public, immutable";
}
```

- **Why**: Reduces server load and improves page load times
- **Strategy**: Long cache for hashed assets (1 year), short cache for HTML (1 hour)

### **Sendfile & TCP Optimizations**

```nginx
sendfile on;
tcp_nopush on;
tcp_nodelay on;
```

- **Why**: Optimizes file transfer performance
- **Impact**: Reduces CPU usage and network overhead

### **Buffer Optimizations**

```nginx
client_body_buffer_size 128k;
proxy_buffer_size 4k;
proxy_buffers 8 4k;
```

- **Why**: Reduces memory usage while maintaining performance
- **Tuning**: Adjusted based on typical request/response sizes

---

## Rate Limiting & DDoS Protection

### **Request Rate Limiting**

```nginx
limit_req_zone $binary_remote_addr zone=api_limit:10m rate=100r/m;
limit_req zone=api_limit burst=20 nodelay;
```

- **Purpose**: Prevents DDoS attacks and brute force attempts
- **Implementation**:
  - **API**: 100 requests per minute per IP, burst of 20
  - **Client**: 200 requests per minute per IP, burst of 50
- **Impact**: Protects backend from being overwhelmed

### **Connection Limiting**

```nginx
limit_conn_zone $binary_remote_addr zone=conn_limit_per_ip:10m;
limit_conn conn_limit_per_ip 20;
```

- **Purpose**: Limits concurrent connections per IP
- **Impact**: Prevents connection exhaustion attacks

---

## Production Checklist

### ✅ **Development (Current Setup)**

- ✅ HTTPS configured with nginx
- ✅ Self-signed certificates (development only)
- ✅ HTTP → HTTPS redirects
- ✅ Security headers configured
- ✅ TLS 1.2 & 1.3 enabled

### 🔒 **Production (Required)**

- [ ] **CA-signed certificates** (Let's Encrypt or commercial CA)
- [ ] **OCSP stapling** enabled
- [ ] **Diffie-Hellman parameters** generated (4096-bit)
- [ ] **Certificate auto-renewal** configured
- [ ] **Monitoring** for certificate expiration
- [ ] **Backup** of private keys
- [ ] **Security testing** (SSL Labs A+ rating)
- [ ] **Adjust rate limits** based on your traffic
- [ ] **Customize CSP** for your app
- [ ] **Set up monitoring** and alerts

### 🔧 **How to Upgrade to Production HTTPS**

#### **Option 1: Let's Encrypt (Free, Recommended)**

```bash
# Using certbot with nginx
certbot --nginx -d api.yourdomain.com

# Auto-renewal
certbot renew --dry-run
```

#### **Option 2: Commercial CA (For Enterprise)**

Purchase from:

- DigiCert
- Sectigo
- GlobalSign
- Thawte

#### **Option 3: Cloud Provider Managed Certificates**

- **AWS**: ACM (Application Load Balancer)
- **Azure**: App Service Certificates
- **GCP**: Cloud Load Balancing
- **Cloudflare**: Free SSL/TLS

---

## Testing

### **Test Frontend**

```bash
curl -k https://localhost:5443
# Should return Vue.js HTML

curl http://localhost:5173
# Should redirect to HTTPS (301)
```

### **Test Backend**

```bash
curl -k https://localhost:8443/api/Products
# Should return JSON products list

curl http://localhost:8080/api/Products
# Should redirect to HTTPS (301)
```

### **Health Checks**

```bash
curl -k https://localhost:5443/health
# Should return "healthy"

curl -k https://localhost:8443/health
# Should return "healthy"
```

### **Test SSL Configuration**

```bash
# Check SSL rating
curl -k -v https://localhost:8443 2>&1 | grep -i "SSL\|TLS"

# Test with openssl
openssl s_client -connect localhost:8443 -tls1_3
```

### **Test Security Headers**

```bash
curl -k -I https://localhost:8443 | grep -i "strict-transport\|x-frame\|csp"
```

### **Test Rate Limiting**

```bash
# Should get 503 after rate limit
for i in {1..150}; do curl -k https://localhost:8443/api/Products; done
```

### **Test Compression**

```bash
curl -k -H "Accept-Encoding: gzip" https://localhost:5443 -v 2>&1 | grep -i "content-encoding"
```

### **Check Logs**

```bash
# View access logs
docker compose -p crudapp exec api-nginx tail -f /var/log/nginx/api_access.log

# View error logs
docker compose -p crudapp exec api-nginx tail -f /var/log/nginx/api_error.log
```

---

## Troubleshooting

### **Issue: Browser shows SSL warning**

**Solution**: This is normal for self-signed certificates. Click "Advanced" → "Proceed to localhost"

### **Issue: CORS errors**

**Solution**: Check that API's CORS policy allows the frontend origin. See `backend/Program.cs` for CORS configuration.

### **Issue: 502 Bad Gateway**

**Solution**: API container might not be running. Check: `docker compose -p crudapp ps`

### **Issue: 404 on frontend routes**

**Solution**: Make sure `try_files $uri $uri/ /index.html;` is in nginx config for SPA routing.

### **Issue: API not accessible**

**Solution**:

1. Check if `api-nginx` is running: `docker compose -p crudapp ps`
2. Check nginx logs: `docker compose -p crudapp logs api-nginx`
3. Verify API is running: `docker compose -p crudapp logs api`

---

## Kubernetes Setup

For Kubernetes, a simpler setup is used:

- **Frontend**: `nginx/client-k8s.conf` (HTTP only, no SSL)
- **Backend**: Direct API access (no nginx proxy needed)
- **Reason**: Kubernetes ingress handles HTTPS/SSL termination

See `frontend/Dockerfile.k8s` for Kubernetes-specific configuration.

---

## Security Checklist

- [x] TLS 1.2 and 1.3 only
- [x] Strong cipher suites with PFS
- [x] SSL session caching configured
- [x] HSTS with preload
- [x] Security headers (OWASP compliant)
- [x] Rate limiting enabled
- [x] Connection limits set
- [x] Server version hidden
- [x] Static file caching optimized
- [x] Gzip compression enabled
- [x] HTTP/2 enabled
- [ ] CA-signed certificates (production)
- [ ] OCSP stapling enabled (with CA certs)
- [ ] Diffie-Hellman parameters generated
- [ ] IP whitelisting (if needed)
- [ ] CSP customized for your app
- [ ] Monitoring & alerting configured

---

## Key Features Summary

| Feature           | API Nginx        | Client Nginx      |
| ----------------- | ---------------- | ----------------- |
| TLS 1.2 & 1.3     | ✅               | ✅                |
| HTTP/2            | ✅               | ✅                |
| Rate Limiting     | ✅ (100 req/min) | ✅ (200 req/min)  |
| Connection Limits | ✅ (20/IP)       | ✅ (20/IP)        |
| Security Headers  | ✅ (OWASP)       | ✅ (OWASP)        |
| Compression       | N/A              | ✅ (Gzip)         |
| Caching           | N/A              | ✅ (Static files) |
| Health Checks     | ✅               | ✅                |

---

## Additional Resources

- **Mozilla SSL Configuration Generator**: https://ssl-config.mozilla.org/
- **SSL Labs SSL Test**: https://www.ssllabs.com/ssltest/
- **Security Headers Test**: https://securityheaders.com/
- **OWASP Secure Headers**: https://owasp.org/www-project-secure-headers/
- **Nginx Security Guide**: https://nginx.org/en/docs/http/configuring_https_servers.html
- **Let's Encrypt**: https://letsencrypt.org/
- **OWASP API Security**: https://owasp.org/www-project-api-security/

---

## Summary

This setup provides a **production-ready environment** with:

✅ **Security**: Modern TLS, strong ciphers, security headers, rate limiting
✅ **Performance**: HTTP/2, compression, caching, optimized buffers
✅ **Monitoring**: Detailed logging, health checks
✅ **Production-Ready**: Follows OWASP and Mozilla recommendations

**All configurations follow industry best practices from Mozilla, OWASP, and Nginx official recommendations.**
