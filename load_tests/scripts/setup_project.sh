#!/bin/bash -e

# Import variables
source config.sh

echo "[$(date +"%F %T")] Setting up devops config: $AZDO_ORG | $AZDO_PROJECT ..."
az devops configure --defaults organization=$AZDO_ORG project=$AZDO_PROJECT

az pipelines create --name "$PIPELINE_NAME_PROVISIONER" \
    --repository $REPOSITORY_NAME \
    --organization=$AZDO_ORG \
    --project=$AZDO_PROJECT \
    --folder-path loadtest \
    --repository-type tfsgit \
    --branch $REPOSITORY_BRANCH \
    --skip-first-run \
    --yml-path load_tests/pipelines/pipeline.locust.provisioner.aci.yml

# Create input variables for provisioner pipeline
az pipelines variable create --pipeline-name "$PIPELINE_NAME_PROVISIONER" \
    --name PROFILE_CONFIG \
    --value burst --allow-override \
    --organization=$AZDO_ORG --project=$AZDO_PROJECT

az pipelines variable create --pipeline-name "$PIPELINE_NAME_PROVISIONER" \
    --name TARGET_CONFIG \
    --value test --allow-override \
    --organization=$AZDO_ORG --project=$AZDO_PROJECT

az pipelines variable create --pipeline-name "$PIPELINE_NAME_PROVISIONER" \
    --name LOCUST_TEST_NAME \
    --value locustfile_burst --allow-override \
    --organization=$AZDO_ORG --project=$AZDO_PROJECT

az pipelines variable create --pipeline-name "$PIPELINE_NAME_PROVISIONER" \
    --name AZURE_RESOURCE_LOCATION \
    --value $AZURE_RESOURCE_LOCATION --allow-override \
    --organization=$AZDO_ORG --project=$AZDO_PROJECT

az pipelines variable create --pipeline-name "$PIPELINE_NAME_PROVISIONER" \
    --name AZURERM_SERVICE_CONNECTION_NAME \
    --value "$AZURERM_SERVICE_CONNECTION_NAME" --allow-override \
    --organization=$AZDO_ORG --project=$AZDO_PROJECT

az pipelines variable create --pipeline-name "$PIPELINE_NAME_PROVISIONER" \
    --name SUT_SERVICE_CONNECTION_NAME \
    --value "$SUT_SERVICE_CONNECTION_NAME" --allow-override \
    --organization=$AZDO_ORG --project=$AZDO_PROJECT

az pipelines variable create --pipeline-name "$PIPELINE_NAME_PROVISIONER" \
    --name LOCUST_VERSION \
    --value $LOCUST_VERSION --allow-override \
    --organization=$AZDO_ORG --project=$AZDO_PROJECT

az pipelines variable create --pipeline-name "$PIPELINE_NAME_PROVISIONER" \
    --name IOT_HUB_NAME \
    --value "" --allow-override \
    --organization=$AZDO_ORG --project=$AZDO_PROJECT