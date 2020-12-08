// -----------------------------------------------------------------------
// <copyright file="LoggerFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace TestUtils.Factories
{
    using System.Diagnostics.CodeAnalysis;
    using TestUtils.Utils;

    /// <summary>
    /// Produces instances of ILogger classes.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class LoggerFactory
    {
        /// <summary>
        /// Creates a new Logger instance.
        /// </summary>
        /// <returns>Logged messages available for evaluation in tests.</returns>
        public static ListLogger CreateLogger()
        {
            return new ListLogger();
        }
    }
}