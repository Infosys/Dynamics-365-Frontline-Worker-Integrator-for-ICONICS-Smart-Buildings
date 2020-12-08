// -----------------------------------------------------------------------
// <copyright file="IIconicsService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace DynamicsConnector.Iconics.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Contains IconicService Interface information.
    /// </summary>
    public interface IIconicsService
    {
        /// <summary>
        /// Method declaration for sending Work Order Acknowledgment message to Cloud-to-Device.
        /// </summary>
        /// <param name="responseString">Work Order Acknowledgment Data.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SendWorkOrderMessageAsync(string responseString);
    }
}
