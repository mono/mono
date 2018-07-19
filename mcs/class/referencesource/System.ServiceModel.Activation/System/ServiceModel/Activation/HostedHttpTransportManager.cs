//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Activation
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Threading;
    using System.Web;

    class HostedHttpTransportManager : HttpTransportManager
    {
        string scheme;
        int port;
        string host;
        static AsyncCallback onHttpContextReceived = Fx.ThunkCallback(OnHttpContextReceived);

        internal HostedHttpTransportManager(BaseUriWithWildcard baseAddress) :
            base(baseAddress.BaseAddress, baseAddress.HostNameComparisonMode)
        {
            base.IsHosted = true;
        }

        internal override bool IsCompatible(HttpChannelListener factory)
        {
            return true;
        }

        internal override void OnClose(TimeSpan timeout)
        {
            // empty
        }

        internal override void OnOpen()
        {
            // empty
        }

        internal override void OnAbort()
        {
            // empty
        }

        internal override string Scheme
        {
            get
            {
                return this.scheme ?? (this.scheme = this.ListenUri.Scheme);
            }
        }

        internal string Host
        {
            get
            {
                return this.host ?? (this.host = this.ListenUri.Host);
            }
        }

        internal int Port
        {
            get
            {
                return this.port == 0 ? (this.port = this.ListenUri.Port) : this.port;
            }
        }

        static bool canTraceConnectionInformation = true;
        public void TraceConnectionInformation(HostedHttpRequestAsyncResult result)
        {
            if (result != null && DiagnosticUtility.ShouldTraceInformation && canTraceConnectionInformation)
            {
                try
                {
                    AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
                    IServiceProvider provider = (IServiceProvider)result.Application.Context;
                    HttpWorkerRequest workerRequest = (HttpWorkerRequest)provider.GetService(typeof(HttpWorkerRequest));
                    string localAddress = string.Format(CultureInfo.InvariantCulture,
                        "{0}:{1}", workerRequest.GetLocalAddress(), workerRequest.GetLocalPort());
                    string remoteAddress = string.Format(CultureInfo.InvariantCulture,
                        "{0}:{1}", workerRequest.GetRemoteAddress(), workerRequest.GetRemotePort());
                    TraceUtility.TraceHttpConnectionInformation(localAddress, remoteAddress, this);
                }
                catch (SecurityException e)
                {
                    canTraceConnectionInformation = false;

                    // not re-throwing on purpose
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects.", Safe = "Only returns the activity, doesn't leak the ASP .NET objects.")]
        [SecuritySafeCritical]
        public ServiceModelActivity CreateReceiveBytesActivity(HostedHttpRequestAsyncResult result)
        {
            ServiceModelActivity retval = null;
            if (result != null)
            {
                TraceMessageReceived(result.EventTraceActivity, result.RequestUri);
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    IServiceProvider provider = (IServiceProvider)result.Application.Context;
                    retval = ServiceModelActivity.CreateBoundedActivity(GetRequestTraceIdentifier(provider));
                    StartReceiveBytesActivity(retval, result.RequestUri);
                }
            }
            return retval;
        }

        [Fx.Tag.SecurityNote(Critical = "Uses the HttpWorkerRequest to get the trace identifier, which Demands UnamangedCode.", Safe = "Only returns the trace id, doesn't leak the HttpWorkerRequest.")]
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
        static Guid GetRequestTraceIdentifier(IServiceProvider provider)
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
            return ((HttpWorkerRequest)provider.GetService(typeof(HttpWorkerRequest))).RequestTraceIdentifier;
        }

        internal void HttpContextReceived(HostedHttpRequestAsyncResult result)
        {
            using (DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.BoundOperation(this.Activity) : null)
            {
                using (ServiceModelActivity activity = this.CreateReceiveBytesActivity(result))
                {
                    this.TraceConnectionInformation(result);
                    HttpChannelListener listener;

                    if (base.TryLookupUri(result.RequestUri, result.GetHttpMethod(),
                        this.HostNameComparisonMode, result.IsWebSocketRequest, out listener))
                    {
                        HostedHttpContext hostedContext = new HostedHttpContext(listener, result);
                        object state = DiagnosticUtility.ShouldUseActivity ? (object)new ActivityHolder(activity, hostedContext) : (object)hostedContext;
                        IAsyncResult httpContextReceivedResult = listener.BeginHttpContextReceived(hostedContext,
                                                                                                        null,
                                                                                                        onHttpContextReceived,
                                                                                                        state);
                        if (httpContextReceivedResult.CompletedSynchronously)
                        {
                            EndHttpContextReceived(httpContextReceivedResult);
                        }

                        return;
                    }

                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Error, TraceCode.HttpChannelMessageReceiveFailed, SR.TraceCodeHttpChannelMessageReceiveFailed,
                            new StringTraceRecord("IsRecycling", ServiceHostingEnvironment.IsRecycling.ToString(CultureInfo.CurrentCulture)),
                            this, null);
                    }

                    if (ServiceHostingEnvironment.IsRecycling)
                    {
                        throw FxTrace.Exception.AsError(
                            new EndpointNotFoundException(SR.Hosting_ListenerNotFoundForActivationInRecycling(result.RequestUri.ToString())));
                    }
                    else
                    {
                        throw FxTrace.Exception.AsError(
                            new EndpointNotFoundException(SR.Hosting_ListenerNotFoundForActivation(result.RequestUri.ToString())));
                    }
                }
            }
        }

        static void EndHttpContextReceived(IAsyncResult httpContextReceivedResult)
        {
            using (DiagnosticUtility.ShouldUseActivity ? (ActivityHolder)httpContextReceivedResult.AsyncState : null)
            {
                HttpChannelListener channelListener =
                    (DiagnosticUtility.ShouldUseActivity ?
                        ((ActivityHolder)httpContextReceivedResult.AsyncState).context :
                        (HttpRequestContext)httpContextReceivedResult.AsyncState).Listener;

                channelListener.EndHttpContextReceived(httpContextReceivedResult);
            }
        }

        static void OnHttpContextReceived(IAsyncResult httpContextReceivedResult)
        {
            if (httpContextReceivedResult.CompletedSynchronously)
            {
                return;
            }

            Exception completionException = null;
            try
            {
                EndHttpContextReceived(httpContextReceivedResult);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }

                completionException = exception;
            }

            if (completionException != null)
            {
                HostedHttpContext context =
                    (HostedHttpContext)(DiagnosticUtility.ShouldUseActivity ?
                        ((ActivityHolder)httpContextReceivedResult.AsyncState).context :
                        httpContextReceivedResult.AsyncState);

                context.CompleteWithException(completionException);
            }
        }
    }

    [Fx.Tag.SecurityNote(Critical = "Captures HttpContext.Current on construction, then can apply that state at a later time and reset on Dispose."
        + "Whole object is critical because where it was initially constructed can be used to control HttpContext.set_Current later and HttpContext.set_Current requires an elevation")]
