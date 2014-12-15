//------------------------------------------------------------------------------
// <copyright file="_LoggingObject.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

//
//  We have function based stack and thread based logging of basic behavior.  We
//  also now have the ability to run a "watch thread" which does basic hang detection
//  and error-event based logging.   The logging code buffers the callstack/picture
//  of all COMNET threads, and upon error from an assert or a hang, it will open a file
//  and dump the snapsnot.  Future work will allow this to be configed by registry and
//  to use Runtime based logging.  We'd also like to have different levels of logging.
//

namespace System.Net {
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Diagnostics;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Security;
    using Microsoft.Win32;
    using System.Runtime.ConstrainedExecution;
    using System.Globalization;
    using System.Configuration;

    //
    // BaseLoggingObject - used to disable logging,
    //  this is just a base class that does nothing.
    //

    internal class BaseLoggingObject {

        internal BaseLoggingObject() {
        }

        internal virtual void EnterFunc(string funcname) {
        }

        internal virtual void LeaveFunc(string funcname) {
        }

        internal virtual void DumpArrayToConsole() {
        }

        internal virtual void PrintLine(string msg) {
        }

        internal virtual void DumpArray(bool shouldClose) {
        }

        internal virtual void DumpArrayToFile(bool shouldClose) {
        }

        internal virtual void Flush() {
        }

        internal virtual void Flush(bool close) {
        }

        internal virtual void LoggingMonitorTick() {
        }

        internal virtual void Dump(byte[] buffer) {
        }

        internal virtual void Dump(byte[] buffer, int length) {
        }

        internal virtual void Dump(byte[] buffer, int offset, int length) {
        }

        internal virtual void Dump(IntPtr pBuffer, int offset, int length) {
        }

    } // class BaseLoggingObject

#if TRAVE
    /// <internalonly/>
    /// <devdoc>
    /// </devdoc>
    internal class LoggingObject : BaseLoggingObject {
        public ArrayList _Logarray;
        private Hashtable _ThreadNesting;
        private int _AddCount;
        private StreamWriter _Stream;
        private int _IamAlive;
        private int _LastIamAlive;
        private bool _Finalized = false;
        private double _NanosecondsPerTick;
        private int _StartMilliseconds;
        private long _StartTicks;

        internal LoggingObject() : base() {
            _Logarray      = new ArrayList();
            _ThreadNesting = new Hashtable();
            _AddCount      = 0;
            _IamAlive      = 0;
            _LastIamAlive  = -1;

            if (GlobalLog.s_UsePerfCounter) {
                long ticksPerSecond;
                SafeNativeMethods.QueryPerformanceFrequency(out ticksPerSecond);
                _NanosecondsPerTick = 10000000.0/(double)ticksPerSecond;
                SafeNativeMethods.QueryPerformanceCounter(out _StartTicks);
            } else {
                _StartMilliseconds = Environment.TickCount;
            }
        }

        //
        // LoggingMonitorTick - this function is run from the monitor thread,
        //  and used to check to see if there any hangs, ie no logging
        //  activitity
        //

        internal override void LoggingMonitorTick() {
            if ( _LastIamAlive == _IamAlive ) {
                PrintLine("================= Error TIMEOUT - HANG DETECTED =================");
                DumpArray(true);
            }
            _LastIamAlive = _IamAlive;
        }

        internal override void EnterFunc(string funcname) {
            if (_Finalized) {
                return;
            }
            IncNestingCount();
            ValidatePush(funcname);
            PrintLine(funcname);
        }

        internal override void LeaveFunc(string funcname) {
            if (_Finalized) {
                return;
            }
            PrintLine(funcname);
            DecNestingCount();
            ValidatePop(funcname);
        }

        internal override void DumpArrayToConsole() {
            for (int i=0; i < _Logarray.Count; i++) {
                Console.WriteLine((string) _Logarray[i]);
            }
        }

