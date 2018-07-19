namespace System.ServiceModel.Activities.Diagnostics
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.PerformanceData;
    using System.Runtime;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Administration;
    using System.ServiceModel.Diagnostics;

    sealed class WorkflowServiceHostPerformanceCounters : PerformanceCountersBase
    {
        static object syncRoot = new object();
        static Guid workflowServiceHostProviderId = new Guid("{f6c5ad57-a5be-4259-9060-b2c4ebfccd96}");
        static Guid workflowServiceHostCounterSetId = new Guid("{1f7207c2-0b8c-48de-9dcd-64ff98cc24e1}");

        // Double-checked locking pattern requires volatile for read/write synchronization
        static volatile CounterSet workflowServiceHostCounterSet;         // Defines the counter set

        // The strings defined here are not used in the counter set and defined only to implement CounterNames property.
        // CounterNames is not used currently and hence these strings need not be localized. 
        // The Counter Names and description are defined in the manifest which will be localized and installed by the setup.
        static readonly string[] perfCounterNames = 
        {
            "Workflows Created",
            "Workflows Created Per Second",
            "Workflows Executing",
            "Workflows Completed",
            "Workflows Completed Per Second",
            "Workflows Aborted",
            "Workflows Aborted Per Second",
            "Workflows In Memory",
            "Workflows Persisted",
            "Workflows Persisted Per Second",
            "Workflows Terminated",
            "Workflows Terminated Per Second",
            "Workflows Loaded",
            "Workflows Loaded Per Second",
            "Workflows Unloaded",
            "Workflows Unloaded Per Second",
            "Workflows Suspended",
            "Workflows Suspended Per Second",
            "Workflows Idle Per Second",
            "Average Workflow Load Time",
            "Average Workflow Load Time Base",
            "Average Workflow Persist Time",
            "Average Workflow Persist Time Base",
        };

        WorkflowServiceHost serviceHost;

        const int maxCounterLength = 64;
        const int hashLength = 2;

        CounterSetInstance workflowServiceHostCounterSetInstance; // Instance of the counter set
        bool initialized;
        CounterData[] counters;

        string instanceName;
        bool isPerformanceCounterEnabled;

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
                return perfCounterNames;
            }
        }

        internal override int PerfCounterStart
        {
            get 
            { 
                return (int)PerfCounters.WorkflowsCreated;
            }
        }

        internal override int PerfCounterEnd
        {
            get 
            { 
                return (int)PerfCounters.TotalCounters; 
            }
        }

        internal bool PerformanceCountersEnabled
        {
            get 
            { 
                return this.isPerformanceCounterEnabled;
            }
        }

        internal override bool Initialized
        {
            get { return this.initialized; }
        }
                
        internal WorkflowServiceHostPerformanceCounters(WorkflowServiceHost serviceHost)
        {
            this.serviceHost = serviceHost;
        }

        internal static void EnsureCounterSet()
        {
            if (workflowServiceHostCounterSet == null)
            {
                lock (syncRoot)
                {
                    if (workflowServiceHostCounterSet == null)
                    {
                        CounterSet localCounterSet = CreateCounterSet();

                        // Add the counters to the counter set definition.
                        localCounterSet.AddCounter((int)PerfCounters.WorkflowsCreated, CounterType.RawData32);
                        localCounterSet.AddCounter((int)PerfCounters.WorkflowsCreatedPerSecond, CounterType.RateOfCountPerSecond32);
                        localCounterSet.AddCounter((int)PerfCounters.WorkflowsExecuting, CounterType.RawData32);
                        localCounterSet.AddCounter((int)PerfCounters.WorkflowsCompleted, CounterType.RawData32);
                        localCounterSet.AddCounter((int)PerfCounters.WorkflowsCompletedPerSecond, CounterType.RateOfCountPerSecond32);
                        localCounterSet.AddCounter((int)PerfCounters.WorkflowsAborted, CounterType.RawData32);
                        localCounterSet.AddCounter((int)PerfCounters.WorkflowsAbortedPerSecond, CounterType.RateOfCountPerSecond32);
                        localCounterSet.AddCounter((int)PerfCounters.WorkflowsInMemory, CounterType.RawData32);
                        localCounterSet.AddCounter((int)PerfCounters.WorkflowsPersisted, CounterType.RawData32);
                        localCounterSet.AddCounter((int)PerfCounters.WorkflowsPersistedPerSecond, CounterType.RateOfCountPerSecond32);
                        localCounterSet.AddCounter((int)PerfCounters.WorkflowsTerminated, CounterType.RawData32);
                        localCounterSet.AddCounter((int)PerfCounters.WorkflowsTerminatedPerSecond, CounterType.RateOfCountPerSecond32);
                        localCounterSet.AddCounter((int)PerfCounters.WorkflowsLoaded, CounterType.RawData32);
                        localCounterSet.AddCounter((int)PerfCounters.WorkflowsLoadedPerSecond, CounterType.RateOfCountPerSecond32);
                        localCounterSet.AddCounter((int)PerfCounters.WorkflowsUnloaded, CounterType.RawData32);
                        localCounterSet.AddCounter((int)PerfCounters.WorkflowsUnloadedPerSecond, CounterType.RateOfCountPerSecond32);
                        localCounterSet.AddCounter((int)PerfCounters.WorkflowsSuspended, CounterType.RawData32, perfCounterNames[(int)PerfCounters.WorkflowsSuspended]);
                        localCounterSet.AddCounter((int)PerfCounters.WorkflowsSuspendedPerSecond, CounterType.RateOfCountPerSecond32);
                        localCounterSet.AddCounter((int)PerfCounters.WorkflowsIdlePerSecond, CounterType.RateOfCountPerSecond32);
                        localCounterSet.AddCounter((int)PerfCounters.AverageWorkflowLoadTime, CounterType.AverageTimer32);
                        localCounterSet.AddCounter((int)PerfCounters.AverageWorkflowLoadTimeBase, CounterType.AverageBase);
                        localCounterSet.AddCounter((int)PerfCounters.AverageWorkflowPersistTime, CounterType.AverageTimer32);
                        localCounterSet.AddCounter((int)PerfCounters.AverageWorkflowPersistTimeBase, CounterType.AverageBase);

                        workflowServiceHostCounterSet = localCounterSet;
                    }
                }
            }
        }

        static internal string CreateFriendlyInstanceName(ServiceHostBase serviceHost)
        {
            // instance name is: serviceName@uri
            ServiceInfo serviceInfo = new ServiceInfo(serviceHost);
            string serviceName = serviceInfo.ServiceName;
            string uri;
            if (!TryGetFullVirtualPath(serviceHost, out uri))
            {
                uri = serviceInfo.FirstAddress;
            }
            int length = serviceName.Length + uri.Length + 2;

            if (length > maxCounterLength)
            {
                int count = 0;

                InstanceNameTruncOptions tasks = GetCompressionTasks(
                    length, serviceName.Length, uri.Length);

                //if necessary, compress service name to 8 chars with a 2 char hash code
                if ((tasks & InstanceNameTruncOptions.Service32) > 0)
                {
                    count = 32;
                    serviceName = GetHashedString(serviceName, count - hashLength, serviceName.Length - count + hashLength, true);
                }

                //if necessary,  compress uri to 36 chars with a 2 char hash code
                if ((tasks & InstanceNameTruncOptions.Uri31) > 0)
                {
                    count = 31;
                    uri = GetHashedString(uri, 0, uri.Length - count + hashLength, false);
                }
            }

            // replace '/' with '|' because perfmon fails when '/' is in perfcounter instance name
            return serviceName + "@" + uri.Replace('/', '|');
        }

        static bool TryGetFullVirtualPath(ServiceHostBase serviceHost, out string uri)
        {
            VirtualPathExtension pathExtension = serviceHost.Extensions.Find<VirtualPathExtension>();
            if (pathExtension == null)
            {
                uri = null;
                return false;
            }
            uri = pathExtension.ApplicationVirtualPath + pathExtension.VirtualPath.ToString().Replace("~", "");
            return uri != null;
        }

        static InstanceNameTruncOptions GetCompressionTasks(int totalLen, int serviceLen, int uriLen)
        {
            InstanceNameTruncOptions bitmask = 0;

            if (totalLen > maxCounterLength)
            {
                int workingLen = totalLen;

                //note: order of if statements important (see spec)!
                if (workingLen > maxCounterLength && serviceLen > 32)
                {
                    bitmask |= InstanceNameTruncOptions.Service32; //compress service name to 16 chars
                    workingLen -= serviceLen - 32;
                }
                if (workingLen > maxCounterLength && uriLen > 31)
                {
                    bitmask |= InstanceNameTruncOptions.Uri31; //compress uri to 31 chars
                }
            }

            return bitmask;
        }

        [Fx.Tag.SecurityNote(Critical = "Calls into Sys.Diag.PerformanceData.CounterSet..ctor marked as SecurityCritical", Safe = "We only make the call if PartialTrustHelper.AppDomainFullyTrusted is true.")]
        [SecuritySafeCritical]
        static CounterSet CreateCounterSet()
        {
            if (PartialTrustHelpers.AppDomainFullyTrusted)
            {
                return new CounterSet(workflowServiceHostProviderId, workflowServiceHostCounterSetId, CounterSetInstanceType.Multiple);
            }
            else
                return null;
        }

        [Fx.Tag.SecurityNote(Critical = "Calls into Sys.Diag.PerformanceData.CounterSetInstance.CreateCounterSetInstance marked as SecurityCritical",
            Safe = "We only make the call if PartialTrustHelper.AppDomainFullyTrusted is true.")]
        [SecuritySafeCritical]
        static CounterSetInstance CreateCounterSetInstance(string name)
        {
            CounterSetInstance workflowServiceHostCounterSetInstance = null;

            if (PartialTrustHelpers.AppDomainFullyTrusted)
            {
                try
                {
                    workflowServiceHostCounterSetInstance = workflowServiceHostCounterSet.CreateCounterSetInstance(name);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    // A conflicting instance name already exists and probably the unmanaged resource is not yet disposed. 
                    FxTrace.Exception.AsWarning(exception);
                    workflowServiceHostCounterSetInstance = null;
                }
            }

            return workflowServiceHostCounterSetInstance;
        }

        internal void InitializePerformanceCounters()
        {
            this.instanceName = CreateFriendlyInstanceName(this.serviceHost);

            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                EnsureCounterSet();
                // Create an instance of the counter set (contains the counter data).
                this.workflowServiceHostCounterSetInstance = CreateCounterSetInstance(this.InstanceName);

                if (this.workflowServiceHostCounterSetInstance != null)
                {
                    this.counters = new CounterData[(int)PerfCounters.TotalCounters]; 
                    for (int i = 0; i < (int)PerfCounters.TotalCounters; i++)
                    {
                        this.counters[i] = this.workflowServiceHostCounterSetInstance.Counters[i];
                        this.counters[i].Value = 0;
                    }
                    // Enable perf counter only if CounterSetInstance is created without instance name conflict. 
                    this.isPerformanceCounterEnabled = PerformanceCounters.PerformanceCountersEnabled;
                }
            }
            this.initialized = true;
        }
        
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && this.workflowServiceHostCounterSetInstance != null)
                {
                    this.workflowServiceHostCounterSetInstance.Dispose();
                    this.workflowServiceHostCounterSetInstance = null;
                    this.isPerformanceCounterEnabled = false;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        internal void WorkflowCreated()
        {
            if (PerformanceCountersEnabled)
            {
                this.counters[(int)PerfCounters.WorkflowsCreated].Increment();
                this.counters[(int)PerfCounters.WorkflowsCreatedPerSecond].Increment();
            }
        }
        
        internal void WorkflowExecuting(bool increment)
        {
            if (PerformanceCountersEnabled)
            {
                if (increment)
                {
                    this.counters[(int)PerfCounters.WorkflowsExecuting].Increment();
                }
                else
                {
                    this.counters[(int)PerfCounters.WorkflowsExecuting].Decrement();
                }
            }
        }

        internal void WorkflowCompleted()
        {
            if (PerformanceCountersEnabled)
            {
                this.counters[(int)PerfCounters.WorkflowsCompleted].Increment();
                this.counters[(int)PerfCounters.WorkflowsCompletedPerSecond].Increment();
            }
        }
        
        internal void WorkflowAborted()
        {
            if (PerformanceCountersEnabled)
            {
                this.counters[(int)PerfCounters.WorkflowsAborted].Increment();
                this.counters[(int)PerfCounters.WorkflowsAbortedPerSecond].Increment();
            }
        }

        internal void WorkflowInMemory()
        {
            if (PerformanceCountersEnabled)
            {
                this.counters[(int)PerfCounters.WorkflowsInMemory].Increment();
            }
        }

        internal void WorkflowOutOfMemory()
        {
            if (PerformanceCountersEnabled)
            {
                if (this.counters[(int)PerfCounters.WorkflowsInMemory].RawValue > 0 )
                    this.counters[(int)PerfCounters.WorkflowsInMemory].Decrement();
            }
        }

        internal void WorkflowPersisted()
        {
            if (PerformanceCountersEnabled)
            {
                this.counters[(int)PerfCounters.WorkflowsPersisted].Increment();
                this.counters[(int)PerfCounters.WorkflowsPersistedPerSecond].Increment();
            }
        }

        internal void WorkflowTerminated()
        {
            if (PerformanceCountersEnabled)
            {
                this.counters[(int)PerfCounters.WorkflowsTerminated].Increment();
                this.counters[(int)PerfCounters.WorkflowsTerminatedPerSecond].Increment();
            }
        }

        internal void WorkflowLoaded()
        {
            if (PerformanceCountersEnabled)
            {
                this.counters[(int)PerfCounters.WorkflowsLoaded].Increment();
                this.counters[(int)PerfCounters.WorkflowsLoadedPerSecond].Increment();
            }
        }

        internal void WorkflowUnloaded()
        {
            if (PerformanceCountersEnabled)
            {
                this.counters[(int)PerfCounters.WorkflowsUnloaded].Increment();
                this.counters[(int)PerfCounters.WorkflowsUnloadedPerSecond].Increment();
            }
        }

        internal void WorkflowSuspended()
        {
            if (PerformanceCountersEnabled)
            {
                this.counters[(int)PerfCounters.WorkflowsSuspended].Increment();
                this.counters[(int)PerfCounters.WorkflowsSuspendedPerSecond].Increment();
            }
        }

        internal void WorkflowIdle()
        {
            if (PerformanceCountersEnabled)
            {
                this.counters[(int)PerfCounters.WorkflowsIdlePerSecond].Increment();
            }
        }

        internal void WorkflowLoadDuration(long time)
        {
            if (PerformanceCountersEnabled)
            {
                    this.counters[(int)PerfCounters.AverageWorkflowLoadTime].IncrementBy(time);
                    this.counters[(int)PerfCounters.AverageWorkflowLoadTimeBase].Increment();
            }
        }

        internal void WorkflowPersistDuration(long time)
        {
            if (PerformanceCountersEnabled)
            {
                    this.counters[(int)PerfCounters.AverageWorkflowPersistTime].IncrementBy(time);
                    this.counters[(int)PerfCounters.AverageWorkflowPersistTimeBase].Increment();
            }
        }

        internal enum PerfCounters : int
        {
            WorkflowsCreated = 0,
            WorkflowsCreatedPerSecond,
            WorkflowsExecuting,
            WorkflowsCompleted,
            WorkflowsCompletedPerSecond,
            WorkflowsAborted,
            WorkflowsAbortedPerSecond,
            WorkflowsInMemory,
            WorkflowsPersisted,
            WorkflowsPersistedPerSecond,
            WorkflowsTerminated,
            WorkflowsTerminatedPerSecond,
            WorkflowsLoaded,
            WorkflowsLoadedPerSecond,
            WorkflowsUnloaded,
            WorkflowsUnloadedPerSecond,
            WorkflowsSuspended,
            WorkflowsSuspendedPerSecond,
            WorkflowsIdlePerSecond,
            AverageWorkflowLoadTime,
            AverageWorkflowLoadTimeBase,
            AverageWorkflowPersistTime,
            AverageWorkflowPersistTimeBase,            
            TotalCounters = AverageWorkflowPersistTimeBase + 1
        }

        // Truncate options for the Instance name in ServiceName@uri format. 
        [Flags]
        enum InstanceNameTruncOptions : uint
        {
            NoBits = 0,
            Service32 = 0x01, //compress service name to 16 chars
            Uri31 = 0x04      //compress uri to 31 chars
        }
        
    }
}
