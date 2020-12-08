// -----------------------------------------------------------------------
// <copyright file="CreateAlertFromIconicsFault.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace DynamicsConnector.Functions
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using CircuitBreaker.Models;
    using DynamicsConnector.Dynamics.Interfaces;
    using DynamicsConnector.Dynamics.Models;
    using DynamicsConnector.Iconics.Interfaces;
    using DynamicsConnector.Iconics.Models;
    using DynamicsConnector.Properties;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// Azure function to process Iconic Fault Data to create Dynamics IoT Alert.
    /// </summary>
    public class CreateAlertFromIconicsFault
    {
        // The Azure resourceID of the Azure Function.
        // The ResourceID is to be used by the circuit breaker function, which will stop the specified function app (using the ResourceID)
        private static readonly string ResourceId = Environment.GetEnvironmentVariable("ResourceId");

        // The full URI for the circuit breaker function.
        private static readonly string CircuitRequestUri = Environment.GetEnvironmentVariable("CircuitRequestUri");

        // The function authentication key.
        private static readonly string CircuitCode = Environment.GetEnvironmentVariable("FunctionAppKey");

        private readonly IDynamicsEntityService dynamicsEntityService;
        private readonly IValidationService validationService;
        private readonly IErrorQueueService errorQueueService;
        private readonly InstanceId instanceId;
        private readonly TelemetryClient telemetryClient;
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAlertFromIconicsFault"/> class.
        /// </summary>
        /// <param name="dynamicsEntityService">set dynamicsEntityService via dependency injection.</param>
        /// <param name="validationService">set validationService via dependency injection.</param>
        /// <param name="errorQueueService">set errorQueueService via dependency injection.</param>
        /// <param name="telemetryConfiguration">Set Application Insights telemetry configuration via dependency injection.</param>
        /// <param name="instance">set instanceId via dependency injection.</param>
        /// <param name="httpclient">set httpClient via dependency injection.</param>
        public CreateAlertFromIconicsFault(TelemetryConfiguration telemetryConfiguration, IDynamicsEntityService dynamicsEntityService, IValidationService validationService, IErrorQueueService errorQueueService, InstanceId instance, HttpClient httpclient)
        {
            // dynamicsEntityService is added via dependency injection
            this.dynamicsEntityService = dynamicsEntityService;
            this.validationService = validationService;
            this.errorQueueService = errorQueueService;
            this.instanceId = instance;
            this.httpClient = httpclient;
            this.telemetryClient = new TelemetryClient(telemetryConfiguration);
        }

        /// <summary>
        /// Azure function main method.
        /// </summary>
        /// <param name="message">Topic Message content.</param>
        /// <param name="log">ILogger object to log message to Application insights.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [FunctionName("CreateAlertFromIconicsFault")]
        public async Task Run(
            [ServiceBusTrigger("%ServiceBusTopic%", "%ServiceBusSubscription%", Connection = "ServiceBusConnection")] Message message,
            ILogger log)
        {
            IconicsFault iconicsFault = null;
            string messageContents = null;

            if (message == null)
            {
                // This should never happen.
                throw new ArgumentNullException(nameof(message));
            }

            try
            {
                if (message != null)
                {
                    // deserialize message contents into iconics fault class
                    messageContents = Encoding.UTF8.GetString(message.Body);
                    iconicsFault = JsonConvert.DeserializeObject<IconicsFault>(messageContents);

                    log.LogInformation(
                        "Received ICONICS fault with FaultName '{faultName}' and FaultActiveTime of '{faultActiveTime}' and AssetPath of '{assetPath}'.",
                        iconicsFault.FaultName,
                        iconicsFault.FaultActiveTime,
                        iconicsFault.AssetPath);
                }
            }
            catch (JsonException e)
            {
                // message could not be deserialized, send to error queue
                log.LogError(
                    "Exception while processing message: {message}, exception :{exceptionMessage}. Sending message to the error queue.",
                    message,
                    e.Message);

                await this.errorQueueService.SendMessageToErrorQueueAsync(messageContents, e.Message).ConfigureAwait(false);
                return;
            }

            // validate fault data properties
            if (!this.validationService.IsValidFault(iconicsFault, out string validationMessage))
            {
                // send message to error queue
                log.LogError(Resources.FaultValidationFailure);
                await this.errorQueueService.SendMessageToErrorQueueAsync(messageContents, validationMessage).ConfigureAwait(false);
            }
            else
            {
                if (this.dynamicsEntityService.IsCdsServiceReady())
                {
                    // If Iconic Fault State is "Active", Function should create new IoT Alert in Dynamics
                    if (string.IsNullOrEmpty(iconicsFault.FaultState) || iconicsFault.FaultState.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    {
                        // Create Dynamics asset (if necessary) and alert
                        Guid assetId = this.dynamicsEntityService.CreateAssetIfNotExists(iconicsFault, log);
                        Guid alertId = this.dynamicsEntityService.CreateAlert(iconicsFault, assetId, log);

                        // Determine the elapsed time to create the alert.
                        // Lack of an EventEnqueuedUtcTime should not prevent the alert from being created. The field impacts telemetry only.
                        var parseSucceeded = DateTime.TryParse(iconicsFault.EventEnqueuedUtcTime, out DateTime enqueuedDateTimeUtc);
                        if (!parseSucceeded)
                        {
                            log.LogWarning("Unable to determine IoT alert created elapsed time due to missing fault event enqueued time.");
                        }
                        else
                        {
                            var et = new EventTelemetry("IoTAlertCreated");
                            et.Metrics.Add("AlertCreatedElapsedTimeMs", (DateTime.UtcNow - enqueuedDateTimeUtc).TotalMilliseconds);
                            this.telemetryClient.TrackEvent(et);
                        }
                    }
                    else if (iconicsFault.FaultState.Equals("InActive", StringComparison.OrdinalIgnoreCase))
                    {
                        // If if Iconic Fault State is "InActive", Function should find existing IoT Alert and update Alert Data and Status of an IoT Alert to InActive in Dynamics

                        // Update IoT Alert State to InActive and Alert Data with latest fault data
                        this.dynamicsEntityService.UpdateIoTAlert(iconicsFault, log, (int)IotAlertStateCode.Inactive, (int)IotAlertStatusCode.Inactive);
                    }
                }
                else
                {
                    log.LogError("{msg}, error: {error}", Resources.CDSConnectionFailure, this.dynamicsEntityService.RetrieveCdsError());

                    FailureRequest failureReq = new FailureRequest
                    {
                        // RequestId = $"{message?.MessageId}:{Guid.NewGuid()}",
                        RequestId = Guid.NewGuid().ToString(),
                        FailureTime = DateTime.UtcNow,
                        InstanceId = this.instanceId.Id,
                        ResourceId = ResourceId,
                    };

                    string appName = ResourceId.Split('/').Last();

                    string circuitBreakerBaseUri = $"{CircuitRequestUri}/{appName}/AddFailure";
                    var response = await this.httpClient.PostAsJsonAsync($"{circuitBreakerBaseUri}?code={CircuitCode}", failureReq).ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        log.LogError("Failed to invoke Circuit Breaker function, {circuitBreakerBaseUri}.  Status code: {code}.  Reason: '{reason}'", circuitBreakerBaseUri, response.StatusCode, response.ReasonPhrase);
                    }
                    else
                    {
                        log.LogInformation("Successfully invoked Circuit Breaker function, {circuitBreakerBaseUri}", circuitBreakerBaseUri);
                    }

                    // Throw an exception here in order to instruct the binding to Abandon the message (for the function to fail).
                    // See https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?tabs=csharp#peeklock-behavior
                    throw new ApplicationException($"{Resources.CDSConnectionFailure}, error: {this.dynamicsEntityService.RetrieveCdsError()}");
                 }
            }
        }
    }
}