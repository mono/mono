//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Diagnostics;
    using System.ServiceModel;
    using System.Collections.Generic;
    using System.ServiceModel.Diagnostics;

    abstract class ConnectionPoolRegistry
    {
        Dictionary<string, List<ConnectionPool>> registry;

        protected ConnectionPoolRegistry()
        {
            registry = new Dictionary<string, List<ConnectionPool>>();
        }

        object ThisLock
        {
            get { return this.registry; }
        }

        // NOTE: performs the open on the pool for you
        public ConnectionPool Lookup(IConnectionOrientedTransportChannelFactorySettings settings)
        {
            ConnectionPool result = null;
            string key = settings.ConnectionPoolGroupName;

            lock (ThisLock)
            {
                List<ConnectionPool> registryEntry = null;

                if (registry.TryGetValue(key, out registryEntry))
                {
                    for (int i = 0; i < registryEntry.Count; i++)
                    {
                        if (registryEntry[i].IsCompatible(settings) && registryEntry[i].TryOpen())
                        {
                            result = registryEntry[i];
                            break;
                        }
                    }
                }
                else
                {
                    registryEntry = new List<ConnectionPool>();
                    registry.Add(key, registryEntry);
                }

                if (result == null)
                {
                    result = CreatePool(settings);
                    registryEntry.Add(result);
                }
            }

            return result;
        }

        protected abstract ConnectionPool CreatePool(IConnectionOrientedTransportChannelFactorySettings settings);

        public void Release(ConnectionPool pool, TimeSpan timeout)
        {
            lock (ThisLock)
            {
                if (pool.Close(timeout))
                {
                    List<ConnectionPool> registryEntry = registry[pool.Name];
                    for (int i = 0; i < registryEntry.Count; i++)
                    {
                        if (object.ReferenceEquals(registryEntry[i], pool))
                        {
                            registryEntry.RemoveAt(i);
                            break;
                        }
                    }

                    if (registryEntry.Count == 0)
                    {
                        registry.Remove(pool.Name);
                    }
                }
            }
        }
    }
}


