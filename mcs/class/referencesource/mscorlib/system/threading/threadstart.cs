// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//
// <OWNER>ericeil</OWNER>
/*=============================================================================
**
** Class: ThreadStart
**
**
** Purpose: This class is a Delegate which defines the start method
**  for starting a thread.  That method must match this delegate.
**
**
=============================================================================*/

namespace System.Threading {
    using System.Security.Permissions;
    using System.Threading;

    // Define the delegate
    // NOTE: If you change the signature here, there is code in COMSynchronization
    //  that invokes this delegate in native.
[System.Runtime.InteropServices.ComVisible(true)]
    public delegate void ThreadStart();
}
