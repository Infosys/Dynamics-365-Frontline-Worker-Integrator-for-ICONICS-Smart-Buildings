// -----------------------------------------------------------------------
// <copyright file="ErrorQueueServiceTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace DynamicsConnectorTests.Unit
{
    using System.Text;
    using DynamicsConnector.Dynamics.Models;
    using DynamicsConnector.Dynamics.Services;
    using Microsoft.Azure.ServiceBus;
    using Moq;
    using Newtonsoft.Json;
    using Xunit;

    /// <summary>
    /// Contains Unit Test methods for Error Queue Service.
    /// </summary>
    public class ErrorQueueServiceTests
    {
        private readonly Mock<IQueueClient> queueClientMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorQueueServiceTests"/> class.
        /// </summary>
        public ErrorQueueServiceTests()
        {
            // mock IQueueClient
            this.queueClientMock = new Mock<IQueueClient>();
        }

        /// <summary>
        /// Send Message To Error Queue Should Send Message With Correct Properties.
        /// </summary>
        [Fact]
        public async void SendMessageToErrorQueueShouldSendMessageWithCorrectProperties()
        {
            // create expected message body
            string messageContents = "messageContents";
            string validationMessage = "validationMessage";
            string expectedMessageBody = JsonConvert.SerializeObject(new ErrorQueueMessage()
            {
                OriginalBody = messageContents,
                ErrorDetails = new ErrorQueueMessageErrorDetails()
                {
                    ErrorMessage = validationMessage,
                },
            });

            // set actualMessageParameter equal to the parameter passed into the SendAsync method
            Message actualMessageParameter = null;
            this.queueClientMock.Setup(queueClient => queueClient.SendAsync(It.IsAny<Message>())).Callback<Message>(message => actualMessageParameter = message);

            // call SendMessageToErrorQueue, which calls SendAsync internally
            ErrorQueueService errorQueueService = new ErrorQueueService(this.queueClientMock.Object);
            await errorQueueService.SendMessageToErrorQueueAsync(messageContents, validationMessage).ConfigureAwait(false);

            // verify that the correct message structure was passed in to the SendAsync method
            Assert.Equal(Encoding.UTF8.GetBytes(expectedMessageBody), actualMessageParameter.Body);
        }
    }
}
