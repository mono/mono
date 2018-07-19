//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.ServiceModel.PeerResolvers;
    using System.ServiceModel;

    public abstract class PeerResolverBindingElement : BindingElement
    {
        protected PeerResolverBindingElement() { }
        protected PeerResolverBindingElement(PeerResolverBindingElement other) : base(other) { }
        public abstract PeerReferralPolicy ReferralPolicy { get; set; }
        public abstract PeerResolver CreatePeerResolver();
    }
}
