//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Transactions;
    using System.Xml.Linq;
    using System.Threading;

    [Fx.Tag.XamlVisible(false)]
    public sealed class SqlWorkflowInstanceStore : InstanceStore
    {
        internal const int DefaultMaximumRetries = 4;
        internal const string CommonConnectionPoolName = "System.Activities.DurableInstancing.SqlWorkflowInstanceStore";
        static readonly TimeSpan defaultConnectionOpenTime = TimeSpan.FromSeconds(15);
        static readonly TimeSpan defaultInstancePersistenceEventDetectionPeriod = TimeSpan.FromSeconds(5);
        static readonly TimeSpan defaultLockRenewalPeriod = TimeSpan.FromSeconds(30);        
        static readonly TimeSpan minimumTimeSpanAllowed = TimeSpan.FromSeconds(1);

        const string DefaultPromotionName = "System.Activities.InstanceMetadata";
        TimeSpan bufferedHostLockRenewalPeriod;
        string cachedConnectionString;

        string connectionString;
        Dictionary<string, Tuple<List<XName>, List<XName>>> definedPromotions;
        bool enqueueRunCommands;
        TimeSpan hostLockRenewalPeriod;
        InstanceCompletionAction instanceCompletionAction;

        InstanceEncodingOption instanceEncodingOption;
        InstanceLockedExceptionAction instanceLockedExceptionAction;
        TimeSpan instancePersistenceEventDetectionPeriod;
        bool isReadOnly;
        Action<object> scheduledUnlockInstance;
        SqlWorkflowInstanceStoreLock storeLock;

        AsyncCallback unlockInstanceCallback;

        // Volatile: multiple threads could simultaneously do a TestVersionAndRunAsyncResult, and read/update this value.
        volatile Version databaseVersion;

        public SqlWorkflowInstanceStore() :
            this(null)
        {
        }

        public SqlWorkflowInstanceStore(string connectionString)
        {
            this.InstanceEncodingOption = SqlWorkflowInstanceStoreConstants.DefaultInstanceEncodingOption;
            this.InstanceCompletionAction = SqlWorkflowInstanceStoreConstants.DefaultInstanceCompletionAction;
            this.InstanceLockedExceptionAction = SqlWorkflowInstanceStoreConstants.DefaultInstanceLockedExceptionAction;
            this.HostLockRenewalPeriod = SqlWorkflowInstanceStore.defaultLockRenewalPeriod;
            this.RunnableInstancesDetectionPeriod = SqlWorkflowInstanceStore.defaultInstancePersistenceEventDetectionPeriod;
            this.EnqueueRunCommands = false;
            this.LoadRetryHandler = new LoadRetryHandler();
            this.ConnectionString = connectionString;
            this.definedPromotions = new Dictionary<string, Tuple<List<XName>, List<XName>>>();
            this.bufferedHostLockRenewalPeriod = TimeSpan.Zero;
            this.unlockInstanceCallback = Fx.ThunkCallback(UnlockInstanceCallback);
            this.scheduledUnlockInstance = new Action<object>(ScheduledUnlockInstance);
            this.storeLock = new SqlWorkflowInstanceStoreLock(this);
            this.MaxConnectionRetries = DefaultMaximumRetries;
        }

        public string ConnectionString
        {
            get 
            { 
                return this.connectionString; 
            }
            set
            {
                ThrowIfReadOnly();
                this.connectionString = value;
            }
        }

        public bool EnqueueRunCommands
        {
            get
            {
                return this.enqueueRunCommands;
            }
            set
            {
                ThrowIfReadOnly();
                this.enqueueRunCommands = value;
            }
        }

        public TimeSpan HostLockRenewalPeriod
        {
            get
            {
                return this.hostLockRenewalPeriod;
            }
            set
            {
                if (value.CompareTo(SqlWorkflowInstanceStore.minimumTimeSpanAllowed) < 0)
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("lockRenewalPeriod", value, SR.InvalidLockRenewalPeriod(value, SqlWorkflowInstanceStore.minimumTimeSpanAllowed));
                }
                ThrowIfReadOnly();
                this.hostLockRenewalPeriod = value;
            }
        }

        public InstanceCompletionAction InstanceCompletionAction
        {
            get
            {
                return this.instanceCompletionAction;
            }
            set
            {
                ThrowIfReadOnly();
                this.instanceCompletionAction = value;
            }
        }

        public InstanceEncodingOption InstanceEncodingOption
        {
            get
            {
                return this.instanceEncodingOption;
            }
            set
            {
                ThrowIfReadOnly();
                this.instanceEncodingOption = value;
            }
        }

        public InstanceLockedExceptionAction InstanceLockedExceptionAction
        {
            get
            {
                return this.instanceLockedExceptionAction;
            }
            set
            {
                ThrowIfReadOnly();
                this.instanceLockedExceptionAction = value;
            }
        }

        public TimeSpan RunnableInstancesDetectionPeriod
        {
            get
            {
                return this.instancePersistenceEventDetectionPeriod;
            }
            set
            {
                if (value.CompareTo(SqlWorkflowInstanceStore.minimumTimeSpanAllowed) < 0)
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("instancePersistenceEventDetectionPeriod", value, SR.InvalidRunnableInstancesDetectionPeriod(value, SqlWorkflowInstanceStore.minimumTimeSpanAllowed));
                }
                ThrowIfReadOnly();
                this.instancePersistenceEventDetectionPeriod = value;
            }
        }

        public int MaxConnectionRetries
        {
            get;
            set;
        }

        internal TimeSpan BufferedHostLockRenewalPeriod
        {
            get
            {
                Fx.Assert(this.isReadOnly, "Should not be called before there are any handles");
                if (this.bufferedHostLockRenewalPeriod == TimeSpan.Zero)
                {
                    double lockBuffer = Math.Min(SqlWorkflowInstanceStoreConstants.LockOwnerTimeoutBuffer.TotalSeconds, (TimeSpan.MaxValue.Subtract(this.HostLockRenewalPeriod)).TotalSeconds);
                    this.bufferedHostLockRenewalPeriod = TimeSpan.FromSeconds(Math.Min(Int32.MaxValue, lockBuffer + this.HostLockRenewalPeriod.TotalSeconds));
                }
                return this.bufferedHostLockRenewalPeriod;
            }
        }

        internal string CachedConnectionString
        {
            get
            {
                return this.cachedConnectionString;
            }
        }

        internal LoadRetryHandler LoadRetryHandler
        {
            get;
            set;
        }

        internal Dictionary<string, Tuple<List<XName>, List<XName>>> Promotions
        {
            get
            {
                return this.definedPromotions;
            }
        }

        internal ILoadRetryStrategy RetryStrategy 
        { 
            get; 
            set; 
        }

        internal Guid WorkflowHostType
        {
            get;
            set;
        }

        internal bool InstanceOwnersExist
        {
            get
            {
                return base.GetInstanceOwners().Length > 0;
            }
        }

        internal Version DatabaseVersion
        {
            get
            {
                return this.databaseVersion;
            }
            set
            {
                Fx.Assert(this.databaseVersion == null || this.databaseVersion == value, "Database version should not have changed out from under us");
                this.databaseVersion = value;
            }
        }

        object ThisLock
        {
            get
            {
                return this.definedPromotions;
            }
        }

        public void Promote(string name, IEnumerable<XName> promoteAsVariant, IEnumerable<XName> promoteAsBinary)
        {
            ThrowIfReadOnly();

            if (string.IsNullOrEmpty(name))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("name");
            }

            if (this.definedPromotions.ContainsKey(name))
            {
                throw FxTrace.Exception.Argument("name", SR.PromotionAlreadyDefined(name));
            }

            if (promoteAsVariant == null && promoteAsBinary == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.NoPromotionsDefined(name)));
            }

            if (promoteAsVariant != null && promoteAsVariant.Count() > SqlWorkflowInstanceStoreConstants.MaximumPropertiesPerPromotion)
            {
                throw FxTrace.Exception.Argument("promoteAsVariant", SR.PromotionTooManyDefined(name,
                    promoteAsVariant.Count(), "variant", SqlWorkflowInstanceStoreConstants.MaximumPropertiesPerPromotion));
            }

            if (promoteAsBinary != null && promoteAsBinary.Count() > SqlWorkflowInstanceStoreConstants.MaximumPropertiesPerPromotion)
            {
                throw FxTrace.Exception.Argument("promoteAsVariant", SR.PromotionTooManyDefined(name,
                    promoteAsVariant.Count(), "binary", SqlWorkflowInstanceStoreConstants.MaximumPropertiesPerPromotion));
            }

            HashSet<XName> promotedXNames = new HashSet<XName>();
            List<XName> variant = new List<XName>();

            if (promoteAsVariant != null)
            {
                foreach (XName xname in promoteAsVariant)
                {
                    if (xname == null)
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CanNotDefineNullForAPromotion("variant", name)));
                    }

                    if (promotedXNames.Contains(xname))
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CannotPromoteXNameTwiceInPromotion(xname.ToString(), name)));
                    }

                    variant.Add(xname);
                    promotedXNames.Add(xname);
                }
            }

            List<XName> binary = new List<XName>();

            if (promoteAsBinary != null)
            {
                foreach (XName xname in promoteAsBinary)
                {
                    if (name == null)
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CanNotDefineNullForAPromotion("binary", xname)));
                    }

                    if (promotedXNames.Contains(xname))
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CannotPromoteXNameTwiceInPromotion(xname.ToString(), name)));
                    }

                    binary.Add(xname);
                    promotedXNames.Add(xname);
                }
            }

            this.definedPromotions.Add(name, new Tuple<List<XName>, List<XName>>(variant, binary));
        }

        protected internal override IAsyncResult BeginTryCommand(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }

            if (command == null)
            {
                throw FxTrace.Exception.ArgumentNull("command");
            }

            if (!this.storeLock.IsValid && !(command is CreateWorkflowOwnerCommand) && !(command is CreateWorkflowOwnerWithIdentityCommand))
            {
                throw FxTrace.Exception.AsError(new InstanceOwnerException(command.Name, this.storeLock.LockOwnerId));
            }

            if (this.IsRetryCommand(command))
            {
                return new LoadRetryAsyncResult(this, context, command, timeout, callback, state);
            }

            return BeginTryCommandSkipRetry(context, command, timeout, callback, state);
        }
                
        internal IAsyncResult BeginTryCommandSkipRetry(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (command is CreateWorkflowOwnerWithIdentityCommand)
            {
                return this.BeginTryCommandInternalWithVersionCheck(context, command, timeout, callback, state, StoreUtilities.Version45);
            }
            else if (command is DetectRunnableInstancesCommand)
            {
                return this.BeginTryCommandInternalWithVersionCheck(context, command, timeout, callback, state, StoreUtilities.Version40);
            }
            else if (command is SaveWorkflowCommand)
            {
                return this.BeginTryCommandInternalWithVersionCheck(context, command, timeout, callback, state, StoreUtilities.Version40);
            }
            else
            {
                return this.BeginTryCommandInternal(context, command, timeout, callback, state);
            }
        }

        protected internal override bool EndTryCommand(IAsyncResult result)
        {
            if (result is LoadRetryAsyncResult)
            {
                return LoadRetryAsyncResult.End(result);
            }
            else if (result is SqlWorkflowInstanceStoreAsyncResult)
            {
                return SqlWorkflowInstanceStoreAsyncResult.End(result);
            }
            else
            {
                return base.EndTryCommand(result);
            }
        }

        internal IAsyncResult BeginTryCommandInternalWithVersionCheck(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state, Version targetVersion)
        {
            SqlWorkflowInstanceStoreAsyncResult sqlWorkflowInstanceStoreAsyncResult = new TestDatabaseVersionAndRunAsyncResult(context, command, this, this.storeLock, Transaction.Current, timeout, targetVersion, callback, state);
            sqlWorkflowInstanceStoreAsyncResult.ScheduleCallback();
            return sqlWorkflowInstanceStoreAsyncResult;
        }

        internal IAsyncResult BeginTryCommandInternal(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return BeginTryCommandInternal(context, command, Transaction.Current, timeout, callback, state);
        }

        internal IAsyncResult BeginTryCommandInternal(InstancePersistenceContext context, InstancePersistenceCommand command, Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
        {
            SqlWorkflowInstanceStoreAsyncResult sqlWorkflowInstanceStoreAsyncResult = null;

            if (command is SaveWorkflowCommand)
            {
                sqlWorkflowInstanceStoreAsyncResult = new SaveWorkflowAsyncResult(context, command, this, this.storeLock, transaction, timeout, callback, state);
            }
            else if (command is TryLoadRunnableWorkflowCommand)
            {
                sqlWorkflowInstanceStoreAsyncResult = new TryLoadRunnableWorkflowAsyncResult(context, command, this, this.storeLock, transaction, timeout, callback, state);
            }
            else if (command is LoadWorkflowCommand)
            {
                sqlWorkflowInstanceStoreAsyncResult = new LoadWorkflowAsyncResult(context, command, this, this.storeLock, transaction, timeout, callback, state);
            }
            else if (command is LoadWorkflowByInstanceKeyCommand)
            {
                sqlWorkflowInstanceStoreAsyncResult = new LoadWorkflowByKeyAsyncResult(context, command, this, this.storeLock, transaction, timeout, callback, state);
            }
            else if (command is ExtendLockCommand)
            {
                sqlWorkflowInstanceStoreAsyncResult = new ExtendLockAsyncResult(null, command, this, this.storeLock, null, timeout, callback, state);
            }
            else if (command is DetectRunnableInstancesCommand)
            {
                sqlWorkflowInstanceStoreAsyncResult = new DetectRunnableInstancesAsyncResult(null, command, this, this.storeLock, null, timeout, callback, state);
            }
            else if (command is DetectActivatableWorkflowsCommand)
            {
                sqlWorkflowInstanceStoreAsyncResult = new DetectActivatableWorkflowsAsyncResult(null, command, this, this.storeLock, null, timeout, callback, state);
            }             
            else if (command is RecoverInstanceLocksCommand)
            {
                sqlWorkflowInstanceStoreAsyncResult = new RecoverInstanceLocksAsyncResult(null, command, this, this.storeLock, null, timeout, callback, state);
            }
            else if (command is UnlockInstanceCommand)
            {
                sqlWorkflowInstanceStoreAsyncResult = new UnlockInstanceAsyncResult(null, command, this, this.storeLock, transaction, timeout, callback, state);
            }
            else if (command is CreateWorkflowOwnerCommand || command is CreateWorkflowOwnerWithIdentityCommand)
            {
                sqlWorkflowInstanceStoreAsyncResult = new CreateWorkflowOwnerAsyncResult(context, command, this, this.storeLock, transaction, timeout, callback, state);
            }
            else if (command is DeleteWorkflowOwnerCommand)
            {
                sqlWorkflowInstanceStoreAsyncResult = new DeleteWorkflowOwnerAsyncResult(context, command, this, this.storeLock, transaction, timeout, callback, state);
            }
            else if (command is QueryActivatableWorkflowsCommand)
            {
                sqlWorkflowInstanceStoreAsyncResult = new QueryActivatableWorkflowAsyncResult(context, command, this, this.storeLock, transaction, timeout, callback, state);
            }
            else
            {
                return base.BeginTryCommand(context, command, timeout, callback, state);
            }

            sqlWorkflowInstanceStoreAsyncResult.ScheduleCallback();

            return sqlWorkflowInstanceStoreAsyncResult;
        }

        internal bool EnqueueRetry(LoadRetryAsyncResult loadRetryAsyncResult)
        {
            Fx.Assert(this.IsLockRetryEnabled(),
                "EnqueueRetry() should not be invoked if retry algorithm is set to NoRetry");

            bool result = false;

            if (this.storeLock.IsValid)
            {
                result = this.LoadRetryHandler.Enqueue(loadRetryAsyncResult);
            }

            return result;
        }

        internal InstancePersistenceEvent FindEvent(InstancePersistenceEvent eventType, out InstanceOwner instanceOwner)
        {
            return FindEventHelper(eventType, out instanceOwner, false);
        }

        internal InstancePersistenceEvent FindEventWithReset(InstancePersistenceEvent eventType, out InstanceOwner instanceOwner)
        {
            return FindEventHelper(eventType, out instanceOwner, true);
        }

        internal void GenerateUnlockCommand(InstanceLockTracking instanceLockTracking)
        {
            UnlockInstanceCommand command = new UnlockInstanceCommand
                {
                    SurrogateOwnerId = this.storeLock.SurrogateLockOwnerId,
                    InstanceId = instanceLockTracking.InstanceId,
                    InstanceVersion = instanceLockTracking.InstanceVersion
                };

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                this.BeginTryCommandInternal(null, command, TimeSpan.MaxValue, this.unlockInstanceCallback, command);
            }
        }

        internal TimeSpan GetNextRetryDelay(int retryAttempt)
        {
            Fx.Assert(this.IsLockRetryEnabled(),
                "GetNextRetryDelay() should not be invoked if retry algorithm is set to NoRetry");

            return (this.RetryStrategy.RetryDelay(retryAttempt));
        }

        internal bool IsLockRetryEnabled()
        {
            return (this.InstanceLockedExceptionAction != InstanceLockedExceptionAction.NoRetry);
        }

        internal void UpdateEventStatus(bool signalEvent, InstancePersistenceEvent eventToUpdate)
        {
            // FindEventWithReset will allow the event to be cleaned up, even if it is signalled.  The returned event will
            // always be reset.
            InstanceOwner instanceOwner;
            InstancePersistenceEvent requiredEvent = this.FindEventWithReset(eventToUpdate, out instanceOwner);
            if (requiredEvent != null)
            {
                if (signalEvent)
                {
                    base.SignalEvent(requiredEvent, instanceOwner);
                }
            }            
        }
        
        protected override void OnFreeInstanceHandle(InstanceHandle instanceHandle, object userContext)
        {
            InstanceLockTracking instanceLockTracking = (InstanceLockTracking)(userContext);
            instanceLockTracking.HandleFreed();
        }

        protected override object OnNewInstanceHandle(InstanceHandle instanceHandle)
        {
            MakeReadOnly();
            return new InstanceLockTracking(this);
        }

        void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                lock (ThisLock)
                {
                    if (!this.isReadOnly)
                    {
                        this.cachedConnectionString = this.CreateCachedConnectionString();
                        this.SetLoadRetryStrategy();
                        this.isReadOnly = true;
                    }
                }
            }
        }

        string CreateCachedConnectionString()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(this.ConnectionString)
                {
                    AsynchronousProcessing = true,
                    ConnectTimeout = (int) SqlWorkflowInstanceStore.defaultConnectionOpenTime.TotalSeconds,
                    ApplicationName = "DefaultPool"
                };

            return builder.ToString();
        }

        InstancePersistenceEvent FindEventHelper(InstancePersistenceEvent eventType, out InstanceOwner instanceOwner, bool withReset)
        {
            instanceOwner = null;
            InstanceOwner[] instanceOwners = GetInstanceOwners();

            if (instanceOwners.Length > 0)
            {
                foreach (InstanceOwner owner in instanceOwners)
                {
                    if (owner.InstanceOwnerId == this.storeLock.LockOwnerId)
                    {
                        instanceOwner = owner;
                        break;
                    }
                }

                if (instanceOwner != null)
                {
                    // Reset first.  That will allow the event to be cleaned up, so GetEvents won't return it (it will always return signalled events).
                    if (withReset)
                    {
                        base.ResetEvent(eventType, instanceOwner);
                    }
                    InstancePersistenceEvent[] registeredEvents = base.GetEvents(instanceOwner);

                    foreach (InstancePersistenceEvent persistenceEvent in registeredEvents)
                    {
                        if (persistenceEvent == eventType)
                        {
                            return persistenceEvent;
                        }
                    }
                }
            }

            return null;
        }

        bool IsRetryCommand(InstancePersistenceCommand command)
        {
            return 
                (
                this.IsLockRetryEnabled() &&
                (
                command is LoadWorkflowByInstanceKeyCommand ||
                command is LoadWorkflowCommand
                )
                );
        }

        void ScheduledUnlockInstance(object state)
        {
            UnlockInstanceState unlockInstanceState = (UnlockInstanceState) state;
            UnlockInstanceCommand command = unlockInstanceState.UnlockInstanceCommand;

            try
            {
                this.BeginTryCommandInternal(null, command, TimeSpan.MaxValue, unlockInstanceCallback, command);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                if (TD.UnlockInstanceExceptionIsEnabled())
                {
                    TD.UnlockInstanceException(e.Message);
                }
                // Keep on going - if problems are severe the host will be faulted and we'll give up then.
                unlockInstanceState.BackoffTimeoutHelper.WaitAndBackoff(this.scheduledUnlockInstance, unlockInstanceState);
            }
        }

        void SetLoadRetryStrategy()
        {
            this.RetryStrategy = LoadRetryStrategyFactory.CreateRetryStrategy(this.InstanceLockedExceptionAction);
        }

        void ThrowIfReadOnly()
        {
            if (this.isReadOnly)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.InstanceStoreReadOnly));
            }
        }

        void UnlockInstanceCallback(IAsyncResult result)
        {
            try
            {
                this.EndTryCommand(result);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }

                if (TD.UnlockInstanceExceptionIsEnabled())
                {
                    TD.UnlockInstanceException(exception.Message);
                }

                UnlockInstanceState unlockInstanceState = new UnlockInstanceState
                    {
                        UnlockInstanceCommand = (UnlockInstanceCommand)(result.AsyncState),
                        BackoffTimeoutHelper = new BackoffTimeoutHelper(TimeSpan.MaxValue)
                    };

                unlockInstanceState.BackoffTimeoutHelper.WaitAndBackoff(this.scheduledUnlockInstance, unlockInstanceState);
            }
        }

        class UnlockInstanceState
        {
            public BackoffTimeoutHelper BackoffTimeoutHelper { get; set; }
            public UnlockInstanceCommand UnlockInstanceCommand { get; set; }
        }
    }
}
