# Logging and Diagnostic Data

## Application Logs

Application logs are written to [Application Insights](https://docs.microsoft.com/azure/azure-monitor/app/app-insights-overview).  The Azure Functions are configured to use Application Insights via the [provided Application Insights instrumentation ID](https://docs.microsoft.com/azure/azure-functions/functions-monitoring?tabs=cmd#enable-application-insights-integration).  

Please see the official Microsoft documentation for more information on [monitoring Azure Functions](https://docs.microsoft.com/azure/azure-functions/functions-monitoring).

## Diagnostic data

The following Azure resources are configured to send metrics and diagnostic data to a Log Analytics workspace:

- Azure Log Analytics
- Azure IoT Hub (optionally created)

### Sample queries

Below are a few sample queries that may be helpful in finding key information logged via Application Insights.

#### Find log messages related to an operation

In the query below, replace the sample operation ID value with the desired value to query against.

```sql
union *
| order by timestamp asc
| where operation_Id == "5de2bacd02407d4fb18fd4c794584715"
```

### Find log messages for a specific fault property

The example query below shows searching for log statements related to the fault's name.

```sql
traces
| where timestamp > ago(1h)
| where customDimensions.prop__faultName == "Failed Fan Motor"
```

### Summarize faults over time

The example query below summarizing the active faults over a period of time.

```sql
traces
| where timestamp > ago(3h)
| summarize count() by todatetime(customDimensions.prop__faultActiveTime)
```

### Find log data for a specific Azure Service Bus message

In the query below, replace the sample message ID value with the desired value to query against.

```sql
requests
| where timestamp > ago(7d)
| where source contains "type:Azure Service Bus"
| where customDimensions.MessageId == "f1b249fe-a334-4ab9-b46f-a85edf206323"
```

### Find how many alerts took more than 5 minutes to be created (end-to-end)

In the query below, replace "300000" with the value for the desired timespan. The value needs to be in milliseconds (e.g. 300000 is 5 minutes).

```sql
customEvents
| where timestamp > ago(1h)
| where name == "IoTAlertCreated"
| extend elapsedTime = todecimal(customMeasurements.AlertCreatedElapsedTimeMs)
| where elapsedTime > 300000
```

### Find the average

The Azure Function which creates the IoTAlert entity in Dynamics is responsible for calculating the elapsed time to create the alert.  The time is calculated by subtracting the current UTC time from the (UTC) time the original fault was enqueued to the Azure IoT Hub.  The function uses the `EventEnqueuedUtcTime` which is [available when Stream Analytics uses Azure IoT Hub as an input](https://docs.microsoft.com/azure/stream-analytics/stream-analytics-define-inputs#create-an-input-from-event-hubs).

The function creates a [custom event](https://docs.microsoft.com/azure/azure-monitor/app/api-custom-events-metrics#trackevent) with the name of "IoTAlertCreated".  The event includes a metric by the name of "AlertCreatedElapsedTimeMs", for which the value is the difference between the current UTC time (after the Dynamics IoT Alert is created) and the original event's `EventEnqueuedUtcTime`.

```sql
customEvents
| where timestamp > ago(12h)
| where name == "IoTAlertCreated"
| extend elapsedTime = todecimal(customMeasurements.AlertCreatedElapsedTimeMs)
| summarize avg(elapsedTime) by name
```

### Find percentiles

Use the sample query below find the percentiles for the time it takes to create an IoT Alert (from ingestion to alert created in Dynamics).

```sql
customEvents
| where timestamp > ago(7d)
| where name == "IoTAlertCreated"
| extend elapsedTime = todecimal(customMeasurements.AlertCreatedElapsedTimeMs)
| summarize percentiles(elapsedTime, 25, 50, 75, 90) by name
```
