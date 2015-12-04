//------------------------------------------------------------------------------
// <copyright file="Process.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {
    using System.Text;
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Diagnostics;
    using System;
    using System.Collections;
    using System.IO;
    using Microsoft.Win32;        
    using Microsoft.Win32.SafeHandles;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Runtime.Versioning;
    
    /// <devdoc>
    ///    <para>
    ///       Provides access to local and remote
    ///       processes. Enables you to start and stop system processes.
    ///    </para>
    /// </devdoc>
    [
    MonitoringDescription(SR.ProcessDesc),
    DefaultEvent("Exited"), 
    DefaultProperty("StartInfo"),
    Designer("System.Diagnostics.Design.ProcessDesigner, " + AssemblyRef.SystemDesign),
    // Disabling partial trust scenarios
    PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"),
    PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"),
    HostProtection(SharedState=true, Synchronization=true, ExternalProcessMgmt=true, SelfAffectingProcessMgmt=true)
    ]
    public class Process : Component {
        //
        // FIELDS
        //

        bool haveProcessId;
        int processId;
        bool haveProcessHandle;
        SafeProcessHandle m_processHandle;
        bool isRemoteMachine;
        string machineName;
        ProcessInfo processInfo;
        Int32 m_processAccess;

#if !FEATURE_PAL        
        ProcessThreadCollection threads;
        ProcessModuleCollection modules;
#endif // !FEATURE_PAL        

        bool haveMainWindow;
        IntPtr mainWindowHandle;  // no need to use SafeHandle for window        
        string mainWindowTitle;
        
        bool haveWorkingSetLimits;
        IntPtr minWorkingSet;
        IntPtr maxWorkingSet;
        
        bool haveProcessorAffinity;
        IntPtr processorAffinity;

        bool havePriorityClass;
        ProcessPriorityClass priorityClass;

        ProcessStartInfo startInfo;
        
        bool watchForExit;
        bool watchingForExit;
        EventHandler onExited;
        bool exited;
        int exitCode;
        bool signaled;
		
        DateTime exitTime;
        bool haveExitTime;
        
        bool responding;
        bool haveResponding;
        
        bool priorityBoostEnabled;
        bool havePriorityBoostEnabled;
        
        bool raisedOnExited;
        RegisteredWaitHandle registeredWaitHandle;
        WaitHandle waitHandle;
        ISynchronizeInvoke synchronizingObject;
        StreamReader standardOutput;
        StreamWriter standardInput;
        StreamReader standardError;
        OperatingSystem operatingSystem;
        bool disposed;
        
        static object s_CreateProcessLock = new object();
        
        // This enum defines the operation mode for redirected process stream.
        // We don't support switching between synchronous mode and asynchronous mode.
        private enum StreamReadMode 
        {
            undefined, 
            syncMode, 
            asyncMode
        }
        
        StreamReadMode outputStreamReadMode;
        StreamReadMode errorStreamReadMode;
        
       
        // Support for asynchrously reading streams
        [Browsable(true), MonitoringDescription(SR.ProcessAssociated)]
        //[System.Runtime.InteropServices.ComVisible(false)]        
        public event DataReceivedEventHandler OutputDataReceived;
        [Browsable(true), MonitoringDescription(SR.ProcessAssociated)]
        //[System.Runtime.InteropServices.ComVisible(false)]        
        public event DataReceivedEventHandler ErrorDataReceived;
        // Abstract the stream details
        internal AsyncStreamReader output;
        internal AsyncStreamReader error;
        internal bool pendingOutputRead;
        internal bool pendingErrorRead;


        private static SafeFileHandle InvalidPipeHandle = new SafeFileHandle(IntPtr.Zero, false);
#if DEBUG
        internal static TraceSwitch processTracing = new TraceSwitch("processTracing", "Controls debug output from Process component");
#else
        internal static TraceSwitch processTracing = null;
#endif

        //
        // CONSTRUCTORS
        //

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Diagnostics.Process'/> class.
        ///    </para>
        /// </devdoc>
        public Process() {
            this.machineName = ".";
            this.outputStreamReadMode = StreamReadMode.undefined;
            this.errorStreamReadMode = StreamReadMode.undefined;            
            this.m_processAccess = NativeMethods.PROCESS_ALL_ACCESS;
        }        
        
        [ResourceExposure(ResourceScope.Machine)]
        Process(string machineName, bool isRemoteMachine, int processId, ProcessInfo processInfo) : base() {
            Debug.Assert(SyntaxCheck.CheckMachineName(machineName), "The machine name should be valid!");
            this.processInfo = processInfo;
            this.machineName = machineName;
            this.isRemoteMachine = isRemoteMachine;
            this.processId = processId;
            this.haveProcessId = true;
            this.outputStreamReadMode = StreamReadMode.undefined;
            this.errorStreamReadMode = StreamReadMode.undefined;
            this.m_processAccess = NativeMethods.PROCESS_ALL_ACCESS;
        }

        //
        // PROPERTIES
        //

        /// <devdoc>
        ///     Returns whether this process component is associated with a real process.
        /// </devdoc>
        /// <internalonly/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessAssociated)]
        bool Associated {
            get {
                return haveProcessId || haveProcessHandle;
            }
        }

 #if !FEATURE_PAL
        /// <devdoc>
        ///    <para>
        ///       Gets the base priority of
        ///       the associated process.
        ///    </para>
        /// </devdoc>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessBasePriority)]
        public int BasePriority {
            get {
                EnsureState(State.HaveProcessInfo);
                return processInfo.basePriority;
            }
        }
#endif // FEATURE_PAL

        /// <devdoc>
        ///    <para>
        ///       Gets
        ///       the
        ///       value that was specified by the associated process when it was terminated.
        ///    </para>
        /// </devdoc>
        [Browsable(false),DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessExitCode)]
        public int ExitCode {
            get {
                EnsureState(State.Exited);
                return exitCode;
            }
          }

        /// <devdoc>
        ///    <para>
        ///       Gets a
        ///       value indicating whether the associated process has been terminated.
        ///    </para>
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessTerminated)]
        public bool HasExited {
            get {
                if (!exited) {
                    EnsureState(State.Associated);
                    SafeProcessHandle handle = null;
                    try {
                        handle = GetProcessHandle(NativeMethods.PROCESS_QUERY_INFORMATION | NativeMethods.SYNCHRONIZE, false);
                        if (handle.IsInvalid) {
                            exited = true;
                        }
                        else {
                            int exitCode;
                            
                            // Although this is the wrong way to check whether the process has exited,
                            // it was historically the way we checked for it, and a lot of code then took a dependency on
                            // the fact that this would always be set before the pipes were closed, so they would read
                            // the exit code out after calling ReadToEnd() or standard output or standard error. In order
                            // to allow 259 to function as a valid exit code and to break as few people as possible that
                            // took the ReadToEnd dependency, we check for an exit code before doing the more correct
                            // check to see if we have been signalled.
                            if (NativeMethods.GetExitCodeProcess(handle, out exitCode) && exitCode != NativeMethods.STILL_ACTIVE) {
                                this.exited = true;
                                this.exitCode = exitCode;                                
                            }
                            else {                                                        

                                // The best check for exit is that the kernel process object handle is invalid, 
                                // or that it is valid and signaled.  Checking if the exit code != STILL_ACTIVE 
                                // does not guarantee the process is closed,
                                // since some process could return an actual STILL_ACTIVE exit code (259).
                                if (!signaled) // if we just came from WaitForExit, don't repeat
                                {
                                    ProcessWaitHandle wh = null;
                                    try 
                                    {
                                        wh = new ProcessWaitHandle(handle);
                                        this.signaled = wh.WaitOne(0, false);					
                                    }
                                    finally
                                    {

                                        if (wh != null)
                                        wh.Close();
                                    }
                                }
                                if (signaled) 
                                {
                                    if (!NativeMethods.GetExitCodeProcess(handle, out exitCode))                               
                                        throw new Win32Exception();
                                
                                    this.exited = true;
                                    this.exitCode = exitCode;
                                }
                            }
                        }	
                    }
                    finally 
                    {
                        ReleaseProcessHandle(handle);
                    }

                    if (exited) {
                        RaiseOnExited();
                    }
                }
                return exited;
            }
        }

        private ProcessThreadTimes GetProcessTimes() {
            ProcessThreadTimes processTimes = new ProcessThreadTimes();
            SafeProcessHandle handle = null;
            try {
                int access = NativeMethods.PROCESS_QUERY_INFORMATION;

                if (EnvironmentHelpers.IsWindowsVistaOrAbove()) 
                    access = NativeMethods.PROCESS_QUERY_LIMITED_INFORMATION;

                handle = GetProcessHandle(access, false);
                if( handle.IsInvalid) {
                    // On OS older than XP, we will not be able to get the handle for a process
                    // after it terminates. 
                    // On Windows XP and newer OS, the information about a process will stay longer. 
                    throw new InvalidOperationException(SR.GetString(SR.ProcessHasExited, processId.ToString(CultureInfo.CurrentCulture)));
                }

                if (!NativeMethods.GetProcessTimes(handle, 
                                                   out processTimes.create, 
                                                   out processTimes.exit, 
                                                   out processTimes.kernel, 
                                                   out processTimes.user)) {
                    throw new Win32Exception();
                }

            }
            finally {
                ReleaseProcessHandle(handle);                
            }
            return processTimes;
        }

 #if !FEATURE_PAL
        /// <devdoc>
        ///    <para>
        ///       Gets the time that the associated process exited.
        ///    </para>
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessExitTime)]
        public DateTime ExitTime {
            get {
                if (!haveExitTime) {
                    EnsureState(State.IsNt | State.Exited);
                    exitTime = GetProcessTimes().ExitTime;
                    haveExitTime = true;
                }
                return exitTime;
            }
        }
