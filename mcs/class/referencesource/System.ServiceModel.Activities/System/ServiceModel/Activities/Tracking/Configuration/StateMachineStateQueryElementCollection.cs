//------------------------------------------------------------------------------
// <copyright file="StateMachineStateQueryElementCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Configuration;
    using System.Runtime;

    /// <summary>
    /// Configure StateMachineStateQueries element in DotNetConfig.xsd file.
    /// </summary>
    [Fx.Tag.XamlVisible(false)]
    [ConfigurationCollection(typeof(StateMachineStateQueryElement),
        CollectionType = ConfigurationElementCollectionType.BasicMap,
        AddItemName = TrackingConfigurationStrings.StateMachineStateQuery)]
    public class StateMachineStateQueryElementCollection : TrackingConfigurationCollection<StateMachineStateQueryElement>
    {
        /// <summary>
        /// Generate the StateMachineTrackingQuery element in the DotNetConfig.xsd file.
        /// </summary>
        protected override string ElementName
        {
            get { return TrackingConfigurationStrings.StateMachineStateQuery; }
        }
    }
}
