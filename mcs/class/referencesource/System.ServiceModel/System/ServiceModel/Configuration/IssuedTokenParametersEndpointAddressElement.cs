//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Runtime;

    public sealed partial class IssuedTokenParametersEndpointAddressElement : EndpointAddressElementBase, IConfigurationContextProviderInternal
    {
        public IssuedTokenParametersEndpointAddressElement()
            : base()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.Binding, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string Binding
        {
            get { return (string)base[ConfigurationStrings.Binding]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.Binding] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.BindingConfiguration, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string BindingConfiguration
        {
            get { return (string)base[ConfigurationStrings.BindingConfiguration]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.BindingConfiguration] = value;
            }
        }

        internal void Copy(IssuedTokenParametersEndpointAddressElement source)
        {
            base.Copy(source);
            this.BindingConfiguration = source.BindingConfiguration;
            this.Binding = source.Binding;
        }

        internal void Validate()
        {
            ContextInformation context = ConfigurationHelpers.GetEvaluationContext(this);

            if (context != null && !String.IsNullOrEmpty(this.Binding))
            {
                BindingsSection.ValidateBindingReference(this.Binding,
                    this.BindingConfiguration,
                    context,
                    this);
            }
        }

        ContextInformation IConfigurationContextProviderInternal.GetEvaluationContext()
        {
            return this.EvaluationContext;
        }

        [Fx.Tag.SecurityNote(Miscellaneous =
            "RequiresReview - the return value will be used for a security decision -- see comment in interface definition")]
        ContextInformation IConfigurationContextProviderInternal.GetOriginalEvaluationContext()
        {
            Fx.Assert("Not implemented: IConfigurationContextProviderInternal.GetOriginalEvaluationContext");
            return null;
        }
    }
}



