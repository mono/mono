//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    // 

    [Obsolete("This has been replaced by System.Runtime.Diagnostics.EventLogEventId")]
    enum EventLogEventId : uint
    {
        // EventIDs from shared Diagnostics and Reliability code
        FailedToSetupTracing = 0xC0010064,
        FailedToInitializeTraceSource,
        FailFast,
        FailFastException,
        FailedToTraceEvent,
        FailedToTraceEventWithException,
        InvariantAssertionFailed,
        PiiLoggingOn,
        PiiLoggingNotAllowed,

        // ServiceModel EventIDs
        WebHostUnhandledException = 0xC0020001,
        WebHostHttpError,
        WebHostFailedToProcessRequest,
        WebHostFailedToListen,
        FailedToLogMessage,
        RemovedBadFilter,
        FailedToCreateMessageLoggingTraceSource,
        MessageLoggingOn,
        MessageLoggingOff,
        FailedToLoadPerformanceCounter,
        FailedToRemovePerformanceCounter,
        WmiGetObjectFailed,
        WmiPutInstanceFailed,
        WmiDeleteInstanceFailed,
        WmiCreateInstanceFailed,
        WmiExecQueryFailed,
        WmiExecMethodFailed,
        WmiRegistrationFailed,
        WmiUnregistrationFailed,
        WmiAdminTypeMismatch,
        WmiPropertyMissing,
        ComPlusServiceHostStartingServiceError,
        ComPlusDllHostInitializerStartingError,
        ComPlusTLBImportError,
        ComPlusInvokingMethodFailed,
        ComPlusInstanceCreationError,
        ComPlusInvokingMethodFailedMismatchedTransactions,

        // TransactionBridge
        UnhandledStateMachineExceptionRecordDescription = 0xC0030001,
        FatalUnexpectedStateMachineEvent,
        ParticipantRecoveryLogEntryCorrupt,
        CoordinatorRecoveryLogEntryCorrupt,
        CoordinatorRecoveryLogEntryCreationFailure,
        ParticipantRecoveryLogEntryCreationFailure,
        ProtocolInitializationFailure,
        ProtocolStartFailure,
        ProtocolRecoveryBeginningFailure,
        ProtocolRecoveryCompleteFailure,
        TransactionBridgeRecoveryFailure,
        ProtocolStopFailure,
        NonFatalUnexpectedStateMachineEvent,
        PerformanceCounterInitializationFailure,
        ProtocolRecoveryComplete,
        ProtocolStopped,
        ThumbPrintNotFound,
        ThumbPrintNotValidated,
        SslNoPrivateKey,
        SslNoAccessiblePrivateKey,
        MissingNecessaryKeyUsage,
        MissingNecessaryEnhancedKeyUsage,

        // SMSvcHost
        StartErrorPublish = 0xC0040001,
        BindingError,
        LAFailedToListenForApp,
        UnknownListenerAdapterError,
        WasDisconnected,
        WasConnectionTimedout,
        ServiceStartFailed,
        MessageQueueDuplicatedSocketLeak,
        MessageQueueDuplicatedPipeLeak,
        SharingUnhandledException,

        // SecurityAudit
        ServiceAuthorizationSuccess = 0x40060001,
        ServiceAuthorizationFailure = 0xC0060002,
        MessageAuthenticationSuccess = 0x40060003,
        MessageAuthenticationFailure = 0xC0060004,
        SecurityNegotiationSuccess = 0x40060005,
        SecurityNegotiationFailure = 0xC0060006,
        TransportAuthenticationSuccess = 0x40060007,
        TransportAuthenticationFailure = 0xC0060008,
        ImpersonationSuccess = 0x40060009,
        ImpersonationFailure = 0xC006000A
    }
}
