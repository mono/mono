// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
//
// <OWNER>[....]</OWNER>
/*============================================================
**
** Class:  SendOrPostCallback
**
**
** Purpose: Represents a method to be called when a message is to be dispatched to a synchronization context. 
**
** 
===========================================================*/

namespace System.Threading
{    
    public delegate void SendOrPostCallback(Object state);
}
