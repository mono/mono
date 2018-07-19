//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime.Serialization;

    //
    // Summary
    // Wraps exceptions thrown during the request security token process.
    //
    [Serializable]
    public class StsCommunicationException : System.Exception
    {
        public StsCommunicationException()
        : base()
        {
        }

        public StsCommunicationException( string message )
        : base( message )
        {
        }

        public StsCommunicationException( string message, Exception innerException )
        : base( message, innerException )
        {
        }

        protected StsCommunicationException( SerializationInfo info, StreamingContext context )
        : base( info, context )
        {
        }
    }
}
