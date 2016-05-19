//------------------------------------------------------------------------------
// <copyright file="FormsAuthenticationEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * CustomAuthenticationEventHandler class
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */
namespace System.Web.Security {
    using  System.Security.Principal;


    /// <devdoc>
    ///    <para>This delegate defines the signature for the
    ///       FormsAuthentication_OnAuthenticate event handler.</para>
    /// </devdoc>
    public delegate void FormsAuthenticationEventHandler(Object sender,  FormsAuthenticationEventArgs e);
}