#endif // !FEATURE_PAL

        /// <devdoc>
        ///    <para>
        ///       Returns the native handle for the associated process. The handle is only available
        ///       if this component started the process.
        ///    </para>
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessHandle)]
        public IntPtr Handle {
            [ResourceExposure(ResourceScope.Machine)]
            [ResourceConsumption(ResourceScope.Machine)]
            get {
                EnsureState(State.Associated);
                return OpenProcessHandle(this.m_processAccess).DangerousGetHandle();
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]  
        public SafeProcessHandle SafeHandle {
            get {
                EnsureState(State.Associated);
                return OpenProcessHandle(this.m_processAccess);
            }
        }

#if !FEATURE_PAL
        /// <devdoc>
        ///    <para>
        ///       Gets the number of handles that are associated
        ///       with the process.
        ///    </para>
        /// </devdoc>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessHandleCount)]
        public int HandleCount {
            get {
                EnsureState(State.HaveProcessInfo);
                return processInfo.handleCount;
            }
        }
#endif // !FEATURE_PAL

        /// <devdoc>
        ///    <para>
        ///       Gets
        ///       the unique identifier for the associated process.
        ///    </para>
        /// </devdoc>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessId)]
        public int Id {
            get {
                EnsureState(State.HaveId);
                return processId;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets
        ///       the name of the computer on which the associated process is running.
        ///    </para>
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessMachineName)]
        public string MachineName {
            get {
                EnsureState(State.Associated);
                return machineName;
            }
        }

 #if !FEATURE_PAL

        /// <devdoc>
        ///    <para>
        ///       Returns the window handle of the main window of the associated process.
        ///    </para>
        /// </devdoc>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessMainWindowHandle)]
        public IntPtr MainWindowHandle {
            [ResourceExposure(ResourceScope.Machine)]
            [ResourceConsumption(ResourceScope.Machine)]
            get {
                if (!haveMainWindow) {
                    EnsureState(State.IsLocal | State.HaveId);
                    mainWindowHandle = ProcessManager.GetMainWindowHandle(processId);

                    if (mainWindowHandle != (IntPtr)0) {
                        haveMainWindow = true;
                    } else {
                        // We do the following only for the side-effect that it will throw when if the process no longer exists on the system.  In Whidbey
                        // we always did this check but have now changed it to just require a ProcessId. In the case where someone has called Refresh() 
                        // and the process has exited this call will throw an exception where as the above code would return 0 as the handle.
                        EnsureState(State.HaveProcessInfo);
                    }
                }
                return mainWindowHandle;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Returns the caption of the <see cref='System.Diagnostics.Process.MainWindowHandle'/> of
        ///       the process. If the handle is zero (0), then an empty string is returned.
        ///    </para>
        /// </devdoc>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessMainWindowTitle)]
        public string MainWindowTitle {
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
            get {
                if (mainWindowTitle == null) {
                    IntPtr handle = MainWindowHandle;
                    if (handle == (IntPtr)0) {
                        mainWindowTitle = String.Empty;
                    }
                    else {
                        int length = NativeMethods.GetWindowTextLength(new HandleRef(this, handle)) * 2;
                        StringBuilder builder = new StringBuilder(length);
                        NativeMethods.GetWindowText(new HandleRef(this, handle), builder, builder.Capacity);
                        mainWindowTitle = builder.ToString();
                    }
                }
                return mainWindowTitle;
            }
        }



        /// <devdoc>
        ///    <para>
        ///       Gets
        ///       the main module for the associated process.
        ///    </para>
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessMainModule)]
        public ProcessModule MainModule {
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
            get {
                // We only return null if we couldn't find a main module.
                // This could be because
                //      1. The process hasn't finished loading the main module (most likely)
                //      2. There are no modules loaded (possible for certain OS processes)
                //      3. Possibly other?
                
                if (OperatingSystem.Platform == PlatformID.Win32NT) {
                    EnsureState(State.HaveId | State.IsLocal);
                    // on NT the first module is the main module                    
                    ModuleInfo module = NtProcessManager.GetFirstModuleInfo(processId);
                    return new ProcessModule(module);
                }
                else {
                    ProcessModuleCollection moduleCollection = Modules;                        
                    // on 9x we have to do a little more work
                    EnsureState(State.HaveProcessInfo);
                    foreach (ProcessModule pm in moduleCollection) {
                        if (pm.moduleInfo.Id == processInfo.mainModuleId) {
                            return pm;
                        }
                    }
                    return null;
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the maximum allowable working set for the associated
        ///       process.
        ///    </para>
        /// </devdoc>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessMaxWorkingSet)]
        public IntPtr MaxWorkingSet {
            get {
                EnsureWorkingSetLimits();
                return maxWorkingSet;
            }
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            set {
                SetWorkingSetLimits(null, value);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the minimum allowable working set for the associated
        ///       process.
        ///    </para>
        /// </devdoc>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessMinWorkingSet)]
        public IntPtr MinWorkingSet {
            get {
                EnsureWorkingSetLimits();
                return minWorkingSet;
            }
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            set {
                SetWorkingSetLimits(value, null);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets
        ///       the modules that have been loaded by the associated process.
        ///    </para>
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessModules)]
        public ProcessModuleCollection Modules {
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
            get {
                if (modules == null) {
                    EnsureState(State.HaveId | State.IsLocal);
                    ModuleInfo[] moduleInfos = ProcessManager.GetModuleInfos(processId);
                    ProcessModule[] newModulesArray = new ProcessModule[moduleInfos.Length];
                    for (int i = 0; i < moduleInfos.Length; i++) {
                        newModulesArray[i] = new ProcessModule(moduleInfos[i]);
                    }
                    ProcessModuleCollection newModules = new ProcessModuleCollection(newModulesArray);
                    modules = newModules;
                }
                return modules;
            }
        }

        /// <devdoc>
        ///     Returns the amount of memory that the system has allocated on behalf of the
        ///     associated process that can not be written to the virtual memory paging file.
        /// </devdoc>
        [Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.NonpagedSystemMemorySize64 instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessNonpagedSystemMemorySize)]
        public int NonpagedSystemMemorySize {
            get {
                EnsureState(State.HaveNtProcessInfo);
                return unchecked((int)processInfo.poolNonpagedBytes);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessNonpagedSystemMemorySize)]
        [System.Runtime.InteropServices.ComVisible(false)]            
        public long NonpagedSystemMemorySize64 {
            get {
                EnsureState(State.HaveNtProcessInfo);
                return processInfo.poolNonpagedBytes;
            }
        }

        /// <devdoc>
        ///     Returns the amount of memory that the associated process has allocated
        ///     that can be written to the virtual memory paging file.
        /// </devdoc>
        [Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.PagedMemorySize64 instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessPagedMemorySize)]
        public int PagedMemorySize {
            get {
                EnsureState(State.HaveNtProcessInfo);
                return unchecked((int)processInfo.pageFileBytes);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessPagedMemorySize)]
        [System.Runtime.InteropServices.ComVisible(false)]
        public long PagedMemorySize64 {
            get {
                EnsureState(State.HaveNtProcessInfo);
                return processInfo.pageFileBytes;
            }
        }


        /// <devdoc>
        ///     Returns the amount of memory that the system has allocated on behalf of the
        ///     associated process that can be written to the virtual memory paging file.
        /// </devdoc>
        [Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.PagedSystemMemorySize64 instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessPagedSystemMemorySize)]
        public int PagedSystemMemorySize {
            get {
                EnsureState(State.HaveNtProcessInfo);
                return unchecked((int)processInfo.poolPagedBytes);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessPagedSystemMemorySize)]
        [System.Runtime.InteropServices.ComVisible(false)]
        public long PagedSystemMemorySize64 {
            get {
                EnsureState(State.HaveNtProcessInfo);
                return processInfo.poolPagedBytes;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Returns the maximum amount of memory that the associated process has
        ///       allocated that could be written to the virtual memory paging file.
        ///    </para>
        /// </devdoc>
        [Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.PeakPagedMemorySize64 instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessPeakPagedMemorySize)]
        public int PeakPagedMemorySize {
            get {
                EnsureState(State.HaveNtProcessInfo);
                return unchecked((int)processInfo.pageFileBytesPeak);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessPeakPagedMemorySize)]
        [System.Runtime.InteropServices.ComVisible(false)]
        public long PeakPagedMemorySize64 {
            get {
                EnsureState(State.HaveNtProcessInfo);
                return processInfo.pageFileBytesPeak;
            }
        }
        
        /// <devdoc>
        ///    <para>
        ///       Returns the maximum amount of physical memory that the associated
        ///       process required at once.
        ///    </para>
        /// </devdoc>
        [Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.PeakWorkingSet64 instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessPeakWorkingSet)]
        public int PeakWorkingSet {
            get {
                EnsureState(State.HaveNtProcessInfo);
                return unchecked((int)processInfo.workingSetPeak);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessPeakWorkingSet)]
        [System.Runtime.InteropServices.ComVisible(false)]
        public long PeakWorkingSet64 {
            get {
                EnsureState(State.HaveNtProcessInfo);
                return processInfo.workingSetPeak;
            }
        }

        /// <devdoc>
        ///     Returns the maximum amount of virtual memory that the associated
        ///     process has requested.
        /// </devdoc>
        [Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.PeakVirtualMemorySize64 instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessPeakVirtualMemorySize)]
        public int PeakVirtualMemorySize {
            get {
                EnsureState(State.HaveNtProcessInfo);
                return unchecked((int)processInfo.virtualBytesPeak);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessPeakVirtualMemorySize)]
        [System.Runtime.InteropServices.ComVisible(false)]
        public long PeakVirtualMemorySize64 {
            get {
                EnsureState(State.HaveNtProcessInfo);
                return processInfo.virtualBytesPeak;
            }
        }

        private OperatingSystem OperatingSystem {
            get {
                if (operatingSystem == null) {
                    operatingSystem = Environment.OSVersion;
                }                
                return operatingSystem;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value indicating whether the associated process priority
        ///       should be temporarily boosted by the operating system when the main window
        ///       has focus.
        ///    </para>
        /// </devdoc>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessPriorityBoostEnabled)]
        public bool PriorityBoostEnabled {
            get {
                EnsureState(State.IsNt);
                if (!havePriorityBoostEnabled) {
                    SafeProcessHandle handle = null;
                    try {
                        handle = GetProcessHandle(NativeMethods.PROCESS_QUERY_INFORMATION);
                        bool disabled = false;
                        if (!NativeMethods.GetProcessPriorityBoost(handle, out disabled)) {
                            throw new Win32Exception();
                        }
                        priorityBoostEnabled = !disabled;
                        havePriorityBoostEnabled = true;
                    }
                    finally {
                        ReleaseProcessHandle(handle);
                    }
                }
                return priorityBoostEnabled;
            }
            set {
                EnsureState(State.IsNt);
                SafeProcessHandle handle = null;
                try {
                    handle = GetProcessHandle(NativeMethods.PROCESS_SET_INFORMATION);
                    if (!NativeMethods.SetProcessPriorityBoost(handle, !value))
                        throw new Win32Exception();
                    priorityBoostEnabled = value;
                    havePriorityBoostEnabled = true;
                }
                finally {
                    ReleaseProcessHandle(handle);
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the overall priority category for the
        ///       associated process.
        ///    </para>
        /// </devdoc>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessPriorityClass)]
        public ProcessPriorityClass PriorityClass {
            get {
                if (!havePriorityClass) {
                    SafeProcessHandle handle = null;
                    try {
                        handle = GetProcessHandle(NativeMethods.PROCESS_QUERY_INFORMATION);
                        int value = NativeMethods.GetPriorityClass(handle);
                        if (value == 0) {
                            throw new Win32Exception();
                        }
                        priorityClass = (ProcessPriorityClass)value;
                        havePriorityClass = true;
                    }
                    finally {
                        ReleaseProcessHandle(handle);
                    }
                }
                return priorityClass;
            }
            [ResourceExposure(ResourceScope.Machine)]
            [ResourceConsumption(ResourceScope.Machine)]
            set {
                if (!Enum.IsDefined(typeof(ProcessPriorityClass), value)) { 
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(ProcessPriorityClass));
                }

                // BelowNormal and AboveNormal are only available on Win2k and greater.
                if (((value & (ProcessPriorityClass.BelowNormal | ProcessPriorityClass.AboveNormal)) != 0)   && 
                    (OperatingSystem.Platform != PlatformID.Win32NT || OperatingSystem.Version.Major < 5)) {
                    throw new PlatformNotSupportedException(SR.GetString(SR.PriorityClassNotSupported), null);
                }                
                                    
                SafeProcessHandle handle = null;

                try {
                    handle = GetProcessHandle(NativeMethods.PROCESS_SET_INFORMATION);
                    if (!NativeMethods.SetPriorityClass(handle, (int)value)) {
                        throw new Win32Exception();
                    }
                    priorityClass = value;
                    havePriorityClass = true;
                }
                finally {
                    ReleaseProcessHandle(handle);
                }
            }
        }

        /// <devdoc>
        ///     Returns the number of bytes that the associated process has allocated that cannot
        ///     be shared with other processes.
        /// </devdoc>
        [Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.PrivateMemorySize64 instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessPrivateMemorySize)]
        public int PrivateMemorySize {
            get {
                EnsureState(State.HaveNtProcessInfo);
                return unchecked((int)processInfo.privateBytes);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessPrivateMemorySize)]
        [System.Runtime.InteropServices.ComVisible(false)]            
        public long PrivateMemorySize64 {
            get {
                EnsureState(State.HaveNtProcessInfo);
                return processInfo.privateBytes;
            }
        }

        /// <devdoc>
        ///     Returns the amount of time the process has spent running code inside the operating
        ///     system core.
        /// </devdoc>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessPrivilegedProcessorTime)]
        public TimeSpan PrivilegedProcessorTime {
            get {
                EnsureState(State.IsNt);
                return GetProcessTimes().PrivilegedProcessorTime;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets
        ///       the friendly name of the process.
        ///    </para>
        /// </devdoc>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessProcessName)]
        public string ProcessName {
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
            get {
                EnsureState(State.HaveProcessInfo);
                String processName =  processInfo.processName;
                //
                // On some old NT-based OS like win2000, the process name from NTQuerySystemInformation is up to 15 characters.
                // Processes executing notepad_1234567.exe and notepad_12345678.exe will have the same process name.
                // GetProcessByNames will not be able find the process for notepad_12345678.exe.
                // So we will try to replace the name of the process by its main module name if the name is 15 characters.
                // However we can't always get the module name:
                //     (1) Normal user will not be able to get module information about processes. 
                //     (2) We can't get module information about remoting process.
                // We can't get module name for a remote process
                // 
                if (processName.Length == 15 && ProcessManager.IsNt && ProcessManager.IsOSOlderThanXP && !isRemoteMachine) { 
                    try {
                        String mainModuleName = MainModule.ModuleName;
                        if (mainModuleName != null) {
                            processInfo.processName = Path.ChangeExtension(Path.GetFileName(mainModuleName), null);
                        }
                    }
                    catch(Exception) {
                        // If we can't access the module information, we can still use the might-be-truncated name. 
                        // We could fail for a few reasons:
                        // (1) We don't enough privilege to get module information.
                        // (2) The process could have terminated.
                    }
                }
                
                return processInfo.processName;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets
        ///       or sets which processors the threads in this process can be scheduled to run on.
        ///    </para>
        /// </devdoc>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessProcessorAffinity)]
        public IntPtr ProcessorAffinity {
            get {
                if (!haveProcessorAffinity) {
                    SafeProcessHandle handle = null;
                    try {
                        handle = GetProcessHandle(NativeMethods.PROCESS_QUERY_INFORMATION);
                        IntPtr processAffinity;
                        IntPtr systemAffinity;
                        if (!NativeMethods.GetProcessAffinityMask(handle, out processAffinity, out systemAffinity))
                            throw new Win32Exception();
                        processorAffinity = processAffinity;
                    }
                    finally {
                        ReleaseProcessHandle(handle);
                    }
                    haveProcessorAffinity = true;
                }
                return processorAffinity;
            }
            [ResourceExposure(ResourceScope.Machine)]
            [ResourceConsumption(ResourceScope.Machine)]
            set {
                SafeProcessHandle handle = null;
                try {
                    handle = GetProcessHandle(NativeMethods.PROCESS_SET_INFORMATION);
                    if (!NativeMethods.SetProcessAffinityMask(handle, value)) 
                        throw new Win32Exception();
                        
                    processorAffinity = value;
                    haveProcessorAffinity = true;
                }
                finally {
                    ReleaseProcessHandle(handle);
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether or not the user
        ///       interface of the process is responding.
        ///    </para>
        /// </devdoc>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessResponding)]
        public bool Responding {
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
            get {
                if (!haveResponding) {
                    IntPtr mainWindow = MainWindowHandle;
                    if (mainWindow == (IntPtr)0) {
                        responding = true;
                    }
                    else {
                        IntPtr result;
                        responding = NativeMethods.SendMessageTimeout(new HandleRef(this, mainWindow), NativeMethods.WM_NULL, IntPtr.Zero, IntPtr.Zero, NativeMethods.SMTO_ABORTIFHUNG, 5000, out result) != (IntPtr)0;
                    }
                }
                return responding;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessSessionId)]
        public int SessionId {
            get {
                EnsureState(State.HaveNtProcessInfo);
                return processInfo.sessionId;                
            }
        }

#endif // !FEATURE_PAL

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the properties to pass into the <see cref='System.Diagnostics.Process.Start'/> method for the <see cref='System.Diagnostics.Process'/>
        ///       .
        ///    </para>
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), MonitoringDescription(SR.ProcessStartInfo)]
        public ProcessStartInfo StartInfo {
            get {
                if (startInfo == null) {
                    startInfo = new ProcessStartInfo(this);
                }                
                return startInfo;
            }
            [ResourceExposure(ResourceScope.Machine)]
            set {
                if (value == null) { 
                    throw new ArgumentNullException("value");
                }
                startInfo = value;
            }
        }

#if !FEATURE_PAL        
        /// <devdoc>
        ///     Returns the time the associated process was started.
        /// </devdoc>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessStartTime)]
        public DateTime StartTime {
            get {
                EnsureState(State.IsNt);
                return GetProcessTimes().StartTime;
            }
        }
#endif // !FEATURE_PAL

        /// <devdoc>
        ///   Represents the object used to marshal the event handler
        ///   calls issued as a result of a Process exit. Normally 
        ///   this property will  be set when the component is placed 
        ///   inside a control or  a from, since those components are 
        ///   bound to a specific thread.
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),         
        MonitoringDescription(SR.ProcessSynchronizingObject)
        ]
        public ISynchronizeInvoke SynchronizingObject {
            get {
               if (this.synchronizingObject == null && DesignMode) {
                    IDesignerHost host = (IDesignerHost)GetService(typeof(IDesignerHost));
                    if (host != null) {
                        object baseComponent = host.RootComponent;
                        if (baseComponent != null && baseComponent is ISynchronizeInvoke)
                            this.synchronizingObject = (ISynchronizeInvoke)baseComponent;
                    }                        
                }

                return this.synchronizingObject;
            }
            
            set {
                this.synchronizingObject = value;
            }
        }

#if !FEATURE_PAL        
        
        /// <devdoc>
        ///    <para>
        ///       Gets the set of threads that are running in the associated
        ///       process.
        ///    </para>
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessThreads)]
        public ProcessThreadCollection Threads {
            [ResourceExposure(ResourceScope.Process)]
            [ResourceConsumption(ResourceScope.Process)]
            get {
                if (threads == null) {
                    EnsureState(State.HaveProcessInfo);
                    int count = processInfo.threadInfoList.Count;
                    ProcessThread[] newThreadsArray = new ProcessThread[count];
                    for (int i = 0; i < count; i++) {
                        newThreadsArray[i] = new ProcessThread(isRemoteMachine, (ThreadInfo)processInfo.threadInfoList[i]);
                    }
                    ProcessThreadCollection newThreads = new ProcessThreadCollection(newThreadsArray);
                    threads = newThreads;
                }
                return threads;
            }
        }

        /// <devdoc>
        ///     Returns the amount of time the associated process has spent utilizing the CPU.
        ///     It is the sum of the <see cref='System.Diagnostics.Process.UserProcessorTime'/> and
        ///     <see cref='System.Diagnostics.Process.PrivilegedProcessorTime'/>.
        /// </devdoc>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessTotalProcessorTime)]
        public TimeSpan TotalProcessorTime {
            get {
                EnsureState(State.IsNt);
                return GetProcessTimes().TotalProcessorTime;
            }
        }

        /// <devdoc>
        ///     Returns the amount of time the associated process has spent running code
        ///     inside the application portion of the process (not the operating system core).
        /// </devdoc>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessUserProcessorTime)]
        public TimeSpan UserProcessorTime {
            get {
                EnsureState(State.IsNt);
                return GetProcessTimes().UserProcessorTime;
            }
        }

        /// <devdoc>
        ///     Returns the amount of virtual memory that the associated process has requested.
        /// </devdoc>
        [Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.VirtualMemorySize64 instead.  http://go.microsoft.com/fwlink/?linkid=14202")]        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessVirtualMemorySize)]
        public int VirtualMemorySize {
            get {
                EnsureState(State.HaveNtProcessInfo);
                return unchecked((int)processInfo.virtualBytes);
            }
        }
#endif // !FEATURE_PAL

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessVirtualMemorySize)]
        [System.Runtime.InteropServices.ComVisible(false)]        
        public long VirtualMemorySize64 {
            get {
                EnsureState(State.HaveNtProcessInfo);
                return processInfo.virtualBytes;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets whether the <see cref='System.Diagnostics.Process.Exited'/>
        ///       event is fired
        ///       when the process terminates.
        ///    </para>
        /// </devdoc>
        [Browsable(false), DefaultValue(false), MonitoringDescription(SR.ProcessEnableRaisingEvents)]
        public bool EnableRaisingEvents {
            get {
                return watchForExit;
            }
            set {
                if (value != watchForExit) {
                    if (Associated) {
                        if (value) {
                            OpenProcessHandle();
                            EnsureWatchingForExit();
                        }
                        else {
                            StopWatchingForExit();
                        }
                    }
                    watchForExit = value;
                }
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessStandardInput)]
        public StreamWriter StandardInput {
            get { 
                if (standardInput == null) {
                    throw new InvalidOperationException(SR.GetString(SR.CantGetStandardIn));
                }

                return standardInput;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessStandardOutput)]
        public StreamReader StandardOutput {
            get {
                if (standardOutput == null) {
                    throw new InvalidOperationException(SR.GetString(SR.CantGetStandardOut));
                }

                if(outputStreamReadMode == StreamReadMode.undefined) {
                    outputStreamReadMode = StreamReadMode.syncMode;
                }
                else if (outputStreamReadMode != StreamReadMode.syncMode) {
                    throw new InvalidOperationException(SR.GetString(SR.CantMixSyncAsyncOperation));                    
                }
                    
                return standardOutput;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessStandardError)]
        public StreamReader StandardError {
            get { 
                if (standardError == null) {
                    throw new InvalidOperationException(SR.GetString(SR.CantGetStandardError));
                }

                if(errorStreamReadMode == StreamReadMode.undefined) {
                    errorStreamReadMode = StreamReadMode.syncMode;
                }
                else if (errorStreamReadMode != StreamReadMode.syncMode) {
                    throw new InvalidOperationException(SR.GetString(SR.CantMixSyncAsyncOperation));                    
                }

                return standardError;
            }
        }

#if !FEATURE_PAL  
        /// <devdoc>
        ///     Returns the total amount of physical memory the associated process.
        /// </devdoc>
        [Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.WorkingSet64 instead.  http://go.microsoft.com/fwlink/?linkid=14202")]        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessWorkingSet)]
        public int WorkingSet {
            get {
                EnsureState(State.HaveNtProcessInfo);
                return unchecked((int)processInfo.workingSet);
            }
        }
#endif // !FEATURE_PAL

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription(SR.ProcessWorkingSet)]
        [System.Runtime.InteropServices.ComVisible(false)]        
        public long WorkingSet64 {
            get {
                EnsureState(State.HaveNtProcessInfo);
                return processInfo.workingSet;
            }
        }

        [Category("Behavior"), MonitoringDescription(SR.ProcessExited)]
        public event EventHandler Exited {
            add {
                onExited += value;
            }
            remove {
                onExited -= value;
            }
        }

#if !FEATURE_PAL
    
        /// <devdoc>
        ///    <para>
        ///       Closes a process that has a user interface by sending a close message
        ///       to its main window.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]  // Review usages of this.
        [ResourceConsumption(ResourceScope.Machine)]
        public bool CloseMainWindow() {
            IntPtr mainWindowHandle = MainWindowHandle;
            if (mainWindowHandle == (IntPtr)0) return false;
            int style = NativeMethods.GetWindowLong(new HandleRef(this, mainWindowHandle), NativeMethods.GWL_STYLE);
            if ((style & NativeMethods.WS_DISABLED) != 0) return false;
            NativeMethods.PostMessage(new HandleRef(this, mainWindowHandle), NativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            return true;
        }

#endif // !FEATURE_PAL

        /// <devdoc>
        ///     Release the temporary handle we used to get process information.
        ///     If we used the process handle stored in the process object (we have all access to the handle,) don't release it.
        /// </devdoc>
        /// <internalonly/>
        void ReleaseProcessHandle(SafeProcessHandle handle) {
            if (handle == null) { 
                return; 
            }

            if (haveProcessHandle && handle == m_processHandle) {
                return;
            }
            Debug.WriteLineIf(processTracing.TraceVerbose, "Process - CloseHandle(process)");
            handle.Close();
        }

        /// <devdoc>
        ///     This is called from the threadpool when a proces exits.
        /// </devdoc>
        /// <internalonly/>
        private void CompletionCallback(object context, bool wasSignaled) {
            StopWatchingForExit();
            RaiseOnExited();      
        }

        /// <internalonly/>
        /// <devdoc>
        ///    <para>
        ///       Free any resources associated with this component.
        ///    </para>
        /// </devdoc>
        protected override void Dispose(bool disposing) {
            if( !disposed) {
                if (disposing) {
                    //Dispose managed and unmanaged resources
                    Close();
                }
                this.disposed = true;
                base.Dispose(disposing);                
            }            
        }

        /// <devdoc>
        ///    <para>
        ///       Frees any resources associated with this component.
        ///    </para>
        /// </devdoc>
        public void Close() {
            if (Associated) {
                if (haveProcessHandle) {
                    StopWatchingForExit();
                    Debug.WriteLineIf(processTracing.TraceVerbose, "Process - CloseHandle(process) in Close()");
                    m_processHandle.Close();
                    m_processHandle = null;
                    haveProcessHandle = false;
                }
                haveProcessId = false;
                isRemoteMachine = false;
                machineName = ".";
                raisedOnExited = false;

                //Don't call close on the Readers and writers
                //since they might be referenced by somebody else while the 
                //process is still alive but this method called.
                standardOutput = null;
                standardInput = null;
                standardError = null;

                output = null;
                error = null;
	

                Refresh();
            }
        }

        /// <devdoc>
        ///     Helper method for checking preconditions when accessing properties.
        /// </devdoc>
        /// <internalonly/>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        void EnsureState(State state) {

            if ((state & State.IsWin2k) != (State)0) {
#if !FEATURE_PAL
                if (OperatingSystem.Platform != PlatformID.Win32NT || OperatingSystem.Version.Major < 5)
#endif // !FEATURE_PAL
                    throw new PlatformNotSupportedException(SR.GetString(SR.Win2kRequired));
            }

            if ((state & State.IsNt) != (State)0) {
#if !FEATURE_PAL
                if (OperatingSystem.Platform != PlatformID.Win32NT)
#endif // !FEATURE_PAL                    
                    throw new PlatformNotSupportedException(SR.GetString(SR.WinNTRequired));
            }

            if ((state & State.Associated) != (State)0)
                if (!Associated)
                    throw new InvalidOperationException(SR.GetString(SR.NoAssociatedProcess));

            if ((state & State.HaveId) != (State)0) {
                if (!haveProcessId) {
#if !FEATURE_PAL                    
                    if (haveProcessHandle) {
                        SetProcessId(ProcessManager.GetProcessIdFromHandle(m_processHandle));
                     }
                    else {                     
                        EnsureState(State.Associated);
                        throw new InvalidOperationException(SR.GetString(SR.ProcessIdRequired));
                    }
#else
                    EnsureState(State.Associated);
                    throw new InvalidOperationException(SR.GetString(SR.ProcessIdRequired));
#endif // !FEATURE_PAL
                }
            }

            if ((state & State.IsLocal) != (State)0 && isRemoteMachine) {
                    throw new NotSupportedException(SR.GetString(SR.NotSupportedRemote));
            }
            
            if ((state & State.HaveProcessInfo) != (State)0) {
#if !FEATURE_PAL                
                if (processInfo == null) {
                    if ((state & State.HaveId) == (State)0) EnsureState(State.HaveId);
                    ProcessInfo[] processInfos = ProcessManager.GetProcessInfos(machineName);
                    for (int i = 0; i < processInfos.Length; i++) {
                        if (processInfos[i].processId == processId) {
                            this.processInfo = processInfos[i];
                            break;
                        }
                    }
                    if (processInfo == null) {
                        throw new InvalidOperationException(SR.GetString(SR.NoProcessInfo));
                    }
                }
#else
                throw new InvalidOperationException(SR.GetString(SR.NoProcessInfo));
#endif // !FEATURE_PAL
            }

            if ((state & State.Exited) != (State)0) {
                if (!HasExited) {
                    throw new InvalidOperationException(SR.GetString(SR.WaitTillExit));
                }
                
                if (!haveProcessHandle) {
                    throw new InvalidOperationException(SR.GetString(SR.NoProcessHandle));
                }
            }
        }
            
        /// <devdoc>
        ///     Make sure we are watching for a process exit.
        /// </devdoc>
        /// <internalonly/>
        void EnsureWatchingForExit() {
            if (!watchingForExit) {
                lock (this) {
                    if (!watchingForExit) {
                        Debug.Assert(haveProcessHandle, "Process.EnsureWatchingForExit called with no process handle");
                        Debug.Assert(Associated, "Process.EnsureWatchingForExit called with no associated process");
                        watchingForExit = true;
                        try {
                            this.waitHandle = new ProcessWaitHandle(m_processHandle);
                            this.registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(this.waitHandle,
                                new WaitOrTimerCallback(this.CompletionCallback), null, -1, true);                    
                        }
                        catch {
                            watchingForExit = false;
                            throw;
                        }
                    }
                }
            }
        }

#if !FEATURE_PAL    

        /// <devdoc>
        ///     Make sure we have obtained the min and max working set limits.
        /// </devdoc>
        /// <internalonly/>
        void EnsureWorkingSetLimits() {
            EnsureState(State.IsNt);
            if (!haveWorkingSetLimits) {
                SafeProcessHandle handle = null;
                try {
                    handle = GetProcessHandle(NativeMethods.PROCESS_QUERY_INFORMATION);
                    IntPtr min;
                    IntPtr max;
                    if (!NativeMethods.GetProcessWorkingSetSize(handle, out min, out max)) {
                        throw new Win32Exception();
                    }
                    minWorkingSet = min;
                    maxWorkingSet = max;
                    haveWorkingSetLimits = true;
                }
                finally {
                    ReleaseProcessHandle(handle);
                }
            }
        }

        public static void EnterDebugMode() {
            if (ProcessManager.IsNt) {
                SetPrivilege("SeDebugPrivilege", NativeMethods.SE_PRIVILEGE_ENABLED);
            }
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
        private static void SetPrivilege(string privilegeName, int attrib) {
            IntPtr hToken = (IntPtr)0;
            NativeMethods.LUID debugValue = new NativeMethods.LUID();

            // this is only a "pseudo handle" to the current process - no need to close it later
            IntPtr processHandle = NativeMethods.GetCurrentProcess();

            // get the process token so we can adjust the privilege on it.  We DO need to
            // close the token when we're done with it.
            if (!NativeMethods.OpenProcessToken(new HandleRef(null, processHandle), NativeMethods.TOKEN_ADJUST_PRIVILEGES, out hToken)) {
                throw new Win32Exception();
            }

            try {
                if (!NativeMethods.LookupPrivilegeValue(null, privilegeName, out debugValue)) {
                    throw new Win32Exception();
                }
                
                NativeMethods.TokenPrivileges tkp = new NativeMethods.TokenPrivileges();
                tkp.Luid = debugValue;
                tkp.Attributes = attrib;
    
                NativeMethods.AdjustTokenPrivileges(new HandleRef(null, hToken), false, tkp, 0, IntPtr.Zero, IntPtr.Zero);
    
                // AdjustTokenPrivileges can return true even if it failed to
                // set the privilege, so we need to use GetLastError
                if (Marshal.GetLastWin32Error() != NativeMethods.ERROR_SUCCESS) {
                    throw new Win32Exception();
                }
            }
            finally {
                Debug.WriteLineIf(processTracing.TraceVerbose, "Process - CloseHandle(processToken)");
                SafeNativeMethods.CloseHandle(hToken);
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static void LeaveDebugMode() {
            if (ProcessManager.IsNt) {
                SetPrivilege("SeDebugPrivilege", 0);
            }
        }     

        /// <devdoc>
        ///    <para>
        ///       Returns a new <see cref='System.Diagnostics.Process'/> component given a process identifier and
        ///       the name of a computer in the network.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static Process GetProcessById(int processId, string machineName) {
            if (!ProcessManager.IsProcessRunning(processId, machineName)) {
                throw new ArgumentException(SR.GetString(SR.MissingProccess, processId.ToString(CultureInfo.CurrentCulture)));
            }
            
            return new Process(machineName, ProcessManager.IsRemoteMachine(machineName), processId, null);
        }

        /// <devdoc>
        ///    <para>
        ///       Returns a new <see cref='System.Diagnostics.Process'/> component given the
        ///       identifier of a process on the local computer.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static Process GetProcessById(int processId) {
            return GetProcessById(processId, ".");
        }

        /// <devdoc>
        ///    <para>
        ///       Creates an array of <see cref='System.Diagnostics.Process'/> components that are
        ///       associated
        ///       with process resources on the
        ///       local computer. These process resources share the specified process name.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static Process[] GetProcessesByName(string processName) {
            return GetProcessesByName(processName, ".");
        }

        /// <devdoc>
        ///    <para>
        ///       Creates an array of <see cref='System.Diagnostics.Process'/> components that are associated with process resources on a
        ///       remote computer. These process resources share the specified process name.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static Process[] GetProcessesByName(string processName, string machineName) {
            if (processName == null) processName = String.Empty;
            Process[] procs = GetProcesses(machineName);
            ArrayList list = new ArrayList();

            for(int i = 0; i < procs.Length; i++) {                
                if( String.Equals(processName, procs[i].ProcessName, StringComparison.OrdinalIgnoreCase)) {
                    list.Add( procs[i]);                    
                } else {
                    procs[i].Dispose();
                }
            }
            
            Process[] temp = new Process[list.Count];
            list.CopyTo(temp, 0);
            return temp;
        }

        /// <devdoc>
        ///    <para>
        ///       Creates a new <see cref='System.Diagnostics.Process'/>
        ///       component for each process resource on the local computer.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static Process[] GetProcesses() {
            return GetProcesses(".");
        }

        /// <devdoc>
        ///    <para>
        ///       Creates a new <see cref='System.Diagnostics.Process'/>
        ///       component for each
        ///       process resource on the specified computer.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static Process[] GetProcesses(string machineName) {
            bool isRemoteMachine = ProcessManager.IsRemoteMachine(machineName);
            ProcessInfo[] processInfos = ProcessManager.GetProcessInfos(machineName);
            Process[] processes = new Process[processInfos.Length];
            for (int i = 0; i < processInfos.Length; i++) {
                ProcessInfo processInfo = processInfos[i];
                processes[i] = new Process(machineName, isRemoteMachine, processInfo.processId, processInfo);
            }
            Debug.WriteLineIf(processTracing.TraceVerbose, "Process.GetProcesses(" + machineName + ")");
#if DEBUG
            if (processTracing.TraceVerbose) {
                Debug.Indent();
                for (int i = 0; i < processInfos.Length; i++) {
                    Debug.WriteLine(processes[i].Id + ": " + processes[i].ProcessName);
                }
                Debug.Unindent();
            }
#endif
            return processes;
        }

#endif // !FEATURE_PAL        

        /// <devdoc>
        ///    <para>
        ///       Returns a new <see cref='System.Diagnostics.Process'/>
        ///       component and associates it with the current active process.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Process)]
        public static Process GetCurrentProcess() {
            return new Process(".", false, NativeMethods.GetCurrentProcessId(), null);
        }

        /// <devdoc>
        ///    <para>
        ///       Raises the <see cref='System.Diagnostics.Process.Exited'/> event.
        ///    </para>
        /// </devdoc>
        protected void OnExited() {
            EventHandler exited = onExited;
            if (exited != null) {
                if (this.SynchronizingObject != null && this.SynchronizingObject.InvokeRequired)
                    this.SynchronizingObject.BeginInvoke(exited, new object[]{this, EventArgs.Empty});
                else                        
                   exited(this, EventArgs.Empty);                
            }               
        }

        /// <devdoc>
        ///     Gets a short-term handle to the process, with the given access.  
        ///     If a handle is stored in current process object, then use it.
        ///     Note that the handle we stored in current process object will have all access we need.
        /// </devdoc>
        /// <internalonly/>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        SafeProcessHandle GetProcessHandle(int access, bool throwIfExited) {
            Debug.WriteLineIf(processTracing.TraceVerbose, "GetProcessHandle(access = 0x" + access.ToString("X8", CultureInfo.InvariantCulture) + ", throwIfExited = " + throwIfExited + ")");
#if DEBUG
            if (processTracing.TraceVerbose) {
                StackFrame calledFrom = new StackTrace(true).GetFrame(0);
                Debug.WriteLine("   called from " + calledFrom.GetFileName() + ", line " + calledFrom.GetFileLineNumber());
            }
#endif
            if (haveProcessHandle) {
                if (throwIfExited) {
                    // Since haveProcessHandle is true, we know we have the process handle
                    // open with at least SYNCHRONIZE access, so we can wait on it with 
                    // zero timeout to see if the process has exited.
                    ProcessWaitHandle waitHandle = null;
                    try {
                        waitHandle = new ProcessWaitHandle(m_processHandle);             
                        if (waitHandle.WaitOne(0, false)) {
                            if (haveProcessId)
                                throw new InvalidOperationException(SR.GetString(SR.ProcessHasExited, processId.ToString(CultureInfo.CurrentCulture)));
                            else
                                throw new InvalidOperationException(SR.GetString(SR.ProcessHasExitedNoId));
                        }
                    }
                    finally {
                        if( waitHandle != null) {
                            waitHandle.Close();
                        }
                    }            
                }
                return m_processHandle;
            }
            else {
                EnsureState(State.HaveId | State.IsLocal);
                SafeProcessHandle handle = SafeProcessHandle.InvalidHandle;
#if !FEATURE_PAL                
                handle = ProcessManager.OpenProcess(processId, access, throwIfExited);
#else
                IntPtr pseudohandle = NativeMethods.GetCurrentProcess();
                // Get a real handle
                if (!NativeMethods.DuplicateHandle (new HandleRef(this, pseudohandle), 
                                                    new HandleRef(this, pseudohandle), 
                                                    new HandleRef(this, pseudohandle), 
                                                    out handle,
                                                    0, 
                                                    false, 
                                                    NativeMethods.DUPLICATE_SAME_ACCESS | 
                                                    NativeMethods.DUPLICATE_CLOSE_SOURCE)) {
                    throw new Win32Exception();
                }
#endif // !FEATURE_PAL
                if (throwIfExited && (access & NativeMethods.PROCESS_QUERY_INFORMATION) != 0) {         
                    if (NativeMethods.GetExitCodeProcess(handle, out exitCode) && exitCode != NativeMethods.STILL_ACTIVE) {
                        throw new InvalidOperationException(SR.GetString(SR.ProcessHasExited, processId.ToString(CultureInfo.CurrentCulture)));
                    }
                }
                return handle;
            }

        }

        /// <devdoc>
        ///     Gets a short-term handle to the process, with the given access.  If a handle exists,
        ///     then it is reused.  If the process has exited, it throws an exception.
        /// </devdoc>
        /// <internalonly/>
        SafeProcessHandle GetProcessHandle(int access) {
            return GetProcessHandle(access, true);
        }

        /// <devdoc>
        ///     Opens a long-term handle to the process, with all access.  If a handle exists,
        ///     then it is reused.  If the process has exited, it throws an exception.
        /// </devdoc>
        /// <internalonly/>
        SafeProcessHandle OpenProcessHandle() {
            return OpenProcessHandle(NativeMethods.PROCESS_ALL_ACCESS);
        }

        SafeProcessHandle OpenProcessHandle(Int32 access) {
            if (!haveProcessHandle) {
                //Cannot open a new process handle if the object has been disposed, since finalization has been suppressed.            
                if (this.disposed) {
                    throw new ObjectDisposedException(GetType().Name);
                }
                        
                SetProcessHandle(GetProcessHandle(access));
            }                
            return m_processHandle;
        }

        /// <devdoc>
        ///     Raise the Exited event, but make sure we don't do it more than once.
        /// </devdoc>
        /// <internalonly/>
        void RaiseOnExited() {
            if (!raisedOnExited) {
                lock (this) {
                    if (!raisedOnExited) {
                        raisedOnExited = true;
                        OnExited();
                    }
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Discards any information about the associated process
        ///       that has been cached inside the process component. After <see cref='System.Diagnostics.Process.Refresh'/> is called, the
        ///       first request for information for each property causes the process component
        ///       to obtain a new value from the associated process.
        ///    </para>
        /// </devdoc>
        public void Refresh() {
            processInfo = null;
#if !FEATURE_PAL            
            threads = null;
            modules = null;
#endif // !FEATURE_PAL            
            mainWindowTitle = null;
            exited = false;
            signaled = false;
            haveMainWindow = false;
            haveWorkingSetLimits = false;
            haveProcessorAffinity = false;
            havePriorityClass = false;
            haveExitTime = false;
            haveResponding = false;
            havePriorityBoostEnabled = false;
        }

        /// <devdoc>
        ///     Helper to associate a process handle with this component.
        /// </devdoc>
        /// <internalonly/>
        void SetProcessHandle(SafeProcessHandle processHandle) {
            this.m_processHandle = processHandle;
            this.haveProcessHandle = true;
            if (watchForExit) {
                EnsureWatchingForExit();
            }
        }

        /// <devdoc>
        ///     Helper to associate a process id with this component.
        /// </devdoc>
        /// <internalonly/>
        [ResourceExposure(ResourceScope.Machine)]
        void SetProcessId(int processId) {
            this.processId = processId;
            this.haveProcessId = true;
        }

#if !FEATURE_PAL        

        /// <devdoc>
        ///     Helper to set minimum or maximum working set limits.
        /// </devdoc>
        /// <internalonly/>
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        void SetWorkingSetLimits(object newMin, object newMax) {
            EnsureState(State.IsNt);

            SafeProcessHandle handle = null;
            try {
                handle = GetProcessHandle(NativeMethods.PROCESS_QUERY_INFORMATION | NativeMethods.PROCESS_SET_QUOTA);
                IntPtr min;
                IntPtr max;
                if (!NativeMethods.GetProcessWorkingSetSize(handle, out min, out max)) {
                    throw new Win32Exception();
                }
                
                if (newMin != null) {
                    min = (IntPtr)newMin;
                }
                
                if (newMax != null) { 
                    max = (IntPtr)newMax;
                }
                
                if ((long)min > (long)max) {
                    if (newMin != null) {
                        throw new ArgumentException(SR.GetString(SR.BadMinWorkset));
                    }
                    else {
                        throw new ArgumentException(SR.GetString(SR.BadMaxWorkset));
                    }
                }
                
                if (!NativeMethods.SetProcessWorkingSetSize(handle, min, max)) {
                    throw new Win32Exception();
                }
                
                // The value may be rounded/changed by the OS, so go get it
                if (!NativeMethods.GetProcessWorkingSetSize(handle, out min, out max)) {
                    throw new Win32Exception();
                }
                minWorkingSet = min;
                maxWorkingSet = max;
                haveWorkingSetLimits = true;
            }
            finally {
                ReleaseProcessHandle(handle);
            }
        }

#endif // !FEATURE_PAL

        /// <devdoc>
        ///    <para>
        ///       Starts a process specified by the <see cref='System.Diagnostics.Process.StartInfo'/> property of this <see cref='System.Diagnostics.Process'/>
        ///       component and associates it with the
        ///    <see cref='System.Diagnostics.Process'/> . If a process resource is reused 
        ///       rather than started, the reused process is associated with this <see cref='System.Diagnostics.Process'/>
        ///       component.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public bool Start() {
            Close();
            ProcessStartInfo startInfo = StartInfo;
            if (startInfo.FileName.Length == 0) 
                throw new InvalidOperationException(SR.GetString(SR.FileNameMissing));

            if (startInfo.UseShellExecute) {
#if !FEATURE_PAL                
                return StartWithShellExecuteEx(startInfo);
#else
                throw new InvalidOperationException(SR.GetString(SR.net_perm_invalid_val, "StartInfo.UseShellExecute", true));
#endif // !FEATURE_PAL
            } else {
                return StartWithCreateProcess(startInfo);
            }
        }


        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        private static void CreatePipeWithSecurityAttributes(out SafeFileHandle hReadPipe, out SafeFileHandle hWritePipe, NativeMethods.SECURITY_ATTRIBUTES lpPipeAttributes, int nSize) {
            bool ret = NativeMethods.CreatePipe(out hReadPipe, out hWritePipe, lpPipeAttributes, nSize);
            if (!ret || hReadPipe.IsInvalid || hWritePipe.IsInvalid) {
                throw new Win32Exception();
            }
        }

        // Using synchronous Anonymous pipes for process input/output redirection means we would end up 
        // wasting a worker threadpool thread per pipe instance. Overlapped pipe IO is desirable, since 
        // it will take advantage of the NT IO completion port infrastructure. But we can't really use 
        // Overlapped I/O for process input/output as it would break Console apps (managed Console class 
        // methods such as WriteLine as well as native CRT functions like printf) which are making an
        // assumption that the console standard handles (obtained via GetStdHandle()) are opened
        // for synchronous I/O and hence they can work fine with ReadFile/WriteFile synchrnously!
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private void CreatePipe(out SafeFileHandle parentHandle, out SafeFileHandle childHandle, bool parentInputs) {
            NativeMethods.SECURITY_ATTRIBUTES securityAttributesParent = new NativeMethods.SECURITY_ATTRIBUTES();
            securityAttributesParent.bInheritHandle = true;
            
            SafeFileHandle hTmp = null;
            try {
                if (parentInputs) {
                    CreatePipeWithSecurityAttributes(out childHandle, out hTmp, securityAttributesParent, 0);                                                          
                } 
                else {
                    CreatePipeWithSecurityAttributes(out hTmp, 
                                                          out childHandle, 
                                                          securityAttributesParent, 
                                                          0);                                                                              
                }
                // Duplicate the parent handle to be non-inheritable so that the child process 
                // doesn't have access. This is done for correctness sake, exact reason is unclear.
                // One potential theory is that child process can do something brain dead like 
                // closing the parent end of the pipe and there by getting into a blocking situation
                // as parent will not be draining the pipe at the other end anymore. 
                if (!NativeMethods.DuplicateHandle(new HandleRef(this, NativeMethods.GetCurrentProcess()), 
                                                                   hTmp,
                                                                   new HandleRef(this, NativeMethods.GetCurrentProcess()), 
                                                                   out parentHandle,
                                                                   0, 
                                                                   false, 
                                                                   NativeMethods.DUPLICATE_SAME_ACCESS)) {                                                                       
                    throw new Win32Exception();
                }
            }
            finally {
                if( hTmp != null && !hTmp.IsInvalid) {
                    hTmp.Close();
                }
            }
        }            

        private static StringBuilder BuildCommandLine(string executableFileName, string arguments) {
            // Construct a StringBuilder with the appropriate command line
            // to pass to CreateProcess.  If the filename isn't already 
            // in quotes, we quote it here.  This prevents some security
            // problems (it specifies exactly which part of the string
            // is the file to execute).
            StringBuilder commandLine = new StringBuilder();
            string fileName = executableFileName.Trim();
            bool fileNameIsQuoted = (fileName.StartsWith("\"", StringComparison.Ordinal) && fileName.EndsWith("\"", StringComparison.Ordinal));
            if (!fileNameIsQuoted) { 
                commandLine.Append("\"");
            }
            
            commandLine.Append(fileName);
            
            if (!fileNameIsQuoted) {
                commandLine.Append("\"");
            }
            
            if (!String.IsNullOrEmpty(arguments)) {
                commandLine.Append(" ");
                commandLine.Append(arguments);                
            }                        

            return commandLine;
        }
        
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private bool StartWithCreateProcess(ProcessStartInfo startInfo) {
            if( startInfo.StandardOutputEncoding != null && !startInfo.RedirectStandardOutput) {
                throw new InvalidOperationException(SR.GetString(SR.StandardOutputEncodingNotAllowed));
            }

            if( startInfo.StandardErrorEncoding != null && !startInfo.RedirectStandardError) {
                throw new InvalidOperationException(SR.GetString(SR.StandardErrorEncodingNotAllowed));
            }            
            
            // See knowledge base article Q190351 for an explanation of the following code.  Noteworthy tricky points:
            //    * The handles are duplicated as non-inheritable before they are passed to CreateProcess so
            //      that the child process can not close them
            //    * CreateProcess allows you to redirect all or none of the standard IO handles, so we use
            //      GetStdHandle for the handles that are not being redirected

            //Cannot start a new process and store its handle if the object has been disposed, since finalization has been suppressed.            
            if (this.disposed) {
                throw new ObjectDisposedException(GetType().Name);
            }

            StringBuilder commandLine = BuildCommandLine(startInfo.FileName, startInfo.Arguments);

            NativeMethods.STARTUPINFO startupInfo = new NativeMethods.STARTUPINFO();
            SafeNativeMethods.PROCESS_INFORMATION processInfo = new SafeNativeMethods.PROCESS_INFORMATION();
            SafeProcessHandle procSH = new SafeProcessHandle();
            SafeThreadHandle threadSH = new SafeThreadHandle();
            bool retVal;
            int errorCode = 0;
            // handles used in parent process
            SafeFileHandle standardInputWritePipeHandle = null;
            SafeFileHandle standardOutputReadPipeHandle = null;
            SafeFileHandle standardErrorReadPipeHandle = null;
            GCHandle environmentHandle = new GCHandle();            
            lock (s_CreateProcessLock) {
            try {
                // set up the streams
                if (startInfo.RedirectStandardInput || startInfo.RedirectStandardOutput || startInfo.RedirectStandardError) {                        
                    if (startInfo.RedirectStandardInput) {
                        CreatePipe(out standardInputWritePipeHandle, out startupInfo.hStdInput, true);
                        } else {
                        startupInfo.hStdInput  =  new SafeFileHandle(NativeMethods.GetStdHandle(NativeMethods.STD_INPUT_HANDLE), false);
                    }
    
                    if (startInfo.RedirectStandardOutput) {                        
                        CreatePipe(out standardOutputReadPipeHandle, out startupInfo.hStdOutput, false);
                        } else {
                        startupInfo.hStdOutput = new SafeFileHandle(NativeMethods.GetStdHandle(NativeMethods.STD_OUTPUT_HANDLE), false);
                    }
    
                    if (startInfo.RedirectStandardError) {
                        CreatePipe(out standardErrorReadPipeHandle, out startupInfo.hStdError, false);
                        } else {
                        startupInfo.hStdError = new SafeFileHandle(NativeMethods.GetStdHandle(NativeMethods.STD_ERROR_HANDLE), false);
                    }
    
                    startupInfo.dwFlags = NativeMethods.STARTF_USESTDHANDLES;
                }
    
                // set up the creation flags paramater
                int creationFlags = 0;
#if !FEATURE_PAL                
                if (startInfo.CreateNoWindow)  creationFlags |= NativeMethods.CREATE_NO_WINDOW;               
#endif // !FEATURE_PAL                

                // set up the environment block parameter
                IntPtr environmentPtr = (IntPtr)0;
                if (startInfo.environmentVariables != null) {
                    bool unicode = false;
#if !FEATURE_PAL                    
                    if (ProcessManager.IsNt) {
                        creationFlags |= NativeMethods.CREATE_UNICODE_ENVIRONMENT;                
                        unicode = true;
                    }
#endif // !FEATURE_PAL
                    
                    byte[] environmentBytes = EnvironmentBlock.ToByteArray(startInfo.environmentVariables, unicode);
                    environmentHandle = GCHandle.Alloc(environmentBytes, GCHandleType.Pinned);
                    environmentPtr = environmentHandle.AddrOfPinnedObject();
                }

                string workingDirectory = startInfo.WorkingDirectory;
                if (workingDirectory == string.Empty)
                    workingDirectory = Environment.CurrentDirectory;

#if !FEATURE_PAL                    
                if (startInfo.UserName.Length != 0) {                              
                    if (startInfo.Password != null && startInfo.PasswordInClearText != null)
                            throw new ArgumentException(SR.GetString(SR.CantSetDuplicatePassword));

                    NativeMethods.LogonFlags logonFlags = (NativeMethods.LogonFlags)0;                    
                    if( startInfo.LoadUserProfile) {
                        logonFlags = NativeMethods.LogonFlags.LOGON_WITH_PROFILE;
                    }

                    IntPtr password = IntPtr.Zero;
                    try {
                        if( startInfo.Password != null) {
                            password = Marshal.SecureStringToCoTaskMemUnicode(startInfo.Password);
                        } else if( startInfo.PasswordInClearText != null) {
                            password = Marshal.StringToCoTaskMemUni(startInfo.PasswordInClearText);
                        } else {
                            password = Marshal.StringToCoTaskMemUni(String.Empty);
                        }

                        RuntimeHelpers.PrepareConstrainedRegions();
                        try {} finally {
                           retVal = NativeMethods.CreateProcessWithLogonW(
                                   startInfo.UserName,
                                   startInfo.Domain,
                                   password,
                                   logonFlags,
                                   null,            // we don't need this since all the info is in commandLine
                                   commandLine,
                                   creationFlags,
                                   environmentPtr,
                                   workingDirectory,
                                   startupInfo,        // pointer to STARTUPINFO
                                   processInfo         // pointer to PROCESS_INFORMATION
                               ); 
                           if (!retVal)                            
                              errorCode = Marshal.GetLastWin32Error();
                           if ( processInfo.hProcess!= (IntPtr)0 && processInfo.hProcess!= (IntPtr)NativeMethods.INVALID_HANDLE_VALUE)
                              procSH.InitialSetHandle(processInfo.hProcess);  
                           if ( processInfo.hThread != (IntPtr)0 && processInfo.hThread != (IntPtr)NativeMethods.INVALID_HANDLE_VALUE)
                              threadSH.InitialSetHandle(processInfo.hThread);            
                        }
                        if (!retVal){                            
                                if (errorCode == NativeMethods.ERROR_BAD_EXE_FORMAT || errorCode == NativeMethods.ERROR_EXE_MACHINE_TYPE_MISMATCH) {
                                throw new Win32Exception(errorCode, SR.GetString(SR.InvalidApplication));
                            }

                            throw new Win32Exception(errorCode);
                        }
                        } finally {
                        if( password != IntPtr.Zero) {
                            Marshal.ZeroFreeCoTaskMemUnicode(password);
                        }
                    }
                    } else {
#endif // !FEATURE_PAL
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try {} finally {
                       retVal = NativeMethods.CreateProcess (
                               null,               // we don't need this since all the info is in commandLine
                               commandLine,        // pointer to the command line string
                               null,               // pointer to process security attributes, we don't need to inheriat the handle
                               null,               // pointer to thread security attributes
                               true,               // handle inheritance flag
                               creationFlags,      // creation flags
                               environmentPtr,     // pointer to new environment block
                               workingDirectory,   // pointer to current directory name
                               startupInfo,        // pointer to STARTUPINFO
                               processInfo         // pointer to PROCESS_INFORMATION
                           );
                       if (!retVal)                            
                              errorCode = Marshal.GetLastWin32Error();
                       if ( processInfo.hProcess!= (IntPtr)0 && processInfo.hProcess!= (IntPtr)NativeMethods.INVALID_HANDLE_VALUE)
                           procSH.InitialSetHandle(processInfo.hProcess);  
                       if ( processInfo.hThread != (IntPtr)0 && processInfo.hThread != (IntPtr)NativeMethods.INVALID_HANDLE_VALUE)
                          threadSH.InitialSetHandle(processInfo.hThread);                    
                    }
                    if (!retVal) {
                            if (errorCode == NativeMethods.ERROR_BAD_EXE_FORMAT || errorCode == NativeMethods.ERROR_EXE_MACHINE_TYPE_MISMATCH) {
                          throw new Win32Exception(errorCode, SR.GetString(SR.InvalidApplication));
                       }
                        throw new Win32Exception(errorCode);
                    }
#if !FEATURE_PAL                    
                }
#endif
                } finally {
                // free environment block
                if (environmentHandle.IsAllocated) {
                    environmentHandle.Free();   
                }

                startupInfo.Dispose();
            }
            }

            if (startInfo.RedirectStandardInput) {
                standardInput = new StreamWriter(new FileStream(standardInputWritePipeHandle, FileAccess.Write, 4096, false), Console.InputEncoding, 4096);
                standardInput.AutoFlush = true;
            }
            if (startInfo.RedirectStandardOutput) {
                Encoding enc = (startInfo.StandardOutputEncoding != null) ? startInfo.StandardOutputEncoding : Console.OutputEncoding;
                standardOutput = new StreamReader(new FileStream(standardOutputReadPipeHandle, FileAccess.Read, 4096, false), enc, true, 4096);
            }
            if (startInfo.RedirectStandardError) {
                Encoding enc = (startInfo.StandardErrorEncoding != null) ? startInfo.StandardErrorEncoding : Console.OutputEncoding;
                standardError = new StreamReader(new FileStream(standardErrorReadPipeHandle, FileAccess.Read, 4096, false), enc, true, 4096);
            }
            
            bool ret = false;
            if (!procSH.IsInvalid) {
                SetProcessHandle(procSH);
                SetProcessId(processInfo.dwProcessId);
                threadSH.Close();
                ret = true;
            }

            return ret;

        }

#if !FEATURE_PAL

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private bool StartWithShellExecuteEx(ProcessStartInfo startInfo) {                        
            //Cannot start a new process and store its handle if the object has been disposed, since finalization has been suppressed.            
            if (this.disposed)
                throw new ObjectDisposedException(GetType().Name);

            if( !String.IsNullOrEmpty(startInfo.UserName) || (startInfo.Password != null) ) {
                throw new InvalidOperationException(SR.GetString(SR.CantStartAsUser));                
            }
            
            if (startInfo.RedirectStandardInput || startInfo.RedirectStandardOutput || startInfo.RedirectStandardError) {
                throw new InvalidOperationException(SR.GetString(SR.CantRedirectStreams));
            }

            if (startInfo.StandardErrorEncoding != null) {
                throw new InvalidOperationException(SR.GetString(SR.StandardErrorEncodingNotAllowed));
            }

            if (startInfo.StandardOutputEncoding != null) {
                throw new InvalidOperationException(SR.GetString(SR.StandardOutputEncodingNotAllowed));
            }

            // can't set env vars with ShellExecuteEx...
            if (startInfo.environmentVariables != null) {
                throw new InvalidOperationException(SR.GetString(SR.CantUseEnvVars));
            }

            NativeMethods.ShellExecuteInfo shellExecuteInfo = new NativeMethods.ShellExecuteInfo();
            shellExecuteInfo.fMask = NativeMethods.SEE_MASK_NOCLOSEPROCESS;
            if (startInfo.ErrorDialog) {
                shellExecuteInfo.hwnd = startInfo.ErrorDialogParentHandle;
            }
            else {
                shellExecuteInfo.fMask |= NativeMethods.SEE_MASK_FLAG_NO_UI;
            }

            switch (startInfo.WindowStyle) {
                case ProcessWindowStyle.Hidden:
                    shellExecuteInfo.nShow = NativeMethods.SW_HIDE;
                    break;
                case ProcessWindowStyle.Minimized:
                    shellExecuteInfo.nShow = NativeMethods.SW_SHOWMINIMIZED;
                    break;
                case ProcessWindowStyle.Maximized:
                    shellExecuteInfo.nShow = NativeMethods.SW_SHOWMAXIMIZED;
                    break;
                default:
                    shellExecuteInfo.nShow = NativeMethods.SW_SHOWNORMAL;
                    break;
            }

            
            try {
                if (startInfo.FileName.Length != 0)
                    shellExecuteInfo.lpFile = Marshal.StringToHGlobalAuto(startInfo.FileName);
                if (startInfo.Verb.Length != 0)
                    shellExecuteInfo.lpVerb = Marshal.StringToHGlobalAuto(startInfo.Verb);
                if (startInfo.Arguments.Length != 0)
                    shellExecuteInfo.lpParameters = Marshal.StringToHGlobalAuto(startInfo.Arguments);
                if (startInfo.WorkingDirectory.Length != 0)
                    shellExecuteInfo.lpDirectory = Marshal.StringToHGlobalAuto(startInfo.WorkingDirectory);

                shellExecuteInfo.fMask |= NativeMethods.SEE_MASK_FLAG_DDEWAIT;

                ShellExecuteHelper executeHelper = new ShellExecuteHelper(shellExecuteInfo);
                if (!executeHelper.ShellExecuteOnSTAThread()) {
                    int error = executeHelper.ErrorCode;
                    if (error == 0) {
                        switch ((long)shellExecuteInfo.hInstApp) {
                            case NativeMethods.SE_ERR_FNF: error = NativeMethods.ERROR_FILE_NOT_FOUND; break;
                            case NativeMethods.SE_ERR_PNF: error = NativeMethods.ERROR_PATH_NOT_FOUND; break;
                            case NativeMethods.SE_ERR_ACCESSDENIED: error = NativeMethods.ERROR_ACCESS_DENIED; break;
                            case NativeMethods.SE_ERR_OOM: error = NativeMethods.ERROR_NOT_ENOUGH_MEMORY; break;
                            case NativeMethods.SE_ERR_DDEFAIL:
                            case NativeMethods.SE_ERR_DDEBUSY:
                            case NativeMethods.SE_ERR_DDETIMEOUT: error = NativeMethods.ERROR_DDE_FAIL; break;
                            case NativeMethods.SE_ERR_SHARE: error = NativeMethods.ERROR_SHARING_VIOLATION; break;
                            case NativeMethods.SE_ERR_NOASSOC: error = NativeMethods.ERROR_NO_ASSOCIATION; break;
                            case NativeMethods.SE_ERR_DLLNOTFOUND: error = NativeMethods.ERROR_DLL_NOT_FOUND; break;
                            default: error = (int)shellExecuteInfo.hInstApp; break;
                        }
                    }
                    if( error == NativeMethods.ERROR_BAD_EXE_FORMAT || error == NativeMethods.ERROR_EXE_MACHINE_TYPE_MISMATCH) {
                        throw new Win32Exception(error, SR.GetString(SR.InvalidApplication));
                    }
                    throw new Win32Exception(error);
                }
            
            }
            finally {                
                if (shellExecuteInfo.lpFile != (IntPtr)0) Marshal.FreeHGlobal(shellExecuteInfo.lpFile);
                if (shellExecuteInfo.lpVerb != (IntPtr)0) Marshal.FreeHGlobal(shellExecuteInfo.lpVerb);
                if (shellExecuteInfo.lpParameters != (IntPtr)0) Marshal.FreeHGlobal(shellExecuteInfo.lpParameters);
                if (shellExecuteInfo.lpDirectory != (IntPtr)0) Marshal.FreeHGlobal(shellExecuteInfo.lpDirectory);
            }

            if (shellExecuteInfo.hProcess != (IntPtr)0) {                
                SafeProcessHandle handle = new SafeProcessHandle(shellExecuteInfo.hProcess);
                SetProcessHandle(handle);
                return true;
            }
            
            return false;            
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static Process Start( string fileName, string userName, SecureString password, string domain ) {
            ProcessStartInfo startInfo = new ProcessStartInfo(fileName);            
            startInfo.UserName = userName;
            startInfo.Password = password;
            startInfo.Domain = domain;
            startInfo.UseShellExecute = false;
            return Start(startInfo);
        }
        
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static Process Start( string fileName, string arguments, string userName, SecureString password, string domain ) {
            ProcessStartInfo startInfo = new ProcessStartInfo(fileName, arguments);                        
            startInfo.UserName = userName;
            startInfo.Password = password;
            startInfo.Domain = domain;
            startInfo.UseShellExecute = false;            
            return Start(startInfo);            
        }


#endif // !FEATURE_PAL

        /// <devdoc>
        ///    <para>
        ///       Starts a process resource by specifying the name of a
        ///       document or application file. Associates the process resource with a new <see cref='System.Diagnostics.Process'/>
        ///       component.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static Process Start(string fileName) {
            return Start(new ProcessStartInfo(fileName));
        }

        /// <devdoc>
        ///    <para>
        ///       Starts a process resource by specifying the name of an
        ///       application and a set of command line arguments. Associates the process resource
        ///       with a new <see cref='System.Diagnostics.Process'/>
        ///       component.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static Process Start(string fileName, string arguments) {
            return Start(new ProcessStartInfo(fileName, arguments));
        }

        /// <devdoc>
        ///    <para>
        ///       Starts a process resource specified by the process start
        ///       information passed in, for example the file name of the process to start.
        ///       Associates the process resource with a new <see cref='System.Diagnostics.Process'/>
        ///       component.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static Process Start(ProcessStartInfo startInfo) {
            Process process = new Process();
            if (startInfo == null) throw new ArgumentNullException("startInfo");
            process.StartInfo = startInfo;
            if (process.Start()) {
                return process;
            }
            return null;
        }

        /// <devdoc>
        ///    <para>
        ///       Stops the
        ///       associated process immediately.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public void Kill() {
            SafeProcessHandle handle = null;
            try {
                handle = GetProcessHandle(NativeMethods.PROCESS_TERMINATE);
                if (!NativeMethods.TerminateProcess(handle, -1))
                    throw new Win32Exception();
            }
            finally {
                ReleaseProcessHandle(handle);
            }
        }

        /// <devdoc>
        ///     Make sure we are not watching for process exit.
        /// </devdoc>
        /// <internalonly/>
        void StopWatchingForExit() {
            if (watchingForExit) {
                lock (this) {
                    if (watchingForExit) {
                        watchingForExit = false;
                        registeredWaitHandle.Unregister(null);
                        waitHandle.Close();
                        waitHandle = null;
                        registeredWaitHandle = null;
                    }
                }
            }
        }

        public override string ToString() {
#if !FEATURE_PAL        
            if (Associated) {
                string processName  =  String.Empty;  
                //
                // On windows 9x, we can't map a handle to an id.
                // So ProcessName will throw. We shouldn't throw in Process.ToString though.
                // Process.GetProcesses should be used to get all the processes on the machine.
                // The processes returned from it will have a nice name.
                //
                try {
                    processName = this.ProcessName;
                }    
                catch(PlatformNotSupportedException) {
                }
                if( processName.Length != 0) { 
                    return String.Format(CultureInfo.CurrentCulture, "{0} ({1})", base.ToString(), processName);
                }
                return base.ToString();
            }    
            else
#endif // !FEATURE_PAL                
                return base.ToString();
        }

        /// <devdoc>
        ///    <para>
        ///       Instructs the <see cref='System.Diagnostics.Process'/> component to wait the specified number of milliseconds for the associated process to exit.
        ///    </para>
        /// </devdoc>
        public bool WaitForExit(int milliseconds) {
            SafeProcessHandle handle = null;
	     bool exited;
            ProcessWaitHandle processWaitHandle = null;
            try {
                handle = GetProcessHandle(NativeMethods.SYNCHRONIZE, false);                
                if (handle.IsInvalid) {
                    exited = true;
                }
                else {
                    processWaitHandle = new ProcessWaitHandle(handle);
                    if( processWaitHandle.WaitOne(milliseconds, false)) {
                        exited = true;
                        signaled = true;
                    }
                    else {
                        exited = false;
                        signaled = false;
                    }
                }
            }
            finally {
                if( processWaitHandle != null) {
                    processWaitHandle.Close();
                }

                // If we have a hard timeout, we cannot wait for the streams
                if( output != null && milliseconds == -1) {
                    output.WaitUtilEOF();
                }

                if( error != null && milliseconds == -1) {
                    error.WaitUtilEOF();
                }

                ReleaseProcessHandle(handle);

            }
            
            if (exited && watchForExit) {
                RaiseOnExited();
            }
			
            return exited;
        }

        /// <devdoc>
        ///    <para>
        ///       Instructs the <see cref='System.Diagnostics.Process'/> component to wait
        ///       indefinitely for the associated process to exit.
        ///    </para>
        /// </devdoc>
        public void WaitForExit() {
            WaitForExit(-1);
        }

#if !FEATURE_PAL        

        /// <devdoc>
        ///    <para>
        ///       Causes the <see cref='System.Diagnostics.Process'/> component to wait the
        ///       specified number of milliseconds for the associated process to enter an
        ///       idle state.
        ///       This is only applicable for processes with a user interface,
        ///       therefore a message loop.
        ///    </para>
        /// </devdoc>
        public bool WaitForInputIdle(int milliseconds) {
            SafeProcessHandle handle = null;
            bool idle;
            try {
                handle = GetProcessHandle(NativeMethods.SYNCHRONIZE | NativeMethods.PROCESS_QUERY_INFORMATION);
                int ret = NativeMethods.WaitForInputIdle(handle, milliseconds);
                switch (ret) {
                    case NativeMethods.WAIT_OBJECT_0:
                        idle = true;
                        break;
                    case NativeMethods.WAIT_TIMEOUT:
                        idle = false;
                        break;
                    case NativeMethods.WAIT_FAILED:
                    default:
                        throw new InvalidOperationException(SR.GetString(SR.InputIdleUnkownError));
                }
            }
            finally {
                ReleaseProcessHandle(handle);
            }
            return idle;
        }

        /// <devdoc>
        ///    <para>
        ///       Instructs the <see cref='System.Diagnostics.Process'/> component to wait
        ///       indefinitely for the associated process to enter an idle state. This
        ///       is only applicable for processes with a user interface, therefore a message loop.
        ///    </para>
        /// </devdoc>
        public bool WaitForInputIdle() {
            return WaitForInputIdle(Int32.MaxValue);
        }

#endif // !FEATURE_PAL        

        // Support for working asynchronously with streams
        /// <devdoc>
        /// <para>
        /// Instructs the <see cref='System.Diagnostics.Process'/> component to start
        /// reading the StandardOutput stream asynchronously. The user can register a callback
        /// that will be called when a line of data terminated by \n,\r or \r\n is reached, or the end of stream is reached
        /// then the remaining information is returned. The user can add an event handler to OutputDataReceived.
        /// </para>
        /// </devdoc>
        [System.Runtime.InteropServices.ComVisible(false)]        
        public void BeginOutputReadLine() {

            if(outputStreamReadMode == StreamReadMode.undefined) {
                outputStreamReadMode = StreamReadMode.asyncMode;
            }
            else if (outputStreamReadMode != StreamReadMode.asyncMode) {
                throw new InvalidOperationException(SR.GetString(SR.CantMixSyncAsyncOperation));                    
            }
            
            if (pendingOutputRead)
                throw new InvalidOperationException(SR.GetString(SR.PendingAsyncOperation));

            pendingOutputRead = true;
            // We can't detect if there's a pending sychronous read, tream also doesn't.
            if (output == null) {
                if (standardOutput == null) {
                    throw new InvalidOperationException(SR.GetString(SR.CantGetStandardOut));
                }

                Stream s = standardOutput.BaseStream;
                output = new AsyncStreamReader(this, s, new UserCallBack(this.OutputReadNotifyUser), standardOutput.CurrentEncoding);
            }
            output.BeginReadLine();
        }


        /// <devdoc>
        /// <para>
        /// Instructs the <see cref='System.Diagnostics.Process'/> component to start
        /// reading the StandardError stream asynchronously. The user can register a callback
        /// that will be called when a line of data terminated by \n,\r or \r\n is reached, or the end of stream is reached
        /// then the remaining information is returned. The user can add an event handler to ErrorDataReceived.
        /// </para>
        /// </devdoc>
        [System.Runtime.InteropServices.ComVisible(false)]        
        public void BeginErrorReadLine() {

            if(errorStreamReadMode == StreamReadMode.undefined) {
                errorStreamReadMode = StreamReadMode.asyncMode;
            }
            else if (errorStreamReadMode != StreamReadMode.asyncMode) {
                throw new InvalidOperationException(SR.GetString(SR.CantMixSyncAsyncOperation));                    
            }
            
            if (pendingErrorRead) {
                throw new InvalidOperationException(SR.GetString(SR.PendingAsyncOperation));
            }

            pendingErrorRead = true;
            // We can't detect if there's a pending sychronous read, stream also doesn't.
            if (error == null) {
                if (standardError == null) {
                    throw new InvalidOperationException(SR.GetString(SR.CantGetStandardError));
                }

                Stream s = standardError.BaseStream;
                error = new AsyncStreamReader(this, s, new UserCallBack(this.ErrorReadNotifyUser), standardError.CurrentEncoding);
            }
            error.BeginReadLine();
        }

        /// <devdoc>
        /// <para>
        /// Instructs the <see cref='System.Diagnostics.Process'/> component to cancel the asynchronous operation
        /// specified by BeginOutputReadLine().
        /// </para>
        /// </devdoc>
        [System.Runtime.InteropServices.ComVisible(false)]
        public void CancelOutputRead() {        
            if (output != null) {
                output.CancelOperation();
            }
            else {
                throw new InvalidOperationException(SR.GetString(SR.NoAsyncOperation));
            }

            pendingOutputRead = false;
        }

        /// <devdoc>
        /// <para>
        /// Instructs the <see cref='System.Diagnostics.Process'/> component to cancel the asynchronous operation
        /// specified by BeginErrorReadLine().
        /// </para>
        /// </devdoc>
        [System.Runtime.InteropServices.ComVisible(false)]        
        public void CancelErrorRead() {
            if (error != null) {
                error.CancelOperation();
            }
            else {
                throw new InvalidOperationException(SR.GetString(SR.NoAsyncOperation));
            }

            pendingErrorRead = false;
        }

        internal void OutputReadNotifyUser(String data) {
            // To avoid ---- between remove handler and raising the event
            DataReceivedEventHandler outputDataReceived = OutputDataReceived;
            if (outputDataReceived != null) {
                DataReceivedEventArgs e = new DataReceivedEventArgs(data);
                if (SynchronizingObject != null && SynchronizingObject.InvokeRequired) {
                    SynchronizingObject.Invoke(outputDataReceived, new object[] {this, e});
                }
                else {
                    outputDataReceived(this,e);  // Call back to user informing data is available.
                }
            }
        }

        internal void ErrorReadNotifyUser(String data) {
            // To avoid ---- between remove handler and raising the event
            DataReceivedEventHandler errorDataReceived = ErrorDataReceived;
            if (errorDataReceived != null) {
                DataReceivedEventArgs e = new DataReceivedEventArgs(data);
                if (SynchronizingObject != null && SynchronizingObject.InvokeRequired) {
                    SynchronizingObject.Invoke(errorDataReceived, new object[] {this, e});
                }
                else {
                    errorDataReceived(this,e); // Call back to user informing data is available.
                }
            }
        }

        /// <summary>
        ///     A desired internal state.
        /// </summary>
        /// <internalonly/>
        enum State {
            HaveId = 0x1,
            IsLocal = 0x2,
            IsNt = 0x4,
            HaveProcessInfo = 0x8,
            Exited = 0x10,
            Associated = 0x20,
            IsWin2k = 0x40,
            HaveNtProcessInfo = HaveProcessInfo | IsNt
        }
    }

    /// <devdoc>
    ///     This data structure contains information about a process that is collected
    ///     in bulk by querying the operating system.  The reason to make this a separate
    ///     structure from the process component is so that we can throw it away all at once
    ///     when Refresh is called on the component.
    /// </devdoc>
    /// <internalonly/>
    internal class ProcessInfo {
        public ArrayList threadInfoList = new ArrayList();
        public int basePriority;
        public string processName;
        public int processId;
        public int handleCount;
        public long poolPagedBytes;
        public long poolNonpagedBytes;
        public long virtualBytes;
        public long virtualBytesPeak;
        public long workingSetPeak;
        public long workingSet;
        public long pageFileBytesPeak;
        public long pageFileBytes;
        public long privateBytes;
        public int mainModuleId; // used only for win9x - id is only for use with CreateToolHelp32
        public int sessionId; 
    }

    /// <devdoc>
    ///     This data structure contains information about a thread in a process that
    ///     is collected in bulk by querying the operating system.  The reason to
    ///     make this a separate structure from the ProcessThread component is so that we
    ///     can throw it away all at once when Refresh is called on the component.
    /// </devdoc>
    /// <internalonly/>
    internal class ThreadInfo {
        public int threadId;
        public int processId;
        public int basePriority;
        public int currentPriority;
        public IntPtr startAddress;
        public ThreadState threadState;
#if !FEATURE_PAL        
        public ThreadWaitReason threadWaitReason;
#endif // !FEATURE_PAL
    }

    /// <devdoc>
    ///     This data structure contains information about a module in a process that
    ///     is collected in bulk by querying the operating system.  The reason to
    ///     make this a separate structure from the ProcessModule component is so that we
    ///     can throw it away all at once when Refresh is called on the component.
    /// </devdoc>
    /// <internalonly/>
    internal class ModuleInfo {
        public string baseName;
        public string fileName;
        public IntPtr baseOfDll;
        public IntPtr entryPoint;
        public int sizeOfImage;
        public int Id; // used only on win9x - for matching up with ProcessInfo.mainModuleId
    }

    internal static class EnvironmentBlock {
        public static byte[] ToByteArray(StringDictionary sd, bool unicode) {
            // get the keys
            string[] keys = new string[sd.Count];
            byte[] envBlock = null;
            sd.Keys.CopyTo(keys, 0);
            
            // get the values
            string[] values = new string[sd.Count];
            sd.Values.CopyTo(values, 0);
            
            // sort both by the keys
            // Windows 2000 requires the environment block to be sorted by the key
            // It will first converting the case the strings and do ordinal comparison.
            Array.Sort(keys, values, OrdinalCaseInsensitiveComparer.Default);

            // create a list of null terminated "key=val" strings
            StringBuilder stringBuff = new StringBuilder();
            for (int i = 0; i < sd.Count; ++ i) {
                stringBuff.Append(keys[i]);
                stringBuff.Append('=');
                stringBuff.Append(values[i]);
                stringBuff.Append('\0');
            }
            // an extra null at the end indicates end of list.
            stringBuff.Append('\0');
                        
            if( unicode) {
                envBlock = Encoding.Unicode.GetBytes(stringBuff.ToString());                        
            }
            else {
                envBlock = Encoding.Default.GetBytes(stringBuff.ToString());

                if (envBlock.Length > UInt16.MaxValue)
                    throw new InvalidOperationException(SR.GetString(SR.EnvironmentBlockTooLong, envBlock.Length));
            }

            return envBlock;
        }        
    }

    internal class OrdinalCaseInsensitiveComparer : IComparer {
        internal static readonly OrdinalCaseInsensitiveComparer Default = new OrdinalCaseInsensitiveComparer();
        
        public int Compare(Object a, Object b) {
            String sa = a as String;
            String sb = b as String;
            if (sa != null && sb != null) {
                return String.Compare(sa, sb, StringComparison.OrdinalIgnoreCase); 
            }
            return Comparer.Default.Compare(a,b);
        }
    }

    internal class ProcessThreadTimes {
        internal long create;
        internal long exit; 
        internal long kernel; 
        internal long user;

        public DateTime StartTime {   
            get {             
                return DateTime.FromFileTime(create);
            }
        }

        public DateTime ExitTime {
            get {
                return DateTime.FromFileTime(exit);
            }
        }

        public TimeSpan PrivilegedProcessorTime {
            get {
                return new TimeSpan(kernel);
            }
        }

        public TimeSpan UserProcessorTime {
            get {
                return new TimeSpan(user);
            }
        }
        
        public TimeSpan TotalProcessorTime {
            get {
                return new TimeSpan(user + kernel);
            }
        }
    }

    internal class ShellExecuteHelper {
        private NativeMethods.ShellExecuteInfo _executeInfo;
        private int _errorCode;
        private bool _succeeded;
        
        public ShellExecuteHelper(NativeMethods.ShellExecuteInfo executeInfo) {
            _executeInfo = executeInfo;            
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public void ShellExecuteFunction()    {
            if (!(_succeeded = NativeMethods.ShellExecuteEx(_executeInfo))) {
                _errorCode = Marshal.GetLastWin32Error();
            }
        }

        public bool ShellExecuteOnSTAThread() {
            //
            // SHELL API ShellExecute() requires STA in order to work correctly.
            // If current thread is not a STA thread, we need to call ShellExecute on a new thread.
            //
            if( Thread.CurrentThread.GetApartmentState() != ApartmentState.STA) {
                ThreadStart threadStart = new ThreadStart(this.ShellExecuteFunction);
                Thread executionThread = new Thread(threadStart);
                executionThread.SetApartmentState(ApartmentState.STA);    
                executionThread.Start();
                executionThread.Join();
            }    
            else {
                ShellExecuteFunction();
            }
            return _succeeded;
        }        

        public int ErrorCode {
            get { 
                return _errorCode; 
            }
        }
    }
}
