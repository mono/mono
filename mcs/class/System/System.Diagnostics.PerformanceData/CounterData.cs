namespace System.Diagnostics.PerformanceData
{
    using System;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class CounterData
    {
        [SecurityCritical]
        private unsafe long m_offset;

        [SecurityCritical]
        internal unsafe CounterData(long pCounterData)
        {
            this.m_offset = pCounterData;
            this.m_offset = 0L;
        }

        [SecurityCritical]
        public unsafe void Decrement()
        {
            Interlocked.Decrement(ref this.m_offset);
        }

        [SecurityCritical]
        public unsafe void Increment()
        {
            Interlocked.Increment(ref this.m_offset);
        }

        [SecurityCritical]
        public unsafe void IncrementBy(long value)
        {
            Interlocked.Add(ref this.m_offset, value);
        }

        public long RawValue
        {
            [SecurityCritical]
            get
            {
                return this.m_offset;
            }
            [SecurityCritical]
            set
            {
                this.m_offset = value;
            }
        }

        public long Value
        {
            [SecurityCritical]
            get
            {
                return Interlocked.Read(ref this.m_offset);
            }
            [SecurityCritical]
            set
            {
                Interlocked.Exchange(ref this.m_offset, value);
            }
        }
    }
}

