// -----------------------------------------------------------------------
// <copyright file="CircuitBreakerFunctionTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace CircuitBreakerTests.Unit
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using CircuitBreaker.Functions;
    using CircuitBreaker.Interfaces;
    using CircuitBreaker.Models;
    using Microsoft.Azure.WebJobs.Extensions.DurableTask;
    using Moq;
    using Newtonsoft.Json;
    using NuGet.Frameworks;
    using TestUtils.Factories;
    using TestUtils.Utils;
    using Xunit;

    /// <summary>
    /// Contains method to Unit Test AddFailureRequest class.
    /// </summary>
    public class CircuitBreakerFunctionTests
    {
        private readonly Mock<IDurableClient> durableClientMock;
        private readonly ListLogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitBreakerFunctionTests"/> class.
        /// </summary>
        public CircuitBreakerFunctionTests()
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
        public async Task AddFailureFunctionShouldThrowExceptionWhenHttpRequestIsNull()
        {
            Func<Task> testCode = async () => await CircuitBreakerFunction.AddFailureFunction(null, this.durableClientMock.Object, this.logger, "testfunc").ConfigureAwait(false);
            var ex = await Record.ExceptionAsync(testCode).ConfigureAwait(false);

            Assert.NotNull(ex);
            Assert.IsType<ArgumentNullException>(ex);
        }

        /// <summary>
        /// Should Throw Null Exception When Client is null.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task AddFailureFunctionShouldThrowExceptionWhenDurableClienttIsNull()
        {
            HttpRequestMessage req = new HttpRequestMessage();

            Func<Task> testCode = async () => await CircuitBreakerFunction.AddFailureFunction(req, null, this.logger, "testfunc").ConfigureAwait(false);
            var ex = await Record.ExceptionAsync(testCode).ConfigureAwait(false);

            Assert.NotNull(ex);
            Assert.IsType<ArgumentNullException>(ex);

            req.Dispose();
        }

        /// <summary>
        /// Should call CircuitBreakerActor when Add Failure Function is called.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task AddFailureFunctionShouldCallCircuitBreakerActorEntity()
        {
            FailureRequest failureReq = new FailureRequest
            {
                RequestId = "RequestId",
                FailureTime = DateTime.UtcNow,
                InstanceId = "InstanceId",
                ResourceId = "ResourceId",
            };
            string entityKey = "AddFailureTest";

            var reqData = JsonConvert.SerializeObject(failureReq);
            using HttpRequestMessage req = new HttpRequestMessage
            {
                Content = new StringContent(reqData, Encoding.UTF8, "application/json"),
            };

            EntityId actualEntityId;
            this.durableClientMock
                .Setup(service => service.SignalEntityAsync<ICircuitBreakerActor>(It.IsAny<EntityId>(), It.IsAny<Action<ICircuitBreakerActor>>()))
                .Callback<EntityId, Action<ICircuitBreakerActor>>((entityId, operation) => actualEntityId = entityId)
                .Returns(Task.FromResult(string.Empty));

            await CircuitBreakerFunction.AddFailureFunction(req, this.durableClientMock.Object, this.logger, entityKey).ConfigureAwait(false);

            string expectedLogText = $"CircuitBreaker AddFailure triggered";
            Assert.Contains(expectedLogText, this.logger.Logs[0], StringComparison.OrdinalIgnoreCase);

            Assert.Equal(entityKey, actualEntityId.EntityKey);
            req.Dispose();
        }

        /// <summary>
        /// Should call CircuitBreakerActor when Close Circuit Function is called.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CloseCircuitFunctionShouldCallCircuitBreakerActorEntity()
        {
            string entityKey = "CloseCircuitTest";

            var reqData = JsonConvert.SerializeObject("{ }");
            using HttpRequestMessage req = new HttpRequestMessage
            {
                Content = new StringContent(reqData, Encoding.UTF8, "application/json"),
            };

            EntityId actualEntityId;
            this.durableClientMock
                .Setup(service => service.SignalEntityAsync<ICircuitBreakerActor>(It.IsAny<EntityId>(), It.IsAny<Action<ICircuitBreakerActor>>()))
                .Callback<EntityId, Action<ICircuitBreakerActor>>((entityId, operation) => actualEntityId = entityId)
                .Returns(Task.FromResult(string.Empty));

            await CircuitBreakerFunction.CloseCircuitFunction(req, this.durableClientMock.Object, this.logger, entityKey).ConfigureAwait(false);

            string expectedLogText = $"CircuitBreaker Close triggered for entity {entityKey}.";
            Assert.Contains(expectedLogText, this.logger.Logs[0], StringComparison.OrdinalIgnoreCase);

            Assert.Equal(entityKey, actualEntityId.EntityKey);
            req.Dispose();
        }
    }
}