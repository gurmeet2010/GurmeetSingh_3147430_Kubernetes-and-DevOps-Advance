# Kubernetes Multi-Tier Assignment — Customers API

## Overview

This project demonstrates the deployment of a containerized **.NET 8 Customers API** and **PostgreSQL database** on **Google Kubernetes Engine (GKE)** using Kubernetes best practices.

### Key Features

* Multi-tier architecture
* Stateless API deployment
* Stateful database deployment
* Persistent storage
* Configuration management using ConfigMaps
* Secure credential management using Secrets
* Horizontal Pod Autoscaling (HPA)
* Self-healing and rolling updates
* NGINX Ingress for external access
* FinOps cost optimization practices

---

## Repository & Deployment Links

| Item              | URL                                                                                   |
| ----------------- | ------------------------------------------------------------------------------------- |
| GitHub Repository | https://github.com/gurmeet2010/GurmeetSingh_3147430_Kubernetes-and-DevOps-Advance.git |
| Docker Hub Image  | https://hub.docker.com/repository/docker/gurmeetsinghchauhan/customers-api            |
| Live API Endpoint | https://35.200.145.198/customers                                                      |

---

## Solution Architecture

```text
Internet
    │
    ▼
NGINX Ingress
    │
    ▼
customers-api-service (ClusterIP)
    │
    ├── customers-api Pod 1
    ├── customers-api Pod 2
    ├── customers-api Pod 3
    └── customers-api Pod 4
              │
              ▼
postgres-service (ClusterIP)
              │
              ▼
PostgreSQL StatefulSet
              │
              ▼
PersistentVolumeClaim (1Gi)
```

---

## Components

### API Layer

* Built using .NET 8 Minimal API
* Runs as a Kubernetes Deployment
* Configured with 4 replicas
* Supports rolling updates
* Automatically scales using HPA
* Self-healing through Kubernetes controllers

### Database Layer

* PostgreSQL 15 Alpine image
* Deployed using StatefulSet
* Uses Persistent Volume Claims (PVC)
* Data survives pod recreation
* Stable pod identity for persistent storage

### Networking Layer

* Internal communication through ClusterIP Services
* External access through NGINX Ingress Controller
* Kubernetes Ingress-based traffic routing

### Configuration Layer

#### ConfigMap

Stores non-sensitive configuration values:

* Database Host
* Database Port
* Database Name
* Database Username

#### Secret

Stores sensitive configuration values:

* Database Password

---

## Project Structure

```text
assignment/
├── srcc/
│   └── CustomersApi/
│       ├── Program.cs
│       ├── CustomersAPI.csproj
│       └── Dockerfile
│
├── k8s-cls/
│   ├── 00-namespace.yaml
│   ├── 01-configmap.yaml
│   ├── 02-secret.yaml
│   ├── 03-db-statefulset.yaml
│   ├── 04-db-service.yaml
│   ├── 05-api-deployment.yaml
│   ├── 06-api-service.yaml
│   ├── 07-ingress.yaml
│   └── 08-hpa.yaml
│
└── README.md
```

---

## Kubernetes Resources Used

| Resource                        | Purpose                                    |
| ------------------------------- | ------------------------------------------ |
| Namespace                       | Logical isolation of application resources |
| ConfigMap                       | Database configuration management          |
| Secret                          | Secure storage of database credentials     |
| StatefulSet                     | Stable PostgreSQL deployment               |
| PersistentVolumeClaim (PVC)     | Persistent database storage                |
| Deployment                      | API pod management                         |
| Service (ClusterIP)             | Internal service communication             |
| Ingress                         | External application access                |
| Horizontal Pod Autoscaler (HPA) | Automatic scaling based on CPU utilization |

---

## Prerequisites

Ensure the following tools are installed before deployment:

* Google Cloud SDK (gcloud)
* Docker Desktop
* kubectl
* Git
* Google Cloud Project with billing enabled

---

## Authenticate with Google Cloud

```bash
gcloud auth login

gcloud config set project YOUR_PROJECT_ID

gcloud services enable container.googleapis.com
```

---

## Create GKE Cluster

Create an Autopilot cluster:

```bash
gcloud container clusters create-auto k8s-autopilot \
  --region=asia-south1 \
  --project=YOUR_PROJECT_ID
```

Configure kubectl access:

```bash
gcloud container clusters get-credentials k8s-autopilot \
  --region=asia-south1
```

Verify cluster connectivity:

```bash
kubectl get nodes
```

---

## Build and Push Docker Image

Navigate to the application directory:

```bash
cd srcc/CustomersApi
```

Build the Docker image:

```bash
docker build -t gurmeetsinghchauhan/customers-api:v1 .
```

