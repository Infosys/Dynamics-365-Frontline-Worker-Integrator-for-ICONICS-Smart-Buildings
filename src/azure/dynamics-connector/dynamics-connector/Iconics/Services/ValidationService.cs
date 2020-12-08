// -----------------------------------------------------------------------
// <copyright file="ValidationService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace DynamicsConnector.Iconics.Services
{
    using DynamicsConnector.Iconics.Interfaces;
    using DynamicsConnector.Iconics.Models;
    using DynamicsConnector.Properties;

    /// <summary>
    /// Contains Validation Service information.
    /// </summary>
    public class ValidationService : IValidationService
    {
        /// <summary>
        /// Method definition to check if Fault Data is valid. Returns true if valid.
        /// </summary>
        /// <param name="iconicsFault">Fault Data.</param>
        /// <param name="validationMessage"> Validation Message content.</param>
        /// <returns>Returns true or false indicating the status of the validation.</returns>
        public bool IsValidFault(IconicsFault iconicsFault, out string validationMessage)
        {
            bool isValid = true;
            validationMessage = string.Empty;
            if (iconicsFault != null)
            {
                if (string.IsNullOrEmpty(iconicsFault.FaultName) || string.IsNullOrEmpty(iconicsFault.AssetName) || string.IsNullOrEmpty(iconicsFault.AssetPath))
                {
                    validationMessage = $"One or more required fields were not provided. Fault Name: {iconicsFault.FaultName}. Asset Name: {iconicsFault.AssetName}. Asset Path: {iconicsFault.AssetPath}.";
                    isValid = false;
                }
            }
            else
            {
                validationMessage = Resources.SBTopicMsgNull;
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// Method definition to check if Work Order Acknowledgment Data is valid. Returns true if valid.
        /// </summary>
        /// <param name="ack">Work Order Acknowledgment Data.</param>
        /// <param name="validationMessage">Validation Message content.</param>
        /// <returns>Returns true or false indicating the status of the validation.</returns>
        public bool IsValidWorkOrder(WorkOrderAck ack, out string validationMessage)
        {
            bool isValid = true;
            validationMessage = string.Empty;
            if (ack != null)
            {
                if (string.IsNullOrEmpty(ack.AssetPath) || string.IsNullOrEmpty(ack.FaultActiveTime) || string.IsNullOrEmpty(ack.FaultName) || string.IsNullOrEmpty(ack.WorkOrderStatus) || string.IsNullOrEmpty(ack.WorkOrderId) || string.IsNullOrEmpty(ack.WorkOrderUrl))
                {
                    validationMessage = $"One or more required fields were not provided. Asset Path: {ack.AssetPath}. Fault Active Time: {ack.FaultActiveTime}. Fault Name: {ack.FaultName}. Work Order Status: {ack.WorkOrderStatus}. Work Order ID: {ack.WorkOrderId} . Work Order Url: {ack.WorkOrderUrl}.";
                    isValid = false;
                }
            }
            else
            {
                validationMessage = Resources.SBQueueMsgNull;
                isValid = false;
            }

            return isValid;
        }
    }
}
