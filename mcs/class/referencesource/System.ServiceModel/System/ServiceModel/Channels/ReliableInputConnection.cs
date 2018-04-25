//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Runtime;

    sealed class ReliableInputConnection
    {
        bool isLastKnown = false;
        bool isSequenceClosed = false;
        Int64 last = 0;
        SequenceRangeCollection ranges = SequenceRangeCollection.Empty;
        ReliableMessagingVersion reliableMessagingVersion;
        InterruptibleWaitObject shutdownWaitObject = new InterruptibleWaitObject(false);
        bool terminated = false;
        InterruptibleWaitObject terminateWaitObject = new InterruptibleWaitObject(false, false);

        public ReliableInputConnection()
        {
        }

        public bool AllAdded
        {
            get
            {
                return (this.ranges.Count == 1
                    && this.ranges[0].Lower == 1
                    && this.ranges[0].Upper == this.last)
                    || this.isLastKnown;
            }
        }

        public bool IsLastKnown
        {
            get
            {
                return this.last != 0 || this.isLastKnown;
            }
        }

        public bool IsSequenceClosed
        {
            get
            {
                return this.isSequenceClosed;
            }
        }

        public Int64 Last
        {
            get
            {
                return this.last;
            }
        }

        public SequenceRangeCollection Ranges
        {
            get
            {
                return this.ranges;
            }
        }

        public ReliableMessagingVersion ReliableMessagingVersion
        {
            set
            {
                this.reliableMessagingVersion = value;
            }
        }

        public void Abort(ChannelBase channel)
        {
            this.shutdownWaitObject.Abort(channel);
            this.terminateWaitObject.Abort(channel);
        }

        public bool CanMerge(Int64 sequenceNumber)
        {
            return ReliableInputConnection.CanMerge(sequenceNumber, this.ranges);
        }

        // Returns true if merging the number will not increase the number of ranges past MaxSequenceRanges.
        public static bool CanMerge(Int64 sequenceNumber, SequenceRangeCollection ranges)
        {
            if (ranges.Count < ReliableMessagingConstants.MaxSequenceRanges)
            {
                return true;
            }

            ranges = ranges.MergeWith(sequenceNumber);
            return ranges.Count <= ReliableMessagingConstants.MaxSequenceRanges;
        }

        public void Fault(ChannelBase channel)
        {
            this.shutdownWaitObject.Fault(channel);
            this.terminateWaitObject.Fault(channel);
        }

        public bool IsValid(Int64 sequenceNumber, bool isLast)
        {
            if (this.reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                if (isLast)
                {
                    if (this.last == 0)
                    {
                        if (this.ranges.Count > 0)
                        {
                            return sequenceNumber > this.ranges[this.ranges.Count - 1].Upper;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return sequenceNumber == this.last;
                    }
                }
                else if (this.last > 0)
                {
                    return sequenceNumber < this.last;
                }
            }
            else
            {
                if (this.isLastKnown)
                {
                    return this.ranges.Contains(sequenceNumber);
                }
            }

            return true;
        }

        public void Merge(Int64 sequenceNumber, bool isLast)
        {
            this.ranges = this.ranges.MergeWith(sequenceNumber);

            if (isLast)
                this.last = sequenceNumber;

            if (this.AllAdded)
                this.shutdownWaitObject.Set();
        }

        public bool SetCloseSequenceLast(Int64 last)
        {
            WsrmUtilities.AssertWsrm11(this.reliableMessagingVersion);
            bool validLast;

            if ((last < 1) || (this.ranges.Count == 0))
            {
                validLast = true;
            }
            else
            {
                validLast = last >= this.ranges[this.ranges.Count - 1].Upper;
            }

            if (validLast)
            {
                this.isSequenceClosed = true;
                this.SetLast(last);
            }

            return validLast;
        }

        void SetLast(Int64 last)
        {
            if (this.isLastKnown)
            {
                throw Fx.AssertAndThrow("Last can only be set once.");
            }

            this.last = last;
            this.isLastKnown = true;
            this.shutdownWaitObject.Set();
        }

        // Two error cases:
        // (1) The sequence contains holes.
        // (2) TerminateSequence.LastMsgNumber < last received message number.
        // In both cases the channel should be faulted. In case (2) the channel should send a fault.
        public bool SetTerminateSequenceLast(Int64 last, out bool isLastLargeEnough)
        {
            WsrmUtilities.AssertWsrm11(this.reliableMessagingVersion);
            isLastLargeEnough = true;

            // unspecified last
            if (last < 1)
            {
                return false;
            }

            int rangeCount = this.ranges.Count;
            Int64 lastReceived = (rangeCount > 0) ? this.ranges[rangeCount - 1].Upper : 0;

            // last is too small to be valid
            if (last < lastReceived)
            {
                isLastLargeEnough = false;
                return false;
            }

            // there is a hole in the sequence
            if ((rangeCount > 1) || (last > lastReceived))
            {
                return false;
            }

            this.SetLast(last);
            return true;
        }

        public bool Terminate()
        {
            if ((this.reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
                || this.isSequenceClosed)
            {
                if (!this.terminated && this.AllAdded)
                {
                    this.terminateWaitObject.Set();
                    this.terminated = true;
                }

                return this.terminated;
            }

            return this.isLastKnown;
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            OperationWithTimeoutBeginCallback[] beginCallbacks
                = new OperationWithTimeoutBeginCallback[] { shutdownWaitObject.BeginWait, terminateWaitObject.BeginWait };
            OperationEndCallback[] endCallbacks
                = new OperationEndCallback[] { shutdownWaitObject.EndWait, terminateWaitObject.EndWait };

            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout, beginCallbacks, endCallbacks, callback, state);
        }

        public void Close(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.shutdownWaitObject.Wait(timeoutHelper.RemainingTime());
            this.terminateWaitObject.Wait(timeoutHelper.RemainingTime());
        }

        public void EndClose(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
        }
    }
}
