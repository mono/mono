//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Tracking
{
    using System;
    using System.Runtime;

    public abstract class TrackingParticipant
    {
        protected TrackingParticipant()
        {
        }

        public virtual TrackingProfile TrackingProfile
        {
            get;
            set;
        }

        [Fx.Tag.InheritThrows(From = "Track", FromDeclaringType = typeof(TrackingParticipant))]
        protected internal virtual IAsyncResult BeginTrack(TrackingRecord record, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new TrackAsyncResult(this, record, timeout, callback, state);
        }

        [Fx.Tag.InheritThrows(From = "Track", FromDeclaringType = typeof(TrackingParticipant))]
        protected internal virtual void EndTrack(IAsyncResult result)
        {
            TrackAsyncResult.End(result);
        }

        [Fx.Tag.Throws(typeof(Exception), "extensibility point")]
        [Fx.Tag.Throws.Timeout("Tracking data could not be saved before the timeout")]
        protected internal abstract void Track(TrackingRecord record, TimeSpan timeout);

        class TrackAsyncResult : AsyncResult
        {
            static Action<object> asyncExecuteTrack = new Action<object>(ExecuteTrack);
            TrackingParticipant participant;
            TrackingRecord record;
            TimeSpan timeout;

            public TrackAsyncResult(TrackingParticipant participant, TrackingRecord record, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.participant = participant;
                this.record = record;
                this.timeout = timeout;
                ActionItem.Schedule(asyncExecuteTrack, this);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<TrackAsyncResult>(result);
            }

            static void ExecuteTrack(object state)
            {
                TrackAsyncResult thisPtr = (TrackAsyncResult)state;
                thisPtr.TrackCore();
            }

            void TrackCore()
            {
                Exception participantException = null;
                try
                {
                    this.participant.Track(this.record, this.timeout);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }

                    participantException = exception;
                }
                base.Complete(false, participantException);
            }
        }

    }
}
