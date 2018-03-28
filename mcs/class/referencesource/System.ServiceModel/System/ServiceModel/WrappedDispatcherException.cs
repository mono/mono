//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    class WrappedDispatcherException : SystemException
    {
        public WrappedDispatcherException()
            : base()
        {
        }

        public WrappedDispatcherException(string message)
            : base(message)
        {
        }

        public WrappedDispatcherException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public WrappedDispatcherException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
