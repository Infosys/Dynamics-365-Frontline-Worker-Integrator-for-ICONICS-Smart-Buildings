// -----------------------------------------------------------------------
// <copyright file="CircuitBreakerActor.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace CircuitBreaker.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBreaker.Interfaces;
    using CircuitBreaker.Models;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.DurableTask;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    // Note: Circuit breaker functionality derived from https://github.com/jeffhollan/functions-durable-actor-circuitbreaker.

    /// <summary>
    /// Circuit Breaker Actor Class.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class CircuitBreakerActor : ICircuitBreakerActor
    {
        /// <summary>
        /// Gets or sets the failure window.
        /// </summary>
        [JsonProperty]
#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable SA1401 // Fields should be private
        public IDictionary<string, FailureRequest> FailureWindow = new Dictionary<string, FailureRequest>();
#pragma warning restore SA1401 // Fields should be private
#pragma warning restore CA1051 // Do not declare visible instance fields

        // The TimeSpan difference from latest to keep failures in the window
        private static readonly TimeSpan WindowSize = TimeSpan.Parse(Environment.GetEnvironmentVariable("WindowSize") ?? "00:01:00", null);

        // The number of failures in the window until opening the circuit
        private static readonly int FailureThreshold = int.Parse(Environment.GetEnvironmentVariable("FailureThreshold") ?? "20", null);

        private static string entityKey;

        private readonly ILogger log;
        private readonly IDurableClient durableClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitBreakerActor"/> class.
        /// </summary>
        /// <param name="client">Client context.</param>
        /// <param name="log">ILogger object to log message to Application insights.</param>
        public CircuitBreakerActor(IDurableClient client, ILogger log)
        {
            this.durableClient = client;
            this.log = log;
        }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public CircuitState State { get; set; }

        /// <summary>
        /// Azure function main method.
        /// </summary>
        /// <param name="ctx">Context.</param>
        /// <param name="client">Client.</param>
        /// <param name="log">ILogger object to log message to Application insights.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [FunctionName(nameof(CircuitBreakerActor))]
        public static Task Run(
            [EntityTrigger] IDurableEntityContext ctx,
            [DurableClient] IDurableClient client,
            ILogger log)
        {
            entityKey = Entity.Current.EntityKey;
            return ctx.DispatchAsync<CircuitBreakerActor>(client, log);
        }

        /// <summary>
        /// Close the Circuit.
        /// </summary>
        public void CloseCircuit() => this.State = CircuitState.Closed;

        /// <summary>
        /// Open the Circuit.
        /// </summary>
        public void OpenCircuit() => this.State = CircuitState.Open;

        /// <summary>
        /// Task to Add Failure.
        /// </summary>
        /// <param name="req">FailureRequest.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task AddFailure(FailureRequest req)
        {
            if (req == null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            if (this.State == CircuitState.Open)
            {
                this.log.LogWarning("Tried to add additional failure to {entityKey} that is already open.", entityKey);
                return;
            }

            this.FailureWindow.Add(req.RequestId, req);

            var cutoff = req.FailureTime.Subtract(WindowSize);

            // Filter the window only to exceptions within the cutoff timespan
            this.FailureWindow = this.FailureWindow.Where(p => p.Value.FailureTime >= cutoff).ToDictionary(p => p.Key, p => p.Value);

            if (this.FailureWindow.Count >= FailureThreshold)
            {
                this.log.LogCritical("Break this circuit for entity {entityKey}!", entityKey);

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
                string instanceId = await this.durableClient.StartNewAsync(nameof(OpenCircuitOrchestrator.OpenCircuit), req);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

                this.log.LogInformation("Started {orchestration} with instance ID {instanceId}.", nameof(OpenCircuitOrchestrator.OpenCircuit), instanceId);

                // Mark the circuit as "open" (circuit is broken)
                this.State = CircuitState.Open;
            }
            else
            {
                this.log.LogInformation(
                    "The circuit {entityKey} currently has {Count} exceptions in the window of {WindowSize}.",
                    entityKey,
                    this.FailureWindow.Count,
                    WindowSize);
            }
        }
    }
}
