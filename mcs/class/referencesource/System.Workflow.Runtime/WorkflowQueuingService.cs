//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.     All rights    reserved.
//------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Workflow.ComponentModel;
using System.Runtime.Serialization;
using System.Messaging;

namespace System.Workflow.Runtime
{
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class WorkflowQueuingService
    {
        Object syncRoot = new Object();
        IWorkflowCoreRuntime rootWorkflowExecutor;
        List<IComparable> dirtyQueues;
        EventQueueState pendingQueueState = new EventQueueState();
        Dictionary<IComparable, EventQueueState> persistedQueueStates;

        // event handler used by atomic execution context's Q service for message delivery 
        List<WorkflowQueuingService> messageArrivalEventHandlers;

        // set for inner queuing service
        WorkflowQueuingService rootQueuingService;

        // Runtime information visible to host, stored on the root activity
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
            Justification = "Design has been approved.  This is a false positive. DependencyProperty is an immutable type.")]
        public readonly static DependencyProperty PendingMessagesProperty = DependencyProperty.RegisterAttached("PendingMessages", typeof(Queue), typeof(WorkflowQueuingService), new PropertyMetadata(DependencyPropertyOptions.NonSerialized));

        // Persisted state properties
        internal static DependencyProperty RootPersistedQueueStatesProperty = DependencyProperty.RegisterAttached("RootPersistedQueueStates", typeof(Dictionary<IComparable, EventQueueState>), typeof(WorkflowQueuingService));
        internal static DependencyProperty LocalPersistedQueueStatesProperty = DependencyProperty.RegisterAttached("LocalPersistedQueueStates", typeof(Dictionary<IComparable, EventQueueState>), typeof(WorkflowQueuingService));
        private const string pendingNotification = "*PendingNotifications";

        // Snapshots created during pre-persist and dumped during post-persist
        // If  persistence fails, changes made to queuing service during pre-persist must be undone
        //    in post-persist.
        // Created for ref. 20575.
        private Dictionary<IComparable, EventQueueState> persistedQueueStatesSnapshot = null;
        private EventQueueState pendingQueueStateSnapshot = null;

        // root Q service constructor
        internal WorkflowQueuingService(IWorkflowCoreRuntime rootWorkflowExecutor)
        {
            this.rootWorkflowExecutor = rootWorkflowExecutor;
            this.rootWorkflowExecutor.RootActivity.SetValue(WorkflowQueuingService.PendingMessagesProperty, this.pendingQueueState.Messages);
            this.persistedQueueStates = (Dictionary<IComparable, EventQueueState>)this.rootWorkflowExecutor.RootActivity.GetValue(WorkflowQueuingService.RootPersistedQueueStatesProperty);
            if (this.persistedQueueStates == null)
            {
                this.persistedQueueStates = new Dictionary<IComparable, EventQueueState>();
                this.rootWorkflowExecutor.RootActivity.SetValue(WorkflowQueuingService.RootPersistedQueueStatesProperty, this.persistedQueueStates);
            }
            if (!this.Exists(pendingNotification))
                this.CreateWorkflowQueue(pendingNotification, false);
        }

        // inner Q service constructor
        internal WorkflowQueuingService(WorkflowQueuingService copyFromQueuingService)
        {
            this.rootQueuingService = copyFromQueuingService;
            this.rootWorkflowExecutor = copyFromQueuingService.rootWorkflowExecutor;
            this.rootWorkflowExecutor.RootActivity.SetValue(WorkflowQueuingService.PendingMessagesProperty, this.pendingQueueState.Messages);
            this.persistedQueueStates = new Dictionary<IComparable, EventQueueState>();
            this.rootWorkflowExecutor.RootActivity.SetValue(WorkflowQueuingService.LocalPersistedQueueStatesProperty, this.persistedQueueStates);
            SubscribeForRootMessageDelivery();
        }

        public WorkflowQueue CreateWorkflowQueue(IComparable queueName, bool transactional)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");

            lock (SyncRoot)
            {
                // if not transactional create one at the root 
                // so it is visible outside this transaction
                if (this.rootQueuingService != null && !transactional)
                {
                    return this.rootQueuingService.CreateWorkflowQueue(queueName, false);
                }

                NewQueue(queueName, true, transactional);

                return new WorkflowQueue(this, queueName);
            }
        }

        public void DeleteWorkflowQueue(IComparable queueName)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");

            lock (SyncRoot)
            {
                // when we are deleting the queue from activity
                // message delivery should not happen.
                if (this.rootQueuingService != null && !IsTransactionalQueue(queueName))
                {
                    this.rootQueuingService.DeleteWorkflowQueue(queueName);
                    return;
                }

                EventQueueState queueState = GetEventQueueState(queueName);

                Queue queue = queueState.Messages;
                Queue pendingQueue = this.pendingQueueState.Messages;

                while (queue.Count != 0)
                {
                    pendingQueue.Enqueue(queue.Dequeue());
                }

                WorkflowTrace.Runtime.TraceInformation("Queuing Service: Deleting Queue with ID {0} for {1}", queueName.GetHashCode(), queueName);
                this.persistedQueueStates.Remove(queueName);
            }
        }

        public bool Exists(IComparable queueName)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");

            lock (SyncRoot)
            {
                if (this.rootQueuingService != null && !IsTransactionalQueue(queueName))
                {
                    return this.rootQueuingService.Exists(queueName);
                }

                return this.persistedQueueStates.ContainsKey(queueName);
            }
        }

        public WorkflowQueue GetWorkflowQueue(IComparable queueName)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");

            lock (SyncRoot)
            {
                if (this.rootQueuingService != null && !IsTransactionalQueue(queueName))
                {
                    return this.rootQueuingService.GetWorkflowQueue(queueName);
                }

                GetEventQueueState(queueName);

                return new WorkflowQueue(this, queueName);
            }
        }

        #region internal functions

        internal Object SyncRoot
        {
            get { return syncRoot; }
        }

        internal void EnqueueEvent(IComparable queueName, Object item)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");

            lock (SyncRoot)
            {
                if (this.rootQueuingService != null && !IsTransactionalQueue(queueName))
                {
                    this.rootQueuingService.EnqueueEvent(queueName, item);
                    return;
                }

                EventQueueState qState = GetQueue(queueName);
                if (!qState.Enabled)
                {
                    throw new QueueException(String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.QueueNotEnabled, queueName), MessageQueueErrorCode.QueueNotAvailable);
                }

                // note enqueue allowed irrespective of dirty flag since it is delivered through
                qState.Messages.Enqueue(item);

                WorkflowTrace.Runtime.TraceInformation("Queuing Service: Enqueue item Queue ID {0} for {1}", queueName.GetHashCode(), queueName);

                // notify message arrived subscribers
                for (int i = 0; messageArrivalEventHandlers != null && i < messageArrivalEventHandlers.Count; ++i)
                {
                    this.messageArrivalEventHandlers[i].OnItemEnqueued(queueName, item);
                }

                NotifyExternalSubscribers(queueName, qState, item);
            }
        }
        internal bool SafeEnqueueEvent(IComparable queueName, Object item)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");

            lock (SyncRoot)
            {
                if (this.rootQueuingService != null && !IsTransactionalQueue(queueName))
                {
                    return this.rootQueuingService.SafeEnqueueEvent(queueName, item);
                }

                EventQueueState qState = GetQueue(queueName);
                if (!qState.Enabled)
                {
                    throw new QueueException(String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.QueueNotEnabled, queueName), MessageQueueErrorCode.QueueNotAvailable);
                }

                // note enqueue allowed irrespective of dirty flag since it is delivered through
                qState.Messages.Enqueue(item);

                WorkflowTrace.Runtime.TraceInformation("Queuing Service: Enqueue item Queue ID {0} for {1}", queueName.GetHashCode(), queueName);

                // notify message arrived subscribers
                for (int i = 0; messageArrivalEventHandlers != null && i < messageArrivalEventHandlers.Count; ++i)
                {
                    this.messageArrivalEventHandlers[i].OnItemSafeEnqueued(queueName, item);
                }

                NotifySynchronousSubscribers(queueName, qState, item);
                return QueueAsynchronousEvent(queueName, qState);
            }
        }


        internal object Peek(IComparable queueName)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");

            lock (SyncRoot)
            {
                if (this.rootQueuingService != null && !IsTransactionalQueue(queueName))
                {
                    return this.rootQueuingService.Peek(queueName);
                }

                EventQueueState queueState = GetEventQueueState(queueName);
                if (queueState.Messages.Count != 0)
                    return queueState.Messages.Peek();

                object[] args = new object[] { System.Messaging.MessageQueueErrorCode.MessageNotFound, queueName };
                string message = string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.EventQueueException, args);

                throw new QueueException(message, MessageQueueErrorCode.MessageNotFound);
            }
        }

        internal Object DequeueEvent(IComparable queueName)
        {
            if (queueName == null)
                throw new ArgumentNullException("queueName");

            lock (SyncRoot)
            {
                if (this.rootQueuingService != null && !IsTransactionalQueue(queueName))
                {
                    return this.rootQueuingService.DequeueEvent(queueName);
                }

                EventQueueState queueState = GetEventQueueState(queueName);
                if (queueState.Messages.Count != 0)
                    return queueState.Messages.Dequeue();

                object[] args = new object[] { System.Messaging.MessageQueueErrorCode.MessageNotFound, queueName };
                string message = string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.EventQueueException, args);

                throw new QueueException(message, MessageQueueErrorCode.MessageNotFound);
            }
        }

        internal EventQueueState GetQueueState(IComparable eventType)
        {
            lock (SyncRoot)
            {
                return GetQueue(eventType);
            }
        }

        Activity caller;

        internal Activity CallingActivity
        {
            get
            {
                if (this.rootQueuingService != null)
                    return this.rootQueuingService.CallingActivity;
                return this.caller;
            }
            set
            {
                if (this.rootQueuingService != null)
                    this.rootQueuingService.CallingActivity = value;

                this.caller = value;
            }
        }

        private bool QueueAsynchronousEvent(IComparable queueName, EventQueueState qState)
        {
            if (qState.AsynchronousListeners.Count != 0 || IsNestedListenersExist(queueName))
            {
                Queue q = GetQueue(pendingNotification).Messages;
                q.Enqueue(new KeyValuePair<IComparable, EventQueueState>(queueName, qState));
                WorkflowTrace.Runtime.TraceInformation("Queuing Service: Queued delayed message notification for '{0}'", queueName.ToString());
                return q.Count == 1;
            }
            return false;
        }

        bool IsNestedListenersExist(IComparable queueName)
        {
            for (int i = 0; messageArrivalEventHandlers != null && i < messageArrivalEventHandlers.Count; ++i)
            {
                WorkflowQueuingService qService = messageArrivalEventHandlers[i];
                EventQueueState queueState = null;

                if (qService.persistedQueueStates.TryGetValue(queueName, out queueState) &&
                    queueState.AsynchronousListeners.Count != 0)
                    return true;
            }
            return false;
        }
        internal void ProcessesQueuedAsynchronousEvents()
        {
            Queue q = GetQueue(pendingNotification).Messages;
            while (q.Count > 0)
            {
                KeyValuePair<IComparable, EventQueueState> pair = (KeyValuePair<IComparable, EventQueueState>)q.Dequeue();
                // notify message arrived subscribers
                WorkflowTrace.Runtime.TraceInformation("Queuing Service: Processing delayed message notification '{0}'", pair.Key.ToString());
                for (int i = 0; messageArrivalEventHandlers != null && i < messageArrivalEventHandlers.Count; ++i)
                {
                    WorkflowQueuingService service = this.messageArrivalEventHandlers[i];
                    if (service.persistedQueueStates.ContainsKey(pair.Key))
                    {
                        EventQueueState qState = service.GetQueue(pair.Key);
                        if (qState.Enabled)
                        {
                            service.NotifyAsynchronousSubscribers(pair.Key, qState, 1);
                        }
                    }
                }
                NotifyAsynchronousSubscribers(pair.Key, pair.Value, 1);
            }
        }

        internal void NotifyAsynchronousSubscribers(IComparable queueName, EventQueueState qState, int numberOfNotification)
        {
            for (int i = 0; i < numberOfNotification; ++i)
            {
                QueueEventArgs args = new QueueEventArgs(queueName);
                lock (SyncRoot)
                {
                    foreach (ActivityExecutorDelegateInfo<QueueEventArgs> subscriber in qState.AsynchronousListeners)
                    {
                        Activity contextActivity = rootWorkflowExecutor.GetContextActivityForId(subscriber.ContextId);
                        Debug.Assert(contextActivity != null);
                        subscriber.InvokeDelegate(contextActivity, args, false);
                        WorkflowTrace.Runtime.TraceInformation("Queuing Service: Notifying async subscriber on queue:'{0}' activity:{1}", queueName.ToString(), subscriber.ActivityQualifiedName);
                    }
                }
            }
        }

        /// <summary>
        /// At termination/completion point, need to move messages from all queues to the pending queue
        /// </summary>
        internal void MoveAllMessagesToPendingQueue()
        {
            lock (SyncRoot)
            {
                Queue pendingQueue = this.pendingQueueState.Messages;
                foreach (EventQueueState queueState in this.persistedQueueStates.Values)
                {
                    Queue queue = queueState.Messages;
                    while (queue.Count != 0)
                    {
                        pendingQueue.Enqueue(queue.Dequeue());
                    }
                }
            }
        }

        #endregion

        #region private root q service helpers

        private EventQueueState GetEventQueueState(IComparable queueName)
        {
            EventQueueState queueState = GetQueue(queueName);
            if (queueState.Dirty)
            {
                string message =
                    string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.QueueBusyException, new object[] { queueName });

                throw new QueueException(message, MessageQueueErrorCode.QueueNotAvailable);
            }

            return queueState;
        }

        private void NewQueue(IComparable queueID, bool enabled, bool transactional)
        {
            WorkflowTrace.Runtime.TraceInformation("Queuing Service: Creating new Queue with ID {0} for {1}", queueID.GetHashCode(), queueID);

            if (this.persistedQueueStates.ContainsKey(queueID))
            {
                object[] args =
                    new object[] { System.Messaging.MessageQueueErrorCode.QueueExists, queueID };
                string message =
                    string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.EventQueueException, args);

                throw new QueueException(message, MessageQueueErrorCode.QueueExists);
            }

            EventQueueState queueState = new EventQueueState();
            queueState.Enabled = enabled;
            queueState.queueName = queueID;
            queueState.Transactional = transactional;
            this.persistedQueueStates.Add(queueID, queueState);
        }

        internal EventQueueState GetQueue(IComparable queueID)
        {
            EventQueueState queue;
            if (this.persistedQueueStates.TryGetValue(queueID, out queue))
            {
                queue.queueName = queueID;
                return queue;
            }

            object[] args =
                new object[] { System.Messaging.MessageQueueErrorCode.QueueNotFound, queueID };
            string message =
                string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.EventQueueException, args);

            throw new QueueException(message, MessageQueueErrorCode.QueueNotFound);
        }

        internal IEnumerable<IComparable> QueueNames
        {
            get
            {
                List<IComparable> list = new List<IComparable>(this.persistedQueueStates.Keys);
                foreach (IComparable name in list)
                {
                    if (name is String && (String)name == pendingNotification)
                    {
                        list.Remove(name);
                        break;
                    }
                }
                return list;
            }
        }

        private void ApplyChangesFrom(EventQueueState srcPendingQueueState, Dictionary<IComparable, EventQueueState> srcPersistedQueueStates)
        {
            lock (SyncRoot)
            {
                Dictionary<IComparable, EventQueueState> modifiedItems = new Dictionary<IComparable, EventQueueState>();

                foreach (KeyValuePair<IComparable, EventQueueState> mergeItem in srcPersistedQueueStates)
                {
                    Debug.Assert(mergeItem.Value.Transactional, "Queue inside a transactional context is not transactional!");

                    if (mergeItem.Value.Transactional)
                    {
                        if (this.persistedQueueStates.ContainsKey(mergeItem.Key))
                        {
                            EventQueueState oldvalue = this.persistedQueueStates[mergeItem.Key];
                            if (!oldvalue.Dirty)
                            {
                                // we could get here when there
                                // are conflicting create Qs
                                string message =
                                    string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.QueueBusyException, new object[] { mergeItem.Key });

                                throw new QueueException(message, MessageQueueErrorCode.QueueNotAvailable);
                            }
                        }
                        modifiedItems.Add(mergeItem.Key, mergeItem.Value);
                    }
                }

                // no conflicts detected now make the updates visible
                foreach (KeyValuePair<IComparable, EventQueueState> modifiedItem in modifiedItems)
                {
                    // shared queue in the root, swap out to new value 
                    // or add new item
                    this.persistedQueueStates[modifiedItem.Key] = modifiedItem.Value;
                }

                this.pendingQueueState.CopyFrom(srcPendingQueueState);
            }
        }

        // message arrival async notification
        private void NotifyExternalSubscribers(IComparable queueName, EventQueueState qState, Object eventInstance)
        {
            NotifySynchronousSubscribers(queueName, qState, eventInstance);
            NotifyAsynchronousSubscribers(queueName, qState, 1);
        }

        private void NotifySynchronousSubscribers(IComparable queueName, EventQueueState qState, Object eventInstance)
        {
            QueueEventArgs args = new QueueEventArgs(queueName);

            for (int i = 0; i < qState.SynchronousListeners.Count; ++i)
            {
                if (qState.SynchronousListeners[i].HandlerDelegate != null)
                    qState.SynchronousListeners[i].HandlerDelegate(new WorkflowQueue(this, queueName), args);
                else
                    qState.SynchronousListeners[i].EventListener.OnEvent(new WorkflowQueue(this, queueName), args);
            }
        }

        // returns a valid state only if transactional and entry exists
        private EventQueueState MarkQueueDirtyIfTransactional(IComparable queueName)
        {
            lock (SyncRoot)
            {
                Debug.Assert(this.rootQueuingService == null, "MarkQueueDirty should be done at root");

                if (!this.persistedQueueStates.ContainsKey(queueName))
                    return null;

                EventQueueState queueState = GetQueue(queueName);

                if (!queueState.Transactional)
                    return null;

                if (queueState.Dirty)
                    return queueState; // already marked

                queueState.Dirty = true;

                if (this.dirtyQueues == null)
                    this.dirtyQueues = new List<IComparable>();

                // add to the list of dirty queues
                this.dirtyQueues.Add(queueName);

                return queueState;
            }
        }

        private void AddMessageArrivedEventHandler(WorkflowQueuingService handler)
        {
            lock (SyncRoot)
            {
                if (this.messageArrivalEventHandlers == null)
                    this.messageArrivalEventHandlers = new List<WorkflowQueuingService>();
                this.messageArrivalEventHandlers.Add(handler);
            }
        }

        private void RemoveMessageArrivedEventHandler(WorkflowQueuingService handler)
        {
            lock (SyncRoot)
            {
                if (this.messageArrivalEventHandlers != null)
                    this.messageArrivalEventHandlers.Remove(handler);

                if (this.dirtyQueues != null)
                {
                    foreach (IComparable queueName in this.dirtyQueues)
                    {
                        EventQueueState qState = GetQueue(queueName);
                        qState.Dirty = false;
                    }
                }
            }
        }
        #endregion

        #region inner QueuingService functions
        private bool IsTransactionalQueue(IComparable queueName)
        {
            // check inner service for existense
            if (!this.persistedQueueStates.ContainsKey(queueName))
            {
                EventQueueState queueState = this.rootQueuingService.MarkQueueDirtyIfTransactional(queueName);

                if (queueState != null)
                {
                    // if transactional proceed to the inner queue service 
                    // for this operation after adding the state                    
                    EventQueueState snapshotState = new EventQueueState();
                    snapshotState.CopyFrom(queueState);
                    this.persistedQueueStates.Add(queueName, snapshotState);
                    return true;
                }

                return false;
            }

            return true; // if entry exits, it must be transactional
        }

        private void SubscribeForRootMessageDelivery()
        {
            if (this.rootQueuingService == null)
                return;
            this.rootQueuingService.AddMessageArrivedEventHandler(this);
        }

        private void UnSubscribeFromRootMessageDelivery()
        {
            if (this.rootQueuingService == null)
                return;
            this.rootQueuingService.RemoveMessageArrivedEventHandler(this);
        }

        // listen on its internal(parent) queuing service 
        // messages and pull messages. There is one parent queuing service visible to the external
        // host environment. A queueing service snapshot exists per atomic scope and external messages
        // for existing queues need to be pushed through
        private void OnItemEnqueued(IComparable queueName, object item)
        {
            if (this.persistedQueueStates.ContainsKey(queueName))
            {
                // make the message visible to inner queueing service
                EventQueueState qState = GetQueue(queueName);
                if (!qState.Enabled)
                {
                    object[] msgArgs = new object[] { System.Messaging.MessageQueueErrorCode.QueueNotFound, queueName };
                    string message = string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.EventQueueException, msgArgs);
                    throw new QueueException(message, MessageQueueErrorCode.QueueNotAvailable);
                }
                qState.Messages.Enqueue(item);
                NotifyExternalSubscribers(queueName, qState, item);
            }
        }

        private void OnItemSafeEnqueued(IComparable queueName, object item)
        {
            if (this.persistedQueueStates.ContainsKey(queueName))
            {
                // make the message visible to inner queueing service
                EventQueueState qState = GetQueue(queueName);
                if (!qState.Enabled)
                {
                    object[] msgArgs = new object[] { System.Messaging.MessageQueueErrorCode.QueueNotFound, queueName };
                    string message = string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.EventQueueException, msgArgs);
                    throw new QueueException(message, MessageQueueErrorCode.QueueNotAvailable);
                }
                qState.Messages.Enqueue(item);
                NotifySynchronousSubscribers(queueName, qState, item);
            }
        }

        internal void Complete(bool commitSucceeded)
        {
            if (commitSucceeded)
            {
                this.rootQueuingService.ApplyChangesFrom(this.pendingQueueState, this.persistedQueueStates);
            }

            UnSubscribeFromRootMessageDelivery();
        }
        #endregion

        #region Pre-persist and Post-persist helpers for queuing service states

        // Created for ref. 20575
        internal void PostPersist(bool isPersistSuccessful)
        {
            // If persist is unsuccessful, we'll undo the changes done
            //   because of the call to .Complete() in PrePresist
            if (!isPersistSuccessful)
            {
                Debug.Assert(rootWorkflowExecutor.CurrentAtomicActivity != null);
                Debug.Assert(pendingQueueStateSnapshot != null);
                Debug.Assert(persistedQueueStatesSnapshot != null);

                TransactionalProperties transactionalProperties = rootWorkflowExecutor.CurrentAtomicActivity.GetValue(WorkflowExecutor.TransactionalPropertiesProperty) as TransactionalProperties;
                Debug.Assert(transactionalProperties != null);

                // Restore queuing states and set root activity's dependency properties to the new values.
                pendingQueueState = pendingQueueStateSnapshot;
                persistedQueueStates = persistedQueueStatesSnapshot;
                rootWorkflowExecutor.RootActivity.SetValue(WorkflowQueuingService.RootPersistedQueueStatesProperty, persistedQueueStatesSnapshot);
                rootWorkflowExecutor.RootActivity.SetValue(WorkflowQueuingService.PendingMessagesProperty, pendingQueueStateSnapshot.Messages);

                // Also call Subscribe...() because the .Complete() call called Unsubscribe
                transactionalProperties.LocalQueuingService.SubscribeForRootMessageDelivery();
            }

            // The backups are no longer necessary.
            // The next call to PrePresistQueuingServiceState() will do a re-backup.
            persistedQueueStatesSnapshot = null;
            pendingQueueStateSnapshot = null;
        }

        // Created for ref. 20575
        internal void PrePersist()
        {
            if (rootWorkflowExecutor.CurrentAtomicActivity != null)
            {
                // Create transactionalProperties from currentAtomicActivity                
                TransactionalProperties transactionalProperties = this.rootWorkflowExecutor.CurrentAtomicActivity.GetValue(WorkflowExecutor.TransactionalPropertiesProperty) as TransactionalProperties;

                // Create backup snapshot of root queuing service's persistedQueuesStates
                // qService.persistedQueueStates is changed when LocalQueuingService.Complete is called later.
                persistedQueueStatesSnapshot = new Dictionary<IComparable, EventQueueState>();
                foreach (KeyValuePair<IComparable, EventQueueState> kv in persistedQueueStates)
                {
                    EventQueueState individualPersistedQueueStateValue = new EventQueueState();
                    individualPersistedQueueStateValue.CopyFrom(kv.Value);
                    persistedQueueStatesSnapshot.Add(kv.Key, individualPersistedQueueStateValue);
                }

                // Create backup snapshot of root queuing service's pendingQueueState
                // qService.pendingQueueState is changed when LocalQueuingService.Complete is called later.
                pendingQueueStateSnapshot = new EventQueueState();
                pendingQueueStateSnapshot.CopyFrom(pendingQueueState);

                // Reconcile differences between root and local queuing services.
                transactionalProperties.LocalQueuingService.Complete(true);
            }
        }


        #endregion Pre-persist and post-persist helpers for queuing service states
    }
}

