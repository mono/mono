//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel.Channels;

    public sealed partial class NamedPipeTransportElement : ConnectionOrientedTransportElement
    {
        public NamedPipeTransportElement()
        {
        }

        public override Type BindingElementType
        {
            get { return typeof(NamedPipeTransportBindingElement); }
        }

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            NamedPipeTransportBindingElement binding = (NamedPipeTransportBindingElement)bindingElement;
#pragma warning suppress 56506 //[....]; base.ApplyConfiguration above checks for bindingElement being null
            this.ConnectionPoolSettings.ApplyConfiguration(binding.ConnectionPoolSettings);
            this.PipeSettings.ApplyConfiguration(binding.PipeSettings);
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
#pragma warning suppress 56506 // [....], base.CopyFrom() validates the argument
            NamedPipeTransportBindingElement binding = (NamedPipeTransportBindingElement)bindingElement;
            this.ConnectionPoolSettings.InitializeFrom(binding.ConnectionPoolSettings);
            this.PipeSettings.InitializeFrom(binding.PipeSettings);
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            NamedPipeTransportElement source = (NamedPipeTransportElement)from;
#pragma warning suppress 56506 // [....], base.CopyFrom() validates the argument
            this.ConnectionPoolSettings.CopyFrom(source.ConnectionPoolSettings);
            this.PipeSettings.CopyFrom(source.PipeSettings);
        }

        protected override TransportBindingElement CreateDefaultBindingElement()
        {
            return new NamedPipeTransportBindingElement();
        }

        [ConfigurationProperty(ConfigurationStrings.ConnectionPoolSettings)]
        public NamedPipeConnectionPoolSettingsElement ConnectionPoolSettings
        {
            get { return (NamedPipeConnectionPoolSettingsElement)base[ConfigurationStrings.ConnectionPoolSettings]; }
            set { base[ConfigurationStrings.ConnectionPoolSettings] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.PipeSettings)]
        public NamedPipeSettingsElement PipeSettings
        {
            get { return (NamedPipeSettingsElement)base[ConfigurationStrings.PipeSettings]; }
            set { base[ConfigurationStrings.PipeSettings] = value; }
        }
    }
}



