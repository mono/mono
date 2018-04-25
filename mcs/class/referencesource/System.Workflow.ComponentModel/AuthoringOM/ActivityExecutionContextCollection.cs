#pragma warning disable 1634, 1691

namespace System.Workflow.ComponentModel
{
    #region Imports

    using System;
    using System.Diagnostics;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Workflow.ComponentModel.Serialization;
    #endregion

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ActivityExecutionContextManager
    {
        #region Data members and constructor

        private ActivityExecutionContext ownerContext = null;
        private List<ActivityExecutionContext> executionContexts = new List<ActivityExecutionContext>();

        internal ActivityExecutionContextManager(ActivityExecutionContext ownerContext)
        {
            this.ownerContext = ownerContext;

            // Populate the child collection.
            IList<Activity> activeContexts = (IList<Activity>)this.ownerContext.Activity.ContextActivity.GetValue(Activity.ActiveExecutionContextsProperty);
            if (activeContexts != null)
            {
                foreach (Activity activeContextActivity in activeContexts)
                    this.executionContexts.Add(new ActivityExecutionContext(activeContextActivity));
            }
        }

        #endregion

        #region Public members

        public ReadOnlyCollection<ActivityExecutionContext> ExecutionContexts
        {
            get
            {
                if (this.ownerContext == null)
#pragma warning suppress 56503
                    throw new ObjectDisposedException("ActivityExecutionContextManager");

                return new ReadOnlyCollection<ActivityExecutionContext>(this.executionContexts);
            }
        }

        public ActivityExecutionContext CreateExecutionContext(Activity activity)
        {
            if (this.ownerContext == null)
                throw new ObjectDisposedException("ActivityExecutionContextManager");

            if (activity == null)
                throw new ArgumentNullException("activity");

            if (!this.ownerContext.IsValidChild(activity, true))
                throw new ArgumentException(SR.GetString(SR.AEC_InvalidActivity), "activity");

            Activity copiedActivity = activity.Clone();
            ((IDependencyObjectAccessor)copiedActivity).InitializeInstanceForRuntime(this.ownerContext.Activity.WorkflowCoreRuntime);

            //Reset the cloned tree for execution.
            Queue<Activity> activityQueue = new Queue<Activity>();
            activityQueue.Enqueue(copiedActivity);

            while (activityQueue.Count != 0)
            {
                Activity clonedActivity = activityQueue.Dequeue();
                if (clonedActivity.ExecutionStatus != ActivityExecutionStatus.Initialized)
                {
                    clonedActivity.ResetAllKnownDependencyProperties();
                    CompositeActivity compositeActivity = clonedActivity as CompositeActivity;

                    if (compositeActivity != null)
                    {
                        for (int i = 0; i < compositeActivity.EnabledActivities.Count; ++i)
                        {
                            activityQueue.Enqueue(compositeActivity.EnabledActivities[i]);
                        }

                        ISupportAlternateFlow alternateFlow = compositeActivity as ISupportAlternateFlow;

                        if (alternateFlow != null)
                        {
                            for (int i = 0; i < alternateFlow.AlternateFlowActivities.Count; ++i)
                            {
                                activityQueue.Enqueue(alternateFlow.AlternateFlowActivities[i]);
                            }
                        }
                    }
                }
            }

            // get active context activities and add it to this one
            IList<Activity> activeContexts = (IList<Activity>)this.ownerContext.Activity.ContextActivity.GetValue(Activity.ActiveExecutionContextsProperty);
            if (activeContexts == null)
            {
                activeContexts = new List<Activity>();
                this.ownerContext.Activity.ContextActivity.SetValue(Activity.ActiveExecutionContextsProperty, activeContexts);
            }
            activeContexts.Add(copiedActivity);

            // prepare the copied activity as a context activity
            ActivityExecutionContextInfo contextInfo = new ActivityExecutionContextInfo(activity.QualifiedName, this.ownerContext.WorkflowCoreRuntime.GetNewContextActivityId(), Guid.NewGuid(), this.ownerContext.ContextId);
            copiedActivity.SetValue(Activity.ActivityExecutionContextInfoProperty, contextInfo);
            copiedActivity.SetValue(Activity.ActivityContextGuidProperty, contextInfo.ContextGuid);
            ActivityExecutionContext newExecutionContext = null;
            try
            {
                // inform workflow runtime
                this.ownerContext.Activity.WorkflowCoreRuntime.RegisterContextActivity(copiedActivity);

                // return the new context
                newExecutionContext = new ActivityExecutionContext(copiedActivity);
                this.executionContexts.Add(newExecutionContext);
                newExecutionContext.InitializeActivity(newExecutionContext.Activity);
                return newExecutionContext;
            }
            catch (Exception)
            {
                if (newExecutionContext != null)
                {
                    this.CompleteExecutionContext(newExecutionContext);
                }
                else
                {
                    activeContexts.Remove(copiedActivity);
                }
                throw;
            }
        }

        public void CompleteExecutionContext(ActivityExecutionContext childContext)
        {
            if (this.ownerContext == null)
                throw new ObjectDisposedException("ActivityExecutionContextManager");

            CompleteExecutionContext(childContext, false);
        }

        public void CompleteExecutionContext(ActivityExecutionContext childContext, bool forcePersist)
        {
            if (this.ownerContext == null)
                throw new ObjectDisposedException("ActivityExecutionContextManager");

            if (childContext == null)
                throw new ArgumentNullException("childContext");

            if (childContext.Activity == null)
                throw new ArgumentException("childContext", SR.GetString(SR.Error_MissingActivityProperty));

            if (childContext.Activity.ContextActivity == null)
                throw new ArgumentException("childContext", SR.GetString(SR.Error_MissingContextActivityProperty));

            if (!this.executionContexts.Contains(childContext))
                throw new ArgumentException();

            if (childContext.Activity.ContextActivity.ExecutionStatus != ActivityExecutionStatus.Closed && childContext.Activity.ContextActivity.ExecutionStatus != ActivityExecutionStatus.Initialized)
                throw new InvalidOperationException(SR.GetString(System.Globalization.CultureInfo.CurrentCulture, SR.Error_CannotCompleteContext));

            // make sure that this is in the active contexts collections
            ActivityExecutionContextInfo childContextInfo = childContext.Activity.ContextActivity.GetValue(Activity.ActivityExecutionContextInfoProperty) as ActivityExecutionContextInfo;
            IList<Activity> activeContexts = (IList<Activity>)this.ownerContext.Activity.ContextActivity.GetValue(Activity.ActiveExecutionContextsProperty);
            if (activeContexts == null || !activeContexts.Contains(childContext.Activity.ContextActivity))
                throw new ArgumentException();

            // add it to completed contexts collection
            bool needsCompensation = childContext.Activity.NeedsCompensation;
            if (needsCompensation || forcePersist)
            {
                // add it to completed contexts
                List<ActivityExecutionContextInfo> completedContexts = this.ownerContext.Activity.ContextActivity.GetValue(Activity.CompletedExecutionContextsProperty) as List<ActivityExecutionContextInfo>;
                if (completedContexts == null)
                {
                    completedContexts = new List<ActivityExecutionContextInfo>();
                    this.ownerContext.Activity.ContextActivity.SetValue(Activity.CompletedExecutionContextsProperty, completedContexts);
                }

                if (needsCompensation)
                    childContextInfo.Flags = PersistFlags.NeedsCompensation;
                if (forcePersist)
                    childContextInfo.Flags |= PersistFlags.ForcePersist;

                childContextInfo.SetCompletedOrderId(this.ownerContext.Activity.IncrementCompletedOrderId());
                completedContexts.Add(childContextInfo);

                // ask runtime to save the context activity
                this.ownerContext.Activity.WorkflowCoreRuntime.SaveContextActivity(childContext.Activity);
            }

            // remove it from active contexts
            activeContexts.Remove(childContext.Activity.ContextActivity);
            this.executionContexts.Remove(childContext);

            //Case for those context which has compensatable child context, when those context
            //are completed at the end of Compensation chain we need to uninitialize the context
            //activity associated to them.
            if (childContext.Activity.ContextActivity.CanUninitializeNow && childContext.Activity.ContextActivity.ExecutionResult != ActivityExecutionResult.Uninitialized)
            {
                childContext.Activity.ContextActivity.Uninitialize(this.ownerContext.Activity.RootActivity.WorkflowCoreRuntime);
                childContext.Activity.ContextActivity.SetValue(Activity.ExecutionResultProperty, ActivityExecutionResult.Uninitialized);
            }

            // unregister it from runtime
            this.ownerContext.Activity.WorkflowCoreRuntime.UnregisterContextActivity(childContext.Activity);

            if (!(needsCompensation || forcePersist))
            {
                childContext.Activity.Dispose();
            }
        }

        public ActivityExecutionContext GetExecutionContext(Activity activity)
        {
            if (this.ownerContext == null)
                throw new ObjectDisposedException("ActivityExecutionContextManager");

            if (activity == null)
                throw new ArgumentNullException("activity");

            ActivityExecutionContextInfo contextInfo = activity.GetValue(Activity.ActivityExecutionContextInfoProperty) as ActivityExecutionContextInfo;

            // Returns the first context for an activity with the same qualified name.
            foreach (ActivityExecutionContext context in ExecutionContexts)
            {
                if (contextInfo == null) //Template being passed.
                {
                    if (context.Activity.ContextActivity.QualifiedName == activity.QualifiedName)
                        return context;
                }
                else //Context Sensitive Activity
                {
                    if (context.ContextGuid.Equals(contextInfo.ContextGuid))
                        return context;
                }
            }
            return null;
        }

        public IEnumerable<Guid> PersistedExecutionContexts
        {
            get
            {
                if (this.ownerContext == null)
#pragma warning suppress 56503
                    throw new ObjectDisposedException("ActivityExecutionContextManager");

                List<ActivityExecutionContextInfo> completedContexts = this.ownerContext.Activity.ContextActivity.GetValue(Activity.CompletedExecutionContextsProperty) as List<ActivityExecutionContextInfo>;
                completedContexts = (completedContexts == null) ? new List<ActivityExecutionContextInfo>() : completedContexts;

                List<Guid> persistedContexts = new List<Guid>();
                foreach (ActivityExecutionContextInfo contextInfo in completedContexts)
                    if ((contextInfo.Flags & PersistFlags.ForcePersist) != 0)
                        persistedContexts.Add(contextInfo.ContextGuid);

                return persistedContexts;
            }
        }

        public ActivityExecutionContext GetPersistedExecutionContext(Guid contextGuid)
        {
            if (this.ownerContext == null)
                throw new ObjectDisposedException("ActivityExecutionContextManager");

            // Check if child execution context exists.
            IList<ActivityExecutionContextInfo> completedContexts = this.ownerContext.Activity.ContextActivity.GetValue(Activity.CompletedExecutionContextsProperty) as IList<ActivityExecutionContextInfo>;
            if (completedContexts == null)
                throw new ArgumentException();

            ActivityExecutionContextInfo contextInfo = null;
            foreach (ActivityExecutionContextInfo completedContextInfo in completedContexts)
            {
                if (completedContextInfo.ContextGuid == contextGuid && ((completedContextInfo.Flags & PersistFlags.ForcePersist) != 0))
                {
                    contextInfo = completedContextInfo;
                    break;
                }
            }

            if (contextInfo == null)
                throw new ArgumentException();

            // The caller would have to close the AEC with forcepersist the next time
            // around.
            contextInfo.Flags &= ~PersistFlags.ForcePersist;
            return DiscardPersistedExecutionContext(contextInfo);
        }

        #endregion

        internal void Dispose()
        {
            if (this.ownerContext != null)
            {
                foreach (ActivityExecutionContext executionContext in this.ExecutionContexts)
                    ((IDisposable)executionContext).Dispose();
                this.ownerContext = null;
            }
        }

        #region Internal members

        internal ReadOnlyCollection<ActivityExecutionContextInfo> CompletedExecutionContexts
        {
            get
            {
                List<ActivityExecutionContextInfo> completedContexts = this.ownerContext.Activity.ContextActivity.GetValue(Activity.CompletedExecutionContextsProperty) as List<ActivityExecutionContextInfo>;
                completedContexts = (completedContexts == null) ? new List<ActivityExecutionContextInfo>() : completedContexts;
                return completedContexts.AsReadOnly();
            }
        }

        internal ActivityExecutionContext DiscardPersistedExecutionContext(ActivityExecutionContextInfo contextInfo)
        {
            if (contextInfo == null)
                throw new ArgumentNullException("contextInfo");

            // check if child execution context
            IList<ActivityExecutionContextInfo> completedContexts = this.ownerContext.Activity.ContextActivity.GetValue(Activity.CompletedExecutionContextsProperty) as IList<ActivityExecutionContextInfo>;
            if (completedContexts == null || !completedContexts.Contains(contextInfo))
                throw new ArgumentException();

            // revoke from persistence service
            Activity revokedActivity = this.ownerContext.WorkflowCoreRuntime.LoadContextActivity(contextInfo, this.ownerContext.Activity.ContextActivity.GetActivityByName(contextInfo.ActivityQualifiedName));
            ((IDependencyObjectAccessor)revokedActivity).InitializeInstanceForRuntime(this.ownerContext.Activity.WorkflowCoreRuntime);

            // add it back to active contexts
            IList<Activity> activeContexts = (IList<Activity>)this.ownerContext.Activity.ContextActivity.GetValue(Activity.ActiveExecutionContextsProperty);
            if (activeContexts == null)
            {
                activeContexts = new List<Activity>();
                this.ownerContext.Activity.ContextActivity.SetValue(Activity.ActiveExecutionContextsProperty, activeContexts);
            }
            activeContexts.Add(revokedActivity);

            // inform workflow runtime
            this.ownerContext.Activity.WorkflowCoreRuntime.RegisterContextActivity(revokedActivity);

            // return the new context
            ActivityExecutionContext revokedContext = new ActivityExecutionContext(revokedActivity);
            this.executionContexts.Add(revokedContext);
            System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Revoking context {0}:{1}", revokedContext.ContextId, revokedContext.Activity.ContextActivity.QualifiedName);

            // remove it from completed contexts
            completedContexts.Remove(contextInfo);
            return revokedContext;
        }

        #endregion
    }