        internal override void PrintLine(string msg) {
            if (_Finalized) {
                return;
            }
            string spc = "";

            _IamAlive++;

            spc = GetNestingString();

            string tickString = "";

            if (GlobalLog.s_UsePerfCounter) {
                long nowTicks;
                SafeNativeMethods.QueryPerformanceCounter(out nowTicks);
                if (_StartTicks>nowTicks) { // counter reset, restart from 0
                    _StartTicks = nowTicks;
                }
                nowTicks -= _StartTicks;
                if (GlobalLog.s_UseTimeSpan) {
                    tickString = new TimeSpan((long)(nowTicks*_NanosecondsPerTick)).ToString();
                    // note: TimeSpan().ToString() doesn't return the uSec part
                    // if its 0. .ToString() returns [H*]HH:MM:SS:uuuuuuu, hence 16
                    if (tickString.Length < 16) {
                        tickString += ".0000000";
                    }
                }
                else {
                    tickString = ((double)nowTicks*_NanosecondsPerTick/10000).ToString("f3");
                }
            }
            else {
                int nowMilliseconds = Environment.TickCount;
                if (_StartMilliseconds>nowMilliseconds) {
                    _StartMilliseconds = nowMilliseconds;
                }
                nowMilliseconds -= _StartMilliseconds;
                if (GlobalLog.s_UseTimeSpan) {
                    tickString = new TimeSpan(nowMilliseconds*10000).ToString();
                    // note: TimeSpan().ToString() doesn't return the uSec part
                    // if its 0. .ToString() returns [H*]HH:MM:SS:uuuuuuu, hence 16
                    if (tickString.Length < 16) {
                        tickString += ".0000000";
                    }
                }
                else {
                    tickString = nowMilliseconds.ToString();
                }
            }

            uint threadId = 0;

            if (GlobalLog.s_UseThreadId) {
                try {
                    object threadData = Thread.GetData(GlobalLog.s_ThreadIdSlot);
                    if (threadData!= null) {
                        threadId = (uint)threadData;
                    }

                }
                catch(Exception exception) {
                    if (exception is ThreadAbortException || exception is StackOverflowException || exception is OutOfMemoryException) {
                        throw;
                    }
                }
                if (threadId == 0) {
                    threadId = UnsafeNclNativeMethods.GetCurrentThreadId();
                    Thread.SetData(GlobalLog.s_ThreadIdSlot, threadId);
                }
            }
            if (threadId == 0) {
                threadId = (uint)Thread.CurrentThread.GetHashCode();
            }

            string str = "[" + threadId.ToString("x8") + "]"  + " (" +tickString+  ") " + spc + msg;

            lock(this) {
                _AddCount++;
                _Logarray.Add(str);
                int MaxLines = GlobalLog.s_DumpToConsole ? 0 : GlobalLog.MaxLinesBeforeSave;
                if (_AddCount > MaxLines) {
                    _AddCount = 0;
                    DumpArray(false);
                    _Logarray = new ArrayList();
                }
            }
        }

        internal override void DumpArray(bool shouldClose) {
            if ( GlobalLog.s_DumpToConsole ) {
                DumpArrayToConsole();
            } else {
                DumpArrayToFile(shouldClose);
            }
        }

        internal unsafe override void Dump(byte[] buffer, int offset, int length) {
            //if (!GlobalLog.s_DumpWebData) {
            //    return;
            //}
            if (buffer==null) {
                PrintLine("(null)");
                return;
            }
            if (offset > buffer.Length) {
                PrintLine("(offset out of range)");
                return;
            }
            if (length > GlobalLog.s_MaxDumpSize) {
                PrintLine("(printing " + GlobalLog.s_MaxDumpSize.ToString() + " out of " + length.ToString() + ")");
                length = GlobalLog.s_MaxDumpSize;
            }
            if ((length < 0) || (length > buffer.Length - offset)) {
                length = buffer.Length - offset;
            }
            fixed (byte* pBuffer = buffer) {
                Dump((IntPtr)pBuffer, offset, length);
            }
        }

        internal unsafe override void Dump(IntPtr pBuffer, int offset, int length) {
            //if (!GlobalLog.s_DumpWebData) {
            //    return;
            //}
            if (pBuffer==IntPtr.Zero || length<0) {
                PrintLine("(null)");
                return;
            }
            if (length > GlobalLog.s_MaxDumpSize) {
                PrintLine("(printing " + GlobalLog.s_MaxDumpSize.ToString() + " out of " + length.ToString() + ")");
                length = GlobalLog.s_MaxDumpSize;
            }
            byte* buffer = (byte*)pBuffer + offset;
            Dump(buffer, length);
        }

        unsafe void Dump(byte* buffer, int length) {
            do {
                int offset = 0;
                int n = Math.Min(length, 16);
                string disp = ((IntPtr)buffer).ToString("X8") + " : " + offset.ToString("X8") + " : ";
                byte current;
                for (int i = 0; i < n; ++i) {
                    current = *(buffer + i);
                    disp += current.ToString("X2") + ((i == 7) ? '-' : ' ');
                }
                for (int i = n; i < 16; ++i) {
                    disp += "   ";
                }
                disp += ": ";
                for (int i = 0; i < n; ++i) {
                    current = *(buffer + i);
                    disp += ((current < 0x20) || (current > 0x7e)) ? '.' : (char)current;
                }
                PrintLine(disp);
                offset += n;
                buffer += n;
                length -= n;
            } while (length > 0);
        }

