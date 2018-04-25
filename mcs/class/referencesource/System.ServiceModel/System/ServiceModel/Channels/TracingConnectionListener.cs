//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics;
    using System.Threading;
    using System.ServiceModel.Diagnostics.Application;


    class TracingConnectionListener : IConnectionListener
    {
        ServiceModelActivity activity;
        IConnectionListener listener;

        internal TracingConnectionListener(IConnectionListener listener, string traceStartInfo) :
            this(listener, traceStartInfo, true)
        {
        }

        internal TracingConnectionListener(IConnectionListener listener, Uri uri)
            : this(listener, uri.ToString())
        {
        }

        internal TracingConnectionListener(IConnectionListener listener)
        {
            this.listener = listener;
            this.activity = ServiceModelActivity.CreateActivity(DiagnosticTraceBase.ActivityId, false);
        }

        internal TracingConnectionListener(IConnectionListener listener, string traceStartInfo, bool newActivity)
        {
            this.listener = listener;
            if (newActivity)
            {
                this.activity = ServiceModelActivity.CreateActivity();
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    if (null != FxTrace.Trace)
                    {
                        FxTrace.Trace.TraceTransfer(this.activity.Id);
                    }
                    ServiceModelActivity.Start(this.activity, SR.GetString(SR.ActivityListenAt, traceStartInfo), ActivityType.ListenAt);
                }
            }
            else
            {
                this.activity = ServiceModelActivity.CreateActivity(DiagnosticTraceBase.ActivityId, false);
                if (this.activity != null)
                {
                    this.activity.Name = traceStartInfo;
                }
            }
        }

        public void Listen()
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                this.listener.Listen();
            }
        }

        public IAsyncResult BeginAccept(AsyncCallback callback, object state)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                return this.listener.BeginAccept(callback, state);
            }
        }

        public IConnection EndAccept(IAsyncResult result)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                ServiceModelActivity activity = ServiceModelActivity.CreateActivity();
                if (activity != null)
                {
                    if (null != FxTrace.Trace)
                    {
                        FxTrace.Trace.TraceTransfer(activity.Id);
                    }
                }
                using (ServiceModelActivity.BoundOperation(activity))
                {
                    ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityReceiveBytes, this.activity.Name), ActivityType.ReceiveBytes);
                    IConnection innerConnection = this.listener.EndAccept(result);
                    if (innerConnection == null)
                    {
                        return null;
                    }
                    TracingConnection retval = new TracingConnection(innerConnection, activity);
                    return retval;
                }
            }
        }

        public void Dispose()
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                this.listener.Dispose();
                this.activity.Dispose();
            }
        }
    }
}
