// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Configuration
{
    /// <summary>
    /// NetHttpsBindingCollectionElement for NetHttpsBinding
    /// </summary>
    public partial class NetHttpsBindingCollectionElement : StandardBindingCollectionElement<NetHttpsBinding, NetHttpsBindingElement>
    {
        internal static NetHttpsBindingCollectionElement GetBindingCollectionElement()
        {
            return (NetHttpsBindingCollectionElement)ConfigurationHelpers.GetBindingCollectionElement(ConfigurationStrings.NetHttpsBindingCollectionElementName);
        }
    }
}
