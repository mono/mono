//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.PeerResolvers
{
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.Runtime.Serialization;

    [MessageContract(IsWrapped = false)]
    public class RegisterInfo
    {
        [DataContract(Name = "Register", Namespace = PeerStrings.Namespace)]
        class RegisterInfoDC
        {
            [DataMember(Name = "ClientId")]
            public Guid ClientId;

            [DataMember(Name = "MeshId")]
            public string MeshId;

            [DataMember(Name = "NodeAddress")]
            public PeerNodeAddress NodeAddress;

            //            public TimeSpan RegistrationLifeTime;
            public RegisterInfoDC() { }
            public RegisterInfoDC(Guid client, string meshId, PeerNodeAddress address)
            {
                this.ClientId = client;
                this.MeshId = meshId;
                this.NodeAddress = address;
            }
        }

        public RegisterInfo(Guid client, string meshId, PeerNodeAddress address)
        {
            body = new RegisterInfoDC(client, meshId, address);
        }
        public RegisterInfo() { body = new RegisterInfoDC(); }

        [MessageBodyMember(Name = "Register", Namespace = PeerStrings.Namespace)]
        RegisterInfoDC body;

        public Guid ClientId
        {
            get { return this.body.ClientId; }
        }

        public string MeshId
        {
            get { return this.body.MeshId; }
        }

        public PeerNodeAddress NodeAddress
        {
            get { return this.body.NodeAddress; }
        }

        public bool HasBody()
        {
            return body != null;
        }
    }
}

