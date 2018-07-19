
using System;

namespace System.Net.NetworkInformation
{
    
    /// Provides information about a network interface's unicast address
    public abstract class UnicastIPAddressInformation : IPAddressInformation
    {

        /// [To be supplied.]
        public abstract long AddressPreferredLifetime {get;}

        /// [To be supplied.]
        public abstract long AddressValidLifetime {get;}
     
        /// Specifies the amount of time remaining on the Dynamic Host Configuration Protocol (DHCP) lease for this IP address.
        public abstract long DhcpLeaseLifetime {get;}

        /// Gets a value that indicates the state of the duplicate address detection algorithm.
        public abstract DuplicateAddressDetectionState DuplicateAddressDetectionState {get;}
     
        /// Gets a value that identifies the source of a unicast IP address prefix.
        public abstract PrefixOrigin PrefixOrigin  {get;}
     
        /// Gets a value that identifies the source of a unicast IP address suffix.
        public abstract SuffixOrigin SuffixOrigin  {get;}
 
        public abstract IPAddress IPv4Mask {get;}

        /// The CIDR representation of the subnet mask.
        public virtual int PrefixLength
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}

