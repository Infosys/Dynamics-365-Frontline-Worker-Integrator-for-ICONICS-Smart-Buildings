#!/bin/bash

# Working directory assumed to be ~/deployment/azure/scripts/
# Pre-requisites: 
#   azure cli >=2.11.1
#   dotnet core 3.1
#   azure functions core tools >=3.0.2881

# Ensure you have logged into Azure and Set the correct azure subscription like so:
# az login
# az account set --subscription {name or id of subscription}

PARAMETERS_FILE_NAME="deploy-parameters.sh"

# Load the paramters file
source $PARAMETERS_FILE_NAME

# Base variables
BASE_NAME="$GROUPID$ENVIRONMENT$LOCATION"
DEPLOYMENT_NUMBER=$RANDOM

SUBSCRIPTION_ID=$(az account show -o json --query "id" | tr -d \")
SUBSCRIPTION_NAME=$(az account show -o json --query "name" | tr -d \")
VAULT_ID="/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/${RESOURCE_GROUP_NAME}/providers/Microsoft.KeyVault/vaults/${BASE_NAME}kvt"
STARTING_PWD=$(pwd)

echo "Provisioning resources to the ${RESOURCE_GROUP_NAME} resource group within the ${SUBSCRIPTION_NAME} (${SUBSCRIPTION_ID}) subscription. Resources will have the base name: ${BASE_NAME}."
read -p "Continue (y/n)?" choice
if [[ ! $choice =~ ^[Yy]$ ]]
then
    echo "Cancelling operation."
    [[ "$0" = "$BASH_SOURCE" ]] && exit 1 || return 1 # handle exits from shell or function but don't exit interactive shell
fi

# Following will fail if the resource group doesn't exist (thats ok)
set +e

az group show -n $RESOURCE_GROUP_NAME || CREATE_GROUP=true

if [[ $CREATE_GROUP ]]
then
    echo "Creating resource group ${RESOURCE_GROUP_NAME}"
    az group create -n $RESOURCE_GROUP_NAME -l $LOCATION
fi

# Bail if any command fails
set -e

echo "Deployment Number: ${DEPLOYMENT_NUMBER}"

echo "Deploying Foundational Resources"
az deployment group create \
    -g $RESOURCE_GROUP_NAME \
    -n "CLI-Foundational-Resources-${DEPLOYMENT_NUMBER}" \
    -f ../azure-deploy.json \
    -p groupId=$GROUPID environment=$ENVIRONMENT sku=$SKU

if [ "$INCLUDE_IOT_HUB" == "true" ]; then
    echo "Deploying IoT Hub"
    az deployment group create \
        -g $RESOURCE_GROUP_NAME \
        -n "CLI-IotHub-${DEPLOYMENT_NUMBER}" \
        -f ../iot-hub/azure-deploy.json \
        -p groupId=$GROUPID environment=$ENVIRONMENT sku=$SKU

    echo "Creating Device $IOT_HUB_FAULT_DEVICE_ID on IoT Hub $IOT_HUB_NAME"
    IOT_HUB_FAULT_DEVICE_KEY=$(./create-iot-device.sh $IOT_HUB_NAME $IOT_HUB_FAULT_DEVICE_ID)
    echo "Device Key for $IOT_HUB_FAULT_DEVICE_ID is $IOT_HUB_FAULT_DEVICE_KEY"

    echo "Creating Device $IOT_HUB_WORKORDER_DEVICE_ID on IoT Hub $IOT_HUB_NAME"
    IOT_HUB_FAULT_DEVICE_KEY=$(./create-iot-device.sh $IOT_HUB_NAME $IOT_HUB_WORKORDER_DEVICE_ID)
    echo "Device Key for $IOT_HUB_WORKORDER_DEVICE_ID is $IOT_HUB_WORKORDER_DEVICE_KEY"
fi

echo "Deploying External Secrets"
az deployment group create \
    -g $RESOURCE_GROUP_NAME \
    -n "CLI-External-Secrets-${DEPLOYMENT_NUMBER}" \
    -f ../key-vault-secrets/azure-deploy.json \
    -p groupId=$GROUPID \
       environment=$ENVIRONMENT \
       dynamicsClientId=$DYNAMICS_CLIENT_ID \
       dynamicsClientSecret=$DYNAMICS_SECRET \
       iotHubIncludedInPipeline="$INCLUDE_IOT_HUB" \
       iotHubServiceSecret=$IOT_HUB_SERVICE_SECRET \
       iotHubDeviceSecret=$IOT_HUB_DEVICE_SECRET

echo "Deploying Function App Settings"
az deployment group create \
    -g $RESOURCE_GROUP_NAME \
    -n "CLI-Function-App-Settings-${DEPLOYMENT_NUMBER}" \
    -f ../function-apps/azure-deploy.json \
    -p groupId=$GROUPID environment=$ENVIRONMENT \
       dynamicsEndpoint=$DYNAMICS_URL \
       iotHubName=$IOT_HUB_NAME

echo "Adding stream analytics extension"
az extension add -n stream-analytics

# Following will fail if the stream analytics job is already in stopped state (thats ok)
set +e

echo "Stopping ${BASE_NAME}sa from ${RESOURCE_GROUP_NAME}"
az stream-analytics job stop --resource-group ${RESOURCE_GROUP_NAME} --name ${BASE_NAME}sa

# Set to bail if any command fails
set -e

echo "Deploying Stream Analytics"
PARAMETERS="{ \"groupId\": { \"value\": \"${GROUPID}\" }, \"environment\": { \"value\": \"${ENVIRONMENT}\" }, \"iotHubName\": { \"value\": \"${IOT_HUB_NAME}\" }, \"iotHubSharedAccessPolicyKey\": { \"reference\": { \"keyVault\": { \"id\": \"${VAULT_ID}\" }, \"secretName\": \"IoTServiceKey-faults\" } }, \"iotHubFaultDeviceId\": { \"value\": \"${IOT_HUB_FAULT_DEVICE_ID}\" }, \"serviceBusSharedAccessPolicyKey\": { \"reference\": { \"keyVault\": { \"id\": \"${VAULT_ID}\" }, \"secretName\": \"ServiceBusKeySend\" } } }"
echo "Parameters: ${PARAMETERS}"
az deployment group create \
    -g $RESOURCE_GROUP_NAME \
    -n "CLI-Stream-Analytics-Job-${DEPLOYMENT_NUMBER}" \
    -f ../stream-analytics/azure-deploy.json \
    -p "${PARAMETERS}"

echo "Deploying Work Order Ack Logic App"
PARAMETERS="{ \"groupId\": { \"value\": \"${GROUPID}\" }, \"environment\": { \"value\": \"${ENVIRONMENT}\" }, \"dynamicsEndpoint\": { \"value\": \"${DYNAMICS_ENDPOINT}\" }, \"clientId\": { \"reference\": { \"keyVault\": { \"id\": \"${VAULT_ID}\" }, \"secretName\": \"DynamicsClientId\" } }, \"clientSecret\": { \"reference\": { \"keyVault\": { \"id\": \"${VAULT_ID}\" }, \"secretName\": \"DynamicsClientSecret\" } }, \"tenantId\": { \"value\": \"${DYNAMICS_TENANT_ID}\" }, \"ServiceBusConnectionString\": { \"reference\": { \"keyVault\": { \"id\": \"${VAULT_ID}\" }, \"secretName\": \"ServiceBusConnectionSend\" } } }"
echo "Parameters: ${PARAMETERS}"
az deployment group create \
    -g $RESOURCE_GROUP_NAME \
    -n "CLI-Work-Order-Ack-Logic-App-${DEPLOYMENT_NUMBER}" \
    -f ../logic-apps/work-order-ack/azure-deploy.json \
    -p "${PARAMETERS}"

if [ "$AUTO_WO_CREATE" == "true" ]; then
    echo "Deploying Work Order Create Logic App"
    PARAMETERS="{ \"groupId\": { \"value\": \"${GROUPID}\" }, \"environment\": { \"value\": \"${ENVIRONMENT}\" }, \"dynamicsEndpoint\": { \"value\": \"${DYNAMICS_ENDPOINT}\" }, \"clientId\": { \"reference\": { \"keyVault\": { \"id\": \"${VAULT_ID}\" }, \"secretName\": \"DynamicsClientId\" } }, \"clientSecret\": { \"reference\": { \"keyVault\": { \"id\": \"${VAULT_ID}\" }, \"secretName\": \"DynamicsClientSecret\" } }, \"tenantId\": { \"value\": \"${DYNAMICS_TENANT_ID}\" } }"
    echo "Parameters: ${PARAMETERS}"
    az deployment group create \
        -g $RESOURCE_GROUP_NAME \
        -n "CLI-Work-Order-Create-Logic-App-${DEPLOYMENT_NUMBER}" \
        -f ../logic-apps/work-order-create/azure-deploy.json \
        -p "${PARAMETERS}"
fi

CODE_DIRECTORY="../../../src/azure/dynamics-connector/dynamics-connector"
cd $CODE_DIRECTORY
PWD=$(pwd)
FUNCTION_APP_NAME="${BASE_NAME}fa"

echo "Publishing function app code from ${PWD} to ${FUNCTION_APP_NAME}"
func azure functionapp publish $FUNCTION_APP_NAME --csharp

CODE_DIRECTORY="../circuit-breaker"
cd $CODE_DIRECTORY
PWD=$(pwd)
FUNCTION_APP_NAME="${BASE_NAME}fa-circuit-breaker"

echo "Publishing function app code from ${PWD} to ${FUNCTION_APP_NAME}"
func azure functionapp publish $FUNCTION_APP_NAME --csharp

cd $STARTING_PWD
