//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Reflection;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Dispatcher;
    using System.Threading;
    using System.Xml;
    using System.Xml.Schema;
    using WsdlNS = System.Web.Services.Description;

    // the description/metadata "mix-in"
    public class ServiceMetadataExtension : IExtension<ServiceHostBase>
    {
        const string BaseAddressPattern = "{%BaseAddress%}";
        static readonly Uri EmptyUri = new Uri(String.Empty, UriKind.Relative);

        static readonly Type[] httpGetSupportedChannels = new Type[] { typeof(IReplyChannel), };

        ServiceMetadataBehavior.MetadataExtensionInitializer initializer;
        MetadataSet metadata;
        WsdlNS.ServiceDescription singleWsdl;
        bool isInitialized = false;
        bool isSingleWsdlInitialized = false;
        Uri externalMetadataLocation;
        ServiceHostBase owner;
        object syncRoot = new object();
        object singleWsdlSyncRoot = new object();
        bool mexEnabled = false;
        bool httpGetEnabled = false;
        bool httpsGetEnabled = false;
        bool httpHelpPageEnabled = false;
        bool httpsHelpPageEnabled = false;
        Uri mexUrl;
        Uri httpGetUrl;
        Uri httpsGetUrl;
        Uri httpHelpPageUrl;
        Uri httpsHelpPageUrl;
        Binding httpHelpPageBinding;
        Binding httpsHelpPageBinding;
        Binding httpGetBinding;
        Binding httpsGetBinding;

        public ServiceMetadataExtension()
            : this(null)
        {
        }

        internal ServiceMetadataExtension(ServiceMetadataBehavior.MetadataExtensionInitializer initializer)
        {
            this.initializer = initializer;
        }

        internal ServiceMetadataBehavior.MetadataExtensionInitializer Initializer
        {
            get { return this.initializer; }
            set { this.initializer = value; }
        }

        public MetadataSet Metadata
        {
            get
            {
                EnsureInitialized();

                return this.metadata;
            }
        }

        public WsdlNS.ServiceDescription SingleWsdl
        {
            get
            {
                EnsureSingleWsdlInitialized();

                return this.singleWsdl;
            }
        }

        internal Uri ExternalMetadataLocation
        {
            get { return this.externalMetadataLocation; }
            set { this.externalMetadataLocation = value; }
        }

        internal bool MexEnabled
        {
            get { return this.mexEnabled; }
            set { this.mexEnabled = value; }
        }

        internal bool HttpGetEnabled
        {
            get { return this.httpGetEnabled; }
            set { this.httpGetEnabled = value; }
        }

        internal bool HttpsGetEnabled
        {
            get { return this.httpsGetEnabled; }
            set { this.httpsGetEnabled = value; }
        }

        internal bool HelpPageEnabled
        {
            get { return this.httpHelpPageEnabled || this.httpsHelpPageEnabled; }
        }

        internal bool MetadataEnabled
        {
            get { return this.mexEnabled || this.httpGetEnabled || this.httpsGetEnabled; }
        }

        internal bool HttpHelpPageEnabled
        {
            get { return this.httpHelpPageEnabled; }
            set { this.httpHelpPageEnabled = value; }
        }

        internal bool HttpsHelpPageEnabled
        {
            get { return this.httpsHelpPageEnabled; }
            set { this.httpsHelpPageEnabled = value; }
        }

        internal Uri MexUrl
        {
            get { return this.mexUrl; }
            set { this.mexUrl = value; }
        }

        internal Uri HttpGetUrl
        {
            get { return this.httpGetUrl; }
            set { this.httpGetUrl = value; }
        }

        internal Uri HttpsGetUrl
        {
            get { return this.httpsGetUrl; }
            set { this.httpsGetUrl = value; }
        }

        internal Uri HttpHelpPageUrl
        {
            get { return this.httpHelpPageUrl; }
            set { this.httpHelpPageUrl = value; }
        }

        internal Uri HttpsHelpPageUrl
        {
            get { return this.httpsHelpPageUrl; }
            set { this.httpsHelpPageUrl = value; }
        }

        internal Binding HttpHelpPageBinding
        {
            get { return this.httpHelpPageBinding; }
            set { this.httpHelpPageBinding = value; }
        }

        internal Binding HttpsHelpPageBinding
        {
            get { return this.httpsHelpPageBinding; }
            set { this.httpsHelpPageBinding = value; }
        }

        internal Binding HttpGetBinding
        {
            get { return this.httpGetBinding; }
            set { this.httpGetBinding = value; }
        }

        internal Binding HttpsGetBinding
        {
            get { return this.httpsGetBinding; }
            set { this.httpsGetBinding = value; }
        }

        internal bool UpdateAddressDynamically { get; set; }

        // This dictionary should not be mutated after open
        internal IDictionary<string, int> UpdatePortsByScheme { get; set; }

        internal static bool TryGetHttpHostAndPort(Uri listenUri, Message request, out string host, out int port)
        {
            host = null;
            port = 0;

            // Get the host hedaer
            object property;
            if (!request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out property))
            {
                return false;
            }
            HttpRequestMessageProperty httpRequest = property as HttpRequestMessageProperty;
            if (httpRequest == null)
            {
                return false;
            }
            string hostHeader = httpRequest.Headers[HttpRequestHeader.Host];
            if (string.IsNullOrEmpty(hostHeader))
            {
                return false;
            }

            // Split and validate the host and port
            string hostUriString = string.Concat(listenUri.Scheme, "://", hostHeader);
            Uri hostUri;
            if (!Uri.TryCreate(hostUriString, UriKind.Absolute, out hostUri))
            {
                return false;
            }

            host = hostUri.Host;
            port = hostUri.Port;
            return true;
        }

        void EnsureInitialized()
        {
            if (!this.isInitialized)
            {
                lock (this.syncRoot)
                {
                    if (!this.isInitialized)
                    {
                        if (this.initializer != null)
                        {
                            // the following call will initialize this
                            // it will use the Metadata property to do the initialization
                            // this will call back into this method, but exit because isInitialized is set.
                            // if other threads try to call these methods, they will block on the lock
                            this.metadata = this.initializer.GenerateMetadata();
                        }

                        if (this.metadata == null)
                        {
                            this.metadata = new MetadataSet();
                        }

                        Thread.MemoryBarrier();

                        this.isInitialized = true;
                        this.initializer = null;
                    }
                }
            }
        }

        void EnsureSingleWsdlInitialized()
        {
            if (!this.isSingleWsdlInitialized)
            {
                lock (this.singleWsdlSyncRoot)
                {
                    if (!this.isSingleWsdlInitialized)
                    {
                        // Could throw NotSupportedException if multiple contract namespaces. Let the exception propagate to the dispatcher and show up on the html error page
                        this.singleWsdl = WsdlHelper.GetSingleWsdl(this.Metadata);
                        this.isSingleWsdlInitialized = true;
                    }
                }
            }
        }

        void IExtension<ServiceHostBase>.Attach(ServiceHostBase owner)
        {
            if (owner == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("owner"));

            if (this.owner != null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.TheServiceMetadataExtensionInstanceCouldNot2_0)));

            owner.ThrowIfClosedOrOpened();

            this.owner = owner;
        }

        void IExtension<ServiceHostBase>.Detach(ServiceHostBase owner)
        {
            if (owner == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("owner");

            if (this.owner == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.TheServiceMetadataExtensionInstanceCouldNot3_0)));

            if (this.owner != owner)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("owner", SR.GetString(SR.TheServiceMetadataExtensionInstanceCouldNot4_0));

            this.owner.ThrowIfClosedOrOpened();

            this.owner = null;
        }

        static internal ServiceMetadataExtension EnsureServiceMetadataExtension(ServiceDescription description, ServiceHostBase host)
        {
            ServiceMetadataExtension mex = host.Extensions.Find<ServiceMetadataExtension>();
            if (mex == null)
            {
                mex = new ServiceMetadataExtension();
                host.Extensions.Add(mex);
            }

            return mex;
        }

        internal ChannelDispatcher EnsureGetDispatcher(Uri listenUri)
        {
            ChannelDispatcher channelDispatcher = FindGetDispatcher(listenUri);

            if (channelDispatcher == null)
            {
                channelDispatcher = CreateGetDispatcher(listenUri);
                owner.ChannelDispatchers.Add(channelDispatcher);
            }

            return channelDispatcher;
        }

        internal ChannelDispatcher EnsureGetDispatcher(Uri listenUri, bool isServiceDebugBehavior)
        {
            ChannelDispatcher channelDispatcher = FindGetDispatcher(listenUri);

            Binding binding;
            if (channelDispatcher == null)
            {
                if (listenUri.Scheme == Uri.UriSchemeHttp)
                {
                    if (isServiceDebugBehavior)
                    {
                        binding = this.httpHelpPageBinding ?? MetadataExchangeBindings.HttpGet;
                    }
                    else
                    {
                        binding = this.httpGetBinding ?? MetadataExchangeBindings.HttpGet;
                    }
                }
                else if (listenUri.Scheme == Uri.UriSchemeHttps)
                {
                    if (isServiceDebugBehavior)
                    {
                        binding = this.httpsHelpPageBinding ?? MetadataExchangeBindings.HttpsGet;
                    }
                    else
                    {
                        binding = this.httpsGetBinding ?? MetadataExchangeBindings.HttpsGet;
                    }
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.SFxGetChannelDispatcherDoesNotSupportScheme, typeof(ChannelDispatcher).Name, Uri.UriSchemeHttp, Uri.UriSchemeHttps)));
                }
                channelDispatcher = CreateGetDispatcher(listenUri, binding);
                owner.ChannelDispatchers.Add(channelDispatcher);
            }

            return channelDispatcher;
        }

        ChannelDispatcher FindGetDispatcher(Uri listenUri)
        {
            foreach (ChannelDispatcherBase channelDispatcherBase in owner.ChannelDispatchers)
            {
                ChannelDispatcher channelDispatcher = channelDispatcherBase as ChannelDispatcher;
                if (channelDispatcher != null && channelDispatcher.Listener.Uri == listenUri)
                {
                    if (channelDispatcher.Endpoints.Count == 1 &&
                        channelDispatcher.Endpoints[0].DispatchRuntime.SingletonInstanceContext != null &&
                        channelDispatcher.Endpoints[0].DispatchRuntime.SingletonInstanceContext.UserObject is HttpGetImpl)
                    {
                        return channelDispatcher;
                    }
                }
            }
            return null;
        }

        ChannelDispatcher CreateGetDispatcher(Uri listenUri)
        {
            if (listenUri.Scheme == Uri.UriSchemeHttp)
            {
                return CreateGetDispatcher(listenUri, MetadataExchangeBindings.HttpGet);
            }
            else if (listenUri.Scheme == Uri.UriSchemeHttps)
            {
                return CreateGetDispatcher(listenUri, MetadataExchangeBindings.HttpsGet);
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.SFxGetChannelDispatcherDoesNotSupportScheme, typeof(ChannelDispatcher).Name, Uri.UriSchemeHttp, Uri.UriSchemeHttps)));
            }
        }

        ChannelDispatcher CreateGetDispatcher(Uri listenUri, Binding binding)
        {
            EndpointAddress address = new EndpointAddress(listenUri);
            Uri listenUriBaseAddress = listenUri;
            string listenUriRelativeAddress = string.Empty;

            //Set up binding parameter collection 
            BindingParameterCollection parameters = owner.GetBindingParameters();
            AspNetEnvironment.Current.AddMetadataBindingParameters(listenUriBaseAddress, owner.Description.Behaviors, parameters);

            // find listener for HTTP GET
            IChannelListener listener = null;
            if (binding.CanBuildChannelListener<IReplyChannel>(parameters))
            {
                listener = binding.BuildChannelListener<IReplyChannel>(listenUriBaseAddress, listenUriRelativeAddress, parameters);
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.SFxBindingNotSupportedForMetadataHttpGet)));
            }

            //create dispatchers
            ChannelDispatcher channelDispatcher = new ChannelDispatcher(listener, HttpGetImpl.MetadataHttpGetBinding, binding);
            channelDispatcher.MessageVersion = binding.MessageVersion;
            EndpointDispatcher dispatcher = new EndpointDispatcher(address, HttpGetImpl.ContractName, HttpGetImpl.ContractNamespace, true);

            //Add operation
            DispatchOperation operationDispatcher = new DispatchOperation(dispatcher.DispatchRuntime, HttpGetImpl.GetMethodName, HttpGetImpl.RequestAction, HttpGetImpl.ReplyAction);
            operationDispatcher.Formatter = MessageOperationFormatter.Instance;
            MethodInfo methodInfo = typeof(IHttpGetMetadata).GetMethod(HttpGetImpl.GetMethodName);
            operationDispatcher.Invoker = new SyncMethodInvoker(methodInfo);
            dispatcher.DispatchRuntime.Operations.Add(operationDispatcher);

            //wire up dispatchers
            HttpGetImpl impl = new HttpGetImpl(this, listener.Uri);
            dispatcher.DispatchRuntime.SingletonInstanceContext = new InstanceContext(owner, impl, false);
            dispatcher.DispatchRuntime.MessageInspectors.Add(impl);
            channelDispatcher.Endpoints.Add(dispatcher);
            dispatcher.ContractFilter = new MatchAllMessageFilter();
            dispatcher.FilterPriority = 0;
            dispatcher.DispatchRuntime.InstanceContextProvider = InstanceContextProviderBase.GetProviderForMode(InstanceContextMode.Single, dispatcher.DispatchRuntime);
            channelDispatcher.ServiceThrottle = owner.ServiceThrottle;

            ServiceDebugBehavior sdb = owner.Description.Behaviors.Find<ServiceDebugBehavior>();
            if (sdb != null)
                channelDispatcher.IncludeExceptionDetailInFaults |= sdb.IncludeExceptionDetailInFaults;

            ServiceBehaviorAttribute sba = owner.Description.Behaviors.Find<ServiceBehaviorAttribute>();
            if (sba != null)
                channelDispatcher.IncludeExceptionDetailInFaults |= sba.IncludeExceptionDetailInFaults;


            return channelDispatcher;
        }

        WriteFilter GetWriteFilter(Message request, Uri listenUri, bool removeBaseAddress)
        {
            WriteFilter result = null;
            if (this.UpdateAddressDynamically)
            {
                // Update address dynamically based on the request URI
                result = GetDynamicAddressWriter(request, listenUri, removeBaseAddress);
            }
            if (result == null)
            {
                // Just use the statically known listen URI
                if (removeBaseAddress)
                {
                    result = new LocationUpdatingWriter(BaseAddressPattern, null);
                }
                else
                {
                    result = new LocationUpdatingWriter(BaseAddressPattern, listenUri.ToString());
                }
            }
            return result;
        }

        DynamicAddressUpdateWriter GetDynamicAddressWriter(Message request, Uri listenUri, bool removeBaseAddress)
        {
            string requestHost;
            int requestPort;
            if (!TryGetHttpHostAndPort(listenUri, request, out requestHost, out requestPort))
            {
                if (request.Headers.To == null)
                {
                    return null;
                }
                requestHost = request.Headers.To.Host;
                requestPort = request.Headers.To.Port;
            }

            // Perf optimization: don't do dynamic update if it would be a no-op.
            // Ordinal string comparison is okay; it just means we don't get the perf optimization
            // if the listen host and request host are case-insensitively equal.
            if (requestHost == listenUri.Host &&
                requestPort == listenUri.Port &&
                (UpdatePortsByScheme == null || UpdatePortsByScheme.Count == 0))
            {
                return null;
            }
            return new DynamicAddressUpdateWriter(
                listenUri, requestHost, requestPort, this.UpdatePortsByScheme, removeBaseAddress);
        }

        internal class MetadataBindingParameter { }

        internal class WSMexImpl : IMetadataExchange
        {
            internal const string MetadataMexBinding = "ServiceMetadataBehaviorMexBinding";
            internal const string ContractName = MetadataStrings.WSTransfer.Name;
            internal const string ContractNamespace = MetadataStrings.WSTransfer.Namespace;
            internal const string GetMethodName = "Get";
            internal const string RequestAction = MetadataStrings.WSTransfer.GetAction;
            internal const string ReplyAction = MetadataStrings.WSTransfer.GetResponseAction;

            ServiceMetadataExtension parent;
            MetadataSet metadataLocationSet;
            TypedMessageConverter converter;
            Uri listenUri;

            bool isListeningOnHttps;

            internal WSMexImpl(ServiceMetadataExtension parent, bool isListeningOnHttps, Uri listenUri)
            {
                this.parent = parent;
                this.isListeningOnHttps = isListeningOnHttps;
                this.listenUri = listenUri;

                if (this.parent.ExternalMetadataLocation != null && this.parent.ExternalMetadataLocation != EmptyUri)
                {
                    this.metadataLocationSet = new MetadataSet();
                    string location = this.GetLocationToReturn();
                    MetadataSection metadataLocationSection = new MetadataSection(MetadataSection.ServiceDescriptionDialect, null, new MetadataLocation(location));
                    this.metadataLocationSet.MetadataSections.Add(metadataLocationSection);
                }
            }

            internal bool IsListeningOnHttps
            {
                get { return this.isListeningOnHttps; }
                set { this.isListeningOnHttps = value; }
            }

            string GetLocationToReturn()
            {
                Fx.Assert(this.parent.ExternalMetadataLocation != null, "");
                Uri location = this.parent.ExternalMetadataLocation;

                if (!location.IsAbsoluteUri)
                {
                    Uri httpAddr = parent.owner.GetVia(Uri.UriSchemeHttp, location);
                    Uri httpsAddr = parent.owner.GetVia(Uri.UriSchemeHttps, location);

                    if (this.IsListeningOnHttps && httpsAddr != null)
                    {
                        location = httpsAddr;
                    }
                    else if (httpAddr != null)
                    {
                        location = httpAddr;
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("ExternalMetadataLocation", SR.GetString(SR.SFxBadMetadataLocationNoAppropriateBaseAddress, this.parent.ExternalMetadataLocation.OriginalString));
                    }
                }

                return location.ToString();
            }

            MetadataSet GatherMetadata(string dialect, string identifier)
            {
                if (metadataLocationSet != null)
                {
                    return metadataLocationSet;
                }
                else
                {
                    MetadataSet metadataSet = new MetadataSet();
                    foreach (MetadataSection document in parent.Metadata.MetadataSections)
                    {
                        if ((dialect == null || dialect == document.Dialect) &&
                            (identifier == null || identifier == document.Identifier))
                            metadataSet.MetadataSections.Add(document);
                    }
                    
                    return metadataSet;
                }
            }

            public Message Get(Message request)
            {
                GetResponse response = new GetResponse();
                response.Metadata = GatherMetadata(null, null);

                response.Metadata.WriteFilter = parent.GetWriteFilter(request, this.listenUri, true);

                if (converter == null)
                    converter = TypedMessageConverter.Create(typeof(GetResponse), ReplyAction);

                return converter.ToMessage(response, request.Version);
            }

            public IAsyncResult BeginGet(Message request, AsyncCallback callback, object state)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }

            public Message EndGet(IAsyncResult result)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }
        }

        //If this contract is changed, you may need to change ServiceMetadataExtension.CreateHttpGetDispatcher()
        [ServiceContract]
        internal interface IHttpGetMetadata
        {
            [OperationContract(Action = MessageHeaders.WildcardAction, ReplyAction = MessageHeaders.WildcardAction)]
            Message Get(Message msg);
        }

        internal class HttpGetImpl : IHttpGetMetadata, IDispatchMessageInspector
        {
            const string DiscoToken = "disco token";
            const string DiscoQueryString = "disco";
            const string WsdlQueryString = "wsdl";
            const string XsdQueryString = "xsd";
            const string SingleWsdlQueryString = "singleWsdl";

            const string HtmlContentType = "text/html; charset=UTF-8";
            const string XmlContentType = "text/xml; charset=UTF-8";

            const int closeTimeoutInSeconds = 90;
            const int maxQueryStringChars = 2048;

            internal const string MetadataHttpGetBinding = "ServiceMetadataBehaviorHttpGetBinding";
            internal const string ContractName = "IHttpGetHelpPageAndMetadataContract";
            internal const string ContractNamespace = "http://schemas.microsoft.com/2006/04/http/metadata";
            internal const string GetMethodName = "Get";
            internal const string RequestAction = MessageHeaders.WildcardAction;
            internal const string ReplyAction = MessageHeaders.WildcardAction;
            internal const string HtmlBreak = "<BR/>";


            static string[] NoQueries = new string[0];

            ServiceMetadataExtension parent;
            object sync = new object();
            InitializationData initData;
            Uri listenUri;
            bool helpPageEnabled = false;
            bool getWsdlEnabled = false;

            internal HttpGetImpl(ServiceMetadataExtension parent, Uri listenUri)
            {
                this.parent = parent;
                this.listenUri = listenUri;
            }

            public bool HelpPageEnabled
            {
                get { return this.helpPageEnabled; }
                set { this.helpPageEnabled = value; }
            }
            public bool GetWsdlEnabled
            {
                get { return this.getWsdlEnabled; }
                set { this.getWsdlEnabled = value; }
            }

            InitializationData GetInitData()
            {
                if (this.initData == null)
                {
                    lock (this.sync)
                    {
                        if (this.initData == null)
                        {
                            this.initData = InitializationData.InitializeFrom(parent);
                        }
                    }
                }
                return this.initData;
            }

            string FindWsdlReference(DynamicAddressUpdateWriter addressUpdater)
            {

                if (this.parent.ExternalMetadataLocation == null || this.parent.ExternalMetadataLocation == EmptyUri)
                {
                    return null;
                }
                else
                {
                    Uri location = this.parent.ExternalMetadataLocation;

                    Uri result = ServiceHost.GetUri(this.listenUri, location);
                    if (addressUpdater != null)
                    {
                        addressUpdater.UpdateUri(ref result);
                    }
                    return result.ToString();
                }
            }

            bool TryHandleDocumentationRequest(Message httpGetRequest, string[] queries, out Message replyMessage)
            {
                replyMessage = null;

                if (!this.HelpPageEnabled)
                    return false;

                if (parent.MetadataEnabled)
                {
                    string discoUrl = null;
                    string wsdlUrl = null;
                    string httpGetUrl = null;
                    string singleWsdlUrl = null;
                    bool linkMetadata = true;

                    DynamicAddressUpdateWriter addressUpdater = null;
                    if (parent.UpdateAddressDynamically)
                    {
                        addressUpdater = parent.GetDynamicAddressWriter(httpGetRequest, this.listenUri, false);
                    }

                    wsdlUrl = FindWsdlReference(addressUpdater);

                    httpGetUrl = GetHttpGetUrl(addressUpdater);

                    if (wsdlUrl == null && httpGetUrl != null)
                    {
                        wsdlUrl = httpGetUrl + "?" + WsdlQueryString;
                        singleWsdlUrl = httpGetUrl + "?" + SingleWsdlQueryString;
                    }

                    if (httpGetUrl != null)
                        discoUrl = httpGetUrl + "?" + DiscoQueryString;

                    if (wsdlUrl == null)
                    {
                        wsdlUrl = GetMexUrl(addressUpdater);
                        linkMetadata = false;
                    }

                    replyMessage = new MetadataOnHelpPageMessage(discoUrl, wsdlUrl, singleWsdlUrl, GetInitData().ServiceName, GetInitData().ClientName, linkMetadata);
                }
                else
                {
                    replyMessage = new MetadataOffHelpPageMessage(GetInitData().ServiceName);
                }

                AddHttpProperty(replyMessage, HttpStatusCode.OK, HtmlContentType);

                return true;
            }

            string GetHttpGetUrl(DynamicAddressUpdateWriter addressUpdater)
            {
                Uri result = null;
                if (this.listenUri.Scheme == Uri.UriSchemeHttp)
                {
                    if (parent.HttpGetEnabled)
                        result = parent.HttpGetUrl;
                    else if (parent.HttpsGetEnabled)
                        result = parent.HttpsGetUrl;
                }
                else
                {
                    if (parent.HttpsGetEnabled)
                        result = parent.HttpsGetUrl;
                    else if (parent.HttpGetEnabled)
                        result = parent.HttpGetUrl;
                }

                if (result != null)
                {
                    if (addressUpdater != null)
                    {
                        addressUpdater.UpdateUri(ref result, this.listenUri.Scheme != result.Scheme /*updateBaseAddressOnly*/);
                    }
                    return result.ToString();
                }

                return null;
            }

            string GetMexUrl(DynamicAddressUpdateWriter addressUpdater)
            {
                if (parent.MexEnabled)
                {
                    Uri result = parent.MexUrl;
                    if (addressUpdater != null)
                    {
                        addressUpdater.UpdateUri(ref result);
                    }
                    return result.ToString();
                }

                return null;
            }

            bool TryHandleMetadataRequest(Message httpGetRequest, string[] queries, out Message replyMessage)
            {
                replyMessage = null;

                if (!this.GetWsdlEnabled)
                    return false;

                WriteFilter writeFilter = parent.GetWriteFilter(httpGetRequest, this.listenUri, false);

                string query = FindQuery(queries);

                if (String.IsNullOrEmpty(query))
                {
                    //if the documentation page is not available return the default wsdl if it exists
                    if (!this.helpPageEnabled && GetInitData().DefaultWsdl != null)
                    {
                        // use the default WSDL
                        using (httpGetRequest)
                        {
                            replyMessage = new ServiceDescriptionMessage(
                                GetInitData().DefaultWsdl, writeFilter);
                            AddHttpProperty(replyMessage, HttpStatusCode.OK, XmlContentType);

                            GetInitData().FixImportAddresses();
                            return true;
                        }
                    }
                    return false;
                }

                // try to look the document up in the query table
                object doc;
                if (GetInitData().TryQueryLookup(query, out doc))
                {
                    using (httpGetRequest)
                    {
                        if (doc is WsdlNS.ServiceDescription)
                        {
                            replyMessage = new ServiceDescriptionMessage(
                                (WsdlNS.ServiceDescription)doc, writeFilter);
                        }
                        else if (doc is XmlSchema)
                        {
                            replyMessage = new XmlSchemaMessage(
                                ((XmlSchema)doc), writeFilter);
                        }
                        else if (doc is string)
                        {
                            if (((string)doc) == DiscoToken)
                            {
                                replyMessage = CreateDiscoMessage(writeFilter as DynamicAddressUpdateWriter);
                            }
                            else
                            {
                                Fx.Assert("Bad object in HttpGetImpl docFromQuery table");
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Bad object in HttpGetImpl docFromQuery table")));
                            }
                        }
                        else
                        {
                            Fx.Assert("Bad object in HttpGetImpl docFromQuery table");
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Bad object in HttpGetImpl docFromQuery table")));
                        }

                        AddHttpProperty(replyMessage, HttpStatusCode.OK, XmlContentType);

                        GetInitData().FixImportAddresses();
                        return true;
                    }
                }

                // otherwise see if they just wanted ?WSDL
                if (String.Compare(query, WsdlQueryString, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (GetInitData().DefaultWsdl != null)
                    {
                        // use the default WSDL
                        using (httpGetRequest)
                        {
                            replyMessage = new ServiceDescriptionMessage(
                                GetInitData().DefaultWsdl, writeFilter);
                            AddHttpProperty(replyMessage, HttpStatusCode.OK, XmlContentType);

                            GetInitData().FixImportAddresses();

                            return true;
                        }
                    }

                    // or redirect to an external WSDL
                    string wsdlReference = FindWsdlReference(writeFilter as DynamicAddressUpdateWriter);
                    if (wsdlReference != null)
                    {
                        replyMessage = CreateRedirectMessage(wsdlReference);
                        return true;
                    }
                }

                // ?singleWSDL
                if (String.Compare(query, SingleWsdlQueryString, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    WsdlNS.ServiceDescription singleWSDL = parent.SingleWsdl;
                    if (singleWSDL != null)
                    {
                        using (httpGetRequest)
                        {
                            replyMessage = new ServiceDescriptionMessage(
                                singleWSDL, writeFilter);
                            AddHttpProperty(replyMessage, HttpStatusCode.OK, XmlContentType);
                            return true;
                        }
                    }
                }

                // we weren't able to handle the request -- return the documentation page if available
                return false;
            }

            Message CreateDiscoMessage(DynamicAddressUpdateWriter addressUpdater)
            {
                Uri wsdlUrlBase = this.listenUri;
                if (addressUpdater != null)
                {
                    addressUpdater.UpdateUri(ref wsdlUrlBase);
                }
                string wsdlUrl = wsdlUrlBase.ToString() + "?" + WsdlQueryString;

                Uri docUrl = null;
                if (this.listenUri.Scheme == Uri.UriSchemeHttp)
                {
                    if (parent.HttpHelpPageEnabled)
                        docUrl = parent.HttpHelpPageUrl;
                    else if (parent.HttpsHelpPageEnabled)
                        docUrl = parent.HttpsGetUrl;
                }
                else
                {
                    if (parent.HttpsHelpPageEnabled)
                        docUrl = parent.HttpsHelpPageUrl;
                    else if (parent.HttpHelpPageEnabled)
                        docUrl = parent.HttpGetUrl;
                }
                if (addressUpdater != null)
                {
                    addressUpdater.UpdateUri(ref docUrl);
                }

                return new DiscoMessage(wsdlUrl, docUrl.ToString());
            }

            string FindQuery(string[] queries)
            {
                string query = null;
                foreach (string q in queries)
                {
                    int start = (q.Length > 0 && q[0] == '?') ? 1 : 0;
                    if (String.Compare(q, start, WsdlQueryString, 0, WsdlQueryString.Length, StringComparison.OrdinalIgnoreCase) == 0)
                        query = q;
                    else if (String.Compare(q, start, XsdQueryString, 0, XsdQueryString.Length, StringComparison.OrdinalIgnoreCase) == 0)
                        query = q;
                    else if (String.Compare(q, start, SingleWsdlQueryString, 0, SingleWsdlQueryString.Length, StringComparison.OrdinalIgnoreCase) == 0)
                        query = q;
                    else if (parent.HelpPageEnabled && (String.Compare(q, start, DiscoQueryString, 0, DiscoQueryString.Length, StringComparison.OrdinalIgnoreCase) == 0))
                        query = q;
                }
                return query;
            }

            Message ProcessHttpRequest(Message httpGetRequest)
            {
                string queryString = httpGetRequest.Properties.Via.Query;

                if (queryString.Length > maxQueryStringChars)
                    return CreateHttpResponseMessage(HttpStatusCode.RequestUriTooLong);

                if (queryString.StartsWith("?", StringComparison.OrdinalIgnoreCase))
                    queryString = queryString.Substring(1);

                string[] queries = queryString.Length > 0 ? queryString.Split('&') : NoQueries;

                Message replyMessage = null;
                if (TryHandleMetadataRequest(httpGetRequest, queries, out replyMessage))
                    return replyMessage;

                if (TryHandleDocumentationRequest(httpGetRequest, queries, out replyMessage))
                    return replyMessage;


                return CreateHttpResponseMessage(HttpStatusCode.MethodNotAllowed);
            }

            public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
            {
                return request.Version;
            }

            public void BeforeSendReply(ref Message reply, object correlationState)
            {
                if ((reply != null) && reply.IsFault)
                {
                    string error = SR.GetString(SR.SFxInternalServerError);
                    ExceptionDetail exceptionDetail = null;

                    MessageFault fault = MessageFault.CreateFault(reply, /* maxBufferSize */ 64 * 1024);
                    if (fault.HasDetail)
                    {
                        exceptionDetail = fault.GetDetail<ExceptionDetail>();
                        if (exceptionDetail != null)
                        {
                            error = SR.GetString(SR.SFxDocExt_Error);
                        }
                    }

                    reply = new MetadataOnHelpPageMessage(error, exceptionDetail);
                    AddHttpProperty(reply, HttpStatusCode.InternalServerError, HtmlContentType);
                }
            }

            public Message Get(Message message)
            {
                return ProcessHttpRequest(message);
            }

            class InitializationData
            {
                readonly Dictionary<string, object> docFromQuery;
                readonly Dictionary<object, string> queryFromDoc;

                WsdlNS.ServiceDescriptionCollection wsdls;
                XmlSchemaSet xsds;

                public string ServiceName;
                public string ClientName;
                public WsdlNS.ServiceDescription DefaultWsdl;

                InitializationData(
                    Dictionary<string, object> docFromQuery,
                    Dictionary<object, string> queryFromDoc,
                    WsdlNS.ServiceDescriptionCollection wsdls,
                    XmlSchemaSet xsds)
                {
                    this.docFromQuery = docFromQuery;
                    this.queryFromDoc = queryFromDoc;
                    this.wsdls = wsdls;
                    this.xsds = xsds;
                }

                public bool TryQueryLookup(string query, out object doc)
                {
                    return docFromQuery.TryGetValue(query, out doc);
                }

                public static InitializationData InitializeFrom(ServiceMetadataExtension extension)
                {
                    Dictionary<string, object> docFromQueryInit = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    Dictionary<object, string> queryFromDocInit = new Dictionary<object, string>();

                    // this collection type provides useful lookup features
                    WsdlNS.ServiceDescriptionCollection wsdls = CollectWsdls(extension.Metadata);
                    XmlSchemaSet xsds = CollectXsds(extension.Metadata);

                    WsdlNS.ServiceDescription defaultWsdl = null;
                    WsdlNS.Service someService = GetAnyService(wsdls);
                    if (someService != null)
                        defaultWsdl = someService.ServiceDescription;

                    // WSDLs
                    {
                        int i = 0;
                        foreach (WsdlNS.ServiceDescription wsdlDoc in wsdls)
                        {
                            string query = WsdlQueryString;
                            if (wsdlDoc != defaultWsdl) // don't count the WSDL at ?WSDL
                                query += "=wsdl" + (i++).ToString(System.Globalization.CultureInfo.InvariantCulture);

                            docFromQueryInit.Add(query, wsdlDoc);
                            queryFromDocInit.Add(wsdlDoc, query);
                        }
                    }

                    // XSDs
                    {
                        int i = 0;
                        foreach (XmlSchema xsdDoc in xsds.Schemas())
                        {
                            string query = XsdQueryString + "=xsd" + (i++).ToString(System.Globalization.CultureInfo.InvariantCulture);
                            docFromQueryInit.Add(query, xsdDoc);
                            queryFromDocInit.Add(xsdDoc, query);
                        }
                    }

                    // Disco
                    if (extension.HelpPageEnabled)
                    {
                        string query = DiscoQueryString;
                        docFromQueryInit.Add(query, DiscoToken);
                        queryFromDocInit.Add(DiscoToken, query);
                    }

                    InitializationData data = new InitializationData(docFromQueryInit, queryFromDocInit, wsdls, xsds);

                    data.DefaultWsdl = defaultWsdl;
                    data.ServiceName = GetAnyWsdlName(wsdls);
                    data.ClientName = ClientClassGenerator.GetClientClassName(GetAnyContractName(wsdls) ?? "IHello");

                    return data;
                }

                static WsdlNS.ServiceDescriptionCollection CollectWsdls(MetadataSet metadata)
                {
                    WsdlNS.ServiceDescriptionCollection wsdls = new WsdlNS.ServiceDescriptionCollection();
                    foreach (MetadataSection section in metadata.MetadataSections)
                    {
                        if (section.Metadata is WsdlNS.ServiceDescription)
                        {
                            wsdls.Add((WsdlNS.ServiceDescription)section.Metadata);
                        }
                    }
                    return wsdls;
                }

                static XmlSchemaSet CollectXsds(MetadataSet metadata)
                {
                    XmlSchemaSet xsds = new XmlSchemaSet();
                    xsds.XmlResolver = null;
                    foreach (MetadataSection section in metadata.MetadataSections)
                    {
                        if (section.Metadata is XmlSchema)
                        {
                            xsds.Add((XmlSchema)section.Metadata);
                        }
                    }
                    return xsds;
                }

                internal void FixImportAddresses()
                {
                    // fixup imports and includes with addresses
                    // WSDLs
                    foreach (WsdlNS.ServiceDescription wsdlDoc in this.wsdls)
                    {
                        FixImportAddresses(wsdlDoc);
                    }
                    // XSDs
                    foreach (XmlSchema xsdDoc in this.xsds.Schemas())
                    {
                        FixImportAddresses(xsdDoc);
                    }

                }

                void FixImportAddresses(WsdlNS.ServiceDescription wsdlDoc)
                {
                    foreach (WsdlNS.Import import in wsdlDoc.Imports)
                    {
                        if (!String.IsNullOrEmpty(import.Location)) continue;

                        WsdlNS.ServiceDescription targetDoc = this.wsdls[import.Namespace ?? String.Empty];
                        if (targetDoc != null)
                        {
                            string query = queryFromDoc[targetDoc];
                            import.Location = BaseAddressPattern + "?" + query;
                        }
                    }

                    if (wsdlDoc.Types != null)
                    {
                        foreach (XmlSchema xsdDoc in wsdlDoc.Types.Schemas)
                        {
                            FixImportAddresses(xsdDoc);
                        }
                    }
                }

                void FixImportAddresses(XmlSchema xsdDoc)
                {
                    foreach (XmlSchemaObject o in xsdDoc.Includes)
                    {
                        XmlSchemaExternal external = o as XmlSchemaExternal;
                        if (external == null || !String.IsNullOrEmpty(external.SchemaLocation)) continue;

                        string targetNs = external is XmlSchemaImport ? ((XmlSchemaImport)external).Namespace : xsdDoc.TargetNamespace;

                        foreach (XmlSchema targetXsd in this.xsds.Schemas(targetNs ?? String.Empty))
                        {
                            if (targetXsd != xsdDoc)
                            {
                                string query = this.queryFromDoc[targetXsd];
                                external.SchemaLocation = BaseAddressPattern + "?" + query;
                                break;
                            }
                        }
                    }
                }

                static string GetAnyContractName(WsdlNS.ServiceDescriptionCollection wsdls)
                {
                    // try to track down a WSDL portType name using a wsdl:service as a starting point
                    foreach (WsdlNS.ServiceDescription wsdl in wsdls)
                    {
                        foreach (WsdlNS.Service service in wsdl.Services)
                        {
                            foreach (WsdlNS.Port port in service.Ports)
                            {
                                if (!port.Binding.IsEmpty)
                                {
                                    WsdlNS.Binding binding = wsdls.GetBinding(port.Binding);
                                    if (!binding.Type.IsEmpty)
                                    {
                                        return binding.Type.Name;
                                    }
                                }
                            }
                        }
                    }
                    return null;
                }

                static WsdlNS.Service GetAnyService(WsdlNS.ServiceDescriptionCollection wsdls)
                {
                    // try to track down a WSDL service
                    foreach (WsdlNS.ServiceDescription wsdl in wsdls)
                    {
                        if (wsdl.Services.Count > 0)
                        {
                            return wsdl.Services[0];
                        }
                    }
                    return null;
                }

                static string GetAnyWsdlName(WsdlNS.ServiceDescriptionCollection wsdls)
                {
                    // try to track down a WSDL name
                    foreach (WsdlNS.ServiceDescription wsdl in wsdls)
                    {
                        if (!String.IsNullOrEmpty(wsdl.Name))
                        {
                            return wsdl.Name;
                        }
                    }
                    return null;
                }
            }

            #region static helpers
            static void AddHttpProperty(Message message, HttpStatusCode status, string contentType)
            {
                HttpResponseMessageProperty responseProperty = new HttpResponseMessageProperty();
                responseProperty.StatusCode = status;
                responseProperty.Headers.Add(HttpResponseHeader.ContentType, contentType);
                message.Properties.Add(HttpResponseMessageProperty.Name, responseProperty);
            }

            static Message CreateRedirectMessage(string redirectedDestination)
            {
                Message redirectMessage = CreateHttpResponseMessage(HttpStatusCode.RedirectKeepVerb);
                HttpResponseMessageProperty httpResponseProperty = (HttpResponseMessageProperty)redirectMessage.Properties[HttpResponseMessageProperty.Name];
                httpResponseProperty.Headers["Location"] = redirectedDestination;
                return redirectMessage;
            }

            static Message CreateHttpResponseMessage(HttpStatusCode code)
            {
                Message message = new NullMessage();
                HttpResponseMessageProperty httpResponseProperty = new HttpResponseMessageProperty();
                httpResponseProperty.StatusCode = code;
                message.Properties.Add(HttpResponseMessageProperty.Name, httpResponseProperty);
                return message;
            }

            #endregion static helpers

            #region Helper Message implementations
            class DiscoMessage : ContentOnlyMessage
            {
                string wsdlAddress;
                string docAddress;

                public DiscoMessage(string wsdlAddress, string docAddress)
                    : base()
                {
                    this.wsdlAddress = wsdlAddress;
                    this.docAddress = docAddress;
                }

                protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("discovery", "http://schemas.xmlsoap.org/disco/");
                    writer.WriteStartElement("contractRef", "http://schemas.xmlsoap.org/disco/scl/");
                    writer.WriteAttributeString("ref", wsdlAddress);
                    writer.WriteAttributeString("docRef", docAddress);
                    writer.WriteEndElement(); // </contractRef>
                    writer.WriteEndElement(); // </discovery>
                    writer.WriteEndDocument();
                }
            }

            class MetadataOnHelpPageMessage : ContentOnlyMessage
            {
                string discoUrl;
                string metadataUrl;
                string singleWsdlUrl;
                string serviceName;
                string clientName;
                bool linkMetadata;

                string errorMessage;
                ExceptionDetail exceptionDetail;

                public MetadataOnHelpPageMessage(string discoUrl, string metadataUrl, string singleWsdlUrl, string serviceName, string clientName, bool linkMetadata)
                    : base()
                {
                    this.discoUrl = discoUrl;
                    this.metadataUrl = metadataUrl;
                    this.singleWsdlUrl = singleWsdlUrl;
                    this.serviceName = serviceName;
                    this.clientName = clientName;
                    this.linkMetadata = linkMetadata;
                }

                public MetadataOnHelpPageMessage(string errorMessage, ExceptionDetail exceptionDetail)
                    : base()
                {
                    this.errorMessage = errorMessage;
                    this.exceptionDetail = exceptionDetail;
                }


                protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
                {
                    HelpPageWriter page = new HelpPageWriter(writer);

                    writer.WriteStartElement("HTML");
                    writer.WriteStartElement("HEAD");

                    if (!String.IsNullOrEmpty(this.discoUrl))
                    {
                        page.WriteDiscoLink(this.discoUrl);
                    }

                    page.WriteStyleSheet();

                    page.WriteTitle(!String.IsNullOrEmpty(this.serviceName) ? SR.GetString(SR.SFxDocExt_MainPageTitle, this.serviceName) : SR.GetString(SR.SFxDocExt_MainPageTitleNoServiceName));

                    if (!String.IsNullOrEmpty(this.errorMessage))
                    {
                        page.WriteError(this.errorMessage);

                        if (this.exceptionDetail != null)
                        {
                            page.WriteExceptionDetail(this.exceptionDetail);
                        }
                    }
                    else
                    {
                        page.WriteToolUsage(this.metadataUrl, this.singleWsdlUrl, this.linkMetadata);
                        page.WriteSampleCode(this.clientName);
                    }

                    writer.WriteEndElement(); // BODY
                    writer.WriteEndElement(); // HTML
                }

                struct HelpPageWriter
                {
                    XmlWriter writer;
                    public HelpPageWriter(XmlWriter writer)
                    {
                        this.writer = writer;
                    }

                    internal void WriteClass(string className)
                    {
                        writer.WriteStartElement("font");
                        writer.WriteAttributeString("color", "teal");
                        writer.WriteString(className);
                        writer.WriteEndElement(); // font
                    }

                    internal void WriteComment(string comment)
                    {
                        writer.WriteStartElement("font");
                        writer.WriteAttributeString("color", "green");
                        writer.WriteString(comment);
                        writer.WriteEndElement(); // font
                    }

                    internal void WriteDiscoLink(string discoUrl)
                    {
                        writer.WriteStartElement("link");
                        writer.WriteAttributeString("rel", "alternate");
                        writer.WriteAttributeString("type", "text/xml");
                        writer.WriteAttributeString("href", discoUrl);
                        writer.WriteEndElement(); // link
                    }

                    internal void WriteError(string message)
                    {
                        writer.WriteStartElement("P");
                        writer.WriteAttributeString("class", "intro");
                        writer.WriteString(message);
                        writer.WriteEndElement(); // P

                    }

                    internal void WriteKeyword(string keyword)
                    {
                        writer.WriteStartElement("font");
                        writer.WriteAttributeString("color", "blue");
                        writer.WriteString(keyword);
                        writer.WriteEndElement(); // font
                    }

                    internal void WriteSampleCode(string clientName)
                    {
                        writer.WriteStartElement("P");
                        writer.WriteAttributeString("class", "intro");
                        writer.WriteEndElement(); // P

                        writer.WriteRaw(SR.GetString(SR.SFxDocExt_MainPageIntro2));


                        // C#
                        writer.WriteRaw(SR.GetString(SR.SFxDocExt_CS));
                        writer.WriteStartElement("PRE");
                        WriteKeyword("class ");
                        WriteClass("Test\n");
                        writer.WriteString("{\n");
                        WriteKeyword("    static void ");
                        writer.WriteString("Main()\n");
                        writer.WriteString("    {\n");
                        writer.WriteString("        ");
                        WriteClass(clientName);
                        writer.WriteString(" client = ");
                        WriteKeyword("new ");
                        WriteClass(clientName);
                        writer.WriteString("();\n\n");
                        WriteComment("        // " + SR.GetString(SR.SFxDocExt_MainPageComment) + "\n\n");
                        WriteComment("        // " + SR.GetString(SR.SFxDocExt_MainPageComment2) + "\n");
                        writer.WriteString("        client.Close();\n");
                        writer.WriteString("    }\n");
                        writer.WriteString("}\n");
                        writer.WriteEndElement(); // PRE
                        writer.WriteRaw(HttpGetImpl.HtmlBreak);


                        // VB
                        writer.WriteRaw(SR.GetString(SR.SFxDocExt_VB));
                        writer.WriteStartElement("PRE");
                        WriteKeyword("Class ");
                        WriteClass("Test\n");
                        WriteKeyword("    Shared Sub ");
                        writer.WriteString("Main()\n");
                        WriteKeyword("        Dim ");
                        writer.WriteString("client As ");
                        WriteClass(clientName);
                        writer.WriteString(" = ");
                        WriteKeyword("New ");
                        WriteClass(clientName);
                        writer.WriteString("()\n");
                        WriteComment("        ' " + SR.GetString(SR.SFxDocExt_MainPageComment) + "\n\n");
                        WriteComment("        ' " + SR.GetString(SR.SFxDocExt_MainPageComment2) + "\n");
                        writer.WriteString("        client.Close()\n");
                        WriteKeyword("    End Sub\n");
                        WriteKeyword("End Class");
                        writer.WriteEndElement(); // PRE
                    }

                    internal void WriteExceptionDetail(ExceptionDetail exceptionDetail)
                    {
                        writer.WriteStartElement("PRE");
                        writer.WriteString(exceptionDetail.ToString().Replace("\r", ""));
                        writer.WriteEndElement(); // PRE
                    }

                    internal void WriteStyleSheet()
                    {
                        writer.WriteStartElement("STYLE");
                        writer.WriteAttributeString("type", "text/css");
                        writer.WriteString("#content{ FONT-SIZE: 0.7em; PADDING-BOTTOM: 2em; MARGIN-LEFT: 30px}");
                        writer.WriteString("BODY{MARGIN-TOP: 0px; MARGIN-LEFT: 0px; COLOR: #000000; FONT-FAMILY: Verdana; BACKGROUND-COLOR: white}");
                        writer.WriteString("P{MARGIN-TOP: 0px; MARGIN-BOTTOM: 12px; COLOR: #000000; FONT-FAMILY: Verdana}");
                        writer.WriteString("PRE{BORDER-RIGHT: #f0f0e0 1px solid; PADDING-RIGHT: 5px; BORDER-TOP: #f0f0e0 1px solid; MARGIN-TOP: -5px; PADDING-LEFT: 5px; FONT-SIZE: 1.2em; PADDING-BOTTOM: 5px; BORDER-LEFT: #f0f0e0 1px solid; PADDING-TOP: 5px; BORDER-BOTTOM: #f0f0e0 1px solid; FONT-FAMILY: Courier New; BACKGROUND-COLOR: #e5e5cc}");
                        writer.WriteString(".heading1{MARGIN-TOP: 0px; PADDING-LEFT: 15px; FONT-WEIGHT: normal; FONT-SIZE: 26px; MARGIN-BOTTOM: 0px; PADDING-BOTTOM: 3px; MARGIN-LEFT: -30px; WIDTH: 100%; COLOR: #ffffff; PADDING-TOP: 10px; FONT-FAMILY: Tahoma; BACKGROUND-COLOR: #003366}");
                        writer.WriteString(".intro{MARGIN-LEFT: -15px}");
                        writer.WriteEndElement(); // STYLE
                    }

                    internal void WriteTitle(string title)
                    {
                        writer.WriteElementString("TITLE", title);
                        writer.WriteEndElement(); // HEAD
                        writer.WriteStartElement("BODY");
                        writer.WriteStartElement("DIV");
                        writer.WriteAttributeString("id", "content");
                        writer.WriteStartElement("P");
                        writer.WriteAttributeString("class", "heading1");
                        writer.WriteString(title);
                        writer.WriteEndElement(); // P
                        writer.WriteRaw(HttpGetImpl.HtmlBreak);

                    }

                    internal void WriteToolUsage(string wsdlUrl, string singleWsdlUrl, bool linkMetadata)
                    {
                        writer.WriteStartElement("P");
                        writer.WriteAttributeString("class", "intro");

                        if (wsdlUrl != null)
                        {
                            WriteMetadataAddress(SR.SFxDocExt_MainPageIntro1a, "svcutil.exe ", wsdlUrl, linkMetadata);
                            if (singleWsdlUrl != null)
                            {
                                // ?singleWsdl message
                                writer.WriteStartElement("P");
                                WriteMetadataAddress(SR.SFxDocExt_MainPageIntroSingleWsdl, null, singleWsdlUrl, linkMetadata);
                                writer.WriteEndElement();
                            }
                        }
                        else
                        {
                            // no metadata message
                            writer.WriteRaw(SR.GetString(SR.SFxDocExt_MainPageIntro1b));
                        }
                        writer.WriteEndElement(); // P
                    }

                    void WriteMetadataAddress(string introductionText, string clientToolName, string wsdlUrl, bool linkMetadata)
                    {
                        writer.WriteRaw(SR.GetString(introductionText));
                        writer.WriteRaw(HttpGetImpl.HtmlBreak);
                        writer.WriteStartElement("PRE");
                        if (!string.IsNullOrEmpty(clientToolName))
                        {
                            writer.WriteString(clientToolName);
                        }

                        if (linkMetadata)
                        {
                            writer.WriteStartElement("A");
                            writer.WriteAttributeString("HREF", wsdlUrl);
                        }

                        writer.WriteString(wsdlUrl);

                        if (linkMetadata)
                        {
                            writer.WriteEndElement(); // A
                        }

                        writer.WriteEndElement(); // PRE
                    }
                }
            }

            class MetadataOffHelpPageMessage : ContentOnlyMessage
            {

                public MetadataOffHelpPageMessage(string serviceName)
                {

                }

                protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
                {
                    writer.WriteStartElement("HTML");
                    writer.WriteStartElement("HEAD");
                    writer.WriteRaw(String.Format(CultureInfo.InvariantCulture,
                        @"<STYLE type=""text/css"">#content{{ FONT-SIZE: 0.7em; PADDING-BOTTOM: 2em; MARGIN-LEFT: 30px}}BODY{{MARGIN-TOP: 0px; MARGIN-LEFT: 0px; COLOR: #000000; FONT-FAMILY: Verdana; BACKGROUND-COLOR: white}}P{{MARGIN-TOP: 0px; MARGIN-BOTTOM: 12px; COLOR: #000000; FONT-FAMILY: Verdana}}PRE{{BORDER-RIGHT: #f0f0e0 1px solid; PADDING-RIGHT: 5px; BORDER-TOP: #f0f0e0 1px solid; MARGIN-TOP: -5px; PADDING-LEFT: 5px; FONT-SIZE: 1.2em; PADDING-BOTTOM: 5px; BORDER-LEFT: #f0f0e0 1px solid; PADDING-TOP: 5px; BORDER-BOTTOM: #f0f0e0 1px solid; FONT-FAMILY: Courier New; BACKGROUND-COLOR: #e5e5cc}}.heading1{{MARGIN-TOP: 0px; PADDING-LEFT: 15px; FONT-WEIGHT: normal; FONT-SIZE: 26px; MARGIN-BOTTOM: 0px; PADDING-BOTTOM: 3px; MARGIN-LEFT: -30px; WIDTH: 100%; COLOR: #ffffff; PADDING-TOP: 10px; FONT-FAMILY: Tahoma; BACKGROUND-COLOR: #003366}}.intro{{MARGIN-LEFT: -15px}}</STYLE>
<TITLE>Service</TITLE>"));
                    writer.WriteEndElement(); //HEAD

                    writer.WriteRaw(String.Format(CultureInfo.InvariantCulture,
                                            @"<BODY>
<DIV id=""content"">
<P class=""heading1"">Service</P>
<BR/>
<P class=""intro"">{0}</P>
<PRE>
<font color=""blue"">&lt;<font color=""darkred"">" + ConfigurationStrings.BehaviorsSectionName + @"</font>&gt;</font>
<font color=""blue"">    &lt;<font color=""darkred"">" + ConfigurationStrings.ServiceBehaviors + @"</font>&gt;</font>
<font color=""blue"">        &lt;<font color=""darkred"">" + ConfigurationStrings.Behavior + @" </font><font color=""red"">" + ConfigurationStrings.Name + @"</font>=<font color=""black"">""</font>MyServiceTypeBehaviors<font color=""black"">"" </font>&gt;</font>
<font color=""blue"">            &lt;<font color=""darkred"">" + ConfigurationStrings.ServiceMetadataPublishingSectionName + @" </font><font color=""red"">" + ConfigurationStrings.HttpGetEnabled + @"</font>=<font color=""black"">""</font>true<font color=""black"">"" </font>/&gt;</font>
<font color=""blue"">        &lt;<font color=""darkred"">/" + ConfigurationStrings.Behavior + @"</font>&gt;</font>
<font color=""blue"">    &lt;<font color=""darkred"">/" + ConfigurationStrings.ServiceBehaviors + @"</font>&gt;</font>
<font color=""blue"">&lt;<font color=""darkred"">/" + ConfigurationStrings.BehaviorsSectionName + @"</font>&gt;</font>
</PRE>
<P class=""intro"">{1}</P>
<PRE>
<font color=""blue"">&lt;<font color=""darkred"">" + ConfigurationStrings.Service + @" </font><font color=""red"">" + ConfigurationStrings.Name + @"</font>=<font color=""black"">""</font><i>MyNamespace.MyServiceType</i><font color=""black"">"" </font><font color=""red"">" + ConfigurationStrings.BehaviorConfiguration + @"</font>=<font color=""black"">""</font><i>MyServiceTypeBehaviors</i><font color=""black"">"" </font>&gt;</font>
</PRE>
<P class=""intro"">{2}</P>
<PRE>
<font color=""blue"">&lt;<font color=""darkred"">" + ConfigurationStrings.Endpoint + @" </font><font color=""red"">" + ConfigurationStrings.Contract + @"</font>=<font color=""black"">""</font>" + ServiceMetadataBehavior.MexContractName + @"<font color=""black"">"" </font><font color=""red"">" + ConfigurationStrings.Binding + @"</font>=<font color=""black"">""</font>mexHttpBinding<font color=""black"">"" </font><font color=""red"">" + ConfigurationStrings.Address + @"</font>=<font color=""black"">""</font>mex<font color=""black"">"" </font>/&gt;</font>
</PRE>

<P class=""intro"">{3}</P>
<PRE>
<font color=""blue"">&lt;<font color=""darkred"">configuration</font>&gt;</font>
<font color=""blue"">    &lt;<font color=""darkred"">" + ConfigurationStrings.SectionGroupName + @"</font>&gt;</font>
 
<font color=""blue"">        &lt;<font color=""darkred"">" + ConfigurationStrings.ServicesSectionName + @"</font>&gt;</font>
<font color=""blue"">            &lt;!-- <font color=""green"">{4}</font> --&gt;</font>
<font color=""blue"">            &lt;<font color=""darkred"">" + ConfigurationStrings.Service + @" </font><font color=""red"">" + ConfigurationStrings.Name + @"</font>=<font color=""black"">""</font><i>MyNamespace.MyServiceType</i><font color=""black"">"" </font><font color=""red"">" + ConfigurationStrings.BehaviorConfiguration + @"</font>=<font color=""black"">""</font><i>MyServiceTypeBehaviors</i><font color=""black"">"" </font>&gt;</font>
<font color=""blue"">                &lt;!-- <font color=""green"">{5}</font> --&gt;</font>
<font color=""blue"">                &lt;!-- <font color=""green"">{6}</font> --&gt;</font>
<font color=""blue"">                &lt;<font color=""darkred"">" + ConfigurationStrings.Endpoint + @" </font><font color=""red"">" + ConfigurationStrings.Contract + @"</font>=<font color=""black"">""</font>" + ServiceMetadataBehavior.MexContractName + @"<font color=""black"">"" </font><font color=""red"">" + ConfigurationStrings.Binding + @"</font>=<font color=""black"">""</font>mexHttpBinding<font color=""black"">"" </font><font color=""red"">" + ConfigurationStrings.Address + @"</font>=<font color=""black"">""</font>mex<font color=""black"">"" </font>/&gt;</font>
<font color=""blue"">            &lt;<font color=""darkred"">/" + ConfigurationStrings.Service + @"</font>&gt;</font>
<font color=""blue"">        &lt;<font color=""darkred"">/" + ConfigurationStrings.ServicesSectionName + @"</font>&gt;</font>
 
<font color=""blue"">        &lt;<font color=""darkred"">" + ConfigurationStrings.BehaviorsSectionName + @"</font>&gt;</font>
<font color=""blue"">            &lt;<font color=""darkred"">" + ConfigurationStrings.ServiceBehaviors + @"</font>&gt;</font>
<font color=""blue"">                &lt;<font color=""darkred"">" + ConfigurationStrings.Behavior + @" </font><font color=""red"">name</font>=<font color=""black"">""</font><i>MyServiceTypeBehaviors</i><font color=""black"">"" </font>&gt;</font>
<font color=""blue"">                    &lt;!-- <font color=""green"">{7}</font> --&gt;</font>
<font color=""blue"">                    &lt;<font color=""darkred"">" + ConfigurationStrings.ServiceMetadataPublishingSectionName + @" </font><font color=""red"">" + ConfigurationStrings.HttpGetEnabled + @"</font>=<font color=""black"">""</font>true<font color=""black"">"" </font>/&gt;</font>
<font color=""blue"">                &lt;<font color=""darkred"">/" + ConfigurationStrings.Behavior + @"</font>&gt;</font>
<font color=""blue"">            &lt;<font color=""darkred"">/" + ConfigurationStrings.ServiceBehaviors + @"</font>&gt;</font>
<font color=""blue"">        &lt;<font color=""darkred"">/" + ConfigurationStrings.BehaviorsSectionName + @"</font>&gt;</font>
 
<font color=""blue"">    &lt;<font color=""darkred"">/" + ConfigurationStrings.SectionGroupName + @"</font>&gt;</font>
<font color=""blue"">&lt;<font color=""darkred"">/configuration</font>&gt;</font>
</PRE>
<P class=""intro"">{8}</P>
</DIV>
</BODY>",
        SR.GetString(SR.SFxDocExt_NoMetadataSection1), SR.GetString(SR.SFxDocExt_NoMetadataSection2),
        SR.GetString(SR.SFxDocExt_NoMetadataSection3), SR.GetString(SR.SFxDocExt_NoMetadataSection4),
        SR.GetString(SR.SFxDocExt_NoMetadataConfigComment1), SR.GetString(SR.SFxDocExt_NoMetadataConfigComment2),
        SR.GetString(SR.SFxDocExt_NoMetadataConfigComment3), SR.GetString(SR.SFxDocExt_NoMetadataConfigComment4),
        SR.GetString(SR.SFxDocExt_NoMetadataSection5)
        ));

                    writer.WriteEndElement(); //HTML
                }
            }

            class ServiceDescriptionMessage : ContentOnlyMessage
            {
                WsdlNS.ServiceDescription description;
                WriteFilter responseWriter;

                public ServiceDescriptionMessage(WsdlNS.ServiceDescription description, WriteFilter responseWriter)
                    : base()
                {
                    this.description = description;
                    this.responseWriter = responseWriter;
                }

                protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
                {
                    this.responseWriter.Writer = writer;
                    description.Write(this.responseWriter);
                }
            }

            class XmlSchemaMessage : ContentOnlyMessage
            {
                XmlSchema schema;
                WriteFilter responseWriter;

                public XmlSchemaMessage(XmlSchema schema, WriteFilter responseWriter)
                    : base()
                {
                    this.schema = schema;
                    this.responseWriter = responseWriter;
                }

                protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
                {
                    this.responseWriter.Writer = writer;
                    schema.Write(responseWriter);
                }
            }
            #endregion //Helper Message implementations
        }

        internal abstract class WriteFilter : XmlDictionaryWriter
        {
            internal XmlWriter Writer;
            public abstract WriteFilter CloneWriteFilter();
            public override void Close()
            {
                this.Writer.Close();
            }

            public override void Flush()
            {
                this.Writer.Flush();
            }

            public override string LookupPrefix(string ns)
            {
                return this.Writer.LookupPrefix(ns);
            }

            public override void WriteBase64(byte[] buffer, int index, int count)
            {
                this.Writer.WriteBase64(buffer, index, count);
            }

            public override void WriteCData(string text)
            {
                this.Writer.WriteCData(text);
            }

            public override void WriteCharEntity(char ch)
            {
                this.Writer.WriteCharEntity(ch);
            }

            public override void WriteChars(char[] buffer, int index, int count)
            {
                this.Writer.WriteChars(buffer, index, count);
            }

            public override void WriteComment(string text)
            {
                this.Writer.WriteComment(text);
            }

            public override void WriteDocType(string name, string pubid, string sysid, string subset)
            {
                this.Writer.WriteDocType(name, pubid, sysid, subset);
            }

            public override void WriteEndAttribute()
            {
                this.Writer.WriteEndAttribute();
            }

            public override void WriteEndDocument()
            {
                this.Writer.WriteEndDocument();
            }

            public override void WriteEndElement()
            {
                this.Writer.WriteEndElement();
            }

            public override void WriteEntityRef(string name)
            {
                this.Writer.WriteEntityRef(name);
            }

            public override void WriteFullEndElement()
            {
                this.Writer.WriteFullEndElement();
            }

            public override void WriteProcessingInstruction(string name, string text)
            {
                this.Writer.WriteProcessingInstruction(name, text);
            }

            public override void WriteRaw(string data)
            {
                this.Writer.WriteRaw(data);
            }

            public override void WriteRaw(char[] buffer, int index, int count)
            {
                this.Writer.WriteRaw(buffer, index, count);
            }

            public override void WriteStartAttribute(string prefix, string localName, string ns)
            {
                this.Writer.WriteStartAttribute(prefix, localName, ns);
            }

            public override void WriteStartDocument(bool standalone)
            {
                this.Writer.WriteStartDocument(standalone);
            }

            public override void WriteStartDocument()
            {
                this.Writer.WriteStartDocument();
            }

            public override void WriteStartElement(string prefix, string localName, string ns)
            {
                this.Writer.WriteStartElement(prefix, localName, ns);
            }

            public override WriteState WriteState
            {
                get { return this.Writer.WriteState; }
            }

            public override void WriteString(string text)
            {
                this.Writer.WriteString(text);
            }

            public override void WriteSurrogateCharEntity(char lowChar, char highChar)
            {
                this.Writer.WriteSurrogateCharEntity(lowChar, highChar);
            }

            public override void WriteWhitespace(string ws)
            {
                this.Writer.WriteWhitespace(ws);
            }
        }

        class LocationUpdatingWriter : WriteFilter
        {
            readonly string oldValue;
            readonly string newValue;

            // passing null for newValue filters any string with oldValue as a prefix rather than replacing
            internal LocationUpdatingWriter(string oldValue, string newValue)
            {
                this.oldValue = oldValue;

                this.newValue = newValue;
            }

            public override WriteFilter CloneWriteFilter()
            {
                return new LocationUpdatingWriter(oldValue, newValue);
            }

            public override void WriteString(string text)
            {
                if (this.newValue != null)
                    text = text.Replace(this.oldValue, this.newValue);
                else if (text.StartsWith(this.oldValue, StringComparison.Ordinal))
                    text = String.Empty;

                base.WriteString(text);
            }
        }

        class DynamicAddressUpdateWriter : WriteFilter
        {
            readonly string oldHostName;
            readonly string newHostName;
            readonly string newBaseAddress;
            readonly bool removeBaseAddress;
            readonly string requestScheme;
            readonly int requestPort;
            readonly IDictionary<string, int> updatePortsByScheme;

            internal DynamicAddressUpdateWriter(Uri listenUri, string requestHost, int requestPort,
                IDictionary<string, int> updatePortsByScheme, bool removeBaseAddress)
                : this(listenUri.Host, requestHost, removeBaseAddress, listenUri.Scheme, requestPort, updatePortsByScheme)
            {
                this.newBaseAddress = UpdateUri(listenUri).ToString();
            }

            DynamicAddressUpdateWriter(string oldHostName, string newHostName, string newBaseAddress, bool removeBaseAddress, string requestScheme,
                int requestPort, IDictionary<string, int> updatePortsByScheme)
                : this(oldHostName, newHostName, removeBaseAddress, requestScheme, requestPort, updatePortsByScheme)
            {
                this.newBaseAddress = newBaseAddress;
            }

            DynamicAddressUpdateWriter(string oldHostName, string newHostName, bool removeBaseAddress, string requestScheme,
                int requestPort, IDictionary<string, int> updatePortsByScheme)
            {
                this.oldHostName = oldHostName;
                this.newHostName = newHostName;
                this.removeBaseAddress = removeBaseAddress;
                this.requestScheme = requestScheme;
                this.requestPort = requestPort;
                this.updatePortsByScheme = updatePortsByScheme;
            }

            public override WriteFilter CloneWriteFilter()
            {
                return new DynamicAddressUpdateWriter(this.oldHostName, this.newHostName, this.newBaseAddress, this.removeBaseAddress,
                    this.requestScheme, this.requestPort, this.updatePortsByScheme);
            }

            public override void WriteString(string text)
            {
                Uri uri;
                if (this.removeBaseAddress &&
                    text.StartsWith(ServiceMetadataExtension.BaseAddressPattern, StringComparison.Ordinal))
                {
                    text = string.Empty;
                }
                else if (!this.removeBaseAddress &&
                    text.Contains(ServiceMetadataExtension.BaseAddressPattern))
                {
                    text = text.Replace(ServiceMetadataExtension.BaseAddressPattern, this.newBaseAddress);
                }
                else if (Uri.TryCreate(text, UriKind.Absolute, out uri))
                {
                    Uri newUri = UpdateUri(uri);
                    if (newUri != null)
                    {
                        text = newUri.ToString();
                    }
                }
                base.WriteString(text);
            }

            public void UpdateUri(ref Uri uri, bool updateBaseAddressOnly = false)
            {
                Uri newUri = UpdateUri(uri, updateBaseAddressOnly);
                if (newUri != null)
                {
                    uri = newUri;
                }
            }

            Uri UpdateUri(Uri uri, bool updateBaseAddressOnly = false)
            {
                // Ordinal comparison okay: we're filtering for auto-generated URIs which will
                // always be based off the listenURI, so always match in case
                if (uri.Host != oldHostName)
                {
                    return null;
                }

                UriBuilder result = new UriBuilder(uri);
                result.Host = this.newHostName;

                if (!updateBaseAddressOnly)
                {
                    int port;
                    if (uri.Scheme == this.requestScheme)
                    {
                        port = requestPort;
                    }
                    else if (!this.updatePortsByScheme.TryGetValue(uri.Scheme, out port))
                    {
                        return null;
                    }
                    result.Port = port;
                }

                return result.Uri;
            }
        }
    }
}
