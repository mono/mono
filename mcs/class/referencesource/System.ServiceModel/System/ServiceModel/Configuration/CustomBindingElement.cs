//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel.Channels;

    public partial class CustomBindingElement
        : NamedServiceModelExtensionCollectionElement<BindingElementExtensionElement>,
        ICollection<BindingElementExtensionElement>,
        IBindingConfigurationElement
    {
        public CustomBindingElement()
            : this(null)
        { }

        public CustomBindingElement(string name) :
            base(ConfigurationStrings.BindingElementExtensions, name)
        { }

        [ConfigurationProperty(ConfigurationStrings.CloseTimeout, DefaultValue = ServiceDefaults.CloseTimeoutString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan CloseTimeout
        {
            get { return (TimeSpan)base[ConfigurationStrings.CloseTimeout]; }
            set
            {
                base[ConfigurationStrings.CloseTimeout] = value;
                this.SetIsModified();
            }
        }

        [ConfigurationProperty(ConfigurationStrings.OpenTimeout, DefaultValue = ServiceDefaults.OpenTimeoutString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan OpenTimeout
        {
            get { return (TimeSpan)base[ConfigurationStrings.OpenTimeout]; }
            set
            {
                base[ConfigurationStrings.OpenTimeout] = value;
                this.SetIsModified();
            }
        }

        [ConfigurationProperty(ConfigurationStrings.ReceiveTimeout, DefaultValue = ServiceDefaults.ReceiveTimeoutString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan ReceiveTimeout
        {
            get { return (TimeSpan)base[ConfigurationStrings.ReceiveTimeout]; }
            set
            {
                base[ConfigurationStrings.ReceiveTimeout] = value;
                this.SetIsModified();
            }
        }

        [ConfigurationProperty(ConfigurationStrings.SendTimeout, DefaultValue = ServiceDefaults.SendTimeoutString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan SendTimeout
        {
            get { return (TimeSpan)base[ConfigurationStrings.SendTimeout]; }
            set
            {
                base[ConfigurationStrings.SendTimeout] = value;
                this.SetIsModified();
            }
        }

        public override void Add(BindingElementExtensionElement element)
        {
            if (null == element)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            BindingElementExtensionElement existingElement = null;
            if (!this.CanAddEncodingElement(element, ref existingElement))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigMessageEncodingAlreadyInBinding,
                    existingElement.ConfigurationElementName,
                    existingElement.GetType().AssemblyQualifiedName)));
            }
            else if (!this.CanAddStreamUpgradeElement(element, ref existingElement))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigStreamUpgradeElementAlreadyInBinding,
                    existingElement.ConfigurationElementName,
                    existingElement.GetType().AssemblyQualifiedName)));
            }
            else if (!this.CanAddTransportElement(element, ref existingElement))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigTransportAlreadyInBinding,
                    existingElement.ConfigurationElementName,
                    existingElement.GetType().AssemblyQualifiedName)));
            }
            else
            {
                base.Add(element);
            }
        }

        public void ApplyConfiguration(Binding binding)
        {
            if (null == binding)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            if (binding.GetType() != typeof(CustomBinding))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.ConfigInvalidTypeForBinding,
                    typeof(CustomBinding).AssemblyQualifiedName,
                    binding.GetType().AssemblyQualifiedName));
            }

            binding.CloseTimeout = this.CloseTimeout;
            binding.OpenTimeout = this.OpenTimeout;
            binding.ReceiveTimeout = this.ReceiveTimeout;
            binding.SendTimeout = this.SendTimeout;

            this.OnApplyConfiguration(binding);
        }

        public override bool CanAdd(BindingElementExtensionElement element)
        {
            if (null == element)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            BindingElementExtensionElement existingElement = null;
            return !this.ContainsKey(element.GetType()) && this.CanAddEncodingElement(element, ref existingElement) &&
                this.CanAddStreamUpgradeElement(element, ref existingElement) && this.CanAddTransportElement(element, ref existingElement);
        }

        bool CanAddEncodingElement(BindingElementExtensionElement element, ref BindingElementExtensionElement existingElement)
        {
            return this.CanAddExclusiveElement(typeof(MessageEncodingBindingElement), element.BindingElementType, ref existingElement);
        }

        bool CanAddExclusiveElement(Type exclusiveType, Type bindingElementType, ref BindingElementExtensionElement existingElement)
        {
            bool retval = true;
            if (exclusiveType.IsAssignableFrom(bindingElementType))
            {
                foreach (BindingElementExtensionElement existing in this)
                {
                    if (exclusiveType.IsAssignableFrom(existing.BindingElementType))
                    {
                        retval = false;
                        existingElement = existing;
                        break;
                    }
                }
            }
            return retval;
        }

        bool CanAddStreamUpgradeElement(BindingElementExtensionElement element, ref BindingElementExtensionElement existingElement)
        {
            return this.CanAddExclusiveElement(typeof(StreamUpgradeBindingElement), element.BindingElementType, ref existingElement);
        }

        bool CanAddTransportElement(BindingElementExtensionElement element, ref BindingElementExtensionElement existingElement)
        {
            return this.CanAddExclusiveElement(typeof(TransportBindingElement), element.BindingElementType, ref existingElement);
        }

        protected void OnApplyConfiguration(Binding binding)
        {
            CustomBinding theBinding = (CustomBinding)binding;
            foreach (BindingElementExtensionElement bindingConfig in this)
            {
                theBinding.Elements.Add(bindingConfig.CreateBindingElement());
            }
        }
    }
}