        // SECURITY: This is dev-debugging class and we need some permissions
        // to use it under trust-restricted environment as well.
        [PermissionSet(SecurityAction.Assert, Name="FullTrust")]
        internal override void DumpArrayToFile(bool shouldClose) {
            lock (this) {
                if (!shouldClose) {
                    if (_Stream==null) {
                        string mainLogFileRoot = GlobalLog.s_RootDirectory + "System.Net";
                        string mainLogFile = mainLogFileRoot;
                        for (int k=0; k<20; k++) {
                            if (k>0) {
                                mainLogFile = mainLogFileRoot + "." + k.ToString();
                            }
                            string fileName = mainLogFile + ".log";
                            if (!File.Exists(fileName)) {
                                try {
                                    _Stream = new StreamWriter(fileName);
                                    break;
                                }
                                catch (Exception exception) {
                                    if (exception is ThreadAbortException || exception is StackOverflowException || exception is OutOfMemoryException) {
                                        throw;
                                    }
                                    if (exception is SecurityException || exception is UnauthorizedAccessException) {
                                        // can't be CAS (we assert) this is an ACL issue
                                        break;
                                    }
                                }
                            }
                        }
                        if (_Stream==null) {
                            _Stream = StreamWriter.Null;
                        }
                        // write a header with information about the Process and the AppDomain
                        _Stream.Write("# MachineName: " + Environment.MachineName + "\r\n");
                        _Stream.Write("# ProcessName: " + Process.GetCurrentProcess().ProcessName + " (pid: " + Process.GetCurrentProcess().Id + ")\r\n");
                        _Stream.Write("# AppDomainId: " + AppDomain.CurrentDomain.Id + "\r\n");
                        _Stream.Write("# CurrentIdentity: " + WindowsIdentity.GetCurrent().Name + "\r\n");
                        _Stream.Write("# CommandLine: " + Environment.CommandLine + "\r\n");
                        _Stream.Write("# ClrVersion: " + Environment.Version + "\r\n");
                        _Stream.Write("# CreationDate: " + DateTime.Now.ToString("g") + "\r\n");
                    }
                }
                try {
                    if (_Logarray!=null) {
                        for (int i=0; i<_Logarray.Count; i++) {
                            _Stream.Write((string)_Logarray[i]);
                            _Stream.Write("\r\n");
                        }

                        if (_Logarray.Count > 0 && _Stream != null)
                            _Stream.Flush();
                    }
                }
                catch (Exception exception) {
                    if (exception is ThreadAbortException || exception is StackOverflowException || exception is OutOfMemoryException) {
                        throw;
                    }
                }
                if (shouldClose && _Stream!=null) {
                    try {
                        _Stream.Close();
                    }
                    catch (ObjectDisposedException) { }
                    _Stream = null;
                }
            }
        }

        internal override void Flush() {
            Flush(false);
        }

        internal override void Flush(bool close) {
            lock (this) {
                if (!GlobalLog.s_DumpToConsole) {
                    DumpArrayToFile(close);
                    _AddCount = 0;
                }
            }
        }

        private class ThreadInfoData {
            public ThreadInfoData(string indent) {
                Indent = indent;
                NestingStack = new Stack();
            }
            public string Indent;
            public Stack NestingStack;
        };

        string IndentString {
            get {
                string indent = " ";
                Object obj = _ThreadNesting[Thread.CurrentThread.GetHashCode()];
                if (!GlobalLog.s_DebugCallNesting) {
                    if (obj == null) {
                        _ThreadNesting[Thread.CurrentThread.GetHashCode()] = indent;
                    } else {
                        indent = (String) obj;
                    }
                } else {
                    ThreadInfoData threadInfo = obj as ThreadInfoData;
                    if (threadInfo == null) {
                        threadInfo = new ThreadInfoData(indent);
                        _ThreadNesting[Thread.CurrentThread.GetHashCode()] = threadInfo;
                    }
                    indent = threadInfo.Indent;
                }
                return indent;
            }
            set {
                Object obj = _ThreadNesting[Thread.CurrentThread.GetHashCode()];
                if (obj == null) {
                    return;
                }
                if (!GlobalLog.s_DebugCallNesting) {
                    _ThreadNesting[Thread.CurrentThread.GetHashCode()] = value;
                } else {
                    ThreadInfoData threadInfo = obj as ThreadInfoData;
                    if (threadInfo == null) {
                        threadInfo = new ThreadInfoData(value);
                        _ThreadNesting[Thread.CurrentThread.GetHashCode()] = threadInfo;
                    }
                    threadInfo.Indent = value;
                }
            }
        }

        [System.Diagnostics.Conditional("TRAVE")]
        private void IncNestingCount() {
            IndentString = IndentString + " ";
        }

        [System.Diagnostics.Conditional("TRAVE")]
        private void DecNestingCount() {
            string indent = IndentString;
            if (indent.Length>1) {
                try {
                    indent = indent.Substring(1);
                }
                catch {
                    indent = string.Empty;
                }
            }
            if (indent.Length==0) {
                indent = "< ";
            }
            IndentString = indent;
        }

        private string GetNestingString() {
            return IndentString;
        }

        [System.Diagnostics.Conditional("TRAVE")]
        private void ValidatePush(string name) {
            if (GlobalLog.s_DebugCallNesting) {
                Object obj = _ThreadNesting[Thread.CurrentThread.GetHashCode()];
                ThreadInfoData threadInfo = obj as ThreadInfoData;
                if (threadInfo == null) {
                    return;
                }
                threadInfo.NestingStack.Push(name);
            }
        }

