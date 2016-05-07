// ****************************************************************************
// Copyright (C) Microsoft Corporation.  All rights reserved.
//

using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Workflow.Runtime;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime.Hosting;

namespace System.Workflow.Runtime
{
    /// <summary>
    /// Schedule Instance handed over to the client
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class WorkflowInstance
    {
        private WorkflowRuntime _runtime;
        private Guid _instanceId;
        private WorkflowExecutor _deadWorkflow;

        internal WorkflowInstance(Guid instanceId, WorkflowRuntime workflowRuntime)
        {
            if (instanceId == Guid.Empty)
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.CantBeEmptyGuid, "instanceId"));
            if (workflowRuntime == null)
                throw new ArgumentNullException("workflowRuntime");

            this._instanceId = instanceId;
            this._runtime = workflowRuntime;
        }

        public Guid InstanceId
        {
            get
            {
                return _instanceId;
            }
        }

        public WorkflowRuntime WorkflowRuntime
        {
            get
            {
                return _runtime;
            }
        }

        internal WorkflowExecutor DeadWorkflow
        {
            set
            {
                Debug.Assert(value.WorkflowStatus == WorkflowStatus.Completed || value.WorkflowStatus == WorkflowStatus.Terminated,
                    "Dead workflow is not dead.");
                _deadWorkflow = value;
            }
        }

        public ReadOnlyCollection<WorkflowQueueInfo> GetWorkflowQueueData()
        {
            if (_deadWorkflow != null)
                return _deadWorkflow.GetWorkflowQueueInfos();

            while (true)
            {
                WorkflowExecutor executor = _runtime.Load(this);
                if (executor.IsInstanceValid)
                {
                    try
                    {
                        return executor.GetWorkflowQueueInfos();
                    }
                    catch (InvalidOperationException)
                    {
                        if (executor.IsInstanceValid)
                            throw;
                    }
                }
            }
        }

        public DateTime GetWorkflowNextTimerExpiration()
        {
            while (true)
            {
                WorkflowExecutor executor = _runtime.Load(this);
                if (executor.IsInstanceValid)
                {
                    try
                    {
                        return executor.GetWorkflowNextTimerExpiration();
                    }
                    catch (InvalidOperationException)
                    {
                        if (executor.IsInstanceValid)
                            throw;
                    }
                }
            }
        }

        public Activity GetWorkflowDefinition()
        {
            while (true)
            {
                WorkflowExecutor executor = _runtime.Load(this);
                if (executor.IsInstanceValid)
                {
                    try
                    {
                        // Make sure to get the clone here since the
                        // definition is mutable and shared across all
                        // instances.
                        return executor.GetWorkflowDefinitionClone("");
                    }
                    catch (InvalidOperationException)
                    {
                        if (executor.IsInstanceValid)
                            throw;
                    }
                }
            }
        }

        public void Load()
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                this._runtime.Load(this);
            }
        }

        public bool TryUnload()
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                WorkflowExecutor executor = _runtime.Load(this);
                using (executor.ExecutorLock.Enter())
                {
                    if (executor.IsInstanceValid)
                    {
                        try
                        {
                            return executor.TryUnload();
                        }
                        catch (InvalidOperationException)
                        {
                            if (executor.IsInstanceValid)
                                throw;
                        }
                    }
                }
                return false;
            }
        }

        public void Suspend(string error)
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                while (true)
                {
                    WorkflowExecutor executor = _runtime.Load(this);
                    if (executor.IsInstanceValid)
                    {
                        if (executor.WorkflowStatus == WorkflowStatus.Created)
                            throw new InvalidOperationException(ExecutionStringManager.CannotSuspendBeforeStart);
                        try
                        {
                            executor.Suspend(error);
                            return;
                        }
                        catch (InvalidOperationException)
                        {
                            if (executor.IsInstanceValid)
                                throw;
                        }
                        catch (ExecutorLocksHeldException e)
                        {
                            try
                            {
                                e.Handle.WaitOne();
                            }
                            catch (ObjectDisposedException)
                            {
                                // If an ObjectDisposedException is thrown because
                                // the WaitHandle has already closed, nothing to worry
                                // about. Move on.
                            }
                        }
                    }
                }
            }
        }

        public void Unload()
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                if (_runtime == null || _runtime.GetService<WorkflowPersistenceService>() == null)
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.MissingPersistenceService, this.InstanceId));
                while (true)
                {
                    WorkflowExecutor executor = _runtime.Load(this);
                    if (executor.IsInstanceValid)
                    {
                        try
                        {
                            executor.Unload();
                            return;
                        }
                        catch (InvalidOperationException)
                        {
                            if (executor.IsInstanceValid)
                                throw;
                        }
                        catch (ExecutorLocksHeldException e)
                        {
                            try
                            {
                                e.Handle.WaitOne(/* maybe should have a timeout here?*/);
                            }
                            catch (ObjectDisposedException)
                            {
                                // If an ObjectDisposedException is thrown because
                                // the WaitHandle has already closed, nothing to worry
                                // about. Move on.
                            }
                        }
                    }
                }
            }
        }

        public void Resume()
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                while (true)
                {
                    WorkflowExecutor executor = _runtime.Load(this);
                    if (executor.IsInstanceValid)
                    {
                        try
                        {
                            executor.Resume();
                            break;
                        }
                        catch (InvalidOperationException)
                        {
                            if (executor.IsInstanceValid)
                                throw;
                        }
                    }
                }
            }
        }

        internal void ProcessTimers(object ignored)
        {
            ProcessTimers();
        }

        internal void ProcessTimers()
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                while (true)
                {
                    WorkflowExecutor executor = null;
                    try
                    {
                        executor = _runtime.Load(this);
                    }
                    catch (InvalidOperationException)
                    {
                        break;
                    }
                    if (executor != null && executor.IsInstanceValid)
                    {
                        try
                        {
                            executor.DeliverTimerSubscriptions();
                            break;
                        }
                        catch (InvalidOperationException)
                        {
                            if (executor.IsInstanceValid)
                                throw;
                        }
                    }
                }
            }
        }

        public void Terminate(string error)
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                while (true)
                {
                    WorkflowExecutor executor = _runtime.Load(this);
                    if (executor.IsInstanceValid)
                    {
                        try
                        {
                            executor.Terminate(error);
                            break;
                        }
                        catch (InvalidOperationException)
                        {
                            if (executor.IsInstanceValid)
                                throw;
                        }
                    }
                }
            }
        }

        public void Abort()
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                while (true)
                {
                    WorkflowExecutor executor = _runtime.Load(this);
                    if (executor.IsInstanceValid)
                    {
                        if (executor.WorkflowStatus == WorkflowStatus.Created)
                            throw new InvalidOperationException(ExecutionStringManager.CannotAbortBeforeStart);

                        try
                        {
                            executor.Abort();
                            break;
                        }
                        catch (InvalidOperationException)
                        {
                            if (executor.IsInstanceValid)
                                throw;
                        }
                    }
                }
            }
        }

        public void ReloadTrackingProfiles()
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                while (true)
                {
                    WorkflowExecutor executor = _runtime.Load(this);
                    if (executor.IsInstanceValid)
                    {
                        try
                        {
                            _runtime.TrackingListenerFactory.ReloadProfiles(executor);
                            break;
                        }
                        catch (InvalidOperationException)
                        {
                            if (executor.IsInstanceValid)
                                throw;
                        }
                    }
                }
            }
        }

        public void ApplyWorkflowChanges(WorkflowChanges workflowChanges)
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                while (true)
                {
                    WorkflowExecutor executor = _runtime.Load(this);
                    if (executor.IsInstanceValid)
                    {
                        try
                        {
                            executor.ApplyWorkflowChanges(workflowChanges);
                            break;
                        }
                        catch (InvalidOperationException)
                        {
                            if (executor.IsInstanceValid)
                                throw;
                        }
                    }
                }
            }
        }

        public void EnqueueItem(IComparable queueName, Object item, IPendingWork pendingWork, Object workItem)
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                while (true)
                {
                    WorkflowExecutor executor = _runtime.Load(this);
                    try
                    {
                        executor.EnqueueItem(queueName, item, pendingWork, workItem);
                        break;
                    }
                    catch (InvalidOperationException)
                    {
                        if (executor.IsInstanceValid)
                            throw;
                    }
                }
            }
        }

        public void EnqueueItemOnIdle(IComparable queueName, Object item, IPendingWork pendingWork, Object workItem)
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                while (true)
                {
                    WorkflowExecutor executor = _runtime.Load(this);
                    if (executor.IsInstanceValid)
                    {
                        try
                        {
                            executor.EnqueueItemOnIdle(queueName, item, pendingWork, workItem);
                            break;
                        }
                        catch (InvalidOperationException)
                        {
                            if (executor.IsInstanceValid)
                                throw;
                        }
                    }
                }
            }
        }

        internal WorkflowExecutor GetWorkflowResourceUNSAFE()
        {
            while (true)
            {
                WorkflowExecutor executor = _runtime.Load(this);
                if (executor.IsInstanceValid)
                {
                    try
                    {
                        return executor;
                    }
                    catch (InvalidOperationException)
                    {
                        if (executor.IsInstanceValid)
                            throw;
                    }
                }
            }
        }

        public override bool Equals(Object obj)
        {
            WorkflowInstance instance = obj as WorkflowInstance;
            if (instance == null)
                return false;

            return this._instanceId == instance._instanceId;
        }

        public override int GetHashCode()
        {
            return this._instanceId.GetHashCode();
        }

        public void Start()
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                while (true)
                {
                    WorkflowExecutor executor = _runtime.Load(this);
                    if (executor.IsInstanceValid)
                    {
                        try
                        {
                            executor.Start();
                            break;
                        }
                        catch (InvalidOperationException)
                        {
                            if (executor.IsInstanceValid)
                                throw;
                        }
                    }
                }
            }
        }
    }
}
