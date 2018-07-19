//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IO
{
    using System.Runtime.Serialization;

    [Serializable]
    public class PipeException : IOException
    {
        public PipeException()
            : base()
        {
        }

        public PipeException(string message)
            : base(message)
        {
        }

        public PipeException(string message, int errorCode)
            : base(message, errorCode)
        {
        }

        public PipeException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected PipeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public virtual int ErrorCode
        {
            get { return this.HResult; }
        }
    }
}
