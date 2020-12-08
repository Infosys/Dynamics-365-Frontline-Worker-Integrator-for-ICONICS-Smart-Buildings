// -----------------------------------------------------------------------
// <copyright file="IDynamicsEntityService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace DynamicsConnector.Dynamics.Interfaces
{
    using System;
    using DynamicsConnector.Iconics.Models;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Interface for Dynamics Entity Service.
    /// </summary>
    public interface IDynamicsEntityService
    {
        /// <summary>
        /// Method declaration for creating an IoT Alert in Dynamics.
        /// </summary>
        /// <param name="iconicsFault">Fault Data.</param>
        /// <param name="assetId">Customer Asset ID.</param>
        /// <param name="log">Log Content.</param>
        /// <returns>IoT Alert record GUID.</returns>
        Guid CreateAlert(IconicsFault iconicsFault, Guid assetId, ILogger log);

        /// <summary>
        /// Method declaration for creating an Asset if not exist in Dynamics.
        /// </summary>
        /// <param name="iconicsFault">Fault Data.</param>
        /// <param name="log">Log Content.</param>
        /// <returns>Customer Asset record GUID.</returns>
        Guid CreateAssetIfNotExists(IconicsFault iconicsFault, ILogger log);

        /// <summary>
        /// Method declaration for getting previous IoT Alert based on Alert Token Hash Value.
        /// </summary>
        /// <param name="iconicsFault">Fault Data.</param>
        /// <param name="log">Log Content.</param>
        /// <returns>An existing IoT Alert record GUID.</returns>
        Guid GetIoTAlert(IconicsFault iconicsFault, ILogger log);

        /// <summary>
        /// Update existing IoT Alert to set status to InActive and update latest fault data.
        /// </summary>
        /// <param name="fault">Fault Data.</param>
        /// <param name="log">Log Content.</param>
        /// <param name="alertState">IoT Alert State Code.</param>
        /// <param name="alertStatus">IoT Alert Status Code.</param>
        void UpdateIoTAlert(IconicsFault fault, ILogger log, int alertState = 0, int alertStatus = 0);

        /// <summary>
        /// Method declaration to check if CDS client service is ready.
        /// </summary>
        /// <returns>Status(true|false) of CDS Client Service.</returns>
        bool IsCdsServiceReady();

        /// <summary>
        /// Method declaration to retrieve CDS Error.
        /// </summary>
        /// <returns>CDS Service LastError.</returns>
        string RetrieveCdsError();
    }
}
