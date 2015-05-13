namespace System.Diagnostics.PerformanceData
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class CounterSetInstanceCounterDataSet : IDisposable
    {
        private Dictionary<int, CounterData> m_counters;
        [SecurityCritical]
        internal unsafe byte* m_dataBlock;
        private int m_disposed;
        internal CounterSetInstance m_instance;

        [SecurityCritical]
        internal unsafe CounterSetInstanceCounterDataSet(CounterSetInstance thisInst)
        {
            this.m_instance = thisInst;
            this.m_counters = new Dictionary<int, CounterData>();
            if (this.m_instance.m_counterSet.m_provider == null)
            {
                throw new ArgumentException(System.SR.GetString("Perflib_Argument_ProviderNotFound", new object[] { this.m_instance.m_counterSet.m_providerGuid }), "ProviderGuid");
            }
            if (this.m_instance.m_counterSet.m_provider.m_hProvider.IsInvalid)
            {
                throw new InvalidOperationException(System.SR.GetString("Perflib_InvalidOperation_NoActiveProvider", new object[] { this.m_instance.m_counterSet.m_providerGuid }));
            }
            this.m_dataBlock = (byte*) Marshal.AllocHGlobal((int) (this.m_instance.m_counterSet.m_idToCounter.Count * 8));
            if (this.m_dataBlock == null)
            {
                throw new InsufficientMemoryException(System.SR.GetString("Perflib_InsufficientMemory_InstanceCounterBlock", new object[] { this.m_instance.m_counterSet.m_counterSet, this.m_instance.m_instName }));
            }
            int num = 0;
            foreach (KeyValuePair<int, CounterType> pair in this.m_instance.m_counterSet.m_idToCounter)
            {
                CounterData data = new CounterData((long)(this.m_dataBlock + (num * 8)));
                this.m_counters.Add(pair.Key, data);
                int num2 = Microsoft.Win32.UnsafeNativeMethods.PerfSetCounterRefValue(this.m_instance.m_counterSet.m_provider.m_hProvider, this.m_instance.m_nativeInst, pair.Key, (void*) (this.m_dataBlock + (num * 8)));
                if (num2 != 0)
                {
                    this.Dispose(true);
                    switch (num2)
                    {
                        case 0x490:
                            throw new InvalidOperationException(System.SR.GetString("Perflib_InvalidOperation_CounterRefValue", new object[] { this.m_instance.m_counterSet.m_counterSet, pair.Key, this.m_instance.m_instName }));
                    }
                    throw new Win32Exception((int) num2);
                }
                num++;
            }
        }

        [SecurityCritical]
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SecurityCritical]
        private unsafe void Dispose(bool disposing)
        {
            if ((Interlocked.Exchange(ref this.m_disposed, 1) == 0) && (this.m_dataBlock != null))
            {
                Marshal.FreeHGlobal((IntPtr) this.m_dataBlock);
                this.m_dataBlock = null;
            }
        }

        [SecurityCritical]
        ~CounterSetInstanceCounterDataSet()
        {
            this.Dispose(false);
        }

        public CounterData this[int counterId]
        {
            get
            {
                CounterData data;
                if (this.m_disposed != 0)
                {
                    return null;
                }
                try
                {
                    data = this.m_counters[counterId];
                }
                catch (KeyNotFoundException)
                {
                    data = null;
                }
                catch
                {
                    throw;
                }
                return data;
            }
        }

        public CounterData this[string counterName]
        {
            get
            {
                CounterData data;
                if (counterName == null)
                {
                    throw new ArgumentNullException("CounterName");
                }
                if (counterName.Length == 0)
                {
                    throw new ArgumentNullException("CounterName");
                }
                if (this.m_disposed != 0)
                {
                    return null;
                }
                try
                {
                    int num = this.m_instance.m_counterSet.m_stringToId[counterName];
                    try
                    {
                        data = this.m_counters[num];
                    }
                    catch (KeyNotFoundException)
                    {
                        data = null;
                    }
                    catch
                    {
                        throw;
                    }
                }
                catch (KeyNotFoundException)
                {
                    data = null;
                }
                catch
                {
                    throw;
                }
                return data;
            }
        }
    }
}

