// -----------------------------------------------------------------------
// <copyright file="DynamicsEntityServiceTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace DynamicsConnectorTests.Unit
{
    using System;
    using DynamicsConnector.Dynamics.Services;
    using DynamicsConnector.Iconics.Models;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Moq;
    using TestUtils.Utils;
    using Xunit;

    /// <summary>
    /// Contains Unit Test methods for Dynamics Entity Service.
    /// </summary>
    public class DynamicsEntityServiceTests
    {
        private readonly Mock<IOrganizationService> cdsServiceClientMock;

        // private readonly Mock<ILogger> loggerMock;
        private readonly ListLogger loggerMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicsEntityServiceTests"/> class.
        /// </summary>
        public DynamicsEntityServiceTests()
        {
            // mock IOrganizationService and ILogger
            this.cdsServiceClientMock = new Mock<IOrganizationService>();
            this.loggerMock = new ListLogger();
        }

        /// <summary>
        /// Create Asset If Not Exists Should Return Entity Id When Asset Exists.
        /// </summary>
        [Fact]
        public void CreateAssetIfNotExistsShouldReturnEntityIdWhenAssetExists()
        {
            // mock RetrieveMultiple call to retrieve an entity with a known id
            Guid assetId = Guid.NewGuid();
            EntityCollection queryExpressionResult = new EntityCollection();
            queryExpressionResult.Entities.Add(new Entity() { Id = assetId });
            this.cdsServiceClientMock.Setup(service => service.RetrieveMultiple(It.IsAny<QueryBase>())).Returns(queryExpressionResult);

            // create DynamicsEntityService to test
            DynamicsEntityService dynamicsEntityService = new DynamicsEntityService(this.cdsServiceClientMock.Object);

            // call CreateAssetIfNotExists
            IconicsFault iconicsFault = new IconicsFault()
            {
                AssetName = "TestAsset",
            };

            Guid returnedId = dynamicsEntityService.CreateAssetIfNotExists(iconicsFault, this.loggerMock);

            this.cdsServiceClientMock.Verify(x => x.RetrieveMultiple(It.IsAny<QueryBase>()), Times.Once);

            Assert.Equal($"Retrieved Asset {returnedId}.", this.loggerMock.Logs[0]);

            // verify that returned id is equal to the generated entity id
            Assert.Equal(assetId, returnedId);
        }

        /// <summary>
        /// Create Asset If Not Exists Should Return New Id When Asset Does Not Exist.
        /// </summary>
        [Fact]
        public void CreateAssetIfNotExistsShouldReturnNewIdWhenAssetDoesNotExist()
        {
            // mock RetrieveMultiple call to retrieve an empty entity collection
            EntityCollection queryExpressionResult = new EntityCollection();
            this.cdsServiceClientMock.Setup(service => service.RetrieveMultiple(It.IsAny<QueryBase>())).Returns(queryExpressionResult);

            // mock Create call to retrieve a known asset id
            Guid assetId = Guid.NewGuid();
            this.cdsServiceClientMock.Setup(service => service.Create(It.IsAny<Entity>())).Returns(assetId);

            // create DynamicsEntityService to test
            DynamicsEntityService dynamicsEntityService = new DynamicsEntityService(this.cdsServiceClientMock.Object);

            // call CreateAssetIfNotExists
            IconicsFault iconicsFault = new IconicsFault()
            {
                AssetName = "TestAsset",
                FaultName = "TestFaultName",
                FaultActiveTime = "2020-08-13T20:01:04.6565528Z",
                AssetPath = "TestAssetPath",
            };

            Guid returnedId = dynamicsEntityService.CreateAssetIfNotExists(iconicsFault, this.loggerMock);

            this.cdsServiceClientMock.Verify(x => x.RetrieveMultiple(It.IsAny<QueryBase>()), Times.Once);
            this.cdsServiceClientMock.Verify(x => x.Create(It.IsAny<Entity>()), Times.Once);

            Assert.Equal(
                $"Created Asset {returnedId} from Fault (FaultName: {iconicsFault.FaultName}, FaultActiveTime: {iconicsFault.FaultActiveTime}, AssetName: {iconicsFault.AssetName}, AssetPath: {iconicsFault.AssetPath}).",
                this.loggerMock.Logs[0]);

            // verify that returned id is equal to the generated entity id
            Assert.Equal(assetId, returnedId);
        }

        /// <summary>
        /// Create Alert Should Return New Id.
        /// </summary>
        [Fact]
        public void CreateAlertShouldReturnNewId()
        {
            // mock Create call to retrieve a known alert id
            Guid alertId = Guid.NewGuid();
            this.cdsServiceClientMock.Setup(service => service.Create(It.IsAny<Entity>())).Returns(alertId);

            // create DynamicsEntityService to test
            DynamicsEntityService dynamicsEntityService = new DynamicsEntityService(this.cdsServiceClientMock.Object);

            // call CreateAlert
            Guid assetId = Guid.NewGuid();
            IconicsFault iconicsFault = new IconicsFault()
            {
                FaultName = "TestFault",
                AssetName = "TestAsset",
                AssetPath = "TestAssetPath",
                FaultState = "Active",
                FaultActiveTime = "2020-08-13T20:01:04.6565528Z",
            };
            Guid returnedId = dynamicsEntityService.CreateAlert(iconicsFault, assetId, this.loggerMock);

            this.cdsServiceClientMock.Verify(x => x.Create(It.IsAny<Entity>()), Times.Once);

            Assert.Equal(
                $"Created Alert {returnedId} from Fault (FaultName: {iconicsFault.FaultName}, FaultActiveTime: {iconicsFault.FaultActiveTime}, AssetName: {iconicsFault.AssetName}, AssetPath: {iconicsFault.AssetPath}).",
                this.loggerMock.Logs[0]);

            Assert.Equal($"Associated Alert {returnedId} with Asset {assetId}.", this.loggerMock.Logs[1]);

            // verify that returned id is equal to the generated entity id
            Assert.Equal(alertId, returnedId);
        }

        /// <summary>
        /// Get IoT Alert Should Return Alert Id.
        /// </summary>
        [Fact]
        public void GetIoTAlertShouldReturnAlertId()
        {
            // mock RetrieveMultiple call to retrieve an entity with a known id
            Guid alertId = Guid.NewGuid();
            EntityCollection queryExpressionResult = new EntityCollection();
            queryExpressionResult.Entities.Add(new Entity() { Id = alertId });
            this.cdsServiceClientMock.Setup(service => service.RetrieveMultiple(It.IsAny<QueryBase>())).Returns(queryExpressionResult);

            // create DynamicsEntityService to test
            DynamicsEntityService dynamicsEntityService = new DynamicsEntityService(this.cdsServiceClientMock.Object);

            // call GetIoTAlert
            IconicsFault iconicsFault = new IconicsFault()
            {
                FaultName = "TestFault",
                AssetName = "TestAsset",
                AssetPath = "TestAssetPath",
                FaultState = "Active",
                FaultActiveTime = "2020-08-13T20:01:04.6565528Z",
            };

            Guid returnedId = dynamicsEntityService.GetIoTAlert(iconicsFault, this.loggerMock);

            // verify that returned id is equal to the entity id
            Assert.Equal(alertId, returnedId);
        }

        /// <summary>
        /// Get IoT Alert Should Return Alert Id.
        /// </summary>
        [Fact]
        public void GetIoTAlertShouldreturnEmptywhenAlertdoesntexists()
        {
            // mock RetrieveMultiple call to retrieve an empty entity collection
            EntityCollection queryExpressionResult = new EntityCollection();
            this.cdsServiceClientMock.Setup(service => service.RetrieveMultiple(It.IsAny<QueryBase>())).Returns(queryExpressionResult);
            Guid alertId = Guid.Empty;

            // create DynamicsEntityService to test
            DynamicsEntityService dynamicsEntityService = new DynamicsEntityService(this.cdsServiceClientMock.Object);

            // call GetIoTAlert
            IconicsFault iconicsFault = new IconicsFault()
            {
                FaultName = "TestFault",
                AssetName = "TestAsset",
                AssetPath = "TestAssetPath",
                FaultState = "Active",
                FaultActiveTime = "2020-08-13T20:01:04.6565528Z",
            };

            Guid returnedId = dynamicsEntityService.GetIoTAlert(iconicsFault, this.loggerMock);

            // verify that Empty is returned and correct message is logged
            Assert.Equal(alertId, returnedId);
            string expectedLogText = "An Existing IoT Alert with Alert Token";
            Assert.Contains(expectedLogText, this.loggerMock.Logs[0], StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Update IoT Alert Should update Alert data and Alert Status.
        /// </summary>
        [Fact]
        public void UpdateIoTAlertShouldUpdateAlertInfo()
        {
            // mock RetrieveMultiple call to retrieve an entity with a known id
            Guid alertId = Guid.NewGuid();
            EntityCollection queryExpressionResult = new EntityCollection();
            queryExpressionResult.Entities.Add(new Entity() { Id = alertId });
            this.cdsServiceClientMock.Setup(service => service.RetrieveMultiple(It.IsAny<QueryBase>())).Returns(queryExpressionResult);

            Guid updatedAlertid = Guid.Empty;
            this.cdsServiceClientMock.Setup(service => service.Update(It.IsAny<Entity>())).Callback<Entity>(entity => updatedAlertid = entity.Id);

            // create DynamicsEntityService to test
            DynamicsEntityService dynamicsEntityService = new DynamicsEntityService(this.cdsServiceClientMock.Object);

            IconicsFault iconicsFault = new IconicsFault()
            {
                FaultName = "TestFault",
                AssetName = "TestAsset",
                AssetPath = "TestAssetPath",
                FaultState = "InActive",
                FaultActiveTime = "2020-08-13T20:01:04.6565528Z",
            };

            // call UpdateIoTAlert
            dynamicsEntityService.UpdateIoTAlert(iconicsFault, this.loggerMock, 0, 0);

            this.cdsServiceClientMock.Verify(service => service.Update(It.IsAny<Entity>()), Times.Once);
            this.cdsServiceClientMock.Verify(service => service.RetrieveMultiple(It.IsAny<QueryBase>()), Times.Once);

            // verify that the correct id is passed into Update call
            Assert.Equal(alertId, updatedAlertid);
            string expectedupdatelog = "Updated State of IoT Alert to InActive and Alert Data with latest Fault Data";

            // Verify that the correct message is logged from UpdateIoTAlert
            Assert.Contains(expectedupdatelog, this.loggerMock.Logs[0], StringComparison.OrdinalIgnoreCase);
        }
    }
}
