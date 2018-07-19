//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Globalization;

    class TransactionChannelFaultConverter<TChannel> : FaultConverter
        where TChannel : class, IChannel
    {
        TransactionChannel<TChannel> channel;

        internal TransactionChannelFaultConverter(TransactionChannel<TChannel> channel)
        {
            this.channel = channel;
        }

        protected override bool OnTryCreateException(Message message, MessageFault fault, out Exception exception)
        {
            if (message.Headers.Action == FaultCodeConstants.Actions.Transactions)
            {
                exception = new ProtocolException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text);
                return true;
            }

            if (fault.IsMustUnderstandFault)
            {
                MessageHeader header = this.channel.Formatter.EmptyTransactionHeader;
                if (MessageFault.WasHeaderNotUnderstood(message.Headers, header.Name, header.Namespace))
                {
                    exception = new ProtocolException(SR.GetString(SR.SFxTransactionHeaderNotUnderstood, header.Name, header.Namespace, this.channel.Protocol));
                    return true;
                }
            }

            FaultConverter inner = this.channel.GetInnerProperty<FaultConverter>();
            if (inner != null)
            {
                return inner.TryCreateException(message, fault, out exception);
            }
            else
            {
                exception = null;
                return false;
            }
        }

        protected override bool OnTryCreateFaultMessage(Exception exception, out Message message)
        {
            FaultConverter inner = this.channel.GetInnerProperty<FaultConverter>();
            if (inner != null)
            {
                return inner.TryCreateFaultMessage(exception, out message);
            }
            else
            {
                message = null;
                return false;
            }
        }
    }
}
