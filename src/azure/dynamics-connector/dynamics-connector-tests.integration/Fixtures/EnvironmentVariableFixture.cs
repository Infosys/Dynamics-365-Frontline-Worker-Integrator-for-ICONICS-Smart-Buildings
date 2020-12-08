// -----------------------------------------------------------------------
// <copyright file="EnvironmentVariableFixture.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using Xunit;

namespace DynamicsConnector.Tests.Integration
{
    public class EnvironmentVariableFixture : IDisposable
    {
        public EnvironmentVariableFixture()
        {
            string agentId = Environment.GetEnvironmentVariable("AGENT_ID", EnvironmentVariableTarget.Process);
            if (agentId == null)
            {
                // running outside of Azure Dev Ops, add environment variables
                using (var file = File.OpenText("../../../appsettings.json"))
                {
                    JsonTextReader reader = new JsonTextReader(file);
                    JObject jObject = JObject.Load(reader);
                    foreach (JProperty variable in jObject.Properties())
                    {
                        Environment.SetEnvironmentVariable(variable.Name, variable.Value.ToString());
                    }
                }
            }
        }

        public void Dispose() { }
    }

    [CollectionDefinition("Environment variable collection")]
    public class EnvironmentVariableCollection : ICollectionFixture<EnvironmentVariableFixture> { }
}
