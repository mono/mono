//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel.Channels;
    using System.ServiceModel;

    public sealed partial class CompositeDuplexElement : BindingElementExtensionElement
    {
        public CompositeDuplexElement() 
        {
        }

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            CompositeDuplexBindingElement binding = (CompositeDuplexBindingElement)bindingElement;
            PropertyInformationCollection propertyInfo = this.ElementInformation.Properties;
            if (propertyInfo[ConfigurationStrings.ClientBaseAddress].ValueOrigin != PropertyValueOrigin.Default)
            {
                binding.ClientBaseAddress = this.ClientBaseAddress;
            }
        }

        public override Type BindingElementType
        {
            get { return typeof(CompositeDuplexBindingElement); }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            CompositeDuplexElement source = (CompositeDuplexElement)from;
#pragma warning suppress 56506 // Microsoft, base.CopyFrom() validates the argument
            this.ClientBaseAddress = source.ClientBaseAddress;
        }

        protected internal override BindingElement CreateBindingElement()
        {
            CompositeDuplexBindingElement binding = new CompositeDuplexBindingElement();
            this.ApplyConfiguration(binding);
            return binding;
        }

        [ConfigurationProperty(ConfigurationStrings.ClientBaseAddress, DefaultValue = null)]
        public Uri ClientBaseAddress
        {
            get { return (Uri)base[ConfigurationStrings.ClientBaseAddress]; }
            set { base[ConfigurationStrings.ClientBaseAddress] = value; }
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            CompositeDuplexBindingElement binding = (CompositeDuplexBindingElement)bindingElement;
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ClientBaseAddress, binding.ClientBaseAddress);
        }

    }
}



