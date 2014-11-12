// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//
// <OWNER>[....]</OWNER>
/*=============================================================================
**
** Enum: EventResetMode
**
**
** Purpose: Enum to determine the Event type to create
**
**
=============================================================================*/


namespace System.Threading
{
    using System.Runtime.InteropServices;
    [ComVisibleAttribute(false)]
    public enum EventResetMode
    {
        AutoReset = 0,
        ManualReset = 1
    }
}
