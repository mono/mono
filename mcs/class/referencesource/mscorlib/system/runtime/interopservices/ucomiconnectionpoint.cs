// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: UCOMIConnectionPoint
**
**
** Purpose: UCOMIConnectionPoint interface definition.
**
**
=============================================================================*/

namespace System.Runtime.InteropServices
{
    using System;

    [Obsolete("Use System.Runtime.InteropServices.ComTypes.IConnectionPoint instead. http://go.microsoft.com/fwlink/?linkid=14202", false)]
    [Guid("B196B286-BAB4-101A-B69C-00AA00341D07")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface UCOMIConnectionPoint
    {
        void GetConnectionInterface(out Guid pIID);
        void GetConnectionPointContainer(out UCOMIConnectionPointContainer ppCPC);
        void Advise([MarshalAs(UnmanagedType.Interface)] Object pUnkSink, out int pdwCookie);
        void Unadvise(int dwCookie);
        void EnumConnections(out UCOMIEnumConnections ppEnum);
    }
}
