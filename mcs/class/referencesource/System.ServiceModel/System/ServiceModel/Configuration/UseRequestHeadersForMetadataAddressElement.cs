//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel.Description;
    using System.Collections.Generic;
    using System.ComponentModel;

    public sealed partial class UseRequestHeadersForMetadataAddressElement : BehaviorExtensionElement
    {
        public UseRequestHeadersForMetadataAddressElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.DefaultPorts)]
        public DefaultPortElementCollection DefaultPorts
        {
            get { return (DefaultPortElementCollection)base[ConfigurationStrings.DefaultPorts]; }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            UseRequestHeadersForMetadataAddressElement source = (UseRequestHeadersForMetadataAddressElement)from;
            this.DefaultPorts.Clear();
            foreach (DefaultPortElement DefaultPort in source.DefaultPorts)
            {
                this.DefaultPorts.Add(new DefaultPortElement(DefaultPort));
            }
        }

        protected internal override object CreateBehavior()
        {
            UseRequestHeadersForMetadataAddressBehavior behavior = new UseRequestHeadersForMetadataAddressBehavior();
            foreach (DefaultPortElement DefaultPort in this.DefaultPorts)
            {
                behavior.DefaultPortsByScheme.Add(DefaultPort.Scheme, DefaultPort.Port);
            }
            return behavior;
        }

        public override Type BehaviorType
        {
            get { return typeof(UseRequestHeadersForMetadataAddressBehavior); }
        }
    }
}
