#!/bin/bash

# Existing resource details
AZDO_ORG=""
AZDO_PROJECT=""
REPOSITORY_NAME=""
REPOSITORY_BRANCH=""

# Azure region to deploy test infrastructure (e.g. westus).
AZURE_RESOURCE_LOCATION=""

# Service connection to an Azure subscription where the load test resources will be deployed (e.g. azure_<subcriptionname>)
AZURERM_SERVICE_CONNECTION_NAME=""

# Service connection to a subscription where the system under test (sut) is deployed
SUT_SERVICE_CONNECTION_NAME=""

# Default pipeline names
PIPELINE_NAME_PROVISIONER="[fieldservice] Load Test"

# The version of Locust to use for the tests (see https://docs.locust.io/en/stable/changelog.html).
LOCUST_VERSION="1.1"