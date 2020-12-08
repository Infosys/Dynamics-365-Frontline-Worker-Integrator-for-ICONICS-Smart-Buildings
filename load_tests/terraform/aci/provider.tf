provider "azurerm" {
  version = "~>2.0.0"
  features {}
}

terraform {
  required_version = ">=0.12.28"
  backend "local" {}
}