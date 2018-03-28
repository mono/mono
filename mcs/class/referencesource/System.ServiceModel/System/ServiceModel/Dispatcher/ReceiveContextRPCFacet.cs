//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.Transactions;

    class ReceiveContextRPCFacet
    {
        static AsyncCallback handleEndComplete = Fx.ThunkCallback(new AsyncCallback(HandleEndComplete));
        ReceiveContext receiveContext;

        ReceiveContextRPCFacet(ReceiveContext receiveContext)
        {
            this.receiveContext = receiveContext;
        }

        //Called from ProcessMessage1
        //ManualAcknowledgementMode : No-Op.
        //Non-transacted V1 Operation : Remove RC; RC.Complete;(Will pause RPC if truly async)
        //Else : Create and Attach RCFacet to MessageRPC.
        public static void CreateIfRequired(ImmutableDispatchRuntime dispatchRuntime, ref MessageRpc messageRpc)
        {
            if (messageRpc.Operation.ReceiveContextAcknowledgementMode == ReceiveContextAcknowledgementMode.ManualAcknowledgement)
            {
                //Manual mode, user owns the acknowledgement.
                return;
            }

            //Retrieve RC from request and ensure it is removed from Message.
            ReceiveContext receiveContext = null;
            if (!ReceiveContext.TryGet(messageRpc.Request, out receiveContext))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(
                    SR.GetString(SR.SFxReceiveContextPropertyMissing,
                    typeof(ReceiveContext).Name)));
            }
            messageRpc.Request.Properties.Remove(ReceiveContext.Name);

            if (messageRpc.Operation.ReceiveContextAcknowledgementMode == ReceiveContextAcknowledgementMode.AutoAcknowledgeOnReceive)
            {
                if (!messageRpc.Operation.TransactionRequired)
                {
                    //Attempt to complete the ReceiveContext.
                    //Async Result Ensures RPC is paused if it goes ASYNC.
                    IAsyncResult result = new AcknowledgementCompleteAsyncResult(
                        receiveContext,
                        TimeSpan.MaxValue,
                        ref messageRpc,
                        null,
                        handleEndComplete,
                        new AcknowledgementCompleteCallbackState
                        {
                            DispatchRuntime = dispatchRuntime,
                            Rpc = messageRpc
                        });

                    if (result.CompletedSynchronously)
                    {
                        AcknowledgementCompleteAsyncResult.End(result);
                    }
                    return;
                }
            }
            //We have to create a Facet for acknowledgement at later stage.
            messageRpc.ReceiveContext = new ReceiveContextRPCFacet(receiveContext);
        }

        //Called from ProcessMessage31.
        //Mode is TransactedOperation && !ManualAcknowledgement
        //Will pause RPC if Complete is truly Async.
        public void Complete(ImmutableDispatchRuntime dispatchRuntime, ref MessageRpc rpc, TimeSpan timeout, Transaction transaction)
        {
            Fx.Assert(transaction != null, "Cannot reach here with null transaction");
            //Async Result Ensures the RPC is paused if the request goes Async.
            IAsyncResult result = new AcknowledgementCompleteAsyncResult(
                this.receiveContext,
                timeout,
                ref rpc,
                transaction,
                handleEndComplete,
                new AcknowledgementCompleteCallbackState
                {
                    DispatchRuntime = dispatchRuntime,
                    Rpc = rpc
                });

            if (result.CompletedSynchronously)
            {
                AcknowledgementCompleteAsyncResult.End(result);
            }
        }

        //Called from RPC.DisposeRequestContext for sucessful invoke.
        //Mode is RCBA.ManualAcknowledgement = false.
        public IAsyncResult BeginComplete(TimeSpan timeout, Transaction transaction, ChannelHandler channelHandler, AsyncCallback callback, object state)
        {
            IAsyncResult result = null;
            if (transaction != null)
            {
                using (TransactionScope scope = new TransactionScope(transaction))
                {
                    TransactionOutcomeListener.EnsureReceiveContextAbandonOnTransactionRollback(this.receiveContext, transaction, channelHandler);
                    result = this.receiveContext.BeginComplete(
                        timeout,
                        callback,
                        state);
                    scope.Complete();
                }
            }
            else
            {
                result = this.receiveContext.BeginComplete(
                    timeout,
                    callback,
                    state);
            }
            return result;
        }
        public void EndComplete(IAsyncResult result)
        {
            this.receiveContext.EndComplete(result);
        }

        //Called from RPC.AbortRequestContext for failed invoke.
        //Mode is RCBA.ManualAcknowledgement = false.
        public IAsyncResult BeginAbandon(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.receiveContext.BeginAbandon(
                timeout,
                callback,
                state);
        }
        public void EndAbandon(IAsyncResult result)
        {
            this.receiveContext.EndAbandon(result);
        }

        //Callback handler for ReceiveContext.BeginComplete made from ProcessMessage*
        static void HandleEndComplete(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            try
            {
                AcknowledgementCompleteAsyncResult.End(result);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                AcknowledgementCompleteCallbackState callbackState = (AcknowledgementCompleteCallbackState)result.AsyncState;
                MessageRpc rpc = callbackState.Rpc;
                rpc.Error = e;
                callbackState.DispatchRuntime.ErrorBehavior.HandleError(ref rpc);
                return;
            }
        }

        class AcknowledgementCompleteCallbackState
        {
            public ImmutableDispatchRuntime DispatchRuntime
            {
                get;
                set;
            }

            public MessageRpc Rpc
            {
                get;
                set;
            }
        }
        class AcknowledgementCompleteAsyncResult : AsyncResult
        {
            static AsyncCallback completeCallback = Fx.ThunkCallback(new AsyncCallback(CompleteCallback));
            IResumeMessageRpc resumableRPC;
            ReceiveContext receiveContext;
            Transaction currentTransaction;
            ChannelHandler channelHandler;

            public AcknowledgementCompleteAsyncResult(
                ReceiveContext receiveContext,
                TimeSpan timeout,
                ref MessageRpc rpc,
                Transaction transaction,
                AsyncCallback callback,
                object state) : base(callback, state)
            {
                this.receiveContext = receiveContext;
                this.currentTransaction = transaction;
                this.channelHandler = rpc.channelHandler;
                this.resumableRPC = rpc.Pause();

                bool completeThrew = true;
                try
                {
                    bool completed = this.Complete(timeout);
                    completeThrew = false;

                    if (completed)
                    {
                        this.resumableRPC = null;
                        rpc.UnPause();
                        this.Complete(true);
                    }
                }
                finally
                {
                    if (completeThrew)
                    {
                        rpc.UnPause();
                    }
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<AcknowledgementCompleteAsyncResult>(result);
            }

            bool Complete(TimeSpan timeout)
            {
                IAsyncResult result = null;

                if (this.currentTransaction != null)
                {
                    using (TransactionScope scope = new TransactionScope(this.currentTransaction))
                    {
                        TransactionOutcomeListener.EnsureReceiveContextAbandonOnTransactionRollback(this.receiveContext, this.currentTransaction, this.channelHandler);
                        result = this.receiveContext.BeginComplete(
                            timeout,
                            completeCallback,
                            this);
                        scope.Complete();
                    }
                }
                else
                {
                    result = this.receiveContext.BeginComplete(
                        timeout,
                        completeCallback,
                        this);
                }

                if (result.CompletedSynchronously)
                {
                    return HandleComplete(result);
                }
                return false;
            }

            static bool HandleComplete(IAsyncResult result)
            {
                AcknowledgementCompleteAsyncResult thisPtr = (AcknowledgementCompleteAsyncResult)result.AsyncState;
                thisPtr.receiveContext.EndComplete(result);
                return true;
            }

            static void CompleteCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                //Async Completion Path.
                Exception completionException = null;
                bool completeSelf = true;

                try
                {
                    completeSelf = HandleComplete(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    completionException = e;
                }

                if (completeSelf)
                {
                    AcknowledgementCompleteAsyncResult thisPtr = (AcknowledgementCompleteAsyncResult)result.AsyncState;
                    thisPtr.resumableRPC.Resume();
                    thisPtr.Complete(false, completionException);
                }
            }
        }
        class TransactionOutcomeListener
        {
            static AsyncCallback abandonCallback = Fx.ThunkCallback(new AsyncCallback(AbandonCallback));
            ReceiveContext receiveContext;
            ChannelHandler channelHandler;

            public TransactionOutcomeListener(ReceiveContext receiveContext, Transaction transaction, ChannelHandler handler)
            {
                this.receiveContext = receiveContext;
                transaction.TransactionCompleted += new TransactionCompletedEventHandler(this.OnTransactionComplete);
                this.channelHandler = handler;
            }

            public static void EnsureReceiveContextAbandonOnTransactionRollback(ReceiveContext receiveContext, Transaction transaction, ChannelHandler channelHandler)
            {
                new TransactionOutcomeListener(receiveContext, transaction, channelHandler);
            }

            void OnTransactionComplete(object sender, TransactionEventArgs e)
            {
                if (e.Transaction.TransactionInformation.Status == TransactionStatus.Aborted)
                {
                    try
                    {
                        IAsyncResult result = this.receiveContext.BeginAbandon(
                            TimeSpan.MaxValue,
                            abandonCallback,
                            new CallbackState
                            {
                                ChannelHandler = this.channelHandler,
                                ReceiveContext = this.receiveContext
                            });

                        if (result.CompletedSynchronously)
                        {
                            this.receiveContext.EndAbandon(result);
                        }
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        this.channelHandler.HandleError(exception);
                    }
                }
            }

            static void AbandonCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                CallbackState callbackState = (CallbackState)result.AsyncState;

                try
                {
                    callbackState.ReceiveContext.EndAbandon(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    callbackState.ChannelHandler.HandleError(e);
                }
            }

            class CallbackState
            {
                public ChannelHandler ChannelHandler
                {
                    get;
                    set;
                }

                public ReceiveContext ReceiveContext
                {
                    get;
                    set;
                }
            }
        }
    }
}
