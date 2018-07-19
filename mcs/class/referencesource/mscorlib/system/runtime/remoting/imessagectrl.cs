// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    IMessageCtrl.cs
**
**
** Purpose: Defines the message sink control interface for
**          async calls
**
**
===========================================================*/
namespace System.Runtime.Remoting.Messaging {
    using System.Runtime.Remoting;
    using System.Security.Permissions;
    using System;
[System.Runtime.InteropServices.ComVisible(true)]
    public interface IMessageCtrl
    {
        [System.Security.SecurityCritical]  // auto-generated_required
        void Cancel(int msToCancel);
    }
}
