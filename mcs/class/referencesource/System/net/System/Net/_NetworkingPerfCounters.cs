//------------------------------------------------------------------------------
// <copyright file="_NetworkingPerfCounters.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net
{
    using System;
    using System.Reflection;
    using System.Net.Sockets;
    using System.Diagnostics;
    using System.Security.Permissions;
    using System.ComponentModel;
    using System.Globalization;
    using System.Text;
    using System.Runtime.Versioning;
    using System.Threading;
    using System.Net.Configuration;

    internal enum NetworkingPerfCounterName
    {
        SocketConnectionsEstablished = 0, // these enum values are used as index
        SocketBytesReceived,
        SocketBytesSent,
        SocketDatagramsReceived,
        SocketDatagramsSent,
        HttpWebRequestCreated,
        HttpWebRequestAvgLifeTime,
        HttpWebRequestAvgLifeTimeBase,
        HttpWebRequestQueued,
        HttpWebRequestAvgQueueTime,
        HttpWebRequestAvgQueueTimeBase,
        HttpWebRequestAborted,
        HttpWebRequestFailed
    }

    internal sealed class NetworkingPerfCounters
    {

        private class CounterPair
        {
            private PerformanceCounter instanceCounter;
            private PerformanceCounter globalCounter;

            public PerformanceCounter InstanceCounter
            {
                get { return instanceCounter; }
            }

            public PerformanceCounter GlobalCounter
            {
                get { return globalCounter; }
            }

            public CounterPair(PerformanceCounter instanceCounter, PerformanceCounter globalCounter)
            {
                Debug.Assert(instanceCounter != null);
                Debug.Assert(globalCounter != null);

                this.instanceCounter = instanceCounter;
                this.globalCounter = globalCounter;
            }
        }

        private const int instanceNameMaxLength = 127;
        private const string categoryName = ".NET CLR Networking 4.0.0.0";
        private const string globalInstanceName = "_Global_";
        private static readonly string[] counterNames = {
                                                            "Connections Established",
                                                            "Bytes Received",
                                                            "Bytes Sent",
                                                            "Datagrams Received",
                                                            "Datagrams Sent",
                                                            "HttpWebRequests Created/Sec",
                                                            "HttpWebRequests Average Lifetime",
                                                            "HttpWebRequests Average Lifetime Base",
                                                            "HttpWebRequests Queued/Sec",
                                                            "HttpWebRequests Average Queue Time",
                                                            "HttpWebRequests Average Queue Time Base",
                                                            "HttpWebRequests Aborted/Sec",
                                                            "HttpWebRequests Failed/Sec",
                                                        };

        private static volatile NetworkingPerfCounters instance;
        private static object lockObject = new object();

        private volatile bool initDone; // keep this volatile to prevent load-load reordering
        private bool initSuccessful;
        private CounterPair[] counters;
        private bool enabled;
        private volatile bool cleanupCalled;

        private NetworkingPerfCounters()
        {
            enabled = SettingsSectionInternal.Section.PerformanceCountersEnabled;
        }

        public static NetworkingPerfCounters Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            CreateInstance();
                        }
                    }
                }
                return instance;
            }
        }

        public static long GetTimestamp()
        {
            return Stopwatch.GetTimestamp();
        }

        public bool Enabled
        {
            get { return enabled; }
        }

        public void Increment(NetworkingPerfCounterName perfCounter)
        {
            Increment(perfCounter, 1);
        }

        public void Increment(NetworkingPerfCounterName perfCounter, long amount)
        {
            if (CounterAvailable())
            {
                try
                {
                    CounterPair cp = counters[(int)perfCounter];
                    Debug.Assert(cp != null);
                    cp.InstanceCounter.IncrementBy(amount);
                    cp.GlobalCounter.IncrementBy(amount);
                }
                // in case there is something wrong with the counter instance, just log and continue
                catch (InvalidOperationException e)
                {
                    if (Logging.On) Logging.Exception(Logging.Web, "NetworkingPerfCounters", "Increment", e);
                }
                catch (Win32Exception e)
                {
                    if (Logging.On) Logging.Exception(Logging.Web, "NetworkingPerfCounters", "Increment", e);
                }
            }
        }

        public void Decrement(NetworkingPerfCounterName perfCounter)
        {
            Increment(perfCounter, -1);
        }

        public void Decrement(NetworkingPerfCounterName perfCounter, long amount)
        {
            Increment(perfCounter, -amount);
        }

        public void IncrementAverage(NetworkingPerfCounterName perfCounter, long startTimestamp)
        {
            if (CounterAvailable())
            {
                long stopTimestamp = GetTimestamp();
                int avgCounterIndex = (int)perfCounter;
                Debug.Assert(avgCounterIndex + 1 < counters.Length);

                long duration = ((stopTimestamp - startTimestamp) * 1000) / Stopwatch.Frequency;
                Increment(perfCounter, duration);

                // base counter is always the next one (otherwise we wouldn't even be able to initialize the counters)
                Increment(perfCounter + 1, 1);
            }
        }

        private void Initialize(object state)
        {
            if (Logging.On) Logging.PrintInfo(Logging.Web, SR.GetString(SR.net_perfcounter_initialization_started));

            PerformanceCounterPermission perfCounterPermission = new PerformanceCounterPermission(PermissionState.Unrestricted);
            perfCounterPermission.Assert();
            try
            {
                if (!PerformanceCounterCategory.Exists(categoryName))
                {
                    // if the perf. counter category doesn't exist, just log this information and exit.
                    if (Logging.On) Logging.PrintError(Logging.Web, SR.GetString(SR.net_perfcounter_nocategory, categoryName));
                    return;
                }

                string instanceName = GetInstanceName();

                Debug.Assert(counterNames.Length == Enum.GetValues(typeof(NetworkingPerfCounterName)).Length,
                    "The number of NetworkingPerfCounterName items must match the number of CounterNames");

                // create the counters, this will check for the right permissions (false)
                // means the counter is not readonly (it's read/write) and cache them while
                // we're under the Assert(), which will be reverted in the finally below.
                counters = new CounterPair[counterNames.Length];
                for (int i = 0; i < counterNames.Length; i++)
                {
                    counters[i] = CreateCounterPair(counterNames[i], instanceName);
                }

                AppDomain.CurrentDomain.DomainUnload += new EventHandler(UnloadEventHandler);
                AppDomain.CurrentDomain.ProcessExit += new EventHandler(ExitEventHandler);
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(ExceptionEventHandler);

                initSuccessful = true;
            }
            catch (Win32Exception e)
            {
                if (Logging.On) Logging.Exception(Logging.Web, "NetworkingPerfCounters", "Initialize", e);
                Cleanup();
                return;
            }
            catch (InvalidOperationException e)
            {
                if (Logging.On) Logging.Exception(Logging.Web, "NetworkingPerfCounters", "Initialize", e);
                Cleanup();
                return;
            }
            finally
            {
                PerformanceCounterPermission.RevertAssert();

                initDone = true;

                if (Logging.On)
                {
                    if (initSuccessful)
                    {
                        Logging.PrintInfo(Logging.Web, SR.GetString(SR.net_perfcounter_initialized_success));
                    }
                    else
                    {
                        Logging.PrintInfo(Logging.Web, SR.GetString(SR.net_perfcounter_initialized_error));
                    }
                }
            }
        }

        private static void CreateInstance()
        {
            instance = new NetworkingPerfCounters();
            if (instance.Enabled)
            {
                // as recommended by the perf. counter team: initialize perf. counters in background thread
                // since initialization may take a long time. Therefore we should not block.
                if (!ThreadPool.QueueUserWorkItem(instance.Initialize))
                {
                    if (Logging.On) Logging.PrintError(Logging.Web, SR.GetString(SR.net_perfcounter_cant_queue_workitem));
                }
            }
        }

        private static CounterPair CreateCounterPair(string counterName, string instanceName)
        {
            // first create global counter. If it throws we don't create an instance counter. If creating
            // the instance counter throws, we don't need to cleanup the global counter (done by perf. counter
            // infrastructure)

            // use this ctor for global counters: it makes sure the counter is actually initialized. If
            // we would use the ctor without params, as for the instance counters, the global counter
            // would not be initialized => only when we increment it the first time it gets initialized. But
            // at that point, we're no more under the permission assertion.
            PerformanceCounter globalCounter = new PerformanceCounter(categoryName, counterName,
                globalInstanceName, false);

            // for instance counters we use the default ctor, since we also need to set the lifetime. The
            // counter gets actually initialized when we set RawValue.
            PerformanceCounter instanceCounter = new PerformanceCounter();
            instanceCounter.CategoryName = categoryName;
            instanceCounter.CounterName = counterName;
            instanceCounter.InstanceName = instanceName;
            instanceCounter.InstanceLifetime = PerformanceCounterInstanceLifetime.Process;
            instanceCounter.ReadOnly = false;
            instanceCounter.RawValue = 0;

            return new CounterPair(instanceCounter, globalCounter);
        }

        private void ExceptionEventHandler(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating)
            {
                Cleanup();
            }
        }

        // Keep the UnloadEventHandler and ExitEventHandler methods separate in order to
        // facilitate better debugging and crash troubleshooting during call stack analysis
        // of ProcessExit and DomainUnload events from the AppDomain.

        private void UnloadEventHandler(object sender, EventArgs e)
        {
            Cleanup();
        }

        private void ExitEventHandler(object sender, EventArgs e)
        {
            Cleanup();
        }

        // Need to check for Environment.HasShutdownStarted.  This flag is ONLY
        // set when the CLR is running the GC Finalize thread and thus it is invalid
        // to access the static performanceCounter objects and their methods.
        private void Cleanup()
        {
            // In cases where Cleanup() gets called simultaneously on different threads,
            // we use the lock to make sure it gets executed only once.
            lock (lockObject)
            {
                if (!cleanupCalled)
                {
                    cleanupCalled = true;

                    // counters may be null if Initialize() throws before counters gets created
                    if (counters != null)
                    {
                        foreach (CounterPair cp in counters)
                        {
                            // cp != null check: if the loop creating the counters in Initialize() throws
                            // an excpetion, some CounterPairs may not be created yet, thus the null check below.
                            if (!Environment.HasShutdownStarted && cp != null)
                            {
                                try
                                {
                                    cp.InstanceCounter.RemoveInstance();
                                }
                                // in case there is something wrong with the counter instance, just log.
                                catch (InvalidOperationException e)
                                {
                                    if (Logging.On) Logging.Exception(Logging.Web, "NetworkingPerfCounters",
                                        "Cleanup", e);
                                }
                                catch (Win32Exception e)
                                {
                                    if (Logging.On) Logging.Exception(Logging.Web, "NetworkingPerfCounters",
                                        "Cleanup", e);
                                }
                            }
                        }
                    }

                    // No need to clean up global counters.
                }
            }
        }

        private static string GetInstanceName()
        {
            string friendlyName = ReplaceInvalidChars(AppDomain.CurrentDomain.FriendlyName);
            string postfix = VersioningHelper.MakeVersionSafeName(string.Empty, ResourceScope.Machine,
                ResourceScope.AppDomain);

            string result = friendlyName + postfix;

            if (result.Length > instanceNameMaxLength)
            {
                result = friendlyName.Substring(0, instanceNameMaxLength - postfix.Length) + postfix;
            }

            return result;
        }

        private static string ReplaceInvalidChars(string instanceName)
        {
            // map invalid characters as suggested by MSDN (see PerformanceCounter.InstanceName Property help)

            StringBuilder result = new StringBuilder(instanceName);
            for (int i = 0; i < result.Length; i++)
            {
                switch (result[i])
                {
                    case '(':
                        result[i] = '[';
                        break;
                    case ')':
                        result[i] = ']';
                        break;
                    case '/':
                    case '\\':
                    case '#':
                        result[i] = '_';
                        break;
                }
            }

            return result.ToString();
        }

        private bool CounterAvailable()
        {
            // Checking cleanupCalled below is not really necessary, since incrementing an already released
            // PerformanceCounter object is allowed. But since there is no point in incrementing a released
            // counter, we return false.
            // The only scenario where we increment a released counter is: Cleanup() is called after the
            // cleanupCalled check below, but before the counter is incremented.
            if (!enabled || cleanupCalled)
            {
                return false;
            }

            if (initDone)
            {
                return initSuccessful;
            }

            return false;
        }
    }
}

