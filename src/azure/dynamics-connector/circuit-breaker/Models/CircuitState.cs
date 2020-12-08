// -----------------------------------------------------------------------
// <copyright file="CircuitState.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace CircuitBreaker.Models
{
    /// <summary>
    /// Enum for Circuit state.
    /// </summary>
    public enum CircuitState
    {
        /// <summary>
        /// Variable declared to state circuit state as closed.
        /// </summary>
        Closed,

        /// <summary>
        /// Variable declared to state circuit state as open.
        /// </summary>
        Open,
    }
}