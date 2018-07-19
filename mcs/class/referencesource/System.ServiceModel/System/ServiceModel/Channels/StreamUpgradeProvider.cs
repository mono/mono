//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    public abstract class StreamUpgradeProvider : CommunicationObject
    {
        TimeSpan closeTimeout;
        TimeSpan openTimeout;

        protected StreamUpgradeProvider()
            : this(null)
        {
        }

        protected StreamUpgradeProvider(IDefaultCommunicationTimeouts timeouts)
        {
            if (timeouts != null)
            {
                this.closeTimeout = timeouts.CloseTimeout;
                this.openTimeout = timeouts.OpenTimeout;
            }
            else
            {
                this.closeTimeout = ServiceDefaults.CloseTimeout;
                this.openTimeout = ServiceDefaults.OpenTimeout;
            }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get { return this.closeTimeout; }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get { return this.closeTimeout; }
        }

        public virtual T GetProperty<T>() where T : class
        {
            return null;
        }

        public abstract StreamUpgradeInitiator CreateUpgradeInitiator(EndpointAddress remoteAddress, Uri via);
        public abstract StreamUpgradeAcceptor CreateUpgradeAcceptor();
    }
}
