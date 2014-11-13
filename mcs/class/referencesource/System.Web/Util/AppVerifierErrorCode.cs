namespace System.Web.Util {
    using System;

    // IMPORTANT: Each error code must be used in *exactly one place* in order to make
    // diagnosing the failure easier.

    internal enum AppVerifierErrorCode {

        Ok,

        /* ASYNC OPERATIONS */
        HttpApplicationInstanceWasNull,
        BeginHandlerDelegateWasNull,
        AsyncCallbackInvokedMultipleTimes,
        AsyncCallbackInvokedWithNullParameter,
        AsyncCallbackGivenAsyncResultWhichWasNotCompleted,
        AsyncCallbackInvokedSynchronouslyButAsyncResultWasNotMarkedCompletedSynchronously,
        AsyncCallbackInvokedAsynchronouslyButAsyncResultWasMarkedCompletedSynchronously,
        AsyncCallbackInvokedWithUnexpectedAsyncResultInstance,
        AsyncCallbackInvokedAsynchronouslyThenBeginHandlerThrew,
        BeginHandlerThrewThenAsyncCallbackInvokedAsynchronously,
        AsyncCallbackInvokedSynchronouslyThenBeginHandlerThrew,
        AsyncCallbackInvokedWithUnexpectedAsyncResultAsyncState,
        AsyncCallbackCalledAfterHttpApplicationReassigned,
        BeginHandlerReturnedNull,
        BeginHandlerReturnedAsyncResultMarkedCompletedSynchronouslyButWhichWasNotCompleted,
        BeginHandlerReturnedAsyncResultMarkedCompletedSynchronouslyButAsyncCallbackNeverCalled,
        BeginHandlerReturnedUnexpectedAsyncResultInstance,
        BeginHandlerReturnedUnexpectedAsyncResultAsyncState,
        SyncContextSendOrPostCalledAfterRequestCompleted

    }
}
