// -----------------------------------------------------------------------
// <copyright file="SendWorkOrderAck.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace DynamicsConnector.Functions
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using DynamicsConnector.Dynamics.Interfaces;
    using DynamicsConnector.Iconics.Interfaces;
    using DynamicsConnector.Iconics.Models;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// Azure function to send Work Order Acknowledgment to Cloud-to-Device.
    /// </summary>
    public class SendWorkOrderAck
    {
        private readonly IIconicsService iconicsService;
        private readonly IValidationService validationService;
        private readonly IErrorQueueService errorQueueService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendWorkOrderAck"/> class.
        /// </summary>
        /// <param name="iconicsService">set iconicsService via dependency injection.</param>
        /// <param name="validationService">set validationService via dependency injection.</param>
        /// <param name="errorQueueService">set errorQueueService via dependency injection.</param>
        public SendWorkOrderAck(IIconicsService iconicsService, IValidationService validationService, IErrorQueueService errorQueueService)
        {
            this.iconicsService = iconicsService;
            this.validationService = validationService;
            this.errorQueueService = errorQueueService;
        }

        /// <summary>
        /// Azure function main method.
        /// </summary>
        /// <param name="queueMessage">Queue message content.</param>
        /// <param name="log">ILogger object to log message to Application insights.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [FunctionName("SendWorkOrderAck")]
        public async Task Run([ServiceBusTrigger("%WorkOrderCreateQueueName%", Connection = "ServiceBusConnection")]Message queueMessage, ILogger log)
        {
            WorkOrderAck workOrderAck = null;
            string messageContents = null;

            try
            {
                if (queueMessage != null)
                {
                    // Deserialize message contents into work order ack class
                    messageContents = Encoding.UTF8.GetString(queueMessage.Body);
                    workOrderAck = JsonConvert.DeserializeObject<WorkOrderAck>(messageContents);

                    log.LogInformation(
                        "Recieved work order acknowledgment message for work order ID {workOrderId}, related to FaultName '{faultName}' with FaultActiveTime of '{faultActiveTime}' and AssetPath of '{assetPath}'. Message content: {messageContent}",
                        workOrderAck.WorkOrderId,
                        workOrderAck.FaultName,
                        workOrderAck.FaultActiveTime,
                        workOrderAck.AssetPath,
                        messageContents);
                }
            }
            catch (JsonException je)
            {
                log.LogError(
                    "Exception while processing message, {queueMessageContent}. Exception: {exceptionMessage}",
                    messageContents,
                    je.Message);

                await this.errorQueueService.SendMessageToErrorQueueAsync(messageContents, je.Message).ConfigureAwait(false);
                return;
            }

            // validate work order ack properties
            if (!this.validationService.IsValidWorkOrder(workOrderAck, out string validationMessage))
            {
                log.LogError(
                    "Work Order ack data failed validation. Validation Message: {validationMessage}",
                    validationMessage);

                await this.errorQueueService.SendMessageToErrorQueueAsync(messageContents, validationMessage).ConfigureAwait(false);
            }
            else
            {
                // Serialize message contents
                var sendMessage = JsonConvert.SerializeObject(workOrderAck);

                // Send Message to IoTHub
                await this.iconicsService.SendWorkOrderMessageAsync(sendMessage).ConfigureAwait(false);
            }
        }
    }
}
