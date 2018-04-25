//------------------------------------------------------------------------------
// <copyright file="ProcessProtocolHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;
    using System.Security.Permissions;
    
    public abstract class ProcessProtocolHandler : MarshalByRefObject {

        protected ProcessProtocolHandler() {
        }


        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure)]
        public override Object InitializeLifetimeService(){
            return null; // never expire lease
        }


        public abstract void StartListenerChannel(IListenerChannelCallback listenerChannelCallback, IAdphManager AdphManager);


        public abstract void StopListenerChannel(int listenerChannelId, bool immediate);


        public abstract void StopProtocol(bool immediate);
    }

}


