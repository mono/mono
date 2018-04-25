//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class LambdaSerializationException : Exception
    {
        public LambdaSerializationException()
            : base(SR.LambdaNotXamlSerializable)
        {
        }

        public LambdaSerializationException(string message)
            : base(message)
        {
        }

        public LambdaSerializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected LambdaSerializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {            
        }
    }
}
