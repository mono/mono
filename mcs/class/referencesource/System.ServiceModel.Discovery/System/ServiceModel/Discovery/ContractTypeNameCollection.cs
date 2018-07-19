//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System;    
    using System.Xml;    

    internal class ContractTypeNameCollection : NonNullItemCollection<XmlQualifiedName>
    {
        protected override void InsertItem(int index, XmlQualifiedName item)
        {
            if ((item != null) && (item.Name == string.Empty))
            {
                throw FxTrace.Exception.Argument("item", SR.DiscoveryArgumentEmptyContractTypeName);
            }
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, XmlQualifiedName item)
        {
            if ((item != null) && (item.Name == string.Empty))
            {
                throw FxTrace.Exception.Argument("item", SR.DiscoveryArgumentEmptyContractTypeName);
            }
            base.SetItem(index, item);
        }
    }
}
