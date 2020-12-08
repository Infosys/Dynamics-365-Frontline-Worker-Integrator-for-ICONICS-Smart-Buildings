// -----------------------------------------------------------------------
// <copyright file="DynamicsEntityService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace DynamicsConnector.Dynamics.Services
{
    using System;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text;
    using DynamicsConnector.Dynamics.Interfaces;
    using DynamicsConnector.Dynamics.Models;
    using DynamicsConnector.Iconics.Models;
    using Microsoft.Extensions.Logging;
    using Microsoft.PowerPlatform.Cds.Client;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Newtonsoft.Json;
    using Polly;

    /// <summary>
    /// Contains Dynamics Entity Service information.
    /// </summary>
    public class DynamicsEntityService : IDynamicsEntityService
    {
        private readonly IOrganizationService cdsServiceClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicsEntityService"/> class.
        /// </summary>
        /// <param name="cdsServiceClient">CDS Service Client.</param>
        public DynamicsEntityService(IOrganizationService cdsServiceClient)
        {
            this.cdsServiceClient = cdsServiceClient;
        }

        /// <summary>
        /// Computes the SHA256 hash of ICONICS' FaultName + FaultActiveTime + AssetPath.
        /// </summary>
        /// <param name="iconicsFault">Fault Data.</param>
        /// <returns>return hash value as string.</returns>
        public static string ComputeSha256Hash(IconicsFault iconicsFault)
        {
            string hashValue = string.Empty;
            if (iconicsFault != null)
            {
                // create a hash of the ICONICS' FaultName + FaultActiveTime + AssetPath
                string alertToken = iconicsFault.FaultName + iconicsFault.FaultActiveTime + iconicsFault.AssetPath;
                using (var sha256 = new SHA256Managed())
                {
                    byte[] data = sha256.ComputeHash(Encoding.UTF8.GetBytes(alertToken));
                    hashValue = BitConverter.ToString(data).Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase);
                }
            }

            return hashValue;
        }

        /// <summary>
        /// Method definition for creating an IoT Alert in Dynamics.
        /// </summary>
        /// <param name="iconicsFault">Fault Data.</param>
        /// <param name="assetId">Customer Asset ID.</param>
        /// <param name="log">Log Content.</param>
        /// <returns>IoT Alert record GUID.</returns>
        public Guid CreateAlert(IconicsFault iconicsFault, Guid assetId, ILogger log)
        {
            Guid alertId = Guid.Empty;
            if (iconicsFault != null)
            {
                // create new alert from fault data
                Entity newAlert = new Entity(DynamicsEntities.IoTAlert);
                newAlert[IoTAlertProperties.MsdynAlertData] = JsonConvert.SerializeObject(iconicsFault);
                newAlert[IoTAlertProperties.MsdynAlertTime] = DateTimeOffset.Parse(iconicsFault.FaultActiveTime, CultureInfo.CurrentCulture);
                newAlert[IoTAlertProperties.MsdynAlertType] = IotAlertAlertType.Anomaly;
                newAlert[IoTAlertProperties.MsdynDescription] = iconicsFault.FaultName;
                newAlert[IoTAlertProperties.StateCode] = IotAlertStateCode.Active;
                newAlert[IoTAlertProperties.StatusCode] = IotAlertStatusCode.Active;
                newAlert[DynamicsRelationships.IoTAlertToOneCustomerAsset] = new EntityReference(DynamicsEntities.CustomerAsset, assetId);
                newAlert[IoTAlertProperties.MsdynAlertToken] = DynamicsEntityService.ComputeSha256Hash(iconicsFault);

                alertId = GetExecutionPolicy(log).Execute(() =>
                {
                    return this.cdsServiceClient.Create(newAlert);
                });

                log.LogInformation(
                    "Created Alert {alertId} from Fault (FaultName: {FaultName}, FaultActiveTime: {FaultActiveTime}, AssetName: {AssetName}, AssetPath: {AssetPath}).",
                    alertId,
                    iconicsFault.FaultName,
                    iconicsFault.FaultActiveTime,
                    iconicsFault.AssetName,
                    iconicsFault.AssetPath);

                log.LogInformation("Associated Alert {alertId} with Asset {assetId}.", alertId, assetId);
            }

            return alertId;
        }

        /// <summary>
        /// Method definition for creating an Asset if not exist in Dynamics.
        /// </summary>
        /// <param name="iconicsFault">Fault Data.</param>
        /// <param name="log">Log Content.</param>
        /// <returns>Customer Asset record GUID.</returns>
        public Guid CreateAssetIfNotExists(IconicsFault iconicsFault, ILogger log)
        {
            Guid assetId = Guid.Empty;
            if (iconicsFault != null)
            {
                // build query expression that finds the asset with the name supplied in the fault
                QueryExpression queryExpression = new QueryExpression()
                {
                    Distinct = false,
                    EntityName = DynamicsEntities.CustomerAsset,
                    ColumnSet = new ColumnSet(CustomerAssetProperties.MsdynName),
                    Criteria =
                {
                    Filters =
                        {
                            new FilterExpression
                            {
                                Conditions =
                                {
                                    new ConditionExpression(CustomerAssetProperties.MsdynName, ConditionOperator.Equal, iconicsFault.AssetName),
                                },
                            },
                        },
                },
                };

                // run the query to determine if the asset already exists in Dynamics
                EntityCollection queryExpressionResult = GetExecutionPolicy(log).Execute(() =>
                {
                    return this.cdsServiceClient.RetrieveMultiple(queryExpression);
                });

                if (queryExpressionResult.Entities.Count == 0)
                {
                    // asset does not exist, create a new one and return its id
                    Entity newAsset = new Entity(DynamicsEntities.CustomerAsset);
                    newAsset[CustomerAssetProperties.MsdynName] = iconicsFault.AssetName;
                    newAsset[CustomerAssetProperties.StateCode] = CustomerAssetStateCode.Active;
                    newAsset[CustomerAssetProperties.StatusCode] = CustomerAssetStatusCode.Active;

                    assetId = GetExecutionPolicy(log).Execute(() =>
                    {
                        return this.cdsServiceClient.Create(newAsset);
                    });

                    log.LogInformation(
                        "Created Asset {assetId} from Fault (FaultName: {FaultName}, FaultActiveTime: {FaultActiveTime}, AssetName: {AssetName}, AssetPath: {AssetPath}).",
                        assetId,
                        iconicsFault.FaultName,
                        iconicsFault.FaultActiveTime,
                        iconicsFault.AssetName,
                        iconicsFault.AssetPath);

                    return assetId;
                }
                else
                {
                    // asset does exist, return its id
                    assetId = queryExpressionResult.Entities[0].Id;
                    log.LogInformation("Retrieved Asset {assetId}.", assetId);

                    return assetId;
                }
            }

            return assetId;
        }

        /// <summary>
        /// Method declaration for getting previous IoT Alert based on Alert Token Hash Value.
        /// </summary>
        /// <param name="iconicsFault">Fault Data.</param>
        /// <param name="log">Log Content.</param>
        /// <returns>An existing IoT Alert record GUID.</returns>
        public Guid GetIoTAlert(IconicsFault iconicsFault, ILogger log)
        {
            Guid alertId = Guid.Empty;
            if (iconicsFault != null)
            {
                string alertToken = DynamicsEntityService.ComputeSha256Hash(iconicsFault);

                // build query expression that finds the Iot Alert with the Hash value supplied in the fault
                QueryExpression queryExpression = new QueryExpression()
                {
                    Distinct = false,
                    EntityName = DynamicsEntities.IoTAlert,
                    ColumnSet = new ColumnSet(IoTAlertProperties.MsdynAlertToken),
                    Criteria =
                {
                    Filters =
                        {
                            new FilterExpression
                            {
                                Conditions =
                                {
                                    new ConditionExpression(IoTAlertProperties.MsdynAlertToken, ConditionOperator.Equal, alertToken),
                                    new ConditionExpression(IoTAlertProperties.StateCode, ConditionOperator.NotEqual, IotAlertStateCode.Inactive.ToString()),
                                },
                            },
                        },
                },
                };

                // run the query to determine if the IoT Alert already exists in Dynamics
                EntityCollection queryExpressionResult = GetExecutionPolicy(log).Execute(() =>
                {
                    return this.cdsServiceClient.RetrieveMultiple(queryExpression);
                });

                if (queryExpressionResult.Entities.Count == 0)
                {
                    // IoT does not exist, update log to Application Insights
                    log.LogWarning($"An Existing IoT Alert with Alert Token {alertToken} not found in Dynamics");
                    alertId = Guid.Empty;
                }
                else
                {
                    // IoT Alert does exist, Update IoT Alert Status to InActive and Alert Data with latest Iconic Fault Data
                    alertId = queryExpressionResult.Entities[0].Id;
                }
            }

            return alertId;
        }

        /// <summary>
        /// Update existing IoT Alert to set status to InActive and update latest fault data.
        /// </summary>
        /// <param name="fault">Fault Data.</param>
        /// <param name="log">Log Content.</param>
        /// <param name="alertState">IoT Alert State Code.</param>
        /// <param name="alertStatus">IoT Alert Status Code.</param>
        public void UpdateIoTAlert(IconicsFault fault, ILogger log, int alertState = 0, int alertStatus = 0)
        {
            if (fault != null)
            {
                Guid alertId = this.GetIoTAlert(fault, log);
                if (alertId != Guid.Empty)
                {
                    Entity updateIoTAlert = new Entity(DynamicsEntities.IoTAlert);
                    updateIoTAlert.Id = alertId;
                    updateIoTAlert[IoTAlertProperties.MsdynAlertData] = JsonConvert.SerializeObject(fault);
                    updateIoTAlert[IoTAlertProperties.StateCode] = alertState;
                    updateIoTAlert[IoTAlertProperties.StatusCode] = alertStatus;

                    GetExecutionPolicy(log).Execute(() =>
                    {
                        this.cdsServiceClient.Update(updateIoTAlert);
                    });

                    log.LogInformation($"Retrieved IoT Alert {alertId}. Updated State of IoT Alert to InActive and Alert Data with latest Fault Data");
                }
            }
        }

        /// <summary>
        /// Method definition to check if CDS client service is ready.
        /// </summary>
        /// <returns>Status(true|false) of CDS Client Service.</returns>
        public bool IsCdsServiceReady()
        {
            CdsServiceClient cdsServiceClient = (CdsServiceClient)this.cdsServiceClient;
            return cdsServiceClient != null ? cdsServiceClient.IsReady : false;
        }

        /// <summary>
        /// Method definition to retrieve CDS Error.
        /// </summary>
        /// <returns>CDS Service Last CDS Error.</returns>
        public string RetrieveCdsError()
        {
            return ((CdsServiceClient)this.cdsServiceClient).LastCdsError;
        }

        /// <summary>
        /// Returns the number of retries, if exists and default value (3) if it doesn't.
        /// </summary>
        /// <param name="failureThreshold">Failure Threshold from appsettings, if defined.</param>
        /// <returns>Integer regarding number of retries.</returns>
        private static int ReturnFailureThreshold(string failureThreshold)
        {
            if (string.IsNullOrWhiteSpace(failureThreshold))
            {
                return 3;
            }

            return int.Parse(failureThreshold, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Returns the number initial delay between retries. If empty, return default (2).
        /// </summary>
        /// <param name="initialDelay">Initial delay from settings, if defined.</param>
        /// <returns>Integer regarding number of retries.</returns>
        private static int ReturnInitialDelay(string initialDelay)
        {
            if (string.IsNullOrWhiteSpace(initialDelay))
            {
                return 2;
            }

            return int.Parse(initialDelay, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Returns the constructed execution policy.
        /// </summary>
        /// <param name="log">Log provider mechanism.</param>
        /// <returns>Policy regarding the one to execute.</returns>
        private static Policy GetExecutionPolicy(ILogger log)
        {
            string failureThreshold = Environment.GetEnvironmentVariable("FailureThreshold");
            string initialDelay = Environment.GetEnvironmentVariable("InitialRetryInterval");

            return Policy
                    .Handle<Exception>()
                    .WaitAndRetry(
                        ReturnFailureThreshold(failureThreshold),
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(ReturnInitialDelay(initialDelay), retryAttempt)),
                        (exception, timeSpan, retryCount, context) =>
                        {
                            if (exception != null)
                            {
                                log.LogError("The retry attempt number {retryCount} raised the following exception: {exceptionMessage}", retryCount, exception.Message);
                            }
                            else
                            {
                                log.LogInformation("The operation succeeded on the attempt number {retryCount}.", retryCount);
                            }
                        });
        }
    }
}
