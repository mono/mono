//------------------------------------------------------------------------------
// <copyright file="ISAPIRuntime.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * The ASP.NET runtime services
 * 
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web.Hosting {
    using System.Runtime.InteropServices;       
    using System.Collections;
    using System.Reflection;
    using System.Threading;
    using System.Web;
    using System.Web.Management;
    using System.Web.Util;
    using System.Globalization;
    using System.Security.Permissions;
    

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    /// <internalonly/>
    [ComImport, Guid("08a2c56f-7c16-41c1-a8be-432917a1a2d1"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IISAPIRuntime {

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)] // DevDiv #180492
        [SecurityPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
        void StartProcessing();

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)] // DevDiv #180492
        [SecurityPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
        void StopProcessing();


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)] // DevDiv #180492
        [SecurityPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
        [return: MarshalAs(UnmanagedType.I4)]
        int ProcessRequest(
                          [In]
                          IntPtr ecb, 
                          [In, MarshalAs(UnmanagedType.I4)]
                          int useProcessModel);

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)] // DevDiv #180492
        [SecurityPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
        void DoGCCollect();
    }

    // DevDiv #195200: Adding link demands to IISAPIRuntime causes partial trust to stop
    // working. Ideally that type should have been internal, but we can't do that since
    // that would be a breaking API change. So instead we introduce a parallel internal
    // type with a different COM GUID. Existing public methods on ISAPIRuntime will be
    // protected by a link demand, but our native layer will go through this specific
    // interface which can bypass the link demands.
    [ComImport, Guid("15eb8d20-d4ed-4855-a276-91a75a696955"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IISAPIRuntime2 {

        void StartProcessing();

        void StopProcessing();

        [return: MarshalAs(UnmanagedType.I4)]
        int ProcessRequest([In]IntPtr ecb, [In, MarshalAs(UnmanagedType.I4)]int useProcessModel);

        void DoGCCollect();

    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    /// <internalonly/>
    public sealed class ISAPIRuntime : MarshalByRefObject, IISAPIRuntime, IISAPIRuntime2, IRegisteredObject {

        // WARNING: do not modify without making corresponding changes in appdomains.h
        private const int WORKER_REQUEST_TYPE_IN_PROC            = 0x0;
        private const int WORKER_REQUEST_TYPE_OOP                = 0x1;
        private const int WORKER_REQUEST_TYPE_IN_PROC_VERSION_2  = 0x2;

        // to control removal from unmanaged table (to it only once)
        private static int _isThisAppDomainRemovedFromUnmanagedTable;

        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public ISAPIRuntime() {
            HostingEnvironment.RegisterObject(this);
        }


        public override Object InitializeLifetimeService() {
            return null; // never expire lease
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)] // DevDiv #180492
        public void StartProcessing() {
            Debug.Trace("ISAPIRuntime", "StartProcessing");
        }

        void IISAPIRuntime2.StartProcessing() {
            StartProcessing();
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)] // DevDiv #180492
        public void StopProcessing() {
            Debug.Trace("ISAPIRuntime", "StopProcessing");
            HostingEnvironment.UnregisterObject(this);
        }

        void IISAPIRuntime2.StopProcessing() {
            StopProcessing();
        }

        /*
         * Process one ISAPI request
         *
         * @param ecb ECB
         * @param useProcessModel flag set to true when out-of-process
         */

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)] // DevDiv #180492
        public int ProcessRequest(IntPtr ecb, int iWRType) {
            IntPtr pHttpCompletion = IntPtr.Zero;
            if (iWRType == WORKER_REQUEST_TYPE_IN_PROC_VERSION_2) {
                pHttpCompletion = ecb;
                ecb = UnsafeNativeMethods.GetEcb(pHttpCompletion);
            } 
            ISAPIWorkerRequest wr = null;
            try {
                bool useOOP = (iWRType == WORKER_REQUEST_TYPE_OOP);
                wr = ISAPIWorkerRequest.CreateWorkerRequest(ecb, useOOP);
                wr.Initialize();

                // check if app path matches (need to restart app domain?)                
                String wrPath = wr.GetAppPathTranslated();
                String adPath = HttpRuntime.AppDomainAppPathInternal;                
                
                if (adPath == null ||
                    StringUtil.EqualsIgnoreCase(wrPath, adPath)) {
                    
                    HttpRuntime.ProcessRequestNoDemand(wr);
                    return 0;
                }
                else {
                    // need to restart app domain
                    HttpRuntime.ShutdownAppDomain(ApplicationShutdownReason.PhysicalApplicationPathChanged,
                                                  SR.GetString(SR.Hosting_Phys_Path_Changed,
                                                                                   adPath,
                                                                                   wrPath));
                    return 1;
                }
            }
            catch(Exception e) {
                try {
                    WebBaseEvent.RaiseRuntimeError(e, this);
                } catch {}
                
                // Have we called HSE_REQ_DONE_WITH_SESSION?  If so, don't re-throw.
                if (wr != null && wr.Ecb == IntPtr.Zero) {
                    if (pHttpCompletion != IntPtr.Zero) {
                        UnsafeNativeMethods.SetDoneWithSessionCalled(pHttpCompletion);
                    }
                    // if this is a thread abort exception, cancel the abort
                    if (e is ThreadAbortException) {
                        Thread.ResetAbort();
                    }                    
                    // IMPORTANT: if this thread is being aborted because of an AppDomain.Unload,
                    // the CLR will still throw an AppDomainUnloadedException. The native caller
                    // must special case COR_E_APPDOMAINUNLOADED(0x80131014) and not
                    // call HSE_REQ_DONE_WITH_SESSION more than once.
                    return 0;
                }
                
                // re-throw if we have not called HSE_REQ_DONE_WITH_SESSION
                throw;
            }
        }

        int IISAPIRuntime2.ProcessRequest(IntPtr ecb, int iWRType) {
            return ProcessRequest(ecb, iWRType);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)] // DevDiv #180492
        public void DoGCCollect() {
            for (int c = 10; c > 0; c--) {
                System.GC.Collect();
            }
        }

        void IISAPIRuntime2.DoGCCollect() {
            DoGCCollect();
        }


        /// <internalonly/>
        void IRegisteredObject.Stop(bool immediate) {
            RemoveThisAppDomainFromUnmanagedTable();
            HostingEnvironment.UnregisterObject(this);
        }


        internal static void RemoveThisAppDomainFromUnmanagedTable() {
            if (Interlocked.Exchange(ref _isThisAppDomainRemovedFromUnmanagedTable, 1) != 0) {
                return;
            }

            try {
                String appId = HttpRuntime.AppDomainAppId;
                if (appId != null ) {
                    Debug.Trace("ISAPIRuntime", "Calling UnsafeNativeMethods.AppDomainRestart appId=" + appId);

                    UnsafeNativeMethods.AppDomainRestart(appId);
                }

                HttpRuntime.AddAppDomainTraceMessage(SR.GetString(SR.App_Domain_Restart));
            }
            catch {
            }
        }
    }
}
