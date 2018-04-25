//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;

    class NamedPipeConnectionPoolRegistry : ConnectionPoolRegistry
    {
        public NamedPipeConnectionPoolRegistry()
            : base()
        {
        }

        protected override ConnectionPool CreatePool(IConnectionOrientedTransportChannelFactorySettings settings)
        {
            Fx.Assert(settings is IPipeTransportFactorySettings, "NamedPipeConnectionPool requires an IPipeTransportFactorySettings.");
            return new NamedPipeConnectionPool((IPipeTransportFactorySettings)settings);
        }

        class NamedPipeConnectionPool : ConnectionPool
        {
            PipeNameCache pipeNameCache;
            IPipeTransportFactorySettings transportFactorySettings;

            public NamedPipeConnectionPool(IPipeTransportFactorySettings settings)
                : base(settings, TimeSpan.MaxValue)
            {
                this.pipeNameCache = new PipeNameCache();
                this.transportFactorySettings = settings;
            }

            protected override EndpointConnectionPool CreateEndpointConnectionPool(string key)
            {
                return new NamedPipeEndpointConnectionPool(this, key);
            }

            protected override string GetPoolKey(EndpointAddress address, Uri via)
            {
                string result;
                lock (base.ThisLock)
                {
                    if (!this.pipeNameCache.TryGetValue(via, out result))
                    {
                        result = PipeConnectionInitiator.GetPipeName(via, this.transportFactorySettings);
                        this.pipeNameCache.Add(via, result);
                    }
                }
                return result;
            }

            protected override void OnClosed()
            {
                base.OnClosed();
                this.pipeNameCache.Clear();
            }

            void OnConnectionAborted(string pipeName)
            {
                // the underlying pipe name may have changed; purge the old one from the cache
                lock (base.ThisLock)
                {
                    this.pipeNameCache.Purge(pipeName);
                }
            }

            protected class NamedPipeEndpointConnectionPool : IdleTimeoutEndpointConnectionPool
            {
                NamedPipeConnectionPool parent;

                public NamedPipeEndpointConnectionPool(NamedPipeConnectionPool parent, string key)
                    : base(parent, key)
                {
                    this.parent = parent;
                }

                protected override void OnConnectionAborted()
                {
                    parent.OnConnectionAborted(this.Key);
                }
            }
        }

        // not thread-safe
        class PipeNameCache
        {
            Dictionary<Uri, string> forwardTable = new Dictionary<Uri, string>();
            Dictionary<string, ICollection<Uri>> reverseTable = new Dictionary<string, ICollection<Uri>>();

            public void Add(Uri uri, string pipeName)
            {
                this.forwardTable.Add(uri, pipeName);

                ICollection<Uri> uris;
                if (!this.reverseTable.TryGetValue(pipeName, out uris))
                {
                    uris = new Collection<Uri>();
                    this.reverseTable.Add(pipeName, uris);
                }
                uris.Add(uri);
            }

            public void Clear()
            {
                this.forwardTable.Clear();
                this.reverseTable.Clear();
            }

            public void Purge(string pipeName)
            {
                ICollection<Uri> uris;
                if (this.reverseTable.TryGetValue(pipeName, out uris))
                {
                    this.reverseTable.Remove(pipeName);
                    foreach (Uri uri in uris)
                    {
                        this.forwardTable.Remove(uri);
                    }
                }
            }

            public bool TryGetValue(Uri uri, out string pipeName)
            {
                return this.forwardTable.TryGetValue(uri, out pipeName);
            }
        }
    }
}


