// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//
//   The IContributeDynamicSink interface is implemented by properties
//   that are registered at run-time through the RemotingServices.
//   RegisterDynamicProperty API. These properties can contribute sinks
//   that are notified when remoting calls start/finish. 
//
//   See also RemotingServices.RegisterDynamicProperty API.
//
namespace System.Runtime.Remoting.Contexts {
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Security.Permissions;
    using System;
    /// <internalonly/>
    [System.Runtime.InteropServices.ComVisible(true)]
    public interface IContributeDynamicSink
    {
        /// <internalonly/>
        //   Returns the message sink that will be notified of call start/finish events
        //   through the IDynamicMessageSink interface.
        [System.Security.SecurityCritical]  // auto-generated_required
        IDynamicMessageSink GetDynamicSink();
    }
}
