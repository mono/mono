//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Runtime
{
    using System;
    using System.Activities.DynamicUpdate;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;

    // can't add FuncCompletionCallbackWrapper<T> since we don't know what to close the generic with
    [KnownType(typeof(ActivityCompletionCallbackWrapper))]
    [KnownType(typeof(DelegateCompletionCallbackWrapper))]
    [DataContract]
    abstract class CompletionCallbackWrapper : CallbackWrapper
    {
        static Type completionCallbackType = typeof(CompletionCallback);
        static Type[] completionCallbackParameters = new Type[] { typeof(NativeActivityContext), typeof(ActivityInstance) };

        bool checkForCancelation;

        bool needsToGatherOutputs;

        protected CompletionCallbackWrapper(Delegate callback, ActivityInstance owningInstance)
            : base(callback, owningInstance)
        {
        }

        protected bool NeedsToGatherOutputs
        {
            get
            {
                return this.needsToGatherOutputs;
            }

            set
            {
                this.needsToGatherOutputs = value;
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "checkForCancelation")]
        internal bool SerializedCheckForCancelation
        {
            get { return this.checkForCancelation; }
            set { this.checkForCancelation = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "needsToGatherOutputs")]
        internal bool SerializedNeedsToGatherOutputs
        {
            get { return this.needsToGatherOutputs; }
            set { this.needsToGatherOutputs = value; }
        }

        public void CheckForCancelation()
        {
            this.checkForCancelation = true;
        }

        protected virtual void GatherOutputs(ActivityInstance completedInstance)
        {
            // No-op in the base class
        }

        internal WorkItem CreateWorkItem(ActivityInstance completedInstance, ActivityExecutor executor)
        {
            // We use the property to guard against the virtual method call
            // since we don't need it in the common case
            if (this.NeedsToGatherOutputs)
            {
                this.GatherOutputs(completedInstance);
            }

            CompletionWorkItem workItem;

            if (this.checkForCancelation)
            {
                workItem = new CompletionWithCancelationCheckWorkItem(this, completedInstance);
            }
            else
            {
                workItem = executor.CompletionWorkItemPool.Acquire();
                workItem.Initialize(this, completedInstance);
            }

            if (completedInstance.InstanceMap != null)
            {
                completedInstance.InstanceMap.AddEntry(workItem);
            }

            return workItem;
        }

        [Fx.Tag.SecurityNote(Critical = "Because any implementation will be calling EnsureCallback",
            Safe = "Safe because the method needs to be part of an Activity and we are casting to the callback type and it has a very specific signature. The author of the callback is buying into being invoked from PT.")]
        [SecuritySafeCritical]
        protected internal abstract void Invoke(NativeActivityContext context, ActivityInstance completedInstance);

        [DataContract]
        public class CompletionWorkItem : ActivityExecutionWorkItem, ActivityInstanceMap.IActivityReference
        {
            CompletionCallbackWrapper callbackWrapper;
            ActivityInstance completedInstance;

            // Called by the Pool.
            public CompletionWorkItem()
            {
                this.IsPooled = true;
            }

            // Only used by non-pooled base classes.
            protected CompletionWorkItem(CompletionCallbackWrapper callbackWrapper, ActivityInstance completedInstance)
                : base(callbackWrapper.ActivityInstance)
            {
                this.callbackWrapper = callbackWrapper;
                this.completedInstance = completedInstance;
            }

            protected ActivityInstance CompletedInstance
            {
                get
                {
                    return this.completedInstance;
                }
            }

            [DataMember(Name = "callbackWrapper")]
            internal CompletionCallbackWrapper SerializedCallbackWrapper
            {
                get { return this.callbackWrapper; }
                set { this.callbackWrapper = value; }
            }

            [DataMember(Name = "completedInstance")]
            internal ActivityInstance SerializedCompletedInstance
            {
                get { return this.completedInstance; }
                set { this.completedInstance = value; }
            }

            public void Initialize(CompletionCallbackWrapper callbackWrapper, ActivityInstance completedInstance)
            {
                base.Reinitialize(callbackWrapper.ActivityInstance);
                this.callbackWrapper = callbackWrapper;
                this.completedInstance = completedInstance;
            }

            protected override void ReleaseToPool(ActivityExecutor executor)
            {
                base.ClearForReuse();
                this.callbackWrapper = null;
                this.completedInstance = null;
            
                executor.CompletionWorkItemPool.Release(this);
            }

            public override void TraceCompleted()
            {
                if (TD.CompleteCompletionWorkItemIsEnabled())
                {
                    TD.CompleteCompletionWorkItem(this.ActivityInstance.Activity.GetType().ToString(), this.ActivityInstance.Activity.DisplayName, this.ActivityInstance.Id, this.completedInstance.Activity.GetType().ToString(), this.completedInstance.Activity.DisplayName, this.completedInstance.Id);
                }
            }

            public override void TraceScheduled()
            {
                if (TD.ScheduleCompletionWorkItemIsEnabled())
                {
                    TD.ScheduleCompletionWorkItem(this.ActivityInstance.Activity.GetType().ToString(), this.ActivityInstance.Activity.DisplayName, this.ActivityInstance.Id, this.completedInstance.Activity.GetType().ToString(), this.completedInstance.Activity.DisplayName, this.completedInstance.Id);
                }
            }

            public override void TraceStarting()
            {
                if (TD.StartCompletionWorkItemIsEnabled())
                {
                    TD.StartCompletionWorkItem(this.ActivityInstance.Activity.GetType().ToString(), this.ActivityInstance.Activity.DisplayName, this.ActivityInstance.Id, this.completedInstance.Activity.GetType().ToString(), this.completedInstance.Activity.DisplayName, this.completedInstance.Id);
                }
            }

            public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
            {
                NativeActivityContext context = executor.NativeActivityContextPool.Acquire();

                Fx.Assert(this.completedInstance.Activity != null, "Activity definition should always be associated with an activity instance.");

                try
                {
                    context.Initialize(this.ActivityInstance, executor, bookmarkManager);
                    this.callbackWrapper.Invoke(context, this.completedInstance);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    this.ExceptionToPropagate = e;
                }
                finally
                {
                    context.Dispose();
                    executor.NativeActivityContextPool.Release(context);

                    if (this.ActivityInstance.InstanceMap != null)
                    {
                        this.ActivityInstance.InstanceMap.RemoveEntry(this);
                    }
                }

                return true;
            }

            Activity ActivityInstanceMap.IActivityReference.Activity
            {
                get 
                {
                    return this.completedInstance.Activity;
                }
            }

            void ActivityInstanceMap.IActivityReference.Load(Activity activity, ActivityInstanceMap instanceMap)
            {
                if (this.completedInstance.Activity == null)
                {
                    ((ActivityInstanceMap.IActivityReference)this.completedInstance).Load(activity, instanceMap);
                }
            }

        }

        [DataContract]
        class CompletionWithCancelationCheckWorkItem : CompletionWorkItem
        {
            public CompletionWithCancelationCheckWorkItem(CompletionCallbackWrapper callbackWrapper, ActivityInstance completedInstance)
                : base(callbackWrapper, completedInstance)
            {
            }

            public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
            {
                if (this.CompletedInstance.State != ActivityInstanceState.Closed && this.ActivityInstance.IsPerformingDefaultCancelation)
                {
                    this.ActivityInstance.MarkCanceled();
                }

                return base.Execute(executor, bookmarkManager);
            }
        }
    }
}
