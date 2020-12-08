// -----------------------------------------------------------------------
// <copyright file="ErrorQueueService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace DynamicsConnector.Dynamics.Services
{
    using System.Text;
    using System.Threading.Tasks;
    using DynamicsConnector.Dynamics.Interfaces;
    using DynamicsConnector.Dynamics.Models;
    using Microsoft.Azure.ServiceBus;
    using Newtonsoft.Json;

    /// <summary>
    /// Contains Error Queue Service information.
    /// </summary>
    public class ErrorQueueService : IErrorQueueService
    {
        private readonly IQueueClient queueClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorQueueService"/> class.
        /// </summary>
        /// <param name="queueClient">Service Bus Queue Client.</param>
        public ErrorQueueService(IQueueClient queueClient)
        {
            this.queueClient = queueClient;
        }

        /// <summary>
        /// Method definition for sending message to Error Queue.
        /// </summary>
        /// <param name="messageContents">Original Message Content.</param>
        /// <param name="validationMessage">Validation Message.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendMessageToErrorQueueAsync(string messageContents, string validationMessage)
        {
            // create error queue message body
            ErrorQueueMessage errorQueueMessage = new ErrorQueueMessage()
            {
                OriginalBody = messageContents,
                ErrorDetails = new ErrorQueueMessageErrorDetails()
                {
                    ErrorMessage = validationMessage,
                },
            };

            // send the message to the queue
            string messageBody = JsonConvert.SerializeObject(errorQueueMessage);
            var message = new Message(Encoding.UTF8.GetBytes(messageBody));
            await this.queueClient.SendAsync(message).ConfigureAwait(false);
        }
    }
}
