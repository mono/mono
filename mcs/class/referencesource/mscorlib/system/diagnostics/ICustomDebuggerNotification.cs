// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  ICustomDebuggerNotification
**
** This interface is implemented by classes that support custom debugger notifications.
**
===========================================================*/
namespace System.Diagnostics {
    
    using System;
    // Defines an interface indicating that a custom debugger notification is requested under specific 
    // conditions. Users should implement this interface to be used as an argument to 
    // System.Diagnostics.Debugger.CustomNotification.  
    // 
    // @dbgtodo dlaw: when this goes public, it must be replaced by a custom attribute
    internal interface ICustomDebuggerNotification
    {
        // Interface does not need to be marked with the serializable attribute
    }
}
