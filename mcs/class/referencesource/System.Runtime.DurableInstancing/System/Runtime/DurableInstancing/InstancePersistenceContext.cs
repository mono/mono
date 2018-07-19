//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime.DurableInstancing
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;
    using System.Transactions;
    using System.Xml.Linq;
    using System.Runtime.Diagnostics;

    [Fx.Tag.XamlVisible(false)]
    public sealed class InstancePersistenceContext
    {
        readonly TimeSpan timeout;

        Transaction transaction;
        bool freezeTransaction;
        CommittableTransaction myTransaction;
        int cancellationHandlerCalled;
        EventTraceActivity eventTraceActivity;

        internal InstancePersistenceContext(InstanceHandle handle, Transaction transaction)
            : this(handle)
        {
            Fx.Assert(transaction != null, "Null Transaction passed to InstancePersistenceContext.");

            // Let's take our own clone of the transaction. We need to do this because we might need to
            // create a TransactionScope using the transaction and in cases where we are dealing with a
            // transaction that is flowed into the workflow on a message, the DependentTransaction that the
            // dispatcher creates and sets to Transaction.Current may already be Completed by the time a
            // Save operation is done. And since TransactionScope creates a DependentTransaction, it won't
            // be able to.
            // We don't create another DependentClone because we are going to do a EnlistVolatile on the
            // transaction ourselves.
            this.transaction = transaction.Clone();
            IsHostTransaction = true;
            this.eventTraceActivity = handle.EventTraceActivity;
        }

        internal InstancePersistenceContext(InstanceHandle handle, TimeSpan timeout)
            : this(handle)
        {
            this.timeout = timeout;
        }

        InstancePersistenceContext(InstanceHandle handle)
        {
            Fx.Assert(handle != null, "Null handle passed to InstancePersistenceContext.");

            InstanceHandle = handle;

            // Fork a copy of the current view to be the new working view. It starts with no query results.
            InstanceView newView = handle.View.Clone();
            newView.InstanceStoreQueryResults = null;
            InstanceView = newView;

            this.cancellationHandlerCalled = 0;
        }

        public InstanceHandle InstanceHandle { get; private set; }
        public InstanceView InstanceView { get; private set; }

        public long InstanceVersion
        {
            get
            {
                return InstanceHandle.Version;
            }
        }

        internal EventTraceActivity EventTraceActivity
        {
            get
            {
                return this.eventTraceActivity;
            }
        }

        public Guid LockToken
        {
            get
            {
                Fx.Assert(InstanceHandle.Owner == null || InstanceHandle.Owner.OwnerToken == InstanceView.InstanceOwner.OwnerToken, "Mismatched lock tokens.");

                // If the handle doesn't own the lock yet, return the owner LockToken, which is needed to check whether this owner already owns locks.
                return InstanceHandle.Owner == null ? Guid.Empty : InstanceHandle.Owner.OwnerToken;
            }
        }

        public object UserContext
        {
            get
            {
                return InstanceHandle.ProviderObject;
            }
        }

        bool CancelRequested { get; set; }

        ExecuteAsyncResult RootAsyncResult { get; set; }
        ExecuteAsyncResult LastAsyncResult { get; set; }
        bool IsHostTransaction { get; set; }

        bool Active
        {
            get
            {
                return RootAsyncResult != null;
            }
        }

        public void SetCancellationHandler(Action<InstancePersistenceContext> cancellationHandler)
        {
            ThrowIfNotActive("SetCancellationHandler");
            LastAsyncResult.CancellationHandler = cancellationHandler;
            if (CancelRequested && (cancellationHandler != null))
            {
                try
                {
                    if (Interlocked.CompareExchange(ref this.cancellationHandlerCalled, 0, 1) == 0)
                    {
                        cancellationHandler(this);
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw Fx.Exception.AsError(new CallbackException(SRCore.OnCancelRequestedThrew, exception));
                }
            }
        }

        public void BindInstanceOwner(Guid instanceOwnerId, Guid lockToken)
        {
            if (instanceOwnerId == Guid.Empty)
            {
                throw Fx.Exception.Argument("instanceOwnerId", SRCore.GuidCannotBeEmpty);
            }
            if (lockToken == Guid.Empty)
            {
                throw Fx.Exception.Argument("lockToken", SRCore.GuidCannotBeEmpty);
            }
            ThrowIfNotActive("BindInstanceOwner");

            InstanceOwner owner = InstanceHandle.Store.GetOrCreateOwner(instanceOwnerId, lockToken);

            InstanceView.BindOwner(owner);
            IsHandleDoomedByRollback = true;

            InstanceHandle.BindOwner(owner);
        }

        public void BindInstance(Guid instanceId)
        {
            if (instanceId == Guid.Empty)
            {
                throw Fx.Exception.Argument("instanceId", SRCore.GuidCannotBeEmpty);
            }
            ThrowIfNotActive("BindInstance");

            InstanceView.BindInstance(instanceId);
            IsHandleDoomedByRollback = true;

            InstanceHandle.BindInstance(instanceId);
        }

        public void BindEvent(InstancePersistenceEvent persistenceEvent)
        {
            if (persistenceEvent == null)
            {
                throw Fx.Exception.ArgumentNull("persistenceEvent");
            }
            ThrowIfNotActive("BindEvent");

            if (!InstanceView.IsBoundToInstanceOwner)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.ContextMustBeBoundToOwner));
            }
            IsHandleDoomedByRollback = true;

            InstanceHandle.BindOwnerEvent(persistenceEvent);
        }

        public void BindAcquiredLock(long instanceVersion)
        {
            if (instanceVersion < 0)
            {
                throw Fx.Exception.ArgumentOutOfRange("instanceVersion", instanceVersion, SRCore.InvalidLockToken);
            }
            ThrowIfNotActive("BindAcquiredLock");

            // This call has a synchronization, so we are guaranteed it is only successful once.
            InstanceView.BindLock(instanceVersion);
            IsHandleDoomedByRollback = true;

            InstanceHandle.Bind(instanceVersion);
        }

        public void BindReclaimedLock(long instanceVersion, TimeSpan timeout)
        {
            AsyncWaitHandle wait = InitiateBindReclaimedLockHelper("BindReclaimedLock", instanceVersion, timeout);
            if (!wait.Wait(timeout))
            {
                InstanceHandle.CancelReclaim(new TimeoutException(SRCore.TimedOutWaitingForLockResolution));
            }
            ConcludeBindReclaimedLockHelper();
        }

        public IAsyncResult BeginBindReclaimedLock(long instanceVersion, TimeSpan timeout, AsyncCallback callback, object state)
        {
            AsyncWaitHandle wait = InitiateBindReclaimedLockHelper("BeginBindReclaimedLock", instanceVersion, timeout);
            return new BindReclaimedLockAsyncResult(this, wait, timeout, callback, state);
        }

        public void EndBindReclaimedLock(IAsyncResult result)
        {
            BindReclaimedLockAsyncResult.End(result);
        }

        public Exception CreateBindReclaimedLockException(long instanceVersion)
        {
            AsyncWaitHandle wait = InitiateBindReclaimedLockHelper("CreateBindReclaimedLockException", instanceVersion, TimeSpan.MaxValue);
            return new BindReclaimedLockException(wait);
        }

        AsyncWaitHandle InitiateBindReclaimedLockHelper(string methodName, long instanceVersion, TimeSpan timeout)
        {
            if (instanceVersion < 0)
            {
                throw Fx.Exception.ArgumentOutOfRange("instanceVersion", instanceVersion, SRCore.InvalidLockToken);
            }
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            ThrowIfNotActive(methodName);

            // This call has a synchronization, so we are guaranteed it is only successful once.
            InstanceView.StartBindLock(instanceVersion);
            IsHandleDoomedByRollback = true;

            AsyncWaitHandle wait = InstanceHandle.StartReclaim(instanceVersion);
            if (wait == null)
            {
                InstanceHandle.Free();
                throw Fx.Exception.AsError(new InstanceHandleConflictException(LastAsyncResult.CurrentCommand.Name, InstanceView.InstanceId));
            }
            return wait;
        }

        void ConcludeBindReclaimedLockHelper()
        {
            // If FinishReclaim doesn't throw an exception, we are done - the reclaim was successful.
            // The Try / Finally makes up for the reverse order of setting the handle, then the view.
            long instanceVersion = -1;
            try
            {
                if (!InstanceHandle.FinishReclaim(ref instanceVersion))
                {
                    InstanceHandle.Free();
                    throw Fx.Exception.AsError(new InstanceHandleConflictException(LastAsyncResult.CurrentCommand.Name, InstanceView.InstanceId));
                }
                Fx.Assert(instanceVersion >= 0, "Where did the instance version go?");
            }
            finally
            {
                if (instanceVersion >= 0)
                {
                    InstanceView.FinishBindLock(instanceVersion);
                }
            }
        }

        public void PersistedInstance(IDictionary<XName, InstanceValue> data)
        {
            ThrowIfNotLocked();
            ThrowIfCompleted();
            ThrowIfNotTransactional("PersistedInstance");

            InstanceView.InstanceData = data.ReadOnlyCopy(true);
            InstanceView.InstanceDataConsistency = InstanceValueConsistency.None;
            InstanceView.InstanceState = InstanceState.Initialized;
        }

        public void LoadedInstance(InstanceState state, IDictionary<XName, InstanceValue> instanceData, IDictionary<XName, InstanceValue> instanceMetadata, IDictionary<Guid, IDictionary<XName, InstanceValue>> associatedInstanceKeyMetadata, IDictionary<Guid, IDictionary<XName, InstanceValue>> completedInstanceKeyMetadata)
        {
            if (state == InstanceState.Uninitialized)
            {
                if (instanceData != null && instanceData.Count > 0)
                {
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.UninitializedCannotHaveData));
                }
            }
            else if (state == InstanceState.Completed)
            {
                if (associatedInstanceKeyMetadata != null && associatedInstanceKeyMetadata.Count > 0)
                {
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.CompletedMustNotHaveAssociatedKeys));
                }
            }
            else if (state != InstanceState.Initialized)
            {
                throw Fx.Exception.Argument("state", SRCore.InvalidInstanceState);
            }
            ThrowIfNoInstance();
            ThrowIfNotActive("PersistedInstance");

            InstanceValueConsistency consistency = InstanceView.IsBoundToLock || state == InstanceState.Completed ? InstanceValueConsistency.None : InstanceValueConsistency.InDoubt;

            ReadOnlyDictionaryInternal<XName, InstanceValue> instanceDataCopy = instanceData.ReadOnlyCopy(false);
            ReadOnlyDictionaryInternal<XName, InstanceValue> instanceMetadataCopy = instanceMetadata.ReadOnlyCopy(false);

            Dictionary<Guid, InstanceKeyView> keysCopy = null;
            int totalKeys = (associatedInstanceKeyMetadata != null ? associatedInstanceKeyMetadata.Count : 0) + (completedInstanceKeyMetadata != null ? completedInstanceKeyMetadata.Count : 0);
            if (totalKeys > 0)
            {
                keysCopy = new Dictionary<Guid, InstanceKeyView>(totalKeys);
            }
            if (associatedInstanceKeyMetadata != null && associatedInstanceKeyMetadata.Count > 0)
            {
                foreach (KeyValuePair<Guid, IDictionary<XName, InstanceValue>> keyMetadata in associatedInstanceKeyMetadata)
                {
                    InstanceKeyView view = new InstanceKeyView(keyMetadata.Key);
                    view.InstanceKeyState = InstanceKeyState.Associated;
                    view.InstanceKeyMetadata = keyMetadata.Value.ReadOnlyCopy(false);
                    view.InstanceKeyMetadataConsistency = InstanceView.IsBoundToLock ? InstanceValueConsistency.None : InstanceValueConsistency.InDoubt;
                    keysCopy.Add(view.InstanceKey, view);
                }
            }

            if (completedInstanceKeyMetadata != null && completedInstanceKeyMetadata.Count > 0)
            {
                foreach (KeyValuePair<Guid, IDictionary<XName, InstanceValue>> keyMetadata in completedInstanceKeyMetadata)
                {
                    InstanceKeyView view = new InstanceKeyView(keyMetadata.Key);
                    view.InstanceKeyState = InstanceKeyState.Completed;
                    view.InstanceKeyMetadata = keyMetadata.Value.ReadOnlyCopy(false);
                    view.InstanceKeyMetadataConsistency = consistency;
                    keysCopy.Add(view.InstanceKey, view);
                }
            }

            InstanceView.InstanceState = state;

            InstanceView.InstanceData = instanceDataCopy;
            InstanceView.InstanceDataConsistency = consistency;

            InstanceView.InstanceMetadata = instanceMetadataCopy;
            InstanceView.InstanceMetadataConsistency = consistency;

            InstanceView.InstanceKeys = keysCopy == null ? null : new ReadOnlyDictionaryInternal<Guid, InstanceKeyView>(keysCopy);
            InstanceView.InstanceKeysConsistency = consistency;
        }

        public void CompletedInstance()
        {
            ThrowIfNotLocked();
            ThrowIfUninitialized();
            ThrowIfCompleted();
            if ((InstanceView.InstanceKeysConsistency & InstanceValueConsistency.InDoubt) == 0)
            {
                foreach (KeyValuePair<Guid, InstanceKeyView> key in InstanceView.InstanceKeys)
                {
                    if (key.Value.InstanceKeyState == InstanceKeyState.Associated)
                    {
                        throw Fx.Exception.AsError(new InvalidOperationException(SRCore.CannotCompleteWithKeys));
                    }
                }
            }
            ThrowIfNotTransactional("CompletedInstance");

            InstanceView.InstanceState = InstanceState.Completed;
        }

        public void ReadInstanceMetadata(IDictionary<XName, InstanceValue> metadata, bool complete)
        {
            ThrowIfNoInstance();
            ThrowIfNotActive("ReadInstanceMetadata");

            if (InstanceView.InstanceMetadataConsistency == InstanceValueConsistency.None)
            {
                return;
            }

            if (complete)
            {
                InstanceView.InstanceMetadata = metadata.ReadOnlyCopy(false);
                InstanceView.InstanceMetadataConsistency = InstanceView.IsBoundToLock || InstanceView.InstanceState == InstanceState.Completed ? InstanceValueConsistency.None : InstanceValueConsistency.InDoubt;
            }
            else
            {
                if ((InstanceView.IsBoundToLock || InstanceView.InstanceState == InstanceState.Completed) && (InstanceView.InstanceMetadataConsistency & InstanceValueConsistency.InDoubt) != 0)
                {
                    // In this case, prefer throwing out old data and keeping only authoritative data.
                    InstanceView.InstanceMetadata = metadata.ReadOnlyMergeInto(null, false);
                    InstanceView.InstanceMetadataConsistency = InstanceValueConsistency.Partial;
                }
                else
                {
                    InstanceView.InstanceMetadata = metadata.ReadOnlyMergeInto(InstanceView.InstanceMetadata, false);
                    InstanceView.InstanceMetadataConsistency |= InstanceValueConsistency.Partial;
                }
            }
        }

        public void WroteInstanceMetadataValue(XName name, InstanceValue value)
        {
            if (name == null)
            {
                throw Fx.Exception.ArgumentNull("name");
            }
            if (value == null)
            {
                throw Fx.Exception.ArgumentNull("value");
            }
            ThrowIfNotLocked();
            ThrowIfCompleted();
            ThrowIfNotTransactional("WroteInstanceMetadataValue");

            InstanceView.AccumulatedMetadataWrites[name] = value;
        }

        public void AssociatedInstanceKey(Guid key)
        {
            if (key == Guid.Empty)
            {
                throw Fx.Exception.Argument("key", SRCore.InvalidKeyArgument);
            }
            ThrowIfNotLocked();
            ThrowIfCompleted();
            ThrowIfNotTransactional("AssociatedInstanceKey");

            Dictionary<Guid, InstanceKeyView> copy = new Dictionary<Guid, InstanceKeyView>(InstanceView.InstanceKeys);
            if ((InstanceView.InstanceKeysConsistency & InstanceValueConsistency.InDoubt) == 0 && copy.ContainsKey(key))
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.KeyAlreadyAssociated));
            }
            InstanceKeyView keyView = new InstanceKeyView(key);
            keyView.InstanceKeyState = InstanceKeyState.Associated;
            keyView.InstanceKeyMetadataConsistency = InstanceValueConsistency.None;
            copy[keyView.InstanceKey] = keyView;
            InstanceView.InstanceKeys = new ReadOnlyDictionaryInternal<Guid, InstanceKeyView>(copy);
        }

        public void CompletedInstanceKey(Guid key)
        {
            if (key == Guid.Empty)
            {
                throw Fx.Exception.Argument("key", SRCore.InvalidKeyArgument);
            }
            ThrowIfNotLocked();
            ThrowIfCompleted();
            ThrowIfNotTransactional("CompletedInstanceKey");

            InstanceKeyView existingKeyView;
            InstanceView.InstanceKeys.TryGetValue(key, out existingKeyView);
            if ((InstanceView.InstanceKeysConsistency & InstanceValueConsistency.InDoubt) == 0)
            {
                if (existingKeyView != null)
                {
                    if (existingKeyView.InstanceKeyState == InstanceKeyState.Completed)
                    {
                        throw Fx.Exception.AsError(new InvalidOperationException(SRCore.KeyAlreadyCompleted));
                    }
                }
                else if ((InstanceView.InstanceKeysConsistency & InstanceValueConsistency.Partial) == 0)
                {
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.KeyNotAssociated));
                }
            }

            if (existingKeyView != null)
            {
                existingKeyView.InstanceKeyState = InstanceKeyState.Completed;
            }
            else
            {
                Dictionary<Guid, InstanceKeyView> copy = new Dictionary<Guid, InstanceKeyView>(InstanceView.InstanceKeys);
                InstanceKeyView keyView = new InstanceKeyView(key);
                keyView.InstanceKeyState = InstanceKeyState.Completed;
                keyView.InstanceKeyMetadataConsistency = InstanceValueConsistency.Partial;
                copy[keyView.InstanceKey] = keyView;
                InstanceView.InstanceKeys = new ReadOnlyDictionaryInternal<Guid, InstanceKeyView>(copy);
            }
        }

        public void UnassociatedInstanceKey(Guid key)
        {
            if (key == Guid.Empty)
            {
                throw Fx.Exception.Argument("key", SRCore.InvalidKeyArgument);
            }
            ThrowIfNotLocked();
            ThrowIfCompleted();
            ThrowIfNotTransactional("UnassociatedInstanceKey");

            InstanceKeyView existingKeyView;
            InstanceView.InstanceKeys.TryGetValue(key, out existingKeyView);
            if ((InstanceView.InstanceKeysConsistency & InstanceValueConsistency.InDoubt) == 0)
            {
                if (existingKeyView != null)
                {
                    if (existingKeyView.InstanceKeyState == InstanceKeyState.Associated)
                    {
                        throw Fx.Exception.AsError(new InvalidOperationException(SRCore.KeyNotCompleted));
                    }
                }
                else if ((InstanceView.InstanceKeysConsistency & InstanceValueConsistency.Partial) == 0)
                {
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.KeyAlreadyUnassociated));
                }
            }

            if (existingKeyView != null)
            {
                Dictionary<Guid, InstanceKeyView> copy = new Dictionary<Guid, InstanceKeyView>(InstanceView.InstanceKeys);
                copy.Remove(key);
                InstanceView.InstanceKeys = new ReadOnlyDictionaryInternal<Guid, InstanceKeyView>(copy);
            }
        }

        public void ReadInstanceKeyMetadata(Guid key, IDictionary<XName, InstanceValue> metadata, bool complete)
        {
            if (key == Guid.Empty)
            {
                throw Fx.Exception.Argument("key", SRCore.InvalidKeyArgument);
            }
            ThrowIfNoInstance();
            ThrowIfNotActive("ReadInstanceKeyMetadata");

            InstanceKeyView keyView;
            if (!InstanceView.InstanceKeys.TryGetValue(key, out keyView))
            {
                if (InstanceView.InstanceKeysConsistency == InstanceValueConsistency.None)
                {
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.KeyNotAssociated));
                }

                Dictionary<Guid, InstanceKeyView> copy = new Dictionary<Guid, InstanceKeyView>(InstanceView.InstanceKeys);
                keyView = new InstanceKeyView(key);
                if (complete)
                {
                    keyView.InstanceKeyMetadata = metadata.ReadOnlyCopy(false);
                    keyView.InstanceKeyMetadataConsistency = InstanceValueConsistency.None;
                }
                else
                {
                    keyView.InstanceKeyMetadata = metadata.ReadOnlyMergeInto(null, false);
                    keyView.InstanceKeyMetadataConsistency = InstanceValueConsistency.Partial;
                }
                if (!InstanceView.IsBoundToLock && InstanceView.InstanceState != InstanceState.Completed)
                {
                    keyView.InstanceKeyMetadataConsistency |= InstanceValueConsistency.InDoubt;
                }
                copy[keyView.InstanceKey] = keyView;
                InstanceView.InstanceKeys = new ReadOnlyDictionaryInternal<Guid, InstanceKeyView>(copy);
            }
            else
            {
                if (keyView.InstanceKeyMetadataConsistency == InstanceValueConsistency.None)
                {
                    return;
                }

                if (complete)
                {
                    keyView.InstanceKeyMetadata = metadata.ReadOnlyCopy(false);
                    keyView.InstanceKeyMetadataConsistency = InstanceView.IsBoundToLock || InstanceView.InstanceState == InstanceState.Completed ? InstanceValueConsistency.None : InstanceValueConsistency.InDoubt;
                }
                else
                {
                    if ((InstanceView.IsBoundToLock || InstanceView.InstanceState == InstanceState.Completed) && (keyView.InstanceKeyMetadataConsistency & InstanceValueConsistency.InDoubt) != 0)
                    {
                        // In this case, prefer throwing out old data and keeping only authoritative data.
                        keyView.InstanceKeyMetadata = metadata.ReadOnlyMergeInto(null, false);
                        keyView.InstanceKeyMetadataConsistency = InstanceValueConsistency.Partial;
                    }
                    else
                    {
                        keyView.InstanceKeyMetadata = metadata.ReadOnlyMergeInto(keyView.InstanceKeyMetadata, false);
                        keyView.InstanceKeyMetadataConsistency |= InstanceValueConsistency.Partial;
                    }
                }
            }
        }

        public void WroteInstanceKeyMetadataValue(Guid key, XName name, InstanceValue value)
        {
            if (key == Guid.Empty)
            {
                throw Fx.Exception.Argument("key", SRCore.InvalidKeyArgument);
            }
            if (name == null)
            {
                throw Fx.Exception.ArgumentNull("name");
            }
            if (value == null)
            {
                throw Fx.Exception.ArgumentNull("value");
            }
            ThrowIfNotLocked();
            ThrowIfCompleted();
            ThrowIfNotTransactional("WroteInstanceKeyMetadataValue");

            InstanceKeyView keyView;
            if (!InstanceView.InstanceKeys.TryGetValue(key, out keyView))
            {
                if (InstanceView.InstanceKeysConsistency == InstanceValueConsistency.None)
                {
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.KeyNotAssociated));
                }

                if (!value.IsWriteOnly() && !value.IsDeletedValue)
                {
                    Dictionary<Guid, InstanceKeyView> copy = new Dictionary<Guid, InstanceKeyView>(InstanceView.InstanceKeys);
                    keyView = new InstanceKeyView(key);
                    keyView.AccumulatedMetadataWrites.Add(name, value);
                    keyView.InstanceKeyMetadataConsistency = InstanceValueConsistency.Partial;
                    copy[keyView.InstanceKey] = keyView;
                    InstanceView.InstanceKeys = new ReadOnlyDictionaryInternal<Guid, InstanceKeyView>(copy);
                    InstanceView.InstanceKeysConsistency |= InstanceValueConsistency.Partial;
                }
            }
            else
            {
                keyView.AccumulatedMetadataWrites.Add(name, value);
            }
        }

        public void ReadInstanceOwnerMetadata(IDictionary<XName, InstanceValue> metadata, bool complete)
        {
            ThrowIfNoOwner();
            ThrowIfNotActive("ReadInstanceOwnerMetadata");

            if (InstanceView.InstanceOwnerMetadataConsistency == InstanceValueConsistency.None)
            {
                return;
            }

            if (complete)
            {
                InstanceView.InstanceOwnerMetadata = metadata.ReadOnlyCopy(false);
                InstanceView.InstanceOwnerMetadataConsistency = InstanceValueConsistency.InDoubt;
            }
            else
            {
                InstanceView.InstanceOwnerMetadata = metadata.ReadOnlyMergeInto(InstanceView.InstanceOwnerMetadata, false);
                InstanceView.InstanceOwnerMetadataConsistency |= InstanceValueConsistency.Partial;
            }
        }

        public void WroteInstanceOwnerMetadataValue(XName name, InstanceValue value)
        {
            if (name == null)
            {
                throw Fx.Exception.ArgumentNull("name");
            }
            if (value == null)
            {
                throw Fx.Exception.ArgumentNull("value");
            }
            ThrowIfNoOwner();
            ThrowIfNotTransactional("WroteInstanceOwnerMetadataValue");

            InstanceView.AccumulatedOwnerMetadataWrites.Add(name, value);
        }

        public void QueriedInstanceStore(InstanceStoreQueryResult queryResult)
        {
            if (queryResult == null)
            {
                throw Fx.Exception.ArgumentNull("queryResult");
            }
            ThrowIfNotActive("QueriedInstanceStore");

            InstanceView.QueryResultsBacking.Add(queryResult);
        }

        [Fx.Tag.Throws.Timeout("The operation timed out.")]
        [Fx.Tag.Throws(typeof(OperationCanceledException), "The operation was canceled because the InstanceHandle has been freed.")]
        [Fx.Tag.Throws(typeof(InstancePersistenceException), "A command failed.")]
        [Fx.Tag.Blocking(CancelMethod = "NotifyHandleFree")]
        public void Execute(InstancePersistenceCommand command, TimeSpan timeout)
        {
            if (command == null)
            {
                throw Fx.Exception.ArgumentNull("command");
            }
            ThrowIfNotActive("Execute");

            try
            {
                ReconcileTransaction();
                ExecuteAsyncResult.End(new ExecuteAsyncResult(this, command, timeout));
            }
            catch (TimeoutException)
            {
                InstanceHandle.Free();
                throw;
            }
            catch (OperationCanceledException)
            {
                InstanceHandle.Free();
                throw;
            }
        }

        // For each level of hierarchy of command execution, only one BeginExecute may be pending at a time.
        [Fx.Tag.InheritThrows(From = "Execute")]
        public IAsyncResult BeginExecute(InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (command == null)
            {
                throw Fx.Exception.ArgumentNull("command");
            }
            ThrowIfNotActive("BeginExecute");

            try
            {
                ReconcileTransaction();
                return new ExecuteAsyncResult(this, command, timeout, callback, state);
            }
            catch (TimeoutException)
            {
                InstanceHandle.Free();
                throw;
            }
            catch (OperationCanceledException)
            {
                InstanceHandle.Free();
                throw;
            }
        }

        [Fx.Tag.InheritThrows(From = "Execute")]
        [Fx.Tag.Blocking(CancelMethod = "NotifyHandleFree", Conditional = "!result.IsCompleted")]
        public void EndExecute(IAsyncResult result)
        {
            ExecuteAsyncResult.End(result);
        }

        internal Transaction Transaction
        {
            get
            {
                return this.transaction;
            }
        }

        internal bool IsHandleDoomedByRollback { get; private set; }

        internal void RequireTransaction()
        {
            if (this.transaction != null)
            {
                return;
            }
            Fx.AssertAndThrow(!this.freezeTransaction, "RequireTransaction called when transaction is frozen.");
            Fx.AssertAndThrow(Active, "RequireTransaction called when no command is active.");

            // It's ok if some time has passed since the timeout value was acquired, it is ok to run long.  This transaction is not generally responsible
            // for timing out the Execute operation. The exception to this rule is during Commit.
            this.myTransaction = new CommittableTransaction(new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted, Timeout = this.timeout });
            Transaction clone = this.myTransaction.Clone();
            RootAsyncResult.SetInteriorTransaction(this.myTransaction, true);
            this.transaction = clone;
        }

        internal void PrepareForReuse()
        {
            Fx.AssertAndThrow(!Active, "Prior use not yet complete!");
            Fx.AssertAndThrow(IsHostTransaction, "Can only reuse contexts with host transactions.");
        }

        internal void NotifyHandleFree()
        {
            CancelRequested = true;
            ExecuteAsyncResult lastAsyncResult = LastAsyncResult;
            Action<InstancePersistenceContext> onCancel = lastAsyncResult == null ? null : lastAsyncResult.CancellationHandler;
            if (onCancel != null)
            {
                try
                {
                    if (Interlocked.CompareExchange(ref this.cancellationHandlerCalled, 0, 1) == 0)
                    {
                        onCancel(this);
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw Fx.Exception.AsError(new CallbackException(SRCore.OnCancelRequestedThrew, exception));
                }
            }
        }

        [Fx.Tag.Blocking(CancelMethod = "NotifyHandleFree")]
        internal static InstanceView OuterExecute(InstanceHandle initialInstanceHandle, InstancePersistenceCommand command, Transaction transaction, TimeSpan timeout)
        {
            try
            {
                return ExecuteAsyncResult.End(new ExecuteAsyncResult(initialInstanceHandle, command, transaction, timeout));
            }
            catch (TimeoutException)
            {
                initialInstanceHandle.Free();
                throw;
            }
            catch (OperationCanceledException)
            {
                initialInstanceHandle.Free();
                throw;
            }
        }

        internal static IAsyncResult BeginOuterExecute(InstanceHandle initialInstanceHandle, InstancePersistenceCommand command, Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
        {
            try
            {
                return new ExecuteAsyncResult(initialInstanceHandle, command, transaction, timeout, callback, state);
            }
            catch (TimeoutException)
            {
                initialInstanceHandle.Free();
                throw;
            }
            catch (OperationCanceledException)
            {
                initialInstanceHandle.Free();
                throw;
            }
        }

        [Fx.Tag.Blocking(CancelMethod = "NotifyHandleFree", Conditional = "!result.IsCompleted")]
        internal static InstanceView EndOuterExecute(IAsyncResult result)
        {
            InstanceView finalState = ExecuteAsyncResult.End(result);
            if (finalState == null)
            {
                throw Fx.Exception.Argument("result", InternalSR.InvalidAsyncResult);
            }
            return finalState;
        }

        void ThrowIfNotLocked()
        {
            if (!InstanceView.IsBoundToLock)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.InstanceOperationRequiresLock));
            }
        }

        void ThrowIfNoInstance()
        {
            if (!InstanceView.IsBoundToInstance)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.InstanceOperationRequiresInstance));
            }
        }

        void ThrowIfNoOwner()
        {
            if (!InstanceView.IsBoundToInstanceOwner)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.InstanceOperationRequiresOwner));
            }
        }

        void ThrowIfCompleted()
        {
            if (InstanceView.IsBoundToLock && InstanceView.InstanceState == InstanceState.Completed)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.InstanceOperationRequiresNotCompleted));
            }
        }

        void ThrowIfUninitialized()
        {
            if (InstanceView.IsBoundToLock && InstanceView.InstanceState == InstanceState.Uninitialized)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.InstanceOperationRequiresNotUninitialized));
            }
        }

        void ThrowIfNotActive(string methodName)
        {
            if (!Active)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.OutsideInstanceExecutionScope(methodName)));
            }
        }

        void ThrowIfNotTransactional(string methodName)
        {
            ThrowIfNotActive(methodName);
            if (RootAsyncResult.CurrentCommand.IsTransactionEnlistmentOptional)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.OutsideTransactionalCommand(methodName)));
            }
        }

        void ReconcileTransaction()
        {
            // If the provider fails to flow the transaction, that's fine, we don't consider that a request
            // not to use one.
            Transaction transaction = Transaction.Current;
            if (transaction != null)
            {
                if (this.transaction == null)
                {
                    if (this.freezeTransaction)
                    {
                        throw Fx.Exception.AsError(new InvalidOperationException(SRCore.MustSetTransactionOnFirstCall));
                    }
                    RootAsyncResult.SetInteriorTransaction(transaction, false);
                    this.transaction = transaction;
                }
                else if (!transaction.Equals(this.transaction))
                {
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.CannotReplaceTransaction));
                }
            }
            this.freezeTransaction = true;
        }

        class ExecuteAsyncResult : TransactedAsyncResult, ISinglePhaseNotification
        {
            static AsyncCompletion onAcquireContext = new AsyncCompletion(OnAcquireContext);
            static AsyncCompletion onTryCommand = new AsyncCompletion(OnTryCommand);
            static AsyncCompletion onCommit = new AsyncCompletion(OnCommit);
            static Action<object, TimeoutException> onBindReclaimed = new Action<object, TimeoutException>(OnBindReclaimed);
            static Action<object, TimeoutException> onCommitWait = new Action<object, TimeoutException>(OnCommitWait);

            readonly InstanceHandle initialInstanceHandle;
            readonly Stack<IEnumerator<InstancePersistenceCommand>> executionStack;
            readonly TimeoutHelper timeoutHelper;
            readonly ExecuteAsyncResult priorAsyncResult;

            InstancePersistenceContext context;
            CommittableTransaction transactionToCommit;
            IEnumerator<InstancePersistenceCommand> currentExecution;
            AsyncWaitHandle waitForTransaction;
            Action<InstancePersistenceContext> cancellationHandler;
            bool executeCalledByCurrentCommand;
            bool rolledBack;
            bool inDoubt;

            InstanceView finalState;

            public ExecuteAsyncResult(InstanceHandle initialInstanceHandle, InstancePersistenceCommand command, Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
                : this(command, timeout, callback, state)
            {
                this.initialInstanceHandle = initialInstanceHandle;

                OnCompleting = new Action<AsyncResult, Exception>(SimpleCleanup);

                IAsyncResult result = this.initialInstanceHandle.BeginAcquireExecutionContext(transaction, this.timeoutHelper.RemainingTime(), PrepareAsyncCompletion(ExecuteAsyncResult.onAcquireContext), this);
                if (result.CompletedSynchronously)
                {
                    // After this stage, must complete explicitly in order to get Cleanup to run correctly.
                    bool completeSelf = false;
                    Exception completionException = null;
                    try
                    {
                        completeSelf = OnAcquireContext(result);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        completeSelf = true;
                        completionException = exception;
                    }
                    if (completeSelf)
                    {
                        Complete(true, completionException);
                    }
                }
            }

            public ExecuteAsyncResult(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
                : this(command, timeout, callback, state)
            {
                this.context = context;

                this.priorAsyncResult = this.context.LastAsyncResult;
                Fx.Assert(this.priorAsyncResult != null, "The LastAsyncResult should already have been checked.");
                this.priorAsyncResult.executeCalledByCurrentCommand = true;

                OnCompleting = new Action<AsyncResult, Exception>(SimpleCleanup);

                bool completeSelf = false;
                bool success = false;
                try
                {
                    this.context.LastAsyncResult = this;
                    if (RunLoop())
                    {
                        completeSelf = true;
                    }
                    success = true;
                }
                finally
                {
                    if (!success)
                    {
                        this.context.LastAsyncResult = this.priorAsyncResult;
                    }
                }
                if (completeSelf)
                {
                    Complete(true);
                }
            }

            [Fx.Tag.Blocking(CancelMethod = "NotifyHandleFree", CancelDeclaringType = typeof(InstancePersistenceContext))]
            public ExecuteAsyncResult(InstanceHandle initialInstanceHandle, InstancePersistenceCommand command, Transaction transaction, TimeSpan timeout)
                : this(command, timeout, null, null)
            {
                this.initialInstanceHandle = initialInstanceHandle;
                this.context = this.initialInstanceHandle.AcquireExecutionContext(transaction, this.timeoutHelper.RemainingTime());

                Exception completionException = null;
                try
                {
                    // After this stage, must complete explicitly in order to get Cleanup to run correctly.
                    this.context.RootAsyncResult = this;
                    this.context.LastAsyncResult = this;
                    OnCompleting = new Action<AsyncResult, Exception>(Cleanup);

                    RunLoopCore(true);

                    if (this.transactionToCommit != null)
                    {
                        try
                        {
                            this.transactionToCommit.Commit();
                        }
                        catch (TransactionException)
                        {
                            // Since we are enlisted in this transaction, we can ignore exceptions from Commit.
                        }
                        this.transactionToCommit = null;
                    }

                    DoWaitForTransaction(true);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    completionException = exception;
                }
                Complete(true, completionException);
            }

            [Fx.Tag.Blocking(CancelMethod = "NotifyHandleFree", CancelDeclaringType = typeof(InstancePersistenceContext))]
            public ExecuteAsyncResult(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout)
                : this(command, timeout, null, null)
            {
                this.context = context;

                this.priorAsyncResult = this.context.LastAsyncResult;
                Fx.Assert(this.priorAsyncResult != null, "The LastAsyncResult should already have been checked.");
                this.priorAsyncResult.executeCalledByCurrentCommand = true;

                bool success = false;
                try
                {
                    this.context.LastAsyncResult = this;
                    RunLoopCore(true);
                    success = true;
                }
                finally
                {
                    this.context.LastAsyncResult = this.priorAsyncResult;
                    if (!success && this.context.IsHandleDoomedByRollback)
                    {
                        this.context.InstanceHandle.Free();
                    }
                }
                Complete(true);
            }

            ExecuteAsyncResult(InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.executionStack = new Stack<IEnumerator<InstancePersistenceCommand>>(2);
                this.timeoutHelper = new TimeoutHelper(timeout);

                this.currentExecution = (new List<InstancePersistenceCommand> { command }).GetEnumerator();
            }

            internal InstancePersistenceCommand CurrentCommand { get; private set; }

            internal Action<InstancePersistenceContext> CancellationHandler
            {
                get
                {
                    Action<InstancePersistenceContext> handler = this.cancellationHandler;
                    ExecuteAsyncResult current = this;
                    while (handler == null)
                    {
                        current = current.priorAsyncResult;
                        if (current == null)
                        {
                            break;
                        }
                        handler = current.cancellationHandler;
                    }
                    return handler;
                }

                set
                {
                    this.cancellationHandler = value;
                }
            }

            public void SetInteriorTransaction(Transaction interiorTransaction, bool needsCommit)
            {
                Fx.Assert(!this.context.IsHostTransaction, "SetInteriorTransaction called for a host transaction.");

                if (this.waitForTransaction != null)
                {
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.ExecuteMustBeNested));
                }

                bool success = false;
                try
                {
                    this.waitForTransaction = new AsyncWaitHandle(EventResetMode.ManualReset);
                    interiorTransaction.EnlistVolatile(this, EnlistmentOptions.None);
                    success = true;
                }
                finally
                {
                    if (!success)
                    {
                        if (this.waitForTransaction != null)
                        {
                            this.waitForTransaction.Set();
                        }
                    }
                    else if (needsCommit)
                    {
                        this.transactionToCommit = (CommittableTransaction)interiorTransaction;
                    }
                }
            }

            [Fx.Tag.Blocking(CancelMethod = "NotifyHandleFree", CancelDeclaringType = typeof(InstancePersistenceContext), Conditional = "!result.IsCOmpleted")]
            public static InstanceView End(IAsyncResult result)
            {
                ExecuteAsyncResult thisPtr = AsyncResult.End<ExecuteAsyncResult>(result);
                Fx.Assert((thisPtr.finalState == null) == (thisPtr.initialInstanceHandle == null), "Should have thrown an exception if this is null on the outer result.");
                return thisPtr.finalState;
            }

            static bool OnAcquireContext(IAsyncResult result)
            {
                ExecuteAsyncResult thisPtr = (ExecuteAsyncResult)result.AsyncState;
                thisPtr.context = thisPtr.initialInstanceHandle.EndAcquireExecutionContext(result);
                thisPtr.context.RootAsyncResult = thisPtr;
                thisPtr.context.LastAsyncResult = thisPtr;
                thisPtr.OnCompleting = new Action<AsyncResult, Exception>(thisPtr.Cleanup);
                return thisPtr.RunLoop();
            }

            [Fx.Tag.Blocking(CancelMethod = "NotifyHandleFree", CancelDeclaringType = typeof(InstancePersistenceContext), Conditional = "synchronous")]
            bool RunLoopCore(bool synchronous)
            {
                while (this.currentExecution != null)
                {
                    if (this.currentExecution.MoveNext())
                    {
                        bool isFirstCommand = CurrentCommand == null;
                        this.executeCalledByCurrentCommand = false;
                        CurrentCommand = this.currentExecution.Current;

                        Fx.Assert(isFirstCommand || this.executionStack.Count > 0, "The first command should always remain at the top of the stack.");

                        if (isFirstCommand)
                        {
                            if (this.priorAsyncResult != null)
                            {
                                if (this.priorAsyncResult.CurrentCommand.IsTransactionEnlistmentOptional && !CurrentCommand.IsTransactionEnlistmentOptional)
                                {
                                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.CannotInvokeTransactionalFromNonTransactional));
                                }
                            }
                        }
                        else if (this.executionStack.Peek().Current.IsTransactionEnlistmentOptional)
                        {
                            if (!CurrentCommand.IsTransactionEnlistmentOptional)
                            {
                                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.CannotInvokeTransactionalFromNonTransactional));
                            }
                        }
                        else if (this.priorAsyncResult == null)
                        {
                            // This is not the first command. Since the whole thing wasn't done at once by the
                            // provider, force a transaction if the first command required one.
                            this.context.RequireTransaction();
                        }

                        // Intentionally calling MayBindLockToInstanceHandle prior to Validate.  This is a publically visible order.
                        bool mayBindLockToInstanceHandle = CurrentCommand.AutomaticallyAcquiringLock;
                        CurrentCommand.Validate(this.context.InstanceView);

                        if (mayBindLockToInstanceHandle)
                        {
                            if (isFirstCommand)
                            {
                                if (this.priorAsyncResult != null)
                                {
                                    if (!this.priorAsyncResult.CurrentCommand.AutomaticallyAcquiringLock)
                                    {
                                        throw Fx.Exception.AsError(new InvalidOperationException(SRCore.CannotInvokeBindingFromNonBinding));
                                    }
                                }
                                else if (!this.context.InstanceView.IsBoundToInstanceOwner)
                                {
                                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.MayBindLockCommandShouldValidateOwner));
                                }
                                else if (!this.context.InstanceView.IsBoundToLock)
                                {
                                    // This is the first command in the set and it may lock, so we must start the bind.
                                    this.context.InstanceHandle.StartPotentialBind();
                                }
                            }
                            else if (!this.executionStack.Peek().Current.AutomaticallyAcquiringLock)
                            {
                                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.CannotInvokeBindingFromNonBinding));
                            }
                        }

                        if (this.context.CancelRequested)
                        {
                            throw Fx.Exception.AsError(new OperationCanceledException(SRCore.HandleFreed));
                        }

                        BindReclaimedLockException bindReclaimedLockException = null;
                        if (synchronous)
                        {
                            bool commandProcessed;
                            TransactionScope txScope = null;
                            try
                            {
                                txScope = TransactionHelper.CreateTransactionScope(this.context.Transaction);
                                commandProcessed = this.context.InstanceHandle.Store.TryCommand(this.context, CurrentCommand, this.timeoutHelper.RemainingTime());
                            }
                            catch (BindReclaimedLockException exception)
                            {
                                bindReclaimedLockException = exception;
                                commandProcessed = true;
                            }
                            finally
                            {
                                TransactionHelper.CompleteTransactionScope(ref txScope);
                            }
                            AfterCommand(commandProcessed);
                            if (bindReclaimedLockException != null)
                            {
                                BindReclaimed(!bindReclaimedLockException.MarkerWaitHandle.Wait(this.timeoutHelper.RemainingTime()));
                            }
                        }
                        else
                        {
                            IAsyncResult result;
                            using (PrepareTransactionalCall(this.context.Transaction))
                            {
                                try
                                {
                                    result = this.context.InstanceHandle.Store.BeginTryCommand(this.context, CurrentCommand, this.timeoutHelper.RemainingTime(), PrepareAsyncCompletion(ExecuteAsyncResult.onTryCommand), this);
                                }
                                catch (BindReclaimedLockException exception)
                                {
                                    bindReclaimedLockException = exception;
                                    result = null;
                                }
                            }
                            if (result == null)
                            {
                                AfterCommand(true);
                                if (!bindReclaimedLockException.MarkerWaitHandle.WaitAsync(ExecuteAsyncResult.onBindReclaimed, this, this.timeoutHelper.RemainingTime()))
                                {
                                    return false;
                                }
                                BindReclaimed(false);
                            }
                            else
                            {
                                if (!CheckSyncContinue(result) || !DoEndCommand(result))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    else if (this.executionStack.Count > 0)
                    {
                        this.currentExecution = this.executionStack.Pop();
                    }
                    else
                    {
                        this.currentExecution = null;
                    }
                }

                CurrentCommand = null;
                return true;
            }

            bool RunLoop()
            {
                if (!RunLoopCore(false))
                {
                    return false;
                }

                // If this is an inner command, return true right away to continue this execution episode in a different async result.
                if (this.initialInstanceHandle == null)
                {
                    return true;
                }

                // This is is an outer scope.  We need to commit and/or wait for commit if necessary.
                if (this.transactionToCommit != null)
                {
                    IAsyncResult result = null;
                    try
                    {
                        result = this.transactionToCommit.BeginCommit(PrepareAsyncCompletion(ExecuteAsyncResult.onCommit), this);
                    }
                    catch (TransactionException)
                    {
                        // Since we are enlisted in the transaction, we can ignore exceptions from Commit.
                        this.transactionToCommit = null;
                    }
                    if (result != null)
                    {
                        return result.CompletedSynchronously ? OnCommit(result) : false;
                    }
                }

                return DoWaitForTransaction(false);
            }

            static bool OnTryCommand(IAsyncResult result)
            {
                ExecuteAsyncResult thisPtr = (ExecuteAsyncResult)result.AsyncState;
                return thisPtr.DoEndCommand(result) && thisPtr.RunLoop();
            }

            [Fx.Tag.GuaranteeNonBlocking]
            bool DoEndCommand(IAsyncResult result)
            {
                bool commandProcessed;
                BindReclaimedLockException bindReclaimedLockException = null;
                try
                {
                    commandProcessed = this.context.InstanceHandle.Store.EndTryCommand(result);
                }
                catch (BindReclaimedLockException exception)
                {
                    bindReclaimedLockException = exception;
                    commandProcessed = true;
                }
                AfterCommand(commandProcessed);
                if (bindReclaimedLockException != null)
                {
                    if (!bindReclaimedLockException.MarkerWaitHandle.WaitAsync(ExecuteAsyncResult.onBindReclaimed, this, this.timeoutHelper.RemainingTime()))
                    {
                        return false;
                    }
                    BindReclaimed(false);
                }
                return true;
            }

            void AfterCommand(bool commandProcessed)
            {
                if (!object.ReferenceEquals(this.context.LastAsyncResult, this))
                {
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.ExecuteMustBeNested));
                }
                if (!commandProcessed)
                {
                    if (this.executeCalledByCurrentCommand)
                    {
                        throw Fx.Exception.AsError(new InvalidOperationException(SRCore.TryCommandCannotExecuteSubCommandsAndReduce));
                    }
                    IEnumerable<InstancePersistenceCommand> reduction = CurrentCommand.Reduce(this.context.InstanceView);
                    if (reduction == null)
                    {
                        throw Fx.Exception.AsError(new NotSupportedException(SRCore.ProviderDoesNotSupportCommand(CurrentCommand.Name)));
                    }
                    this.executionStack.Push(this.currentExecution);
                    this.currentExecution = reduction.GetEnumerator();
                }
            }

            static void OnBindReclaimed(object state, TimeoutException timeoutException)
            {
                ExecuteAsyncResult thisPtr = (ExecuteAsyncResult)state;

                bool completeSelf;
                Exception completionException = null;
                try
                {
                    thisPtr.BindReclaimed(timeoutException != null);
                    completeSelf = thisPtr.RunLoop();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    completionException = exception;
                    completeSelf = true;
                }
                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }

            void BindReclaimed(bool timedOut)
            {
                if (timedOut)
                {
                    this.context.InstanceHandle.CancelReclaim(new TimeoutException(SRCore.TimedOutWaitingForLockResolution));
                }
                this.context.ConcludeBindReclaimedLockHelper();

                // If we get here, the reclaim attempt succeeded and we own the lock - but we are in the
                // CreateBindReclaimedLockException path, which auto-cancels on success.
                this.context.InstanceHandle.Free();
                throw Fx.Exception.AsError(new OperationCanceledException(SRCore.BindReclaimSucceeded));
            }

            [Fx.Tag.GuaranteeNonBlocking]
            static bool OnCommit(IAsyncResult result)
            {
                ExecuteAsyncResult thisPtr = (ExecuteAsyncResult)result.AsyncState;
                try
                {
                    thisPtr.transactionToCommit.EndCommit(result);
                }
                catch (TransactionException)
                {
                    // Since we are enlisted in the transaction, we can ignore exceptions from Commit.
                }
                thisPtr.transactionToCommit = null;
                return thisPtr.DoWaitForTransaction(false);
            }

            [Fx.Tag.Blocking(CancelMethod = "NotifyHandleFree", CancelDeclaringType = typeof(InstancePersistenceContext), Conditional = "synchronous")]
            bool DoWaitForTransaction(bool synchronous)
            {
                if (this.waitForTransaction != null)
                {
                    if (synchronous)
                    {
                        TimeSpan waitTimeout = this.timeoutHelper.RemainingTime();
                        if (!this.waitForTransaction.Wait(waitTimeout))
                        {
                            throw Fx.Exception.AsError(new TimeoutException(InternalSR.TimeoutOnOperation(waitTimeout)));
                        }
                    }
                    else
                    {
                        if (!this.waitForTransaction.WaitAsync(ExecuteAsyncResult.onCommitWait, this, this.timeoutHelper.RemainingTime()))
                        {
                            return false;
                        }
                    }
                    Exception exception = AfterCommitWait();
                    if (exception != null)
                    {
                        throw Fx.Exception.AsError(exception);
                    }
                }
                else if (this.context.IsHostTransaction)
                {
                    // For host transactions, we need to provide a clone of the intermediate state as the final state.
                    this.finalState = this.context.InstanceView.Clone();
                    this.finalState.MakeReadOnly();

                    // The intermediate state should have the query results cleared - they are per-call of Execute.
                    this.context.InstanceView.InstanceStoreQueryResults = null;
                }
                else
                {
                    // If we get here, there's no transaction at all.  Need to "commit" the intermediate state.
                    CommitHelper();
                    if (this.finalState == null)
                    {
                        this.context.InstanceHandle.Free();
                        throw Fx.Exception.AsError(new InstanceHandleConflictException(null, this.context.InstanceView.InstanceId));
                    }
                }
                return true;
            }

            static void OnCommitWait(object state, TimeoutException exception)
            {
                ExecuteAsyncResult thisPtr = (ExecuteAsyncResult)state;
                thisPtr.Complete(false, exception ?? thisPtr.AfterCommitWait());
            }

            Exception AfterCommitWait()
            {
                if (this.inDoubt)
                {
                    this.context.InstanceHandle.Free();
                    return new TransactionInDoubtException(SRCore.TransactionInDoubtNonHost);
                }
                if (this.rolledBack)
                {
                    if (this.context.IsHandleDoomedByRollback)
                    {
                        this.context.InstanceHandle.Free();
                    }
                    return new TransactionAbortedException(SRCore.TransactionRolledBackNonHost);
                }
                if (this.finalState == null)
                {
                    this.context.InstanceHandle.Free();
                    return new InstanceHandleConflictException(null, this.context.InstanceView.InstanceId);
                }
                return null;
            }

            void CommitHelper()
            {
                this.finalState = this.context.InstanceHandle.Commit(this.context.InstanceView);
            }

            void SimpleCleanup(AsyncResult result, Exception exception)
            {
                if (this.initialInstanceHandle == null)
                {
                    Fx.Assert(this.priorAsyncResult != null, "In the non-outer case, we should always have a priorAsyncResult here, since we set it before ----igining OnComplete.");
                    this.context.LastAsyncResult = this.priorAsyncResult;
                }
                if (exception != null)
                {
                    if (this.context != null && this.context.IsHandleDoomedByRollback)
                    {
                        this.context.InstanceHandle.Free();
                    }
                    else if (exception is TimeoutException || exception is OperationCanceledException)
                    {
                        if (this.context == null)
                        {
                            this.initialInstanceHandle.Free();
                        }
                        else
                        {
                            this.context.InstanceHandle.Free();
                        }
                    }
                }
            }

            void Cleanup(AsyncResult result, Exception exception)
            {
                try
                {
                    SimpleCleanup(result, exception);
                    if (this.transactionToCommit != null)
                    {
                        try
                        {
                            this.transactionToCommit.Rollback(exception);
                        }
                        catch (TransactionException)
                        {
                        }
                    }
                }
                finally
                {
                    Fx.AssertAndThrowFatal(this.context.Active, "Out-of-sync between InstanceExecutionContext and ExecutionAsyncResult.");

                    this.context.LastAsyncResult = null;
                    this.context.RootAsyncResult = null;
                    this.context.InstanceHandle.ReleaseExecutionContext();
                }
            }

            void ISinglePhaseNotification.SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
            {
                CommitHelper();
                singlePhaseEnlistment.Committed();
                this.waitForTransaction.Set();
            }

            void IEnlistmentNotification.Commit(Enlistment enlistment)
            {
                CommitHelper();
                enlistment.Done();
                this.waitForTransaction.Set();
            }

            void IEnlistmentNotification.InDoubt(Enlistment enlistment)
            {
                enlistment.Done();
                this.inDoubt = true;
                this.waitForTransaction.Set();
            }

            void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
            {
                preparingEnlistment.Prepared();
            }

            void IEnlistmentNotification.Rollback(Enlistment enlistment)
            {
                enlistment.Done();
                this.rolledBack = true;
                this.waitForTransaction.Set();
            }
        }

        class BindReclaimedLockAsyncResult : AsyncResult
        {
            static Action<object, TimeoutException> waitComplete = new Action<object, TimeoutException>(OnWaitComplete);

            readonly InstancePersistenceContext context;

            public BindReclaimedLockAsyncResult(InstancePersistenceContext context, AsyncWaitHandle wait, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.context = context;

                if (wait.WaitAsync(BindReclaimedLockAsyncResult.waitComplete, this, timeout))
                {
                    this.context.ConcludeBindReclaimedLockHelper();
                    Complete(true);
                }
            }

            static void OnWaitComplete(object state, TimeoutException timeoutException)
            {
                BindReclaimedLockAsyncResult thisPtr = (BindReclaimedLockAsyncResult)state;

                Exception completionException = null;
                try
                {
                    if (timeoutException != null)
                    {
                        thisPtr.context.InstanceHandle.CancelReclaim(new TimeoutException(SRCore.TimedOutWaitingForLockResolution));
                    }
                    thisPtr.context.ConcludeBindReclaimedLockHelper();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    completionException = exception;
                }
                thisPtr.Complete(false, completionException);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<BindReclaimedLockAsyncResult>(result);
            }
        }

        [Serializable]
        class BindReclaimedLockException : Exception
        {
            public BindReclaimedLockException()
            {
            }

            internal BindReclaimedLockException(AsyncWaitHandle markerWaitHandle)
                : base(SRCore.BindReclaimedLockException)
            {
                MarkerWaitHandle = markerWaitHandle;
            }

            internal AsyncWaitHandle MarkerWaitHandle { get; private set; }

            [SecurityCritical]
            protected BindReclaimedLockException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }
        }
    }
}
