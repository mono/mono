//------------------------------------------------------------------------------
// <copyright file="ProfileEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * CustomAuthenticationEventHandler class
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */
namespace System.Web.Profile {
    using  System.Security.Principal;
    using  System.Web.Security;


    /// <devdoc>
    ///    <para>This delegate defines the signature for the
    ///       Profiles_OnAuthenticate event handler.</para>
    /// </devdoc>
    public delegate void ProfileEventHandler(Object sender,  ProfileEventArgs e);
}
