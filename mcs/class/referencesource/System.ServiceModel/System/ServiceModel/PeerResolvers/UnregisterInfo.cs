//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.PeerResolvers
{
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.Runtime.Serialization;

    [MessageContract(IsWrapped = false)]
    public class UnregisterInfo
    {
        [DataContract(Name = "UnregisterInfo", Namespace = PeerStrings.Namespace)]
        class UnregisterInfoDC
        {
            [DataMember(Name = "RegistrationId")]
            public Guid RegistrationId;

            [DataMember(Name = "MeshId")]
            public string MeshId;

            public UnregisterInfoDC() { }
            public UnregisterInfoDC(string meshId, Guid registrationId)
            {
                this.MeshId = meshId;
                this.RegistrationId = registrationId;
            }
        }

        [MessageBodyMember(Name = "Unregister", Namespace = PeerStrings.Namespace)]
        UnregisterInfoDC body;

        public Guid RegistrationId
        {
            get { return body.RegistrationId; }
        }

        public string MeshId
        {
            get { return body.MeshId; }
        }

        public UnregisterInfo() { body = new UnregisterInfoDC(); }
        public UnregisterInfo(string meshId, Guid registrationId)
        {
            this.body = new UnregisterInfoDC(meshId, registrationId);
        }

        public bool HasBody()
        {
            return body != null;
        }
    }
}

