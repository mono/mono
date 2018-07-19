//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.IO;
    using System.ServiceModel.Security;

    public abstract class StreamSecurityUpgradeInitiator : StreamUpgradeInitiator
    {
        protected StreamSecurityUpgradeInitiator()
        {
        }

        public abstract SecurityMessageProperty GetRemoteSecurity(); // works after call to AcceptUpgrade

        internal static SecurityMessageProperty GetRemoteSecurity(StreamUpgradeInitiator upgradeInitiator)
        {
            StreamSecurityUpgradeInitiator securityInitiator = upgradeInitiator as StreamSecurityUpgradeInitiator;
            if (securityInitiator != null)
            {
                return securityInitiator.GetRemoteSecurity();
            }
            else
            {
                return null;
            }
        }
    }
}
