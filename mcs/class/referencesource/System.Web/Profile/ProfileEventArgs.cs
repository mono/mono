//------------------------------------------------------------------------------
// <copyright file="ProfileEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * ProfileEventArgs class
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */
namespace System.Web.Profile {
    using  System.Web.Security;
    using  System.Security.Principal;
    using  System.Security.Permissions;


    /// <devdoc>
    ///    <SPAN>The 
    ///       event argument passed to the Profiles_OnAuthenticate event.<SPAN> </SPAN>Contains a FormsIdentity object and the
    ///    IPrincipal object used for the context.</SPAN>
    /// </devdoc>
    public sealed class ProfileEventArgs : EventArgs {
        private HttpContext       _Context;


        /// <devdoc>
        ///    This is the HttpContext intrinsic - most
        ///    notably provides access to Request, Response, and User objects.
        /// </devdoc>
        public  HttpContext                 Context { get { return _Context;}}

        public  ProfileBase Profile { get { return _Profile; } set { _Profile = value; } }
        private ProfileBase _Profile;


        /// <devdoc>
        ///    Constructor
        /// </devdoc>
        public ProfileEventArgs(HttpContext context) {
            _Context = context;
        }
    }
}
