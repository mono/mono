//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    internal class SecurityChannelFaultConverter : FaultConverter
    {
        IChannel innerChannel;

        internal SecurityChannelFaultConverter(IChannel innerChannel)
        {
            this.innerChannel = innerChannel;
        }

        protected override bool OnTryCreateException(Message message, MessageFault fault, out Exception exception)
        {
            if (this.innerChannel == null)
            {
                exception = null;
                return false;
            }

            FaultConverter inner = this.innerChannel.GetProperty<FaultConverter>();
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
            if (this.innerChannel == null)
            {
                message = null;
                return false;
            }

            FaultConverter inner = innerChannel.GetProperty<FaultConverter>();
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
