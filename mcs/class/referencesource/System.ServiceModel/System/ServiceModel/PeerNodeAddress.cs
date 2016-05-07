//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Runtime.Serialization;
    using System.ServiceModel.Channels;

    [DataContract(Name = "PeerNodeAddress", Namespace = PeerStrings.Namespace)]
    [KnownType(typeof(IPAddress[]))]
    public sealed class PeerNodeAddress
    {
        [DataMember(Name = "EndpointAddress")]
        internal EndpointAddress10 InnerEPR
        {
            get { return this.endpointAddress == null ? null : EndpointAddress10.FromEndpointAddress(this.endpointAddress); }
            set { this.endpointAddress = (value == null ? null : value.ToEndpointAddress()); }
        }

        EndpointAddress endpointAddress;
        string servicePath;

        ReadOnlyCollection<IPAddress> ipAddresses;

        [DataMember(Name = "IPAddresses")]
        internal IList<IPAddress> ipAddressesDataMember
        {
            get { return ipAddresses; }
            set { ipAddresses = new ReadOnlyCollection<IPAddress>((value == null) ? new IPAddress[0] : value); }
        }

        //NOTE: if a default constructor is provided, make sure to review ServicePath property getter.
        public PeerNodeAddress(EndpointAddress endpointAddress, ReadOnlyCollection<IPAddress> ipAddresses)
        {
            if (endpointAddress == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("endpointAddress"));
            if (ipAddresses == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("ipAddresses"));
            Initialize(endpointAddress, ipAddresses);
        }

        void Initialize(EndpointAddress endpointAddress, ReadOnlyCollection<IPAddress> ipAddresses)
        {
            this.endpointAddress = endpointAddress;
            servicePath = this.endpointAddress.Uri.PathAndQuery.ToUpperInvariant();
            this.ipAddresses = ipAddresses;
        }

        public EndpointAddress EndpointAddress
        {
            get { return this.endpointAddress; }
        }

        internal string ServicePath
        {
            get
            {
                if (this.servicePath == null)
                {
                    this.servicePath = this.endpointAddress.Uri.PathAndQuery.ToUpperInvariant();
                }
                return this.servicePath;
            }
        }

        public ReadOnlyCollection<IPAddress> IPAddresses
        {
            get
            {
                if (this.ipAddresses == null)
                {
                    this.ipAddresses = new ReadOnlyCollection<IPAddress>(new IPAddress[0]);
                }
                return this.ipAddresses;
            }
        }
    }
}
