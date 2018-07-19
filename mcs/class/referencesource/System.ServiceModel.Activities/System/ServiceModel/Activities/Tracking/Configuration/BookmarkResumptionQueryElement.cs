//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Configuration;
    using System.Runtime;
    using System.Activities.Tracking;
    using System.Diagnostics.CodeAnalysis;

    [Fx.Tag.XamlVisible(false)]
    public class BookmarkResumptionQueryElement : TrackingQueryElement
    {
        ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty(TrackingConfigurationStrings.Name, typeof(System.String), "*", null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty(TrackingConfigurationStrings.Name, IsKey = true,
            DefaultValue = TrackingConfigurationStrings.StarWildcard)]
        [StringValidator(MinLength = 0)]
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule,
            MessageId = "System.ServiceModel.Activities.Tracking.Configuration.BookmarkResumptionQueryElement.Name",
            Justification = "StringValidator verifies minimum size")]
        public string Name
        {
            get { return (string)base[TrackingConfigurationStrings.Name]; }
            set { base[TrackingConfigurationStrings.Name] = value; }
        }

        protected override TrackingQuery NewTrackingQuery()
        {
            return new BookmarkResumptionQuery
                {
                    Name = this.Name
                };
        }
    }
}
