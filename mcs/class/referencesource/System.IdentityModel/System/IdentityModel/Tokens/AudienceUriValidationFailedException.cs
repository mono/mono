//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// Throw this exception a received Security token failed Audience Uri validation.
    /// </summary>
    [Serializable]
    public class AudienceUriValidationFailedException : SecurityTokenValidationException
    {
        /// <summary>
        /// Initializes a new instance of  <see cref="AudienceUriValidationFailedException"/>
        /// </summary>
        public AudienceUriValidationFailedException()
            : base( SR.GetString( SR.ID4183 ) )
        {
        }

        /// <summary>
        /// Initializes a new instance of  <see cref="AudienceUriValidationFailedException"/>
        /// </summary>
        public AudienceUriValidationFailedException( string message )
            : base( message )
        {
        }

        /// <summary>
        /// Initializes a new instance of  <see cref="AudienceUriValidationFailedException"/>
        /// </summary>
        public AudienceUriValidationFailedException( string message, Exception inner )
            : base( message, inner )
        {
        }

        /// <summary>
        /// Initializes a new instance of  <see cref="AudienceUriValidationFailedException"/>
        /// </summary>
        protected AudienceUriValidationFailedException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }
    }
}
