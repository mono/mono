//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.ComIntegration
{
    using System.Runtime.InteropServices;

    [ComImport,
     Guid("A7549A29-A7C4-42e1-8DC1-7E3D748DC24A"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IContextSecurityPerimeter
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetPerimeterFlag();
        void SetPerimeterFlag([MarshalAs(UnmanagedType.Bool)] bool flag);
    }

}
