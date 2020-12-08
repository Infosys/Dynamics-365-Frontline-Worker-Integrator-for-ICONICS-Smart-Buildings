// -----------------------------------------------------------------------
// <copyright file="IoTAlertProperties.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace DynamicsConnector.Dynamics.Models
{
    /// <summary>
    /// IoT Alert fields information.
    /// </summary>
    public static class IoTAlertProperties
    {
        /// <summary>
        /// Data sent from the device about the alert.
        /// </summary>
        public const string MsdynAlertData = "msdyn_alertdata";

        /// <summary>
        /// The time the alert was issued.
        /// </summary>
        public const string MsdynAlertTime = "msdyn_alerttime";

        /// <summary>
        /// The type of the Alert was issued.
        /// </summary>
        public const string MsdynAlertType = "msdyn_alerttype";

        /// <summary>
        /// A description for the alert.
        /// </summary>
        public const string MsdynDescription = "msdyn_description";

        /// <summary>
        /// Status of the IoT Alert.
        /// </summary>
        public const string StateCode = "statecode";

        /// <summary>
        /// Reason for the status of the IoT Alert.
        /// </summary>
        public const string StatusCode = "statuscode";

        /// <summary>
        /// The unique reference to the event id on the IoT provider.
        /// </summary>
        public const string MsdynAlertToken = "msdyn_alerttoken";
    }
}
