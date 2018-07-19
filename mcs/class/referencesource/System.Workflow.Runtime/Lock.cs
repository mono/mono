// ****************************************************************************
// Copyright (C)  Microsoft Corporation.  All rights reserved.
//
// CONTENTS
//     Value-add wrapper on top of standard CLR monitor
// 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Transactions;

using System.Workflow.Runtime.Hosting;

namespace System.Workflow.Runtime
{
    internal static class LockFactory
    {
        internal static InstanceLock CreateWorkflowExecutorLock(Guid id)
        {
            return new InstanceLock(id, "Workflow Executor Lock: " + id.ToString(), 50, LockPriorityOperator.GreaterThanOrReentrant);
        }

        internal static InstanceLock CreateWorkflowSchedulerLock(Guid id)
        {
            return new InstanceLock(id, "Workflow Scheduler Lock: " + id.ToString(), 40, LockPriorityOperator.GreaterThan);
        }

        internal static InstanceLock CreateWorkflowMessageDeliveryLock(Guid id)
        {
            return new InstanceLock(id, "Workflow Message Delivery Lock: " + id.ToString(), 35, LockPriorityOperator.GreaterThanOrReentrant);
        }
    }

    internal enum LockPriorityOperator
    {
        GreaterThan,
        GreaterThanOrReentrant,
    }

    internal sealed class InstanceLock 
    {
        #region Static Data/Methods

        [ThreadStaticAttribute()]
        private static List<InstanceLock> t_heldLocks = null;

        [Conditional("DEBUG")]
        internal static void AssertNoLocksHeld()
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(HeldLocks.Count == 0, "No locks should be held.");
#endif
        }

