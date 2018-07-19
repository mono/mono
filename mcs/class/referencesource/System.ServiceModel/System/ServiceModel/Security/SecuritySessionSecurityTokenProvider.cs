//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Security
{
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security.Tokens;
    using System.Net;
    using System.Xml;
    using SafeFreeCredentials = System.IdentityModel.SafeFreeCredentials;

    class SecuritySessionSecurityTokenProvider : CommunicationObjectSecurityTokenProvider
    {
        static readonly MessageOperationFormatter operationFormatter = new MessageOperationFormatter();

        BindingContext issuerBindingContext;
        IChannelFactory<IRequestChannel> rstChannelFactory;
        SecurityAlgorithmSuite securityAlgorithmSuite;
        SecurityStandardsManager standardsManager;
        Object thisLock = new Object();
        SecurityKeyEntropyMode keyEntropyMode;
        SecurityTokenParameters issuedTokenParameters;
        bool requiresManualReplyAddressing;
        EndpointAddress targetAddress;
        SecurityBindingElement bootstrapSecurityBindingElement;
        Uri via;
        string sctUri;
        Uri privacyNoticeUri;
        int privacyNoticeVersion;
        MessageVersion messageVersion;
        EndpointAddress localAddress;
        ChannelParameterCollection channelParameters;
        SafeFreeCredentials credentialsHandle;
        bool ownCredentialsHandle;
        WebHeaderCollection webHeaderCollection;

        public SecuritySessionSecurityTokenProvider(SafeFreeCredentials credentialsHandle)
            : base()
        {
            this.credentialsHandle = credentialsHandle;
            this.standardsManager = SecurityStandardsManager.DefaultInstance;
            this.keyEntropyMode = AcceleratedTokenProvider.defaultKeyEntropyMode;
        }

        public WebHeaderCollection WebHeaders
        {
            get
            {
                return this.webHeaderCollection;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.webHeaderCollection = value;
            }
        }

        public SecurityAlgorithmSuite SecurityAlgorithmSuite
        {
            get
            {
                return this.securityAlgorithmSuite;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.securityAlgorithmSuite = value;
            }
        }

        public SecurityKeyEntropyMode KeyEntropyMode
        {
            get
            {
                return this.keyEntropyMode;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                SecurityKeyEntropyModeHelper.Validate(value);
                this.keyEntropyMode = value;
            }
        }

        MessageVersion MessageVersion
        {
            get
            {
                return this.messageVersion;
            }
        }

        public EndpointAddress TargetAddress
        {
            get { return this.targetAddress; }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.targetAddress = value;
            }
        }

        public EndpointAddress LocalAddress
        {
            get { return this.localAddress; }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.localAddress = value;
            }
        }

        public Uri Via
        {
            get { return this.via; }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.via = value;
            }
        }

        public BindingContext IssuerBindingContext
        {
            get
            {
                return this.issuerBindingContext;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.issuerBindingContext = value.Clone();
            }
        }

        public SecurityBindingElement BootstrapSecurityBindingElement
        {
            get { return this.bootstrapSecurityBindingElement; }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.bootstrapSecurityBindingElement = (SecurityBindingElement)value.Clone();
            }
        }

        public SecurityStandardsManager StandardsManager
        {
            get
            {
                return this.standardsManager;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                if (!value.TrustDriver.IsSessionSupported)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.TrustDriverVersionDoesNotSupportSession), "value"));
                }
                if (!value.SecureConversationDriver.IsSessionSupported)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.SecureConversationDriverVersionDoesNotSupportSession), "value"));
                }
                this.standardsManager = value;
            }
        }

        public SecurityTokenParameters IssuedSecurityTokenParameters
        {
            get
            {
                return this.issuedTokenParameters;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.issuedTokenParameters = value;
            }
        }

        public Uri PrivacyNoticeUri
        {
            get { return this.privacyNoticeUri; }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.privacyNoticeUri = value;
            }
        }

        public ChannelParameterCollection ChannelParameters
        {
            get { return this.channelParameters; }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.channelParameters = value;
            }
        }

        public int PrivacyNoticeVersion
        {
            get { return this.privacyNoticeVersion; }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.privacyNoticeVersion = value;
            }
        }

        public virtual XmlDictionaryString IssueAction
        {
            get
            {
                return this.standardsManager.SecureConversationDriver.IssueAction;
            }
        }

        public virtual XmlDictionaryString IssueResponseAction
        {
            get
            {
                return this.standardsManager.SecureConversationDriver.IssueResponseAction;
            }
        }


        public virtual XmlDictionaryString RenewAction
        {
            get
            {
                return this.standardsManager.SecureConversationDriver.RenewAction;
            }
        }

        public virtual XmlDictionaryString RenewResponseAction
        {
            get
            {
                return this.standardsManager.SecureConversationDriver.RenewResponseAction;
            }
        }

        public virtual XmlDictionaryString CloseAction
        {
            get
            {
                return standardsManager.SecureConversationDriver.CloseAction;
            }
        }

        public virtual XmlDictionaryString CloseResponseAction
        {
            get
            {
                return standardsManager.SecureConversationDriver.CloseResponseAction;
            }
        }

        // ISecurityCommunicationObject methods
        public override void OnAbort()
        {
            if (this.rstChannelFactory != null)
            {
                this.rstChannelFactory.Abort();
                this.rstChannelFactory = null;
            }
            FreeCredentialsHandle();
        }

        public override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (this.targetAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.TargetAddressIsNotSet, this.GetType())));
            }
            if (this.IssuerBindingContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.IssuerBuildContextNotSet, this.GetType())));
            }
            if (this.IssuedSecurityTokenParameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.IssuedSecurityTokenParametersNotSet, this.GetType())));
            }
            if (this.BootstrapSecurityBindingElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.BootstrapSecurityBindingElementNotSet, this.GetType())));
            }
            if (this.SecurityAlgorithmSuite == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecurityAlgorithmSuiteNotSet, this.GetType())));
            }
            InitializeFactories();
            this.rstChannelFactory.Open(timeoutHelper.RemainingTime());
            this.sctUri = this.StandardsManager.SecureConversationDriver.TokenTypeUri;
        }

        public override void OnOpening()
        {
            base.OnOpening();
            if (this.credentialsHandle == null)
            {
                if (this.IssuerBindingContext == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.IssuerBuildContextNotSet, this.GetType())));
                }
                if (this.BootstrapSecurityBindingElement == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.BootstrapSecurityBindingElementNotSet, this.GetType())));
                }
                this.credentialsHandle = SecurityUtils.GetCredentialsHandle(this.bootstrapSecurityBindingElement, this.issuerBindingContext);
                this.ownCredentialsHandle = true;
            }
        }

        public override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (this.rstChannelFactory != null)
            {
                this.rstChannelFactory.Close(timeoutHelper.RemainingTime());
                this.rstChannelFactory = null;
            }
            FreeCredentialsHandle();
        }

        void FreeCredentialsHandle()
        {
            if (this.credentialsHandle != null)
            {
                if (this.ownCredentialsHandle)
                {
                    this.credentialsHandle.Close();
                }
                this.credentialsHandle = null;
            }
        }

        void InitializeFactories()
        {
            ISecurityCapabilities securityCapabilities = this.BootstrapSecurityBindingElement.GetProperty<ISecurityCapabilities>(this.IssuerBindingContext);
            SecurityCredentialsManager securityCredentials = this.IssuerBindingContext.BindingParameters.Find<SecurityCredentialsManager>();
            if (securityCredentials == null)
            {
                securityCredentials = ClientCredentials.CreateDefaultCredentials();
            }
            BindingContext context = this.IssuerBindingContext;
            this.bootstrapSecurityBindingElement.ReaderQuotas = context.GetInnerProperty<XmlDictionaryReaderQuotas>();
            if (this.bootstrapSecurityBindingElement.ReaderQuotas == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.EncodingBindingElementDoesNotHandleReaderQuotas)));
            }
            TransportBindingElement transportBindingElement = context.RemainingBindingElements.Find<TransportBindingElement>();
            if (transportBindingElement != null)
                this.bootstrapSecurityBindingElement.MaxReceivedMessageSize = transportBindingElement.MaxReceivedMessageSize;

            SecurityProtocolFactory securityProtocolFactory = this.BootstrapSecurityBindingElement.CreateSecurityProtocolFactory<IRequestChannel>(this.IssuerBindingContext.Clone(), securityCredentials, false, this.IssuerBindingContext.Clone());
            if (securityProtocolFactory is MessageSecurityProtocolFactory)
            {
                MessageSecurityProtocolFactory soapBindingFactory = securityProtocolFactory as MessageSecurityProtocolFactory;
                soapBindingFactory.ApplyConfidentiality = soapBindingFactory.ApplyIntegrity
                    = soapBindingFactory.RequireConfidentiality = soapBindingFactory.RequireIntegrity = true;

                soapBindingFactory.ProtectionRequirements.IncomingSignatureParts.ChannelParts.IsBodyIncluded = true;
                soapBindingFactory.ProtectionRequirements.OutgoingSignatureParts.ChannelParts.IsBodyIncluded = true;

                MessagePartSpecification bodyPart = new MessagePartSpecification(true);
                soapBindingFactory.ProtectionRequirements.IncomingSignatureParts.AddParts(bodyPart, IssueAction);
                soapBindingFactory.ProtectionRequirements.IncomingEncryptionParts.AddParts(bodyPart, IssueAction);
                soapBindingFactory.ProtectionRequirements.IncomingSignatureParts.AddParts(bodyPart, RenewAction);
                soapBindingFactory.ProtectionRequirements.IncomingEncryptionParts.AddParts(bodyPart, RenewAction);

                soapBindingFactory.ProtectionRequirements.OutgoingSignatureParts.AddParts(bodyPart, IssueResponseAction);
                soapBindingFactory.ProtectionRequirements.OutgoingEncryptionParts.AddParts(bodyPart, IssueResponseAction);
                soapBindingFactory.ProtectionRequirements.OutgoingSignatureParts.AddParts(bodyPart, RenewResponseAction);
                soapBindingFactory.ProtectionRequirements.OutgoingEncryptionParts.AddParts(bodyPart, RenewResponseAction);
            }
            securityProtocolFactory.PrivacyNoticeUri = this.PrivacyNoticeUri;
            securityProtocolFactory.PrivacyNoticeVersion = this.privacyNoticeVersion;
            if (this.localAddress != null)
            {
                MessageFilter issueAndRenewFilter = new SessionActionFilter(this.standardsManager, this.IssueResponseAction.Value, this.RenewResponseAction.Value);
                context.BindingParameters.Add(new LocalAddressProvider(localAddress, issueAndRenewFilter));
            }
            ChannelBuilder channelBuilder = new ChannelBuilder(context, true);
            IChannelFactory<IRequestChannel> innerChannelFactory;
            // if the underlying transport does not support request/reply, wrap it inside
            // a service channel factory.
            if (channelBuilder.CanBuildChannelFactory<IRequestChannel>())
            {
                innerChannelFactory = channelBuilder.BuildChannelFactory<IRequestChannel>();
                requiresManualReplyAddressing = true;
            }
            else
            {
                ClientRuntime clientRuntime = new ClientRuntime("RequestSecuritySession", NamingHelper.DefaultNamespace);
                clientRuntime.UseSynchronizationContext = false;
                clientRuntime.AddTransactionFlowProperties = false;
                clientRuntime.ValidateMustUnderstand = false;
                ServiceChannelFactory serviceChannelFactory = ServiceChannelFactory.BuildChannelFactory(channelBuilder, clientRuntime);

                ClientOperation issueOperation = new ClientOperation(serviceChannelFactory.ClientRuntime, "Issue", IssueAction.Value);
                issueOperation.Formatter = operationFormatter;
                serviceChannelFactory.ClientRuntime.Operations.Add(issueOperation);

                ClientOperation renewOperation = new ClientOperation(serviceChannelFactory.ClientRuntime, "Renew", RenewAction.Value);
                renewOperation.Formatter = operationFormatter;
                serviceChannelFactory.ClientRuntime.Operations.Add(renewOperation);
                innerChannelFactory = new RequestChannelFactory(serviceChannelFactory);
                requiresManualReplyAddressing = false;
            }

            SecurityChannelFactory<IRequestChannel> securityChannelFactory = new SecurityChannelFactory<IRequestChannel>(
                securityCapabilities, this.IssuerBindingContext, channelBuilder, securityProtocolFactory, innerChannelFactory);

            // attach the ExtendedProtectionPolicy to the securityProtcolFactory so it will be 
            // available when building the channel.
            if (transportBindingElement != null)
            {
                if (securityChannelFactory.SecurityProtocolFactory != null)
                {
                    securityChannelFactory.SecurityProtocolFactory.ExtendedProtectionPolicy = transportBindingElement.GetProperty<ExtendedProtectionPolicy>(context);
                }
            }

            this.rstChannelFactory = securityChannelFactory;
            this.messageVersion = securityChannelFactory.MessageVersion;
        }

        // token provider methods
        protected override IAsyncResult BeginGetTokenCore(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.CommunicationObject.ThrowIfClosedOrNotOpen();
            return new SessionOperationAsyncResult(this, SecuritySessionOperation.Issue, this.TargetAddress, this.Via, null, timeout, callback, state);
        }

        protected override SecurityToken EndGetTokenCore(IAsyncResult result)
        {
            return SessionOperationAsyncResult.End(result);
        }

        protected override SecurityToken GetTokenCore(TimeSpan timeout)
        {
            this.CommunicationObject.ThrowIfClosedOrNotOpen();
            return this.DoOperation(SecuritySessionOperation.Issue, this.targetAddress, this.via, null, timeout);
        }

        protected override IAsyncResult BeginRenewTokenCore(TimeSpan timeout, SecurityToken tokenToBeRenewed, AsyncCallback callback, object state)
        {
            this.CommunicationObject.ThrowIfClosedOrNotOpen();
            return new SessionOperationAsyncResult(this, SecuritySessionOperation.Renew, this.TargetAddress, this.Via, tokenToBeRenewed, timeout, callback, state);
        }

        protected override SecurityToken EndRenewTokenCore(IAsyncResult result)
        {
            return SessionOperationAsyncResult.End(result);
        }

        protected override SecurityToken RenewTokenCore(TimeSpan timeout, SecurityToken tokenToBeRenewed)
        {
            this.CommunicationObject.ThrowIfClosedOrNotOpen();
            return this.DoOperation(SecuritySessionOperation.Renew, this.targetAddress, this.via, tokenToBeRenewed, timeout);
        }

        IRequestChannel CreateChannel(SecuritySessionOperation operation, EndpointAddress target, Uri via)
        {
            IChannelFactory<IRequestChannel> cf;
            if (operation == SecuritySessionOperation.Issue || operation == SecuritySessionOperation.Renew)
            {
                cf = this.rstChannelFactory;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            IRequestChannel channel;
            if (via != null)
            {
                channel = cf.CreateChannel(target, via);
            }
            else
            {
                channel = cf.CreateChannel(target);
            }
            if (this.channelParameters != null)
            {
                this.channelParameters.PropagateChannelParameters(channel);
            }
            if (this.ownCredentialsHandle)
            {
                ChannelParameterCollection newParameters = channel.GetProperty<ChannelParameterCollection>();
                if (newParameters != null)
                {
                    newParameters.Add(new SspiIssuanceChannelParameter(true, this.credentialsHandle));
                }
            }

            return channel;
        }

        Message CreateRequest(SecuritySessionOperation operation, EndpointAddress target, SecurityToken currentToken, out object requestState)
        {
            if (operation == SecuritySessionOperation.Issue)
            {
                return this.CreateIssueRequest(target, out requestState);
            }
            else if (operation == SecuritySessionOperation.Renew)
            {
                return this.CreateRenewRequest(target, currentToken, out requestState);
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
        }

        GenericXmlSecurityToken ProcessReply(Message reply, SecuritySessionOperation operation, object requestState)
        {
            ThrowIfFault(reply, this.targetAddress);
            GenericXmlSecurityToken issuedToken = null;
            if (operation == SecuritySessionOperation.Issue)
            {
                issuedToken = this.ProcessIssueResponse(reply, requestState);
            }
            else if (operation == SecuritySessionOperation.Renew)
            {
                issuedToken = this.ProcessRenewResponse(reply, requestState);
            }
            return issuedToken;
        }

        void OnOperationSuccess(SecuritySessionOperation operation, EndpointAddress target, SecurityToken issuedToken, SecurityToken currentToken)
        {
            SecurityTraceRecordHelper.TraceSecuritySessionOperationSuccess(operation, target, currentToken, issuedToken);
        }

        void OnOperationFailure(SecuritySessionOperation operation, EndpointAddress target, SecurityToken currentToken, Exception e, IChannel channel)
        {
            SecurityTraceRecordHelper.TraceSecuritySessionOperationFailure(operation, target, currentToken, e);
            if (channel != null)
            {
                channel.Abort();
            }
        }

        GenericXmlSecurityToken DoOperation(SecuritySessionOperation operation, EndpointAddress target, Uri via, SecurityToken currentToken, TimeSpan timeout)
        {
            if (target == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("target");
            }
            if (operation == SecuritySessionOperation.Renew && currentToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("currentToken");
            }
            IRequestChannel channel = null;
            try
            {
                SecurityTraceRecordHelper.TraceBeginSecuritySessionOperation(operation, target, currentToken);
                channel = this.CreateChannel(operation, target, via);

                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                channel.Open(timeoutHelper.RemainingTime());
                object requestState;
                GenericXmlSecurityToken issuedToken;

                using (Message requestMessage = this.CreateRequest(operation, target, currentToken, out requestState))
                {
                    EventTraceActivity eventTraceActivity = null;
                    if (TD.MessageReceivedFromTransportIsEnabled())
                    {
                        eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(requestMessage);
                    }

                    TraceUtility.ProcessOutgoingMessage(requestMessage, eventTraceActivity);

                    using (Message reply = channel.Request(requestMessage, timeoutHelper.RemainingTime()))
                    {
                        if (reply == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.FailToRecieveReplyFromNegotiation)));
                        }

                        if (eventTraceActivity == null && TD.MessageReceivedFromTransportIsEnabled())
                        {
                            eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(reply);
                        }

                        TraceUtility.ProcessIncomingMessage(reply, eventTraceActivity);
                        ThrowIfFault(reply, this.targetAddress);
                        issuedToken = ProcessReply(reply, operation, requestState);
                        ValidateKeySize(issuedToken);
                    }
                }
                channel.Close(timeoutHelper.RemainingTime());
                this.OnOperationSuccess(operation, target, issuedToken, currentToken);
                return issuedToken;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                if (e is TimeoutException)
                {
                    e = new TimeoutException(SR.GetString(SR.ClientSecuritySessionRequestTimeout, timeout), e);
                }

                OnOperationFailure(operation, target, currentToken, e, channel);
                throw;
            }
        }

        byte[] GenerateEntropy(int entropySize)
        {
            byte[] result = DiagnosticUtility.Utility.AllocateByteArray(entropySize / 8);
            CryptoHelper.FillRandomBytes(result);
            return result;
        }

        RequestSecurityToken CreateRst(EndpointAddress target, out object requestState)
        {
            RequestSecurityToken rst = new RequestSecurityToken(this.standardsManager);
            //rst.SetAppliesTo<EndpointAddress>(target, new XmlFormatter());
            rst.KeySize = this.SecurityAlgorithmSuite.DefaultSymmetricKeyLength;
            rst.TokenType = this.sctUri;
            if (this.KeyEntropyMode == SecurityKeyEntropyMode.ClientEntropy || this.KeyEntropyMode == SecurityKeyEntropyMode.CombinedEntropy)
            {
                byte[] entropy = GenerateEntropy(rst.KeySize);
                rst.SetRequestorEntropy(entropy);
                requestState = entropy;
            }
            else
            {
                requestState = null;
            }
            return rst;
        }

        void PrepareRequest(Message message)
        {
            RequestReplyCorrelator.PrepareRequest(message);
            if (this.requiresManualReplyAddressing)
            {
                if (this.localAddress != null)
                {
                    message.Headers.ReplyTo = this.LocalAddress;
                }
                else
                {
                    message.Headers.ReplyTo = EndpointAddress.AnonymousAddress;
                }
            }

            if (this.webHeaderCollection != null && this.webHeaderCollection.Count > 0)
            {
                object prop = null;
                HttpRequestMessageProperty rmp = null;
                if (message.Properties.TryGetValue(HttpRequestMessageProperty.Name, out prop))
                {
                    rmp = prop as HttpRequestMessageProperty;
                }
                else
                {
                    rmp = new HttpRequestMessageProperty();
                    message.Properties.Add(HttpRequestMessageProperty.Name, rmp);
                }

                if (rmp != null && rmp.Headers != null)
                {
                    rmp.Headers.Add(this.webHeaderCollection);
                }
            }

        }

        protected virtual Message CreateIssueRequest(EndpointAddress target, out object requestState)
        {
            this.CommunicationObject.ThrowIfClosedOrNotOpen();
            RequestSecurityToken rst = CreateRst(target, out requestState);
            rst.RequestType = this.StandardsManager.TrustDriver.RequestTypeIssue;
            rst.MakeReadOnly();
            Message result = Message.CreateMessage(this.MessageVersion, ActionHeader.Create(this.IssueAction, this.MessageVersion.Addressing), rst);
            PrepareRequest(result);
            return result;
        }

        GenericXmlSecurityToken ExtractToken(Message response, object requestState)
        {
            // get the claims corresponding to the server
            SecurityMessageProperty serverContextProperty = response.Properties.Security;
            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies;
            if (serverContextProperty != null && serverContextProperty.ServiceSecurityContext != null)
            {
                authorizationPolicies = serverContextProperty.ServiceSecurityContext.AuthorizationPolicies;
            }
            else
            {
                authorizationPolicies = EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
            }
            RequestSecurityTokenResponse rstr = null;
            XmlDictionaryReader bodyReader = response.GetReaderAtBodyContents();
            using (bodyReader)
            {
                if (this.StandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrustFeb2005)
                    rstr = this.StandardsManager.TrustDriver.CreateRequestSecurityTokenResponse(bodyReader);
                else if (this.StandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrust13)
                {
                    RequestSecurityTokenResponseCollection rstrc = this.StandardsManager.TrustDriver.CreateRequestSecurityTokenResponseCollection(bodyReader);
                    foreach (RequestSecurityTokenResponse rstrItem in rstrc.RstrCollection)
                    {
                        if (rstr != null)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.MoreThanOneRSTRInRSTRC)));

                        rstr = rstrItem;
                    }
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }
                response.ReadFromBodyContentsToEnd(bodyReader);
            }
            byte[] requestorEntropy;
            if (requestState != null)
            {
                requestorEntropy = (byte[])requestState;
            }
            else
            {
                requestorEntropy = null;
            }
            GenericXmlSecurityToken issuedToken = rstr.GetIssuedToken(null, null, this.KeyEntropyMode, requestorEntropy, this.sctUri, authorizationPolicies, this.SecurityAlgorithmSuite.DefaultSymmetricKeyLength, false);
            return issuedToken;
        }

        protected virtual GenericXmlSecurityToken ProcessIssueResponse(Message response, object requestState)
        {
            this.CommunicationObject.ThrowIfClosedOrNotOpen();
            return ExtractToken(response, requestState);
        }

        protected virtual Message CreateRenewRequest(EndpointAddress target, SecurityToken currentSessionToken, out object requestState)
        {
            this.CommunicationObject.ThrowIfClosedOrNotOpen();
            RequestSecurityToken rst = CreateRst(target, out requestState);
            rst.RequestType = this.StandardsManager.TrustDriver.RequestTypeRenew;
            rst.RenewTarget = this.IssuedSecurityTokenParameters.CreateKeyIdentifierClause(currentSessionToken, SecurityTokenReferenceStyle.External);
            rst.MakeReadOnly();
            Message result = Message.CreateMessage(this.MessageVersion, ActionHeader.Create(this.RenewAction, this.MessageVersion.Addressing), rst);
            SecurityMessageProperty supportingTokenProperty = new SecurityMessageProperty();
            supportingTokenProperty.OutgoingSupportingTokens.Add(new SupportingTokenSpecification(currentSessionToken, EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance, SecurityTokenAttachmentMode.Endorsing, this.IssuedSecurityTokenParameters));
            result.Properties.Security = supportingTokenProperty;
            PrepareRequest(result);
            return result;
        }

        protected virtual GenericXmlSecurityToken ProcessRenewResponse(Message response, object requestState)
        {
            this.CommunicationObject.ThrowIfClosedOrNotOpen();
            if (response.Headers.Action != this.RenewResponseAction.Value)
            {
                throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.InvalidRenewResponseAction, response.Headers.Action)), response);
            }
            return ExtractToken(response, requestState);
        }

        static protected void ThrowIfFault(Message message, EndpointAddress target)
        {
            SecurityUtils.ThrowIfNegotiationFault(message, target);
        }

        protected void ValidateKeySize(GenericXmlSecurityToken issuedToken)
        {
            this.CommunicationObject.ThrowIfClosedOrNotOpen();
            ReadOnlyCollection<SecurityKey> issuedKeys = issuedToken.SecurityKeys;
            if (issuedKeys != null && issuedKeys.Count == 1)
            {
                SymmetricSecurityKey symmetricKey = issuedKeys[0] as SymmetricSecurityKey;
                if (symmetricKey != null)
                {
                    if (this.SecurityAlgorithmSuite.IsSymmetricKeyLengthSupported(symmetricKey.KeySize))
                    {
                        return;
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.InvalidIssuedTokenKeySize, symmetricKey.KeySize)));
                    }
                }
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.CannotObtainIssuedTokenKeySize)));
            }
        }

        class SessionOperationAsyncResult : AsyncResult
        {
            static AsyncCallback openChannelCallback = Fx.ThunkCallback(new AsyncCallback(OpenChannelCallback));
            static AsyncCallback closeChannelCallback = Fx.ThunkCallback(new AsyncCallback(CloseChannelCallback));
            SecuritySessionSecurityTokenProvider requestor;
            SecuritySessionOperation operation;
            EndpointAddress target;
            Uri via;
            SecurityToken currentToken;
            GenericXmlSecurityToken issuedToken;
            IRequestChannel channel;
            TimeoutHelper timeoutHelper;

            public SessionOperationAsyncResult(SecuritySessionSecurityTokenProvider requestor, SecuritySessionOperation operation, EndpointAddress target, Uri via, SecurityToken currentToken, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.requestor = requestor;
                this.operation = operation;
                this.target = target;
                this.via = via;
                this.currentToken = currentToken;
                this.timeoutHelper = new TimeoutHelper(timeout);
                SecurityTraceRecordHelper.TraceBeginSecuritySessionOperation(operation, target, currentToken);
                bool completeSelf = false;
                try
                {
                    completeSelf = this.StartOperation();
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    this.OnOperationFailure(e);
                    throw;
                }
                if (completeSelf)
                {
                    this.OnOperationComplete();
                    Complete(true);
                }
            }

            /*
             *   Session issuance/renewal consists of the following steps (some may be async):
             *  1. Create a channel (sync)
             *  2. Open the channel (async)
             *  3. Create the request to send to server (sync)
             *  4. Send the message and get reply (async)
             *  5. Process the reply to get the token
             *  6. Close the channel (async) and complete the async result
             */
            bool StartOperation()
            {
                this.channel = this.requestor.CreateChannel(this.operation, this.target, this.via);
                IAsyncResult result = this.channel.BeginOpen(this.timeoutHelper.RemainingTime(), openChannelCallback, this);
                if (!result.CompletedSynchronously)
                {
                    return false;
                }
                this.channel.EndOpen(result);
                return this.OnChannelOpened();
            }

            static void OpenChannelCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                SessionOperationAsyncResult self = (SessionOperationAsyncResult)result.AsyncState;
                bool completeSelf = false;
                Exception completionException = null;
                try
                {
                    self.channel.EndOpen(result);
                    completeSelf = self.OnChannelOpened();
                    if (completeSelf)
                    {
                        self.OnOperationComplete();
                    }
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    completeSelf = true;
                    completionException = e;
                    self.OnOperationFailure(completionException);
                }
                if (completeSelf)
                {
                    self.Complete(false, completionException);
                }
            }

            bool OnChannelOpened()
            {
                object requestState;
                Message requestMessage = this.requestor.CreateRequest(this.operation, this.target, this.currentToken, out requestState);
                if (requestMessage == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.NullSessionRequestMessage, this.operation.ToString())));
                }

                ChannelOpenAsyncResultWrapper wrapper = new ChannelOpenAsyncResultWrapper();
                wrapper.Message = requestMessage;
                wrapper.RequestState = requestState;

                bool closeMessage = true;

                try
                {
                    IAsyncResult result = this.channel.BeginRequest(requestMessage, this.timeoutHelper.RemainingTime(), Fx.ThunkCallback(new AsyncCallback(this.RequestCallback)), wrapper);

                    if (!result.CompletedSynchronously)
                    {
                        closeMessage = false;
                        return false;
                    }

                    Message reply = this.channel.EndRequest(result);
                    return this.OnReplyReceived(reply, requestState);
                }
                finally
                {
                    if (closeMessage)
                    {
                        wrapper.Message.Close();
                    }
                }
            }

            void RequestCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                ChannelOpenAsyncResultWrapper wrapper = (ChannelOpenAsyncResultWrapper)result.AsyncState;

                object requestState = wrapper.RequestState;
                bool completeSelf = false;
                Exception completionException = null;
                try
                {
                    Message reply = this.channel.EndRequest(result);
                    completeSelf = this.OnReplyReceived(reply, requestState);
                    if (completeSelf)
                    {
                        this.OnOperationComplete();
                    }
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    completeSelf = true;
                    completionException = e;
                    this.OnOperationFailure(e);
                }
                finally
                {
                    if (wrapper.Message != null)
                        wrapper.Message.Close();
                }

                if (completeSelf)
                {
                    Complete(false, completionException);
                }
            }

            bool OnReplyReceived(Message reply, object requestState)
            {
                if (reply == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.FailToRecieveReplyFromNegotiation)));
                }

                using (reply)
                {
                    this.issuedToken = this.requestor.ProcessReply(reply, this.operation, requestState);
                    this.requestor.ValidateKeySize(this.issuedToken);
                }
                return this.OnReplyProcessed();
            }

            bool OnReplyProcessed()
            {
                IAsyncResult result = this.channel.BeginClose(this.timeoutHelper.RemainingTime(), closeChannelCallback, this);
                if (!result.CompletedSynchronously)
                {
                    return false;
                }
                this.channel.EndClose(result);
                return true;
            }

            static void CloseChannelCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                SessionOperationAsyncResult self = (SessionOperationAsyncResult)result.AsyncState;
                Exception completionException = null;
                try
                {
                    self.channel.EndClose(result);
                    self.OnOperationComplete();
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    completionException = e;
                    self.OnOperationFailure(completionException);
                }
                self.Complete(false, completionException);
            }

            void OnOperationFailure(Exception e)
            {
                try
                {
                    this.requestor.OnOperationFailure(operation, target, currentToken, e, this.channel);
                }
                catch (CommunicationException ex)
                {
                    DiagnosticUtility.TraceHandledException(ex, TraceEventType.Information);
                }
            }

            void OnOperationComplete()
            {
                this.requestor.OnOperationSuccess(this.operation, this.target, this.issuedToken, this.currentToken);
            }

            public static SecurityToken End(IAsyncResult result)
            {
                SessionOperationAsyncResult self = AsyncResult.End<SessionOperationAsyncResult>(result);
                return self.issuedToken;
            }
        }

        class ChannelOpenAsyncResultWrapper
        {
            public object RequestState;
            public Message Message;
        }

        internal class RequestChannelFactory : ChannelFactoryBase<IRequestChannel>
        {
            ServiceChannelFactory serviceChannelFactory;

            public RequestChannelFactory(ServiceChannelFactory serviceChannelFactory)
            {
                this.serviceChannelFactory = serviceChannelFactory;
            }

            protected override IRequestChannel OnCreateChannel(EndpointAddress address, Uri via)
            {
                return serviceChannelFactory.CreateChannel<IRequestChannel>(address, via);
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.serviceChannelFactory.BeginOpen(timeout, callback, state);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                this.serviceChannelFactory.EndOpen(result);
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new ChainedCloseAsyncResult(timeout, callback, state, base.OnBeginClose, base.OnEndClose, this.serviceChannelFactory);
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                ChainedCloseAsyncResult.End(result);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                base.OnClose(timeout);
                this.serviceChannelFactory.Close(timeout);
            }

            protected override void OnOpen(TimeSpan timeout)
            {
                this.serviceChannelFactory.Open(timeout);
            }

            protected override void OnAbort()
            {
                this.serviceChannelFactory.Abort();
                base.OnAbort();
            }

            public override T GetProperty<T>()
            {
                return this.serviceChannelFactory.GetProperty<T>();
            }
        }
    }
}
