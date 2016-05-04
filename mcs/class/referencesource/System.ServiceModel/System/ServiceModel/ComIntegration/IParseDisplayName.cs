//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using Microsoft.Win32;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Threading;

    [System.Security.SuppressUnmanagedCodeSecurity,
     ComImport,
     Guid("0000011a-0000-0000-C000-000000000046"),
     InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    interface IParseDisplayName
    {
        void ParseDisplayName(IBindCtx pbc,
                                      [MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName,
                                      IntPtr pchEaten,
                                      IntPtr ppmkOut);


    }
}
