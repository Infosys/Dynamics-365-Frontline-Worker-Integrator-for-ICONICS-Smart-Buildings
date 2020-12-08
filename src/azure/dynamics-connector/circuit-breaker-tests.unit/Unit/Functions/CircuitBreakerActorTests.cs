// -----------------------------------------------------------------------
// <copyright file="CircuitBreakerActorTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace CircuitBreakerTests.Unit
{
    using System;
    using System.Threading.Tasks;
    using CircuitBreaker.Functions;
    using CircuitBreaker.Models;
    using Microsoft.Azure.WebJobs.Extensions.DurableTask;
    using Moq;
    using TestUtils.Factories;
    using TestUtils.Utils;
    using Xunit;

    /// <summary>
    /// Contains method to Unit Test CircuitBreakerActor class.
    /// </summary>
    public class CircuitBreakerActorTests
    {
        private readonly Mock<IDurableClient> durableClientMock;
        private readonly ListLogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitBreakerActorTests"/> class.
        /// </summary>
        public CircuitBreakerActorTests()
        {
            Environment.SetEnvironmentVariable("WindowSize", "00:01:00");
            Environment.SetEnvironmentVariable("FailureThreshold", "2");

            // mock services passed to Function class constructor
            this.durableClientMock = new Mock<IDurableClient>();
            this.logger = LoggerFactory.CreateLogger();
        }

        /// <summary>
        /// Should Throw Null Exception When Request is null.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task AddFailureThrowExceptionWhenFailureRequestIsNull()
        {
            CircuitBreakerActor function = new CircuitBreakerActor(this.durableClientMock.Object, this.logger);

            Func<Task> testCode = async () => await function.AddFailure(null).ConfigureAwait(false);
            var ex = await Record.ExceptionAsync(testCode).ConfigureAwait(false);

            Assert.NotNull(ex);
            Assert.IsType<ArgumentNullException>(ex);
        }

        /// <summary>
        /// Should update failure window count when Add Failure method is called.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task AddFailureShouldCountIncomingEvents1()
        {
            this.durableClientMock.Setup(service => service.StartNewAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(string.Empty));

            CircuitBreakerActor function = new CircuitBreakerActor(this.durableClientMock.Object, this.logger);
            FailureRequest req = new FailureRequest
            {
                RequestId = "RequestId",
                FailureTime = DateTime.UtcNow,
                InstanceId = "InstanceId",
                ResourceId = "ResourceID",
            };

            req.RequestId = "Req1";
            await function.AddFailure(req).ConfigureAwait(false);

            string expectedLogText = "currently has 1 exceptions in the window";
            Assert.Contains(expectedLogText, this.logger.Logs[0], StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Should Verify OpenCircuit Orchestrator is called when the threshold is exceeded.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task AddFailureUponCrossingThreadholdShouldCallOpenCircuitOrchestrator()
        {
            Mock<IDurableClient> clientMock = new Mock<IDurableClient>();
            clientMock.Setup(service => service.StartNewAsync("OpenCircuit", It.IsAny<FailureRequest>())).Returns(Task.FromResult(string.Empty)).Verifiable();

            CircuitBreakerActor function = new CircuitBreakerActor(clientMock.Object, this.logger);
            FailureRequest req = new FailureRequest
            {
                RequestId = "RequestId",
                FailureTime = DateTime.UtcNow,
                InstanceId = "InstanceId",
                ResourceId = "ResourceID",
            };

            for (int i = 1; i <= 2; i++)
            {
                req.RequestId = $"Req{i}";
                req.FailureTime = req.FailureTime.AddSeconds(10);
                await function.AddFailure(req).ConfigureAwait(false);
            }

            string expectedLogText = "Break this circuit for entity";
            Assert.Contains(expectedLogText, this.logger.Logs[1], StringComparison.OrdinalIgnoreCase);

            Mock.Verify();
        }

        /// <summary>
        /// Should Verify that Warning log is generated if Circuit is open.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGenerateWarningLogIfCircuitIsAlreadyOpen()
        {
            this.durableClientMock.Setup(service => service.StartNewAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(string.Empty));

            CircuitBreakerActor function = new CircuitBreakerActor(this.durableClientMock.Object, this.logger);
            FailureRequest req = new FailureRequest
            {
                RequestId = "RequestId",
                FailureTime = DateTime.UtcNow,
                InstanceId = "InstanceId",
                ResourceId = "ResourceID",
            };

            for (int i = 1; i <= 3; i++)
            {
                req.RequestId = $"Req{i}";
                req.FailureTime = req.FailureTime.AddSeconds(10);
                await function.AddFailure(req).ConfigureAwait(false);
            }

            string expectedLogText = "Tried to add additional failure to";
            Assert.Contains(expectedLogText, this.logger.Logs[3], StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verify Circuit Closed State.
        /// </summary>
        [Fact]
        public void VeirfyClosedCircuitState()
        {
            CircuitBreakerActor function = new CircuitBreakerActor(this.durableClientMock.Object, this.logger);

            function.CloseCircuit();

            Assert.Equal(CircuitState.Closed, function.State);
        }

        /// <summary>
        /// Verify Circuit Open State.
        /// </summary>
        [Fact]
        public void VeirfyOpenCircuitState()
        {
            CircuitBreakerActor function = new CircuitBreakerActor(this.durableClientMock.Object, this.logger);

            function.OpenCircuit();

            Assert.Equal(CircuitState.Open, function.State);
        }
    }
}