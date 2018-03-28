
using System;

namespace System.Net.NetworkInformation
{
    /// Specifies how an IP address network prefix was located.
    public enum PrefixOrigin
    {
        Other = 0,
        Manual,
        WellKnown,
        Dhcp,
        RouterAdvertisement,
    }
}

