#!/bin/bash

# Lấy IP của Minikube
MINIKUBE_IP=$(minikube ip)

# Thêm data source Prometheus vào Grafana
curl -X POST \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Prometheus-Minikube",
    "type": "prometheus",
    "url": "http://'$MINIKUBE_IP':30090",
    "access": "proxy",
    "basicAuth": false,
    "isDefault": true
  }' \
  http://admin:admin@localhost:3000/api/datasources

echo "Grafana đã được kết nối với Prometheus trên Minikube"