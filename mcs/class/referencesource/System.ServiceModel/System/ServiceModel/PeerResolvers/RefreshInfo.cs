//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.PeerResolvers
{
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.Runtime.Serialization;

    [MessageContract(IsWrapped = false)]
    public class RefreshInfo
    {
        [DataContract(Name = "RefreshInfo", Namespace = PeerStrings.Namespace)]
        class RefreshInfoDC
        {
            [DataMember(Name = "RegistrationId")]
            public Guid RegistrationId;

            [DataMember(Name = "MeshId")]
            public string MeshId;
            public RefreshInfoDC() { }
            public RefreshInfoDC(string meshId, Guid regId)
            {
                MeshId = meshId;
                RegistrationId = regId;
            }
        }

        public RefreshInfo(string meshId, Guid regId)
        {
            this.body = new RefreshInfoDC(meshId, regId);
        }

        public RefreshInfo()
        {
            this.body = new RefreshInfoDC();
        }

        public string MeshId { get { return body.MeshId; } }

        public Guid RegistrationId { get { return body.RegistrationId; } }

        [MessageBodyMember(Name = "Refresh", Namespace = PeerStrings.Namespace)]
        RefreshInfoDC body;

        public bool HasBody()
        {
            return body != null;
        }

    }
}

