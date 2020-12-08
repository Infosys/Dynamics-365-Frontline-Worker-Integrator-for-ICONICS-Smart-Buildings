// -----------------------------------------------------------------------
// <copyright file="CircuitBreakerFunction.cs" company="Microsoft Corporation">
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
    using CircuitBreaker.Interfaces;
    using CircuitBreaker.Models;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.DurableTask;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// Add Failure Request HTTP Trigger Class.
    /// </summary>
    public static class CircuitBreakerFunction
    {
        /// <summary>
        /// Azure function main method.
        /// </summary>
        /// <param name="req">Http Request message.</param>
        /// <param name="client">Client.</param>
        /// <param name="log">ILogger object to log message to Application Insights.</param>
        /// <param name="entityKey">Entity key.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [FunctionName("AddFailureFunction")]
        public static async Task<HttpResponseMessage> AddFailureFunction(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "CircuitBreaker/{entityKey}/AddFailure")] HttpRequestMessage req,
            [DurableClient] IDurableClient client,
            ILogger log,
            string entityKey)
        {
            if (req == null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            string requestBody = await req.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (string.IsNullOrEmpty(requestBody))
            {
                log.LogError("Received an empty request body.");
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Request body cannot be empty.");
            }
            else
            {
                FailureRequest failureReq = JsonConvert.DeserializeObject<FailureRequest>(requestBody);

                log.LogInformation(
                    "CircuitBreaker AddFailure triggered for {RequestId} with failure time of '{FailureTime}' on instance '{InstanceId}' with specified resource '{ResourceId}'.",
                    failureReq.RequestId,
                    failureReq.FailureTime,
                    failureReq.InstanceId,
                    failureReq.ResourceId);

                var entityId = new EntityId("CircuitBreakerActor", entityKey);
                await client.SignalEntityAsync<ICircuitBreakerActor>(entityId, proxy => proxy.AddFailure(failureReq)).ConfigureAwait(false);
                return req.CreateResponse(HttpStatusCode.Accepted);
            }
        }

        /// <summary>
        /// API to close the circuit.
        /// </summary>
        /// <param name="req">HTTP request message.</param>
        /// <param name="client">Durable Function's client.</param>
        /// <param name="log">ILogger object to log message to Application insights.</param>
        /// <param name="entityKey">Defines the entity (the name of the function app related to this entity).</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [FunctionName("CloseCircuitFunction")]
        public static async Task<HttpResponseMessage> CloseCircuitFunction(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "CircuitBreaker/{entityKey}/Close")] HttpRequestMessage req,
            [DurableClient] IDurableClient client,
            ILogger log,
            string entityKey)
        {
            if (req == null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            log.LogInformation("CircuitBreaker Close triggered for entity {entity}.", entityKey);

            var entityId = new EntityId("CircuitBreakerActor", entityKey);
            await client.SignalEntityAsync<ICircuitBreakerActor>(entityId, proxy => proxy.CloseCircuit()).ConfigureAwait(false);
            return req.CreateResponse(HttpStatusCode.Accepted);
        }
    }
}
