//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization
{
    using System;

    [Serializable]
    public class InvalidDataContractException : Exception
    {
        public InvalidDataContractException()
            : base()
        {
        }

        public InvalidDataContractException(String message)
            : base(message)
        {
        }

        public InvalidDataContractException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected InvalidDataContractException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

    }
}

