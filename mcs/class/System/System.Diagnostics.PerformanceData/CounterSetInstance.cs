namespace System.Diagnostics.PerformanceData
{
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class CounterSetInstance : IDisposable
    {
        private int m_active;
        private CounterSetInstanceCounterDataSet m_counters;
        internal CounterSet m_counterSet;
        internal string m_instName;
        [SecurityCritical]
        internal unsafe Microsoft.Win32.UnsafeNativeMethods.PerfCounterSetInstanceStruct* m_nativeInst;

        [SecurityCritical]
        internal unsafe CounterSetInstance(CounterSet counterSetDefined, string instanceName)
        {
            if (counterSetDefined == null)
            {
                throw new ArgumentNullException("counterSetDefined");
            }
            if (instanceName == null)
            {
                throw new ArgumentNullException("InstanceName");
            }
            if (instanceName.Length == 0)
            {
                throw new ArgumentException(System.SR.GetString("Perflib_Argument_EmptyInstanceName"), "InstanceName");
            }
            this.m_counterSet = counterSetDefined;
            this.m_instName = instanceName;
            this.m_nativeInst = Microsoft.Win32.UnsafeNativeMethods.PerfCreateInstance(this.m_counterSet.m_provider.m_hProvider, ref this.m_counterSet.m_counterSet, this.m_instName, 0);
            int error = (this.m_nativeInst != null) ? 0 : Marshal.GetLastWin32Error();
            if (error == 0)
            {
                this.m_counters = new CounterSetInstanceCounterDataSet(this);
            }
            else
            {
                switch (error)
                {
                    case 0x57:
                        if (this.m_counterSet.m_instType == CounterSetInstanceType.Single)
                        {
                            throw new ArgumentException(System.SR.GetString("Perflib_Argument_InvalidInstance", new object[] { this.m_counterSet.m_counterSet }), "InstanceName");
                        }
                        throw new Win32Exception(error);

                    case 0xb7:
                        throw new ArgumentException(System.SR.GetString("Perflib_Argument_InstanceAlreadyExists", new object[] { this.m_instName, this.m_counterSet.m_counterSet }), "InstanceName");

                    case 0x490:
                        throw new InvalidOperationException(System.SR.GetString("Perflib_InvalidOperation_CounterSetNotInstalled", new object[] { this.m_counterSet.m_counterSet }));
                }
                throw new Win32Exception(error);
            }
            this.m_active = 1;
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
            if (disposing && (this.m_counters != null))
            {
                this.m_counters.Dispose();
                this.m_counters = null;
            }
            if (((this.m_nativeInst != null) && (Interlocked.Exchange(ref this.m_active, 0) != 0)) && (this.m_nativeInst != null))
            {
                lock (this.m_counterSet)
                {
                    if (this.m_counterSet.m_provider != null)
                    {
                        Microsoft.Win32.UnsafeNativeMethods.PerfDeleteInstance(this.m_counterSet.m_provider.m_hProvider, this.m_nativeInst);
                    }
                    this.m_nativeInst = null;
                }
            }
        }

        [SecurityCritical]
        ~CounterSetInstance()
        {
            this.Dispose(false);
        }

        public CounterSetInstanceCounterDataSet Counters
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_counters;
            }
        }
    }
}

