//------------------------------------------------------------------------------
// <copyright file="StateMachineStateQueryElement.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Activities.Tracking;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    /// <summary>
    /// Configure StateMachineStateQuery element in DotNetConfig.xsd.
    /// </summary>
    [Fx.Tag.XamlVisible(false)]
    public class StateMachineStateQueryElement : TrackingQueryElement
    {
        // NOTE: this string is implicitly dependancy on StateMachineTrackingRecord.StateMachineStateRecordName 
        // The value should not be updated without updating the referenced location too.
        private static readonly string StateMachineStateRecordName = "System.Activities.Statements.StateMachine";
        private ConfigurationPropertyCollection properties;

        /// <summary>
        /// Gets or sets the Activity name filter attribute in StateMachineTrackingQuery element.
        /// </summary>
        [ConfigurationProperty(TrackingConfigurationStrings.ActivityName, IsKey = true,
            DefaultValue = TrackingConfigurationStrings.StarWildcard)]
        [StringValidator(MinLength = 1)]
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule,
            MessageId = "System.ServiceModel.Activities.Tracking.Configuration.UserTrackingQueryElement.ActivityName",
            Justification = "StringValidator verifies minimum size")]
        public string ActivityName
        {
            get 
            { 
                return (string)base[TrackingConfigurationStrings.ActivityName]; 
            }

            set 
            { 
                base[TrackingConfigurationStrings.ActivityName] = value; 
            }
        }

        /// <summary>
        /// Gets the attributes in StateMachineTrackingQuery element.
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty(
                        TrackingConfigurationStrings.ActivityName, 
                        typeof(string), 
                        "*", 
                        null, 
                        new StringValidator(1, int.MaxValue, null), 
                        System.Configuration.ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }

                return this.properties;
            }
        }

        /// <summary>
        /// Creates a StateMachine-specific CustomTrackingQuery, if the user specifies StateMachineTrackingQuery element in app.config.
        /// </summary>
        /// <returns>
        /// A CustomTrackingQuery instance that tracks StateMachine specific TrackingRecord.
        /// </returns>
        protected override TrackingQuery NewTrackingQuery()
        {
            // NOTE: to avoid a strict dependency over System.Activities.Statements.dll
            // we rely on the fact that StateMachineTrackingQuery is
            // derived from CustomTrackingQuery and is only a
            // CustomTrackingQuery instance with a specific name.
            // Therefore, this method would essentially create a StateMachineTrackingQuery instance.
            return new CustomTrackingQuery
            {
                Name = StateMachineStateRecordName,
                ActivityName = this.ActivityName
            };
        }
    }
}
