// -----------------------------------------------------------------------
// <copyright file="IErrorQueueService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace DynamicsConnector.Dynamics.Interfaces
{
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for ErrorQueueService.
    /// </summary>
    public interface IErrorQueueService
    {
        /// <summary>
        /// Method declaration for sending message to Error Queue.
        /// </summary>
        /// <param name="messageContents">Original Message Content.</param>
        /// <param name="validationMessage">Validation Message.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SendMessageToErrorQueueAsync(string messageContents, string validationMessage);
    }
}
