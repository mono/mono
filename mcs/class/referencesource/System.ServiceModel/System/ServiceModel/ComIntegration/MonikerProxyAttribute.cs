//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Proxies;

    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class MonikerProxyAttribute : ProxyAttribute, ICustomFactory
    {
        public override MarshalByRefObject CreateInstance(Type serverType)
        {
            if (serverType != typeof(ServiceMoniker))
            {
                throw Fx.AssertAndThrow("MonikerProxyAttribute can only be used for the service Moniker");
            }
            return MonikerBuilder.CreateMonikerInstance();
        }

        MarshalByRefObject ICustomFactory.CreateInstance(Type serverType)
        {
            if (serverType != typeof(ServiceMoniker))
            {
                throw Fx.AssertAndThrow("MonikerProxyAttribute can only be used for the service Moniker");
            }
            return MonikerBuilder.CreateMonikerInstance();
        }
    }
}
