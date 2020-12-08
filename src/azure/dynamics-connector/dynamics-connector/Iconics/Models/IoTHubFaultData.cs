// -----------------------------------------------------------------------
// <copyright file="IoTHubFaultData.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace DynamicsConnector.Iconics.Models
{
    /// <summary>
    /// Contains IoTHubFaultData properties.
    /// </summary>
    public class IoTHubFaultData
    {
        /// <summary>
        /// Gets or sets MessageId.
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets CorrelationId.
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets ConnectionDeviceId.
        /// </summary>
        public string ConnectionDeviceId { get; set; }

        /// <summary>
        /// Gets or sets ConnectionDeviceGenerationId.
        /// </summary>
        public string ConnectionDeviceGenerationId { get; set; }

        /// <summary>
        /// Gets or sets EnqueuedTime.
        /// </summary>
        public string EnqueuedTime { get; set; }
    }
}
