// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: ICustomQueryInterface
**
**
** Purpose: This the interface that be implemented by class that want to 
**          customize the behavior of QueryInterface.
**
**
=============================================================================*/

namespace System.Runtime.InteropServices {
    using System;

    //====================================================================
    // The enum of the return value of IQuerable.GetInterface
    //====================================================================
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(false)]
    public enum CustomQueryInterfaceResult
    {
        Handled                 = 0,
        NotHandled              = 1,
        Failed                  = 2,
    }

    //====================================================================
    // The interface for customizing IQueryInterface
    //====================================================================
    [System.Runtime.InteropServices.ComVisible(false)]
    public interface ICustomQueryInterface
    {
        [System.Security.SecurityCritical]
        CustomQueryInterfaceResult GetInterface([In]ref Guid iid, out IntPtr ppv);
    }
}
