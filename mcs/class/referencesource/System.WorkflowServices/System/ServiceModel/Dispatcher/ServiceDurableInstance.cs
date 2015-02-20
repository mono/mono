//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Persistence;
    using System.Transactions;
    using System.Diagnostics;

    class ServiceDurableInstance : DurableInstance
    {
        bool abortInstance;
        DependentTransaction clonedTransaction;
        ServiceDurableInstanceContextProvider contextManager;
        bool existsInPersistence;
        object instance;
        LockingPersistenceProvider lockingProvider;
        bool markedForCompletion;
        Type newServiceType;
        TimeSpan operationTimeout;
        int outstandingOperations;
        PersistenceProvider provider;
        DurableRuntimeValidator runtimeValidator;
        bool saveStateInOperationTransaction;
        UnknownExceptionAction unknownExceptionAction;

        public ServiceDurableInstance(
            PersistenceProvider persistenceProvider,
            ServiceDurableInstanceContextProvider contextManager,
            bool saveStateInOperationTransaction,
            UnknownExceptionAction unknownExceptionAction,
            DurableRuntimeValidator runtimeValidator,
            TimeSpan operationTimeout)
            : this(persistenceProvider, contextManager, saveStateInOperationTransaction, unknownExceptionAction, runtimeValidator, operationTimeout, null)
        {
        }

        public ServiceDurableInstance(
            PersistenceProvider persistenceProvider,
            ServiceDurableInstanceContextProvider contextManager,
            bool saveStateInOperationTransaction,
            UnknownExceptionAction unknownExceptionAction,
            DurableRuntimeValidator runtimeValidator,
            TimeSpan operationTimeout,
            Type serviceType)
            : base(contextManager, persistenceProvider == null ? Guid.Empty : persistenceProvider.Id)
        {
            if (persistenceProvider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("persistenceProvider");
            }

            if (contextManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contextManager");
            }

            if (runtimeValidator == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("runtimeValidator");
            }

            Fx.Assert(operationTimeout > TimeSpan.Zero,
                "Timeout needs to be greater than zero.");

            this.lockingProvider = persistenceProvider as LockingPersistenceProvider;
            this.provider = persistenceProvider;
            this.contextManager = contextManager;
            this.saveStateInOperationTransaction = saveStateInOperationTransaction;
            this.unknownExceptionAction = unknownExceptionAction;
            this.runtimeValidator = runtimeValidator;
            this.operationTimeout = operationTimeout;
            this.newServiceType = serviceType;
        }

        enum OperationType
        {
            None = 0,
            Delete = 1,
            Unlock = 2,
            Create = 3,
            Update = 4
        }

        public object Instance
        {
            get
            {
                return this.instance;
            }
        }

        public void AbortInstance()
        {
            ConcurrencyMode concurrencyMode = this.runtimeValidator.ConcurrencyMode;

            if (concurrencyMode != ConcurrencyMode.Single)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(
                    SR2.GetString(SR2.AbortInstanceRequiresSingle)));
            }

            if (this.saveStateInOperationTransaction)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(
                    SR2.GetString(SR2.CannotAbortWithSaveStateInTransaction)));
            }

            if (this.markedForCompletion)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(
                    SR2.GetString(
                    SR2.DurableOperationMethodInvalid,
                    "AbortInstance",
                    "CompleteInstance")));
            }

            this.abortInstance = true;
        }

        public IAsyncResult BeginFinishOperation(bool completeInstance, bool performPersistence, Exception operationException, AsyncCallback callback, object state)
        {
            return new FinishOperationAsyncResult(this, completeInstance, performPersistence, operationException, callback, state);
        }

        public IAsyncResult BeginStartOperation(bool canCreateInstance, AsyncCallback callback, object state)
        {
            return new StartOperationAsyncResult(this, canCreateInstance, callback, state);
        }

        public void EndFinishOperation(IAsyncResult result)
        {
            FinishOperationAsyncResult.End(result);
        }

        public object EndStartOperation(IAsyncResult result)
        {
            return StartOperationAsyncResult.End(result);
        }

        public void FinishOperation(bool completeInstance, bool performPersistence, Exception operationException)
        {
            try
            {
                bool disposeInstance;
                OperationType operation = FinishOperationCommon(completeInstance, operationException, out disposeInstance);

                Fx.Assert(
                    (performPersistence || (operation != OperationType.Delete && operation != OperationType.Unlock)),
                    "If we aren't performing persistence then we are a NotAllowed contract and therefore should never have loaded from persistence.");

                if (performPersistence)
                {
                    switch (operation)
                    {
                        case OperationType.Unlock:
                            // Do the null check out here to avoid creating the scope
                            if (this.lockingProvider != null)
                            {
                                using (PersistenceScope scope = new PersistenceScope(
                                    this.saveStateInOperationTransaction,
                                    this.clonedTransaction))
                                {
                                    this.lockingProvider.Unlock(this.operationTimeout);
                                }
                            }
                            break;
                        case OperationType.Delete:
                            using (PersistenceScope scope = new PersistenceScope(
                                this.saveStateInOperationTransaction,
                                this.clonedTransaction))
                            {
                                this.provider.Delete(this.instance, this.operationTimeout);

                                if (DiagnosticUtility.ShouldTraceInformation)
                                {
                                    string traceText = SR.GetString(SR.TraceCodeServiceDurableInstanceDeleted, this.InstanceId);
                                    TraceUtility.TraceEvent(TraceEventType.Information,
                                        TraceCode.ServiceDurableInstanceDeleted, traceText,
                                        new StringTraceRecord("DurableInstanceDetail", traceText),
                                        this, null);
                                }
                            }
                            break;
                        case OperationType.Create:
                            using (PersistenceScope scope = new PersistenceScope(
                                this.saveStateInOperationTransaction,
                                this.clonedTransaction))
                            {
                                if (this.lockingProvider != null)
                                {
                                    this.lockingProvider.Create(this.Instance, this.operationTimeout, disposeInstance);
                                }
                                else
                                {
                                    this.provider.Create(this.Instance, this.operationTimeout);
                                }

                                if (DiagnosticUtility.ShouldTraceInformation)
                                {
                                    string traceText = SR2.GetString(SR2.ServiceDurableInstanceSavedDetails, this.InstanceId, (this.lockingProvider != null) ? "True" : "False");
                                    TraceUtility.TraceEvent(TraceEventType.Information,
                                        TraceCode.ServiceDurableInstanceSaved, SR.GetString(SR.TraceCodeServiceDurableInstanceSaved),
                                        new StringTraceRecord("DurableInstanceDetail", traceText),
                                        this, null);
                                }
                            }
                            break;
                        case OperationType.Update:
                            using (PersistenceScope scope = new PersistenceScope(
                                this.saveStateInOperationTransaction,
                                this.clonedTransaction))
                            {
                                if (this.lockingProvider != null)
                                {
                                    this.lockingProvider.Update(this.Instance, this.operationTimeout, disposeInstance);
                                }
                                else
                                {
                                    this.provider.Update(this.Instance, this.operationTimeout);
                                }

                                if (DiagnosticUtility.ShouldTraceInformation)
                                {
                                    string traceText = SR2.GetString(SR2.ServiceDurableInstanceSavedDetails, this.InstanceId, (this.lockingProvider != null) ? "True" : "False");
                                    TraceUtility.TraceEvent(TraceEventType.Information,
                                        TraceCode.ServiceDurableInstanceSaved, SR.GetString(SR.TraceCodeServiceDurableInstanceSaved),
                                        new StringTraceRecord("DurableInstanceDetail", traceText),
                                        this, null);
                                }
                            }
                            break;
                        case OperationType.None:
                            break;
                        default:
                            Fx.Assert("We should never get an unknown OperationType.");
                            break;
                    }
                }

                if (disposeInstance)
                {
                    DisposeInstance();
                }
            }
            finally
            {
                CompleteClonedTransaction();
            }
        }

        public void MarkForCompletion()
        {
            if (this.abortInstance)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(
                    SR2.GetString(
                    SR2.DurableOperationMethodInvalid,
                    "CompleteInstance",
                    "AbortInstance")));
            }

            this.markedForCompletion = true;
        }

        public object StartOperation(bool canCreateInstance)
        {
            using (StartOperationScope scope = new StartOperationScope(this))
            {
                if (this.markedForCompletion)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InstanceNotFoundException(this.InstanceId));
                }

                if (this.instance == null)
                {
                    if (!TryActivateInstance(canCreateInstance))
                    {
                        using (PersistenceScope persistenceScope = new PersistenceScope(
                            this.saveStateInOperationTransaction,
                            this.clonedTransaction))
                        {
                            if (this.lockingProvider != null)
                            {
                                this.instance = this.lockingProvider.Load(this.operationTimeout, true);
                            }
                            else
                            {
                                this.instance = this.provider.Load(this.operationTimeout);
                            }

                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                string traceText = SR2.GetString(SR2.ServiceDurableInstanceLoadedDetails, this.InstanceId, (this.lockingProvider != null) ? "True" : "False");
                                TraceUtility.TraceEvent(TraceEventType.Information,
                                    TraceCode.ServiceDurableInstanceLoaded, SR.GetString(SR.TraceCodeServiceDurableInstanceLoaded),
                                    new StringTraceRecord("DurableInstanceDetail", traceText),
                                    this, null);
                            }
                        }

                        this.existsInPersistence = true;
                    }
                }

                scope.Complete();
            }

            Fx.Assert(
                this.instance != null,
                "Instance should definitely be non-null here or we should have thrown an exception.");

            return this.instance;
        }

        protected override void OnAbort()
        {
            this.provider.Abort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.provider.BeginClose(timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.provider.BeginOpen(timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.provider.Close(timeout);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            this.provider.EndClose(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            this.provider.EndOpen(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.provider.Open(timeout);
        }

        void CompleteClonedTransaction()
        {
            if (this.clonedTransaction != null)
            {
                this.clonedTransaction.Complete();
                this.clonedTransaction = null;
            }
        }

        void DisposeInstance()
        {
            Fx.Assert(
                this.instance != null,
                "Before making this call we should check instance for null.");

            IDisposable disposableInstance = this.instance as IDisposable;

            if (disposableInstance != null)
            {
                disposableInstance.Dispose();

                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    string traceText = SR.GetString(SR.TraceCodeServiceDurableInstanceDisposed, this.InstanceId);
                    TraceUtility.TraceEvent(TraceEventType.Information,
                        TraceCode.ServiceDurableInstanceDisposed, SR.GetString(SR.TraceCodeServiceDurableInstanceDisposed),
                        new StringTraceRecord("DurableInstanceDetail", traceText),
                        this, null);
                }
            }

            this.instance = null;
        }

        OperationType FinishOperationCommon(bool completeInstance, Exception operationException, out bool disposeInstance)
        {
            // No need for Interlocked because we don't support
            // ConcurrencyMode.Multiple
            this.outstandingOperations--;

            DurableOperationContext.EndOperation();

            Fx.Assert(this.outstandingOperations >= 0,
                "OutstandingOperations should never go below zero.");

            Fx.Assert(this.instance != null,
                "Instance should never been null here - we only get here if StartOperation completes successfully.");

            OperationType operation = OperationType.None;
            disposeInstance = false;

            // This is a "fuzzy" still referenced.  Immediately
            // after this line another message could come in and
            // reference this InstanceContext, but it doesn't matter
            // because regardless of scheme used the other message
            // would have had to reacquire the database lock.
            bool stillReferenced = this.contextManager.GetReferenceCount(this.InstanceId) > 1;

            this.markedForCompletion |= completeInstance;

            if (this.outstandingOperations == 0)
            {
                if (this.saveStateInOperationTransaction &&
                    this.clonedTransaction != null &&
                    this.clonedTransaction.TransactionInformation.Status == TransactionStatus.Aborted)
                {
                    this.abortInstance = false;
                    this.markedForCompletion = false;
                    disposeInstance = true;
                }
                else if (operationException != null && !(operationException is FaultException))
                {
                    if (this.unknownExceptionAction == UnknownExceptionAction.TerminateInstance)
                    {
                        if (this.existsInPersistence)
                        {
                            operation = OperationType.Delete;
                        }

                        this.existsInPersistence = true;
                        disposeInstance = true;
                    }
                    else
                    {
                        Fx.Assert(this.unknownExceptionAction == UnknownExceptionAction.AbortInstance, "If it is not TerminateInstance then it must be AbortInstance.");

                        if (this.existsInPersistence)
                        {
                            operation = OperationType.Unlock;
                        }

                        this.existsInPersistence = true;
                        disposeInstance = true;
                        this.markedForCompletion = false;
                    }
                }
                else if (this.abortInstance)
                {
                    this.abortInstance = false;

                    // AbortInstance can only be called in ConcurrencyMode.Single
                    // and therefore markedForCompletion could only have been
                    // set true by this same operation (either declaratively or
                    // programmatically).  We set it false again so that the
                    // next operation doesn't cause instance completion.
                    this.markedForCompletion = false;

                    if (this.existsInPersistence && !stillReferenced)
                    {
                        // No need for a transactional version of this as we do not allow
                        // AbortInstance to be called in scenarios with SaveStateInOperationTransaction
                        // set to true
                        Fx.Assert(!this.saveStateInOperationTransaction,
                            "SaveStateInOperationTransaction must be false if we allowed an abort.");

                        if (this.lockingProvider != null)
                        {
                            operation = OperationType.Unlock;
                        }
                    }

                    this.existsInPersistence = true;
                    disposeInstance = true;
                }
                else if (this.markedForCompletion)
                {
                    if (this.existsInPersistence)
                    {
                        // We don't set exists in persistence to 
                        // false here because we want the proper
                        // persistence exceptions to get back to the
                        // client if we end up here again.

                        operation = OperationType.Delete;
                    }

                    // Even if we didn't delete the instance because it
                    // never existed we should set this to true.  This will
                    // make sure that any future requests to this instance of
                    // ServiceDurableInstance will treat the object as deleted.
                    this.existsInPersistence = true;
                    disposeInstance = true;
                }
                else
                {
                    if (this.existsInPersistence)
                    {
                        operation = OperationType.Update;
                    }
                    else
                    {
                        operation = OperationType.Create;
                    }

                    this.existsInPersistence = true;
                    if (!stillReferenced)
                    {
                        disposeInstance = true;
                    }
                }
            }

            return operation;
        }

        bool TryActivateInstance(bool canCreateInstance)
        {
            if (this.newServiceType != null && !this.existsInPersistence)
            {
                if (canCreateInstance)
                {
                    this.instance = Activator.CreateInstance(this.newServiceType);
                    return true;
                }
                else
                {
                    DurableErrorHandler.CleanUpInstanceContextAtOperationCompletion();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FaultException(new DurableDispatcherAddressingFault()));
                }
            }

            return false;
        }

        class FinishOperationAsyncResult : AsyncResult
        {
            static AsyncCallback createCallback = Fx.ThunkCallback(new AsyncCallback(CreateComplete));
            static AsyncCallback deleteCallback = Fx.ThunkCallback(new AsyncCallback(DeleteComplete));
            static AsyncCallback unlockCallback = Fx.ThunkCallback(new AsyncCallback(UnlockComplete));
            static AsyncCallback updateCallback = Fx.ThunkCallback(new AsyncCallback(UpdateComplete));

            ServiceDurableInstance durableInstance;

            public FinishOperationAsyncResult(ServiceDurableInstance durableInstance, bool completeInstance, bool performPersistence, Exception operationException, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.durableInstance = durableInstance;

                IAsyncResult result = null;
                OperationType operation = OperationType.None;
                bool completeSelf = false;
                bool disposeInstace;
                operation = this.durableInstance.FinishOperationCommon(completeInstance, operationException, out disposeInstace);

                if (performPersistence)
                {
                    switch (operation)
                    {
                        case OperationType.Unlock:
                            if (this.durableInstance.lockingProvider != null)
                            {
                                using (PersistenceScope scope = new PersistenceScope(
                                    this.durableInstance.saveStateInOperationTransaction,
                                    this.durableInstance.clonedTransaction))
                                {
                                    result = this.durableInstance.lockingProvider.BeginUnlock(this.durableInstance.operationTimeout, unlockCallback, this);
                                }
                            }
                            break;
                        case OperationType.Delete:
                            using (PersistenceScope scope = new PersistenceScope(
                                this.durableInstance.saveStateInOperationTransaction,
                                this.durableInstance.clonedTransaction))
                            {
                                result = this.durableInstance.provider.BeginDelete(this.durableInstance.Instance, this.durableInstance.operationTimeout, deleteCallback, this);
                            }
                            break;
                        case OperationType.Create:
                            using (PersistenceScope scope = new PersistenceScope(
                                this.durableInstance.saveStateInOperationTransaction,
                                this.durableInstance.clonedTransaction))
                            {
                                if (this.durableInstance.lockingProvider != null)
                                {
                                    result = this.durableInstance.lockingProvider.BeginCreate(this.durableInstance.Instance, this.durableInstance.operationTimeout, disposeInstace, createCallback, this);
                                }
                                else
                                {
                                    result = this.durableInstance.provider.BeginCreate(this.durableInstance.Instance, this.durableInstance.operationTimeout, createCallback, this);
                                }
                            }
                            break;
                        case OperationType.Update:
                            using (PersistenceScope scope = new PersistenceScope(
                                this.durableInstance.saveStateInOperationTransaction,
                                this.durableInstance.clonedTransaction))
                            {
                                if (this.durableInstance.lockingProvider != null)
                                {
                                    result = this.durableInstance.lockingProvider.BeginUpdate(this.durableInstance.Instance, this.durableInstance.operationTimeout, disposeInstace, updateCallback, this);
                                }
                                else
                                {
                                    result = this.durableInstance.provider.BeginUpdate(this.durableInstance.Instance, this.durableInstance.operationTimeout, updateCallback, this);
                                }
                            }
                            break;
                        case OperationType.None:
                            break;
                        default:
                            Fx.Assert("Unknown OperationType was passed in.");
                            break;
                    }
                }

                if (disposeInstace)
                {
                    this.durableInstance.DisposeInstance();
                }

                if (operation == OperationType.None ||
                    (result != null && result.CompletedSynchronously))
                {
                    completeSelf = true;
                }

                if (!performPersistence)
                {
                    Fx.Assert(result == null, "Should not have had a result if we didn't perform persistence.");
                    Complete(true);
                    return;
                }

                if (completeSelf)
                {
                    CallEndOperation(operation, result);

                    Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<FinishOperationAsyncResult>(result);
            }

            static void CreateComplete(IAsyncResult result)
            {
                HandleOperationCompletion(OperationType.Create, result);
            }

            static void DeleteComplete(IAsyncResult result)
            {
                HandleOperationCompletion(OperationType.Delete, result);
            }

            static void HandleOperationCompletion(OperationType operation, IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                Fx.Assert(result.AsyncState is FinishOperationAsyncResult,
                    "Async state should have been FinishOperationAsyncResult");

                FinishOperationAsyncResult finishResult = (FinishOperationAsyncResult) result.AsyncState;

                Exception completionException = null;
                try
                {
                    finishResult.CallEndOperation(operation, result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                }

                finishResult.Complete(false, completionException);
            }

            static void UnlockComplete(IAsyncResult result)
            {
                HandleOperationCompletion(OperationType.Unlock, result);
            }

            static void UpdateComplete(IAsyncResult result)
            {
                HandleOperationCompletion(OperationType.Update, result);
            }

            void CallEndOperation(OperationType operation, IAsyncResult result)
            {
                try
                {
                    switch (operation)
                    {
                        case OperationType.Delete:
                            this.durableInstance.provider.EndDelete(result);
                            break;
                        case OperationType.Unlock:
                            this.durableInstance.lockingProvider.EndUnlock(result);
                            break;
                        case OperationType.Create:
                            this.durableInstance.provider.EndCreate(result);
                            break;
                        case OperationType.Update:
                            this.durableInstance.provider.EndUpdate(result);
                            break;
                        case OperationType.None:
                            break;
                        default:
                            Fx.Assert("Should never have an unknown value for this enum.");
                            break;
                    }
                }
                finally
                {
                    this.durableInstance.CompleteClonedTransaction();
                }
            }
        }

        class PersistenceScope : IDisposable
        {
            DependentTransaction clonedTransaction;
            TransactionScope scope;

            public PersistenceScope(bool saveStateInOperationTransaction, DependentTransaction clonedTransaction)
            {
                if (!saveStateInOperationTransaction)
                {
                    this.scope = new TransactionScope(TransactionScopeOption.Suppress);
                }
                else if (clonedTransaction != null)
                {
                    this.clonedTransaction = clonedTransaction;
                    this.scope = new TransactionScope(clonedTransaction);
                }
            }

            public void Dispose()
            {
                if (this.scope != null)
                {
                    this.scope.Complete();
                    this.scope.Dispose();
                    this.scope = null;
                }
            }
        }

        class StartOperationAsyncResult : AsyncResult
        {
            static AsyncCallback loadCallback = Fx.ThunkCallback(new AsyncCallback(LoadComplete));

            ServiceDurableInstance durableInstance;
            OperationContext operationContext;
            StartOperationScope scope;

            public StartOperationAsyncResult(ServiceDurableInstance durableInstance, bool canCreateInstance, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.durableInstance = durableInstance;

                bool completeSelf = false;
                IAsyncResult result = null;
                this.operationContext = OperationContext.Current;

                scope = new StartOperationScope(this.durableInstance);               
                bool success = false;
                try
                {                   
                    if (this.durableInstance.instance == null)
                    {
                        if (this.durableInstance.TryActivateInstance(canCreateInstance))
                        {
                            completeSelf = true;
                        }
                        else
                        {
                            using (PersistenceScope persistenceScope = new PersistenceScope(
                                this.durableInstance.saveStateInOperationTransaction,
                                this.durableInstance.clonedTransaction))
                            {
                                if (this.durableInstance.lockingProvider != null)
                                {
                                    result = this.durableInstance.lockingProvider.BeginLoad(this.durableInstance.operationTimeout, true, loadCallback, this);
                                }
                                else
                                {
                                    result = this.durableInstance.provider.BeginLoad(this.durableInstance.operationTimeout, loadCallback, this);
                                }
                            }

                            this.durableInstance.existsInPersistence = true;

                            if (result.CompletedSynchronously)
                            {
                                completeSelf = true;
                            }
                        }
                    }
                    else
                    {
                        completeSelf = true;
                    }


                    success = true;
                }
                finally
                {
                    if (!success)
                    {
                        scope.Dispose();                        
                    }
                }

                if (completeSelf)
                {
                    try
                    {
                        if (result != null)
                        {
                            this.durableInstance.instance = this.durableInstance.provider.EndLoad(result);
                        }

                        Fx.Assert(this.durableInstance.instance != null,
                            "The instance should always be set here.");

                        Complete(true);
                        scope.Complete();
                    }
                    finally
                    {
                        scope.Dispose();
                    }
                }              
            }

            public static object End(IAsyncResult result)
            {
                StartOperationAsyncResult startResult = AsyncResult.End<StartOperationAsyncResult>(result);

                return startResult.durableInstance.instance;
            }

            static void LoadComplete(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                Fx.Assert(result.AsyncState is StartOperationAsyncResult,
                    "Should have been passed a StartOperationAsyncResult as the state");

                StartOperationAsyncResult startResult = (StartOperationAsyncResult) result.AsyncState;                                
                Exception completionException = null;
                OperationContext oldOperationContext = OperationContext.Current;
                OperationContext.Current = startResult.operationContext;

                try
                {
                    try
                    {
                        startResult.durableInstance.instance = startResult.durableInstance.provider.EndLoad(result);

                        Fx.Assert(startResult.durableInstance.instance != null,
                            "The instance should always be set here.");

                        startResult.scope.Complete();
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completionException = e;
                    }
                    finally
                    {
                        startResult.scope.Dispose();                        
                    }

                    startResult.Complete(false, completionException);
                }
                finally
                {
                    OperationContext.Current = oldOperationContext;
                }
            }
        }

        class StartOperationScope : IDisposable
        {
            ServiceDurableInstance durableInstance;
            bool success;

            public StartOperationScope(ServiceDurableInstance durableInstance)
            {
                this.durableInstance = durableInstance;

                this.durableInstance.runtimeValidator.ValidateRuntime();

                DurableOperationContext.BeginOperation();

                // No need for Interlocked because we don't support
                // ConcurrencyMode.Multiple
                this.durableInstance.outstandingOperations++;

                if (this.durableInstance.saveStateInOperationTransaction && Transaction.Current != null)
                {
                    this.durableInstance.clonedTransaction = Transaction.Current.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
                }
            }

            public void Complete()
            {
                this.success = true;
            }

            public void Dispose()
            {
                if (!this.success)
                {
                    Fx.Assert(OperationContext.Current != null, "Operation context should not be null at this point.");

                    DurableOperationContext.EndOperation();

                    this.durableInstance.outstandingOperations--;
                    this.durableInstance.CompleteClonedTransaction();
                }
            }
        }
    }
}
