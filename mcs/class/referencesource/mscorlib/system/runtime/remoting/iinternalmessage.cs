// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    IInternalMessage.cs
**
**
** Purpose: Defines an interface that allows kitchen sink data to be 
**          set and retrieved from the various kinds of message objects.
**          
**
**
===========================================================*/

namespace System.Runtime.Remoting.Messaging {
    using System.Runtime.Remoting;
    using System.Security.Permissions;
    using System;
    // <TODO>Change this back to internal when the classes implementing this interface
    // are also made internal TarunA 12/16/99</TODO>
    internal interface IInternalMessage
    {
        ServerIdentity ServerIdentityObject
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
            [System.Security.SecurityCritical]  // auto-generated_required
            set; 
        }
        Identity IdentityObject
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
            [System.Security.SecurityCritical]  // auto-generated_required
            set;
        }
        [System.Security.SecurityCritical]  // auto-generated_required
        void SetURI(String uri);     
        [System.Security.SecurityCritical]  // auto-generated_required
        void SetCallContext(LogicalCallContext callContext);

        // The following should return true, if the property object hasn't
        //   been created.
        [System.Security.SecurityCritical]  // auto-generated_required
        bool HasProperties();
    }
}
