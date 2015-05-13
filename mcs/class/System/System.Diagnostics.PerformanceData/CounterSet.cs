namespace System.Diagnostics.PerformanceData
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class CounterSet : IDisposable
    {
        internal Guid m_counterSet;
        internal Dictionary<int, CounterType> m_idToCounter;
        private bool m_instanceCreated;
        internal CounterSetInstanceType m_instType;
        private readonly object m_lockObject;
        internal PerfProvider m_provider;
        internal Guid m_providerGuid;
        internal Dictionary<string, int> m_stringToId;
        private static readonly bool s_platformNotSupported = (Environment.OSVersion.Version.Major < 6);

        [SecuritySafeCritical, PermissionSet(SecurityAction.Demand, Unrestricted=true)]
        public CounterSet(Guid providerGuid, Guid counterSetGuid, CounterSetInstanceType instanceType)
        {
            if (s_platformNotSupported)
            {
                throw new PlatformNotSupportedException(System.SR.GetString("Perflib_PlatformNotSupported"));
            }
            if (!PerfProviderCollection.ValidateCounterSetInstanceType(instanceType))
            {
                throw new ArgumentException(System.SR.GetString("Perflib_Argument_InvalidCounterSetInstanceType", new object[] { instanceType }), "instanceType");
            }
            this.m_providerGuid = providerGuid;
            this.m_counterSet = counterSetGuid;
            this.m_instType = instanceType;
            PerfProviderCollection.RegisterCounterSet(this.m_counterSet);
            this.m_provider = PerfProviderCollection.QueryProvider(this.m_providerGuid);
            this.m_lockObject = new object();
            this.m_stringToId = new Dictionary<string, int>();
            this.m_idToCounter = new Dictionary<int, CounterType>();
        }

        public void AddCounter(int counterId, CounterType counterType)
        {
            if (this.m_provider == null)
            {
                throw new InvalidOperationException(System.SR.GetString("Perflib_InvalidOperation_NoActiveProvider", new object[] { this.m_providerGuid }));
            }
            if (!PerfProviderCollection.ValidateCounterType(counterType))
            {
                throw new ArgumentException(System.SR.GetString("Perflib_Argument_InvalidCounterType", new object[] { counterType }), "counterType");
            }
            if (this.m_instanceCreated)
            {
                throw new InvalidOperationException(System.SR.GetString("Perflib_InvalidOperation_AddCounterAfterInstance", new object[] { this.m_counterSet }));
            }
            lock (this.m_lockObject)
            {
                if (this.m_instanceCreated)
                {
                    throw new InvalidOperationException(System.SR.GetString("Perflib_InvalidOperation_AddCounterAfterInstance", new object[] { this.m_counterSet }));
                }
                if (this.m_idToCounter.ContainsKey(counterId))
                {
                    throw new ArgumentException(System.SR.GetString("Perflib_Argument_CounterAlreadyExists", new object[] { counterId, this.m_counterSet }), "CounterId");
                }
                this.m_idToCounter.Add(counterId, counterType);
            }
        }

        public void AddCounter(int counterId, CounterType counterType, string counterName)
        {
            if (counterName == null)
            {
                throw new ArgumentNullException("CounterName");
            }
            if (counterName.Length == 0)
            {
                throw new ArgumentException(System.SR.GetString("Perflib_Argument_EmptyCounterName"), "counterName");
            }
            if (!PerfProviderCollection.ValidateCounterType(counterType))
            {
                throw new ArgumentException(System.SR.GetString("Perflib_Argument_InvalidCounterType", new object[] { counterType }), "counterType");
            }
            if (this.m_provider == null)
            {
                throw new InvalidOperationException(System.SR.GetString("Perflib_InvalidOperation_NoActiveProvider", new object[] { this.m_providerGuid }));
            }
            if (this.m_instanceCreated)
            {
                throw new InvalidOperationException(System.SR.GetString("Perflib_InvalidOperation_AddCounterAfterInstance", new object[] { this.m_counterSet }));
            }
            lock (this.m_lockObject)
            {
                if (this.m_instanceCreated)
                {
                    throw new InvalidOperationException(System.SR.GetString("Perflib_InvalidOperation_AddCounterAfterInstance", new object[] { this.m_counterSet }));
                }
                if (this.m_stringToId.ContainsKey(counterName))
                {
                    throw new ArgumentException(System.SR.GetString("Perflib_Argument_CounterNameAlreadyExists", new object[] { counterName, this.m_counterSet }), "CounterName");
                }
                if (this.m_idToCounter.ContainsKey(counterId))
                {
                    throw new ArgumentException(System.SR.GetString("Perflib_Argument_CounterAlreadyExists", new object[] { counterId, this.m_counterSet }), "CounterId");
                }
                this.m_stringToId.Add(counterName, counterId);
                this.m_idToCounter.Add(counterId, counterType);
            }
        }

        [SecuritySafeCritical, PermissionSet(SecurityAction.Demand, Unrestricted=true)]
        public unsafe CounterSetInstance CreateCounterSetInstance(string instanceName)
        {
            if (instanceName == null)
            {
                throw new ArgumentNullException("instanceName");
            }
            if (instanceName.Length == 0)
            {
                throw new ArgumentException(System.SR.GetString("Perflib_Argument_EmptyInstanceName"), "instanceName");
            }
            if (this.m_provider == null)
            {
                throw new InvalidOperationException(System.SR.GetString("Perflib_InvalidOperation_NoActiveProvider", new object[] { this.m_providerGuid }));
            }
            if (!this.m_instanceCreated)
            {
                lock (this.m_lockObject)
                {
                    if (!this.m_instanceCreated)
                    {
                        if (this.m_provider == null)
                        {
                            throw new ArgumentException(System.SR.GetString("Perflib_Argument_ProviderNotFound", new object[] { this.m_providerGuid }), "ProviderGuid");
                        }
                        if (this.m_provider.m_hProvider.IsInvalid)
                        {
                            throw new InvalidOperationException(System.SR.GetString("Perflib_InvalidOperation_NoActiveProvider", new object[] { this.m_providerGuid }));
                        }
                        if (this.m_idToCounter.Count == 0)
                        {
                            throw new InvalidOperationException(System.SR.GetString("Perflib_InvalidOperation_CounterSetContainsNoCounter", new object[] { this.m_counterSet }));
                        }
                        uint num = 0;
                        uint dwTemplateSize = (uint) (sizeof(Microsoft.Win32.UnsafeNativeMethods.PerfCounterSetInfoStruct) + (this.m_idToCounter.Count * sizeof(Microsoft.Win32.UnsafeNativeMethods.PerfCounterInfoStruct)));
                        uint num3 = 0;
                        byte* numPtr = stackalloc byte[dwTemplateSize];
                        if (numPtr == null)
                        {
                            throw new InsufficientMemoryException(System.SR.GetString("Perflib_InsufficientMemory_CounterSetTemplate", new object[] { this.m_counterSet, dwTemplateSize }));
                        }
                        uint num4 = 0;
                        uint num5 = 0;
                        Microsoft.Win32.UnsafeNativeMethods.PerfCounterSetInfoStruct* pTemplate = (Microsoft.Win32.UnsafeNativeMethods.PerfCounterSetInfoStruct*) numPtr;
                        pTemplate->CounterSetGuid = this.m_counterSet;
                        pTemplate->ProviderGuid = this.m_providerGuid;
                        pTemplate->NumCounters = (uint) this.m_idToCounter.Count;
                        pTemplate->InstanceType = (uint) this.m_instType;
                        foreach (KeyValuePair<int, CounterType> pair in this.m_idToCounter)
                        {
                            num3 = (uint) (sizeof(Microsoft.Win32.UnsafeNativeMethods.PerfCounterSetInfoStruct) + (num4 * sizeof(Microsoft.Win32.UnsafeNativeMethods.PerfCounterInfoStruct)));
                            if (num3 < dwTemplateSize)
                            {
                                Microsoft.Win32.UnsafeNativeMethods.PerfCounterInfoStruct* structPtr2 = (Microsoft.Win32.UnsafeNativeMethods.PerfCounterInfoStruct*) (numPtr + num3);
                                structPtr2->CounterId = pair.Key;
                                structPtr2->CounterType = (int)pair.Value;
                                structPtr2->Attrib = 1L;
                                structPtr2->Size = (uint) sizeof(void*);
                                structPtr2->DetailLevel = 100;
                                structPtr2->Scale = 0;
                                structPtr2->Offset = num5;
                                num5 += structPtr2->Size;
                            }
                            num4++;
                        }
                        num = Microsoft.Win32.UnsafeNativeMethods.PerfSetCounterSetInfo(this.m_provider.m_hProvider, pTemplate, dwTemplateSize);
                        if (num != 0)
                        {
                            switch (num)
                            {
                                case 0xb7:
                                    throw new ArgumentException(System.SR.GetString("Perflib_Argument_CounterSetAlreadyRegister", new object[] { this.m_counterSet }), "CounterSetGuid");
                            }
                            throw new Win32Exception((int) num);
                        }
                        Interlocked.Increment(ref this.m_provider.m_counterSet);
                        this.m_instanceCreated = true;
                    }
                }
            }
            return new CounterSetInstance(this, instanceName);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SecuritySafeCritical]
        protected virtual void Dispose(bool disposing)
        {
            lock (this)
            {
                PerfProviderCollection.UnregisterCounterSet(this.m_counterSet);
                if (this.m_instanceCreated && (this.m_provider != null))
                {
                    lock (this.m_lockObject)
                    {
                        if (this.m_provider != null)
                        {
                            Interlocked.Decrement(ref this.m_provider.m_counterSet);
                            if (this.m_provider.m_counterSet <= 0)
                            {
                                PerfProviderCollection.RemoveProvider(this.m_providerGuid);
                            }
                            this.m_provider = null;
                        }
                    }
                }
            }
        }

        ~CounterSet()
        {
            this.Dispose(false);
        }
    }
}

