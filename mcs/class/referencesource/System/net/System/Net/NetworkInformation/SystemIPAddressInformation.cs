
    /// <summary><para>
    ///    Provides support for ip configuation information and statistics.
    ///</para></summary>
    ///
namespace System.Net.NetworkInformation {

    using System.Net;
    using System.Net.Sockets;
    using System;
    using System.Runtime.InteropServices;
    using System.Collections;
    using System.ComponentModel;
    using System.Security.Permissions;
    using Microsoft.Win32;



    //this is the main addressinformation class that contains the ipaddress
    //and other properties
    internal class SystemIPAddressInformation:IPAddressInformation{

        IPAddress address;
        internal bool transient = false;
        internal bool dnsEligible = true;

        internal SystemIPAddressInformation(IPAddress address, AdapterAddressFlags flags) {
            this.address = address;
            transient = (flags & AdapterAddressFlags.Transient) > 0;
            dnsEligible = (flags & AdapterAddressFlags.DnsEligible) > 0;
        }

        public override IPAddress Address{get {return address;}}

        /// <summary>The address is a cluster address and shouldn't be used by most applications.</summary>
        public override bool IsTransient{
            get {
                return (transient);
            }
        }

        /// <summary>This address can be used for DNS.</summary>
        public override bool IsDnsEligible{
            get {
                return (dnsEligible);
            }
        }
    }
}

