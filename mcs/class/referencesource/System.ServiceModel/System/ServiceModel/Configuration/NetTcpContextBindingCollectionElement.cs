//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Runtime.CompilerServices;
    using System.ServiceModel;

    [TypeForwardedFrom("System.WorkflowServices, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class NetTcpContextBindingCollectionElement : StandardBindingCollectionElement<NetTcpContextBinding, NetTcpContextBindingElement>
    {
        internal const string netTcpContextBindingName = "netTcpContextBinding";

        public NetTcpContextBindingCollectionElement()
            : base()
        {
        }

        internal static NetTcpContextBindingCollectionElement GetBindingCollectionElement()
        {
            return (NetTcpContextBindingCollectionElement) ConfigurationHelpers.GetBindingCollectionElement(netTcpContextBindingName);
        }
    }
}
