//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Runtime
{
    using System;
    using System.Runtime;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Diagnostics.CodeAnalysis;

    [DataContract]
    class EmptyWithCancelationCheckWorkItem : ActivityExecutionWorkItem
    {
        ActivityInstance completedInstance;

        public EmptyWithCancelationCheckWorkItem(ActivityInstance activityInstance, ActivityInstance completedInstance)
            : base(activityInstance)
        {
            this.completedInstance = completedInstance;
            this.IsEmpty = true;
        }

        [DataMember(Name = "completedInstance")]
        internal ActivityInstance SerializedCompletedInstance
        {
            get { return this.completedInstance; }
            set { this.completedInstance = value; }
        }

        public override void TraceCompleted()
        {
            TraceRuntimeWorkItemCompleted();
        }

        public override void TraceScheduled()
        {
            TraceRuntimeWorkItemScheduled();
        }

        public override void TraceStarting()
        {
            TraceRuntimeWorkItemStarting();
        }

        public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
            Fx.Assert("Empty work items should never been executed.");

            return true;
        }

        public override void PostProcess(ActivityExecutor executor)
        {
            if (this.completedInstance.State != ActivityInstanceState.Closed && this.ActivityInstance.IsPerformingDefaultCancelation)
            {
                this.ActivityInstance.MarkCanceled();
            }

            base.PostProcess(executor);
        }
    }
}
