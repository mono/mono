//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.ComponentModel;
    using System.ServiceModel;
    using System.ServiceModel.PeerResolvers;


    public sealed class PnrpPeerResolverBindingElement : PeerResolverBindingElement
    {
        PeerReferralPolicy referralPolicy;
        public PnrpPeerResolverBindingElement() { }

        public PnrpPeerResolverBindingElement(PeerReferralPolicy referralPolicy)
        {
            this.referralPolicy = referralPolicy;
        }

        PnrpPeerResolverBindingElement(PnrpPeerResolverBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            this.referralPolicy = elementToBeCloned.referralPolicy;
        }

        public override PeerReferralPolicy ReferralPolicy
        {
            get
            {
                return referralPolicy;
            }
            set
            {
                if (!PeerReferralPolicyHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int)value, typeof(PeerReferralPolicy)));
                }
                referralPolicy = value;
            }
        }

        public override BindingElement Clone()
        {
            return new PnrpPeerResolverBindingElement(this);
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));

            context.BindingParameters.Add(this);
            return context.BuildInnerChannelFactory<TChannel>();
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));

            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelFactory<TChannel>();
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));

            context.BindingParameters.Add(this);
            return context.BuildInnerChannelListener<TChannel>();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));

            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelListener<TChannel>();
        }

        public override PeerResolver CreatePeerResolver()
        {
            return new PnrpPeerResolver(this.referralPolicy);
        }

        public override T GetProperty<T>(BindingContext context)
        {
            return context.GetInnerProperty<T>();
        }
    }
}