        [System.Diagnostics.Conditional("TRAVE")]
        private void ValidatePop(string name) {
            if (GlobalLog.s_DebugCallNesting) {
                try {
                    Object obj = _ThreadNesting[Thread.CurrentThread.GetHashCode()];
                    ThreadInfoData threadInfo = obj as ThreadInfoData;
                    if (threadInfo == null) {
                        return;
                    }
                    if (threadInfo.NestingStack.Count == 0) {
                        PrintLine("++++====" + "Poped Empty Stack for :"+name);
                    }
                    string popedName = (string) threadInfo.NestingStack.Pop();
                    string [] parsedList = popedName.Split(new char [] {'(',')',' ','.',':',',','#'});
                    foreach (string element in parsedList) {
                        if (element != null && element.Length > 1 && name.IndexOf(element) != -1) {
                            return;
                        }
                    }
                    PrintLine("++++====" + "Expected:" + popedName + ": got :" + name + ": StackSize:"+threadInfo.NestingStack.Count);
                    // relevel the stack
                    while(threadInfo.NestingStack.Count>0) {
                        string popedName2 = (string) threadInfo.NestingStack.Pop();
                        string [] parsedList2 = popedName2.Split(new char [] {'(',')',' ','.',':',',','#'});
                        foreach (string element2 in parsedList2) {
                            if (element2 != null && element2.Length > 1 && name.IndexOf(element2) != -1) {
                                return;
                            }
                        }
                    }
                }
                catch {
                    PrintLine("++++====" + "ValidatePop failed for: "+name);
                }
            }
        }


        ~LoggingObject() {
            if(!_Finalized) {
                _Finalized = true;
                lock(this) {
                    DumpArray(true);
                }
            }
        }


    } // class LoggingObject

    internal static class TraveHelper {
        private static readonly string Hexizer = "0x{0:x}";
        internal static string ToHex(object value) {
            return String.Format(Hexizer, value);
        }
    }
#endif // TRAVE

#if TRAVE 
    internal class IntegerSwitch : BooleanSwitch {
        public IntegerSwitch(string switchName, string switchDescription) : base(switchName, switchDescription) {
        }
        public new int Value {
            get {
                return base.SwitchSetting;
            }
        }
    }

#endif

    [Flags]
    internal enum ThreadKinds
    {
        Unknown        = 0x0000,

        // Mutually exclusive.
        User           = 0x0001,     // Thread has entered via an API.
        System         = 0x0002,     // Thread has entered via a system callback (e.g. completion port) or is our own thread.

        // Mutually exclusive.
        Sync           = 0x0004,     // Thread should block.
        Async          = 0x0008,     // Thread should not block.

        // Mutually exclusive, not always known for a user thread.  Never changes.
        Timer          = 0x0010,     // Thread is the timer thread.  (Can't call user code.)
        CompletionPort = 0x0020,     // Thread is a ThreadPool completion-port thread.
        Worker         = 0x0040,     // Thread is a ThreadPool worker thread.
        Finalization   = 0x0080,     // Thread is the finalization thread.
        Other          = 0x0100,     // Unknown source.

        OwnerMask      = User | System,
        SyncMask       = Sync | Async,
        SourceMask     = Timer | CompletionPort | Worker | Finalization | Other,

        // Useful "macros"
        SafeSources    = SourceMask & ~(Timer | Finalization),  // Methods that "unsafe" sources can call must be explicitly marked.
        ThreadPool     = CompletionPort | Worker,               // Like Thread.CurrentThread.IsThreadPoolThread
    }

    /// <internalonly/>
    /// <devdoc>
    /// </devdoc>
    internal static class GlobalLog {

        //
        // Logging Initalization - I need to disable Logging code, and limit
        //  the effect it has when it is dissabled, so I use a bool here.
        //
        //  This can only be set when the logging code is built and enabled.
        //  By specifing the "CSC_DEFINES=/D:TRAVE" in the build environment,
        //  this code will be built and then checks against an enviroment variable
        //  and a BooleanSwitch to see if any of the two have enabled logging.
        //

        private static BaseLoggingObject Logobject = GlobalLog.LoggingInitialize();
#if TRAVE
        internal static LocalDataStoreSlot s_ThreadIdSlot;
        internal static bool s_UseThreadId;
        internal static bool s_UseTimeSpan;
        internal static bool s_DumpWebData;
        internal static bool s_UsePerfCounter;
        internal static bool s_DebugCallNesting;
        internal static bool s_DumpToConsole;
        internal static int s_MaxDumpSize;
        internal static string s_RootDirectory;

        //
        // Logging Config Variables -  below are list of consts that can be used to config
        //  the logging,
        //