Push the image to Docker Hub:

```bash
docker push gurmeetsinghchauhan/customers-api:v1
```

> Update the deployment manifest with the correct image name before deployment.

---

## Install NGINX Ingress Controller

Install the NGINX Ingress Controller:

```bash
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.10.0/deploy/static/provider/cloud/deploy.yaml
```

Verify installation:

```bash
kubectl get pods -n ingress-nginx
```

---

## Deploy the Application

Deploy Kubernetes resources in sequence:

```bash
kubectl apply -f k8s-cls/01-namespace.yaml

kubectl apply -f k8s-cls/02-configmap.yaml

kubectl apply -f k8s-cls/03-secret.yaml

kubectl apply -f k8s-cls/04-db-statefulset.yaml

kubectl apply -f k8s-cls/05-db-service.yaml

kubectl apply -f k8s-cls/06-api-deployment.yaml

kubectl apply -f k8s-cls/07-api-service.yaml

kubectl apply -f k8s-cls/08-ingress.yaml

kubectl apply -f k8s-cls/09-hpa.yaml
```

Verify deployment:

```bash
kubectl get all -n customers-ns
```

---

## Retrieve External Endpoint

Get the Ingress external IP:

```bash
kubectl get ingress -n customers-ns
```

Example:

```text
NAME                CLASS   HOSTS   ADDRESS
customers-ingress   nginx   *       35.200.145.198
```

Access the API:

```text
https://35.200.145.198/customers
```

---

## API Endpoints

| Method | Endpoint   | Description                   |
| ------ | ---------- | ----------------------------- |
| GET    | /          | Service Information           |
| GET    | /health    | Health Check Endpoint         |
| GET    | /info      | Pod and Configuration Details |
| GET    | /customers | Returns All Customers         |

---

## Validation & Testing

### Verify Kubernetes Resources

```bash
kubectl get all -n customers-ns

kubectl get configmap,secret,pvc,ingress,hpa -n customers-ns
```

### Test API

```bash
curl https://35.200.145.198/customers

curl https://35.200.145.198/health

curl https://35.200.145.198/info
```

---

## Demonstrate Self-Healing

Delete API pods:

```bash
kubectl delete pod -l app=customers-api \
-n customers-ns --wait=false
```

Watch pod recreation:

```bash
kubectl get pods -n customers-ns -w
```

Kubernetes automatically recreates deleted pods to maintain the desired state.

---

## Demonstrate Database Persistence

Delete PostgreSQL pod:

```bash
kubectl delete pod postgres-0 -n customers-ns
```

Monitor recovery:

```bash
kubectl get pods -n customers-ns -w
```

Verify data availability:

```bash
curl https://35.200.145.198/customers
```

Data remains intact because storage is preserved through the Persistent Volume Claim.

---

## Demonstrate Rolling Updates

Deploy a new application version:

```bash
kubectl set image deployment/customers-api \
customers-api=gurmeetsinghchauhan/customers-api:v2 \
-n customers-ns
```

Monitor rollout:

```bash
kubectl rollout status deployment/customers-api \
-n customers-ns
```

Rolling updates ensure application availability during deployments.

---

## Verify Horizontal Pod Autoscaler (HPA)

```bash
kubectl get hpa -n customers-ns

kubectl describe hpa customers-api-hpa -n customers-ns
```

### HPA Configuration

| Configuration          | Value |
| ---------------------- | ----- |
| Minimum Replicas       | 2     |
| Maximum Replicas       | 8     |
| Target CPU Utilization | 50%   |

---

## FinOps Optimizations

### 1. Minimal CPU Requests

```text
50m CPU per API Pod
```

Reduces idle resource consumption.

### 2. Horizontal Pod Autoscaling

```text
Min Replicas: 2
Max Replicas: 8
```

Scales resources only when required.

### 3. Lightweight PostgreSQL Image

```text
postgres:15-alpine
```

Provides a smaller image size and faster startup time.

### 4. Right-Sized Persistent Storage

```text
PVC Size: 1Gi
```

Avoids unnecessary storage expenses.

### 5. Internal Service Communication

ClusterIP Services eliminate the need for additional external load balancers, helping reduce cloud infrastructure costs.

---

## Assumptions

The following assumptions were made during implementation:

* The Kubernetes cluster is hosted on Google Kubernetes Engine (GKE).
* The Docker image is available in Docker Hub.
* The NGINX Ingress Controller is installed and operational.
* PostgreSQL credentials are managed through Kubernetes Secrets.
* All resources are deployed within the `customers-ns` namespace.

---

## Author

**Gurmeet Singh**
Kubernetes & DevOps Advanced Assignment
