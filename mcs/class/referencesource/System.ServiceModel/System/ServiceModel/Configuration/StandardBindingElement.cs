//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public abstract partial class StandardBindingElement : ServiceModelConfigurationElement, IBindingConfigurationElement, IConfigurationContextProviderInternal
    {
        [Fx.Tag.SecurityNote(Critical = "Stores information used in a security decision.")]
        [SecurityCritical]
        EvaluationContextHelper contextHelper;

        protected StandardBindingElement()
            : this(null)
        {
        }

        protected StandardBindingElement(string name)
        {
            if (!String.IsNullOrEmpty(name))
            {
                this.Name = name;
            }
        }

        protected abstract Type BindingElementType
        {
            get;
        }

        [ConfigurationProperty(ConfigurationStrings.Name, Options = ConfigurationPropertyOptions.IsKey)]
        [StringValidator(MinLength = 0)]
        public string Name
        {
            get { return (string)base[ConfigurationStrings.Name]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.Name] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.CloseTimeout, DefaultValue = ServiceDefaults.CloseTimeoutString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan CloseTimeout
        {
            get { return (TimeSpan)base[ConfigurationStrings.CloseTimeout]; }
            set { base[ConfigurationStrings.CloseTimeout] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.OpenTimeout, DefaultValue = ServiceDefaults.OpenTimeoutString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan OpenTimeout
        {
            get { return (TimeSpan)base[ConfigurationStrings.OpenTimeout]; }
            set { base[ConfigurationStrings.OpenTimeout] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ReceiveTimeout, DefaultValue = ServiceDefaults.ReceiveTimeoutString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan ReceiveTimeout
        {
            get { return (TimeSpan)base[ConfigurationStrings.ReceiveTimeout]; }
            set { base[ConfigurationStrings.ReceiveTimeout] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.SendTimeout, DefaultValue = ServiceDefaults.SendTimeoutString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan SendTimeout
        {
            get { return (TimeSpan)base[ConfigurationStrings.SendTimeout]; }
            set { base[ConfigurationStrings.SendTimeout] = value; }
        }

        public void ApplyConfiguration(Binding binding)
        {
            if (null == binding)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            if (binding.GetType() != this.BindingElementType)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.ConfigInvalidTypeForBinding,
                    (this.BindingElementType == null) ? string.Empty : this.BindingElementType.AssemblyQualifiedName,
                    binding.GetType().AssemblyQualifiedName));
            }

            // The properties binding.Name and this.Name are actually two different things:
            //     - binding.Name corresponds to how the WSDL for this binding is surfaced,
            //       it is used in conjunction with binding.Namespace
            //     - this.Name is a token used as a key in the binding collection to identify
            //       a specific bucket of configuration settings.
            // Thus, the Name property is skipped here.
            binding.CloseTimeout = this.CloseTimeout;
            binding.OpenTimeout = this.OpenTimeout;
            binding.ReceiveTimeout = this.ReceiveTimeout;
            binding.SendTimeout = this.SendTimeout;

            this.OnApplyConfiguration(binding);
        }

        protected virtual internal void InitializeFrom(Binding binding)
        {
            if (null == binding)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            if (binding.GetType() != this.BindingElementType)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.ConfigInvalidTypeForBinding,
                    (this.BindingElementType == null) ? string.Empty : this.BindingElementType.AssemblyQualifiedName,
                    binding.GetType().AssemblyQualifiedName));
            }

            // The properties binding.Name and this.Name are actually two different things:
            //     - binding.Name corresponds to how the WSDL for this binding is surfaced,
            //       it is used in conjunction with binding.Namespace
            //     - this.Name is a token used as a key in the binding collection to identify
            //       a specific bucket of configuration settings.
            // Thus, the Name property is skipped here.
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.CloseTimeout, binding.CloseTimeout);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.OpenTimeout, binding.OpenTimeout);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ReceiveTimeout, binding.ReceiveTimeout);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.SendTimeout, binding.SendTimeout);
        }



        protected abstract void OnApplyConfiguration(Binding binding);

        [Fx.Tag.SecurityNote(Critical = "Accesses critical field contextHelper.")]
        [SecurityCritical]
        protected override void Reset(ConfigurationElement parentElement)
        {
            this.contextHelper.OnReset(parentElement);

            base.Reset(parentElement);
        }

        ContextInformation IConfigurationContextProviderInternal.GetEvaluationContext()
        {
            return this.EvaluationContext;
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses critical field contextHelper.",
            Miscellaneous = "RequiresReview -- the return value will be used for a security decision -- see comment in interface definition.")]
        [SecurityCritical]
        ContextInformation IConfigurationContextProviderInternal.GetOriginalEvaluationContext()
        {
            return this.contextHelper.GetOriginalContext(this);
        }
    }
}
