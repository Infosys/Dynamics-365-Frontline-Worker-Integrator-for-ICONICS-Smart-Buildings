###########################################################################################################
# VARIABLES
###########################################################################################################

variable "location" {
  type        = string
  description = "Choose the Azure region that's right for you."
}

variable "aci_provisioner_image" {
  type        = string
  default     = "mcr.microsoft.com/azure-cli:2.7.0"
  description = "Provisioner container image name and tag."
}

variable "aci_master_image_basename" {
  type        = string
  default     = "locustio/locust"
  description = "Locust master container image basename (no tag)."
}

variable "aci_master_cpu" {
  type        = string
  default     = "4.0"
  description = "Locust master container CPU cores."
}

variable "aci_master_memory" {
  type        = string
  default     = "8.0"
  description = "Locust master container memory in GB."
}

variable "aci_worker_image_basename" {
  type        = string
  default     = "locustio/locust"
  description = "Locust worker container image basename (no tag)."
}

variable "aci_worker_cpu" {
  type        = string
  default     = "4.0"
  description = "Locust worker container CPU cores."
}

variable "aci_worker_memory" {
  type        = string
  default     = "4.0"
  description = "Locust worker container memory in GB."
}

variable "locust_test_name" {
  type        = string
  default     = "locustfile"
  description = "Locust test name (without .py extension)."
}

variable "locust_version" {
  type        = string
  default     = "1.1"
  description = "Locust version (Locust docker tag as well)."
}

variable "locust_workers_count" {
  type        = number
  description = "Locust workers count."
}

variable "locust_users" {
  type        = number
  description = "Locust test number of users."
}

variable "locust_hatch_rate" {
  type        = number
  description = "The rate per second in which clients are spawned."
}

variable "locust_run_time" {
  type        = string
  description = "Stop after the specified amount of time, e.g. (300s, 20m, 3h, 1h30m, etc.)."
}

variable "locust_csv_full_history" {
  type        = bool
  default     = true
  description = "Store each stats entry in CSV format to _stats_history.csv file."
}

variable "locust_only_summary" {
  type        = bool
  default     = true
  description = "Only print the summary stats."
}

variable "locust_stop_timeout" {
  type        = number
  default     = 60
  description = "Number of seconds to wait for a simulated user to complete any executing task before exiting."
}

# variable "locust_host" {
#   type        = string
#   description = "Host to load test."
# }

variable "locust_step_load" {
  type        = bool
  default     = false
  description = "Enable Step Load mode to monitor how performance metrics varies when user load increases."
}

variable "locust_step_users" {
  type        = number
  default     = null
  description = "Client count to increase by step in Step Load mode."
}

variable "locust_step_time" {
  type        = string
  default     = null
  description = "Step duration in Step Load mode, e.g. (300s, 20m, 3h, 1h30m, etc.)."
}

variable "locust_exit_code_on_error" {
  type        = number
  default     = 1
  description = "Sets the process exit code to use when a test result contain any failure or error."
}

# variable "network_profile_id" {
#   type        = string
#   description = "Network profile resource ID."
# }

variable "debug" {
  type        = bool
  default     = false
  description = "Debug mode."
}

variable "iot_hub_name" {
  type = string
}

variable "iot_hub_sas_token" {
  type = string
}