    #region Class ActivityExecutionContextInfo

    [Serializable]
    [Flags]
    internal enum PersistFlags : byte
    {
        NeedsCompensation = 1,
        ForcePersist = 2
    }

    [Serializable]
    internal sealed class ActivityExecutionContextInfo
    {
        private string qualifiedID = string.Empty;
        private int contextId = -1;
        private Guid contextGuid = Guid.Empty; //
        private int parentContextId = -1;
        private int completedOrderId = -1;
        private PersistFlags flags = 0;

        internal ActivityExecutionContextInfo(string qualifiedName, int contextId, Guid contextGuid, int parentContextId)
        {
            this.qualifiedID = qualifiedName;
            this.contextId = contextId;
            this.contextGuid = contextGuid;
            this.parentContextId = parentContextId;
        }

        internal int ContextId
        {
            get
            {
                return this.contextId;
            }
        }

        public Guid ContextGuid
        {
            get
            {
                return this.contextGuid;
            }
        }

        public string ActivityQualifiedName
        {
            get
            {
                return this.qualifiedID;
            }
        }

        public int CompletedOrderId
        {
            get
            {
                return this.completedOrderId;
            }
        }

        internal int ParentContextId
        {
            get
            {
                return this.parentContextId;
            }
        }

        internal void SetCompletedOrderId(int completedOrderId)
        {
            this.completedOrderId = completedOrderId;
        }

        internal PersistFlags Flags
        {
            get
            {
                return this.flags;
            }

            set
            {
                this.flags = value;
            }
        }

        public override int GetHashCode()
        {
            return contextGuid.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            ActivityExecutionContextInfo otherContextInfo = obj as ActivityExecutionContextInfo;

            if (otherContextInfo != null)
            {
                return this.ContextGuid.Equals(otherContextInfo.ContextGuid);
            }
            return false;
        }
    }

    #endregion
}
