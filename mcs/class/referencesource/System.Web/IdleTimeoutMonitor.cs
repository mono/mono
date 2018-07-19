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
    using System.Web.Hosting;
    using System.Web.Util;

    internal class IdleTimeoutMonitor {

        private TimeSpan _idleTimeout;  // the timeout value
        private DateTime _lastEvent;    // idle since this time
        private Timer _timer;
        private readonly TimeSpan _timerPeriod = new TimeSpan(0, 0, 30); // 30 secs

        internal IdleTimeoutMonitor(TimeSpan timeout) {
            _idleTimeout = timeout;
            _timer = new Timer(new TimerCallback(this.TimerCompletionCallback), null, _timerPeriod, _timerPeriod);
            _lastEvent = DateTime.UtcNow;
        }

        internal void Stop() {
            // stop the timer
            if (_timer != null) {
                lock (this) {
                    if (_timer != null) {
                        ((IDisposable)_timer).Dispose();
                        _timer = null;
                    }
                }
            }
        }

        internal DateTime LastEvent { // thread-safe property
            get {
                DateTime t;
                lock (this) { t = _lastEvent; }
                return t;
            }

            set {
                lock (this) { _lastEvent = value; }
            }
        }

        private void TimerCompletionCallback(Object state) {
            // user idle timer to trim the free list of app instanced
            HttpApplicationFactory.TrimApplicationInstances();

            // no idle timeout
            if (_idleTimeout == TimeSpan.MaxValue)
                return;

            // don't do idle timeout if already shutting down
            if (HostingEnvironment.ShutdownInitiated)
                return;

            // check if there are active requests
            if (HostingEnvironment.BusyCount != 0)
                return;

            // check if enough time passed
            if (DateTime.UtcNow <= LastEvent.Add(_idleTimeout))
                return;

            // check if debugger is attached
            if (System.Diagnostics.Debugger.IsAttached)
                return;

            // shutdown
            HttpRuntime.SetShutdownReason(ApplicationShutdownReason.IdleTimeout, 
                                          SR.GetString(SR.Hosting_Env_IdleTimeout));
            HostingEnvironment.InitiateShutdownWithoutDemand();
        }
    }
}
