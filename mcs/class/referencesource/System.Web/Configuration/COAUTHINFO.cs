//------------------------------------------------------------------------------
// <copyright file="COAUTHINFO.cs" company="Microsoft">
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
    internal class COAUTHINFO : IDisposable
    {
        internal COAUTHINFO(RpcAuthent authent, RpcAuthor author, string serverprinc, RpcLevel level, RpcImpers impers, IntPtr ciptr) {
            authnsvc = authent;
            authzsvc = author;
            serverprincname = serverprinc;
            authnlevel = level;
            impersonationlevel = impers;
            authidentitydata = ciptr;
        }

        internal RpcAuthent authnsvc;
        internal RpcAuthor authzsvc;
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string serverprincname;
        internal RpcLevel authnlevel;
        internal RpcImpers impersonationlevel;
        internal IntPtr authidentitydata;        // COAUTHIDENTITY*
        internal int capabilities = 0;        // EOAC_NONE

        void IDisposable.Dispose()
        {
            authidentitydata = IntPtr.Zero;
            GC.SuppressFinalize(this);
        }
        ~COAUTHINFO()
        {
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
    internal class COAUTHINFO_X64 : IDisposable
    {
        internal COAUTHINFO_X64(RpcAuthent authent, RpcAuthor author, string serverprinc, RpcLevel level, RpcImpers impers, IntPtr ciptr)
        {
            authnsvc = authent;
            authzsvc = author;
            serverprincname = serverprinc;
            authnlevel = level;
            impersonationlevel = impers;
            authidentitydata = ciptr;
        }

        internal RpcAuthent authnsvc;
        internal RpcAuthor authzsvc;
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string serverprincname;
        internal RpcLevel authnlevel;
        internal RpcImpers impersonationlevel;
        internal IntPtr authidentitydata;        // COAUTHIDENTITY*
        internal int capabilities = 0;        // EOAC_NONE
#pragma warning disable 0649
        internal int padding;
#pragma warning restore 0649

        void IDisposable.Dispose()
        {
            authidentitydata = IntPtr.Zero;
            GC.SuppressFinalize(this);
        }
        ~COAUTHINFO_X64()
        {
        }
    }
}
