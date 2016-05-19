//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Diagnostics;
    using System.Threading;

    class ComPlusSynchronizationContext : SynchronizationContext
    {
        IServiceActivity activity;
        bool postSynchronous;

        public ComPlusSynchronizationContext(IServiceActivity activity,
                                             bool postSynchronous)
        {
            this.activity = activity;
            this.postSynchronous = postSynchronous;
        }

        public override void Send(SendOrPostCallback d, Object state)
        {
            Fx.Assert("Send should never be called");
        }

        public override void Post(SendOrPostCallback d, Object state)
        {
            ComPlusActivityTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationEnteringActivity,
                        SR.TraceCodeComIntegrationEnteringActivity);

            ServiceCall call = new ServiceCall(d, state);
            if (this.postSynchronous)
            {
                this.activity.SynchronousCall(call);
            }
            else
            {
                this.activity.AsynchronousCall(call);
            }
            ComPlusActivityTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationLeftActivity,
                        SR.TraceCodeComIntegrationLeftActivity);
        }

        public void Dispose()
        {
            while (Marshal.ReleaseComObject(this.activity) > 0);
        }

        class ServiceCall : IServiceCall
        {
            SendOrPostCallback callback;
            Object state;

            public ServiceCall(SendOrPostCallback callback,
                               Object state)
            {
                this.callback = callback;
                this.state = state;
            }



            public void OnCall()
            {
                ServiceModelActivity activity = null;
                try
                {
                    Guid guidLogicalThreadID = Guid.Empty;

                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        IComThreadingInfo comThreadingInfo;
                        comThreadingInfo = (IComThreadingInfo)SafeNativeMethods.CoGetObjectContext(ComPlusActivityTrace.IID_IComThreadingInfo);

                        if (comThreadingInfo != null)
                        {

                            comThreadingInfo.GetCurrentLogicalThreadId(out guidLogicalThreadID);

                            activity = ServiceModelActivity.CreateBoundedActivity(guidLogicalThreadID);
                        }
                        ServiceModelActivity.Start(activity, SR.GetString(SR.TransferringToComplus, guidLogicalThreadID.ToString()), ActivityType.TransferToComPlus);
                    }
                    ComPlusActivityTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationExecutingCall, SR.TraceCodeComIntegrationExecutingCall);

                    this.callback(this.state);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    DiagnosticUtility.InvokeFinalHandler(e);
                }
                finally
                {
                    if (activity != null)
                    {
                        activity.Dispose();
                        activity = null;
                    }
                }
            }
        }
    }
}
