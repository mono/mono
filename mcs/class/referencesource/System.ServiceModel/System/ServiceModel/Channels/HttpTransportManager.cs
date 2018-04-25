//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Runtime;
    using System.ServiceModel.Diagnostics.Application;
    using System.Runtime.Diagnostics;

    abstract class HttpTransportManager : TransportManager, ITransportManagerRegistration
    {
        volatile Dictionary<string, UriPrefixTable<HttpChannelListener>> addressTables;
        readonly HostNameComparisonMode hostNameComparisonMode;
        readonly Uri listenUri;
        readonly string realm;

        internal HttpTransportManager()
        {
            this.addressTables = new Dictionary<string, UriPrefixTable<HttpChannelListener>>();
        }

        internal HttpTransportManager(Uri listenUri, HostNameComparisonMode hostNameComparisonMode)
            : this()
        {
            this.hostNameComparisonMode = hostNameComparisonMode;
            this.listenUri = listenUri;
        }

        internal HttpTransportManager(Uri listenUri, HostNameComparisonMode hostNameComparisonMode, string realm)
            : this(listenUri, hostNameComparisonMode)
        {
            this.realm = realm;
        }

        internal string Realm
        {
            get
            {
                return this.realm;
            }
        }

        public HostNameComparisonMode HostNameComparisonMode
        {
            get
            {
                return this.hostNameComparisonMode;
            }
        }

        // are we hosted in Asp.Net? Default is false.
        internal bool IsHosted
        {
            get;
            set;
        }

        internal override string Scheme
        {
            get
            {
                return Uri.UriSchemeHttp;
            }
        }

        internal virtual UriPrefixTable<ITransportManagerRegistration> TransportManagerTable
        {
            get
            {
                return HttpChannelListener.StaticTransportManagerTable;
            }
        }

        public Uri ListenUri
        {
            get
            {
                return this.listenUri;
            }
        }

        protected void Fault(Exception exception)
        {
            lock (ThisLock)
            {
                foreach (KeyValuePair<string, UriPrefixTable<HttpChannelListener>> pair in this.addressTables)
                {
                    this.Fault(pair.Value, exception);
                }
            }
        }

        internal virtual bool IsCompatible(HttpChannelListener listener)
        {
            return (
                (this.hostNameComparisonMode == listener.HostNameComparisonMode) &&
                (this.realm == listener.Realm)
                );
        }

        internal override void OnClose(TimeSpan timeout)
        {
            Cleanup();
        }

        internal override void OnAbort()
        {
            Cleanup();
            base.OnAbort();
        }

        void Cleanup()
        {
            this.TransportManagerTable.UnregisterUri(this.ListenUri, this.HostNameComparisonMode);
        }

        protected void StartReceiveBytesActivity(ServiceModelActivity activity, Uri requestUri)
        {
            Fx.Assert(DiagnosticUtility.ShouldUseActivity, "should only call this if we're using SM Activities");
            ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityReceiveBytes, requestUri.ToString()), ActivityType.ReceiveBytes);
        }

        protected void TraceMessageReceived(EventTraceActivity eventTraceActivity, Uri listenUri)
        {
            if (TD.HttpMessageReceiveStartIsEnabled())
            {                
                TD.HttpMessageReceiveStart(eventTraceActivity);
            }
        }

        protected bool TryLookupUri(Uri requestUri, string requestMethod,
                    HostNameComparisonMode hostNameComparisonMode, bool isWebSocketRequest, out HttpChannelListener listener)
        {
            listener = null;

            if (isWebSocketRequest)
            {
                Fx.Assert(StringComparer.OrdinalIgnoreCase.Compare(requestMethod, "GET") == 0, "The requestMethod must be GET in WebSocket case.");
                requestMethod = WebSocketTransportSettings.WebSocketMethod;
            }
            
            if (requestMethod == null)
            {
                requestMethod = string.Empty;
            }

            UriPrefixTable<HttpChannelListener> addressTable;
            Dictionary<string, UriPrefixTable<HttpChannelListener>> localAddressTables = addressTables;

            // check for a method match if necessary
            HttpChannelListener methodListener = null;
            if (requestMethod.Length > 0)
            {
                if (localAddressTables.TryGetValue(requestMethod, out addressTable))
                {
                    if (addressTable.TryLookupUri(requestUri, hostNameComparisonMode, out methodListener)
                        && string.Compare(requestUri.AbsolutePath, methodListener.Uri.AbsolutePath, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        methodListener = null;
                    }
                }
            }
            // and also check the wildcard bucket 
            if (localAddressTables.TryGetValue(string.Empty, out addressTable)
                && addressTable.TryLookupUri(requestUri, hostNameComparisonMode, out listener))
            {
                if (methodListener != null && methodListener.Uri.AbsoluteUri.Length >= listener.Uri.AbsoluteUri.Length)
                {
                    listener = methodListener;
                }
            }
            else
            {
                listener = methodListener;
            }

            return (listener != null);
        }



        internal override void Register(TransportChannelListener channelListener)
        {
            string method = ((HttpChannelListener)channelListener).Method;

            UriPrefixTable<HttpChannelListener> addressTable;
            if (!addressTables.TryGetValue(method, out addressTable))
            {
                lock (ThisLock)
                {
                    if (!addressTables.TryGetValue(method, out addressTable))
                    {
                        Dictionary<string, UriPrefixTable<HttpChannelListener>> newAddressTables =
                            new Dictionary<string, UriPrefixTable<HttpChannelListener>>(addressTables);

                        addressTable = new UriPrefixTable<HttpChannelListener>();
                        newAddressTables[method] = addressTable;

                        addressTables = newAddressTables;
                    }
                }
            }

            addressTable.RegisterUri(channelListener.Uri,
                channelListener.InheritBaseAddressSettings ? hostNameComparisonMode : channelListener.HostNameComparisonModeInternal,
                (HttpChannelListener)channelListener);
        }

        IList<TransportManager> ITransportManagerRegistration.Select(TransportChannelListener channelListener)
        {
            IList<TransportManager> result = null;
            if (this.IsCompatible((HttpChannelListener)channelListener))
            {
                result = new List<TransportManager>();
                result.Add(this);
            }
            return result;
        }

        internal override void Unregister(TransportChannelListener channelListener)
        {
            UriPrefixTable<HttpChannelListener> addressTable;
            if (!addressTables.TryGetValue(((HttpChannelListener)channelListener).Method, out addressTable))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                     SR.ListenerFactoryNotRegistered, channelListener.Uri)));
            }

            HostNameComparisonMode registeredMode = channelListener.InheritBaseAddressSettings ? hostNameComparisonMode : channelListener.HostNameComparisonModeInternal;

            EnsureRegistered(addressTable, (HttpChannelListener)channelListener, registeredMode);
            addressTable.UnregisterUri(channelListener.Uri, registeredMode);
        }

        protected class ActivityHolder : IDisposable
        {
            internal HttpRequestContext context;
            internal ServiceModelActivity activity;

            public ActivityHolder(ServiceModelActivity activity, HttpRequestContext requestContext)
            {
                Fx.Assert(requestContext != null, "requestContext cannot be null.");
                this.activity = activity;
                this.context = requestContext;
            }

            public void Dispose()
            {
                if (this.activity != null)
                {
                    this.activity.Dispose();
                }
            }
        }
    }
}
