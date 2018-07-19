//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.PeerResolvers;
    using System.ServiceModel.Security;
    using System.Threading;
    using System.Xml;

    class PeerQuotaHelper
    {
        int enqueuedCount = 0;
        int quota = 64;
        AutoResetEvent waiter = new AutoResetEvent(false);

        public PeerQuotaHelper(int limit)
        {
            this.quota = limit;
        }

        public void ReadyToEnqueueItem()
        {
            int value = Interlocked.Increment(ref enqueuedCount);
            if (value > quota)
            {
                waiter.WaitOne();
            }
        }

        public void ItemDequeued()
        {
            int value = Interlocked.Decrement(ref enqueuedCount);
            Fx.Assert(value >= 0, "queue below empty");
            if (value >= quota)
            {
                waiter.Set();
            }
        }

    }

    class PeerNodeConfig
    {
        int connectTimeout;
        MessageEncoder encoder;
        PeerNodeAddress listenAddress;  // EPR + IP addresses
        IPAddress listenIPAddress;
        Uri listenUri;
        long maxReceivedMessageSize;
        int minNeighbors;                                             //Neighbor parameters
        int idealNeighbors;
        int maxNeighbors;
        int maxReferrals;
        int maxReferralCacheSize;
        int maxResolveAddresses;
        string meshId;
        PeerMessagePropagationFilter messagePropagationFilter;
        ulong nodeId;
        int port;
        PeerResolver resolver;
        int maintainerInterval;
        TimeSpan maintainerRetryInterval;
        TimeSpan maintainerTimeout;
        TimeSpan unregisterTimeout;
        PeerSecurityManager securityManager;
        int maxIncomingConcurrentCalls = 128;
        int maxConcurrentSessions = 64;
        XmlDictionaryReaderQuotas readerQuotas = new XmlDictionaryReaderQuotas();
        long maxBufferPoolSize;
        int maxSendQueueSize = 128;

        public PeerNodeConfig(string meshId, ulong nodeId,
            PeerResolver resolver,
            PeerMessagePropagationFilter messagePropagationFilter,
            MessageEncoder encoder,
            Uri listenUri, IPAddress listenIPAddress, int port,
            long maxReceivedMessageSize,
            int minNeighbors, int idealNeighbors, int maxNeighbors,
            int maxReferrals,
            int connectTimeout,
            int maintainerInterval,
            PeerSecurityManager securityManager,
            XmlDictionaryReaderQuotas readerQuotas,
            long maxBufferPool,
            int maxSendQueueSize,
            int maxReceiveQueueSize)
        {
            this.connectTimeout = connectTimeout;
            this.listenIPAddress = listenIPAddress;
            this.listenUri = listenUri;
            this.maxReceivedMessageSize = maxReceivedMessageSize;
            this.minNeighbors = minNeighbors;
            this.idealNeighbors = idealNeighbors;
            this.maxNeighbors = maxNeighbors;
            this.maxReferrals = maxReferrals;
            this.maxReferralCacheSize = PeerTransportConstants.MaxReferralCacheSize;
            this.maxResolveAddresses = PeerTransportConstants.MaxResolveAddresses;
            this.meshId = meshId;
            this.encoder = encoder;
            this.messagePropagationFilter = messagePropagationFilter;
            this.nodeId = nodeId;
            this.port = port;
            this.resolver = resolver;
            this.maintainerInterval = maintainerInterval;
            this.maintainerRetryInterval = new TimeSpan(PeerTransportConstants.MaintainerRetryInterval * 10000);
            this.maintainerTimeout = new TimeSpan(PeerTransportConstants.MaintainerTimeout * 10000);
            this.unregisterTimeout = new TimeSpan(PeerTransportConstants.UnregisterTimeout * 10000);
            this.securityManager = securityManager;
            readerQuotas.CopyTo(this.readerQuotas);
            this.maxBufferPoolSize = maxBufferPool;
            this.maxIncomingConcurrentCalls = maxReceiveQueueSize;
            this.maxSendQueueSize = maxSendQueueSize;
        }

        internal PeerSecurityManager SecurityManager
        {
            get { return securityManager; }
        }

        public int ConnectTimeout
        {
            get { return connectTimeout; }
        }

        public IPAddress ListenIPAddress
        {
            get { return listenIPAddress; }
        }

        public int ListenerPort
        {
            get { return listenAddress.EndpointAddress.Uri.Port; }
        }

        public Uri ListenUri
        {
            get { return listenUri; }
        }

        public int IdealNeighbors
        {
            get { return idealNeighbors; }
        }

        public int MaintainerInterval
        {
            get { return maintainerInterval; }
        }

        public TimeSpan MaintainerRetryInterval
        {
            get { return maintainerRetryInterval; }
        }

        public TimeSpan MaintainerTimeout
        {
            get { return maintainerTimeout; }
        }

        public long MaxBufferPoolSize
        {
            get { return maxBufferPoolSize; }
        }

        public long MaxReceivedMessageSize
        {
            get { return maxReceivedMessageSize; }
        }

        public int MaxNeighbors
        {
            get { return maxNeighbors; }
        }

        public int MaxReferrals
        {
            get { return maxReferrals; }
        }

        public int MaxReferralCacheSize
        {
            get { return maxReferralCacheSize; }
        }

        public int MaxResolveAddresses
        {
            get { return maxResolveAddresses; }
        }

        public int MaxPendingIncomingCalls
        {
            get { return this.maxIncomingConcurrentCalls; }
        }

        public int MaxPendingOutgoingCalls
        {
            get { return this.maxSendQueueSize; }
        }

        public int MaxConcurrentSessions
        {
            get { return this.maxConcurrentSessions; }
        }
        public int MinNeighbors
        {
            get { return minNeighbors; }
        }

        public string MeshId
        {
            get { return meshId; }
        }

        public MessageEncoder MessageEncoder
        {
            get { return encoder; }
        }

        public PeerMessagePropagationFilter MessagePropagationFilter
        {
            get { return messagePropagationFilter; }
        }

        public ulong NodeId
        {
            get { return nodeId; }
        }

        public int Port
        {
            get { return port; }
        }

        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get { return this.readerQuotas; }
        }

        public PeerResolver Resolver
        {
            get { return resolver; }
        }

        public TimeSpan UnregisterTimeout
        {
            get { return unregisterTimeout; }
        }

        // Returns the actual address that the node service is listening on.
        // If retrieving the address in order to send it over the wire, maskScopeId should be true 
        // (scope IDs are not sent over the wire).
        public PeerNodeAddress GetListenAddress(bool maskScopeId)
        {
            PeerNodeAddress localAddress = this.listenAddress;
            return new PeerNodeAddress(localAddress.EndpointAddress, PeerIPHelper.CloneAddresses(localAddress.IPAddresses, maskScopeId));
        }

        public void SetListenAddress(PeerNodeAddress address)
        {
            this.listenAddress = address;
        }

        static Uri BuildUri(string host, int port, Guid guid)
        {
            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Host = host;
            if (port > 0)
                uriBuilder.Port = port;
            uriBuilder.Path = PeerStrings.KnownServiceUriPrefix + '/' + guid;
            uriBuilder.Scheme = Uri.UriSchemeNetTcp;
            TcpChannelListener.FixIpv6Hostname(uriBuilder, uriBuilder.Uri);
            return uriBuilder.Uri;
        }

        public Uri GetSelfUri()
        {
            Uri uri = null;
            Guid serviceGuid = Guid.NewGuid();

            if (this.listenIPAddress == null)
            {
                uri = BuildUri(DnsCache.MachineName, port, serviceGuid);
            }
            else
            {
                uri = BuildUri(this.listenIPAddress.ToString(), port, serviceGuid);
            }
            return uri;
        }

        public Uri GetMeshUri()
        {
            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Host = this.meshId;
            uriBuilder.Scheme = PeerStrings.Scheme;
            return uriBuilder.Uri;
        }
    }

    static class PeerMessageHelpers
    {
        // delegates used to call the callback to process an incoming message
        public delegate void CleanupCallback(IPeerNeighbor neighbor, PeerCloseReason reason, Exception exception);

        public static string GetHeaderString(MessageHeaders headers, string name, string ns)
        {
            string result = null;
            int index = headers.FindHeader(name, ns);
            if (index >= 0)
            {
                using (XmlDictionaryReader reader = headers.GetReaderAtHeader(index))
                {
                    result = reader.ReadElementString();
                }
                headers.UnderstoodHeaders.Add(headers[index]);
            }
            return result;
        }

        public static Uri GetHeaderUri(MessageHeaders headers, string name, string ns)
        {
            Uri result = null;
            string rawString = GetHeaderString(headers, name, ns);
            if (rawString != null)
                result = new Uri(rawString);
            return result;
        }

        public static ulong GetHeaderULong(MessageHeaders headers, int index)
        {
            ulong result = PeerTransportConstants.MaxHopCount;
            if (index >= 0)
            {
                using (XmlDictionaryReader reader = headers.GetReaderAtHeader(index))
                {
                    result = XmlConvert.ToUInt64(reader.ReadElementString());
                }
                headers.UnderstoodHeaders.Add(headers[index]);
            }
            return result;
        }

        public static UniqueId GetHeaderUniqueId(MessageHeaders headers, string name, string ns)
        {
            UniqueId result = null;
            int index = headers.FindHeader(name, ns);
            if (index >= 0)
            {
                using (XmlDictionaryReader reader = headers.GetReaderAtHeader(index))
                {
                    result = reader.ReadElementContentAsUniqueId();
                }
                headers.UnderstoodHeaders.Add(headers[index]);
            }
            return result;
        }

    }

    static class PeerStrings
    {
        public static Dictionary<string, string> protocolActions;

        // Namespace for infrastructure messages
        public const string Namespace = "http://schemas.microsoft.com/net/2006/05/peer";

        // PeerService contract name
        public const string ServiceContractName = "PeerService";

        // Infrastructure message actions
        // PeerConnector
        public const string ConnectAction = Namespace + "/Connect";
        public const string WelcomeAction = Namespace + "/Welcome";
        public const string RefuseAction = Namespace + "/Refuse";
        public const string DisconnectAction = Namespace + "/Disconnect";

        // PeerFlooder
        public const string FloodAction = Namespace + "/Flood";
        public const string InternalFloodAction = Namespace + "/IntFlood";
        public const string LinkUtilityAction = Namespace + "/LinkUtility";
        public const string RequestSecurityTokenAction = "RequestSecurityToken";
        public const string RequestSecurityTokenResponseAction = "RequestSecurityTokenResponse";
        public const string HopCountElementName = "Hops";
        public const string HopCountElementNamespace = Namespace + "/HopCount";
        public const string PingAction = Namespace + "/Ping";

        // Uri
        public const string Scheme = "net.p2p";
        public const string KnownServiceUriPrefix = "PeerChannelEndpoints";
        public const string PeerCustomResolver = "PeerCustomResolver";

        public const string SkipLocalChannels = "SkipLocalChannels";
        public const string Via = "PeerVia";
        public const string MessageVerified = "MessageVerified";
        public const string CacheMiss = "CacheMiss";
        public const string PeerProperty = "PeerProperty";
        public const string MessageId = "MessageID";

        static PeerStrings()
        {
            protocolActions = new Dictionary<string, string>();
            PopulateProtocolActions();
        }

        static void PopulateProtocolActions()
        {
            protocolActions.Add(PeerStrings.ConnectAction, PeerOperationNames.Connect);
            protocolActions.Add(PeerStrings.WelcomeAction, PeerOperationNames.Welcome);
            protocolActions.Add(PeerStrings.RefuseAction, PeerOperationNames.Refuse);
            protocolActions.Add(PeerStrings.DisconnectAction, PeerOperationNames.Disconnect);
            protocolActions.Add(PeerStrings.RequestSecurityTokenAction, PeerOperationNames.ProcessRequestSecurityToken);
            protocolActions.Add(PeerStrings.RequestSecurityTokenResponseAction, PeerOperationNames.RequestSecurityTokenResponse);
            protocolActions.Add(PeerStrings.LinkUtilityAction, PeerOperationNames.LinkUtility);
            protocolActions.Add(Addressing10Strings.FaultAction, PeerOperationNames.Fault);
            protocolActions.Add(PeerStrings.PingAction, PeerOperationNames.Ping);
        }

        public static string FindAction(string action)
        {
            string result = null;
            protocolActions.TryGetValue(action, out result);
            return result;
        }


    }

    class PeerOperationNames
    {
        public const string Connect = "Connect";
        public const string Disconnect = "Disconnect";
        public const string Refuse = "Refuse";
        public const string Welcome = "Welcome";
        public const string LinkUtility = "LinkUtility";
        public const string ProcessRequestSecurityToken = "ProcessRequestSecurityToken";
        public const string RequestSecurityTokenResponse = "RequestSecurityTokenResponse";
        public const string Flood = "FloodMessage";
        public const string Demuxer = "PeerFlooder";
        public const string PeerVia = "PeerVia";
        public const string Fault = "Fault";
        public const string PeerTo = "PeerTo";
        public const string Ping = "Ping";

    }

    class PeerResolverStrings
    {
        public const string Namespace = PeerStrings.Namespace + "/resolver";
        public const string RegisterAction = Namespace + "/Register";
        public const string RegisterResponseAction = Namespace + "/RegisterResponse";
        public const string UnregisterAction = Namespace + "/Unregister";
        public const string ResolveAction = Namespace + "/Resolve";
        public const string ResolveResponseAction = Namespace + "/ResolveResponse";
        public const string UpdateAction = Namespace + "/Update";
        public const string UpdateResponseAction = Namespace + "/UpdateResponse";
        public const string RefreshAction = Namespace + "/Refresh";
        public const string RefreshResponseAction = Namespace + "/RefreshResponse";
        public const string GetServiceSettingsAction = Namespace + "/GetServiceSettings";
        public const string GetServiceSettingsResponseAction = Namespace + "/GetServiceSettingsResponse";
    }

    // constants used by more than one component
    static class PeerTransportConstants
    {
        public const int ConnectTimeout = 60 * 1000;            // 1 minute
        public const ulong InvalidNodeId = 0;
        public const int MinNeighbors = 2;
        public const int IdealNeighbors = 3;
        public const int MaxResolveAddresses = IdealNeighbors;  // We only to resolve Ideal connections

        public const int MaxNeighbors = 7;
        public const int MaxReferrals = 10;
        public const int MaxReferralCacheSize = 50;             // Cache no more than 50 referrals

        public const int MaintainerInterval = 5 * 60 * 1000;    // 5 Minutes
        public const int MaintainerRetryInterval = 10000;       // 10 seconds
        public const int MaintainerTimeout = 2 * 60 * 1000;     // 2 Minutes
        public const int UnregisterTimeout = 2 * 60 * 1000;     // 2 Minutes

        //how long do we want wait before sending each batch of acks?
        public const int AckTimeout = 30 * 1000;

        //how many acks do we want max in each ack message?
        public const uint AckWindow = 32;

        public const long MinMessageSize = 16384;

        public const int MinPort = IPEndPoint.MinPort;
        public const int MaxPort = IPEndPoint.MaxPort;
        public const ulong MaxHopCount = ulong.MaxValue;
        public static TimeSpan ForwardInterval = TimeSpan.FromSeconds(10);
        public static TimeSpan ForwardTimeout = TimeSpan.FromSeconds(60);
        public static int MaxOutgoingMessages = 128;
        public const int MessageThreshold = 32;
    }

    static class PeerValidateHelper
    {
        public static void ValidateListenIPAddress(IPAddress address)
        {
            // null is Okay
            if (address == null)
                return;

            // If an incorrect IP address is given throw.
            if (address.Equals(IPAddress.Any) ||
                address.Equals(IPAddress.IPv6Any) ||
                address.Equals(IPAddress.IPv6None) ||
                address.Equals(IPAddress.None) ||
                address.Equals(IPAddress.Broadcast) ||
                address.IsIPv6Multicast ||
                IPAddress.IsLoopback(address))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentException(SR.GetString(SR.PeerListenIPAddressInvalid, address), "address", null));
            }
        }

        public static void ValidateMaxMessageSize(long value)
        {
            if (value < PeerTransportConstants.MinMessageSize)
            {
                string message = SR.GetString(SR.ArgumentOutOfRange, PeerTransportConstants.MinMessageSize, long.MaxValue);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, message));
            }
        }

        public static void ValidatePort(int value)
        {
            if (value < PeerTransportConstants.MinPort || value > PeerTransportConstants.MaxPort)
            {
                string message = SR.GetString(SR.ArgumentOutOfRange, PeerTransportConstants.MinPort, PeerTransportConstants.MaxPort);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, message));
            }
        }

        public static bool ValidNodeAddress(PeerNodeAddress address)
        {
            return (address != null
                && address.EndpointAddress != null
                && address.EndpointAddress.Uri != null
                && address.IPAddresses != null
                && address.IPAddresses.Count > 0
                && string.Compare(address.EndpointAddress.Uri.Scheme, Uri.UriSchemeNetTcp, StringComparison.OrdinalIgnoreCase) == 0
                );
        }

        public static bool ValidReferralNodeAddress(PeerNodeAddress address)
        {
            bool valid = true;
            long scopeId = -1;

            foreach (IPAddress addr in address.IPAddresses)
            {
                if (addr.IsIPv6LinkLocal)
                {
                    if (scopeId == -1)
                    {
                        scopeId = addr.ScopeId;
                    }
                    else if (scopeId != addr.ScopeId)
                    {
                        valid = false;
                        break;
                    }
                }
            }
            return valid;
        }
    }

    // Indicates which side initiated neighbor close
    enum PeerCloseInitiator
    {
        LocalNode,
        RemoteNode
    }

    // Reason for closing a neighbor
    // WARNING: If you add another enum value, consider whether a corresponding value
    // should be added to PeerConnector.RefuseReason and PeerConnector.DisconnectReason.
    // WARNING: Upon adding a new enum value, consider if PeerCloseReasonHelper.IsDefined
    // should be updated.
    enum PeerCloseReason
    {
        None = 0,               // Reserved value - never serialized used internally.
        InvalidNeighbor,        // used when protocol violations are detected
        LeavingMesh,            // Closing because the node is leaving the mesh
        NotUsefulNeighbor,      // Closing because the neighbor is not useful
        DuplicateNeighbor,      // The node already has a neighbor session to this node
        DuplicateNodeId,        // The neighbor has the same node ID as the local node
        NodeBusy,               // The node has too many neighbor sessions to accept a new session
        ConnectTimedOut,        // Connect processing timedout
        Faulted,                // When neighbor faults
        Closed,                 // When neighbor closes without Disconnect or Refuse
        InternalFailure,        // Eg: an infrastructure msg send fails and requires closing the channel
        AuthenticationFailure,  // Eg, when in secure mode, wrong credentials. 
        NodeTooSlow,            //remote neighbor is too slow 
    }

    // Neighbor event args
    // EventArgs when a neighbor is closing or closed
    class PeerNeighborCloseEventArgs : EventArgs
    {
        PeerCloseInitiator closeInitiator;
        Exception exception;
        PeerCloseReason reason;

        public PeerNeighborCloseEventArgs(PeerCloseReason reason,
            PeerCloseInitiator closeInitiator, Exception exception)
        {
            this.reason = reason;
            this.closeInitiator = closeInitiator;
            this.exception = exception;
        }

        public PeerCloseInitiator CloseInitiator
        {
            get { return this.closeInitiator; }
        }

        public Exception Exception
        {
            get { return this.exception; }
        }

        public PeerCloseReason Reason
        {
            get { return this.reason; }
        }
    }

    class PeerExceptionHelper
    {
        static internal void ThrowInvalidOperation_InsufficientCryptoSupport(Exception innerException)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.InsufficientCryptoSupport), innerException));
        }
        static internal void ThrowArgument_InsufficientCredentials(string property)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.InsufficientCredentials, property)));
        }
        static internal void ThrowArgumentOutOfRange_InvalidTransportCredentialType(int value)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("CredentialType", value,
                SR.GetString(SR.ValueMustBeInRange, PeerTransportCredentialType.Password, PeerTransportCredentialType.Certificate)));
        }

        static internal void ThrowArgumentOutOfRange_InvalidSecurityMode(int value)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("Mode", value,
                SR.GetString(SR.ValueMustBeInRange, SecurityMode.None, SecurityMode.TransportWithMessageCredential)));
        }

        static internal void ThrowInvalidOperation_UnexpectedSecurityTokensDuringHandshake()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnexpectedSecurityTokensDuringHandshake)));
        }

        static internal void ThrowArgument_PnrpAddressesExceedLimit()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.PnrpAddressesExceedLimit)));
        }
        static internal void ThrowInvalidOperation_PnrpNoClouds()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.PnrpNoClouds)));
        }
        static internal void ThrowInvalidOperation_PnrpAddressesUnsupported()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.PnrpAddressesUnsupported)));
        }
        static internal void ThrowArgument_InsufficientResolverSettings()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.InsufficientResolverSettings)));
        }
        static internal void ThrowArgument_MustOverrideInitialize()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MustOverrideInitialize)));
        }
        static internal void ThrowArgument_InvalidResolverMode(PeerResolverMode mode)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.InvalidResolverMode, mode)));
        }

        static internal void ThrowInvalidOperation_NotValidWhenOpen(string operation)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.NotValidWhenOpen, operation)));
        }

        static internal void ThrowInvalidOperation_NotValidWhenClosed(string operation)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.NotValidWhenClosed, operation)));
        }

        static internal void ThrowInvalidOperation_DuplicatePeerRegistration(string servicepath)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.DuplicatePeerRegistration, servicepath)));
        }
        static internal void ThrowPnrpError(int errorCode, string cloud)
        {
            ThrowPnrpError(errorCode, cloud, true);
        }
        static internal void ThrowPnrpError(int errorCode, string cloud, bool trace)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new System.ServiceModel.Channels.PnrpPeerResolver.PnrpException(errorCode, cloud), trace ? TraceEventType.Error : TraceEventType.Information);
        }

        static internal void ThrowInvalidOperation_PeerConflictingPeerNodeSettings(string propertyName)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.PeerConflictingPeerNodeSettings, propertyName)));
        }

        static internal void ThrowInvalidOperation_PeerCertGenFailure(Exception innerException)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.PeerCertGenFailure), innerException));
        }
        static internal void ThrowInvalidOperation_ConflictingHeader(string headerName)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.PeerConflictingHeader, headerName, PeerStrings.Namespace)));
        }

        public static Exception GetLastException()
        {
            return new Win32Exception(Marshal.GetLastWin32Error());
        }

    }

    class PeerBindingPropertyNames
    {
        public static readonly string ListenUri = "ListenUri";
        public static readonly string Port = "Port";
        public static readonly string MaxReceivedMessageSize = "MaxReceivedMessageSize";
        public static readonly string Resolver = "Resolver";
        public static readonly string Security = "Security";
        public static readonly string SecurityDotMode = "Security.Mode";
        public static readonly string ListenIPAddress = "ListenIPAddress";
        public static readonly string Credentials = "Credentials";
        public static readonly string ResolverSettings = "ResolverSettings";
        public static readonly string Password = "Password";
        public static readonly string Certificate = "Certificate";
        public static readonly string MaxBufferPoolSize = "MaxBufferPoolSize";
        public static readonly string ReaderQuotasDotArrayLength = "ReaderQuotas.MaxArrayLength";
        public static readonly string ReaderQuotasDotStringLength = "ReaderQuotas.MaxStringContentLength";
        public static readonly string ReaderQuotasDotMaxDepth = "ReaderQuotas.MaxDepth";
        public static readonly string ReaderQuotasDotMaxCharCount = "ReaderQuotas.MaxNameTableCharCount";
        public static readonly string ReaderQuotasDotMaxBytesPerRead = "ReaderQuotas.MaxBytesPerRead";
    }

    class PeerPropertyNames
    {
        public static readonly string MessageSenderAuthentication = "Credentials.Peer.MessageSenderAuthentication";
        public static readonly string Credentials = "SecurityCredentialsManager";
        public static readonly string Password = "Credentials.Peer.MeshPassword";
        public static readonly string Certificate = "Credentials.Peer.Certificate";
        public static readonly string PeerAuthentication = "Credentials.Peer.PeerAuthentication";
    }

    class OperationSelector : IDispatchOperationSelector
    {
        IPeerNodeMessageHandling messageHandler;

        public OperationSelector(IPeerNodeMessageHandling messageHandler)
        {
            this.messageHandler = messageHandler;
        }

        public static void TurnOffSecurityHeader(Message message)
        {
            int i = message.Headers.FindHeader(SecurityJan2004Strings.Security, SecurityJan2004Strings.Namespace);
            if (i >= 0)
            {
                message.Headers.AddUnderstood(i);
            }
        }

        public string SelectOperation(ref Message message)
        {

            string action = message.Headers.Action;
            string demux = null;
            byte[] id = PeerNodeImplementation.DefaultId;
            string operation = PeerStrings.FindAction(action);
            Uri via = null;
            Uri to = null;
            bool skipped = false;
            PeerMessageProperty peerProperty = new PeerMessageProperty();

            if (operation != null)
                return operation;
            try
            {
                demux = PeerMessageHelpers.GetHeaderString(message.Headers, PeerOperationNames.Flood, PeerStrings.Namespace);
                via = PeerMessageHelpers.GetHeaderUri(message.Headers, PeerStrings.Via, PeerStrings.Namespace);
                to = PeerMessageHelpers.GetHeaderUri(message.Headers, PeerOperationNames.PeerTo, PeerStrings.Namespace);
            }
            catch (MessageHeaderException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
                return PeerOperationNames.Fault;
            }
            catch (SerializationException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
                return PeerOperationNames.Fault;
            }
            catch (XmlException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
                return PeerOperationNames.Fault;
            }
            peerProperty.PeerVia = via;
            peerProperty.PeerTo = to;
            message.Properties.Add(PeerStrings.PeerProperty, peerProperty);
            if (demux == PeerOperationNames.Demuxer)
            {
                try
                {
                    if (!this.messageHandler.ValidateIncomingMessage(ref message, via))
                    {
                        peerProperty.SkipLocalChannels = true;
                        skipped = true;
                        TurnOffSecurityHeader(message);
                    }
                    if (this.messageHandler.IsNotSeenBefore(message, out id, out peerProperty.CacheMiss))
                    {
                        peerProperty.MessageVerified = true;
                    }
                    else
                    {
                        if (!skipped)
                        {
                            peerProperty.SkipLocalChannels = true;
                        }
                    }
                    //means that the message doesnt contain legal id
                    if (id == PeerNodeImplementation.DefaultId)
                        return PeerOperationNames.Fault;
                }
                catch (MessageHeaderException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
                    return PeerOperationNames.Fault;
                }
                catch (SerializationException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
                    return PeerOperationNames.Fault;
                }
                catch (XmlException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
                    return PeerOperationNames.Fault;
                }
                catch (MessageSecurityException e)
                {
                    if (!e.ReplayDetected)
                        return PeerOperationNames.Fault;
                    else
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                return PeerOperationNames.Flood;
            }
            else
                return null;
        }
    }

    internal class PeerOperationSelectorBehavior : IContractBehavior
    {
        IPeerNodeMessageHandling messageHandler;

        internal PeerOperationSelectorBehavior(IPeerNodeMessageHandling messageHandler)
        {
            this.messageHandler = messageHandler;
        }

        void IContractBehavior.AddBindingParameters(ContractDescription description, ServiceEndpoint endpoint, BindingParameterCollection parameters)
        {
        }

        void IContractBehavior.Validate(ContractDescription description, ServiceEndpoint endpoint)
        {
        }

        void IContractBehavior.ApplyDispatchBehavior(ContractDescription description, ServiceEndpoint endpoint, DispatchRuntime
        dispatch)
        {
            dispatch.OperationSelector = new OperationSelector(this.messageHandler);

            if (dispatch.ClientRuntime != null)
            {
                dispatch.ClientRuntime.OperationSelector = new OperationSelectorBehavior.MethodInfoOperationSelector(description, MessageDirection.Output);
            }
        }

        void IContractBehavior.ApplyClientBehavior(ContractDescription description, ServiceEndpoint endpoint, ClientRuntime proxy)
        {
            proxy.OperationSelector = new OperationSelectorBehavior.MethodInfoOperationSelector(description, MessageDirection.Input);
            proxy.CallbackDispatchRuntime.OperationSelector = new OperationSelector(this.messageHandler);
        }

    }

    class PeerDictionaryHeader : DictionaryHeader
    {
        string value;
        XmlDictionaryString name;
        XmlDictionaryString nameSpace;


        public override XmlDictionaryString DictionaryName
        {
            get { return name; }
        }

        public override XmlDictionaryString DictionaryNamespace
        {
            get { return nameSpace; }
        }

        public PeerDictionaryHeader(XmlDictionaryString name, XmlDictionaryString nameSpace, string value)
        {
            this.name = name;
            this.nameSpace = nameSpace;
            this.value = value;
        }



        public PeerDictionaryHeader(XmlDictionaryString name, XmlDictionaryString nameSpace, XmlDictionaryString value)
        {
            this.name = name;
            this.nameSpace = nameSpace;
            this.value = value.Value;
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            writer.WriteString(this.value);
        }

        static internal PeerDictionaryHeader CreateHopCountHeader(ulong hopcount)
        {
            return new PeerDictionaryHeader(XD.PeerWireStringsDictionary.HopCount, XD.PeerWireStringsDictionary.HopCountNamespace, hopcount.ToString(CultureInfo.InvariantCulture));
        }

        static internal PeerDictionaryHeader CreateViaHeader(Uri via)
        {
            return new PeerDictionaryHeader(XD.PeerWireStringsDictionary.PeerVia, XD.PeerWireStringsDictionary.Namespace, via.ToString());
        }

        static internal PeerDictionaryHeader CreateFloodRole()
        {
            return new PeerDictionaryHeader(XD.PeerWireStringsDictionary.FloodAction, XD.PeerWireStringsDictionary.Namespace, XD.PeerWireStringsDictionary.Demuxer);
        }

        static internal PeerDictionaryHeader CreateToHeader(Uri to)
        {
            return new PeerDictionaryHeader(XD.PeerWireStringsDictionary.PeerTo, XD.PeerWireStringsDictionary.Namespace, to.ToString());
        }
        static internal PeerDictionaryHeader CreateMessageIdHeader(System.Xml.UniqueId messageId)
        {
            return new PeerDictionaryHeader(XD.AddressingDictionary.MessageId, XD.PeerWireStringsDictionary.Namespace, messageId.ToString());
        }

    }

    class PeerMessageProperty
    {

        public bool MessageVerified;
        public bool SkipLocalChannels;
        public Uri PeerVia;
        public Uri PeerTo;
        public int CacheMiss;
    }
}

