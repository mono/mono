//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public partial class StandardBindingCollectionElement<TStandardBinding, TBindingConfiguration> : BindingCollectionElement
        where TStandardBinding : Binding
        where TBindingConfiguration : StandardBindingElement, new()
    {

        [ConfigurationProperty(ConfigurationStrings.DefaultCollectionName, Options = ConfigurationPropertyOptions.IsDefaultCollection)]
        public StandardBindingElementCollection<TBindingConfiguration> Bindings
        {
            get { return (StandardBindingElementCollection<TBindingConfiguration>)base[ConfigurationStrings.DefaultCollectionName]; }
        }

        public override Type BindingType
        {
            get { return typeof(TStandardBinding); }
        }

        public override ReadOnlyCollection<IBindingConfigurationElement> ConfiguredBindings
        {
            get
            {
                List<IBindingConfigurationElement> configuredBindings = new List<IBindingConfigurationElement>();
                foreach (IBindingConfigurationElement configuredBinding in this.Bindings)
                {
                    configuredBindings.Add(configuredBinding);
                }

                return new ReadOnlyCollection<IBindingConfigurationElement>(configuredBindings);
            }
        }

        public override bool ContainsKey(string name)
        {
            // This line needed because of the IBindingSection implementation
            StandardBindingCollectionElement<TStandardBinding, TBindingConfiguration> me = (StandardBindingCollectionElement<TStandardBinding, TBindingConfiguration>)this;
#pragma warning suppress 56506 //[....]; me.Bindings can never be null (underlying configuration system guarantees)
            return me.Bindings.ContainsKey(name);
        }
        protected internal override Binding GetDefault()
        {
            return System.Activator.CreateInstance<TStandardBinding>();
        }

        protected internal override bool TryAdd(string name, Binding binding, Configuration config)
        {
            // The configuration item needs to understand the BindingType && be of type CustomBindingConfigurationElement
            // or StandardBindingConfigurationElement
            bool retval = (binding.GetType() == typeof(TStandardBinding)) &&
                typeof(StandardBindingElement).IsAssignableFrom(typeof(TBindingConfiguration));
            if (retval)
            {
                TBindingConfiguration bindingConfig = new TBindingConfiguration();
                bindingConfig.Name = name;
                bindingConfig.InitializeFrom(binding);
                this.Bindings.Add(bindingConfig);
            }
            return retval;
        }
    }
}
