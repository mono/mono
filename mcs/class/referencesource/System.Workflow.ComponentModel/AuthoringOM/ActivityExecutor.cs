using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Workflow.ComponentModel
{
    #region ActivityExecutor

    internal abstract class ActivityExecutor
    {
        public abstract ActivityExecutionStatus Execute(Activity activity, ActivityExecutionContext executionContext);
        public abstract ActivityExecutionStatus Cancel(Activity activity, ActivityExecutionContext executionContext);
        public abstract ActivityExecutionStatus HandleFault(Activity activity, ActivityExecutionContext executionContext, Exception exception);
        public abstract ActivityExecutionStatus Compensate(Activity activity, ActivityExecutionContext executionContext);
    }

    internal class ActivityExecutor<T> : ActivityExecutor where T : Activity
    {
        #region System.Workflow.ComponentModel.ActivityExecutor Methods

        public sealed override ActivityExecutionStatus Execute(Activity activity, ActivityExecutionContext executionContext)
        {
            return this.Execute((T)activity, executionContext);
        }
        public sealed override ActivityExecutionStatus Cancel(Activity activity, ActivityExecutionContext executionContext)
        {
            return this.Cancel((T)activity, executionContext);
        }
        public sealed override ActivityExecutionStatus HandleFault(Activity activity, ActivityExecutionContext executionContext, Exception exception)
        {
            return this.HandleFault((T)activity, executionContext, exception);
        }
        public sealed override ActivityExecutionStatus Compensate(Activity activity, ActivityExecutionContext executionContext)
        {
            return this.Compensate((T)activity, executionContext);
        }
        #endregion

        #region ActivityExecutor<T> Members
        protected virtual ActivityExecutionStatus Execute(T activity, ActivityExecutionContext executionContext)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            return activity.Execute(executionContext);
        }
        protected virtual ActivityExecutionStatus Cancel(T activity, ActivityExecutionContext executionContext)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            return activity.Cancel(executionContext);
        }
        protected virtual ActivityExecutionStatus HandleFault(T activity, ActivityExecutionContext executionContext, Exception exception)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            return activity.HandleFault(executionContext, exception);
        }
        protected virtual ActivityExecutionStatus Compensate(T activity, ActivityExecutionContext executionContext)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            System.Diagnostics.Debug.Assert(activity is ICompensatableActivity, "should not get Compensate, if activity is not compensatable");
            return ((ICompensatableActivity)activity).Compensate(executionContext);
        }

        #endregion ActivityExecutor
    }
    #endregion

    #region CompositeActivityExecutor<T>
    internal class CompositeActivityExecutor<T> : ActivityExecutor<T>, ISupportWorkflowChanges where T : CompositeActivity
    {
        //@@undone:mayankm Once all ActivityExecutor is removed this method should not be virtual.
        void ISupportWorkflowChanges.OnActivityAdded(ActivityExecutionContext executionContext, Activity addedActivity)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");
            if (addedActivity == null)
                throw new ArgumentNullException("addedActivity");

            CompositeActivity compositeActivity = executionContext.Activity as CompositeActivity;
            if (compositeActivity == null)
                throw new ArgumentException(SR.Error_InvalidActivityExecutionContext, "executionContext");

            compositeActivity.OnActivityChangeAdd(executionContext, addedActivity);
        }

        void ISupportWorkflowChanges.OnActivityRemoved(ActivityExecutionContext executionContext, Activity removedActivity)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");
            if (removedActivity == null)
                throw new ArgumentNullException("removedActivity");

            CompositeActivity compositeActivity = executionContext.Activity as CompositeActivity;
            if (compositeActivity == null)
                throw new ArgumentException(SR.Error_InvalidActivityExecutionContext, "executionContext");

            compositeActivity.OnActivityChangeRemove(executionContext, removedActivity);
        }

        void ISupportWorkflowChanges.OnWorkflowChangesCompleted(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            CompositeActivity compositeActivity = executionContext.Activity as CompositeActivity;

            if (compositeActivity == null)
                throw new ArgumentException(SR.Error_InvalidActivityExecutionContext, "executionContext");

            compositeActivity.OnWorkflowChangesCompleted(executionContext);
        }

        // Refer Bug 9339 (VB Compilation Failure - Unable to load one or more of the requested types. Retrieve the LoaderExceptions property for more information.)
        //An unhandled exception of type 'System.TypeLoadException' occurred
        // "Signature of the body and declaration in a method implementation do not match"
        protected override ActivityExecutionStatus Execute(T activity, ActivityExecutionContext executionContext)
        {
            return base.Execute(activity, executionContext);
        }

        protected override ActivityExecutionStatus Cancel(T activity, ActivityExecutionContext executionContext)
        {
            return base.Cancel(activity, executionContext);
        }
    }
    #endregion

    #region ActivityExecutors Class

    internal static class ActivityExecutors
    {
        private static Hashtable typeToExecutorMapping = new Hashtable();
        private static Hashtable executors = new Hashtable();

        internal static ActivityExecutor[] GetActivityExecutors(Activity activity)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");

            Type activityType = activity.GetType();
            ActivityExecutor[] activityExecutors = executors[activityType] as ActivityExecutor[];
            if (activityExecutors != null)
                return activityExecutors;

            lock (executors.SyncRoot)
            {
                activityExecutors = executors[activityType] as ActivityExecutor[];
                if (activityExecutors != null)
                    return activityExecutors;

                object[] activityExecutorsObjects = null;
                try
                {
                    //activityExecutorsObjects = ComponentDispenser.CreateComponents(activityType, typeof(ActivityExecutorAttribute));
                    activityExecutorsObjects = ComponentDispenser.CreateActivityExecutors(activity);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(SR.GetString(SR.ExecutorCreationFailedErrorMessage, activityType.FullName), e);
                }

                if (activityExecutorsObjects == null || activityExecutorsObjects.Length == 0)
                    throw new InvalidOperationException(SR.GetString(SR.ExecutorCreationFailedErrorMessage, activityType.FullName));

                activityExecutors = new ActivityExecutor[activityExecutorsObjects.Length];
                for (int index = 0; index < activityExecutorsObjects.Length; index++)
                {
                    if (!typeToExecutorMapping.Contains(activityExecutorsObjects[index].GetType()))
                    {
                        lock (typeToExecutorMapping.SyncRoot)
                        {
                            if (!typeToExecutorMapping.Contains(activityExecutorsObjects[index].GetType()))
                            {
                                System.Threading.Thread.MemoryBarrier();
                                typeToExecutorMapping[activityExecutorsObjects[index].GetType()] = activityExecutorsObjects[index];
                            }
                        }
                    }
                    activityExecutors[index] = (ActivityExecutor)typeToExecutorMapping[activityExecutorsObjects[index].GetType()];
                }

                System.Threading.Thread.MemoryBarrier();
                executors[activityType] = activityExecutors;
            }
            return activityExecutors;
        }

        public static ActivityExecutor GetActivityExecutorFromType(Type executorType)
        {
            if (executorType == null)
                throw new ArgumentNullException("executorType");
            if (!typeof(ActivityExecutor).IsAssignableFrom(executorType))
                throw new ArgumentException(
                    SR.GetString(SR.Error_NonActivityExecutor, executorType.FullName), "executorType");

            ActivityExecutor activityExecutor = typeToExecutorMapping[executorType] as ActivityExecutor;
            if (activityExecutor != null)
                return activityExecutor;

            lock (typeToExecutorMapping.SyncRoot)
            {
                activityExecutor = typeToExecutorMapping[executorType] as ActivityExecutor;
                if (activityExecutor != null)
                    return activityExecutor;

                System.Threading.Thread.MemoryBarrier();
                typeToExecutorMapping[executorType] = Activator.CreateInstance(executorType);
            }
            return (ActivityExecutor)typeToExecutorMapping[executorType];
        }

        public static ActivityExecutor GetActivityExecutor(Activity activity)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");

            return GetActivityExecutors(activity)[0];
        }
    }
    #endregion
}
