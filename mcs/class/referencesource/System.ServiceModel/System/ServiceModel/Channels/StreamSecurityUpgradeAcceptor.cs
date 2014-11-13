//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.IO;
    using System.ServiceModel.Security;

    public abstract class StreamSecurityUpgradeAcceptor : StreamUpgradeAcceptor
    {
        protected StreamSecurityUpgradeAcceptor()
        {
        }

        public abstract SecurityMessageProperty GetRemoteSecurity(); // works after call to AcceptUpgrade
    }
}
