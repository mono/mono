//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    [Serializable]
    public class ExpiredSecurityTokenException : MessageSecurityException
    {

        public ExpiredSecurityTokenException()
            : base()
        {
        }

        public ExpiredSecurityTokenException(String message)
            : base(message)
        {
        }

        public ExpiredSecurityTokenException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ExpiredSecurityTokenException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
