//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.ServiceModel.Channels;
using System.Runtime;

namespace System.ServiceModel.Dispatcher
{
    class WebFaultFormatter : IDispatchFaultFormatter, IDispatchFaultFormatterWrapper
    {
        IDispatchFaultFormatter faultFormatter;

        internal WebFaultFormatter(IDispatchFaultFormatter faultFormatter)
        {
            this.faultFormatter = faultFormatter;
        }

        public MessageFault Serialize(FaultException faultException, out string action)
        {
            try
            {
                return this.faultFormatter.Serialize(faultException, out action);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                action = null;
                return MessageFault.Default;
            }
        }

        public IDispatchFaultFormatter InnerFaultFormatter
        {
            get
            {
                return this.faultFormatter;
            }
            set
            {
                this.faultFormatter = value;
            }
        }
    }
}
