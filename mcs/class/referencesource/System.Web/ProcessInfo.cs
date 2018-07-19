//------------------------------------------------------------------------------
// <copyright file="ProcessInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * ProcessInfo class
 */
namespace System.Web {
    using System.Threading;
    using System.Security.Permissions;


    /// <devdoc>
    ///    <para>Provides enumerated values representing status of a process.</para>
    /// </devdoc>
    public enum ProcessStatus {

        /// <devdoc>
        ///    <para>Specifies that the process is running.</para>
        /// </devdoc>
        Alive         = 1,

        /// <devdoc>
        ///    <para>Specifies that the process has begun shutting down.</para>
        /// </devdoc>
        ShuttingDown  = 2,

        /// <devdoc>
        ///    <para>Specifies the the process has been shut down.</para>
        /// </devdoc>
        ShutDown      = 3,

        /// <devdoc>
        ///    <para>Specifies that the process has been terminated.</para>
        /// </devdoc>
        Terminated    = 4
    }
    

    /// <devdoc>
    ///    <para>Provides enumerated values representing the reason a process has shut 
    ///       down.</para>
    /// </devdoc>
    public enum ProcessShutdownReason {

        /// <devdoc>
        ///    <para>Specifies that the process has not been shut down.</para>
        /// </devdoc>
        None                = 0,           // alive

        /// <devdoc>
        ///    <para>Specifies that the process has been shut down unexpectedly.</para>
        /// </devdoc>
        Unexpected          = 1,

        /// <devdoc>
        ///    <para>Specifies that the process request exceeded the limit on number of 
        ///       processes.</para>
        /// </devdoc>
        RequestsLimit       = 2,

        /// <devdoc>
        ///    <para>Specifies that the process request exceeded the limit on number of 
        ///       processes in que.</para>
        /// </devdoc>
        RequestQueueLimit   = 3,

        /// <devdoc>
        ///    <para>Specifies that the process timed out.</para>
        /// </devdoc>
        Timeout             = 4,

        /// <devdoc>
        ///    <para>Specifies that the process exceeded the limit on process idle time.</para>
        /// </devdoc>
        IdleTimeout         = 5,

        /// <devdoc>
        ///    <para>Specifies that the process exceeded the limit of memory available per process.</para>
        /// </devdoc>
        MemoryLimitExceeded = 6,
        

        PingFailed = 7,


        DeadlockSuspected = 8
    }    


    /// <devdoc>
    ///    <para>Provides information on processes.</para>
    /// </devdoc>
    public class ProcessInfo {

        /// <devdoc>
        ///    <para>Indicates the time a process was started.</para>
        /// </devdoc>
        public DateTime               StartTime { get { return _StartTime;}}

        /// <devdoc>
        ///    <para>Indicates the length of time the process has been running.</para>
        /// </devdoc>
        public TimeSpan               Age { get { return _Age;}}

        /// <devdoc>
        ///    <para>Indicates the process id of the process.</para>
        /// </devdoc>
        public int                    ProcessID { get { return _ProcessID;}}

        public int                    RequestCount { get { return _RequestCount;}}

        /// <devdoc>
        ///    <para>Indicates the current status of the process.</para>
        /// </devdoc>
        public ProcessStatus          Status { get { return _Status;}}

        /// <devdoc>
        ///    <para>Indicates the reason the process shut down.</para>
        /// </devdoc>
        public ProcessShutdownReason  ShutdownReason { get { return _ShutdownReason;}}

        /// <devdoc>
        ///    <para>Indicates the maximum amount of memory the process has used.</para>
        /// </devdoc>
        public int                    PeakMemoryUsed { get { return _PeakMemoryUsed;}}

        private DateTime               _StartTime;
        private TimeSpan               _Age;
        private int                    _ProcessID;
        private int                    _RequestCount;
        private ProcessStatus          _Status;
        private ProcessShutdownReason  _ShutdownReason;
        private int                    _PeakMemoryUsed;


        /// <devdoc>
        ///    <para>Sets internal information indicating the status of the process.</para>
        /// </devdoc>
        public void SetAll (DateTime startTime, TimeSpan age, int processID, int requestCount, ProcessStatus status, 
                            ProcessShutdownReason  shutdownReason, int peakMemoryUsed) {
            _StartTime = startTime;
            _Age = age;
            _ProcessID = processID;
            _RequestCount = requestCount;
            _Status = status;
            _ShutdownReason = shutdownReason;
            _PeakMemoryUsed = peakMemoryUsed;
        }


        /// <devdoc>
        /// <para>Initializes a new instance of the <see langword='ProcessInfo'/> class and sets internal information 
        ///    indicating the status of the process.</para>
        /// </devdoc>
        public ProcessInfo (DateTime startTime, TimeSpan age, int processID, int requestCount, ProcessStatus status, 
                            ProcessShutdownReason  shutdownReason, int peakMemoryUsed) {
            _StartTime = startTime;
            _Age = age;
            _ProcessID = processID;
            _RequestCount = requestCount;
            _Status = status;
            _ShutdownReason = shutdownReason;
            _PeakMemoryUsed = peakMemoryUsed;
        }


        public ProcessInfo() {
        }

    }

}
