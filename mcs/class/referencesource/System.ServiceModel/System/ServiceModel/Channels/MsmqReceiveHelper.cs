//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.ServiceModel.Diagnostics;
    using System.Threading;
    using System.Transactions;
    using System.ComponentModel;
    using System.Runtime.Versioning;

    // PostRollbackErrorStrategy
    interface IPostRollbackErrorStrategy
    {
        bool AnotherTryNeeded();
    }

    // SimplePostRollbackErrorStrategy
    class SimplePostRollbackErrorStrategy : IPostRollbackErrorStrategy
    {
        const int Attempts = 50;
        const int MillisecondsToSleep = 100;

        int attemptsLeft = Attempts;
        long lookupId;

        internal SimplePostRollbackErrorStrategy(long lookupId)
        {
            this.lookupId = lookupId;
        }

        public bool AnotherTryNeeded()
        {
            if (--this.attemptsLeft > 0)
            {
                if (attemptsLeft == (Attempts - 1))
                    MsmqDiagnostics.MessageLockedUnderTheTransaction(lookupId);
                Thread.Sleep(TimeSpan.FromMilliseconds(MillisecondsToSleep));
                return true;
            }
            else
            {
                MsmqDiagnostics.MoveOrDeleteAttemptFailed(lookupId);
                return false;
            }
        }
    }

    sealed class MsmqReceiveHelper
    {
        IPoisonHandlingStrategy poisonHandler;
        string queueName;
        MsmqQueue queue;
        MsmqReceiveParameters receiveParameters;
        Uri uri;
        string instanceId;
        IMsmqMessagePool pool;
        MsmqInputChannelBase channel;
        MsmqChannelListenerBase listener;
        ServiceModelActivity activity;
        string msmqRuntimeNativeLibrary;

        internal MsmqReceiveHelper(MsmqReceiveParameters receiveParameters, Uri uri, IMsmqMessagePool messagePool, MsmqInputChannelBase channel, MsmqChannelListenerBase listener)
        {
            this.queueName = receiveParameters.AddressTranslator.UriToFormatName(uri);
            this.receiveParameters = receiveParameters;
            this.uri = uri;
            this.instanceId = uri.ToString().ToUpperInvariant();
            this.pool = messagePool;
            this.poisonHandler = Msmq.CreatePoisonHandler(this);
            this.channel = channel;
            this.listener = listener;
            this.queue = Msmq.CreateMsmqQueue(this);
        }

        internal ServiceModelActivity Activity
        {
            get { return this.activity; }
        }

        IPoisonHandlingStrategy PoisonHandler
        {
            get { return this.poisonHandler; }
        }

        internal MsmqReceiveParameters MsmqReceiveParameters
        {
            get { return this.receiveParameters; }
        }

        internal MsmqInputChannelBase Channel
        {
            get { return this.channel; }
        }

        internal MsmqChannelListenerBase ChannelListener
        {
            get { return this.listener; }
        }

        internal Uri ListenUri
        {
            get { return this.uri; }
        }

        internal string InstanceId
        {
            get { return this.instanceId; }
        }

        internal MsmqQueue Queue
        {
            get { return this.queue; }
        }

        internal bool Transactional
        {
            get { return this.receiveParameters.ExactlyOnce; }
        }

        internal string MsmqRuntimeNativeLibrary
        {
            get
            {
                if (this.msmqRuntimeNativeLibrary == null)
                {
                    this.msmqRuntimeNativeLibrary = Environment.SystemDirectory + "\\" + UnsafeNativeMethods.MQRT;
                }
                return this.msmqRuntimeNativeLibrary;
            }
        }

        internal void Open()
        {
            this.activity = MsmqDiagnostics.StartListenAtActivity(this);
            using (MsmqDiagnostics.BoundOpenOperation(this))
            {
                this.queue.EnsureOpen();
                this.poisonHandler.Open();
            }
        }

        internal void Close()
        {
            using (ServiceModelActivity.BoundOperation(this.Activity))
            {
                this.poisonHandler.Dispose();
                this.queue.Dispose();
            }
            ServiceModelActivity.Stop(this.activity);
        }

        internal MsmqInputMessage TakeMessage()
        {
            return this.pool.TakeMessage();
        }

        internal void ReturnMessage(MsmqInputMessage message)
        {
            this.pool.ReturnMessage(message);
        }

        internal static void TryAbortTransactionCurrent()
        {
            if (null != Transaction.Current)
            {
                try
                {
                    Transaction.Current.Rollback();
                }
                catch (TransactionAbortedException ex)
                {
                    MsmqDiagnostics.ExpectedException(ex);
                }
                catch (ObjectDisposedException ex)
                {
                    MsmqDiagnostics.ExpectedException(ex);
                }
            }
        }

        internal void DropOrRejectReceivedMessage(MsmqMessageProperty messageProperty, bool reject)
        {
            this.DropOrRejectReceivedMessage(this.Queue, messageProperty, reject);
        }

        internal void DropOrRejectReceivedMessage(MsmqQueue queue, MsmqMessageProperty messageProperty, bool reject)
        {
            if (this.Transactional)
            {
                TryAbortTransactionCurrent();
                IPostRollbackErrorStrategy postRollback = new SimplePostRollbackErrorStrategy(messageProperty.LookupId);
                MsmqQueue.MoveReceiveResult result = MsmqQueue.MoveReceiveResult.Unknown;
                do
                {
                    using (MsmqEmptyMessage emptyMessage = new MsmqEmptyMessage())
                    {
                        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                        {
                            result = queue.TryReceiveByLookupId(messageProperty.LookupId, emptyMessage, MsmqTransactionMode.CurrentOrThrow);
                            if (MsmqQueue.MoveReceiveResult.Succeeded == result && reject)
                                queue.MarkMessageRejected(messageProperty.LookupId);
                            scope.Complete();
                        }
                    }

                    if (result == MsmqQueue.MoveReceiveResult.Succeeded)
                        // If 'Reject' supported and 'Reject' requested, put reject in the trace, otherwise put 'Drop'
                        MsmqDiagnostics.MessageConsumed(instanceId, messageProperty.MessageId, (Msmq.IsRejectMessageSupported && reject));

                    if (result != MsmqQueue.MoveReceiveResult.MessageLockedUnderTransaction)
                        break;
                }
                while (postRollback.AnotherTryNeeded());
            }
            else
            {
                MsmqDiagnostics.MessageConsumed(instanceId, messageProperty.MessageId, false);
            }
        }

        //
        internal static void MoveReceivedMessage(MsmqQueue queueFrom, MsmqQueue queueTo, long lookupId)
        {
            TryAbortTransactionCurrent();

            IPostRollbackErrorStrategy postRollback = new SimplePostRollbackErrorStrategy(lookupId);
            MsmqQueue.MoveReceiveResult result = MsmqQueue.MoveReceiveResult.Unknown;
            do
            {
                result = queueFrom.TryMoveMessage(lookupId, queueTo, MsmqTransactionMode.Single);

                if (result != MsmqQueue.MoveReceiveResult.MessageLockedUnderTransaction)
                    break;
            }
            while (postRollback.AnotherTryNeeded());
        }

        internal void FinalDisposition(MsmqMessageProperty messageProperty)
        {
            this.poisonHandler.FinalDisposition(messageProperty);
        }

        // WaitForMessage
        internal bool WaitForMessage(TimeSpan timeout)
        {
            using (MsmqEmptyMessage message = new MsmqEmptyMessage())
            {
                return (MsmqQueue.ReceiveResult.Timeout != this.queue.TryPeek(message, timeout));
            }
        }
        //
        internal IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new WaitForMessageAsyncResult(this.queue, timeout, callback, state);
        }
        //
        public bool EndWaitForMessage(IAsyncResult result)
        {
            return WaitForMessageAsyncResult.End(result);
        }

        internal bool TryReceive(MsmqInputMessage msmqMessage, TimeSpan timeout, MsmqTransactionMode transactionMode, out MsmqMessageProperty property)
        {
            property = null;

            MsmqQueue.ReceiveResult receiveResult = this.Queue.TryReceive(msmqMessage, timeout, transactionMode);
            if (MsmqQueue.ReceiveResult.OperationCancelled == receiveResult)
                return true;
            if (MsmqQueue.ReceiveResult.Timeout == receiveResult)
                return false;
            else
            {
                property = new MsmqMessageProperty(msmqMessage);
                if (this.Transactional)
                {
                    if (this.PoisonHandler.CheckAndHandlePoisonMessage(property))
                    {
                        long lookupId = property.LookupId;
                        property = null;
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new MsmqPoisonMessageException(lookupId));
                    }
                }
                return true;
            }
        }
        //
        internal IAsyncResult BeginTryReceive(MsmqInputMessage msmqMessage, TimeSpan timeout, MsmqTransactionMode transactionMode, AsyncCallback callback, object state)
        {
            if (this.receiveParameters.ExactlyOnce || this.queue is ILockingQueue)
                return new TryTransactedReceiveAsyncResult(this, msmqMessage, timeout, transactionMode, callback, state);
            else
                return new TryNonTransactedReceiveAsyncResult(this, msmqMessage, timeout, callback, state);
        }
        //
        internal bool EndTryReceive(IAsyncResult result, out MsmqInputMessage msmqMessage, out MsmqMessageProperty msmqProperty)
        {
            msmqMessage = null;
            msmqProperty = null;

            if (null == result)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");

            if (this.receiveParameters.ExactlyOnce)
            {
                TryTransactedReceiveAsyncResult receiveResult = result as TryTransactedReceiveAsyncResult;
                if (null == receiveResult)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.InvalidAsyncResult));
                return TryTransactedReceiveAsyncResult.End(receiveResult, out msmqMessage, out msmqProperty);
            }
            else
            {
                TryNonTransactedReceiveAsyncResult receiveResult = result as TryNonTransactedReceiveAsyncResult;
                if (null == receiveResult)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.InvalidAsyncResult));
                return TryNonTransactedReceiveAsyncResult.End(receiveResult, out msmqMessage, out msmqProperty);
            }
        }

        // TryReceiveAsyncResult (tx version)
        class TryTransactedReceiveAsyncResult : AsyncResult
        {
            bool expired;
            MsmqReceiveHelper receiver;
            TimeoutHelper timeoutHelper;
            Transaction txCurrent;
            MsmqInputMessage msmqMessage;
            MsmqMessageProperty messageProperty;
            MsmqTransactionMode transactionMode;
            static Action<object> onComplete = new Action<object>(OnComplete);

            internal TryTransactedReceiveAsyncResult(MsmqReceiveHelper receiver, MsmqInputMessage msmqMessage,
                TimeSpan timeout, MsmqTransactionMode transactionMode, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.txCurrent = Transaction.Current;
                this.receiver = receiver;
                this.msmqMessage = msmqMessage;
                this.transactionMode = transactionMode;
                ActionItem.Schedule(onComplete, this);
            }

            static void OnComplete(object parameter)
            {
                TryTransactedReceiveAsyncResult result = parameter as TryTransactedReceiveAsyncResult;
                Transaction savedTransaction = Transaction.Current;
                Transaction.Current = result.txCurrent;
                try
                {
                    Exception ex = null;
                    try
                    {
                        result.expired = !result.receiver.TryReceive(result.msmqMessage, result.timeoutHelper.RemainingTime(), result.transactionMode, out result.messageProperty);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                            throw;
                        ex = e;
                    }
                    result.Complete(false, ex);
                }
                finally
                {
                    Transaction.Current = savedTransaction;
                }
            }

            internal static bool End(IAsyncResult result, out MsmqInputMessage msmqMessage, out MsmqMessageProperty property)
            {
                TryTransactedReceiveAsyncResult receiveResult = AsyncResult.End<TryTransactedReceiveAsyncResult>(result);
                msmqMessage = receiveResult.msmqMessage;
                property = receiveResult.messageProperty;
                return !receiveResult.expired;
            }
        }

        // TryReceiveAsyncResult (non-tx version)
        class TryNonTransactedReceiveAsyncResult : AsyncResult
        {
            MsmqQueue.ReceiveResult receiveResult;
            MsmqReceiveHelper receiver;
            MsmqInputMessage msmqMessage;
            static AsyncCallback onCompleteStatic = Fx.ThunkCallback(new AsyncCallback(OnCompleteStatic));


            internal TryNonTransactedReceiveAsyncResult(MsmqReceiveHelper receiver, MsmqInputMessage msmqMessage, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.receiver = receiver;
                this.msmqMessage = msmqMessage;
                receiver.Queue.BeginTryReceive(msmqMessage, timeout, onCompleteStatic, this);
            }

            static void OnCompleteStatic(IAsyncResult result)
            {
                (result.AsyncState as TryNonTransactedReceiveAsyncResult).OnComplete(result);
            }

            void OnComplete(IAsyncResult result)
            {
                Exception ex = null;
                try
                {
                    receiveResult = receiver.Queue.EndTryReceive(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;
                    ex = e;
                }
                Complete(result.CompletedSynchronously, ex);
            }

            internal static bool End(IAsyncResult result, out MsmqInputMessage msmqMessage, out MsmqMessageProperty property)
            {
                TryNonTransactedReceiveAsyncResult asyncResult = AsyncResult.End<TryNonTransactedReceiveAsyncResult>(result);
                msmqMessage = asyncResult.msmqMessage;
                property = null;
                if (MsmqQueue.ReceiveResult.Timeout == asyncResult.receiveResult)
                    return false;
                else if (MsmqQueue.ReceiveResult.OperationCancelled == asyncResult.receiveResult)
                    return true;
                else
                {
                    property = new MsmqMessageProperty(msmqMessage);
                    return true;
                }
            }
        }

        // WaitForMessageAsyncResult
        class WaitForMessageAsyncResult : AsyncResult
        {
            MsmqQueue msmqQueue;
            MsmqEmptyMessage msmqMessage;
            bool successResult;
            static AsyncCallback onCompleteStatic = Fx.ThunkCallback(new AsyncCallback(OnCompleteStatic));

            public WaitForMessageAsyncResult(MsmqQueue msmqQueue, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.msmqMessage = new MsmqEmptyMessage();
                this.msmqQueue = msmqQueue;
                this.msmqQueue.BeginPeek(this.msmqMessage, timeout, onCompleteStatic, this);
            }

            static void OnCompleteStatic(IAsyncResult result)
            {
                ((WaitForMessageAsyncResult)result.AsyncState).OnComplete(result);
            }

            void OnComplete(IAsyncResult result)
            {
                this.msmqMessage.Dispose();
                MsmqQueue.ReceiveResult receiveResult = MsmqQueue.ReceiveResult.Unknown;
                Exception completionException = null;
                try
                {
                    receiveResult = this.msmqQueue.EndPeek(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;
                    completionException = e;
                }

                this.successResult = receiveResult != MsmqQueue.ReceiveResult.Timeout;
                base.Complete(result.CompletedSynchronously, completionException);
            }

            public static bool End(IAsyncResult result)
            {
                WaitForMessageAsyncResult thisPtr = AsyncResult.End<WaitForMessageAsyncResult>(result);
                return thisPtr.successResult;
            }
        }
    }
}
