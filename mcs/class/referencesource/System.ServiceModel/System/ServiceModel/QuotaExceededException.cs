//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class QuotaExceededException : SystemException
    {
        public QuotaExceededException()
            : base()
        {
        }

        public QuotaExceededException(string message)
            : base(message)
        {
        }

        public QuotaExceededException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected QuotaExceededException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

