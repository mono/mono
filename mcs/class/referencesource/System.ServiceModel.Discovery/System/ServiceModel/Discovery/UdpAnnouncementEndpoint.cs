//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Channels;

    [Fx.Tag.XamlVisible(false)]
    public class UdpAnnouncementEndpoint : AnnouncementEndpoint
    {
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        [SuppressMessage(FxCop.Category.Naming, "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Pv", Justification = "IPv4 is valid.")]
        public static readonly Uri DefaultIPv4MulticastAddress = DiscoveryDefaults.Udp.IPv4MulticastAddress;

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        [SuppressMessage(FxCop.Category.Naming, "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Pv", Justification = "IPv6 is valid.")]
        public static readonly Uri DefaultIPv6MulticastAddress = DiscoveryDefaults.Udp.IPv6MulticastAddress;

        DiscoveryViaBehavior viaBehavior;
        UdpTransportSettings udpTransportSettings;

        public UdpAnnouncementEndpoint()
            : this(DefaultIPv4MulticastAddress)
        {
        }

        public UdpAnnouncementEndpoint(string multicastAddress)
            : this(new Uri(multicastAddress))
        {
        }

        public UdpAnnouncementEndpoint(Uri multicastAddress)
            : this(DiscoveryVersion.DefaultDiscoveryVersion, multicastAddress)
        {
        }

        public UdpAnnouncementEndpoint(DiscoveryVersion discoveryVersion)
            : this(discoveryVersion, DefaultIPv4MulticastAddress)
        {
        }

        public UdpAnnouncementEndpoint(DiscoveryVersion discoveryVersion, string multicastAddress)
            : this(discoveryVersion, new Uri(multicastAddress))
        {
        }

        public UdpAnnouncementEndpoint(DiscoveryVersion discoveryVersion, Uri multicastAddress)
            : base(discoveryVersion)
        {
            if (multicastAddress == null)
            {
                throw FxTrace.Exception.ArgumentNull("multicastAddress");
            }
            if (discoveryVersion == null)
            {
                throw FxTrace.Exception.ArgumentNull("discoveryVersion");
            }

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

        [Obsolete("TranportSettings property in System.SerivceModel.Discovery.UdpAnnouncementEndpoint is obsolete. Consider using System.ServiceModel.Channels.UdpTransportBindingElement for setting the transport properties.")]
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

            base.MaxAnnouncementDelay = DiscoveryDefaults.Udp.AppMaxDelay;

            base.Address = new EndpointAddress(base.DiscoveryVersion.Implementation.DiscoveryAddress);
            base.Binding = binding;
            base.Behaviors.Add(this.viaBehavior);
            base.Behaviors.Add(new UdpContractFilterBehavior());
        }
    }
}