        // Max number of lines written into a buffer, before a save is invoked
        // s_DumpToConsole disables.
        public const int MaxLinesBeforeSave = 0;

#endif
        [ReliabilityContract(Consistency.MayCorruptAppDomain, Cer.None)]
        private static BaseLoggingObject LoggingInitialize() {

#if DEBUG
            if (GetSwitchValue("SystemNetLogging", "System.Net logging module", false) &&
                GetSwitchValue("SystemNetLog_ConnectionMonitor", "System.Net connection monitor thread", false)) {
                InitConnectionMonitor();
            }
#endif // DEBUG
#if TRAVE
            // by default we'll log to c:\temp\ so that non interactive services (like w3wp.exe) that don't have environment
            // variables can easily be debugged, note that the ACLs of the directory might need to be adjusted
            if (!GetSwitchValue("SystemNetLog_OverrideDefaults", "System.Net log override default settings", false)) {
                s_ThreadIdSlot = Thread.AllocateDataSlot();
                s_UseThreadId = true;
                s_UseTimeSpan = true;
                s_DumpWebData = true;
                s_MaxDumpSize = 256;
                s_UsePerfCounter = true;
                s_DebugCallNesting = true;
                s_DumpToConsole = false;
                s_RootDirectory = "C:\\Temp\\";
                return new LoggingObject();
            }
            if (GetSwitchValue("SystemNetLogging", "System.Net logging module", false)) {
                s_ThreadIdSlot = Thread.AllocateDataSlot();
                s_UseThreadId = GetSwitchValue("SystemNetLog_UseThreadId", "System.Net log display system thread id", false);
                s_UseTimeSpan = GetSwitchValue("SystemNetLog_UseTimeSpan", "System.Net log display ticks as TimeSpan", false);
                s_DumpWebData = GetSwitchValue("SystemNetLog_DumpWebData", "System.Net log display HTTP send/receive data", false);
                s_MaxDumpSize = GetSwitchValue("SystemNetLog_MaxDumpSize", "System.Net log max size of display data", 256);
                s_UsePerfCounter = GetSwitchValue("SystemNetLog_UsePerfCounter", "System.Net log use QueryPerformanceCounter() to get ticks ", false);
                s_DebugCallNesting = GetSwitchValue("SystemNetLog_DebugCallNesting", "System.Net used to debug call nesting", false);
                s_DumpToConsole = GetSwitchValue("SystemNetLog_DumpToConsole", "System.Net log to console", false);
                s_RootDirectory = GetSwitchValue("SystemNetLog_RootDirectory", "System.Net root directory of log file", string.Empty);
                return new LoggingObject();
            }
#endif // TRAVE
            return new BaseLoggingObject();
        }


#if TRAVE 
        private static string GetSwitchValue(string switchName, string switchDescription, string defaultValue) {
            new EnvironmentPermission(PermissionState.Unrestricted).Assert();
            try {
                defaultValue = Environment.GetEnvironmentVariable(switchName);
            }
            finally {
                EnvironmentPermission.RevertAssert();
            }
            return defaultValue;
        }

        private static int GetSwitchValue(string switchName, string switchDescription, int defaultValue) {
            IntegerSwitch theSwitch = new IntegerSwitch(switchName, switchDescription);
            if (theSwitch.Enabled) {
                return theSwitch.Value;
            }
            new EnvironmentPermission(PermissionState.Unrestricted).Assert();
            try {
                string environmentVar = Environment.GetEnvironmentVariable(switchName);
                if (environmentVar!=null) {
                    defaultValue = Int32.Parse(environmentVar.Trim(), CultureInfo.InvariantCulture);
                }
            }
            finally {
                EnvironmentPermission.RevertAssert();
            }
            return defaultValue;
        }

#endif

#if TRAVE || DEBUG
        private static bool GetSwitchValue(string switchName, string switchDescription, bool defaultValue) {
            BooleanSwitch theSwitch = new BooleanSwitch(switchName, switchDescription);
            new EnvironmentPermission(PermissionState.Unrestricted).Assert();
            try {
                if (theSwitch.Enabled) {
                    return true;
                }
                string environmentVar = Environment.GetEnvironmentVariable(switchName);
                defaultValue = environmentVar!=null && environmentVar.Trim()=="1";
            }
            catch (ConfigurationException) { }
            finally {
                EnvironmentPermission.RevertAssert();
            }
            return defaultValue;
        }
#endif // TRAVE || DEBUG


        // Enables thread tracing, detects mis-use of threads.
#if DEBUG
        [ThreadStatic]
        private static Stack<ThreadKinds> t_ThreadKindStack;

        private static Stack<ThreadKinds> ThreadKindStack
        {
            get
            {
                if (t_ThreadKindStack == null)
                {
                    t_ThreadKindStack = new Stack<ThreadKinds>();
                }
                return t_ThreadKindStack;
            }
        }
#endif
                
        internal static ThreadKinds CurrentThreadKind
        {
            get
            {
#if DEBUG
                return ThreadKindStack.Count > 0 ? ThreadKindStack.Peek() : ThreadKinds.Other;
#else
                return ThreadKinds.Unknown;
#endif
            }
        }

#if DEBUG
        // ifdef'd instead of conditional since people are forced to handle the return value.
        // [Conditional("DEBUG")]
        [ReliabilityContract(Consistency.MayCorruptAppDomain, Cer.None)]
        internal static IDisposable SetThreadKind(ThreadKinds kind)
        {
            if ((kind & ThreadKinds.SourceMask) != ThreadKinds.Unknown)
            {
                throw new InternalException();
            }

            // Ignore during shutdown.
            if (NclUtilities.HasShutdownStarted)
            {
                return null;
            }

            ThreadKinds threadKind = CurrentThreadKind;
            ThreadKinds source = threadKind & ThreadKinds.SourceMask;

#if TRAVE
            // Special warnings when doing dangerous things on a thread.
            if ((threadKind & ThreadKinds.User) != 0 && (kind & ThreadKinds.System) != 0)
            {
                Print("WARNING: Thread changed from User to System; user's thread shouldn't be hijacked.");
            }

            if ((threadKind & ThreadKinds.Async) != 0 && (kind & ThreadKinds.Sync) != 0)
            {
                Print("WARNING: Thread changed from Async to Sync, may block an Async thread.");
            }
            else if ((threadKind & (ThreadKinds.Other | ThreadKinds.CompletionPort)) == 0 && (kind & ThreadKinds.Sync) != 0)
            {
                Print("WARNING: Thread from a limited resource changed to Sync, may deadlock or bottleneck.");
            }
#endif

            ThreadKindStack.Push(
                (((kind & ThreadKinds.OwnerMask) == 0 ? threadKind : kind) & ThreadKinds.OwnerMask) |
                (((kind & ThreadKinds.SyncMask) == 0 ? threadKind : kind) & ThreadKinds.SyncMask) |
                (kind & ~(ThreadKinds.OwnerMask | ThreadKinds.SyncMask)) |
                source);

#if TRAVE
            if (CurrentThreadKind != threadKind)
            {
                Print("Thread becomes:(" + CurrentThreadKind.ToString() + ")");
            }
#endif

            return new ThreadKindFrame();
        }

