//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Workflow.ComponentModel;

namespace System.Workflow.Runtime
{
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class WorkflowQueue
    {
        IComparable queueName;
        WorkflowQueuingService qService;

        internal WorkflowQueue(WorkflowQueuingService qService, IComparable queueName)
        {
            this.qService = qService;
            this.queueName = queueName;
        }

        public event EventHandler<QueueEventArgs> QueueItemAvailable
        {
            add
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                lock (qService.SyncRoot)
                {
                    EventQueueState qState = qService.GetQueueState(this.queueName);
                    ActivityExecutorDelegateInfo<QueueEventArgs> subscriber = new ActivityExecutorDelegateInfo<QueueEventArgs>(value, qService.CallingActivity);
                    qState.AsynchronousListeners.Add(subscriber);
                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "WorkflowQueue:QueueItemAvailable subscribe for activity '{0}' with context Id {1}", subscriber.ActivityQualifiedName, subscriber.ContextId);

                    if (qState.AsynchronousListeners.Count == 1)
                        qService.NotifyAsynchronousSubscribers(this.queueName, qState, qState.Messages.Count); 
                }
            }
            remove
            {
                lock (qService.SyncRoot)
                {
                    ActivityExecutorDelegateInfo<QueueEventArgs> subscriber = new ActivityExecutorDelegateInfo<QueueEventArgs>(value, qService.CallingActivity);
                    bool removed = qService.GetQueueState(this.queueName).AsynchronousListeners.Remove(subscriber);
                    if (!removed)
                    {
                        WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "WorkflowQueue:QueueItemAvailable unsubscribe failed for activity '{0}' with context Id {1} ", subscriber.ActivityQualifiedName, subscriber.ContextId);
                    }
                }
            }
        }

        public void RegisterForQueueItemAvailable(IActivityEventListener<QueueEventArgs> eventListener)
        {
            RegisterForQueueItemAvailable(eventListener, null);
        }
        public void RegisterForQueueItemAvailable(IActivityEventListener<QueueEventArgs> eventListener, string subscriberQualifiedName)
        {
            if (eventListener == null)
                throw new ArgumentNullException("eventListener");

            lock (qService.SyncRoot)
            {
                EventQueueState qState = qService.GetQueueState(this.queueName);
                ActivityExecutorDelegateInfo<QueueEventArgs> subscriber = new ActivityExecutorDelegateInfo<QueueEventArgs>(eventListener, qService.CallingActivity);
                if (subscriberQualifiedName != null)
                {
                    subscriber.SubscribedActivityQualifiedName = subscriberQualifiedName;
                }
                qState.AsynchronousListeners.Add(subscriber);
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "WorkflowQueue:QueueItemAvailable subscribe for activity '{0}' with context Id {1}", subscriber.ActivityQualifiedName, subscriber.ContextId);

                if (qState.AsynchronousListeners.Count == 1)
                    qService.NotifyAsynchronousSubscribers(this.queueName, qState, qState.Messages.Count);
            }
        }
        public void UnregisterForQueueItemAvailable(IActivityEventListener<QueueEventArgs> eventListener)
        {
            if (eventListener == null)
                throw new ArgumentNullException("eventListener");
        
            lock (qService.SyncRoot)
            {
                ActivityExecutorDelegateInfo<QueueEventArgs> subscriber = new ActivityExecutorDelegateInfo<QueueEventArgs>(eventListener, qService.CallingActivity);
                bool removed = qService.GetQueueState(this.queueName).AsynchronousListeners.Remove(subscriber);
                if (!removed)
                {
                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "WorkflowQueue:QueueItemAvailable unsubscribe failed for activity '{0}' with context Id {1}", subscriber.ActivityQualifiedName, subscriber.ContextId);
                }
            }
        }

        public event EventHandler<QueueEventArgs> QueueItemArrived
        {
            add
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                lock (qService.SyncRoot)
                {
                    qService.GetQueueState(this.queueName).SynchronousListeners.Add(new ActivityExecutorDelegateInfo<QueueEventArgs>(value, qService.CallingActivity));
                }
            }
            remove
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                lock (qService.SyncRoot)
                {
                    qService.GetQueueState(this.queueName).SynchronousListeners.Remove(new ActivityExecutorDelegateInfo<QueueEventArgs>(value, qService.CallingActivity));                                                           
                }
            }
        }
        public void RegisterForQueueItemArrived(IActivityEventListener<QueueEventArgs> eventListener)
        {
            if (eventListener == null)
                throw new ArgumentNullException("eventListener");

            lock (qService.SyncRoot)
            {
                qService.GetQueueState(this.queueName).SynchronousListeners.Add(new ActivityExecutorDelegateInfo<QueueEventArgs>(eventListener, qService.CallingActivity));
            }
        }
        public void UnregisterForQueueItemArrived(IActivityEventListener<QueueEventArgs> eventListener)
        {
            if (eventListener == null)
                throw new ArgumentNullException("eventListener");

            lock (qService.SyncRoot)
            {
                qService.GetQueueState(this.queueName).SynchronousListeners.Remove(new ActivityExecutorDelegateInfo<QueueEventArgs>(eventListener, qService.CallingActivity));
            }
        }

        public IComparable QueueName
        {
            get
            {
                return this.queueName;
            }
        }

        public WorkflowQueuingService QueuingService
        {
            get
            {
                return this.qService;
            }
        }      

        public void Enqueue(object item)
        {
            lock (qService.SyncRoot)
            {
                qService.EnqueueEvent(this.queueName, item);
            }
        }

        public object Dequeue() 
        {
            lock (qService.SyncRoot)
            {
                object message = qService.Peek(this.queueName);

                return qService.DequeueEvent(this.queueName);
            }
        }

        public object Peek()
        {
            lock (qService.SyncRoot)
            {
                object message = qService.Peek(this.queueName);

                return message;
            }
        }

        public int Count
        {
            get
            {
                lock (qService.SyncRoot)
                {
                    return this.qService.GetQueueState(this.queueName).Messages.Count;
                } 
            }
        }

        public bool Enabled
        {
            get
            {
                lock (qService.SyncRoot)
                {
                    return this.qService.GetQueueState(this.queueName).Enabled;
                }
            }
            set
            {
                lock (qService.SyncRoot)
                {
                    this.qService.GetQueueState(this.queueName).Enabled = value;
                }
            }           
        }
    }
}

