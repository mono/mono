//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Services;

    class MonikerBuilder : IProxyCreator
    {
        ComProxy comProxy;

        MonikerBuilder()
        {
        }

        void IDisposable.Dispose()
        {
        }

        ComProxy IProxyCreator.CreateProxy(IntPtr outer, ref Guid riid)
        {
            if ((riid != typeof(IMoniker).GUID) && (riid != typeof(IParseDisplayName).GUID))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidCastException(SR.GetString(SR.NoInterface, riid)));
            if (outer == IntPtr.Zero)
            {
                throw Fx.AssertAndThrow("OuterProxy cannot be null");
            }

            if (comProxy == null)
            {
                ServiceMonikerInternal moniker = null;
                try
                {
                    moniker = new ServiceMonikerInternal();
                    comProxy = ComProxy.Create(outer, moniker, moniker);
                    return comProxy;

                }
                finally
                {
                    if ((comProxy == null) && (moniker != null))
                        ((IDisposable)moniker).Dispose();

                }
            }
            else
                return comProxy.Clone();
        }

        bool IProxyCreator.SupportsErrorInfo(ref Guid riid)
        {
            if ((riid != typeof(IMoniker).GUID) && (riid != typeof(IParseDisplayName).GUID))
                return false;
            else
                return true;

        }

        bool IProxyCreator.SupportsDispatch()
        {
            return false;
        }

        bool IProxyCreator.SupportsIntrinsics()
        {
            return false;
        }

        public static MarshalByRefObject CreateMonikerInstance()
        {
            IProxyCreator serviceChannelBuilder = new MonikerBuilder();
            IProxyManager proxyManager = new ProxyManager(serviceChannelBuilder);
            Guid iid = typeof(IMoniker).GUID;
            IntPtr ppv = OuterProxyWrapper.CreateOuterProxyInstance(proxyManager, ref iid);
            MarshalByRefObject ret = EnterpriseServicesHelper.WrapIUnknownWithComObject(ppv) as MarshalByRefObject;
            Marshal.Release(ppv);

            return ret;
        }
    }
}
