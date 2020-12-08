// -----------------------------------------------------------------------
// <copyright file="IconicsService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace DynamicsConnector.Iconics.Services
{
    using System.Text;
    using System.Threading.Tasks;

    using DynamicsConnector.Iconics.Interfaces;

    using Microsoft.Azure.Devices.Client;

    /// <summary>
    /// Contains Iconics Service information.
    /// </summary>
    public class IconicsService : IIconicsService
    {
        private readonly DeviceClient deviceClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="IconicsService"/> class.
        /// </summary>
        /// <param name="deviceClient">IoT Hub Service Client.</param>
        public IconicsService(DeviceClient deviceClient)
        {
            this.deviceClient = deviceClient;
        }

        /// <summary>
        /// Method definition for sending Work Order Acknowledgment message to Cloud-to-Device.
        /// </summary>
        /// <param name="responseString">Work Order Acknowledgment Data.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendWorkOrderMessageAsync(string responseString)
        {
            using (var workOrderMessage = new Message(Encoding.ASCII.GetBytes(responseString)))
            {
                await this.deviceClient.SendEventAsync(workOrderMessage).ConfigureAwait(false);
            }
        }
    }
}
