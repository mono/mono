//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Security
{
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Xml;
    using System.ServiceModel.Diagnostics.Application;

    // IssuanceTokenProviderBase is a base class for token providers that fetch tokens from
    // another party.
    // This class manages caching of tokens, async messaging, concurrency
    abstract class IssuanceTokenProviderBase<T> : CommunicationObjectSecurityTokenProvider
        where T : IssuanceTokenProviderState
    {
        internal const string defaultClientMaxTokenCachingTimeString = "10675199.02:48:05.4775807";
        internal const bool defaultClientCacheTokens = true;
        internal const int defaultServiceTokenValidityThresholdPercentage = 60;

        // if an issuer is explicitly specified it will be used otherwise target is the issuer
        EndpointAddress issuerAddress;
        // the target service's address and via
        EndpointAddress targetAddress;
        Uri via = null;

        // This controls whether the token provider caches the service tokens it obtains
        bool cacheServiceTokens = defaultClientCacheTokens;
        // This is a fudge factor that controls how long the client can use a service token
        int serviceTokenValidityThresholdPercentage = defaultServiceTokenValidityThresholdPercentage;
        // the maximum time that the client is willing to cache service tokens
        TimeSpan maxServiceTokenCachingTime;

        SecurityStandardsManager standardsManager;
        SecurityAlgorithmSuite algorithmSuite;
        ChannelProtectionRequirements applicationProtectionRequirements;
        SecurityToken cachedToken;
        Object thisLock = new Object();

        string sctUri;

        protected IssuanceTokenProviderBase()
            : base()
        {
            this.cacheServiceTokens = defaultClientCacheTokens;
            this.serviceTokenValidityThresholdPercentage = defaultServiceTokenValidityThresholdPercentage;
            this.maxServiceTokenCachingTime = DefaultClientMaxTokenCachingTime;
            this.standardsManager = null;
        }

        // settings
        public EndpointAddress IssuerAddress
        {
            get
            {
                return this.issuerAddress;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.issuerAddress = value;
            }
        }

        public EndpointAddress TargetAddress
        {
            get
            {
                return this.targetAddress;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.targetAddress = value;
            }
        }

        public bool CacheServiceTokens
        {
            get
            {
                return this.cacheServiceTokens;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.cacheServiceTokens = value;
            }
        }

        internal static TimeSpan DefaultClientMaxTokenCachingTime
        {
            get
            {
                Fx.Assert(TimeSpan.Parse(defaultClientMaxTokenCachingTimeString, CultureInfo.InvariantCulture) == TimeSpan.MaxValue, "TimeSpan value not correct");
                return TimeSpan.MaxValue;
            }
        }

        public int ServiceTokenValidityThresholdPercentage
        {
            get
            {
                return this.serviceTokenValidityThresholdPercentage;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value <= 0 || value > 100)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", SR.GetString(SR.ValueMustBeInRange, 1, 100)));
                }
                this.serviceTokenValidityThresholdPercentage = value;
            }
        }

        public SecurityAlgorithmSuite SecurityAlgorithmSuite
        {
            get
            {
                return this.algorithmSuite;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.algorithmSuite = value;
            }
        }

        public TimeSpan MaxServiceTokenCachingTime
        {
            get
            {
                return this.maxServiceTokenCachingTime;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", SR.GetString(SR.TimeSpanMustbeGreaterThanTimeSpanZero)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                this.maxServiceTokenCachingTime = value;
            }
        }


        public SecurityStandardsManager StandardsManager
        {
            get
            {
                if (this.standardsManager == null)
                    return SecurityStandardsManager.DefaultInstance;
                return this.standardsManager;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.standardsManager = value;
            }
        }

        public ChannelProtectionRequirements ApplicationProtectionRequirements
        {
            get
            {
                return this.applicationProtectionRequirements;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.applicationProtectionRequirements = value;
            }
        }

        public Uri Via
        {
            get
            {
                return this.via;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.via = value;
            }
        }

        public override bool SupportsTokenCancellation
        {
            get
            {
                return true;
            }
        }

        protected Object ThisLock
        {
            get { return this.thisLock; }
        }

        protected virtual bool IsMultiLegNegotiation
        {
            get { return true; }
        }

        protected abstract MessageVersion MessageVersion
        {
            get;
        }

        protected abstract bool RequiresManualReplyAddressing
        {
            get;
        }

        public abstract XmlDictionaryString RequestSecurityTokenAction
        {
            get;
        }

        public abstract XmlDictionaryString RequestSecurityTokenResponseAction
        {
            get;
        }

        protected string SecurityContextTokenUri
        {
            get
            {
                ThrowIfCreated();
                return this.sctUri;
            }
        }

        protected void ThrowIfCreated()
        {
            CommunicationState state = this.CommunicationObject.State;
            if (state == CommunicationState.Created)
            {
                Exception e = new InvalidOperationException(SR.GetString(SR.CommunicationObjectCannotBeUsed, this.GetType().ToString(), state.ToString()));
                throw TraceUtility.ThrowHelperError(e, Guid.Empty, this);
            }
        }

        protected void ThrowIfClosedOrCreated()
        {
            this.CommunicationObject.ThrowIfClosed();
            ThrowIfCreated();
        }

        // ISecurityCommunicationObject methods
        public override void OnOpen(TimeSpan timeout)
        {
            if (this.targetAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.TargetAddressIsNotSet, this.GetType())));
            }
            if (this.SecurityAlgorithmSuite == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecurityAlgorithmSuiteNotSet, this.GetType())));
            }
            this.sctUri = this.StandardsManager.SecureConversationDriver.TokenTypeUri;
        }

        // helper methods
        protected void EnsureEndpointAddressDoesNotRequireEncryption(EndpointAddress target)
        {
            if (this.ApplicationProtectionRequirements == null
                  || this.ApplicationProtectionRequirements.OutgoingEncryptionParts == null)
            {
                return;
            }
            MessagePartSpecification channelEncryptionParts = this.ApplicationProtectionRequirements.OutgoingEncryptionParts.ChannelParts;
            if (channelEncryptionParts == null)
            {
                return;
            }
            for (int i = 0; i < this.targetAddress.Headers.Count; ++i)
            {
                AddressHeader header = target.Headers[i];
                if (channelEncryptionParts.IsHeaderIncluded(header.Name, header.Namespace))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.SecurityNegotiationCannotProtectConfidentialEndpointHeader, target, header.Name, header.Namespace)));
                }
            }
        }

        DateTime GetServiceTokenEffectiveExpirationTime(SecurityToken serviceToken)
        {
            // if the token never expires, return the max date time
            // else return effective expiration time
            if (serviceToken.ValidTo.ToUniversalTime() >= SecurityUtils.MaxUtcDateTime)
            {
                return serviceToken.ValidTo;
            }

            TimeSpan interval = serviceToken.ValidTo.ToUniversalTime() - serviceToken.ValidFrom.ToUniversalTime();
            long serviceTokenTicksInterval = interval.Ticks;
            long effectiveTicksInterval = Convert.ToInt64((double)this.ServiceTokenValidityThresholdPercentage / 100.0 * (double)serviceTokenTicksInterval, NumberFormatInfo.InvariantInfo);
            DateTime effectiveExpirationTime = TimeoutHelper.Add(serviceToken.ValidFrom.ToUniversalTime(), new TimeSpan(effectiveTicksInterval));
            DateTime maxCachingTime = TimeoutHelper.Add(serviceToken.ValidFrom.ToUniversalTime(), this.MaxServiceTokenCachingTime);
            if (effectiveExpirationTime <= maxCachingTime)
            {
                return effectiveExpirationTime;
            }
            else
            {
                return maxCachingTime;
            }
        }

        bool IsServiceTokenTimeValid(SecurityToken serviceToken)
        {
            DateTime effectiveExpirationTime = GetServiceTokenEffectiveExpirationTime(serviceToken);
            return (DateTime.UtcNow <= effectiveExpirationTime);
        }

        SecurityToken GetCurrentServiceToken()
        {
            if (this.CacheServiceTokens && this.cachedToken != null && IsServiceTokenTimeValid(cachedToken))
            {
                return this.cachedToken;
            }
            else
            {
                return null;
            }
        }

        static protected void ThrowIfFault(Message message, EndpointAddress target)
        {
            SecurityUtils.ThrowIfNegotiationFault(message, target);
        }

        protected override IAsyncResult BeginGetTokenCore(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.CommunicationObject.ThrowIfClosedOrNotOpen();
            IAsyncResult asyncResult;
            lock (ThisLock)
            {
                SecurityToken token = GetCurrentServiceToken();
                if (token != null)
                {
                    SecurityTraceRecordHelper.TraceUsingCachedServiceToken(this, token, this.targetAddress);
                    asyncResult = new CompletedAsyncResult<SecurityToken>(token, callback, state);
                }
                else
                {
                    asyncResult = BeginNegotiation(timeout, callback, state);
                }
            }
            return asyncResult;
        }

        protected override SecurityToken EndGetTokenCore(IAsyncResult result)
        {
            if (result is CompletedAsyncResult<SecurityToken>)
            {
                return CompletedAsyncResult<SecurityToken>.End(result);
            }
            else
            {
                return this.EndNegotiation(result);
            }
        }

        protected override SecurityToken GetTokenCore(TimeSpan timeout)
        {
            this.CommunicationObject.ThrowIfClosedOrNotOpen();
            SecurityToken result;
            lock (ThisLock)
            {
                result = GetCurrentServiceToken();
                if (result != null)
                {
                    SecurityTraceRecordHelper.TraceUsingCachedServiceToken(this, result, this.targetAddress);
                }
            }
            if (result == null)
            {
                result = DoNegotiation(timeout);
            }
            return result;
        }

        protected override void CancelTokenCore(TimeSpan timeout, SecurityToken token)
        {
            if (this.CacheServiceTokens)
            {
                lock (ThisLock)
                {
                    if (Object.ReferenceEquals(token, this.cachedToken))
                    {
                        this.cachedToken = null;
                    }
                }
            }
        }

        // Negotiation state creation methods
        protected abstract bool CreateNegotiationStateCompletesSynchronously(EndpointAddress target, Uri via);
        protected abstract IAsyncResult BeginCreateNegotiationState(EndpointAddress target, Uri via, TimeSpan timeout, AsyncCallback callback, object state);
        protected abstract T CreateNegotiationState(EndpointAddress target, Uri via, TimeSpan timeout);
        protected abstract T EndCreateNegotiationState(IAsyncResult result);

        // Negotiation message processing methods
        protected abstract BodyWriter GetFirstOutgoingMessageBody(T negotiationState, out MessageProperties properties);
        protected abstract BodyWriter GetNextOutgoingMessageBody(Message incomingMessage, T negotiationState);
        protected abstract bool WillInitializeChannelFactoriesCompleteSynchronously(EndpointAddress target);
        protected abstract void InitializeChannelFactories(EndpointAddress target, TimeSpan timeout);
        protected abstract IAsyncResult BeginInitializeChannelFactories(EndpointAddress target, TimeSpan timeout, AsyncCallback callback, object state);
        protected abstract void EndInitializeChannelFactories(IAsyncResult result);
        protected abstract IRequestChannel CreateClientChannel(EndpointAddress target, Uri via);

        void PrepareRequest(Message nextMessage)
        {
            PrepareRequest(nextMessage, null);
        }

        void PrepareRequest(Message nextMessage, RequestSecurityToken rst)
        {
            if (rst != null && !rst.IsReadOnly)
            {
                rst.Message = nextMessage;
            }
            RequestReplyCorrelator.PrepareRequest(nextMessage);
            if (this.RequiresManualReplyAddressing)
            {
                // if we are on HTTP, we need to explicitly add a reply-to header for interop
                nextMessage.Headers.ReplyTo = EndpointAddress.AnonymousAddress;
            }

        }

        /*
        *   Negotiation consists of the following steps (some may be async in the async case):
        *   1. Create negotiation state 
        *   2. Initialize channel factories 
        *   3. Create an channel 
        *   4. Open the channel
        *   5. Create the next message to send to server
        *   6. Send the message and get reply 
        *   8. Process incoming message and get next outgoing message.
        *   9. If no outgoing message, then negotiation is over. Go to step 11.
        *   10. Goto step 6
        *   11. Close the IRequest channel and complete
        */
        protected SecurityToken DoNegotiation(TimeSpan timeout)
        {
            ThrowIfClosedOrCreated();
            SecurityTraceRecordHelper.TraceBeginSecurityNegotiation(this, this.targetAddress);
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            IRequestChannel rstChannel = null;
            T negotiationState = null;
            TimeSpan timeLeft = timeout;
            int legs = 1;
            try
            {
                negotiationState = this.CreateNegotiationState(this.targetAddress, this.via, timeoutHelper.RemainingTime());
                InitializeNegotiationState(negotiationState);
                this.InitializeChannelFactories(negotiationState.RemoteAddress, timeoutHelper.RemainingTime());
                rstChannel = this.CreateClientChannel(negotiationState.RemoteAddress, this.via);
                rstChannel.Open(timeoutHelper.RemainingTime());
                Message nextOutgoingMessage = null;
                Message incomingMessage = null;
                SecurityToken serviceToken = null;
                for (;;)
                {
                    nextOutgoingMessage = this.GetNextOutgoingMessage(incomingMessage, negotiationState);
                    if (incomingMessage != null)
                    {
                        incomingMessage.Close();
                    }
                    if (nextOutgoingMessage != null)
                    {
                        using (nextOutgoingMessage)
                        {
                            EventTraceActivity eventTraceActivity = null;
                            if (TD.MessageSentToTransportIsEnabled())
                            {
                                eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(nextOutgoingMessage);
                            }

                            TraceUtility.ProcessOutgoingMessage(nextOutgoingMessage, eventTraceActivity);
                            timeLeft = timeoutHelper.RemainingTime();
                            incomingMessage = rstChannel.Request(nextOutgoingMessage, timeLeft);
                            if (incomingMessage == null)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.FailToRecieveReplyFromNegotiation)));
                            }

                            if (eventTraceActivity == null && TD.MessageReceivedFromTransportIsEnabled())
                            {
                                eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(incomingMessage);
                            }

                            TraceUtility.ProcessIncomingMessage(incomingMessage, eventTraceActivity);
                        }
                        legs += 2;
                    }
                    else
                    {
                        if (!negotiationState.IsNegotiationCompleted)
                        {
                            throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.NoNegotiationMessageToSend)), incomingMessage);
                        }

                        try
                        {
                            rstChannel.Close(timeoutHelper.RemainingTime());
                        }
                        catch (CommunicationException e)
                        {
                            DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);

                            rstChannel.Abort();
                        }
                        catch (TimeoutException e)
                        {
                            if (TD.CloseTimeoutIsEnabled())
                            {
                                TD.CloseTimeout(e.Message);
                            }
                            DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);

                            rstChannel.Abort();
                        }

                        rstChannel = null;
                        this.ValidateAndCacheServiceToken(negotiationState);
                        serviceToken = negotiationState.ServiceToken;
                        SecurityTraceRecordHelper.TraceEndSecurityNegotiation(this, serviceToken, this.targetAddress);
                        break;
                    }
                }
                return serviceToken;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                if (e is TimeoutException)
                {
                    e = new TimeoutException(SR.GetString(SR.ClientSecurityNegotiationTimeout, timeout, legs, timeLeft), e);
                }
                EndpointAddress temp = (negotiationState == null) ? null : negotiationState.RemoteAddress;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(WrapExceptionIfRequired(e, temp, this.issuerAddress));
            }
            finally
            {
                Cleanup(rstChannel, negotiationState);
            }
        }

        void InitializeNegotiationState(T negotiationState)
        {
            negotiationState.TargetAddress = this.targetAddress;
            if (negotiationState.Context == null && this.IsMultiLegNegotiation)
            {
                negotiationState.Context = SecurityUtils.GenerateId();
            }
            if (this.IssuerAddress != null)
            {
                negotiationState.RemoteAddress = this.IssuerAddress;
            }
            else
            {
                negotiationState.RemoteAddress = negotiationState.TargetAddress;
            }
        }

        Message GetNextOutgoingMessage(Message incomingMessage, T negotiationState)
        {
            BodyWriter nextMessageBody;
            MessageProperties nextMessageProperties = null;
            if (incomingMessage == null)
            {
                nextMessageBody = this.GetFirstOutgoingMessageBody(negotiationState, out nextMessageProperties);
            }
            else
            {
                nextMessageBody = this.GetNextOutgoingMessageBody(incomingMessage, negotiationState);
            }
            if (nextMessageBody != null)
            {
                Message nextMessage;
                if (incomingMessage == null)
                {
                    nextMessage = Message.CreateMessage(this.MessageVersion, ActionHeader.Create(this.RequestSecurityTokenAction, this.MessageVersion.Addressing), nextMessageBody);
                }
                else
                {
                    nextMessage = Message.CreateMessage(this.MessageVersion, ActionHeader.Create(this.RequestSecurityTokenResponseAction, this.MessageVersion.Addressing), nextMessageBody);
                }
                if (nextMessageProperties != null)
                {
                    nextMessage.Properties.CopyProperties(nextMessageProperties);
                }

                PrepareRequest(nextMessage, nextMessageBody as RequestSecurityToken);
                return nextMessage;
            }
            else
            {
                return null;
            }
        }

        void Cleanup(IChannel rstChannel, T negotiationState)
        {
            if (negotiationState != null)
            {
                negotiationState.Dispose();
            }
            if (rstChannel != null)
            {
                rstChannel.Abort();
            }
        }

        protected IAsyncResult BeginNegotiation(TimeSpan timeout, AsyncCallback callback, object state)
        {
            ThrowIfClosedOrCreated();
            SecurityTraceRecordHelper.TraceBeginSecurityNegotiation(this, this.targetAddress);
            return new SecurityNegotiationAsyncResult(this, timeout, callback, state);
        }

        protected SecurityToken EndNegotiation(IAsyncResult result)
        {
            SecurityToken token = SecurityNegotiationAsyncResult.End(result);
            SecurityTraceRecordHelper.TraceEndSecurityNegotiation(this, token, this.targetAddress);
            return token;
        }

        protected virtual void ValidateKeySize(GenericXmlSecurityToken issuedToken)
        {
            if (this.SecurityAlgorithmSuite == null)
            {
                return;
            }
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

        static bool ShouldWrapException(Exception e)
        {
            return (e is System.ComponentModel.Win32Exception
                || e is XmlException
                || e is InvalidOperationException
                || e is ArgumentException
                || e is QuotaExceededException
                || e is System.Security.SecurityException
                || e is System.Security.Cryptography.CryptographicException
                || e is SecurityTokenException);
        }

        static Exception WrapExceptionIfRequired(Exception e, EndpointAddress targetAddress, EndpointAddress issuerAddress)
        {
            if (ShouldWrapException(e))
            {
                Uri targetUri;
                if (targetAddress != null)
                {
                    targetUri = targetAddress.Uri;
                }
                else
                {
                    targetUri = null;
                }

                Uri issuerUri;
                if (issuerAddress != null)
                {
                    issuerUri = issuerAddress.Uri;
                }
                else
                {
                    issuerUri = targetUri;
                }

                // => issuerUri != null
                if (targetUri != null)
                {
                    e = new SecurityNegotiationException(SR.GetString(SR.SoapSecurityNegotiationFailedForIssuerAndTarget, issuerUri, targetUri), e);
                }
                else
                {
                    e = new SecurityNegotiationException(SR.GetString(SR.SoapSecurityNegotiationFailed), e);
                }
            }
            return e;
        }

        void ValidateAndCacheServiceToken(T negotiationState)
        {
            this.ValidateKeySize(negotiationState.ServiceToken);
            lock (ThisLock)
            {
                if (this.CacheServiceTokens)
                {
                    this.cachedToken = negotiationState.ServiceToken;
                }
            }
        }

        class SecurityNegotiationAsyncResult : AsyncResult
        {
            static AsyncCallback createNegotiationStateCallback = Fx.ThunkCallback(new AsyncCallback(CreateNegotiationStateCallback));
            static AsyncCallback initializeChannelFactoriesCallback = Fx.ThunkCallback(new AsyncCallback(InitializeChannelFactoriesCallback));
            static AsyncCallback closeChannelCallback = Fx.ThunkCallback(new AsyncCallback(CloseChannelCallback));
            static AsyncCallback sendRequestCallback = Fx.ThunkCallback(new AsyncCallback(SendRequestCallback));
            static AsyncCallback openChannelCallback = Fx.ThunkCallback(new AsyncCallback(OpenChannelCallback));

            TimeSpan timeout;
            TimeoutHelper timeoutHelper;
            SecurityToken serviceToken;
            IssuanceTokenProviderBase<T> tokenProvider;
            IRequestChannel rstChannel;
            T negotiationState;
            Message nextOutgoingMessage;
            EndpointAddress target;
            EndpointAddress issuer;
            Uri via;

            public SecurityNegotiationAsyncResult(IssuanceTokenProviderBase<T> tokenProvider, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.timeout = timeout;
                timeoutHelper = new TimeoutHelper(timeout);
                this.tokenProvider = tokenProvider;
                this.target = tokenProvider.targetAddress;
                this.issuer = tokenProvider.issuerAddress;
                this.via = tokenProvider.via;
                bool completeSelf = false;
                try
                {
                    completeSelf = this.StartNegotiation();
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.OnSyncNegotiationFailure(e));
                }
                if (completeSelf)
                {
                    this.OnNegotiationComplete();
                    Complete(true);
                }
            }

            bool StartNegotiation()
            {
                if (this.tokenProvider.CreateNegotiationStateCompletesSynchronously(this.target, this.via))
                {
                    this.negotiationState = this.tokenProvider.CreateNegotiationState(target, this.via, timeoutHelper.RemainingTime());
                }
                else
                {
                    IAsyncResult createStateResult = this.tokenProvider.BeginCreateNegotiationState(target, this.via, timeoutHelper.RemainingTime(), createNegotiationStateCallback, this);
                    if (!createStateResult.CompletedSynchronously)
                    {
                        return false;
                    }
                    this.negotiationState = this.tokenProvider.EndCreateNegotiationState(createStateResult);
                }
                return this.OnCreateStateComplete();
            }

            static void CreateNegotiationStateCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                SecurityNegotiationAsyncResult self = (SecurityNegotiationAsyncResult)result.AsyncState;
                bool completeSelf = false;
                Exception completionException = null;
                try
                {
                    self.negotiationState = self.tokenProvider.EndCreateNegotiationState(result);
                    completeSelf = self.OnCreateStateComplete();
                    if (completeSelf)
                    {
                        self.OnNegotiationComplete();
                    }
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    completeSelf = true;
                    completionException = self.OnAsyncNegotiationFailure(e);
                }
                if (completeSelf)
                {
                    self.Complete(false, completionException);
                }
            }

            bool OnCreateStateComplete()
            {
                this.tokenProvider.InitializeNegotiationState(negotiationState);
                return InitializeChannelFactories();
            }

            bool InitializeChannelFactories()
            {
                if (this.tokenProvider.WillInitializeChannelFactoriesCompleteSynchronously(negotiationState.RemoteAddress))
                {
                    this.tokenProvider.InitializeChannelFactories(negotiationState.RemoteAddress, timeoutHelper.RemainingTime());
                }
                else
                {
                    IAsyncResult result = this.tokenProvider.BeginInitializeChannelFactories(negotiationState.RemoteAddress, timeoutHelper.RemainingTime(), initializeChannelFactoriesCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    this.tokenProvider.EndInitializeChannelFactories(result);
                }
                return this.OnChannelFactoriesInitialized();
            }

            static void InitializeChannelFactoriesCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                SecurityNegotiationAsyncResult self = (SecurityNegotiationAsyncResult)result.AsyncState;
                bool completeSelf = false;
                Exception completionException = null;
                try
                {
                    self.tokenProvider.EndInitializeChannelFactories(result);
                    completeSelf = self.OnChannelFactoriesInitialized();
                    if (completeSelf)
                    {
                        self.OnNegotiationComplete();
                    }
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    completeSelf = true;
                    completionException = self.OnAsyncNegotiationFailure(e);
                }
                if (completeSelf)
                {
                    self.Complete(false, completionException);
                }
            }

            bool OnChannelFactoriesInitialized()
            {
                this.rstChannel = this.tokenProvider.CreateClientChannel(negotiationState.RemoteAddress, this.via);
                this.nextOutgoingMessage = null;
                return this.OnRequestChannelCreated();
            }

            bool OnRequestChannelCreated()
            {
                IAsyncResult result = rstChannel.BeginOpen(timeoutHelper.RemainingTime(), openChannelCallback, this);
                if (!result.CompletedSynchronously)
                {
                    return false;
                }
                rstChannel.EndOpen(result);
                return this.OnRequestChannelOpened();
            }

            static void OpenChannelCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                SecurityNegotiationAsyncResult self = (SecurityNegotiationAsyncResult)result.AsyncState;
                bool completeSelf = false;
                Exception completionException = null;
                try
                {
                    self.rstChannel.EndOpen(result);
                    completeSelf = self.OnRequestChannelOpened();
                    if (completeSelf)
                    {
                        self.OnNegotiationComplete();
                    }
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    completeSelf = true;
                    completionException = self.OnAsyncNegotiationFailure(e);
                }
                if (completeSelf)
                {
                    self.Complete(false, completionException);
                }
            }

            bool OnRequestChannelOpened()
            {
                return this.SendRequest();
            }

            bool SendRequest()
            {
                if (this.nextOutgoingMessage == null)
                {
                    return this.DoNegotiation(null);
                }
                else
                {
                    this.tokenProvider.PrepareRequest(this.nextOutgoingMessage);
                    bool closeMessage = true;
                    Message incomingMessage = null;

                    IAsyncResult result = null;
                    try
                    {
                        result = this.rstChannel.BeginRequest(this.nextOutgoingMessage, timeoutHelper.RemainingTime(), sendRequestCallback, this);

                        if (!result.CompletedSynchronously)
                        {
                            closeMessage = false;
                            return false;
                        }


                        incomingMessage = rstChannel.EndRequest(result);
                    }
                    finally
                    {
                        if (closeMessage && this.nextOutgoingMessage != null)
                        {
                            this.nextOutgoingMessage.Close();
                        }
                    }

                    using (incomingMessage)
                    {
                        if (incomingMessage == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.FailToRecieveReplyFromNegotiation)));
                        }
                        return this.DoNegotiation(incomingMessage);
                    }
                }
            }

            static void SendRequestCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                SecurityNegotiationAsyncResult self = (SecurityNegotiationAsyncResult)result.AsyncState;
                bool completeSelf = false;
                Exception completionException = null;
                try
                {
                    Message incomingMessage = null;
                    try
                    {
                        incomingMessage = self.rstChannel.EndRequest(result);
                    }
                    finally
                    {
                        if (self.nextOutgoingMessage != null)
                        {
                            self.nextOutgoingMessage.Close();
                        }
                    }

                    using (incomingMessage)
                    {
                        if (incomingMessage == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.FailToRecieveReplyFromNegotiation)));
                        }
                        completeSelf = self.DoNegotiation(incomingMessage);
                    }

                    if (completeSelf)
                    {
                        self.OnNegotiationComplete();
                    }
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    completeSelf = true;
                    completionException = self.OnAsyncNegotiationFailure(e);
                }
                if (completeSelf)
                {
                    self.Complete(false, completionException);
                }
            }

            bool DoNegotiation(Message incomingMessage)
            {
                this.nextOutgoingMessage = this.tokenProvider.GetNextOutgoingMessage(incomingMessage, this.negotiationState);
                if (this.nextOutgoingMessage != null)
                {
                    return SendRequest();
                }
                else
                {
                    if (!negotiationState.IsNegotiationCompleted)
                    {
                        throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.NoNegotiationMessageToSend)), incomingMessage);
                    }
                    return this.CloseRequestChannel();
                }
            }

            bool CloseRequestChannel()
            {
                IAsyncResult result = rstChannel.BeginClose(timeoutHelper.RemainingTime(), closeChannelCallback, this);
                if (!result.CompletedSynchronously)
                {
                    return false;
                }
                rstChannel.EndClose(result);
                return true;
            }

            static void CloseChannelCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                SecurityNegotiationAsyncResult self = (SecurityNegotiationAsyncResult)result.AsyncState;
                bool completeSelf = false;
                Exception completionException = null;
                try
                {
                    self.rstChannel.EndClose(result);
                    self.OnNegotiationComplete();
                    completeSelf = true;
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;
                    completeSelf = true;
                    completionException = self.OnAsyncNegotiationFailure(e);
                }
                if (completeSelf)
                {
                    self.Complete(false, completionException);
                }
            }

            void Cleanup()
            {
                this.tokenProvider.Cleanup(this.rstChannel, this.negotiationState);
                this.rstChannel = null;
                this.negotiationState = null;
            }

            Exception OnAsyncNegotiationFailure(Exception e)
            {
                EndpointAddress pinnedEpr = null;
                try
                {
                    pinnedEpr = (this.negotiationState == null) ? null : this.negotiationState.RemoteAddress;
                    Cleanup();
                }
                catch (CommunicationException ex)
                {
                    DiagnosticUtility.TraceHandledException(ex, TraceEventType.Information);
                }

                return IssuanceTokenProviderBase<T>.WrapExceptionIfRequired(e, pinnedEpr, this.issuer);
            }

            Exception OnSyncNegotiationFailure(Exception e)
            {
                EndpointAddress pinnedTarget = (this.negotiationState == null) ? null : this.negotiationState.RemoteAddress;
                return IssuanceTokenProviderBase<T>.WrapExceptionIfRequired(e, pinnedTarget, this.issuer);
            }

            void OnNegotiationComplete()
            {
                using (negotiationState)
                {
                    SecurityToken token = negotiationState.ServiceToken;
                    this.tokenProvider.ValidateAndCacheServiceToken(negotiationState);
                    this.serviceToken = token;
                }
            }

            public static SecurityToken End(IAsyncResult result)
            {
                SecurityNegotiationAsyncResult self = AsyncResult.End<SecurityNegotiationAsyncResult>(result);
                return self.serviceToken;
            }
        }
    }
}
