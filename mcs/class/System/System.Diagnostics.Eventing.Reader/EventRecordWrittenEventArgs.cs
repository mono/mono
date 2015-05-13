namespace System.Diagnostics.Eventing.Reader
{
    using System;
    using System.Runtime;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class EventRecordWrittenEventArgs : EventArgs
    {
        private Exception exception;
        private System.Diagnostics.Eventing.Reader.EventRecord record;

        internal EventRecordWrittenEventArgs(EventLogRecord record)
        {
            this.record = record;
        }

        internal EventRecordWrittenEventArgs(Exception exception)
        {
            this.exception = exception;
        }

        public Exception EventException
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.exception;
            }
        }

        public System.Diagnostics.Eventing.Reader.EventRecord EventRecord
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.record;
            }
        }
    }
}

