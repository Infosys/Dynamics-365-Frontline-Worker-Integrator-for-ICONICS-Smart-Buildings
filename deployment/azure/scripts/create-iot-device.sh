#!/bin/bash

# Optional - ensure Azure CLI context is set to appropriate Azure subscription.
# SUBSCRIPTION_ID="[YOUR-AZURE-SUBSCRIPTION-ID]"
# az account set --subscription $SUBSCRIPTION_ID

IOT_HUB_NAME=$1
IOT_HUB_FAULT_DEVICE_NAME=$2

if [ ! -n "$IOT_HUB_NAME" ]
then 
    #Error IOT_HUB_NAME not set or NULL
    exit 1
fi

if [ ! -n "$IOT_HUB_FAULT_DEVICE_NAME" ]
then 
    #Error IOT_HUB_FAULT_DEVICE_NAME not set or NULL
    exit 1
fi

# Create IoT Hub Device
CreateDevice () {
    #Adding Iot Hub extension
    az extension add -n azure-iot &>/dev/null

    #Creating Device
    az iot hub device-identity create -n $IOT_HUB_NAME -d $IOT_HUB_FAULT_DEVICE_NAME --ee false &>/dev/null

    IOT_HUB_FAULT_DEVICE_NAME_KEY=$(az iot hub device-identity show -d $IOT_HUB_FAULT_DEVICE_NAME -n $IOT_HUB_NAME -o json --query "authentication.symmetricKey.primaryKey"  | tr -d \")
}

# Invoke your function
CreateDevice
echo $IOT_HUB_FAULT_DEVICE_NAME_KEY

exit 0