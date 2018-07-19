//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.Win32;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Threading;
    internal static class ProxyBuilder
    {
        internal static void Build(Dictionary<MonikerHelper.MonikerAttribute, string> propertyTable, ref Guid riid, IntPtr ppv)
        {
            if (IntPtr.Zero == ppv)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ppv");

            Marshal.WriteIntPtr(ppv, IntPtr.Zero);

            string temp;
            IProxyCreator proxyCreator = null;
            if (propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Wsdl, out temp))
            {
                proxyCreator = new WsdlServiceChannelBuilder(propertyTable);
            }
            else if (propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexAddress, out temp))
            {
                proxyCreator = new MexServiceChannelBuilder(propertyTable);
            }
            else
            {
                proxyCreator = new TypedServiceChannelBuilder(propertyTable);
            }
            IProxyManager proxyManager = new ProxyManager(proxyCreator);

            Marshal.WriteIntPtr(ppv, OuterProxyWrapper.CreateOuterProxyInstance(proxyManager, ref riid));

        }
    }
}
