//------------------------------------------------------------------------------
// <copyright file="AppDomainProtocolHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;
    using System.Security.Permissions;

    public abstract class AppDomainProtocolHandler : MarshalByRefObject, IRegisteredObject  {

        protected AppDomainProtocolHandler() {
            HostingEnvironment.RegisterObject(this);
        }
 

        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure)]	
        public override Object InitializeLifetimeService(){
            return null; // never expire lease
        }
 

        public abstract void StartListenerChannel(IListenerChannelCallback listenerChannelCallback);


        public abstract void StopListenerChannel(int listenerChannelId, bool immediate);


        public abstract void StopProtocol(bool immediate);
 

        public virtual void Stop(bool immediate) {
            StopProtocol(true);
            HostingEnvironment.UnregisterObject(this);
        }
    }
}

