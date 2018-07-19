// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Delegate: ObjectCreationDelegate
**
**
** Purpose: Delegate called to create a classic COM object as an alternative to
**          CoCreateInstance.
**
**
=============================================================================*/

namespace System.Runtime.InteropServices {

    // Delegate called when a managed object wishes to instantiate its unmanaged
    // portion. The IUnknown of the managed object (the aggregator) is passed as a
    // parameter and the delegate should return the IUnknown of the unmanaged object
    // (the aggregatee). Both are passed as int's to avoid any marshalling.
[System.Runtime.InteropServices.ComVisible(true)]
    public delegate IntPtr ObjectCreationDelegate(IntPtr aggregator);
}
