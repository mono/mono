namespace System.Diagnostics.Eventing.Reader
{
    using System;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class EventLogWatcher : IDisposable
    {
        private EventLogException asyncException;
        private EventBookmark bookmark;
        private ProviderMetadataCachedInformation cachedMetadataInformation;
        private int callbackThreadId;
        private EventLogQuery eventQuery;
        private IntPtr[] eventsBuffer;
        private EventLogHandle handle;
        private bool isSubscribing;
        private int numEventsInBuffer;
        private bool readExistingEvents;
        private RegisteredWaitHandle registeredWaitHandle;
        private AutoResetEvent subscriptionWaitHandle;
        private AutoResetEvent unregisterDoneHandle;

        public event EventHandler<EventRecordWrittenEventArgs> EventRecordWritten;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventLogWatcher(EventLogQuery eventQuery) : this(eventQuery, null, false)
        {
        }

        public EventLogWatcher(string path) : this(new EventLogQuery(path, PathType.LogName), null, false)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventLogWatcher(EventLogQuery eventQuery, EventBookmark bookmark) : this(eventQuery, bookmark, false)
        {
        }

        public EventLogWatcher(EventLogQuery eventQuery, EventBookmark bookmark, bool readExistingEvents)
        {
            if (eventQuery == null)
            {
                throw new ArgumentNullException("eventQuery");
            }
            if (bookmark != null)
            {
                readExistingEvents = false;
            }
            this.eventQuery = eventQuery;
            this.readExistingEvents = readExistingEvents;
            if (this.eventQuery.ReverseDirection)
            {
                throw new InvalidOperationException();
            }
            this.eventsBuffer = new IntPtr[0x40];
            this.cachedMetadataInformation = new ProviderMetadataCachedInformation(eventQuery.Session, null, 50);
            this.bookmark = bookmark;
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
                this.StopSubscribing();
            }
            else
            {
                for (int i = 0; i < this.numEventsInBuffer; i++)
                {
                    if (this.eventsBuffer[i] != IntPtr.Zero)
                    {
                        NativeWrapper.EvtClose(this.eventsBuffer[i]);
                        this.eventsBuffer[i] = IntPtr.Zero;
                    }
                }
                this.numEventsInBuffer = 0;
            }
        }

        [SecurityCritical]
        private void HandleEventsRequestCompletion()
        {
            if (this.asyncException != null)
            {
                EventRecordWrittenEventArgs eventArgs = new EventRecordWrittenEventArgs(this.asyncException.Data["RealException"] as Exception);
                this.IssueCallback(eventArgs);
            }
            for (int i = 0; i < this.numEventsInBuffer; i++)
            {
                if (!this.isSubscribing)
                {
                    return;
                }
                EventLogRecord record = new EventLogRecord(new EventLogHandle(this.eventsBuffer[i], true), this.eventQuery.Session, this.cachedMetadataInformation);
                EventRecordWrittenEventArgs args2 = new EventRecordWrittenEventArgs(record);
                this.eventsBuffer[i] = IntPtr.Zero;
                this.IssueCallback(args2);
            }
        }

        private void IssueCallback(EventRecordWrittenEventArgs eventArgs)
        {
            if (this.EventRecordWritten != null)
            {
                this.EventRecordWritten(this, eventArgs);
            }
        }

        [SecuritySafeCritical]
        private void RequestEvents()
        {
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            this.asyncException = null;
            bool flag = false;
            do
            {
                if (!this.isSubscribing)
                {
                    return;
                }
                try
                {
                    if (!NativeWrapper.EvtNext(this.handle, this.eventsBuffer.Length, this.eventsBuffer, 0, 0, ref this.numEventsInBuffer))
                    {
                        break;
                    }
                }
                catch (Exception exception)
                {
                    this.asyncException = new EventLogException();
                    this.asyncException.Data.Add("RealException", exception);
                }
                this.HandleEventsRequestCompletion();
            }
            while (flag);
        }

        [SecuritySafeCritical]
        internal void StartSubscribing()
        {
            if (this.isSubscribing)
            {
                throw new InvalidOperationException();
            }
            int flags = 0;
            if (this.bookmark != null)
            {
                flags |= 3;
            }
            else if (this.readExistingEvents)
            {
                flags |= 2;
            }
            else
            {
                flags |= 1;
            }
            if (this.eventQuery.TolerateQueryErrors)
            {
                flags |= 0x1000;
            }
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            this.callbackThreadId = -1;
            this.unregisterDoneHandle = new AutoResetEvent(false);
            this.subscriptionWaitHandle = new AutoResetEvent(false);
            EventLogHandle bookmarkHandleFromBookmark = EventLogRecord.GetBookmarkHandleFromBookmark(this.bookmark);
            using (bookmarkHandleFromBookmark)
            {
                this.handle = NativeWrapper.EvtSubscribe(this.eventQuery.Session.Handle, this.subscriptionWaitHandle.SafeWaitHandle, this.eventQuery.Path, this.eventQuery.Query, bookmarkHandleFromBookmark, IntPtr.Zero, IntPtr.Zero, flags);
            }
            this.isSubscribing = true;
            this.RequestEvents();
            this.registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(this.subscriptionWaitHandle, new WaitOrTimerCallback(this.SubscribedEventsAvailableCallback), null, -1, false);
        }

        [SecuritySafeCritical]
        internal void StopSubscribing()
        {
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            this.isSubscribing = false;
            if (this.registeredWaitHandle != null)
            {
                this.registeredWaitHandle.Unregister(this.unregisterDoneHandle);
                if ((this.callbackThreadId != Thread.CurrentThread.ManagedThreadId) && (this.unregisterDoneHandle != null))
                {
                    this.unregisterDoneHandle.WaitOne();
                }
                this.registeredWaitHandle = null;
            }
            if (this.unregisterDoneHandle != null)
            {
                this.unregisterDoneHandle.Close();
                this.unregisterDoneHandle = null;
            }
            if (this.subscriptionWaitHandle != null)
            {
                this.subscriptionWaitHandle.Close();
                this.subscriptionWaitHandle = null;
            }
            for (int i = 0; i < this.numEventsInBuffer; i++)
            {
                if (this.eventsBuffer[i] != IntPtr.Zero)
                {
                    NativeWrapper.EvtClose(this.eventsBuffer[i]);
                    this.eventsBuffer[i] = IntPtr.Zero;
                }
            }
            this.numEventsInBuffer = 0;
            if ((this.handle != null) && !this.handle.IsInvalid)
            {
                this.handle.Dispose();
            }
        }

        internal void SubscribedEventsAvailableCallback(object state, bool timedOut)
        {
            this.callbackThreadId = Thread.CurrentThread.ManagedThreadId;
            try
            {
                this.RequestEvents();
            }
            finally
            {
                this.callbackThreadId = -1;
            }
        }

        public bool Enabled
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isSubscribing;
            }
            set
            {
                if (value && !this.isSubscribing)
                {
                    this.StartSubscribing();
                }
                else if (!value && this.isSubscribing)
                {
                    this.StopSubscribing();
                }
            }
        }
    }
}

