#!/bin/bash

# Base variables
GROUPID="mtc1"
ENVIRONMENT="s"
LOCATION="westus2"
BASE_NAME="$GROUPID$ENVIRONMENT$LOCATION"
RESOURCE_GROUP_NAME="EXP-VFDDemoV2-RG"
SKU="standard"

# Dynamics Variables
DYNAMICS_ENDPOINT="org33202ada.crm"
DYNAMICS_URL="https://${DYNAMICS_ENDPOINT}.crm.dynamics.com"
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
