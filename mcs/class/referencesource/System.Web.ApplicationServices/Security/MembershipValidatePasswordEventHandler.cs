//------------------------------------------------------------------------------
// <copyright file="MembershipValidatePasswordEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * MembershipValidatePasswordEventHandler class
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */
namespace System.Web.Security 
{
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web, Version=2.0.0.0, Culture=Neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public delegate void MembershipValidatePasswordEventHandler( Object sender,  ValidatePasswordEventArgs e );
}
