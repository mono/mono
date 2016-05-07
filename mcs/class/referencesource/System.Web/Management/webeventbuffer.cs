//------------------------------------------------------------------------------
// <copyright file="WebEventBuffer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Management {
    using System.Configuration;
    using System.Web.Configuration;
    using System.Configuration.Provider;
    using System.Collections.Specialized;
    using System.Collections;
    using System.Web.Util;
    using System.Web.Mail;
    using System.Globalization;
    using System.Xml;
    using System.Threading;
    using System.Web.Hosting;
    using System.Security.Permissions;

    public enum EventNotificationType
    {
        // regularly scheduled notification
        Regular,
        
        // urgent notification
        Urgent,
        
        // notification triggered by a user requested flush
        Flush,

        // Notification fired when buffer=false
        Unbuffered,
    }

    internal enum FlushCallReason {
        UrgentFlushThresholdExceeded,
        Timer,
        StaticFlush
    }

    public sealed class WebEventBufferFlushInfo {
        WebBaseEventCollection  _events;
        DateTime                _lastNotification;
        int                     _eventsDiscardedSinceLastNotification;
        int                     _eventsInBuffer;
        int                     _notificationSequence;
        EventNotificationType   _notificationType;
        
        internal WebEventBufferFlushInfo(  WebBaseEventCollection events,
                                            EventNotificationType notificationType,
                                            int notificationSequence,
                                            DateTime lastNotification,
                                            int eventsDiscardedSinceLastNotification,
                                            int eventsInBuffer) {
            _events = events;
            _notificationType = notificationType;
            _notificationSequence = notificationSequence;
            _lastNotification = lastNotification;
            _eventsDiscardedSinceLastNotification = eventsDiscardedSinceLastNotification;
            _eventsInBuffer = eventsInBuffer;
        }

        public WebBaseEventCollection  Events {
            get { return _events; }
        }
        
        public DateTime LastNotificationUtc {
            get { return _lastNotification; }
        }
        
        public int EventsDiscardedSinceLastNotification {
            get { return _eventsDiscardedSinceLastNotification; }
        }
        
        public int EventsInBuffer {
            get { return _eventsInBuffer; }
        }
        
        public int NotificationSequence {
            get { return _notificationSequence; }
        }
        
        public EventNotificationType NotificationType {
            get { return _notificationType; }
        }

    }
    
    internal delegate void WebEventBufferFlushCallback(WebEventBufferFlushInfo flushInfo);

    internal sealed class WebEventBuffer {

        static long Infinite = Int64.MaxValue;
        
        long        _burstWaitTimeMs = 2 * 1000;  

        BufferedWebEventProvider    _provider;
        
        long        _regularFlushIntervalMs;
        int         _urgentFlushThreshold;
        int         _maxBufferSize;
        int         _maxFlushSize;
        long        _urgentFlushIntervalMs;
        int         _maxBufferThreads;
        
        Queue       _buffer = null;
        Timer       _timer;
        DateTime    _lastFlushTime = DateTime.MinValue;
        DateTime    _lastScheduledFlushTime = DateTime.MinValue;
        DateTime    _lastAdd = DateTime.MinValue;
        DateTime    _startTime = DateTime.MinValue;
        bool        _urgentFlushScheduled;
        int         _discardedSinceLastFlush = 0;
        int         _threadsInFlush = 0;
        int         _notificationSequence = 0;
        bool        _regularTimeoutUsed;

#if DBG
        DateTime    _nextFlush = DateTime.MinValue;
        DateTime    _lastRegularFlush = DateTime.MinValue;
        DateTime    _lastUrgentFlush = DateTime.MinValue;
        int         _totalAdded = 0;
        int         _totalFlushed = 0;
        int         _totalAbandoned = 0;
#endif        

        WebEventBufferFlushCallback _flushCallback;

        internal WebEventBuffer(BufferedWebEventProvider provider, string bufferMode,
                        WebEventBufferFlushCallback callback) {
            Debug.Assert(callback != null, "callback != null");

            _provider = provider;
            
            HealthMonitoringSection section = RuntimeConfig.GetAppLKGConfig().HealthMonitoring;

            BufferModesCollection bufferModes = section.BufferModes;

            BufferModeSettings bufferModeInfo = bufferModes[bufferMode];
            if (bufferModeInfo == null) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Health_mon_buffer_mode_not_found, bufferMode));
            }

            if (bufferModeInfo.RegularFlushInterval == TimeSpan.MaxValue) {
                _regularFlushIntervalMs = Infinite;
            }
            else {
                try {
                    _regularFlushIntervalMs = (long)bufferModeInfo.RegularFlushInterval.TotalMilliseconds;
                }
                catch (OverflowException) {
                    _regularFlushIntervalMs = Infinite;
                }
            }
            
            if (bufferModeInfo.UrgentFlushInterval == TimeSpan.MaxValue) {
                _urgentFlushIntervalMs = Infinite;
            }
            else {
                try {
                    _urgentFlushIntervalMs = (long)bufferModeInfo.UrgentFlushInterval.TotalMilliseconds;
                }
                catch (OverflowException) {
                    _urgentFlushIntervalMs = Infinite;
                }
            }

            _urgentFlushThreshold = bufferModeInfo.UrgentFlushThreshold;
            _maxBufferSize = bufferModeInfo.MaxBufferSize;
            _maxFlushSize = bufferModeInfo.MaxFlushSize;
            _maxBufferThreads = bufferModeInfo.MaxBufferThreads;

            _burstWaitTimeMs = Math.Min(_burstWaitTimeMs, _urgentFlushIntervalMs);
            
            _flushCallback = callback;

            _buffer = new Queue();

            if (_regularFlushIntervalMs != Infinite) {
                _startTime = DateTime.UtcNow;
                _regularTimeoutUsed = true;
                _urgentFlushScheduled = false;
                SetTimer(GetNextRegularFlushDueTimeInMs());
            }

            Debug.Trace("WebEventBuffer",   
                        "\n_regularFlushIntervalMs=" + _regularFlushIntervalMs +
                        "\n_urgentFlushThreshold=" + _urgentFlushThreshold +
                        "\n_maxBufferSize=" + _maxBufferSize +
                        "\n_maxFlushSize=" + _maxFlushSize +
                        "\n_urgentFlushIntervalMs=" + _urgentFlushIntervalMs);
        }

        void FlushTimerCallback(object state) {
            Flush(_maxFlushSize, FlushCallReason.Timer);
        }

        //
        // If we're in notification mode, meaning urgentFlushThreshold="1", we'll flush
        // as soon as there's an event in the buffer.
        // 
        // For example, if bufferMode == "notification", we have this setting:
        //  <add name="Notification" maxBufferSize="300" maxFlushSize="20"
        //    urgentFlushThreshold="1" regularFlushInterval="Infinite" 
        //    urgentFlushInterval="00:01:00" maxBufferThreads="1" />
        //
        // The ideal situation is that we have events coming in regularly,
        // and we flush (max 20 events at a time), wait for _urgentFlushIntervalMs (1 minute), 
        // then flush the buffer, then wait 1 minute, then flush, and so on and on.
        //
        // However, there is a scenario where there's been no event coming in, and suddenly  
        // a burst of events (e.g. 20) arrive. If we flush immediately when the 1st event comes in, 
        // we then have to wait for 1 minute before we can flush the remaining 19 events.
        //
        // To solve this problem, we demand that if we're in notification mode, and
        // we just added an event to an empty buffer, then we may anticipate a burst
        // by waiting _burstWaitTimeMs amount of time (2s).
        //
        // But how long does a buffer needs to be empty before we consider
        // waiting for a burst?  We cannot come up with a good formula, and thus
        // pick this:
        //      ((now - _lastAdd).TotalMilliseconds) >= _urgentFlushIntervalMs
        // 
        bool AnticipateBurst(DateTime now) {
            // Please note this is called while we're within the lock held in AddEvent.
            return _urgentFlushThreshold == 1 &&    // we're in notification mode
                    _buffer.Count == 1 &&           // we just added an event to an empty buffer
                    ((now - _lastAdd).TotalMilliseconds) >= _urgentFlushIntervalMs;
        }

        long GetNextRegularFlushDueTimeInMs() {
            long   nextRegularFlushFromStartTime;
            long   nowFromStartTime;
            long   regularFlushIntervalms = _regularFlushIntervalMs;

            // Need to calculate in milliseconds in order to avoid time shift due to round-down
            if (_regularFlushIntervalMs == Infinite) {
                return Infinite;
            }

            DateTime    now = DateTime.UtcNow;
            nowFromStartTime = (long)((now - _startTime).TotalMilliseconds);

            // For some unknown reason the Timer may fire prematurely (usually less than 50ms).  This will bring
            // us into a situation where the timer fired just tens of milliseconds before the originally planned 
            // fire time, and this method will return a due time == tens of milliseconds.
            // To workaround this problem, I added 499 ms when doing the calculation to compensate for a
            // premature firing.
            nextRegularFlushFromStartTime = ((nowFromStartTime + regularFlushIntervalms + 499) / regularFlushIntervalms) * regularFlushIntervalms;

            Debug.Assert(nextRegularFlushFromStartTime >= nowFromStartTime);

            return nextRegularFlushFromStartTime - nowFromStartTime;
        }

        void SetTimer(long waitTimeMs) {
            if (_timer == null) {
                _timer = new System.Threading.Timer(new TimerCallback(this.FlushTimerCallback),
                                                null, waitTimeMs, Timeout.Infinite);
            }
            else {
                _timer.Change(waitTimeMs, Timeout.Infinite);
            }

#if DBG
            _nextFlush = DateTime.UtcNow.AddMilliseconds(waitTimeMs);
#endif            
        }

        // This method can be called by the timer, or by AddEvent.
        //
        // Basic design:
        // - We have one timer, and one buffer.
        // - We flush periodically every _regularFlushIntervalMs ms
        // - But if # of items in buffer has reached _urgentFlushThreshold, we will flush more frequently,
        //   but at most once every _urgentFlushIntervalMs ms.  However, these urgent flushes will not
        //   prevent the regular flush from happening.
        // - We never flush synchronously, meaning if we're called by AddEvent and decide to flush
        //   because we've reached the _urgentFlushThreshold, we will still use the timer thread
        //   to flush the buffer.
        // - At any point only a maximum of _maxBufferThreads threads can be flushing.  If exceeded,
        //   we will delay a flush.
        //
        //

        // For example, let's say we have this setting:
        // "1 minute urgentFlushInterval and 5 minute regularFlushInterval"
        //
        // Assume regular flush timer starts at 10:00am.  It means regular 
        // flush will happen at 10:05am, 10:10am, 10:15am, and so on, 
        // regardless of when urgent flush happens.  
        // 
        // An "urgent flush" happens whenever urgentFlushThreshold is reached.
        // However, when we schedule an "urgent flush", we ensure that the time
        // between an urgent flush and the last flush (no matter it's urgent or
        // regular) will be at least urgentFlushInterval.
        //
        // One interesting case here.  Assume at 10:49:30 we had an urgent 
        // flush, but the # of events left is still above urgentFlushThreshold.
        // You may think we'll schedule the next urgent flush at 10:50:30
        // (urgentFlushInterval == 1 min).  However, because we know we will 
        // have a regular flush at 10:50:00, we won't schedule the next urgent
        // flush.  Instead, during the regular flush at 10:50:00 happens, we'll
        // check if there're still too many events; and if so, we will schedule
        // the next urgent flush at 10:51:00.
        //
        internal void Flush(int max, FlushCallReason reason) {
            WebBaseEvent[]  events = null;
            DateTime    nowUtc = DateTime.UtcNow;
            long        waitTime = 0;
            DateTime    lastFlushTime = DateTime.MaxValue;
            int         discardedSinceLastFlush = -1;
            int         eventsInBuffer = -1;
            int         toFlush = 0;
            EventNotificationType   notificationType = EventNotificationType.Regular;

            // By default, this call will flush, but will not schedule the next flush.
            bool        flushThisTime = true;
            bool        scheduleNextFlush = false;
            bool        nextFlushIsUrgent = false;

            lock(_buffer) {
                Debug.Assert(max > 0, "max > 0");

                if (_buffer.Count == 0) {
                    // We have nothing in the buffer.  Don't flush this time.
                    Debug.Trace("WebEventBufferExtended", "Flush: buffer is empty, don't flush");
                    flushThisTime = false;
                }

                switch (reason) {
                case FlushCallReason.StaticFlush:
                    // It means somebody calls provider.Flush()
                    break;

                case FlushCallReason.Timer:
                    // It's a callback from a timer.  We will schedule the next regular flush if needed.
                    
                    if (_regularFlushIntervalMs != Infinite) {
                        scheduleNextFlush = true;
                        waitTime = GetNextRegularFlushDueTimeInMs();
                    }
                    break;

                case FlushCallReason.UrgentFlushThresholdExceeded:
                    // It means this method is called by AddEvent because the urgent flush threshold is reached.
                    
                    // If an urgent flush has already been scheduled by someone else, we don't need to duplicate the
                    // effort.  Just return.
                    if (_urgentFlushScheduled) {
                        return;
                    }

                    // Flush triggered by AddEvent isn't synchronous, so we won't flush this time, but will 
                    // schedule an urgent flush instead.
                    flushThisTime = false;      
                    scheduleNextFlush = true;
                    nextFlushIsUrgent = true;         

                    // Calculate how long we have to wait when scheduling the flush
                    if (AnticipateBurst(nowUtc)) {
                        Debug.Trace("WebEventBuffer", "Flush: Called by AddEvent.  Waiting for burst");
                        waitTime = _burstWaitTimeMs;
                    }
                    else {
                        Debug.Trace("WebEventBuffer", "Flush: Called by AddEvent.  Schedule an immediate flush");
                        waitTime = 0;
                    }
                    
                    // Have to wait longer because of _urgentFlushIntervalMs
                    long    msSinceLastScheduledFlush = (long)(nowUtc - _lastScheduledFlushTime).TotalMilliseconds;
                    if (msSinceLastScheduledFlush + waitTime < _urgentFlushIntervalMs ) {
                        
                        Debug.Trace("WebEventBuffer", "Flush: Called by AddEvent.  Have to wait longer because of _urgentFlushIntervalMs.");
                        waitTime = _urgentFlushIntervalMs - msSinceLastScheduledFlush;
                    }
                    
                    Debug.Trace("WebEventBuffer", "Wait time=" + waitTime +
                        "; nowUtc=" + PrintTime(nowUtc) +
                        "; _lastScheduledFlushTime=" + PrintTime(_lastScheduledFlushTime) + 
                        "; _urgentFlushIntervalMs=" + _urgentFlushIntervalMs);
                    
                    break;
                }
                
                Debug.Trace("WebEventBuffer", "Flush called: max=" + max + 
                    "; reason=" + reason);
                    
                if (flushThisTime) {
                    // Check if we've exceeded the # of flushing threads.  If so,
                    // don't flush this time.
                    
                    if (_threadsInFlush >= _maxBufferThreads) {
                        // Won't set flushThisTime to false because we depend on
                        // the logic inside the next "if" block to schedule the
                        // next urgent flush as needed.
                        toFlush = 0;
                    }
                    else {
                        toFlush = Math.Min(_buffer.Count, max);
                    }
                }
                
#if DBG
                DebugUpdateStats(flushThisTime, nowUtc, toFlush, reason);
#endif

                if (flushThisTime) {
                    Debug.Assert(reason != FlushCallReason.UrgentFlushThresholdExceeded, "reason != FlushCallReason.UrgentFlushThresholdExceeded");

                    if (toFlush > 0) {
                        // Move the to-be-flushed events to an array                
                        events = new WebBaseEvent[toFlush];

                        for (int i = 0; i < toFlush; i++) {
                            events[i] = (WebBaseEvent)_buffer.Dequeue();
                        }

                        lastFlushTime = _lastFlushTime;

                        // Update _lastFlushTime and _lastScheduledFlushTime.
                        // These information are used when Flush is called the next time.
                        _lastFlushTime = nowUtc;
                        if (reason == FlushCallReason.Timer) {
                            _lastScheduledFlushTime = nowUtc;
                        }

                        discardedSinceLastFlush = _discardedSinceLastFlush;
                        _discardedSinceLastFlush = 0;

                        if (reason == FlushCallReason.StaticFlush) {
                            notificationType = EventNotificationType.Flush;
                        }
                        else {
                            Debug.Assert(!(!_regularTimeoutUsed && !_urgentFlushScheduled),
                                "It's impossible to have a non-regular flush and yet the flush isn't urgent");

                            notificationType = _regularTimeoutUsed ? 
                                               EventNotificationType.Regular :
                                               EventNotificationType.Urgent;
                        }
                    }

                    eventsInBuffer = _buffer.Count;

                    // If we still have at least _urgentFlushThreshold left, set timer
                    // to flush asap.
                    if (eventsInBuffer >= _urgentFlushThreshold) {
                        Debug.Trace("WebEventBuffer", "Flush: going to flush " + toFlush + " events, but still have at least _urgentFlushThreshold left. Schedule a flush");
                        scheduleNextFlush = true;
                        nextFlushIsUrgent = true;
                        waitTime = _urgentFlushIntervalMs;
                    }
                    else {
                        Debug.Trace("WebEventBuffer", "Flush: going to flush " + toFlush + " events");
                    }
                }

                // We are done moving the flushed events to the 'events' array.  
                // Now schedule the next flush if needed.

                _urgentFlushScheduled = false;
                
                if (scheduleNextFlush) {
                    if (nextFlushIsUrgent) {
                        long nextRegular = GetNextRegularFlushDueTimeInMs();

                        // If next regular flush is closer than next urgent flush,
                        // use regular flush instead.
                        if (nextRegular < waitTime) {
                            Debug.Trace("WebEventBuffer", "Switch to use regular timeout");
                            waitTime = nextRegular;
                            _regularTimeoutUsed = true;
                        }
                        else {
                            _regularTimeoutUsed = false;
                        }
                    }
                    else {
                        _regularTimeoutUsed = true;
                    }
                    
                    SetTimer(waitTime);
                    _urgentFlushScheduled = nextFlushIsUrgent;
#if DBG
                    Debug.Trace("WebEventBuffer", "Flush: Registered for a flush.  Waittime = " + waitTime + "ms" +
                        "; _nextFlush=" + PrintTime(_nextFlush) +
                        "; _urgentFlushScheduled=" + _urgentFlushScheduled);
#endif

                }

                // Cleanup.  If we are called by a timer callback, but we haven't scheduled for the next
                // one (can only happen if _regularFlushIntervalMs == Infinite), we should dispose the timer
                if (reason == FlushCallReason.Timer && !scheduleNextFlush) {
                    Debug.Trace("WebEventBuffer", "Flush: Disposing the timer");
                    Debug.Assert(_regularFlushIntervalMs == Infinite, "We can dispose the timer only if _regularFlushIntervalMs == Infinite");
                    ((IDisposable)_timer).Dispose();
                    _timer = null;
                    _urgentFlushScheduled = false;
                }

                // We want to increment the thread count within the lock to ensure we don't let too many threads in
                if (events != null) {
                    Interlocked.Increment(ref _threadsInFlush);
                }
            } // Release lock

            // Now call the providers to flush the events
            if (events != null) {
                Debug.Assert(lastFlushTime != DateTime.MaxValue, "lastFlushTime != DateTime.MaxValue");
                Debug.Assert(discardedSinceLastFlush != -1, "discardedSinceLastFlush != -1");
                Debug.Assert(eventsInBuffer != -1, "eventsInBuffer != -1");

                Debug.Trace("WebEventBufferSummary", "_threadsInFlush=" + _threadsInFlush);

                using (new ApplicationImpersonationContext()) {
                    try {
                        WebEventBufferFlushInfo flushInfo = new WebEventBufferFlushInfo(
                                                                new WebBaseEventCollection(events),
                                                                notificationType,
                                                                Interlocked.Increment(ref _notificationSequence),
                                                                lastFlushTime,
                                                                discardedSinceLastFlush,
                                                                eventsInBuffer);

                        _flushCallback(flushInfo);
                    }
                    catch (Exception e) {
                        try {
                            _provider.LogException(e);
                        }
                        catch {
                            // Ignore all errors
                        }
                    }
#pragma warning disable 1058
                    catch { // non compliant exceptions are caught and logged as Unknown
                        try {
                            _provider.LogException(new Exception(SR.GetString(SR.Provider_Error)));
                        }
                        catch {
                            // Ignore all errors
                        }
                    }
#pragma warning restore 1058
                }

                Interlocked.Decrement(ref _threadsInFlush);
            }
        }
        
        internal void AddEvent(WebBaseEvent webEvent) {
            lock(_buffer) {
#if DBG
                _totalAdded++;
#endif
                // If we have filled up the buffer, remove items using FIFO order.
                if (_buffer.Count == _maxBufferSize) {
                    Debug.Trace("WebEventBuffer", "Buffer is full.  Need to remove one from the tail");
                    _buffer.Dequeue();
                    _discardedSinceLastFlush++;
#if DBG
                    _totalAbandoned++;
#endif
                }

                _buffer.Enqueue(webEvent);
                
                // If we have at least _urgentFlushThreshold, flush.  Please note the flush is async.
                if (_buffer.Count >= _urgentFlushThreshold) {
                    Flush(_maxFlushSize, FlushCallReason.UrgentFlushThresholdExceeded);
                }
                
                // Note that Flush uses _lastAdd, which is the time an event (not including this one) 
                // was last added.  That's why we call it after calling Flush.
                _lastAdd = DateTime.UtcNow;
            }   // Release the lock
        }

        internal void Shutdown() {
            if (_timer != null) {
                _timer.Dispose();
                _timer = null;
            }
        }

        string PrintTime(DateTime t) {
            return t.ToString("T", DateTimeFormatInfo.InvariantInfo) + "." + t.Millisecond.ToString("d03", CultureInfo.InvariantCulture);
        }


