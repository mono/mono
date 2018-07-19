//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Net.Sockets;

    [Fx.Tag.XamlVisible(false)]
    public class UdpDiscoveryEndpoint : DiscoveryEndpoint
    {
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        [SuppressMessage(FxCop.Category.Naming, "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Pv", Justification = "IPv4 is valid.")]
        public static readonly Uri DefaultIPv4MulticastAddress = DiscoveryDefaults.Udp.IPv4MulticastAddress;

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        [SuppressMessage(FxCop.Category.Naming, "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Pv", Justification = "IPv6 is valid.")]
        public static readonly Uri DefaultIPv6MulticastAddress = DiscoveryDefaults.Udp.IPv6MulticastAddress;

        DiscoveryViaBehavior viaBehavior;
        UdpTransportSettings udpTransportSettings;

        public UdpDiscoveryEndpoint()
            : this(GetDefaultMulticastAddress())
        {
        }

        public UdpDiscoveryEndpoint(string multicastAddress)
            : this(new Uri(multicastAddress))
        {
        }

        public UdpDiscoveryEndpoint(Uri multicastAddress)
            : this(DiscoveryVersion.DefaultDiscoveryVersion, multicastAddress)
        {
        }

        public UdpDiscoveryEndpoint(DiscoveryVersion discoveryVersion)
            : this(discoveryVersion, GetDefaultMulticastAddress())
        {
        }

        public UdpDiscoveryEndpoint(DiscoveryVersion discoveryVersion, string multicastAddress)
            : this(discoveryVersion, new Uri(multicastAddress))
        {
        }

        public UdpDiscoveryEndpoint(DiscoveryVersion discoveryVersion, Uri multicastAddress)
            : base(discoveryVersion, ServiceDiscoveryMode.Adhoc)
        {
            if (multicastAddress == null)
            {
                throw FxTrace.Exception.ArgumentNull("multicastAddress");
            }
            if (discoveryVersion == null)
            {
                throw FxTrace.Exception.ArgumentNull("discoveryVersion");
            }

            // Send replies async to maintain performance
            base.Behaviors.Add(new DispatcherSynchronizationBehavior { AsynchronousSendEnabled = true });

            Initialize(multicastAddress);
        }

        public Uri MulticastAddress
        {
            get
            {
                return this.viaBehavior.Via;
            }
            set
            {
                if (value == null)
                {
                    throw FxTrace.Exception.ArgumentNull("value");
                }

                this.viaBehavior.Via = value;
                base.ListenUri = value;
            }
        }

        [Obsolete("TranportSettings property in System.SerivceModel.Discovery.UdpDiscoveryEndpoint is obsolete. Consider using System.ServiceModel.Channels.UdpTransportBindingElement for setting the transport properties.")]
        public UdpTransportSettings TransportSettings
        {
            get
            {
                return this.udpTransportSettings;
            }
        }

        void Initialize(Uri multicastAddress)
        {
            this.viaBehavior = new DiscoveryViaBehavior(multicastAddress);
            base.ListenUri = multicastAddress;

            TextMessageEncodingBindingElement textBE = new TextMessageEncodingBindingElement();
            textBE.MessageVersion = base.DiscoveryVersion.Implementation.MessageVersion;

            UdpTransportBindingElement udpBE = DiscoveryDefaults.Udp.CreateUdpTransportBindingElement();
            this.udpTransportSettings = new UdpTransportSettings(udpBE);

            CustomBinding binding = new CustomBinding();
            binding.Elements.Add(textBE);
            binding.Elements.Add(udpBE);

            base.MaxResponseDelay = DiscoveryDefaults.Udp.AppMaxDelay;
            base.Address = new EndpointAddress(base.DiscoveryVersion.Implementation.DiscoveryAddress);
            base.Binding = binding;
            base.Behaviors.Add(this.viaBehavior);
            base.Behaviors.Add(new UdpReplyToBehavior(udpBE.Scheme));
            base.Behaviors.Add(new UdpContractFilterBehavior());
        }

        static Uri GetDefaultMulticastAddress()
        {
            return Socket.OSSupportsIPv4 ? DefaultIPv4MulticastAddress : DefaultIPv6MulticastAddress;
        }
    }
}
