namespace System.Diagnostics.Eventing.Reader
{
    using Microsoft.Win32;
    using System;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class EventLogInformation
    {
        private DateTime? creationTime;
        private int? fileAttributes;
        private long? fileSize;
        private bool? isLogFull;
        private DateTime? lastAccessTime;
        private DateTime? lastWriteTime;
        private long? oldestRecordNumber;
        private long? recordCount;

        [SecuritySafeCritical]
        internal EventLogInformation(EventLogSession session, string channelName, PathType pathType)
        {
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            EventLogHandle handle = NativeWrapper.EvtOpenLog(session.Handle, channelName, pathType);
            using (handle)
            {
                this.creationTime = (DateTime?) NativeWrapper.EvtGetLogInfo(handle, Microsoft.Win32.UnsafeNativeMethods.EvtLogPropertyId.EvtLogCreationTime);
                this.lastAccessTime = (DateTime?) NativeWrapper.EvtGetLogInfo(handle, Microsoft.Win32.UnsafeNativeMethods.EvtLogPropertyId.EvtLogLastAccessTime);
                this.lastWriteTime = (DateTime?) NativeWrapper.EvtGetLogInfo(handle, Microsoft.Win32.UnsafeNativeMethods.EvtLogPropertyId.EvtLogLastWriteTime);
                long? nullable = (long?) NativeWrapper.EvtGetLogInfo(handle, Microsoft.Win32.UnsafeNativeMethods.EvtLogPropertyId.EvtLogFileSize);
                this.fileSize = nullable.HasValue ? new long?(nullable.GetValueOrDefault()) : null;
                int? nullable3 = (int?) NativeWrapper.EvtGetLogInfo(handle, Microsoft.Win32.UnsafeNativeMethods.EvtLogPropertyId.EvtLogAttributes);
                this.fileAttributes = nullable3.HasValue ? new int?(nullable3.GetValueOrDefault()) : null;
                long? nullable5 = (long?) NativeWrapper.EvtGetLogInfo(handle, Microsoft.Win32.UnsafeNativeMethods.EvtLogPropertyId.EvtLogNumberOfLogRecords);
                this.recordCount = nullable5.HasValue ? new long?(nullable5.GetValueOrDefault()) : null;
                long? nullable7 = (long?) NativeWrapper.EvtGetLogInfo(handle, Microsoft.Win32.UnsafeNativeMethods.EvtLogPropertyId.EvtLogOldestRecordNumber);
                this.oldestRecordNumber = nullable7.HasValue ? new long?(nullable7.GetValueOrDefault()) : null;
                this.isLogFull = (bool?) NativeWrapper.EvtGetLogInfo(handle, Microsoft.Win32.UnsafeNativeMethods.EvtLogPropertyId.EvtLogFull);
            }
        }

        public int? Attributes
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.fileAttributes;
            }
        }

        public DateTime? CreationTime
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.creationTime;
            }
        }

        public long? FileSize
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.fileSize;
            }
        }

        public bool? IsLogFull
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isLogFull;
            }
        }

        public DateTime? LastAccessTime
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.lastAccessTime;
            }
        }

        public DateTime? LastWriteTime
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.lastWriteTime;
            }
        }

        public long? OldestRecordNumber
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.oldestRecordNumber;
            }
        }

        public long? RecordCount
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.recordCount;
            }
        }
    }
}

