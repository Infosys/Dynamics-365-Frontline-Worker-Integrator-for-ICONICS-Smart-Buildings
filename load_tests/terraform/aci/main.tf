resource "random_string" "uniquer" {
  length  = 15
  upper   = false
  lower   = true
  number  = false
  special = false
}

resource "azurerm_resource_group" "rg" {
  name     = "rg-locust-${random_string.uniquer.result}"
  location = var.location
}

resource "azurerm_storage_account" "storage" {
  name                     = "st${random_string.uniquer.result}"
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = azurerm_resource_group.rg.location
  account_kind             = "StorageV2"
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_storage_share" "share" {
  name                 = "locust"
  storage_account_name = azurerm_storage_account.storage.name
  quota                = 2
}

resource "azurerm_container_group" "provisioner" {
  name                = "aci-provisioner-${random_string.uniquer.result}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name

  ip_address_type = "Public"
  os_type         = "Linux"
  restart_policy  = "OnFailure"

  container {
    name   = "provisioner-${random_string.uniquer.result}"
    image  = "busybox"
    cpu    = "0.5"
    memory = "0.5"

    commands = [
      "/bin/sh",
      "-c",
      "mkdir -p /mnt/locust/tests; mkdir -p /mnt/locust/results; mkdir -p /mnt/locust/logs; echo ${filebase64("locust.tar.gz")} | base64 -d | tar -zx -C /mnt/locust/tests;"
    ]

    # fake port, but block required by TF - bug?
    ports {
      port     = 443
      protocol = "TCP"
    }

    volume {
      name                 = "filesharevolume"
      mount_path           = "/mnt/locust"
      share_name           = azurerm_storage_share.share.name
      storage_account_name = azurerm_storage_account.storage.name
      storage_account_key  = azurerm_storage_account.storage.primary_access_key
    }
  }
}

resource "azurerm_container_group" "locust_master" {
  
  depends_on = [
    azurerm_container_group.provisioner
  ]

  name                = "aci-locustmaster-${random_string.uniquer.result}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name

  # network_profile_id = var.network_profile_id
  # ip_address_type    = "Private"
  os_type        = "Linux"
  restart_policy = "Never"

  container {
    name   = "locustmaster-${random_string.uniquer.result}"
    image  = "${var.aci_master_image_basename}:${var.locust_version}"
    cpu    = var.aci_master_cpu
    memory = var.aci_master_memory

    ports {
      port     = 5557
      protocol = "TCP"
    }

    environment_variables = {
      LOCUST_MODE_MASTER        = true
      LOCUST_HEADLESS           = true
      LOCUST_CSV_FULL_HISTORY   = var.locust_csv_full_history
      LOCUST_ONLY_SUMMARY       = var.locust_only_summary
      LOCUST_STOP_TIMEOUT       = var.locust_stop_timeout
      LOCUST_LOCUSTFILE         = "/mnt/locust/tests/${var.locust_test_name}.py"
      #LOCUST_HOST               = var.locust_host
      LOCUST_USERS              = var.locust_users
      LOCUST_HATCH_RATE         = var.locust_hatch_rate
      LOCUST_RUN_TIME           = var.locust_run_time
      LOCUST_EXPECT_WORKERS     = var.locust_workers_count
      LOCUST_CSV                = "/mnt/locust/results/${random_string.uniquer.result}"
      LOCUST_LOGFILE            = "/mnt/locust/logs/${random_string.uniquer.result}_locustmaster.txt"
      LOCUST_LOGLEVEL           = var.debug == true ? "DEBUG" : "INFO"
      LOCUST_STEP_LOAD          = var.locust_step_load
      LOCUST_STEP_USERS         = var.locust_step_users
      LOCUST_STEP_TIME          = var.locust_step_time
      LOCUST_EXIT_CODE_ON_ERROR = var.locust_exit_code_on_error
      DEBUG                     = var.debug

      IOT_HUB_NAME      = var.iot_hub_name
      IOT_HUB_SAS_TOKEN = var.iot_hub_sas_token
    }

    volume {
      name                 = "filesharevolume"
      mount_path           = "/mnt/locust"
      share_name           = azurerm_storage_share.share.name
      storage_account_name = azurerm_storage_account.storage.name
      storage_account_key  = azurerm_storage_account.storage.primary_access_key
    }
  }
}

resource "azurerm_container_group" "locust_worker" {
  
  depends_on = [
    azurerm_container_group.provisioner
  ]

  count               = var.locust_workers_count
  name                = "aci-locustworker-${random_string.uniquer.result}-${count.index}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name

  # network_profile_id = var.network_profile_id
  # ip_address_type    = "Private"
  os_type        = "Linux"
  restart_policy = "OnFailure"

  container {
    name   = "locustworker-${random_string.uniquer.result}"
    image  = "${var.aci_worker_image_basename}:${var.locust_version}"
    cpu    = var.aci_worker_cpu
    memory = var.aci_worker_memory

    ports {
      port     = 5557
      protocol = "TCP"
    }

    environment_variables = {
      LOCUST_MODE_WORKER      = true
      LOCUST_LOCUSTFILE       = "/mnt/locust/tests/${var.locust_test_name}.py"
      LOCUST_MASTER_NODE_HOST = azurerm_container_group.locust_master.ip_address
      #LOCUST_HOST             = var.locust_host
      LOCUST_LOGFILE          = "/mnt/locust/logs/${random_string.uniquer.result}_locustworker_${count.index}.txt"
      LOCUST_LOGLEVEL         = var.debug == true ? "DEBUG" : "INFO"
      DEBUG                   = var.debug

      IOT_HUB_NAME      = var.iot_hub_name
      IOT_HUB_SAS_TOKEN = var.iot_hub_sas_token
    }
    

    volume {
      name                 = "filesharevolume"
      mount_path           = "/mnt/locust"
      share_name           = azurerm_storage_share.share.name
      storage_account_name = azurerm_storage_account.storage.name
      storage_account_key  = azurerm_storage_account.storage.primary_access_key
    }
  }
}