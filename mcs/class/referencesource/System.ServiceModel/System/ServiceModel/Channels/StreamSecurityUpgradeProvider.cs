//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    public abstract class StreamSecurityUpgradeProvider : StreamUpgradeProvider
    {
        protected StreamSecurityUpgradeProvider()
            : base()
        {
        }

        protected StreamSecurityUpgradeProvider(IDefaultCommunicationTimeouts timeouts)
            : base(timeouts)
        {
        }

        public abstract EndpointIdentity Identity
        {
            get;
        }
    }
}
