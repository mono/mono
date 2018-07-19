//------------------------------------------------------------------------------
// <copyright file="ClsCtx.cs" company="Microsoft">
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

    internal enum ClsCtx
    {
        Inproc = 0x03,
        Server = 0x15,
        All = 0x17,

        InprocServer = 0x1,
        InprocHandler = 0x2,
        LocalServer = 0x4,
        InprocServer16 = 0x8,
        RemoteServer = 0x10,
        InprocHandler16 = 0x20,
        InprocServerX86 = 0x40,
        InprocHandlerX86 = 0x80,
        EServerHandler = 0x100,
        Reserved = 0x200,
        NoCodeDownload = 0x400,
        NoWX86Translation = 0x800,
        NoCustomMarshal = 0x1000,
        EnableCodeDownload = 0x2000,
        NoFailureLog = 0x4000,
        DisableAAA = 0x8000,
        EnableAAA = 0x10000,
        FromDefaultContext = 0x20000,
    }
}
