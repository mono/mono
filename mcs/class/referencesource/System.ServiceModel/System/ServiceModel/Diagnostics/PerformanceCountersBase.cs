//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Diagnostics
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.PerformanceData;
    using System.Globalization;
    using System.Runtime;
    using System.Threading;
    using System.Linq;

    abstract class PerformanceCountersBase : IDisposable
    {
        internal abstract string InstanceName
        {
            get;
        }

        internal abstract string[] CounterNames
        {
            get;
        }

        internal abstract int PerfCounterStart
        {
            get;
        }

        internal abstract int PerfCounterEnd
        {
            get;
        }

        // remove count chars from string and add a 2 char hash code to beginning or end, as specified.
        protected static string GetHashedString(string str, int startIndex, int count, bool hashAtEnd)
        {
            string returnVal = str.Remove(startIndex, count);
            string hash = ((uint)str.GetHashCode() % 99).ToString("00", CultureInfo.InvariantCulture);
            return hashAtEnd ? returnVal + hash : hash + returnVal;
        }

        internal abstract bool Initialized { get; }

        protected int disposed = 0;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) == 0)
            {
                Dispose(true);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        // A CounterSetInstance is not disposed immediately when a service, endpoint or operation perf counter is disposed. Because messages 
        // can be processed while a ServiceHost is being closed, and such messages can try to update perf counters data, resulting in AVs or 
        // corruptions (see bug 249132 @ CSDMain). So instead of disposing a CounterSetInstance, we hold a WeakReference to it, until either 
        // GC reclaims it or a new service/endpoint/operation perf counter is started with the same name (and re-uses the CounterSetInstance).
        // The CounterSetInstance finalizer will free up the perf counters memory, so we don't have a leak.
        protected class CounterSetInstanceCache
        {
            // instance name -> WeakReference of CounterSetInstance
            private readonly Dictionary<string, WeakReference> cache = new Dictionary<string, WeakReference>();

            /// <summary>
            /// Returns and removes the CounterSetInstance with the specified name from the cache. Returns null if not found.
            /// </summary>
            internal CounterSetInstance Get(string instanceName)
            {
                Fx.Assert(instanceName != null, "Invalid argument.");

                lock (this.cache)
                {
                    WeakReference wr;
                    if (this.cache.TryGetValue(instanceName, out wr))
                    {
                        Fx.Assert(wr != null, "The values in 'availableCounterSetInstances' should not be null.");
                        this.cache.Remove(instanceName);
                        return (CounterSetInstance)wr.Target;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            /// <summary>
            /// Adds a CounterSetInstance to the cache, from where it will be garbage collected or re-used by another performance counter (whichever occurs first).
            /// </summary>
            internal void Add(string instanceName, CounterSetInstance instance)
            {
                Fx.Assert(instanceName != null, "Invalid argument.");
                Fx.Assert(instance != null, "Invalid argument.");

                lock (this.cache)
                {
                    this.cache[instanceName] = new WeakReference(instance);
                }
            }

            /// <summary>
            /// Clear the entries for CounterSetInstances that were garbage collected.
            /// </summary>
            internal void Cleanup()
            {
                lock (this.cache)
                {
                    foreach (var entry in this.cache.Where(pair => !pair.Value.IsAlive).ToList())
                    {
                        this.cache.Remove(entry.Key);
                    }
                }
            }
        }
    }
}
