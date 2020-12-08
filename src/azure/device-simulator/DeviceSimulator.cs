// -----------------------------------------------------------------------
// <copyright file="DeviceSimulator.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace IoTDeviceSimulator
{
    using System;
    using System.CommandLine.DragonFruit;
    using System.Threading.Tasks;
    using global::DeviceSimulator.Properties;
    using Microsoft.Azure.Devices.Client;

    /// <summary>
    /// Device Simulator for simulating Device-to-Cloud and Cloud-to-Device message from IoT Hub.
    /// </summary>
    public static class DeviceSimulator
    {
        private static string deviceConnectionString = Environment.GetEnvironmentVariable("IoTHubDeviceConnString");
        private static TransportType transportType = TransportType.Mqtt;

        /// <summary>
        /// Main Method for simulating Device-to-Cloud and Cloud-to-Device message from IoT Hub.
        /// </summary>
        /// <param name="count">Number of messages to be sent to IoT Hub.</param>
        /// <param name="interval">Interval in which the messages to be sent.</param>
        /// <param name="inputfile">Name of input file whose contents to be used to send message.</param>
        /// <param name="outputfile">Name of the output file to save the messages recieved from Iot Hub.</param>
        /// <param name="waittime">Wait time to receive messages from IoT Hub.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<int> Main(
                int count = 1,
                int interval = 30,
                string inputfile = null,
                string outputfile = "OutputData.json",
                int waittime = 60)
        {
            DeviceClient deviceClient;
            IotDevice device;

            int waitTime = Math.Max(waittime, count * interval);

            try
            {
                using (deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, transportType))
                {
                    device = new IotDevice(deviceClient, count, interval, inputfile, outputfile, waitTime);
                    await device.StartDevice().ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in connecting IoTHub. Conn String: \"{deviceConnectionString}\", Error {e.Message}\n");
                throw;
            }

            Console.WriteLine(Resources.ExitSimulator);

            return 0;
        }
    }
}
