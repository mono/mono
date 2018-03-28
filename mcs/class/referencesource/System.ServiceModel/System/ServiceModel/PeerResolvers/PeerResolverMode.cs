//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.PeerResolvers
{
    public enum PeerResolverMode
    {
        Auto,
        Pnrp,
        Custom
    }

    static class PeerResolverModeHelper
    {
        internal static bool IsDefined(PeerResolverMode value)
        {
            return (
                value == PeerResolverMode.Auto ||
                value == PeerResolverMode.Pnrp ||
                value == PeerResolverMode.Custom );
        }
    }
}

