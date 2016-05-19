// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: ICustomAdapter
**
**
** Purpose: This the base interface that custom adapters can chose to implement
**          when they want to expose the underlying object.
**
**
=============================================================================*/

namespace System.Runtime.InteropServices {
    using System;

[System.Runtime.InteropServices.ComVisible(true)]
    public interface ICustomAdapter
    {        
        [return:MarshalAs(UnmanagedType.IUnknown)] Object GetUnderlyingObject();
    }
}
