//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics;
    using System.Threading;
    using System.ServiceModel.Diagnostics.Application;

    class TracingConnection : DelegatingConnection
    {
        ServiceModelActivity activity;

        static WaitCallback callback;

        public TracingConnection(IConnection connection, ServiceModelActivity activity)
            : base(connection)
        {
            this.activity = activity;
        }

        public TracingConnection(IConnection connection, bool inheritCurrentActivity)
            : base(connection)
        {
            this.activity = inheritCurrentActivity ?
                ServiceModelActivity.CreateActivity(DiagnosticTraceBase.ActivityId, false) :
                ServiceModelActivity.CreateActivity();
            Fx.Assert(this.activity != null, "");
            if (DiagnosticUtility.ShouldUseActivity && !inheritCurrentActivity)
            {
                if (null != FxTrace.Trace)
                {
                    FxTrace.Trace.TraceTransfer(this.activity.Id);
                }
            }
        }

        public override void Abort()
        {
            try
            {
                using (ServiceModelActivity.BoundOperation(this.activity))
                {
                    base.Abort();
                }
            }
            finally
            {
                if (this.activity != null)
                {
                    this.activity.Dispose();
                }
            }
        }

        static WaitCallback Callback
        {
            get
            {
                if (TracingConnection.callback == null)
                {
                    TracingConnection.callback = new WaitCallback(TracingConnection.WaitCallback);
                }
                return TracingConnection.callback;
            }
        }

        public override void Close(TimeSpan timeout, bool asyncAndLinger)
        {
            try
            {
                using (ServiceModelActivity.BoundOperation(this.activity, true))
                {
                    base.Close(timeout, asyncAndLinger);
                }
            }
            finally
            {
                if (this.activity != null)
                {
                    this.activity.Dispose();
                }
            }
        }

        public override void Shutdown(TimeSpan timeout)
        {
            using (ServiceModelActivity.BoundOperation(this.activity, true))
            {
                base.Shutdown(timeout);
            }
        }

        internal void ActivityStart(string name)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                ServiceModelActivity.Start(this.activity, SR.GetString(SR.ActivityReceiveBytes, name), ActivityType.ReceiveBytes);
            }
        }

        internal void ActivityStart(Uri uri)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                ServiceModelActivity.Start(this.activity, SR.GetString(SR.ActivityReceiveBytes, uri.ToString()), ActivityType.ReceiveBytes);
            }
        }

        public override AsyncCompletionResult BeginWrite(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout, WaitCallback callback, object state)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                return base.BeginWrite(buffer, offset, size, immediate, timeout, callback, state);
            }
        }

        public override void EndWrite()
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                base.EndWrite();
            }
        }

        public override void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                base.Write(buffer, offset, size, immediate, timeout);
            }
        }

        public override void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout, BufferManager bufferManager)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                base.Write(buffer, offset, size, immediate, timeout, bufferManager);
            }
        }

        public override int Read(byte[] buffer, int offset, int size, TimeSpan timeout)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                return base.Read(buffer, offset, size, timeout);
            }
        }

        static void WaitCallback(object state)
        {
            TracingConnectionState tracingData = (TracingConnectionState)state;
            tracingData.ExecuteCallback();
        }

        public override AsyncCompletionResult BeginRead(int offset, int size, TimeSpan timeout, System.Threading.WaitCallback callback, object state)
        {
            using (ServiceModelActivity.BoundOperation(this.activity))
            {
                TracingConnectionState completion = new TracingConnectionState(callback, this.activity, state);
                return base.BeginRead(offset, size, timeout, TracingConnection.Callback, completion);
            }
        }

        public override int EndRead()
        {
            int retval = 0;
            try
            {
                if (this.activity != null)
                {
                    ExceptionUtility.UseActivityId(this.activity.Id);
                }
                retval = base.EndRead();
            }
            finally
            {
                ExceptionUtility.ClearActivityId();
            }
            return retval;
        }

        public override object DuplicateAndClose(int targetProcessId)
        {
            using (ServiceModelActivity.BoundOperation(this.activity, true))
            {
                return base.DuplicateAndClose(targetProcessId);
            }
        }

        class TracingConnectionState
        {
            object state;
            WaitCallback callback;
            ServiceModelActivity activity;

            internal TracingConnectionState(WaitCallback callback, ServiceModelActivity activity, object state)
            {
                this.activity = activity;
                this.callback = callback;
                this.state = state;
            }

            internal void ExecuteCallback()
            {
                using (ServiceModelActivity.BoundOperation(this.activity))
                {
                    this.callback(state);
                }
            }
        }
    }
}
