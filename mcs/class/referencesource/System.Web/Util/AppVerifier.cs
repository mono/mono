namespace System.Web.Util {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Web;

    internal static class AppVerifier {

        // It's possible that multiple error codes might get mapped to the same description string.
        // This can happen if there might be multiple ways for a single problem to manifest itself.
        private static readonly Dictionary<AppVerifierErrorCode, string> _errorStringMappings = new Dictionary<AppVerifierErrorCode, string>() {
            { AppVerifierErrorCode.HttpApplicationInstanceWasNull, SR.AppVerifier_Errors_HttpApplicationInstanceWasNull },
            { AppVerifierErrorCode.BeginHandlerDelegateWasNull, SR.AppVerifier_Errors_BeginHandlerDelegateWasNull },
            { AppVerifierErrorCode.AsyncCallbackInvokedMultipleTimes, SR.AppVerifier_Errors_AsyncCallbackInvokedMultipleTimes },
            { AppVerifierErrorCode.AsyncCallbackInvokedWithNullParameter, SR.AppVerifier_Errors_AsyncCallbackInvokedWithNullParameter },
            { AppVerifierErrorCode.AsyncCallbackGivenAsyncResultWhichWasNotCompleted, SR.AppVerifier_Errors_AsyncCallbackGivenAsyncResultWhichWasNotCompleted },
            { AppVerifierErrorCode.AsyncCallbackInvokedSynchronouslyButAsyncResultWasNotMarkedCompletedSynchronously, SR.AppVerifier_Errors_AsyncCallbackInvokedSynchronouslyButAsyncResultWasNotMarkedCompletedSynchronously },
            { AppVerifierErrorCode.AsyncCallbackInvokedAsynchronouslyButAsyncResultWasMarkedCompletedSynchronously, SR.AppVerifier_Errors_AsyncCallbackInvokedAsynchronouslyButAsyncResultWasMarkedCompletedSynchronously },
            { AppVerifierErrorCode.AsyncCallbackInvokedWithUnexpectedAsyncResultInstance, SR.AppVerifier_Errors_AsyncCallbackInvokedWithUnexpectedAsyncResultInstance },
            { AppVerifierErrorCode.AsyncCallbackInvokedAsynchronouslyThenBeginHandlerThrew, SR.AppVerifier_Errors_AsyncCallbackInvokedEvenThoughBeginHandlerThrew },
            { AppVerifierErrorCode.BeginHandlerThrewThenAsyncCallbackInvokedAsynchronously, SR.AppVerifier_Errors_AsyncCallbackInvokedEvenThoughBeginHandlerThrew },
            { AppVerifierErrorCode.AsyncCallbackInvokedSynchronouslyThenBeginHandlerThrew, SR.AppVerifier_Errors_AsyncCallbackInvokedEvenThoughBeginHandlerThrew },
            { AppVerifierErrorCode.AsyncCallbackInvokedWithUnexpectedAsyncResultAsyncState, SR.AppVerifier_Errors_AsyncCallbackInvokedWithUnexpectedAsyncResultAsyncState },
            { AppVerifierErrorCode.AsyncCallbackCalledAfterHttpApplicationReassigned, SR.AppVerifier_Errors_AsyncCallbackCalledAfterHttpApplicationReassigned },
            { AppVerifierErrorCode.BeginHandlerReturnedNull, SR.AppVerifier_Errors_BeginHandlerReturnedNull },
            { AppVerifierErrorCode.BeginHandlerReturnedAsyncResultMarkedCompletedSynchronouslyButWhichWasNotCompleted, SR.AppVerifier_Errors_BeginHandlerReturnedAsyncResultMarkedCompletedSynchronouslyButWhichWasNotCompleted },
            { AppVerifierErrorCode.BeginHandlerReturnedAsyncResultMarkedCompletedSynchronouslyButAsyncCallbackNeverCalled, SR.AppVerifier_Errors_BeginHandlerReturnedAsyncResultMarkedCompletedSynchronouslyButAsyncCallbackNeverCalled },
            { AppVerifierErrorCode.BeginHandlerReturnedUnexpectedAsyncResultInstance, SR.AppVerifier_Errors_AsyncCallbackInvokedWithUnexpectedAsyncResultInstance },
            { AppVerifierErrorCode.BeginHandlerReturnedUnexpectedAsyncResultAsyncState, SR.AppVerifier_Errors_BeginHandlerReturnedUnexpectedAsyncResultAsyncState },
            { AppVerifierErrorCode.SyncContextSendOrPostCalledAfterRequestCompleted, SR.AppVerifier_Errors_SyncContextSendOrPostCalledAfterRequestCompleted },
        };

        // Provides an option for different wrappers to specify whether to collect the call stacks traces
        [FlagsAttribute]
        internal enum CallStackCollectionBitMasks : int {
            AllMask = -1,

            // used for a 3-parameter Begin* method [(T, AsyncCallback, object) -> IAsyncResult] wrapper
            BeginCallHandlerMask = 1,
            CallHandlerCallbackMask = 2,

            // used for a BeginEventHandler method [(object, sender, EventArgs, object) -> IAsyncResult] wrapper
            BeginExecutionStepMask = 4,
            ExecutionStepCallbackMask = 8,

            // when adding new bits above also update the following:
            AllHandlerMask = BeginCallHandlerMask | CallHandlerCallbackMask,
            AllStepMask = BeginExecutionStepMask | ExecutionStepCallbackMask,
        
            AllBeginMask = BeginCallHandlerMask | BeginExecutionStepMask,
            AllCallbackMask = CallHandlerCallbackMask | ExecutionStepCallbackMask
        };

        // The declarative order of these two fields is important; don't swap them!
        private static Action<AppVerifierException> DefaultAppVerifierBehavior = GetAppVerifierBehaviorFromRegistry();
        private static readonly bool IsAppVerifierEnabled = (DefaultAppVerifierBehavior != null);
        private static long AppVerifierErrorCodeCollectCallStackMask;
        private static long AppVerifierErrorCodeEnableAssertMask;
        private static CallStackCollectionBitMasks AppVerifierCollectCallStackMask;

        private delegate void AssertDelegate(bool condition, AppVerifierErrorCode errorCode);

        // Returns an AppVerifier handler (something that can record exceptions appropriately)
        // appropriate to what was set in the system registry. If the key we're looking for
        // doesn't exist or doesn't have a known value, we return 'null', signifying that
        // AppVerifier is disabled.
        private static Action<AppVerifierException> GetAppVerifierBehaviorFromRegistry() {
            // use 0 as the default value if the key doesn't exist or is of the wrong type
            int valueFromRegistry = (Misc.GetAspNetRegValue(subKey: null, valueName: "RuntimeVerificationBehavior", defaultValue: null) as int?) ?? 0;

            // REG_QWORD used as a mask to disable individual asserts. No key means all asserts are enabled
            AppVerifierErrorCodeEnableAssertMask = (Misc.GetAspNetRegValue(subKey: null, valueName: "AppVerifierErrorCodeEnableAssertMask", defaultValue: (long)(-1)) as long?) ?? (long)(-1);

            // REG_QWORD used as a mask to control call stack collection on individual asserts (useful if we event log only). No key means all asserts will collect stack traces
            AppVerifierErrorCodeCollectCallStackMask = (Misc.GetAspNetRegValue(subKey: null, valueName: "AppVerifierErrorCodeCollectCallstackMask", defaultValue: (long)(-1)) as long?) ?? (long)(-1);

            // REG_DWORD mask to disable call stack collection on begin* / end* methods. No key means all call stacks are collected
            AppVerifierCollectCallStackMask = (CallStackCollectionBitMasks)((Misc.GetAspNetRegValue(subKey: null, valueName: "AppVerifierCollectCallStackMask", defaultValue: (int)(-1)) as int?) ?? (int)(-1));
            
            switch (valueFromRegistry) {
                case 1:
                    // Just write to the event log
                    return WriteToEventLog;

                case 2:
                    // Write to the event log and Debugger.Launch / Debugger.Break
                    return WriteToEventLogAndSoftBreak;

                case 3:
                    // Write to the event log and kernel32!DebugBreak
                    return WriteToEventLogAndHardBreak;

                default:
                    // not enabled
                    return null;
            }
        }

        // Writes an exception to the Windows Event Log (Application section)
        private static void WriteToEventLog(AppVerifierException ex) {
            Misc.WriteUnhandledExceptionToEventLog(AppDomain.CurrentDomain, ex); // method won't throw
        }

        [SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)] // safe since AppVerifier can only be enabled via registry, which already requires admin privileges
        private static void WriteToEventLogAndSoftBreak(AppVerifierException ex) {
            // A "soft" break means that we prompt to launch a debugger, and if one is attached we'll signal it.
            WriteToEventLog(ex);
            if (Debugger.Launch()) {
                Debugger.Break();
            }
        }

        [SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)] // safe since AppVerifier can only be enabled via registry, which already requires admin privileges
        private static void WriteToEventLogAndHardBreak(AppVerifierException ex) {
            // A "hard" break means that we'll signal any attached debugger, and if none is attached
            // we'll just INT 3 and hope for the best. (This may cause a Watson dump depending on environment.)
            WriteToEventLog(ex);
            if (Debugger.IsAttached) {
                Debugger.Break();
            }
            else {
                NativeMethods.DebugBreak();
            }
        }

        // Instruments a 3-parameter Begin* method [(T, AsyncCallback, object) -> IAsyncResult].
        // If AppVerifier is disabled, returns the original method unmodified.
        public static Func<T, AsyncCallback, object, IAsyncResult> WrapBeginMethod<T>(HttpApplication httpApplication, Func<T, AsyncCallback, object, IAsyncResult> originalDelegate) {
            if (!IsAppVerifierEnabled) {
                return originalDelegate;
            }

            return (arg, callback, state) => WrapBeginMethodImpl(
                httpApplication: httpApplication,
                beginMethod: (innerCallback, innerState) => originalDelegate(arg, innerCallback, innerState),
                originalDelegate: originalDelegate,
                errorHandler: HandleAppVerifierException,
                callStackMask: CallStackCollectionBitMasks.AllHandlerMask)
                (callback, state);
        }

        // Instruments a BeginEventHandler method [(object, sender, EventArgs, object) -> IAsyncResult].
        // This pattern is commonly used, such as by IHttpModule, PageAsyncTask, and others.
        // If AppVerifier is disabled, returns the original method unmodified.
        public static BeginEventHandler WrapBeginMethod(HttpApplication httpApplication, BeginEventHandler originalDelegate) {
            if (!IsAppVerifierEnabled) {
                return originalDelegate;
            }

            return (sender, e, cb, extraData) => WrapBeginMethodImpl(
                httpApplication: httpApplication,
                beginMethod: (innerCallback, innerState) => originalDelegate(sender, e, innerCallback, innerState),
                originalDelegate: originalDelegate,
                errorHandler: HandleAppVerifierException,
                callStackMask: CallStackCollectionBitMasks.AllStepMask)
                (cb, extraData);
        }

        /// <summary>
        /// Wraps the Begin* part of a Begin / End method pair to allow for signaling when assertions have been violated.
        /// The instrumentation can be a performance hit, so this method should not be called if AppVerifier is not enabled.
        /// </summary>
        /// <param name="httpApplication">The HttpApplication instance for this request, used to get HttpContext and related items.</param>
        /// <param name="beginMethod">The Begin* part of a Begin / End method pair, likely wrapped in a lambda so only the AsyncCallback and object parameters are exposed.</param>
        /// <param name="originalDelegate">The original user-provided delegate, e.g. the thing that 'beginMethod' wraps. Provided so that we can show correct methods when asserting.</param>
        /// <param name="errorHandler">The listener that can handle verification failures.</param>
        /// <returns>The instrumented Begin* method.</returns>
        internal static Func<AsyncCallback, object, IAsyncResult> WrapBeginMethodImpl(HttpApplication httpApplication, Func<AsyncCallback, object, IAsyncResult> beginMethod, Delegate originalDelegate, Action<AppVerifierException> errorHandler, CallStackCollectionBitMasks callStackMask) {
            return (callback, state) => {
                // basic diagnostic info goes at the top since it's used during generation of the error message
                AsyncCallbackInvocationHelper asyncCallbackInvocationHelper = new AsyncCallbackInvocationHelper();
                CallStackCollectionBitMasks myBeginMask = callStackMask & CallStackCollectionBitMasks.AllBeginMask;
                bool captureBeginStack = (myBeginMask & (CallStackCollectionBitMasks)AppVerifierCollectCallStackMask) == myBeginMask;

                InvocationInfo beginHandlerInvocationInfo = InvocationInfo.Capture(captureBeginStack);
                Uri requestUrl = null;
                RequestNotification? currentNotification = null;
                bool isPostNotification = false;
                Type httpHandlerType = null;

                // need to collect all this up-front since it might go away during the async operation
                if (httpApplication != null) {
                    HttpContext context = httpApplication.Context;
                    if (context != null) {
                        if (!context.HideRequestResponse && context.Request != null) {
                            requestUrl = context.Request.Unvalidated.Url;
                        }

                        if (context.NotificationContext != null) {
                            currentNotification = context.NotificationContext.CurrentNotification;
                            isPostNotification = context.NotificationContext.IsPostNotification;
                        }

                        if (context.Handler != null) {
                            httpHandlerType = context.Handler.GetType();
                        }
                    }
                }

                // If the condition passed to this method evaluates to false, we will raise an error to whoever is listening.
                AssertDelegate assert = (condition, errorCode) => {
                    long mask = 1L<<(int)errorCode;
                    // assert only if it was not masked out by a bit set
                    bool enableAssert = (AppVerifierErrorCodeEnableAssertMask & mask) == mask;

                    if (!condition && enableAssert) {
                        // capture the stack only if it was not masked out by a bit set
                        bool captureStack = (AppVerifierErrorCodeCollectCallStackMask & mask) == mask;

                        InvocationInfo assertInvocationInfo = InvocationInfo.Capture(captureStack);

                        // header
                        StringBuilder errorString = new StringBuilder();
                        errorString.AppendLine(FormatErrorString(SR.AppVerifier_Title));
                        errorString.AppendLine(FormatErrorString(SR.AppVerifier_Subtitle));
                        errorString.AppendLine();

                        // basic info (about the assert)
                        errorString.AppendLine(FormatErrorString(SR.AppVerifier_BasicInfo_URL, requestUrl));
                        errorString.AppendLine(FormatErrorString(SR.AppVerifier_BasicInfo_ErrorCode, (int)errorCode));
                        errorString.AppendLine(FormatErrorString(SR.AppVerifier_BasicInfo_Description, GetLocalizedDescriptionStringForError(errorCode)));
                        errorString.AppendLine(FormatErrorString(SR.AppVerifier_BasicInfo_ThreadInfo, assertInvocationInfo.ThreadId, assertInvocationInfo.Timestamp.ToLocalTime()));
                        errorString.AppendLine(assertInvocationInfo.StackTrace.ToString());

                        // Begin* method info
                        errorString.AppendLine(FormatErrorString(SR.AppVerifier_BeginMethodInfo_EntryMethod, PrettyPrintDelegate(originalDelegate)));
                        if (currentNotification != null) {
                            errorString.AppendLine(FormatErrorString(SR.AppVerifier_BeginMethodInfo_RequestNotification_Integrated, currentNotification, isPostNotification));
                        }
                        else {
                            errorString.AppendLine(FormatErrorString(SR.AppVerifier_BeginMethodInfo_RequestNotification_NotIntegrated));
                        }
                        errorString.AppendLine(FormatErrorString(SR.AppVerifier_BeginMethodInfo_CurrentHandler, httpHandlerType));
                        errorString.AppendLine(FormatErrorString(SR.AppVerifier_BeginMethodInfo_ThreadInfo, beginHandlerInvocationInfo.ThreadId, beginHandlerInvocationInfo.Timestamp.ToLocalTime()));
                        errorString.AppendLine(beginHandlerInvocationInfo.StackTrace.ToString());

                        // AsyncCallback info
                        int totalAsyncInvocationCount;
                        InvocationInfo firstAsyncInvocation = asyncCallbackInvocationHelper.GetFirstInvocationInfo(out totalAsyncInvocationCount);
                        errorString.AppendLine(FormatErrorString(SR.AppVerifier_AsyncCallbackInfo_InvocationCount, totalAsyncInvocationCount));
                        if (firstAsyncInvocation != null) {
                            errorString.AppendLine(FormatErrorString(SR.AppVerifier_AsyncCallbackInfo_FirstInvocation_ThreadInfo, firstAsyncInvocation.ThreadId, firstAsyncInvocation.Timestamp.ToLocalTime()));
                            errorString.AppendLine(firstAsyncInvocation.StackTrace.ToString());
                        }

                        AppVerifierException ex = new AppVerifierException(errorCode, errorString.ToString());
                        errorHandler(ex);
                        throw ex;
                    }
                };

                assert(httpApplication != null, AppVerifierErrorCode.HttpApplicationInstanceWasNull);
                assert(originalDelegate != null, AppVerifierErrorCode.BeginHandlerDelegateWasNull);

                object lockObj = new object(); // used to synchronize access to certain locals which can be touched by multiple threads
                IAsyncResult asyncResultReturnedByBeginHandler = null;
                IAsyncResult asyncResultPassedToCallback = null;
                object beginHandlerReturnValueHolder = null; // used to hold the IAsyncResult returned by or Exception thrown by BeginHandler; see comments on Holder<T> for more info
                Thread threadWhichCalledBeginHandler = Thread.CurrentThread; // used to determine whether the callback was invoked synchronously
                bool callbackRanToCompletion = false; // don't need to lock when touching this local since it's only read in the synchronous case

                HttpContext assignedContextUponCallingBeginHandler = httpApplication.Context; // used to determine whether the underlying request disappeared

                try {
                    asyncResultReturnedByBeginHandler = beginMethod(
                       asyncResult => {
                           try {
                               CallStackCollectionBitMasks myCallbackMask = callStackMask & CallStackCollectionBitMasks.AllCallbackMask;
                               bool captureEndCallStack = (myCallbackMask & AppVerifierCollectCallStackMask ) == myCallbackMask;
                               // The callback must never be called more than once.
                               int newAsyncCallbackInvocationCount = asyncCallbackInvocationHelper.RecordInvocation(captureEndCallStack);
                               assert(newAsyncCallbackInvocationCount == 1, AppVerifierErrorCode.AsyncCallbackInvokedMultipleTimes);

                               // The 'asyncResult' parameter must never be null.
                               assert(asyncResult != null, AppVerifierErrorCode.AsyncCallbackInvokedWithNullParameter);

                               object tempBeginHandlerReturnValueHolder;
                               Thread tempThreadWhichCalledBeginHandler;
                               lock (lockObj) {
                                   asyncResultPassedToCallback = asyncResult;
                                   tempBeginHandlerReturnValueHolder = beginHandlerReturnValueHolder;
                                   tempThreadWhichCalledBeginHandler = threadWhichCalledBeginHandler;
                               }

                               // At this point, 'IsCompleted = true' is mandatory.
                               assert(asyncResult.IsCompleted, AppVerifierErrorCode.AsyncCallbackGivenAsyncResultWhichWasNotCompleted);

                               if (tempBeginHandlerReturnValueHolder == null) {
                                   // BeginHandler hasn't yet returned, so this call may be synchronous or asynchronous.
                                   // We can tell by comparing the current thread with the thread which called BeginHandler.
                                   // From a correctness perspective, it is valid to invoke the AsyncCallback delegate either
                                   // synchronously or asynchronously. From [....]: if 'CompletedSynchronously = true', then
                                   // AsyncCallback invocation can happen either on the same thread or on a different thread,
                                   // just as long as BeginHandler hasn't yet returned (which in true in this case).
                                   if (!asyncResult.CompletedSynchronously) {
                                       // If 'CompletedSynchronously = false', we must be on a different thread than the BeginHandler invocation.
                                       assert(tempThreadWhichCalledBeginHandler != Thread.CurrentThread, AppVerifierErrorCode.AsyncCallbackInvokedSynchronouslyButAsyncResultWasNotMarkedCompletedSynchronously);
                                   }
                               }
                               else {
                                   // BeginHandler already returned, so this invocation is definitely asynchronous.

                                   Holder<IAsyncResult> asyncResultHolder = tempBeginHandlerReturnValueHolder as Holder<IAsyncResult>;
                                   if (asyncResultHolder != null) {
                                       // We need to verify that the IAsyncResult we're given is the same that was returned by BeginHandler
                                       // and that the IAsyncResult is marked 'CompletedSynchronously = false'.
                                       assert(asyncResult == asyncResultHolder.Value, AppVerifierErrorCode.AsyncCallbackInvokedWithUnexpectedAsyncResultInstance);
                                       assert(!asyncResult.CompletedSynchronously, AppVerifierErrorCode.AsyncCallbackInvokedAsynchronouslyButAsyncResultWasMarkedCompletedSynchronously);
                                   }
                                   else {
                                       // If we reached this point, BeginHandler threw an exception.
                                       // The AsyncCallback should never be invoked if BeginHandler has already failed.
                                       assert(false, AppVerifierErrorCode.BeginHandlerThrewThenAsyncCallbackInvokedAsynchronously);
                                   }
                               }

                               // AsyncState must match the 'state' parameter passed to BeginHandler
                               assert(asyncResult.AsyncState == state, AppVerifierErrorCode.AsyncCallbackInvokedWithUnexpectedAsyncResultAsyncState);

                               // Make sure the underlying HttpApplication is still assigned to the captured HttpContext instance.
                               // If not, this AsyncCallback invocation could end up completing *some other request's* operation,
                               // resulting in data corruption.
                               assert(assignedContextUponCallingBeginHandler == httpApplication.Context, AppVerifierErrorCode.AsyncCallbackCalledAfterHttpApplicationReassigned);
                           }
                           catch (AppVerifierException) {
                               // We want to ---- any exceptions thrown by our verification logic, as the failure
                               // has already been recorded by the appropriate listener. Just go straight to
                               // invoking the callback.
                           }

                           // all checks complete - delegate control to the actual callback
                           if (callback != null) {
                               callback(asyncResult);
                           }
                           callbackRanToCompletion = true;
                       },
                       state);

                    // The return value must never be null.
                    assert(asyncResultReturnedByBeginHandler != null, AppVerifierErrorCode.BeginHandlerReturnedNull);

                    lock (lockObj) {
                        beginHandlerReturnValueHolder = new Holder<IAsyncResult>(asyncResultReturnedByBeginHandler);
                    }

                    if (asyncResultReturnedByBeginHandler.CompletedSynchronously) {
                        // If 'CompletedSynchronously = true', the IAsyncResult must be marked 'IsCompleted = true'
                        // and the AsyncCallback must have been invoked synchronously (checked in the AsyncCallback verification logic).
                        assert(asyncResultReturnedByBeginHandler.IsCompleted, AppVerifierErrorCode.BeginHandlerReturnedAsyncResultMarkedCompletedSynchronouslyButWhichWasNotCompleted);
                        assert(asyncCallbackInvocationHelper.TotalInvocations != 0, AppVerifierErrorCode.BeginHandlerReturnedAsyncResultMarkedCompletedSynchronouslyButAsyncCallbackNeverCalled);
                    }

                    IAsyncResult tempAsyncResultPassedToCallback;
                    lock (lockObj) {
                        tempAsyncResultPassedToCallback = asyncResultPassedToCallback;
                    }

                    // The AsyncCallback may have been invoked (either synchronously or asynchronously). If it has been
                    // invoked, we need to verify that it was given the same IAsyncResult returned by BeginHandler.
                    // If the AsyncCallback hasn't yet been called, we skip this check, as the AsyncCallback verification
                    // logic will eventually perform the check at the appropriate time.
                    if (tempAsyncResultPassedToCallback != null) {
                        assert(tempAsyncResultPassedToCallback == asyncResultReturnedByBeginHandler, AppVerifierErrorCode.BeginHandlerReturnedUnexpectedAsyncResultInstance);
                    }

                    // AsyncState must match the 'state' parameter passed to BeginHandler
                    assert(asyncResultReturnedByBeginHandler.AsyncState == state, AppVerifierErrorCode.BeginHandlerReturnedUnexpectedAsyncResultAsyncState);

                    // all checks complete
                    return asyncResultReturnedByBeginHandler;
                }
                catch (AppVerifierException) {
                    // We want to ---- any exceptions thrown by our verification logic, as the failure
                    // has already been recorded by the appropriate listener. Just return the original
                    // IAsyncResult so that the application continues to run.
                    return asyncResultReturnedByBeginHandler;
                }
                catch (Exception ex) {
                    if (asyncResultReturnedByBeginHandler == null) {
                        // If we reached this point, an exception was thrown by BeginHandler, so we need to
                        // record it and rethrow it.

                        IAsyncResult tempAsyncResultPassedToCallback;
                        lock (lockObj) {
                            beginHandlerReturnValueHolder = new Holder<Exception>(ex);
                            tempAsyncResultPassedToCallback = asyncResultPassedToCallback;
                        }

                        try {
                            // The AsyncCallback should only be invoked if BeginHandler ran to completion.
                            if (tempAsyncResultPassedToCallback != null) {

                                // If AsyncCallback was invoked asynchronously, then by definition it was
                                // scheduled prematurely since BeginHandler hadn't yet run to completion
                                // (since whatever additional work it did after invoking the callback failed).
                                // Therefore it is always wrong for BeginHandler to both throw and
                                // asynchronously invoke AsyncCallback.
                                assert(tempAsyncResultPassedToCallback.CompletedSynchronously, AppVerifierErrorCode.AsyncCallbackInvokedAsynchronouslyThenBeginHandlerThrew);

                                // If AsyncCallback was invoked synchronously, then it must have been invoked
                                // before BeginHandler surfaced the exception (since otherwise BeginHandler
                                // wouldn't have reached the line of code that invoked AsyncCallback). But
                                // AsyncCallback itself could have thrown, bubbling the exception up through
                                // BeginHandler and back to us. If AsyncCallback ran to completion, then this
                                // means BeginHandler did extra work (which failed) after invoking AsyncCallback,
                                // so BeginHandler by definition hadn't yet run to completion.
                                assert(!callbackRanToCompletion, AppVerifierErrorCode.AsyncCallbackInvokedSynchronouslyThenBeginHandlerThrew);
                            }
                        }
                        catch (AppVerifierException) {
                            // We want to ---- any exceptions thrown by our verification logic, as the failure
                            // has already been recorded by the appropriate listener. Propagate the original
                            // exception upward.
                        }

                        throw;
                    }
                    else {
                        // We want to ---- any exceptions thrown by our verification logic, as the failure
                        // has already been recorded by the appropriate listener. Just return the original
                        // IAsyncResult so that the application continues to run.
                        return asyncResultReturnedByBeginHandler;
                    }
                }
                finally {
                    // Since our local variables are GC-rooted in an anonymous object, we should
                    // clear references to objects we no longer need so that the GC can reclaim
                    // them if appropriate.
                    lock (lockObj) {
                        threadWhichCalledBeginHandler = null;
                    }
                }
            };
        }

        // Gets a delegate that checks for application code trying to call into the SyncContext after
        // the request is already completed. The Action returned by this method could be null.
        public static Action GetSyncContextCheckDelegate(ISyncContext syncContext) {
            if (!IsAppVerifierEnabled) {
                return null;
            }

            return GetSyncContextCheckDelegateImpl(syncContext, HandleAppVerifierException);
        }

        /// <summary>
        /// Returns an Action that determines whether SynchronizationContext.Send or Post was called after the underlying request finished.
        /// The instrumentation can be a performance hit, so this method should not be called if AppVerifier is not enabled.
        /// </summary>
        /// <param name="syncContext">The ISyncContext (HttpApplication, WebSocketPipeline, etc.) on which to perform the check.</param>
        /// <param name="errorHandler">The listener that can handle verification failures.</param>
        /// <returns>A callback which performs the verification.</returns>
        internal static Action GetSyncContextCheckDelegateImpl(ISyncContext syncContext, Action<AppVerifierException> errorHandler) {
            Uri requestUrl = null;
            object originalThreadContextId = null;

            // collect all of the diagnostic information upfront
            HttpContext originalHttpContext = (syncContext != null) ? syncContext.HttpContext : null;
            if (originalHttpContext != null) {
                if (!originalHttpContext.HideRequestResponse && originalHttpContext.Request != null) {
                    requestUrl = originalHttpContext.Request.Unvalidated.Url;
                }

                // This will be used as a surrogate for the captured HttpContext so that we don't
                // have a long-lived reference to a heavy object graph. See comments on ThreadContextId
                // for more info.
                originalThreadContextId = originalHttpContext.ThreadContextId;
                originalHttpContext = null;
            }

            // If the condition passed to this method evaluates to false, we will raise an error to whoever is listening.
            AssertDelegate assert = (condition, errorCode) => {
                long mask = 1L << (int)errorCode;
                // assert only if it was not masked out by a bit set
                bool enableAssert = (AppVerifierErrorCodeEnableAssertMask & mask) == mask;

                if (!condition && enableAssert) {
                    // capture the stack only if it was not masked out by a bit set
                    bool captureStack = (AppVerifierErrorCodeCollectCallStackMask & mask) == mask;
                    InvocationInfo assertInvocationInfo = InvocationInfo.Capture(captureStack);

                    // header
                    StringBuilder errorString = new StringBuilder();
                    errorString.AppendLine(FormatErrorString(SR.AppVerifier_Title));
                    errorString.AppendLine(FormatErrorString(SR.AppVerifier_Subtitle));
                    errorString.AppendLine();

                    // basic info (about the assert)
                    errorString.AppendLine(FormatErrorString(SR.AppVerifier_BasicInfo_URL, requestUrl));
                    errorString.AppendLine(FormatErrorString(SR.AppVerifier_BasicInfo_ErrorCode, (int)errorCode));
                    errorString.AppendLine(FormatErrorString(SR.AppVerifier_BasicInfo_Description, GetLocalizedDescriptionStringForError(errorCode)));
                    errorString.AppendLine(FormatErrorString(SR.AppVerifier_BasicInfo_ThreadInfo, assertInvocationInfo.ThreadId, assertInvocationInfo.Timestamp.ToLocalTime()));
                    errorString.AppendLine(assertInvocationInfo.StackTrace.ToString());

                    AppVerifierException ex = new AppVerifierException(errorCode, errorString.ToString());
                    errorHandler(ex);
                    throw ex;
                }
            };

            return () => {
                try {
                    // Make sure that the ISyncContext is still associated with the same HttpContext that
                    // we captured earlier.
                    HttpContext currentHttpContext = (syncContext != null) ? syncContext.HttpContext : null;
                    object currentThreadContextId = (currentHttpContext != null) ? currentHttpContext.ThreadContextId : null;
                    assert(currentThreadContextId != null && ReferenceEquals(originalThreadContextId, currentThreadContextId), AppVerifierErrorCode.SyncContextSendOrPostCalledAfterRequestCompleted);
                }
                catch (AppVerifierException) {
                    // We want to ---- any exceptions thrown by our verification logic, as the failure
                    // has already been recorded by the appropriate listener. Propagate the original
                    // exception upward.
                }
            };
        }

        // This is the default implementation of an AppVerifierException handler;
        // it just delegates to the configured behavior.
        [SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive", Justification = "Want to keep these locals on the stack to assist with debugging.")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void HandleAppVerifierException(AppVerifierException ex) {
            // This method is specifically written to maximize the chance of
            // useful information being on the stack as a local, where it's more
            // easily observed by the debugger.

            AppVerifierErrorCode errorCode = ex.ErrorCode;
            string fullMessage = ex.Message;

            DefaultAppVerifierBehavior(ex);

            GC.KeepAlive(errorCode);
            GC.KeepAlive(fullMessage);
            GC.KeepAlive(ex);
        }

        internal static string PrettyPrintDelegate(Delegate del) {
            return PrettyPrintMemberInfo((del != null) ? del.Method : null);
        }

        // prints "TResult MethodName(TArg1, TArg2, ...) [Module.dll!Namespace.TypeName]"
        internal static string PrettyPrintMemberInfo(MethodInfo method) {
            if (method == null) {
                return null;
            }

            string retVal = method.ToString();

            Type type = method.ReflectedType;
            if (type != null) {
                retVal = retVal + " [";
                if (type.Module != null) {
                    retVal += type.Module.Name + "!";
                }

                retVal += type.FullName + "]";
            }

            return retVal;
        }

        internal static string GetLocalizedDescriptionStringForError(AppVerifierErrorCode errorCode) {
            return FormatErrorString(_errorStringMappings[errorCode]);
        }

        // We use InstalledUICulture rather than CurrentCulture / CurrentUICulture since these strings will
        // be stored in the system event log.
        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.IFormatProvider,System.String,System.Object[])",
            Justification = "Matches culture specified in Misc.WriteUnhandledExceptionToEventLog.")]
        internal static string FormatErrorString(string name, params object[] args) {
            return String.Format(CultureInfo.InstalledUICulture, SR.Resources.GetString(name, CultureInfo.InstalledUICulture), args);
        }

        // contains a counter and invocation information for an AsyncCallback delegate
        private sealed class AsyncCallbackInvocationHelper {
            private InvocationInfo _firstInvocationInfo;
            private int _totalInvocationCount;

            public int TotalInvocations {
                [MethodImpl(MethodImplOptions.Synchronized)]
                get { return _totalInvocationCount; }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public InvocationInfo GetFirstInvocationInfo(out int totalInvocationCount) {
                totalInvocationCount = _totalInvocationCount;
                return _firstInvocationInfo;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public int RecordInvocation(bool captureCallStack) {
                _totalInvocationCount++;
                if (_firstInvocationInfo == null) {
                    _firstInvocationInfo = InvocationInfo.Capture(captureCallStack);
                }
                return _totalInvocationCount;
            }
        }

        // We use a special class for holding data so that we can store the local's
        // intended type alongside its real value. Prevents us from misinterpreting
        // the degenerate case of "----CustomType : Exception, IAsyncResult" so that
        // we know whether it was returned as an IAsyncResult or thrown as an Exception.
        private sealed class Holder<T> {
            public readonly T Value;

            public Holder(T value) {
                Value = value;
            }
        }

        // holds diagnostic information about a particular invocation
        private sealed class InvocationInfo {
            public readonly int ThreadId;
            public readonly DateTimeOffset Timestamp;
            public readonly string StackTrace;

            private InvocationInfo(bool captureStack) {
                ThreadId = Thread.CurrentThread.ManagedThreadId;
                Timestamp = DateTimeOffset.UtcNow; // UTC is faster, will convert to local on error
                StackTrace = captureStack? CaptureStackTrace(): "n/a";
            }

            public static InvocationInfo Capture(bool captureStack) {
                return new InvocationInfo(captureStack);
            }

            // captures a stack trace, removing AppVerifier.* frames from the top of the stack to minimize noise
            private static string CaptureStackTrace() {
                StackTrace fullStackTrace = new StackTrace(fNeedFileInfo: true);
                string[] traceLines = fullStackTrace.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                for (int i = 0; i < fullStackTrace.FrameCount && i < traceLines.Length; i++) {
                    StackFrame thisFrame = fullStackTrace.GetFrame(i);
                    if (thisFrame.GetMethod().Module == typeof(AppVerifier).Module
                        && thisFrame.GetMethod().DeclaringType.FullName.StartsWith("System.Web.Util.AppVerifier", StringComparison.Ordinal)) {
                        // we want to skip this frame since it's an AppVerifier.* frame
                        continue;
                    }
                    else {
                        // this is the first frame that is not an AppVerifier.* frame, so start the stack trace from here
                        return String.Join(Environment.NewLine, traceLines.Skip(i));
                    }
                }

                // if we reached this point, not sure what happened, so just return the original stack trace
                return fullStackTrace.ToString();
            }
        }

        [SuppressUnmanagedCodeSecurityAttribute]
        private static class NativeMethods {
            [DllImport("kernel32.dll")]
            internal extern static void DebugBreak();
        }

    }
}
