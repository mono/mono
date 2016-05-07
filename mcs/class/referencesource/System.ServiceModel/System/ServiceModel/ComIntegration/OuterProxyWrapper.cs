//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Runtime.Versioning;

    class ProxySupportWrapper
    {
        internal delegate int DelegateDllGetClassObject([In, MarshalAs(UnmanagedType.LPStruct)] Guid clsid, [In, MarshalAs(UnmanagedType.LPStruct)] Guid iid, ref IClassFactory ppv);

        const string fileName = @"ServiceMonikerSupport.dll";
        const string functionName = @"DllGetClassObject";

        static readonly Guid ClsidProxyInstanceProvider = new Guid("(BF0514FB-6912-4659-AD69-B727E5B7ADD4)");

        // Double-checked locking pattern requires volatile for read/write synchronization
        volatile SafeLibraryHandle monikerSupportLibrary;

        // Double-checked locking pattern requires volatile for read/write synchronization
        volatile DelegateDllGetClassObject getCODelegate;

        internal ProxySupportWrapper()
        {
            monikerSupportLibrary = null;
            getCODelegate = null;
        }

        ~ProxySupportWrapper()
        {
            if (null != monikerSupportLibrary)
            {
                monikerSupportLibrary.Close();
                monikerSupportLibrary = null;
            }
        }

        [ResourceConsumption(ResourceScope.Process)]
        internal IProxyProvider GetProxyProvider()
        {
            if (null == monikerSupportLibrary)
            {
                lock (this)
                {
                    if (null == monikerSupportLibrary)
                    {
                        getCODelegate = null;
                        using (RegistryHandle regKey = RegistryHandle.GetCorrectBitnessHKLMSubkey((IntPtr.Size == 8), ServiceModelInstallStrings.WinFXRegistryKey))
                        {
                            string file = regKey.GetStringValue(ServiceModelInstallStrings.RuntimeInstallPathName).TrimEnd('\0') + "\\" + fileName;
                            SafeLibraryHandle tempLibrary = UnsafeNativeMethods.LoadLibrary(file);
                            tempLibrary.DoNotFreeLibraryOnRelease();

                            monikerSupportLibrary = tempLibrary;
                            if (monikerSupportLibrary.IsInvalid)
                            {
                                monikerSupportLibrary.SetHandleAsInvalid();
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ServiceMonikerSupportLoadFailed(file));
                            }
                        }
                    }
                }
            }

            if (null == getCODelegate)
            {
                lock (this)
                {
                    if (null == getCODelegate)
                    {
                        try
                        {
                            IntPtr procaddr = UnsafeNativeMethods.GetProcAddress(monikerSupportLibrary, functionName);
                            getCODelegate = (DelegateDllGetClassObject)Marshal.GetDelegateForFunctionPointer(procaddr, typeof(DelegateDllGetClassObject));

                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                                throw;

                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ComPlusProxyProviderException(SR.GetString(SR.FailedProxyProviderCreation), e));
                        }
                    }
                }
            }

            IClassFactory cf = null;
            IProxyProvider proxyProvider = null;

            try
            {
                getCODelegate(ClsidProxyInstanceProvider, typeof(IClassFactory).GUID, ref cf);

                proxyProvider = cf.CreateInstance(null, typeof(IProxyProvider).GUID) as IProxyProvider;
                Thread.MemoryBarrier();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ComPlusProxyProviderException(SR.GetString(SR.FailedProxyProviderCreation), e));
            }
            finally
            {
                if (null != cf)
                {
                    Marshal.ReleaseComObject(cf);
                    cf = null;
                }
            }

            return proxyProvider;
        }
    }


    internal static class OuterProxyWrapper
    {
        static ProxySupportWrapper proxySupport = new ProxySupportWrapper();

        public static IntPtr CreateOuterProxyInstance(IProxyManager proxyManager, ref Guid riid)
        {
            IntPtr pOuter = IntPtr.Zero;
            IProxyProvider proxyProvider = proxySupport.GetProxyProvider();

            if (proxyProvider == null)
            {
                throw Fx.AssertAndThrowFatal("Proxy Provider cannot be NULL");
            }
            Guid riid2 = riid;
            int hr = proxyProvider.CreateOuterProxyInstance(proxyManager, ref riid2, out pOuter);

            Marshal.ReleaseComObject(proxyProvider);

            if (hr != HR.S_OK)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.FailedProxyProviderCreation), hr));

            return pOuter;

        }

        public static IntPtr CreateDispatchProxy(IntPtr pOuter, IPseudoDispatch proxy)
        {
            IntPtr pInner = IntPtr.Zero;
            IProxyProvider proxyProvider = proxySupport.GetProxyProvider();

            if (proxyProvider == null)
            {
                throw Fx.AssertAndThrowFatal("Proxy Provider cannot be NULL");
            }
            int hr = proxyProvider.CreateDispatchProxyInstance(pOuter, proxy, out pInner);

            Marshal.ReleaseComObject(proxyProvider);

            if (hr != HR.S_OK)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.FailedProxyProviderCreation), hr));

            return pInner;

        }
    }
}
     
