//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.Text;
    using System.Threading;
    using System.Xml;

    partial class PeerNodeImplementation : IPeerNodeMessageHandling
    {
        const int maxViaSize = 4096;

        public delegate void MessageAvailableCallback(Message message);

        // configuration
        int connectTimeout;
        IPAddress listenIPAddress;
        Uri listenUri;
        int port;
        long maxReceivedMessageSize;
        int minNeighbors;
        int idealNeighbors;
        int maxNeighbors;
        int maxReferrals;
        string meshId;
        PeerMessagePropagationFilter messagePropagationFilter;
        SynchronizationContext messagePropagationFilterContext;
        int maintainerInterval = PeerTransportConstants.MaintainerInterval;          // milliseconds before a maintainer kicks in
        PeerResolver resolver;

        PeerNodeConfig config;

        PeerSecurityManager securityManager;
        internal MessageEncodingBindingElement EncodingElement;

        // internal state
        ManualResetEvent connectCompletedEvent; // raised when maintainer has connected or given up
        MessageEncoder encoder; // used for encoding internal messages

        // Double-checked locking pattern requires volatile for read/write synchronization
        volatile bool isOpen;
        Exception openException; // exception to be thrown from Open
        Dictionary<object, MessageFilterRegistration> messageFilters;
        int refCount; // number of factories/channels that are using this instance
        SimpleStateManager stateManager; // manages open/close operations
        object thisLock = new Object();
        PeerNodeTraceRecord traceRecord;
        PeerNodeTraceRecord completeTraceRecord;    // contains address info as well

        // primary infrastructure components
        internal PeerConnector connector;                           // Purely for testing do not take a internal dependency on this
        PeerMaintainer maintainer;
        internal PeerFlooder flooder;                               // Purely for testing do not take an internal dependency on this
        PeerNeighborManager neighborManager;
        PeerIPHelper ipHelper;
        PeerService service;

        object resolverRegistrationId;
        bool registered;

        public event EventHandler Offline;
        public event EventHandler Online;
        Dictionary<Uri, RefCountedSecurityProtocol> uri2SecurityProtocol;
        Dictionary<Type, object> serviceHandlers;
        BufferManager bufferManager = null;
        internal static byte[] DefaultId = new byte[0];
        XmlDictionaryReaderQuotas readerQuotas;
        long maxBufferPoolSize;
        internal int MaxSendQueue = 128, MaxReceiveQueue = 128;



        public PeerNodeImplementation()
        {
            // intialize default configuration
            connectTimeout = PeerTransportConstants.ConnectTimeout;
            maxReceivedMessageSize = TransportDefaults.MaxReceivedMessageSize;
            minNeighbors = PeerTransportConstants.MinNeighbors;
            idealNeighbors = PeerTransportConstants.IdealNeighbors;
            maxNeighbors = PeerTransportConstants.MaxNeighbors;
            maxReferrals = PeerTransportConstants.MaxReferrals;
            port = PeerTransportDefaults.Port;

            // initialize internal state
            connectCompletedEvent = new ManualResetEvent(false);
            encoder = new BinaryMessageEncodingBindingElement().CreateMessageEncoderFactory().Encoder;
            messageFilters = new Dictionary<object, MessageFilterRegistration>();
            stateManager = new SimpleStateManager(this);
            uri2SecurityProtocol = new Dictionary<Uri, RefCountedSecurityProtocol>();
            readerQuotas = new XmlDictionaryReaderQuotas();
            this.maxBufferPoolSize = TransportDefaults.MaxBufferPoolSize;
        }

        // To facilitate testing
        public event EventHandler<PeerNeighborCloseEventArgs> NeighborClosed;
        public event EventHandler<PeerNeighborCloseEventArgs> NeighborClosing;
        public event EventHandler NeighborConnected;
        public event EventHandler NeighborOpened;

        public event EventHandler Aborted;

        public PeerNodeConfig Config
        {
            get
            {
                return this.config;
            }
            private set
            {
                Fx.Assert(value != null, "PeerNodeImplementation.Config can not be set to null");
                this.config = value;
            }
        }

        public bool IsOnline
        {
            get
            {
                lock (ThisLock)
                {
                    if (isOpen)
                        return neighborManager.IsOnline;
                    else
                        return false;
                }
            }
        }

        internal bool IsOpen
        {
            get { return isOpen; }
        }

        public IPAddress ListenIPAddress
        {
            get { return listenIPAddress; }
            set
            {
                // No validation necessary at this point. When the service is opened, it will throw if the IP address is invalid
                lock (ThisLock)
                {
                    ThrowIfOpen();
                    listenIPAddress = value;
                }
            }
        }

        public Uri ListenUri
        {
            get { return listenUri; }
            set
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");

                if (value.Scheme != PeerStrings.Scheme)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.InvalidUriScheme,
                        value.Scheme, PeerStrings.Scheme));
                }

                Fx.Assert(value.PathAndQuery == "/", "PeerUriCannotContainPath");

                lock (ThisLock)
                {
                    ThrowIfOpen();
                    listenUri = value;
                }
            }
        }

        public long MaxBufferPoolSize
        {
            get { return maxBufferPoolSize; }
            set
            {
                lock (ThisLock)
                {
                    ThrowIfOpen();
                    maxBufferPoolSize = value;
                }
            }
        }

        public long MaxReceivedMessageSize
        {
            get { return maxReceivedMessageSize; }
            set
            {
                if (!(value >= PeerTransportConstants.MinMessageSize))
                {
                    throw Fx.AssertAndThrow("invalid MaxReceivedMessageSize");
                }

                lock (ThisLock)
                {
                    ThrowIfOpen();
                    maxReceivedMessageSize = value;
                }
            }
        }

        public string MeshId
        {
            get
            {
                lock (ThisLock)
                {
                    ThrowIfNotOpen();
                    return meshId;
                }
            }
        }

        public PeerMessagePropagationFilter MessagePropagationFilter
        {
            get { return messagePropagationFilter; }
            set
            {
                lock (ThisLock)
                {
                    // null is ok and causes optimised flooding codepath
                    messagePropagationFilter = value;
                    messagePropagationFilterContext = ThreadBehavior.GetCurrentSynchronizationContext();
                }
            }
        }

        // Made internal to facilitate testing
        public PeerNeighborManager NeighborManager
        {
            get { return neighborManager; }
        }

        public ulong NodeId
        {
            get
            {
                ThrowIfNotOpen();
                return config.NodeId;
            }
        }

        public int Port
        {
            get { return port; }
            set
            {
                lock (ThisLock)
                {
                    ThrowIfOpen();
                    port = value;
                }
            }
        }

        public int ListenerPort
        {
            get
            {
                ThrowIfNotOpen();
                return config.ListenerPort;
            }
        }

        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get
            {
                return this.readerQuotas;
            }
        }

        public PeerResolver Resolver
        {
            get { return resolver; }
            set
            {
                Fx.Assert(value != null, "null Resolver");

                lock (ThisLock)
                {
                    ThrowIfOpen();
                    resolver = value;
                }
            }
        }

        public PeerSecurityManager SecurityManager
        {
            get { return this.securityManager; }
            set { this.securityManager = value; }
        }

        internal PeerService Service
        {
            get
            {
                return this.service;
            }
            set
            {
                lock (ThisLock)
                {
                    ThrowIfNotOpen();
                    this.service = value;
                }
            }
        }

        object ThisLock
        {
            get { return thisLock; }
        }

        public void Abort()
        {
            stateManager.Abort();
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return stateManager.BeginClose(timeout, callback, state);
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state, bool waitForOnline)
        {
            return stateManager.BeginOpen(timeout, callback, state, waitForOnline);
        }

        public Guid ProcessOutgoingMessage(Message message, Uri via)
        {
            Guid result = Guid.NewGuid();
            System.Xml.UniqueId messageId = new System.Xml.UniqueId(result);
            if (-1 != message.Headers.FindHeader(PeerStrings.MessageId, PeerStrings.Namespace))
                PeerExceptionHelper.ThrowInvalidOperation_ConflictingHeader(PeerStrings.MessageId);
            if (-1 != message.Headers.FindHeader(PeerOperationNames.PeerTo, PeerStrings.Namespace))
                PeerExceptionHelper.ThrowInvalidOperation_ConflictingHeader(PeerOperationNames.PeerTo);
            if (-1 != message.Headers.FindHeader(PeerOperationNames.PeerVia, PeerStrings.Namespace))
                PeerExceptionHelper.ThrowInvalidOperation_ConflictingHeader(PeerOperationNames.PeerVia);
            if (-1 != message.Headers.FindHeader(PeerOperationNames.Flood, PeerStrings.Namespace, PeerOperationNames.Demuxer))
                PeerExceptionHelper.ThrowInvalidOperation_ConflictingHeader(PeerOperationNames.Flood);

            message.Headers.Add(PeerDictionaryHeader.CreateMessageIdHeader(messageId));
            message.Properties.Via = via;
            message.Headers.Add(MessageHeader.CreateHeader(PeerOperationNames.PeerTo, PeerStrings.Namespace, message.Headers.To));
            message.Headers.Add(PeerDictionaryHeader.CreateViaHeader(via));
            message.Headers.Add(PeerDictionaryHeader.CreateFloodRole());
            return result;
        }

        public void SecureOutgoingMessage(ref Message message, Uri via, TimeSpan timeout, SecurityProtocol securityProtocol)
        {
            if (securityProtocol != null)
            {
                securityProtocol.SecureOutgoingMessage(ref message, timeout);
            }
        }

        public IAsyncResult BeginSend(object registrant, Message message, Uri via,
            ITransportFactorySettings settings, TimeSpan timeout, AsyncCallback callback, object state, SecurityProtocol securityProtocol)
        {
            PeerFlooder localFlooder;
            int factoryMaxReceivedMessageSize;
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

            MessageBuffer messageBuffer = null;
            Message securedMessage = null;
            ulong hopcount = PeerTransportConstants.MaxHopCount;
            PeerMessagePropagation propagateFlags = PeerMessagePropagation.LocalAndRemote;
            int messageSize = (int)-1;
            byte[] id;
            SendAsyncResult result = new SendAsyncResult(callback, state);
            AsyncCallback onFloodComplete = Fx.ThunkCallback(new AsyncCallback(result.OnFloodComplete));

            try
            {
                lock (ThisLock)
                {
                    ThrowIfNotOpen();
                    localFlooder = flooder;
                }

                // we know this will fit in an int because of our MaxReceivedMessageSize restrictions
                factoryMaxReceivedMessageSize = (int)Math.Min(maxReceivedMessageSize, settings.MaxReceivedMessageSize);
                Guid guid = ProcessOutgoingMessage(message, via);
                SecureOutgoingMessage(ref message, via, timeout, securityProtocol);
                if ((message is SecurityAppliedMessage))
                {
                    ArraySegment<byte> buffer = encoder.WriteMessage(message, int.MaxValue, bufferManager);
                    securedMessage = encoder.ReadMessage(buffer, bufferManager);
                    id = (message as SecurityAppliedMessage).PrimarySignatureValue;
                    messageSize = (int)buffer.Count;
                }
                else
                {
                    securedMessage = message;
                    id = guid.ToByteArray();
                }

                messageBuffer = securedMessage.CreateBufferedCopy(factoryMaxReceivedMessageSize);
                string contentType = settings.MessageEncoderFactory.Encoder.ContentType;
                if (this.messagePropagationFilter != null)
                {
                    using (Message filterMessage = messageBuffer.CreateMessage())
                    {
                        propagateFlags = ((IPeerNodeMessageHandling)this).DetermineMessagePropagation(filterMessage, PeerMessageOrigination.Local);
                    }
                }

                if ((propagateFlags & PeerMessagePropagation.Remote) != PeerMessagePropagation.None)
                {
                    if (hopcount == 0)
                        propagateFlags &= ~PeerMessagePropagation.Remote;
                }

                // flood it out
                IAsyncResult ar = null;
                if ((propagateFlags & PeerMessagePropagation.Remote) != 0)
                {
                    ar = localFlooder.BeginFloodEncodedMessage(id, messageBuffer, timeoutHelper.RemainingTime(), onFloodComplete, null);
                    if (DiagnosticUtility.ShouldTraceVerbose)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.PeerChannelMessageSent, SR.GetString(SR.TraceCodePeerChannelMessageSent), this, message);
                    }
                }
                else
                {
                    ar = new CompletedAsyncResult(onFloodComplete, null);
                }
                if (ar == null)
                {
                    Fx.Assert("SendAsyncResult must have an Async Result for onFloodComplete");
                }

                // queue up the pre-encoded message for local channels
                if ((propagateFlags & PeerMessagePropagation.Local) != 0)
                {
                    using (Message msg = messageBuffer.CreateMessage())
                    {
                        int i = msg.Headers.FindHeader(SecurityJan2004Strings.Security, SecurityJan2004Strings.Namespace);
                        if (i >= 0)
                        {
                            msg.Headers.AddUnderstood(i);
                        }
                        using (MessageBuffer clientBuffer = msg.CreateBufferedCopy(factoryMaxReceivedMessageSize))
                        {
                            DeliverMessageToClientChannels(registrant, clientBuffer, via, message.Headers.To, contentType, messageSize, -1, null);
                        }
                    }
                }
                result.OnLocalDispatchComplete(result);
            }
            finally
            {
                message.Close();
                if (securedMessage != null)
                    securedMessage.Close();
                if (messageBuffer != null)
                    messageBuffer.Close();
            }

            return result;
        }

        public void Close(TimeSpan timeout)
        {
            stateManager.Close(timeout);
        }

        void CloseCore(TimeSpan timeout, bool graceful)
        {
            PeerService lclService;
            PeerMaintainer lclMaintainer;
            PeerNeighborManager lclNeighborManager;
            PeerConnector lclConnector;
            PeerIPHelper lclIPHelper;
            PeerNodeConfig lclConfig;
            PeerFlooder lclFlooder;
            Exception exception = null;

            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PeerNodeClosing, SR.GetString(SR.TraceCodePeerNodeClosing), this.traceRecord, this, null);
            }

            lock (ThisLock)
            {
                isOpen = false;
                lclMaintainer = maintainer;
                lclNeighborManager = neighborManager;
                lclConnector = connector;
                lclIPHelper = ipHelper;
                lclService = service;
                lclConfig = config;
                lclFlooder = flooder;
            }

            // only unregister if we are doing a g----ful shutdown
            try
            {
                if (graceful)
                {
                    UnregisterAddress(timeout);
                }
                else
                {
                    if (lclConfig != null)
                    {
                        ActionItem.Schedule(new Action<object>(UnregisterAddress), lclConfig.UnregisterTimeout);
                    }
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                if (exception == null) exception = e;
            }

            try
            {
                if (lclConnector != null)
                    lclConnector.Closing();

                if (lclService != null)
                {
                    try
                    {
                        lclService.Abort();
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        if (exception == null) exception = e;
                    }
                }

                if (lclMaintainer != null)
                {
                    try
                    {
                        lclMaintainer.Close();
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        if (exception == null) exception = e;
                    }
                }

                if (lclIPHelper != null)
                {
                    try
                    {
                        lclIPHelper.Close();
                        lclIPHelper.AddressChanged -= new EventHandler(stateManager.OnIPAddressesChanged);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        if (exception == null) exception = e;
                    }
                }
                if (lclNeighborManager != null)
                {
                    lclNeighborManager.NeighborConnected -= new EventHandler(OnNeighborConnected);
                    lclNeighborManager.NeighborOpened -= new EventHandler(this.securityManager.OnNeighborOpened);
                    this.securityManager.OnNeighborAuthenticated -= new EventHandler(this.OnNeighborAuthenticated);
                    lclNeighborManager.Online -= new EventHandler(FireOnline);
                    lclNeighborManager.Offline -= new EventHandler(FireOffline);
                    try
                    {
                        lclNeighborManager.Shutdown(graceful, timeoutHelper.RemainingTime());
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        if (exception == null) exception = e;
                    }

                    // unregister for neighbor close events once shutdown has completed
                    lclNeighborManager.NeighborClosed -= new EventHandler<PeerNeighborCloseEventArgs>(OnNeighborClosed);
                    lclNeighborManager.NeighborClosing -= new EventHandler<PeerNeighborCloseEventArgs>(OnNeighborClosing);
                    lclNeighborManager.Close();
                }

                if (lclConnector != null)
                {
                    try
                    {
                        lclConnector.Close();
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        if (exception == null) exception = e;
                    }
                }

                if (lclFlooder != null)
                {
                    try
                    {
                        lclFlooder.Close();
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        if (exception == null) exception = e;
                    }
                }

            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                if (exception == null) exception = e;
            }

            // reset object for next call to open
            EventHandler abortedHandler = null;
            lock (ThisLock)
            {
                // clear out old components (so they can be garbage collected)
                neighborManager = null;
                connector = null;
                maintainer = null;
                flooder = null;
                ipHelper = null;
                service = null;

                // reset generated config
                config = null;
                meshId = null;
                abortedHandler = Aborted;
            }

            // Notify anyone who is interested that abort has occured 
            if (!graceful && abortedHandler != null)
            {
                try
                {
                    abortedHandler(this, EventArgs.Empty);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e)) throw;
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    if (exception == null) exception = e;
                }
            }

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PeerNodeClosed, SR.GetString(SR.TraceCodePeerNodeClosed), this.traceRecord, this, null);
            }
            if (exception != null && graceful == true)                          // Swallows all non fatal exceptions during Abort
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
        }

        // Performs case-insensitive comparison of two vias
        bool CompareVia(Uri via1, Uri via2)
        {
            return (Uri.Compare(via1, via2,
                (UriComponents.Scheme | UriComponents.UserInfo | UriComponents.Host | UriComponents.Port | UriComponents.Path),
                UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0);
        }

        public static void EndClose(IAsyncResult result)
        {
            if (result == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");

            SimpleStateManager.EndClose(result);
        }

        public static void EndOpen(IAsyncResult result)
        {
            if (result == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");

            SimpleStateManager.EndOpen(result);
        }

        public static void EndSend(IAsyncResult result)
        {
            if (result == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");

            SendAsyncResult.End(result);
        }

        // Necessary to allow access of the EventHandlers which can only be done from inside the class
        void FireOffline(object sender, EventArgs e)
        {
            if (!isOpen)
            {
                return;
            }

            EventHandler handler = Offline;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        // Necessary to allow access of the EventHandlers which can only be done from inside the class
        void FireOnline(object sender, EventArgs e)
        {
            if (!isOpen)
            {
                return;
            }

            EventHandler handler = Online;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        // static Uri -> PeerNode mapping
        static internal Dictionary<Uri, PeerNodeImplementation> peerNodes = new Dictionary<Uri, PeerNodeImplementation>();

        internal static PeerNodeImplementation Get(Uri listenUri)
        {
            PeerNodeImplementation node = null;
            if (!TryGet(listenUri, out node))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR.GetString(SR.NoTransportManagerForUri, listenUri)));
            }
            return node;
        }

        internal protected static bool TryGet(Uri listenUri, out PeerNodeImplementation result)
        {
            if (listenUri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("listenUri");
            }

            if (listenUri.Scheme != PeerStrings.Scheme)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("listenUri", SR.GetString(SR.InvalidUriScheme,
                    listenUri.Scheme, PeerStrings.Scheme));
            }
            result = null;
            bool success = false;
            // build base uri
            Uri baseUri = new UriBuilder(PeerStrings.Scheme, listenUri.Host).Uri;

            lock (peerNodes)
            {
                if (peerNodes.ContainsKey(baseUri))
                {
                    result = peerNodes[baseUri];
                    success = true;
                }
            }
            return success;
        }

        public static bool TryGet(string meshId, out PeerNodeImplementation result)
        {
            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Host = meshId;
            uriBuilder.Scheme = PeerStrings.Scheme;
            bool success = PeerNodeImplementation.TryGet(uriBuilder.Uri, out result);
            return success;
        }

        // internal method to return an existing PeerNode or create a new one with the given settings
        public static PeerNodeImplementation Get(Uri listenUri, Registration registration)
        {
            if (listenUri == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("listenUri");

            if (listenUri.Scheme != PeerStrings.Scheme)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("listenUri", SR.GetString(SR.InvalidUriScheme,
                    listenUri.Scheme, PeerStrings.Scheme));
            }

            // build base uri
            Uri baseUri = new UriBuilder(PeerStrings.Scheme, listenUri.Host).Uri;

            lock (peerNodes)
            {
                PeerNodeImplementation peerNodeImpl = null;
                PeerNodeImplementation peerNode = null;
                if (peerNodes.TryGetValue(baseUri, out peerNode))
                {
                    peerNodeImpl = (PeerNodeImplementation)peerNode;

                    // ensure that the PeerNode is compatible
                    registration.CheckIfCompatible(peerNodeImpl, listenUri);
                    peerNodeImpl.refCount++;
                    return peerNodeImpl;
                }

                // create a new PeerNode, and add it to the dictionary
                peerNodeImpl = registration.CreatePeerNode();
                peerNodes[baseUri] = peerNodeImpl;
                peerNodeImpl.refCount = 1;
                return peerNodeImpl;
            }
        }

        // SimpleStateManager callback - Called on final release of PeerNode.
        void InternalClose(TimeSpan timeout, bool graceful)
        {
            CloseCore(timeout, graceful);
            lock (ThisLock)
            {
                messageFilters.Clear();
            }
        }

        protected void OnAbort()
        {
            InternalClose(TimeSpan.FromTicks(0), false);
        }

        protected void OnClose(TimeSpan timeout)
        {
            InternalClose(timeout, true);
        }

        // called when the maintainer has completed the connection attempt (successful or not)
        void OnConnectionAttemptCompleted(Exception e)
        {
            // store the exception if one occured when trying to connect, so that it can be rethrown from Open

            Fx.Assert(openException == null, "OnConnectionAttemptCompleted twice");
            openException = e;

            if (openException == null && DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PeerNodeOpened, SR.GetString(SR.TraceCodePeerNodeOpened), this.completeTraceRecord, this, null);
            }
            else if (openException != null && DiagnosticUtility.ShouldTraceError)
            {
                TraceUtility.TraceEvent(TraceEventType.Error, TraceCode.PeerNodeOpenFailed, SR.GetString(SR.TraceCodePeerNodeOpenFailed), this.completeTraceRecord, this, e);
            }

            connectCompletedEvent.Set();
        }

        bool IPeerNodeMessageHandling.ValidateIncomingMessage(ref Message message, Uri via)
        {
            SecurityProtocol protocol = null;

            if (via == null)
            {
                Fx.Assert("FloodMessage doesn't contain Via header!");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.PeerMessageMustHaveVia, message.Headers.Action)));
            }
            if (TryGetSecurityProtocol(via, out protocol))
            {
                protocol.VerifyIncomingMessage(ref message, ServiceDefaults.SendTimeout, null);
                return true;
            }
            return false;
        }

        internal bool TryGetSecurityProtocol(Uri via, out SecurityProtocol protocol)
        {
            lock (ThisLock)
            {
                RefCountedSecurityProtocol wrapper = null;
                bool result = false;
                protocol = null;
                if (uri2SecurityProtocol.TryGetValue(via, out wrapper))
                {
                    protocol = wrapper.Protocol;
                    result = true;
                }
                return result;
            }
        }

        void IPeerNodeMessageHandling.HandleIncomingMessage(MessageBuffer messageBuffer, PeerMessagePropagation propagateFlags,
            int index, MessageHeader hopHeader, Uri via, Uri to)
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.PeerFloodedMessageReceived, SR.GetString(SR.TraceCodePeerFloodedMessageReceived), this.traceRecord, this, null);
            }

            if (via == null)
            {
                Fx.Assert("No VIA in the forwarded message!");
                using (Message message = messageBuffer.CreateMessage())
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.PeerMessageMustHaveVia, message.Headers.Action)));
                }
            }
            if ((propagateFlags & PeerMessagePropagation.Local) != 0)
            {
                DeliverMessageToClientChannels(null, messageBuffer, via, to, messageBuffer.MessageContentType, (int)maxReceivedMessageSize, index, hopHeader);
                messageBuffer = null;
            }
            else
            {
                if (DiagnosticUtility.ShouldTraceVerbose)
                {
                    using (Message traceMessage = messageBuffer.CreateMessage())
                    {
                        TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.PeerFloodedMessageNotPropagated, SR.GetString(SR.TraceCodePeerFloodedMessageNotPropagated), this.traceRecord, this, null, traceMessage);
                    }
                }
            }
        }

        PeerMessagePropagation IPeerNodeMessageHandling.DetermineMessagePropagation(Message message, PeerMessageOrigination origination)
        {
            PeerMessagePropagation propagateFlags = PeerMessagePropagation.LocalAndRemote;
            PeerMessagePropagationFilter filter = MessagePropagationFilter;
            if (filter != null)
            {
                try
                {
                    SynchronizationContext context = messagePropagationFilterContext;
                    if (context != null)
                    {
                        context.Send(delegate(object state) { propagateFlags = filter.ShouldMessagePropagate(message, origination); }, null);
                    }
                    else
                    {
                        propagateFlags = filter.ShouldMessagePropagate(message, origination);
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e)) throw;
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(SR.GetString(SR.MessagePropagationException), e);
                }
            }

            // Don't flood if the Node is closed
            if (!isOpen)
            {
                propagateFlags = PeerMessagePropagation.None;
            }

            return propagateFlags;
        }


        // Queued callback to actually process the address change
        // The design is such that any address change notifications are queued just like Open/Close operations.
        // So, we need not worry about address changes racing with other address changes or Open/Close operations.
        // Abort can happen at any time. However, Abort skips unregistering addresses, so this method doesn't have 
        // to worry about undoing its work if Abort happens.
        void OnIPAddressChange()
        {
            string lclMeshId = null;
            PeerNodeAddress nodeAddress = null;
            object lclResolverRegistrationId = null;
            bool lclRegistered = false;
            PeerIPHelper lclIPHelper = ipHelper;
            PeerNodeConfig lclconfig = config;
            bool processChange = false;
            TimeoutHelper timeoutHelper = new TimeoutHelper(ServiceDefaults.SendTimeout);

            // Determine if IP addresses have really changed before notifying the resolver
            // since it is possible that another change notification ahead of this one in the queue 
            // may have already completed notifying the resolver of the most current change.
            if (lclIPHelper != null && config != null)
            {
                nodeAddress = lclconfig.GetListenAddress(false);
                processChange = lclIPHelper.AddressesChanged(nodeAddress.IPAddresses);
                if (processChange)
                {
                    // Build the nodeAddress with the updated IP addresses
                    nodeAddress = new PeerNodeAddress(
                        nodeAddress.EndpointAddress, lclIPHelper.GetLocalAddresses());
                }
            }

            lock (ThisLock)
            {
                // Skip processing if the node isn't open anymore or if addresses haven't changed
                if (processChange && isOpen)
                {
                    lclMeshId = meshId;
                    lclResolverRegistrationId = resolverRegistrationId;
                    lclRegistered = registered;
                    config.SetListenAddress(nodeAddress);
                    completeTraceRecord = new PeerNodeTraceRecord(config.NodeId, meshId, nodeAddress);
                }
                else
                {
                    return;
                }
            }
            //#57954 - log and ---- non-critical exceptions during network change event notifications
            try
            {
                // Do we have any addresses? If so, update or re-register. Otherwise, unregister.
                if (nodeAddress.IPAddresses.Count > 0)
                {
                    if (lclRegistered)
                    {
                        resolver.Update(lclResolverRegistrationId, nodeAddress, timeoutHelper.RemainingTime());
                    }
                    else
                    {
                        RegisterAddress(lclMeshId, nodeAddress, timeoutHelper.RemainingTime());
                    }
                }
                else
                {
                    UnregisterAddress(timeoutHelper.RemainingTime());
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
            }
            PingConnections();

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PeerNodeAddressChanged, SR.GetString(SR.TraceCodePeerNodeAddressChanged), this.completeTraceRecord, this, null);
            }
        }

        // Register with the resolver
        void RegisterAddress(string lclMeshId, PeerNodeAddress nodeAddress, TimeSpan timeout)
        {
            // Register only if we have any addresses
            if (nodeAddress.IPAddresses.Count > 0)
            {
                object lclResolverRegistrationId = null;
                try
                {
                    lclResolverRegistrationId = resolver.Register(lclMeshId, nodeAddress, timeout);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e)) throw;
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.ResolverException), e));
                }
                lock (ThisLock)
                {
                    if (!(!registered))
                    {
                        throw Fx.AssertAndThrow("registered expected to be false");
                    }
                    registered = true;
                    resolverRegistrationId = lclResolverRegistrationId;
                }
            }
        }

        // Unregister that should only be called from non-user threads.
        //since this is invoked on background threads, we log and ---- all non-critical exceptions
        //#57972
        void UnregisterAddress(object timeout)
        {
            try
            {
                UnregisterAddress((TimeSpan)timeout);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
            }
        }

        void UnregisterAddress(TimeSpan timeout)
        {
            bool needToUnregister = false;
            object lclResolverRegistrationId = null;
            lock (ThisLock)
            {
                if (registered)
                {
                    needToUnregister = true;
                    lclResolverRegistrationId = resolverRegistrationId;
                    registered = false;                 // this ensures that the current thread will do unregistration
                }
                resolverRegistrationId = null;
            }
            if (needToUnregister)
            {
                try
                {
                    resolver.Unregister(lclResolverRegistrationId, timeout);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e)) throw;
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.ResolverException), e));
                }
            }
        }

        void OnNeighborClosed(object sender, PeerNeighborCloseEventArgs e)
        {
            IPeerNeighbor neighbor = (IPeerNeighbor)sender;
            PeerConnector localConnector;
            PeerMaintainer localMaintainer;
            PeerFlooder localFlooder;

            localConnector = connector;
            localMaintainer = maintainer;
            localFlooder = flooder;

            UtilityExtension.OnNeighborClosed(neighbor);
            PeerChannelAuthenticatorExtension.OnNeighborClosed(neighbor);

            if (localConnector != null)
                localConnector.OnNeighborClosed(neighbor);
            if (localMaintainer != null)
                localMaintainer.OnNeighborClosed(neighbor);
            if (localFlooder != null)
                localFlooder.OnNeighborClosed(neighbor);

            // Finally notify any Peernode client
            EventHandler<PeerNeighborCloseEventArgs> handler = NeighborClosed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        void OnNeighborClosing(object sender, PeerNeighborCloseEventArgs e)
        {
            IPeerNeighbor neighbor = (IPeerNeighbor)sender;
            PeerConnector localConnector;

            localConnector = connector;

            if (localConnector != null)
                localConnector.OnNeighborClosing(neighbor, e.Reason);

            // Finally notify any Peernode client
            EventHandler<PeerNeighborCloseEventArgs> handler = NeighborClosing;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        void OnNeighborConnected(object sender, EventArgs e)
        {
            IPeerNeighbor neighbor = (IPeerNeighbor)sender;
            PeerMaintainer localMaintainer = maintainer;
            PeerFlooder localFlooder = flooder;

            if (localFlooder != null)
                localFlooder.OnNeighborConnected(neighbor);

            if (localMaintainer != null)
                localMaintainer.OnNeighborConnected(neighbor);

            UtilityExtension.OnNeighborConnected(neighbor);

            // Finally notify any Peernode client
            EventHandler handler = NeighborConnected;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        // raised by the neighbor manager when any connection has reached the opened state
        void OnNeighborAuthenticated(object sender, EventArgs e)
        {
            IPeerNeighbor n = (IPeerNeighbor)sender;

            //hand the authenticated neighbor over to connector.
            //If neighbor is aborted before 
            PeerConnector localConnector = connector;
            if (localConnector != null)
                connector.OnNeighborAuthenticated(n);

            // Finally notify any Peernode client
            EventHandler handler = NeighborOpened;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        // Open blocks the thread until either Online happens or Open times out.
        void OnOpen(TimeSpan timeout, bool waitForOnline)
        {
            bool aborted = false;
            EventHandler connectedHandler = delegate(object source, EventArgs args) { connectCompletedEvent.Set(); };
            EventHandler abortHandler = delegate(object source, EventArgs args) { aborted = true; connectCompletedEvent.Set(); };
            openException = null;                                               // clear out the open exception from the last Open attempt
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

            try
            {
                NeighborConnected += connectedHandler;
                Aborted += abortHandler;
                OpenCore(timeout);

                if (waitForOnline)
                {
                    if (!TimeoutHelper.WaitOne(connectCompletedEvent, timeoutHelper.RemainingTime()))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException());
                    }
                }

                if (aborted)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationObjectAbortedException(SR.GetString(SR.PeerNodeAborted)));
                }

                // retrieve listen addresses and register with the resolver
                if (isOpen)
                {
                    if (openException != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(openException);
                    }
                    else
                    {
                        string lclMeshId = null;
                        PeerNodeConfig lclConfig = null;
                        lock (ThisLock)
                        {
                            lclMeshId = meshId;
                            lclConfig = config;
                        }

                        // The design is such that any address change notifications are queued behind Open operation
                        // So, we need not worry about address changes racing with the initial registration.
                        RegisterAddress(lclMeshId, lclConfig.GetListenAddress(false), timeoutHelper.RemainingTime());
                    }
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                CloseCore(TimeSpan.FromTicks(0), false);
                throw;
            }
            finally
            {
                NeighborConnected -= connectedHandler;
                Aborted -= abortHandler;
            }
        }

        internal void Open(TimeSpan timeout, bool waitForOnline)
        {
            stateManager.Open(timeout, waitForOnline);
        }

        // the core functionality of open (all but waiting for a connection)
        void OpenCore(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            PeerMaintainer lclMaintainer;
            PeerNodeConfig lclConfig;
            string lclMeshId;

            lock (ThisLock)
            {
                if (ListenUri == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ListenUriNotSet, this.GetType())));
                }

                // extract mesh id from listen uri
                meshId = ListenUri.Host;

                // generate the node id
                byte[] bytes = new byte[sizeof(ulong)];
                ulong nodeId = 0;
                do
                {
                    System.ServiceModel.Security.CryptoHelper.FillRandomBytes(bytes);
                    for (int i = 0; i < sizeof(ulong); i++)
                        nodeId |= ((ulong)bytes[i]) << i * 8;
                }
                while (nodeId == PeerTransportConstants.InvalidNodeId);

                // now that the node id has been generated, create the trace record that describes this
                traceRecord = new PeerNodeTraceRecord(nodeId, meshId);
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PeerNodeOpening, SR.GetString(SR.TraceCodePeerNodeOpening), this.traceRecord, this, null);
                }

                // create the node configuration
                config = new PeerNodeConfig(meshId,
                                                nodeId,
                                                resolver,
                                                messagePropagationFilter,
                                                encoder,
                                                ListenUri, listenIPAddress, port,
                                                maxReceivedMessageSize, minNeighbors, idealNeighbors, maxNeighbors, maxReferrals,
                                                connectTimeout, maintainerInterval,
                                                securityManager,
                                                this.readerQuotas,
                                                this.maxBufferPoolSize,
                                                this.MaxSendQueue,
                                                this.MaxReceiveQueue);

                // create components
                if (listenIPAddress != null)
                    ipHelper = new PeerIPHelper(listenIPAddress);
                else
                    ipHelper = new PeerIPHelper();
                bufferManager = BufferManager.CreateBufferManager(64 * config.MaxReceivedMessageSize, (int)config.MaxReceivedMessageSize);
                neighborManager = new PeerNeighborManager(ipHelper,
                                                            config,
                                                            this);
                flooder = PeerFlooder.CreateFlooder(config, neighborManager, this);
                maintainer = new PeerMaintainer(config, neighborManager, flooder);
                connector = new PeerConnector(config, neighborManager, maintainer);

                Dictionary<Type, object> services = serviceHandlers;
                if (services == null)
                {
                    services = new Dictionary<Type, object>();
                    services.Add(typeof(IPeerConnectorContract), connector);
                    services.Add(typeof(IPeerFlooderContract<Message, UtilityInfo>), flooder);
                }
                service = new PeerService(this.config,
                                        neighborManager.ProcessIncomingChannel,
                                        neighborManager.GetNeighborFromProxy,
                                        services,
                                        this);
                this.securityManager.MeshId = this.meshId;
                service.Open(timeoutHelper.RemainingTime());

                // register for events
                neighborManager.NeighborClosed += new EventHandler<PeerNeighborCloseEventArgs>(OnNeighborClosed);
                neighborManager.NeighborClosing += new EventHandler<PeerNeighborCloseEventArgs>(OnNeighborClosing);
                neighborManager.NeighborConnected += new EventHandler(OnNeighborConnected);
                neighborManager.NeighborOpened += new EventHandler(this.SecurityManager.OnNeighborOpened);
                this.securityManager.OnNeighborAuthenticated += new EventHandler(this.OnNeighborAuthenticated);
                neighborManager.Online += new EventHandler(FireOnline);
                neighborManager.Offline += new EventHandler(FireOffline);
                ipHelper.AddressChanged += new EventHandler(stateManager.OnIPAddressesChanged);

                // open components
                ipHelper.Open();

                // Set the listen address before opening any more components
                PeerNodeAddress nodeAddress = new PeerNodeAddress(service.GetListenAddress(), ipHelper.GetLocalAddresses());
                config.SetListenAddress(nodeAddress);

                neighborManager.Open(service.Binding, service);
                connector.Open();
                maintainer.Open();
                flooder.Open();

                isOpen = true;
                completeTraceRecord = new PeerNodeTraceRecord(nodeId, meshId, nodeAddress);

                // Set these locals inside the lock (Abort may occur whilst Opening)
                lclMaintainer = maintainer;

                lclMeshId = meshId;
                lclConfig = config;
                openException = null;

            }

            // retrieve listen addresses and register with the resolver
            if (isOpen)
            {
                // attempt to connect to the mesh
                lclMaintainer.ScheduleConnect(new PeerMaintainer.ConnectCallback(OnConnectionAttemptCompleted));
            }
        }

        void DeliverMessageToClientChannels(
                                object registrant,
                                MessageBuffer messageBuffer,
                                Uri via,
                                Uri peerTo,
                                string contentType,
                                int messageSize,
                                int index,
                                MessageHeader hopHeader)
        {
            Message message = null;
            try
            {
                // create a list of callbacks so they can each be called outside the lock
                ArrayList callbacks = new ArrayList();
                Uri to = peerTo;
                Fx.Assert(peerTo != null, "Invalid To header value!");
                if (isOpen)
                {
                    lock (ThisLock)
                    {
                        if (isOpen)
                        {
                            foreach (MessageFilterRegistration mfr in messageFilters.Values)
                            {
                                // first, the via's must match
                                bool match = CompareVia(via, mfr.via);
                                if (messageSize < 0)
                                {
                                    //messageSize <0 indicates that this message is coming from BeginSend
                                    //and the size is not computed yet.
                                    if (message == null)
                                    {
                                        message = messageBuffer.CreateMessage();
                                        Fx.Assert(message.Headers.To == to, "To Header is inconsistent in Send() case!");
                                        Fx.Assert(message.Properties.Via == via, "Via property is inconsistent in Send() case!");
                                    }
                                    //incoming message need not be verified MaxReceivedSize
                                    //only do this for local channels
                                    if (registrant != null)
                                    {
                                        ArraySegment<byte> buffer = encoder.WriteMessage(message, int.MaxValue, bufferManager);
                                        messageSize = (int)buffer.Count;
                                    }
                                }
                                // only queue the message for registrants expecting this size
                                match = match && (messageSize <= mfr.settings.MaxReceivedMessageSize);

                                // if a filter is specified, it must match as well
                                if (match && mfr.filters != null)
                                {
                                    for (int i = 0; match && i < mfr.filters.Length; i++)
                                    {
                                        match = mfr.filters[i].Match(via, to);
                                    }
                                }

                                if (match)
                                {
                                    callbacks.Add(mfr.callback);
                                }
                            }
                        }
                    }
                }
                foreach (MessageAvailableCallback callback in callbacks)
                {
                    Message localCopy;
                    try
                    {
                        //this copy is free'd by SFx.
                        localCopy = messageBuffer.CreateMessage();
                        localCopy.Properties.Via = via;
                        localCopy.Headers.To = to;
                        //mark security header as understood.
                        try
                        {
                            int i = localCopy.Headers.FindHeader(SecurityJan2004Strings.Security, SecurityJan2004Strings.Namespace);
                            if (i >= 0)
                            {
                                localCopy.Headers.AddUnderstood(i);
                            }
                        }
                        catch (MessageHeaderException e)
                        {
                            DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
                        }
                        catch (SerializationException e)
                        {
                            DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
                        }
                        catch (XmlException e)
                        {
                            DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
                        }

                        if (index != -1)
                        {
                            localCopy.Headers.ReplaceAt(index, hopHeader);
                        }

                        callback(localCopy);
                    }
                    catch (ObjectDisposedException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    catch (CommunicationObjectAbortedException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    catch (CommunicationObjectFaultedException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                }
            }
            finally
            {
                if (message != null)
                    message.Close();
            }
        }

        public void RefreshConnection()
        {
            PeerMaintainer lclMaintainer = null;
            lock (ThisLock)
            {
                ThrowIfNotOpen();
                lclMaintainer = maintainer;
            }
            if (lclMaintainer != null)
            {
                lclMaintainer.RefreshConnection();
            }
        }

        public void PingConnections()
        {
            PeerMaintainer lclMaintainer = null;
            lock (ThisLock)
            {
                lclMaintainer = maintainer;
            }
            if (lclMaintainer != null)
            {
                lclMaintainer.PingConnections();
            }
        }

        //always call methods from inside a lock (of the container)
        class RefCountedSecurityProtocol
        {
            int refCount;
            public SecurityProtocol Protocol;
            public RefCountedSecurityProtocol(SecurityProtocol securityProtocol)
            {
                this.Protocol = securityProtocol;
                this.refCount = 1;
            }
            public int AddRef()
            {
                return ++refCount;
            }
            public int Release()
            {
                return --refCount;
            }
        }

        // internal message filtering
        internal void RegisterMessageFilter(object registrant, Uri via, PeerMessageFilter[] filters,
            ITransportFactorySettings settings, MessageAvailableCallback callback, SecurityProtocol securityProtocol)
        {
            MessageFilterRegistration registration = new MessageFilterRegistration();
            registration.registrant = registrant;
            registration.via = via;
            registration.filters = filters;
            registration.settings = settings;
            registration.callback = callback;
            registration.securityProtocol = securityProtocol;
            lock (ThisLock)
            {
                messageFilters.Add(registrant, registration);
                RefCountedSecurityProtocol protocolWrapper = null;
                if (!this.uri2SecurityProtocol.TryGetValue(via, out protocolWrapper))
                {
                    protocolWrapper = new RefCountedSecurityProtocol(securityProtocol);
                    this.uri2SecurityProtocol.Add(via, protocolWrapper);
                }
                else
                    protocolWrapper.AddRef();
            }
        }

        // internal method to release the reference on an existing PeerNode
        internal void Release()
        {
            lock (peerNodes)
            {
                if (peerNodes.ContainsValue(this))
                {
                    if (--refCount == 0)
                    {
                        // no factories/channels are using this instance (although the application may still be
                        // referring to it directly). either way, we remove this from the registry
                        peerNodes.Remove(listenUri);
                    }
                }
            }
        }

        // Call with null to reset to our implementation
        public void SetServiceHandlers(Dictionary<Type, object> services)
        {
            lock (ThisLock)
            {
                serviceHandlers = services;
            }
        }

        void ThrowIfNotOpen()
        {
            if (!isOpen)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.TransportManagerNotOpen)));
            }
        }

        void ThrowIfOpen()
        {
            if (isOpen)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                    SR.TransportManagerOpen)));
            }
        }

        public override string ToString()
        {
            lock (ThisLock)
            {
                // if open return the mesh id, otherwise return the type
                if (isOpen)
                    return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        "{0} ({1})", MeshId, NodeId);
                else
                    return this.GetType().ToString();
            }
        }

        internal void UnregisterMessageFilter(object registrant, Uri via)
        {
            lock (ThisLock)
            {
                messageFilters.Remove(registrant);
                RefCountedSecurityProtocol protocolWrapper = null;
                if (uri2SecurityProtocol.TryGetValue(via, out protocolWrapper))
                {
                    if (protocolWrapper.Release() == 0)
                        uri2SecurityProtocol.Remove(via);
                }
                else
                    Fx.Assert(false, "Corresponding SecurityProtocol is not Found!");
            }
        }

        internal static void ValidateVia(Uri uri)
        {
            int viaSize = Encoding.UTF8.GetByteCount(uri.OriginalString);
            if (viaSize > maxViaSize)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataException(SR.GetString(
                    SR.PeerChannelViaTooLong, uri, viaSize, maxViaSize)));
        }

        internal class ChannelRegistration
        {
            public object registrant;
            public Uri via;
            public ITransportFactorySettings settings;
            public SecurityProtocol securityProtocol;
            public Type channelType;

        }

        // holds the registration information passed in by channels and listeners. This informtaion is used
        // to determine which channels and listeners will receive an incoming message
        class MessageFilterRegistration : ChannelRegistration
        {
            public PeerMessageFilter[] filters;
            public MessageAvailableCallback callback;
        }

        // represents the settings of a PeerListenerFactory or PeerChannelFactory, used to create a new
        // PeerNode or compare settings to an existing PeerNode
        internal class Registration
        {
            IPAddress listenIPAddress;
            Uri listenUri;
            long maxReceivedMessageSize;
            int port;
            PeerResolver resolver;
            PeerSecurityManager securityManager;
            XmlDictionaryReaderQuotas readerQuotas;
            long maxBufferPoolSize;

            public Registration(Uri listenUri, IPeerFactory factory)
            {
                if (factory.Resolver == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(SR.PeerResolverRequired)));
                }
                if (factory.ListenIPAddress != null)
                {
                    listenIPAddress = factory.ListenIPAddress;
                }
                this.listenUri = new UriBuilder(PeerStrings.Scheme, listenUri.Host).Uri;
                this.port = factory.Port;
                this.maxReceivedMessageSize = factory.MaxReceivedMessageSize;
                this.resolver = factory.Resolver;
                this.securityManager = factory.SecurityManager;
                this.readerQuotas = new XmlDictionaryReaderQuotas();
                factory.ReaderQuotas.CopyTo(this.readerQuotas);
                this.maxBufferPoolSize = factory.MaxBufferPoolSize;
            }

            bool HasMismatchedReaderQuotas(XmlDictionaryReaderQuotas existingOne, XmlDictionaryReaderQuotas newOne, out string result)
            {
                //check for properties that affect the message
                result = null;
                if (existingOne.MaxArrayLength != newOne.MaxArrayLength)
                    result = PeerBindingPropertyNames.ReaderQuotasDotArrayLength;
                else if (existingOne.MaxStringContentLength != newOne.MaxStringContentLength)
                    result = PeerBindingPropertyNames.ReaderQuotasDotStringLength;
                else if (existingOne.MaxDepth != newOne.MaxDepth)
                    result = PeerBindingPropertyNames.ReaderQuotasDotMaxDepth;
                else if (existingOne.MaxNameTableCharCount != newOne.MaxNameTableCharCount)
                    result = PeerBindingPropertyNames.ReaderQuotasDotMaxCharCount;
                else if (existingOne.MaxBytesPerRead != newOne.MaxBytesPerRead)
                    result = PeerBindingPropertyNames.ReaderQuotasDotMaxBytesPerRead;
                return result != null;
            }

            public void CheckIfCompatible(PeerNodeImplementation peerNode, Uri via)
            {
                string mismatch = null;
                // test the settings that must be identical

                if (listenUri != peerNode.ListenUri)
                    mismatch = PeerBindingPropertyNames.ListenUri;
                else if (port != peerNode.Port)
                    mismatch = PeerBindingPropertyNames.Port;
                else if (maxReceivedMessageSize != peerNode.MaxReceivedMessageSize)
                    mismatch = PeerBindingPropertyNames.MaxReceivedMessageSize;
                else if (maxBufferPoolSize != peerNode.MaxBufferPoolSize)
                    mismatch = PeerBindingPropertyNames.MaxBufferPoolSize;
                else if (HasMismatchedReaderQuotas(peerNode.ReaderQuotas, readerQuotas, out mismatch))
                { }
                else if (resolver.GetType() != peerNode.Resolver.GetType())
                    mismatch = PeerBindingPropertyNames.Resolver;
                else if (!resolver.Equals(peerNode.Resolver))
                    mismatch = PeerBindingPropertyNames.ResolverSettings;
                else if (listenIPAddress != peerNode.ListenIPAddress)
                {
                    if ((listenIPAddress == null || peerNode.ListenIPAddress == null)
                        ||
                        (!listenIPAddress.Equals(peerNode.ListenIPAddress)))
                        mismatch = PeerBindingPropertyNames.ListenIPAddress;
                }
                else if ((securityManager == null) && (peerNode.SecurityManager != null))
                    mismatch = PeerBindingPropertyNames.Security;
                if (mismatch != null)
                    PeerExceptionHelper.ThrowInvalidOperation_PeerConflictingPeerNodeSettings(mismatch);
                securityManager.CheckIfCompatibleNodeSettings(peerNode.SecurityManager);
            }

            public PeerNodeImplementation CreatePeerNode()
            {
                PeerNodeImplementation peerNode = new PeerNodeImplementation();
                peerNode.ListenIPAddress = listenIPAddress;
                peerNode.ListenUri = listenUri;
                peerNode.MaxReceivedMessageSize = maxReceivedMessageSize;
                peerNode.Port = port;
                peerNode.Resolver = resolver;
                peerNode.SecurityManager = securityManager;
                this.readerQuotas.CopyTo(peerNode.readerQuotas);
                peerNode.MaxBufferPoolSize = maxBufferPoolSize;
                return peerNode;
            }
        }

        class SendAsyncResult : AsyncResult
        {
            bool floodComplete = false;
            bool localDispatchComplete = false;

            object thisLock = new object();
            object ThisLock { get { return thisLock; } }
            Exception floodException = null;

            public SendAsyncResult(AsyncCallback callback, object state) : base(callback, state) { }

            public void OnFloodComplete(IAsyncResult result)
            {
                if (this.floodComplete || this.IsCompleted)
                    return;

                bool complete = false;
                lock (this.ThisLock)
                {
                    if (this.localDispatchComplete)
                        complete = true;
                    this.floodComplete = true;
                }
                try
                {
                    PeerFlooder.EndFloodEncodedMessage(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e)) throw;
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    floodException = e;
                }
                if (complete)
                {
                    this.Complete(result.CompletedSynchronously, floodException);
                }
            }

            public void OnLocalDispatchComplete(IAsyncResult result)
            {
                SendAsyncResult sr = (SendAsyncResult)result;
                if (this.localDispatchComplete || this.IsCompleted)
                    return;

                bool complete = false;
                lock (this.ThisLock)
                {
                    if (this.floodComplete)
                        complete = true;
                    this.localDispatchComplete = true;
                }

                if (complete)
                {
                    this.Complete(true, floodException);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<SendAsyncResult>(result);
            }
        }

        bool IPeerNodeMessageHandling.HasMessagePropagation
        {
            get
            {
                return this.messagePropagationFilter != null;
            }
        }

        bool IPeerNodeMessageHandling.IsKnownVia(Uri via)
        {
            bool result = false;
            lock (ThisLock)
            {
                result = uri2SecurityProtocol.ContainsKey(via);
            }
            return result;
        }

        bool IPeerNodeMessageHandling.IsNotSeenBefore(Message message, out byte[] id, out int cacheMiss)
        {
            PeerFlooder lclFlooder = flooder;
            id = DefaultId;
            cacheMiss = -1;
            return (lclFlooder != null && lclFlooder.IsNotSeenBefore(message, out id, out cacheMiss));
        }

        public MessageEncodingBindingElement EncodingBindingElement
        {
            get
            {
                return this.EncodingElement;
            }
        }

    }

    interface IPeerNodeMessageHandling
    {
        void HandleIncomingMessage(MessageBuffer messageBuffer, PeerMessagePropagation propagateFlags, int index, MessageHeader header, Uri via, Uri to);
        PeerMessagePropagation DetermineMessagePropagation(Message message, PeerMessageOrigination origination);
        bool HasMessagePropagation { get; }
        bool ValidateIncomingMessage(ref Message data, Uri via);
        bool IsKnownVia(Uri via);
        bool IsNotSeenBefore(Message message, out byte[] id, out int cacheMiss);
        MessageEncodingBindingElement EncodingBindingElement { get; }
    }

}
