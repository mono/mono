//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Transactions;
    using System.Xml.Linq;

    // Persistence Lock Order:
    // InstanceHandle.ThisLock then InstanceStore.ThisLock
    // InstanceHandle.ThisLock then InstanceOwner.HandlesLock

    [Fx.Tag.XamlVisible(false)]
    public abstract class InstanceStore
    {
        readonly Dictionary<Guid, WeakReference> owners = new Dictionary<Guid, WeakReference>(1);

        Guid[] ownerKeysToScan = new Guid[0];
        int ownerKeysIndexToScan = 0;

        protected InstanceStore()
        {
        }

        object ThisLock
        {
            get
            {
                return this.owners;
            }
        }

        public InstanceOwner DefaultInstanceOwner { get; set; }

        public InstanceHandle CreateInstanceHandle()
        {
            return CreateInstanceHandle(DefaultInstanceOwner);
        }

        public InstanceHandle CreateInstanceHandle(InstanceOwner owner)
        {
            return PrepareInstanceHandle(new InstanceHandle(this, owner));
        }

        public InstanceHandle CreateInstanceHandle(Guid instanceId)
        {
            return CreateInstanceHandle(DefaultInstanceOwner, instanceId);
        }

        public InstanceHandle CreateInstanceHandle(InstanceOwner owner, Guid instanceId)
        {
            if (instanceId == Guid.Empty)
            {
                throw Fx.Exception.Argument("instanceId", SRCore.CannotCreateContextWithNullId);
            }
            return PrepareInstanceHandle(new InstanceHandle(this, owner, instanceId));
        }

        [Fx.Tag.Throws.Timeout("The operation timed out; the InstanceHandle is no longer valid.")]
        [Fx.Tag.Throws(typeof(OperationCanceledException), "The operation was canceled; the InstanceHandle is no longer valid.")]
        [Fx.Tag.Throws(typeof(InstancePersistenceException), "A command failed.")]
        [Fx.Tag.Throws(typeof(TransactionException), "The transaction in use for the command failed.")]
        [Fx.Tag.Blocking(CancelMethod = "Free", CancelDeclaringType = typeof(InstanceHandle))]
        public InstanceView Execute(InstanceHandle handle, InstancePersistenceCommand command, TimeSpan timeout)
        {
            if (command == null)
            {
                throw Fx.Exception.ArgumentNull("command");
            }
            if (handle == null)
            {
                throw Fx.Exception.ArgumentNull("handle");
            }
            if (!object.ReferenceEquals(this, handle.Store))
            {
                throw Fx.Exception.Argument("handle", SRCore.ContextNotFromThisStore);
            }
            TimeoutHelper.ThrowIfNegativeArgument(timeout);

            return InstancePersistenceContext.OuterExecute(handle, command, Transaction.Current, timeout);
        }

        [Fx.Tag.InheritThrows(From = "Execute")]
        public IAsyncResult BeginExecute(InstanceHandle handle, InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (command == null)
            {
                throw Fx.Exception.ArgumentNull("command");
            }
            if (handle == null)
            {
                throw Fx.Exception.ArgumentNull("handle");
            }
            if (!object.ReferenceEquals(this, handle.Store))
            {
                throw Fx.Exception.Argument("handle", SRCore.ContextNotFromThisStore);
            }
            TimeoutHelper.ThrowIfNegativeArgument(timeout);

            return InstancePersistenceContext.BeginOuterExecute(handle, command, Transaction.Current, timeout, callback, state);
        }

        [Fx.Tag.InheritThrows(From = "Execute")]
        [Fx.Tag.Blocking(CancelMethod = "Free", CancelDeclaringType = typeof(InstanceHandle), Conditional = "!result.IsCompleted")]
        public InstanceView EndExecute(IAsyncResult result)
        {
            return InstancePersistenceContext.EndOuterExecute(result);
        }

        [Fx.Tag.Throws.Timeout("The operation timed out.")]
        [Fx.Tag.Throws(typeof(OperationCanceledException), "The operation was canceled; the InstanceHandle is no longer valid.")]
        [Fx.Tag.Blocking(CancelMethod = "Free", CancelDeclaringType = typeof(InstanceHandle), Conditional = "timeout != TimeSpan.Zero")]
        public List<InstancePersistenceEvent> WaitForEvents(InstanceHandle handle, TimeSpan timeout)
        {
            // This has to block on something... might as well be the async result, if the caller is already willing to waste a thread.
            // (The TimeSpan.Zero case isn't fully optimized, but it is special-cased internally to not create timers / wait, it always
            // completes synchronously or throws TimeoutException from BeginWaitForEvents.)
            return EndWaitForEvents(BeginWaitForEvents(handle, timeout, null, null));
        }

        [Fx.Tag.InheritThrows(From = "WaitForEvents")]
        public IAsyncResult BeginWaitForEvents(InstanceHandle handle, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (handle == null)
            {
                throw Fx.Exception.ArgumentNull("handle");
            }
            if (!object.ReferenceEquals(this, handle.Store))
            {
                throw Fx.Exception.Argument("handle", SRCore.ContextNotFromThisStore);
            }
            TimeoutHelper.ThrowIfNegativeArgument(timeout);

            return InstanceHandle.BeginWaitForEvents(handle, timeout, callback, state);
        }

        [Fx.Tag.InheritThrows(From = "WaitForEvents")]
        [Fx.Tag.Blocking(CancelMethod = "Free", CancelDeclaringType = typeof(InstanceHandle), Conditional = "!result.IsCompleted")]
        public List<InstancePersistenceEvent> EndWaitForEvents(IAsyncResult result)
        {
            return InstanceHandle.EndWaitForEvents(result);
        }

        protected void SignalEvent(InstancePersistenceEvent persistenceEvent, InstanceOwner owner)
        {
            if (persistenceEvent == null)
            {
                throw Fx.Exception.ArgumentNull("persistenceEvent");
            }
            if (owner == null)
            {
                throw Fx.Exception.ArgumentNull("owner");
            }

            InstanceNormalEvent normal;
            InstanceHandle[] handlesToNotify = null;
            lock (ThisLock)
            {
                WeakReference ownerReference;
                if (!this.owners.TryGetValue(owner.InstanceOwnerId, out ownerReference) || !object.ReferenceEquals(ownerReference.Target, owner))
                {
                    throw Fx.Exception.Argument("owner", SRCore.OwnerBelongsToWrongStore);
                }

                normal = GetOwnerEventHelper(persistenceEvent, owner);
                if (!normal.IsSignaled)
                {
                    normal.IsSignaled = true;
                    if (normal.BoundHandles.Count > 0)
                    {
                        handlesToNotify = normal.BoundHandles.ToArray();
                    }
                }
            }
            if (handlesToNotify != null)
            {
                foreach (InstanceHandle handle in handlesToNotify)
                {
                    handle.EventReady(normal);
                }
            }
        }

        protected void ResetEvent(InstancePersistenceEvent persistenceEvent, InstanceOwner owner)
        {
            if (persistenceEvent == null)
            {
                throw Fx.Exception.ArgumentNull("persistenceEvent");
            }
            if (owner == null)
            {
                throw Fx.Exception.ArgumentNull("owner");
            }

            InstanceNormalEvent normal;
            lock (ThisLock)
            {
                WeakReference ownerReference;
                if (!this.owners.TryGetValue(owner.InstanceOwnerId, out ownerReference) || !object.ReferenceEquals(ownerReference.Target, owner))
                {
                    throw Fx.Exception.Argument("owner", SRCore.OwnerBelongsToWrongStore);
                }

                if (!owner.Events.TryGetValue(persistenceEvent.Name, out normal))
                {
                    return;
                }

                if (normal.IsSignaled)
                {
                    normal.IsSignaled = false;
                    if (normal.BoundHandles.Count == 0 && normal.PendingHandles.Count == 0)
                    {
                        owner.Events.Remove(persistenceEvent.Name);
                    }
                }
            }
        }

        protected virtual object OnNewInstanceHandle(InstanceHandle instanceHandle)
        {
            return null;
        }

        protected virtual void OnFreeInstanceHandle(InstanceHandle instanceHandle, object userContext)
        {
        }

        [Fx.Tag.InheritThrows(From = "Execute")]
        [Fx.Tag.Blocking(CancelMethod = "Free", CancelDeclaringType = typeof(InstanceHandle))]
        protected internal virtual bool TryCommand(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout)
        {
            return EndTryCommand(BeginTryCommand(context, command, timeout, null, null));
        }

        [Fx.Tag.InheritThrows(From = "TryCommand")]
        protected internal virtual IAsyncResult BeginTryCommand(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult<bool>(false, callback, state);
        }

        [Fx.Tag.InheritThrows(From = "TryCommand")]
        [Fx.Tag.Blocking(CancelMethod = "Free", CancelDeclaringType = typeof(InstanceHandle), Conditional = "!result.IsCompleted")]
        protected internal virtual bool EndTryCommand(IAsyncResult result)
        {
            return CompletedAsyncResult<bool>.End(result);
        }

        protected InstanceOwner[] GetInstanceOwners()
        {
            lock (ThisLock)
            {
                return this.owners.Values.Select(weakReference => (InstanceOwner)weakReference.Target).Where(owner => owner != null).ToArray();
            }
        }

        protected InstancePersistenceEvent[] GetEvents(InstanceOwner owner)
        {
            if (owner == null)
            {
                throw Fx.Exception.ArgumentNull("owner");
            }

            lock (ThisLock)
            {
                WeakReference ownerReference;
                if (!this.owners.TryGetValue(owner.InstanceOwnerId, out ownerReference) || !object.ReferenceEquals(ownerReference.Target, owner))
                {
                    throw Fx.Exception.Argument("owner", SRCore.OwnerBelongsToWrongStore);
                }

                return owner.Events.Values.ToArray();
            }
        }

        internal InstanceOwner GetOrCreateOwner(Guid instanceOwnerId, Guid lockToken)
        {
            lock (ThisLock)
            {
                WeakReference ownerRef;
                InstanceOwner owner;
                if (this.owners.TryGetValue(instanceOwnerId, out ownerRef))
                {
                    owner = (InstanceOwner)ownerRef.Target;
                    if (owner == null)
                    {
                        owner = new InstanceOwner(instanceOwnerId, lockToken);
                        ownerRef.Target = owner;
                    }
                    else if (owner.OwnerToken != lockToken)
                    {
                        throw Fx.Exception.AsError(new InvalidOperationException(SRCore.StoreReportedConflictingLockTokens));
                    }
                }
                else
                {
                    owner = new InstanceOwner(instanceOwnerId, lockToken);
                    this.owners.Add(instanceOwnerId, new WeakReference(owner));
                }

                while (true)
                {
                    if (this.ownerKeysToScan.Length == this.ownerKeysIndexToScan)
                    {
                        this.ownerKeysToScan = new Guid[this.owners.Count];
                        this.owners.Keys.CopyTo(this.ownerKeysToScan, 0);
                        this.ownerKeysIndexToScan = 0;
                        break;
                    }

                    Guid current = this.ownerKeysToScan[this.ownerKeysIndexToScan++];
                    if (this.owners.TryGetValue(current, out ownerRef))
                    {
                        if (ownerRef.Target == null)
                        {
                            this.owners.Remove(current);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                return owner;
            }
        }

        internal void PendHandleToEvent(InstanceHandle handle, InstancePersistenceEvent persistenceEvent, InstanceOwner owner)
        {
            lock (ThisLock)
            {
                Fx.Assert(this.owners.ContainsKey(owner.InstanceOwnerId), "InstanceHandle called PendHandleToEvent on wrong InstanceStore!!");
                Fx.Assert(object.ReferenceEquals(this.owners[owner.InstanceOwnerId].Target, owner), "How did multiple of the same owner become simultaneously active?");

                InstanceNormalEvent normal = GetOwnerEventHelper(persistenceEvent, owner);
                Fx.Assert(!normal.PendingHandles.Contains(handle), "Should not have already pended the handle.");
                Fx.Assert(!normal.BoundHandles.Contains(handle), "Should not be able to pend an already-bound handle.");
                normal.PendingHandles.Add(handle);
            }
        }

        internal InstancePersistenceEvent AddHandleToEvent(InstanceHandle handle, InstancePersistenceEvent persistenceEvent, InstanceOwner owner)
        {
            lock (ThisLock)
            {
                Fx.Assert(this.owners.ContainsKey(owner.InstanceOwnerId), "InstanceHandle called AddHandleToEvent on wrong InstanceStore!!");
                Fx.Assert(object.ReferenceEquals(this.owners[owner.InstanceOwnerId].Target, owner), "How did multiple instances of the same owner become simultaneously active?");

                InstanceNormalEvent normal = GetOwnerEventHelper(persistenceEvent, owner);
                Fx.Assert(normal.PendingHandles.Contains(handle), "Should have already pended the handle.");
                Fx.Assert(!normal.BoundHandles.Contains(handle), "Should not be able to add a handle to an event twice.");
                normal.BoundHandles.Add(handle);
                normal.PendingHandles.Remove(handle);
                return normal.IsSignaled ? normal : null;
            }
        }

        internal List<InstancePersistenceEvent> SelectSignaledEvents(IEnumerable<XName> eventNames, InstanceOwner owner)
        {
            List<InstancePersistenceEvent> readyEvents = null;
            lock (ThisLock)
            {
                Fx.Assert(this.owners.ContainsKey(owner.InstanceOwnerId), "InstanceHandle called SelectSignaledEvents on wrong InstanceStore!!");
                Fx.Assert(object.ReferenceEquals(this.owners[owner.InstanceOwnerId].Target, owner), "How did multiple instances of the same owner become simultaneously active?");

                // Entry must exist since it is still registered by the handle.
                foreach (InstanceNormalEvent normal in eventNames.Select(name => owner.Events[name]))
                {
                    if (normal.IsSignaled)
                    {
                        if (readyEvents == null)
                        {
                            readyEvents = new List<InstancePersistenceEvent>(1);
                        }
                        readyEvents.Add(normal);
                    }
                }
            }
            return readyEvents;
        }

        internal void RemoveHandleFromEvents(InstanceHandle handle, IEnumerable<XName> eventNames, InstanceOwner owner)
        {
            lock (ThisLock)
            {
                Fx.Assert(this.owners.ContainsKey(owner.InstanceOwnerId), "InstanceHandle called RemoveHandleFromEvents on wrong InstanceStore!!");
                Fx.Assert(object.ReferenceEquals(this.owners[owner.InstanceOwnerId].Target, owner), "How did multiple instances of the same owner become simultaneously active in RemoveHandleFromEvents?");

                // Entry must exist since it is still registered by the handle.
                foreach (InstanceNormalEvent normal in eventNames.Select(name => owner.Events[name]))
                {
                    Fx.Assert(normal.BoundHandles.Contains(handle) || normal.PendingHandles.Contains(handle), "Event should still have handle registration.");

                    normal.PendingHandles.Remove(handle);
                    normal.BoundHandles.Remove(handle);
                    if (!normal.IsSignaled && normal.BoundHandles.Count == 0 && normal.PendingHandles.Count == 0)
                    {
                        owner.Events.Remove(normal.Name);
                    }
                }
            }
        }

        // Must be called under ThisLock.  Doesn't validate the InstanceOwner.
        InstanceNormalEvent GetOwnerEventHelper(InstancePersistenceEvent persistenceEvent, InstanceOwner owner)
        {
            InstanceNormalEvent normal;
            if (!owner.Events.TryGetValue(persistenceEvent.Name, out normal))
            {
                normal = new InstanceNormalEvent(persistenceEvent);
                owner.Events.Add(persistenceEvent.Name, normal);
            }
            return normal;
        }

        internal void FreeInstanceHandle(InstanceHandle handle, object providerObject)
        {
            try
            {
                OnFreeInstanceHandle(handle, providerObject);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                throw Fx.Exception.AsError(new CallbackException(SRCore.OnFreeInstanceHandleThrew, exception));
            }
        }

        InstanceHandle PrepareInstanceHandle(InstanceHandle handle)
        {
            handle.ProviderObject = OnNewInstanceHandle(handle);
            return handle;
        }
    }
}
