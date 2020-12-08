// -----------------------------------------------------------------------
// <copyright file="IconicsFault.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace DynamicsConnector.Iconics.Models
{
    /// <summary>
    /// Contains Iconic Fault Properties information.
    /// </summary>
    public class IconicsFault
    {
        /// <summary>
        /// Gets or sets fault Asset Path.
        /// </summary>
        public string AssetPath { get; set; }

        /// <summary>
        /// Gets or sets fault Asset Name.
        /// </summary>
        public string AssetName { get; set; }

        /// <summary>
        /// Gets or sets fault Name.
        /// </summary>
        public string FaultName { get; set; }

        /// <summary>
        /// Gets or Sets Priority.
        /// </summary>
        public string Priority { get; set; }

        /// <summary>
        /// Gets or Sets FaultState.
        /// </summary>
        public string FaultState { get; set; }

        /// <summary>
        /// Gets or sets fault Active Time.
        /// </summary>
        public string FaultActiveTime { get; set; }

        /// <summary>
        /// Gets or Sets Fault Activation Variables.
        /// </summary>
        public string FaultActivationVariables { get; set; }

        /// <summary>
        /// Gets or sets fault Cost Value.
        /// </summary>
        public string FaultCostValue { get; set; }

        /// <summary>
        /// Gets or Sets Fault Deactivation Time.
        /// </summary>
        public string FaultDeactivationTime { get; set; }

        /// <summary>
        /// Gets or Sets Fault Deactivation Variables.
        /// </summary>
        public string FaultDeactivationVariables { get; set; }

        /// <summary>
        /// Gets or sets fault Related Value1.
        /// </summary>
        public string RelatedValue1 { get; set; }

        /// <summary>
        /// Gets or sets fault Related Value2.
        /// </summary>
        public string RelatedValue2 { get; set; }

        /// <summary>
        /// Gets or sets fault Related Value3.
        /// </summary>
        public string RelatedValue3 { get; set; }

        /// <summary>
        /// Gets or sets fault Related Value4.
        /// </summary>
        public string RelatedValue4 { get; set; }

        /// <summary>
        /// Gets or sets fault Related Value5.
        /// </summary>
        public string RelatedValue5 { get; set; }

        /// <summary>
        /// Gets or sets fault Related Value6.
        /// </summary>
        public string RelatedValue6 { get; set; }

        /// <summary>
        /// Gets or sets fault Related Value7.
        /// </summary>
        public string RelatedValue7 { get; set; }

        /// <summary>
        /// Gets or sets fault Related Value8.
        /// </summary>
        public string RelatedValue8 { get; set; }

        /// <summary>
        /// Gets or sets fault Related Value9.
        /// </summary>
        public string RelatedValue9 { get; set; }

        /// <summary>
        /// Gets or sets fault Related Value10.
        /// </summary>
        public string RelatedValue10 { get; set; }

        /// <summary>
        /// Gets or sets fault Related Value11.
        /// </summary>
        public string RelatedValue11 { get; set; }

        /// <summary>
        /// Gets or sets fault Related Value12.
        /// </summary>
        public string RelatedValue12 { get; set; }

        /// <summary>
        /// Gets or sets fault Related Value13.
        /// </summary>
        public string RelatedValue13 { get; set; }

        /// <summary>
        /// Gets or sets fault Related Value14.
        /// </summary>
        public string RelatedValue14 { get; set; }

        /// <summary>
        /// Gets or sets fault Related Value15.
        /// </summary>
        public string RelatedValue15 { get; set; }

        /// <summary>
        /// Gets or sets fault Related Value16.
        /// </summary>
        public string RelatedValue16 { get; set; }

        /// <summary>
        /// Gets or sets fault Related Value7.
        /// </summary>
        public string RelatedValue17 { get; set; }

        /// <summary>
        /// Gets or sets fault Related Value18.
        /// </summary>
        public string RelatedValue18 { get; set; }

        /// <summary>
        /// Gets or sets fault Related Value19.
        /// </summary>
        public string RelatedValue19 { get; set; }

        /// <summary>
        /// Gets or sets fault Related Value20.
        /// </summary>
        public string RelatedValue20 { get; set; }

        /// <summary>
        /// Gets or sets fault Message Source.
        /// </summary>
        public string MessageSource { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets EventProcessedUtcTime.
        /// </summary>
        public string EventProcessedUtcTime { get; set; }

        /// <summary>
        /// Gets or sets PartitionId.
        /// </summary>
        public int PartitionId { get; set; }

        /// <summary>
        /// Gets or sets EventEnqueuedUtcTime.
        /// </summary>
        public string EventEnqueuedUtcTime { get; set; }

        /// <summary>
        /// Gets or sets IoTHub.
        /// </summary>
        public IoTHubFaultData IoTHub { get; set; }
    }
}
