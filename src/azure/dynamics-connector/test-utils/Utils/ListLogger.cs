// -----------------------------------------------------------------------
// <copyright file="ListLogger.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace TestUtils.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Holds an internal list of messages to evaluate during a testing.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ListLogger : ILogger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListLogger"/> class.
        /// </summary>
        public ListLogger()
        {
            this.Logs = new List<string>();
        }

        /// <summary>
        /// Gets string type List.
        /// </summary>
        public IList<string> Logs { get; }

        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <typeparam name="TState">The type of the state to begin scope for.</typeparam>
        /// <param name="state">The identifier for the scope.</param>
        /// <returns>A disposable object that ends the logical operation scope on dispose.</returns>
        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        /// <summary>
        /// Checks if the given logLevel is enabled.
        /// </summary>
        /// <param name="logLevel">Pass log level as parameter.</param>
        /// <returns>true if enabled; false otherwise.</returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
        }

        /// <summary>
        /// Writes a log entry.
        /// </summary>
        /// <typeparam name="TState">The type of the state to begin scope for.</typeparam>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="state">The identifier for the scope.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="formatter">Function to format the message.</param>
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            string message = null;
            if (formatter != null)
            {
                message = formatter(state, exception);
            }

            this.Logs.Add(message);
        }
    }
}