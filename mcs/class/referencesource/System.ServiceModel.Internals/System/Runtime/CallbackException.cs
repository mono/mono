//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    class CallbackException : FatalException
    {
        public CallbackException()
        {
        }

        public CallbackException(string message, Exception innerException) : base(message, innerException)
        {
            // This can't throw something like ArgumentException because that would be worse than
            // throwing the callback exception that was requested.
            Fx.Assert(innerException != null, "CallbackException requires an inner exception.");
            Fx.Assert(!Fx.IsFatal(innerException), "CallbackException can't be used to wrap fatal exceptions.");
        }
        protected CallbackException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
