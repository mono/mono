//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Dispatcher
{
    using System.Activities;
    using System.Activities.DurableInstancing;
    using System.Activities.Persistence;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Threading;
    using System.Transactions;
    using System.Xml.Linq;
    using System.ServiceModel.Activation;

    sealed class DurableInstanceManager
    {
        static AsyncCallback waitAndHandleStoreEventsCallback = Fx.ThunkCallback(new AsyncCallback(WaitAndHandleStoreEventsCallback));

        int state;
        InstanceStore store;
        InstanceHandle handle;
        InstanceOwner owner;
        IDictionary<XName, InstanceValue> instanceOwnerMetadata;
        object thisLock;
        IDictionary<XName, InstanceValue> instanceMetadataChanges;
        AsyncWaitHandle waitForStoreEventsLoop;
        WorkflowDefinitionProvider workflowDefinitionProvider;

        internal DurableInstanceManager(WorkflowServiceHost host)
        {
            DurableInstancingOptions = new DurableInstancingOptions(this);
            this.instanceOwnerMetadata = new Dictionary<XName, InstanceValue>();
            this.instanceMetadataChanges = new Dictionary<XName, InstanceValue>();
            this.thisLock = new object();

            // This is for collision detection.  Will replace with the real service name prior to executing.
            InstanceValue sentinel = new InstanceValue(XNamespace.Get("http://tempuri.org").GetName("Sentinel"));
            this.instanceOwnerMetadata.Add(WorkflowNamespace.WorkflowHostType, sentinel);
            this.instanceMetadataChanges.Add(WorkflowNamespace.WorkflowHostType, sentinel);
            this.instanceMetadataChanges.Add(PersistenceMetadataNamespace.InstanceType, new InstanceValue(WorkflowNamespace.WorkflowHostType, InstanceValueOptions.WriteOnly));

            this.Host = host;
        }

        WorkflowServiceHost Host { get; set; }

        internal PersistenceProviderDirectory PersistenceProviderDirectory { get; set; }

        public DurableInstancingOptions DurableInstancingOptions { get; private set; }

        public InstanceStore InstanceStore
        {
            get
            {
                return this.store;
            }
            set
            {
                ThrowIfDisposedOrImmutable(this.state);
                this.store = value;
            }
        }

        public void AddInstanceOwnerValues(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues)
        {
            ThrowIfDisposedOrImmutable(this.state);

            if (readWriteValues != null)
            {
                foreach (KeyValuePair<XName, object> property in readWriteValues)
                {
                    if (this.instanceOwnerMetadata.ContainsKey(property.Key))
                    {
                        throw FxTrace.Exception.Argument("readWriteValues", SR.ConflictingValueName(property.Key));
                    }
                    this.instanceOwnerMetadata.Add(property.Key, new InstanceValue(property.Value));
                }
            }

            if (writeOnlyValues != null)
            {
                foreach (KeyValuePair<XName, object> property in writeOnlyValues)
                {
                    if (this.instanceOwnerMetadata.ContainsKey(property.Key))
                    {
                        throw FxTrace.Exception.Argument("writeOnlyValues", SR.ConflictingValueName(property.Key));
                    }
                    this.instanceOwnerMetadata.Add(property.Key, new InstanceValue(property.Value,
                        InstanceValueOptions.Optional | InstanceValueOptions.WriteOnly));
                }
            }
        }

        public void AddInitialInstanceValues(IDictionary<XName, object> writeOnlyValues)
        {
            ThrowIfDisposedOrImmutable(this.state);

            if (writeOnlyValues != null)
            {
                foreach (KeyValuePair<XName, object> pair in writeOnlyValues)
                {
                    if (this.instanceMetadataChanges.ContainsKey(pair.Key))
                    {
                        throw FxTrace.Exception.Argument("writeOnlyValues", SR.ConflictingValueName(pair.Key));
                    }
                    this.instanceMetadataChanges.Add(pair.Key, new InstanceValue(pair.Value, InstanceValueOptions.Optional | InstanceValueOptions.WriteOnly));
                }
            }
        }

        static void ThrowIfDisposedOrImmutable(int state)
        {
            if (state == States.Aborted)
            {
                throw FxTrace.Exception.AsError(new CommunicationObjectAbortedException(SR.ServiceHostExtensionAborted));
            }
            if (state == States.Closed)
            {
                throw FxTrace.Exception.AsError(new ObjectDisposedException(typeof(DurableInstanceManager).Name));
            }
            if (state == States.Opened)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.ServiceHostExtensionImmutable));
            }
        }

        static void ThrowIfClosedOrAborted(int state)
        {
            if (state == States.Aborted)
            {
                throw FxTrace.Exception.AsError(new CommunicationObjectAbortedException(SR.ServiceHostExtensionAborted));
            }
            if (state == States.Closed)
            {
                throw FxTrace.Exception.AsError(new ObjectDisposedException(typeof(DurableInstanceManager).Name));
            }
        }

        void InitializePersistenceProviderDirectory()
        {   
            int maxInstances = ServiceThrottlingBehavior.DefaultMaxConcurrentInstances;
            ServiceThrottlingBehavior serviceThrottlingBehavior = Host.Description.Behaviors.Find<ServiceThrottlingBehavior>();
            if (serviceThrottlingBehavior != null)
            {
                maxInstances = serviceThrottlingBehavior.MaxConcurrentInstances;
            }

            if (InstanceStore != null)
            {
                PersistenceProviderDirectory = new PersistenceProviderDirectory(InstanceStore, this.owner, this.instanceMetadataChanges, this.workflowDefinitionProvider, Host, DurableConsistencyScope.Global, maxInstances);
            }
            else
            {
                PersistenceProviderDirectory = new PersistenceProviderDirectory(this.workflowDefinitionProvider, Host, maxInstances);
            }

            bool aborted;
            lock (this.thisLock)
            {
                aborted = this.state == States.Aborted;
            }

            if (aborted)
            {
                if (this.handle != null)
                {
                    this.handle.Free();
                }

                PersistenceProviderDirectory.Abort();
            }

            // Start listening to store event
            if (InstanceStore != null && !aborted)
            {
                this.waitForStoreEventsLoop = new AsyncWaitHandle(EventResetMode.ManualReset);
                BeginWaitAndHandleStoreEvents(waitAndHandleStoreEventsCallback, this);
            }
        }

        IAsyncResult BeginWaitAndHandleStoreEvents(AsyncCallback callback, object state)
        {
            return new WaitAndHandleStoreEventsAsyncResult(this, callback, state);
        }

        void EndWaitAndHandleStoreEvents(IAsyncResult result)
        {
            WaitAndHandleStoreEventsAsyncResult.End(result);
        }

        static void WaitAndHandleStoreEventsCallback(IAsyncResult result)
        {
            DurableInstanceManager thisPtr = (DurableInstanceManager)result.AsyncState;
            bool stop = false;
            try
            {
                thisPtr.EndWaitAndHandleStoreEvents(result);
            }
            catch (OperationCanceledException exception)
            {
                FxTrace.Exception.AsWarning(exception);

                // The OCE, bubbled to this layer, is only from store.BeginWaitForEvents.
                // This indicates handle is freed by 1) normal closing sequence 2) store
                // is dead (eg. lock owner expired).  We will fault the host as well as 
                // cease the loop.
                if (thisPtr.Host.State == CommunicationState.Opening || thisPtr.Host.State == CommunicationState.Opened)
                {
                    thisPtr.Host.Fault(exception);
                }
                stop = true;
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception) || !thisPtr.HandleException(exception))
                {
                    throw;
                }
            }

            // Continue
            if (!stop && thisPtr.state == States.Opened)
            {
                thisPtr.BeginWaitAndHandleStoreEvents(waitAndHandleStoreEventsCallback, thisPtr);
            }
            else
            {
                thisPtr.waitForStoreEventsLoop.Set();
            }
        }

        bool HandleException(Exception exception)
        {
            if (exception is TimeoutException ||
                exception is OperationCanceledException ||
                exception is TransactionException ||
                exception is CommunicationObjectAbortedException ||
                // When abort raised by WorkflowServiceInstance
                exception is FaultException ||
                exception is InstancePersistenceException)
            {
                FxTrace.Exception.AsWarning(exception);
                this.Host.FaultServiceHostIfNecessary(exception);
                return true;
            }
            return false;
        }

        void CheckPersistenceProviderBehavior()
        {
            foreach (IServiceBehavior behavior in Host.Description.Behaviors)
            {
                if (behavior.GetType().FullName == "System.ServiceModel.Description.PersistenceProviderBehavior")
                {
                    throw FxTrace.Exception.AsError(new CommunicationException(SR.UseInstanceStoreInsteadOfPersistenceProvider));
                }
            }
        }

        internal IAsyncResult BeginGetInstance(InstanceKey instanceKey, ICollection<InstanceKey> additionalKeys, WorkflowGetInstanceContext parameters, TimeSpan timeout, AsyncCallback callback, object state)
        {
            ThrowIfClosedOrAborted(this.state);
            return new GetInstanceAsyncResult(this, instanceKey, additionalKeys, parameters, timeout, callback, state);
        }

        internal IAsyncResult BeginGetInstance(Guid instanceId, WorkflowGetInstanceContext parameters,
            WorkflowIdentityKey updatedIdentity, TimeSpan timeout, AsyncCallback callback, object state)
        {
            ThrowIfClosedOrAborted(this.state);
            return new GetInstanceAsyncResult(this, instanceId, parameters, updatedIdentity, timeout, callback, state);
        }

        internal WorkflowServiceInstance EndGetInstance(IAsyncResult result)
        {
            return GetInstanceAsyncResult.End(result);
        }

        void AbortDirectory()
        {
            lock (this.thisLock)
            {
                if (this.state == States.Aborted)
                {
                    return;
                }
                this.state = States.Aborted;
            }

            if (this.handle != null)
            {
                this.handle.Free();
            }

            // PersistenceProviderDirectory is assigned on opened.  Abort could happen before (eg. after created)
            if (PersistenceProviderDirectory != null)
            {
                PersistenceProviderDirectory.Abort();
            }
        }

        void SetDefaultOwnerMetadata()
        {
            // Replace the sentinal value with the real scoping name here.
            this.instanceOwnerMetadata[WorkflowNamespace.WorkflowHostType] = new InstanceValue(Host.DurableInstancingOptions.ScopeName);
            this.instanceMetadataChanges[WorkflowNamespace.WorkflowHostType] = new InstanceValue(Host.DurableInstancingOptions.ScopeName);

            this.workflowDefinitionProvider.GetDefinitionIdentityMetadata(this.instanceOwnerMetadata);

            if (!this.instanceMetadataChanges.ContainsKey(WorkflowServiceNamespace.Service))
            {
                this.instanceMetadataChanges[WorkflowServiceNamespace.Service] = new InstanceValue(Host.ServiceName, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);
            }

            // add instance metadata about all of our endpoints
            foreach (ServiceEndpoint endpoint in this.Host.Description.Endpoints)
            {
                if (endpoint.Name != null)
                {
                    // treat the control endpoint as special
                    if (endpoint is WorkflowControlEndpoint)
                    {
                        if (!this.instanceOwnerMetadata.ContainsKey(WorkflowServiceNamespace.ControlEndpoint))
                        {
                            this.instanceOwnerMetadata.Add(WorkflowServiceNamespace.ControlEndpoint, new InstanceValue(endpoint.ListenUri));
                        }
                    }
                    else
                    {
                        XName endpointName = WorkflowServiceNamespace.EndpointsPath.GetName(endpoint.Name);
                        if (!this.instanceOwnerMetadata.ContainsKey(endpointName))
                        {
                            this.instanceOwnerMetadata.Add(endpointName, new InstanceValue(endpoint.ListenUri));
                        }
                    }
                }
            }

            // as well as additional metadata if we're hosted
            VirtualPathExtension virtualPathExtension = this.Host.Extensions.Find<VirtualPathExtension>();
            if (virtualPathExtension != null && !this.instanceMetadataChanges.ContainsKey(PersistenceMetadataNamespace.ActivationType))
            {
                // Example values for various web-host properties
                // SiteName: "Default Website"
                // RelativeApplicationPath/ApplicationVirtualPath: "/myApp1"
                // Virtual Path: "~/ShoppingCartService/ShoppingCartService.xaml"
                // Relative Service Path: "/myApp1/ShoppingCartService/ShoppingCartService.xaml"
                this.instanceMetadataChanges.Add(PersistenceMetadataNamespace.ActivationType, new InstanceValue(PersistenceMetadataNamespace.ActivationTypes.WAS, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional));

                string siteName = this.Host.OverrideSiteName ? this.Host.Description.Name : virtualPathExtension.SiteName;
                
                // The remaining properties will get overritten if the user set them manually.  To control activation, the user should also set ActivationType, even if just to WAS.
                this.instanceMetadataChanges[WorkflowServiceNamespace.SiteName] = new InstanceValue(siteName, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);
                this.instanceMetadataChanges[WorkflowServiceNamespace.RelativeApplicationPath] = new InstanceValue(virtualPathExtension.ApplicationVirtualPath, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);

                string virtualPath = virtualPathExtension.VirtualPath.Substring(1);
                string relativePath = ("/" == virtualPathExtension.ApplicationVirtualPath) ? virtualPath : virtualPathExtension.ApplicationVirtualPath + virtualPath;
                
                this.instanceMetadataChanges[WorkflowServiceNamespace.RelativeServicePath] = new InstanceValue(relativePath, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);
            }
        }

        public void Open(TimeSpan timeout)
        {
            Fx.Assert(Host != null, "Extension should have been attached in WorkflowServiceHost constructor.");

            lock (this.thisLock)
            {
                ThrowIfDisposedOrImmutable(this.state);
                this.state = States.Opened;
            }
            InitializeDefinitionProvider();

            CheckPersistenceProviderBehavior();

            SetDefaultOwnerMetadata();


            if (InstanceStore != null)
            {
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                    InstanceHandle handle = null;
                    try
                    {
                        handle = InstanceStore.CreateInstanceHandle(null);
                        this.owner = InstanceStore.Execute(handle, GetCreateOwnerCommand(), timeoutHelper.RemainingTime()).InstanceOwner;
                        this.handle = handle;
                        handle = null;
                    }
                    catch (InstancePersistenceException exception)
                    {
                        throw FxTrace.Exception.AsError(new CommunicationException(SR.UnableToOpenAndRegisterStore, exception));
                    }
                    finally
                    {
                        if (handle != null)
                        {
                            handle.Free();
                        }
                    }
                }
            }

            InitializePersistenceProviderDirectory();
        }

        void InitializeDefinitionProvider()
        {
            WorkflowServiceBehavior workflowServiceBehavior = Host.Description.Behaviors.Find<WorkflowServiceBehavior>();
            Fx.Assert(workflowServiceBehavior != null && workflowServiceBehavior.WorkflowDefinitionProvider != null,
                "WorkflowServiceBehavior must be present on WorkflowServiceHost and WorkflowDefinitionProvider must be present on WorkflowServiceBehavior.");

            this.workflowDefinitionProvider = workflowServiceBehavior.WorkflowDefinitionProvider;
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            Fx.Assert(Host != null, "Extension should have been attached in WorkflowServiceHost constructor.");

            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                return new OpenInstanceStoreAsyncResult(this, timeout, callback, state);
            }
        }

        public void EndOpen(IAsyncResult result)
        {
            OpenInstanceStoreAsyncResult.End(result);
        }

        public void Close(TimeSpan timeout)
        {
            // We normally would have a purely synchronous path for our synchronous
            // overload, but PersistenceIOParticipant.OnBeginSave() doesn't have a synchronous counterpart.
            // Given that, at the very least we'd have to do PersistencePipeline.EndSave(PersistencePipeline.BeginSave).
            // Therefore we resign ourselves to End(Begin) and take comfort in the unification of logic by not having two codepaths
            CloseAsyncResult.End(new CloseAsyncResult(this, timeout, null, null));
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseAsyncResult(this, timeout, callback, state);
        }

        public void EndClose(IAsyncResult result)
        {
            CloseAsyncResult.End(result);
        }

        public void Abort()
        {
            AbortDirectory();
        }

        InstancePersistenceCommand GetCreateOwnerCommand()
        {
            InstancePersistenceCommand command;
            IDictionary<XName, InstanceValue> commandMetadata;
            if (this.instanceOwnerMetadata.ContainsKey(Workflow45Namespace.DefinitionIdentities))
            {
                CreateWorkflowOwnerWithIdentityCommand withIdentity = new CreateWorkflowOwnerWithIdentityCommand();
                command = withIdentity;
                commandMetadata = withIdentity.InstanceOwnerMetadata;
            }
            else
            {
                CreateWorkflowOwnerCommand withoutIdentity = new CreateWorkflowOwnerCommand();
                command = withoutIdentity;
                commandMetadata = withoutIdentity.InstanceOwnerMetadata;
            }

            foreach (KeyValuePair<XName, InstanceValue> metadata in this.instanceOwnerMetadata)
            {
                commandMetadata.Add(metadata);
            }

            return command;
        }

        static class States
        {
            public const int Created = 0;
            public const int Opened = 1;
            public const int Closed = 2;
            public const int Aborted = 3;
        }

        class OpenInstanceStoreAsyncResult : AsyncResult
        {
            static AsyncCompletion handleEndExecute = new AsyncCompletion(HandleEndExecute);
            static Action<AsyncResult, Exception> onFinally = new Action<AsyncResult, Exception>(OnFinally);

            DurableInstanceManager instanceManager;
            TimeoutHelper timeoutHelper;
            InstanceHandle handle;

            public OpenInstanceStoreAsyncResult(DurableInstanceManager instanceManager, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.instanceManager = instanceManager;
                this.timeoutHelper = new TimeoutHelper(timeout);

                lock (this.instanceManager.thisLock)
                {
                    DurableInstanceManager.ThrowIfDisposedOrImmutable(this.instanceManager.state);
                    this.instanceManager.state = States.Opened;
                }

                this.instanceManager.InitializeDefinitionProvider();

                instanceManager.CheckPersistenceProviderBehavior();

                this.instanceManager.SetDefaultOwnerMetadata();

                this.OnCompleting = OpenInstanceStoreAsyncResult.onFinally;

                bool completeSelf;
                Exception completionException = null;
                try
                {
                    if (instanceManager.InstanceStore == null)
                    {
                        completeSelf = CreateDirectory();
                    }
                    else
                    {
                        this.handle = this.instanceManager.InstanceStore.CreateInstanceHandle(null);
                        IAsyncResult executeResult = this.instanceManager.InstanceStore.BeginExecute(this.handle,
                            this.instanceManager.GetCreateOwnerCommand(), this.timeoutHelper.RemainingTime(),
                            this.PrepareAsyncCompletion(OpenInstanceStoreAsyncResult.handleEndExecute), this);
                        completeSelf = SyncContinue(executeResult);
                    }
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
                    Complete(true, completionException);
                }
            }

            static bool HandleEndExecute(IAsyncResult result)
            {
                OpenInstanceStoreAsyncResult thisPtr = (OpenInstanceStoreAsyncResult)result.AsyncState;

                thisPtr.instanceManager.owner = thisPtr.instanceManager.InstanceStore.EndExecute(result).InstanceOwner;

                return thisPtr.CreateDirectory();
            }

            static void OnFinally(AsyncResult result, Exception exception)
            {
                if (exception != null)
                {
                    try
                    {
                        if (exception is InstancePersistenceException)
                        {
                            throw FxTrace.Exception.AsError(new CommunicationException(SR.UnableToOpenAndRegisterStore, exception));
                        }
                    }
                    finally
                    {
                        OpenInstanceStoreAsyncResult thisPtr = (OpenInstanceStoreAsyncResult)result;
                        if (thisPtr.handle != null)
                        {
                            thisPtr.handle.Free();
                        }
                    }
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<OpenInstanceStoreAsyncResult>(result);
            }

            bool CreateDirectory()
            {
                this.instanceManager.InitializePersistenceProviderDirectory();
                this.instanceManager.handle = this.handle;
                this.handle = null;
                return true;
            }
        }

        class CloseAsyncResult : AsyncResult
        {
            static AsyncCallback handleEndReleaseInstanceWrapperCallback = Fx.ThunkCallback(new AsyncCallback(HandleEndReleaseInstanceWrapperCallback));
            static AsyncCompletion handleEndExecute = new AsyncCompletion(HandleEndExecute);
            static Action<object, TimeoutException> handleWaitForStoreEvents = new Action<object, TimeoutException>(HandleWaitForStoreEvents);
            static int outstandingUnloadCapacity = 10;

            TimeoutHelper timeoutHelper;
            DurableInstanceManager instanceManager;
            IEnumerator<PersistenceContext> workflowServiceInstances;
            int instanceCount;
            InstanceHandle handle;

            object instanceQueueLock;
            int completedUnloadCount;
            bool allReleaseInstancesCompletedSynchronously;

            public CloseAsyncResult(DurableInstanceManager instanceManager, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.instanceManager = instanceManager;
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.instanceQueueLock = new object();
                this.allReleaseInstancesCompletedSynchronously = true;

                if (this.instanceManager.state == States.Opened && this.instanceManager.handle != null)
                {
                    // Note: since we change state before actual openning, this may 
                    // get NullRef (---- already exists in other places) if Close 
                    // is called on an unsuccessful or incompleted opened DIM.  
                    // Assuming it is a non supported scenario.  
                    this.instanceManager.handle.Free();
                    if (WaitForStoreEventsLoop())
                    {
                        Complete(true);
                    }
                }
                else
                {
                    if (PerformClose())
                    {
                        Complete(true);
                    }
                }
            }

            bool PerformClose()
            {
                bool closed;
                bool opened;
                bool aborted;

                lock (this.instanceManager.thisLock)
                {
                    closed = this.instanceManager.state == States.Closed;
                    opened = this.instanceManager.state == States.Opened;
                    aborted = this.instanceManager.state == States.Aborted;
                    if (opened)
                    {
                        this.instanceManager.state = States.Closed;
                    }
                }

                if (closed)
                {
                    return true;
                }
                if (!opened)
                {
                    if (!aborted)
                    {
                        this.instanceManager.AbortDirectory();
                    }

                    // We cannot throw here if the DurableInstanceManager is already aborted since service host could 
                    // be aborted due to a timeout exception. Simply return here
                    return true;
                }

                IEnumerable<PersistenceContext> contexts = this.instanceManager.PersistenceProviderDirectory.GetContexts();
                this.instanceCount = contexts.Count<PersistenceContext>();
                this.workflowServiceInstances = contexts.GetEnumerator();
                // We only call StartProcess if we actually have instances to release.
                if (this.instanceCount > 0)
                {
                    StartProcess();
                }
                else
                {
                    // No instances to release. Do the post processing.
                    return PostProcess();
                }

                return false;
            }

            bool WaitForStoreEventsLoop()
            {
                // Event never get initialized, meaning we have not started the WaitForStoreEvents loop
                if (this.instanceManager.waitForStoreEventsLoop == null
                    || this.instanceManager.waitForStoreEventsLoop.WaitAsync(handleWaitForStoreEvents, this, this.timeoutHelper.RemainingTime()))
                {
                    return PerformClose();
                }
                else
                {
                    return false;
                }
            }

            static void HandleWaitForStoreEvents(object state, TimeoutException exception)
            {
                CloseAsyncResult thisPtr = (CloseAsyncResult)state;
                if (exception != null)
                {
                    thisPtr.Complete(false, exception);
                    return;
                }

                bool completeSelf = false;
                Exception completionException = null;

                try
                {
                    completeSelf = thisPtr.PerformClose();
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
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

            void StartProcess()
            {
                for (int i = 0; i < outstandingUnloadCapacity; i++)
                {
                    if (!Process())
                    {
                        break;
                    }
                }
            }

            bool Process()
            {
                bool shouldContinueProcess;
                WorkflowServiceInstance currentInstance = null;

                lock (this.instanceQueueLock)
                {
                    if (this.workflowServiceInstances.MoveNext())
                    {
                        currentInstance = this.workflowServiceInstances.Current.GetInstance(null);
                        shouldContinueProcess = true;
                    }
                    else
                    {
                        shouldContinueProcess = false;
                    }
                }

                if (shouldContinueProcess)
                {
                    if (currentInstance != null)
                    {
                        try
                        {
                            // Our own wrapper callback will invoke the inner callback even when result is completed synchronously
                            IAsyncResult result = currentInstance.BeginReleaseInstance(
                                false,
                                this.timeoutHelper.RemainingTime(),
                                CloseAsyncResult.handleEndReleaseInstanceWrapperCallback,
                                this);
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                            {
                                throw;
                            }

                            // Ignore exception thrown from BeginReleaseInstance.
                            // We do not complete CloseAsyncResult with this exception.
                            // Instead, we want to keep this thread running so that it can clean up other instances.
                            FxTrace.Exception.AsWarning(e);
                        }
                    }
                    else
                    {
                        if (Interlocked.Increment(ref this.completedUnloadCount) == this.instanceCount)
                        {
                            // We are done with the instances, so do post-processing. If that completes
                            // synchronously, we need to call Complete. We completed synchronously if all
                            // of the ReleaseInstance invocations completed synchronously.
                            // The return value from this method only indicates
                            // if there are more instances to deal with, not if we are Complete.
                            if (PostProcess())
                            {
                                Complete(this.allReleaseInstancesCompletedSynchronously);
                            }
                        }
                    }
                }

                return shouldContinueProcess;
            }

            bool PostProcess()
            {
                //cleanup any buffered receives unassociated with workflowServiceInstances
                BufferedReceiveManager bufferedReceiveManager = this.instanceManager.Host.Extensions.Find<BufferedReceiveManager>();
                if (bufferedReceiveManager != null)
                {
                    bufferedReceiveManager.AbandonBufferedReceives();
                }

                // Send the DeleteWorkflowOwner command to the instance store.
                if (this.instanceManager.InstanceStore != null)
                {
                    IAsyncResult executeResult = null;
                    this.handle = this.instanceManager.InstanceStore.CreateInstanceHandle(this.instanceManager.owner);
                    try
                    {
                        executeResult = this.instanceManager.InstanceStore.BeginExecute(this.handle,
                            new DeleteWorkflowOwnerCommand(), this.timeoutHelper.RemainingTime(),
                            this.PrepareAsyncCompletion(CloseAsyncResult.handleEndExecute), this);
                        return (SyncContinue(executeResult));
                    }
                    // Ignore some exceptions because DeleteWorkflowOwner is best effort.
                    catch (InstancePersistenceCommandException) { }
                    catch (InstanceOwnerException) { }
                    catch (OperationCanceledException) { }
                    finally
                    {
                        if (executeResult == null)
                        {
                            this.handle.Free();
                            this.handle = null;
                        }
                    }
                    return this.SyncContinue(executeResult);
                }
                else
                {
                    CloseProviderDirectory();
                    return true;
                }
            }

            static void HandleEndReleaseInstance(IAsyncResult result)
            {
                CloseAsyncResult thisPtr = (CloseAsyncResult)result.AsyncState;
                thisPtr.allReleaseInstancesCompletedSynchronously = thisPtr.allReleaseInstancesCompletedSynchronously && result.CompletedSynchronously;
                try
                {
                    WorkflowServiceInstance.EndReleaseInstanceForClose(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    // Ignore exception thrown from ReleaseInstanceAsyncResult.End.
                    // We do not complete CloseAsyncResult with this exception.
                    // Instead, we want to keep this thread running so that it can clean up other instances.
                    FxTrace.Exception.AsWarning(e);
                }

                if (Interlocked.Increment(ref thisPtr.completedUnloadCount) == thisPtr.instanceCount)
                {
                    if (thisPtr.PostProcess())
                    {
                        // If PostProcess completed synchronously, then the entire CloseAsyncResult is complete.
                        // Whether or not we completed syncrhonously depends on if all the ReleaseInstance invocations completed
                        // synchronously.
                        thisPtr.Complete(thisPtr.allReleaseInstancesCompletedSynchronously);
                    }
                }
                else
                {
                    thisPtr.Process();
                }
            }

            void CloseProviderDirectory()
            {
                bool success = false;
                try
                {
                    this.instanceManager.PersistenceProviderDirectory.Close();
                    success = true;
                }
                finally
                {
                    if (!success)
                    {
                        this.instanceManager.AbortDirectory();
                    }
                }
            }

            static bool HandleEndExecute(IAsyncResult result)
            {
                CloseAsyncResult thisPtr = (CloseAsyncResult)result.AsyncState;

                try
                {
                    thisPtr.instanceManager.owner = thisPtr.instanceManager.InstanceStore.EndExecute(result).InstanceOwner;
                }
                // Ignore some exceptions because DeleteWorkflowOwner is best effort.
                catch (InstancePersistenceCommandException) { }
                catch (InstanceOwnerException) { }
                catch (OperationCanceledException) { }
                finally
                {
                    thisPtr.handle.Free();
                    thisPtr.handle = null;
                }

                thisPtr.CloseProviderDirectory();
                return true;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseAsyncResult>(result);
            }

            static void HandleEndReleaseInstanceWrapperCallback(IAsyncResult result)
            {
                Fx.Assert(result != null, "Async result cannot be null!");

                CloseAsyncResult thisPtr = (CloseAsyncResult)result.AsyncState;

                Exception completionException = null;
                try
                {
                    HandleEndReleaseInstance(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                }

                // Exceptions thrown from Process and process callback should be handled in those methods respectively.
                // The only exception that can get here should be exception thrown from PostProcess.
                // PostProcess is guaranteed to be called only once.
                if (completionException != null)
                {
                    thisPtr.Complete(false, completionException);
                }
            }
        }

        // Need to ensure that any failure in the methods of GetInstanceAsyncResult after a WorkflowServiceInstance has been acquired
        // results in one of three outcomes, namely :
        // - the WorkflowServiceInstance is set to null
        // - the WorkflowServiceInstance is aborted
        // - ReleaseReference is called on the WorkflowServiceInstance to ensure that unload happens 
        //   (ultimately resulting in the WorkflowServiceInstance being aborted)
        // This is to prevent leaking WorkflowServiceInstances since nothing else has a handle to the WorkflowServiceInstance in those
        // scenarios.

        class GetInstanceAsyncResult : TransactedAsyncResult
        {
            static AsyncCompletion handleEndAcquireReference = new AsyncCompletion(HandleEndAcquireReference);
            static AsyncCompletion handleEndLoad = new AsyncCompletion(HandleEndLoad);
            static AsyncCompletion handleAssociateInfrastructureKeys = new AsyncCompletion(HandleAssociateInfrastructureKeys);
            static AsyncCompletion handleCommit = new AsyncCompletion(HandleCommit);
            static AsyncCompletion handleEndEnlistContext = new AsyncCompletion(HandleEndEnlistContext);
            static Action<AsyncResult, Exception> onCompleting = new Action<AsyncResult, Exception>(Finally);

            DurableInstanceManager instanceManager;
            Guid instanceId;
            InstanceKey instanceKey;
            ICollection<InstanceKey> additionalKeys;
            TimeSpan timeout;
            WorkflowServiceInstance durableInstance;
            bool referenceAcquired;
            PersistenceContext persistenceContext;
            WorkflowGetInstanceContext parameters;
            DependentTransaction transaction;
            CommittableTransaction committableTransaction;
            bool loadAny;
            WorkflowIdentityKey updatedIdentity;

            public GetInstanceAsyncResult(DurableInstanceManager instanceManager, InstanceKey instanceKey, ICollection<InstanceKey> additionalKeys, WorkflowGetInstanceContext parameters,
                TimeSpan timeout, AsyncCallback callback, object state)
                : this(instanceManager, parameters, timeout, callback, state)
            {
                Fx.Assert(instanceKey != null, "Instance key must be set.");

                this.instanceKey = instanceKey;
                this.additionalKeys = additionalKeys;

                if (this.GetInstance())
                {
                    this.Complete(true);
                }
            }

            public GetInstanceAsyncResult(DurableInstanceManager instanceManager, Guid instanceId, WorkflowGetInstanceContext parameters, WorkflowIdentityKey updatedIdentity,
                TimeSpan timeout, AsyncCallback callback, object state)
                : this(instanceManager, parameters, timeout, callback, state)
            {
                this.instanceId = instanceId;
                this.updatedIdentity = updatedIdentity;

                if (this.GetInstance())
                {
                    this.Complete(true);
                }
            }

            GetInstanceAsyncResult(DurableInstanceManager instanceManager, WorkflowGetInstanceContext parameters,
                TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.instanceManager = instanceManager;
                this.parameters = parameters;
                this.timeout = timeout;
                this.loadAny = parameters == null;
                this.OnCompleting = onCompleting;

                Transaction currentTransaction = Transaction.Current;
                if (currentTransaction == null && this.instanceManager.Host.IsLoadTransactionRequired)
                {
                    this.committableTransaction = new CommittableTransaction(this.timeout);
                    currentTransaction = committableTransaction;
                }
                if (currentTransaction != null)
                {
                    this.transaction = currentTransaction.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
                }
            }

            public static WorkflowServiceInstance End(IAsyncResult result)
            {
                return AsyncResult.End<GetInstanceAsyncResult>(result).durableInstance;
            }

            bool TryAcquire(bool fromCache)
            {
                this.durableInstance = this.persistenceContext.GetInstance(this.parameters);

                if (!fromCache)
                {
                    this.referenceAcquired = true;
                    return AssociateKeys();
                }

                IAsyncResult nextResult = this.durableInstance.BeginTryAcquireReference(this.timeout, this.PrepareAsyncCompletion(handleEndAcquireReference), this);
                return SyncContinue(nextResult);
            }

            static bool HandleEndAcquireReference(IAsyncResult result)
            {
                GetInstanceAsyncResult thisPtr = (GetInstanceAsyncResult)result.AsyncState;

                if (thisPtr.durableInstance.EndTryAcquireReference(result))
                {
                    thisPtr.referenceAcquired = true;
                    return thisPtr.TryEnlistContext();
                }
                else
                {
                    //We have to re-dispense this Durable Instance this is not usable.
                    thisPtr.referenceAcquired = false;
                    thisPtr.durableInstance = null;
                    return thisPtr.GetInstance();
                }
            }

            bool TryEnlistContext()
            {
                IAsyncResult enlistResult = null;
                bool tryAgain = false;

                // We need to enlist for the transaction. This call will wait until
                // we obtain the transaction lock on the PersistenceContext, too. If there is no current transaction, this call
                // will still wait to get the transaction lock, but we not create an enlistment.
                using (PrepareTransactionalCall(this.transaction))
                {
                    try
                    {
                        enlistResult = this.persistenceContext.BeginEnlist(this.timeout, PrepareAsyncCompletion(handleEndEnlistContext), this);
                    }
                    catch (ObjectDisposedException)
                    {
                        tryAgain = true;
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        throw FxTrace.Exception.AsError(new OperationCanceledException(SR.DefaultAbortReason));
                    }
                }

                if (tryAgain)
                {
                    this.referenceAcquired = false;
                    this.durableInstance = null;
                    return this.GetInstance();
                }
                else
                {
                    return SyncContinue(enlistResult);
                }
            }

            static bool HandleEndEnlistContext(IAsyncResult result)
            {
                GetInstanceAsyncResult thisPtr = (GetInstanceAsyncResult)result.AsyncState;

                // 

                try
                {
                    thisPtr.persistenceContext.EndEnlist(result);
                }
                catch (ObjectDisposedException)
                {
                    // It's possible that the PersistenceContext was closed and removed from the cache
                    // while we were queued up for it. In that situation, this call to EndEnlist will
                    // throw an ObjectDisposedException because the PersistenceContext is in the closed
                    // state. If that happens, we need to try the load again from the beginning.
                    thisPtr.referenceAcquired = false;
                    thisPtr.durableInstance = null;
                    return thisPtr.GetInstance();
                }
                catch (CommunicationObjectAbortedException)
                {
                    throw FxTrace.Exception.AsError(new OperationCanceledException(SR.DefaultAbortReason));
                }

                return thisPtr.AssociateKeys();
            }

            bool GetInstance()
            {
                IAsyncResult nextResult = null;

                if (!this.loadAny && this.parameters.CanCreateInstance)
                {
                    Fx.Assert(this.updatedIdentity == null, "Update() can never create instance. Enable this path if we ever support updating via user-defined operation.");
                    if (this.instanceKey != null && this.instanceKey.IsValid)
                    {
                        nextResult = this.instanceManager.PersistenceProviderDirectory.BeginLoadOrCreate(
                            this.instanceKey, Guid.Empty, this.additionalKeys, this.transaction,
                            this.timeout, PrepareAsyncCompletion(handleEndLoad), this);
                    }
                    else
                    {
                        // Either invalid key (new instance) or lookup by instance ID.
                        nextResult = this.instanceManager.PersistenceProviderDirectory.BeginLoadOrCreate(
                            this.instanceId, this.additionalKeys, this.transaction,
                            this.timeout, PrepareAsyncCompletion(handleEndLoad), this);
                    }
                }
                else
                {
                    if (this.instanceKey != null)
                    {
                        Fx.Assert(this.updatedIdentity == null, "Update() always has the instance ID. Enable this path if we ever support updating via user-defined operation that relies on correlation.");
                        nextResult = this.instanceManager.PersistenceProviderDirectory.BeginLoad(
                            this.instanceKey, this.additionalKeys, this.transaction,
                            this.timeout, PrepareAsyncCompletion(handleEndLoad), this);
                    }
                    else
                    {
                        nextResult = this.instanceManager.PersistenceProviderDirectory.BeginLoad(
                            this.instanceId, null, this.transaction, this.loadAny, this.updatedIdentity,
                            this.timeout, PrepareAsyncCompletion(handleEndLoad), this);
                    }
                }
                return SyncContinue(nextResult);
            }

            bool AssociateKeys()
            {
                if (this.additionalKeys != null && this.additionalKeys.Count > 0)
                {
                    IAsyncResult result;
                    try
                    {
                        result = this.durableInstance.BeginAssociateInfrastructureKeys(this.additionalKeys, this.transaction, this.timeout,
                                PrepareAsyncCompletion(handleAssociateInfrastructureKeys), this);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        this.persistenceContext.Abort();
                        throw;
                    }

                    return SyncContinue(result);
                }
                else
                {
                    return CommitTransaction();
                }
            }

            static bool HandleEndLoad(IAsyncResult result)
            {
                GetInstanceAsyncResult thisPtr = (GetInstanceAsyncResult)result.AsyncState;

                PersistenceContext previousPersistenceContext = thisPtr.persistenceContext;
                bool fromCache;
                if (!thisPtr.loadAny && thisPtr.parameters.CanCreateInstance)
                {
                    thisPtr.persistenceContext = thisPtr.instanceManager.PersistenceProviderDirectory.EndLoadOrCreate(result, out fromCache);
                }
                else
                {
                    thisPtr.persistenceContext = thisPtr.instanceManager.PersistenceProviderDirectory.EndLoad(result, out fromCache);
                }
                Fx.AssertAndThrow(previousPersistenceContext != thisPtr.persistenceContext, "PPD should not load same PersistenceContext for the same GetInstanceAsyncResult!");
                return thisPtr.TryAcquire(fromCache);
            }

            static bool HandleAssociateInfrastructureKeys(IAsyncResult result)
            {
                GetInstanceAsyncResult thisPtr = (GetInstanceAsyncResult)result.AsyncState;

                try
                {
                    thisPtr.durableInstance.EndAssociateInfrastructureKeys(result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    thisPtr.persistenceContext.Abort();
                    throw;
                }

                return thisPtr.CommitTransaction();
            }

            bool CommitTransaction()
            {
                if (this.transaction != null)
                {
                    this.transaction.Complete();
                }
                if (this.committableTransaction != null)
                {
                    IAsyncResult result = this.committableTransaction.BeginCommit(PrepareAsyncCompletion(handleCommit), this);
                    return SyncContinue(result);
                }
                else
                {
                    return true;
                }
            }

            static bool HandleCommit(IAsyncResult result)
            {
                GetInstanceAsyncResult thisPtr = (GetInstanceAsyncResult)result.AsyncState;
                thisPtr.committableTransaction.EndCommit(result);
                thisPtr.committableTransaction = null;
                return true;
            }

            static void Finally(AsyncResult result, Exception exception)
            {
                GetInstanceAsyncResult thisPtr = (GetInstanceAsyncResult)result;

                if (thisPtr.committableTransaction != null)
                {
                    Fx.Assert(exception != null, "Shouldn't get here in the success case.");

                    try
                    {
                        thisPtr.committableTransaction.Rollback(exception);
                    }
                    catch (Exception rollbackException)
                    {
                        if (Fx.IsFatal(rollbackException))
                        {
                            throw;
                        }
                        FxTrace.Exception.AsWarning(rollbackException);
                    }
                }

                // Reference is acquired on an instance but we fail perform subsequent task before
                // return an instance to the client (Tx Enlist timeout).  We are responsible to 
                // release the reference.  We don't need to worry about Aborted or other State (has
                // no effect on ref counting).
                if (thisPtr.referenceAcquired && exception != null)
                {
                    Fx.Assert(thisPtr.durableInstance != null, "durableInstance must not be null!");
                    thisPtr.durableInstance.ReleaseReference();
                }
            }
        }

        // This async result waits for store events and handle them (currently only support HasRunnableWorkflowEvent).
        // It is intended to always complete async to simplify caller usage. 
        // 1) no code to handle sync completion. 
        // 2) recursive call will be safe from StackOverflow.
        // For simplicity, we handle (load/run) each event one-by-one.
        // We ---- certain set of exception (see HandleException).  Other will crash the process.
        // InvalidOperation is also handled due to TryLoadRunnableWorkflowCommand could fail if ---- with other hosts.
        class WaitAndHandleStoreEventsAsyncResult : AsyncResult
        {
            static Action<object> waitAndHandleStoreEvents = new Action<object>(WaitAndHandleStoreEvents);
            static AsyncCompletion handleEndWaitForStoreEvents = new AsyncCompletion(HandleEndWaitForStoreEvents);
            static AsyncCompletion handleEndGetInstance = new AsyncCompletion(HandleEndGetInstance);
            static AsyncCompletion handleEndRunInstance = new AsyncCompletion(HandleEndRunInstance);

            DurableInstanceManager instanceManager;
            IEnumerator<InstancePersistenceEvent> events;
            WorkflowServiceInstance currentInstance;

            public WaitAndHandleStoreEventsAsyncResult(DurableInstanceManager instanceManager, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.instanceManager = instanceManager;
                ActionItem.Schedule(waitAndHandleStoreEvents, this);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WaitAndHandleStoreEventsAsyncResult>(result);
            }

            static void WaitAndHandleStoreEvents(object state)
            {
                WaitAndHandleStoreEventsAsyncResult thisPtr = (WaitAndHandleStoreEventsAsyncResult)state;

                bool completeSelf;
                Exception completionException = null;
                try
                {
                    completeSelf = thisPtr.WaitForStoreEvents();
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

            bool WaitForStoreEvents()
            {
                // Defense in depth with a predefined timeout
                IAsyncResult result = this.instanceManager.InstanceStore.BeginWaitForEvents(this.instanceManager.handle, 
                    TimeSpan.FromSeconds(600), PrepareAsyncCompletion(handleEndWaitForStoreEvents), this);
                return SyncContinue(result);
            }

            static bool HandleEndWaitForStoreEvents(IAsyncResult result)
            {
                WaitAndHandleStoreEventsAsyncResult thisPtr = (WaitAndHandleStoreEventsAsyncResult)result.AsyncState;
                thisPtr.events = thisPtr.instanceManager.InstanceStore.EndWaitForEvents(result).GetEnumerator();
                return thisPtr.HandleStoreEvents();
            }

            bool HandleStoreEvents()
            {
                if (!this.events.MoveNext())
                {
                    return true;
                }

                InstancePersistenceEvent currentEvent = this.events.Current;
                if (currentEvent.Name == HasRunnableWorkflowEvent.Value.Name)
                {
                    try
                    {
                        IAsyncResult result = this.instanceManager.BeginGetInstance(Guid.Empty, null, null, this.instanceManager.Host.PersistTimeout,
                            PrepareAsyncCompletion(handleEndGetInstance), this);
                        return SyncContinue(result);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception) || !this.instanceManager.HandleException(exception))
                        {
                            throw;
                        }
                    }
                }
                else
                {
                    Fx.AssertAndThrow("Unknown InstancePersistenceEvent (" + currentEvent.Name + ")!");
                }

                return HandleStoreEvents();
            }

            static bool HandleEndGetInstance(IAsyncResult result)
            {
                WaitAndHandleStoreEventsAsyncResult thisPtr = (WaitAndHandleStoreEventsAsyncResult)result.AsyncState;
                try
                {
                    thisPtr.currentInstance = thisPtr.instanceManager.EndGetInstance(result);
                    return thisPtr.RunInstance();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception) || !thisPtr.instanceManager.HandleException(exception))
                    {
                        throw;
                    }
                }
                return thisPtr.HandleStoreEvents();
            }

            bool RunInstance()
            {
                try
                {
                    IAsyncResult result = this.currentInstance.BeginRun(null, TimeSpan.MaxValue, PrepareAsyncCompletion(handleEndRunInstance), this);
                    return SyncContinue(result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (this.currentInstance != null)
                    {
                        this.currentInstance.ReleaseReference();
                        this.currentInstance = null;
                    }
                    if (!this.instanceManager.HandleException(exception))
                    {
                        throw;
                    }
                }
                return HandleStoreEvents();
            }

            static bool HandleEndRunInstance(IAsyncResult result)
            {
                WaitAndHandleStoreEventsAsyncResult thisPtr = (WaitAndHandleStoreEventsAsyncResult)result.AsyncState;
                try
                {
                    thisPtr.currentInstance.EndRun(result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception) || !thisPtr.instanceManager.HandleException(exception))
                    {
                        throw;
                    }
                }
                finally
                {
                    thisPtr.currentInstance.ReleaseReference();
                    thisPtr.currentInstance = null;
                }
                return thisPtr.HandleStoreEvents();
            }
        }
    }
}
