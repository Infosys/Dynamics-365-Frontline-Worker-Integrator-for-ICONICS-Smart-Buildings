#!/bin/bash

# Base variables
GROUPID="scg51"
ENVIRONMENT="d"
LOCATION="westus2"
BASE_NAME="$GROUPID$ENVIRONMENT$LOCATION"
RESOURCE_GROUP_NAME="${BASE_NAME}rg"
SKU="standard"

# Dynamics Variables
DYNAMICS_ENDPOINT="msftindustryinn.crm"
DYNAMICS_URL="https://${DYNAMICS_ENDPOINT}.dynamics.com"
DYNAMICS_CLIENT_ID="XXXXXXXXXXXX"
DYNAMICS_SECRET="XXXXXXXXXXXX"
DYNAMICS_TENANT_ID="XXXXXXXXXXXX"

# IoT Hub Variables
INCLUDE_IOT_HUB="true" # Should be either "true" or "false"
IOT_HUB_NAME="${BASE_NAME}iot-faults" # Replace with external IoT Hub Name if INCLUDE_IOT_HUB is set to "false"
IOT_HUB_SERVICE_SECRET="XXXXXXXXXXX" # Required if INCLUDE_IOT_HUB is set to "false"
IOT_HUB_DEVICE_SECRET="XXXXXXXXXXX" # Required if INCLUDE_IOT_HUB is set to "false"
IOT_HUB_FAULT_DEVICE_ID="BXConnector" # Required if INCLUDE_IOT_HUB is set to "true"
IOT_HUB_WORKORDER_DEVICE_ID="BXConnector-Dynamics" # Required if INCLUDE_IOT_HUB is set to "true"

# Include additional Logic App to create Work Order and Booking for the IoT Alert
AUTO_WO_CREATE="true" # Should be either "true" or "false"

# Log Workspace Name
LOG_WS_SUBSCRIPTION_ID="XXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXX"
LOG_WS_RESOURCE_GROUP="XXXXXXXXX"
LOG_WS_NAME="XXXXXXX"
