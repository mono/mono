// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//
// <OWNER>ericeil</OWNER>
/*=============================================================================
**
** Class: ParameterizedThreadStart
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
    using System.Runtime.InteropServices;

    [ComVisibleAttribute(false)]
    public delegate void ParameterizedThreadStart(object obj);
}
