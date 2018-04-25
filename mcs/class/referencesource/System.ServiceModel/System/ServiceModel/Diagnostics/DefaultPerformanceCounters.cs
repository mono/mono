//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;

    internal class DefaultPerformanceCounters : PerformanceCountersBase
    {
        string instanceName;

        enum PerfCounters : int
        {
            Instances = 0,
            TotalCounters = Instances + 1
        }

        string[] perfCounterNames = 
        {
            PerformanceCounterStrings.SERVICEMODELSERVICE.SInstances,
        };

        const int maxCounterLength = 64;
        const int hashLength = 2;
        [Flags]
        enum truncOptions : uint
        {
            NoBits = 0,
            service32 = 0x01,
            uri31 = 0x04
        }

        internal PerformanceCounter[] Counters { get; set; }

        internal override string InstanceName
        {
            get
            {
                return this.instanceName;
            }
        }

        internal override string[] CounterNames
        {
            get
            {
                return this.perfCounterNames;
            }
        }

        internal override int PerfCounterStart
        {
            get { return (int)PerfCounters.Instances; }
        }

        internal override int PerfCounterEnd
        {
            get { return (int)PerfCounters.TotalCounters; }
        }

        static internal string CreateFriendlyInstanceName(ServiceHostBase serviceHost)
        {
            // It is a shared instance across all services which have the default counter enabled
            return "_WCF_Admin";
        }

        internal DefaultPerformanceCounters(ServiceHostBase serviceHost)
        {
            this.instanceName = DefaultPerformanceCounters.CreateFriendlyInstanceName(serviceHost);
            this.Counters = new PerformanceCounter[(int)PerfCounters.TotalCounters];
            for (int i = 0; i < (int)PerfCounters.TotalCounters; i++)
            {
                try
                {
                    PerformanceCounter counter = PerformanceCounters.GetDefaultPerformanceCounter(this.perfCounterNames[i], this.instanceName);
                    if (counter != null)
                    {
                        this.Counters[i] = counter;
                    }
                    else
                    {
                        break;
                    }
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Error, TraceCode.PerformanceCountersFailedForService,
                            SR.GetString(SR.TraceCodePerformanceCountersFailedForService), null, e);
                    }
                    break;
                }
            }
        }

        internal override bool Initialized
        {
            get { return this.Counters != null; }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (PerformanceCounters.PerformanceCountersEnabled)
                    {
                        if (null != this.Counters)
                        {
                            for (int ctr = this.PerfCounterStart; ctr < this.PerfCounterEnd; ++ctr)
                            {
                                PerformanceCounter counter = this.Counters[ctr];
                                if (counter != null)
                                {
                                    PerformanceCounters.ReleasePerformanceCounter(ref counter);
                                }
                                this.Counters[ctr] = null;
                            }
                            this.Counters = null;
                        }
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}
