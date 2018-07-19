//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime.Serialization;

    //
    // Summary
    //  Exception class to indicate failure in generating the token as the recipient was not trusted
    //  by the user
    //
    [Serializable]
    public class UntrustedRecipientException : System.Exception
    {
        public UntrustedRecipientException()
            : base()
        {
        }

        public UntrustedRecipientException( string message )
            : base( message )
        {
        }

        public UntrustedRecipientException( string message, Exception innerException )
            : base( message, innerException )
        {
        }

        protected UntrustedRecipientException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }
    }

}
