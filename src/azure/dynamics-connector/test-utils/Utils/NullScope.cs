// -----------------------------------------------------------------------
// <copyright file="NullScope.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace TestUtils.Utils
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Mocks a scope for the test cases to pass to the ListLogger class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class NullScope : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NullScope"/> class.
        /// </summary>
        private NullScope()
        {
        }

        /// <summary>
        /// Gets instance.
        /// </summary>
        public static NullScope Instance { get; } = new NullScope();

        /// <summary>
        /// Releases unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged resources.
        /// </summary>
        /// <param name="disposing">False when called from a finalizer, and true when called from the IDisposable.Dispose method.</param>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
