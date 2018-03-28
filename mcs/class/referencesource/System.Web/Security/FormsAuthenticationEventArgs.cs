//------------------------------------------------------------------------------
// <copyright file="FormsAuthenticationEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * FormsAuthenticationEventArgs class
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */
namespace System.Web.Security {
    using  System.Security.Principal;
    using System.Security.Permissions;


    /// <devdoc>
    ///    <SPAN>The 
    ///       event argument passed to the FormsAuthentication_OnAuthenticate event.<SPAN> </SPAN>Contains a FormsIdentity object and the
    ///    IPrincipal object used for the context.</SPAN>
    /// </devdoc>
    public sealed class FormsAuthenticationEventArgs : EventArgs {
        private IPrincipal        _User;
        private HttpContext       _Context;


        /// <devdoc>
        ///    <para><SPAN>The 
        ///       IPrincipal object to be associated with the request.<SPAN>
        ///    </SPAN></SPAN></para>
        /// </devdoc>
        public  IPrincipal        User { 
            get { return _User;} 

            [SecurityPermission(SecurityAction.Demand, ControlPrincipal=true)]
            set { 
                _User = value;
            }
        }

        /// <devdoc>
        ///    This is the HttpContext intrinsic - most
        ///    notably provides access to Request, Response, and User objects.
        /// </devdoc>
        public  HttpContext       Context { get { return _Context;}}


        /// <devdoc>
        ///    Constructor
        /// </devdoc>
        public FormsAuthenticationEventArgs(HttpContext context) {
            _Context = context;
        }
    }
}
