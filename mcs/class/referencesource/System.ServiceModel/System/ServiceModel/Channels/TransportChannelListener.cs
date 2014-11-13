//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Net;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    abstract class TransportChannelListener
        : ChannelListenerBase, ITransportFactorySettings
    {
        // Double-checked locking pattern requires volatile for read/write synchronization
        static volatile bool addressPrefixesInitialized = false;
        static volatile string exactGeneratedAddressPrefix;
        static volatile string strongWildcardGeneratedAddressPrefix;
        static volatile string weakWildcardGeneratedAddressPrefix;
        static object staticLock = new object();

        Uri baseUri;
        BufferManager bufferManager;
        HostNameComparisonMode hostNameComparisonMode;
        bool inheritBaseAddressSettings;
        bool manualAddressing;
        long maxBufferPoolSize;
        long maxReceivedMessageSize;
        MessageEncoderFactory messageEncoderFactory;
        MessageVersion messageVersion;
        Uri uri;
        string hostedVirtualPath;
        Action messageReceivedCallback;
        ServiceSecurityAuditBehavior auditBehavior;
        ServiceModelActivity activity = null;
        TransportManagerContainer transportManagerContainer;

        protected TransportChannelListener(TransportBindingElement bindingElement, BindingContext context)
            : this(bindingElement, context, TransportDefaults.GetDefaultMessageEncoderFactory())
        {
        }

        protected TransportChannelListener(TransportBindingElement bindingElement, BindingContext context,
            MessageEncoderFactory defaultMessageEncoderFactory)
            : this(bindingElement, context, defaultMessageEncoderFactory, TransportDefaults.HostNameComparisonMode)
        {
        }

        protected TransportChannelListener(TransportBindingElement bindingElement, BindingContext context,
            HostNameComparisonMode hostNameComparisonMode)
            : this(bindingElement, context, TransportDefaults.GetDefaultMessageEncoderFactory(), hostNameComparisonMode)
        {
        }

        protected TransportChannelListener(TransportBindingElement bindingElement, BindingContext context,
            MessageEncoderFactory defaultMessageEncoderFactory, HostNameComparisonMode hostNameComparisonMode)
            : base(context.Binding)
        {
            HostNameComparisonModeHelper.Validate(hostNameComparisonMode);
            this.hostNameComparisonMode = hostNameComparisonMode;
            this.manualAddressing = bindingElement.ManualAddressing;
            this.maxBufferPoolSize = bindingElement.MaxBufferPoolSize;
            this.maxReceivedMessageSize = bindingElement.MaxReceivedMessageSize;

            Collection<MessageEncodingBindingElement> messageEncoderBindingElements
                = context.BindingParameters.FindAll<MessageEncodingBindingElement>();

            if (messageEncoderBindingElements.Count > 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MultipleMebesInParameters)));
            }
            else if (messageEncoderBindingElements.Count == 1)
            {
                this.messageEncoderFactory = messageEncoderBindingElements[0].CreateMessageEncoderFactory();
                context.BindingParameters.Remove<MessageEncodingBindingElement>();
            }
            else
            {
                this.messageEncoderFactory = defaultMessageEncoderFactory;
            }

            if (null != this.messageEncoderFactory)
                this.messageVersion = this.messageEncoderFactory.MessageVersion;
            else
                this.messageVersion = MessageVersion.None;

            ServiceSecurityAuditBehavior auditBehavior = context.BindingParameters.Find<ServiceSecurityAuditBehavior>();
            if (auditBehavior != null)
            {
                this.auditBehavior = auditBehavior.Clone();
            }
            else
            {
                this.auditBehavior = new ServiceSecurityAuditBehavior();
            }

            if ((context.ListenUriMode == ListenUriMode.Unique) && (context.ListenUriBaseAddress == null))
            {
                UriBuilder uriBuilder = new UriBuilder(this.Scheme, DnsCache.MachineName);
                uriBuilder.Path = this.GeneratedAddressPrefix;
                context.ListenUriBaseAddress = uriBuilder.Uri;
            }

            UriSchemeKeyedCollection.ValidateBaseAddress(context.ListenUriBaseAddress, "baseAddress");
            if (context.ListenUriBaseAddress.Scheme != this.Scheme)
            {
                // URI schemes are case-insensitive, so try a case insensitive compare now
                if (string.Compare(context.ListenUriBaseAddress.Scheme, this.Scheme, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                        "context.ListenUriBaseAddress",
                        SR.GetString(SR.InvalidUriScheme, context.ListenUriBaseAddress.Scheme, this.Scheme));
                }
            }

            Fx.Assert(context.ListenUriRelativeAddress != null, ""); // validated by BindingContext
            if (context.ListenUriMode == ListenUriMode.Explicit)
            {
                this.SetUri(context.ListenUriBaseAddress, context.ListenUriRelativeAddress);
            }
            else // ListenUriMode.Unique:
            {
                string relativeAddress = context.ListenUriRelativeAddress;
                if (relativeAddress.Length > 0 && !relativeAddress.EndsWith("/", StringComparison.Ordinal))
                {
                    relativeAddress += "/";
                }

                this.SetUri(context.ListenUriBaseAddress, relativeAddress + Guid.NewGuid().ToString());
            }

            this.transportManagerContainer = new TransportManagerContainer(this);
        }

        internal ServiceModelActivity Activity
        {
            get { return this.activity; }
            set { this.activity = value; }
        }

        internal Uri BaseUri
        {
            get
            {
                return this.baseUri;
            }
        }

        string GeneratedAddressPrefix
        {
            get
            {
                EnsureAddressPrefixesInitialized();
            
                // We use different address prefixes based on hostname comparison mode in order to avoid creating
                // starved reservations.  For example, if we register http://+:80/TLA/G1 and http://*:80/TLA/G1, the
                // latter will never receive any traffic.  We handle this case by instead using http://+:80/TLA/G1
                // and http://*:80/TLA/G2.
                switch (this.hostNameComparisonMode)
                {
                    case HostNameComparisonMode.Exact:
                        return exactGeneratedAddressPrefix;
                    case HostNameComparisonMode.StrongWildcard:
                        return strongWildcardGeneratedAddressPrefix;
                    case HostNameComparisonMode.WeakWildcard:
                        return weakWildcardGeneratedAddressPrefix;
                    default:
                        Fx.Assert("invalid HostnameComparisonMode value");
                        return null;
                }
            }
        }

        internal string HostedVirtualPath
        {
            get
            {
                return this.hostedVirtualPath;
            }
        }

        internal bool InheritBaseAddressSettings
        {
            get
            {
                return this.inheritBaseAddressSettings;
            }

            set
            {
                this.inheritBaseAddressSettings = value;
            }
        }

        internal ServiceSecurityAuditBehavior AuditBehavior
        {
            get
            {
                return this.auditBehavior;
            }
        }

        public BufferManager BufferManager
        {
            get
            {
                return this.bufferManager;
            }
        }

        internal HostNameComparisonMode HostNameComparisonModeInternal
        {
            get
            {
                return this.hostNameComparisonMode;
            }
        }

        public bool ManualAddressing
        {
            get
            {
                return this.manualAddressing;
            }
        }

        public long MaxBufferPoolSize
        {
            get
            {
                return this.maxBufferPoolSize;
            }
        }

        public virtual long MaxReceivedMessageSize
        {
            get
            {
                return maxReceivedMessageSize;
            }
        }

        public MessageEncoderFactory MessageEncoderFactory
        {
            get
            {
                return this.messageEncoderFactory;
            }
        }

        public MessageVersion MessageVersion
        {
            get
            {
                return this.messageVersion;
            }
        }

        internal abstract UriPrefixTable<ITransportManagerRegistration> TransportManagerTable
        {
            get;
        }

        public abstract string Scheme { get; }

        public override Uri Uri
        {
            get
            {
                return uri;
            }
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(MessageVersion))
            {
                return (T)(object)this.MessageVersion;
            }

            if (typeof(T) == typeof(FaultConverter))
            {
                if (null == this.MessageEncoderFactory)
                    return null;
                else
                    return this.MessageEncoderFactory.Encoder.GetProperty<T>();
            }

            if (typeof(T) == typeof(ITransportFactorySettings))
            {
                return (T)(object)this;
            }

            return base.GetProperty<T>();
        }

        internal bool IsScopeIdCompatible(HostNameComparisonMode hostNameComparisonMode, Uri uri)
        {
            if (this.hostNameComparisonMode != hostNameComparisonMode)
            {
                return false;
            }

            if (hostNameComparisonMode == HostNameComparisonMode.Exact && uri.HostNameType == UriHostNameType.IPv6)
            {
                // the hostname type of the channel listener MUST be IPv6 if we got here.
                // as this should have been enforced by UriPrefixTable.
                if (this.Uri.HostNameType != UriHostNameType.IPv6)
                {
                    return false;
                }

                IPAddress channelListenerIP = IPAddress.Parse(this.Uri.DnsSafeHost);
                IPAddress otherIP = IPAddress.Parse(uri.DnsSafeHost);

                if (channelListenerIP.ScopeId != otherIP.ScopeId)
                {
                    return false;
                }
            }

            return true;
        }

        internal virtual void ApplyHostedContext(string virtualPath, bool isMetadataListener)
        {
            // Save the original hosted virtual path.
            this.hostedVirtualPath = virtualPath;
        }

        static Uri AddSegment(Uri baseUri, Uri fullUri)
        {
            Uri result = null;
            if (baseUri.AbsolutePath.Length < fullUri.AbsolutePath.Length)
            {
                UriBuilder builder = new UriBuilder(baseUri);
                TcpChannelListener.FixIpv6Hostname(builder, baseUri);
                if (!builder.Path.EndsWith("/", StringComparison.Ordinal))
                {
                    builder.Path = builder.Path + "/";
                    baseUri = builder.Uri;
                }
                Uri relativeUri = baseUri.MakeRelativeUri(fullUri);
                string relativePath = relativeUri.OriginalString;
                int slashIndex = relativePath.IndexOf('/');
                string segment = (slashIndex == -1) ? relativePath : relativePath.Substring(0, slashIndex);
                builder.Path = builder.Path + segment;
                result = builder.Uri;
            }
            return result;
        }

        internal virtual ITransportManagerRegistration CreateTransportManagerRegistration()
        {
            return this.CreateTransportManagerRegistration(this.BaseUri);
        }

        internal abstract ITransportManagerRegistration CreateTransportManagerRegistration(Uri listenUri);

        static void EnsureAddressPrefixesInitialized()
        {
            if (!addressPrefixesInitialized)
            {
                lock (staticLock)
                {
                    if (!addressPrefixesInitialized)
                    {
                        // we use the ephemeral namespace prefix plus a GUID for our App-Domain (which is the
                        // extent to which we can share a TransportManager prefix)
                        exactGeneratedAddressPrefix = "Temporary_Listen_Addresses/" + Guid.NewGuid().ToString();
                        strongWildcardGeneratedAddressPrefix = "Temporary_Listen_Addresses/" + Guid.NewGuid().ToString();
                        weakWildcardGeneratedAddressPrefix = "Temporary_Listen_Addresses/" + Guid.NewGuid().ToString();
                        addressPrefixesInitialized = true;
                    }
                }
            }
        }

        internal virtual int GetMaxBufferSize()
        {
            if (MaxReceivedMessageSize > int.MaxValue)
                return int.MaxValue;
            else
                return (int)MaxReceivedMessageSize;
        }

        protected override void OnOpening()
        {
            base.OnOpening();

            // This check is necessary to avoid that the HostNameComparisonMode from the IIS listener address
            // is copied in an ASP-Net hosted environment when the IIS hosted service acts as client of another destination service
            // (for example using WSDualHttpBinding in a routing service)
            if (this.HostedVirtualPath != null)
            {
                // Copy the HostNameComparisonMode if necessary
                BaseUriWithWildcard baseAddress = AspNetEnvironment.Current.GetBaseUri(this.Scheme, this.Uri);
                if (baseAddress != null)
                {
                    this.hostNameComparisonMode = baseAddress.HostNameComparisonMode;
                }
            }

            this.bufferManager = BufferManager.CreateBufferManager(MaxBufferPoolSize, GetMaxBufferSize());
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.transportManagerContainer.BeginOpen(
                new SelectTransportManagersCallback(this.SelectTransportManagers),
                callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            this.transportManagerContainer.EndOpen(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.transportManagerContainer.Open(new SelectTransportManagersCallback(this.SelectTransportManagers));
        }

        protected override void OnOpened()
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.OpenedListener,
                    SR.GetString(SR.TraceCodeOpenedListener), new UriTraceRecord(this.Uri), this, null);
            }

            base.OnOpened();
        }

        internal TransportManagerContainer GetTransportManagers()
        {
            return TransportManagerContainer.TransferTransportManagers(this.transportManagerContainer);
        }

        protected override void OnAbort()
        {
            this.transportManagerContainer.Abort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.transportManagerContainer.BeginClose(timeout, callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            this.transportManagerContainer.EndClose(result);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.transportManagerContainer.Close(timeout);
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            if (this.bufferManager != null)
            {
                this.bufferManager.Clear();
            }
        }

        bool TryGetTransportManagerRegistration(out ITransportManagerRegistration registration)
        {
            if (!InheritBaseAddressSettings)
            {
                return TryGetTransportManagerRegistration(this.hostNameComparisonMode, out registration);
            }

            if (TryGetTransportManagerRegistration(HostNameComparisonMode.StrongWildcard, out registration))
            {
                return true;
            }

            if (TryGetTransportManagerRegistration(HostNameComparisonMode.Exact, out registration))
            {
                return true;
            }

            if (TryGetTransportManagerRegistration(HostNameComparisonMode.WeakWildcard, out registration))
            {
                return true;
            }

            registration = null;
            return false;
        }

        protected virtual bool TryGetTransportManagerRegistration(HostNameComparisonMode hostNameComparisonMode,
            out ITransportManagerRegistration registration)
        {
            return this.TransportManagerTable.TryLookupUri(this.Uri, hostNameComparisonMode, out registration);
        }

        // This is virtual so that PeerChannelListener and MsmqChannelListener can override it.
        // Will be called under "lock (this.TransportManagerTable)" from TransportManagerContainer.Open
        internal virtual IList<TransportManager> SelectTransportManagers()
        {
            IList<TransportManager> foundTransportManagers = null;

            // Look up an existing transport manager registration.
            ITransportManagerRegistration registration;
            if (!TryGetTransportManagerRegistration(out registration))
            {
                if (DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.NoExistingTransportManager,
                        SR.GetString(SR.TraceCodeNoExistingTransportManager), new UriTraceRecord(this.Uri), this, null);
                }

                // Don't create TransportManagerRegistration in hosted case.
                if (this.HostedVirtualPath == null)
                {
                    // Create a new registration at the default point in the URI hierarchy.
                    registration = this.CreateTransportManagerRegistration();
                    this.TransportManagerTable.RegisterUri(registration.ListenUri, this.hostNameComparisonMode, registration);
                }
            }

            // Use the registration to select/create a set of compatible transport managers.
            if (registration != null)
            {
                foundTransportManagers = registration.Select(this);
                if (foundTransportManagers == null)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.IncompatibleExistingTransportManager,
                            SR.GetString(SR.TraceCodeIncompatibleExistingTransportManager), new UriTraceRecord(this.Uri), this, null);
                    }

                    // Don't create TransportManagerRegistration in hosted case.
                    if (this.HostedVirtualPath == null)
                    {
                        // Create a new registration one segment down from the existing incompatible registration.
                        Uri nextUri = AddSegment(registration.ListenUri, this.Uri);
                        if (nextUri != null)
                        {
                            registration = this.CreateTransportManagerRegistration(nextUri);
                            this.TransportManagerTable.RegisterUri(nextUri, this.hostNameComparisonMode, registration);
                            foundTransportManagers = registration.Select(this);
                        }
                    }
                }
            }

            if (foundTransportManagers == null)
            {
                ThrowTransportManagersNotFound();
            }

            return foundTransportManagers;
        }

        void ThrowTransportManagersNotFound()
        {
            if (this.HostedVirtualPath != null)
            {
                if ((String.Compare(this.Uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) == 0) ||
                    (String.Compare(this.Uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) == 0)
                    )
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(
                            SR.GetString(SR.Hosting_NoHttpTransportManagerForUri, this.Uri)));
                }
                else if ((String.Compare(this.Uri.Scheme, Uri.UriSchemeNetTcp, StringComparison.OrdinalIgnoreCase) == 0) ||
                         (String.Compare(this.Uri.Scheme, Uri.UriSchemeNetPipe, StringComparison.OrdinalIgnoreCase) == 0)
                         )
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(
                            SR.Hosting_NoTcpPipeTransportManagerForUri, this.Uri)));
                }
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                    SR.NoCompatibleTransportManagerForUri, this.Uri)));
        }

        protected void SetUri(Uri baseAddress, string relativeAddress)
        {
            Uri fullUri = baseAddress;

            // Ensure that baseAddress Path does end with a slash if we have a relative address
            if (relativeAddress != string.Empty)
            {
                if (!baseAddress.AbsolutePath.EndsWith("/", StringComparison.Ordinal))
                {
                    UriBuilder uriBuilder = new UriBuilder(baseAddress);
                    TcpChannelListener.FixIpv6Hostname(uriBuilder, baseAddress);
                    uriBuilder.Path = uriBuilder.Path + "/";
                    baseAddress = uriBuilder.Uri;
                }

                fullUri = new Uri(baseAddress, relativeAddress);

                // now see if we need to update our base address (for cases like relative path = "/foo")
                if (!baseAddress.IsBaseOf(fullUri))
                {
                    baseAddress = fullUri;
                }
            }

            this.baseUri = baseAddress;
            ValidateUri(fullUri);
            this.uri = fullUri;
        }

        protected virtual void ValidateUri(Uri uri)
        {
        }

        long ITransportFactorySettings.MaxReceivedMessageSize
        {
            get { return MaxReceivedMessageSize; }
        }

        BufferManager ITransportFactorySettings.BufferManager
        {
            get { return BufferManager; }
        }

        bool ITransportFactorySettings.ManualAddressing
        {
            get { return ManualAddressing; }
        }

        MessageEncoderFactory ITransportFactorySettings.MessageEncoderFactory
        {
            get { return this.MessageEncoderFactory; }
        }

        internal void SetMessageReceivedCallback(Action messageReceivedCallback)
        {
            this.messageReceivedCallback = messageReceivedCallback;
        }

        internal void RaiseMessageReceived()
        {
            Action callback = this.messageReceivedCallback;
            if (callback != null)
            {
                callback();
            }
        }
    }

    interface ITransportManagerRegistration
    {
        HostNameComparisonMode HostNameComparisonMode { get; }
        Uri ListenUri { get; }
        IList<TransportManager> Select(TransportChannelListener factory);
    }

    abstract class TransportManagerRegistration : ITransportManagerRegistration
    {
        HostNameComparisonMode hostNameComparisonMode;
        Uri listenUri;

        protected TransportManagerRegistration(Uri listenUri, HostNameComparisonMode hostNameComparisonMode)
        {
            this.listenUri = listenUri;
            this.hostNameComparisonMode = hostNameComparisonMode;
        }

        public HostNameComparisonMode HostNameComparisonMode
        {
            get { return this.hostNameComparisonMode; }
        }

        public Uri ListenUri
        {
            get
            {
                return this.listenUri;
            }
        }

        public abstract IList<TransportManager> Select(TransportChannelListener factory);
    }

    class UriTraceRecord : TraceRecord
    {
        Uri uri;

        public UriTraceRecord(Uri uri)
        {
            Fx.Assert(uri != null, "UriTraceRecord: Uri is null");
            this.uri = uri;
        }

        internal override void WriteTo(XmlWriter xml)
        {
            xml.WriteElementString("Uri", this.uri.AbsoluteUri);
        }
    }
}
