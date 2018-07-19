//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Configuration
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Configuration;
    using System.Xml;
    using System.Xml.Linq;

    [Fx.Tag.XamlVisible(false)]
    public sealed class FindCriteriaElement : ConfigurationElement
    {
        ConfigurationPropertyCollection properties;

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

        [ConfigurationProperty(ConfigurationStrings.ScopeMatchBy)]
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule, Justification = "No validation requiered.")]        
        public Uri ScopeMatchBy
        {
            get
            {
                return (Uri)base[ConfigurationStrings.ScopeMatchBy];
            }

            set
            {
                if (value == null)
                {
                    throw FxTrace.Exception.ArgumentNull("value");
                }

                base[ConfigurationStrings.ScopeMatchBy] = value;
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

        [ConfigurationProperty(ConfigurationStrings.Duration, DefaultValue = DiscoveryDefaults.DiscoveryOperationDurationString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = "00:00:00.001")]
        public TimeSpan Duration
        {
            get
            {
                return (TimeSpan)base[ConfigurationStrings.Duration];
            }

            set
            {
                base[ConfigurationStrings.Duration] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxResults, DefaultValue = int.MaxValue)]
        [IntegerValidator(MinValue = 1, MaxValue = int.MaxValue)]
        public int MaxResults
        {
            get
            {
                return (int)base[ConfigurationStrings.MaxResults];
            }
            set
            {
                base[ConfigurationStrings.MaxResults] = value;
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
                        ConfigurationStrings.Types,
                        typeof(ContractTypeNameElementCollection),
                        null,
                        null,
                        null,
                        ConfigurationPropertyOptions.None));

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.ScopeMatchBy,
                        typeof(Uri),
                        DiscoveryDefaults.ScopeMatchBy,
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

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.Duration,
                        typeof(TimeSpan),
                        TimeSpan.FromSeconds(20),
                        new TimeSpanOrInfiniteConverter(),
                        new TimeSpanOrInfiniteValidator(TimeSpan.FromMilliseconds(1), TimeSpan.MaxValue),
                        ConfigurationPropertyOptions.None));

                    properties.Add(
                        new ConfigurationProperty(
                        ConfigurationStrings.MaxResults,
                        typeof(int),
                        int.MaxValue,
                        null,
                        new IntegerValidator(1, int.MaxValue),
                        ConfigurationPropertyOptions.None));

                    this.properties = properties;
                }
                return this.properties;
            }
        }        

        internal void ApplyConfiguration(FindCriteria findCriteria)
        {
            foreach (ContractTypeNameElement contractTypeNameElement in this.ContractTypeNames)
            {
                findCriteria.ContractTypeNames.Add(
                    new XmlQualifiedName(
                    contractTypeNameElement.Name, 
                    contractTypeNameElement.Namespace));
            }

            foreach (ScopeElement scopeElement in this.Scopes)
            {
                findCriteria.Scopes.Add(scopeElement.Scope);
            }

            foreach (XmlElementElement xmlElement in this.Extensions)
            {
                findCriteria.Extensions.Add(XElement.Parse(xmlElement.XmlElement.OuterXml));
            }

            findCriteria.ScopeMatchBy = this.ScopeMatchBy;
            findCriteria.Duration = this.Duration;
            findCriteria.MaxResults = this.MaxResults;
        }

        internal void CopyFrom(FindCriteriaElement source)
        {
            foreach (ContractTypeNameElement contractTypeNameElement in source.ContractTypeNames)
            {
                this.ContractTypeNames.Add(contractTypeNameElement);
            }

            foreach (ScopeElement scopeElement in source.Scopes)
            {
                this.Scopes.Add(scopeElement);
            }

            foreach (XmlElementElement extensionElement in source.Extensions)
            {
                this.Extensions.Add(extensionElement);
            }

            this.ScopeMatchBy = source.ScopeMatchBy;
            this.Duration = source.Duration;
            this.MaxResults = source.MaxResults;
        }
    }
}
