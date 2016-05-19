//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.ObjectModel;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Net;
    using System.Net.Security;
    using System.Runtime;
    using System.Security.Authentication;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Security;

    class WindowsStreamSecurityUpgradeProvider : StreamSecurityUpgradeProvider
    {
        bool extractGroupsForWindowsAccounts;
        EndpointIdentity identity;
        IdentityVerifier identityVerifier;
        ProtectionLevel protectionLevel;
        SecurityTokenManager securityTokenManager;
        NetworkCredential serverCredential;
        string scheme;
        bool isClient;
        Uri listenUri;

        public WindowsStreamSecurityUpgradeProvider(WindowsStreamSecurityBindingElement bindingElement,
            BindingContext context, bool isClient)
            : base(context.Binding)
        {
            this.extractGroupsForWindowsAccounts = TransportDefaults.ExtractGroupsForWindowsAccounts;
            this.protectionLevel = bindingElement.ProtectionLevel;
            this.scheme = context.Binding.Scheme;
            this.isClient = isClient;
            this.listenUri = TransportSecurityHelpers.GetListenUri(context.ListenUriBaseAddress, context.ListenUriRelativeAddress);

            SecurityCredentialsManager credentialProvider = context.BindingParameters.Find<SecurityCredentialsManager>();
            if (credentialProvider == null)
            {
                if (isClient)
                {
                    credentialProvider = ClientCredentials.CreateDefaultCredentials();
                }
                else
                {
                    credentialProvider = ServiceCredentials.CreateDefaultCredentials();
                }
            }


            this.securityTokenManager = credentialProvider.CreateSecurityTokenManager();
        }

        public string Scheme
        {
            get { return this.scheme; }
        }

        internal bool ExtractGroupsForWindowsAccounts
        {
            get
            {
                return this.extractGroupsForWindowsAccounts;
            }
        }

        public override EndpointIdentity Identity
        {
            get
            {
                // If the server credential is null, then we have not been opened yet and have no identity to expose.
                if (this.serverCredential != null)
                {
                    if (this.identity == null)
                    {
                        lock (ThisLock)
                        {
                            if (this.identity == null)
                            {
                                this.identity = SecurityUtils.CreateWindowsIdentity(this.serverCredential);
                            }
                        }
                    }
                }
                return this.identity;
            }
        }

        internal IdentityVerifier IdentityVerifier
        {
            get
            {
                return this.identityVerifier;
            }
        }

        public ProtectionLevel ProtectionLevel
        {
            get
            {
                return protectionLevel;
            }
        }

        NetworkCredential ServerCredential
        {
            get
            {
                return this.serverCredential;
            }
        }

        public override StreamUpgradeAcceptor CreateUpgradeAcceptor()
        {
            ThrowIfDisposedOrNotOpen();
            return new WindowsStreamSecurityUpgradeAcceptor(this);
        }

        public override StreamUpgradeInitiator CreateUpgradeInitiator(EndpointAddress remoteAddress, Uri via)
        {
            ThrowIfDisposedOrNotOpen();
            return new WindowsStreamSecurityUpgradeInitiator(this, remoteAddress, via);
        }

        protected override void OnAbort()
        {
        }

        protected override void OnClose(TimeSpan timeout)
        {
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            if (!isClient)
            {
                SecurityTokenRequirement sspiTokenRequirement = TransportSecurityHelpers.CreateSspiTokenRequirement(this.Scheme, this.listenUri);
                this.serverCredential =
                    TransportSecurityHelpers.GetSspiCredential(this.securityTokenManager, sspiTokenRequirement, timeout,
                    out this.extractGroupsForWindowsAccounts);
            }
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            OnOpen(timeout);
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnOpened()
        {
            base.OnOpened();

            if (this.identityVerifier == null)
            {
                this.identityVerifier = IdentityVerifier.CreateDefault();
            }

            if (this.serverCredential == null)
            {
                this.serverCredential = CredentialCache.DefaultNetworkCredentials;
            }
        }

        class WindowsStreamSecurityUpgradeAcceptor : StreamSecurityUpgradeAcceptorBase
        {
            WindowsStreamSecurityUpgradeProvider parent;
            SecurityMessageProperty clientSecurity;

            public WindowsStreamSecurityUpgradeAcceptor(WindowsStreamSecurityUpgradeProvider parent)
                : base(FramingUpgradeString.Negotiate)
            {
                this.parent = parent;
                this.clientSecurity = new SecurityMessageProperty();
            }

            protected override Stream OnAcceptUpgrade(Stream stream, out SecurityMessageProperty remoteSecurity)
            {
                // wrap stream
                NegotiateStream negotiateStream = new NegotiateStream(stream);

                // authenticate
                try
                {
                    if (TD.WindowsStreamSecurityOnAcceptUpgradeIsEnabled())
                    {
                        TD.WindowsStreamSecurityOnAcceptUpgrade(this.EventTraceActivity);
                    }

                    negotiateStream.AuthenticateAsServer(parent.ServerCredential, parent.ProtectionLevel,
                        TokenImpersonationLevel.Identification);
                }
                catch (AuthenticationException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(exception.Message,
                        exception));
                }
                catch (IOException ioException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(
                        SR.GetString(SR.NegotiationFailedIO, ioException.Message), ioException));
                }

                remoteSecurity = CreateClientSecurity(negotiateStream, parent.ExtractGroupsForWindowsAccounts);
                return negotiateStream;
            }

            protected override IAsyncResult OnBeginAcceptUpgrade(Stream stream, AsyncCallback callback, object state)
            {
                AcceptUpgradeAsyncResult result = new AcceptUpgradeAsyncResult(this, callback, state);
                result.Begin(stream);
                return result;
            }

            protected override Stream OnEndAcceptUpgrade(IAsyncResult result,
                out SecurityMessageProperty remoteSecurity)
            {
                return AcceptUpgradeAsyncResult.End(result, out remoteSecurity);
            }

            SecurityMessageProperty CreateClientSecurity(NegotiateStream negotiateStream,
                bool extractGroupsForWindowsAccounts)
            {
                WindowsIdentity remoteIdentity = (WindowsIdentity)negotiateStream.RemoteIdentity;
                SecurityUtils.ValidateAnonymityConstraint(remoteIdentity, false);
                WindowsSecurityTokenAuthenticator authenticator = new WindowsSecurityTokenAuthenticator(extractGroupsForWindowsAccounts);

                // When NegotiateStream returns a WindowsIdentity the AuthenticationType is passed in the constructor to WindowsIdentity
                // by it's internal NegoState class.  If this changes, then the call to remoteIdentity.AuthenticationType could fail if the 
                // current process token doesn't have sufficient priviledges.  It is a first class exception, and caught by the CLR
                // null is returned.
                SecurityToken token = new WindowsSecurityToken(remoteIdentity, SecurityUniqueId.Create().Value, remoteIdentity.AuthenticationType);
                ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = authenticator.ValidateToken(token);
                this.clientSecurity = new SecurityMessageProperty();
                this.clientSecurity.TransportToken = new SecurityTokenSpecification(token, authorizationPolicies);
                this.clientSecurity.ServiceSecurityContext = new ServiceSecurityContext(authorizationPolicies);
                return this.clientSecurity;
            }

            public override SecurityMessageProperty GetRemoteSecurity()
            {
                if (this.clientSecurity.TransportToken != null)
                {
                    return this.clientSecurity;
                }
                return base.GetRemoteSecurity();
            }

            class AcceptUpgradeAsyncResult : StreamSecurityUpgradeAcceptorAsyncResult
            {
                WindowsStreamSecurityUpgradeAcceptor acceptor;
                NegotiateStream negotiateStream;

                public AcceptUpgradeAsyncResult(WindowsStreamSecurityUpgradeAcceptor acceptor, AsyncCallback callback,
                    object state)
                    : base(callback, state)
                {
                    this.acceptor = acceptor;
                }

                protected override IAsyncResult OnBegin(Stream stream, AsyncCallback callback)
                {
                    this.negotiateStream = new NegotiateStream(stream);
                    return this.negotiateStream.BeginAuthenticateAsServer(this.acceptor.parent.ServerCredential,
                        this.acceptor.parent.ProtectionLevel, TokenImpersonationLevel.Identification, callback, this);
                }

                protected override Stream OnCompleteAuthenticateAsServer(IAsyncResult result)
                {
                    this.negotiateStream.EndAuthenticateAsServer(result);
                    return this.negotiateStream;
                }

                protected override SecurityMessageProperty ValidateCreateSecurity()
                {
                    return this.acceptor.CreateClientSecurity(this.negotiateStream, this.acceptor.parent.ExtractGroupsForWindowsAccounts);
                }
            }
        }

        class WindowsStreamSecurityUpgradeInitiator : StreamSecurityUpgradeInitiatorBase
        {
            WindowsStreamSecurityUpgradeProvider parent;
            IdentityVerifier identityVerifier;
            NetworkCredential credential;
            TokenImpersonationLevel impersonationLevel;
            SspiSecurityTokenProvider clientTokenProvider;
            bool allowNtlm;

            public WindowsStreamSecurityUpgradeInitiator(
                WindowsStreamSecurityUpgradeProvider parent, EndpointAddress remoteAddress, Uri via)
                : base(FramingUpgradeString.Negotiate, remoteAddress, via)
            {
                this.parent = parent;
                this.clientTokenProvider = TransportSecurityHelpers.GetSspiTokenProvider(
                    parent.securityTokenManager, remoteAddress, via, parent.Scheme, out this.identityVerifier);
            }

            IAsyncResult BaseBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return base.BeginOpen(timeout, callback, state);
            }

            void BaseEndOpen(IAsyncResult result)
            {
                base.EndOpen(result);
            }

            internal override IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new OpenAsyncResult(this, timeout, callback, state);
            }

            internal override void EndOpen(IAsyncResult result)
            {
                OpenAsyncResult.End(result);
            }

            internal override void Open(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                base.Open(timeoutHelper.RemainingTime());
                SecurityUtils.OpenTokenProviderIfRequired(this.clientTokenProvider, timeoutHelper.RemainingTime());
                this.credential = TransportSecurityHelpers.GetSspiCredential(this.clientTokenProvider, timeoutHelper.RemainingTime(),
                    out this.impersonationLevel, out this.allowNtlm);
            }

            IAsyncResult BaseBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return base.BeginClose(timeout, callback, state);
            }

            void BaseEndClose(IAsyncResult result)
            {
                base.EndClose(result);
            }

            internal override IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CloseAsyncResult(this, timeout, callback, state);
            }

            internal override void EndClose(IAsyncResult result)
            {
                CloseAsyncResult.End(result);
            }

            internal override void Close(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                base.Close(timeoutHelper.RemainingTime());
                SecurityUtils.CloseTokenProviderIfRequired(this.clientTokenProvider, timeoutHelper.RemainingTime());
            }

            protected override IAsyncResult OnBeginInitiateUpgrade(Stream stream, AsyncCallback callback, object state)
            {
                if (TD.WindowsStreamSecurityOnInitiateUpgradeIsEnabled())
                {
                    TD.WindowsStreamSecurityOnInitiateUpgrade();
                }

                InitiateUpgradeAsyncResult result = new InitiateUpgradeAsyncResult(this, callback, state);
                result.Begin(stream);
                return result;
            }

            protected override Stream OnEndInitiateUpgrade(IAsyncResult result,
                out SecurityMessageProperty remoteSecurity)
            {
                return InitiateUpgradeAsyncResult.End(result, out remoteSecurity);
            }

            static SecurityMessageProperty CreateServerSecurity(NegotiateStream negotiateStream)
            {
                GenericIdentity remoteIdentity = (GenericIdentity)negotiateStream.RemoteIdentity;
                string principalName = remoteIdentity.Name;
                if ((principalName != null) && (principalName.Length > 0))
                {
                    ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = SecurityUtils.CreatePrincipalNameAuthorizationPolicies(principalName);
                    SecurityMessageProperty result = new SecurityMessageProperty();
                    result.TransportToken = new SecurityTokenSpecification(null, authorizationPolicies);
                    result.ServiceSecurityContext = new ServiceSecurityContext(authorizationPolicies);
                    return result;
                }
                else
                {
                    return null;
                }
            }

            protected override Stream OnInitiateUpgrade(Stream stream,
                out SecurityMessageProperty remoteSecurity)
            {
                NegotiateStream negotiateStream;
                string targetName;
                EndpointIdentity identity;

                if (TD.WindowsStreamSecurityOnInitiateUpgradeIsEnabled())
                {
                    TD.WindowsStreamSecurityOnInitiateUpgrade();
                }

                // prepare
                this.InitiateUpgradePrepare(stream, out negotiateStream, out targetName, out identity);

                // authenticate
                try
                {
                    negotiateStream.AuthenticateAsClient(credential, targetName, parent.ProtectionLevel, impersonationLevel);
                }
                catch (AuthenticationException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(exception.Message,
                        exception));
                }
                catch (IOException ioException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(
                        SR.GetString(SR.NegotiationFailedIO, ioException.Message), ioException));
                }

                remoteSecurity = CreateServerSecurity(negotiateStream);
                this.ValidateMutualAuth(identity, negotiateStream, remoteSecurity, allowNtlm);

                return negotiateStream;
            }

            void InitiateUpgradePrepare(
                Stream stream,
                out NegotiateStream negotiateStream,
                out string targetName,
                out EndpointIdentity identity)
            {
                negotiateStream = new NegotiateStream(stream);

                targetName = string.Empty;
                identity = null;

                if (parent.IdentityVerifier.TryGetIdentity(this.RemoteAddress, this.Via, out identity))
                {
                    targetName = SecurityUtils.GetSpnFromIdentity(identity, this.RemoteAddress);
                }
                else
                {
                    targetName = SecurityUtils.GetSpnFromTarget(this.RemoteAddress);
                }
            }

            void ValidateMutualAuth(EndpointIdentity expectedIdentity, NegotiateStream negotiateStream,
                SecurityMessageProperty remoteSecurity, bool allowNtlm)
            {
                if (negotiateStream.IsMutuallyAuthenticated)
                {
                    if (expectedIdentity != null)
                    {
                        if (!parent.IdentityVerifier.CheckAccess(expectedIdentity,
                            remoteSecurity.ServiceSecurityContext.AuthorizationContext))
                        {
                            string primaryIdentity = SecurityUtils.GetIdentityNamesFromContext(remoteSecurity.ServiceSecurityContext.AuthorizationContext);
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(
                                SR.RemoteIdentityFailedVerification, primaryIdentity)));
                        }
                    }
                }
                else if (!allowNtlm)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(
                        SR.StreamMutualAuthNotSatisfied)));
                }
            }

            class InitiateUpgradeAsyncResult : StreamSecurityUpgradeInitiatorAsyncResult
            {
                EndpointIdentity expectedIdentity;
                WindowsStreamSecurityUpgradeInitiator initiator;
                NegotiateStream negotiateStream;

                public InitiateUpgradeAsyncResult(WindowsStreamSecurityUpgradeInitiator initiator,
                    AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.initiator = initiator;
                }

                protected override IAsyncResult OnBeginAuthenticateAsClient(Stream stream, AsyncCallback callback)
                {
                    string targetName;
                    this.initiator.InitiateUpgradePrepare(stream, out this.negotiateStream, out targetName,
                        out this.expectedIdentity);

                    return this.negotiateStream.BeginAuthenticateAsClient(this.initiator.credential, targetName,
                            this.initiator.parent.ProtectionLevel, this.initiator.impersonationLevel, callback, this);
                }

                protected override Stream OnCompleteAuthenticateAsClient(IAsyncResult result)
                {
                    this.negotiateStream.EndAuthenticateAsClient(result);
                    return this.negotiateStream;
                }

                protected override SecurityMessageProperty ValidateCreateSecurity()
                {
                    SecurityMessageProperty remoteSecurity = CreateServerSecurity(negotiateStream);
                    this.initiator.ValidateMutualAuth(this.expectedIdentity, this.negotiateStream,
                        remoteSecurity, this.initiator.allowNtlm);
                    return remoteSecurity;
                }
            }

            class OpenAsyncResult : AsyncResult
            {
                WindowsStreamSecurityUpgradeInitiator parent;
                TimeoutHelper timeoutHelper;
                AsyncCallback onBaseOpen;
                AsyncCallback onOpenTokenProvider;
                AsyncCallback onGetSspiCredential;

                public OpenAsyncResult(WindowsStreamSecurityUpgradeInitiator parent, TimeSpan timeout,
                    AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.parent = parent;
                    TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

                    // since we're at channel.Open and not per-message, minimize our statics overhead and leverage GC for our callback
                    this.onBaseOpen = Fx.ThunkCallback(new AsyncCallback(OnBaseOpen));
                    this.onGetSspiCredential = Fx.ThunkCallback(new AsyncCallback(OnGetSspiCredential));
                    this.onOpenTokenProvider = Fx.ThunkCallback(new AsyncCallback(OnOpenTokenProvider));
                    IAsyncResult result = parent.BaseBeginOpen(timeoutHelper.RemainingTime(), onBaseOpen, this);

                    if (!result.CompletedSynchronously)
                    {
                        return;
                    }

                    if (HandleBaseOpenComplete(result))
                    {
                        base.Complete(true);
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<OpenAsyncResult>(result);
                }

                bool HandleBaseOpenComplete(IAsyncResult result)
                {
                    parent.BaseEndOpen(result);
                    IAsyncResult openTokenProviderResult = SecurityUtils.BeginOpenTokenProviderIfRequired(
                        parent.clientTokenProvider, timeoutHelper.RemainingTime(), onOpenTokenProvider, this);

                    if (!openTokenProviderResult.CompletedSynchronously)
                    {
                        return false;
                    }

                    return HandleOpenTokenProviderComplete(openTokenProviderResult);
                }

                bool HandleOpenTokenProviderComplete(IAsyncResult result)
                {
                    SecurityUtils.EndOpenTokenProviderIfRequired(result);
                    IAsyncResult getCredentialResult = TransportSecurityHelpers.BeginGetSspiCredential(
                        parent.clientTokenProvider, timeoutHelper.RemainingTime(), onGetSspiCredential, this);

                    if (!getCredentialResult.CompletedSynchronously)
                    {
                        return false;
                    }

                    return HandleGetSspiCredentialComplete(getCredentialResult);
                }

                bool HandleGetSspiCredentialComplete(IAsyncResult result)
                {
                    parent.credential = TransportSecurityHelpers.EndGetSspiCredential(result,
                        out parent.impersonationLevel, out parent.allowNtlm);
                    return true;
                }

                void OnBaseOpen(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }

                    Exception completionException = null;
                    bool completeSelf = false;
                    try
                    {
                        completeSelf = this.HandleBaseOpenComplete(result);
                    }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completeSelf = true;
                        completionException = e;
                    }

                    if (completeSelf)
                    {
                        base.Complete(false, completionException);
                    }
                }

                void OnOpenTokenProvider(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }

                    Exception completionException = null;
                    bool completeSelf = false;
                    try
                    {
                        completeSelf = this.HandleOpenTokenProviderComplete(result);
                    }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completeSelf = true;
                        completionException = e;
                    }

                    if (completeSelf)
                    {
                        base.Complete(false, completionException);
                    }
                }

                void OnGetSspiCredential(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }

                    Exception completionException = null;
                    bool completeSelf = false;
                    try
                    {
                        completeSelf = this.HandleGetSspiCredentialComplete(result);
                    }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completeSelf = true;
                        completionException = e;
                    }

                    if (completeSelf)
                    {
                        base.Complete(false, completionException);
                    }
                }
            }

            class CloseAsyncResult : AsyncResult
            {
                WindowsStreamSecurityUpgradeInitiator parent;
                TimeoutHelper timeoutHelper;
                AsyncCallback onBaseClose;
                AsyncCallback onCloseTokenProvider;

                public CloseAsyncResult(WindowsStreamSecurityUpgradeInitiator parent, TimeSpan timeout,
                    AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.parent = parent;
                    this.timeoutHelper = new TimeoutHelper(timeout);

                    // since we're at channel.Open and not per-message, minimize our statics overhead and leverage GC for our callback
                    this.onBaseClose = Fx.ThunkCallback(new AsyncCallback(OnBaseClose));
                    this.onCloseTokenProvider = Fx.ThunkCallback(new AsyncCallback(OnCloseTokenProvider));
                    IAsyncResult result = parent.BaseBeginClose(timeoutHelper.RemainingTime(), onBaseClose, this);

                    if (!result.CompletedSynchronously)
                    {
                        return;
                    }

                    if (HandleBaseCloseComplete(result))
                    {
                        base.Complete(true);
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<CloseAsyncResult>(result);
                }

                bool HandleBaseCloseComplete(IAsyncResult result)
                {
                    parent.BaseEndClose(result);
                    IAsyncResult closeTokenProviderResult = SecurityUtils.BeginCloseTokenProviderIfRequired(
                        parent.clientTokenProvider, timeoutHelper.RemainingTime(), onCloseTokenProvider, this);

                    if (!closeTokenProviderResult.CompletedSynchronously)
                    {
                        return false;
                    }

                    SecurityUtils.EndCloseTokenProviderIfRequired(closeTokenProviderResult);
                    return true;
                }

                void OnBaseClose(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }

                    Exception completionException = null;
                    bool completeSelf = false;
                    try
                    {
                        completeSelf = this.HandleBaseCloseComplete(result);
                    }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completeSelf = true;
                        completionException = e;
                    }

                    if (completeSelf)
                    {
                        base.Complete(false, completionException);
                    }
                }

                void OnCloseTokenProvider(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }

                    Exception completionException = null;
                    try
                    {
                        SecurityUtils.EndCloseTokenProviderIfRequired(result);
                    }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completionException = e;
                    }

                    base.Complete(false, completionException);
                }
            }
        }
    }
}