        [Conditional("DEBUG")]
        internal static void AssertIsLocked(InstanceLock theLock)
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(HeldLocks.Contains(theLock), "Lock should be held.");
#endif
        }

        private static List<InstanceLock> HeldLocks
        {
            get
            {
                List<InstanceLock> tLocks = InstanceLock.t_heldLocks;
                if (tLocks == null)
                {
                    InstanceLock.t_heldLocks = new List<InstanceLock>();
                    tLocks = InstanceLock.t_heldLocks;
                }
                return tLocks;
            }
        }

        #endregion Static Data/Methods

        private Guid m_instanceId;
        private String m_name;
        private int m_priority;
        private LockPriorityOperator m_operator;

        internal int Priority
        {
            get
            {
                return this.m_priority;
            }
        }

        internal LockPriorityOperator Operator
        {
            get
            {
                return this.m_operator;
            }
        }

        internal InstanceLock(Guid id, String name, int priority, LockPriorityOperator lockOperator)
        {
            this.m_instanceId = id;
            this.m_name = name;
            this.m_priority = priority;
            this.m_operator = lockOperator;
        }

        internal Guid InstanceId
        {
            get
            {
                return this.m_instanceId;
            }
        }

        internal InstanceLockGuard Enter()
        {
            return new InstanceLockGuard(this);
        }

        internal bool TryEnter()
        {
            InstanceLockGuard.EnforceGuard(this);

            bool lockHeld = false;
            bool success = false;
            try
            {
                Monitor.TryEnter(this, ref lockHeld);

                if (lockHeld)
                {
                    HeldLocks.Add(this);
                    success = true;
                }
            }
            finally
            {
                if (lockHeld && !success)
                {
                    Monitor.Exit(this);
                }
            }
            return success;
        }

        internal void Exit()
        {
            try
            {
                HeldLocks.Remove(this);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        internal struct InstanceLockGuard : IDisposable
        {
            readonly InstanceLock m_lock;

            internal static void EnforceGuard(InstanceLock theLock)
            {
                foreach (InstanceLock heldLock in HeldLocks)
                {
                    switch (theLock.Operator)
                    {
                        case LockPriorityOperator.GreaterThan:
                            if (heldLock.InstanceId == theLock.InstanceId && heldLock.Priority <= theLock.Priority)
                                throw new InvalidOperationException(ExecutionStringManager.InstanceOperationNotValidinWorkflowThread);
                            break;

                        case LockPriorityOperator.GreaterThanOrReentrant:
                            // the checks here assume that locks have unique priorities
                            if (heldLock.InstanceId == theLock.InstanceId && heldLock.Priority < theLock.Priority)
                                throw new InvalidOperationException(ExecutionStringManager.InstanceOperationNotValidinWorkflowThread);
                            break;

                        default:
                            System.Diagnostics.Debug.Assert(false, "Unrecognized lock operator");
                            break;
                    }
                }
            }

            internal InstanceLockGuard(InstanceLock theLock)
            {
                this.m_lock = theLock;

                // Note: the following operations are logically atomic, but since the
                // list we are using is thread local there is no need to take a lock.

                EnforceGuard(theLock);

                try
                {
                }
                finally
                {
                    bool success = false;
#pragma warning disable 0618
//@
                    Monitor.Enter(this.m_lock);
#pragma warning restore 0618
                    try
                    {
                        HeldLocks.Add(this.m_lock);
                        success = true;
                    }
                    finally
                    {
                        if (!success)
                        {
                            Monitor.Exit(this.m_lock);
                        }
                    }
                }
            }

            internal void Pulse()
            {
                Monitor.Pulse(this.m_lock);
            }

            internal void Wait()
            {
                Monitor.Wait(this.m_lock);
            }

            public void Dispose()
            {
                // Note: the following operations are logically atomic, but since the
                // list we are using is thread local there is no need to take a lock.
                try
                {
                    HeldLocks.Remove(this.m_lock);
                }
                finally
                {
                    Monitor.Exit(this.m_lock);
                }
            }
        }
    }

    internal sealed class SchedulerLockGuard : IDisposable
    {
        private InstanceLock.InstanceLockGuard lg;
        private WorkflowExecutor workflowExec;

        internal SchedulerLockGuard(InstanceLock il, WorkflowExecutor w)
        {
            lg = il.Enter();
            workflowExec = w;
        }

        private static void FireEvents(List<SchedulerLockGuardInfo> eventList, WorkflowExecutor workflowExec)
        {
            if (!workflowExec.IsInstanceValid && (workflowExec.WorkflowStatus == WorkflowStatus.Completed || workflowExec.WorkflowStatus == WorkflowStatus.Terminated))
            {
                // The workflow is dead, let the instance have a hard ref to the corpse for support of the query apis.
                workflowExec.WorkflowInstance.DeadWorkflow = workflowExec;
            }
            for (int i = 0; i < eventList.Count; i++)
            {
                SchedulerLockGuardInfo eseg = eventList[i];
                // eseg.EventInfo is non-null only if the event type is Suspended or Terminated
                // If the event type is Suspended, then call FireWorkflowSuspended after casting
                //   the event argument to a String.
                // If the event type is Terminated, then call FireWorkflowTerminated after casting
                //   the event argument to either a String or an Exception.
                switch (eseg.EventType)
                {
                    case WorkflowEventInternal.Suspended:
                        workflowExec.FireWorkflowSuspended((String)eseg.EventInfo);
                        break;
                    case WorkflowEventInternal.Terminated:
                        if ((eseg.EventInfo as System.Exception) != null)
                        {
                            workflowExec.FireWorkflowTerminated((Exception)eseg.EventInfo);
                        }
                        else
                        {
                            workflowExec.FireWorkflowTerminated((String)eseg.EventInfo);
                        }
                        break;
                    default:
                        workflowExec.FireWorkflowExecutionEvent(eseg.Sender, eseg.EventType);
                        break;
                }
            }
        }

        internal static void Exit(InstanceLock il, WorkflowExecutor w)
        {
            List<SchedulerLockGuardInfo> eventList = new List<SchedulerLockGuardInfo>(w.EventsToFireList);
            w.EventsToFireList.Clear();
            il.Exit();
            FireEvents(eventList, w);
        }

        public void Dispose()
        {
            List<SchedulerLockGuardInfo> eventList = new List<SchedulerLockGuardInfo>(workflowExec.EventsToFireList);
            workflowExec.EventsToFireList.Clear();
            lg.Dispose();
            FireEvents(eventList, workflowExec);
        }
    }

    internal sealed class SchedulerLockGuardInfo
    {
        private Object sender;
        private WorkflowEventInternal eventType;
        private object eventInfo;

        internal SchedulerLockGuardInfo(Object _sender, WorkflowEventInternal _eventType)
        {
            sender = _sender;
            eventType = _eventType;
            eventInfo = null;
        }

        internal SchedulerLockGuardInfo(Object _sender, WorkflowEventInternal _eventType, object _eventInfo)
            : this(_sender, _eventType)
        {
            eventInfo = _eventInfo;
        }

        internal Object Sender
        {
            get
            {
                return sender;
            }
        }

        internal WorkflowEventInternal EventType
        {
            get
            {
                return eventType;
            }
        }

        internal Object EventInfo
        {
            get
            {
                return eventInfo;
            }
        }
    }
}
