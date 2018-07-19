//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.ServiceModel.Channels;

    class AndMessageFilter : MessageFilter
    {
        MessageFilter filter1;
        MessageFilter filter2;

        public AndMessageFilter(MessageFilter filter1, MessageFilter filter2)
        {
            if (filter1 == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter1");
            if (filter2 == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("filter2");

            this.filter1 = filter1;
            this.filter2 = filter2;
        }

        public MessageFilter Filter1
        {
            get
            {
                return this.filter1;
            }
        }

        public MessageFilter Filter2
        {
            get
            {
                return this.filter2;
            }
        }

        protected internal override IMessageFilterTable<FilterData> CreateFilterTable<FilterData>()
        {
            return new AndMessageFilterTable<FilterData>();
        }

        public override bool Match(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            return this.filter1.Match(message) && this.filter2.Match(message);
        }

        internal bool Match(Message message, out bool addressMatched)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            if (this.filter1.Match(message))
            {
                addressMatched = true;
                return this.filter2.Match(message);
            }
            else
            {
                addressMatched = false;
                return false;
            }
        }

        public override bool Match(MessageBuffer messageBuffer)
        {
            if (messageBuffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }

            return this.filter1.Match(messageBuffer) && this.filter2.Match(messageBuffer);
        }
    }
}
