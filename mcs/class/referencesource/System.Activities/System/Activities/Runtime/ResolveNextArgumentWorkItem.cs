//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Runtime
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Runtime;

    [DataContract]
    class ResolveNextArgumentWorkItem : ActivityExecutionWorkItem
    {
        int nextArgumentIndex;

        IDictionary<string, object> argumentValueOverrides;

        Location resultLocation;

        public ResolveNextArgumentWorkItem()
        {
            this.IsPooled = true;
        }

        [DataMember(EmitDefaultValue = false, Name = "nextArgumentIndex")]
        internal int SerializedNextArgumentIndex
        {
            get { return this.nextArgumentIndex; }
            set { this.nextArgumentIndex = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "argumentValueOverrides")]
        internal IDictionary<string, object> SerializedArgumentValueOverrides
        {
            get { return this.argumentValueOverrides; }
            set { this.argumentValueOverrides = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "resultLocation")]
        internal Location SerializedResultLocation
        {
            get { return this.resultLocation; }
            set { this.resultLocation = value; }
        }

        public override void TraceScheduled()
        {
            TraceRuntimeWorkItemScheduled();
        }

        public override void TraceStarting()
        {
            TraceRuntimeWorkItemStarting();
        }

        public override void TraceCompleted()
        {
            TraceRuntimeWorkItemCompleted();
        }

        public void Initialize(ActivityInstance activityInstance, int nextArgumentIndex, IDictionary<string, object> argumentValueOverrides, Location resultLocation)
        {
            Fx.Assert(nextArgumentIndex > 0, "The nextArgumentIndex must be greater than 0 otherwise we will incorrectly set the sub-state when ResolveArguments completes");
            base.Reinitialize(activityInstance);
            this.nextArgumentIndex = nextArgumentIndex;
            this.argumentValueOverrides = argumentValueOverrides;
            this.resultLocation = resultLocation;
        }

        // Knowledge at a distance! This method relies on the fact that ResolveArguments will
        // always schedule a separate work item for expressions that aren't OldFastPath.
        internal bool CanExecuteUserCode()
        {
            Activity activity = this.ActivityInstance.Activity;
            for (int i = this.nextArgumentIndex; i < activity.RuntimeArguments.Count; i++)
            {
                RuntimeArgument argument = activity.RuntimeArguments[i];
                if (argument.IsBound && argument.BoundArgument.Expression != null)
                {
                    return argument.BoundArgument.Expression.UseOldFastPath;
                }
            }
            return false;
        }

        protected override void ReleaseToPool(ActivityExecutor executor)
        {
            base.ClearForReuse();
            this.nextArgumentIndex = 0;
            this.resultLocation = null;
            this.argumentValueOverrides = null;

            executor.ResolveNextArgumentWorkItemPool.Release(this);
        }

        public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
            this.ActivityInstance.ResolveArguments(executor, argumentValueOverrides, resultLocation, nextArgumentIndex);

            // Return true always to prevent scheduler from yielding silently.
            return true;
        }
    }
}
