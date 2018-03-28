//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    class ProxyManager : IProxyManager
    {
        Dictionary<Guid, ComProxy> InterfaceIDToComProxy;
        IProxyCreator proxyCreator;

        internal ProxyManager(IProxyCreator proxyCreator)
        {
            this.proxyCreator = proxyCreator;
            InterfaceIDToComProxy = new Dictionary<Guid, ComProxy>();
        }
        bool IsIntrinsic(ref Guid riid)
        {
            if ((riid == typeof(IChannelOptions).GUID)
                 ||
                 (riid == typeof(IChannelCredentials).GUID)
                 )
                return true;
            return false;
        }

        void IProxyManager.TearDownChannels()
        {
            lock (this)
            {
                IEnumerator<KeyValuePair<Guid, ComProxy>> enumeratorInterfaces = InterfaceIDToComProxy.GetEnumerator();
                while (enumeratorInterfaces.MoveNext())
                {
                    KeyValuePair<Guid, ComProxy> current = enumeratorInterfaces.Current;
                    IDisposable comProxy = current.Value as IDisposable;
                    if (comProxy == null)
                        Fx.Assert("comProxy should not be null");
                    else
                        comProxy.Dispose();
                }
                InterfaceIDToComProxy.Clear();
                proxyCreator.Dispose();
                enumeratorInterfaces.Dispose();
                proxyCreator = null;
            }
        }

        ComProxy CreateServiceChannel(IntPtr outerProxy, ref Guid riid)
        {
            return proxyCreator.CreateProxy(outerProxy, ref riid);
        }

        ComProxy GenerateIntrinsic(IntPtr outerProxy, ref Guid riid)
        {
            if (proxyCreator.SupportsIntrinsics())
            {
                if (riid == typeof(IChannelOptions).GUID)
                    return ChannelOptions.Create(outerProxy, proxyCreator as IProvideChannelBuilderSettings);
                else if (riid == typeof(IChannelCredentials).GUID)
                    return ChannelCredentials.Create(outerProxy, proxyCreator as IProvideChannelBuilderSettings);
                else
                {
                    throw Fx.AssertAndThrow("Given IID is not an intrinsic");
                }
            }
            else
            {
                throw Fx.AssertAndThrow("proxyCreator does not support intrinsic");
            }
        }

        void FindOrCreateProxyInternal(IntPtr outerProxy, ref Guid riid, out ComProxy comProxy)
        {
            comProxy = null;
            lock (this)
            {
                InterfaceIDToComProxy.TryGetValue(riid, out comProxy);
                if (comProxy == null)
                {
                    if (IsIntrinsic(ref riid))
                        comProxy = GenerateIntrinsic(outerProxy, ref riid);
                    else
                        comProxy = CreateServiceChannel(outerProxy, ref riid);
                    InterfaceIDToComProxy[riid] = comProxy;
                }
            }
            if (comProxy == null)
            {
                throw Fx.AssertAndThrow("comProxy should not be null at this point");
            }
        }

        int IProxyManager.FindOrCreateProxy(IntPtr outerProxy, ref Guid riid, out IntPtr tearOff)
        {
            tearOff = IntPtr.Zero;
            try
            {
                ComProxy comProxy = null;
                FindOrCreateProxyInternal(outerProxy, ref riid, out comProxy);
                comProxy.QueryInterface(ref riid, out tearOff);
                return HR.S_OK;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                e = e.GetBaseException();
                return Marshal.GetHRForException(e);

            }

        }

        int IProxyManager.InterfaceSupportsErrorInfo(ref Guid riid)
        {
            if (IsIntrinsic(ref riid))
                return HR.S_OK;
            else
                return proxyCreator.SupportsErrorInfo(ref riid) ? HR.S_OK : HR.S_FALSE;

        }

        void IProxyManager.GetIDsOfNames(
                        [MarshalAs(UnmanagedType.LPWStr)] string name,
                        IntPtr pDispID)
        {

            Int32 dispID = -1;
            switch (name)
            {
                case "ChannelOptions":
                    dispID = 1;
                    break;
                case "ChannelCredentials":
                    dispID = 2;
                    break;
            }
            Marshal.WriteInt32(pDispID, (int)dispID);
        }
        int IProxyManager.Invoke(
                       UInt32 dispIdMember,
                       IntPtr outerProxy,
                       IntPtr pVarResult,
                       IntPtr pExcepInfo

                   )
        {

            try
            {

                ComProxy comProxy = null;
                Guid riid;
                if ((dispIdMember == 1))
                    riid = typeof(IChannelOptions).GUID;
                else if ((dispIdMember == 2))
                    riid = typeof(IChannelCredentials).GUID;
                else
                    return HR.DISP_E_MEMBERNOTFOUND;
                FindOrCreateProxyInternal(outerProxy, ref riid, out comProxy);
                TagVariant variant = new TagVariant();
                variant.vt = (ushort)VarEnum.VT_DISPATCH;
                IntPtr tearOffDispatch = IntPtr.Zero;
                comProxy.QueryInterface(ref riid, out tearOffDispatch);
                variant.ptr = tearOffDispatch;
                Marshal.StructureToPtr(variant, pVarResult, true);
                return HR.S_OK;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                if (pExcepInfo != IntPtr.Zero)
                {
                    System.Runtime.InteropServices.ComTypes.EXCEPINFO exceptionInfo = new System.Runtime.InteropServices.ComTypes.EXCEPINFO();
                    e = e.GetBaseException();
                    exceptionInfo.bstrDescription = e.Message;
                    exceptionInfo.bstrSource = e.Source;
                    exceptionInfo.scode = Marshal.GetHRForException(e);
                    Marshal.StructureToPtr(exceptionInfo, pExcepInfo, false);
                }
                return HR.DISP_E_EXCEPTION;
            }
        }

        int IProxyManager.SupportsDispatch()
        {
            if (proxyCreator.SupportsDispatch())
                return HR.S_OK;
            else
                return HR.E_FAIL;
        }
    }
}
