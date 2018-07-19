//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Runtime.Serialization;

    [Serializable]
    public class SecurityTokenException : SystemException
    {
        public SecurityTokenException()
            : base()
        {
        }

        public SecurityTokenException(String message)
            : base(message)
        {
        }

        public SecurityTokenException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected SecurityTokenException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
