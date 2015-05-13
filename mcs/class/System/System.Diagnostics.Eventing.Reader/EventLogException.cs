namespace System.Diagnostics.Eventing.Reader
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class EventLogException : Exception, ISerializable
    {
        private int errorCode;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventLogException()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected EventLogException(int errorCode)
        {
            this.errorCode = errorCode;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventLogException(string message) : base(message)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected EventLogException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventLogException(string message, Exception innerException) : base(message, innerException)
        {
        }

        [SecurityCritical, SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("errorCode", this.errorCode);
            base.GetObjectData(info, context);
        }

        internal static void Throw(int errorCode)
        {
            switch (errorCode)
            {
                case 0x4c7:
                case 0x71a:
                    throw new OperationCanceledException();

                case 2:
                case 3:
                case 0x3a9f:
                case 0x3a9a:
                case 0x3ab3:
                case 0x3ab4:
                    throw new EventLogNotFoundException(errorCode);

                case 5:
                    throw new UnauthorizedAccessException();

                case 13:
                case 0x3a9d:
                    throw new EventLogInvalidDataException(errorCode);

                case 0x3aa3:
                case 0x3aa4:
                    throw new EventLogReadingException(errorCode);

                case 0x3abd:
                    throw new EventLogProviderDisabledException(errorCode);
            }
            throw new EventLogException(errorCode);
        }

        public override string Message
        {
            [SecurityCritical]
            get
            {
                EventLogPermissionHolder.GetEventLogPermission().Demand();
                Win32Exception exception = new Win32Exception(this.errorCode);
                return exception.Message;
            }
        }
    }
}

