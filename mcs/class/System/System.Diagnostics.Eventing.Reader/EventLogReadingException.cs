namespace System.Diagnostics.Eventing.Reader
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class EventLogReadingException : EventLogException
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventLogReadingException()
        {
        }

        internal EventLogReadingException(int errorCode) : base(errorCode)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventLogReadingException(string message) : base(message)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected EventLogReadingException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventLogReadingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

