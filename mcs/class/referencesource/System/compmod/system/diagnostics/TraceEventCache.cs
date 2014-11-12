//------------------------------------------------------------------------------
// <copyright file="TraceEventCache.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Security.Permissions;
using System.Text;
using System.Collections;
using System.Globalization;
using System.Runtime.Versioning;

namespace System.Diagnostics {
    public class TraceEventCache {

        private static volatile int processId;
        private static volatile string processName;

        private long timeStamp = -1;
        private DateTime dateTime = DateTime.MinValue;
        private string stackTrace = null;

        internal Guid ActivityId {
            get { return Trace.CorrelationManager.ActivityId; }
        }
        
        public string Callstack {
            get {
                if (stackTrace == null)
                    stackTrace = Environment.StackTrace;
                else
                    new EnvironmentPermission(PermissionState.Unrestricted).Demand();

                return stackTrace;
            }
        }

        public Stack LogicalOperationStack {
            get {
                return Trace.CorrelationManager.LogicalOperationStack;
            }
        }

        public DateTime DateTime {
            get {
                if (dateTime == DateTime.MinValue)
                    dateTime = DateTime.UtcNow;
                return dateTime;
            }
        }

        public int ProcessId {
            [ResourceExposure(ResourceScope.Process)]  // Returns the current process's pid
            [ResourceConsumption(ResourceScope.Process)]
            get {
                return GetProcessId();
            }
        }

        public string ThreadId {
            get {
                return GetThreadId().ToString(CultureInfo.InvariantCulture);
            }
        }

        public long Timestamp {
            get {
                if (timeStamp == -1)
                    timeStamp = Stopwatch.GetTimestamp();
                return timeStamp ;
            }
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private static void InitProcessInfo() {
            // Demand unmanaged code permission
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();

            if (processName == null) {
                Process p = Process.GetCurrentProcess();
                try {
                    processId = p.Id;
                    processName = p.ProcessName;
                }
                finally {
                    p.Dispose();
                }
            }
        }

        [ResourceExposure(ResourceScope.Process)]
        internal static int GetProcessId() {
            InitProcessInfo();
            return processId;
        }
        
        internal static string GetProcessName() {
            InitProcessInfo();
            return processName;
        }
        
        internal static int GetThreadId() {
            return Thread.CurrentThread.ManagedThreadId;
        }
    }
}

