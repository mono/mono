// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class: EventLogWatcher
**
** Purpose: 
** This public class is used for subscribing to event record 
** notifications from event log. 
**
============================================================*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Security.Permissions;
using Microsoft.Win32;

namespace System.Diagnostics.Eventing.Reader {

    /// <summary>
    /// Used for subscribing to event record notifications from 
    /// event log. 
    /// </summary>
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public class EventLogWatcher : IDisposable {

        public event EventHandler<EventRecordWrittenEventArgs> EventRecordWritten;

        private EventLogQuery eventQuery;
        private EventBookmark bookmark;
        private bool readExistingEvents;

        private EventLogHandle handle;
        private IntPtr[] eventsBuffer;
        private int numEventsInBuffer;
        private bool isSubscribing;
        private int callbackThreadId;

        AutoResetEvent subscriptionWaitHandle;
        AutoResetEvent unregisterDoneHandle;
        RegisteredWaitHandle registeredWaitHandle;

        /// <summary>
        /// Maintains cached display / metadata information returned from 
        /// EventRecords that were obtained from this reader.
        /// </summary>
        ProviderMetadataCachedInformation cachedMetadataInformation;

        EventLogException asyncException;

        public EventLogWatcher(string path)
            : this(new EventLogQuery(path, PathType.LogName), null, false) {
        }

        public EventLogWatcher(EventLogQuery eventQuery)
            : this(eventQuery, null, false) {
        }

        public EventLogWatcher(EventLogQuery eventQuery, EventBookmark bookmark)
            : this(eventQuery, bookmark, false) {
        }

        public EventLogWatcher(EventLogQuery eventQuery, EventBookmark bookmark, bool readExistingEvents) {

            if (eventQuery == null)
                throw new ArgumentNullException("eventQuery");

            if (bookmark != null)
                readExistingEvents = false;

            //explicit data
            this.eventQuery = eventQuery;
            this.readExistingEvents = readExistingEvents;

            if (this.eventQuery.ReverseDirection)
                throw new InvalidOperationException();

            this.eventsBuffer = new IntPtr[64];
            this.cachedMetadataInformation = new ProviderMetadataCachedInformation(eventQuery.Session, null, 50);
            this.bookmark = bookmark;
        }

        public bool Enabled {
            get {
                return isSubscribing;
            }
            set {
                if (value && !isSubscribing) {
                    StartSubscribing();
                }
                else if (!value && isSubscribing) {
                    StopSubscribing();
                }
            }
        }

        [System.Security.SecuritySafeCritical]
        internal void StopSubscribing() {

            EventLogPermissionHolder.GetEventLogPermission().Demand();

            //
            // need to set isSubscribing to false before waiting for completion of callback.
            // 
            this.isSubscribing = false;

            if (this.registeredWaitHandle != null) {

                this.registeredWaitHandle.Unregister( this.unregisterDoneHandle );

                if (this.callbackThreadId != Thread.CurrentThread.ManagedThreadId) {
                    //
                    // not calling Stop from within callback - wait for 
                    // any outstanding callbacks to complete.
                    // 
                    if ( this.unregisterDoneHandle != null )
                        this.unregisterDoneHandle.WaitOne();
                }
   
                this.registeredWaitHandle = null;
            }

            if (this.unregisterDoneHandle != null) {
                this.unregisterDoneHandle.Close();
                this.unregisterDoneHandle = null;
            }

            if (this.subscriptionWaitHandle != null) {
                this.subscriptionWaitHandle.Close();
                this.subscriptionWaitHandle = null;
            }

            for (int i = 0; i < this.numEventsInBuffer; i++) {

                if (eventsBuffer[i] != IntPtr.Zero) {
                    NativeWrapper.EvtClose(eventsBuffer[i]);
                    eventsBuffer[i] = IntPtr.Zero;
                }
            }

            this.numEventsInBuffer = 0;
            
            if (handle != null && !handle.IsInvalid)
                handle.Dispose();
        }

        [System.Security.SecuritySafeCritical]
        internal void StartSubscribing() {

            if (this.isSubscribing)
                throw new InvalidOperationException();

            int flag = 0;
            if (bookmark != null)
                flag |= (int)UnsafeNativeMethods.EvtSubscribeFlags.EvtSubscribeStartAfterBookmark;
            else if (this.readExistingEvents)
                flag |= (int)UnsafeNativeMethods.EvtSubscribeFlags.EvtSubscribeStartAtOldestRecord;
            else
                flag |= (int)UnsafeNativeMethods.EvtSubscribeFlags.EvtSubscribeToFutureEvents;

            if (this.eventQuery.TolerateQueryErrors)
                flag |= (int)UnsafeNativeMethods.EvtSubscribeFlags.EvtSubscribeTolerateQueryErrors;

            EventLogPermissionHolder.GetEventLogPermission().Demand();

            this.callbackThreadId = -1;
            this.unregisterDoneHandle = new AutoResetEvent(false);
            this.subscriptionWaitHandle = new AutoResetEvent(false);

            EventLogHandle bookmarkHandle = EventLogRecord.GetBookmarkHandleFromBookmark(bookmark);

            using (bookmarkHandle) {

                handle = NativeWrapper.EvtSubscribe(this.eventQuery.Session.Handle,
                    this.subscriptionWaitHandle.SafeWaitHandle,
                    this.eventQuery.Path,
                    this.eventQuery.Query,
                    bookmarkHandle,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    flag);
            }

            this.isSubscribing = true;

            RequestEvents();

            this.registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(
                this.subscriptionWaitHandle,
                new WaitOrTimerCallback(SubscribedEventsAvailableCallback),
                null,
                -1,
                false);          
        }

        internal void SubscribedEventsAvailableCallback(object state, bool timedOut) {
            this.callbackThreadId = Thread.CurrentThread.ManagedThreadId;
            try {
                RequestEvents();
            }
            finally {
                this.callbackThreadId = -1;
            }
        }

        [System.Security.SecuritySafeCritical]
        private void RequestEvents() {

            EventLogPermissionHolder.GetEventLogPermission().Demand();

            this.asyncException = null;
            Debug.Assert(this. numEventsInBuffer == 0);

            bool results = false;

            do {

                if (!this.isSubscribing)
                    break;

                try {

                    results = NativeWrapper.EvtNext(this.handle, this.eventsBuffer.Length, this.eventsBuffer, 0, 0, ref this. numEventsInBuffer);

                    if (!results)
                        return;
                }
                catch (Exception e) {
                    this.asyncException = new EventLogException();
                    this.asyncException.Data.Add("RealException", e);                    
                }

                HandleEventsRequestCompletion();

            } while (results);
        }

        private void IssueCallback(EventRecordWrittenEventArgs eventArgs) {
            
            if (EventRecordWritten != null) {
                EventRecordWritten(this, eventArgs);
            }
        }

        // marked as SecurityCritical because allocates SafeHandles.
        [System.Security.SecurityCritical]
        private void HandleEventsRequestCompletion() {

            if (this.asyncException != null) {
                EventRecordWrittenEventArgs args = new EventRecordWrittenEventArgs(this.asyncException.Data["RealException"] as Exception);             
                IssueCallback(args);
            }

            for (int i = 0; i < this. numEventsInBuffer; i++) {
                if (!this.isSubscribing)
                    break;
                EventLogRecord record = new EventLogRecord(new EventLogHandle(this.eventsBuffer[i], true), this.eventQuery.Session, this.cachedMetadataInformation);
                EventRecordWrittenEventArgs args = new EventRecordWrittenEventArgs(record);
                this.eventsBuffer[i] = IntPtr.Zero;  // user is responsible for calling Dispose().
                IssueCallback(args);
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [System.Security.SecuritySafeCritical]
        protected virtual void Dispose(bool disposing) {

            if (disposing) {
                    StopSubscribing();
                return;
            }

            for (int i = 0; i < this.numEventsInBuffer; i++) {

                if (eventsBuffer[i] != IntPtr.Zero) {
                    NativeWrapper.EvtClose(eventsBuffer[i]);
                    eventsBuffer[i] = IntPtr.Zero;
                }
            }

            this.numEventsInBuffer = 0;
        }
    }
}
