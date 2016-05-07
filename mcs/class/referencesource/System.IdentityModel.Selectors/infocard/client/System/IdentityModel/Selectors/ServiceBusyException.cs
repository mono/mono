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
    public class ServiceBusyException : System.Exception
    {
        public ServiceBusyException()
        : base()
        {
        }

        public ServiceBusyException( string message )
        : base( message )
        {
        }

        public ServiceBusyException( string message, Exception innerException )
        : base( message, innerException )
        {
        }

        protected ServiceBusyException( SerializationInfo info, StreamingContext context )
        : base( info, context )
        {
        }
    }

}
