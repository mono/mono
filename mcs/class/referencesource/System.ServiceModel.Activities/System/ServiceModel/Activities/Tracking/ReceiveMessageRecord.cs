//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking
{
    using System.Activities.Tracking;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public class ReceiveMessageRecord : CustomTrackingRecord
    {
        public ReceiveMessageRecord(string name)
            : base(name)
        {
        }

        protected ReceiveMessageRecord(ReceiveMessageRecord record)
            : base(record)
        {            
        }

        public Guid E2EActivityId
        {
            get
            {
                return (Guid)this.Data[MessagingActivityHelper.E2EActivityId];
            }

            internal set
            {
                this.Data[MessagingActivityHelper.E2EActivityId] = value;
            }
        }

        public Guid MessageId
        {
            get
            {
                return (Guid)this.Data[MessagingActivityHelper.MessageId];
            }
        }

        protected override TrackingRecord Clone()
        {
            return new ReceiveMessageRecord(this);
        }
    }
}
