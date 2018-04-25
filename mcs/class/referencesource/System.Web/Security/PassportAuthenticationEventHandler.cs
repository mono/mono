//------------------------------------------------------------------------------
// <copyright file="PassportAuthenticationEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * PassportAuthenticationEventHandler class
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */
namespace System.Web.Security {
    using  System.Security.Principal;


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [Obsolete("This type is obsolete. The Passport authentication product is no longer supported and has been superseded by Live ID.")]
    public delegate void PassportAuthenticationEventHandler(Object sender, PassportAuthenticationEventArgs e);
}
