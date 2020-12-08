// -----------------------------------------------------------------------
// <copyright file="CustomerAssetProperties.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace DynamicsConnector.Dynamics.Models
{
    /// <summary>
    /// Customer Asset fields information.
    /// </summary>
    public static class CustomerAssetProperties
    {
        /// <summary>
        /// Name of the Asset.
        /// </summary>
        public const string MsdynName = "msdyn_name";

        /// <summary>
        /// Status of the Customer Asset.
        /// </summary>
        public const string StateCode = "statecode";

        /// <summary>
        /// /// Reason for the status of the Customer Asset.
        /// </summary>
        public const string StatusCode = "statuscode";
    }
}
