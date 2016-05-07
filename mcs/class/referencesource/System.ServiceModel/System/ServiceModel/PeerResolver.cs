//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Collections.ObjectModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.PeerResolvers;

    public abstract class PeerResolver
    {
        public abstract bool CanShareReferrals { get; }
        public abstract object Register(string meshId, PeerNodeAddress nodeAddress, TimeSpan timeout);
        public abstract ReadOnlyCollection<PeerNodeAddress> Resolve(string meshId, int maxAddresses, TimeSpan timeout);
        public abstract void Unregister(object registrationId, TimeSpan timeout);
        public abstract void Update(object registrationId, PeerNodeAddress updatedNodeAddress, TimeSpan timeout);
        public virtual void Initialize(EndpointAddress address, Binding binding, ClientCredentials credentials, PeerReferralPolicy referralPolicy)
        {
        }
    }
}

