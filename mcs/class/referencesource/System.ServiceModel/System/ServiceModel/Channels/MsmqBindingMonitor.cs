//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Messaging;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.Threading;

    class MsmqBindingMonitor
    {
        static readonly TimeSpan DefaultUpdateInterval = TimeSpan.FromMinutes(10);

        CommunicationState currentState = CommunicationState.Created;
        List<MsmqBindingFilter> filters = new List<MsmqBindingFilter>();
        string host;
        int iteration;
        Dictionary<string, MatchState> knownPublicQueues = new Dictionary<string, MatchState>();
        Dictionary<string, MatchState> knownPrivateQueues = new Dictionary<string, MatchState>();
        object thisLock = new object();
        IOThreadTimer timer;
        TimeSpan updateInterval;
        ManualResetEvent firstRoundComplete;
        bool retryMatchedFilters;

        public MsmqBindingMonitor(string host)
            : this(host, DefaultUpdateInterval, false)
        {
        }

        public MsmqBindingMonitor(string host, TimeSpan updateInterval, bool retryMatchedFilters)
        {
            if (string.Compare(host, "localhost", StringComparison.OrdinalIgnoreCase) == 0)
            {
                this.host = ".";
            }
            else
            {
                this.host = host;
            }

            this.firstRoundComplete = new ManualResetEvent(false);

            this.updateInterval = updateInterval;
            this.retryMatchedFilters = retryMatchedFilters;
            this.iteration = 1;
        }

        public void AddFilter(MsmqBindingFilter filter)
        {
            lock (this.thisLock)
            {
                this.filters.Add(filter);

                // Now - see if we match any known queues
                MatchFilter(filter, knownPublicQueues.Values);
                MatchFilter(filter, knownPrivateQueues.Values);
            }
        }

        public bool ContainsFilter(MsmqBindingFilter filter)
        {
            lock (this.thisLock)
            {
                return this.filters.Contains(filter);
            }
        }

        public void Open()
        {
            lock (this.thisLock)
            {
                if (this.currentState != CommunicationState.Created)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CommunicationObjectCannotBeModified, this.GetType().ToString())));
                }

                this.currentState = CommunicationState.Opened;
                this.ScheduleRetryTimerIfNotSet();
            }
        }

        public void Close()
        {
            lock (this.thisLock)
            {
                if (this.currentState != CommunicationState.Opened)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CommunicationObjectCannotBeModified, this.GetType().ToString())));
                }

                this.currentState = CommunicationState.Closed;
                this.CancelRetryTimer();
            }
        }

        public void RemoveFilter(MsmqBindingFilter filter)
        {
            lock (this.thisLock)
            {
                this.filters.Remove(filter);

                RematchQueues(filter, knownPublicQueues.Values);
                RematchQueues(filter, knownPrivateQueues.Values);
            }
        }

        public void WaitForFirstRoundComplete()
        {
            this.firstRoundComplete.WaitOne();
        }

        void ScheduleRetryTimerIfNotSet()
        {
            if (this.timer == null)
            {
                this.timer = new IOThreadTimer(new Action<object>(OnTimer), null, false);
                // Schedule one enumeration to run immediately...
                this.timer.Set(0);
            }
        }

        void CancelRetryTimer()
        {
            if (this.timer != null)
            {
                this.timer.Cancel();
                this.timer = null;
            }
        }

        void MatchFilter(MsmqBindingFilter filter, IEnumerable<MatchState> queues)
        {
            // Run through all the queues - see if we are better than any existing matches...
            foreach (MatchState state in queues)
            {
                int matchLength = filter.Match(state.QueueName);
                if (matchLength > state.LastMatchLength)
                {
                    if (state.LastMatch != null)
                    {
                        state.LastMatch.MatchLost(this.host, state.QueueName, state.IsPrivate, state.CallbackState);
                    }

                    state.LastMatchLength = matchLength;
                    state.LastMatch = filter;

                    state.CallbackState = filter.MatchFound(this.host, state.QueueName, state.IsPrivate);
                }
            }
        }

        void RetryMatchFilters(IEnumerable<MatchState> queues)
        {
            // Run through all the queues and call match found on them
            foreach (MatchState state in queues)
            {
                if (state.LastMatch != null)
                {
                    state.CallbackState = state.LastMatch.MatchFound(this.host, state.QueueName, state.IsPrivate);
                }
            }
        }

        void MatchQueue(MatchState state)
        {
            MsmqBindingFilter bestMatch = state.LastMatch;
            int bestMatchLength = state.LastMatchLength;

            // look through all the filters for the largest match:
            foreach (MsmqBindingFilter filter in this.filters)
            {
                int matchLength = filter.Match(state.QueueName);
                if (matchLength > bestMatchLength)
                {
                    bestMatchLength = matchLength;
                    bestMatch = filter;
                }
            }

            if (bestMatch != state.LastMatch)
            {
                if (state.LastMatch != null)
                {
                    state.LastMatch.MatchLost(this.host, state.QueueName, state.IsPrivate, state.CallbackState);
                }

                state.LastMatchLength = bestMatchLength;
                state.LastMatch = bestMatch;

                state.CallbackState = bestMatch.MatchFound(this.host, state.QueueName, state.IsPrivate);
            }
        }

        // The demand is not added now (in 4.5), to avoid a breaking change. To be considered in the next version.
        /*
        // We demand full trust because this method calls into MessageQueue, which is defined in a non-APTCA assembly.
        // MSMQ is not enabled in partial trust, so this demand should not break customers.
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        */
        void OnTimer(object state)
        {
            try
            {
                if (this.currentState != CommunicationState.Opened)
                    return;

                lock (this.thisLock)
                {
                    if (this.retryMatchedFilters)
                    {
                        RetryMatchFilters(knownPublicQueues.Values);
                        RetryMatchFilters(knownPrivateQueues.Values);
                    }

                    bool scanNeeded = ((this.retryMatchedFilters == false) || 
                        (this.retryMatchedFilters && (this.iteration % 2) != 0));
                    if (scanNeeded)
                    {
                        MsmqDiagnostics.ScanStarted();

                        // enumerate the public queues first
                        try
                        {
                            MessageQueue[] queues = MessageQueue.GetPublicQueuesByMachine(this.host);
                            ProcessFoundQueues(queues, knownPublicQueues, false);
                        }
                        catch (MessageQueueException ex)
                        {
                            MsmqDiagnostics.CannotReadQueues(this.host, true, ex);
                        }

                        // enumerate the private queues next
                        try
                        {
                            MessageQueue[] queues = MessageQueue.GetPrivateQueuesByMachine(this.host);
                            ProcessFoundQueues(queues, knownPrivateQueues, true);
                        }
                        catch (MessageQueueException ex)
                        {
                            MsmqDiagnostics.CannotReadQueues(this.host, false, ex);
                        }

                        // Figure out if we lost any queues:
                        ProcessLostQueues(knownPublicQueues);
                        ProcessLostQueues(knownPrivateQueues);
                    }

                    this.iteration++;
                    this.timer.Set(this.updateInterval);
                }
            }
            finally
            {
                this.firstRoundComplete.Set();
            }
        }

        // The demand is not added now (in 4.5), to avoid a breaking change. To be considered in the next version.
        /*
        // We demand full trust because this method calls into MessageQueue, which is defined in a non-APTCA assembly.
        // MSMQ is not enabled in partial trust, so this demand should not break customers.
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        */
        void ProcessFoundQueues(MessageQueue[] queues, Dictionary<string, MatchState> knownQueues, bool isPrivate)
        {
            foreach (MessageQueue queue in queues)
            {
                MatchState state;
                string name = ExtractQueueName(queue.QueueName, isPrivate);

                if (!knownQueues.TryGetValue(name, out state))
                {
                    state = new MatchState(name, this.iteration, isPrivate);
                    knownQueues.Add(name, state);

                    MatchQueue(state);
                }
                else
                {
                    state.DiscoveryIteration = this.iteration;
                }
            }
        }

        string ExtractQueueName(string name, bool isPrivate)
        {
            // private queues start with "private$\\"
            if (isPrivate)
            {
                return name.Substring("private$\\".Length);
            }
            else
            {
                return name;
            }
        }

        void ProcessLostQueues(Dictionary<string, MatchState> knownQueues)
        {
            List<MatchState> lostQueues = new List<MatchState>();

            foreach (MatchState state in knownQueues.Values)
            {
                if (state.DiscoveryIteration != this.iteration)
                {
                    // we lost this queue!
                    lostQueues.Add(state);
                }
            }

            foreach (MatchState state in lostQueues)
            {
                knownQueues.Remove(state.QueueName);
                if (state.LastMatch != null)
                {
                    state.LastMatch.MatchLost(this.host, state.QueueName, state.IsPrivate, state.CallbackState);
                }
            }
        }

        void RematchQueues(MsmqBindingFilter filter, IEnumerable<MatchState> queues)
        {
            // if any queue currently matches "filter", re-match it against the other filters:
            foreach (MatchState state in queues)
            {
                if (state.LastMatch == filter)
                {
                    state.LastMatch.MatchLost(this.host, state.QueueName, state.IsPrivate, state.CallbackState);
                    state.LastMatch = null;
                    state.LastMatchLength = -1;
                    MatchQueue(state);
                }
            }
        }

        class MatchState
        {
            string name;
            int iteration;
            MsmqBindingFilter lastMatch;
            int lastMatchLength;
            object callbackState;
            bool isPrivate;

            public MatchState(string name, int iteration, bool isPrivate)
            {
                this.name = name;
                this.iteration = iteration;
                this.isPrivate = isPrivate;
                this.lastMatchLength = -1;
            }

            public object CallbackState
            {
                get { return this.callbackState; }
                set { this.callbackState = value; }
            }

            public int DiscoveryIteration
            {
                get { return this.iteration; }
                set { this.iteration = value; }
            }

            public bool IsPrivate
            {
                get { return this.isPrivate; }
            }

            public MsmqBindingFilter LastMatch
            {
                get { return this.lastMatch; }
                set { this.lastMatch = value; }
            }

            public int LastMatchLength
            {
                get { return this.lastMatchLength; }
                set { this.lastMatchLength = value; }
            }

            public string QueueName
            {
                get { return this.name; }
            }
        }
    }
}
