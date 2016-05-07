// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Configuration
{
    /// <summary>
    /// NetHttpBindingCollectionElement for NetHttpBinding
    /// </summary>
    public partial class NetHttpBindingCollectionElement : StandardBindingCollectionElement<NetHttpBinding, NetHttpBindingElement>
    {
        internal static NetHttpBindingCollectionElement GetBindingCollectionElement()
        {
            return (NetHttpBindingCollectionElement)ConfigurationHelpers.GetBindingCollectionElement(ConfigurationStrings.NetHttpBindingCollectionElementName);
        }
    }
}