        private class ThreadKindFrame : IDisposable
        {
            private int m_FrameNumber;

            internal ThreadKindFrame()
            {
                m_FrameNumber = ThreadKindStack.Count;
            }

            void IDisposable.Dispose()
            {
                // Ignore during shutdown.
                if (NclUtilities.HasShutdownStarted)
                {
                    return;
                }

                if (m_FrameNumber != ThreadKindStack.Count)
                {
                    throw new InternalException();
                }

                ThreadKinds previous = ThreadKindStack.Pop();

#if TRAVE
                if (CurrentThreadKind != previous)
                {
                    Print("Thread reverts:(" + CurrentThreadKind.ToString() + ")");
                }
#endif
            }
        }
#endif

        [Conditional("DEBUG")]
        [ReliabilityContract(Consistency.MayCorruptAppDomain, Cer.None)]
        internal static void SetThreadSource(ThreadKinds source)
        {
#if DEBUG
            if ((source & ThreadKinds.SourceMask) != source || source == ThreadKinds.Unknown)
            {
                throw new ArgumentException("Must specify the thread source.", "source");
            }

            if (ThreadKindStack.Count == 0)
            {
                ThreadKindStack.Push(source);
                return;
            }

            if (ThreadKindStack.Count > 1)
            {
                Print("WARNING: SetThreadSource must be called at the base of the stack, or the stack has been corrupted.");
                while (ThreadKindStack.Count > 1)
                {
                    ThreadKindStack.Pop();
                }
            }

            if (ThreadKindStack.Peek() != source)
            {
                // SQL can fail to clean up the stack, leaving the default Other at the bottom.  Replace it.
                Print("WARNING: The stack has been corrupted.");
                ThreadKinds last = ThreadKindStack.Pop() & ThreadKinds.SourceMask;
                Assert(last == source || last == ThreadKinds.Other, "Thread source changed.|Was:({0}) Now:({1})", last, source);
                ThreadKindStack.Push(source);
            }
#endif
        }

        [Conditional("DEBUG")]
        [ReliabilityContract(Consistency.MayCorruptAppDomain, Cer.None)]
        internal static void ThreadContract(ThreadKinds kind, string errorMsg)
        {
            ThreadContract(kind, ThreadKinds.SafeSources, errorMsg);
        }

        [Conditional("DEBUG")]
        [ReliabilityContract(Consistency.MayCorruptAppDomain, Cer.None)]
        internal static void ThreadContract(ThreadKinds kind, ThreadKinds allowedSources, string errorMsg)
        {
            if ((kind & ThreadKinds.SourceMask) != ThreadKinds.Unknown || (allowedSources & ThreadKinds.SourceMask) != allowedSources)
            {
                throw new InternalException();
            }

            ThreadKinds threadKind = CurrentThreadKind;
            Assert((threadKind & allowedSources) != 0, errorMsg, "Thread Contract Violation.|Expected source:({0}) Actual source:({1})", allowedSources , threadKind & ThreadKinds.SourceMask);
            Assert((threadKind & kind) == kind, errorMsg, "Thread Contract Violation.|Expected kind:({0}) Actual kind:({1})", kind, threadKind & ~ThreadKinds.SourceMask);
        }

#if DEBUG
        // Enables auto-hang detection, which will "snap" a log on hang
        internal static bool EnableMonitorThread = false;

        // Default value for hang timer
#if FEATURE_PAL // ROTORTODO - after speedups (like real JIT and GC) remove this
        public const int DefaultTickValue = 1000*60*5; // 5 minutes
#else
        public const int DefaultTickValue = 1000*60; // 60 secs
#endif // FEATURE_PAL
#endif // DEBUG

        [System.Diagnostics.Conditional("TRAVE")]
        public static void AddToArray(string msg) {
#if TRAVE
            GlobalLog.Logobject.PrintLine(msg);
#endif
        }

        [System.Diagnostics.Conditional("TRAVE")]
        public static void Ignore(object msg) {
        }

