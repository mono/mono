//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Security
{
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Xml;

    // This is the base class for all token providers that negotiate an SCT from
    // the target service.
    abstract class NegotiationTokenProvider<T> : IssuanceTokenProviderBase<T>
        where T : IssuanceTokenProviderState
    {
        IChannelFactory<IRequestChannel> rstChannelFactory;
        bool requiresManualReplyAddressing;
        BindingContext issuanceBindingContext;
        MessageVersion messageVersion;

        protected NegotiationTokenProvider()
            : base()
        {
        }

        public BindingContext IssuerBindingContext
        {
            get { return this.issuanceBindingContext; }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.issuanceBindingContext = value.Clone();
            }
        }

        public override XmlDictionaryString RequestSecurityTokenAction
        {
            get 
            {
                return this.StandardsManager.TrustDriver.RequestSecurityTokenAction;
            }
        }

        public override XmlDictionaryString RequestSecurityTokenResponseAction
        {
            get 
            {
                return this.StandardsManager.TrustDriver.RequestSecurityTokenResponseAction;
            }
        }

        protected override MessageVersion MessageVersion
        {
            get
            {
                return this.messageVersion;
            }
        }

        protected override bool RequiresManualReplyAddressing
        {
            get 
            {
                ThrowIfCreated();
                return this.requiresManualReplyAddressing;
            }
        }

        public override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (this.rstChannelFactory != null)
            {
                this.rstChannelFactory.Close(timeout);
                this.rstChannelFactory = null;
            }
            base.OnClose(timeoutHelper.RemainingTime());
        }

        public override void OnAbort()
        {
            if (this.rstChannelFactory != null)
            {
                this.rstChannelFactory.Abort();
                this.rstChannelFactory = null;
            }
            base.OnAbort();
        }

        public override void OnOpen(TimeSpan timeout)
        {
            if (this.IssuerBindingContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.IssuerBuildContextNotSet, this.GetType())));
            }
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.SetupRstChannelFactory();
            this.rstChannelFactory.Open(timeout);
            base.OnOpen(timeoutHelper.RemainingTime());
        }

        protected abstract IChannelFactory<IRequestChannel> GetNegotiationChannelFactory(IChannelFactory<IRequestChannel> transportChannelFactory, ChannelBuilder channelBuilder);

        void SetupRstChannelFactory()
        {
            IChannelFactory<IRequestChannel> innerChannelFactory = null;
            ChannelBuilder channelBuilder = new ChannelBuilder(this.IssuerBindingContext.Clone(), true);
            // if the underlying transport does not support request/reply, wrap it inside
            // a service channel factory.
            if (channelBuilder.CanBuildChannelFactory<IRequestChannel>())
            {
                innerChannelFactory = channelBuilder.BuildChannelFactory<IRequestChannel>();
                this.requiresManualReplyAddressing = true;
            }
            else
            {
                ClientRuntime clientRuntime = new ClientRuntime("RequestSecurityTokenContract", NamingHelper.DefaultNamespace);
                clientRuntime.ValidateMustUnderstand = false;
                ServiceChannelFactory serviceChannelFactory = ServiceChannelFactory.BuildChannelFactory(channelBuilder, clientRuntime);

                serviceChannelFactory.ClientRuntime.UseSynchronizationContext = false;
                serviceChannelFactory.ClientRuntime.AddTransactionFlowProperties = false;
                ClientOperation rstOperation = new ClientOperation(serviceChannelFactory.ClientRuntime, "RequestSecurityToken", this.RequestSecurityTokenAction.Value);
                rstOperation.Formatter = MessageOperationFormatter.Instance;
                serviceChannelFactory.ClientRuntime.Operations.Add(rstOperation);

                if (this.IsMultiLegNegotiation)
                {
                    ClientOperation rstrOperation = new ClientOperation(serviceChannelFactory.ClientRuntime, "RequestSecurityTokenResponse", this.RequestSecurityTokenResponseAction.Value);
                    rstrOperation.Formatter = MessageOperationFormatter.Instance;
                    serviceChannelFactory.ClientRuntime.Operations.Add(rstrOperation);
                }
                // service channel automatically adds reply headers
                this.requiresManualReplyAddressing = false;
                innerChannelFactory = new SecuritySessionSecurityTokenProvider.RequestChannelFactory(serviceChannelFactory);
            }

            this.rstChannelFactory = GetNegotiationChannelFactory(innerChannelFactory, channelBuilder);
            this.messageVersion = channelBuilder.Binding.MessageVersion;
        }
       
        // negotiation message processing overrides
        protected override bool WillInitializeChannelFactoriesCompleteSynchronously(EndpointAddress target)
        {
            return true;
        }

        protected override void InitializeChannelFactories(EndpointAddress target, TimeSpan timeout)
        {
        }

        protected override IAsyncResult BeginInitializeChannelFactories(EndpointAddress target, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void EndInitializeChannelFactories(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override IRequestChannel CreateClientChannel(EndpointAddress target, Uri via)
        {
            if (via != null)
            {
                return this.rstChannelFactory.CreateChannel(target, via);
            }
            else
            {
                return this.rstChannelFactory.CreateChannel(target);
            }
        }
    }
}
