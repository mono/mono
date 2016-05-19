//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.PeerResolvers
{
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.Runtime.Serialization;

    [MessageContract(IsWrapped = false)]
    public class ResolveResponseInfo
    {
        [DataContract(Name = "ResolveResponseInfo", Namespace = PeerStrings.Namespace)]
        class ResolveResponseInfoDC
        {
            [DataMember(Name = "Addresses")]
            public IList<PeerNodeAddress> Addresses;

            public ResolveResponseInfoDC(PeerNodeAddress[] addresses)
            {
                this.Addresses = (IList<PeerNodeAddress>)addresses;
            }
        }

        public ResolveResponseInfo() : this(null) { }

        public ResolveResponseInfo(PeerNodeAddress[] addresses)
        {
            this.body = new ResolveResponseInfoDC(addresses);
        }

        public IList<PeerNodeAddress> Addresses
        {
            get { return body.Addresses; }
            set { this.body.Addresses = value; }
        }

        [MessageBodyMember(Name = "ResolveResponse", Namespace = PeerStrings.Namespace)]
        ResolveResponseInfoDC body;

        public bool HasBody()
        {
            return body != null;
        }
    }
}

