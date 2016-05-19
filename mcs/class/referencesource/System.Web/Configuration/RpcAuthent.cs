//------------------------------------------------------------------------------
// <copyright file="RpcAuthent.cs" company="Microsoft">
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

    internal enum RpcAuthent
    {                                    // RPC_C_AUTHN_xxx
        None = 0,
        DcePrivate = 1,
        DcePublic = 2,
        DecPublic = 4,
        GssNegotiate = 9,
        WinNT = 10,
        GssSchannel = 14,
        GssKerberos = 16,
        DPA = 17,
        MSN = 18,
        Digest = 21,
        MQ = 100,
        Default = -1
    }
}