        [System.Diagnostics.Conditional("TRAVE")]
        [ReliabilityContract(Consistency.MayCorruptAppDomain, Cer.None)]
        public static void Print(string msg) {
#if TRAVE
            GlobalLog.Logobject.PrintLine(msg);
#endif
        }

        [System.Diagnostics.Conditional("TRAVE")]
        public static void PrintHex(string msg, object value) {
#if TRAVE
            GlobalLog.Logobject.PrintLine(msg+TraveHelper.ToHex(value));
#endif
        }

        [System.Diagnostics.Conditional("TRAVE")]
        public static void Enter(string func) {
#if TRAVE
            GlobalLog.Logobject.EnterFunc(func + "(*none*)");
#endif
        }

        [System.Diagnostics.Conditional("TRAVE")]
        public static void Enter(string func, string parms) {
#if TRAVE
            GlobalLog.Logobject.EnterFunc(func + "(" + parms + ")");
#endif
        }

        [Conditional("DEBUG")]
        [Conditional("_FORCE_ASSERTS")]
        [ReliabilityContract(Consistency.MayCorruptAppDomain, Cer.None)]
        public static void Assert(bool condition, string messageFormat, params object[] data)
        {
            if (!condition)
            {
                string fullMessage = string.Format(CultureInfo.InvariantCulture, messageFormat, data);
                int pipeIndex = fullMessage.IndexOf('|');
                if (pipeIndex == -1)
                {
                    Assert(fullMessage);
                }
                else
                {
                    int detailLength = fullMessage.Length - pipeIndex - 1;
                    Assert(fullMessage.Substring(0, pipeIndex), detailLength > 0 ? fullMessage.Substring(pipeIndex + 1, detailLength) : null);
                }
            }
        }

        [Conditional("DEBUG")]
        [Conditional("_FORCE_ASSERTS")]
        [ReliabilityContract(Consistency.MayCorruptAppDomain, Cer.None)]
        public static void Assert(string message)
        {
            Assert(message, null);
        }

        [Conditional("DEBUG")]
        [Conditional("_FORCE_ASSERTS")]
        [ReliabilityContract(Consistency.MayCorruptAppDomain, Cer.None)]
        public static void Assert(string message, string detailMessage)
        {
            try
            {
                Print("Assert: " + message + (!string.IsNullOrEmpty(detailMessage) ? ": " + detailMessage : ""));
                Print("*******");
                Logobject.DumpArray(false);
            }
            finally
            {
#if DEBUG && !STRESS
                Debug.Assert(false, message, detailMessage);
#else
                UnsafeNclNativeMethods.DebugBreak();
                Debugger.Break();
#endif
            }
        }


        [System.Diagnostics.Conditional("TRAVE")]
        public static void LeaveException(string func, Exception exception) {
#if TRAVE
            GlobalLog.Logobject.LeaveFunc(func + " exception " + ((exception!=null) ? exception.Message : String.Empty));
#endif
        }

        [System.Diagnostics.Conditional("TRAVE")]
        public static void Leave(string func) {
#if TRAVE
            GlobalLog.Logobject.LeaveFunc(func + " returns ");
#endif
        }

        [System.Diagnostics.Conditional("TRAVE")]
        public static void Leave(string func, string result) {
#if TRAVE
            GlobalLog.Logobject.LeaveFunc(func + " returns " + result);
#endif
        }

        [System.Diagnostics.Conditional("TRAVE")]
        public static void Leave(string func, int returnval) {
#if TRAVE
            GlobalLog.Logobject.LeaveFunc(func + " returns " + returnval.ToString());
#endif
        }

        [System.Diagnostics.Conditional("TRAVE")]
        public static void Leave(string func, bool returnval) {
#if TRAVE
            GlobalLog.Logobject.LeaveFunc(func + " returns " + returnval.ToString());
#endif
        }

        [System.Diagnostics.Conditional("TRAVE")]
        public static void DumpArray() {
#if TRAVE
            GlobalLog.Logobject.DumpArray(true);
#endif
        }

        [System.Diagnostics.Conditional("TRAVE")]
        public static void Dump(byte[] buffer) {
#if TRAVE
            Logobject.Dump(buffer, 0, buffer!=null ? buffer.Length : -1);
#endif
        }

        [System.Diagnostics.Conditional("TRAVE")]
        public static void Dump(byte[] buffer, int length) {
#if TRAVE
            Logobject.Dump(buffer, 0, length);
#endif
        }

        [System.Diagnostics.Conditional("TRAVE")]
        public static void Dump(byte[] buffer, int offset, int length) {
#if TRAVE
            Logobject.Dump(buffer, offset, length);
#endif
        }

        [System.Diagnostics.Conditional("TRAVE")]
        public static void Dump(IntPtr buffer, int offset, int length) {
#if TRAVE
            Logobject.Dump(buffer, offset, length);
#endif
        }

#if DEBUG
        private class HttpWebRequestComparer : IComparer {
            public int Compare(
                   object x1,
                   object y1
                   ) {

                HttpWebRequest x = (HttpWebRequest) x1;
                HttpWebRequest y = (HttpWebRequest) y1;

                if (x.GetHashCode() == y.GetHashCode()) {
                    return 0;
                } else if (x.GetHashCode() < y.GetHashCode()) {
                    return -1;
                } else if (x.GetHashCode() > y.GetHashCode()) {
                    return 1;
                }

                return 0;
            }
        }

