//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.PeerResolvers
{
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.Runtime.Serialization;

    [MessageContract(IsWrapped = false)]
    public class ResolveInfo
    {
        [DataContract(Name = "ResolveInfo", Namespace = PeerStrings.Namespace)]
        class ResolveInfoDC
        {
            [DataMember(Name = "ClientId")]
            public Guid ClientId;

            [DataMember(Name = "MeshId")]
            public string MeshId;

            [DataMember(Name = "MaxAddresses")]
            public int MaxAddresses;

            public ResolveInfoDC(Guid clientId, string meshId, int maxAddresses)
            {
                this.ClientId = clientId;
                this.MeshId = meshId;
                this.MaxAddresses = maxAddresses;
            }
            public ResolveInfoDC() { }
        }

        [MessageBodyMember(Name = "Resolve", Namespace = PeerStrings.Namespace)]
        ResolveInfoDC body;

        public ResolveInfo(Guid clientId, string meshId, int maxAddresses)
        {
            body = new ResolveInfoDC(clientId, meshId, maxAddresses);
        }

        public ResolveInfo()
        {
            body = new ResolveInfoDC();
        }

        public Guid ClientId
        {
            get { return this.body.ClientId; }
        }

        public string MeshId
        {
            get { return this.body.MeshId; }
        }

        public int MaxAddresses
        {
            get { return this.body.MaxAddresses; }
        }

        public bool HasBody()
        {
            return body != null;
        }
    }
}

