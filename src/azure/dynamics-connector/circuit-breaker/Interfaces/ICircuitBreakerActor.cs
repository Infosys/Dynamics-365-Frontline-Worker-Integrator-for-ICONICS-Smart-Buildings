// -----------------------------------------------------------------------
// <copyright file="ICircuitBreakerActor.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace CircuitBreaker.Interfaces
{
    using System.Threading.Tasks;
    using CircuitBreaker.Models;

    /// <summary>
    /// Interface for the circuit breaker entity.
    /// </summary>
    public interface ICircuitBreakerActor
    {
        /// <summary>
        /// Task to Add Failure.
        /// </summary>
        /// <param name="failureRequest">FailureRequest.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task AddFailure(FailureRequest failureRequest);

        /// <summary>
        /// Sets the circuit's state to Open.
        /// </summary>
        void OpenCircuit();

        /// <summary>
        /// Sets the circuit's state to Closed.
        /// </summary>
        void CloseCircuit();
    }
}