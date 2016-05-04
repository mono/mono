//------------------------------------------------------------------------------
// <copyright file="COSERVERINFO.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System.Collections;
    using System.Configuration;
    using System.Configuration.Internal;
    using System.Web;
    using System.Web.Util;
    using System.Security;
    using System.IO;
    using System.Web.Hosting;
    using System.Runtime.InteropServices;
    using System.Reflection;
    using System.Collections.Specialized;
    using System.Xml;
    using System.Security.Principal;
    using System.Threading;
    using System.Globalization;

    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
    internal class COSERVERINFO : IDisposable
    {
        internal COSERVERINFO(string srvname, IntPtr authinf) {
            servername = srvname;
            authinfo = authinf;
        }

        #pragma warning disable 0649
        internal int reserved1;
        #pragma warning restore 0649
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string servername;
        internal IntPtr authinfo;                // COAUTHINFO*
        #pragma warning disable 0649
        internal int reserved2;
        #pragma warning restore 0649
        void IDisposable.Dispose()
        {
            authinfo = IntPtr.Zero;
            GC.SuppressFinalize(this);
        }
        ~COSERVERINFO()
        {
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
    internal class COSERVERINFO_X64 : IDisposable
    {
        internal COSERVERINFO_X64(string srvname, IntPtr authinf)
        {
            servername = srvname;
            authinfo = authinf;
        }

#pragma warning disable 0649
        internal int reserved1;
        internal int padding1;
#pragma warning restore 0649
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string servername;
        internal IntPtr authinfo;                // COAUTHINFO*
#pragma warning disable 0649
        internal int reserved2;
        internal int padding2;
        #pragma warning restore 0649
        void IDisposable.Dispose()
        {
            authinfo = IntPtr.Zero;
            GC.SuppressFinalize(this);
        }
        ~COSERVERINFO_X64()
        {
        }
    }
}
