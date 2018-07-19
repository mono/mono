//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Configuration
{
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Configuration;
    using System.Xml.Linq;
    using System.Xml;

    public sealed class EndpointDiscoveryElement : BehaviorExtensionElement
    {
        ConfigurationPropertyCollection properties;

        public EndpointDiscoveryElement()
        {
        }

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationPropertyAttributeRule,
            Justification = "This property is defined by the base class to determine the type of the behavior.")]
        public override Type BehaviorType
        {
            get
            {
                return typeof(EndpointDiscoveryBehavior);
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Enabled, DefaultValue = true)]
        public bool Enabled
        {
            get
            {
                return (bool)base[ConfigurationStrings.Enabled];
            }

            set
            {
                base[ConfigurationStrings.Enabled] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Types)]
        [SuppressMessage(
            FxCop.Category.Configuration,
            FxCop.Rule.ConfigurationPropertyNameRule,
            Justification = "The configuration name for this element is 'types'.")]
        public ContractTypeNameElementCollection ContractTypeNames
        {
            get
            {
                return (ContractTypeNameElementCollection)base[ConfigurationStrings.Types];
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Scopes)]
        public ScopeElementCollection Scopes
        {
            get
            {
                return (ScopeElementCollection)base[ConfigurationStrings.Scopes];
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Extensions)]
        public XmlElementElementCollection Extensions
        {
            get
            {
                return (XmlElementElementCollection)base[ConfigurationStrings.Extensions]; 
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
                        ConfigurationStrings.Enabled, 
                        typeof(Boolean), 
                        true, 
                        null, 
                        null, 
                        ConfigurationPropertyOptions.None));

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.Types,
                        typeof(ContractTypeNameElementCollection),
                        null,
                        null,
                        null,
                        ConfigurationPropertyOptions.None));

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.Scopes, 
                        typeof(ScopeElementCollection), 
                        null, 
                        null, 
                        null, 
                        ConfigurationPropertyOptions.None));

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.Extensions, 
                        typeof(XmlElementElementCollection), 
                        null, 
                        null, 
                        null, 
                        ConfigurationPropertyOptions.None));

                    this.properties = properties;
                }
                return this.properties;
            }
        }

        protected internal override object CreateBehavior()
        {
            EndpointDiscoveryBehavior behavior = new EndpointDiscoveryBehavior();
            behavior.Enabled = Enabled;

            if ((Scopes != null) && (Scopes.Count > 0))
            {
                foreach (ScopeElement scopeElement in Scopes)
                {
                    behavior.Scopes.Add(scopeElement.Scope);
                }
            }

            if (ContractTypeNames != null)
            {
                foreach (ContractTypeNameElement contractTypeNameElement in ContractTypeNames)
                {
                    behavior.ContractTypeNames.Add(
                        new XmlQualifiedName(contractTypeNameElement.Name, contractTypeNameElement.Namespace));
                }
            }

            if ((Extensions != null) && (Extensions.Count > 0))
            {
                foreach (XmlElementElement xmlElement in Extensions)
                {
                    behavior.Extensions.Add(XElement.Parse(xmlElement.XmlElement.OuterXml));
                }
            }

            return behavior;
        }
    }
}
