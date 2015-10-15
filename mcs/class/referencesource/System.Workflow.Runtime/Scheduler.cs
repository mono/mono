using System;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Transactions;
using System.Workflow.ComponentModel;

namespace System.Workflow.Runtime
{
    #region Scheduler

    // Only one instance of this type is used for a workflow instance.
    //
    class Scheduler
    {
        #region data

        // state to be persisted for the scheduler
        internal static DependencyProperty HighPriorityEntriesQueueProperty = DependencyProperty.RegisterAttached("HighPriorityEntriesQueue", typeof(Queue<SchedulableItem>), typeof(Scheduler));
        internal static DependencyProperty NormalPriorityEntriesQueueProperty = DependencyProperty.RegisterAttached("NormalPriorityEntriesQueue", typeof(Queue<SchedulableItem>), typeof(Scheduler));
        Queue<SchedulableItem> highPriorityEntriesQueue;
        Queue<SchedulableItem> normalPriorityEntriesQueue;

        // non-persisted state for the scheduler
        WorkflowExecutor rootWorkflowExecutor;
        bool empty;
        bool canRun;
        bool threadRequested;
        bool abortOrTerminateRequested;
        Queue<SchedulableItem> transactedEntries;
        object syncObject = new object();

        #endregion data

        #region ctors

        // loading with some state
        public Scheduler(WorkflowExecutor rootExec, bool canRun)
        {
            this.rootWorkflowExecutor = rootExec;
            this.threadRequested = false;

            // canRun is true if normal creation
            // false if loading from a persisted state. Will be set to true later at ResumeOnIdle
            this.canRun = canRun;

            this.highPriorityEntriesQueue = (Queue<SchedulableItem>)rootExec.RootActivity.GetValue(Scheduler.HighPriorityEntriesQueueProperty);
            this.normalPriorityEntriesQueue = (Queue<SchedulableItem>)rootExec.RootActivity.GetValue(Scheduler.NormalPriorityEntriesQueueProperty);
            if (this.highPriorityEntriesQueue == null)
            {
                this.highPriorityEntriesQueue = new Queue<SchedulableItem>();
                rootExec.RootActivity.SetValue(Scheduler.HighPriorityEntriesQueueProperty, this.highPriorityEntriesQueue);
            }
            if (this.normalPriorityEntriesQueue == null)
            {
                this.normalPriorityEntriesQueue = new Queue<SchedulableItem>();
                rootExec.RootActivity.SetValue(Scheduler.NormalPriorityEntriesQueueProperty, this.normalPriorityEntriesQueue);
            }

            this.empty = ((this.normalPriorityEntriesQueue.Count == 0) && (this.highPriorityEntriesQueue.Count == 0));
        }

        #endregion ctors

        #region Misc properties

        public override string ToString()
        {
            return "Scheduler('" + ((Activity)this.RootWorkflowExecutor.WorkflowDefinition).QualifiedName + "')";
        }

        protected WorkflowExecutor RootWorkflowExecutor
        {
            get { return this.rootWorkflowExecutor; }
        }

        public bool IsStalledNow
        {
            get
            {
                return empty;
            }
        }

        public bool CanRun
        {
            get
            {
                return canRun;
            }

            set
            {
                canRun = value;
            }
        }

        internal bool AbortOrTerminateRequested
        {
            get
            {
                return abortOrTerminateRequested;
            }
            set
            {
                abortOrTerminateRequested = value;
            }
        }

        #endregion Misc properties

        #region Run work

        public void Run()
        {
            do
            {
                this.RootWorkflowExecutor.ProcessQueuedEvents();
                // Get item to run
                SchedulableItem item = GetItemToRun();
                bool runningItem = false;

                // no ready work to run... go away
                if (item == null)
                    break;

                Activity itemActivity = null;
                Exception exp = null;

                TransactionalProperties transactionalProperties = null;
                int contextId = item.ContextId;

                // This function gets the root or enclosing while-loop activity
                Activity contextActivity = this.RootWorkflowExecutor.GetContextActivityForId(contextId);
                if (contextActivity == null)
                    throw new InvalidOperationException(ExecutionStringManager.InvalidExecutionContext);

                // This is the activity corresponding to the item's ActivityId
                itemActivity = contextActivity.GetActivityByName(item.ActivityId);
                using (new ServiceEnvironment(itemActivity))
                {
                    exp = null;
                    bool ignoreFinallyBlock = false;

                    try
                    {
                        // item preamble 
                        // set up the item transactional context if necessary
                        //
                        Debug.Assert(itemActivity != null, "null itemActivity");
                        if (itemActivity == null)
                            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InvalidActivityName, item.ActivityId));

                        Activity atomicActivity = null;
                        if (this.RootWorkflowExecutor.IsActivityInAtomicContext(itemActivity, out atomicActivity))
                        {
                            transactionalProperties = (TransactionalProperties)atomicActivity.GetValue(WorkflowExecutor.TransactionalPropertiesProperty);
                            // If we've aborted for any reason stop now!
                            // If we attempt to enter a new TransactionScope the com+ context will get corrupted
                            // See windows se 
                            if (!WorkflowExecutor.CheckAndProcessTransactionAborted(transactionalProperties))
                            {
                                if (transactionalProperties.TransactionScope == null)
                                {
                                    // Use TimeSpan.Zero so scope will not create timeout independent of the transaction
                                    // Use EnterpriseServicesInteropOption.Full to flow transaction to COM+
                                    transactionalProperties.TransactionScope =
                                        new TransactionScope(transactionalProperties.Transaction, TimeSpan.Zero, EnterpriseServicesInteropOption.Full);

                                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0,
                                        "Workflow Runtime: Scheduler: instanceId: " + this.RootWorkflowExecutor.InstanceIdString +
                                        "Entered into TransactionScope, Current atomic acitivity " + atomicActivity.Name);
                                }
                            }
                        }

                        // Run the item
                        //
                        runningItem = true;
                        WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 1, "Workflow Runtime: Scheduler: InstanceId: {0} : Running scheduled entry: {1}", this.RootWorkflowExecutor.InstanceIdString, item.ToString());

