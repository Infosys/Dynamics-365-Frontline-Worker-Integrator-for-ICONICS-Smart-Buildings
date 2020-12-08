// -----------------------------------------------------------------------
// <copyright file="FailureRequest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace CircuitBreaker.Models
{
    using System;

    /// <summary>
    /// Circuit Breaker Failure Information.
    /// </summary>
    public class FailureRequest
    {
        /// <summary>
        /// Gets or sets the Request ID.
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Gets or sets the Failure Time.
        /// </summary>
        public DateTime FailureTime { get; set; }

        /// <summary>
        /// Gets or sets the Instance ID.
        /// </summary>
        public string InstanceId { get; set; }

        /// <summary>
        /// Gets or sets the Resource ID.
        /// </summary>
        public string ResourceId { get; set; }
    }
}