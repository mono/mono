namespace System.Diagnostics.PerformanceData
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.ComponentModel;
    using System.Security;

    internal sealed class PerfProvider
    {
        internal int m_counterSet;
        [SecurityCritical]
        internal SafePerfProviderHandle m_hProvider;
        internal Guid m_providerGuid;

        [SecurityCritical]
        internal PerfProvider(Guid providerGuid)
        {
            this.m_providerGuid = providerGuid;
            uint num = Microsoft.Win32.UnsafeNativeMethods.PerfStartProvider(ref this.m_providerGuid, null, out this.m_hProvider);
            if (num != 0)
            {
                throw new Win32Exception((int) num);
            }
        }
    }
}

