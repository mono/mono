//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.PeerResolvers
{
    public enum PeerReferralPolicy
    {
        Service,
        Share,
        DoNotShare,
    }

    static class PeerReferralPolicyHelper
    {
        internal static bool IsDefined(PeerReferralPolicy value)
        {
            return (
                value == PeerReferralPolicy.Service ||
                value == PeerReferralPolicy.Share ||
                value == PeerReferralPolicy.DoNotShare);
        }
    }
}


