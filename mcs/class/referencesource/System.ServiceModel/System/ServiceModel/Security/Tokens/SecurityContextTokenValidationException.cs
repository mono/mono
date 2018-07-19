//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security.Tokens
{
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.IdentityModel.Tokens;

    [Serializable]
    class SecurityContextTokenValidationException : SecurityTokenValidationException
    {
        public SecurityContextTokenValidationException()
            : base()
        {
        }

        public SecurityContextTokenValidationException(String message)
            : base(message)
        {
        }

        public SecurityContextTokenValidationException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected SecurityContextTokenValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
