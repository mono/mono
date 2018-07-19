//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel.Description;
    using System.ServiceModel.Web;

    public sealed partial class WebHttpElement : BehaviorExtensionElement
    {       
        ConfigurationPropertyCollection properties;
        
        public WebHttpElement()
        {
        }
        

        [ConfigurationProperty(WebConfigurationStrings.HelpEnabled)]
        public bool HelpEnabled
        {
            get { return (bool)base[WebConfigurationStrings.HelpEnabled]; }
            set { base[WebConfigurationStrings.HelpEnabled] = value; }
        }

        [ConfigurationProperty(WebConfigurationStrings.DefaultBodyStyle)]
        [InternalEnumValidator(typeof(WebMessageBodyStyleHelper))]
        public WebMessageBodyStyle DefaultBodyStyle
        {
            get { return (WebMessageBodyStyle)base[WebConfigurationStrings.DefaultBodyStyle]; }
            set { base[WebConfigurationStrings.DefaultBodyStyle] = value; }
        }

        [ConfigurationProperty(WebConfigurationStrings.DefaultOutgoingResponseFormat)]
        [InternalEnumValidator(typeof(WebMessageFormatHelper))]
        public WebMessageFormat DefaultOutgoingResponseFormat
        {
            get { return (WebMessageFormat)base[WebConfigurationStrings.DefaultOutgoingResponseFormat]; }
            set { base[WebConfigurationStrings.DefaultOutgoingResponseFormat] = value; }
        }

        [ConfigurationProperty(WebConfigurationStrings.AutomaticFormatSelectionEnabled)]
        public bool AutomaticFormatSelectionEnabled
        {
            get { return (bool)base[WebConfigurationStrings.AutomaticFormatSelectionEnabled]; }
            set { base[WebConfigurationStrings.AutomaticFormatSelectionEnabled] = value; }
        }

        [ConfigurationProperty(WebConfigurationStrings.FaultExceptionEnabled)]
        public bool FaultExceptionEnabled
        {
            get { return (bool)base[WebConfigurationStrings.FaultExceptionEnabled]; }
            set { base[WebConfigurationStrings.FaultExceptionEnabled] = value; }
        }

        protected override ConfigurationPropertyCollection Properties 
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();                    
                    properties.Add(new ConfigurationProperty(WebConfigurationStrings.HelpEnabled, typeof(bool), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(WebConfigurationStrings.DefaultBodyStyle, typeof(System.ServiceModel.Web.WebMessageBodyStyle), System.ServiceModel.Web.WebMessageBodyStyle.Bare, null, new System.ServiceModel.Configuration.InternalEnumValidator(typeof(System.ServiceModel.Web.WebMessageBodyStyleHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(WebConfigurationStrings.DefaultOutgoingResponseFormat, typeof(System.ServiceModel.Web.WebMessageFormat), System.ServiceModel.Web.WebMessageFormat.Xml, null, new System.ServiceModel.Configuration.InternalEnumValidator(typeof(System.ServiceModel.Web.WebMessageFormatHelper)), System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(WebConfigurationStrings.AutomaticFormatSelectionEnabled, typeof(bool), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(WebConfigurationStrings.FaultExceptionEnabled, typeof(bool), false, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Configuration", "Configuration102:ConfigurationPropertyAttributeRule", MessageId = "System.ServiceModel.Configuration.WebHttpElement.BehaviorType", Justification = "Not a configurable property; a property that had to be overridden from abstract parent class")]
        public override Type BehaviorType
        {
            get { return typeof(WebHttpBehavior); }
        }

        internal protected override object CreateBehavior()
        {
            return new WebHttpBehavior() 
            { 
                HelpEnabled = this.HelpEnabled, 
                DefaultBodyStyle = this.DefaultBodyStyle, 
                DefaultOutgoingResponseFormat = this.DefaultOutgoingResponseFormat,
                AutomaticFormatSelectionEnabled = this.AutomaticFormatSelectionEnabled, 
                FaultExceptionEnabled = this.FaultExceptionEnabled, 
            };
        }
    }
}
