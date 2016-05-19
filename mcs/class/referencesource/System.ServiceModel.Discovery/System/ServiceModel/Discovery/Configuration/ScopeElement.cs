//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Configuration
{
    using System.Configuration;
    using System.Runtime;
    using SR2 = System.ServiceModel.Discovery.SR;

    [Fx.Tag.XamlVisible(false)]
    public sealed class ScopeElement : ConfigurationElement
    {
        ConfigurationPropertyCollection properties;

        [ConfigurationProperty(ConfigurationStrings.Scope, Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
        [CallbackValidator(CallbackMethodName = "ScopeValidatorCallback", Type = typeof(ScopeElement))]
        public Uri Scope
        {
            get
            {
                return (Uri)base[ConfigurationStrings.Scope];
            }

            set
            {
                base[ConfigurationStrings.Scope] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.Scope, 
                        typeof(Uri), 
                        null, 
                        null, 
                        new CallbackValidator(typeof(Uri), new ValidatorCallback(ScopeElement.ScopeValidatorCallback)), 
                        System.Configuration.ConfigurationPropertyOptions.IsRequired | System.Configuration.ConfigurationPropertyOptions.IsKey));

                    this.properties = properties;
                }
                return this.properties;
            }
        }

        internal static void ScopeValidatorCallback(object scope)
        {
            if ((scope != null) && !((Uri)scope).IsAbsoluteUri)
            {
                throw FxTrace.Exception.AsError(new ArgumentException(SR2.DiscoveryConfigInvalidScopeUri(scope)));
            }
        }
    }
}
