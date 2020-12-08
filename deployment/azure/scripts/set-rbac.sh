#!/bin/bash

RESOURCE_GROUP_NAME="[YOUR-AZURE-RESOURCE-GROUP-NAME]"
SERVICE_PRINCIPAL_OBJECT_ID="[YOUR-AZURE-FUNCTION-MANAGED-IDENTITY-OBJECT-ID]"
RBAC_ROLE_NAME="Website Contributor"

# Optional - ensure Azure CLI context is set to appropriate Azure subscription.
# SUBSCRIPTION_ID="[YOUR-AZURE-SUBSCRIPTION-ID]"
# az account set --subscription $SUBSCRIPTION_ID

echo "Listing current assignments for service principal with ObjectId of $SERVICE_PRINCIPAL_OBJECT_ID"
az role assignment list --assignee $SERVICE_PRINCIPAL_OBJECT_ID  --resource-group $RESOURCE_GROUP_NAME

echo "Creating role assignment of '$RBAC_ROLE_NAME' in resource group $resourceGroup"
az role assignment create --assignee $SERVICE_PRINCIPAL_OBJECT_ID --role "$RBAC_ROLE_NAME" --resource-group $RESOURCE_GROUP_NAME

# To remove the role assignment, execute the following line.
# az role assignment delete --assignee $SERVICE_PRINCIPAL_OBJECT_ID --role "$RBAC_ROLE_NAME" --resource-group $RESOURCE_GROUP_NAME