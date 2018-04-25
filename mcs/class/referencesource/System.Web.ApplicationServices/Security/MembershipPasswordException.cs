//------------------------------------------------------------------------------
// <copyright file="MembershipPasswordException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Security {
    using System;
    using System.Runtime.Serialization;
    using System.Web;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;    

    [Serializable]
    [TypeForwardedFrom("System.Web, Version=2.0.0.0, Culture=Neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class MembershipPasswordException : Exception
    {

        public MembershipPasswordException(String message) : base(message)
        { }


        protected MembershipPasswordException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }

        public MembershipPasswordException() 
        { }

        public MembershipPasswordException(String message, Exception innerException) : base(message, innerException) 
        { } 
    }
}
