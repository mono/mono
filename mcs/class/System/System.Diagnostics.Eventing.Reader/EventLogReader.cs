namespace System.Diagnostics.Eventing.Reader
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class EventLogReader : IDisposable
    {
        private int batchSize;
        private ProviderMetadataCachedInformation cachedMetadataInformation;
        private int currentIndex;
        private int eventCount;
        private EventLogQuery eventQuery;
        private IntPtr[] eventsBuffer;
        private EventLogHandle handle;
        private bool isEof;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventLogReader(EventLogQuery eventQuery) : this(eventQuery, null)
        {
        }

        public EventLogReader(string path) : this(new EventLogQuery(path, PathType.LogName), null)
        {
        }

        [SecurityCritical]
        public EventLogReader(EventLogQuery eventQuery, EventBookmark bookmark)
        {
            if (eventQuery == null)
            {
                throw new ArgumentNullException("eventQuery");
            }
            string logfile = null;
            if (eventQuery.ThePathType == PathType.FilePath)
            {
                logfile = eventQuery.Path;
            }
            this.cachedMetadataInformation = new ProviderMetadataCachedInformation(eventQuery.Session, logfile, 50);
            this.eventQuery = eventQuery;
            this.batchSize = 0x40;
            this.eventsBuffer = new IntPtr[this.batchSize];
            int flags = 0;
            if (this.eventQuery.ThePathType == PathType.LogName)
            {
                flags |= 1;
            }
            else
            {
                flags |= 2;
            }
            if (this.eventQuery.ReverseDirection)
            {
                flags |= 0x200;
            }
            if (this.eventQuery.TolerateQueryErrors)
            {
                flags |= 0x1000;
            }
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            this.handle = NativeWrapper.EvtQuery(this.eventQuery.Session.Handle, this.eventQuery.Path, this.eventQuery.Query, flags);
            EventLogHandle bookmarkHandleFromBookmark = EventLogRecord.GetBookmarkHandleFromBookmark(bookmark);
            if (!bookmarkHandleFromBookmark.IsInvalid)
            {
                using (bookmarkHandleFromBookmark)
                {
                    NativeWrapper.EvtSeek(this.handle, 1L, bookmarkHandleFromBookmark, 0, Microsoft.Win32.UnsafeNativeMethods.EvtSeekFlags.EvtSeekRelativeToBookmark);
                }
            }
        }

        public EventLogReader(string path, PathType pathType) : this(new EventLogQuery(path, pathType), null)
        {
        }

        public void CancelReading()
        {
            NativeWrapper.EvtCancel(this.handle);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SecuritySafeCritical]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                EventLogPermissionHolder.GetEventLogPermission().Demand();
            }
            while (this.currentIndex < this.eventCount)
            {
                NativeWrapper.EvtClose(this.eventsBuffer[this.currentIndex]);
                this.currentIndex++;
            }
            if ((this.handle != null) && !this.handle.IsInvalid)
            {
                this.handle.Dispose();
            }
        }

        [SecurityCritical]
        private bool GetNextBatch(TimeSpan ts)
        {
            int totalMilliseconds;
            if (ts == TimeSpan.MaxValue)
            {
                totalMilliseconds = -1;
            }
            else
            {
                totalMilliseconds = (int) ts.TotalMilliseconds;
            }
            if (this.batchSize != this.eventsBuffer.Length)
            {
                this.eventsBuffer = new IntPtr[this.batchSize];
            }
            int returned = 0;
            if (!NativeWrapper.EvtNext(this.handle, this.batchSize, this.eventsBuffer, totalMilliseconds, 0, ref returned))
            {
                this.eventCount = 0;
                this.currentIndex = 0;
                return false;
            }
            this.currentIndex = 0;
            this.eventCount = returned;
            return true;
        }

        public EventRecord ReadEvent()
        {
            return this.ReadEvent(TimeSpan.MaxValue);
        }

        [SecurityCritical]
        public EventRecord ReadEvent(TimeSpan timeout)
        {
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            if (this.isEof)
            {
                throw new InvalidOperationException();
            }
            if (this.currentIndex >= this.eventCount)
            {
                this.GetNextBatch(timeout);
                if (this.currentIndex >= this.eventCount)
                {
                    this.isEof = true;
                    return null;
                }
            }
            EventLogRecord record = new EventLogRecord(new EventLogHandle(this.eventsBuffer[this.currentIndex], true), this.eventQuery.Session, this.cachedMetadataInformation);
            this.currentIndex++;
            return record;
        }

        public void Seek(EventBookmark bookmark)
        {
            this.Seek(bookmark, 0L);
        }

        [SecurityCritical]
        public void Seek(EventBookmark bookmark, long offset)
        {
            if (bookmark == null)
            {
                throw new ArgumentNullException("bookmark");
            }
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            this.SeekReset();
            using (EventLogHandle handle = EventLogRecord.GetBookmarkHandleFromBookmark(bookmark))
            {
                NativeWrapper.EvtSeek(this.handle, offset, handle, 0, Microsoft.Win32.UnsafeNativeMethods.EvtSeekFlags.EvtSeekRelativeToBookmark);
            }
        }

        [SecurityCritical]
        public void Seek(SeekOrigin origin, long offset)
        {
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            switch (origin)
            {
                case SeekOrigin.Begin:
                    this.SeekReset();
                    NativeWrapper.EvtSeek(this.handle, offset, EventLogHandle.Zero, 0, Microsoft.Win32.UnsafeNativeMethods.EvtSeekFlags.EvtSeekRelativeToFirst);
                    return;

                case SeekOrigin.Current:
                    if (offset < 0L)
                    {
                        if ((this.currentIndex + offset) >= 0L)
                        {
                            this.SeekCommon(offset);
                        }
                        else
                        {
                            this.SeekCommon(offset);
                        }
                        return;
                    }
                    if ((this.currentIndex + offset) >= this.eventCount)
                    {
                        this.SeekCommon(offset);
                        return;
                    }
                    for (int i = this.currentIndex; i < (this.currentIndex + offset); i++)
                    {
                        NativeWrapper.EvtClose(this.eventsBuffer[i]);
                    }
                    this.currentIndex += (int) offset;
                    return;

                case SeekOrigin.End:
                    this.SeekReset();
                    NativeWrapper.EvtSeek(this.handle, offset, EventLogHandle.Zero, 0, Microsoft.Win32.UnsafeNativeMethods.EvtSeekFlags.EvtSeekRelativeToLast);
                    return;
            }
        }

        [SecurityCritical]
        internal void SeekCommon(long offset)
        {
            offset -= this.eventCount - this.currentIndex;
            this.SeekReset();
            NativeWrapper.EvtSeek(this.handle, offset, EventLogHandle.Zero, 0, Microsoft.Win32.UnsafeNativeMethods.EvtSeekFlags.EvtSeekRelativeToCurrent);
        }

        [SecurityCritical]
        internal void SeekReset()
        {
            while (this.currentIndex < this.eventCount)
            {
                NativeWrapper.EvtClose(this.eventsBuffer[this.currentIndex]);
                this.currentIndex++;
            }
            this.currentIndex = 0;
            this.eventCount = 0;
            this.isEof = false;
        }

        public int BatchSize
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.batchSize;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.batchSize = value;
            }
        }

        public IList<EventLogStatus> LogStatus
        {
            [SecurityCritical]
            get
            {
                EventLogPermissionHolder.GetEventLogPermission().Demand();
                List<EventLogStatus> list = null;
                string[] strArray = null;
                int[] numArray = null;
                EventLogHandle handle = this.handle;
                if (handle.IsInvalid)
                {
                    throw new InvalidOperationException();
                }
                strArray = (string[]) NativeWrapper.EvtGetQueryInfo(handle, Microsoft.Win32.UnsafeNativeMethods.EvtQueryPropertyId.EvtQueryNames);
                numArray = (int[]) NativeWrapper.EvtGetQueryInfo(handle, Microsoft.Win32.UnsafeNativeMethods.EvtQueryPropertyId.EvtQueryStatuses);
                if (strArray.Length != numArray.Length)
                {
                    throw new InvalidOperationException();
                }
                list = new List<EventLogStatus>(strArray.Length);
                for (int i = 0; i < strArray.Length; i++)
                {
                    EventLogStatus item = new EventLogStatus(strArray[i], numArray[i]);
                    list.Add(item);
                }
                return list.AsReadOnly();
            }
        }
    }
}

