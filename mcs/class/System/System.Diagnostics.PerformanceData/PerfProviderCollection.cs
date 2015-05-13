namespace System.Diagnostics.PerformanceData
{
    using System;
    using System.Collections.Generic;
    using System.Security;
    using System.Threading;

    internal static class PerfProviderCollection
    {
        private static CounterSetInstanceType[] s_counterSetInstanceTypes = ((CounterSetInstanceType[]) Enum.GetValues(typeof(CounterSetInstanceType)));
        private static Dictionary<object, int> s_counterSetList = new Dictionary<object, int>();
        private static CounterType[] s_counterTypes = ((CounterType[]) Enum.GetValues(typeof(CounterType)));
        private static object s_hiddenInternalSyncObject;
        private static List<PerfProvider> s_providerList = new List<PerfProvider>();

        [SecurityCritical]
        internal static PerfProvider QueryProvider(Guid providerGuid)
        {
            lock (s_lockObject)
            {
                foreach (PerfProvider provider in s_providerList)
                {
                    if (provider.m_providerGuid == providerGuid)
                    {
                        return provider;
                    }
                }
                PerfProvider item = new PerfProvider(providerGuid);
                s_providerList.Add(item);
                return item;
            }
        }

        internal static void RegisterCounterSet(Guid counterSetGuid)
        {
            lock (s_lockObject)
            {
                if (s_counterSetList.ContainsKey(counterSetGuid))
                {
                    throw new ArgumentException(System.SR.GetString("Perflib_Argument_CounterSetAlreadyRegister", new object[] { counterSetGuid }), "CounterSetGuid");
                }
                s_counterSetList.Add(counterSetGuid, 0);
            }
        }

        [SecurityCritical]
        internal static void RemoveProvider(Guid providerGuid)
        {
            lock (s_lockObject)
            {
                PerfProvider item = null;
                foreach (PerfProvider provider2 in s_providerList)
                {
                    if (provider2.m_providerGuid == providerGuid)
                    {
                        item = provider2;
                    }
                }
                if (item != null)
                {
                    item.m_hProvider.Dispose();
                    s_providerList.Remove(item);
                }
            }
        }

        internal static void UnregisterCounterSet(Guid counterSetGuid)
        {
            lock (s_lockObject)
            {
                s_counterSetList.Remove(counterSetGuid);
            }
        }

        internal static bool ValidateCounterSetInstanceType(CounterSetInstanceType inCounterSetInstanceType)
        {
            foreach (CounterSetInstanceType type in s_counterSetInstanceTypes)
            {
                if (type == inCounterSetInstanceType)
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool ValidateCounterType(CounterType inCounterType)
        {
            foreach (CounterType type in s_counterTypes)
            {
                if (type == inCounterType)
                {
                    return true;
                }
            }
            return false;
        }

        private static object s_lockObject
        {
            get
            {
                if (s_hiddenInternalSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref s_hiddenInternalSyncObject, obj2, null);
                }
                return s_hiddenInternalSyncObject;
            }
        }
    }
}

