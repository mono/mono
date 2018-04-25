//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    class SecurityTraceRecord : TraceRecord
    {
        String traceName;
        internal SecurityTraceRecord(String traceName)
        {
            if (String.IsNullOrEmpty(traceName))
                this.traceName = "Empty";
            else
                this.traceName = traceName;
        }

        internal override string EventId { get { return BuildEventId(traceName); } }
    }

    internal static class SecurityTraceRecordHelper
    {
        internal static void TraceRemovedCachedServiceToken<T>(IssuanceTokenProviderBase<T> provider, SecurityToken serviceToken)
            where T : IssuanceTokenProviderState
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.IssuanceTokenProviderRemovedCachedToken, SR.GetString(SR.TraceCodeIssuanceTokenProviderRemovedCachedToken), new IssuanceProviderTraceRecord<T>(provider, serviceToken));
            }
        }

        internal static void TraceUsingCachedServiceToken<T>(IssuanceTokenProviderBase<T> provider, SecurityToken serviceToken, EndpointAddress target)
             where T : IssuanceTokenProviderState
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.IssuanceTokenProviderUsingCachedToken, SR.GetString(SR.TraceCodeIssuanceTokenProviderUsingCachedToken), new IssuanceProviderTraceRecord<T>(provider, serviceToken, target));
            }
        }

        internal static void TraceBeginSecurityNegotiation<T>(IssuanceTokenProviderBase<T> provider, EndpointAddress target)
             where T : IssuanceTokenProviderState
        {

            if (TD.SecurityNegotiationStartIsEnabled())
            {
                TD.SecurityNegotiationStart(provider.EventTraceActivity);
            }

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.IssuanceTokenProviderBeginSecurityNegotiation, SR.GetString(SR.TraceCodeIssuanceTokenProviderBeginSecurityNegotiation), new IssuanceProviderTraceRecord<T>(provider, target));
            }
        }

        internal static void TraceEndSecurityNegotiation<T>(IssuanceTokenProviderBase<T> provider, SecurityToken serviceToken, EndpointAddress target)
             where T : IssuanceTokenProviderState
        {
            if (TD.SecurityNegotiationStopIsEnabled())
            {
                TD.SecurityNegotiationStop(provider.EventTraceActivity);
            }

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.IssuanceTokenProviderEndSecurityNegotiation, SR.GetString(SR.TraceCodeIssuanceTokenProviderEndSecurityNegotiation), new IssuanceProviderTraceRecord<T>(provider, serviceToken, target));
            }
        }

        internal static void TraceRedirectApplied<T>(IssuanceTokenProviderBase<T> provider, EndpointAddress newTarget, EndpointAddress oldTarget)
             where T : IssuanceTokenProviderState
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.IssuanceTokenProviderRedirectApplied, SR.GetString(SR.TraceCodeIssuanceTokenProviderRedirectApplied), new IssuanceProviderTraceRecord<T>(provider, newTarget, oldTarget));
            }
        }

        internal static void TraceClientServiceTokenCacheFull<T>(IssuanceTokenProviderBase<T> provider, int cacheSize)
             where T : IssuanceTokenProviderState
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.IssuanceTokenProviderServiceTokenCacheFull, SR.GetString(SR.TraceCodeIssuanceTokenProviderServiceTokenCacheFull), new IssuanceProviderTraceRecord<T>(provider, cacheSize));
            }
        }

        internal static void TraceClientSpnego(WindowsSspiNegotiation windowsNegotiation)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SpnegoClientNegotiationCompleted, SR.GetString(SR.TraceCodeSpnegoClientNegotiationCompleted), new WindowsSspiNegotiationTraceRecord(windowsNegotiation));
            }
        }

        internal static void TraceServiceSpnego(WindowsSspiNegotiation windowsNegotiation)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SpnegoServiceNegotiationCompleted, SR.GetString(SR.TraceCodeSpnegoServiceNegotiationCompleted), new WindowsSspiNegotiationTraceRecord(windowsNegotiation));
            }
        }

        internal static void TraceClientOutgoingSpnego(WindowsSspiNegotiation windowsNegotiation)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SpnegoClientNegotiation, SR.GetString(SR.TraceCodeSpnegoClientNegotiation), new WindowsSspiNegotiationTraceRecord(windowsNegotiation));
            }
        }

        internal static void TraceServiceOutgoingSpnego(WindowsSspiNegotiation windowsNegotiation)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SpnegoServiceNegotiation, SR.GetString(SR.TraceCodeSpnegoServiceNegotiation), new WindowsSspiNegotiationTraceRecord(windowsNegotiation));
            }
        }

        internal static void TraceNegotiationTokenAuthenticatorAttached<T>(NegotiationTokenAuthenticator<T> authenticator, IChannelListener transportChannelListener)
            where T : NegotiationTokenAuthenticatorState
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.NegotiationAuthenticatorAttached, SR.GetString(SR.TraceCodeNegotiationAuthenticatorAttached), new NegotiationAuthenticatorTraceRecord<T>(authenticator, transportChannelListener));
            }
        }

        internal static void TraceServiceSecurityNegotiationCompleted<T>(Message message, NegotiationTokenAuthenticator<T> authenticator, SecurityContextSecurityToken serviceToken)
            where T : NegotiationTokenAuthenticatorState
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.ServiceSecurityNegotiationCompleted, SR.GetString(SR.TraceCodeServiceSecurityNegotiationCompleted),
                    new NegotiationAuthenticatorTraceRecord<T>(authenticator, serviceToken));
            }

            if (TD.ServiceSecurityNegotiationCompletedIsEnabled())
            {
                EventTraceActivity activity = EventTraceActivityHelper.TryExtractActivity(message);
                TD.ServiceSecurityNegotiationCompleted(activity);
            }
        }

        internal static void TraceServiceSecurityNegotiationFailure<T>(EventTraceActivity eventTraceActivity, NegotiationTokenAuthenticator<T> authenticator, Exception e)
            where T : NegotiationTokenAuthenticatorState
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.SecurityNegotiationProcessingFailure, SR.GetString(SR.TraceCodeSecurityNegotiationProcessingFailure), new NegotiationAuthenticatorTraceRecord<T>(authenticator, e));
            }

            if (TD.SecurityNegotiationProcessingFailureIsEnabled())
            {
                TD.SecurityNegotiationProcessingFailure(eventTraceActivity);
            }
        }


        internal static void TraceSecurityContextTokenCacheFull(int capacity, int pruningAmount)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecurityContextTokenCacheFull, SR.GetString(SR.TraceCodeSecurityContextTokenCacheFull),
                    new SecurityContextTokenCacheTraceRecord(capacity, pruningAmount));
            }
        }

        internal static void TraceIdentityVerificationSuccess(EventTraceActivity eventTraceActivity, EndpointIdentity identity, Claim claim, Type identityVerifier)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecurityIdentityVerificationSuccess, SR.GetString(SR.TraceCodeSecurityIdentityVerificationSuccess), new IdentityVerificationSuccessTraceRecord(identity, claim, identityVerifier));

            if (TD.SecurityIdentityVerificationSuccessIsEnabled())
            {
                TD.SecurityIdentityVerificationSuccess(eventTraceActivity);
            }
        }

        internal static void TraceIdentityVerificationFailure(EndpointIdentity identity, AuthorizationContext authContext, Type identityVerifier)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecurityIdentityVerificationFailure, SR.GetString(SR.TraceCodeSecurityIdentityVerificationFailure), new IdentityVerificationFailureTraceRecord(identity, authContext, identityVerifier));
        }

        internal static void TraceIdentityDeterminationSuccess(EndpointAddress epr, EndpointIdentity identity, Type identityVerifier)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecurityIdentityDeterminationSuccess, SR.GetString(SR.TraceCodeSecurityIdentityDeterminationSuccess), new IdentityDeterminationSuccessTraceRecord(epr, identity, identityVerifier));
        }

        internal static void TraceIdentityDeterminationFailure(EndpointAddress epr, Type identityVerifier)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecurityIdentityDeterminationFailure, SR.GetString(SR.TraceCodeSecurityIdentityDeterminationFailure), new IdentityDeterminationFailureTraceRecord(epr, identityVerifier));
        }

        internal static void TraceIdentityHostNameNormalizationFailure(EndpointAddress epr, Type identityVerifier, Exception e)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecurityIdentityHostNameNormalizationFailure, SR.GetString(SR.TraceCodeSecurityIdentityHostNameNormalizationFailure), new IdentityHostNameNormalizationFailureTraceRecord(epr, identityVerifier, e));
        }

        internal static void TraceExportChannelBindingEntry()
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.ExportSecurityChannelBindingEntry, SR.GetString(SR.TraceCodeExportSecurityChannelBindingEntry), (object)null);
        }

        internal static void TraceExportChannelBindingExit()
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.ExportSecurityChannelBindingExit, SR.GetString(SR.TraceCodeExportSecurityChannelBindingExit));
        }

        internal static void TraceImportChannelBindingEntry()
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.ImportSecurityChannelBindingEntry, SR.GetString(SR.TraceCodeImportSecurityChannelBindingEntry), (object)null);
        }

        internal static void TraceImportChannelBindingExit()
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.ImportSecurityChannelBindingExit, SR.GetString(SR.TraceCodeImportSecurityChannelBindingExit));
        }

        internal static void TraceTokenProviderOpened(EventTraceActivity eventTraceActivity, SecurityTokenProvider provider)
        {
            if (TD.SecurityTokenProviderOpenedIsEnabled())
            {
                TD.SecurityTokenProviderOpened(eventTraceActivity);
            }

            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecurityTokenProviderOpened, SR.GetString(SR.TraceCodeSecurityTokenProviderOpened), new TokenProviderTraceRecord(provider));
        }

        internal static void TraceTokenProviderClosed(SecurityTokenProvider provider)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecurityTokenProviderClosed, SR.GetString(SR.TraceCodeSecurityTokenProviderClosed), new TokenProviderTraceRecord(provider));
        }

        internal static void TraceTokenAuthenticatorOpened(SecurityTokenAuthenticator authenticator)
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
                TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.SecurityTokenAuthenticatorOpened, SR.GetString(SR.TraceCodeSecurityTokenAuthenticatorOpened), new TokenAuthenticatorTraceRecord(authenticator));
        }

        internal static void TraceTokenAuthenticatorClosed(SecurityTokenAuthenticator authenticator)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecurityTokenAuthenticatorClosed, SR.GetString(SR.TraceCodeSecurityTokenAuthenticatorClosed), new TokenAuthenticatorTraceRecord(authenticator));
        }

        internal static void TraceOutgoingMessageSecured(SecurityProtocol binding, Message message)
        {
            if (TD.OutgoingMessageSecuredIsEnabled())
            {                
                EventTraceActivity eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(message);
                TD.OutgoingMessageSecured(eventTraceActivity);
            }

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecurityBindingOutgoingMessageSecured,
                    SR.GetString(SR.TraceCodeSecurityBindingOutgoingMessageSecured), new MessageSecurityTraceRecord(binding, message), null, null, message);
            }
        }

        internal static void TraceIncomingMessageVerified(SecurityProtocol binding, Message message)
        {
            if (TD.IncomingMessageVerifiedIsEnabled())
            {                
                EventTraceActivity eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(message);
                TD.IncomingMessageVerified(eventTraceActivity);
            }

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecurityBindingIncomingMessageVerified,
                    SR.GetString(SR.TraceCodeSecurityBindingIncomingMessageVerified), new MessageSecurityTraceRecord(binding, message), null, null, message);
            }
        }

        internal static void TraceSecureOutgoingMessageFailure(SecurityProtocol binding, Message message)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.SecurityBindingSecureOutgoingMessageFailure,
                    SR.GetString(SR.TraceCodeSecurityBindingSecureOutgoingMessageFailure), new MessageSecurityTraceRecord(binding, message), null, null, message);
            }
        }

        internal static void TraceVerifyIncomingMessageFailure(SecurityProtocol binding, Message message)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.SecurityBindingVerifyIncomingMessageFailure,
                    SR.GetString(SR.TraceCodeSecurityBindingVerifyIncomingMessageFailure), new MessageSecurityTraceRecord(binding, message), null, null, message);
            }
        }

        internal static void TraceSpnToSidMappingFailure(string spn, Exception e)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
                TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.SecuritySpnToSidMappingFailure, SR.GetString(SR.TraceCodeSecuritySpnToSidMappingFailure), new SpnToSidMappingTraceRecord(spn, e));
        }

        internal static void TraceSessionRedirectApplied(EndpointAddress previousTarget, EndpointAddress newTarget, GenericXmlSecurityToken sessionToken)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecuritySessionRedirectApplied, SR.GetString(SR.TraceCodeSecuritySessionRedirectApplied), new SessionRedirectAppliedTraceRecord(previousTarget, newTarget, sessionToken));
        }

        internal static void TraceCloseMessageSent(SecurityToken sessionToken, EndpointAddress remoteTarget)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecurityClientSessionCloseSent, SR.GetString(SR.TraceCodeSecurityClientSessionCloseSent), new ClientSessionTraceRecord(sessionToken, null, remoteTarget));
        }

        internal static void TraceCloseResponseMessageSent(SecurityToken sessionToken, EndpointAddress remoteTarget)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecurityClientSessionCloseResponseSent, SR.GetString(SR.TraceCodeSecurityClientSessionCloseResponseSent), new ClientSessionTraceRecord(sessionToken, null, remoteTarget));
        }

        internal static void TraceCloseMessageReceived(SecurityToken sessionToken, EndpointAddress remoteTarget)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecurityClientSessionCloseMessageReceived, SR.GetString(SR.TraceCodeSecurityClientSessionCloseMessageReceived), new ClientSessionTraceRecord(sessionToken, null, remoteTarget));
        }

        internal static void TraceSessionKeyRenewalFault(SecurityToken sessionToken, EndpointAddress remoteTarget)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
                TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.SecuritySessionKeyRenewalFaultReceived, SR.GetString(SR.TraceCodeSecuritySessionKeyRenewalFaultReceived), new ClientSessionTraceRecord(sessionToken, null, remoteTarget));
        }

        internal static void TraceRemoteSessionAbortedFault(SecurityToken sessionToken, EndpointAddress remoteTarget)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
                TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.SecuritySessionAbortedFaultReceived, SR.GetString(SR.TraceCodeSecuritySessionAbortedFaultReceived), new ClientSessionTraceRecord(sessionToken, null, remoteTarget));
        }

        internal static void TraceCloseResponseReceived(SecurityToken sessionToken, EndpointAddress remoteTarget)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecuritySessionClosedResponseReceived, SR.GetString(SR.TraceCodeSecuritySessionClosedResponseReceived), new ClientSessionTraceRecord(sessionToken, null, remoteTarget));
        }

        internal static void TracePreviousSessionKeyDiscarded(SecurityToken previousSessionToken, SecurityToken currentSessionToken, EndpointAddress remoteAddress)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecurityClientSessionPreviousKeyDiscarded, SR.GetString(SR.TraceCodeSecurityClientSessionPreviousKeyDiscarded), new ClientSessionTraceRecord(currentSessionToken, previousSessionToken, remoteAddress));
        }

        internal static void TraceSessionKeyRenewed(SecurityToken newSessionToken, SecurityToken currentSessionToken, EndpointAddress remoteAddress)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecurityClientSessionKeyRenewed, SR.GetString(SR.TraceCodeSecurityClientSessionKeyRenewed), new ClientSessionTraceRecord(newSessionToken, currentSessionToken, remoteAddress));
        }

        internal static void TracePendingSessionAdded(UniqueId sessionId, Uri listenAddress)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecurityPendingServerSessionAdded, SR.GetString(SR.TraceCodeSecurityPendingServerSessionAdded), new ServerSessionTraceRecord(sessionId, listenAddress));
        }

        internal static void TracePendingSessionClosed(UniqueId sessionId, Uri listenAddress)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
                TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.SecurityPendingServerSessionClosed, SR.GetString(SR.TraceCodeSecurityPendingServerSessionClosed), new ServerSessionTraceRecord(sessionId, listenAddress));
        }

        internal static void TracePendingSessionActivated(UniqueId sessionId, Uri listenAddress)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecurityPendingServerSessionActivated, SR.GetString(SR.TraceCodeSecurityPendingServerSessionActivated), new ServerSessionTraceRecord(sessionId, listenAddress));
        }

        internal static void TraceActiveSessionRemoved(UniqueId sessionId, Uri listenAddress)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecurityActiveServerSessionRemoved, SR.GetString(SR.TraceCodeSecurityActiveServerSessionRemoved), new ServerSessionTraceRecord(sessionId, listenAddress));
        }

        internal static void TraceNewServerSessionKeyIssued(SecurityContextSecurityToken newToken, SecurityContextSecurityToken supportingToken, Uri listenAddress)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecurityNewServerSessionKeyIssued, SR.GetString(SR.TraceCodeSecurityNewServerSessionKeyIssued), new ServerSessionTraceRecord(newToken, supportingToken, listenAddress));
        }

        internal static void TraceInactiveSessionFaulted(SecurityContextSecurityToken sessionToken, Uri listenAddress)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
                TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.SecurityInactiveSessionFaulted, SR.GetString(SR.TraceCodeSecurityInactiveSessionFaulted), new ServerSessionTraceRecord(sessionToken, (SecurityContextSecurityToken)null, listenAddress));
        }

        internal static void TraceServerSessionKeyUpdated(SecurityContextSecurityToken sessionToken, Uri listenAddress)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecurityServerSessionKeyUpdated, SR.GetString(SR.TraceCodeSecurityServerSessionKeyUpdated), new ServerSessionTraceRecord(sessionToken, (SecurityContextSecurityToken)null, listenAddress));
        }

        internal static void TraceServerSessionCloseReceived(SecurityContextSecurityToken sessionToken, Uri listenAddress)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecurityServerSessionCloseReceived, SR.GetString(SR.TraceCodeSecurityServerSessionCloseReceived), new ServerSessionTraceRecord(sessionToken, (SecurityContextSecurityToken)null, listenAddress));
        }

        internal static void TraceServerSessionCloseResponseReceived(SecurityContextSecurityToken sessionToken, Uri listenAddress)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecurityServerSessionCloseResponseReceived, SR.GetString(SR.TraceCodeSecurityServerSessionCloseResponseReceived), new ServerSessionTraceRecord(sessionToken, (SecurityContextSecurityToken)null, listenAddress));
        }

        internal static void TraceSessionRenewalFaultSent(SecurityContextSecurityToken sessionToken, Uri listenAddress, Message message)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.SecurityServerSessionRenewalFaultSent,
                    SR.GetString(SR.TraceCodeSecurityServerSessionRenewalFaultSent), new ServerSessionTraceRecord(sessionToken, message, listenAddress), null, null, message);
            }
        }

        internal static void TraceSessionAbortedFaultSent(SecurityContextSecurityToken sessionToken, Uri listenAddress)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
                TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.SecurityServerSessionAbortedFaultSent, SR.GetString(SR.TraceCodeSecurityServerSessionAbortedFaultSent), new ServerSessionTraceRecord(sessionToken, (SecurityContextSecurityToken)null, listenAddress));
        }

        internal static void TraceSessionClosedResponseSent(SecurityContextSecurityToken sessionToken, Uri listenAddress)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecuritySessionCloseResponseSent, SR.GetString(SR.TraceCodeSecuritySessionCloseResponseSent), new ServerSessionTraceRecord(sessionToken, (SecurityContextSecurityToken)null, listenAddress));
        }

        internal static void TraceSessionClosedSent(SecurityContextSecurityToken sessionToken, Uri listenAddress)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecuritySessionServerCloseSent, SR.GetString(SR.TraceCodeSecuritySessionServerCloseSent), new ServerSessionTraceRecord(sessionToken, (SecurityContextSecurityToken)null, listenAddress));
        }

        internal static void TraceRenewFaultSendFailure(SecurityContextSecurityToken sessionToken, Uri listenAddress, Exception e)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
                TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.SecuritySessionRenewFaultSendFailure, SR.GetString(SR.TraceCodeSecuritySessionRenewFaultSendFailure), new ServerSessionTraceRecord(sessionToken, listenAddress), e);
        }

        internal static void TraceSessionAbortedFaultSendFailure(SecurityContextSecurityToken sessionToken, Uri listenAddress, Exception e)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
                TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.SecuritySessionAbortedFaultSendFailure, SR.GetString(SR.TraceCodeSecuritySessionAbortedFaultSendFailure), new ServerSessionTraceRecord(sessionToken, listenAddress), e);
        }

        internal static void TraceSessionClosedResponseSendFailure(SecurityContextSecurityToken sessionToken, Uri listenAddress, Exception e)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
                TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.SecuritySessionClosedResponseSendFailure, SR.GetString(SR.TraceCodeSecuritySessionClosedResponseSendFailure), new ServerSessionTraceRecord(sessionToken, listenAddress), e);
        }

        internal static void TraceSessionCloseSendFailure(SecurityContextSecurityToken sessionToken, Uri listenAddress, Exception e)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
                TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.SecuritySessionServerCloseSendFailure, SR.GetString(SR.TraceCodeSecuritySessionServerCloseSendFailure), new ServerSessionTraceRecord(sessionToken, listenAddress), e);
        }

        internal static void TraceBeginSecuritySessionOperation(SecuritySessionOperation operation, EndpointAddress target, SecurityToken currentToken)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecuritySessionRequestorStartOperation, SR.GetString(SR.TraceCodeSecuritySessionRequestorStartOperation), new SessionRequestorTraceRecord(operation, currentToken, (GenericXmlSecurityToken)null, target));
        }

        internal static void TraceSecuritySessionOperationSuccess(SecuritySessionOperation operation, EndpointAddress target, SecurityToken currentToken, SecurityToken issuedToken)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecuritySessionRequestorOperationSuccess, SR.GetString(SR.TraceCodeSecuritySessionRequestorOperationSuccess), new SessionRequestorTraceRecord(operation, currentToken, issuedToken, target));
        }

        internal static void TraceSecuritySessionOperationFailure(SecuritySessionOperation operation, EndpointAddress target, SecurityToken currentToken, Exception e)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
                TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.SecuritySessionRequestorOperationFailure, SR.GetString(SR.TraceCodeSecuritySessionRequestorOperationFailure), new SessionRequestorTraceRecord(operation, currentToken, e, target));
        }

        internal static void TraceServerSessionOperationException(SecuritySessionOperation operation, Exception e, Uri listenAddress)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
                TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.SecuritySessionResponderOperationFailure, SR.GetString(SR.TraceCodeSecuritySessionResponderOperationFailure), new SessionResponderTraceRecord(operation, e, listenAddress));
        }

        internal static void TraceImpersonationSucceeded(EventTraceActivity eventTraceActivity, DispatchOperationRuntime operation)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.SecurityImpersonationSuccess, SR.GetString(SR.TraceCodeSecurityImpersonationSuccess), new ImpersonationTraceRecord(operation));

            if (TD.SecurityImpersonationSuccessIsEnabled())
            {
                TD.SecurityImpersonationSuccess(eventTraceActivity);
            }

        }

        internal static void TraceImpersonationFailed(EventTraceActivity eventTraceActivity, DispatchOperationRuntime operation, Exception e)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
                TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.SecurityImpersonationFailure, SR.GetString(SR.TraceCodeSecurityImpersonationFailure), new ImpersonationTraceRecord(operation), e);

            if (TD.SecurityImpersonationFailureIsEnabled())
            {
                TD.SecurityImpersonationFailure(eventTraceActivity);
            }
        }

        static void WritePossibleGenericXmlToken(XmlWriter writer, string startElement, SecurityToken token)
        {
            if (writer == null)
                return;

            writer.WriteStartElement(startElement);
            GenericXmlSecurityToken gxt = token as GenericXmlSecurityToken;
            if (gxt != null)
            {
                WriteGenericXmlToken(writer, gxt);
            }
            else
            {
                if (token != null)
                    writer.WriteElementString("TokenType", token.GetType().ToString());
            }
            writer.WriteEndElement();
        }

        static void WriteGenericXmlToken(XmlWriter xml, SecurityToken sessiontoken)
        {
            if (xml == null || sessiontoken == null)
                return;
            xml.WriteElementString("SessionTokenType", sessiontoken.GetType().ToString());
            xml.WriteElementString("ValidFrom", XmlConvert.ToString(sessiontoken.ValidFrom, XmlDateTimeSerializationMode.Utc));
            xml.WriteElementString("ValidTo", XmlConvert.ToString(sessiontoken.ValidTo, XmlDateTimeSerializationMode.Utc));
            GenericXmlSecurityToken token = sessiontoken as GenericXmlSecurityToken;
            if (token != null)
            {
                if (token.InternalTokenReference != null)
                {
                    xml.WriteElementString("InternalTokenReference", token.InternalTokenReference.ToString());
                }
                if (token.ExternalTokenReference != null)
                {
                    xml.WriteElementString("ExternalTokenReference", token.ExternalTokenReference.ToString());
                }
                xml.WriteElementString("IssuedTokenElementName", token.TokenXml.LocalName);
                xml.WriteElementString("IssuedTokenElementNamespace", token.TokenXml.NamespaceURI);
            }
        }

        static void WriteSecurityContextToken(XmlWriter xml, SecurityContextSecurityToken token)
        {

            xml.WriteElementString("ContextId", token.ContextId.ToString());
            if (token.KeyGeneration != null)
            {
                xml.WriteElementString("KeyGeneration", token.KeyGeneration.ToString());
            }
        }

        static internal void WriteClaim(XmlWriter xml, Claim claim)
        {
            if (xml == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xml");

            if (claim != null)
            {
                xml.WriteStartElement("Claim");

                if (null != DiagnosticUtility.DiagnosticTrace
                    && null != DiagnosticUtility.DiagnosticTrace.TraceSource
                    && DiagnosticUtility.DiagnosticTrace.ShouldLogPii)
                {
                    //
                    // ClaimType
                    //
                    xml.WriteElementString("ClaimType", claim.ClaimType);

                    //
                    // Right
                    //
                    xml.WriteElementString("Right", claim.Right);

                    //
                    // Resource object type: most of time, it is a System.String
                    //
                    if (claim.Resource != null)
                        xml.WriteElementString("ResourceType", claim.Resource.GetType().ToString());
                    else
                        xml.WriteElementString("Resource", "null");
                }
                else
                {
                    xml.WriteString(claim.GetType().AssemblyQualifiedName);
                }


                xml.WriteEndElement();
            }
        }

        class SessionResponderTraceRecord : SecurityTraceRecord
        {
            SecuritySessionOperation operation;
            Exception e;
            Uri listenAddress;

            public SessionResponderTraceRecord(SecuritySessionOperation operation, Exception e, Uri listenAddress)
                : base("SecuritySession")
            {
                this.operation = operation;
                this.e = e;
                this.listenAddress = listenAddress;
            }

            internal override void WriteTo(XmlWriter xml)
            {
                if (xml == null)
                    return;

                xml.WriteElementString("Operation", this.operation.ToString());

                if (this.e != null)
                {
                    xml.WriteElementString("Exception", e.ToString());
                }

                if (this.listenAddress != null)
                    xml.WriteElementString("ListenAddress", this.listenAddress.ToString());
            }
        }

        class SessionRequestorTraceRecord : SecurityTraceRecord
        {
            SecuritySessionOperation operation;
            SecurityToken currentToken;
            SecurityToken issuedToken;
            EndpointAddress target;
            Exception e;

            public SessionRequestorTraceRecord(SecuritySessionOperation operation, SecurityToken currentToken, SecurityToken issuedToken, EndpointAddress target)
                : base("SecuritySession")
            {
                this.operation = operation;
                this.currentToken = currentToken;
                this.issuedToken = issuedToken;
                this.target = target;
            }

            public SessionRequestorTraceRecord(SecuritySessionOperation operation, SecurityToken currentToken, Exception e, EndpointAddress target)
                : base("SecuritySession")
            {
                this.operation = operation;
                this.currentToken = currentToken;
                this.e = e;
                this.target = target;
            }

            internal override void WriteTo(XmlWriter xml)
            {
                if (xml == null)
                    return;

                xml.WriteElementString("Operation", this.operation.ToString());

                if (this.currentToken != null)
                {
                    WritePossibleGenericXmlToken(xml, "SupportingToken", this.currentToken);
                }
                if (this.issuedToken != null)
                {
                    WritePossibleGenericXmlToken(xml, "IssuedToken", this.issuedToken);
                }
                if (this.e != null)
                {
                    xml.WriteElementString("Exception", e.ToString());
                }
                if (this.target != null)
                {
                    xml.WriteElementString("RemoteAddress", this.target.ToString());
                }
            }
        }

        class ServerSessionTraceRecord : SecurityTraceRecord
        {
            SecurityContextSecurityToken currentSessionToken;
            SecurityContextSecurityToken newSessionToken;
            UniqueId sessionId;
            Message message;
            Uri listenAddress;

            public ServerSessionTraceRecord(SecurityContextSecurityToken currentSessionToken, SecurityContextSecurityToken newSessionToken, Uri listenAddress)
                : base("SecuritySession")
            {
                this.currentSessionToken = currentSessionToken;
                this.newSessionToken = newSessionToken;
                this.listenAddress = listenAddress;
            }

            public ServerSessionTraceRecord(SecurityContextSecurityToken currentSessionToken, Message message, Uri listenAddress)
                : base("SecuritySession")
            {
                this.currentSessionToken = currentSessionToken;
                this.message = message;
                this.listenAddress = listenAddress;
            }

            public ServerSessionTraceRecord(SecurityContextSecurityToken currentSessionToken, Uri listenAddress)
                : base("SecuritySession")
            {
                this.currentSessionToken = currentSessionToken;
                this.listenAddress = listenAddress;
            }

            public ServerSessionTraceRecord(UniqueId sessionId, Uri listenAddress)
                : base("SecuritySession")
            {
                this.sessionId = sessionId;
                this.listenAddress = listenAddress;
            }

            internal override void WriteTo(XmlWriter xml)
            {
                if (xml == null)
                    return;

                if (this.currentSessionToken != null)
                {
                    xml.WriteStartElement("CurrentSessionToken");
                    WriteSecurityContextToken(xml, this.currentSessionToken);
                    xml.WriteEndElement();
                }
                if (this.newSessionToken != null)
                {
                    xml.WriteStartElement("NewSessionToken");
                    WriteSecurityContextToken(xml, this.newSessionToken);
                    xml.WriteEndElement();
                }
                if (this.sessionId != null)
                {
                    XmlHelper.WriteElementStringAsUniqueId(xml, "SessionId", this.sessionId);
                }
                if (this.message != null)
                {
                    xml.WriteElementString("MessageAction", message.Headers.Action);
                }
                if (this.listenAddress != null)
                {
                    xml.WriteElementString("ListenAddress", this.listenAddress.ToString());
                }
            }
        }

        class ClientSessionTraceRecord : SecurityTraceRecord
        {
            SecurityToken currentSessionToken;
            SecurityToken previousSessionToken;
            EndpointAddress remoteAddress;

            public ClientSessionTraceRecord(SecurityToken currentSessionToken, SecurityToken previousSessionToken, EndpointAddress remoteAddress)
                : base("SecuritySession")
            {
                this.currentSessionToken = currentSessionToken;
                this.previousSessionToken = previousSessionToken;
                this.remoteAddress = remoteAddress;
            }

            internal override void WriteTo(XmlWriter xml)
            {
                if (xml == null)
                    return;

                if (this.remoteAddress != null)
                    xml.WriteElementString("RemoteAddress", remoteAddress.ToString());

                if (this.currentSessionToken != null)
                {
                    xml.WriteStartElement("CurrentSessionToken");
                    WriteGenericXmlToken(xml, this.currentSessionToken);
                    xml.WriteEndElement();
                }
                if (this.previousSessionToken != null)
                {
                    xml.WriteStartElement("PreviousSessionToken");
                    WriteGenericXmlToken(xml, this.previousSessionToken);
                    xml.WriteEndElement();
                }
            }
        }

        class SessionRedirectAppliedTraceRecord : SecurityTraceRecord
        {
            EndpointAddress previousTarget;
            EndpointAddress newTarget;
            GenericXmlSecurityToken sessionToken;

            public SessionRedirectAppliedTraceRecord(EndpointAddress previousTarget, EndpointAddress newTarget, GenericXmlSecurityToken sessionToken)
                : base("SecuritySession")
            {
                this.previousTarget = previousTarget;
                this.newTarget = newTarget;
                this.sessionToken = sessionToken;
            }

            internal override void WriteTo(XmlWriter xml)
            {
                if (xml == null)
                    return;

                if (this.previousTarget != null)
                    xml.WriteElementString("OriginalRemoteAddress", this.previousTarget.ToString());

                if (this.newTarget != null)
                    xml.WriteElementString("NewRemoteAddress", this.newTarget.ToString());

                if (this.sessionToken != null)
                {
                    xml.WriteStartElement("SessionToken");
                    WriteGenericXmlToken(xml, this.sessionToken);
                    xml.WriteEndElement();
                }
            }
        }

        class SpnToSidMappingTraceRecord : SecurityTraceRecord
        {
            string spn;
            Exception e;

            public SpnToSidMappingTraceRecord(string spn, Exception e)
                : base("SecurityIdentity")
            {
                this.spn = spn;
                this.e = e;
            }

            internal override void WriteTo(XmlWriter xml)
            {
                if (xml == null)
                    return;

                if (this.spn != null)
                    xml.WriteElementString("ServicePrincipalName", this.spn);

                if (this.e != null)
                    xml.WriteElementString("Exception", this.e.ToString());
            }
        }

        class MessageSecurityTraceRecord : SecurityTraceRecord
        {
            SecurityProtocol binding;
            Message message;

            public MessageSecurityTraceRecord(SecurityProtocol binding, Message message)
                : base("SecurityProtocol")
            {
                this.binding = binding;
                this.message = message;
            }

            internal override void WriteTo(XmlWriter xml)
            {
                if (xml == null)
                    return;

                if (this.binding != null)
                    xml.WriteElementString("SecurityProtocol", this.binding.ToString());

                if (this.message != null)
                {
                    string action = this.message.Headers.Action;
                    Uri to = this.message.Headers.To;
                    EndpointAddress replyTo = this.message.Headers.ReplyTo;
                    UniqueId id = this.message.Headers.MessageId;
                    if (!String.IsNullOrEmpty(action))
                    {
                        xml.WriteElementString("Action", action);
                    }
                    if (to != null)
                    {
                        xml.WriteElementString("To", to.AbsoluteUri);
                    }
                    if (replyTo != null)
                    {
                        replyTo.WriteTo(this.message.Version.Addressing, xml);
                    }
                    if (id != null)
                    {
                        xml.WriteElementString("MessageId", id.ToString());
                    }
                }
                else
                {
                    xml.WriteElementString("Message", "null");
                }
            }
        }

        class TokenProviderTraceRecord : SecurityTraceRecord
        {
            SecurityTokenProvider provider;

            public TokenProviderTraceRecord(SecurityTokenProvider provider)
                : base("SecurityTokenProvider")
            {
                this.provider = provider;
            }

            internal override void WriteTo(XmlWriter xml)
            {
                if (xml == null)
                    return;

                if (this.provider != null)
                    xml.WriteElementString("SecurityTokenProvider", this.provider.ToString());
            }
        }

        class TokenAuthenticatorTraceRecord : SecurityTraceRecord
        {
            SecurityTokenAuthenticator authenticator;

            public TokenAuthenticatorTraceRecord(SecurityTokenAuthenticator authenticator)
                : base("SecurityTokenAuthenticator")
            {
                this.authenticator = authenticator;
            }

            internal override void WriteTo(XmlWriter xml)
            {
                if (xml == null)
                    return;

                if (this.authenticator != null)
                    xml.WriteElementString("SecurityTokenAuthenticator", this.authenticator.ToString());
            }
        }

        class SecurityContextTokenCacheTraceRecord : SecurityTraceRecord
        {
            int capacity;
            int pruningAmount;

            public SecurityContextTokenCacheTraceRecord(int capacity, int pruningAmount)
                : base("ServiceSecurityNegotiation")
            {
                this.capacity = capacity;
                this.pruningAmount = pruningAmount;
            }

            internal override void WriteTo(XmlWriter xml)
            {
                if (xml == null)
                    return;

                xml.WriteElementString("Capacity", this.capacity.ToString(NumberFormatInfo.InvariantInfo));
                xml.WriteElementString("PruningAmount", this.pruningAmount.ToString(NumberFormatInfo.InvariantInfo));
            }
        }

        class NegotiationAuthenticatorTraceRecord<T> : SecurityTraceRecord
            where T : NegotiationTokenAuthenticatorState
        {
            NegotiationTokenAuthenticator<T> authenticator;
            IChannelListener transportChannelListener;
            SecurityContextSecurityToken serviceToken;
            Exception e;

            public NegotiationAuthenticatorTraceRecord(NegotiationTokenAuthenticator<T> authenticator, IChannelListener transportChannelListener)
                : base("NegotiationTokenAuthenticator")
            {
                this.authenticator = authenticator;
                this.transportChannelListener = transportChannelListener;
            }

            public NegotiationAuthenticatorTraceRecord(NegotiationTokenAuthenticator<T> authenticator, Exception e)
                : base("NegotiationTokenAuthenticator")
            {
                this.authenticator = authenticator;
                this.e = e;
            }

            public NegotiationAuthenticatorTraceRecord(NegotiationTokenAuthenticator<T> authenticator, SecurityContextSecurityToken serviceToken)
                : base("NegotiationTokenAuthenticator")
            {
                this.authenticator = authenticator;
                this.serviceToken = serviceToken;
            }

            internal override void WriteTo(XmlWriter xml)
            {
                if (xml == null)
                    return;

                if (this.authenticator != null)
                    xml.WriteElementString("NegotiationTokenAuthenticator", base.XmlEncode(this.authenticator.ToString()));

                if (this.authenticator != null && this.authenticator.ListenUri != null)
                    xml.WriteElementString("AuthenticatorListenUri", this.authenticator.ListenUri.AbsoluteUri);

                if (this.serviceToken != null)
                {
                    xml.WriteStartElement("SecurityContextSecurityToken");
                    WriteSecurityContextToken(xml, this.serviceToken);
                    xml.WriteEndElement();
                }
                if (this.transportChannelListener != null)
                {
                    xml.WriteElementString("TransportChannelListener", base.XmlEncode(this.transportChannelListener.ToString()));

                    if (this.transportChannelListener.Uri != null)
                        xml.WriteElementString("ListenUri", this.transportChannelListener.Uri.AbsoluteUri);
                }
                if (this.e != null)
                {
                    xml.WriteElementString("Exception", base.XmlEncode(e.ToString()));
                }
            }
        }

        class IdentityVerificationSuccessTraceRecord : SecurityTraceRecord
        {
            EndpointIdentity identity;
            Claim claim;
            Type identityVerifier;

            public IdentityVerificationSuccessTraceRecord(EndpointIdentity identity, Claim claim, Type identityVerifier)
                : base("ServiceIdentityVerification")
            {
                this.identity = identity;
                this.claim = claim;
                this.identityVerifier = identityVerifier;
            }

            internal override void WriteTo(XmlWriter xml)
            {
                if (xml == null)
                    return;

                XmlDictionaryWriter xmlWriter = XmlDictionaryWriter.CreateDictionaryWriter(xml);

                if (this.identityVerifier != null)
                    xml.WriteElementString("IdentityVerifierType", this.identityVerifier.ToString());

                if (this.identity != null)
                    this.identity.WriteTo(xmlWriter);

                if (this.claim != null)
                    SecurityTraceRecordHelper.WriteClaim(xmlWriter, this.claim);
            }
        }

        class IdentityVerificationFailureTraceRecord : SecurityTraceRecord
        {
            EndpointIdentity identity;
            AuthorizationContext authContext;
            Type identityVerifier;

            public IdentityVerificationFailureTraceRecord(EndpointIdentity identity, AuthorizationContext authContext, Type identityVerifier)
                : base("ServiceIdentityVerification")
            {
                this.identity = identity;
                this.authContext = authContext;
                this.identityVerifier = identityVerifier;
            }

            internal override void WriteTo(XmlWriter xml)
            {
                if (xml == null)
                    return;

                XmlDictionaryWriter xmlWriter = XmlDictionaryWriter.CreateDictionaryWriter(xml);

                if (this.identityVerifier != null)
                    xml.WriteElementString("IdentityVerifierType", this.identityVerifier.ToString());

                if (this.identity != null)
                    this.identity.WriteTo(xmlWriter);

                if (this.authContext != null)
                {
                    for (int i = 0; i < this.authContext.ClaimSets.Count; ++i)
                    {
                        ClaimSet claimSet = this.authContext.ClaimSets[i];
                        if (this.authContext.ClaimSets[i] == null)
                            continue;

                        for (int j = 0; j < claimSet.Count; ++j)
                        {
                            Claim claim = claimSet[j];
                            if (claimSet[j] == null)
                                continue;

                            xml.WriteStartElement("Claim");

                            // currently ClaimType and Right cannot be null.  Just being defensive
                            if (claim.ClaimType != null)
                                xml.WriteElementString("ClaimType", claim.ClaimType);
                            else
                                xml.WriteElementString("ClaimType", "null");

                            if (claim.Right != null)
                                xml.WriteElementString("Right", claim.Right);
                            else
                                xml.WriteElementString("Right", "null");

                            if (claim.Resource != null)
                                xml.WriteElementString("ResourceType", claim.Resource.GetType().ToString());
                            else
                                xml.WriteElementString("Resource", "null");

                            xml.WriteEndElement();
                        }
                    }
                }
            }
        }

        class IdentityDeterminationSuccessTraceRecord : SecurityTraceRecord
        {
            EndpointIdentity identity;
            EndpointAddress epr;
            Type identityVerifier;

            public IdentityDeterminationSuccessTraceRecord(EndpointAddress epr, EndpointIdentity identity, Type identityVerifier)
                : base("ServiceIdentityDetermination")
            {
                this.identity = identity;
                this.epr = epr;
                this.identityVerifier = identityVerifier;
            }

            internal override void WriteTo(XmlWriter xml)
            {
                if (xml == null)
                    return;

                if (this.identityVerifier != null)
                    xml.WriteElementString("IdentityVerifierType", this.identityVerifier.ToString());

                if (this.identity != null)
                    this.identity.WriteTo(XmlDictionaryWriter.CreateDictionaryWriter(xml));

                if (this.epr != null)
                    this.epr.WriteTo(AddressingVersion.WSAddressing10, xml);
            }
        }

        class IdentityDeterminationFailureTraceRecord : SecurityTraceRecord
        {
            Type identityVerifier;
            EndpointAddress epr;

            public IdentityDeterminationFailureTraceRecord(EndpointAddress epr, Type identityVerifier)
                : base("ServiceIdentityDetermination")
            {
                this.epr = epr;
                this.identityVerifier = identityVerifier;
            }

            internal override void WriteTo(XmlWriter xml)
            {
                if (xml == null)
                    return;

                if (this.identityVerifier != null)
                    xml.WriteElementString("IdentityVerifierType", this.identityVerifier.ToString());

                if (this.epr != null)
                    this.epr.WriteTo(AddressingVersion.WSAddressing10, xml);
            }
        }

        class IdentityHostNameNormalizationFailureTraceRecord : SecurityTraceRecord
        {
            Type identityVerifier;
            Exception e;
            EndpointAddress epr;

            public IdentityHostNameNormalizationFailureTraceRecord(EndpointAddress epr, Type identityVerifier, Exception e)
                : base("ServiceIdentityDetermination")
            {
                this.epr = epr;
                this.identityVerifier = identityVerifier;
                this.e = e;
            }

            internal override void WriteTo(XmlWriter xml)
            {
                if (xml == null)
                    return;

                if (this.identityVerifier != null)
                    xml.WriteElementString("IdentityVerifierType", this.identityVerifier.ToString());

                if (this.epr != null)
                    this.epr.WriteTo(AddressingVersion.WSAddressing10, xml);

                if (e != null)
                    xml.WriteElementString("Exception", e.ToString());
            }
        }

        class IssuanceProviderTraceRecord<T> : SecurityTraceRecord
           where T : IssuanceTokenProviderState
        {
            IssuanceTokenProviderBase<T> provider;
            EndpointAddress target;
            EndpointAddress newTarget;
            SecurityToken serviceToken;
            int cacheSize;

            public IssuanceProviderTraceRecord(IssuanceTokenProviderBase<T> provider, SecurityToken serviceToken)
                : this(provider, serviceToken, null)
            { }

            public IssuanceProviderTraceRecord(IssuanceTokenProviderBase<T> provider, EndpointAddress target)
                : this(provider, (SecurityToken)null, target)
            { }

            public IssuanceProviderTraceRecord(IssuanceTokenProviderBase<T> provider, SecurityToken serviceToken, EndpointAddress target)
                : base("ClientSecurityNegotiation")
            {
                this.provider = provider;
                this.serviceToken = serviceToken;
                this.target = target;
            }

            public IssuanceProviderTraceRecord(IssuanceTokenProviderBase<T> provider, EndpointAddress newTarget, EndpointAddress oldTarget)
                : base("ClientSecurityNegotiation")
            {
                this.provider = provider;
                this.newTarget = newTarget;
                this.target = oldTarget;
            }

            public IssuanceProviderTraceRecord(IssuanceTokenProviderBase<T> provider, int cacheSize)
                : base("ClientSecurityNegotiation")
            {
                this.provider = provider;
                this.cacheSize = cacheSize;
            }

            internal override void WriteTo(XmlWriter xml)
            {
                if (xml == null)
                    return;

                if (this.provider != null)
                    xml.WriteElementString("IssuanceTokenProvider", this.provider.ToString());

                if (this.serviceToken != null)
                    WritePossibleGenericXmlToken(xml, "ServiceToken", this.serviceToken);


                if (this.target != null)
                {
                    xml.WriteStartElement("Target");
                    this.target.WriteTo(AddressingVersion.WSAddressing10, xml);
                    xml.WriteEndElement();
                }

                if (this.newTarget != null)
                {
                    xml.WriteStartElement("PinnedTarget");
                    this.newTarget.WriteTo(AddressingVersion.WSAddressing10, xml);
                    xml.WriteEndElement();
                }

                if (this.cacheSize != 0)
                {
                    xml.WriteElementString("CacheSize", this.cacheSize.ToString(NumberFormatInfo.InvariantInfo));
                }
            }
        }

        class WindowsSspiNegotiationTraceRecord : SecurityTraceRecord
        {
            WindowsSspiNegotiation windowsNegotiation;

            public WindowsSspiNegotiationTraceRecord(WindowsSspiNegotiation windowsNegotiation)
                : base("SpnegoSecurityNegotiation")
            {
                this.windowsNegotiation = windowsNegotiation;
            }

            internal override void WriteTo(XmlWriter xml)
            {
                if (xml == null)
                    return;

                if (this.windowsNegotiation != null)
                {
                    xml.WriteElementString("Protocol", this.windowsNegotiation.ProtocolName);
                    xml.WriteElementString("ServicePrincipalName", this.windowsNegotiation.ServicePrincipalName);
                    xml.WriteElementString("MutualAuthentication", this.windowsNegotiation.IsMutualAuthFlag.ToString());

                    if (this.windowsNegotiation.IsIdentifyFlag)
                    {
                        xml.WriteElementString("ImpersonationLevel", "Identify");
                    }
                    else if (this.windowsNegotiation.IsDelegationFlag)
                    {
                        xml.WriteElementString("ImpersonationLevel", "Delegate");
                    }
                    else
                    {
                        xml.WriteElementString("ImpersonationLevel", "Impersonate");
                    }
                }
            }
        }


        class ImpersonationTraceRecord : SecurityTraceRecord
        {
            private DispatchOperationRuntime operation;

            internal ImpersonationTraceRecord(DispatchOperationRuntime operation)
                : base("SecurityImpersonation")
            {
                this.operation = operation;
            }

            internal override void WriteTo(XmlWriter xml)
            {
                if (xml == null)
                {
                    // We are inside tracing. Don't throw an exception here just
                    // return.
                    return;
                }

                if (this.operation != null)
                {
                    xml.WriteElementString("OperationAction", this.operation.Action);
                    xml.WriteElementString("OperationName", this.operation.Name);
                }
            }
        }
    }
}
