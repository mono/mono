//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime.Serialization;

    //
    // Indicates that the Ui Agent process failed in the initialization stage
    // For example during creation of the private desktop.
    //
    [Serializable]
    internal class UIInitializationException : Exception
    {

        public UIInitializationException()
        : base()
        {
        }

        public UIInitializationException( string message )
        : base( message )
        {
        }

        public UIInitializationException( string message, Exception innerException )
        : base( message, innerException )
        {
        }

        protected UIInitializationException( SerializationInfo info, StreamingContext context )
        : base( info, context )
        {
        }
        
    }
}
