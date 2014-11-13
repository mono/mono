//------------------------------------------------------------------------------
// <copyright file="GenericAuthenticationEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * DefaultAuthenticationEventHandler class
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */
namespace System.Web.Security {
    using  System.Security.Principal;


    /// <devdoc>
    ///    This delegate defines the signature of the
    ///    event handler for the PassportAuthentication_OnAuthenticate event.
    /// </devdoc>
    public delegate void DefaultAuthenticationEventHandler(Object sender,  DefaultAuthenticationEventArgs e);
}
