//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Configuration
{
    using System;
    using System.Runtime;
    using System.Configuration;
    using System.ServiceModel.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Activities.Tracking.Configuration;
    using SR2 = System.ServiceModel.Activities.SR;

    public class EtwTrackingBehaviorElement : BehaviorExtensionElement
    {
        ConfigurationPropertyCollection properties;
        const string profileNameParameter = "profileName";

        public EtwTrackingBehaviorElement()
        {
        }

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationPropertyAttributeRule,
            Justification = "This property is defined by the base class to determine the type of the behavior.")]
        public override Type BehaviorType
        {
            get { return typeof(EtwTrackingBehavior); }
        }

        [ConfigurationProperty(profileNameParameter, DefaultValue = "", Options = ConfigurationPropertyOptions.IsKey)]
        [StringValidator(MinLength = 0)]
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule,
            MessageId = "System.ServiceModel.Activities.Configuration.EtwTrackingBehaviorElement.ProfileName",
            Justification = "StringValidator validates minimal size")]
        public string ProfileName
        {
            get
            {
                return (string)base[profileNameParameter];
            }

            set
            {
                base[profileNameParameter] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty(profileNameParameter, typeof(System.String), string.Empty, null, new System.Configuration.StringValidator(0, 2147483647, null), System.Configuration.ConfigurationPropertyOptions.IsKey));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

        protected internal override object CreateBehavior()
        {
            EtwTrackingBehavior trackingBehavior = new EtwTrackingBehavior
            {
                ProfileName = this.ProfileName
            };

            return trackingBehavior;
        }        
    }
}
