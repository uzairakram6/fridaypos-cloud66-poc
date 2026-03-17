# FridayPOS Cloud 66 POC

This repository is a small Dockerized ASP.NET Core service designed for learning Cloud 66 with a shape similar to FridayPOS.

It includes:

- a web API service
- a SignalR hub at `/hubs/notifications`
- health endpoint at `/healthz`
- database connectivity endpoint at `/healthz/db`
- environment-driven configuration for database, Redis, RabbitMQ, and storage
- a production-style multi-stage Docker build
- starter Cloud 66 `service.yml` and `manifest.yml`

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
  -e ConnectionStrings__MainDb="Host=db;Port=5432;Database=fridaypos;Username=postgres;Password=postgres" \
  -e Redis__ConnectionString="redis:6379" \
  -e RabbitMq__Host="rabbitmq" \
  -e Storage__BlobContainer="fridaypos-assets" \
  fridaypos-cloud66-poc
```

Then test:

```bash
curl http://localhost:8080/healthz
curl http://localhost:8080/healthz/db
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

## Cloud 66 config in repo

This repository includes:

- `service.yml` to describe the service build and port mapping
- `manifest.yml` as a minimal starting point for initial environment values
- `manifest.azure.example.yml` as a terminal-friendly Azure server definition example

Before using `service.yml`, update:

- `git_url` to your real GitHub SSH repo URL

Before using `manifest.azure.example.yml`, update:

- `key_name` to the name of your Azure cloud provider connection inside Cloud 66
- `region` to your actual Azure region
- `size` to the Azure VM size you want Cloud 66 to provision

This repository also includes `manifest.aws.example.yml` for the AWS test path. Before using it, update:

- `key_name` to the name of your AWS cloud provider connection inside Cloud 66
- `region` to the AWS region where the app server will run
- `size` to the EC2 size you want Cloud 66 to provision
- `ConnectionStrings__MainDb` to the real RDS PostgreSQL connection string

## Use in Cloud 66

1. Click `New Application`.
2. Choose GitHub or enter the repo URL.
3. Choose `ASP.NET Core` or `.NET` in the framework dropdown if Cloud 66 asks.
4. Let Cloud 66 detect the `Dockerfile`.
5. Set environment variables in Cloud 66 using the names in this README.
6. Deploy to a small test stack first.

## Create a stack with Toolbelt

After installing and authenticating `cx`, you can create the stack from the repo config:

```bash
cx stacks create \
  --name fridaypos-cloud66-poc \
  --service_yaml service.yml \
  --manifest_yaml manifest.yml
```

The minimal `manifest.yml` in this repo does not include server definitions, so it is not enough for initial provisioning.

For Azure via terminal, use the Azure example manifest after filling in real values:

```bash
cx stacks create \
  --name fridaypos-cloud66-poc \
  --service_yaml service.yml \
  --manifest_yaml manifest.azure.example.yml
```

This avoids the interactive cloud-selection prompt because the target cloud is already defined in the manifest.

For AWS with an external RDS PostgreSQL instance:

```bash
cx stacks create \
  --name fridaypos-cloud66-poc-aws \
  --service_yaml service.yml \
  --manifest_yaml manifest.aws.example.yml
```

The recommended RDS test shape is:

1. Create the RDS PostgreSQL instance in AWS first.
2. Keep the database external to Cloud 66.
3. Deploy only the app server(s) through Cloud 66.
4. Inject the RDS connection string through Cloud 66 environment variables.
5. Verify `/healthz/db` from the deployed app.

For the AWS test, keep the Cloud 66 app server and the RDS instance in the same VPC/region where possible, and allow inbound `5432` on the RDS security group from the Cloud 66-created EC2/app security group.

## Suggested Cloud 66 variables

- `FridayPos__TenantName`
- `FridayPos__Region`
- `FridayPos__EnableRealtime`
- `FridayPos__DeploymentLabel`
- `ConnectionStrings__MainDb`
- `Redis__ConnectionString`
- `RabbitMq__Host`
- `Storage__BlobContainer`
