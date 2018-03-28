
using System;

namespace System.Net.NetworkInformation
{

    /// Specifies the current state of an IP address.
    public enum DuplicateAddressDetectionState
    {
        Invalid    = 0,
        Tentative,
        Duplicate,
        Deprecated,
        Preferred,
    }
}

