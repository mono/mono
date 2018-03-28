//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Remoting.Messaging;
    using System.Threading;
    using System.Globalization;
    using System.Collections.Generic;
    using System.ServiceModel.Diagnostics.Application;
    using System.Runtime.Diagnostics;


    class ServiceModelActivity : IDisposable
    {
        [ThreadStatic]
        static ServiceModelActivity currentActivity;

        static string[] ActivityTypeNames = new string[(int)ActivityType.NumItems];

        ServiceModelActivity previousActivity = null;
        static string activityBoundaryDescription = null;
        ActivityState lastState = ActivityState.Unknown;
        string name = null;
        bool autoStop = false;
        bool autoResume = false;
        Guid activityId;
        bool disposed = false;
        bool isAsync = false;
        int stopCount = 0;
        const int AsyncStopCount = 2;
        TransferActivity activity = null;
        ActivityType activityType = ActivityType.Unknown;

        static ServiceModelActivity()
        {
            ActivityTypeNames[(int)ActivityType.Unknown] = "Unknown";
            ActivityTypeNames[(int)ActivityType.Close] = "Close";
            ActivityTypeNames[(int)ActivityType.Construct] = "Construct";
            ActivityTypeNames[(int)ActivityType.ExecuteUserCode] = "ExecuteUserCode";
            ActivityTypeNames[(int)ActivityType.ListenAt] = "ListenAt";
            ActivityTypeNames[(int)ActivityType.Open] = "Open";
            ActivityTypeNames[(int)ActivityType.OpenClient] = "Open";
            ActivityTypeNames[(int)ActivityType.ProcessMessage] = "ProcessMessage";
            ActivityTypeNames[(int)ActivityType.ProcessAction] = "ProcessAction";
            ActivityTypeNames[(int)ActivityType.ReceiveBytes] = "ReceiveBytes";
            ActivityTypeNames[(int)ActivityType.SecuritySetup] = "SecuritySetup";
            ActivityTypeNames[(int)ActivityType.TransferToComPlus] = "TransferToComPlus";
            ActivityTypeNames[(int)ActivityType.WmiGetObject] = "WmiGetObject";
            ActivityTypeNames[(int)ActivityType.WmiPutInstance] = "WmiPutInstance";
        }

        ServiceModelActivity(Guid activityId)
        {
            this.activityId = activityId;
            this.previousActivity = ServiceModelActivity.Current;
        }

        static string ActivityBoundaryDescription
        {
            get
            {
                if (ServiceModelActivity.activityBoundaryDescription == null)
                {
                    ServiceModelActivity.activityBoundaryDescription = TraceSR.GetString(TraceSR.ActivityBoundary);
                }
                return ServiceModelActivity.activityBoundaryDescription;
            }
        }

        internal ActivityType ActivityType
        {
            get { return this.activityType; }
        }

        internal ServiceModelActivity PreviousActivity
        {
            get { return this.previousActivity; }
        }

        static internal Activity BoundOperation(ServiceModelActivity activity)
        {
            if (!DiagnosticUtility.ShouldUseActivity)
            {
                return null;
            }
            return ServiceModelActivity.BoundOperation(activity, false);
        }

        static internal Activity BoundOperation(ServiceModelActivity activity, bool addTransfer)
        {
            return activity == null ? null : ServiceModelActivity.BoundOperationCore(activity, addTransfer);
        }

        static Activity BoundOperationCore(ServiceModelActivity activity, bool addTransfer)
        {
            if (!DiagnosticUtility.ShouldUseActivity)
            {
                return null;
            }
            TransferActivity retval = null;
            if (activity != null)
            {
                retval = TransferActivity.CreateActivity(activity.activityId, addTransfer);
                if (retval != null)
                {
                    retval.SetPreviousServiceModelActivity(ServiceModelActivity.Current);
                }
                ServiceModelActivity.Current = activity;
            }
            return retval;
        }

        internal static ServiceModelActivity CreateActivity()
        {
            if (!DiagnosticUtility.ShouldUseActivity)
            {
                return null;
            }
            return ServiceModelActivity.CreateActivity(Guid.NewGuid(), true);
        }

        internal static ServiceModelActivity CreateActivity(bool autoStop)
        {
            if (!DiagnosticUtility.ShouldUseActivity)
            {
                return null;
            }
            ServiceModelActivity activity = ServiceModelActivity.CreateActivity(Guid.NewGuid(), true);
            if (activity != null)
            {
                activity.autoStop = autoStop;
            }
            return activity;
        }

        internal static ServiceModelActivity CreateActivity(bool autoStop, string activityName, ActivityType activityType)
        {
            if (!DiagnosticUtility.ShouldUseActivity)
            {
                return null;
            }
            ServiceModelActivity activity = ServiceModelActivity.CreateActivity(autoStop);
            ServiceModelActivity.Start(activity, activityName, activityType);
            return activity;
        }

        internal static ServiceModelActivity CreateAsyncActivity()
        {
            if (!DiagnosticUtility.ShouldUseActivity)
            {
                return null;
            }
            ServiceModelActivity activity = ServiceModelActivity.CreateActivity(true);
            if (activity != null)
            {
                activity.isAsync = true;
            }
            return activity;
        }

        internal static ServiceModelActivity CreateBoundedActivity()
        {
            return ServiceModelActivity.CreateBoundedActivity(false);
        }

        internal static ServiceModelActivity CreateBoundedActivity(bool suspendCurrent)
        {
            if (!DiagnosticUtility.ShouldUseActivity)
            {
                return null;
            }
            ServiceModelActivity activityToSuspend = ServiceModelActivity.Current;
            ServiceModelActivity retval = ServiceModelActivity.CreateActivity(true);
            if (retval != null)
            {
                retval.activity = (TransferActivity)ServiceModelActivity.BoundOperation(retval, true);
                retval.activity.SetPreviousServiceModelActivity(activityToSuspend);
                if (suspendCurrent)
                {
                    retval.autoResume = true;
                }
            }
            if (suspendCurrent && activityToSuspend != null)
            {
                activityToSuspend.Suspend();
            }
            return retval;
        }

        internal static ServiceModelActivity CreateBoundedActivity(Guid activityId)
        {
            if (!DiagnosticUtility.ShouldUseActivity)
            {
                return null;
            }
            ServiceModelActivity retval = ServiceModelActivity.CreateActivity(activityId, true);
            if (retval != null)
            {
                retval.activity = (TransferActivity)ServiceModelActivity.BoundOperation(retval, true);
            }
            return retval;
        }

        internal static ServiceModelActivity CreateBoundedActivityWithTransferInOnly(Guid activityId)
        {
            if (!DiagnosticUtility.ShouldUseActivity)
            {
                return null;
            }
            ServiceModelActivity retval = ServiceModelActivity.CreateActivity(activityId, true);
            if (retval != null)
            {
                if (null != FxTrace.Trace)
                {
                    FxTrace.Trace.TraceTransfer(activityId);
                }
                retval.activity = (TransferActivity)ServiceModelActivity.BoundOperation(retval);
            }
            return retval;
        }

        internal static ServiceModelActivity CreateLightWeightAsyncActivity(Guid activityId)
        {
            return new ServiceModelActivity(activityId);
        }

        internal static ServiceModelActivity CreateActivity(Guid activityId)
        {
            if (!DiagnosticUtility.ShouldUseActivity)
            {
                return null;
            }
            ServiceModelActivity retval = null;
            if (activityId != Guid.Empty)
            {
                retval = new ServiceModelActivity(activityId);
            }
            if (retval != null)
            {
                ServiceModelActivity.Current = retval;
            }
            return retval;
        }

        internal static ServiceModelActivity CreateActivity(Guid activityId, bool autoStop)
        {
            if (!DiagnosticUtility.ShouldUseActivity)
            {
                return null;
            }
            ServiceModelActivity retval = ServiceModelActivity.CreateActivity(activityId);
            if (retval != null)
            {
                retval.autoStop = autoStop;
            }
            return retval;
        }

        internal static ServiceModelActivity Current
        {
            get { return ServiceModelActivity.currentActivity; }
            private set { ServiceModelActivity.currentActivity = value; }
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                try
                {
                    if (this.activity != null)
                    {
                        this.activity.Dispose();
                    }
                    if (this.autoStop)
                    {
                        this.Stop();
                    }
                    if (this.autoResume &&
                        ServiceModelActivity.Current != null)
                    {
                        ServiceModelActivity.Current.Resume();
                    }
                }
                finally
                {
                    ServiceModelActivity.Current = this.previousActivity;
                    GC.SuppressFinalize(this);
                }
            }
        }

        internal Guid Id
        {
            get { return this.activityId; }
        }

        ActivityState LastState
        {
            get { return this.lastState; }
            set { this.lastState = value; }
        }

        internal string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        internal void Resume()
        {
            if (this.LastState == ActivityState.Suspend)
            {
                this.LastState = ActivityState.Resume;
                this.TraceMilestone(TraceEventType.Resume);
            }
        }

        internal void Resume(string activityName)
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                this.name = activityName;
            }
            this.Resume();
        }

        static internal void Start(ServiceModelActivity activity, string activityName, ActivityType activityType)
        {
            if (activity != null && activity.LastState == ActivityState.Unknown)
            {
                activity.LastState = ActivityState.Start;
                activity.name = activityName;
                activity.activityType = activityType;
                activity.TraceMilestone(TraceEventType.Start);
            }
        }

        internal void Stop()
        {
            int newStopCount = 0;
            if (this.isAsync)
            {
                newStopCount = Interlocked.Increment(ref this.stopCount);
            }
            if (this.LastState != ActivityState.Stop &&
                (!this.isAsync || (this.isAsync && newStopCount >= ServiceModelActivity.AsyncStopCount)))
            {
                this.LastState = ActivityState.Stop;
                this.TraceMilestone(TraceEventType.Stop);
            }
        }

        static internal void Stop(ServiceModelActivity activity)
        {
            if (activity != null)
            {
                activity.Stop();
            }
        }

        internal void Suspend()
        {
            if (this.LastState != ActivityState.Stop)
            {
                this.LastState = ActivityState.Suspend;
                this.TraceMilestone(TraceEventType.Suspend);
            }
        }

        public override string ToString()
        {
            return this.Id.ToString();
        }

        void TraceMilestone(TraceEventType type)
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                if (null != FxTrace.Trace)
                {
                    CallEtwMileStoneEvent(type, null);
                }
                if (null != DiagnosticUtility.DiagnosticTrace)
                {
                    TraceUtility.TraceEventNoCheck(type, TraceCode.ActivityBoundary, ServiceModelActivity.ActivityBoundaryDescription, null, ServiceModelActivity.ActivityBoundaryDescription, (Exception)null);
                }
            }
            else
            {
                if (null != FxTrace.Trace)
                {
                    Dictionary<string, string> values = new Dictionary<string, string>(2);
                    values["ActivityName"] = this.Name;
                    values["ActivityType"] = ServiceModelActivity.ActivityTypeNames[(int)this.activityType];
                    using (DiagnosticUtility.ShouldUseActivity && Guid.Empty == activityId ? null : Activity.CreateActivity(this.Id))
                    {
                        CallEtwMileStoneEvent(type, new DictionaryTraceRecord(values));
                    }
                }
                if (null != DiagnosticUtility.DiagnosticTrace)
                {
                    Dictionary<string, string> values = new Dictionary<string, string>(2);
                    values["ActivityName"] = this.Name;
                    values["ActivityType"] = ServiceModelActivity.ActivityTypeNames[(int)this.activityType];
                    TraceUtility.TraceEventNoCheck(type, TraceCode.ActivityBoundary, ServiceModelActivity.ActivityBoundaryDescription, new DictionaryTraceRecord(values), null, null, this.Id);
                }
            }
        }

        void CallEtwMileStoneEvent(TraceEventType type, DictionaryTraceRecord record)
        {
            switch (type)
            {
                case TraceEventType.Start:

                    if (TD.StartSignpostEventIsEnabled())
                    {
                        TD.StartSignpostEvent(record);
                    }
                    break;
                case TraceEventType.Stop:
                    if (TD.StopSignpostEventIsEnabled())
                    {
                        TD.StopSignpostEvent(record);
                    }
                    break;
                case TraceEventType.Suspend:
                    if (TD.SuspendSignpostEventIsEnabled())
                    {
                        TD.SuspendSignpostEvent(record);
                    }
                    break;
                case TraceEventType.Resume:
                    if (TD.ResumeSignpostEventIsEnabled())
                    {
                        TD.ResumeSignpostEvent(record);
                    }
                    break;
            }
        }

        enum ActivityState
        {
            Unknown,
            Start,
            Suspend,
            Resume,
            Stop,
        }

        class TransferActivity : Activity
        {
            bool addTransfer = false;
            bool changeCurrentServiceModelActivity = false;
            ServiceModelActivity previousActivity = null;

            TransferActivity(Guid activityId, Guid parentId)
                : base(activityId, parentId)
            {
            }

            internal static TransferActivity CreateActivity(Guid activityId, bool addTransfer)
            {
                if (!DiagnosticUtility.ShouldUseActivity)
                {
                    return null;
                }
                TransferActivity retval = null;
                if (DiagnosticUtility.TracingEnabled && activityId != Guid.Empty)
                {
                    Guid currentActivityId = DiagnosticTraceBase.ActivityId;
                    if (activityId != currentActivityId)
                    {
                        if (addTransfer)
                        {
                            if (null != FxTrace.Trace)
                            {
                                FxTrace.Trace.TraceTransfer(activityId);
                            }
                        }
                        TransferActivity activity = new TransferActivity(activityId, currentActivityId);
                        activity.addTransfer = addTransfer;
                        retval = activity;
                    }
                }
                return retval;
            }

            internal void SetPreviousServiceModelActivity(ServiceModelActivity previous)
            {
                this.previousActivity = previous;
                this.changeCurrentServiceModelActivity = true;
            }

            public override void Dispose()
            {
                try
                {
                    if (addTransfer)
                    {
                        // Make sure that we are transferring from our AID to the 
                        // parent. It is possible for someone else to change the ambient
                        // in user code (MB 49318).
                        using (Activity.CreateActivity(this.Id))
                        {
                            if (null != FxTrace.Trace)
                            {
                                FxTrace.Trace.TraceTransfer(this.parentId);
                            }
                        }
                    }
                }
                finally
                {
                    if (this.changeCurrentServiceModelActivity)
                    {
                        ServiceModelActivity.Current = this.previousActivity;
                    }
                    base.Dispose();
                }
            }
        }
    }
}
