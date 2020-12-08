# A test profile specifically for testing the pipeline E2E, just deploys two locust workers and runs tests for two minutes
# A test profile specifically for testing the pipeline E2E, just deploys two locust workers and runs tests for two minutes
locust_step_load     = false
locust_users         = 18
locust_hatch_rate    = 1
locust_run_time      = "30m"
locust_workers_count = 1