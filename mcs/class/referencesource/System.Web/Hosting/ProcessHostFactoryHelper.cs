//------------------------------------------------------------------------------
// <copyright file="ApplicationManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Runtime.InteropServices;  
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Util;
    

    [ComImport, Guid("02fd465d-5c5d-4b7e-95b6-82faa031b74a"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IProcessHostFactoryHelper {

#if FEATURE_PAL // FEATURE_PAL does not enable COM
        [return: MarshalAs(UnmanagedType.Error)] 
#else // FEATURE_PAL
        [return: MarshalAs(UnmanagedType.Interface)]
#endif // FEATURE_PAL

        Object GetProcessHost(IProcessHostSupportFunctions functions);
    }

    public sealed class ProcessHostFactoryHelper : MarshalByRefObject, IProcessHostFactoryHelper {

        public ProcessHostFactoryHelper() {
        }


        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure)]
        public override Object InitializeLifetimeService() {
            return null; // never expire lease
        }


        public Object GetProcessHost(IProcessHostSupportFunctions functions) {
            try {
                return ProcessHost.GetProcessHost(functions);
            }
            catch(Exception e) {
                Misc.ReportUnhandledException(e, new string[] {SR.GetString(SR.Cant_Create_Process_Host)});
                throw;
            }
        }
    }
}

