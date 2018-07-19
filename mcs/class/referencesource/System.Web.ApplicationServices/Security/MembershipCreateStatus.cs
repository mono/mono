//------------------------------------------------------------------------------
// <copyright file="MembershipCreateStatus.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Security {
    using System;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web, Version=2.0.0.0, Culture=Neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public enum MembershipCreateStatus {

        Success = 0,

        InvalidUserName = 1,  // invalid user name

        InvalidPassword = 2, // new password was not accepted (invalid format)

        InvalidQuestion = 3, // new question was not accepted (invalid format)

        InvalidAnswer = 4, // new passwordAnswer was not acceppted (invalid format)

        InvalidEmail = 5, // new email was not accepted (invalid format)

        DuplicateUserName = 6, // username already exists

        DuplicateEmail = 7, // email already exists

        UserRejected = 8, // provider rejected user (for some user-specific reason)

        InvalidProviderUserKey = 9,  // new provider user key was not accepted (invalid format)

        DuplicateProviderUserKey = 10, // provider user key already exists

        ProviderError = 11  // provider-specific error (couldn't map onto this enumeration)
    }

}
