// -----------------------------------------------------------------------
// <copyright file="AssetAlertVerification.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace DynamicsConnector.Tests.Integration.Models
{
    public class AssetAlertVerification
    {
        public bool assetVerified { get; set; }
        public bool alertVerified { get; set; }
        public Guid verifiedAssetId { get; set; }
        public Guid verifiedAlertId { get; set; }
    }
}
