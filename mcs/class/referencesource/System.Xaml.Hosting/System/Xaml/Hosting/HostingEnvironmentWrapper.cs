//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Xaml.Hosting
{
    using System;
    using System.Web;
    using System.Web.Hosting;
    using System.Security;
    using System.Runtime;

    static class HostingEnvironmentWrapper
    {
        public static IDisposable UnsafeImpersonate()
        {
            return HostingEnvironment.Impersonate();
        }
    }
}




