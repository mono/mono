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
    public class PolicyValidationException : System.Exception
    {
        public PolicyValidationException()
        : base()
        {
        }

        public PolicyValidationException( string message )
        : base( message )
        {
        }

        public PolicyValidationException( string message, Exception innerException )
        : base( message, innerException )
        {
        }

        protected PolicyValidationException( SerializationInfo info, StreamingContext context )
        : base( info, context )
        {
        }
    }

}
