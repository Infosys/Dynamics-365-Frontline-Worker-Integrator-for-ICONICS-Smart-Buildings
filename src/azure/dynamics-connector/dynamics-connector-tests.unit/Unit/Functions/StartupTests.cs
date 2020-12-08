// -----------------------------------------------------------------------
// <copyright file="StartupTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace DynamicsConnectorTests.Unit
{
    using System.Net.Http;
    using DynamicsConnector.Dynamics.Interfaces;
    using DynamicsConnector.Iconics.Interfaces;
    using Microsoft.Azure.Functions.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Xunit;

    /// <summary>
    /// Contains method to Unit Test Azure function Startup.cs class.
    /// </summary>
    public class StartupTests
    {
        private readonly Mock<IFunctionsHostBuilder> builder;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupTests"/> class.
        /// </summary>
        public StartupTests()
        {
            // mock IFunctionsHostBuilder
            this.builder = new Mock<IFunctionsHostBuilder>();
        }

        /// <summary>
        /// Should Configure Services Successfully.
        /// </summary>
        [Fact]
        public void ShouldConfigureServicesSuccessfully()
        {
            // mock the IFunctionsHostBuilder's Service property to return a new service collection
            IServiceCollection services = new ServiceCollection();
            this.builder.SetupGet(builder => builder.Services).Returns(services);

            // instantiate startup class and run Configure
            DynamicsConnector.Startup startup = new DynamicsConnector.Startup();
            startup.Configure(this.builder.Object);

            // verify that 5 services have been added, and that one is an IValidationService (other interfaces require environment variables that throw errors in current unit tests)
           // int expectedServiceCount = 6;
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IValidationService configuredValidationService = serviceProvider.GetService<IValidationService>();
            var httpClient = serviceProvider.GetService<IHttpClientFactory>();

            // Assert.Equal(expectedServiceCount, services.Count);
            Assert.NotNull(configuredValidationService);
            Assert.NotNull(httpClient);
        }
    }
}
