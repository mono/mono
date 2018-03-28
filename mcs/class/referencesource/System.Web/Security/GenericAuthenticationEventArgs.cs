//------------------------------------------------------------------------------
// <copyright file="GenericAuthenticationEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * DefaultAuthenticationEventArgs class
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */
namespace System.Web.Security {
    using  System.Security.Principal;
    using System.Security.Permissions;


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class DefaultAuthenticationEventArgs : EventArgs {
        private HttpContext       _Context;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public  HttpContext       Context { get { return _Context;}}


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public DefaultAuthenticationEventArgs(HttpContext context) {
            _Context = context;
        }
    }
}
