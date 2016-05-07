// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    IMessage.cs
**
**
** Purpose: Defines the message object interface
**
**
===========================================================*/
namespace System.Runtime.Remoting.Messaging {
    using System;
    using IDictionary = System.Collections.IDictionary;
    using System.Security.Permissions;
    
[System.Runtime.InteropServices.ComVisible(true)]
    public interface IMessage
    {
        IDictionary Properties     
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
        }
    }
}
