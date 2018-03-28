//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Routing
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.Transactions;

    class ProcessMessagesAsyncResult<TContract> : TransactedAsyncResult
    {
        static AsyncCompletion clientOperationCallback = ClientOperationCallback;
        static AsyncCompletion completeReceiveContextCallback = CompleteReceiveContextCallback;
        static AsyncCompletion commitTransactionCallback = CommitTransactionCallback;
        static AsyncCompletion channelCloseCallback = ChannelCloseCallback;
        
        bool abortedRetry;
        bool allCompletedSync = true;
        RoutingChannelExtension channelExtension;
        IRoutingClient client;
        bool closeOutboundChannels;
        int destinationIndex;
        RoutingService service;
        int sessionMessageIndex;
        ProcessingState state = ProcessingState.Initial;
        TimeoutHelper timeoutHelper;

        public ProcessMessagesAsyncResult(Message message, RoutingService service, TimeSpan timeout, AsyncCallback callback, object state)
            : base(callback, state)
        {
            this.service = service;
            this.channelExtension = service.ChannelExtension;
            this.timeoutHelper = new TimeoutHelper(timeout);
            this.timeoutHelper.RemainingTime(); //Start the timer

            if (message == null)
            {
                //Null message means end of session, time to close everything
                this.closeOutboundChannels = true;
                this.state = ProcessingState.ClosingChannels;
            }
            else
            {
                this.closeOutboundChannels = false;
                MessageRpc messageRpc = new MessageRpc(message, OperationContext.Current, this.channelExtension.ImpersonationRequired);
                if (TD.RoutingServiceProcessingMessageIsEnabled())
                {
                    TD.RoutingServiceProcessingMessage(messageRpc.EventTraceActivity, messageRpc.UniqueID, messageRpc.Message.Headers.Action, messageRpc.OperationContext.EndpointDispatcher.EndpointAddress.Uri.ToString(), (messageRpc.Transaction != null).ToString());
                }

                EndpointNameMessageFilter.Set(messageRpc.Message.Properties, this.channelExtension.EndpointName);
                messageRpc.RouteToEndpoints<TContract>(this.service.RoutingConfig);
                this.service.SessionMessages.Add(messageRpc);

                this.sessionMessageIndex = this.service.SessionMessages.Count - 1;
                if (this.sessionMessageIndex == 0)
                {
                    //First message, do initialization stuff
                    this.state = ProcessingState.Initial;
                }
                else
                {
                    this.state = ProcessingState.SendingSessionMessages;
                }
            }
            this.ProcessWhileSync();
        }

        void ProcessWhileSync()
        {
            try
            {
                bool callAgain;
                do
                {
                    callAgain = this.ProcessNext();
                } while (callAgain);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                this.Fault(e);
            }
        }

        void ChangeState(ProcessingState newState)
        {
            this.sessionMessageIndex = 0;
            this.destinationIndex = 0;
            this.state = newState;
        }

        void ResetState()
        {
            this.service.ResetSession();
            this.ChangeState(ProcessingState.Initial);
        }

        bool ProcessNext()
        {
            switch (this.state)
            {
                case ProcessingState.Initial:
                    {
                        this.service.CreateNewTransactionIfNeeded(this.service.SessionMessages[0]);
                        return this.DoneInitializing();
                    }

                case ProcessingState.SendingSessionMessages:
                    {
                        return this.SendToCurrentClient();
                    }

                case ProcessingState.ClosingChannels:
                    {
                        return this.CloseCurrentChannel();
                    }

                case ProcessingState.CompletingReceiveContexts:
                    {
                        return this.CompleteCurrentReceiveContext();
                    }

                case ProcessingState.CommittingTransaction:
                    {
                        return this.CommitTransaction();
                    }

                case ProcessingState.Completing:
                    {
                        this.CompleteSelf(null);
                        return false;
                    }

                default:
                    Fx.Assert("ProcessNext shouldn't be called in this state: " + this.state);
                    return false;
            }
        }

        bool SendToCurrentClient()
        {
            MessageRpc messageRpc = this.service.SessionMessages[this.sessionMessageIndex];
            SendOperation sendOperation = messageRpc.Operations[this.destinationIndex];
            if (sendOperation.Sent)
            {
                this.MoveToNextClientOperation(messageRpc.Operations.Count);
                return true;
            }
            else if (!this.channelExtension.ReceiveContextEnabled &&
                this.channelExtension.TransactedReceiveEnabled &&
                sendOperation.HasAlternate)
            {
                // We can't do error handling for oneway Transactional unless there's RC.
                throw FxTrace.Exception.AsError(new ConfigurationErrorsException(SR.ErrorHandlingNotSupportedTxNoRC(messageRpc.OperationContext.Channel.LocalAddress)));
            }

            RoutingEndpointTrait endpointTrait = sendOperation.CurrentEndpoint;
            this.client = this.service.GetOrCreateClient<TContract>(endpointTrait, messageRpc.Impersonating);
            try
            {
                // We always work on cloned message when there are backup endpoints to handle exception cases
                Message message;
                if (messageRpc.Operations.Count == 1 && sendOperation.AlternateEndpointCount == 0)
                {
                    message = messageRpc.Message;
                }
                else
                {
                    message = messageRpc.CreateBuffer().CreateMessage();
                }

                sendOperation.PrepareMessage(message);
                IAsyncResult result;

                if (TD.RoutingServiceTransmittingMessageIsEnabled())
                {
                    TD.RoutingServiceTransmittingMessage(messageRpc.EventTraceActivity, messageRpc.UniqueID, this.destinationIndex.ToString(TD.Culture), this.client.Key.ToString());
                }

                Transaction transaction = this.service.GetTransactionForSending(messageRpc);
                using (this.PrepareTransactionalCall(transaction))
                {
                    IDisposable impersonationContext = null;
                    try
                    {
                        //Perform the assignment in a finally block so it won't be interrupted asynchronously
                        try { } 
                        finally
                        {
                            impersonationContext = messageRpc.PrepareCall();
                        }
                        
                        result = this.client.BeginOperation(message, transaction, this.PrepareAsyncCompletion(clientOperationCallback), this);
                    }
                    finally
                    {
                        if (impersonationContext != null)
                        {
                            impersonationContext.Dispose();
                        }
                    }
                }

                if (this.CheckSyncContinue(result))
                {
                    this.ClientOperationComplete(result);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }

                //See if we can handle this Exception...
                if (this.HandleClientOperationFailure(exception))
                {
                    return true;
                }
                throw;
            }
        }

        static bool ClientOperationCallback(IAsyncResult result)
        {
            ProcessMessagesAsyncResult<TContract> thisPtr = (ProcessMessagesAsyncResult<TContract>)result.AsyncState;
            FxTrace.Trace.SetAndTraceTransfer(thisPtr.channelExtension.ActivityID, true);
            try
            {
                try
                {
                    thisPtr.allCompletedSync = false;
                    thisPtr.ClientOperationComplete(result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }

                    //See if we can handle this Exception...
                    if (!thisPtr.HandleClientOperationFailure(exception))
                    {
                        throw;
                    }
                }

                thisPtr.ProcessWhileSync();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                thisPtr.Fault(e);
            }

            return false;
        }

        void ClientOperationComplete(IAsyncResult result)
        {
            MessageRpc messageRpc = this.service.SessionMessages[this.sessionMessageIndex];
            SendOperation currentDest = messageRpc.Operations[this.destinationIndex];

            this.client.EndOperation(result);
            currentDest.TransmitSucceeded(this.service.GetTransactionForSending(messageRpc));

            if (TD.RoutingServiceTransmitSucceededIsEnabled())
            {
                TD.RoutingServiceTransmitSucceeded(messageRpc.EventTraceActivity, messageRpc.UniqueID, this.destinationIndex.ToString(TD.Culture), currentDest.CurrentEndpoint.ToString());
            }
            MoveToNextClientOperation(messageRpc.Operations.Count);
        }

        void MoveToNextClientOperation(int operationCount)
        {
            if (++this.destinationIndex >= operationCount)
            {
                //We've processed all multicasts for a given MessageRpc, move on to the next message (if any)
                this.destinationIndex = 0;

                // If we're one-way non-transactional and non-ReceiveContext then
                // we don't need to store messages for session replay or RC.Complete
                if (!this.channelExtension.ReceiveContextEnabled && !this.channelExtension.TransactedReceiveEnabled)
                {
                    this.service.SessionMessages.RemoveAt(this.sessionMessageIndex);
                    --this.sessionMessageIndex;
                }

                if (++this.sessionMessageIndex >= this.service.SessionMessages.Count)
                {
                    this.DoneSendingMessages();
                }
            }
        }

        void AbandonReceiveContexts()
        {
            if (this.channelExtension.ReceiveContextEnabled)
            {
                foreach (MessageRpc messageRpc in this.service.SessionMessages)
                {
                    try
                    {
                        if (TD.RoutingServiceAbandoningReceiveContextIsEnabled())
                        {
                            TD.RoutingServiceAbandoningReceiveContext(messageRpc.EventTraceActivity, messageRpc.UniqueID);
                        }
                        messageRpc.ReceiveContext.Abandon(this.timeoutHelper.RemainingTime());
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        if (TD.RoutingServiceHandledExceptionIsEnabled())
                        {
                            TD.RoutingServiceHandledException(messageRpc.EventTraceActivity, e);
                        }
                    }
                }
            }
        }

        bool DoneClosingChannels()
        {
            if (this.channelExtension.ReceiveContextEnabled)
            {
                this.ChangeState(ProcessingState.CompletingReceiveContexts);
            }
            else if (this.service.RetryTransaction != null || this.channelExtension.TransactedReceiveEnabled)
            {
                this.ChangeState(ProcessingState.CommittingTransaction);
            }
            else
            {
                this.ChangeState(ProcessingState.Completing);
            }
            return true;
        }

        bool DoneCommittingTransaction()
        {
            this.ChangeState(ProcessingState.Completing);
            return true;
        }

        bool DoneCompletingReceiveContexts()
        {
            if (this.service.RetryTransaction != null || this.channelExtension.TransactedReceiveEnabled)
            {
                this.ChangeState(ProcessingState.CommittingTransaction);
            }
            else
            {
                this.ChangeState(ProcessingState.Completing);
            }
            return true;
        }

        bool DoneInitializing()
        {
            this.ChangeState(ProcessingState.SendingSessionMessages);
            return true;
        }

        bool DoneSendingMessages()
        {
            if (this.closeOutboundChannels)
            {
                this.ChangeState(ProcessingState.ClosingChannels);
            }
            else if (this.channelExtension.HasSession)
            {
                this.ChangeState(ProcessingState.Completing);
            }
            else if (this.channelExtension.ReceiveContextEnabled)
            {
                this.ChangeState(ProcessingState.CompletingReceiveContexts);
            }
            else if (this.service.RetryTransaction != null || this.channelExtension.TransactedReceiveEnabled)
            {
                this.ChangeState(ProcessingState.CommittingTransaction);
            }
            else
            {
                this.ChangeState(ProcessingState.Completing);
            }
            return true;
        }

        bool CloseCurrentChannel()
        {
            this.client = this.channelExtension.SessionChannels.ReleaseChannel();
            
            if (this.client == null)
            {
                return this.DoneClosingChannels();
            }

            try
            {
                if (TD.RoutingServiceClosingClientIsEnabled())
                {
                    TD.RoutingServiceClosingClient(this.client.Key.ToString());
                }
                IAsyncResult result;
                using (this.PrepareTransactionalCall(this.service.GetTransactionForSending(null)))
                {
                    result = ((ICommunicationObject)this.client).BeginClose(this.timeoutHelper.RemainingTime(),
                        this.PrepareAsyncCompletion(channelCloseCallback), this);
                }

                if (this.CheckSyncContinue(result))
                {
                    this.ChannelCloseComplete(result);
                    return true;
                }
                return false;
            }
            catch (Exception exception)
            {
                if (this.HandleCloseFailure(exception))
                {
                    return true;
                }
                throw;
            }
        }

        static bool ChannelCloseCallback(IAsyncResult result)
        {
            ProcessMessagesAsyncResult<TContract> thisPtr = (ProcessMessagesAsyncResult<TContract>)result.AsyncState;
            FxTrace.Trace.SetAndTraceTransfer(thisPtr.channelExtension.ActivityID, true);
            try
            {
                thisPtr.allCompletedSync = false;
                try
                {
                    thisPtr.ChannelCloseComplete(result);
                }
                catch (Exception exception)
                {
                    if (!thisPtr.HandleCloseFailure(exception))
                    {
                        throw;
                    }
                }

                thisPtr.ProcessWhileSync();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                thisPtr.Fault(e);
            }
            return false;
        }

        void ChannelCloseComplete(IAsyncResult result)
        {
            ((ICommunicationObject)this.client).EndClose(result);
        }

        bool HandleClientOperationFailure(Exception e)
        {
            if (TD.RoutingServiceTransmitFailedIsEnabled())
            {
                TD.RoutingServiceTransmitFailed(null, this.client.Key.ToString(), e);
            }

            if (!(e is CommunicationException || e is TimeoutException))
            {
                //We only move to backup for CommunicationExceptions and TimeoutExceptions
                return false;
            }

            bool canHandle;
            MessageRpc messageRpc = this.service.SessionMessages[this.sessionMessageIndex];
            SendOperation sendOperation = messageRpc.Operations[this.destinationIndex];

            if ((e is CommunicationObjectAbortedException || e is CommunicationObjectFaultedException) && 
                !this.channelExtension.HasSession)
            {
                // Messages on a non sessionful channel share outbound connections and can 
                // fail due to other messages failing on the same channel
                bool canRetry = (this.channelExtension.ReceiveContextEnabled || !this.channelExtension.TransactedReceiveEnabled);
                if (canRetry && !this.abortedRetry)
                {
                    //No session and ReceiveContext or non transactional, retry the message 1 time (before moving to backup)
                    this.abortedRetry = true;
                    this.ResetState();
                    return true;
                }
            }
            else if (e is EndpointNotFoundException)
            {
                // The channel may not fault for this exception for bindings other than netTcpBinding
                // We abort the channel in that case. We proactively clean up so that we don't have to cleanup later
                SessionChannels sessionChannels = this.service.GetSessionChannels(messageRpc.Impersonating);
                if (sessionChannels != null)
                {
                    sessionChannels.AbortChannel(sendOperation.CurrentEndpoint);
                }
            }
            else if (e is MessageSecurityException)
            {
                // The service may have been stopped and restarted without the routing service knowledge.
                // When we try to use a cached channel to the service, the channel can fault due to this exception
                // The faulted channel gets cleaned up and we retry one more time only when service has backup
                // If there is no backup, we do not retry since we do not create a buffered message to prevent performance degradation
                if (!this.abortedRetry && (sendOperation.AlternateEndpointCount > 0))
                {
                    this.abortedRetry = true;
                    this.ResetState();
                    return true;
                }
            }

            if (sendOperation.TryMoveToAlternate(e))
            {
                if (TD.RoutingServiceMovedToBackupIsEnabled())
                {
                    TD.RoutingServiceMovedToBackup(messageRpc.EventTraceActivity, messageRpc.UniqueID, this.destinationIndex.ToString(TD.Culture), sendOperation.CurrentEndpoint.ToString());
                }
                this.ResetState();
                canHandle = true; 
            }
            else if (this.service.GetTransactionForSending(messageRpc) == null)
            {
                // This is OneWay with no Transaction...
                // store this exception for when we complete, but continue any multicasting
                this.service.SessionException = e;
                
                // Mark the SendOperation as 'Sent' because there's no more work we can do (non-tx and no more backups)
                sendOperation.TransmitSucceeded(null);

                if (this.channelExtension.HasSession)
                {
                    this.channelExtension.SessionChannels.AbortChannel(this.client.Key);
                }

                this.MoveToNextClientOperation(messageRpc.Operations.Count);
                canHandle = true;
            }
            else
            {
                canHandle = false;
            }

            return canHandle;
        }

        // A Sessionful channel failed when closing, find all messages that went on that 
        // session/channel and move them to their backup endpoints
        bool HandleCloseFailure(Exception e)
        {
            if (!(e is CommunicationException || e is TimeoutException))
            {
                return false;
            }

            if (TD.RoutingServiceCloseFailedIsEnabled())
            {
                TD.RoutingServiceCloseFailed(this.client.Key.ToString(), e);
            }
            this.channelExtension.SessionChannels.AbortChannel(this.client.Key);

            if (this.service.SessionMessages.Count == 0)
            {
                //All messages have been sent and we're non-transactional
                Fx.Assert(!this.service.ChannelExtension.TransactedReceiveEnabled, "Should only happen for non-transactional cases");
                return true;
            }

            foreach (MessageRpc messageRpc in this.service.SessionMessages)
            {
                for (this.destinationIndex = 0; this.destinationIndex < messageRpc.Operations.Count; this.destinationIndex++)
                {
                    SendOperation sendOperation = messageRpc.Operations[this.destinationIndex];
                    if (client.Key.Equals(sendOperation.CurrentEndpoint))
                    {
                        if (!sendOperation.TryMoveToAlternate(e))
                        {
                            return false;
                        }
                        if (TD.RoutingServiceMovedToBackupIsEnabled())
                        {
                            TD.RoutingServiceMovedToBackup(messageRpc.EventTraceActivity, messageRpc.UniqueID, this.destinationIndex.ToString(TD.Culture), sendOperation.CurrentEndpoint.ToString());
                        }
                    }
                }
            }

            this.ResetState();
            return true;
        }

        bool CompleteCurrentReceiveContext()
        {
            if (this.service.SessionException != null)
            {
                //This means at least one multicast branch did not reach any of the configured endpoints
                this.Fault(this.service.SessionException);
                return false;
            }

            bool keepGoing;
            MessageRpc messageRpc = this.service.SessionMessages[this.sessionMessageIndex];
            if (messageRpc.ReceiveContext != null)
            {
                if (TD.RoutingServiceCompletingReceiveContextIsEnabled())
                {
                    TD.RoutingServiceCompletingReceiveContext(messageRpc.EventTraceActivity, messageRpc.UniqueID);
                }

                IAsyncResult result;
                using (this.PrepareTransactionalCall(this.service.GetTransactionForSending(messageRpc)))
                {
                    result = messageRpc.ReceiveContext.BeginComplete(this.timeoutHelper.RemainingTime(),
                        this.PrepareAsyncCompletion(completeReceiveContextCallback), this);
                }
                if (this.CheckSyncContinue(result))
                {
                    keepGoing = this.CompleteReceiveContextCompleted(result);
                }
                else
                {
                    keepGoing = false;
                }
            }
            else
            {
                // Either all messages have RC or all messages don't have RC.  Since we don't have one
                // we know that none of these messages will, so we don't have to look at the other messages
                Fx.Assert("We shouldn't enter CompletingReceiveContexts state if the binding is not ReceiveContext capable");
                keepGoing = this.DoneCompletingReceiveContexts();
            }

            return keepGoing;
        }

        static bool CompleteReceiveContextCallback(IAsyncResult result)
        {
            ProcessMessagesAsyncResult<TContract> thisPtr = (ProcessMessagesAsyncResult<TContract>)result.AsyncState;
            FxTrace.Trace.SetAndTraceTransfer(thisPtr.channelExtension.ActivityID, true);
            try
            {
                thisPtr.allCompletedSync = false;
                if (thisPtr.CompleteReceiveContextCompleted(result))
                {
                    thisPtr.ProcessWhileSync();
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                thisPtr.Fault(e);
            }
            return false;
        }

        bool CompleteReceiveContextCompleted(IAsyncResult result)
        {
            MessageRpc messageRpc = this.service.SessionMessages[this.sessionMessageIndex];
            messageRpc.ReceiveContext.EndComplete(result);

            if (++this.sessionMessageIndex >= this.service.SessionMessages.Count)
            {
                return this.DoneCompletingReceiveContexts();
            }
            return true;
        }

        bool CommitTransaction()
        {
            if (this.service.RetryTransaction != null)
            {
                if (TD.RoutingServiceCommittingTransactionIsEnabled())
                {
                    TD.RoutingServiceCommittingTransaction(this.service.RetryTransaction.TransactionInformation.LocalIdentifier);
                }

                IAsyncResult result = this.service.RetryTransaction.BeginCommit(
                    this.PrepareAsyncCompletion(commitTransactionCallback), this);
                if (this.CheckSyncContinue(result))
                {
                    return this.CommitTransactionCompleted(result);
                }
                return false;
            }
            else if (this.channelExtension.TransactedReceiveEnabled)
            {
                if (TD.RoutingServiceCommittingTransactionIsEnabled())
                {
                    Transaction transaction = this.service.GetTransactionForSending(null);
                    TD.RoutingServiceCommittingTransaction(transaction != null ? transaction.TransactionInformation.LocalIdentifier : string.Empty);
                }
            }

            return this.DoneCommittingTransaction();
        }

        static bool CommitTransactionCallback(IAsyncResult result)
        {
            ProcessMessagesAsyncResult<TContract> thisPtr = (ProcessMessagesAsyncResult<TContract>)result.AsyncState;
            FxTrace.Trace.SetAndTraceTransfer(thisPtr.channelExtension.ActivityID, true);
            try
            {
                thisPtr.allCompletedSync = false;
                if (thisPtr.CommitTransactionCompleted(result))
                {
                    thisPtr.ProcessWhileSync();
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                thisPtr.Fault(e);
            }
            return false;
        }

        bool CommitTransactionCompleted(IAsyncResult result)
        {
            this.service.RetryTransaction.EndCommit(result);
            return this.DoneCommittingTransaction();
        }

        void CompleteSelf(Exception operationException)
        {
            Exception exception = operationException;
            if (exception == null && (this.closeOutboundChannels || !this.channelExtension.HasSession))
            {
                // It's possible that this last operation in a session didn't result in an exception
                // but we still have an exception to report when closing the session...
                exception = this.service.SessionException;
            }

            if (!this.closeOutboundChannels)
            {
                //When we're closing the channels that means end of session, there's no message per se.
                if (TD.RoutingServiceCompletingOneWayIsEnabled()) { TD.RoutingServiceCompletingOneWay(exception); }
            }
            this.Complete(this.allCompletedSync, exception);
        }

        internal static void End(IAsyncResult result)
        {
            AsyncResult.End<ProcessMessagesAsyncResult<TContract>>(result);
        }

        void Fault(Exception e)
        {
            this.service.ResetSession();
            this.AbandonReceiveContexts();

            this.CompleteSelf(e);
        }

        enum ProcessingState
        {
            Initial = 0,
            SendingSessionMessages,
            ClosingChannels,
            CompletingReceiveContexts,
            CommittingTransaction,
            Completing,
            Completed
        }
    }
}
