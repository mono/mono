//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime.Serialization;
    using Microsoft.InfoCards.Diagnostics;

    //
    // Summary
    // Wraps exceptions thrown during the request security token process.
    //
    [Serializable]
    public class UnsupportedPolicyOptionsException : System.Exception
    {
        public UnsupportedPolicyOptionsException()
        : base()
        {
        }

        public UnsupportedPolicyOptionsException( string message )
        : base( message )
        {
        }

        public UnsupportedPolicyOptionsException( string message, Exception innerException )
        : base( message, innerException )
        {
        }

        protected UnsupportedPolicyOptionsException( SerializationInfo info, StreamingContext context )
        : base( info, context )
        {
        }
    }

}
