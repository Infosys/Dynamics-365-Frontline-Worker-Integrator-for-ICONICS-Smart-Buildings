// -----------------------------------------------------------------------
// <copyright file="ErrorQueueMessage.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace DynamicsConnector.Dynamics.Models
{
    /// <summary>
    /// Define structure of the Error Queue Message.
    /// </summary>
    public class ErrorQueueMessage
    {
        /// <summary>
        /// Gets or sets original Fault Data.
        /// </summary>
        public string OriginalBody { get; set; }

        /// <summary>
        /// Gets or sets ErrorDetails.
        /// </summary>
        public ErrorQueueMessageErrorDetails ErrorDetails { get; set; }
    }
}
