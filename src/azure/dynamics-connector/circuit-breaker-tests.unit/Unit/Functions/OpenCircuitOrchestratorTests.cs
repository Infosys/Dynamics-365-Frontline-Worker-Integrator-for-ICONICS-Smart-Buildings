// -----------------------------------------------------------------------
// <copyright file="OpenCircuitOrchestratorTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace CircuitBreakerTests.Unit
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using CircuitBreaker.Functions;
    using CircuitBreaker.Models;
    using Microsoft.Azure.WebJobs.Extensions.DurableTask;
    using Moq;
    using TestUtils.Factories;
    using TestUtils.Utils;
    using Xunit;

    /// <summary>
    /// Contains method to Unit Test OpenCircuitOrchestrator class.
    /// </summary>
    public class OpenCircuitOrchestratorTests
    {
        private readonly Mock<IDurableOrchestrationContext> durableOrchestrationContextMock;
        private readonly ListLogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenCircuitOrchestratorTests"/> class.
        /// </summary>
        public OpenCircuitOrchestratorTests()
        {
            // mock services passed to Function class constructor
            this.durableOrchestrationContextMock = new Mock<IDurableOrchestrationContext>();
            this.logger = LoggerFactory.CreateLogger();
        }

        /// <summary>
        /// Should Throw Null Exception When Context is null.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldThrowExceptionWhenContextIsNull()
        {
            OpenCircuitOrchestrator function = new OpenCircuitOrchestrator();
            Func<Task> testCode = async () => await function.OpenCircuit(null, this.logger).ConfigureAwait(false);

            var ex = await Record.ExceptionAsync(testCode).ConfigureAwait(false);
            Assert.NotNull(ex);
            Assert.IsType<ArgumentNullException>(ex);
        }

        /// <summary>
        /// Should Verify CallHttpAsync is Called.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task VerifyOpenCircuitFunctionInvokesCallHttpAsync()
        {
            FailureRequest mockFailureRequest = new FailureRequest
            {
                FailureTime = DateTime.UtcNow,
                InstanceId = "TEST-INSTANCEID",
                RequestId = "TEST-REQUESTID",
                ResourceId = "TEST-RESOURCEID",
            };

            this.durableOrchestrationContextMock.Setup(client => client.GetInput<FailureRequest>()).Returns(mockFailureRequest);
            this.durableOrchestrationContextMock.Setup(client => client.CallHttpAsync(It.IsAny<DurableHttpRequest>())).Returns(Task.FromResult(new DurableHttpResponse(HttpStatusCode.OK)));

            OpenCircuitOrchestrator function = new OpenCircuitOrchestrator();

            await function.OpenCircuit(this.durableOrchestrationContextMock.Object, this.logger).ConfigureAwait(false);

            this.durableOrchestrationContextMock.Verify(service => service.CallHttpAsync(It.IsAny<DurableHttpRequest>()), Times.Once);

            string expectedLogText = "Successfully STOPPED Azure Function with Resource ID";
            Assert.Contains(expectedLogText, this.logger.Logs[1], StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Exception to be raised when CallHttpAsync return non-Status OK response.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task VerifyExceptionWhenCallHttpAsyncReturnsNonStatusOK()
        {
            FailureRequest mockFailureRequest = new FailureRequest
            {
                FailureTime = DateTime.UtcNow,
                InstanceId = "TEST-INSTANCEID",
                RequestId = "TEST-REQUESTID",
                ResourceId = "TEST-RESOURCEID",
            };

            this.durableOrchestrationContextMock.Setup(client => client.GetInput<FailureRequest>()).Returns(mockFailureRequest);
            this.durableOrchestrationContextMock.Setup(client => client.CallHttpAsync(It.IsAny<DurableHttpRequest>())).Returns(Task.FromResult(new DurableHttpResponse(HttpStatusCode.BadRequest)));

            OpenCircuitOrchestrator function = new OpenCircuitOrchestrator();

            Func<Task> testCode = async () => await function.OpenCircuit(this.durableOrchestrationContextMock.Object, this.logger).ConfigureAwait(false);

            var ex = await Record.ExceptionAsync(testCode).ConfigureAwait(false);
            Assert.NotNull(ex);
            Assert.IsType<ApplicationException>(ex);

            string expectedLog = "Failed to stop Function App";
            Assert.Contains(expectedLog, ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}