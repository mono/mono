// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    IMessageSink.cs
**
**
** Purpose: Defines the message sink interface
**
**
===========================================================*/
namespace System.Runtime.Remoting.Messaging {
    using System.Runtime.Remoting;
    using System.Security.Permissions;
    using System;
[System.Runtime.InteropServices.ComVisible(true)]
    public interface IMessageSink
    {
        [System.Security.SecurityCritical]  // auto-generated_required
        IMessage     SyncProcessMessage(IMessage msg);

        [System.Security.SecurityCritical]  // auto-generated_required
        IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink);

        // Retrieves the next message sink held by this sink.
        IMessageSink NextSink
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
        }
    }
}
