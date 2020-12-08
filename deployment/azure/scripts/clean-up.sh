#!/bin/bash

GROUPID="fldsrv1"
ENVIRONMENT="s"
LOCATION="eastus"
RESOURCE_GROUP_NAME="CSEInfosysConnector-staging"

BASE_NAME="$GROUPID$ENVIRONMENT$LOCATION"

echo "Deleting ${BASE_NAME}fa from ${RESOURCE_GROUP_NAME}"
az functionapp delete --name "${BASE_NAME}fa" -g "$RESOURCE_GROUP_NAME"

echo "Deleting ${BASE_NAME}sa2 from ${RESOURCE_GROUP_NAME}"
az storage account delete -n "${BASE_NAME}sa2" -g "$RESOURCE_GROUP_NAME" -y

echo "Deleting ${BASE_NAME}asp from ${RESOURCE_GROUP_NAME}"
az appservice plan delete -n "${BASE_NAME}asp" -g "$RESOURCE_GROUP_NAME" -y

echo "Deleting ${BASE_NAME}fa-circuit-breaker from ${RESOURCE_GROUP_NAME}"
az functionapp delete --name "${BASE_NAME}fa-circuit-breaker" -g "$RESOURCE_GROUP_NAME"

echo "Deleting ${BASE_NAME}sa2cb from ${RESOURCE_GROUP_NAME}"
az storage account delete -n "${BASE_NAME}sa2cb" -g "$RESOURCE_GROUP_NAME" -y

echo "Deleting ${BASE_NAME}asp-circuit-breaker from ${RESOURCE_GROUP_NAME}"
az appservice plan delete -n "${BASE_NAME}asp-circuit-breaker" -g "$RESOURCE_GROUP_NAME" -y

echo "Deleting ${BASE_NAME}log from ${RESOURCE_GROUP_NAME}"
az monitor log-analytics workspace delete -n "${BASE_NAME}log" -g "$RESOURCE_GROUP_NAME" -y -f true

echo "Deleting ${BASE_NAME}sb from ${RESOURCE_GROUP_NAME}"
az servicebus namespace delete -n "${BASE_NAME}sb" -g "$RESOURCE_GROUP_NAME"

echo "Deleting ${BASE_NAME}kvt from ${RESOURCE_GROUP_NAME}"
az keyvault delete -n "${BASE_NAME}kvt"

echo "Purging ${BASE_NAME}kvt from ${RESOURCE_GROUP_NAME}"
az keyvault purge -n "${BASE_NAME}kvt"

echo "Adding app insight extension"
az extension add -n application-insights

echo "Deleting ${BASE_NAME}ai from ${RESOURCE_GROUP_NAME}"
az monitor app-insights component delete -a "${BASE_NAME}ai" -g "$RESOURCE_GROUP_NAME"

echo "Adding stream analytics extension"
az extension add -n stream-analytics

echo "Deleting ${BASE_NAME}sa from ${RESOURCE_GROUP_NAME}"
az stream-analytics job delete -n "${BASE_NAME}sa" -g "$RESOURCE_GROUP_NAME"

echo "Adding logic extension"
az extension add -n logic

echo "Deleting ${BASE_NAME}la-work-order-ack from ${RESOURCE_GROUP_NAME}"
az logic workflow delete -n "${BASE_NAME}la-work-order-ack" -g "$RESOURCE_GROUP_NAME" -y

echo "Deleting servicebus connection from ${RESOURCE_GROUP_NAME}"
az resource delete --resource-type 'Microsoft.Web/connections' -n "servicebus" -g "$RESOURCE_GROUP_NAME"

echo "Deleting commondataservice connection from ${RESOURCE_GROUP_NAME}"
az resource delete --resource-type 'Microsoft.Web/connections' -n "commondataservice" -g "$RESOURCE_GROUP_NAME"

echo "Deleting ${BASE_NAME}iot-faults from ${RESOURCE_GROUP_NAME}"
az iot hub delete -n "${BASE_NAME}iot-faults" -g "$RESOURCE_GROUP_NAME"
