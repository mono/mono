//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Diagnostics
{
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Threading;
    using System.Security;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Activation;
    using System.Xml;
    using System.ServiceModel.Diagnostics.Application;
    using System.Globalization;
    using System.Collections.Generic;

    static class TraceUtility
    {
        const string ActivityIdKey = "ActivityId";
        const string AsyncOperationActivityKey = "AsyncOperationActivity";
        const string AsyncOperationStartTimeKey = "AsyncOperationStartTime";
        static bool shouldPropagateActivity;
        static bool shouldPropagateActivityGlobal;
        static bool activityTracing;
        static bool messageFlowTracing;
        static bool messageFlowTracingOnly;
        static long messageNumber = 0;
        static Func<Action<AsyncCallback, IAsyncResult>> asyncCallbackGenerator;
        static SortedList<int, string> traceCodes = new SortedList<int, string>(382)
        {
            // Administration trace codes (TraceCode.Administration)
            { TraceCode.WmiPut, "WmiPut" },

            // Diagnostic trace codes (TraceCode.Diagnostics)
            { TraceCode.AppDomainUnload, "AppDomainUnload" },
            { TraceCode.EventLog, "EventLog" },
            { TraceCode.ThrowingException, "ThrowingException" },
            { TraceCode.TraceHandledException, "TraceHandledException" },
            { TraceCode.UnhandledException, "UnhandledException" },
            { TraceCode.FailedToAddAnActivityIdHeader, "FailedToAddAnActivityIdHeader" },
            { TraceCode.FailedToReadAnActivityIdHeader, "FailedToReadAnActivityIdHeader" },
            { TraceCode.FilterNotMatchedNodeQuotaExceeded, "FilterNotMatchedNodeQuotaExceeded" },
            { TraceCode.MessageCountLimitExceeded, "MessageCountLimitExceeded" },
            { TraceCode.DiagnosticsFailedMessageTrace, "DiagnosticsFailedMessageTrace" },
            { TraceCode.MessageNotLoggedQuotaExceeded, "MessageNotLoggedQuotaExceeded" },
            { TraceCode.TraceTruncatedQuotaExceeded, "TraceTruncatedQuotaExceeded" },
            { TraceCode.ActivityBoundary, "ActivityBoundary" },

            // Serialization trace codes (TraceCode.Serialization)
            { TraceCode.ElementIgnored, "" }, // shared by ServiceModel, need to investigate if should put this one in the SM section

            // Channels trace codes (TraceCode.Channels)
            { TraceCode.ConnectionAbandoned, "ConnectionAbandoned" },
            { TraceCode.ConnectionPoolCloseException, "ConnectionPoolCloseException" },
            { TraceCode.ConnectionPoolIdleTimeoutReached, "ConnectionPoolIdleTimeoutReached" },
            { TraceCode.ConnectionPoolLeaseTimeoutReached, "ConnectionPoolLeaseTimeoutReached" },
            { TraceCode.ConnectionPoolMaxOutboundConnectionsPerEndpointQuotaReached, "ConnectionPoolMaxOutboundConnectionsPerEndpointQuotaReached" },
            { TraceCode.ServerMaxPooledConnectionsQuotaReached, "ServerMaxPooledConnectionsQuotaReached" },
            { TraceCode.EndpointListenerClose, "EndpointListenerClose" },
            { TraceCode.EndpointListenerOpen, "EndpointListenerOpen" },
            { TraceCode.HttpResponseReceived, "HttpResponseReceived" },
            { TraceCode.HttpChannelConcurrentReceiveQuotaReached, "HttpChannelConcurrentReceiveQuotaReached" }, 
            { TraceCode.HttpChannelMessageReceiveFailed, "HttpChannelMessageReceiveFailed" },
            { TraceCode.HttpChannelUnexpectedResponse, "HttpChannelUnexpectedResponse" },
            { TraceCode.HttpChannelRequestAborted, "HttpChannelRequestAborted" },
            { TraceCode.HttpChannelResponseAborted, "HttpChannelResponseAborted" },
            { TraceCode.HttpsClientCertificateInvalid, "HttpsClientCertificateInvalid" },
            { TraceCode.HttpsClientCertificateNotPresent, "HttpsClientCertificateNotPresent" },
            { TraceCode.NamedPipeChannelMessageReceiveFailed, "NamedPipeChannelMessageReceiveFailed" },
            { TraceCode.NamedPipeChannelMessageReceived, "NamedPipeChannelMessageReceived" },
            { TraceCode.MessageReceived, "MessageReceived" },
            { TraceCode.MessageSent, "MessageSent" },
            { TraceCode.RequestChannelReplyReceived, "RequestChannelReplyReceived" },
            { TraceCode.TcpChannelMessageReceiveFailed, "TcpChannelMessageReceiveFailed" },
            { TraceCode.TcpChannelMessageReceived, "TcpChannelMessageReceived" },
            { TraceCode.ConnectToIPEndpoint, "ConnectToIPEndpoint" },
            { TraceCode.SocketConnectionCreate, "SocketConnectionCreate" },
            { TraceCode.SocketConnectionClose, "SocketConnectionClose" },
            { TraceCode.SocketConnectionAbort, "SocketConnectionAbort" },
            { TraceCode.SocketConnectionAbortClose, "SocketConnectionAbortClose" },
            { TraceCode.PipeConnectionAbort, "PipeConnectionAbort" },
            { TraceCode.RequestContextAbort, "RequestContextAbort" },
            { TraceCode.ChannelCreated, "ChannelCreated" },
            { TraceCode.ChannelDisposed, "ChannelDisposed" },
            { TraceCode.ListenerCreated, "ListenerCreated" },
            { TraceCode.ListenerDisposed, "ListenerDisposed" },
            { TraceCode.PrematureDatagramEof, "PrematureDatagramEof" },
            { TraceCode.MaxPendingConnectionsReached, "MaxPendingConnectionsReached" },
            { TraceCode.MaxAcceptedChannelsReached, "MaxAcceptedChannelsReached" },
            { TraceCode.ChannelConnectionDropped, "ChannelConnectionDropped" },
            { TraceCode.HttpAuthFailed, "HttpAuthFailed" },
            { TraceCode.NoExistingTransportManager, "NoExistingTransportManager" },
            { TraceCode.IncompatibleExistingTransportManager, "IncompatibleExistingTransportManager" },
            { TraceCode.InitiatingNamedPipeConnection, "InitiatingNamedPipeConnection" },
            { TraceCode.InitiatingTcpConnection, "InitiatingTcpConnection" },
            { TraceCode.OpenedListener, "OpenedListener" },
            { TraceCode.SslClientCertMissing, "SslClientCertMissing" },
            { TraceCode.StreamSecurityUpgradeAccepted, "StreamSecurityUpgradeAccepted" },
            { TraceCode.TcpConnectError, "TcpConnectError" },
            { TraceCode.FailedAcceptFromPool, "FailedAcceptFromPool" },
            { TraceCode.FailedPipeConnect, "FailedPipeConnect" },
            { TraceCode.SystemTimeResolution, "SystemTimeResolution" },
            { TraceCode.PeerNeighborCloseFailed, "PeerNeighborCloseFailed" },
            { TraceCode.PeerNeighborClosingFailed, "PeerNeighborClosingFailed" },
            { TraceCode.PeerNeighborNotAccepted, "PeerNeighborNotAccepted" },
            { TraceCode.PeerNeighborNotFound, "PeerNeighborNotFound" },
            { TraceCode.PeerNeighborOpenFailed, "PeerNeighborOpenFailed" },
            { TraceCode.PeerNeighborStateChanged, "PeerNeighborStateChanged" },
            { TraceCode.PeerNeighborStateChangeFailed, "PeerNeighborStateChangeFailed" },
            { TraceCode.PeerNeighborMessageReceived, "PeerNeighborMessageReceived" },
            { TraceCode.PeerNeighborManagerOffline, "PeerNeighborManagerOffline" },
            { TraceCode.PeerNeighborManagerOnline, "PeerNeighborManagerOnline" },
            { TraceCode.PeerChannelMessageReceived, "PeerChannelMessageReceived" },
            { TraceCode.PeerChannelMessageSent, "PeerChannelMessageSent" },
            { TraceCode.PeerNodeAddressChanged, "PeerNodeAddressChanged" },
            { TraceCode.PeerNodeOpening, "PeerNodeOpening" },
            { TraceCode.PeerNodeOpened, "PeerNodeOpened" },
            { TraceCode.PeerNodeOpenFailed, "PeerNodeOpenFailed" },
            { TraceCode.PeerNodeClosing, "PeerNodeClosing" },
            { TraceCode.PeerNodeClosed, "PeerNodeClosed" },
            { TraceCode.PeerFloodedMessageReceived, "PeerFloodedMessageReceived" },
            { TraceCode.PeerFloodedMessageNotPropagated, "PeerFloodedMessageNotPropagated" },
            { TraceCode.PeerFloodedMessageNotMatched, "PeerFloodedMessageNotMatched" },
            { TraceCode.PnrpRegisteredAddresses, "PnrpRegisteredAddresses" },
            { TraceCode.PnrpUnregisteredAddresses, "PnrpUnregisteredAddresses" },
            { TraceCode.PnrpResolvedAddresses, "PnrpResolvedAddresses" },
            { TraceCode.PnrpResolveException, "PnrpResolveException" },
            { TraceCode.PeerReceiveMessageAuthenticationFailure, "PeerReceiveMessageAuthenticationFailure" },
            { TraceCode.PeerNodeAuthenticationFailure, "PeerNodeAuthenticationFailure" },
            { TraceCode.PeerNodeAuthenticationTimeout, "PeerNodeAuthenticationTimeout" },
            { TraceCode.PeerFlooderReceiveMessageQuotaExceeded, "PeerFlooderReceiveMessageQuotaExceeded" },
            { TraceCode.PeerServiceOpened, "PeerServiceOpened" },
            { TraceCode.PeerMaintainerActivity, "PeerMaintainerActivity" },
            { TraceCode.MsmqCannotPeekOnQueue, "MsmqCannotPeekOnQueue" },
            { TraceCode.MsmqCannotReadQueues, "MsmqCannotReadQueues" },
            { TraceCode.MsmqDatagramSent, "MsmqDatagramSent" },
            { TraceCode.MsmqDatagramReceived, "MsmqDatagramReceived" },
            { TraceCode.MsmqDetected, "MsmqDetected" },
            { TraceCode.MsmqEnteredBatch, "MsmqEnteredBatch" },
            { TraceCode.MsmqExpectedException, "MsmqExpectedException" },
            { TraceCode.MsmqFoundBaseAddress, "MsmqFoundBaseAddress" },
            { TraceCode.MsmqLeftBatch, "MsmqLeftBatch" },
            { TraceCode.MsmqMatchedApplicationFound, "MsmqMatchedApplicationFound" },
            { TraceCode.MsmqMessageDropped, "MsmqMessageDropped" },
            { TraceCode.MsmqMessageLockedUnderTheTransaction, "MsmqMessageLockedUnderTheTransaction" },
            { TraceCode.MsmqMessageRejected, "MsmqMessageRejected" },
            { TraceCode.MsmqMoveOrDeleteAttemptFailed, "MsmqMoveOrDeleteAttemptFailed" },
            { TraceCode.MsmqPoisonMessageMovedPoison, "MsmqPoisonMessageMovedPoison" },
            { TraceCode.MsmqPoisonMessageMovedRetry, "MsmqPoisonMessageMovedRetry" },
            { TraceCode.MsmqPoisonMessageRejected, "MsmqPoisonMessageRejected" },
            { TraceCode.MsmqPoolFull, "MsmqPoolFull" },
            { TraceCode.MsmqPotentiallyPoisonMessageDetected, "MsmqPotentiallyPoisonMessageDetected" },
            { TraceCode.MsmqQueueClosed, "MsmqQueueClosed" },
            { TraceCode.MsmqQueueOpened, "MsmqQueueOpened" },
            { TraceCode.MsmqQueueTransactionalStatusUnknown, "MsmqQueueTransactionalStatusUnknown" },
            { TraceCode.MsmqScanStarted, "MsmqScanStarted" },
            { TraceCode.MsmqSessiongramReceived, "MsmqSessiongramReceived" },
            { TraceCode.MsmqSessiongramSent, "MsmqSessiongramSent" },
            { TraceCode.MsmqStartingApplication, "MsmqStartingApplication" },
            { TraceCode.MsmqStartingService, "MsmqStartingService" },
            { TraceCode.MsmqUnexpectedAcknowledgment, "MsmqUnexpectedAcknowledgment" },
            { TraceCode.WsrmNegativeElapsedTimeDetected, "WsrmNegativeElapsedTimeDetected" },
            { TraceCode.TcpTransferError, "TcpTransferError" },
            { TraceCode.TcpConnectionResetError, "TcpConnectionResetError" },
            { TraceCode.TcpConnectionTimedOut, "TcpConnectionTimedOut" },

            // ComIntegration trace codes (TraceCode.ComIntegration)
            { TraceCode.ComIntegrationServiceHostStartingService, "ComIntegrationServiceHostStartingService" },
            { TraceCode.ComIntegrationServiceHostStartedService, "ComIntegrationServiceHostStartedService" },
            { TraceCode.ComIntegrationServiceHostCreatedServiceContract, "ComIntegrationServiceHostCreatedServiceContract" },
            { TraceCode.ComIntegrationServiceHostStartedServiceDetails, "ComIntegrationServiceHostStartedServiceDetails" },
            { TraceCode.ComIntegrationServiceHostCreatedServiceEndpoint, "ComIntegrationServiceHostCreatedServiceEndpoint" },
            { TraceCode.ComIntegrationServiceHostStoppingService, "ComIntegrationServiceHostStoppingService" },
            { TraceCode.ComIntegrationServiceHostStoppedService, "ComIntegrationServiceHostStoppedService" },
            { TraceCode.ComIntegrationDllHostInitializerStarting, "ComIntegrationDllHostInitializerStarting" },
            { TraceCode.ComIntegrationDllHostInitializerAddingHost, "ComIntegrationDllHostInitializerAddingHost" },
            { TraceCode.ComIntegrationDllHostInitializerStarted, "ComIntegrationDllHostInitializerStarted" },
            { TraceCode.ComIntegrationDllHostInitializerStopping, "ComIntegrationDllHostInitializerStopping" },
            { TraceCode.ComIntegrationDllHostInitializerStopped, "ComIntegrationDllHostInitializerStopped" },
            { TraceCode.ComIntegrationTLBImportStarting, "ComIntegrationTLBImportStarting" },
            { TraceCode.ComIntegrationTLBImportFromAssembly, "ComIntegrationTLBImportFromAssembly" },
            { TraceCode.ComIntegrationTLBImportFromTypelib, "ComIntegrationTLBImportFromTypelib" },
            { TraceCode.ComIntegrationTLBImportConverterEvent, "ComIntegrationTLBImportConverterEvent" },
            { TraceCode.ComIntegrationTLBImportFinished, "ComIntegrationTLBImportFinished" },
            { TraceCode.ComIntegrationInstanceCreationRequest, "ComIntegrationInstanceCreationRequest" },
            { TraceCode.ComIntegrationInstanceCreationSuccess, "ComIntegrationInstanceCreationSuccess" },
            { TraceCode.ComIntegrationInstanceReleased, "ComIntegrationInstanceReleased" },
            { TraceCode.ComIntegrationEnteringActivity, "ComIntegrationEnteringActivity" },
            { TraceCode.ComIntegrationExecutingCall, "ComIntegrationExecutingCall" },
            { TraceCode.ComIntegrationLeftActivity, "ComIntegrationLeftActivity" },
            { TraceCode.ComIntegrationInvokingMethod, "ComIntegrationInvokingMethod" },
            { TraceCode.ComIntegrationInvokedMethod, "ComIntegrationInvokedMethod" },
            { TraceCode.ComIntegrationInvokingMethodNewTransaction, "ComIntegrationInvokingMethodNewTransaction" },
            { TraceCode.ComIntegrationInvokingMethodContextTransaction, "ComIntegrationInvokingMethodContextTransaction" },
            { TraceCode.ComIntegrationServiceMonikerParsed, "ComIntegrationServiceMonikerParsed" },
            { TraceCode.ComIntegrationWsdlChannelBuilderLoaded, "ComIntegrationWsdlChannelBuilderLoaded" },
            { TraceCode.ComIntegrationTypedChannelBuilderLoaded, "ComIntegrationTypedChannelBuilderLoaded" },
            { TraceCode.ComIntegrationChannelCreated, "ComIntegrationChannelCreated" },
            { TraceCode.ComIntegrationDispatchMethod, "ComIntegrationDispatchMethod" },
            { TraceCode.ComIntegrationTxProxyTxCommitted, "ComIntegrationTxProxyTxCommitted" },
            { TraceCode.ComIntegrationTxProxyTxAbortedByContext, "ComIntegrationTxProxyTxAbortedByContext" },
            { TraceCode.ComIntegrationTxProxyTxAbortedByTM, "ComIntegrationTxProxyTxAbortedByTM" },
            { TraceCode.ComIntegrationMexMonikerMetadataExchangeComplete, "ComIntegrationMexMonikerMetadataExchangeComplete" },
            { TraceCode.ComIntegrationMexChannelBuilderLoaded, "ComIntegrationMexChannelBuilderLoaded" },

            // Security trace codes (TraceCode.Security)
            { TraceCode.Security, "Security" },
            { TraceCode.SecurityIdentityVerificationSuccess, "SecurityIdentityVerificationSuccess" },
            { TraceCode.SecurityIdentityVerificationFailure, "SecurityIdentityVerificationFailure" },
            { TraceCode.SecurityIdentityDeterminationSuccess, "SecurityIdentityDeterminationSuccess" },
            { TraceCode.SecurityIdentityDeterminationFailure, "SecurityIdentityDeterminationFailure" },
            { TraceCode.SecurityIdentityHostNameNormalizationFailure, "SecurityIdentityHostNameNormalizationFailure" },
            { TraceCode.SecurityImpersonationSuccess, "SecurityImpersonationSuccess" },
            { TraceCode.SecurityImpersonationFailure, "SecurityImpersonationFailure" },
            { TraceCode.SecurityNegotiationProcessingFailure, "SecurityNegotiationProcessingFailure" },
            { TraceCode.IssuanceTokenProviderRemovedCachedToken, "IssuanceTokenProviderRemovedCachedToken" },
            { TraceCode.IssuanceTokenProviderUsingCachedToken, "IssuanceTokenProviderUsingCachedToken" },
            { TraceCode.IssuanceTokenProviderBeginSecurityNegotiation, "IssuanceTokenProviderBeginSecurityNegotiation" },
            { TraceCode.IssuanceTokenProviderEndSecurityNegotiation, "IssuanceTokenProviderEndSecurityNegotiation" },
            { TraceCode.IssuanceTokenProviderRedirectApplied, "IssuanceTokenProviderRedirectApplied" },
            { TraceCode.IssuanceTokenProviderServiceTokenCacheFull, "IssuanceTokenProviderServiceTokenCacheFull" },
            { TraceCode.NegotiationTokenProviderAttached, "NegotiationTokenProviderAttached" },
            { TraceCode.SpnegoClientNegotiationCompleted, "SpnegoClientNegotiationCompleted" },
            { TraceCode.SpnegoServiceNegotiationCompleted, "SpnegoServiceNegotiationCompleted" },
            { TraceCode.SpnegoClientNegotiation, "SpnegoClientNegotiation" },
            { TraceCode.SpnegoServiceNegotiation, "SpnegoServiceNegotiation" },
            { TraceCode.NegotiationAuthenticatorAttached, "NegotiationAuthenticatorAttached" },
            { TraceCode.ServiceSecurityNegotiationCompleted, "ServiceSecurityNegotiationCompleted" },
            { TraceCode.SecurityContextTokenCacheFull, "SecurityContextTokenCacheFull" },
            { TraceCode.ExportSecurityChannelBindingEntry, "ExportSecurityChannelBindingEntry" },
            { TraceCode.ExportSecurityChannelBindingExit, "ExportSecurityChannelBindingExit" },
            { TraceCode.ImportSecurityChannelBindingEntry, "ImportSecurityChannelBindingEntry" },
            { TraceCode.ImportSecurityChannelBindingExit, "ImportSecurityChannelBindingExit" },
            { TraceCode.SecurityTokenProviderOpened, "SecurityTokenProviderOpened" },
            { TraceCode.SecurityTokenProviderClosed, "SecurityTokenProviderClosed" },
            { TraceCode.SecurityTokenAuthenticatorOpened, "SecurityTokenAuthenticatorOpened" },
            { TraceCode.SecurityTokenAuthenticatorClosed, "SecurityTokenAuthenticatorClosed" },
            { TraceCode.SecurityBindingOutgoingMessageSecured, "SecurityBindingOutgoingMessageSecured" },
            { TraceCode.SecurityBindingIncomingMessageVerified, "SecurityBindingIncomingMessageVerified" },
            { TraceCode.SecurityBindingSecureOutgoingMessageFailure, "SecurityBindingSecureOutgoingMessageFailure" },
            { TraceCode.SecurityBindingVerifyIncomingMessageFailure, "SecurityBindingVerifyIncomingMessageFailure" },
            { TraceCode.SecuritySpnToSidMappingFailure, "SecuritySpnToSidMappingFailure" },
            { TraceCode.SecuritySessionRedirectApplied, "SecuritySessionRedirectApplied" },
            { TraceCode.SecurityClientSessionCloseSent, "SecurityClientSessionCloseSent" },
            { TraceCode.SecurityClientSessionCloseResponseSent, "SecurityClientSessionCloseResponseSent" },
            { TraceCode.SecurityClientSessionCloseMessageReceived, "SecurityClientSessionCloseMessageReceived" },
            { TraceCode.SecuritySessionKeyRenewalFaultReceived, "SecuritySessionKeyRenewalFaultReceived" },
            { TraceCode.SecuritySessionAbortedFaultReceived, "SecuritySessionAbortedFaultReceived" },
            { TraceCode.SecuritySessionClosedResponseReceived, "SecuritySessionClosedResponseReceived" },
            { TraceCode.SecurityClientSessionPreviousKeyDiscarded, "SecurityClientSessionPreviousKeyDiscarded" },
            { TraceCode.SecurityClientSessionKeyRenewed, "SecurityClientSessionKeyRenewed" },
            { TraceCode.SecurityPendingServerSessionAdded, "SecurityPendingServerSessionAdded" },
            { TraceCode.SecurityPendingServerSessionClosed, "SecurityPendingServerSessionClosed" },
            { TraceCode.SecurityPendingServerSessionActivated, "SecurityPendingServerSessionActivated" },
            { TraceCode.SecurityActiveServerSessionRemoved, "SecurityActiveServerSessionRemoved" },
            { TraceCode.SecurityNewServerSessionKeyIssued, "SecurityNewServerSessionKeyIssued" },
            { TraceCode.SecurityInactiveSessionFaulted, "SecurityInactiveSessionFaulted" },
            { TraceCode.SecurityServerSessionKeyUpdated, "SecurityServerSessionKeyUpdated" },
            { TraceCode.SecurityServerSessionCloseReceived, "SecurityServerSessionCloseReceived" },
            { TraceCode.SecurityServerSessionRenewalFaultSent, "SecurityServerSessionRenewalFaultSent" },
            { TraceCode.SecurityServerSessionAbortedFaultSent, "SecurityServerSessionAbortedFaultSent" },
            { TraceCode.SecuritySessionCloseResponseSent, "SecuritySessionCloseResponseSent" },
            { TraceCode.SecuritySessionServerCloseSent, "SecuritySessionServerCloseSent" },
            { TraceCode.SecurityServerSessionCloseResponseReceived, "SecurityServerSessionCloseResponseReceived" },
            { TraceCode.SecuritySessionRenewFaultSendFailure, "SecuritySessionRenewFaultSendFailure" },
            { TraceCode.SecuritySessionAbortedFaultSendFailure, "SecuritySessionAbortedFaultSendFailure" },
            { TraceCode.SecuritySessionClosedResponseSendFailure, "SecuritySessionClosedResponseSendFailure" },
            { TraceCode.SecuritySessionServerCloseSendFailure, "SecuritySessionServerCloseSendFailure" },
            { TraceCode.SecuritySessionRequestorStartOperation, "SecuritySessionRequestorStartOperation" },
            { TraceCode.SecuritySessionRequestorOperationSuccess, "SecuritySessionRequestorOperationSuccess" },
            { TraceCode.SecuritySessionRequestorOperationFailure, "SecuritySessionRequestorOperationFailure" },
            { TraceCode.SecuritySessionResponderOperationFailure, "SecuritySessionResponderOperationFailure" },
            { TraceCode.SecuritySessionDemuxFailure, "SecuritySessionDemuxFailure" },
            { TraceCode.SecurityAuditWrittenSuccess, "SecurityAuditWrittenSuccess" },
            { TraceCode.SecurityAuditWrittenFailure, "SecurityAuditWrittenFailure" },

            // ServiceModel trace codes (TraceCode.ServiceModel)
            { TraceCode.AsyncCallbackThrewException, "AsyncCallbackThrewException" },
            { TraceCode.CommunicationObjectAborted, "CommunicationObjectAborted" },
            { TraceCode.CommunicationObjectAbortFailed, "CommunicationObjectAbortFailed" },
            { TraceCode.CommunicationObjectCloseFailed, "CommunicationObjectCloseFailed" },
            { TraceCode.CommunicationObjectOpenFailed, "CommunicationObjectOpenFailed" },
            { TraceCode.CommunicationObjectClosing, "CommunicationObjectClosing" },
            { TraceCode.CommunicationObjectClosed, "CommunicationObjectClosed" },
            { TraceCode.CommunicationObjectCreated, "CommunicationObjectCreated" },
            { TraceCode.CommunicationObjectDisposing, "CommunicationObjectDisposing" },
            { TraceCode.CommunicationObjectFaultReason, "CommunicationObjectFaultReason" },
            { TraceCode.CommunicationObjectFaulted, "CommunicationObjectFaulted" },
            { TraceCode.CommunicationObjectOpening, "CommunicationObjectOpening" },
            { TraceCode.CommunicationObjectOpened, "CommunicationObjectOpened" },
            { TraceCode.DidNotUnderstandMessageHeader, "DidNotUnderstandMessageHeader" },
            { TraceCode.UnderstoodMessageHeader, "UnderstoodMessageHeader" },
            { TraceCode.MessageClosed, "MessageClosed" },
            { TraceCode.MessageClosedAgain, "MessageClosedAgain" },
            { TraceCode.MessageCopied, "MessageCopied" },
            { TraceCode.MessageRead, "MessageRead" },
            { TraceCode.MessageWritten, "MessageWritten" },
            { TraceCode.BeginExecuteMethod, "BeginExecuteMethod" },
            { TraceCode.ConfigurationIsReadOnly, "ConfigurationIsReadOnly" },
            { TraceCode.ConfiguredExtensionTypeNotFound, "ConfiguredExtensionTypeNotFound" },
            { TraceCode.EvaluationContextNotFound, "EvaluationContextNotFound" },
            { TraceCode.EndExecuteMethod, "EndExecuteMethod" },
            { TraceCode.ExtensionCollectionDoesNotExist, "ExtensionCollectionDoesNotExist" },
            { TraceCode.ExtensionCollectionNameNotFound, "ExtensionCollectionNameNotFound" },
            { TraceCode.ExtensionCollectionIsEmpty, "ExtensionCollectionIsEmpty" },
            { TraceCode.ExtensionElementAlreadyExistsInCollection, "ExtensionElementAlreadyExistsInCollection" },
            { TraceCode.ElementTypeDoesntMatchConfiguredType, "ElementTypeDoesntMatchConfiguredType" },
            { TraceCode.ErrorInvokingUserCode, "ErrorInvokingUserCode" },
            { TraceCode.GetBehaviorElement, "GetBehaviorElement" },
            { TraceCode.GetCommonBehaviors, "GetCommonBehaviors" },
            { TraceCode.GetConfiguredBinding, "GetConfiguredBinding" },
            { TraceCode.GetChannelEndpointElement, "GetChannelEndpointElement" },
            { TraceCode.GetConfigurationSection, "GetConfigurationSection" },
            { TraceCode.GetDefaultConfiguredBinding, "GetDefaultConfiguredBinding" },
            { TraceCode.GetServiceElement, "GetServiceElement" },
            { TraceCode.MessageProcessingPaused, "MessageProcessingPaused" },
            { TraceCode.ManualFlowThrottleLimitReached, "ManualFlowThrottleLimitReached" },
            { TraceCode.OverridingDuplicateConfigurationKey, "OverridingDuplicateConfigurationKey" },
            { TraceCode.RemoveBehavior, "RemoveBehavior" },
            { TraceCode.ServiceChannelLifetime, "ServiceChannelLifetime" },
            { TraceCode.ServiceHostCreation, "ServiceHostCreation" },
            { TraceCode.ServiceHostBaseAddresses, "ServiceHostBaseAddresses" },
            { TraceCode.ServiceHostTimeoutOnClose, "ServiceHostTimeoutOnClose" },
            { TraceCode.ServiceHostFaulted, "ServiceHostFaulted" },
            { TraceCode.ServiceHostErrorOnReleasePerformanceCounter, "ServiceHostErrorOnReleasePerformanceCounter" },
            { TraceCode.ServiceThrottleLimitReached, "ServiceThrottleLimitReached" },
            { TraceCode.ServiceOperationMissingReply, "ServiceOperationMissingReply" },
            { TraceCode.ServiceOperationMissingReplyContext, "ServiceOperationMissingReplyContext" },
            { TraceCode.ServiceOperationExceptionOnReply, "ServiceOperationExceptionOnReply" },
            { TraceCode.SkipBehavior, "SkipBehavior" },
            { TraceCode.TransportListen, "TransportListen" },
            { TraceCode.UnhandledAction, "UnhandledAction" },
            { TraceCode.PerformanceCounterFailedToLoad, "PerformanceCounterFailedToLoad" },
            { TraceCode.PerformanceCountersFailed, "PerformanceCountersFailed" },
            { TraceCode.PerformanceCountersFailedDuringUpdate, "PerformanceCountersFailedDuringUpdate" },
            { TraceCode.PerformanceCountersFailedForService, "PerformanceCountersFailedForService" },
            { TraceCode.PerformanceCountersFailedOnRelease, "PerformanceCountersFailedOnRelease" },
            { TraceCode.WsmexNonCriticalWsdlExportError, "WsmexNonCriticalWsdlExportError" },
            { TraceCode.WsmexNonCriticalWsdlImportError, "WsmexNonCriticalWsdlImportError" },
            { TraceCode.FailedToOpenIncomingChannel, "FailedToOpenIncomingChannel" },
            { TraceCode.UnhandledExceptionInUserOperation, "UnhandledExceptionInUserOperation" },
            { TraceCode.DroppedAMessage, "DroppedAMessage" },
            { TraceCode.CannotBeImportedInCurrentFormat, "CannotBeImportedInCurrentFormat" },
            { TraceCode.GetConfiguredEndpoint, "GetConfiguredEndpoint" },
            { TraceCode.GetDefaultConfiguredEndpoint, "GetDefaultConfiguredEndpoint" },
            { TraceCode.ExtensionTypeNotFound, "ExtensionTypeNotFound" },
            { TraceCode.DefaultEndpointsAdded, "DefaultEndpointsAdded" },

            //ServiceModel Metadata codes
            { TraceCode.MetadataExchangeClientSendRequest, "MetadataExchangeClientSendRequest" },
            { TraceCode.MetadataExchangeClientReceiveReply, "MetadataExchangeClientReceiveReply" },
            { TraceCode.WarnHelpPageEnabledNoBaseAddress, "WarnHelpPageEnabledNoBaseAddress" },
            
            // PortSharingtrace codes (TraceCode.PortSharing)
            { TraceCode.PortSharingClosed, "PortSharingClosed" },
            { TraceCode.PortSharingDuplicatedPipe, "PortSharingDuplicatedPipe" },
            { TraceCode.PortSharingDupHandleGranted, "PortSharingDupHandleGranted" },
            { TraceCode.PortSharingDuplicatedSocket, "PortSharingDuplicatedSocket" },
            { TraceCode.PortSharingListening, "PortSharingListening" },            
            { TraceCode.SharedManagerServiceEndpointNotExist, "SharedManagerServiceEndpointNotExist" },
                        
            //Indigo Tx trace codes (TraceCode.ServiceModelTransaction)
            { TraceCode.TxSourceTxScopeRequiredIsTransactedTransport, "TxSourceTxScopeRequiredIsTransactedTransport" },
            { TraceCode.TxSourceTxScopeRequiredIsTransactionFlow, "TxSourceTxScopeRequiredIsTransactionFlow" },
            { TraceCode.TxSourceTxScopeRequiredIsAttachedTransaction, "TxSourceTxScopeRequiredIsAttachedTransaction" },
            { TraceCode.TxSourceTxScopeRequiredIsCreateNewTransaction, "TxSourceTxScopeRequiredIsCreateNewTransaction" },
            { TraceCode.TxCompletionStatusCompletedForAutocomplete, "TxCompletionStatusCompletedForAutocomplete" },
            { TraceCode.TxCompletionStatusCompletedForError, "TxCompletionStatusCompletedForError" },
            { TraceCode.TxCompletionStatusCompletedForSetComplete, "TxCompletionStatusCompletedForSetComplete" },
            { TraceCode.TxCompletionStatusCompletedForTACOSC, "TxCompletionStatusCompletedForTACOSC" },
            { TraceCode.TxCompletionStatusCompletedForAsyncAbort, "TxCompletionStatusCompletedForAsyncAbort" },
            { TraceCode.TxCompletionStatusRemainsAttached, "TxCompletionStatusRemainsAttached" },
            { TraceCode.TxCompletionStatusAbortedOnSessionClose, "TxCompletionStatusAbortedOnSessionClose" },
            { TraceCode.TxReleaseServiceInstanceOnCompletion, "TxReleaseServiceInstanceOnCompletion" },
            { TraceCode.TxAsyncAbort, "TxAsyncAbort" },
            { TraceCode.TxFailedToNegotiateOleTx, "TxFailedToNegotiateOleTx" },
            { TraceCode.TxSourceTxScopeRequiredUsingExistingTransaction, "TxSourceTxScopeRequiredUsingExistingTransaction" },

            //CfxGreen trace codes (TraceCode.NetFx35)
            { TraceCode.ActivatingMessageReceived, "ActivatingMessageReceived" }, 
            { TraceCode.InstanceContextBoundToDurableInstance, "InstanceContextBoundToDurableInstance" },
            { TraceCode.InstanceContextDetachedFromDurableInstance, "InstanceContextDetachedFromDurableInstance" },
            { TraceCode.ContextChannelFactoryChannelCreated, "ContextChannelFactoryChannelCreated" },
            { TraceCode.ContextChannelListenerChannelAccepted, "ContextChannelListenerChannelAccepted" },
            { TraceCode.ContextProtocolContextAddedToMessage, "ContextProtocolContextAddedToMessage" },
            { TraceCode.ContextProtocolContextRetrievedFromMessage, "ContextProtocolContextRetrievedFromMessage" },
            { TraceCode.DICPInstanceContextCached, "DICPInstanceContextCached" },
            { TraceCode.DICPInstanceContextRemovedFromCache, "DICPInstanceContextRemovedFromCache" },
            { TraceCode.ServiceDurableInstanceDeleted, "ServiceDurableInstanceDeleted" },
            { TraceCode.ServiceDurableInstanceDisposed, "ServiceDurableInstanceDisposed" },
            { TraceCode.ServiceDurableInstanceLoaded, "ServiceDurableInstanceLoaded" },
            { TraceCode.ServiceDurableInstanceSaved, "ServiceDurableInstanceSaved" },
            { TraceCode.SqlPersistenceProviderSQLCallStart, "SqlPersistenceProviderSQLCallStart" },
            { TraceCode.SqlPersistenceProviderSQLCallEnd, "SqlPersistenceProviderSQLCallEnd" },
            { TraceCode.SqlPersistenceProviderOpenParameters, "SqlPersistenceProviderOpenParameters" },
            { TraceCode.SyncContextSchedulerServiceTimerCancelled, "SyncContextSchedulerServiceTimerCancelled" },
            { TraceCode.SyncContextSchedulerServiceTimerCreated, "SyncContextSchedulerServiceTimerCreated" },
            { TraceCode.WorkflowDurableInstanceLoaded, "WorkflowDurableInstanceLoaded" },
            { TraceCode.WorkflowDurableInstanceAborted, "WorkflowDurableInstanceAborted" },
            { TraceCode.WorkflowDurableInstanceActivated, "WorkflowDurableInstanceActivated" },
            { TraceCode.WorkflowOperationInvokerItemQueued, "WorkflowOperationInvokerItemQueued" },
            { TraceCode.WorkflowRequestContextReplySent, "WorkflowRequestContextReplySent" },
            { TraceCode.WorkflowRequestContextFaultSent, "WorkflowRequestContextFaultSent" },
            { TraceCode.WorkflowServiceHostCreated, "WorkflowServiceHostCreated" },
            { TraceCode.SyndicationReadFeedBegin, "SyndicationReadFeedBegin" },
            { TraceCode.SyndicationReadFeedEnd, "SyndicationReadFeedEnd" },
            { TraceCode.SyndicationReadItemBegin, "SyndicationReadItemBegin" },
            { TraceCode.SyndicationReadItemEnd, "SyndicationReadItemEnd" },
            { TraceCode.SyndicationWriteFeedBegin, "SyndicationWriteFeedBegin" },
            { TraceCode.SyndicationWriteFeedEnd, "SyndicationWriteFeedEnd" },
            { TraceCode.SyndicationWriteItemBegin, "SyndicationWriteItemBegin" },
            { TraceCode.SyndicationWriteItemEnd, "SyndicationWriteItemEnd" },
            { TraceCode.SyndicationProtocolElementIgnoredOnRead, "SyndicationProtocolElementIgnoredOnRead" },
            { TraceCode.SyndicationProtocolElementIgnoredOnWrite, "SyndicationProtocolElementIgnoredOnWrite" },
            { TraceCode.SyndicationProtocolElementInvalid, "SyndicationProtocolElementInvalid" },
            { TraceCode.WebUnknownQueryParameterIgnored, "WebUnknownQueryParameterIgnored" },
            { TraceCode.WebRequestMatchesOperation, "WebRequestMatchesOperation" },
            { TraceCode.WebRequestDoesNotMatchOperations, "WebRequestDoesNotMatchOperations" },
            { TraceCode.WebRequestRedirect, "WebRequestRedirect" },
            { TraceCode.SyndicationReadServiceDocumentBegin, "SyndicationReadServiceDocumentBegin" },
            { TraceCode.SyndicationReadServiceDocumentEnd, "SyndicationReadServiceDocumentEnd" },
            { TraceCode.SyndicationReadCategoriesDocumentBegin, "SyndicationReadCategoriesDocumentBegin" },
            { TraceCode.SyndicationReadCategoriesDocumentEnd, "SyndicationReadCategoriesDocumentEnd" },
            { TraceCode.SyndicationWriteServiceDocumentBegin, "SyndicationWriteServiceDocumentBegin" },
            { TraceCode.SyndicationWriteServiceDocumentEnd, "SyndicationWriteServiceDocumentEnd" },
            { TraceCode.SyndicationWriteCategoriesDocumentBegin, "SyndicationWriteCategoriesDocumentBegin" },
            { TraceCode.SyndicationWriteCategoriesDocumentEnd, "SyndicationWriteCategoriesDocumentEnd" },
            { TraceCode.AutomaticFormatSelectedOperationDefault, "AutomaticFormatSelectedOperationDefault" },
            { TraceCode.AutomaticFormatSelectedRequestBased, "AutomaticFormatSelectedRequestBased" },
            { TraceCode.RequestFormatSelectedFromContentTypeMapper, "RequestFormatSelectedFromContentTypeMapper" },
            { TraceCode.RequestFormatSelectedByEncoderDefaults, "RequestFormatSelectedByEncoderDefaults" },
            { TraceCode.AddingResponseToOutputCache, "AddingResponseToOutputCache" },
            { TraceCode.AddingAuthenticatedResponseToOutputCache, "AddingAuthenticatedResponseToOutputCache" },
            { TraceCode.JsonpCallbackNameSet, "JsonpCallbackNameSet" },
        };

        public const string E2EActivityId = "E2EActivityId";
        public const string TraceApplicationReference = "TraceApplicationReference";

        public static InputQueue<T> CreateInputQueue<T>() where T : class
        {
            if (asyncCallbackGenerator == null)
            {
                asyncCallbackGenerator = new Func<Action<AsyncCallback, IAsyncResult>>(CallbackGenerator);
            }

            return new InputQueue<T>(asyncCallbackGenerator)
            {
                DisposeItemCallback = value =>
                    {
                        if (value is ICommunicationObject)
                        {
                            ((ICommunicationObject)value).Abort();
                        }
                    }
            };
        }

        static Action<AsyncCallback, IAsyncResult> CallbackGenerator()
        {
            if (DiagnosticUtility.ShouldUseActivity)
            {
                ServiceModelActivity callbackActivity = ServiceModelActivity.Current;
                if (callbackActivity != null)
                {
                    return delegate(AsyncCallback callback, IAsyncResult result)
                        {
                            using (ServiceModelActivity.BoundOperation(callbackActivity))
                            {
                                callback(result);
                            }
                        };
                }
            }
            return null;
        }

        static internal void AddActivityHeader(Message message)
        {
            try
            {
                ActivityIdHeader activityIdHeader = new ActivityIdHeader(TraceUtility.ExtractActivityId(message));
                activityIdHeader.AddTo(message);
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                TraceUtility.TraceEvent(TraceEventType.Error, TraceCode.FailedToAddAnActivityIdHeader,
                    SR.GetString(SR.TraceCodeFailedToAddAnActivityIdHeader), e, message);
            }
        }

        static internal void AddAmbientActivityToMessage(Message message)
        {
            try
            {
                ActivityIdHeader activityIdHeader = new ActivityIdHeader(DiagnosticTraceBase.ActivityId);
                activityIdHeader.AddTo(message);
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                TraceUtility.TraceEvent(TraceEventType.Error, TraceCode.FailedToAddAnActivityIdHeader,
                    SR.GetString(SR.TraceCodeFailedToAddAnActivityIdHeader), e, message);
            }
        }

        static internal void CopyActivity(Message source, Message destination)
        {
            if (DiagnosticUtility.ShouldUseActivity)
            {
                TraceUtility.SetActivity(destination, TraceUtility.ExtractActivity(source));
            }
        }

        internal static long GetUtcBasedDurationForTrace(long startTicks)
        {
            if (startTicks > 0)
            {
                TimeSpan elapsedTime = new TimeSpan(DateTime.UtcNow.Ticks - startTicks);
                return (long)elapsedTime.TotalMilliseconds;
            }
            return 0;
        }

        internal static ServiceModelActivity ExtractActivity(Message message)
        {
            ServiceModelActivity retval = null;

            if ((DiagnosticUtility.ShouldUseActivity || TraceUtility.ShouldPropagateActivityGlobal) &&
                (message != null) &&
                (message.State != MessageState.Closed))
            {
                object property;

                if (message.Properties.TryGetValue(TraceUtility.ActivityIdKey, out property))
                {
                    retval = property as ServiceModelActivity;
                }
            }
            return retval;
        }

        internal static Guid ExtractActivityId(Message message)
        {
            if (TraceUtility.MessageFlowTracingOnly)
            {
                return ActivityIdHeader.ExtractActivityId(message);
            }

            ServiceModelActivity activity = ExtractActivity(message);
            return activity == null ? Guid.Empty : activity.Id;
        }

        internal static Guid GetReceivedActivityId(OperationContext operationContext)
        {
            object activityIdFromProprties;
            if (!operationContext.IncomingMessageProperties.TryGetValue(E2EActivityId, out activityIdFromProprties))
            {
                return TraceUtility.ExtractActivityId(operationContext.IncomingMessage);
            }
            else
            {
                return (Guid)activityIdFromProprties;
            }
        }

        internal static ServiceModelActivity ExtractAndRemoveActivity(Message message)
        {
            ServiceModelActivity retval = TraceUtility.ExtractActivity(message);
            if (retval != null)
            {
                // If the property is just removed, the item is disposed and we don't want the thing
                // to be disposed of.
                message.Properties[TraceUtility.ActivityIdKey] = false;
            }
            return retval;
        }

        internal static void ProcessIncomingMessage(Message message, EventTraceActivity eventTraceActivity)
        {
            ServiceModelActivity activity = ServiceModelActivity.Current;
            if (activity != null && DiagnosticUtility.ShouldUseActivity)
            {
                ServiceModelActivity incomingActivity = TraceUtility.ExtractActivity(message);
                if (null != incomingActivity && incomingActivity.Id != activity.Id)
                {
                    using (ServiceModelActivity.BoundOperation(incomingActivity))
                    {
                        if (null != FxTrace.Trace)
                        {
                            FxTrace.Trace.TraceTransfer(activity.Id);
                        }
                    }
                }
                TraceUtility.SetActivity(message, activity);
            }

            TraceUtility.MessageFlowAtMessageReceived(message, null, eventTraceActivity, true);

            if (MessageLogger.LogMessagesAtServiceLevel)
            {
                MessageLogger.LogMessage(ref message, MessageLoggingSource.ServiceLevelReceiveReply | MessageLoggingSource.LastChance);
            }
        }

        internal static void ProcessOutgoingMessage(Message message, EventTraceActivity eventTraceActivity)
        {
            ServiceModelActivity activity = ServiceModelActivity.Current;
            if (DiagnosticUtility.ShouldUseActivity)
            {
                TraceUtility.SetActivity(message, activity);
            }
            if (TraceUtility.PropagateUserActivity || TraceUtility.ShouldPropagateActivity)
            {
                TraceUtility.AddAmbientActivityToMessage(message);
            }

            TraceUtility.MessageFlowAtMessageSent(message, eventTraceActivity);

            if (MessageLogger.LogMessagesAtServiceLevel)
            {
                MessageLogger.LogMessage(ref message, MessageLoggingSource.ServiceLevelSendRequest | MessageLoggingSource.LastChance);
            }
        }

        internal static void SetActivity(Message message, ServiceModelActivity activity)
        {
            if (DiagnosticUtility.ShouldUseActivity && message != null && message.State != MessageState.Closed)
            {
                message.Properties[TraceUtility.ActivityIdKey] = activity;
            }
        }

        internal static void TraceDroppedMessage(Message message, EndpointDispatcher dispatcher)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                EndpointAddress endpointAddress = null;
                if (dispatcher != null)
                {
                    endpointAddress = dispatcher.EndpointAddress;
                }
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.DroppedAMessage,
                    SR.GetString(SR.TraceCodeDroppedAMessage), new MessageDroppedTraceRecord(message, endpointAddress));
            }
        }

        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription)
        {
            TraceEvent(severity, traceCode, traceDescription, null, traceDescription, (Exception)null);
        }

        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription, TraceRecord extendedData)
        {
            TraceEvent(severity, traceCode, traceDescription, extendedData, null, (Exception)null);
        }

        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription, object source)
        {
            TraceEvent(severity, traceCode, traceDescription, null, source, (Exception)null);
        }

        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription, object source, Exception exception)
        {
            TraceEvent(severity, traceCode, traceDescription, null, source, exception);
        }

        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription, Message message)
        {
            if (message == null)
            {
                TraceEvent(severity, traceCode, traceDescription, null, (Exception)null);
            }
            else
            {
                TraceEvent(severity, traceCode, traceDescription, message, message);
            }
        }

        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription, object source, Message message)
        {
            Guid activityId = TraceUtility.ExtractActivityId(message);
            if (DiagnosticUtility.ShouldTrace(severity))
            {
                DiagnosticUtility.DiagnosticTrace.TraceEvent(severity, traceCode, GenerateMsdnTraceCode(traceCode), traceDescription, new MessageTraceRecord(message), null, activityId, message);
            }
        }

        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription, Exception exception, Message message)
        {
            Guid activityId = TraceUtility.ExtractActivityId(message);
            if (DiagnosticUtility.ShouldTrace(severity))
            {
                DiagnosticUtility.DiagnosticTrace.TraceEvent(severity, traceCode, GenerateMsdnTraceCode(traceCode), traceDescription, new MessageTraceRecord(message), exception, activityId, null);
            }
        }

        internal static void TraceEventNoCheck(TraceEventType severity, int traceCode, string traceDescription, TraceRecord extendedData, object source, Exception exception)
        {
            DiagnosticUtility.DiagnosticTrace.TraceEvent(severity, traceCode, GenerateMsdnTraceCode(traceCode), traceDescription, extendedData, exception, source);
        }

        // These methods require a TraceRecord to be allocated, so we want them to show up on profiles if the caller didn't avoid
        // allocating the TraceRecord by using ShouldTrace.
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription, TraceRecord extendedData, object source, Exception exception)
        {
            if (DiagnosticUtility.ShouldTrace(severity))
            {
                DiagnosticUtility.DiagnosticTrace.TraceEvent(severity, traceCode, GenerateMsdnTraceCode(traceCode), traceDescription, extendedData, exception, source);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription, TraceRecord extendedData, object source, Exception exception, Message message)
        {
            Guid activityId = TraceUtility.ExtractActivityId(message);
            if (DiagnosticUtility.ShouldTrace(severity))
            {
                DiagnosticUtility.DiagnosticTrace.TraceEvent(severity, traceCode, GenerateMsdnTraceCode(traceCode),
                    traceDescription, extendedData, exception, activityId, source);
            }
        }

        internal static void TraceEventNoCheck(TraceEventType severity, int traceCode, string traceDescription, TraceRecord extendedData, object source, Exception exception, Guid activityId)
        {
            DiagnosticUtility.DiagnosticTrace.TraceEvent(severity, traceCode, GenerateMsdnTraceCode(traceCode),
                traceDescription, extendedData, exception, activityId, source);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription, TraceRecord extendedData, object source, Exception exception, Guid activityId)
        {
            if (DiagnosticUtility.ShouldTrace(severity))
            {
                DiagnosticUtility.DiagnosticTrace.TraceEvent(severity, traceCode, GenerateMsdnTraceCode(traceCode),
                    traceDescription, extendedData, exception, activityId, source);
            }
        }

        static string GenerateMsdnTraceCode(int traceCode)
        {
            int group = (int)(traceCode & 0xFFFF0000);
            string terminatorUri = null;
            switch (group)
            {
                case TraceCode.Administration:
                    terminatorUri = "System.ServiceModel.Administration";
                    break;
                case TraceCode.Channels:
                    terminatorUri = "System.ServiceModel.Channels";
                    break;
                case TraceCode.ComIntegration:
                    terminatorUri = "System.ServiceModel.ComIntegration";
                    break;
                case TraceCode.Diagnostics:
                    terminatorUri = "System.ServiceModel.Diagnostics";
                    break;
                case TraceCode.PortSharing:
                    terminatorUri = "System.ServiceModel.PortSharing";
                    break;
                case TraceCode.Security:
                    terminatorUri = "System.ServiceModel.Security";
                    break;
                case TraceCode.Serialization:
                    terminatorUri = "System.Runtime.Serialization";
                    break;
                case TraceCode.ServiceModel:
                case TraceCode.ServiceModelTransaction:
                    terminatorUri = "System.ServiceModel";
                    break;
                default:
                    terminatorUri = string.Empty;
                    break;
            }

            Fx.Assert(traceCodes.ContainsKey(traceCode),
                string.Format(CultureInfo.InvariantCulture, "Unsupported trace code: Please add trace code 0x{0} to the SortedList TraceUtility.traceCodes in {1}",
                traceCode.ToString("X", CultureInfo.InvariantCulture), typeof(TraceUtility)));
            return LegacyDiagnosticTrace.GenerateMsdnTraceCode(terminatorUri, traceCodes[traceCode]);
        }

        internal static Exception ThrowHelperError(Exception exception, Message message)
        {
            // If the message is closed, we won't get an activity
            Guid activityId = TraceUtility.ExtractActivityId(message);
            if (DiagnosticUtility.ShouldTraceError)
            {
                DiagnosticUtility.DiagnosticTrace.TraceEvent(TraceEventType.Error, TraceCode.ThrowingException, GenerateMsdnTraceCode(TraceCode.ThrowingException),
                    TraceSR.GetString(TraceSR.ThrowingException), null, exception, activityId, null);
            }
            return exception;
        }

        internal static Exception ThrowHelperError(Exception exception, Guid activityId, object source)
        {
            if (DiagnosticUtility.ShouldTraceError)
            {
                DiagnosticUtility.DiagnosticTrace.TraceEvent(TraceEventType.Error, TraceCode.ThrowingException, GenerateMsdnTraceCode(TraceCode.ThrowingException),
                    TraceSR.GetString(TraceSR.ThrowingException), null, exception, activityId, source);
            }
            return exception;
        }

        internal static Exception ThrowHelperWarning(Exception exception, Message message)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                Guid activityId = TraceUtility.ExtractActivityId(message);
                DiagnosticUtility.DiagnosticTrace.TraceEvent(TraceEventType.Warning, TraceCode.ThrowingException, GenerateMsdnTraceCode(TraceCode.ThrowingException),
                    TraceSR.GetString(TraceSR.ThrowingException), null, exception, activityId, null);
            }
            return exception;
        }

        internal static ArgumentException ThrowHelperArgument(string paramName, string message, Message msg)
        {
            return (ArgumentException)TraceUtility.ThrowHelperError(new ArgumentException(message, paramName), msg);
        }

        internal static ArgumentNullException ThrowHelperArgumentNull(string paramName, Message message)
        {
            return (ArgumentNullException)TraceUtility.ThrowHelperError(new ArgumentNullException(paramName), message);
        }

        internal static string CreateSourceString(object source)
        {
            return source.GetType().ToString() + "/" + source.GetHashCode().ToString(CultureInfo.CurrentCulture);
        }

        internal static void TraceHttpConnectionInformation(string localEndpoint, string remoteEndpoint, object source)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                Dictionary<string, string> values = new Dictionary<string, string>(2)
                {
                    { "LocalEndpoint", localEndpoint },
                    { "RemoteEndpoint", remoteEndpoint }
                };
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.ConnectToIPEndpoint,
                    SR.GetString(SR.TraceCodeConnectToIPEndpoint), new DictionaryTraceRecord(values), source, null);
            }
        }

        internal static void TraceUserCodeException(Exception e, MethodInfo method)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                StringTraceRecord record = new StringTraceRecord("Comment",
                    SR.GetString(SR.SFxUserCodeThrewException, method.DeclaringType.FullName, method.Name));
                DiagnosticUtility.DiagnosticTrace.TraceEvent(TraceEventType.Warning,
                    TraceCode.UnhandledExceptionInUserOperation, GenerateMsdnTraceCode(TraceCode.UnhandledExceptionInUserOperation),
                    SR.GetString(SR.TraceCodeUnhandledExceptionInUserOperation, method.DeclaringType.FullName, method.Name),
                    record,
                    e, null);
            }
        }

        static TraceUtility()
        {
            //Maintain the order of calls
            TraceUtility.SetEtwProviderId();
            TraceUtility.SetEndToEndTracingFlags();
            if (DiagnosticUtility.DiagnosticTrace != null)
            {
                DiagnosticTraceSource ts = (DiagnosticTraceSource)DiagnosticUtility.DiagnosticTrace.TraceSource;
                TraceUtility.shouldPropagateActivity = (ts.PropagateActivity || TraceUtility.shouldPropagateActivityGlobal);
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls critical method DiagnosticSection.UnsafeGetSection.",
            Safe = "Doesn't leak config section instance, just reads and stores bool values.")]
        [SecuritySafeCritical]
        static void SetEndToEndTracingFlags()
        {
            EndToEndTracingElement element = DiagnosticSection.UnsafeGetSection().EndToEndTracing;
            TraceUtility.shouldPropagateActivityGlobal = element.PropagateActivity;
            // if Sys.Diag trace is not enabled then the value is true if shouldPropagateActivityGlobal is true
            TraceUtility.shouldPropagateActivity = TraceUtility.shouldPropagateActivityGlobal || TraceUtility.shouldPropagateActivity;

            //Activity tracing is enabled by either of the flags (Sys.Diag trace source or E2E config element)
            DiagnosticUtility.ShouldUseActivity = (DiagnosticUtility.ShouldUseActivity || element.ActivityTracing);
            TraceUtility.activityTracing = DiagnosticUtility.ShouldUseActivity;

            TraceUtility.messageFlowTracing = element.MessageFlowTracing || TraceUtility.activityTracing;
            TraceUtility.messageFlowTracingOnly = element.MessageFlowTracing && !element.ActivityTracing;

            //Set the flag if activity tracing is enabled through the E2E config element as well
            DiagnosticUtility.TracingEnabled = (DiagnosticUtility.TracingEnabled || TraceUtility.activityTracing);
        }

        static public long RetrieveMessageNumber()
        {
            return Interlocked.Increment(ref TraceUtility.messageNumber);
        }

        static public bool PropagateUserActivity
        {
            get
            {
                return TraceUtility.ShouldPropagateActivity &&
                    TraceUtility.PropagateUserActivityCore;
            }
        }

        // Most of the time, shouldPropagateActivity will be false.
        // This property will rarely be executed as a result. 
        static bool PropagateUserActivityCore
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get
            {
                return !(DiagnosticUtility.TracingEnabled) &&
                    DiagnosticTraceBase.ActivityId != Guid.Empty;
            }
        }

        static internal string GetCallerInfo(OperationContext context)
        {
            if (context != null && context.IncomingMessageProperties != null)
            {
                object endpointMessageProperty;
                if (context.IncomingMessageProperties.TryGetValue(RemoteEndpointMessageProperty.Name, out endpointMessageProperty))
                {
                    RemoteEndpointMessageProperty endpoint = endpointMessageProperty as RemoteEndpointMessageProperty;
                    if (endpoint != null)
                    {
                        return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", endpoint.Address, endpoint.Port);
                    }
                }
            }
            return "null";
        }

        [Fx.Tag.SecurityNote(Critical = "Calls critical method DiagnosticSection.UnsafeGetSection.",
            Safe = "Doesn't leak config section instance, just reads and stores string values for Guid")]
        [SecuritySafeCritical]
        static internal void SetEtwProviderId()
        {
            // Get section should not trace as the ETW provider id is not set yet
            DiagnosticSection diagnostics = DiagnosticSection.UnsafeGetSectionNoTrace();
            Guid etwProviderId = Guid.Empty;
            //set the Id in PT if specified in the config file. If not, ETW tracing is off. 
            if (PartialTrustHelpers.HasEtwPermissions() || diagnostics.IsEtwProviderIdFromConfigFile())
            {
                etwProviderId = Fx.CreateGuid(diagnostics.EtwProviderId);
            }
            System.Runtime.Diagnostics.EtwDiagnosticTrace.DefaultEtwProviderId = etwProviderId;
        }

        static internal void SetActivityId(MessageProperties properties)
        {
            Guid activityId;
            if ((null != properties) && properties.TryGetValue(TraceUtility.E2EActivityId, out activityId))
            {
                DiagnosticTraceBase.ActivityId = activityId;
            }
        }

        static internal bool ShouldPropagateActivity
        {
            get { return TraceUtility.shouldPropagateActivity; }
        }

        static internal bool ShouldPropagateActivityGlobal
        {
            get { return TraceUtility.shouldPropagateActivityGlobal; }
        }

        static internal bool ActivityTracing
        {
            get { return TraceUtility.activityTracing; }
        }

        static internal bool MessageFlowTracing
        {
            get { return TraceUtility.messageFlowTracing; }
        }

        static internal bool MessageFlowTracingOnly
        {
            get { return TraceUtility.messageFlowTracingOnly; }
        }

        static internal void MessageFlowAtMessageSent(Message message, EventTraceActivity eventTraceActivity)
        {
            if (TraceUtility.MessageFlowTracing)
            {
                Guid activityId;
                Guid correlationId;
                bool activityIdFound = ActivityIdHeader.ExtractActivityAndCorrelationId(message, out activityId, out correlationId);

                if (TraceUtility.MessageFlowTracingOnly)
                {
                    if (activityIdFound && activityId != DiagnosticTraceBase.ActivityId)
                    {
                        DiagnosticTraceBase.ActivityId = activityId;
                    }
                }

                if (TD.MessageSentToTransportIsEnabled())
                {
                    TD.MessageSentToTransport(eventTraceActivity, correlationId);
                }
            }
        }

        static internal void MessageFlowAtMessageReceived(Message message, OperationContext context, EventTraceActivity eventTraceActivity, bool createNewActivityId)
        {
            if (TraceUtility.MessageFlowTracing)
            {
                Guid activityId;
                Guid correlationId;
                bool activityIdFound = ActivityIdHeader.ExtractActivityAndCorrelationId(message, out activityId, out correlationId);
                if (TraceUtility.MessageFlowTracingOnly)
                {
                    if (createNewActivityId)
                    {
                        if (!activityIdFound)
                        {
                            activityId = Guid.NewGuid();
                            activityIdFound = true;
                        }
                        //message flow tracing only - start fresh
                        DiagnosticTraceBase.ActivityId = Guid.Empty;
                    }

                    if (activityIdFound)
                    {
                        FxTrace.Trace.SetAndTraceTransfer(activityId, !createNewActivityId);
                        message.Properties[TraceUtility.E2EActivityId] = Trace.CorrelationManager.ActivityId;
                    }
                }
                if (TD.MessageReceivedFromTransportIsEnabled())
                {
                    if (context == null)
                    {
                        context = OperationContext.Current;
                    }

                    TD.MessageReceivedFromTransport(eventTraceActivity, correlationId, TraceUtility.GetAnnotation(context));
                }
            }
        }

        internal static string GetAnnotation(OperationContext context)
        {
            object hostReference;
            if (context != null && null != context.IncomingMessage && (MessageState.Closed != context.IncomingMessage.State))
            {
                if (!context.IncomingMessageProperties.TryGetValue(TraceApplicationReference, out hostReference))
                {
                    hostReference = AspNetEnvironment.Current.GetAnnotationFromHost(context.Host);
                    context.IncomingMessageProperties.Add(TraceApplicationReference, hostReference);
                }
            }
            else
            {
                hostReference = AspNetEnvironment.Current.GetAnnotationFromHost(null);
            }
            return (string)hostReference;
        }

        internal static void TransferFromTransport(Message message)
        {
            if (message != null && DiagnosticUtility.ShouldUseActivity)
            {
                Guid guid = Guid.Empty;

                // Only look if we are allowing user propagation
                if (TraceUtility.ShouldPropagateActivity)
                {
                    guid = ActivityIdHeader.ExtractActivityId(message);
                }

                if (guid == Guid.Empty)
                {
                    guid = Guid.NewGuid();
                }

                ServiceModelActivity activity = null;
                bool emitStart = true;
                if (ServiceModelActivity.Current != null)
                {
                    if ((ServiceModelActivity.Current.Id == guid) ||
                        (ServiceModelActivity.Current.ActivityType == ActivityType.ProcessAction))
                    {
                        activity = ServiceModelActivity.Current;
                        emitStart = false;
                    }
                    else if (ServiceModelActivity.Current.PreviousActivity != null &&
                        ServiceModelActivity.Current.PreviousActivity.Id == guid)
                    {
                        activity = ServiceModelActivity.Current.PreviousActivity;
                        emitStart = false;
                    }
                }

                if (activity == null)
                {
                    activity = ServiceModelActivity.CreateActivity(guid);
                }
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    if (emitStart)
                    {
                        if (null != FxTrace.Trace)
                        {
                            FxTrace.Trace.TraceTransfer(guid);
                        }
                        ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityProcessAction, message.Headers.Action), ActivityType.ProcessAction);
                    }
                }
                message.Properties[TraceUtility.ActivityIdKey] = activity;
            }
        }

        static internal void UpdateAsyncOperationContextWithActivity(object activity)
        {
            if (OperationContext.Current != null && activity != null)
            {
                OperationContext.Current.OutgoingMessageProperties[TraceUtility.AsyncOperationActivityKey] = activity;
            }
        }

        static internal object ExtractAsyncOperationContextActivity()
        {
            object data = null;
            if (OperationContext.Current != null && OperationContext.Current.OutgoingMessageProperties.TryGetValue(TraceUtility.AsyncOperationActivityKey, out data))
            {
                OperationContext.Current.OutgoingMessageProperties.Remove(TraceUtility.AsyncOperationActivityKey);
            }
            return data;
        }

        static internal void UpdateAsyncOperationContextWithStartTime(EventTraceActivity eventTraceActivity, long startTime)
        {
            if (OperationContext.Current != null)
            {
                OperationContext.Current.OutgoingMessageProperties[TraceUtility.AsyncOperationStartTimeKey] = new EventTraceActivityTimeProperty(eventTraceActivity, startTime);
            }
        }

        static internal void ExtractAsyncOperationStartTime(out EventTraceActivity eventTraceActivity, out long startTime)
        {
            EventTraceActivityTimeProperty data = null;
            eventTraceActivity = null;
            startTime = 0;
            if (OperationContext.Current != null && OperationContext.Current.OutgoingMessageProperties.TryGetValue<EventTraceActivityTimeProperty>(TraceUtility.AsyncOperationStartTimeKey, out data))
            {
                OperationContext.Current.OutgoingMessageProperties.Remove(TraceUtility.AsyncOperationStartTimeKey);
                eventTraceActivity = data.EventTraceActivity;
                startTime = data.StartTime;
            }
        }

        internal class TracingAsyncCallbackState
        {
            object innerState;
            Guid activityId;

            internal TracingAsyncCallbackState(object innerState)
            {
                this.innerState = innerState;
                this.activityId = DiagnosticTraceBase.ActivityId;
            }

            internal object InnerState
            {
                get { return this.innerState; }
            }

            internal Guid ActivityId
            {
                get { return this.activityId; }
            }
        }

        internal static AsyncCallback WrapExecuteUserCodeAsyncCallback(AsyncCallback callback)
        {
            return (DiagnosticUtility.ShouldUseActivity && callback != null) ?
                (new ExecuteUserCodeAsync(callback)).Callback
                : callback;
        }

        sealed class ExecuteUserCodeAsync
        {
            AsyncCallback callback;

            public ExecuteUserCodeAsync(AsyncCallback callback)
            {
                this.callback = callback;
            }

            public AsyncCallback Callback
            {
                get
                {
                    return Fx.ThunkCallback(new AsyncCallback(this.ExecuteUserCode));
                }
            }

            void ExecuteUserCode(IAsyncResult result)
            {
                using (ServiceModelActivity activity = ServiceModelActivity.CreateBoundedActivity())
                {
                    ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityCallback), ActivityType.ExecuteUserCode);
                    this.callback(result);
                }
            }
        }
        

        class EventTraceActivityTimeProperty
        {
            long startTime;
            EventTraceActivity eventTraceActivity;

            public EventTraceActivityTimeProperty(EventTraceActivity eventTraceActivity, long startTime)
            {
                this.eventTraceActivity = eventTraceActivity;
                this.startTime = startTime;
            }

            internal long StartTime
            {
                get { return this.startTime; }
            }
            internal EventTraceActivity EventTraceActivity
            {
                get { return this.eventTraceActivity; }
            }
        }


        internal static string GetRemoteEndpointAddressPort(Net.IPEndPoint iPEndPoint)
        {
            //We really don't want any exceptions out of TraceUtility.
            if (iPEndPoint != null)
            {
                try
                {
                    return iPEndPoint.Address.ToString() + ":" + iPEndPoint.Port;
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    //ignore and continue with all non-fatal exceptions.
                }
            }

            return string.Empty;
        }

        internal static string GetRemoteEndpointAddressPort(RemoteEndpointMessageProperty remoteEndpointMessageProperty)
        {
            try
            {
                if (remoteEndpointMessageProperty != null)
                {
                    return remoteEndpointMessageProperty.Address + ":" + remoteEndpointMessageProperty.Port;
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                //ignore and continue with all non-fatal exceptions.
            }

            return string.Empty;
        }
    }
}
