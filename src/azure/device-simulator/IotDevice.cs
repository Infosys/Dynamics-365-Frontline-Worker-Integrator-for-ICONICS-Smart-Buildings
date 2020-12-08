// -----------------------------------------------------------------------
// <copyright file="IotDevice.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace IoTDeviceSimulator
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using global::DeviceSimulator.Properties;
    using Microsoft.Azure.Devices.Client;
    using Newtonsoft.Json;

    /// <summary>
    /// Device Simulator for simulating Device-to-Cloud and Cloud-to-Device message from IoT Hub.
    /// </summary>
    public class IotDevice
    {
        private readonly DeviceClient deviceClient;
        private int messageCount;
        private int messageInterval;
        private string inputFileName;
        private string outputFilename;
        private int waitTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="IotDevice"/> class.
        /// Constructor to initialize members of this class.
        /// </summary>
        /// <param name="deviceClient">IoT Device Service Client.</param>
        /// <param name="count">Number of messages to be sent to IoT Hub.</param>
        /// <param name="interval">Interval in which the messages to be sent.</param>
        /// <param name="inputFileName">Name of input file whose contents to be used to send message.</param>
        /// <param name="outputFileName">Name of the output file to save the messages recieved from Iot Hub.</param>
        /// <param name="waitTime">Wait time to receive messages from IoT Hub.</param>
        public IotDevice(
            DeviceClient deviceClient,
            int count,
            int interval,
            string inputFileName,
            string outputFileName,
            int waitTime)
        {
            this.messageCount = count;
            this.messageInterval = interval;
            this.inputFileName = inputFileName;
            this.outputFilename = outputFileName;
            this.waitTime = waitTime;

            this.deviceClient = deviceClient ?? throw new ArgumentNullException(nameof(deviceClient));
        }

        [System.ComponentModel.DefaultValue(0)]
        private static int MessageId { get; set; }

        /// <summary>
        /// This method is to start the Device.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task StartDevice()
        {
            this.SendMessagesToIoTHub();
            await this.ReceiveMessagesFromIotHub().ConfigureAwait(false);
        }

        private static object GenerateFaultData()
        {
            MessageId++;
            string messageType = "ICONICS FDD";

            // Create JSON message
            var faultData = new
            {
                messageId = MessageId,
                AssetName = "AssetName",
                AssetPath = "Campus\\Bldg\\Device\\Sensor\\AssetName",
                FaultName = "FaultName",
                FaultActiveTime = DateTime.Now.ToString("s", CultureInfo.CurrentCulture),
                FaultCostValue = "FaultCostNumeric",
                RelatedValue1 = "RelatedValue01",
                RelatedValue2 = "RelatedValue02",
                RelatedValue3 = "RelatedValue03",
                RelatedValue4 = "RelatedValue04",
                RelatedValue5 = "RelatedValue05",
                RelatedValue6 = "RelatedValue06",
                RelatedValue7 = "RelatedValue07",
                RelatedValue8 = "RelatedValue08",
                RelatedValue9 = "RelatedValue09",
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
                MessageSource = messageType,
            };

            return faultData;
        }

        private async void SendMessagesToIoTHub()
        {
            int sentCount = 0;
            string messageString;

            while (true)
            {
                if (!string.IsNullOrEmpty(this.inputFileName))
                {
                    if (File.Exists(this.inputFileName))
                    {
                        messageString = File.ReadAllText(this.inputFileName);
                    }
                    else
                    {
                        Console.WriteLine($"File not found {this.inputFileName}");
                        break;
                    }
                }
                else
                {
                    object faultData = GenerateFaultData();
                    messageString = JsonConvert.SerializeObject(faultData);
                }

                using (var message = new Message(Encoding.ASCII.GetBytes(messageString)))
                {
                    // Send fault data as device to cloud message
                    await this.deviceClient.SendEventAsync(message).ConfigureAwait(false);
                }

                sentCount++;
                Console.WriteLine($"{sentCount} > Sending message: {messageString}");

                if (this.messageCount != 0 && sentCount >= this.messageCount)
                {
                    break;
                }
                else
                {
                    await Task.Delay(this.messageInterval * 1000).ConfigureAwait(false);
                }
            }
        }

        private async Task ReceiveMessagesFromIotHub()
        {
            Console.WriteLine(Resources.DeviceWaitMessage);

            File.Delete(this.outputFilename);

            int remaningWaitTime = this.waitTime;
            DateTime startTime = DateTime.Now;

            while (remaningWaitTime > 0)
            {
                Message receivedMessage = await this.deviceClient.ReceiveAsync(TimeSpan.FromSeconds(remaningWaitTime)).ConfigureAwait(false);
                DateTime now = DateTime.Now;
                remaningWaitTime = this.waitTime - ((int)(now - startTime).TotalSeconds);
                if (receivedMessage == null)
                {
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                string messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                Console.WriteLine($"\t{DateTime.Now.ToString("s", CultureInfo.CurrentCulture)}> Received message: {messageData}");
                Console.ResetColor();

                using (StreamWriter outputFile = new StreamWriter(this.outputFilename, true))
                {
                    outputFile.WriteLine(messageData);
                }

                await this.deviceClient.CompleteAsync(receivedMessage).ConfigureAwait(false);
            }
        }
    }
}
