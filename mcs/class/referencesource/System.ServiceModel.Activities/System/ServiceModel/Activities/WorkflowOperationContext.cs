//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Activities.Dispatcher;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Transactions;
    using TD2 = System.ServiceModel.Diagnostics.Application.TD;

    // This AsyncResult completion is very tricky. It can complete in two path.
    // FastPath: When workflow executes synchronously(SendFault/SendReply called before ResumeBookmark completes) we complete the AsyncResult at HandleEndResumeBookmark;
    // Async Path: If workflow goes async (SendFault/SendReply called after ResumeBookmark completes) we complete the AsyncResult at SendFault/SendReply.
    class WorkflowOperationContext : AsyncResult
    {
        static readonly ReadOnlyDictionaryInternal<string, string> emptyDictionary = new ReadOnlyDictionaryInternal<string, string>(new Dictionary<string, string>());
        static readonly object[] emptyObjectArray = new object[0];
        static AsyncCompletion handleEndResumeBookmark = new AsyncCompletion(HandleEndResumeBookmark);
        static AsyncCompletion handleEndWaitForPendingOperations = new AsyncCompletion(HandleEndWaitForPendingOperations);
        static AsyncCompletion handleEndProcessReceiveContext;
        static Action<AsyncResult, Exception> onCompleting = new Action<AsyncResult, Exception>(Finally);
        object[] inputs;
        string operationName;
        object[] outputs;
        object operationReturnValue;
        object thisLock;

        WorkflowServiceInstance workflowInstance;
        Bookmark bookmark;
        object bookmarkValue;
        BookmarkScope bookmarkScope;

        IInvokeReceivedNotification notification;
        IAsyncResult pendingAsyncResult;

        TimeoutHelper timeoutHelper;
        Exception pendingException;

        ReceiveContext receiveContext;

        // perf counter data
        bool performanceCountersEnabled;
        long beginTime;

        // tracing data
        bool propagateActivity;
        Guid ambientActivityId;
        Guid e2eActivityId;
        EventTraceActivity eventTraceActivity;
        long beginOperation;

        //Tracking for decrement of ASP.NET busy count
        bool hasDecrementedBusyCount;

        WorkflowOperationContext(object[] inputs, OperationContext operationContext, string operationName,
            bool performanceCountersEnabled, bool propagateActivity, Transaction currentTransaction,
            WorkflowServiceInstance workflowInstance, IInvokeReceivedNotification notification, WorkflowOperationBehavior behavior, ServiceEndpoint endpoint,
            TimeSpan timeout, AsyncCallback callback, object state)
            : base(callback, state)
        {
            this.inputs = inputs;
            this.operationName = operationName;
            this.OperationContext = operationContext;
            this.ServiceEndpoint = endpoint;
            this.CurrentTransaction = currentTransaction;
            this.performanceCountersEnabled = performanceCountersEnabled;
            this.propagateActivity = propagateActivity;
            this.timeoutHelper = new TimeoutHelper(timeout);
            this.workflowInstance = workflowInstance;
            this.thisLock = new object();
            this.notification = notification;
            this.OnCompleting = onCompleting;

            // Resolve bookmark
            Fx.Assert(behavior != null, "behavior must not be null!");
            this.bookmark = behavior.OnResolveBookmark(this, out this.bookmarkScope, out this.bookmarkValue);
            Fx.Assert(this.bookmark != null, "bookmark must not be null!");

            bool completeSelf = false;

            try
            {
                // set activity ID on the executing thread (Bug 113386)
                if (TraceUtility.MessageFlowTracingOnly)
                {
                    this.e2eActivityId = TraceUtility.GetReceivedActivityId(this.OperationContext);
                    DiagnosticTraceBase.ActivityId = this.e2eActivityId;
                }

                if (Fx.Trace.IsEtwProviderEnabled)
                {
                    this.eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(this.OperationContext.IncomingMessage);
                }

                // Take ownership of the ReceiveContext when buffering is enabled by removing the property
                if (this.workflowInstance.BufferedReceiveManager != null)
                {
                    if (!ReceiveContext.TryGet(this.OperationContext.IncomingMessageProperties, out this.receiveContext))
                    {
                        Fx.Assert("ReceiveContext expected when BufferedReceives are enabled");
                    }

                    this.OperationContext.IncomingMessageProperties.Remove(ReceiveContext.Name);
                }

                completeSelf = ProcessRequest();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                // Failing synchronously is one case where AsyncResult won't handle calling OnCompleting
                OnCompleting(this, e);
                throw;
            }

            if (completeSelf)
            {
                base.Complete(true);
            }
        }

        public object[] Inputs
        {
            get
            {
                return this.inputs;
            }
        }

        public OperationContext OperationContext
        {
            get;
            private set;
        }

        public ServiceEndpoint ServiceEndpoint
        {
            get;
            private set;
        }

        public Transaction CurrentTransaction
        {
            get;
            private set;
        }

        public object BookmarkValue
        {
            get 
            { 
                return this.bookmarkValue; 
            }
        }

        public bool HasResponse
        {
            get
            {
                lock (this.thisLock)
                {
                    return this.CurrentState == State.Completed || this.CurrentState == State.ResultReceived;
                }
            }
        }

        //Completion state of this AsyncResult guarded by propertyLock
        State CurrentState
        {
            get;
            set;
        }

        public Guid E2EActivityId
        {
            get
            {
                return this.e2eActivityId;
            }
        }

        public static IAsyncResult BeginProcessRequest(WorkflowServiceInstance workflowInstance, OperationContext operationContext, string operationName,
            object[] inputs, bool performanceCountersEnabled, bool propagateActivity, Transaction currentTransaction, IInvokeReceivedNotification notification,
            WorkflowOperationBehavior behavior, ServiceEndpoint endpoint, TimeSpan timeout, AsyncCallback callback, object state)
        {
            Fx.Assert(inputs != null, "Null inputs");
            return new WorkflowOperationContext(inputs, operationContext, operationName, performanceCountersEnabled,
                propagateActivity, currentTransaction, workflowInstance, notification, behavior, endpoint, timeout, callback, state);
        }

        public static object EndProcessRequest(IAsyncResult result, out object[] outputs)
        {
            WorkflowOperationContext thisPtr = AsyncResult.End<WorkflowOperationContext>(result);
            outputs = thisPtr.outputs;
            return thisPtr.operationReturnValue;
        }

        public void SendFault(Exception exception)
        {
            Fx.Assert(exception != null, "Null Exception");

            this.pendingException = exception;

            bool completeNow;

            lock (this.thisLock)
            {
                Fx.Assert(this.CurrentState != State.Completed && this.CurrentState != State.ResultReceived, "Cannot receive this call after completion/result");
                completeNow = ProcessReply();
            }

            if (completeNow)
            {
                this.Complete(false, exception);
            }
        }

        public void SendReply(Message returnValue)
        {
            bool completeNow;

            lock (this.thisLock)
            {
                Fx.Assert(this.CurrentState != State.Completed && this.CurrentState != State.ResultReceived, "Cannot receive this call after completion/result");
                this.outputs = WorkflowOperationContext.emptyObjectArray; // everything is in the Message return value for workflow
                this.operationReturnValue = returnValue;
                completeNow = ProcessReply();
            }

            if (completeNow)
            {
                base.Complete(false);
            }
        }

        public void SendReply(object returnValue, object[] outputs)
        {
            bool completeNow;

            lock (this.thisLock)
            {
                Fx.Assert(this.CurrentState != State.Completed && this.CurrentState != State.ResultReceived, "Cannot receive this call after completion/result");
                this.outputs = outputs ?? WorkflowOperationContext.emptyObjectArray;
                this.operationReturnValue = returnValue;
                completeNow = ProcessReply();
            }

            if (completeNow)
            {
                base.Complete(false);
            }
        }

        //No-op for two-ways.
        public void SetOperationCompleted()
        {
            bool completeNow;

            lock (this.thisLock)
            {
                completeNow = ProcessReply();
            }

            if (completeNow)
            {
                base.Complete(false);
            }
        }

        bool ProcessReply()
        {
            bool completed = false;
            this.workflowInstance.ReleaseContext(this);
            this.RemovePendingOperation();

            if (this.CurrentState == State.BookmarkResumption) //We are still in Bookmark Resume
            {
                this.CurrentState = State.ResultReceived; //HandleEndResumeBookmark will take care of Completing AsyncResult.
            }
            else if (this.CurrentState == State.WaitForResult) //We already finished the bookmarkOperation; Now have to signal the AsynResult.
            {
                this.CurrentState = State.Completed;
                completed = true;
            }

            // we are not really completed until the ReceiveContext finishes its work
            if (completed)
            {
                if (this.pendingException == null)
                {
                    completed = ProcessReceiveContext();
                }
                else
                {
                    // if there's a pendingException, we let the RC abandon async so there's no need
                    // to affect the completed status
                    BufferedReceiveManager.AbandonReceiveContext(this.receiveContext);
                }
            }

            return completed;
        }

        void ProcessInitializationTraces()
        {
            //Let asp.net know that it needs to wait
            IncrementBusyCount();

            try
            {
                if (TraceUtility.MessageFlowTracingOnly)
                {
                    //ensure that Activity ID is set
                    DiagnosticTraceBase.ActivityId = this.E2EActivityId;
                    this.propagateActivity = false;
                }
                if (TraceUtility.ActivityTracing || (!TraceUtility.MessageFlowTracing && this.propagateActivity))
                {
                    this.e2eActivityId = TraceUtility.GetReceivedActivityId(this.OperationContext);

                    if ((this.E2EActivityId != Guid.Empty) && (this.E2EActivityId != InternalReceiveMessage.TraceCorrelationActivityId))
                    {
                        this.propagateActivity = true;
                        this.OperationContext.IncomingMessageProperties[MessagingActivityHelper.E2EActivityId] = this.E2EActivityId;
                        this.ambientActivityId = InternalReceiveMessage.TraceCorrelationActivityId;
                        FxTrace.Trace.SetAndTraceTransfer(this.E2EActivityId, true);
                        if (TD.StartSignpostEventIsEnabled())
                        {
                            TD.StartSignpostEvent(new DictionaryTraceRecord(new Dictionary<string, string>(2) {
                                                    { MessagingActivityHelper.ActivityName, MessagingActivityHelper.ActivityNameWorkflowOperationInvoke },
                                                    { MessagingActivityHelper.ActivityType, MessagingActivityHelper.ActivityTypeExecuteUserCode }
                            }));
                        }
                    }
                    else
                    {
                        this.propagateActivity = false;
                    }
                }
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw;
                }
                FxTrace.Exception.AsInformation(ex);
            }
        }


        void DecrementBusyCount()
        {
            lock (this.thisLock)
            {
                if (!this.hasDecrementedBusyCount)
                {
                    AspNetEnvironment.Current.DecrementBusyCount();
                    if (AspNetEnvironment.Current.TraceDecrementBusyCountIsEnabled())
                    {
                        AspNetEnvironment.Current.TraceDecrementBusyCount(SR.BusyCountTraceFormatString(this.workflowInstance.Id));
                    }
                    this.hasDecrementedBusyCount = true;
                }
            }
        }

        void IncrementBusyCount()
        {
            AspNetEnvironment.Current.IncrementBusyCount();
            if (AspNetEnvironment.Current.TraceIncrementBusyCountIsEnabled())
            {
                AspNetEnvironment.Current.TraceIncrementBusyCount(SR.BusyCountTraceFormatString(this.workflowInstance.Id));
            }
        }

        void EmitTransferFromInstanceId()
        {
            if (TraceUtility.MessageFlowTracing)
            {
                //set the WF instance ID as the Activity ID
                if (DiagnosticTraceBase.ActivityId != this.workflowInstance.Id)
                {
                    DiagnosticTraceBase.ActivityId = this.workflowInstance.Id;
                }
                FxTrace.Trace.SetAndTraceTransfer(this.E2EActivityId, true);
            }
        }

        void ProcessFinalizationTraces()
        {
            try
            {
                if (this.propagateActivity)
                {
                    Guid oldId = InternalReceiveMessage.TraceCorrelationActivityId;
                    if (TD.StopSignpostEventIsEnabled())
                    {
                        TD.StopSignpostEvent(new DictionaryTraceRecord(new Dictionary<string, string>(2) {
                                                    { MessagingActivityHelper.ActivityName, MessagingActivityHelper.ActivityNameWorkflowOperationInvoke },
                                                    { MessagingActivityHelper.ActivityType, MessagingActivityHelper.ActivityTypeExecuteUserCode }
                        }));
                    }
                    FxTrace.Trace.SetAndTraceTransfer(this.ambientActivityId, true);
                    this.ambientActivityId = Guid.Empty;
                }
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw;
                }
                FxTrace.Exception.AsInformation(ex);
            }
            DecrementBusyCount();
        }

        //Perf counter helpers.
        [Fx.Tag.SecurityNote(Critical = "Critical because it accesses UnsafeNativeMethods.QueryPerformanceCounter.",
            Safe = "Safe because we only make the call if the PartialTrustHelper.AppDomainFullyTrusted is true.")]
        [SecuritySafeCritical]
        void TrackMethodCalled()
        {
            if (PartialTrustHelpers.AppDomainFullyTrusted && this.performanceCountersEnabled)
            {
                using (new OperationContextScopeHelper(this.OperationContext))
                {
                    PerformanceCounters.MethodCalled(this.operationName);
                }

                if (System.Runtime.Interop.UnsafeNativeMethods.QueryPerformanceCounter(out this.beginTime) == 0)
                {
                    this.beginTime = -1;
                }
            }

            if (TD2.OperationCompletedIsEnabled() ||
                    TD2.OperationFaultedIsEnabled() ||
                    TD2.OperationFailedIsEnabled())
            {
                this.beginOperation = DateTime.UtcNow.Ticks;
            }

            if (TD2.OperationInvokedIsEnabled())
            {
                using (new OperationContextScopeHelper(this.OperationContext))
                {
                    TD2.OperationInvoked(this.eventTraceActivity, this.operationName, TraceUtility.GetCallerInfo(this.OperationContext));
                }
            }
        }

        void TrackMethodFaulted()
        {
            if (this.performanceCountersEnabled)
            {
                long duration = this.GetDuration();
                using (new OperationContextScopeHelper(this.OperationContext))
                {
                    PerformanceCounters.MethodReturnedFault(this.operationName, duration);
                }
            }
            if (TD2.OperationFaultedIsEnabled())
            {
                using (new OperationContextScopeHelper(this.OperationContext))
                {
                    TD2.OperationFaulted(this.eventTraceActivity, this.operationName,
                        TraceUtility.GetUtcBasedDurationForTrace(this.beginOperation));
                }
            }
        }

        void TrackMethodFailed()
        {
            if (this.performanceCountersEnabled)
            {
                long duration = this.GetDuration();
                using (new OperationContextScopeHelper(this.OperationContext))
                {
                    PerformanceCounters.MethodReturnedError(this.operationName, duration);
                }
            }
            if (TD2.OperationFailedIsEnabled())
            {
                using (new OperationContextScopeHelper(this.OperationContext))
                {
                    TD2.OperationFailed(this.eventTraceActivity, this.operationName, 
                        TraceUtility.GetUtcBasedDurationForTrace(this.beginOperation));
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Critical because it accesses UnsafeNativeMethods.QueryPerformanceCounter.",
            Safe = "Safe because we only make the call if the PartialTrustHelper.AppDomainFullyTrusted is true.")]
        [SecuritySafeCritical]
        long GetDuration()
        {
            long currentTime = 0;
            long duration = 0;

            if (PartialTrustHelpers.AppDomainFullyTrusted && this.performanceCountersEnabled && (this.beginTime >= 0) &&
                (System.Runtime.Interop.UnsafeNativeMethods.QueryPerformanceCounter(out currentTime) != 0))
            {
                duration = currentTime - this.beginTime;
            }

            return duration;
        }

        void TrackMethodSucceeded()
        {
            if (this.performanceCountersEnabled)
            {
                long duration = this.GetDuration();
                using (new OperationContextScopeHelper(this.OperationContext))
                {
                    PerformanceCounters.MethodReturnedSuccess(this.operationName, duration);
                }
            }
            if (TD2.OperationCompletedIsEnabled())
            {
                using (new OperationContextScopeHelper(this.OperationContext))
                {
                    TD2.OperationCompleted(this.eventTraceActivity, this.operationName, 
                        TraceUtility.GetUtcBasedDurationForTrace(this.beginOperation));
                }
            }
        }

        void TrackMethodCompleted(object returnValue)
        {
            // WorkflowOperationInvoker always deals at the Message/Message level
            Message faultMessage = returnValue as Message;
            if (faultMessage != null && faultMessage.IsFault)
            {
                TrackMethodFaulted();
            }
            else
            {
                TrackMethodSucceeded();
            }
        }

        bool ProcessRequest()
        {
            this.TrackMethodCalled();
            this.ProcessInitializationTraces();

            if (this.notification == null)
            {
                return OnResumeBookmark();
            }

            string sessionId = this.OperationContext.SessionId;
            if (sessionId == null)
            {
                return OnResumeBookmark();
            }

            // if there is a session, queue up this request in the per session pending request queue before notifying
            // the dispatcher to start the next invoke           
            IAsyncResult pendingAsyncResult = this.workflowInstance.BeginWaitForPendingOperations(sessionId, this.timeoutHelper.RemainingTime(), this.PrepareAsyncCompletion(handleEndWaitForPendingOperations), this);
            bool completed;

            this.notification.NotifyInvokeReceived();
            if (pendingAsyncResult.CompletedSynchronously)
            {
                completed = HandleEndWaitForPendingOperations(pendingAsyncResult);
            }
            else
            {
                completed = false;
            }
            
            return completed;
        }

        void RemovePendingOperation()
        {
            if (this.pendingAsyncResult != null)
            {
                this.workflowInstance.RemovePendingOperation(this.OperationContext.SessionId, this.pendingAsyncResult);
                this.pendingAsyncResult = null;
            }
        }

        static bool HandleEndWaitForPendingOperations(IAsyncResult result)
        {
            WorkflowOperationContext thisPtr = (WorkflowOperationContext)result.AsyncState;
            thisPtr.pendingAsyncResult = result;

            bool success = false;
            try
            {
                thisPtr.workflowInstance.EndWaitForPendingOperations(result);
                bool retval = thisPtr.OnResumeBookmark();
                success = true;
                return retval;
            }
            finally
            {
                if (!success)
                {
                    thisPtr.RemovePendingOperation();
                }
            }
        }

        bool OnResumeBookmark()
        {
            bool success = false;
            try
            {
                IAsyncResult nextResult = this.workflowInstance.BeginResumeProtocolBookmark(
                    this.bookmark, this.bookmarkScope, this,
                    this.timeoutHelper.RemainingTime(), this.PrepareAsyncCompletion(handleEndResumeBookmark), this);

                bool completed;
                if (nextResult.CompletedSynchronously)
                {
                    completed = HandleEndResumeBookmark(nextResult);
                }
                else
                {
                    completed = false;
                }

                success = true;
                return completed;
            }
            finally
            {
                if (!success)
                {
                    this.RemovePendingOperation();
                }
            }
        }

        static bool HandleEndResumeBookmark(IAsyncResult result)
        {
            WorkflowOperationContext thisPtr = (WorkflowOperationContext)result.AsyncState;

            bool completed = false;
            bool shouldAbandon = true;
            try
            {
                BookmarkResumptionResult resumptionResult = thisPtr.workflowInstance.EndResumeProtocolBookmark(result);
                if (resumptionResult != BookmarkResumptionResult.Success)
                {
                    // Raise UnkownMessageReceivedEvent when we fail to resume bookmark
                    thisPtr.OperationContext.Host.RaiseUnknownMessageReceived(thisPtr.OperationContext.IncomingMessage);

                    // Only delay-retry this operation once (and only if retries are supported). Future calls will ensure the bookmark is set.
                    if (thisPtr.workflowInstance.BufferedReceiveManager != null)
                    {
                        bool bufferSuccess = thisPtr.workflowInstance.BufferedReceiveManager.BufferReceive(
                            thisPtr.OperationContext, thisPtr.receiveContext, thisPtr.bookmark.Name, BufferedReceiveState.WaitingOnBookmark, false);
                        if (bufferSuccess)
                        {
                            if (TD.BufferOutOfOrderMessageNoBookmarkIsEnabled())
                            {
                                TD.BufferOutOfOrderMessageNoBookmark(thisPtr.eventTraceActivity, thisPtr.workflowInstance.Id.ToString(), thisPtr.bookmark.Name);
                            }

                            shouldAbandon = false;
                        }
                    }

                    // The throw exception is intentional whether or not BufferedReceiveManager is set.   
                    // This is to allow exception to bubble up the stack to WCF to cleanup various state (like Transaction).   
                    // This is queue scenario and as far as the client is concerned, the client will not see any exception.
                    throw FxTrace.Exception.AsError(new FaultException(OperationExecutionFault.CreateOperationNotAvailableFault(thisPtr.workflowInstance.Id, thisPtr.bookmark.Name)));
                }

                lock (thisPtr.thisLock)
                {
                    if (thisPtr.CurrentState == State.ResultReceived)
                    {
                        thisPtr.CurrentState = State.Completed;
                        if (thisPtr.pendingException != null)
                        {
                            throw FxTrace.Exception.AsError(thisPtr.pendingException);
                        }
                        completed = true;
                    }
                    else
                    {
                        thisPtr.CurrentState = State.WaitForResult;
                        completed = false;
                    }

                    // we are not really completed until the ReceiveContext finishes its work
                    if (completed)
                    {
                        completed = thisPtr.ProcessReceiveContext();
                    }

                    shouldAbandon = false;
                }
            }
            finally
            {
                if (shouldAbandon)
                {
                    BufferedReceiveManager.AbandonReceiveContext(thisPtr.receiveContext);
                }
                thisPtr.RemovePendingOperation();
            }

            return completed;
        }

        bool ProcessReceiveContext()
        {
            if (this.receiveContext != null)
            {
                if (handleEndProcessReceiveContext == null)
                {
                    handleEndProcessReceiveContext = new AsyncCompletion(HandleEndProcessReceiveContext);
                }

                IAsyncResult nextResult = ReceiveContextAsyncResult.BeginProcessReceiveContext(this, this.receiveContext, PrepareAsyncCompletion(handleEndProcessReceiveContext), this);
                return SyncContinue(nextResult);
            }

            return true;
        }

        static bool HandleEndProcessReceiveContext(IAsyncResult result)
        {
            ReceiveContextAsyncResult.EndProcessReceiveContext(result);
            return true;
        }

        static void Finally(AsyncResult result, Exception completionException)
        {
            WorkflowOperationContext thisPtr = (WorkflowOperationContext)result;
            thisPtr.EmitTransferFromInstanceId();

            if (completionException != null)
            {
                if (completionException is FaultException)
                {
                    thisPtr.TrackMethodFaulted();
                }
                else
                {
                    thisPtr.TrackMethodFailed();
                }
            }
            else
            {
                thisPtr.TrackMethodCompleted(thisPtr.operationReturnValue);
            }
            thisPtr.ProcessFinalizationTraces();
            // will be a no-op if we were never added to the instance
            thisPtr.workflowInstance.ReleaseContext(thisPtr);
            thisPtr.RemovePendingOperation();
        }

        enum State
        {
            BookmarkResumption,
            WaitForResult,
            ResultReceived,
            Completed
        }

        class OperationContextScopeHelper : IDisposable
        {
            OperationContext currentOperationContext;

            public OperationContextScopeHelper(OperationContext operationContext)
            {
                this.currentOperationContext = OperationContext.Current;
                OperationContext.Current = operationContext;
            }

            void IDisposable.Dispose()
            {
                OperationContext.Current = this.currentOperationContext;
            }
        }

        class ReceiveContextAsyncResult : TransactedAsyncResult
        {
            static AsyncCompletion handleEndComplete = new AsyncCompletion(HandleEndComplete);

            WorkflowOperationContext context;
            ReceiveContext receiveContext;

            ReceiveContextAsyncResult(WorkflowOperationContext context, ReceiveContext receiveContext, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.context = context;
                this.receiveContext = receiveContext;

                if (ProcessReceiveContext())
                {
                    base.Complete(true);
                }
            }

            public static IAsyncResult BeginProcessReceiveContext(WorkflowOperationContext context, ReceiveContext receiveContext, AsyncCallback callback, object state)
            {
                return new ReceiveContextAsyncResult(context, receiveContext, callback, state);
            }

            public static void EndProcessReceiveContext(IAsyncResult result)
            {
                ReceiveContextAsyncResult.End(result);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ReceiveContextAsyncResult>(result);
            }

            bool ProcessReceiveContext()
            {
                IAsyncResult result;

                using (PrepareTransactionalCall(this.context.CurrentTransaction))
                {
                    if (this.context.CurrentTransaction != null)
                    {
                        // make sure we Abandon if the transaction ends up with an outcome of Aborted
                        this.context.CurrentTransaction.TransactionCompleted += new TransactionCompletedEventHandler(OnTransactionComplete);
                    }
                    result = this.receiveContext.BeginComplete(
                        this.context.timeoutHelper.RemainingTime(), PrepareAsyncCompletion(handleEndComplete), this);
                }

                return SyncContinue(result);
            }

            // This happens out-of-band of ReceiveAsyncResult.
            // When transaction was aborted, we best-effort abandon the context.
            void OnTransactionComplete(object sender, TransactionEventArgs e)
            {
                if (e.Transaction.TransactionInformation.Status != TransactionStatus.Committed)
                {
                    BufferedReceiveManager.AbandonReceiveContext(this.context.receiveContext);
                }
            }

            static bool HandleEndComplete(IAsyncResult result)
            {
                ReceiveContextAsyncResult thisPtr = (ReceiveContextAsyncResult)result.AsyncState;
                thisPtr.receiveContext.EndComplete(result);
                return true;
            }
        }
    }
}
