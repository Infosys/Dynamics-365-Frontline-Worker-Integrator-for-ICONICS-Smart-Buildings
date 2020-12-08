// -----------------------------------------------------------------------
// <copyright file="ValidationServiceTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace DynamicsConnectorTests.Unit
{
    using DynamicsConnector.Iconics.Models;
    using DynamicsConnector.Iconics.Services;
    using Xunit;

    /// <summary>
    /// Contains Unit Test methods for Validation Service.
    /// </summary>
    public class ValidationServiceTests
    {
        /// <summary>
        /// Validate Iconics Fault Should Return Null If Valid.
        /// </summary>
        [Fact]
        public void ValidateIconicsFaultShouldReturnNullIfValid()
        {
            ValidationService validationService = new ValidationService();
            IconicsFault iconicsFault = new IconicsFault()
            {
                AssetName = "AssetName",
                AssetPath = "AssetPath",
                FaultName = "FaultName",
                FaultActiveTime = "FaultActiveTime",
                FaultCostValue = "FaultCostValue",
                RelatedValue1 = "RelatedValue1",
                RelatedValue2 = "RelatedValue2",
                RelatedValue3 = "RelatedValue3",
                RelatedValue4 = "RelatedValue4",
                RelatedValue5 = "RelatedValue5",
                RelatedValue6 = "RelatedValue6",
                RelatedValue7 = "RelatedValue7",
                RelatedValue8 = "RelatedValue8",
                RelatedValue9 = "RelatedValue9",
                RelatedValue10 = "RelatedValue10",
                RelatedValue11 = "RelatedValue11",
                RelatedValue12 = "RelatedValue12",
                RelatedValue13 = "RelatedValue13",
                RelatedValue14 = "RelatedValue14",
                RelatedValue15 = "RelatedValue15",
                RelatedValue16 = "RelatedValue16",
                RelatedValue17 = "RelatedValue17",
                RelatedValue18 = "RelatedValue18",
                RelatedValue19 = "RelatedValue19",
                RelatedValue20 = "RelatedValue20",
                MessageSource = "MessageSource",
                Description = "Description",
                EventProcessedUtcTime = "EventProcessedUtcTime",
                PartitionId = 1,
                EventEnqueuedUtcTime = "EventEnqueuedUtcTime",
                IoTHub = new IoTHubFaultData()
                {
                    MessageId = "MessageId",
                    CorrelationId = "CorrelationId",
                    ConnectionDeviceGenerationId = "ConnectionDeviceGenerationId",
                    ConnectionDeviceId = "ConnectionDeviceId",
                    EnqueuedTime = "EnqueuedTime",
                },
            };
            string errorMessage;
            bool isValid = validationService.IsValidFault(iconicsFault, out errorMessage);
            Assert.True(isValid);
        }

        /// <summary>
        /// Validate Iconics Fault Should Return False If Fault Is Null.
        /// </summary>
        [Fact]
        public void ValidateWorkOrderAckShouldReturnFalseIfFaultIsNull()
        {
            string validationMessage = string.Empty;
            ValidationService validationService = new ValidationService();
            IconicsFault iconicsFault = null;
            string errorMessage;
            string expectedMessage = "Service Bus Topic message is null.";
            bool isValid = validationService.IsValidFault(iconicsFault, out errorMessage);
            Assert.Equal(expectedMessage, errorMessage);
            Assert.False(isValid);
        }

        /// <summary>
        /// Validate Iconics Fault Should Return Message If Invalid Asset Name.
        /// </summary>
        [Fact]
        public void ValidateIconicsFaultShouldReturnMessageIfInvalidAssetName()
        {
            ValidationService validationService = new ValidationService();
            IconicsFault iconicsFault = new IconicsFault()
            {
                FaultName = "FaultName",
                AssetPath = "AssetPath",
            };
            string errorMessage;
            bool isValid = validationService.IsValidFault(iconicsFault, out errorMessage);
            string expectedMessage = $"One or more required fields were not provided. Fault Name: {iconicsFault.FaultName}. Asset Name: {iconicsFault.AssetName}. Asset Path: {iconicsFault.AssetPath}.";
            Assert.Equal(expectedMessage, errorMessage);
            Assert.False(isValid);
        }

        /// <summary>
        /// Validate Iconics Fault Should Return Message If Invalid Fault Name.
        /// </summary>
        [Fact]
        public void ValidateIconicsFaultShouldReturnMessageIfInvalidFaultName()
        {
            ValidationService validationService = new ValidationService();
            IconicsFault iconicsFault = new IconicsFault()
            {
                AssetName = "AssetName",
                AssetPath = "AssetPath",
            };
            string errorMessage;
            bool isValid = validationService.IsValidFault(iconicsFault, out errorMessage);
            string expectedMessage = $"One or more required fields were not provided. Fault Name: {iconicsFault.FaultName}. Asset Name: {iconicsFault.AssetName}. Asset Path: {iconicsFault.AssetPath}.";
            Assert.Equal(expectedMessage, errorMessage);
            Assert.False(isValid);
        }

        /// <summary>
        /// Validate Iconics Fault Should Return Message If Invalid Asset Path.
        /// </summary>
        [Fact]
        public void ValidateIconicsFaultShouldReturnMessageIfInvalidAssetPath()
        {
            ValidationService validationService = new ValidationService();
            IconicsFault iconicsFault = new IconicsFault()
            {
                FaultName = "FaultName",
                AssetName = "AssetName",
            };
            string errorMessage;
            bool isValid = validationService.IsValidFault(iconicsFault, out errorMessage);
            string expectedMessage = $"One or more required fields were not provided. Fault Name: {iconicsFault.FaultName}. Asset Name: {iconicsFault.AssetName}. Asset Path: {iconicsFault.AssetPath}.";
            Assert.Equal(expectedMessage, errorMessage);
            Assert.False(isValid);
        }

        /// <summary>
        /// Validate Work Order Acknowledgment Should Return Null If Valid.
        /// </summary>
        [Fact]
        public void ValidateWorkOrderAckShouldReturnNullIfValid()
        {
            ValidationService validationService = new ValidationService();
            WorkOrderAck workOrderAck = new WorkOrderAck()
            {
                AssetPath = "AssetPath",
                FaultActiveTime = "FaultActiveTime",
                FaultName = "FaultName",
                WorkOrderStatus = "WorkOrderStatus",
                WorkOrderId = "WorkOrderId",
                WorkOrderUrl = "WorkOrderUrl",
            };
            string errorMessage;
            bool isValid = validationService.IsValidWorkOrder(workOrderAck, out errorMessage);
            Assert.True(isValid);
        }

        /// <summary>
        /// Validate Work Order Acknowledgement Should Return False If Ack Is Null.
        /// </summary>
        [Fact]
        public void ValidateWorkOrderAckShouldReturnFalseIfAckIsNull()
        {
            string validationMessage = string.Empty;
            ValidationService validationService = new ValidationService();
            WorkOrderAck workOrderAck = null;
            string errorMessage;
            string expectedMessage = "Service Bus Queue message is null";
            bool isValid = validationService.IsValidWorkOrder(workOrderAck, out errorMessage);
            Assert.Equal(expectedMessage, errorMessage);
            Assert.False(isValid);
        }

        /// <summary>
        /// Validate Work Order Acknowledgment Should Return Message If Invalid Asset Path.
        /// </summary>
        [Fact]
        public void ValidateWorkOrderAckShouldReturnMessageIfInvalidAssetPath()
        {
            ValidationService validationService = new ValidationService();
            WorkOrderAck workOrderAck = new WorkOrderAck()
            {
                FaultActiveTime = "FaultActiveTime",
                FaultName = "FaultName",
                WorkOrderStatus = "WorkOrderStatus",
                WorkOrderId = "WorkOrderId",
                WorkOrderUrl = "WorkOrderUrl",
            };
            string errorMessage;
            bool isValid = validationService.IsValidWorkOrder(workOrderAck, out errorMessage);
            string expectedMessage = $"One or more required fields were not provided. Asset Path: {workOrderAck.AssetPath}. Fault Active Time: {workOrderAck.FaultActiveTime}. Fault Name: {workOrderAck.FaultName}. Work Order Status: {workOrderAck.WorkOrderStatus}. Work Order ID: {workOrderAck.WorkOrderId} . Work Order Url: {workOrderAck.WorkOrderUrl}.";
            Assert.Equal(expectedMessage, errorMessage);
            Assert.False(isValid);
        }

        /// <summary>
        /// Validate Work Order Acknowledgment Should Return Message If Invalid Fault Active Time.
        /// </summary>
        [Fact]
        public void ValidateWorkOrderAckShouldReturnMessageIfInvalidFaultActiveTime()
        {
            ValidationService validationService = new ValidationService();
            WorkOrderAck workOrderAck = new WorkOrderAck()
            {
                AssetPath = "AssetPath",
                FaultName = "FaultName",
                WorkOrderStatus = "WorkOrderStatus",
                WorkOrderId = "WorkOrderId",
                WorkOrderUrl = "WorkOrderUrl",
            };
            string errorMessage;
            bool isValid = validationService.IsValidWorkOrder(workOrderAck, out errorMessage);
            string expectedMessage = $"One or more required fields were not provided. Asset Path: {workOrderAck.AssetPath}. Fault Active Time: {workOrderAck.FaultActiveTime}. Fault Name: {workOrderAck.FaultName}. Work Order Status: {workOrderAck.WorkOrderStatus}. Work Order ID: {workOrderAck.WorkOrderId} . Work Order Url: {workOrderAck.WorkOrderUrl}.";
            Assert.Equal(expectedMessage, errorMessage);
            Assert.False(isValid);
        }

        /// <summary>
        /// Validate Work Order Acknowledgment Should Return Message If Invalid Fault Name.
        /// </summary>
        [Fact]
        public void ValidateWorkOrderAckShouldReturnMessageIfInvalidFaultName()
        {
            ValidationService validationService = new ValidationService();
            WorkOrderAck workOrderAck = new WorkOrderAck()
            {
                AssetPath = "AssetPath",
                FaultActiveTime = "FaultActiveTime",
                WorkOrderStatus = "WorkOrderStatus",
                WorkOrderId = "WorkOrderId",
                WorkOrderUrl = "WorkOrderUrl",
            };
            string errorMessage;
            bool isValid = validationService.IsValidWorkOrder(workOrderAck, out errorMessage);
            string expectedMessage = $"One or more required fields were not provided. Asset Path: {workOrderAck.AssetPath}. Fault Active Time: {workOrderAck.FaultActiveTime}. Fault Name: {workOrderAck.FaultName}. Work Order Status: {workOrderAck.WorkOrderStatus}. Work Order ID: {workOrderAck.WorkOrderId} . Work Order Url: {workOrderAck.WorkOrderUrl}.";
            Assert.Equal(expectedMessage, errorMessage);
            Assert.False(isValid);
        }

        /// <summary>
        /// Validate Work Order Acknowledgment Should Return Message If Invalid Work Order Status.
        /// </summary>
        [Fact]
        public void ValidateWorkOrderAckShouldReturnMessageIfInvalidWorkOrderStatus()
        {
            ValidationService validationService = new ValidationService();
            WorkOrderAck workOrderAck = new WorkOrderAck()
            {
                AssetPath = "AssetPath",
                FaultActiveTime = "FaultActiveTime",
                FaultName = "FaultName",
                WorkOrderId = "WorkOrderId",
                WorkOrderUrl = "WorkOrderUrl",
            };
            string errorMessage;
            bool isValid = validationService.IsValidWorkOrder(workOrderAck, out errorMessage);
            string expectedMessage = $"One or more required fields were not provided. Asset Path: {workOrderAck.AssetPath}. Fault Active Time: {workOrderAck.FaultActiveTime}. Fault Name: {workOrderAck.FaultName}. Work Order Status: {workOrderAck.WorkOrderStatus}. Work Order ID: {workOrderAck.WorkOrderId} . Work Order Url: {workOrderAck.WorkOrderUrl}.";
            Assert.Equal(expectedMessage, errorMessage);
            Assert.False(isValid);
        }

        /// <summary>
        /// Validate Work Order Acknowledgment Should Return Message If Invalid Work Order Id.
        /// </summary>
        [Fact]
        public void ValidateWorkOrderAckShouldReturnMessageIfInvalidWorkOrderId()
        {
            ValidationService validationService = new ValidationService();
            WorkOrderAck workOrderAck = new WorkOrderAck()
            {
                AssetPath = "AssetPath",
                FaultActiveTime = "FaultActiveTime",
                FaultName = "FaultName",
                WorkOrderStatus = "WorkOrderStatus",
                WorkOrderUrl = "WorkOrderUrl",
            };
            string errorMessage;
            bool isValid = validationService.IsValidWorkOrder(workOrderAck, out errorMessage);
            string expectedMessage = $"One or more required fields were not provided. Asset Path: {workOrderAck.AssetPath}. Fault Active Time: {workOrderAck.FaultActiveTime}. Fault Name: {workOrderAck.FaultName}. Work Order Status: {workOrderAck.WorkOrderStatus}. Work Order ID: {workOrderAck.WorkOrderId} . Work Order Url: {workOrderAck.WorkOrderUrl}.";
            Assert.Equal(expectedMessage, errorMessage);
            Assert.False(isValid);
        }

        /// <summary>
        /// Validate Work Order Acknowledgment Should Return Message If Invalid Work Order Url.
        /// </summary>
        [Fact]
        public void ValidateWorkOrderAckShouldReturnMessageIfInvalidWorkOrderUrl()
        {
            ValidationService validationService = new ValidationService();
            WorkOrderAck workOrderAck = new WorkOrderAck()
            {
                AssetPath = "AssetPath",
                FaultActiveTime = "FaultActiveTime",
                FaultName = "FaultName",
                WorkOrderStatus = "WorkOrderStatus",
            };
            string errorMessage;
            bool isValid = validationService.IsValidWorkOrder(workOrderAck, out errorMessage);
            string expectedMessage = $"One or more required fields were not provided. Asset Path: {workOrderAck.AssetPath}. Fault Active Time: {workOrderAck.FaultActiveTime}. Fault Name: {workOrderAck.FaultName}. Work Order Status: {workOrderAck.WorkOrderStatus}. Work Order ID: {workOrderAck.WorkOrderId} . Work Order Url: {workOrderAck.WorkOrderUrl}.";
            Assert.Equal(expectedMessage, errorMessage);
            Assert.False(isValid);
        }
    }
}
