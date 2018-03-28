//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.PeerResolvers
{
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.Runtime.Serialization;

    [MessageContract(IsWrapped = false)]
    public class UpdateInfo
    {
        [DataContract(Name = "Update", Namespace = PeerStrings.Namespace)]
        class UpdateInfoDC
        {
            [DataMember(Name = "ClientId")]
            public Guid ClientId;

            [DataMember(Name = "MeshId")]
            public string MeshId;

            [DataMember(Name = "NodeAddress")]
            public PeerNodeAddress NodeAddress;

            [DataMember(Name = "RegistrationId")]
            public Guid RegistrationId;
            public UpdateInfoDC() { }
            public UpdateInfoDC(Guid registrationId, Guid client, string meshId, PeerNodeAddress address)
            {
                this.ClientId = client;
                this.MeshId = meshId;
                this.NodeAddress = address;
                this.RegistrationId = registrationId;
            }
        }

        public UpdateInfo(Guid registrationId, Guid client, string meshId, PeerNodeAddress address)
        {
            body = new UpdateInfoDC(registrationId, client, meshId, address);
        }
        public UpdateInfo() { body = new UpdateInfoDC(); }

        public Guid ClientId
        {
            get { return this.body.ClientId; }
        }

        public Guid RegistrationId
        {
            get { return this.body.RegistrationId; }
        }

        public string MeshId
        {
            get { return this.body.MeshId; }
        }

        public PeerNodeAddress NodeAddress
        {
            get { return this.body.NodeAddress; }
        }

        [MessageBodyMember(Name = "Update", Namespace = PeerStrings.Namespace)]
        UpdateInfoDC body;

        public bool HasBody()
        {
            return body != null;
        }
    }
}

