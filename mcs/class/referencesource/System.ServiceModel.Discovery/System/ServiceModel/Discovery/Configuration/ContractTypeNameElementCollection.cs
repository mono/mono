//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Configuration
{
    using System.Configuration;
    using System.ServiceModel.Configuration;
    using System.Xml;

    [ConfigurationCollection(typeof(ContractTypeNameElement))]
    public sealed class ContractTypeNameElementCollection : ServiceModelConfigurationElementCollection<ContractTypeNameElement>
    {
        protected override object GetElementKey(ConfigurationElement element)
        {
            ContractTypeNameElement contractTypeNameElement = (ContractTypeNameElement)element;
            return new XmlQualifiedName(contractTypeNameElement.Name, contractTypeNameElement.Namespace);
        }
    }
}
