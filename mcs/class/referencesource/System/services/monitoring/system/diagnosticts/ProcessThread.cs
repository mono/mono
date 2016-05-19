//------------------------------------------------------------------------------
// <copyright file="ProcessThread.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {

    using System.Threading;
    using System.Runtime.InteropServices;
    using System.ComponentModel;
    using System.Diagnostics;
    using System;
    using System.Collections;
    using System.IO;
    using Microsoft.Win32;       
    using System.Security.Permissions;
    using Microsoft.Win32.SafeHandles;
    using System.Runtime.Versioning;
//    using System.Windows.Forms;

    /// <devdoc>
    ///    <para>
    ///       Represents a Win32 thread. This can be used to obtain
    ///       information about the thread, such as it's performance characteristics. This is
    ///       returned from the System.Diagnostics.Process.ProcessThread property of the System.Diagnostics.Process component.
    ///    </para>
    ///    <note type="rnotes">
    ///       I don't understand
    ///       the following comment associated with the previous sentence: "property of
    ///       Process component " Rather than just "processTHread". There is no such
    ///       member on Process. Do we mean 'threads'?
    ///    </note>
    /// </devdoc>
    [
    Designer("System.Diagnostics.Design.ProcessThreadDesigner, " + AssemblyRef.SystemDesign),
    HostProtection(SelfAffectingProcessMgmt=true, SelfAffectingThreading=true)
    ]
    public class ProcessThread : Component {

        //
        // FIELDS
        //

        ThreadInfo threadInfo;
        bool isRemoteMachine;
        bool priorityBoostEnabled;
        bool havePriorityBoostEnabled;
        ThreadPriorityLevel priorityLevel;
        bool havePriorityLevel;

        //
        // CONSTRUCTORS
        //

        /// <devdoc>
        ///     Internal constructor.
        /// </devdoc>
        /// <internalonly/>
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        internal ProcessThread(bool isRemoteMachine, ThreadInfo threadInfo) {
            this.isRemoteMachine = isRemoteMachine;
            this.threadInfo = threadInfo;
            GC.SuppressFinalize(this);
        }

        //
        // PROPERTIES
        //

        /// <devdoc>
        ///     Returns the base priority of the thread which is computed by combining the
        ///     process priority class with the priority level of the associated thread.
        /// </devdoc>
        [MonitoringDescription(SR.ThreadBasePriority)]
        public int BasePriority {
            get {
                return threadInfo.basePriority;
            }
        }

        /// <devdoc>
        ///     The current priority indicates the actual priority of the associated thread,
        ///     which may deviate from the base priority based on how the OS is currently
        ///     scheduling the thread.
        /// </devdoc>
        [MonitoringDescription(SR.ThreadCurrentPriority)]
        public int CurrentPriority {
            get {
                return threadInfo.currentPriority;
            }
        }

        /// <devdoc>
        ///     Returns the unique identifier for the associated thread.
        /// </devdoc>
        [MonitoringDescription(SR.ThreadId)]
        public int Id {
            [ResourceExposure(ResourceScope.Process)]
            get {
                return threadInfo.threadId;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Sets the processor that this thread would ideally like to run on.
        ///    </para>
        /// </devdoc>
        [Browsable(false)]
        public int IdealProcessor {
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            set {
                SafeThreadHandle threadHandle = null;
                try {
                    threadHandle = OpenThreadHandle(NativeMethods.THREAD_SET_INFORMATION);
                    if (NativeMethods.SetThreadIdealProcessor(threadHandle, value) < 0) {
                        throw new Win32Exception();
                    }
                }
                finally {
                    CloseThreadHandle(threadHandle);
                }
            }
        }

        /// <devdoc>
        ///      Returns or sets whether this thread would like a priority boost if the user interacts
        ///      with user interface associated with this thread.
        /// </devdoc>
        [MonitoringDescription(SR.ThreadPriorityBoostEnabled)]
        public bool PriorityBoostEnabled {
            get {
                if (!havePriorityBoostEnabled) {
                    SafeThreadHandle threadHandle = null;
                    try {
                        threadHandle = OpenThreadHandle(NativeMethods.THREAD_QUERY_INFORMATION);
                        bool disabled = false;
                        if (!NativeMethods.GetThreadPriorityBoost(threadHandle, out disabled)) {
                            throw new Win32Exception();
                        }
                        priorityBoostEnabled = !disabled;
                        havePriorityBoostEnabled = true;
                    }
                    finally {
                        CloseThreadHandle(threadHandle);
                    }
                }
                return priorityBoostEnabled;
            }
            set {
                SafeThreadHandle threadHandle = null;
                try {
                    threadHandle = OpenThreadHandle(NativeMethods.THREAD_SET_INFORMATION);
                    if (!NativeMethods.SetThreadPriorityBoost(threadHandle, !value))
                        throw new Win32Exception();
                    priorityBoostEnabled = value;
                    havePriorityBoostEnabled = true;
                }
                finally {
                    CloseThreadHandle(threadHandle);
                }
            }
        }

        /// <devdoc>
        ///     Returns or sets the priority level of the associated thread.  The priority level is
        ///     not an absolute level, but instead contributes to the actual thread priority by
        ///     considering the priority class of the process.
        /// </devdoc>
        [MonitoringDescription(SR.ThreadPriorityLevel)]
        public ThreadPriorityLevel PriorityLevel {
            get {
                if (!havePriorityLevel) {
                    SafeThreadHandle threadHandle = null;
                    try {
                        threadHandle = OpenThreadHandle(NativeMethods.THREAD_QUERY_INFORMATION);
                        int value = NativeMethods.GetThreadPriority(threadHandle);
                        if (value == 0x7fffffff) {
                            throw new Win32Exception();
                        }
                        priorityLevel = (ThreadPriorityLevel)value;
                        havePriorityLevel = true;
                    }
                    finally {
                        CloseThreadHandle(threadHandle);
                    }
                }
                return priorityLevel;
            }
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            set {            
                SafeThreadHandle threadHandle = null;
                try {
                    threadHandle = OpenThreadHandle(NativeMethods.THREAD_SET_INFORMATION);
                    if (!NativeMethods.SetThreadPriority(threadHandle, (int)value)) {
                        throw new Win32Exception();
                    }
                    priorityLevel = value;
                }
                finally {
                    CloseThreadHandle(threadHandle);
                }
            }
        }

        /// <devdoc>
        ///     Returns the amount of time the thread has spent running code inside the operating
        ///     system core.
        /// </devdoc>
        [MonitoringDescription(SR.ThreadPrivilegedProcessorTime)]
        public TimeSpan PrivilegedProcessorTime {
            get {
                EnsureState(State.IsNt);
                return GetThreadTimes().PrivilegedProcessorTime;
            }
        }

        /// <devdoc>
        ///     Returns the memory address of the function that was called when the associated
        ///     thread was started.
        /// </devdoc>
        [MonitoringDescription(SR.ThreadStartAddress)]
        public IntPtr StartAddress {
            get {
                EnsureState(State.IsNt);
                return threadInfo.startAddress;
            }
        }

        /// <devdoc>
        ///     Returns the time the associated thread was started.
        /// </devdoc>
        [MonitoringDescription(SR.ThreadStartTime)]
        public DateTime StartTime {
            get {
                EnsureState(State.IsNt);
                return GetThreadTimes().StartTime;
            }
        }

        /// <devdoc>
        ///     Returns the current state of the associated thread, e.g. is it running, waiting, etc.
        /// </devdoc>
        [MonitoringDescription(SR.ThreadThreadState)]
        public ThreadState ThreadState {
            get {
                EnsureState(State.IsNt);
                return threadInfo.threadState;
            }
        }

        /// <devdoc>
        ///     Returns the amount of time the associated thread has spent utilizing the CPU.
        ///     It is the sum of the System.Diagnostics.ProcessThread.UserProcessorTime and
        ///     System.Diagnostics.ProcessThread.PrivilegedProcessorTime.
        /// </devdoc>
        [MonitoringDescription(SR.ThreadTotalProcessorTime)]
        public TimeSpan TotalProcessorTime {
            get {
                EnsureState(State.IsNt);
                return GetThreadTimes().TotalProcessorTime;
            }
        }

        /// <devdoc>
        ///     Returns the amount of time the associated thread has spent running code
        ///     inside the application (not the operating system core).
        /// </devdoc>
        [MonitoringDescription(SR.ThreadUserProcessorTime)]
        public TimeSpan UserProcessorTime {
            get {
                EnsureState(State.IsNt);
                return GetThreadTimes().UserProcessorTime;
            }
        }

        /// <devdoc>
        ///     Returns the reason the associated thread is waiting, if any.
        /// </devdoc>
        [MonitoringDescription(SR.ThreadWaitReason)]
        public ThreadWaitReason WaitReason {
            get {
                EnsureState(State.IsNt);
                if (threadInfo.threadState != ThreadState.Wait) {
                    throw new InvalidOperationException(SR.GetString(SR.WaitReasonUnavailable));
                }
                return threadInfo.threadWaitReason;
            }
        }

        //
        // METHODS
        //

        /// <devdoc>
        ///     Helper to close a thread handle.
        /// </devdoc>
        /// <internalonly/>
        private static void CloseThreadHandle(SafeThreadHandle handle) {
            if (handle != null) {
                handle.Close();
            }
        }

        /// <devdoc>
        ///     Helper to check preconditions for property access.
        /// </devdoc>
        void EnsureState(State state) {
            if ( ((state & State.IsLocal) != (State)0) && isRemoteMachine) {
                throw new NotSupportedException(SR.GetString(SR.NotSupportedRemoteThread));
            }

            if ((state & State.IsNt) != (State)0) {
                if (Environment.OSVersion.Platform != PlatformID.Win32NT) {
                    throw new PlatformNotSupportedException(SR.GetString(SR.WinNTRequired));
                }
            }
        }

        /// <devdoc>
        ///     Helper to open a thread handle.
        /// </devdoc>
        /// <internalonly/>
        [ResourceExposure(ResourceScope.None)]  // Scoped by threadId
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
        SafeThreadHandle OpenThreadHandle(int access) {
            EnsureState(State.IsLocal);
            return ProcessManager.OpenThread(threadInfo.threadId, access);
        }

        /// <devdoc>
        ///     Resets the ideal processor so there is no ideal processor for this thread (e.g.
        ///     any processor is ideal).
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public void ResetIdealProcessor() {
            // 32 means "any processor is fine"
            IdealProcessor = 32;
        }

        /// <devdoc>
        ///     Sets which processors the associated thread is allowed to be scheduled to run on.
        ///     Each processor is represented as a bit: bit 0 is processor one, bit 1 is processor
        ///     two, etc.  For example, the value 1 means run on processor one, 2 means run on
        ///     processor two, 3 means run on processor one or two.
        /// </devdoc>
        [Browsable(false)]
        public IntPtr ProcessorAffinity {
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            set {
                SafeThreadHandle threadHandle = null;
                try {
                    threadHandle = OpenThreadHandle(NativeMethods.THREAD_SET_INFORMATION | NativeMethods.THREAD_QUERY_INFORMATION);
                    if (NativeMethods.SetThreadAffinityMask(threadHandle, new HandleRef(this, value)) == IntPtr.Zero) {
                        throw new Win32Exception();
                    }
                }
                finally {
                    CloseThreadHandle(threadHandle);
                }
            }
        }

        private ProcessThreadTimes GetThreadTimes() {
            ProcessThreadTimes threadTimes = new ProcessThreadTimes();

            SafeThreadHandle threadHandle = null;
            try {
                threadHandle = OpenThreadHandle(NativeMethods.THREAD_QUERY_INFORMATION);

                if (!NativeMethods.GetThreadTimes(threadHandle, 
                    out threadTimes.create, 
                    out threadTimes.exit, 
                    out threadTimes.kernel, 
                    out threadTimes.user)) {
                    throw new Win32Exception();
                }
            }
            finally {
                CloseThreadHandle(threadHandle);
            }

            return threadTimes;
        }


        /// <summary>
        ///      Preconditions for accessing properties.
        /// </summary>
        /// <internalonly/>
        enum State {
            IsLocal = 0x2,
            IsNt = 0x4
        }
    }
}