                        // running any entry implicitly changes some state of the workflow instance                    
                        this.RootWorkflowExecutor.stateChangedSincePersistence = true;

                        item.Run(this.RootWorkflowExecutor);
                    }
                    catch (Exception e)
                    {
                        if (WorkflowExecutor.IsIrrecoverableException(e))
                        {
                            ignoreFinallyBlock = true;
                            throw;
                        }
                        else
                        {
                            if (transactionalProperties != null)
                                transactionalProperties.TransactionState = TransactionProcessState.AbortProcessed;
                            exp = e;
                        }
                    }
                    finally
                    {
                        if (!ignoreFinallyBlock)
                        {
                            if (runningItem)
                                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 1, "Workflow Runtime: Scheduler: InstanceId: {0} : Done with running scheduled entry: {1}", this.RootWorkflowExecutor.InstanceIdString, item.ToString());

                            // Process exception
                            //
                            if (exp != null)
                            {
                                // 
                                this.RootWorkflowExecutor.ExceptionOccured(exp, itemActivity == null ? contextActivity : itemActivity, null);
                                exp = null;
                            }
                        }
                    }
                }
            } while (true);
        }

        private SchedulableItem GetItemToRun()
        {
            SchedulableItem ret = null;

            lock (this.syncObject)
            {
                bool workToDo = false;
                if ((this.highPriorityEntriesQueue.Count > 0) || (this.normalPriorityEntriesQueue.Count > 0))
                {
                    workToDo = true;

                    // If an abort or termination of the workflow has been requested,
                    // then the workflow should try to terminate ASAP. Even transaction scopes
                    // in progress shouldn't be executed to completion. (Ref: 16534)
                    if (this.AbortOrTerminateRequested)
                    {
                        ret = null;
                    }
                    // got work to do in the scheduler
                    else if ((this.highPriorityEntriesQueue.Count > 0))
                    {
                        ret = this.highPriorityEntriesQueue.Dequeue();
                    }
                    else if (this.CanRun)
                    {
                        // the scheduler can run right now
                        //

                        // pick an entry to run
                        //
                        if (((IWorkflowCoreRuntime)this.RootWorkflowExecutor).CurrentAtomicActivity == null &&
                            (this.normalPriorityEntriesQueue.Count > 0))
                            ret = this.normalPriorityEntriesQueue.Dequeue();
                    }
                    else
                    {
                        // scheduler can't run right now.. even though there is ready work
                        // do nothing in the scheduler
                        ret = null;
                    }
                }

                if (!workToDo)
                {
                    // no ready work to do in the scheduler...
                    // we are gonna return the thread back
                    this.empty = true;
                }

                // set it to true only iff there is something to run
                this.threadRequested = (ret != null);
            }
            return ret;
        }

        // This method should be called only after we have determined that
        // this instance can start running now
        public void Resume()
        {
            canRun = true;

            if (!empty)
            {
                // There is scheduled work
                // ask the threadprovider for a thread
                this.RootWorkflowExecutor.ScheduleForWork();
            }
        }

        // This method should be called only after we have determined that
        // this instance can start running now
        public void ResumeIfRunnable()
        {
            if (!canRun)
                return;

            if (!empty)
            {
                // There is scheduled work
                // ask the threadprovider for a thread
                this.RootWorkflowExecutor.ScheduleForWork();
            }
        }
        #endregion Run work

        #region Schedule work

        public void ScheduleItem(SchedulableItem s, bool isInAtomicTransaction, bool transacted)
        {
            lock (this.syncObject)
            {
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 1, "Workflow Runtime: Scheduler: InstanceId: {0} : Scheduling entry: {1}", this.RootWorkflowExecutor.InstanceIdString, s.ToString());
                // SchedulableItems in AtomicTransaction has higher priority
                Queue<SchedulableItem> q = isInAtomicTransaction ? this.highPriorityEntriesQueue : this.normalPriorityEntriesQueue;
                q.Enqueue(s);

                if (transacted)
                {
                    if (transactedEntries == null)
                        transactedEntries = new Queue<SchedulableItem>();
                    transactedEntries.Enqueue(s);
                }

                if (!this.threadRequested)
                {
                    if (this.CanRun)
                    {
                        this.RootWorkflowExecutor.ScheduleForWork();
                        this.threadRequested = true;
                    }
                }
                this.empty = false;
            }
        }

        #endregion Schedule work

        #region psuedo-transacted support

        public void PostPersist()
        {
            transactedEntries = null;
        }

        public void Rollback()
        {
            if (transactedEntries != null && transactedEntries.Count > 0)
            {
                // make a list of non-transacted entries
                // @undone: bmalhi: transacted entries only on priority-0

                IEnumerator<SchedulableItem> e = this.normalPriorityEntriesQueue.GetEnumerator();
                Queue<SchedulableItem> newScheduled = new Queue<SchedulableItem>();
                while (e.MoveNext())
                {
                    if (!transactedEntries.Contains(e.Current))
                        newScheduled.Enqueue(e.Current);
                }

                // clear the scheduled items
                this.normalPriorityEntriesQueue.Clear();

                // schedule the non-transacted items back
                e = newScheduled.GetEnumerator();
                while (e.MoveNext())
                    this.normalPriorityEntriesQueue.Enqueue(e.Current);

                transactedEntries = null;
            }
        }

        #endregion psuedo-transacted support
    }

    #endregion Scheduler
}
