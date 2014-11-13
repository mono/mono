//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------


[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(System.Runtime.FxCop.Category.Performance, 
System.Runtime.FxCop.Rule.AvoidUncalledPrivateCode, 
Scope = "member", 
Target = "System.ServiceModel.Routing.SR.RoutingExtensionNotFound", 
Justification = "gets called in RoutingService.ctor(). bug in fxcop")]

namespace System.ServiceModel.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Transactions;
    using SR2 = System.ServiceModel.Routing.SR;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics;

    // Some of the [ServiceBehavior] settings are configured in RoutingBehavior class since 
    // we need to pick the options dynamically based on whether we have transactions or not
    [SuppressMessage(FxCop.Category.Xaml, FxCop.Rule.TypesMustHaveXamlCallableConstructors)]
    [SuppressMessage(FxCop.Category.Xaml, FxCop.Rule.TypesShouldHavePublicParameterlessConstructors)]    
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any, InstanceContextMode = InstanceContextMode.PerSession,
        UseSynchronizationContext = false, ValidateMustUnderstand = false)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public sealed class RoutingService :
        ISimplexDatagramRouter,
        ISimplexSessionRouter,
        IRequestReplyRouter,
        IDuplexSessionRouter, 
        IDisposable
    {
        SessionChannels perMessageChannels;
        OperationContext operationContext;
        EventTraceActivity eventTraceActivity;

        RoutingService()
        {
            this.SessionMessages = new List<MessageRpc>(1);
            
            //We only need to call this here if we trace in this method.  BeginXXX methods call it again.
            //FxTrace.Trace.SetAndTraceTransfer(this.ActivityID, true);

            this.operationContext = OperationContext.Current;
            if (Fx.Trace.IsEtwProviderEnabled)
            {
                this.eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(this.operationContext.IncomingMessage);
            }

            IContextChannel channel = this.operationContext.Channel;
            
            ServiceHostBase host = this.operationContext.Host;
            this.ChannelExtension = channel.Extensions.Find<RoutingChannelExtension>();
            if (this.ChannelExtension == null)
            {
                throw FxTrace.Exception.AsError(new ConfigurationErrorsException(SR2.RoutingExtensionNotFound));
            }
            
            this.RoutingConfig = host.Extensions.Find<RoutingExtension>().RoutingConfiguration;
            this.RoutingConfig.VerifyConfigured();
            this.ChannelExtension.AttachService(this);
        }

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "private setter does get called")]
        internal RoutingChannelExtension ChannelExtension
        {
            get;
            private set;
        }

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "private setter does get called")]
        internal RoutingConfiguration RoutingConfig
        {
            get;
            private set;
        }

        internal CommittableTransaction RetryTransaction
        {
            get;
            private set;
        }

        internal Exception SessionException
        {
            get;
            set;
        }

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "private setter does get called")]
        internal IList<MessageRpc> SessionMessages
        {
            get;
            private set;
        }

        Transaction ReceiveTransaction
        {
            get;
            set;
        }

        internal void CreateNewTransactionIfNeeded(MessageRpc messageRpc)
        {
            if (messageRpc.Transaction != null && this.ChannelExtension.TransactedReceiveEnabled)
            {
                if (TD.RoutingServiceUsingExistingTransactionIsEnabled()) 
                {
                    TD.RoutingServiceUsingExistingTransaction(messageRpc.EventTraceActivity, messageRpc.Transaction.TransactionInformation.LocalIdentifier); 
                }
                Fx.Assert(this.ReceiveTransaction == null, "Should only happen at the start of a session.");
                this.ReceiveTransaction = messageRpc.Transaction;
                return;
            }
            else if (!this.ChannelExtension.TransactedReceiveEnabled || !this.ChannelExtension.ReceiveContextEnabled)
            {
                return;
            }

            Fx.Assert(this.RetryTransaction == null, "Logic error, we shouldn't be calling CreateNewTransactionIfNeeded if we have a RC Transaction");

            ChannelDispatcher channelDispatcher = this.operationContext.EndpointDispatcher.ChannelDispatcher;
            TimeSpan timeout = channelDispatcher.TransactionTimeout;
            IsolationLevel isolation = channelDispatcher.TransactionIsolationLevel;
            TransactionOptions options = new TransactionOptions();
            if (timeout > TimeSpan.Zero)
            {
                options.Timeout = timeout;
            }
            if (isolation != IsolationLevel.Unspecified)
            {
                options.IsolationLevel = isolation;
            }

            this.RetryTransaction = new CommittableTransaction(options);
            if (TD.RoutingServiceCreatingTransactionIsEnabled())
            {
                TD.RoutingServiceCreatingTransaction(messageRpc.EventTraceActivity, this.RetryTransaction.TransactionInformation.LocalIdentifier);
            }
        }

        internal SessionChannels GetSessionChannels(bool impersonating)
        {
            if (impersonating && !this.ChannelExtension.HasSession)
            {
                return this.perMessageChannels;
            }
            else
            {
                return this.ChannelExtension.SessionChannels;
            }
        }

        internal IRoutingClient GetOrCreateClient<TContract>(RoutingEndpointTrait endpointTrait, bool impersonating)
        {
            if (impersonating && !this.ChannelExtension.HasSession)
            {
                if (this.perMessageChannels == null)
                {
                    this.perMessageChannels = new SessionChannels(this.ChannelExtension.ActivityID);
                }
                return this.perMessageChannels.GetOrCreateClient<TContract>(endpointTrait, this, impersonating);
            }
            else
            {
                return this.ChannelExtension.SessionChannels.GetOrCreateClient<TContract>(endpointTrait, this, impersonating);
            }
        }

        internal Transaction GetTransactionForSending(MessageRpc messageRpc)
        {
            if (messageRpc != null && messageRpc.Transaction != null)
            {
                return messageRpc.Transaction;
            }
            if (this.ReceiveTransaction != null)
            {
                //This is the transaction used for the receive, we cannot perform error handling since we 
                //didn't create it.
                return this.ReceiveTransaction;
            }
            else
            {
                //This could be null, indicating non-transactional behavior
                return this.RetryTransaction;
            }
        }

        void IDisposable.Dispose()
        {
            if (this.perMessageChannels != null)
            {
                //This is for impersonation, thus it's supposed to complete [....]
                IAsyncResult result = this.perMessageChannels.BeginClose(this.ChannelExtension.OperationTimeout, null, null);
                this.perMessageChannels.EndClose(result);
                this.perMessageChannels = null;
            }
        }

        internal void ResetSession()
        {
            //Are we in a transactional error handling case (i.e. ReceiveContext)?
            if (this.RetryTransaction != null)
            {
                if (this.ChannelExtension.HasSession)
                {
                    this.ChannelExtension.SessionChannels.AbortAll();
                }

                RoutingUtilities.SafeRollbackTransaction(this.RetryTransaction);
                this.RetryTransaction = null;
            }
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        IAsyncResult ISimplexSessionRouter.BeginProcessMessage(Message message, AsyncCallback callback, object state)
        {
            return this.BeginProcessMessage<ISimplexSessionRouter>(message, callback, state);
        }

        void ISimplexSessionRouter.EndProcessMessage(IAsyncResult result)
        {
            this.EndProcessMessage<ISimplexSessionRouter>(result);
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        IAsyncResult IRequestReplyRouter.BeginProcessRequest(Message message, AsyncCallback callback, object state)
        {
            return this.BeginProcessRequest<IRequestReplyRouter>(message, callback, state);
        }

        Message IRequestReplyRouter.EndProcessRequest(IAsyncResult result)
        {
            return this.EndProcessRequest<IRequestReplyRouter>(result);
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        IAsyncResult IDuplexSessionRouter.BeginProcessMessage(Message message, AsyncCallback callback, object state)
        {
            return this.BeginProcessMessage<IDuplexSessionRouter>(message, callback, state);
        }

        void IDuplexSessionRouter.EndProcessMessage(IAsyncResult result)
        {
            this.EndProcessMessage<IDuplexSessionRouter>(result);
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        IAsyncResult ISimplexDatagramRouter.BeginProcessMessage(Message message, AsyncCallback callback, object state)
        {
            return this.BeginProcessMessage<ISimplexDatagramRouter>(message, callback, state);
        }

        void ISimplexDatagramRouter.EndProcessMessage(IAsyncResult result)
        {
            this.EndProcessMessage<ISimplexDatagramRouter>(result);
        }

        IAsyncResult BeginProcessMessage<TContract>(Message message, AsyncCallback callback, object state)
        {
            try
            {
                FxTrace.Trace.SetAndTraceTransfer(this.ChannelExtension.ActivityID, true);
                return new ProcessMessagesAsyncResult<TContract>(message, this, this.ChannelExtension.OperationTimeout, callback, state);
            }
            catch (Exception exception)
            {
                if (TD.RoutingServiceProcessingFailureIsEnabled())
                {
                    TD.RoutingServiceProcessingFailure(this.eventTraceActivity, OperationContext.Current.Channel.LocalAddress.ToString(), exception);
                }
                throw;
            }
        }

        void EndProcessMessage<TContract>(IAsyncResult result)
        {
            try
            {
                FxTrace.Trace.SetAndTraceTransfer(this.ChannelExtension.ActivityID, true);
                ProcessMessagesAsyncResult<TContract>.End(result);
            }
            catch (Exception exception)
            {
                if (TD.RoutingServiceProcessingFailureIsEnabled())
                {
                    TD.RoutingServiceProcessingFailure(this.eventTraceActivity, OperationContext.Current.Channel.LocalAddress.ToString(), exception);
                }
                throw;
            }
        }

        IAsyncResult BeginProcessRequest<TContract>(Message message, AsyncCallback callback, object state)
        {
            try
            {
                FxTrace.Trace.SetAndTraceTransfer(this.ChannelExtension.ActivityID, true);
                return new ProcessRequestAsyncResult<TContract>(this, message, callback, state);
            }
            catch (Exception exception)
            {
                if (TD.RoutingServiceProcessingFailureIsEnabled())
                {
                    TD.RoutingServiceProcessingFailure(this.eventTraceActivity, OperationContext.Current.Channel.LocalAddress.ToString(), exception);
                }
                throw;
            }
        }

        Message EndProcessRequest<TContract>(IAsyncResult result)
        {
            try
            {
                FxTrace.Trace.SetAndTraceTransfer(this.ChannelExtension.ActivityID, true);
                return ProcessRequestAsyncResult<TContract>.End(result);
            }
            catch (Exception exception)
            {
                if (TD.RoutingServiceProcessingFailureIsEnabled())
                {
                    TD.RoutingServiceProcessingFailure(this.eventTraceActivity, OperationContext.Current.Channel.LocalAddress.ToString(), exception);
                }
                throw;
            }
        }
    }
}
