
    /// <summary><para>
    ///    Provides support for ip configuation information and statistics.
    ///</para></summary>
    ///
namespace System.Net.NetworkInformation {

    using System.Net;
    
    /// <include file='doc\NetworkInterface.uex' path='docs/doc[@for="IPMulticastAddressInformation"]/*' />
    /// <summary>Specifies the Multicast addresses for an interface.</summary>
    /// </platnote>
    internal class SystemMulticastIPAddressInformation:MulticastIPAddressInformation {

        private SystemIPAddressInformation innerInfo;

        private SystemMulticastIPAddressInformation() {
        }

        public SystemMulticastIPAddressInformation(SystemIPAddressInformation addressInfo) {
            innerInfo = addressInfo;
        }

       /// <include file='doc\NetworkInterface.uex' path='docs/doc[@for="IPAddressInformation.Address"]/*' />
        public override IPAddress Address{get {return innerInfo.Address;}}

        /// <include file='doc\NetworkInterface.uex' path='docs/doc[@for="IPAddressInformation.Transient"]/*' />
        /// <summary>The address is a cluster address and shouldn't be used by most applications.</summary>
        public override bool IsTransient{
            get {
                return (innerInfo.IsTransient);
            }
        }

        /// <include file='doc\NetworkInterface.uex' path='docs/doc[@for="IPAddressInformation.DnsEligible"]/*' />
        /// <summary>This address can be used for DNS.</summary>
        public override bool IsDnsEligible{
            get {
                return (innerInfo.IsDnsEligible);
            }
        }


        /// <include file='doc\NetworkInterface.uex' path='docs/doc[@for="IPMulticastAddressInformation.PrefixOrigin"]/*' />
        public override PrefixOrigin PrefixOrigin{
            get {
                return PrefixOrigin.Other;
            }
        }

        /// <include file='doc\NetworkInterface.uex' path='docs/doc[@for="IPMulticastAddressInformation.SuffixOrigin"]/*' />
        public override SuffixOrigin SuffixOrigin{
            get {
                return SuffixOrigin.Other;
            }
        }
        /// <include file='doc\NetworkInterface.uex' path='docs/doc[@for="IPMulticastAddressInformation.DuplicateAddressDetectionState"]/*' />
        /// <summary>IPv6 only.  Specifies the duplicate address detection state. Only supported
        /// for IPv6. If called on an IPv4 address, will throw a "not supported" exception.</summary>
        public override DuplicateAddressDetectionState DuplicateAddressDetectionState{
            get {
                return DuplicateAddressDetectionState.Invalid;
            }
        }


        /// <include file='doc\NetworkInterface.uex' path='docs/doc[@for="IPMulticastAddressInformation.ValidLifetime"]/*' />
        /// <summary>Specifies the valid lifetime of the address in seconds.</summary>
        public override long AddressValidLifetime{
            get {
                return 0;
                }
            }
        /// <include file='doc\NetworkInterface.uex' path='docs/doc[@for="IPMulticastAddressInformation.PreferredLifetime"]/*' />
        /// <summary>Specifies the prefered lifetime of the address in seconds.</summary>

        public override long AddressPreferredLifetime{
            get {
                return 0;
                }
            }
        /// <include file='doc\NetworkInterface.uex' path='docs/doc[@for="IPMulticastAddressInformation.PreferredLifetime"]/*' />

        /// <include file='doc\NetworkInterface.uex' path='docs/doc[@for="IPMulticastAddressInformation.DhcpLeaseLifetime"]/*' />
        /// <summary>Specifies the prefered lifetime of the address in seconds.</summary>
        public override long DhcpLeaseLifetime{
            get {
                return 0;
                }
            }


        internal static MulticastIPAddressInformationCollection ToMulticastIpAddressInformationCollection(IPAddressInformationCollection addresses) {
            MulticastIPAddressInformationCollection multicastList = new MulticastIPAddressInformationCollection();
            foreach (IPAddressInformation addressInfo in addresses) {
                multicastList.InternalAdd(new SystemMulticastIPAddressInformation((SystemIPAddressInformation)addressInfo));
            }
            return multicastList;
        }
    }
}

