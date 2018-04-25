//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking
{
    using System.Activities.Tracking;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public class SendMessageRecord : CustomTrackingRecord
    {
        public SendMessageRecord(string name)
            : base(name)
        {
        }

        protected SendMessageRecord(SendMessageRecord record)
            : base(record)
        {
        }

        public Guid E2EActivityId
        {
            set
            {
                this.Data[MessagingActivityHelper.E2EActivityId] = value;
            }

            get
            {
                return (Guid)this.Data[MessagingActivityHelper.E2EActivityId];
            }
        }

        protected override TrackingRecord Clone()
        {
            return new SendMessageRecord(this);
        }
    }
}
