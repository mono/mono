//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime.DurableInstancing
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Transactions;
    using System.Xml.Linq;
    using System.Runtime.Diagnostics;

    [Fx.Tag.XamlVisible(false)]
    public sealed class InstanceHandle
    {
        [Fx.Tag.SynchronizationObject(Blocking = false)]
        readonly object thisLock = new object();

        object providerObject;
        bool providerObjectSet;
        bool needFreedNotification;
        PreparingEnlistment pendingPreparingEnlistment;
        AcquireContextAsyncResult pendingRollback;
        InstanceHandleReference inProgressBind;
        WaitForEventsAsyncResult waitResult;
        HashSet<XName> boundOwnerEvents;
        HashSet<InstancePersistenceEvent> pendingOwnerEvents;
        EventTraceActivity eventTraceActivity;

        // Fields used to implement an atomic Guid Id get/set property.
        Guid id;
        volatile bool idIsSet;

        internal InstanceHandle(InstanceStore store, InstanceOwner owner)
        {
            Fx.Assert(store != null, "Shouldn't be possible.");

            Version = -1;
            Store = store;
            Owner = owner;
            View = new InstanceView(owner);
            IsValid = true;
        }

        internal InstanceHandle(InstanceStore store, InstanceOwner owner, Guid instanceId)
        {
            Fx.Assert(store != null, "Shouldn't be possible here either.");
            Fx.Assert(instanceId != Guid.Empty, "Should be validating this.");

            Version = -1;
            Store = store;
            Owner = owner;
            Id = instanceId;
            View = new InstanceView(owner, instanceId);
            IsValid = true;
            if (Fx.Trace.IsEtwProviderEnabled)
            {
                eventTraceActivity = new EventTraceActivity(instanceId);
            }
        }


        public bool IsValid { get; private set; }


        internal InstanceView View { get; private set; }
        internal InstanceStore Store { get; private set; }

        internal InstanceOwner Owner { get; private set; }

        // Since writing to a Guid field is not atomic, we need synchronization between reading and writing. The idIsSet boolean field can only
        // appear true once the id field is completely written due to the memory barriers implied by the reads and writes to a volatile field.
        // Writes to bool fields are atomic, and this property is only written to once. By checking the bool prior to reading the Guid, we can
        // be sure that the Guid is fully materialized when read.
        internal Guid Id
        {
            get
            {
                // this.idIsSet is volatile.
                if (!this.idIsSet)
                {
                    return Guid.Empty;
                }
                return this.id;
            }

            private set
            {
                Fx.Assert(value != Guid.Empty, "Cannot set an empty Id.");
                Fx.Assert(this.id == Guid.Empty, "Cannot set Id more than once.");
                Fx.Assert(!this.idIsSet, "idIsSet out of [....] with id.");

                this.id = value;

                if (Fx.Trace.IsEtwProviderEnabled)
                {
                    eventTraceActivity = new EventTraceActivity(value);
                }

                // this.isIdSet is volatile.
                this.idIsSet = true;
            }
        }

        internal long Version { get; private set; }

        internal InstanceHandle ConflictingHandle { get; set; }

        internal object ProviderObject
        {
            get
            {
                return this.providerObject;
            }
            set
            {
                this.providerObject = value;
                this.providerObjectSet = true;
            }
        }

        internal EventTraceActivity EventTraceActivity
        {
            get
            {
                return this.eventTraceActivity;
            }
        }

        // When non-null, a transaction is pending.
        AcquireContextAsyncResult CurrentTransactionalAsyncResult { get; set; }

        bool OperationPending { get; set; }
        bool TooLateToEnlist { get; set; }
        AcquireContextAsyncResult AcquirePending { get; set; }
        InstancePersistenceContext CurrentExecutionContext { get; set; }

        object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }


        public void Free()
        {
            if (!this.providerObjectSet)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.HandleFreedBeforeInitialized));
            }

            if (!IsValid)
            {
                return;
            }

            List<InstanceHandleReference> handlesPendingResolution = null;
            WaitForEventsAsyncResult resultToCancel = null;

            try
            {
                bool needNotification = false;
                InstancePersistenceContext currentContext = null;

                lock (ThisLock)
                {
                    if (!IsValid)
                    {
                        return;
                    }
                    IsValid = false;

                    IEnumerable<XName> eventsToUnbind = null;
                    if (this.pendingOwnerEvents != null && this.pendingOwnerEvents.Count > 0)
                    {
                        eventsToUnbind = this.pendingOwnerEvents.Select(persistenceEvent => persistenceEvent.Name);
                    }
                    if (this.boundOwnerEvents != null && this.boundOwnerEvents.Count > 0)
                    {
                        eventsToUnbind = eventsToUnbind == null ? this.boundOwnerEvents : eventsToUnbind.Concat(this.boundOwnerEvents);
                    }
                    if (eventsToUnbind != null)
                    {
                        Fx.Assert(Owner != null, "How do we have owner events without an owner.");
                        Store.RemoveHandleFromEvents(this, eventsToUnbind, Owner);
                    }
                    if (this.waitResult != null)
                    {
                        resultToCancel = this.waitResult;
                        this.waitResult = null;
                    }

                    if (OperationPending)
                    {
                        if (AcquirePending != null)
                        {
                            // If in this stage, we need to short-circuit the pending transaction.
                            Fx.Assert(CurrentTransactionalAsyncResult != null, "Should have a pending transaction if we are waiting for it.");
                            CurrentTransactionalAsyncResult.WaitForHostTransaction.Set();
                            this.needFreedNotification = true;
                        }
                        else
                        {
                            // Here, just notify the currently executing command.
                            Fx.Assert(CurrentExecutionContext != null, "Must have either this or AcquirePending set.");
                            currentContext = CurrentExecutionContext;
                        }
                    }
                    else
                    {
                        needNotification = true;

                        if (this.inProgressBind != null)
                        {
                            Owner.CancelBind(ref this.inProgressBind, ref handlesPendingResolution);
                        }
                        else if (Version != -1)
                        {
                            // This means the handle was successfully bound in the past.  Need to remove it from the table of handles.
                            Owner.Unbind(this);
                        }
                    }
                }

                if (currentContext != null)
                {
                    // Need to do this not in a lock.
                    currentContext.NotifyHandleFree();

                    lock (ThisLock)
                    {
                        if (OperationPending)
                        {
                            this.needFreedNotification = true;

                            // Cancel any pending lock reclaim here.
                            if (this.inProgressBind != null)
                            {
                                Fx.Assert(Owner != null, "Must be bound to owner to have an inProgressBind for the lock in CancelReclaim.");

                                // Null reason defaults to OperationCanceledException.  (Defer creating it since this might not be a
                                // reclaim attempt, but we don't know until we take the HandlesLock.)
                                Owner.FaultBind(ref this.inProgressBind, ref handlesPendingResolution, null);
                            }
                        }
                        else
                        {
                            needNotification = true;
                        }
                    }
                }

                if (needNotification)
                {
                    Store.FreeInstanceHandle(this, ProviderObject);
                }
            }
            finally
            {
                if (resultToCancel != null)
                {
                    resultToCancel.Canceled();
                }

                InstanceOwner.ResolveHandles(handlesPendingResolution);
            }
        }

        internal void BindOwnerEvent(InstancePersistenceEvent persistenceEvent)
        {
            lock (ThisLock)
            {
                Fx.Assert(OperationPending, "Should only be called during an operation.");
                Fx.Assert(AcquirePending == null, "Should only be called after acquiring the transaction.");
                Fx.Assert(Owner != null, "Must be bound to owner to have an owner-scoped event.");

                if (IsValid && (this.boundOwnerEvents == null || !this.boundOwnerEvents.Contains(persistenceEvent.Name)))
                {
                    if (this.pendingOwnerEvents == null)
                    {
                        this.pendingOwnerEvents = new HashSet<InstancePersistenceEvent>();
                    }
                    else if (this.pendingOwnerEvents.Contains(persistenceEvent))
                    {
                        return;
                    }
                    this.pendingOwnerEvents.Add(persistenceEvent);
                    Store.PendHandleToEvent(this, persistenceEvent, Owner);
                }
            }
        }

        internal void StartPotentialBind()
        {
            lock (ThisLock)
            {
                Fx.AssertAndThrow(Version == -1, "Handle already bound to a lock.");

                Fx.Assert(OperationPending, "Should only be called during an operation.");
                Fx.Assert(AcquirePending == null, "Should only be called after acquiring the transaction.");
                Fx.Assert(this.inProgressBind == null, "StartPotentialBind should only be called once per command.");
                Fx.Assert(Owner != null, "Must be bound to owner to have an inProgressBind for the lock.");

                Owner.StartBind(this, ref this.inProgressBind);
            }
        }

        internal void BindOwner(InstanceOwner owner)
        {
            Fx.Assert(owner != null, "Null owner passed to BindOwner.");

            lock (ThisLock)
            {
                Fx.Assert(this.inProgressBind == null, "How did we get a bind in progress without an owner?");

                Fx.Assert(Owner == null, "BindOwner called when we already have an owner.");
                Owner = owner;
            }
        }

        internal void BindInstance(Guid instanceId)
        {
            Fx.Assert(instanceId != Guid.Empty, "BindInstance called with empty Guid.");

            List<InstanceHandleReference> handlesPendingResolution = null;
            try
            {
                lock (ThisLock)
                {
                    Fx.Assert(Id == Guid.Empty, "Instance already boud in BindInstance.");
                    Id = instanceId;

                    Fx.Assert(OperationPending, "BindInstance should only be called during an operation.");
                    Fx.Assert(AcquirePending == null, "BindInstance should only be called after acquiring the transaction.");
                    if (this.inProgressBind != null)
                    {
                        Fx.Assert(Owner != null, "Must be bound to owner to have an inProgressBind for the lock.");
                        Owner.InstanceBound(ref this.inProgressBind, ref handlesPendingResolution);
                    }
                }
            }
            finally
            {
                InstanceOwner.ResolveHandles(handlesPendingResolution);
            }
        }

        internal void Bind(long instanceVersion)
        {
            Fx.AssertAndThrow(instanceVersion >= 0, "Negative instanceVersion passed to Bind.");
            Fx.Assert(Owner != null, "Bind called before owner bound.");
            Fx.Assert(Id != Guid.Empty, "Bind called before instance bound.");

            lock (ThisLock)
            {
                Fx.AssertAndThrow(Version == -1, "This should only be reachable once per handle.");
                Version = instanceVersion;

                Fx.Assert(OperationPending, "Bind should only be called during an operation.");
                Fx.Assert(AcquirePending == null, "Bind should only be called after acquiring the transaction.");
                if (this.inProgressBind == null)
                {
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.BindLockRequiresCommandFlag));
                }
            }
        }

        // Returns null if an InstanceHandleConflictException should be thrown.
        internal AsyncWaitHandle StartReclaim(long instanceVersion)
        {
            List<InstanceHandleReference> handlesPendingResolution = null;
            try
            {
                lock (ThisLock)
                {
                    Fx.AssertAndThrow(Version == -1, "StartReclaim should only be reachable if the lock hasn't been bound.");

                    Fx.Assert(OperationPending, "StartReclaim should only be called during an operation.");
                    Fx.Assert(AcquirePending == null, "StartReclaim should only be called after acquiring the transaction.");
                    if (this.inProgressBind == null)
                    {
                        throw Fx.Exception.AsError(new InvalidOperationException(SRCore.BindLockRequiresCommandFlag));
                    }

                    Fx.Assert(Owner != null, "Must be bound to owner to have an inProgressBind for the lock in StartReclaim.");
                    return Owner.InitiateLockResolution(instanceVersion, ref this.inProgressBind, ref handlesPendingResolution);
                }
            }
            finally
            {
                InstanceOwner.ResolveHandles(handlesPendingResolution);
            }
        }

        // After calling this method, the caller doesn't need to wait for the wait handle to become set (but they can).
        internal void CancelReclaim(Exception reason)
        {
            List<InstanceHandleReference> handlesPendingResolution = null;
            try
            {
                lock (ThisLock)
                {
                    if (this.inProgressBind == null)
                    {
                        throw Fx.Exception.AsError(new InvalidOperationException(SRCore.DoNotCompleteTryCommandWithPendingReclaim));
                    }

                    Fx.Assert(Owner != null, "Must be bound to owner to have an inProgressBind for the lock in CancelReclaim.");
                    Owner.FaultBind(ref this.inProgressBind, ref handlesPendingResolution, reason);
                }
            }
            finally
            {
                InstanceOwner.ResolveHandles(handlesPendingResolution);
            }
        }

        // Returns the false if an InstanceHandleConflictException should be thrown.
        internal bool FinishReclaim(ref long instanceVersion)
        {
            List<InstanceHandleReference> handlesPendingResolution = null;
            try
            {
                lock (ThisLock)
                {
                    if (this.inProgressBind == null)
                    {
                        throw Fx.Exception.AsError(new InvalidOperationException(SRCore.DoNotCompleteTryCommandWithPendingReclaim));
                    }

                    Fx.Assert(Owner != null, "Must be bound to owner to have an inProgressBind for the lock in CancelReclaim.");
                    if (!Owner.FinishBind(ref this.inProgressBind, ref instanceVersion, ref handlesPendingResolution))
                    {
                        return false;
                    }

                    Fx.AssertAndThrow(Version == -1, "Should only be able to set the version once per handle.");
                    Fx.AssertAndThrow(instanceVersion >= 0, "Incorrect version resulting from conflict resolution.");
                    Version = instanceVersion;
                    return true;
                }
            }
            finally
            {
                InstanceOwner.ResolveHandles(handlesPendingResolution);
            }
        }

        [Fx.Tag.Blocking(CancelMethod = "Free")]
        internal InstancePersistenceContext AcquireExecutionContext(Transaction hostTransaction, TimeSpan timeout)
        {
            bool setOperationPending = false;
            InstancePersistenceContext result = null;
            try
            {
                result = AcquireContextAsyncResult.End(new AcquireContextAsyncResult(this, hostTransaction, timeout, out setOperationPending));
                Fx.AssertAndThrow(result != null, "Null result returned from AcquireContextAsyncResult (synchronous).");
                return result;
            }
            finally
            {
                if (result == null && setOperationPending)
                {
                    FinishOperation();
                }
            }
        }

        internal IAsyncResult BeginAcquireExecutionContext(Transaction hostTransaction, TimeSpan timeout, AsyncCallback callback, object state)
        {
            bool setOperationPending = false;
            IAsyncResult result = null;
            try
            {
                result = new AcquireContextAsyncResult(this, hostTransaction, timeout, out setOperationPending, callback, state);
                return result;
            }
            finally
            {
                if (result == null && setOperationPending)
                {
                    FinishOperation();
                }
            }
        }

        [Fx.Tag.Blocking(CancelMethod = "Free", Conditional = "!result.IsCompleted")]
        internal InstancePersistenceContext EndAcquireExecutionContext(IAsyncResult result)
        {
            return AcquireContextAsyncResult.End(result);
        }

        internal void ReleaseExecutionContext()
        {
            Fx.Assert(OperationPending, "ReleaseExecutionContext called with no operation pending.");
            FinishOperation();
        }

        // Returns null if an InstanceHandleConflictException should be thrown.
        internal InstanceView Commit(InstanceView newState)
        {
            Fx.Assert(newState != null, "Null view passed to Commit.");
            newState.MakeReadOnly();
            View = newState;

            List<InstanceHandleReference> handlesPendingResolution = null;
            InstanceHandle handleToFree = null;
            List<InstancePersistenceEvent> normals = null;
            WaitForEventsAsyncResult resultToComplete = null;
            try
            {
                lock (ThisLock)
                {
                    if (this.inProgressBind != null)
                    {
                        // If there's a Version, it should be committed.
                        if (Version != -1)
                        {
                            if (!Owner.TryCompleteBind(ref this.inProgressBind, ref handlesPendingResolution, out handleToFree))
                            {
                                return null;
                            }
                        }
                        else
                        {
                            Fx.Assert(OperationPending, "Should have cancelled this bind in FinishOperation.");
                            Fx.Assert(AcquirePending == null, "Should not be in Commit during AcquirePending.");
                            Owner.CancelBind(ref this.inProgressBind, ref handlesPendingResolution);
                        }
                    }

                    if (this.pendingOwnerEvents != null && IsValid)
                    {
                        if (this.boundOwnerEvents == null)
                        {
                            this.boundOwnerEvents = new HashSet<XName>();
                        }

                        foreach (InstancePersistenceEvent persistenceEvent in this.pendingOwnerEvents)
                        {
                            if (!this.boundOwnerEvents.Add(persistenceEvent.Name))
                            {
                                Fx.Assert("Should not have conflicts between pending and bound events.");
                                continue;
                            }

                            InstancePersistenceEvent normal = Store.AddHandleToEvent(this, persistenceEvent, Owner);
                            if (normal != null)
                            {
                                if (normals == null)
                                {
                                    normals = new List<InstancePersistenceEvent>(this.pendingOwnerEvents.Count);
                                }
                                normals.Add(normal);
                            }
                        }

                        this.pendingOwnerEvents = null;

                        if (normals != null && this.waitResult != null)
                        {
                            resultToComplete = this.waitResult;
                            this.waitResult = null;
                        }
                    }

                    return View;
                }
            }
            finally
            {
                InstanceOwner.ResolveHandles(handlesPendingResolution);

                // This is a convenience, it is not required for correctness.
                if (handleToFree != null)
                {
                    Fx.Assert(!object.ReferenceEquals(handleToFree, this), "Shouldn't have been told to free ourselves.");
                    handleToFree.Free();
                }

                if (resultToComplete != null)
                {
                    resultToComplete.Signaled(normals);
                }
            }
        }

        void OnPrepare(PreparingEnlistment preparingEnlistment)
        {
            bool prepareNeeded = false;
            lock (ThisLock)
            {
                if (TooLateToEnlist)
                {
                    // Skip this if somehow we already got rolled back or committed.
                    return;
                }
                TooLateToEnlist = true;
                if (OperationPending && AcquirePending == null)
                {
                    Fx.Assert(CurrentExecutionContext != null, "Should either be acquiring or executing in Prepare.");
                    this.pendingPreparingEnlistment = preparingEnlistment;
                }
                else
                {
                    prepareNeeded = true;
                }
            }
            if (prepareNeeded)
            {
                preparingEnlistment.Prepared();
            }
        }

        void OnRollBack(AcquireContextAsyncResult rollingBack)
        {
            bool rollbackNeeded = false;
            lock (ThisLock)
            {
                TooLateToEnlist = true;
                if (OperationPending && AcquirePending == null)
                {
                    Fx.Assert(CurrentExecutionContext != null, "Should either be acquiring or executing in RollBack.");
                    this.pendingRollback = rollingBack;

                    // Don't prepare and roll back.
                    this.pendingPreparingEnlistment = null;
                }
                else
                {
                    rollbackNeeded = true;
                }
            }
            if (rollbackNeeded)
            {
                rollingBack.RollBack();
            }
        }

        void FinishOperation()
        {
            List<InstanceHandleReference> handlesPendingResolution = null;
            try
            {
                bool needNotification;
                PreparingEnlistment preparingEnlistment;
                AcquireContextAsyncResult pendingRollback;
                lock (ThisLock)
                {
                    OperationPending = false;
                    AcquirePending = null;
                    CurrentExecutionContext = null;

                    // This means we could have bound the handle, but didn't - clear the state here.
                    if (this.inProgressBind != null && (Version == -1 || !IsValid))
                    {
                        Owner.CancelBind(ref this.inProgressBind, ref handlesPendingResolution);
                    }
                    else if (Version != -1 && !IsValid)
                    {
                        // This means the handle was successfully bound in the past.  Need to remove it from the table of handles.
                        Owner.Unbind(this);
                    }

                    needNotification = this.needFreedNotification;
                    this.needFreedNotification = false;

                    preparingEnlistment = this.pendingPreparingEnlistment;
                    this.pendingPreparingEnlistment = null;

                    pendingRollback = this.pendingRollback;
                    this.pendingRollback = null;
                }
                try
                {
                    if (needNotification)
                    {
                        Store.FreeInstanceHandle(this, ProviderObject);
                    }
                }
                finally
                {
                    if (pendingRollback != null)
                    {
                        Fx.Assert(preparingEnlistment == null, "Should not have both.");
                        pendingRollback.RollBack();
                    }
                    else if (preparingEnlistment != null)
                    {
                        preparingEnlistment.Prepared();
                    }
                }
            }
            finally
            {
                InstanceOwner.ResolveHandles(handlesPendingResolution);
            }
        }

        List<InstancePersistenceEvent> StartWaiting(WaitForEventsAsyncResult result, IOThreadTimer timeoutTimer, TimeSpan timeout)
        {
            lock (ThisLock)
            {
                if (this.waitResult != null)
                {
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.WaitAlreadyInProgress));
                }
                if (!IsValid)
                {
                    throw Fx.Exception.AsError(new OperationCanceledException(SRCore.HandleFreed));
                }

                if (this.boundOwnerEvents != null && this.boundOwnerEvents.Count > 0)
                {
                    Fx.Assert(Owner != null, "How do we have owner events without an owner.");
                    List<InstancePersistenceEvent> readyEvents = Store.SelectSignaledEvents(this.boundOwnerEvents, Owner);
                    if (readyEvents != null)
                    {
                        Fx.Assert(readyEvents.Count != 0, "Should not return a zero-length list.");
                        return readyEvents;
                    }
                }

                this.waitResult = result;

                // This is done here to be under the lock.  That way it doesn't get canceled before it is set.
                if (timeoutTimer != null)
                {
                    timeoutTimer.Set(timeout);
                }

                return null;
            }
        }

        bool CancelWaiting(WaitForEventsAsyncResult result)
        {
            lock (ThisLock)
            {
                Fx.Assert(result != null, "Null result passed to CancelWaiting.");
                if (!object.ReferenceEquals(this.waitResult, result))
                {
                    return false;
                }
                this.waitResult = null;
                return true;
            }
        }

        internal void EventReady(InstancePersistenceEvent persistenceEvent)
        {
            WaitForEventsAsyncResult resultToComplete = null;
            lock (ThisLock)
            {
                if (this.waitResult != null)
                {
                    resultToComplete = this.waitResult;
                    this.waitResult = null;
                }
            }

            if (resultToComplete != null)
            {
                resultToComplete.Signaled(persistenceEvent);
            }
        }

        internal static IAsyncResult BeginWaitForEvents(InstanceHandle handle, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new WaitForEventsAsyncResult(handle, timeout, callback, state);
        }

        internal static List<InstancePersistenceEvent> EndWaitForEvents(IAsyncResult result)
        {
            return WaitForEventsAsyncResult.End(result);
        }

        class AcquireContextAsyncResult : AsyncResult, IEnlistmentNotification
        {
            static Action<object, TimeoutException> onHostTransaction = new Action<object, TimeoutException>(OnHostTransaction);

            readonly InstanceHandle handle;
            readonly TimeoutHelper timeoutHelper;

            InstancePersistenceContext executionContext;

            public AcquireContextAsyncResult(InstanceHandle handle, Transaction hostTransaction, TimeSpan timeout, out bool setOperationPending, AsyncCallback callback, object state)
                : this(handle, hostTransaction, timeout, out setOperationPending, false, callback, state)
            {
            }

            [Fx.Tag.Blocking(CancelMethod = "Free", CancelDeclaringType = typeof(InstanceHandle))]
            public AcquireContextAsyncResult(InstanceHandle handle, Transaction hostTransaction, TimeSpan timeout, out bool setOperationPending)
                : this(handle, hostTransaction, timeout, out setOperationPending, true, null, null)
            {
            }

            [Fx.Tag.Blocking(CancelMethod = "Free", CancelDeclaringType = typeof(InstanceHandle), Conditional = "synchronous")]
            AcquireContextAsyncResult(InstanceHandle handle, Transaction hostTransaction, TimeSpan timeout, out bool setOperationPending, bool synchronous, AsyncCallback callback, object state)
                : base(callback, state)
            {
                // Need to report back to the caller whether or not we set OperationPending.
                setOperationPending = false;

                this.handle = handle;
                HostTransaction = hostTransaction;
                this.timeoutHelper = new TimeoutHelper(timeout);

                AcquireContextAsyncResult transactionWait;
                bool reuseContext = false;
                lock (this.handle.ThisLock)
                {
                    if (!this.handle.IsValid)
                    {
                        throw Fx.Exception.AsError(new OperationCanceledException(SRCore.HandleFreed));
                    }

                    if (this.handle.OperationPending)
                    {
                        throw Fx.Exception.AsError(new InvalidOperationException(SRCore.CommandExecutionCannotOverlap));
                    }
                    setOperationPending = true;
                    this.handle.OperationPending = true;

                    transactionWait = this.handle.CurrentTransactionalAsyncResult;
                    if (transactionWait != null)
                    {
                        Fx.Assert(this.handle.AcquirePending == null, "Overlapped acquires pending.");

                        // If the transaction matches but is already completed (or completing), the easiest ting to do
                        // is wait for it to complete, then try to re-enlist, and have that failure be the failure mode for Execute.
                        // We do that by following the regular, non-matching transaction path.
                        if (transactionWait.HostTransaction.Equals(hostTransaction) && !this.handle.TooLateToEnlist)
                        {
                            reuseContext = true;
                            this.executionContext = transactionWait.ReuseContext();
                            this.handle.CurrentExecutionContext = this.executionContext;
                        }
                        else
                        {
                            this.handle.AcquirePending = this;
                        }
                    }
                }

                if (transactionWait != null)
                {
                    Fx.Assert(transactionWait.IsCompleted, "Old AsyncResult must be completed by now.");

                    // Reuse the existing InstanceExecutionContext if this is the same transaction we're waiting for.
                    if (reuseContext)
                    {
                        Complete(true);
                        return;
                    }

                    TimeSpan waitTimeout = this.timeoutHelper.RemainingTime();
                    if (synchronous)
                    {
                        if (!transactionWait.WaitForHostTransaction.Wait(waitTimeout))
                        {
                            throw Fx.Exception.AsError(new TimeoutException(InternalSR.TimeoutOnOperation(waitTimeout)));
                        }
                    }
                    else
                    {
                        if (!transactionWait.WaitForHostTransaction.WaitAsync(AcquireContextAsyncResult.onHostTransaction, this, waitTimeout))
                        {
                            return;
                        }
                    }
                }

                if (DoAfterTransaction())
                {
                    Complete(true);
                }
            }

            public Transaction HostTransaction { get; private set; }
            public AsyncWaitHandle WaitForHostTransaction { get; private set; }

            public static InstancePersistenceContext End(IAsyncResult result)
            {
                AcquireContextAsyncResult pThis = AsyncResult.End<AcquireContextAsyncResult>(result);
                Fx.Assert(pThis.executionContext != null, "Somehow the execution context didn't get set.");
                return pThis.executionContext;
            }

            internal void RollBack()
            {
                if (this.executionContext.IsHandleDoomedByRollback)
                {
                    this.handle.Free();
                }
                else
                {
                    Fx.Assert(this.handle.inProgressBind == null, "Either this should have been bound to a lock, hence dooming the handle by rollback, or this should have been cancelled in FinishOperation.");
                    Fx.Assert(this.handle.pendingOwnerEvents == null, "Either this should have doomed the handle or already been committed.");
                    WaitForHostTransaction.Set();
                }
            }

            static void OnHostTransaction(object state, TimeoutException timeoutException)
            {
                AcquireContextAsyncResult pThis = (AcquireContextAsyncResult)state;
                Exception exception = timeoutException;
                bool completeSelf = exception != null;
                if (!completeSelf)
                {
                    try
                    {
                        if (pThis.DoAfterTransaction())
                        {
                            completeSelf = true;
                        }
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        exception = e;
                        completeSelf = true;
                    }
                }
                if (completeSelf)
                {
                    if (exception != null)
                    {
                        pThis.handle.FinishOperation();
                    }
                    pThis.Complete(false, exception);
                }
            }

            bool DoAfterTransaction()
            {
                AcquireContextAsyncResult setWaitTo = null;
                try
                {
                    lock (this.handle.ThisLock)
                    {
                        if (!this.handle.IsValid)
                        {
                            throw Fx.Exception.AsError(new OperationCanceledException(SRCore.HandleFreed));
                        }

                        if (HostTransaction == null)
                        {
                            this.executionContext = new InstancePersistenceContext(this.handle, this.timeoutHelper.RemainingTime());
                        }
                        else
                        {
                            this.executionContext = new InstancePersistenceContext(this.handle, HostTransaction);
                        }

                        this.handle.AcquirePending = null;
                        this.handle.CurrentExecutionContext = this.executionContext;
                        this.handle.TooLateToEnlist = false;
                    }

                    if (HostTransaction != null)
                    {
                        WaitForHostTransaction = new AsyncWaitHandle(EventResetMode.ManualReset);
                        HostTransaction.EnlistVolatile(this, EnlistmentOptions.None);
                        setWaitTo = this;
                    }
                }
                finally
                {
                    this.handle.CurrentTransactionalAsyncResult = setWaitTo;
                }

                return true;
            }

            InstancePersistenceContext ReuseContext()
            {
                Fx.Assert(this.executionContext != null, "ReuseContext called but there is no context.");

                this.executionContext.PrepareForReuse();
                return this.executionContext;
            }

            void IEnlistmentNotification.Commit(Enlistment enlistment)
            {
                Fx.AssertAndThrow(this.handle.CurrentExecutionContext == null, "Prepare should have been called first and waited until after command processing.");

                bool commitSuccessful = this.handle.Commit(this.executionContext.InstanceView) != null;
                enlistment.Done();
                if (commitSuccessful)
                {
                    WaitForHostTransaction.Set();
                }
                else
                {
                    this.handle.Free();
                }
            }

            void IEnlistmentNotification.InDoubt(Enlistment enlistment)
            {
                enlistment.Done();
                this.handle.Free();
            }

            void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
            {
                this.handle.OnPrepare(preparingEnlistment);
            }

            void IEnlistmentNotification.Rollback(Enlistment enlistment)
            {
                enlistment.Done();
                this.handle.OnRollBack(this);
            }
        }

        class WaitForEventsAsyncResult : AsyncResult
        {
            static readonly Action<object> timeoutCallback = new Action<object>(OnTimeout);

            readonly InstanceHandle handle;
            readonly TimeSpan timeout;

            IOThreadTimer timer;

            List<InstancePersistenceEvent> readyEvents;

            internal WaitForEventsAsyncResult(InstanceHandle handle, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.handle = handle;
                this.timeout = timeout;

                if (this.timeout != TimeSpan.Zero && this.timeout != TimeSpan.MaxValue)
                {
                    this.timer = new IOThreadTimer(WaitForEventsAsyncResult.timeoutCallback, this, false);
                }

                List<InstancePersistenceEvent> existingReadyEvents = this.handle.StartWaiting(this, this.timer, this.timeout);
                if (existingReadyEvents == null)
                {
                    if (this.timeout == TimeSpan.Zero)
                    {
                        this.handle.CancelWaiting(this);
                        throw Fx.Exception.AsError(new TimeoutException(SRCore.WaitForEventsTimedOut(TimeSpan.Zero)));
                    }
                }
                else
                {
                    this.readyEvents = existingReadyEvents;
                    Complete(true);
                }
            }

            internal void Signaled(InstancePersistenceEvent persistenceEvent)
            {
                Signaled(new List<InstancePersistenceEvent>(1) { persistenceEvent });
            }

            internal void Signaled(List<InstancePersistenceEvent> persistenceEvents)
            {
                if (this.timer != null)
                {
                    this.timer.Cancel();
                }
                this.readyEvents = persistenceEvents;
                Complete(false);
            }

            internal void Canceled()
            {
                if (this.timer != null)
                {
                    this.timer.Cancel();
                }
                Complete(false, new OperationCanceledException(SRCore.HandleFreed));
            }

            static void OnTimeout(object state)
            {
                WaitForEventsAsyncResult thisPtr = (WaitForEventsAsyncResult)state;
                if (thisPtr.handle.CancelWaiting(thisPtr))
                {
                    thisPtr.Complete(false, new TimeoutException(SRCore.WaitForEventsTimedOut(thisPtr.timeout)));
                }
            }

            internal static List<InstancePersistenceEvent> End(IAsyncResult result)
            {
                return AsyncResult.End<WaitForEventsAsyncResult>(result).readyEvents;
            }
        }
    }
}
