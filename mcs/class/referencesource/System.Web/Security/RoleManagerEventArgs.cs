//------------------------------------------------------------------------------
// <copyright file="RoleManagerEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * RoleManagerEventArgs class
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */
namespace System.Web.Security {
    using  System.Security.Principal;
    using System.Security.Permissions;


    /// <devdoc>
    ///    <SPAN>The event argument passed to the RoleManager_GetRoles event.<SPAN> 
    /// </devdoc>
    public sealed class RoleManagerEventArgs : EventArgs {
        private HttpContext       _Context;


        public  bool  RolesPopulated { 
            get { return _RolesPopulated;} 
            set { 
                _RolesPopulated = value;
            }
        }
        private bool _RolesPopulated;


        /// <devdoc>
        ///    This is the HttpContext intrinsic - most
        ///    notably provides access to Request, Response, and User objects.
        /// </devdoc>
        public  HttpContext       Context { get { return _Context;}}


        /// <devdoc>
        ///    Constructor
        /// </devdoc>
        public RoleManagerEventArgs(HttpContext context) {
            _Context = context;
        }
    }
}
