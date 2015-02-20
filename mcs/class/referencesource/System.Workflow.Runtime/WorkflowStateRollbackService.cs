using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Transactions;
using System.Workflow.ComponentModel;

namespace System.Workflow.Runtime
{
    internal sealed class WorkflowStateRollbackService
    {
        WorkflowExecutor workflowExecutor;

        // cache the revert back data
        MemoryStream clonedInstanceStateStream;
        Activity workflowDefinition = null;
        bool isInstanceStateRevertRequested = false;

        // revert back notification info
        string activityQualifiedName;
        int activityContextId;
        EventArgs callbackData;
        EventHandler<EventArgs> callbackHandler;
        bool suspendOnRevert;
        string suspendOnRevertInfo;

        Hashtable completedContextActivities = new Hashtable();

        public WorkflowStateRollbackService(WorkflowExecutor workflowExecutor)
        {
            this.workflowExecutor = workflowExecutor;
        }

        internal bool IsInstanceStateRevertRequested
        {
            get { return this.isInstanceStateRevertRequested; }
        }

        internal void CheckpointInstanceState()
        {
            Debug.Assert(this.workflowExecutor.InstanceId != null, "instance id null at checkpoint time");

            // serialize the instance state
            this.clonedInstanceStateStream = new MemoryStream(10240);
            this.workflowExecutor.RootActivity.Save(this.clonedInstanceStateStream);
            this.workflowDefinition = this.workflowExecutor.WorkflowDefinition;
            this.completedContextActivities = (Hashtable)this.workflowExecutor.CompletedContextActivities.Clone();
            this.clonedInstanceStateStream.Position = 0;
        }

        internal void RequestRevertToCheckpointState(Activity currentActivity, EventHandler<EventArgs> callbackHandler, EventArgs callbackData, bool suspendOnRevert, string suspendInfo)
        {
            if (this.clonedInstanceStateStream == null)
                throw new InvalidOperationException(ExecutionStringManager.InvalidRevertRequest);

            // cache the after revert information
            this.activityContextId = ContextActivityUtils.ContextId(ContextActivityUtils.ContextActivity(currentActivity));
            this.activityQualifiedName = currentActivity.QualifiedName;
            this.callbackData = callbackData;
            this.callbackHandler = callbackHandler;
            this.suspendOnRevert = suspendOnRevert;
            this.suspendOnRevertInfo = suspendInfo;

            // ask scheduler to stop
            this.isInstanceStateRevertRequested = true;
            this.workflowExecutor.Scheduler.CanRun = false;
        }

        internal void DisposeCheckpointState()
        {
            this.clonedInstanceStateStream = null;
        }

        internal void RevertToCheckpointState()
        {
            Debug.Assert(this.clonedInstanceStateStream != null, "cloned instance-state stream null at restore time");

            // deserialize only on first access
            Activity clonedRootActivity = null;
            this.clonedInstanceStateStream.Position = 0;
            using (RuntimeEnvironment runtimeEnv = new RuntimeEnvironment(this.workflowExecutor.WorkflowRuntime))
            {
                clonedRootActivity = Activity.Load(this.clonedInstanceStateStream, (Activity)this.workflowDefinition);
            }
            Debug.Assert(clonedRootActivity != null);
            //
            // Set the trackingListenerBroker before initializing the executor so the tracking
            // runtime gets a reference to the correct object
            clonedRootActivity.SetValue(WorkflowExecutor.TrackingListenerBrokerProperty, workflowExecutor.RootActivity.GetValue(WorkflowExecutor.TrackingListenerBrokerProperty));

            // create the new workflowExecutor
            WorkflowExecutor newWorkflowExecutor = new WorkflowExecutor(Guid.Empty);    // use a dummy guid while swapping executors
            newWorkflowExecutor.Initialize(clonedRootActivity, this.workflowExecutor.WorkflowRuntime, this.workflowExecutor);

            // enqueue the activity notifier
            Activity activityContext = newWorkflowExecutor.GetContextActivityForId(this.activityContextId);
            Activity activity = activityContext.GetActivityByName(this.activityQualifiedName);
            using (new ServiceEnvironment(activity))
            {
                using (newWorkflowExecutor.SetCurrentActivity(activity))
                {
                    using (ActivityExecutionContext executionContext = new ActivityExecutionContext(activity))
                        executionContext.Invoke<EventArgs>(this.callbackHandler, this.callbackData);
                }
            }
            //
            // Push the batch item ordering id to the new instance
            newWorkflowExecutor.BatchCollection.WorkItemOrderId = this.workflowExecutor.BatchCollection.WorkItemOrderId;
            // replace pending batch items
            foreach (KeyValuePair<object, WorkBatch> batch in this.workflowExecutor.BatchCollection)
            {
                batch.Value.SetWorkBatchCollection(newWorkflowExecutor.BatchCollection);
                Activity oldActivity = batch.Key as Activity;
                // no need to add the transient state batch
                if (oldActivity != null)
                {
                    Activity newactivity = activityContext.GetActivityByName(oldActivity.QualifiedName);
                    newWorkflowExecutor.BatchCollection.Add(newactivity, batch.Value);
                }
            }
            this.workflowExecutor.BatchCollection.Clear();

            Debug.Assert(this.completedContextActivities != null);
            newWorkflowExecutor.CompletedContextActivities = this.completedContextActivities;

            // replace with the WorkflowRuntime
            Debug.Assert(this.workflowExecutor.IsInstanceValid);
            this.workflowExecutor.WorkflowRuntime.ReplaceWorkflowExecutor(this.workflowExecutor.InstanceId, this.workflowExecutor, newWorkflowExecutor);

            // now resume or suspend the scheduler as needed
            if (!this.suspendOnRevert)
            {
                // get the new one going
                newWorkflowExecutor.Scheduler.Resume();
            }
            else
            {
                // this call will be old scheduler's thread
                newWorkflowExecutor.SuspendOnIdle(this.suspendOnRevertInfo);
            }
            DisposeCheckpointState();
        }
    }
}
