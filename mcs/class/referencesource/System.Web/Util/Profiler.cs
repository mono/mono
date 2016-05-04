//------------------------------------------------------------------------------
// <copyright file="Profiler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Profiler.cs
 *
 * Copyright (c) 2000 Microsoft Corporation
 */

namespace System.Web.Util {

    using System;
    using System.Web;
    using System.Web.SessionState;
    using System.Web.UI;
    using System.Threading;
    using System.Collections;

    internal class Profiler {
        private int             _requestsToProfile;
        private Queue           _requests;
        private bool            _pageOutput;
        private bool            _isEnabled;
        private bool            _oldEnabled;
        private bool            _localOnly;
        private bool            _mostRecent;
        private TraceMode       _outputMode;


        internal Profiler() {
            _requestsToProfile = 10;
            _outputMode = TraceMode.SortByTime;
            _localOnly = true;
            _mostRecent = false;
            _requests = new Queue(_requestsToProfile);
        }

        internal bool IsEnabled {
            get { return _isEnabled;}
            set {
               _isEnabled = value;
               _oldEnabled = value;
            }
        }

        internal bool PageOutput {
            get {
                // calling HttpContext.Current is slow, but we'll only get there if _pageOutput is true.
                return (_pageOutput && !(_localOnly && !HttpContext.Current.Request.IsLocal));
            }
            set {
                _pageOutput = value;
            }
        }

        internal TraceMode OutputMode {
            get { return _outputMode;}
            set { _outputMode = value;}
        }

        internal bool LocalOnly {
            get { return _localOnly;}
            set { _localOnly = value; }
        }

        internal bool MostRecent {
            get { return _mostRecent; }
            set { _mostRecent = value; }
        }

        internal bool IsConfigEnabled {
            get { return _oldEnabled; }
        }

        internal int RequestsToProfile {
            get { return _requestsToProfile;}
            set {
                // VSWhidbey195368 Silently cap request limit at 10,000
                if (value > 10000) {
                    value = 10000;
                }
                _requestsToProfile = value;
            }
        }

        internal int RequestsRemaining {
            get { return _requestsToProfile - _requests.Count;}
        }

        internal void Reset() {
            // start profiling and clear the current log of requests
            _requests = new Queue(_requestsToProfile);

            if (_requestsToProfile != 0)
                _isEnabled = _oldEnabled;
            else
                _isEnabled = false;
        }

        internal void StartRequest(HttpContext context) {
            context.Trace.VerifyStart();
        }

        internal void EndRequest(HttpContext context) {
            context.Trace.EndRequest();

            // Don't add the trace data if we aren't enabled
            if (!IsEnabled) return;

            // grab trace data and add it to the list
            lock (_requests) {
                _requests.Enqueue(context.Trace.GetData());

                // If we are storing the most recent, we may need to kick out the first request
                if (MostRecent) {
                    if (_requests.Count > _requestsToProfile) _requests.Dequeue();
                }
            }

            // Turn off profiling if we are only tracking the first N requests and we hit the limit.
            if (!MostRecent && _requests.Count >= _requestsToProfile) EndProfiling();
        }

        internal void EndProfiling() {
            _isEnabled = false;
        }

        internal IList GetData() {
            return  _requests.ToArray();
        }

    }
}
