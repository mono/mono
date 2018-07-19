//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel.Channels;
    using System.ServiceModel;

    public sealed partial class OneWayElement : BindingElementExtensionElement
    {
        public OneWayElement() 
        {
        }

        public override Type BindingElementType
        {
            get { return typeof(OneWayBindingElement); }
        }

        [ConfigurationProperty(ConfigurationStrings.ChannelPoolSettings)]
        public ChannelPoolSettingsElement ChannelPoolSettings
        {
            get { return (ChannelPoolSettingsElement)base[ConfigurationStrings.ChannelPoolSettings]; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxAcceptedChannels, DefaultValue = OneWayDefaults.MaxAcceptedChannels)]
        [IntegerValidator(MinValue = 1)]
        public int MaxAcceptedChannels
        {
            get { return (int)base[ConfigurationStrings.MaxAcceptedChannels]; }
            set { base[ConfigurationStrings.MaxAcceptedChannels] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.PacketRoutable, DefaultValue = OneWayDefaults.PacketRoutable)]
        public bool PacketRoutable
        {
            get { return (bool)base[ConfigurationStrings.PacketRoutable]; }
            set { base[ConfigurationStrings.PacketRoutable] = value; }
        }
        
        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            OneWayBindingElement oneWayBindingElement = (OneWayBindingElement)bindingElement;
            PropertyInformationCollection propertyInfo = this.ElementInformation.Properties;
            if (propertyInfo[ConfigurationStrings.ChannelPoolSettings].ValueOrigin != PropertyValueOrigin.Default)
            {
#pragma warning suppress 56506 // Microsoft, base.ApplyConfiguration() validates the argument
                this.ChannelPoolSettings.ApplyConfiguration(oneWayBindingElement.ChannelPoolSettings);
            }
            oneWayBindingElement.MaxAcceptedChannels = this.MaxAcceptedChannels;
            oneWayBindingElement.PacketRoutable = this.PacketRoutable;
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            OneWayElement source = (OneWayElement)from;
#pragma warning suppress 56506 // Microsoft, base.CopyFrom() validates the argument
            PropertyInformationCollection propertyInfo = source.ElementInformation.Properties;
            if (propertyInfo[ConfigurationStrings.ChannelPoolSettings].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.ChannelPoolSettings.CopyFrom(source.ChannelPoolSettings);
            }
            this.MaxAcceptedChannels = source.MaxAcceptedChannels;
            this.PacketRoutable = source.PacketRoutable;
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            OneWayBindingElement source = (OneWayBindingElement)bindingElement;
            this.ChannelPoolSettings.InitializeFrom(source.ChannelPoolSettings);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxAcceptedChannels, source.MaxAcceptedChannels);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.PacketRoutable, source.PacketRoutable);
        }

        protected internal override BindingElement CreateBindingElement()
        {
            OneWayBindingElement result = new OneWayBindingElement();
            this.ApplyConfiguration(result);
            return result;
        }
    }
}



