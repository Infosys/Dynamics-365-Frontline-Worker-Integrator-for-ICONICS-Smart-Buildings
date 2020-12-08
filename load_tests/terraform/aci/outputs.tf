###########################################################################################################
# OUTPUTS
###########################################################################################################

output "locust_uniquer" {
  value = random_string.uniquer.result
}

output "locust_rg_id" {
  value = azurerm_resource_group.rg.id
}

output "locust_rg_name" {
  value = azurerm_resource_group.rg.name
}

output "locust_master_aci_name" {
  value = azurerm_container_group.locust_master.name
}

output "locust_storage_name" {
  value = azurerm_storage_account.storage.name
}

output "locust_master_resources_id" {
  value = azurerm_container_group.locust_master.id
}

output "locust_provisioner_resources_id" {
  value = azurerm_container_group.provisioner.id
}
