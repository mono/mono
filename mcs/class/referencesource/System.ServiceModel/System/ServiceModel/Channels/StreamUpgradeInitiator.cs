//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.IO;
    using System.Runtime;

    public abstract class StreamUpgradeInitiator
    {
        protected StreamUpgradeInitiator()
        {
        }

        public abstract string GetNextUpgrade();

        public abstract Stream InitiateUpgrade(Stream stream);
        public abstract IAsyncResult BeginInitiateUpgrade(Stream stream, AsyncCallback callback, object state);
        public abstract Stream EndInitiateUpgrade(IAsyncResult result);

        internal virtual IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        internal virtual void EndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        internal virtual IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        internal virtual void EndClose(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        internal virtual void Open(TimeSpan timeout)
        {
        }

        internal virtual void Close(TimeSpan timeout)
        {
        }
    }
}
