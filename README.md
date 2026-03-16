# FridayPOS Cloud 66 POC

This repository is a small Dockerized ASP.NET Core service designed for learning Cloud 66 with a shape similar to FridayPOS.

It includes:

- a web API service
- a SignalR hub at `/hubs/notifications`
- health endpoint at `/healthz`
- environment-driven configuration for database, Redis, RabbitMQ, and storage
- a production-style multi-stage Docker build

## Why this repo exists

The real FridayPOS stack is larger and riskier to onboard first. This sample lets you explore how Cloud 66 handles:

- source repository onboarding
- framework detection for ASP.NET Core
- Docker builds
- environment variables and secrets
- container deploys on Azure-hosted infrastructure

## Local run with Docker

```bash
docker build -t fridaypos-cloud66-poc .
docker run --rm -p 8080:8080 \
  -e FridayPos__TenantName="FridayPOS UAE" \
  -e FridayPos__Region="uae-north" \
  -e FridayPos__EnableRealtime="true" \
  -e ConnectionStrings__MainDb="Server=db;Database=fridaypos;" \
  -e Redis__ConnectionString="redis:6379" \
  -e RabbitMq__Host="rabbitmq" \
  -e Storage__BlobContainer="fridaypos-assets" \
  fridaypos-cloud66-poc
```

Then test:

```bash
curl http://localhost:8080/healthz
curl http://localhost:8080/api/config
```

## Push to GitHub

```bash
git init
git add .
git commit -m "Add FridayPOS Cloud 66 POC"
git branch -M main
git remote add origin <your-github-repo-url>
git push -u origin main
```

## Use in Cloud 66

1. Click `New Application`.
2. Choose GitHub or enter the repo URL.
3. Choose `ASP.NET Core` or `.NET` in the framework dropdown if Cloud 66 asks.
4. Let Cloud 66 detect the `Dockerfile`.
5. Set environment variables in Cloud 66 using the names in this README.
6. Deploy to a small test stack first.

## Suggested Cloud 66 variables

- `FridayPos__TenantName`
- `FridayPos__Region`
- `FridayPos__EnableRealtime`
- `ConnectionStrings__MainDb`
- `Redis__ConnectionString`
- `RabbitMq__Host`
- `Storage__BlobContainer`
