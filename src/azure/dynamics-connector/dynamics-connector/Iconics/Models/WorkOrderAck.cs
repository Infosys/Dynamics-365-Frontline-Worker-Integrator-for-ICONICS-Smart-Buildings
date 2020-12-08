// -----------------------------------------------------------------------
// <copyright file="WorkOrderAck.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace DynamicsConnector.Iconics.Models
{
    /// <summary>
    /// Contains Work Order Acknowledgment properties information.
    /// </summary>
    public class WorkOrderAck
    {
        /// <summary>
        /// Gets or sets original Fault Asset Path.
        /// </summary>
        public string AssetPath { get; set; }

        /// <summary>
        /// Gets or sets original Fault Active Time.
        /// </summary>
        public string FaultActiveTime { get; set; }

        /// <summary>
        /// Gets or sets original Fault Name.
        /// </summary>
        public string FaultName { get; set; }

        /// <summary>
        /// Gets or sets status of the Work Order. Possible Values [Created, Deleted].
        /// </summary>
        public string WorkOrderStatus { get; set; }

        /// <summary>
        /// Gets or sets unique identifier of the Work Order.
        /// </summary>
        public string WorkOrderId { get; set; }

        /// <summary>
        /// Gets or sets deeplink Work Order Record URL.
        /// </summary>
#pragma warning disable CA1056 // Uri properties should not be strings
        public string WorkOrderUrl { get; set; }

#pragma warning restore CA1056 // Uri properties should not be strings

        /// <summary>
        /// Gets or sets work Order Created Date and Time.
        /// </summary>
        public string WorkOrderCreatedOn { get; set; }

        /// <summary>
        /// Gets or sets work Order Modified Date and Time.
        /// </summary>
        public string WorkOrderModifiedOn { get; set; }
    }
}
