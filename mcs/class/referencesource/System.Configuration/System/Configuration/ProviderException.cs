//------------------------------------------------------------------------------
// <copyright file="ProviderException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration.Provider {
    using  System.Collections.Specialized;
    using  System.Runtime.Serialization;

    [Serializable]
    public class ProviderException : Exception
    {
        public ProviderException() {}

        public ProviderException( string message )
            : base( message )
        {}

        public ProviderException( string message, Exception innerException )
            : base( message, innerException )
        {}

        protected ProviderException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {}
    }
}
