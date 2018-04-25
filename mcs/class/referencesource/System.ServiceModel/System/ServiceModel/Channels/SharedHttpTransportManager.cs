//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Diagnostics;
    using System.Net;
    using System.Runtime;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Dispatcher;
    using System.Threading;
    using System.Runtime.Diagnostics;

    class SharedHttpTransportManager : HttpTransportManager
    {
        int maxPendingAccepts;
        HttpListener listener;
        ManualResetEvent listenStartedEvent;
        Exception listenStartedException;
        AsyncCallback onGetContext;
        AsyncCallback onContextReceived;
        Action onMessageDequeued;
        Action<object> onCompleteGetContextLater;
        bool unsafeConnectionNtlmAuthentication;
        ReaderWriterLockSlim listenerRWLock;

        internal SharedHttpTransportManager(Uri listenUri, HttpChannelListener channelListener)
            : base(listenUri, channelListener.HostNameComparisonMode, channelListener.Realm)
        {
            this.onGetContext = Fx.ThunkCallback(new AsyncCallback(OnGetContext));
            this.onMessageDequeued = new Action(OnMessageDequeued);
            this.unsafeConnectionNtlmAuthentication = channelListener.UnsafeConnectionNtlmAuthentication;
            this.onContextReceived = new AsyncCallback(this.HandleHttpContextReceived);
            this.listenerRWLock = new ReaderWriterLockSlim();

            this.maxPendingAccepts = channelListener.MaxPendingAccepts;
        }

        // We are NOT checking the RequestInitializationTimeout here since the HttpChannelListener should be handle them
        // individually. However, some of the scenarios might be impacted, e.g., if we have one endpoint with high RequestInitializationTimeout
        // and the other is just normal, the first endpoint might be occupying all the receiving loops, then the requests to the normal endpoint
        // will experience timeout issues. The mitigation for this issue is that customers should be able to increase the MaxPendingAccepts number.
        internal override bool IsCompatible(HttpChannelListener channelListener)
        {
            if (channelListener.InheritBaseAddressSettings)
                return true;

            if (!channelListener.IsScopeIdCompatible(HostNameComparisonMode, this.ListenUri))
            {
                return false;
            }

            if (this.maxPendingAccepts != channelListener.MaxPendingAccepts)
            {
                return false;
            }

            return channelListener.UnsafeConnectionNtlmAuthentication == this.unsafeConnectionNtlmAuthentication
                && base.IsCompatible(channelListener);
        }

        internal override void OnClose(TimeSpan timeout)
        {
            Cleanup(false, timeout);
        }

        internal override void OnAbort()
        {
            Cleanup(true, TimeSpan.Zero);
            base.OnAbort();
        }

        void Cleanup(bool aborting, TimeSpan timeout)
        {
            using (LockHelper.TakeWriterLock(this.listenerRWLock))
            {
                HttpListener listenerSnapshot = this.listener;
                if (listenerSnapshot == null)
                {
                    return;
                }

                try
                {
                    listenerSnapshot.Stop();
                }
                finally
                {
                    try
                    {
                        listenerSnapshot.Close();
                    }
                    finally
                    {
                        if (!aborting)
                        {
                            base.OnClose(timeout);
                        }
                        else
                        {
                            base.OnAbort();
                        }
                    }
                }

                this.listener = null;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls into critical method ExecutionContext.SuppressFlow",
            Safe = "Doesn't leak information\\resources; the callback that is invoked is safe")]
        [SecuritySafeCritical]
        IAsyncResult BeginGetContext(bool startListening)
        {
            EventTraceActivity eventTraceActivity = null;
            if (FxTrace.Trace.IsEnd2EndActivityTracingEnabled)
            {
                eventTraceActivity = EventTraceActivity.GetFromThreadOrCreate(true);
                if (TD.HttpGetContextStartIsEnabled())
                {
                    TD.HttpGetContextStart(eventTraceActivity);
                }
            }

            while (true)
            {
                Exception unexpectedException = null;
                try
                {
                    try
                    {
                        if (ExecutionContext.IsFlowSuppressed())
                        {
                            return this.BeginGetContextCore(eventTraceActivity);
                        }
                        else
                        {
                            using (ExecutionContext.SuppressFlow())
                            {
                                return this.BeginGetContextCore(eventTraceActivity);
                            }
                        }
                    }
                    catch (HttpListenerException e)
                    {
                        if (!this.HandleHttpException(e))
                        {
                            throw;
                        }
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    if (startListening)
                    {
                        // Since we're under a call to StartListening(), just throw the exception up the stack.
                        throw;
                    }
                    unexpectedException = e;
                }

                if (unexpectedException != null)
                {
                    this.Fault(unexpectedException);
                    return null;
                }
            }
        }

        IAsyncResult BeginGetContextCore(EventTraceActivity eventTraceActivity)
        {
            using (LockHelper.TakeReaderLock(this.listenerRWLock))
            {
                if (this.listener == null)
                {
                    return null;
                }

                return this.listener.BeginGetContext(onGetContext, eventTraceActivity);
            }
        }

        void OnGetContext(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            OnGetContextCore(result);
        }

        void OnCompleteGetContextLater(object state)
        {
            OnGetContextCore((IAsyncResult)state);
        }

        void OnGetContextCore(IAsyncResult listenerContextResult)
        {
            Fx.Assert(listenerContextResult != null, "listenerContextResult cannot be null.");
            bool enqueued = false;

            while (!enqueued)
            {
                Exception unexpectedException = null;
                try
                {
                    try
                    {
                        enqueued = this.EnqueueContext(listenerContextResult);
                    }
                    catch (HttpListenerException e)
                    {
                        if (!this.HandleHttpException(e))
                        {
                            throw;
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }

                    unexpectedException = exception;
                }

                if (unexpectedException != null)
                {
                    this.Fault(unexpectedException);
                }

                // NormalHttpPipeline calls HttpListener.BeginGetContext() by itself (via its dequeuedCallback) in the short-circuit case
                // when there was no error processing the inboud request (see the comments in the NormalHttpPipeline.Close() for details).
                if (!enqueued) // onMessageDequeued will handle this in the enqueued case
                {
                    // Continue the loop with the async result if it completed synchronously.
                    listenerContextResult = this.BeginGetContext(false);
                    if ((listenerContextResult == null) || !listenerContextResult.CompletedSynchronously)
                    {
                        return;
                    }
                }
            }
        }

        bool EnqueueContext(IAsyncResult listenerContextResult)
        {
            EventTraceActivity eventTraceActivity = null;
            HttpListenerContext listenerContext;
            bool enqueued = false;

            if (FxTrace.Trace.IsEnd2EndActivityTracingEnabled)
            {
                eventTraceActivity = (EventTraceActivity)listenerContextResult.AsyncState;
                if (eventTraceActivity == null)
                {
                    eventTraceActivity = EventTraceActivity.GetFromThreadOrCreate(true);
                }
            }

            using (LockHelper.TakeReaderLock(this.listenerRWLock))
            {
                if (this.listener == null)
                {
                    return true;
                }

                listenerContext = this.listener.EndGetContext(listenerContextResult);
            }

            // Grab the activity from the context and set that as the surrounding activity.
            // If a message appears, we will transfer to the message's activity next
            using (DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.BoundOperation(this.Activity) : null)
            {
                ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivityWithTransferInOnly(listenerContext.Request.RequestTraceIdentifier) : null;                
                try
                {
                    if (activity != null)
                    {
                        StartReceiveBytesActivity(activity, listenerContext.Request.Url);
                    }
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        TraceUtility.TraceHttpConnectionInformation(listenerContext.Request.LocalEndPoint.ToString(),
                            listenerContext.Request.RemoteEndPoint.ToString(), this);
                    }

                    base.TraceMessageReceived(eventTraceActivity, this.ListenUri);

                    HttpChannelListener channelListener;
                    if (base.TryLookupUri(listenerContext.Request.Url,
                                        listenerContext.Request.HttpMethod,
                                        this.HostNameComparisonMode,
                                        listenerContext.Request.IsWebSocketRequest,
                                        out channelListener))
                    {
                        HttpRequestContext context = HttpRequestContext.CreateContext(channelListener, listenerContext, eventTraceActivity);

                        IAsyncResult httpContextReceivedResult = channelListener.BeginHttpContextReceived(context,
                                                                                                        onMessageDequeued,
                                                                                                        onContextReceived,
                                                                                                        DiagnosticUtility.ShouldUseActivity ? (object)new ActivityHolder(activity, context) : (object)context);
                        if (httpContextReceivedResult.CompletedSynchronously)
                        {
                            enqueued = EndHttpContextReceived(httpContextReceivedResult);
                        }
                        else
                        {
                            // The callback has been enqueued.
                            enqueued = true;
                        }
                    }
                    else
                    {
                        HandleMessageReceiveFailed(listenerContext);
                    }
                }
                finally 
                {
                    if (DiagnosticUtility.ShouldUseActivity && activity != null)
                    {
                        if (!enqueued) 
                        {
                            // Error during enqueuing
                            activity.Dispose();
                        }
                    }
                }
            }

            return enqueued;
        }

        void HandleHttpContextReceived(IAsyncResult httpContextReceivedResult)
        {
            if (httpContextReceivedResult.CompletedSynchronously)
            {
                return;
            }

            bool enqueued = false;
            Exception unexpectedException = null;
            try
            {
                try
                {
                    enqueued = EndHttpContextReceived(httpContextReceivedResult);
                }
                catch (HttpListenerException e)
                {
                    if (!this.HandleHttpException(e))
                    {
                        throw;
                    }
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }

                unexpectedException = exception;
            }

            if (unexpectedException != null)
            {
                this.Fault(unexpectedException);
            }

            IAsyncResult listenerContextResult = null;
            if (!enqueued) // onMessageDequeued will handle this in the enqueued case
            {
                listenerContextResult = this.BeginGetContext(false);
                if ((listenerContextResult == null) || !listenerContextResult.CompletedSynchronously)
                {
                    return;
                }

                // Handle the context and continue the receive loop.
                this.OnGetContextCore(listenerContextResult);
            }
        }

        static bool EndHttpContextReceived(IAsyncResult httpContextReceivedResult)
        {
            using (DiagnosticUtility.ShouldUseActivity ? (ActivityHolder)httpContextReceivedResult.AsyncState : null)
            {
                HttpChannelListener channelListener =
                    (DiagnosticUtility.ShouldUseActivity ?
                        ((ActivityHolder)httpContextReceivedResult.AsyncState).context :
                        (HttpRequestContext)httpContextReceivedResult.AsyncState).Listener;

                return channelListener.EndHttpContextReceived(httpContextReceivedResult);
            }            
        }

        bool HandleHttpException(HttpListenerException e)
        {
            switch (e.ErrorCode)
            {
                case UnsafeNativeMethods.ERROR_NOT_ENOUGH_MEMORY:
                case UnsafeNativeMethods.ERROR_OUTOFMEMORY:
                case UnsafeNativeMethods.ERROR_NO_SYSTEM_RESOURCES:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InsufficientMemoryException(SR.GetString(SR.InsufficentMemory), e));
                default:
                    return ExceptionHandler.HandleTransportExceptionHelper(e);
            }
        }

        static void HandleMessageReceiveFailed(HttpListenerContext listenerContext)
        {
            TraceMessageReceiveFailed();

            // no match -- 405 or 404
            if (string.Compare(listenerContext.Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) != 0)
            {
                listenerContext.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                listenerContext.Response.Headers.Add(HttpResponseHeader.Allow, "POST");
            }
            else
            {
                listenerContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }

            listenerContext.Response.ContentLength64 = 0;
            listenerContext.Response.Close();
        }

        static void TraceMessageReceiveFailed()
        {
            if (TD.HttpMessageReceiveStartIsEnabled())
            {
                TD.HttpMessageReceiveFailed();
            }

            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.HttpChannelMessageReceiveFailed,
                    SR.GetString(SR.TraceCodeHttpChannelMessageReceiveFailed), (object)null);
            }
        }

        void StartListening()
        {
            for (int i = 0; i < maxPendingAccepts; i++)
            {
                IAsyncResult result = this.BeginGetContext(true);
                if (result.CompletedSynchronously)
                {
                    if (onCompleteGetContextLater == null)
                    {
                        onCompleteGetContextLater = new Action<object>(OnCompleteGetContextLater);
                    }
                    ActionItem.Schedule(onCompleteGetContextLater, result);
                }
            }
        }

        void OnListening(object state)
        {
            try
            {
                this.StartListening();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                this.listenStartedException = e;
            }
            finally
            {
                this.listenStartedEvent.Set();
            }
        }

        void OnMessageDequeued()
        {
            ThreadTrace.Trace("message dequeued");
            IAsyncResult result = this.BeginGetContext(false);
            if (result != null && result.CompletedSynchronously)
            {
                if (onCompleteGetContextLater == null)
                {
                    onCompleteGetContextLater = new Action<object>(OnCompleteGetContextLater);
                }
                ActionItem.Schedule(onCompleteGetContextLater, result);
            }
        }

        internal override void OnOpen()
        {
            listener = new HttpListener();

            string host;

            switch (HostNameComparisonMode)
            {
                case HostNameComparisonMode.Exact:
                    // Uri.DnsSafeHost strips the [], but preserves the scopeid for IPV6 addresses.
                    if (ListenUri.HostNameType == UriHostNameType.IPv6)
                    {
                        host = string.Concat("[", ListenUri.DnsSafeHost, "]");
                    }
                    else
                    {
                        host = ListenUri.NormalizedHost();
                    }
                    break;

                case HostNameComparisonMode.StrongWildcard:
                    host = "+";
                    break;

                case HostNameComparisonMode.WeakWildcard:
                    host = "*";
                    break;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnrecognizedHostNameComparisonMode, HostNameComparisonMode.ToString())));
            }

            string path = ListenUri.GetComponents(UriComponents.Path, UriFormat.Unescaped);
            if (!path.StartsWith("/", StringComparison.Ordinal))
                path = "/" + path;

            if (!path.EndsWith("/", StringComparison.Ordinal))
                path = path + "/";

            string httpListenUrl = string.Concat(Scheme, "://", host, ":", ListenUri.Port, path);

            listener.UnsafeConnectionNtlmAuthentication = this.unsafeConnectionNtlmAuthentication;
            listener.AuthenticationSchemeSelectorDelegate =
                new AuthenticationSchemeSelector(SelectAuthenticationScheme);

            if (ExtendedProtectionPolicy.OSSupportsExtendedProtection)
            {
                //This API will throw if on an unsupported platform.
                listener.ExtendedProtectionSelectorDelegate =
                    new HttpListener.ExtendedProtectionSelector(SelectExtendedProtectionPolicy);
            }

            if (this.Realm != null)
            {
                listener.Realm = this.Realm;
            }

            bool success = false;
            try
            {
                listener.Prefixes.Add(httpListenUrl);
                listener.Start();

                bool startedListening = false;
                try
                {
                    if (Thread.CurrentThread.IsThreadPoolThread)
                    {
                        StartListening();
                    }
                    else
                    {
                        // If we're not on a threadpool thread, then we need to post a callback to start our accepting loop
                        // Otherwise if the calling thread aborts then the async I/O will get inadvertantly cancelled
                        listenStartedEvent = new ManualResetEvent(false);
                        ActionItem.Schedule(OnListening, null);
                        listenStartedEvent.WaitOne();
                        listenStartedEvent.Close();
                        listenStartedEvent = null;
                        if (listenStartedException != null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(listenStartedException);
                        }
                    }
                    startedListening = true;
                }
                finally
                {
                    if (!startedListening)
                    {
                        listener.Stop();
                    }
                }

                success = true;
            }
            catch (HttpListenerException listenerException)
            {
                switch (listenerException.NativeErrorCode)
                {
                    case UnsafeNativeMethods.ERROR_ALREADY_EXISTS:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new AddressAlreadyInUseException(SR.GetString(SR.HttpRegistrationAlreadyExists, httpListenUrl), listenerException));

                    case UnsafeNativeMethods.ERROR_SHARING_VIOLATION:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new AddressAlreadyInUseException(SR.GetString(SR.HttpRegistrationPortInUse, httpListenUrl, ListenUri.Port), listenerException));

                    case UnsafeNativeMethods.ERROR_ACCESS_DENIED:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new AddressAccessDeniedException(SR.GetString(SR.HttpRegistrationAccessDenied, httpListenUrl), listenerException));

                    case UnsafeNativeMethods.ERROR_ALLOTTED_SPACE_EXCEEDED:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.HttpRegistrationLimitExceeded, httpListenUrl), listenerException));

                    case UnsafeNativeMethods.ERROR_INVALID_PARAMETER:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.HttpInvalidListenURI, ListenUri.OriginalString), listenerException));

                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            HttpChannelUtilities.CreateCommunicationException(listenerException));
                }
            }
            finally
            {
                if (!success)
                {
                    listener.Abort();
                }
            }
        }

        AuthenticationSchemes SelectAuthenticationScheme(HttpListenerRequest request)
        {
            try
            {
                AuthenticationSchemes result;
                HttpChannelListener channelListener;
                if (base.TryLookupUri(request.Url, request.HttpMethod,
                    this.HostNameComparisonMode, request.IsWebSocketRequest, out channelListener))
                {
                    result = channelListener.AuthenticationScheme;
                }
                else
                {
                    // if we don't match a listener factory, we want to "fall through" the
                    // auth delegate code and run through our normal OnGetContext codepath.
                    // System.Net treats "None" as Access Denied, which is not our intent here.
                    // In most cases this will just fall through to the code that returns a "404 Not Found"
                    result = AuthenticationSchemes.Anonymous;
                }

                return result;
            }
            catch (Exception e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);
                throw;
            }
        }

        ExtendedProtectionPolicy SelectExtendedProtectionPolicy(HttpListenerRequest request)
        {
            ExtendedProtectionPolicy result = null;

            try
            {
                HttpChannelListener channelListener;
                if (base.TryLookupUri(request.Url, request.HttpMethod,
                    this.HostNameComparisonMode, request.IsWebSocketRequest, out channelListener))
                {
                    result = channelListener.ExtendedProtectionPolicy;
                }
                else
                {
                    //if the listener isn't found, then the auth scheme will be anonymous 
                    //(see SelectAuthenticationScheme function) and will fall through to the
                    //404 Not Found code path, so it doesn't really matter what we return from here...
                    result = ChannelBindingUtility.DisabledPolicy;
                }

                return result;
            }
            catch (Exception e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);
                throw;
            }
        }
    }
}
