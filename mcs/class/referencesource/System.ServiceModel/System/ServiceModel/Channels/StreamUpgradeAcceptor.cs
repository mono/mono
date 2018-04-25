//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.IO;

    public abstract class StreamUpgradeAcceptor
    {
        protected StreamUpgradeAcceptor()
        {
        }

        public abstract bool CanUpgrade(string contentType);

        public virtual Stream AcceptUpgrade(Stream stream)
        {
            return EndAcceptUpgrade(BeginAcceptUpgrade(stream, null, null));
        }

        public abstract IAsyncResult BeginAcceptUpgrade(Stream stream, AsyncCallback callback, object state);
        public abstract Stream EndAcceptUpgrade(IAsyncResult result);

    }
}
