// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//
//   IDynamicMessageSink is implemented by message sinks provided by
//   dynamically registered properties. These sinks are provided notifications
//   of call-start and call-finish with flags indicating whether 
//   the call is currently on the client-side or server-side (this is useful
//   for the context level sinks).
//
//
namespace System.Runtime.Remoting.Contexts{
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Security.Permissions;
    using System;
    /// <internalonly/>
    [System.Runtime.InteropServices.ComVisible(true)]
    public interface IDynamicProperty
    {
        /// <internalonly/>
        String Name 
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
        }
    }

    /// <internalonly/>
    [System.Runtime.InteropServices.ComVisible(true)]
    public interface IDynamicMessageSink
    {
        /// <internalonly/>
        //   Indicates that a call is starting. 
        //   The booleans tell if we are on the client side or the server side, 
        //   and if the call is using AsyncProcessMessage
        [System.Security.SecurityCritical]  // auto-generated_required
        void ProcessMessageStart(IMessage reqMsg, bool bCliSide, bool bAsync);
        /// <internalonly/>
        //   Indicates that a call is returning.
        //   The booleans tell if we are on the client side or the server side, 
        //   and if the call is using AsyncProcessMessage
        [System.Security.SecurityCritical]  // auto-generated_required
        void ProcessMessageFinish(IMessage replyMsg, bool bCliSide, bool bAsync);
    }
}
