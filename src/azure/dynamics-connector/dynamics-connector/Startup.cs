// -----------------------------------------------------------------------
// <copyright file="Startup.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Net.Http;
using DynamicsConnector.Dynamics.Interfaces;
using DynamicsConnector.Dynamics.Services;
using DynamicsConnector.Iconics.Interfaces;
using DynamicsConnector.Iconics.Services;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Cds.Client;

[assembly: FunctionsStartup(typeof(DynamicsConnector.Startup))]

namespace DynamicsConnector
{
    /// <summary>
    /// Azure function dependency injection singleton class.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        /// <summary>
        /// Initialize Iconic , Service Bus and CDS Service client.
        /// </summary>
        /// <param name="builder">FUnction Host Builder.</param>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            if (builder != null)
            {
                // add DynamicsEntityService singleton
                builder.Services.AddSingleton<IDynamicsEntityService>((dynamicsEntityService) =>
                    {
                        string dynamicsEnvironmentUrl = Environment.GetEnvironmentVariable("DynamicsEnvironmentUrl");
                        string dynamicsClientId = Environment.GetEnvironmentVariable("DynamicsClientId");
                        string dynamicsClientSecret = Environment.GetEnvironmentVariable("DynamicsClientSecret");
                        string connectionString = $@"AuthType=ClientSecret;Url={dynamicsEnvironmentUrl};ClientId={dynamicsClientId};ClientSecret={dynamicsClientSecret}";
                        CdsServiceClient service = new CdsServiceClient(connectionString);
                        return new DynamicsEntityService(service);
                    });

                // add ValidationService singleton
                builder.Services.AddSingleton<IValidationService>((validationService) =>
                {
                    return new ValidationService();
                });

                // add ErrorQueueService singleton
                builder.Services.AddSingleton<IErrorQueueService>((errorQueueService) =>
                {
                    string serviceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnection");
                    string queueName = Environment.GetEnvironmentVariable("ServiceBusErrorQueue");
                    QueueClient queueClient = new QueueClient(serviceBusConnectionString, queueName);
                    return new ErrorQueueService(queueClient);
                });

                // add IconicsService singleton
                builder.Services.AddSingleton<IIconicsService>((iconicsservice) =>
                {
                    string ioTHubName = Environment.GetEnvironmentVariable("IoTHubName");
                    string ioTHubSharedAccessPolicyName = Environment.GetEnvironmentVariable("IoTHubSharedAccessPolicyName");
                    string ioTHubSharedAccessPolicyKey = Environment.GetEnvironmentVariable("IoTHubSharedAccessPolicyKey");
                    string targetDevice = Environment.GetEnvironmentVariable("IoTHubDeviceName");
                    string connectionString = $@"HostName={ioTHubName}.azure-devices.net;DeviceId={targetDevice};SharedAccessKeyName={ioTHubSharedAccessPolicyName};SharedAccessKey={ioTHubSharedAccessPolicyKey}";
                    DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(connectionString);
                    return new IconicsService(deviceClient);
                });

                // add singleton InstanceId
                builder.Services.AddSingleton(sp => new InstanceId { Id = Guid.NewGuid().ToString() });

                builder.Services.AddHttpClient();
            }
        }
    }
}