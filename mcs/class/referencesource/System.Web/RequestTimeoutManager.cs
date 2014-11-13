//------------------------------------------------------------------------------
// <copyright file="RequestTimeoutManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Request timeout manager -- implements the request timeout mechanism
 */
namespace System.Web {
    using System.Threading;
    using System.Collections;
    using System.Web.Util;

    internal class RequestTimeoutManager {
        private int                 _requestCount;
        private DoubleLinkList[]    _lists;           // partitioned to avoid contention
        private int                 _currentList;
        private int                 _inProgressLock;  // only 1 thread can be cancelling
        private readonly TimeSpan   _timerPeriod = new TimeSpan(0, 0, 15); // 15 second init precision
        private Timer               _timer;

        internal RequestTimeoutManager() {
            // initialize request lists

            _requestCount = 0;

            _lists = new DoubleLinkList[13];
            for (int i = 0; i < _lists.Length; i++)
                _lists[i] = new DoubleLinkList();
            _currentList = 0;

            // init lock

            _inProgressLock = 0;

            // create the timer

#if DBG
            if (!Debug.IsTagPresent("Timer") || Debug.IsTagEnabled("Timer"))
#endif
            {
                _timer = new Timer(new TimerCallback(this.TimerCompletionCallback), null, _timerPeriod, _timerPeriod);
            }

        }

        internal void Stop() {
            // stop the timer

            if (_timer != null) {
                ((IDisposable)_timer).Dispose();
                _timer = null;
            }

            while (_inProgressLock != 0)
                Thread.Sleep(100);

            // cancel all cancelable requests

            if (_requestCount > 0)
                CancelTimedOutRequests(DateTime.UtcNow.AddYears(1)); // future date
        }

        private void TimerCompletionCallback(Object state) {
            if (_requestCount > 0)
                CancelTimedOutRequests(DateTime.UtcNow);
        }

        private void CancelTimedOutRequests(DateTime now) {

            // only one thread can be doing it

            if (Interlocked.CompareExchange(ref _inProgressLock, 1, 0) != 0)
                return;

            // collect them all into a separate list with minimal locking

            ArrayList entries = new ArrayList(_requestCount); // size can change
            DoubleLinkListEnumerator en;

            for (int i = 0; i < _lists.Length; i++) {
                lock (_lists[i]) {
                    en = _lists[i].GetEnumerator();

                    while (en.MoveNext())
                        entries.Add(en.GetDoubleLink());

                    en = null;
                }
            }

            // walk through the collected list to timeout what's needed

            int n = entries.Count;

            for (int i = 0; i < n; i++)
                ((RequestTimeoutEntry)entries[i]).TimeoutIfNeeded(now);

            // this thread is done -- unlock

            Interlocked.Exchange(ref _inProgressLock, 0);
        }

        internal void Add(HttpContext context) {
            if (context.TimeoutLink != null) {
                ((RequestTimeoutEntry)context.TimeoutLink).IncrementCount();
                return;
            }
           
            // create new entry

            RequestTimeoutEntry entry = new RequestTimeoutEntry(context);

            // add it to the list

            int i = _currentList++;
            if (i >= _lists.Length) {
                i = 0;
                _currentList = 0;
            }

            entry.AddToList(_lists[i]);
            Interlocked.Increment(ref _requestCount);

            // update HttpContext
            context.TimeoutLink = entry;
        }

        internal void Remove(HttpContext context) {
            RequestTimeoutEntry entry = (RequestTimeoutEntry)context.TimeoutLink;

            // remove from the list
            if (entry != null) {
                if( entry.DecrementCount() == 0 ) {
                    entry.RemoveFromList();
                    Interlocked.Decrement(ref _requestCount);
                } else {
                    return;
                }
            }

            // update HttpContext
            context.TimeoutLink = null;
        }

        private class RequestTimeoutEntry : DoubleLink {
            private  HttpContext    _context;   // the request
            private  DoubleLinkList _list;
            private int _count;

            internal RequestTimeoutEntry(HttpContext context) {
                _context = context;
                _count = 1;
            }

            internal void AddToList(DoubleLinkList list) {
                lock(list) {
                    list.InsertTail(this);
                    _list = list;
                }
            }

            internal void RemoveFromList() {
                if (_list != null) {
                    lock(_list) {
                        Remove();
                        _list = null;
                    }
                }
            }

            internal void TimeoutIfNeeded(DateTime now) {
                Thread thread = _context.MustTimeout(now);
                if (thread != null) {
                    RemoveFromList();
                    thread.Abort(new HttpApplication.CancelModuleException(true));
                }
            }

            internal void IncrementCount() {
                Interlocked.Increment( ref _count );
            }

            internal int DecrementCount() {
                return Interlocked.Decrement( ref _count );
            }
        }
    }

}
