//------------------------------------------------------------------------------
// <copyright file="COAUTHIDENTITY.cs" company="Microsoft">
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
    internal class COAUTHIDENTITY
    {
        internal COAUTHIDENTITY(string usr, string dom, string pwd) {
            user = usr;
            userlen = (user==null) ? 0 : user.Length;
            domain = dom;
            domainlen = (domain==null) ? 0 : domain.Length;
            password = pwd;
            passwordlen = (password==null) ? 0 : password.Length;
        }

        [MarshalAs(UnmanagedType.LPWStr)]
        internal string user = null;
        internal int userlen = 0;

        [MarshalAs(UnmanagedType.LPWStr)]
        internal string domain = null;
        internal int domainlen = 0;

        [MarshalAs(UnmanagedType.LPWStr)]
        internal string password = null;
        internal int passwordlen = 0;
        internal int flags = 2;        // SEC_WINNT_AUTH_IDENTITY_UNICODE
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
    internal class COAUTHIDENTITY_X64
    {
        internal COAUTHIDENTITY_X64(string usr, string dom, string pwd)
        {
            user = usr;
            userlen = (user == null) ? 0 : user.Length;
            domain = dom;
            domainlen = (domain == null) ? 0 : domain.Length;
            password = pwd;
            passwordlen = (password == null) ? 0 : password.Length;
        }

        [MarshalAs(UnmanagedType.LPWStr)]
        internal string user = null;
        internal int userlen = 0;
#pragma warning disable 0649
        internal int padding1;
#pragma warning restore 0649

        [MarshalAs(UnmanagedType.LPWStr)]
        internal string domain = null;
        internal int domainlen = 0;
#pragma warning disable 0649
        internal int padding2;
#pragma warning restore 0649

        [MarshalAs(UnmanagedType.LPWStr)]
        internal string password = null;
        internal int passwordlen = 0;
        internal int flags = 2;        // SEC_WINNT_AUTH_IDENTITY_UNICODE
    }
}
