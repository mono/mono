//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class SecurityAccessDeniedException : CommunicationException
    {
        public SecurityAccessDeniedException()
            : base()
        {
        }

        public SecurityAccessDeniedException(String message)
            : base(message)
        {
        }

        public SecurityAccessDeniedException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected SecurityAccessDeniedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