        private class ConnectionMonitorEntry {
            public HttpWebRequest m_Request;
            public int m_Flags;
            public DateTime m_TimeAdded;
            public Connection m_Connection;

            public ConnectionMonitorEntry(HttpWebRequest request, Connection connection, int flags) {
                m_Request = request;
                m_Connection = connection;
                m_Flags = flags;
                m_TimeAdded = DateTime.Now;
            }
        }

        private static volatile ManualResetEvent s_ShutdownEvent;
        private static volatile SortedList s_RequestList;

        internal const int WaitingForReadDoneFlag = 0x1;
#endif

#if DEBUG
        private static void ConnectionMonitor() {
            while(! s_ShutdownEvent.WaitOne(DefaultTickValue, false)) {
                if (GlobalLog.EnableMonitorThread) {
#if TRAVE
                    GlobalLog.Logobject.LoggingMonitorTick();
#endif
                }

                int hungCount = 0;
                lock (s_RequestList) {
                    DateTime dateNow = DateTime.Now;
                    DateTime dateExpired = dateNow.AddSeconds(-DefaultTickValue);
                    foreach (ConnectionMonitorEntry monitorEntry in s_RequestList.GetValueList() ) {
                        if (monitorEntry != null &&
                            (dateExpired > monitorEntry.m_TimeAdded))
                        {
                            hungCount++;
#if TRAVE
                            GlobalLog.Print("delay:" + (dateNow - monitorEntry.m_TimeAdded).TotalSeconds +
                                " req#" + monitorEntry.m_Request.GetHashCode() +
                                " cnt#" + monitorEntry.m_Connection.GetHashCode() +
                                " flags:" + monitorEntry.m_Flags);

#endif
                            monitorEntry.m_Connection.DebugMembers(monitorEntry.m_Request.GetHashCode());
                        }
                    }
                }
                Assert(hungCount == 0, "Warning: Hang Detected on Connection(s) of greater than {0} ms.  {1} request(s) hung.|Please Dump System.Net.GlobalLog.s_RequestList for pending requests, make sure your streams are calling Close(), and that your destination server is up.", DefaultTickValue, hungCount);
            }
        }
#endif // DEBUG

#if DEBUG
        [ReliabilityContract(Consistency.MayCorruptAppDomain, Cer.None)]
        internal static void AppDomainUnloadEvent(object sender, EventArgs e) {
            s_ShutdownEvent.Set();
        }
#endif

#if DEBUG
        [System.Diagnostics.Conditional("DEBUG")]
        private static void InitConnectionMonitor() {
            s_RequestList = new SortedList(new HttpWebRequestComparer(), 10);
            s_ShutdownEvent = new ManualResetEvent(false);
            AppDomain.CurrentDomain.DomainUnload += new EventHandler(AppDomainUnloadEvent);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(AppDomainUnloadEvent);
            Thread threadMonitor = new Thread(new ThreadStart(ConnectionMonitor));
            threadMonitor.IsBackground = true;
            threadMonitor.Start();
        }
#endif

        [System.Diagnostics.Conditional("DEBUG")]
        internal static void DebugAddRequest(HttpWebRequest request, Connection connection, int flags) {
#if DEBUG
            // null if the connection monitor is off
            if(s_RequestList == null)
                return;

            lock(s_RequestList) {
                Assert(!s_RequestList.ContainsKey(request), "s_RequestList.ContainsKey(request)|A HttpWebRequest should not be submitted twice.");

                ConnectionMonitorEntry requestEntry =
                    new ConnectionMonitorEntry(request, connection, flags);

                try {
                    s_RequestList.Add(request, requestEntry);
                } catch {
                }
            }
#endif
        }

        [System.Diagnostics.Conditional("DEBUG")]
        internal static void DebugRemoveRequest(HttpWebRequest request) {
#if DEBUG
            // null if the connection monitor is off
            if(s_RequestList == null)
                return;

            lock(s_RequestList) {
                Assert(s_RequestList.ContainsKey(request), "!s_RequestList.ContainsKey(request)|A HttpWebRequest should not be removed twice.");

                try {
                    s_RequestList.Remove(request);
                } catch {
                }
            }
#endif
        }

        [System.Diagnostics.Conditional("DEBUG")]
        internal static void DebugUpdateRequest(HttpWebRequest request, Connection connection, int flags) {
#if DEBUG
            // null if the connection monitor is off
            if(s_RequestList == null)
                return;

            lock(s_RequestList) {
                if(!s_RequestList.ContainsKey(request)) {
                    return;
                }

                ConnectionMonitorEntry requestEntry =
                    new ConnectionMonitorEntry(request, connection, flags);

                try {
                    s_RequestList.Remove(request);
                    s_RequestList.Add(request, requestEntry);
                } catch {
                }
            }
#endif
        }
    } // class GlobalLog
} // namespace System.Net
