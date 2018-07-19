//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Globalization;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using SR2 = System.ServiceModel.Activities.SR;

    public class CorrelationActionMessageFilter : MessageFilter
    {
        ActionMessageFilter innerFilter;

        public CorrelationActionMessageFilter()
            : base()
        {
        }

        public string Action
        {
            get;
            set;
        }

        ActionMessageFilter GetInnerFilter()
        {
            if (this.innerFilter == null)
            {
                this.innerFilter = new ActionMessageFilter(this.Action);
            }

            return this.innerFilter;
        }

        public override bool Match(Message message)
        {
            if (message == null)
            {
                throw FxTrace.Exception.ArgumentNull("message");
            }

            return this.GetInnerFilter().Match(message);
        }

        public override bool Match(MessageBuffer messageBuffer)
        {
            if (messageBuffer == null)
            {
                throw FxTrace.Exception.ArgumentNull("messageBuffer");
            }

            return this.GetInnerFilter().Match(messageBuffer);
        }

        public override bool Equals(object other)
        {
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }

            CorrelationActionMessageFilter otherFilter = other as CorrelationActionMessageFilter;
            if (otherFilter == null)
            {
                return false;
            }

            return this.Action == otherFilter.Action;
        }

        public override int GetHashCode()
        {
            return (this.Action != null) ? this.Action.GetHashCode() : 0;
        }

        public override string ToString()
        {
            if (this.Action != null)
            {
                return string.Format(CultureInfo.InvariantCulture, "Action: {0}", this.Action);
            }

            return base.ToString();
        }
    }
}
