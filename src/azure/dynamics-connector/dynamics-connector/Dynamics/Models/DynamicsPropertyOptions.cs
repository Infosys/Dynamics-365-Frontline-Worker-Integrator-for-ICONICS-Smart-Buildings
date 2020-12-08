// -----------------------------------------------------------------------
// <copyright file="DynamicsPropertyOptions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace DynamicsConnector.Dynamics.Models
{
    /// <summary>
    /// Alert Type Optionset Text and Value information.
    /// </summary>
    public enum IotAlertAlertType
    {
        /// <summary>
        /// Anomaly Alert Type
        /// </summary>
        Anomaly = 192350000,

        /// <summary>
        /// Info Alert Type
        /// </summary>
        Info = 192350001,

        /// <summary>
        /// PreventiveMaintenance Alert Type
        /// </summary>
        PreventiveMaintenance = 192350002,

        /// <summary>
        /// Test Alert Type
        /// </summary>
        Test = 192350003,
    }

    /// <summary>
    /// State Code Optionset Text and Value information.
    /// </summary>
    public enum IotAlertStateCode
    {
        /// <summary>
        /// Active State Code
        /// </summary>
        Active = 0,

        /// <summary>
        /// Inactive State Code
        /// </summary>
        Inactive = 1,

        /// <summary>
        /// InProgress State Code
        /// </summary>
        InProgress = 2,

        /// <summary>
        /// Closed State Code
        /// </summary>
        Closed = 3,
    }

    /// <summary>
    /// Status Code Optionset Text and Value information.
    /// </summary>
    public enum IotAlertStatusCode
    {
        /// <summary>
        /// Active Status Code
        /// </summary>
        Active = 1,

        /// <summary>
        /// Inactive Status Code
        /// </summary>
        Inactive = 2,

        /// <summary>
        /// InProgressCaseCreated Status Code
        /// </summary>
        InProgressCaseCreated = 3,

        /// <summary>
        /// InProgressWorkOrderCreated Status Code
        /// </summary>
        InProgressWorkOrderCreated = 4,

        /// <summary>
        /// InProgressCommandSent Status Code
        /// </summary>
        InProgressCommandSent = 5,

        /// <summary>
        /// Closed Status Code
        /// </summary>
        Closed = 6,
    }

    /// <summary>
    /// RegistrationStatus Optionset Text and Value information.
    /// </summary>
    public enum CustomerAssetRegistrationStatus
    {
        /// <summary>
        /// Unknown RegistrationStatus
        /// </summary>
        Unknown = 192350000,

        /// <summary>
        /// Unregistered RegistrationStatus
        /// </summary>
        Unregistered = 1923500001,

        /// <summary>
        /// InProgress RegistrationStatus
        /// </summary>
        InProgress = 192350002,

        /// <summary>
        /// Registered RegistrationStatus
        /// </summary>
        Registered = 192350003,

        /// <summary>
        /// Error RegistrationStatus
        /// </summary>
        Error = 192350004,
    }

    /// <summary>
    /// CustomerAssetStateCode Optionset Text and Value information.
    /// </summary>
    public enum CustomerAssetStateCode
    {
        /// <summary>
        /// Active AssetStateCode
        /// </summary>
        Active = 0,

        /// <summary>
        /// Inactive AssetStateCode
        /// </summary>
        Inactive = 1,
    }

    /// <summary>
    /// CustomerAssetStatusCode Optionset Text and Value information.
    /// </summary>
    public enum CustomerAssetStatusCode
    {
        /// <summary>
        /// Active AssetStatusCode
        /// </summary>
        Active = 1,

        /// <summary>
        /// Inactive AssetStatusCode
        /// </summary>
        Inactive = 2,
    }
}
