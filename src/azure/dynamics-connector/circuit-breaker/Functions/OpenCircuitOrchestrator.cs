// -----------------------------------------------------------------------
// <copyright file="OpenCircuitOrchestrator.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace CircuitBreaker.Functions
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using CircuitBreaker.Models;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.DurableTask;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class for Open Circuit Orchestrator.
    /// </summary>
    public class OpenCircuitOrchestrator
    {
        /// <summary>
        /// Function defined for OpenCircuit.
        /// </summary>
        /// <param name="context"> representing context for function OpenCircuit.</param>
        /// <param name="log"> representing log for function OpenCircuit.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [FunctionName(nameof(OpenCircuit))]
        public async Task OpenCircuit(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            try
            {
                var failureRequest = context.GetInput<FailureRequest>();
                var resourceId = failureRequest.ResourceId;

                if (!context.IsReplaying)
                {
                    log.LogInformation("Disabling function app ({resourceId}) to open circuit.", resourceId);
                }

                var managedIdentity = new ManagedIdentityTokenSource("https://management.azure.com/.default");

                // Note: Configurable resourceId works only for other Azure App Service applications.
                var stopFunctionRequest = new DurableHttpRequest(
                    HttpMethod.Post,
                    new Uri($"https://management.azure.com{resourceId}/stop?api-version=2019-08-01"),
                    tokenSource: managedIdentity);

                // Note: Use of ConfigureAwait can break orchestration tasks (prevents from completing; instance remains in a 'running' state).
                // See https://github.com/Azure/azure-functions-durable-extension/issues/995 and
                // https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-code-constraints
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
                DurableHttpResponse restartResponse = await context.CallHttpAsync(stopFunctionRequest);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

                if (restartResponse.StatusCode != HttpStatusCode.OK)
                {
                    if (!context.IsReplaying)
                    {
                        log.LogError("Failed to stop Function App: {restartResponse.StatusCode}: {restartResponse.Content}", restartResponse.StatusCode, restartResponse.Content);
                    }

                    throw new ApplicationException($"Failed to stop Function App: {restartResponse.StatusCode}: {restartResponse.Content}");
                }

                if (restartResponse.StatusCode == HttpStatusCode.OK)
                {
                    if (!context.IsReplaying)
                    {
                        // Logging as a warning to aide in noticing this messages, as stopping of an Azure Function
                        // may be an action worthy of human intervention.
                        log.LogWarning("Successfully STOPPED Azure Function with Resource ID {resourceId}.", resourceId);
                    }
                }

                return;
            }
            catch (Exception e)
            {
                log.LogError(e, "Unexpected error '{msg}' while processing.", e.Message);
                throw;
            }
        }
    }
}