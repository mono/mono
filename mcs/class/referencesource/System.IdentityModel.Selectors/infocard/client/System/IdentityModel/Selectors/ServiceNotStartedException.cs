//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime.Serialization;

    //
    // Summary
    //  Exception class to indicate that the infocard service has not been started on the system
    //
    [Serializable]
    public class ServiceNotStartedException : System.Exception
    {
        public ServiceNotStartedException()
            : base()
        {
        }

        public ServiceNotStartedException( string message )
            : base( message )
        {
        }

        public ServiceNotStartedException( string message, Exception innerException )
            : base( message, innerException )
        {
        }

        protected ServiceNotStartedException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }
    }
}
