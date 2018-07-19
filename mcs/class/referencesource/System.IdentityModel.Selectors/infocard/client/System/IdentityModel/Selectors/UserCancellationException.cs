//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime.Serialization;

    //
    // Summary
    //  Exception class to indicate failure in generating the token as the user cancelled the operation
    //
    [Serializable]
    public class UserCancellationException : System.Exception
    {
        public UserCancellationException()
            : base()
        {
        }

        public UserCancellationException( string message )
            : base( message )
        {
        }

        public UserCancellationException( string message, Exception innerException )
            : base( message, innerException )
        {
        }

        protected UserCancellationException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }
    }

}
