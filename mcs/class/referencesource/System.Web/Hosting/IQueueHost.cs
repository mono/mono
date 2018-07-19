//------------------------------------------------------------------------------
// <copyright file="IQueueHost.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Runtime.InteropServices;  
    using System.Runtime.Remoting;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Util;
    

    [ComImport, Guid("dc3b0a85-9da7-47e4-ba1b-e27da9db8a1e"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IListenerChannelCallback {

        void ReportStarted();

        void ReportStopped(int hr);

        void ReportMessageReceived();

        int  GetId();
		
        int  GetBlobLength();

        void GetBlob([In, Out, MarshalAs(UnmanagedType.LPArray)] byte[] buffer, ref int bufferSize);
    }
}
