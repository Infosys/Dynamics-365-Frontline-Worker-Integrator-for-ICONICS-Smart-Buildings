// -----------------------------------------------------------------------
// <copyright file="SendWorkOrderAckTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace DynamicsConnectorTests.Unit
{
    using System.Text;
    using DynamicsConnector.Dynamics.Interfaces;
    using DynamicsConnector.Functions;
    using DynamicsConnector.Iconics.Interfaces;
    using DynamicsConnector.Iconics.Models;
    using Microsoft.Azure.ServiceBus;
    using Moq;
    using Newtonsoft.Json;
    using TestUtils.Factories;
    using TestUtils.Utils;
    using Xunit;

    /// <summary>
    /// Contains Unit Test method for SendWorkOrderAckTests.
    /// </summary>
    public class SendWorkOrderAckTests
    {
        private readonly Mock<IIconicsService> iconicsServiceMock;
        private readonly Mock<IValidationService> validationServiceMock;
        private readonly Mock<IErrorQueueService> errorQueueServiceMock;
        private readonly ListLogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendWorkOrderAckTests"/> class.
        /// </summary>
        public SendWorkOrderAckTests()
        {
            // mock services passed to Function class constructor
            this.iconicsServiceMock = new Mock<IIconicsService>();
            this.validationServiceMock = new Mock<IValidationService>();
            this.errorQueueServiceMock = new Mock<IErrorQueueService>();
            this.logger = LoggerFactory.CreateLogger();
        }

        /// <summary>
        /// Should Run Successfully On Valid Ack.
        /// </summary>
        [Fact]
        public async void ShouldRunSuccessfullyOnValidAck()
        {
            // mock IsValidWorkOrder calls to return true
            string validationMessage;
            string actualMessageParameter = null;
            this.validationServiceMock.Setup(service => service.IsValidWorkOrder(It.IsAny<WorkOrderAck>(), out validationMessage)).Returns(true);
            this.iconicsServiceMock.Setup(service => service.SendWorkOrderMessageAsync(It.IsAny<string>())).Callback<string>(workOrderMessage => actualMessageParameter = workOrderMessage);

            // create message to pass to Run method
            var workOrderAck = new
            {
                AssetPath = "AssetPath",
                FaultActiveTime = "FaultActiveTime",
                FaultName = "FaultActiveTime",
                WorkOrderStatus = "WorkOrderStatus",
                WorkOrderId = "WorkOrderId",
                WorkOrderUrl = "WorkOrderUrl",
            };
            string messageBody = JsonConvert.SerializeObject(workOrderAck);
            Message message = new Message(Encoding.UTF8.GetBytes(messageBody));

            // instantiate function class and run function
            SendWorkOrderAck function = new SendWorkOrderAck(this.iconicsServiceMock.Object, this.validationServiceMock.Object, this.errorQueueServiceMock.Object);
            await function.Run(message, this.logger).ConfigureAwait(false);

            // verify that SendWorkOrderMessageAsync sent a string message
            this.iconicsServiceMock.Verify(service => service.SendWorkOrderMessageAsync(It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// Should Run Unsuccessfully On Invalid Ack.
        /// </summary>
        [Fact]
        public async void ShouldRunUnsuccessfullyOnInvalidAck()
        {
            // mock calls to return true
            string validationMessage;
            this.validationServiceMock.Setup(service => service.IsValidWorkOrder(It.IsAny<WorkOrderAck>(), out validationMessage)).Returns(false);

            // create message to pass to Run method
            WorkOrderAck workOrderAck = new WorkOrderAck()
            {
                AssetPath = "AssetPath",
                FaultActiveTime = "FaultActiveTime",
                FaultName = "FaultName",
                WorkOrderStatus = "WorkOrderStatus",
                WorkOrderId = "WorkOrderId",
                WorkOrderUrl = "WorkOrderUrl",
                WorkOrderCreatedOn = "WorkOrderCreatedOn",
                WorkOrderModifiedOn = "WorkOrderModifiedOn",
            };

            string messageBody = JsonConvert.SerializeObject(workOrderAck);
            Message message = new Message(Encoding.UTF8.GetBytes(messageBody));

            // set actualMessageContents equal to the first parameter passed into the SendMessageToErrorQueue method which should be called within the Run method
            string actualMessageContents = null;
            this.errorQueueServiceMock.Setup(service => service.SendMessageToErrorQueueAsync(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((messageContents, validationMessage) => actualMessageContents = messageContents);

            // instantiate function class and run function
            SendWorkOrderAck function = new SendWorkOrderAck(this.iconicsServiceMock.Object, this.validationServiceMock.Object, this.errorQueueServiceMock.Object);
            await function.Run(message, this.logger).ConfigureAwait(false);

            // verify that SendMessageToErrorQueue was called and received the correct messageContents parameter
            string expectedErrorText = "Work Order ack data failed validation. Validation Message:";
            this.errorQueueServiceMock.Verify(service => service.SendMessageToErrorQueueAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.Equal(Encoding.UTF8.GetString(message.Body), actualMessageContents);
            Assert.Contains(expectedErrorText, this.logger.Logs[1], System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Should Run Unsuccessfully On Invalid Service Bus Message.
        /// </summary>
        [Fact]
        public async void ShouldRunUnsuccessfullyOnInvalidServiceBusMessage()
        {
            // create message to pass to Run method that will cause deserialization error
            dynamic workOrderAck = new
            {
                WorkOrderId = new int[] { 10, 20 },
            };

            string messageBody = JsonConvert.SerializeObject(workOrderAck);
            Message message = new Message(Encoding.UTF8.GetBytes(messageBody));

            // set actualMessageContents equal to the first parameter passed into the SendMessageToErrorQueue method which should be called within the Run method
            string actualMessageContents = null;
            this.errorQueueServiceMock.Setup(service => service.SendMessageToErrorQueueAsync(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((messageContents, validationMessage) => actualMessageContents = messageContents);

            // instantiate function class and run function
            SendWorkOrderAck function = new SendWorkOrderAck(this.iconicsServiceMock.Object, this.validationServiceMock.Object, this.errorQueueServiceMock.Object);
            await function.Run(message, this.logger).ConfigureAwait(false);

            // verify that SendMessageToErrorQueue received the correct messageContents parameter and that the correct error is logged
            string expectedErrorText = "Exception while processing message";
            this.errorQueueServiceMock.Verify(service => service.SendMessageToErrorQueueAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.Equal(Encoding.UTF8.GetString(message.Body), actualMessageContents);
            Assert.Contains(expectedErrorText, this.logger.Logs[0], System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
