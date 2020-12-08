// -----------------------------------------------------------------------
// <copyright file="E2ETests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

using Azure.Messaging.EventHubs.Consumer;
using DynamicsConnector.Dynamics.Models;
using DynamicsConnector.Iconics.Models;
using DynamicsConnector.Tests.Integration.Models;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.PowerPlatform.Cds.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace DynamicsConnector.Tests.Integration
{
    [Collection("Environment variable collection")]
    public class E2ETests : IAsyncLifetime
    {
        private EnvironmentVariableFixture environmentVariableFixture;
        private CdsServiceClient cdsServiceClient;
        private QueueClient workOrderQueueClient;
        private QueueClient createAlertErrorQueueClient;
        private SubscriptionClient createAlertTopicSubscriptionClient;
        private readonly ITestOutputHelper output;
        private List<KeyValuePair<Guid, string>> entitiesToDelete;
        private string uniqueFaultName;

        public E2ETests(EnvironmentVariableFixture fixture, ITestOutputHelper output)
        {
            this.output = output;
            this.environmentVariableFixture = fixture;
            string dynamicsConnectionString = $@"AuthType=ClientSecret;Url={Environment.GetEnvironmentVariable("DynamicsEnvironmentUrl")};ClientId={Environment.GetEnvironmentVariable("DynamicsClientId")};ClientSecret={Environment.GetEnvironmentVariable("DynamicsClientSecret")}";
            this.cdsServiceClient = new CdsServiceClient(dynamicsConnectionString);
            this.createAlertErrorQueueClient = new QueueClient(Environment.GetEnvironmentVariable("ServiceBusConnectionString"), Environment.GetEnvironmentVariable("CreateAlertErrorQueueName"));
            this.workOrderQueueClient = new QueueClient(Environment.GetEnvironmentVariable("ServiceBusConnectionString"), Environment.GetEnvironmentVariable("WorkOrderQueueName"));
            this.createAlertTopicSubscriptionClient = new SubscriptionClient(Environment.GetEnvironmentVariable("ServiceBusConnectionString"), Environment.GetEnvironmentVariable("CreateAlertTopicName"), Environment.GetEnvironmentVariable("CreateAlertTopicSubscriptionName"));
            this.entitiesToDelete = new List<KeyValuePair<Guid, string>>();
        }

        [Fact]
        public async void RunE2ETest()
        {
            string deviceConnectionString = Environment.GetEnvironmentVariable("IoTHubDeviceConnectionString");
            string maxRetriesEnvironmentVariable = Environment.GetEnvironmentVariable("VerificationMaxRetries");
            string retrySecondsEnvironmentVariable = Environment.GetEnvironmentVariable("VerificationRetrySeconds");
            string shouldCreateWorkOrderEnvironmentVariable = Environment.GetEnvironmentVariable("ShouldCreateWorkOrder");
            int maxRetries = maxRetriesEnvironmentVariable != null ? int.Parse(maxRetriesEnvironmentVariable) : 10;
            int retrySeconds = retrySecondsEnvironmentVariable != null ? int.Parse(retrySecondsEnvironmentVariable) : 5;
            bool shouldCreateWorkOrder = shouldCreateWorkOrderEnvironmentVariable != null ? bool.Parse(shouldCreateWorkOrderEnvironmentVariable) : false;
            Microsoft.Azure.Devices.Client.TransportType transportType = Microsoft.Azure.Devices.Client.TransportType.Mqtt;
            using (DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, transportType))
            {
                // generate unique GUIDs
                string uniqueAssetName = Guid.NewGuid().ToString();
                string uniqueAssetPath = Guid.NewGuid().ToString();
                uniqueFaultName = Guid.NewGuid().ToString();
                output.WriteLine($"Generated unique AssetName - {uniqueAssetName}");
                output.WriteLine($"Generated unique AssetPath - {uniqueAssetPath}");
                output.WriteLine($"Generated unique FaultName - {uniqueFaultName}");

                // send message to IoTHub
                output.WriteLine("Sending message to IoT Hub");
                string currentISOTimeString = DateTime.UtcNow.ToString("o");
                await SendIconicsFaultDataToIoTHubAsync(deviceClient, uniqueAssetName, uniqueAssetPath, uniqueFaultName, "Active", currentISOTimeString);

                // verify Dynamics Alert and Asset
                output.WriteLine("Attempting Dynamics Alert and Asset verification");
                AssetAlertVerification assetAlertVerification = VerifyDynamicsAssetAndAlert(maxRetries, retrySeconds, uniqueAssetName, uniqueFaultName);

                // create WorkOrder if necessary
                if (shouldCreateWorkOrder)
                {
                    CreateWorkOrderIfNecessary(assetAlertVerification.verifiedAlertId);
                }

                EventHubConsumerClient consumer = new EventHubConsumerClient(EventHubConsumerClient.DefaultConsumerGroupName, Environment.GetEnvironmentVariable("IoTHubEventHubEndpoint"), Environment.GetEnvironmentVariable("IoTHubEventHubName"));

                // verify work order acknowledgement in IoTHub
                output.WriteLine("Attempting IoT Hub work order acknowledgement verification");
                bool iotHubMessageVerified = await VerifyWorkOrderAcknowledgementAsync(consumer, maxRetries, retrySeconds, uniqueAssetPath, uniqueFaultName);

                // assert
                Assert.True(assetAlertVerification.assetVerified);
                Assert.True(assetAlertVerification.alertVerified);
                Assert.True(iotHubMessageVerified);
            }
        }

        [Fact]
        public async void ValidateDynamicsInActiveAlert()
        {
            string deviceConnectionString = Environment.GetEnvironmentVariable("IoTHubDeviceConnectionString");
            string maxRetriesEnvironmentVariable = Environment.GetEnvironmentVariable("VerificationMaxRetries");
            string retrySecondsEnvironmentVariable = Environment.GetEnvironmentVariable("VerificationRetrySeconds");
            string shouldCreateWorkOrderEnvironmentVariable = Environment.GetEnvironmentVariable("ShouldCreateWorkOrder");
            int maxRetries = maxRetriesEnvironmentVariable != null ? int.Parse(maxRetriesEnvironmentVariable) : 10;
            int retrySeconds = retrySecondsEnvironmentVariable != null ? int.Parse(retrySecondsEnvironmentVariable) : 5;
            Microsoft.Azure.Devices.Client.TransportType transportType = Microsoft.Azure.Devices.Client.TransportType.Mqtt;
            using (DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, transportType))
            {
                // generate unique GUIDs
                string uniqueAssetName = Guid.NewGuid().ToString();
                string uniqueAssetPath = Guid.NewGuid().ToString();
                uniqueFaultName = Guid.NewGuid().ToString();
                output.WriteLine($"Generated unique AssetName - {uniqueAssetName}");
                output.WriteLine($"Generated unique AssetPath - {uniqueAssetPath}");
                output.WriteLine($"Generated unique FaultName - {uniqueFaultName}");

                // send message to IoTHub
                output.WriteLine("Sending message to IoT Hub");
                string currentISOTimeString = DateTime.UtcNow.ToString("o");
                await SendIconicsFaultDataToIoTHubAsync(deviceClient, uniqueAssetName, uniqueAssetPath, uniqueFaultName, "Active", currentISOTimeString);

                // verify Dynamics Alert and Asset
                output.WriteLine("Attempting Dynamics Alert and Asset verification");
                AssetAlertVerification assetAlertVerification = VerifyDynamicsAssetAndAlert(maxRetries, retrySeconds, uniqueAssetName, uniqueFaultName);
                output.WriteLine("Sending message to IoT Hub with Fault State as InActive");
                await SendIconicsFaultDataToIoTHubAsync(deviceClient, uniqueAssetName, uniqueAssetPath, uniqueFaultName, "InActive", currentISOTimeString);

                bool inActiveAlertVerified = VerifyDynamicsInActiveAlert(maxRetries, retrySeconds, uniqueFaultName);

                // assert
                Assert.True(assetAlertVerification.assetVerified);
                Assert.True(assetAlertVerification.alertVerified);
                Assert.True(inActiveAlertVerified);
            }
        }

        public async Task DisposeAsync()
        {
            string deviceConnectionString = Environment.GetEnvironmentVariable("IoTHubDeviceConnectionString");
            Microsoft.Azure.Devices.Client.TransportType transportType = Microsoft.Azure.Devices.Client.TransportType.Mqtt;
            using (DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, transportType))
            {
                // purge messaging services
                output.WriteLine("Purging Service Bus messages");
                PurgeServiceBusMessagesAsync(workOrderQueueClient, output);
                PurgeServiceBusMessagesAsync(createAlertErrorQueueClient, output);
                PurgeServiceBusMessagesAsync(createAlertTopicSubscriptionClient, output);
                output.WriteLine("Purging IoT Hub messages");
                await PurgeIoTHubMessagesAsync(deviceClient);

                // find the case (title property is equal to alertid)
                output.WriteLine("Deleting Dynamics entities");
                QueryExpression caseQueryExpression = FindEntityWithPropertyValue("incident", "title", uniqueFaultName);
                EntityCollection caseQueryExpressionResult = this.cdsServiceClient.RetrieveMultiple(caseQueryExpression);
                if (caseQueryExpressionResult != null && caseQueryExpressionResult.Entities.Count > 0)
                {
                    Entity retrievedCase = caseQueryExpressionResult.Entities[0];
                    this.cdsServiceClient.Delete("incident", retrievedCase.Id);
                    output.WriteLine($"Deleted incident entity with id {retrievedCase.Id}");
                }

                // find the work order (msdyn_workordersummary property is equal to alertid) associated with the alert
                QueryExpression workOrderQueryExpression = FindEntityWithPropertyValue("msdyn_workorder", "msdyn_workordersummary", uniqueFaultName, true);
                EntityCollection workOrderQueryExpressionResult = this.cdsServiceClient.RetrieveMultiple(workOrderQueryExpression);
                if (workOrderQueryExpressionResult != null && workOrderQueryExpressionResult.Entities.Count > 0)
                {
                    // find the resource requirement attached to work order
                    Entity retrievedWorkOrder = workOrderQueryExpressionResult.Entities[0];
                    string workOrderName = (string)retrievedWorkOrder["msdyn_name"];
                    QueryExpression resourceRequirementExpression = FindEntityWithPropertyValue("msdyn_resourcerequirement", "msdyn_name", workOrderName);
                    EntityCollection resourceRequirementExpressionResult = this.cdsServiceClient.RetrieveMultiple(resourceRequirementExpression);
                    if (resourceRequirementExpressionResult != null && resourceRequirementExpressionResult.Entities.Count > 0)
                    {
                        Entity retrievedResourceRequirement = resourceRequirementExpressionResult.Entities[0];
                        this.cdsServiceClient.Delete("msdyn_resourcerequirement", retrievedResourceRequirement.Id);
                        output.WriteLine($"Deleted msdyn_resourcerequirement entity with id {retrievedResourceRequirement.Id}");
                    }
                    this.cdsServiceClient.Delete("msdyn_workorder", retrievedWorkOrder.Id);
                    output.WriteLine($"Deleted msdyn_workorder entity with id {retrievedWorkOrder.Id}");
                }

                foreach (KeyValuePair<Guid, string> pair in entitiesToDelete)
                {
                    this.cdsServiceClient.Delete(pair.Value, pair.Key);
                    output.WriteLine($"Deleted {pair.Value} entity with id {pair.Key}");
                }
                entitiesToDelete.Clear();
            }
        }

        public async Task InitializeAsync() { }

        public async Task SendIconicsFaultDataToIoTHubAsync(DeviceClient deviceClient, string uniqueAssetName, string uniqueAssetPath, string uniqueFaultName, string faultState, string faultActiveTime)
        {
            IconicsFault iconicsFault = GenerateFaultData(uniqueAssetName, uniqueAssetPath, uniqueFaultName, faultState, faultActiveTime);
            string messageString = JsonConvert.SerializeObject(iconicsFault);
            using (var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(messageString)))
            {
                await deviceClient.SendEventAsync(message).ConfigureAwait(false);
            }
        }

        public void CreateWorkOrderIfNecessary(Guid alertId)
        {
            // create account, case, work order type, price list, and work order 
            Entity newAccount = new Entity("account");
            string uniqueAccountName = Guid.NewGuid().ToString();
            newAccount["name"] = uniqueAccountName;
            Guid createdAccountId = cdsServiceClient.Create(newAccount);
            entitiesToDelete.Add(new KeyValuePair<Guid, string>(createdAccountId, "account"));
            output.WriteLine($"Created Account with id {createdAccountId}");

            Entity newCase = new Entity("incident");
            newCase["title"] = uniqueFaultName;
            newCase["customerid"] = new EntityReference("account", createdAccountId);
            Guid createdCaseId = cdsServiceClient.Create(newCase);
            output.WriteLine($"Created Incident with id {createdCaseId}");

            string uniqueWorkOrderTypeName = Guid.NewGuid().ToString();
            Entity newWorkOrderType = new Entity("msdyn_workordertype");
            newWorkOrderType["msdyn_name"] = uniqueWorkOrderTypeName;
            newWorkOrderType["msdyn_incidentrequired"] = false;
            newWorkOrderType["msdyn_taxable"] = false;
            Guid createdWorkOrderTypeId = cdsServiceClient.Create(newWorkOrderType);
            entitiesToDelete.Add(new KeyValuePair<Guid, string>(createdWorkOrderTypeId, "msdyn_workordertype"));
            output.WriteLine($"Created WorkOrderType with id {createdWorkOrderTypeId}");

            QueryExpression usDollarQueryExpression = FindEntityWithPropertyValue("transactioncurrency", "isocurrencycode", "USD");
            EntityCollection usDollarQueryExpressionResult = cdsServiceClient.RetrieveMultiple(usDollarQueryExpression);
            Guid currencyId = usDollarQueryExpressionResult.Entities[0].Id;

            string uniquePriceListName = Guid.NewGuid().ToString();
            Entity newPriceList = new Entity("pricelevel");
            newPriceList["name"] = uniquePriceListName;
            newPriceList["transactioncurrencyid"] = new EntityReference("transactioncurrency", currencyId);
            Guid createdPriceListId = cdsServiceClient.Create(newPriceList);
            entitiesToDelete.Add(new KeyValuePair<Guid, string>(createdPriceListId, "pricelevel"));
            output.WriteLine($"Created PriceLevel with id {createdPriceListId}");

            Entity newWorkOrder = new Entity("msdyn_workorder");
            newWorkOrder["msdyn_systemstatus"] = 690970000;
            newWorkOrder["msdyn_taxable"] = false;
            newWorkOrder["msdyn_workordersummary"] = uniqueFaultName;
            newWorkOrder["msdyn_serviceaccount"] = new EntityReference("account", createdAccountId);
            newWorkOrder["msdyn_pricelist"] = new EntityReference("pricelevel", createdPriceListId);
            newWorkOrder["msdyn_workordertype"] = new EntityReference("msdyn_workordertype", createdWorkOrderTypeId);
            newWorkOrder["msdyn_iotalert"] = new EntityReference(DynamicsEntities.IoTAlert, alertId);
            Guid createdWorkOrderId = cdsServiceClient.Create(newWorkOrder);
            output.WriteLine($"Created WorkOrder with id {createdWorkOrderId}");
        }

        public AssetAlertVerification VerifyDynamicsAssetAndAlert(int maxRetries, int retrySeconds, string uniqueAssetName, string uniqueFaultName)
        {
            bool assetVerified = false;
            bool alertVerified = false;
            Guid verifiedAssetId = Guid.Empty;
            Guid verifiedAlertId = Guid.Empty;
            int dynamicsTryCount = 0;
            while ((alertVerified == false || assetVerified == false) && dynamicsTryCount < (maxRetries + 1))
            {
                dynamicsTryCount++;
                QueryExpression assetQueryExpression = FindEntityWithPropertyValue(DynamicsEntities.CustomerAsset, CustomerAssetProperties.MsdynName, uniqueAssetName);
                QueryExpression alertQueryExpression = FindEntityWithPropertyValue(DynamicsEntities.IoTAlert, IoTAlertProperties.MsdynDescription, uniqueFaultName);
                EntityCollection assetQueryExpressionResult = cdsServiceClient.RetrieveMultiple(assetQueryExpression);
                EntityCollection alertQueryExpressionResult = cdsServiceClient.RetrieveMultiple(alertQueryExpression);
                if (assetQueryExpressionResult.Entities.Count > 0)
                {
                    assetVerified = true;
                    verifiedAssetId = assetQueryExpressionResult.Entities[0].Id;
                    entitiesToDelete.Add(new KeyValuePair<Guid, string>(verifiedAssetId, DynamicsEntities.CustomerAsset));
                    output.WriteLine($"Verified Asset with id {verifiedAssetId}");
                }
                if (alertQueryExpressionResult.Entities.Count > 0)
                {
                    alertVerified = true;
                    verifiedAlertId = alertQueryExpressionResult.Entities[0].Id;
                    entitiesToDelete.Add(new KeyValuePair<Guid, string>(verifiedAlertId, DynamicsEntities.IoTAlert));
                    output.WriteLine($"Verified Alert with id {verifiedAlertId}");
                }
                if (assetVerified == false || alertVerified == false)
                {
                    Thread.Sleep(retrySeconds * 1000);
                }
            }
            return new AssetAlertVerification()
            {
                alertVerified = alertVerified,
                assetVerified = assetVerified,
                verifiedAlertId = verifiedAlertId,
                verifiedAssetId = verifiedAssetId
            };
        }

        public bool VerifyDynamicsInActiveAlert(int maxRetries, int retrySeconds, string uniqueFaultName)
        {
            bool alertVerified = false;
            Guid verifiedAlertId = Guid.NewGuid();
            int dynamicsTryCount = 0;
            while ((alertVerified == false) && dynamicsTryCount < (maxRetries + 1))
            {
                dynamicsTryCount++;
                QueryExpression alertQueryExpression = new QueryExpression()
                {
                    Distinct = false,
                    EntityName = DynamicsEntities.IoTAlert,
                    ColumnSet = new ColumnSet(false),
                    Criteria =
                {
                    Filters =
                        {
                            new FilterExpression
                            {
                                Conditions =
                                {
                                    new ConditionExpression(IoTAlertProperties.MsdynDescription, ConditionOperator.Equal, uniqueFaultName),
                                    new ConditionExpression(IoTAlertProperties.StateCode, ConditionOperator.Equal, IotAlertStateCode.Inactive.ToString()),
                                },
                            },
                        },
                },
                };
                EntityCollection alertQueryExpressionResult = cdsServiceClient.RetrieveMultiple(alertQueryExpression);
                if (alertQueryExpressionResult.Entities.Count > 0)
                {
                    alertVerified = true;
                    verifiedAlertId = alertQueryExpressionResult.Entities[0].Id;
                    output.WriteLine($"Verified InActive Alert with id {verifiedAlertId}");
                }
                if (alertVerified == false)
                {
                    Thread.Sleep(retrySeconds * 1000);
                }
            }
            return alertVerified;
        }

        public async Task<bool> VerifyWorkOrderAcknowledgementAsync(EventHubConsumerClient consumer, int maxRetries, int retrySeconds, string uniqueAssetPath, string uniqueFaultName)
        {
            bool iotHubMessageVerified = false;
            int remainingWaitTime = maxRetries * retrySeconds;

            using CancellationTokenSource cancellationSource = new CancellationTokenSource();
            cancellationSource.CancelAfter(TimeSpan.FromSeconds(remainingWaitTime));

            //Microsoft.Azure.Devices.Client.Message receivedMessage = await deviceClient.ReceiveAsync(TimeSpan.FromSeconds(remainingWaitTime)).ConfigureAwait(false);
            await foreach (PartitionEvent partitionEvent in consumer.ReadEventsAsync(cancellationSource.Token))
            {
                string messageData = Encoding.UTF8.GetString(partitionEvent.Data.Body.ToArray());
                if (messageData.Contains(uniqueAssetPath) && messageData.Contains(uniqueFaultName))
                {
                    output.WriteLine("Verified work order acknowledgement");
                    iotHubMessageVerified = true;
                    break;
                }
            }
            return iotHubMessageVerified;
        }

        public async Task PurgeIoTHubMessagesAsync(DeviceClient deviceClient)
        {
            while (true)
            {
                Microsoft.Azure.Devices.Client.Message receivedMessage = await deviceClient.ReceiveAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                if (receivedMessage == null)
                {
                    // all messages purged
                    break;
                }
                // purge message
                await deviceClient.CompleteAsync(receivedMessage).ConfigureAwait(false);
            }
        }

        public void PurgeServiceBusMessagesAsync(IReceiverClient receiverClient, ITestOutputHelper output)
        {
            receiverClient.RegisterMessageHandler(async (Microsoft.Azure.ServiceBus.Message message, CancellationToken token) =>
            {
                // purge message
                await receiverClient.CompleteAsync(message.SystemProperties.LockToken);
            }, new MessageHandlerOptions(async args => output.WriteLine(args.Exception.ToString()))
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            });
        }

        public QueryExpression FindEntityWithPropertyValue(string entityType, string propertyName, string propertyValue, bool retrieveAllPropertyValues = false)
        {
            return new QueryExpression()
            {
                Distinct = false,
                EntityName = entityType,
                ColumnSet = new ColumnSet(retrieveAllPropertyValues),
                Criteria =
                {
                    Filters =
                        {
                            new FilterExpression
                            {
                                Conditions =
                                {
                                    new ConditionExpression(propertyName, ConditionOperator.Equal, propertyValue),
                                },
                            },
                        },
                },
            };
        }

        public IconicsFault GenerateFaultData(string assetName, string assetPath, string faultName, string faultState, string faultActiveTime)
        {
            return new IconicsFault()
            {
                AssetName = assetName,
                AssetPath = assetPath,
                FaultName = faultName,
                FaultState = faultState,
                FaultActiveTime = faultActiveTime,
                FaultCostValue = "0",
                RelatedValue1 = "Bad - No Data (8/13/2020 1=28=05 PM)",
                RelatedValue2 = "PugetSound\\WestCampus\\B121\\SF\\L01\\1",
                RelatedValue3 = "Failed Fan Motor",
                RelatedValue4 = "ESBM011",
                RelatedValue5 = "30",
                RelatedValue6 = "Maintenance Fault",
                RelatedValue7 = "",
                RelatedValue8 = "IF(trueforduration( <<CMD>> == 1 && <<STS>> == 0,1800000)) THEN 1 ELSE 0",
                RelatedValue9 = "",
                RelatedValue10 = "Fan",
                RelatedValue11 = "{{Equipment Commanded On}} && {{Equipment Proof Off}}",
                RelatedValue12 = "5",
                RelatedValue13 = "None",
                RelatedValue14 = "High",
                RelatedValue15 = "- Fan is failed.",
                RelatedValue16 = "1",
                RelatedValue17 = "",
                RelatedValue18 = "",
                RelatedValue19 = "USWARED121",
                RelatedValue20 = "",
                MessageSource = "ICONICS FDD",
                Description = "Description"
            };
        }
    }
}
