//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// This indicates an error has occured while processing an encrypted security token
    /// </summary>
    [Serializable]
    public class EncryptedTokenDecryptionFailedException : SecurityTokenException
    {
        /// <summary>
        /// Initializes a new instance of  <see cref="EncryptedTokenDecryptionFailedException"/>
        /// </summary>
        public EncryptedTokenDecryptionFailedException()
            : base( SR.GetString( SR.ID4022 ) )
        {
        }

        /// <summary>
        /// Initializes a new instance of  <see cref="EncryptedTokenDecryptionFailedException"/>
        /// </summary>
        public EncryptedTokenDecryptionFailedException( string message )
            : base( message )
        {
        }

        /// <summary>
        /// Initializes a new instance of  <see cref="EncryptedTokenDecryptionFailedException"/>
        /// </summary>
        public EncryptedTokenDecryptionFailedException( string message, Exception inner )
            : base( message, inner )
        {
        }

        /// <summary>
        /// Initializes a new instance of  <see cref="EncryptedTokenDecryptionFailedException"/>
        /// </summary>
        protected EncryptedTokenDecryptionFailedException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }
    }
}