#pragma warning disable 618 // have not moved to the v4 security model yet
    [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
    class HostedThreadData
    {
        CultureInfo cultureInfo;
        CultureInfo uiCultureInfo;
        HttpContext httpContext;

        public HostedThreadData()
        {
            this.cultureInfo = CultureInfo.CurrentCulture;
            this.uiCultureInfo = CultureInfo.CurrentUICulture;
            this.httpContext = HttpContext.Current;
        }

        public IDisposable CreateContext()
        {
            return new HostedAspNetContext(this);
        }

        [SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
        static void UnsafeApplyData(HostedThreadData data)
        {
            // We set the CallContext.HostContext directly instead of setting HttpContext.Current because
            // the latter uses a demand instead of a link demand, which is very expensive in partial trust.
            System.Runtime.Remoting.Messaging.CallContext.HostContext = data.httpContext;
            
            Thread currentThread = Thread.CurrentThread;
            if (currentThread.CurrentCulture != data.cultureInfo)
            {
                currentThread.CurrentCulture = data.cultureInfo;
            }

            if (currentThread.CurrentUICulture != data.uiCultureInfo)
            {
                currentThread.CurrentUICulture = data.uiCultureInfo;
            }
        }

        class HostedAspNetContext : IDisposable
        {
            HostedThreadData oldData;

            public HostedAspNetContext(HostedThreadData newData)
            {
                oldData = new HostedThreadData();
                HostedThreadData.UnsafeApplyData(newData);
            }

            public void Dispose()
            {
                HostedThreadData.UnsafeApplyData(oldData);
            }
        }

    }
}
