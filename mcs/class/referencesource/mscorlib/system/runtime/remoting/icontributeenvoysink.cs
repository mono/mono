// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//  The IContributeEnvoySink interface is implemented by properties
//  in the ServerContext that contribute sinks to the serverContext
//  and serverObject chains. The sinks contributed through this 
//  interface are expected to act as Envoys for the corresponding
//  property sinks in the ServerContext and ServerObject chains.
//
namespace System.Runtime.Remoting.Contexts {

    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Security.Permissions;
    using System;
    /// <internalonly/>
    [System.Runtime.InteropServices.ComVisible(true)]
    public interface IContributeEnvoySink
    {
        /// <internalonly/>
        //  Chain your message sink in front of the chain formed thus far and 
        //  return the composite sink chain. This method is used when creating
        //  the sink chain for X-Context cases. The server object is provided
        //  for the interest of object-specific 
        // 
        [System.Security.SecurityCritical]  // auto-generated_required
        IMessageSink GetEnvoySink(MarshalByRefObject obj, IMessageSink nextSink);
    }
}
