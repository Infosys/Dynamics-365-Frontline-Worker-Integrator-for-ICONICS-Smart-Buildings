// -----------------------------------------------------------------------
// <copyright file="FakeTelemetryChannel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace TestUtils.Utils
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.DataContracts;

    /// <summary>
    /// Fake telemetry channel useful for unit testing.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class FakeTelemetryChannel : ITelemetryChannel
    {
        /// <summary>
        /// Initializes the internal list of telemetry.
        /// </summary>
        private readonly List<ITelemetry> sentTelemetries = new List<ITelemetry>();

        /// <summary>
        /// Gets or sets a value indicating whether the chennel is flushed.
        /// </summary>
        public bool IsFlushed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this channel is in developer mode.
        /// </summary>
        public bool? DeveloperMode { get; set; }

        /// <summary>
        /// Gets or sets the endpoint address of the channel.
        /// </summary>
        public string EndpointAddress { get; set; }

        /// <summary>
        /// Gets the request telemetry sent through the channel.
        /// </summary>
        public IEnumerable<RequestTelemetry> SentRequestTelemetries => this.sentTelemetries.Where(x => x is RequestTelemetry).Cast<RequestTelemetry>();

        /// <summary>
        /// Gets the event telemetry sent through the channel.
        /// </summary>
        public IEnumerable<EventTelemetry> SentEventTelemetries => this.sentTelemetries.Where(x => x is EventTelemetry).Cast<EventTelemetry>();

        /// <summary>
        /// Sends an instance of ITelemetry through the channel.
        /// </summary>
        /// <param name="item">The telemetry to send.</param>
        public void Send(ITelemetry item)
        {
            this.sentTelemetries.Add(item);
        }

        /// <summary>
        /// Flushed the channel.
        /// </summary>
        public void Flush()
        {
            this.IsFlushed = true;
        }

        /// <summary>
        /// Disposes of the resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
