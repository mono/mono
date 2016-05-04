
using System;

namespace System.Net.NetworkInformation
{
           
    /// Provides information about a network interface address.
    public abstract class GatewayIPAddressInformation
    {
        /// Gets the Internet Protocol (IP) address.
        public abstract IPAddress Address {get;}
    }
}

