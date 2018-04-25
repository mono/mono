//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections;
    using System.ServiceModel.Channels;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml;

    [Serializable]
    class SessionKeyExpiredException : MessageSecurityException
    {
        public SessionKeyExpiredException()
            : base()
        {
        }

        public SessionKeyExpiredException(String message)
            : base(message)
        {
        }

        public SessionKeyExpiredException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected SessionKeyExpiredException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
