
using System;

namespace System.Net.NetworkInformation
{

    /// Specifies how an IP address host suffix was located.
    public enum SuffixOrigin
    {
        Other = 0,
        Manual,
        WellKnown,
        OriginDhcp,
        LinkLayerAddress,
        Random,
    }
}

