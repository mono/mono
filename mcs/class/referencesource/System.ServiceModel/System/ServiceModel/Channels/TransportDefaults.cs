//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Net;
    using System.Net.Security;
    using System.Net.WebSockets;
    using System.Runtime;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.Text;
    using System.Xml;

    internal static class TransactionFlowDefaults
    {
        internal const TransactionFlowOption IssuedTokens = TransactionFlowOption.NotAllowed;
        internal const bool Transactions = false;
        internal static TransactionProtocol TransactionProtocol = System.ServiceModel.TransactionProtocol.OleTransactions;
        internal const string TransactionProtocolString = System.ServiceModel.Configuration.ConfigurationStrings.OleTransactions;
    }

    static class EncoderDefaults
    {
        internal const int MaxReadPoolSize = 64;
        internal const int MaxWritePoolSize = 16;

        internal const int MaxDepth = 32;
        internal const int MaxStringContentLength = 8192;
        internal const int MaxArrayLength = 16384;
        internal const int MaxBytesPerRead = 4096;
        internal const int MaxNameTableCharCount = 16384;

        internal const int BufferedReadDefaultMaxDepth = 128;
        internal const int BufferedReadDefaultMaxStringContentLength = Int32.MaxValue;
        internal const int BufferedReadDefaultMaxArrayLength = Int32.MaxValue;
        internal const int BufferedReadDefaultMaxBytesPerRead = Int32.MaxValue;
        internal const int BufferedReadDefaultMaxNameTableCharCount = Int32.MaxValue;

        internal const CompressionFormat DefaultCompressionFormat = CompressionFormat.None;

        internal static readonly XmlDictionaryReaderQuotas ReaderQuotas = new XmlDictionaryReaderQuotas();

        internal static bool IsDefaultReaderQuotas(XmlDictionaryReaderQuotas quotas)
        {
            return quotas.ModifiedQuotas == 0x00;
        }
    }

    static class TextEncoderDefaults
    {
        internal static readonly Encoding Encoding = Encoding.GetEncoding(TextEncoderDefaults.EncodingString, new EncoderExceptionFallback(), new DecoderExceptionFallback());
        internal const string EncodingString = "utf-8";
        internal static readonly Encoding[] SupportedEncodings = new Encoding[] { Encoding.UTF8, Encoding.Unicode, Encoding.BigEndianUnicode };
        internal const string MessageVersionString = System.ServiceModel.Configuration.ConfigurationStrings.Soap12WSAddressing10;
        internal static readonly CharSetEncoding[] CharSetEncodings = new CharSetEncoding[]
        {
            new CharSetEncoding("utf-8", Encoding.UTF8),
            new CharSetEncoding("utf-16LE", Encoding.Unicode),
            new CharSetEncoding("utf-16BE", Encoding.BigEndianUnicode),
            new CharSetEncoding("utf-16", null),   // Ignore.  Ambiguous charSet, so autodetect.
            new CharSetEncoding(null, null),       // CharSet omitted, so autodetect.
        };

        internal static void ValidateEncoding(Encoding encoding)
        {
            string charSet = encoding.WebName;
            Encoding[] supportedEncodings = SupportedEncodings;
            for (int i = 0; i < supportedEncodings.Length; i++)
            {
                if (charSet == supportedEncodings[i].WebName)
                    return;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MessageTextEncodingNotSupported, charSet), "encoding"));
        }

        internal static string EncodingToCharSet(Encoding encoding)
        {
            string webName = encoding.WebName;
            CharSetEncoding[] charSetEncodings = CharSetEncodings;
            for (int i = 0; i < charSetEncodings.Length; i++)
            {
                Encoding enc = charSetEncodings[i].Encoding;
                if (enc == null)
                    continue;

                if (enc.WebName == webName)
                    return charSetEncodings[i].CharSet;
            }
            return null;
        }

        internal static bool TryGetEncoding(string charSet, out Encoding encoding)
        {
            CharSetEncoding[] charSetEncodings = CharSetEncodings;

            // Quick check for exact equality
            for (int i = 0; i < charSetEncodings.Length; i++)
            {
                if (charSetEncodings[i].CharSet == charSet)
                {
                    encoding = charSetEncodings[i].Encoding;
                    return true;
                }
            }

            // Check for case insensative match
            for (int i = 0; i < charSetEncodings.Length; i++)
            {
                string compare = charSetEncodings[i].CharSet;
                if (compare == null)
                    continue;

                if (compare.Equals(charSet, StringComparison.OrdinalIgnoreCase))
                {
                    encoding = charSetEncodings[i].Encoding;
                    return true;
                }
            }

            encoding = null;
            return false;
        }

        internal class CharSetEncoding
        {
            internal string CharSet;
            internal Encoding Encoding;

            internal CharSetEncoding(string charSet, Encoding enc)
            {
                CharSet = charSet;
                Encoding = enc;
            }
        }
    }

    static class MtomEncoderDefaults
    {
        internal const int MaxBufferSize = 65536;
    }

    static class BinaryEncoderDefaults
    {
        internal static EnvelopeVersion EnvelopeVersion { get { return EnvelopeVersion.Soap12; } }
        internal static BinaryVersion BinaryVersion { get { return BinaryVersion.Version1; } }
        internal const int MaxSessionSize = 2048;
    }

    static class MsmqDefaults
    {
        internal const MessageCredentialType DefaultClientCredentialType = MessageCredentialType.Windows;
        internal const Uri CustomDeadLetterQueue = null;
        internal const DeadLetterQueue DeadLetterQueue = System.ServiceModel.DeadLetterQueue.System;
        internal const bool Durable = true;
        internal const bool ExactlyOnce = true;
        internal const bool ReceiveContextEnabled = true;
        internal const int MaxRetryCycles = 2;
        internal const int MaxPoolSize = 8;
        internal const MsmqAuthenticationMode MsmqAuthenticationMode = System.ServiceModel.MsmqAuthenticationMode.WindowsDomain;
        internal const MsmqEncryptionAlgorithm MsmqEncryptionAlgorithm = System.ServiceModel.MsmqEncryptionAlgorithm.RC4Stream;
        internal const MsmqSecureHashAlgorithm MsmqSecureHashAlgorithm = System.ServiceModel.MsmqSecureHashAlgorithm.Sha1;
        internal const ProtectionLevel MsmqProtectionLevel = ProtectionLevel.Sign;
        internal const ReceiveErrorHandling ReceiveErrorHandling = System.ServiceModel.ReceiveErrorHandling.Fault;
        internal const int ReceiveRetryCount = 5;
        internal const QueueTransferProtocol QueueTransferProtocol = System.ServiceModel.QueueTransferProtocol.Native;
        internal static TimeSpan RetryCycleDelay { get { return TimeSpanHelper.FromMinutes(30, MsmqDefaults.RetryCycleDelayString); } }
        internal const string RetryCycleDelayString = "00:30:00";
        internal static TimeSpan TimeToLive { get { return TimeSpanHelper.FromDays(1, MsmqDefaults.TimeToLiveString); } }
        internal const string TimeToLiveString = "1.00:00:00";
        internal const bool UseActiveDirectory = false;
        internal const bool UseSourceJournal = false;
        internal const bool UseMsmqTracing = false;
        internal static TimeSpan ValidityDuration { get { return TimeSpanHelper.FromMinutes(5, MsmqDefaults.ValidityDurationString); } }
        internal const string ValidityDurationString = "00:05:00";
        internal static SecurityAlgorithmSuite MessageSecurityAlgorithmSuite
        {
            get { return SecurityAlgorithmSuite.Default; }
        }
    }

    static class MsmqIntegrationDefaults
    {
        internal const System.ServiceModel.MsmqIntegration.MsmqMessageSerializationFormat SerializationFormat =
            System.ServiceModel.MsmqIntegration.MsmqMessageSerializationFormat.Xml;
    }

    static class TransportDefaults
    {
        internal const bool ExtractGroupsForWindowsAccounts = SspiSecurityTokenProvider.DefaultExtractWindowsGroupClaims;
        internal const HostNameComparisonMode HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.Exact;
        internal const TokenImpersonationLevel ImpersonationLevel = TokenImpersonationLevel.Identification;
        internal const bool ManualAddressing = false;
        internal const long MaxReceivedMessageSize = 65536;
        internal const int MaxDrainSize = (int)MaxReceivedMessageSize;
        internal const long MaxBufferPoolSize = 512 * 1024;
        internal const int MaxBufferSize = (int)MaxReceivedMessageSize;
        internal const bool RequireClientCertificate = false;
        internal const int MaxFaultSize = MaxBufferSize;
        internal const int MaxSecurityFaultSize = 16384;

        // Calling CreateFault on an incoming message can expose some DoS-related security 
        // vulnerabilities when a service is in streaming mode.  See MB 47592 for more details. 
        // The RM protocol service does not use streaming mode on any of its bindings, so the
        // message we have in hand has already passed the binding’s MaxReceivedMessageSize check.
        // Custom transports can use RM so int.MaxValue is dangerous.
        internal const int MaxRMFaultSize = (int)MaxReceivedMessageSize;

        internal static MessageEncoderFactory GetDefaultMessageEncoderFactory()
        {
            return new BinaryMessageEncodingBindingElement().CreateMessageEncoderFactory();
        }
    }

    static class ConnectionOrientedTransportDefaults
    {
        internal const bool AllowNtlm = SspiSecurityTokenProvider.DefaultAllowNtlm;
        internal const int ConnectionBufferSize = 8192;
        internal const string ConnectionPoolGroupName = "default";
        internal const HostNameComparisonMode HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
        internal static TimeSpan IdleTimeout { get { return TimeSpanHelper.FromMinutes(2, IdleTimeoutString); } }
        internal const string IdleTimeoutString = "00:02:00";
        internal static TimeSpan ChannelInitializationTimeout { get { return TimeSpanHelper.FromSeconds(30, ChannelInitializationTimeoutString); } }
        internal const string ChannelInitializationTimeoutString = "00:00:30";
        internal const int MaxContentTypeSize = 256;
        internal const int MaxOutboundConnectionsPerEndpoint = 10;
        internal const int MaxPendingConnectionsConst = 0;
        internal static TimeSpan MaxOutputDelay { get { return TimeSpanHelper.FromMilliseconds(200, MaxOutputDelayString); } }
        internal const string MaxOutputDelayString = "00:00:00.2";
        internal const int MaxPendingAcceptsConst = 0;
        internal const int MaxViaSize = 2048;
        internal const ProtectionLevel ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
        internal const TransferMode TransferMode = System.ServiceModel.TransferMode.Buffered;

        private const int MaxPendingConnectionsPre45 = 10;
        private const int MaxPendingAcceptsPre45 = 1;

        internal static int GetMaxConnections()
        {
            return GetMaxPendingConnections();
        }

        internal static int GetMaxPendingConnections()
        {
            // OSEnvironmentHelper.IsApplicationTargeting45 checks for whether target Fx version is >= 4.5 despite its name
            if (OSEnvironmentHelper.IsApplicationTargeting45)
            {
                return 12 * OSEnvironmentHelper.ProcessorCount;
            }

            return MaxPendingConnectionsPre45;
        }

        internal static int GetMaxPendingAccepts()
        {
            // OSEnvironmentHelper.IsApplicationTargeting45 checks for whether target Fx version is >= 4.5 despite its name
            if (OSEnvironmentHelper.IsApplicationTargeting45)
            {
                return 2 * OSEnvironmentHelper.ProcessorCount;
            }

            return MaxPendingAcceptsPre45;
        }
    }

    static class TcpTransportDefaults
    {
        internal const int ListenBacklogConst = 0;
        internal static TimeSpan ConnectionLeaseTimeout { get { return TimeSpanHelper.FromMinutes(5, TcpTransportDefaults.ConnectionLeaseTimeoutString); } }
        internal const string ConnectionLeaseTimeoutString = "00:05:00";
        internal const bool PortSharingEnabled = false;
        internal const bool TeredoEnabled = false;
        
        private const int ListenBacklogPre45 = 10;

        internal static int GetListenBacklog()
        {
            // OSEnvironmentHelper.IsApplicationTargeting45 checks for whether target Fx version is >= 4.5 despite its name
            if (OSEnvironmentHelper.IsApplicationTargeting45)
            {
                return 12 * OSEnvironmentHelper.ProcessorCount;
            }

            return ListenBacklogPre45;
        }
    }

    static class ApplicationContainerSettingsDefaults
    {
        internal const string CurrentUserSessionDefaultString = "CurrentSession";
        internal const string Session0ServiceSessionString = "ServiceSession";
        internal const string PackageFullNameDefaultString = null;

        /// <summary>
        /// The current session will be used for resource lookup.
        /// </summary>
        internal const int CurrentSession = -1;

        /// <summary>
        /// Session 0 is the NT Service session
        /// </summary>
        internal const int ServiceSession = 0;
    }

    static class HttpTransportDefaults
    {
        internal const bool AllowCookies = false;
        internal const AuthenticationSchemes AuthenticationScheme = AuthenticationSchemes.Anonymous;
        internal const bool BypassProxyOnLocal = false;
        internal const bool DecompressionEnabled = true;
        internal const HostNameComparisonMode HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
        internal const bool KeepAliveEnabled = true;
        internal const Uri ProxyAddress = null;
        internal const AuthenticationSchemes ProxyAuthenticationScheme = AuthenticationSchemes.Anonymous;
        internal const string Realm = "";
        internal const TransferMode TransferMode = System.ServiceModel.TransferMode.Buffered;
        internal const bool UnsafeConnectionNtlmAuthentication = false;
        internal const bool UseDefaultWebProxy = true;
        internal const string UpgradeHeader = "Upgrade";
        internal const string ConnectionHeader = "Connection";
        internal const HttpMessageHandlerFactory MessageHandlerFactory = null;

        internal static TimeSpan RequestInitializationTimeout { get { return TimeSpanHelper.FromMilliseconds(0, RequestInitializationTimeoutString); } }
        internal const string RequestInitializationTimeoutString = "00:00:00";

        // We use 0 as the default value of the MaxPendingAccepts property on HttpTransportBindingElement. In 4.5 we always
        // use 10 under the hood if the default value is picked. In future releases, we could adjust the underlying default
        // value when we have the dynamic expending pattern of BeginGetContext call implemented and the heap fragmentation issue
        // from NCL layer solved.
        const int PendingAcceptsConstant = 10;
        internal const int DefaultMaxPendingAccepts = 0;
        internal const int MaxPendingAcceptsUpperLimit = 100000;
        internal static int GetEffectiveMaxPendingAccepts(int maxPendingAccepts)
        {
            return maxPendingAccepts == HttpTransportDefaults.DefaultMaxPendingAccepts ?
                                        PendingAcceptsConstant :
                                        maxPendingAccepts;
        }


        internal static WebSocketTransportSettings GetDefaultWebSocketTransportSettings()
        {
            return new WebSocketTransportSettings();
        }

        internal static MessageEncoderFactory GetDefaultMessageEncoderFactory()
        {
            return new TextMessageEncoderFactory(MessageVersion.Default, TextEncoderDefaults.Encoding, EncoderDefaults.MaxReadPoolSize, EncoderDefaults.MaxWritePoolSize, EncoderDefaults.ReaderQuotas);
        }

        internal static SecurityAlgorithmSuite MessageSecurityAlgorithmSuite
        {
            get { return SecurityAlgorithmSuite.Default; }
        }
    }

    static class NetTcpDefaults
    {
        internal const MessageCredentialType MessageSecurityClientCredentialType = MessageCredentialType.Windows;
        internal const bool TransactionsEnabled = false;

        internal static TransactionProtocol TransactionProtocol
        {
            get { return TransactionProtocol.Default; }
        }

        internal static SecurityAlgorithmSuite MessageSecurityAlgorithmSuite
        {
            get { return SecurityAlgorithmSuite.Default; }
        }
    }

    static class NetHttpDefaults
    {
        internal static TransactionProtocol TransactionProtocol
        {
            get { return TransactionProtocol.Default; }
        }
    }

    static class PeerTransportDefaults
    {
        internal const IPAddress ListenIPAddress = null;
        internal const int Port = 0;
        internal const string ResolverTypeString = null;
        internal const PeerAuthenticationMode PeerNodeAuthenticationMode = PeerAuthenticationMode.Password;
        internal const bool MessageAuthentication = false;

        internal static bool ResolverAvailable
        {
            get { return PnrpPeerResolver.IsPnrpAvailable; }
        }

        internal static bool ResolverInstalled
        {
            get { return PnrpPeerResolver.IsPnrpInstalled; }
        }

        internal static Type ResolverType
        {
            get { return typeof(PnrpPeerResolver); }
        }

        internal static Type ResolverBindingElementType
        {
            get { return typeof(PnrpPeerResolverBindingElement); }
        }

        internal static PeerResolver CreateResolver()
        {
            return new PnrpPeerResolver();
        }

    }

    static class OneWayDefaults
    {
        internal static TimeSpan IdleTimeout { get { return TimeSpanHelper.FromMinutes(2, IdleTimeoutString); } }
        internal const string IdleTimeoutString = "00:02:00";
        internal const int MaxOutboundChannelsPerEndpoint = 10;
        internal static TimeSpan LeaseTimeout { get { return TimeSpanHelper.FromMinutes(10, LeaseTimeoutString); } }
        internal const string LeaseTimeoutString = "00:10:00";
        internal const int MaxAcceptedChannels = 10;
        internal const bool PacketRoutable = false;
    }

    static class ReliableSessionDefaults
    {
        internal const string AcknowledgementIntervalString = "00:00:00.2";
        internal static TimeSpan AcknowledgementInterval { get { return TimeSpanHelper.FromMilliseconds(200, AcknowledgementIntervalString); } }
        internal const bool Enabled = false;
        internal const bool FlowControlEnabled = true;
        internal const string InactivityTimeoutString = "00:10:00";
        internal static TimeSpan InactivityTimeout { get { return TimeSpanHelper.FromMinutes(10, InactivityTimeoutString); } }
        internal const int MaxPendingChannels = 4;
        internal const int MaxRetryCount = 8;
        internal const int MaxTransferWindowSize = 8;
        internal const bool Ordered = true;
        internal static ReliableMessagingVersion ReliableMessagingVersion { get { return System.ServiceModel.ReliableMessagingVersion.WSReliableMessagingFebruary2005; } }
        internal const string ReliableMessagingVersionString = System.ServiceModel.Configuration.ConfigurationStrings.WSReliableMessagingFebruary2005;
    }

    static class BasicHttpBindingDefaults
    {
        internal const BasicHttpMessageCredentialType MessageSecurityClientCredentialType = BasicHttpMessageCredentialType.UserName;
        internal const WSMessageEncoding MessageEncoding = WSMessageEncoding.Text;
        internal const TransferMode TransferMode = System.ServiceModel.TransferMode.Buffered;
        internal static Encoding TextEncoding
        {
            get { return TextEncoderDefaults.Encoding; }
        }
    }

    static class WSHttpBindingDefaults
    {
        internal const WSMessageEncoding MessageEncoding = WSMessageEncoding.Text;
    }

    static class WSDualHttpBindingDefaults
    {
        internal const WSMessageEncoding MessageEncoding = WSMessageEncoding.Text;
    }

    static class WebSocketDefaults
    {
        internal const WebSocketTransportUsage TransportUsage = WebSocketTransportUsage.Never;
        internal const bool CreateNotificationOnConnection = false;
        internal const string DefaultKeepAliveIntervalString = "00:00:00";
        internal static readonly TimeSpan DefaultKeepAliveInterval = TimeSpanHelper.FromSeconds(0, DefaultKeepAliveIntervalString);

        internal const int BufferSize = 16 * 1024;
        internal const int MinReceiveBufferSize = 256;
        internal const int MinSendBufferSize = 16;
        internal const bool DisablePayloadMasking = false;
        internal const WebSocketMessageType DefaultWebSocketMessageType = WebSocketMessageType.Binary;
        internal const string SubProtocol = null;

        internal const int DefaultMaxPendingConnections = 0;
        // We set this number larger than that in TCP transport because in WebSocket cases, the connection is already authenticated
        // after we create the half-open channel. The default value is set as the default one as MaxConcurrentSessions to make it work
        // well in burst scenarios.
        internal static readonly int MaxPendingConnectionsCpuCount = ServiceThrottle.DefaultMaxConcurrentSessionsCpuCount;

        internal const string WebSocketConnectionHeaderValue = "Upgrade";
        internal const string WebSocketUpgradeHeaderValue = "websocket";

        [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.Globalization, "CA1303",
                            Justification = "These strings don't need to be localized.")]
        static WebSocketDefaults()
        {
        }
    }

    static class NetHttpBindingDefaults
    {
        internal const NetHttpMessageEncoding MessageEncoding = NetHttpMessageEncoding.Binary;
        internal const WebSocketTransportUsage TransportUsage = WebSocketTransportUsage.WhenDuplex;
    }
}
