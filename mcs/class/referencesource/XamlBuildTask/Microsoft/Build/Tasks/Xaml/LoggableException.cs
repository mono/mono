//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    class LoggableException : Exception
    {
        int lineNumber;
        int linePosition;

        public LoggableException()
        {
        }

        public int LineNumber
        {
            get
            {
                return this.lineNumber;
            }
            set
            {
                this.lineNumber = value;
            }
        }

        public int LinePosition
        {
            get
            {
                return this.linePosition;
            }
            set
            {
                this.linePosition = value;
            }
        }
        
        public LoggableException(string message)
            : base(message)
        {
        }

        public LoggableException(Exception innerException)
            : base(innerException.Message, innerException)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            if (info != null)
            {
                info.AddValue("lineNumber", this.lineNumber);
                info.AddValue("columnNumber", this.linePosition);
            }
        }
    }
}
