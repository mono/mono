//------------------------------------------------------------------------------
// <copyright file="ApplicatonManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Hosting {

    using System.Web;
    using System.Web.Configuration;
    using System.Runtime.Remoting.Messaging;
    using System.Security.Permissions;
    
    internal class ContextBase {

        internal static Object Current {
            get {
                return CallContext.HostContext;
            }

            [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
            set {
                CallContext.HostContext = value;
            }
        }

        // static methods

        internal static Object SwitchContext(Object newContext) {
            Object oldContext = CallContext.HostContext;
            if (oldContext != newContext)
                CallContext.HostContext = newContext;
            return oldContext;
        }
    }
}
