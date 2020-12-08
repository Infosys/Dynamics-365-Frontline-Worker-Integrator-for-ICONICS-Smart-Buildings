// -----------------------------------------------------------------------
// <copyright file="CreateAlertFromIconicsFaultTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace DynamicsConnectorTests.Unit
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using CircuitBreaker.Models;
    using DynamicsConnector;
    using DynamicsConnector.Dynamics.Interfaces;
    using DynamicsConnector.Functions;
    using DynamicsConnector.Iconics.Interfaces;
    using DynamicsConnector.Iconics.Models;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.WebJobs.Extensions.DurableTask;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Moq.Protected;
    using Newtonsoft.Json;
    using TestUtils.Factories;
    using TestUtils.Utils;
    using Xunit;
    using LoggerFactory = TestUtils.Factories.LoggerFactory;

    /// <summary>
    /// Contains Unit Test methods for CreateAlertFromIconicsFault.
    /// </summary>
    public class CreateAlertFromIconicsFaultTests
    {
        // Application Insights instrumentation ID for testing.
        private const string INSTRUMENTATIONID = "FAKE KEY";

        private readonly Mock<IDynamicsEntityService> dynamicsEntityServiceMock;
        private readonly Mock<IValidationService> validationServiceMock;
        private readonly Mock<IErrorQueueService> errorQueueServiceMock;
        private readonly Mock<HttpClient> httpClientMock;
        private readonly InstanceId instanceId = new InstanceId { Id = Guid.NewGuid().ToString() };
        private readonly ListLogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAlertFromIconicsFaultTests"/> class.
        /// </summary>
        public CreateAlertFromIconicsFaultTests()
        {
            Environment.SetEnvironmentVariable("ResourceId", "ResourceId");
            Environment.SetEnvironmentVariable("CircuitRequestUri", "http://localhost/api/CircuitBreakerActor/AddFailure");
            Environment.SetEnvironmentVariable("AppName", "AppName");
            Environment.SetEnvironmentVariable("CircuitCode", "CircuitCode");

            // mock services passed to Function class constructor
            this.dynamicsEntityServiceMock = new Mock<IDynamicsEntityService>();
            this.validationServiceMock = new Mock<IValidationService>();
            this.errorQueueServiceMock = new Mock<IErrorQueueService>();
            this.httpClientMock = new Mock<HttpClient>();
            this.logger = LoggerFactory.CreateLogger();
        }

        /// <summary>
        /// Should Run Successfully On Valid Active Fault.
        /// </summary>
        [Fact]
        public async void ShouldRunSuccessfullyOnValidActiveFault()
        {
            // mock IsValidFault and IsCdsServiceReady calls to return true
            string validationMessage;
            this.validationServiceMock.Setup(service => service.IsValidFault(It.IsAny<IconicsFault>(), out validationMessage)).Returns(true);
            this.dynamicsEntityServiceMock.Setup(service => service.IsCdsServiceReady()).Returns(true);

            var durableClientMock = new Mock<IDurableEntityClient>();

            // create message to pass to Run method
            IconicsFault iconicsFault = new IconicsFault()
            {
                FaultName = "FaultName",
                AssetName = "AssetName",
                AssetPath = "AssetPath",
                FaultState = "Active",
                FaultActiveTime = "2020-08-13T20:01:04.6565528Z",
                EventEnqueuedUtcTime = DateTime.UtcNow.ToString("o", CultureInfo.CurrentCulture),
            };

            string messageBody = JsonConvert.SerializeObject(iconicsFault);
            Message message = new Message(Encoding.UTF8.GetBytes(messageBody));

            using FakeTelemetryChannel fakeTelemetryChannel = new FakeTelemetryChannel();

            using TelemetryConfiguration telemetryConfiguration = new TelemetryConfiguration(INSTRUMENTATIONID, fakeTelemetryChannel);

            // instantiate function class and run function
            CreateAlertFromIconicsFault function = new CreateAlertFromIconicsFault(telemetryConfiguration, this.dynamicsEntityServiceMock.Object, this.validationServiceMock.Object, this.errorQueueServiceMock.Object, this.instanceId, this.httpClientMock.Object);
            await function.Run(message, this.logger).ConfigureAwait(false);

            // verify that asset and alert are created
            string msg;
            this.validationServiceMock.Verify(x => x.IsValidFault(It.IsAny<IconicsFault>(), out msg), Times.Once);
            this.dynamicsEntityServiceMock.Verify(x => x.IsCdsServiceReady(), Times.Once);
            this.dynamicsEntityServiceMock.Verify(service => service.CreateAssetIfNotExists(It.IsAny<IconicsFault>(), It.IsAny<ILogger>()), Times.Once);
            this.dynamicsEntityServiceMock.Verify(service => service.CreateAlert(It.IsAny<IconicsFault>(), It.IsAny<Guid>(), It.IsAny<ILogger>()), Times.Once);

            var logMsg = this.logger.Logs[0];
            Assert.Equal(
                $"Received ICONICS fault with FaultName '{iconicsFault.FaultName}' and FaultActiveTime of '{iconicsFault.FaultActiveTime}' and AssetPath of '{iconicsFault.AssetPath}'.",
                logMsg);
        }

        /// <summary>
        /// Should Run Successfully On Valid InActive Fault.
        /// </summary>
        [Fact]
        public async void ShouldRunSuccessfullyOnValidInActiveFault()
        {
            // mock IsValidFault and IsCdsServiceReady calls to return true
            string validationMessage;
            this.validationServiceMock.Setup(service => service.IsValidFault(It.IsAny<IconicsFault>(), out validationMessage)).Returns(true);
            this.dynamicsEntityServiceMock.Setup(service => service.IsCdsServiceReady()).Returns(true);

            var durableClientMock = new Mock<IDurableEntityClient>();

            // create message to pass to Run method
            IconicsFault iconicsFault = new IconicsFault()
            {
                FaultName = "FaultName",
                AssetName = "AssetName",
                AssetPath = "AssetPath",
                FaultState = "InActive",
                FaultActiveTime = "2020-08-13T20:01:04.6565528Z",
                EventEnqueuedUtcTime = DateTime.UtcNow.ToString("o", CultureInfo.CurrentCulture),
            };

            string messageBody = JsonConvert.SerializeObject(iconicsFault);
            Message message = new Message(Encoding.UTF8.GetBytes(messageBody));

            // instantiate function class and run function
            using (TelemetryConfiguration tc = new TelemetryConfiguration(INSTRUMENTATIONID))
            {
                // instantiate function class and run function
                CreateAlertFromIconicsFault function = new CreateAlertFromIconicsFault(tc, this.dynamicsEntityServiceMock.Object, this.validationServiceMock.Object, this.errorQueueServiceMock.Object, this.instanceId, this.httpClientMock.Object);
                await function.Run(message, this.logger).ConfigureAwait(false);
            }

            // verify that alert is updated
            this.dynamicsEntityServiceMock.Verify(service => service.UpdateIoTAlert(It.IsAny<IconicsFault>(), It.IsAny<ILogger>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

        /// <summary>
        /// Should Run Unsuccessfully On Invalid Fault.
        /// </summary>
        [Fact]
        public async void ShouldRunUnsuccessfullyOnInvalidFault()
        {
            // mock IsValidFault call to return false
            string validationMessage;
            this.validationServiceMock.Setup(service => service.IsValidFault(It.IsAny<IconicsFault>(), out validationMessage)).Returns(false);

            // create message to pass to Run method
            IconicsFault iconicsFault = new IconicsFault()
            {
                FaultName = "FaultName",
                AssetName = "AssetName",
                AssetPath = "AssetPath",
                EventEnqueuedUtcTime = DateTime.UtcNow.ToString("o", CultureInfo.CurrentCulture),
            };

            string messageBody = JsonConvert.SerializeObject(iconicsFault);
            Message message = new Message(Encoding.UTF8.GetBytes(messageBody));

            var durableClientMock = new Mock<IDurableEntityClient>();

            // set actualMessageContents equal to the first parameter passed into the SendMessageToErrorQueue method which should be called within the Run method
            string actualMessageContents = null;
            this.errorQueueServiceMock.Setup(service => service.SendMessageToErrorQueueAsync(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((messageContents, validationMessage) => actualMessageContents = messageContents);

            using FakeTelemetryChannel fakeTelemetryChannel = new FakeTelemetryChannel();

            using TelemetryConfiguration telemetryConfiguration = new TelemetryConfiguration(INSTRUMENTATIONID, fakeTelemetryChannel);

            // instantiate function class and run function
            CreateAlertFromIconicsFault function = new CreateAlertFromIconicsFault(telemetryConfiguration, this.dynamicsEntityServiceMock.Object, this.validationServiceMock.Object, this.errorQueueServiceMock.Object, this.instanceId, this.httpClientMock.Object);
            await function.Run(message, this.logger).ConfigureAwait(false);

            string msg;
            this.validationServiceMock.Verify(x => x.IsValidFault(It.IsAny<IconicsFault>(), out msg), Times.Once);

            // verify that SendMessageToErrorQueue was called and received the correct messageContents parameter
            string expectedErrorText = "Iconics fault data failed validation. Sending message to the error queue.";
            this.errorQueueServiceMock.Verify(service => service.SendMessageToErrorQueueAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.Equal(Encoding.UTF8.GetString(message.Body), actualMessageContents);
            Assert.Equal(expectedErrorText, this.logger.Logs[1]);
        }

        /// <summary>
        /// Should Run Unsuccessfully On Invalid Service Bus Message.
        /// </summary>
        [Fact]
        public async void ShouldRunUnsuccessfullyOnInvalidServiceBusMessage()
        {
            // create message to pass to Run method that will cause deserialization error
            dynamic iconicsFault = new
            {
                PartitionId = "PartitionId",
            };
            string messageBody = JsonConvert.SerializeObject(iconicsFault);
            Message message = new Message(Encoding.UTF8.GetBytes(messageBody));

            // set actualMessageContents equal to the first parameter passed into the SendMessageToErrorQueue method which should be called within the Run method
            string actualMessageContents = null;
            this.errorQueueServiceMock.Setup(service => service.SendMessageToErrorQueueAsync(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((messageContents, validationMessage) => actualMessageContents = messageContents);

            using FakeTelemetryChannel fakeTelemetryChannel = new FakeTelemetryChannel();

            using TelemetryConfiguration telemetryConfiguration = new TelemetryConfiguration(INSTRUMENTATIONID, fakeTelemetryChannel);

            var durableClientMock = new Mock<IDurableEntityClient>();

            // instantiate function class and run function
            CreateAlertFromIconicsFault function = new CreateAlertFromIconicsFault(telemetryConfiguration, this.dynamicsEntityServiceMock.Object, this.validationServiceMock.Object, this.errorQueueServiceMock.Object, this.instanceId, this.httpClientMock.Object);
            await function.Run(message, this.logger).ConfigureAwait(false);

            this.errorQueueServiceMock.Verify(x => x.SendMessageToErrorQueueAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            // verify that SendMessageToErrorQueue received the correct messageContents parameter and that the correct error is logged
            string expectedErrorText = "Exception while processing message:";
            this.errorQueueServiceMock.Verify(service => service.SendMessageToErrorQueueAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.Equal(Encoding.UTF8.GetString(message.Body), actualMessageContents);
            Assert.Contains(expectedErrorText, this.logger.Logs[0], StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Should Throw Error On Valid Fault When Cds Service Is Not Ready.
        /// </summary>
        [Fact]
        public async void ShouldThrowErrorOnValidFaultWhenCdsServiceIsNotReady()
        {
            // mock IsValidFault call to return true, but IsCdsServiceReady to return false
            string validationMessage;
            this.validationServiceMock.Setup(service => service.IsValidFault(It.IsAny<IconicsFault>(), out validationMessage)).Returns(true).Verifiable();
            this.dynamicsEntityServiceMock.Setup(service => service.IsCdsServiceReady()).Returns(false).Verifiable();

            // create message to pass to Run method
            IconicsFault iconicsFault = new IconicsFault()
            {
                FaultName = "FaultName",
                AssetName = "AssetName",
                AssetPath = "AssetPath",
            };

            string messageBody = JsonConvert.SerializeObject(iconicsFault);
            Message message = new Message(Encoding.UTF8.GetBytes(messageBody)) { MessageId = Guid.NewGuid().ToString() };

            using FakeTelemetryChannel fakeTelemetryChannel = new FakeTelemetryChannel();

            using TelemetryConfiguration telemetryConfiguration = new TelemetryConfiguration(INSTRUMENTATIONID, fakeTelemetryChannel);

            using HttpResponseMessage httpResponseMsg = new HttpResponseMessage { StatusCode = HttpStatusCode.Accepted };

            // Setting up a mock message handler to ensure that HttpClient.PostAsJsonAsync is called.
            // PostAsJsonAsync is an extension method, and thus cannot use normal setup/verify steps.
            // Using a mock messge handler to verify the handler's SendAsync method is called.
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMsg).Verifiable();

            using HttpClient httpClient = new HttpClient(mockHttpMessageHandler.Object);

            // instantiate function class and run function
            CreateAlertFromIconicsFault function = new CreateAlertFromIconicsFault(
                telemetryConfiguration,
                this.dynamicsEntityServiceMock.Object,
                this.validationServiceMock.Object,
                this.errorQueueServiceMock.Object,
                this.instanceId,
                httpClient);

            await Assert.ThrowsAsync<ApplicationException>(async () => await function.Run(message, this.logger).ConfigureAwait(false)).ConfigureAwait(false);

            // verify that error is thrown and that correct error is logged
            string expectedErrorText = "Failed to connect to the CDS";
            Assert.Contains(expectedErrorText, this.logger.Logs[1], StringComparison.OrdinalIgnoreCase);

            Mock.Verify();
        }

        /// <summary>
        /// Should log a warning log statement when the fault EventEnqueuedUtcTime is unable to be parsed to a DateTime.
        /// </summary>
        [Fact]
        public async void ShouldLogWarningIfEventEnqueuedTimeUnavailable()
        {
            // mock IsValidFault call to return true, but IsCdsServiceReady to return false
            string validationMessage;
            this.validationServiceMock.Setup(service => service.IsValidFault(It.IsAny<IconicsFault>(), out validationMessage)).Returns(true);
            this.dynamicsEntityServiceMock.Setup(service => service.IsCdsServiceReady()).Returns(true);

            var durableClientMock = new Mock<IDurableEntityClient>();

            // create message to pass to Run method
            IconicsFault iconicsFault = new IconicsFault()
            {
                FaultName = "FaultName",
                FaultActiveTime = "2020-08-13T20:01:04.6565528Z",
                AssetName = "AssetName",
                AssetPath = "AssetPath",
            };

            string messageBody = JsonConvert.SerializeObject(iconicsFault);
            Message message = new Message(Encoding.UTF8.GetBytes(messageBody));

            using FakeTelemetryChannel fakeTelemetryChannel = new FakeTelemetryChannel();

            using TelemetryConfiguration telemetryConfiguration = new TelemetryConfiguration(INSTRUMENTATIONID, fakeTelemetryChannel);

            // instantiate function class and run function
            CreateAlertFromIconicsFault function = new CreateAlertFromIconicsFault(telemetryConfiguration, this.dynamicsEntityServiceMock.Object, this.validationServiceMock.Object, this.errorQueueServiceMock.Object, this.instanceId, this.httpClientMock.Object);

            await function.Run(message, this.logger).ConfigureAwait(false);

            string msg;
            this.validationServiceMock.Verify(x => x.IsValidFault(It.IsAny<IconicsFault>(), out msg), Times.Once);
            this.dynamicsEntityServiceMock.Verify(x => x.IsCdsServiceReady(), Times.Once);

            Assert.Empty(fakeTelemetryChannel.SentEventTelemetries);
            Assert.Equal("Unable to determine IoT alert created elapsed time due to missing fault event enqueued time.", this.logger.Logs[1]);
        }

        /// <summary>
        /// Should log a warning log statement when the fault EventEnqueuedUtcTime is unable to be parsed to a DateTime.
        /// </summary>
        [Fact]
        public async void ShouldRecordIoTAlerCreatedTelemetryEvent()
        {
            // mock IsValidFault call to return true, but IsCdsServiceReady to return false
            string validationMessage;
            this.validationServiceMock.Setup(service => service.IsValidFault(It.IsAny<IconicsFault>(), out validationMessage)).Returns(true);
            this.dynamicsEntityServiceMock.Setup(service => service.IsCdsServiceReady()).Returns(true);

            var durableClientMock = new Mock<IDurableEntityClient>();

            using FakeTelemetryChannel fakeTelemetryChannel = new FakeTelemetryChannel();

            using TelemetryConfiguration telemetryConfiguration = new TelemetryConfiguration(INSTRUMENTATIONID, fakeTelemetryChannel);

            // create message to pass to Run method
            IconicsFault iconicsFault = new IconicsFault()
            {
                FaultName = "FaultName",
                FaultActiveTime = "2020-08-13T20:01:04.6565528Z",
                AssetName = "AssetName",
                AssetPath = "AssetPath",
                EventEnqueuedUtcTime = "2020-08-13T20:02:04.6565528Z",
            };

            string messageBody = JsonConvert.SerializeObject(iconicsFault);
            Message message = new Message(Encoding.UTF8.GetBytes(messageBody));

            // Instantiate function class and run function
            CreateAlertFromIconicsFault function = new CreateAlertFromIconicsFault(telemetryConfiguration, this.dynamicsEntityServiceMock.Object, this.validationServiceMock.Object, this.errorQueueServiceMock.Object, this.instanceId, this.httpClientMock.Object);

            await function.Run(message, this.logger).ConfigureAwait(false);

            string msg;
            this.validationServiceMock.Verify(x => x.IsValidFault(It.IsAny<IconicsFault>(), out msg), Times.Once);
            this.dynamicsEntityServiceMock.Verify(x => x.IsCdsServiceReady(), Times.Once);

            Assert.Single(fakeTelemetryChannel.SentEventTelemetries);

            if (fakeTelemetryChannel.SentEventTelemetries.Any())
            {
                var eventTelemetry = fakeTelemetryChannel.SentEventTelemetries.ToList()[0];

                Assert.NotNull(eventTelemetry);
                Assert.Equal("IoTAlertCreated", eventTelemetry.Name);

                Assert.Single(eventTelemetry.Metrics);
                Assert.True(eventTelemetry.Metrics.ContainsKey("AlertCreatedElapsedTimeMs"));
            }
        }
    }
}
