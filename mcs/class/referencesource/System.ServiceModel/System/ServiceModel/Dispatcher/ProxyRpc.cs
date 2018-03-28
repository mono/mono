//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Runtime.Diagnostics;

    struct ProxyRpc
    {
        internal readonly string Action;
        internal ServiceModelActivity Activity;
        internal Guid ActivityId;
        internal readonly ServiceChannel Channel;
        internal object[] Correlation;
        internal readonly object[] InputParameters;
        internal readonly ProxyOperationRuntime Operation;
        internal object[] OutputParameters;
        internal Message Request;
        internal Message Reply;
        internal object ReturnValue;
        internal MessageVersion MessageVersion;
        internal readonly TimeoutHelper TimeoutHelper;
        EventTraceActivity eventTraceActivity;

        internal ProxyRpc(ServiceChannel channel, ProxyOperationRuntime operation, string action, object[] inputs, TimeSpan timeout)
        {
            this.Action = action;
            this.Activity = null;
            this.eventTraceActivity = null;
            this.Channel = channel;
            this.Correlation = EmptyArray.Allocate(operation.Parent.CorrelationCount);
            this.InputParameters = inputs;
            this.Operation = operation;
            this.OutputParameters = null;
            this.Request = null;
            this.Reply = null;
            this.ActivityId = Guid.Empty;
            this.ReturnValue = null;
            this.MessageVersion = channel.MessageVersion;
            this.TimeoutHelper = new TimeoutHelper(timeout);
        }

        internal EventTraceActivity EventTraceActivity
        {
            get
            {
                if (this.eventTraceActivity == null)
                {
                    this.eventTraceActivity = new EventTraceActivity();
                }
                return this.eventTraceActivity;
            }

            set
            {
                this.eventTraceActivity = value;
            }
        }

    }
}
