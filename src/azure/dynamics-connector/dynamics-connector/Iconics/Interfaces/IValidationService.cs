// -----------------------------------------------------------------------
// <copyright file="IValidationService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace DynamicsConnector.Iconics.Interfaces
{
    using DynamicsConnector.Iconics.Models;

    /// <summary>
    /// Contains Validation Service Interface information.
    /// </summary>
    public interface IValidationService
    {
        /// <summary>
        /// Method declaration to check if Fault Data is valid. Returns true if valid.
        /// </summary>
        /// <param name="fault">Fault Data.</param>
        /// <param name="validationMessage"> Validation Message content.</param>
        /// <returns>Returns true or false indicating the status of the validation.</returns>
        bool IsValidFault(IconicsFault fault, out string validationMessage);

        /// <summary>
        /// Method declaration to check if Work Order Acknowledgment Data is valid. Returns true if valid.
        /// </summary>
        /// <param name="ack">Work Order Acknowledgment Data.</param>
        /// <param name="validationMessage">Validation Message content.</param>
        /// <returns>Returns true or false indicating the status of the validation.</returns>
        bool IsValidWorkOrder(WorkOrderAck ack, out string validationMessage);
    }
}
