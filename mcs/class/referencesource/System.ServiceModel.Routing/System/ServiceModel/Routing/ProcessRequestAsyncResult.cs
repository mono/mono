//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Routing
{
    using System;
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.Threading;
    using System.Transactions;
    //using System.Security.Principal;

    class ProcessRequestAsyncResult<TContract> : TransactedAsyncResult
    {
        static AsyncCompletion operationCallback = new AsyncCompletion(OperationCallback);
        RoutingService service;
        IRoutingClient currentClient;
        MessageRpc messageRpc;
        Message replyMessage;
        bool allCompletedSync;
        bool abortedRetry;

        public ProcessRequestAsyncResult(RoutingService service, Message message, AsyncCallback callback, object state)
            : base(callback, state)
        {
            this.allCompletedSync = true;
            this.service = service;
            this.messageRpc = new MessageRpc(message, OperationContext.Current, service.ChannelExtension.ImpersonationRequired);
            if (TD.RoutingServiceProcessingMessageIsEnabled())
            {
                TD.RoutingServiceProcessingMessage(this.messageRpc.EventTraceActivity, this.messageRpc.UniqueID,
                    message.Headers.Action, this.messageRpc.OperationContext.EndpointDispatcher.EndpointAddress.Uri.ToString(), messageRpc.Transaction != null ? "True" : "False");
            }

            try
            {
                EndpointNameMessageFilter.Set(this.messageRpc.Message.Properties, service.ChannelExtension.EndpointName);
                this.messageRpc.RouteToSingleEndpoint<TContract>(this.service.RoutingConfig);
            }
            catch (MultipleFilterMatchesException matchesException)
            {
                // Wrap this exception with one that is more meaningful to users of RoutingService:
                throw FxTrace.Exception.AsError(new ConfigurationErrorsException(SR.ReqReplyMulticastNotSupported(this.messageRpc.OperationContext.Channel.LocalAddress), matchesException));
            }

            while (this.StartProcessing())
            {
            }
        }

        bool StartProcessing()
        {
            bool callAgain = false;            
            SendOperation sendOperation = this.messageRpc.Operations[0];
            this.currentClient = this.service.GetOrCreateClient<TContract>(sendOperation.CurrentEndpoint, this.messageRpc.Impersonating);

            if (TD.RoutingServiceTransmittingMessageIsEnabled())
            {
                TD.RoutingServiceTransmittingMessage(this.messageRpc.EventTraceActivity, this.messageRpc.UniqueID, "0", this.currentClient.Key.ToString());
            }
            
            try
            {
                if (messageRpc.Transaction != null && sendOperation.HasAlternate)
                {
                    throw FxTrace.Exception.AsError(new ConfigurationErrorsException(SR.ErrorHandlingNotSupportedReqReplyTxn(this.messageRpc.OperationContext.Channel.LocalAddress)));
                }

                // We always work on cloned message when there are backup endpoints to handle exception cases
                Message message;
                if (sendOperation.AlternateEndpointCount > 0)
                {
                    message = messageRpc.CreateBuffer().CreateMessage();
                }
                else
                {
                    message = messageRpc.Message;
                }

                sendOperation.PrepareMessage(message);

                IAsyncResult result = null;
                using (this.PrepareTransactionalCall(messageRpc.Transaction))
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
                        result = this.currentClient.BeginOperation(message, messageRpc.Transaction, this.PrepareAsyncCompletion(operationCallback), this);
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
                    if (this.OperationComplete(result))
                    {
                        this.Complete(this.allCompletedSync);
                    }
                    else
                    {
                        callAgain = true;
                    }
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                
                if (!this.HandleClientOperationFailure(exception))
                {
                    throw;
                }
                callAgain = true;
            }
            return callAgain;
        }

        static bool OperationCallback(IAsyncResult result)
        {
            ProcessRequestAsyncResult<TContract> thisPtr = (ProcessRequestAsyncResult<TContract>)result.AsyncState;
            FxTrace.Trace.SetAndTraceTransfer(thisPtr.service.ChannelExtension.ActivityID, true);
            thisPtr.allCompletedSync = false;

            try
            {
                if (thisPtr.OperationComplete(result))
                {
                    return true;
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }

                if (!thisPtr.HandleClientOperationFailure(exception))
                {
                    throw;
                }
            }

            while (thisPtr.StartProcessing())
            {
            }

            return false;
        }

        // Returns true if we're all done and can complete this AsyncResult now
        bool OperationComplete(IAsyncResult result)
        {
            bool completeSelf = false;
            Message responseMsg = this.currentClient.EndOperation(result);

            if (TD.RoutingServiceTransmitSucceededIsEnabled())
            {
                TD.RoutingServiceTransmitSucceeded(this.messageRpc.EventTraceActivity, this.messageRpc.UniqueID, "0", this.currentClient.Key.ToString());
            }

            if (responseMsg == null || !responseMsg.IsFault)
            {
                if (TD.RoutingServiceSendingResponseIsEnabled())
                {
                    string action = (responseMsg != null) ? responseMsg.Headers.Action : string.Empty;
                    TD.RoutingServiceSendingResponse(this.messageRpc.EventTraceActivity, action);
                }
            }
            else
            {
                if (TD.RoutingServiceSendingFaultResponseIsEnabled()) { TD.RoutingServiceSendingFaultResponse(this.messageRpc.EventTraceActivity, responseMsg.Headers.Action); }
            }
            this.replyMessage = responseMsg;
            completeSelf = true;

            if (TD.RoutingServiceCompletingTwoWayIsEnabled()) { TD.RoutingServiceCompletingTwoWay(this.messageRpc.EventTraceActivity); }
            return completeSelf;
        }

        internal static Message End(IAsyncResult result)
        {
            ProcessRequestAsyncResult<TContract> processRequest = AsyncResult.End<ProcessRequestAsyncResult<TContract>>(result);
            return processRequest.replyMessage;
        }

        bool HandleClientOperationFailure(Exception exception)
        {
            SendOperation sendOperation = this.messageRpc.Operations[0];
            if (TD.RoutingServiceTransmitFailedIsEnabled()) { TD.RoutingServiceTransmitFailed(this.messageRpc.EventTraceActivity, sendOperation.CurrentEndpoint.ToString(), exception); }

            if (!(exception is CommunicationException || exception is TimeoutException))
            {
                //We only move to backup for CommunicationExceptions and TimeoutExceptions
                return false;
            }

            if ((exception is CommunicationObjectAbortedException || exception is CommunicationObjectFaultedException) && 
                !this.service.ChannelExtension.HasSession)
            {
                // Messages on a non sessionful channel share outbound connections and can 
                // fail due to other messages failing on the same channel
                if (messageRpc.Transaction == null && !this.abortedRetry)
                {
                    //No session and non transactional, retry the message 1 time (before moving to backup)
                    this.abortedRetry = true;
                    return true;
                }
            }
            else if (exception is EndpointNotFoundException)
            {
                // The channel may not fault for this exception for bindings other than netTcpBinding
                // We abort the channel in that case. We proactively clean up so that we don't have to cleanup later
                SessionChannels sessionChannels = this.service.GetSessionChannels(this.messageRpc.Impersonating);
                if (sessionChannels != null)
                {
                    sessionChannels.AbortChannel(sendOperation.CurrentEndpoint);
                }
            }
            else if (exception is MessageSecurityException)
            {
                // The service may have been stopped and restarted without the routing service knowledge.
                // When we try to use a cached channel to the service, the channel can fault due to this exception
                // The faulted channel gets cleaned up and we retry one more time only when service has backup
                // If there is no backup, we do not retry since we do not create a buffered message to prevent performance degradation
                if (!this.abortedRetry && (sendOperation.AlternateEndpointCount > 0))
                {
                    this.abortedRetry = true;
                    return true;
                }
            }

            if (sendOperation.TryMoveToAlternate(exception))
            {
                if (TD.RoutingServiceMovedToBackupIsEnabled())
                {
                    TD.RoutingServiceMovedToBackup(this.messageRpc.EventTraceActivity, messageRpc.UniqueID, "0", sendOperation.CurrentEndpoint.ToString());
                }

                return true;
            }

            return false;
        }
    }
}
