//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Mime;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Diagnostics;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using WsdlNS = System.Web.Services.Description;
    using XsdNS = System.Xml.Schema;

    public class MetadataExchangeClient
    {
        ChannelFactory<IMetadataExchange> factory;
        ICredentials webRequestCredentials;

        TimeSpan resolveTimeout = TimeSpan.FromMinutes(1);
        int maximumResolvedReferences = 10;
        bool resolveMetadataReferences = true;
        long maxMessageSize;
        XmlDictionaryReaderQuotas readerQuotas;

        EndpointAddress ctorEndpointAddress = null;
        Uri ctorUri = null;

        object thisLock = new object();

        internal const string MetadataExchangeClientKey = "MetadataExchangeClientKey";

        public MetadataExchangeClient()
        {
            this.factory = new ChannelFactory<IMetadataExchange>("*");
            this.maxMessageSize = GetMaxMessageSize(this.factory.Endpoint.Binding);
        }
        public MetadataExchangeClient(Uri address, MetadataExchangeClientMode mode)
        {
            Validate(address, mode);

            if (mode == MetadataExchangeClientMode.HttpGet)
            {
                this.ctorUri = address;
            }
            else
            {
                this.ctorEndpointAddress = new EndpointAddress(address);
            }

            CreateChannelFactory(address.Scheme);
        }
        public MetadataExchangeClient(EndpointAddress address)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }

            this.ctorEndpointAddress = address;

            CreateChannelFactory(address.Uri.Scheme);
        }
        public MetadataExchangeClient(string endpointConfigurationName)
        {
            if (endpointConfigurationName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
            }
            this.factory = new ChannelFactory<IMetadataExchange>(endpointConfigurationName);
            this.maxMessageSize = GetMaxMessageSize(this.factory.Endpoint.Binding);
        }
        public MetadataExchangeClient(Binding mexBinding)
        {
            if (mexBinding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("mexBinding");
            }
            this.factory = new ChannelFactory<IMetadataExchange>(mexBinding);
            this.maxMessageSize = GetMaxMessageSize(this.factory.Endpoint.Binding);
        }

        //Configuration for credentials
        public ClientCredentials SoapCredentials
        {
            get { return this.factory.Credentials; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.factory.Endpoint.Behaviors.RemoveAll<ClientCredentials>();
                this.factory.Endpoint.Behaviors.Add(value);
            }
        }
        public ICredentials HttpCredentials
        {
            get { return this.webRequestCredentials; }
            set { this.webRequestCredentials = value; }

        }

        // Configuration options for the entire MetadataResolver
        public TimeSpan OperationTimeout
        {
            get { return this.resolveTimeout; }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRange0)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                this.resolveTimeout = value;
            }
        }
        public int MaximumResolvedReferences
        {
            get { return this.maximumResolvedReferences; }
            set
            {
                if (value < 1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", SR.GetString(SR.SFxMaximumResolvedReferencesOutOfRange, value)));
                }
                this.maximumResolvedReferences = value;
            }

        }
        public bool ResolveMetadataReferences
        {
            get { return this.resolveMetadataReferences; }
            set { this.resolveMetadataReferences = value; }
        }

        internal object ThisLock
        {
            get { return this.thisLock; }
        }

        internal long MaxMessageSize
        {
            get { return this.maxMessageSize; }
            set { this.maxMessageSize = value; }
        }

        internal XmlDictionaryReaderQuotas ReaderQuotas
        {
            get
            {
                if (this.readerQuotas == null)
                {
                    if (this.factory != null)
                    {
                        BindingElementCollection bindingElementCollection = this.factory.Endpoint.Binding.CreateBindingElements();
                        if (bindingElementCollection != null)
                        {
                            MessageEncodingBindingElement bindingElement = bindingElementCollection.Find<MessageEncodingBindingElement>();
                            if (bindingElement != null)
                            {
                                this.readerQuotas = bindingElement.GetIndividualProperty<XmlDictionaryReaderQuotas>();
                            }
                        }
                    }
                    this.readerQuotas = this.readerQuotas ?? EncoderDefaults.ReaderQuotas;
                }
                return this.readerQuotas;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Uses ClientSection.UnsafeGetSection to get config in PT.",
            Safe = "Does not leak config object, just calculates a bool.")]
        [SecuritySafeCritical]
        bool ClientEndpointExists(string name)
        {
            ClientSection clientSection = ClientSection.UnsafeGetSection();

            if (clientSection == null)
                return false;

            foreach (ChannelEndpointElement endpoint in clientSection.Endpoints)
            {
                if (endpoint.Name == name && endpoint.Contract == ServiceMetadataBehavior.MexContractName)
                    return true;
            }

            return false;
        }
        bool IsHttpOrHttps(Uri address)
        {
            return address.Scheme == Uri.UriSchemeHttp || address.Scheme == Uri.UriSchemeHttps;
        }
        void CreateChannelFactory(string scheme)
        {
            if (ClientEndpointExists(scheme))
            {
                this.factory = new ChannelFactory<IMetadataExchange>(scheme);
            }
            else
            {
                Binding mexBinding = null;
                if (MetadataExchangeBindings.TryGetBindingForScheme(scheme, out mexBinding))
                {
                    this.factory = new ChannelFactory<IMetadataExchange>(mexBinding);
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("scheme", SR.GetString(SR.SFxMetadataExchangeClientCouldNotCreateChannelFactoryBadScheme, scheme));
                }
            }
            this.maxMessageSize = GetMaxMessageSize(this.factory.Endpoint.Binding);
        }
        void Validate(Uri address, MetadataExchangeClientMode mode)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }

            if (!address.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("address", SR.GetString(SR.SFxCannotGetMetadataFromRelativeAddress, address));
            }

            if (mode == MetadataExchangeClientMode.HttpGet && !IsHttpOrHttps(address))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("address", SR.GetString(SR.SFxCannotHttpGetMetadataFromAddress, address));
            }

            MetadataExchangeClientModeHelper.Validate(mode);
        }

        public IAsyncResult BeginGetMetadata(AsyncCallback callback, object asyncState)
        {
            if (ctorUri != null)
                return BeginGetMetadata(ctorUri, MetadataExchangeClientMode.HttpGet, callback, asyncState);
            if (ctorEndpointAddress != null)
                return BeginGetMetadata(ctorEndpointAddress, callback, asyncState);
            else
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxMetadataExchangeClientNoMetadataAddress)));
        }
        public IAsyncResult BeginGetMetadata(Uri address, MetadataExchangeClientMode mode, AsyncCallback callback, object asyncState)
        {
            Validate(address, mode);

            if (mode == MetadataExchangeClientMode.HttpGet)
            {
                return this.BeginGetMetadata(new MetadataLocationRetriever(address, this), callback, asyncState);
            }
            else
            {
                return this.BeginGetMetadata(new MetadataReferenceRetriever(new EndpointAddress(address), this), callback, asyncState);
            }
        }
        public IAsyncResult BeginGetMetadata(EndpointAddress address, AsyncCallback callback, object asyncState)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            return this.BeginGetMetadata(new MetadataReferenceRetriever(address, this), callback, asyncState);
        }
        IAsyncResult BeginGetMetadata(MetadataRetriever retriever, AsyncCallback callback, object asyncState)
        {
            ResolveCallState state = new ResolveCallState(this.maximumResolvedReferences, this.resolveMetadataReferences, new TimeoutHelper(this.OperationTimeout), this);
            state.StackedRetrievers.Push(retriever);
            return new AsyncMetadataResolver(state, callback, asyncState);
        }

        public MetadataSet EndGetMetadata(IAsyncResult result)
        {
            return AsyncMetadataResolver.End(result);
        }

        public Task<MetadataSet> GetMetadataAsync()
        {
            if (ctorUri != null)
                return GetMetadataAsync(ctorUri, MetadataExchangeClientMode.HttpGet);
            if (ctorEndpointAddress != null)
                return GetMetadataAsync(ctorEndpointAddress);
            else
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxMetadataExchangeClientNoMetadataAddress)));
        }

        public Task<MetadataSet> GetMetadataAsync(Uri address, MetadataExchangeClientMode mode)
        {
            Validate(address, mode);

            MetadataRetriever retriever = (mode == MetadataExchangeClientMode.HttpGet)
                                                ? (MetadataRetriever) new MetadataLocationRetriever(address, this)
                                                : (MetadataRetriever) new MetadataReferenceRetriever(new EndpointAddress(address), this);

            return Task.Factory.FromAsync<MetadataRetriever, MetadataSet>(this.BeginGetMetadata, this.EndGetMetadata, retriever, /* state */ null);
        }

        public Task<MetadataSet> GetMetadataAsync(EndpointAddress address)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }

            return Task.Factory.FromAsync<MetadataRetriever, MetadataSet>(this.BeginGetMetadata, this.EndGetMetadata, new MetadataReferenceRetriever(address, this), /* state */ null);
        }

        public Task<MetadataSet> GetMetadataAsync(EndpointAddress address, Uri via)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }

            if (via == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("via");
            }
                
            return Task.Factory.FromAsync<MetadataRetriever, MetadataSet>(this.BeginGetMetadata, this.EndGetMetadata, new MetadataReferenceRetriever(address, via, this), /* state */ null);
        }

        public MetadataSet GetMetadata()
        {
            if (ctorUri != null)
                return GetMetadata(ctorUri, MetadataExchangeClientMode.HttpGet);
            if (ctorEndpointAddress != null)
                return GetMetadata(ctorEndpointAddress);
            else
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxMetadataExchangeClientNoMetadataAddress)));
        }
        public MetadataSet GetMetadata(Uri address, MetadataExchangeClientMode mode)
        {
            Validate(address, mode);

            MetadataRetriever retriever;
            if (mode == MetadataExchangeClientMode.HttpGet)
            {
                retriever = new MetadataLocationRetriever(address, this);
            }
            else
            {
                retriever = new MetadataReferenceRetriever(new EndpointAddress(address), this);
            }
            return GetMetadata(retriever);
        }

        public MetadataSet GetMetadata(EndpointAddress address)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }

            MetadataReferenceRetriever retriever = new MetadataReferenceRetriever(address, this);
            return GetMetadata(retriever);
        }

        public MetadataSet GetMetadata(EndpointAddress address, Uri via)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }

            if (via == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("via");
            }

            MetadataReferenceRetriever retriever = new MetadataReferenceRetriever(address, via, this);
            return GetMetadata(retriever);
        }

        MetadataSet GetMetadata(MetadataRetriever retriever)
        {
            ResolveCallState resolveCallState = new ResolveCallState(this.maximumResolvedReferences, this.resolveMetadataReferences, new TimeoutHelper(this.OperationTimeout), this);
            resolveCallState.StackedRetrievers.Push(retriever);
            this.ResolveNext(resolveCallState);

            return resolveCallState.MetadataSet;
        }

        void ResolveNext(ResolveCallState resolveCallState)
        {
            if (resolveCallState.StackedRetrievers.Count > 0)
            {
                MetadataRetriever retriever = resolveCallState.StackedRetrievers.Pop();

                if (resolveCallState.HasBeenUsed(retriever))
                {
                    this.ResolveNext(resolveCallState);
                }
                else
                {
                    if (resolveCallState.ResolvedMaxResolvedReferences)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxResolvedMaxResolvedReferences)));
                    }

                    resolveCallState.LogUse(retriever);
                    resolveCallState.HandleSection(retriever.Retrieve(resolveCallState.TimeoutHelper));
                    this.ResolveNext(resolveCallState);
                }
            }
        }

        protected internal virtual ChannelFactory<IMetadataExchange> GetChannelFactory(EndpointAddress metadataAddress, string dialect, string identifier)
        {
            return this.factory;
        }

        static long GetMaxMessageSize(Binding mexBinding)
        {
            BindingElementCollection bindingElementCollection = mexBinding.CreateBindingElements();
            TransportBindingElement bindingElement = bindingElementCollection.Find<TransportBindingElement>();
            if (bindingElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxBindingDoesNotHaveATransportBindingElement)));
            }
            return bindingElement.MaxReceivedMessageSize;
        }

        protected internal virtual HttpWebRequest GetWebRequest(Uri location, string dialect, string identifier)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(location);
            request.Method = "GET";
            request.Credentials = this.HttpCredentials;

            return request;
        }

        internal static void TraceSendRequest(Uri address)
        {
            TraceSendRequest(TraceCode.MetadataExchangeClientSendRequest, SR.GetString(SR.TraceCodeMetadataExchangeClientSendRequest),
                address.ToString(), MetadataExchangeClientMode.HttpGet.ToString());
        }
        internal static void TraceSendRequest(EndpointAddress address)
        {
            TraceSendRequest(TraceCode.MetadataExchangeClientSendRequest, SR.GetString(SR.TraceCodeMetadataExchangeClientSendRequest),
                address.ToString(), MetadataExchangeClientMode.MetadataExchange.ToString());
        }
        static void TraceSendRequest(int traceCode, string traceDescription, string address, string mode)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                Hashtable h = new Hashtable(2)
                {
                    { "Address", address },
                    { "Mode", mode }
                };
                TraceUtility.TraceEvent(TraceEventType.Information, traceCode, traceDescription, new DictionaryTraceRecord(h), null, null);
            }
        }
        internal static void TraceReceiveReply(string sourceUrl, Type metadataType)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                Hashtable h = new Hashtable(2);
                h.Add("SourceUrl", sourceUrl);
                h.Add("MetadataType", metadataType.ToString());
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.MetadataExchangeClientReceiveReply, SR.GetString(SR.TraceCodeMetadataExchangeClientReceiveReply),
                    new DictionaryTraceRecord(h), null, null);
            }
        }

        class ResolveCallState
        {
            Dictionary<MetadataRetriever, MetadataRetriever> usedRetrievers;   // to prevent looping when chasing MetadataReferences
            MetadataSet metadataSet;
            int maxResolvedReferences;
            bool resolveMetadataReferences;
            Stack<MetadataRetriever> stackedRetrievers;
            MetadataExchangeClient resolver;
            TimeoutHelper timeoutHelper;

            internal ResolveCallState(int maxResolvedReferences, bool resolveMetadataReferences,
                TimeoutHelper timeoutHelper, MetadataExchangeClient resolver)
            {
                this.maxResolvedReferences = maxResolvedReferences;
                this.resolveMetadataReferences = resolveMetadataReferences;
                this.resolver = resolver;
                this.timeoutHelper = timeoutHelper;
                this.metadataSet = new MetadataSet();
                this.usedRetrievers = new Dictionary<MetadataRetriever, MetadataRetriever>();
                this.stackedRetrievers = new Stack<MetadataRetriever>();
            }

            internal MetadataSet MetadataSet
            {
                get { return this.metadataSet; }
            }

            internal Stack<MetadataRetriever> StackedRetrievers
            {
                get { return this.stackedRetrievers; }
            }

            internal bool ResolvedMaxResolvedReferences
            {
                get { return this.usedRetrievers.Count == this.maxResolvedReferences; }
            }

            internal TimeoutHelper TimeoutHelper
            {
                get { return this.timeoutHelper; }
            }

            internal void HandleSection(MetadataSection section)
            {
                if (section.Metadata is MetadataSet)
                {
                    foreach (MetadataSection innerSection in ((MetadataSet)section.Metadata).MetadataSections)
                    {
                        innerSection.SourceUrl = section.SourceUrl;
                        this.HandleSection(innerSection);
                    }
                }
                else if (section.Metadata is MetadataReference)
                {
                    if (this.resolveMetadataReferences)
                    {

                        EndpointAddress address = ((MetadataReference)section.Metadata).Address;
                        MetadataRetriever retriever = new MetadataReferenceRetriever(address, this.resolver, section.Dialect, section.Identifier);
                        this.stackedRetrievers.Push(retriever);
                    }
                    else
                    {
                        this.metadataSet.MetadataSections.Add(section);
                    }
                }
                else if (section.Metadata is MetadataLocation)
                {
                    if (this.resolveMetadataReferences)
                    {
                        string location = ((MetadataLocation)section.Metadata).Location;
                        MetadataRetriever retriever = new MetadataLocationRetriever(this.CreateUri(section.SourceUrl, location), this.resolver, section.Dialect, section.Identifier);
                        this.stackedRetrievers.Push(retriever);
                    }
                    else
                    {
                        this.metadataSet.MetadataSections.Add(section);
                    }
                }
                else if (section.Metadata is WsdlNS.ServiceDescription)
                {
                    if (this.resolveMetadataReferences)
                    {
                        this.HandleWsdlImports(section);
                    }
                    this.metadataSet.MetadataSections.Add(section);
                }
                else if (section.Metadata is XsdNS.XmlSchema)
                {
                    if (this.resolveMetadataReferences)
                    {
                        this.HandleSchemaImports(section);
                    }
                    this.metadataSet.MetadataSections.Add(section);
                }
                else
                {
                    this.metadataSet.MetadataSections.Add(section);
                }
            }

            void HandleSchemaImports(MetadataSection section)
            {
                XsdNS.XmlSchema schema = (XsdNS.XmlSchema)section.Metadata;
                foreach (XsdNS.XmlSchemaExternal external in schema.Includes)
                {
                    if (!String.IsNullOrEmpty(external.SchemaLocation))
                    {
                        EnqueueRetrieverIfShouldResolve(
                            new MetadataLocationRetriever(
                                this.CreateUri(section.SourceUrl, external.SchemaLocation),
                                this.resolver));
                    }
                }
            }

            void HandleWsdlImports(MetadataSection section)
            {
                WsdlNS.ServiceDescription wsdl = (WsdlNS.ServiceDescription)section.Metadata;
                foreach (WsdlNS.Import import in wsdl.Imports)
                {
                    if (!String.IsNullOrEmpty(import.Location))
                    {
                        EnqueueRetrieverIfShouldResolve(new MetadataLocationRetriever(this.CreateUri(section.SourceUrl, import.Location), this.resolver));
                    }
                }

                foreach (XsdNS.XmlSchema schema in wsdl.Types.Schemas)
                {
                    MetadataSection schemaSection = new MetadataSection(null, null, schema);
                    schemaSection.SourceUrl = section.SourceUrl;
                    this.HandleSchemaImports(schemaSection);
                }
            }

            Uri CreateUri(string baseUri, string relativeUri)
            {
                return new Uri(new Uri(baseUri), relativeUri);
            }

            void EnqueueRetrieverIfShouldResolve(MetadataRetriever retriever)
            {
                if (this.resolveMetadataReferences)
                {
                    this.stackedRetrievers.Push(retriever);
                }
            }

            internal bool HasBeenUsed(MetadataRetriever retriever)
            {
                return this.usedRetrievers.ContainsKey(retriever);
            }

            internal void LogUse(MetadataRetriever retriever)
            {
                this.usedRetrievers.Add(retriever, retriever);
            }
        }

        abstract class MetadataRetriever
        {
            protected MetadataExchangeClient resolver;
            protected string dialect;
            protected string identifier;

            public MetadataRetriever(MetadataExchangeClient resolver, string dialect, string identifier)
            {
                this.resolver = resolver;
                this.dialect = dialect;
                this.identifier = identifier;
            }

            internal MetadataSection Retrieve(TimeoutHelper timeoutHelper)
            {
                try
                {
                    using (XmlReader reader = this.DownloadMetadata(timeoutHelper))
                    {
                        return MetadataRetriever.CreateMetadataSection(reader, this.SourceUrl);
                    }
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.SFxBadMetadataReference, this.SourceUrl), e));
                }
            }

            internal abstract IAsyncResult BeginRetrieve(TimeoutHelper timeoutHelper, AsyncCallback callback, object state);
            internal abstract MetadataSection EndRetrieve(IAsyncResult result);

            static internal MetadataSection CreateMetadataSection(XmlReader reader, string sourceUrl)
            {
                MetadataSection section = null;
                Type metadataType = null;

                if (CanReadMetadataSet(reader))
                {
                    MetadataSet newSet = MetadataSet.ReadFrom(reader);
                    section = new MetadataSection(MetadataSection.MetadataExchangeDialect, null, newSet);
                    metadataType = typeof(MetadataSet);
                }
                else if (WsdlNS.ServiceDescription.CanRead(reader))
                {
                    WsdlNS.ServiceDescription wsdl = WsdlNS.ServiceDescription.Read(reader);
                    section = MetadataSection.CreateFromServiceDescription(wsdl);
                    metadataType = typeof(WsdlNS.ServiceDescription);
                }
                else if (CanReadSchema(reader))
                {
                    XsdNS.XmlSchema schema = XsdNS.XmlSchema.Read(reader, null);
                    section = MetadataSection.CreateFromSchema(schema);
                    metadataType = typeof(XsdNS.XmlSchema);
                }
                else
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(reader);
                    section = new MetadataSection(null, null, doc.DocumentElement);
                    metadataType = typeof(XmlElement);
                }

                section.SourceUrl = sourceUrl;

                TraceReceiveReply(sourceUrl, metadataType);

                return section;
            }

            protected abstract XmlReader DownloadMetadata(TimeoutHelper timeoutHelper);

            protected abstract string SourceUrl { get; }

            static bool CanReadSchema(XmlReader reader)
            {
                return reader.LocalName == MetadataStrings.XmlSchema.Schema
                    && reader.NamespaceURI == XsdNS.XmlSchema.Namespace;
            }

            static bool CanReadMetadataSet(XmlReader reader)
            {
                return reader.LocalName == MetadataStrings.MetadataExchangeStrings.Metadata
                    && reader.NamespaceURI == MetadataStrings.MetadataExchangeStrings.Namespace;
            }
        }

        class MetadataLocationRetriever : MetadataRetriever
        {
            Uri location;
            Uri responseLocation;

            internal MetadataLocationRetriever(Uri location, MetadataExchangeClient resolver)
                : this(location, resolver, null, null)
            {

            }

            internal MetadataLocationRetriever(Uri location, MetadataExchangeClient resolver, string dialect, string identifier)
                : base(resolver, dialect, identifier)
            {
                ValidateLocation(location);
                this.location = location;
                this.responseLocation = location;
            }

            internal static void ValidateLocation(Uri location)
            {
                if (location == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("location");
                }

                if (location.Scheme != Uri.UriSchemeHttp && location.Scheme != Uri.UriSchemeHttps)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("location", SR.GetString(SR.SFxCannotGetMetadataFromLocation, location.ToString()));
                }
            }

            public override bool Equals(object obj)
            {
                return obj is MetadataLocationRetriever && ((MetadataLocationRetriever)obj).location == this.location;
            }

            public override int GetHashCode()
            {
                return location.GetHashCode();
            }

            protected override XmlReader DownloadMetadata(TimeoutHelper timeoutHelper)
            {
                HttpWebResponse response;
                HttpWebRequest request;
                try
                {
                    request = this.resolver.GetWebRequest(this.location, this.dialect, this.identifier);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.SFxMetadataExchangeClientCouldNotCreateWebRequest, this.location, this.dialect, this.identifier), e));
                }

                TraceSendRequest(this.location);
                request.Timeout = TimeoutHelper.ToMilliseconds(timeoutHelper.RemainingTime());
                response = (HttpWebResponse)request.GetResponse();
                responseLocation = request.Address;

                return MetadataLocationRetriever.GetXmlReader(response, this.resolver.MaxMessageSize, this.resolver.ReaderQuotas);
            }

            internal static XmlReader GetXmlReader(HttpWebResponse response, long maxMessageSize, XmlDictionaryReaderQuotas readerQuotas)
            {
                readerQuotas = readerQuotas ?? EncoderDefaults.ReaderQuotas;
                XmlReader reader = XmlDictionaryReader.CreateTextReader(
                    new MaxMessageSizeStream(response.GetResponseStream(), maxMessageSize),
                    EncodingHelper.GetDictionaryReaderEncoding(response.ContentType),
                    readerQuotas,
                    null);

                reader.Read();
                reader.MoveToContent();

                return reader;
            }

            internal override IAsyncResult BeginRetrieve(TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
            {

                AsyncMetadataLocationRetriever result;
                try
                {
                    HttpWebRequest request;
                    try
                    {
                        request = this.resolver.GetWebRequest(this.location, this.dialect, this.identifier);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                            throw;
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.SFxMetadataExchangeClientCouldNotCreateWebRequest, this.location, this.dialect, this.identifier), e));
                    }

                    TraceSendRequest(this.location);
                    result = new AsyncMetadataLocationRetriever(request, this.resolver.MaxMessageSize, this.resolver.ReaderQuotas, timeoutHelper, callback, state);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.SFxBadMetadataReference, this.SourceUrl), e));
                }
                return result;
            }

            internal override MetadataSection EndRetrieve(IAsyncResult result)
            {
                try
                {
                    return AsyncMetadataLocationRetriever.End(result);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.SFxBadMetadataReference, this.SourceUrl), e));
                }
            }

            protected override string SourceUrl
            {
                get { return this.responseLocation.ToString(); }
            }

            class AsyncMetadataLocationRetriever : AsyncResult
            {
                MetadataSection section;
                long maxMessageSize;
                XmlDictionaryReaderQuotas readerQuotas;

                internal AsyncMetadataLocationRetriever(WebRequest request, long maxMessageSize, XmlDictionaryReaderQuotas readerQuotas, TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.maxMessageSize = maxMessageSize;
                    this.readerQuotas = readerQuotas;
                    IAsyncResult result = request.BeginGetResponse(Fx.ThunkCallback(new AsyncCallback(this.GetResponseCallback)), request);

                    //Register a callback to abort the request if we hit the timeout.
                    ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle,
                        Fx.ThunkCallback(new WaitOrTimerCallback(RetrieveTimeout)), request,
                        TimeoutHelper.ToMilliseconds(timeoutHelper.RemainingTime()), /* executeOnlyOnce */ true);

                    if (result.CompletedSynchronously)
                    {
                        HandleResult(result);
                        this.Complete(true);
                    }
                }

                static void RetrieveTimeout(object state, bool timedOut)
                {
                    if (timedOut)
                    {
                        HttpWebRequest request = state as HttpWebRequest;
                        if (request != null)
                        {
                            request.Abort();
                        }
                    }
                }

                internal static MetadataSection End(IAsyncResult result)
                {
                    AsyncMetadataLocationRetriever retrieverResult = AsyncResult.End<AsyncMetadataLocationRetriever>(result);
                    return retrieverResult.section;
                }

                internal void GetResponseCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                        return;

                    Exception exception = null;
                    try
                    {
                        HandleResult(result);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                            throw;
                        exception = e;
                    }
                    this.Complete(false, exception);
                }

                void HandleResult(IAsyncResult result)
                {
                    HttpWebRequest request = (HttpWebRequest)result.AsyncState;

                    using (XmlReader reader =
                        MetadataLocationRetriever.GetXmlReader((HttpWebResponse)request.EndGetResponse(result), this.maxMessageSize, this.readerQuotas))
                    {
                        section = MetadataRetriever.CreateMetadataSection(reader, request.Address.ToString());
                    }
                }
            }


        }

        class MetadataReferenceRetriever : MetadataRetriever
        {
            EndpointAddress address;
            Uri via;

            public MetadataReferenceRetriever(EndpointAddress address, MetadataExchangeClient resolver)
                : this(address, null, resolver, null, null)
            {
            }

            public MetadataReferenceRetriever(EndpointAddress address, Uri via, MetadataExchangeClient resolver)
                : this(address, via, resolver, null, null)
            {
            }

            public MetadataReferenceRetriever(EndpointAddress address, MetadataExchangeClient resolver, string dialect, string identifier)
                : this(address, null, resolver, dialect, identifier)
            {
            }

            MetadataReferenceRetriever(EndpointAddress address, Uri via, MetadataExchangeClient resolver, string dialect, string identifier)
                : base(resolver, dialect, identifier)
            {
                if (address == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
                }

                this.address = address;
                this.via = via;
            }

            protected override string SourceUrl
            {
                get { return this.address.Uri.ToString(); }
            }

            internal override IAsyncResult BeginRetrieve(TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
            {
                try
                {
                    IMetadataExchange metadataClient;
                    MessageVersion messageVersion;
                    lock (this.resolver.ThisLock)
                    {
                        ChannelFactory<IMetadataExchange> channelFactory;
                        try
                        {
                            channelFactory = this.resolver.GetChannelFactory(this.address, this.dialect, this.identifier);
                        }
#pragma warning suppress 56500 // covered by FxCOP
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                                throw;
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                                SR.GetString(SR.SFxMetadataExchangeClientCouldNotCreateChannelFactory, this.address, this.dialect, this.identifier), e));
                        }
                        metadataClient = CreateChannel(channelFactory);
                        messageVersion = channelFactory.Endpoint.Binding.MessageVersion;
                    }
                    TraceSendRequest(this.address);
                    return new AsyncMetadataReferenceRetriever(metadataClient, messageVersion, timeoutHelper, callback, state);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.SFxBadMetadataReference, this.SourceUrl), e));
                }
            }

            IMetadataExchange CreateChannel(ChannelFactory<IMetadataExchange> channelFactory)
            {
                if (this.via != null)
                {
                    return channelFactory.CreateChannel(this.address, this.via);
                }
                else
                {
                    return channelFactory.CreateChannel(this.address);
                }
            }

            static Message CreateGetMessage(MessageVersion messageVersion)
            {
                return Message.CreateMessage(messageVersion, MetadataStrings.WSTransfer.GetAction);
            }

            protected override XmlReader DownloadMetadata(TimeoutHelper timeoutHelper)
            {
                IMetadataExchange metadataClient;
                MessageVersion messageVersion;

                lock (this.resolver.ThisLock)
                {
                    ChannelFactory<IMetadataExchange> channelFactory;
                    try
                    {
                        channelFactory = this.resolver.GetChannelFactory(this.address, this.dialect, this.identifier);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                            throw;
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.SFxMetadataExchangeClientCouldNotCreateChannelFactory, this.address, this.dialect, this.identifier), e));
                    }

                    metadataClient = CreateChannel(channelFactory);
                    messageVersion = channelFactory.Endpoint.Binding.MessageVersion;
                }
                Message response;

                TraceSendRequest(this.address);

                try
                {
                    using (Message getMessage = CreateGetMessage(messageVersion))
                    {
                        ((IClientChannel)metadataClient).OperationTimeout = timeoutHelper.RemainingTime();
                        response = metadataClient.Get(getMessage);
                    }

                    ((IClientChannel)metadataClient).Close();
                }
                finally
                {
                    ((IClientChannel)metadataClient).Abort();
                }

                if (response.IsFault)
                {
                    MessageFault fault = MessageFault.CreateFault(response, 64 * 1024);
                    StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
                    XmlWriter xmlWriter = XmlWriter.Create(stringWriter);
                    fault.WriteTo(xmlWriter, response.Version.Envelope);
                    xmlWriter.Flush();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(stringWriter.ToString()));
                }

                return response.GetReaderAtBodyContents();
            }

            internal override MetadataSection EndRetrieve(IAsyncResult result)
            {
                try
                {
                    return AsyncMetadataReferenceRetriever.End(result);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.SFxBadMetadataReference, this.SourceUrl), e));
                }
            }

            public override bool Equals(object obj)
            {
                return obj is MetadataReferenceRetriever && ((MetadataReferenceRetriever)obj).address == this.address;
            }

            public override int GetHashCode()
            {
                return address.GetHashCode();
            }

            class AsyncMetadataReferenceRetriever : AsyncResult
            {
                MetadataSection section;
                Message message;
                internal AsyncMetadataReferenceRetriever(IMetadataExchange metadataClient, MessageVersion messageVersion, TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
                    : base(callback, state)
                {

                    message = MetadataReferenceRetriever.CreateGetMessage(messageVersion);
                    ((IClientChannel)metadataClient).OperationTimeout = timeoutHelper.RemainingTime();
                    IAsyncResult result = metadataClient.BeginGet(message, Fx.ThunkCallback(new AsyncCallback(this.RequestCallback)), metadataClient);

                    if (result.CompletedSynchronously)
                    {
                        HandleResult(result);

                        this.Complete(true);
                    }
                }

                internal static MetadataSection End(IAsyncResult result)
                {
                    AsyncMetadataReferenceRetriever retrieverResult = AsyncResult.End<AsyncMetadataReferenceRetriever>(result);
                    return retrieverResult.section;
                }

                internal void RequestCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                        return;

                    Exception exception = null;
                    try
                    {
                        HandleResult(result);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                            throw;
                        exception = e;
                    }
                    this.Complete(false, exception);
                }

                void HandleResult(IAsyncResult result)
                {
                    IMetadataExchange metadataClient = (IMetadataExchange)result.AsyncState;
                    Message response = metadataClient.EndGet(result);

                    using (this.message)
                    {
                        if (response.IsFault)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxBadMetadataReference,
                                ((IClientChannel)metadataClient).RemoteAddress.Uri.ToString())));
                        }
                        else
                        {
                            using (XmlReader reader = response.GetReaderAtBodyContents())
                            {
                                section = MetadataRetriever.CreateMetadataSection(reader, ((IClientChannel)metadataClient).RemoteAddress.Uri.ToString());
                            }
                        }
                    }
                }
            }


        }

        class AsyncMetadataResolver : AsyncResult
        {
            ResolveCallState resolveCallState;

            internal AsyncMetadataResolver(ResolveCallState resolveCallState, AsyncCallback callerCallback, object callerAsyncState)
                : base(callerCallback, callerAsyncState)
            {
                if (resolveCallState == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("resolveCallState");
                }

                this.resolveCallState = resolveCallState;


                Exception exception = null;
                bool doneResolving = false;
                try
                {
                    doneResolving = this.ResolveNext();
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;
                    exception = e;
                    doneResolving = true;
                }

                if (doneResolving)
                {
                    this.Complete(true, exception);
                }
            }

            bool ResolveNext()
            {
                bool doneResolving = false;
                if (this.resolveCallState.StackedRetrievers.Count > 0)
                {
                    MetadataRetriever retriever = this.resolveCallState.StackedRetrievers.Pop();

                    if (resolveCallState.HasBeenUsed(retriever))
                    {
                        doneResolving = this.ResolveNext();
                    }
                    else
                    {
                        if (resolveCallState.ResolvedMaxResolvedReferences)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxResolvedMaxResolvedReferences)));
                        }
                        else
                        {
                            resolveCallState.LogUse(retriever);
                            IAsyncResult result = retriever.BeginRetrieve(this.resolveCallState.TimeoutHelper, Fx.ThunkCallback(new AsyncCallback(this.RetrieveCallback)), retriever);

                            if (result.CompletedSynchronously)
                            {
                                doneResolving = HandleResult(result);
                            }
                        }
                    }
                }
                else
                {
                    doneResolving = true;
                }

                return doneResolving;
            }

            internal static MetadataSet End(IAsyncResult result)
            {
                AsyncMetadataResolver resolverResult = AsyncResult.End<AsyncMetadataResolver>(result);
                return resolverResult.resolveCallState.MetadataSet;
            }

            internal void RetrieveCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                    return;

                Exception exception = null;
                bool doneResolving = false;
                try
                {
                    doneResolving = HandleResult(result);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;
                    exception = e;
                    doneResolving = true;
                }

                if (doneResolving)
                {
                    this.Complete(false, exception);
                }
            }

            bool HandleResult(IAsyncResult result)
            {
                MetadataRetriever retriever = (MetadataRetriever)result.AsyncState;
                MetadataSection section = retriever.EndRetrieve(result);
                this.resolveCallState.HandleSection(section);
                return this.ResolveNext();
            }
        }


        internal class EncodingHelper
        {
            internal const string ApplicationBase = "application";

            internal static Encoding GetRfcEncoding(string contentTypeStr)
            {
                Encoding e = null;
                ContentType contentType = null;
                try
                {
                    contentType = new ContentType(contentTypeStr);
                    string charset = contentType == null ? string.Empty : contentType.CharSet;

                    if (charset != null && charset.Length > 0)
                        e = Encoding.GetEncoding(charset);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                        throw;
                }

                // default to ASCII encoding per RFC 2376/3023
                if (IsApplication(contentType))
                    return e == null ? new ASCIIEncoding() : e;
                else
                    return e;
            }

            internal static bool IsApplication(ContentType contentType)
            {
                return string.Compare(contentType == null ? string.Empty : contentType.MediaType,
                    ApplicationBase, StringComparison.OrdinalIgnoreCase) == 0;
            }

            internal static Encoding GetDictionaryReaderEncoding(string contentTypeStr)
            {
                if (String.IsNullOrEmpty(contentTypeStr))
                    return TextEncoderDefaults.Encoding;

                Encoding encoding = GetRfcEncoding(contentTypeStr);

                if (encoding == null)
                    return TextEncoderDefaults.Encoding;

                string charSet = encoding.WebName;
                Encoding[] supportedEncodings = TextEncoderDefaults.SupportedEncodings;
                for (int i = 0; i < supportedEncodings.Length; i++)
                {
                    if (charSet == supportedEncodings[i].WebName)
                        return encoding;
                }

                return TextEncoderDefaults.Encoding;
            }
        }
    }

    public enum MetadataExchangeClientMode
    {
        MetadataExchange,
        HttpGet,
    }

    static class MetadataExchangeClientModeHelper
    {
        static public bool IsDefined(MetadataExchangeClientMode x)
        {
            return
                x == MetadataExchangeClientMode.MetadataExchange ||
                x == MetadataExchangeClientMode.HttpGet ||
                false;
        }

        public static void Validate(MetadataExchangeClientMode value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int)value,
                    typeof(MetadataExchangeClientMode)));
            }
        }
    }


}