#if DBG
        void DebugUpdateStats(bool flushThisTime, DateTime now, int toFlush, FlushCallReason reason) {
            Debug.Assert(_totalAdded == _totalAbandoned + _totalFlushed + _buffer.Count, 
                    "_totalAdded == _totalAbandoned + _totalFlushed + _buffer.Count");
            
            _totalFlushed += toFlush;
    
            if (reason != FlushCallReason.Timer) {
                return;
            }
            
            Debug.Trace("WebEventBufferSummary", 
                "_Added=" + _totalAdded + "; deleted=" + _totalAbandoned +
                "; Flushed=" + _totalFlushed + "; buffer=" + _buffer.Count +
                "; toFlush=" + toFlush +
                "; lastFlush=" + PrintTime(_lastRegularFlush) +
                "; lastUrgentFlush=" + PrintTime(_lastUrgentFlush) +
                "; GetRegFlushDueTime=" + GetNextRegularFlushDueTimeInMs() +
                "; toFlush=" + toFlush +
                "; now=" + PrintTime(now));

            if (!_regularTimeoutUsed) {
                if (!flushThisTime) {
                    return;
                }
                
                Debug.Assert((now - _lastUrgentFlush).TotalMilliseconds + 499 > _urgentFlushIntervalMs, 
                    "(now - _lastUrgentFlush).TotalMilliseconds + 499 > _urgentFlushIntervalMs" +
                    "\n_lastUrgentFlush=" + PrintTime(_lastUrgentFlush) +
                    "\nnow=" + PrintTime(now) +
                    "\n_urgentFlushIntervalMs=" + _urgentFlushIntervalMs);
                
                _lastUrgentFlush = now;
            }
            else {
                /*
                // It's a regular callback
                if (_lastRegularFlush != DateTime.MinValue) {
                    Debug.Assert(Math.Abs((now - _lastRegularFlush).TotalMilliseconds - _regularFlushIntervalMs) < 2000, 
                        "Math.Abs((now - _lastRegularFlush).TotalMilliseconds - _regularFlushIntervalMs) < 2000" +
                        "\n_lastRegularFlush=" + PrintTime(_lastRegularFlush) +
                        "\nnow=" + PrintTime(now) +
                        "\n_regularFlushIntervalMs=" + _regularFlushIntervalMs);
                }
                */
                
                _lastRegularFlush = now;
            }
        }

#endif
    }
}

