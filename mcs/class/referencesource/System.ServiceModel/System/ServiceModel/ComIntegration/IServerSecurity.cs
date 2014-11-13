//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.ComIntegration
{
    using System.Runtime.InteropServices;

    [ComImport,
     Guid("0000013E-0000-0000-C000-000000000046"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IServerSecurity
    {
        void QueryBlanket
        (
            IntPtr authnSvc,
            IntPtr authzSvc,
            IntPtr serverPrincipalName,
            IntPtr authnLevel,
            IntPtr impLevel,
            IntPtr clientPrincipalName,
            IntPtr Capabilities
        );
        [PreserveSig]
        int ImpersonateClient();
        [PreserveSig]
        int RevertToSelf();
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Bool)]
        bool IsImpersonating();
    }

}
