//------------------------------------------------------------------------------
// <copyright file="RpcLevel.cs" company="Microsoft">
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

    internal enum RpcLevel
    {                                // RPC_C_AUTHN_LEVEL_xxx
        Default = 0,
        None = 1,
        Connect = 2,
        Call = 3,
        Pkt = 4,
        PktIntegrity = 5,
        PktPrivacy = 6
    }
}
