//------------------------------------------------------------------------------
// <copyright file="WindowsAuthenticationEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * WindowsAuthenticationEventArgs class
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */
namespace System.Web.Security {
    using  System.Security.Principal;
    using System.Security.Permissions;


    /// <devdoc>
    ///    <para>The Windows authentication module raises this event. This 
    ///       is the event argument passed to the WindowsAuthentication_OnAuthenticate event.
    ///       Contains a WindowsIdentity object and the IPrincipal object used for the context.</para>
    /// </devdoc>
    public sealed class WindowsAuthenticationEventArgs : EventArgs {
        private IPrincipal             _User;
        private HttpContext       _Context;
        private WindowsIdentity   _Identity;


        /// <devdoc>
        ///    <para>IPrincipal object to be associated with the request.
        ///       <SPAN>
        ///       </SPAN>The user object should be attached 
        ///       to the context.<SPAN>
        ///    </SPAN>If User is non null
        ///    and Context.User is null, the WindowsAuthenticationModule will initialize
        ///    Context.User with WindowsAuthenticationEventArgs.User.</para>
        /// </devdoc>
        public  IPrincipal            User { 
            get { return _User;} 

            [SecurityPermission(SecurityAction.Demand, ControlPrincipal=true)]
            set { 
                _User = value;
            }
        }


        /// <devdoc>
        ///    The HttpContext intrinsic (provides access to
        ///    Request, Response, and User objects).
        /// </devdoc>
        public  HttpContext      Context { get { return _Context;}}

        /// <devdoc>
        ///    An authenticated Windows identity.
        /// </devdoc>
        public  WindowsIdentity  Identity { get { return _Identity;}}


        /// <devdoc>
        ///    <para>Initializes a newly created instance of the
        ///       WindowsAuthenticationEventArgs Class.</para>
        /// </devdoc>
        public WindowsAuthenticationEventArgs(WindowsIdentity identity, HttpContext context) {
            _Identity = identity;
            _Context = context;
        }
    }
}
