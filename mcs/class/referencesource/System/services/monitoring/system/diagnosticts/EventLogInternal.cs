//------------------------------------------------------------------------------
// <copyright file="EventLogInternal.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

//#define RETRY_ON_ALL_ERRORS

/*
 * EventLogInternal contains most of the logic for interacting with the Windows Event Log.
 * The reason for this class existing (instead of the logic being in EventLog itself) is 
 * that we'd like to be able to have the invariant that the Source, MachineName and Log Name
 * don't change across the lifetime of an event log object, but we exposed public setters for
 * these properties.  EventLog holds a reference to an EventLogInternal instance, plumbs all
 * calls to it and replaces it when any of these properites change.
 * 
 * Note that EventLogInternal also holds a reference back to the EventLog instnace that is 
 * exposing it so it is not prematurely collected.
 */

namespace System.Diagnostics {
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.ComponentModel;
    using System.Diagnostics;
    using System;
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System.IO;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.ComponentModel.Design;
    using System.Security;
    using System.Security.Permissions;
    using System.Reflection;
    using System.Runtime.Versioning;
    using System.Runtime.CompilerServices;
    using System.Diagnostics.CodeAnalysis;

    /// <devdoc>
    ///    <para>
    ///       Provides interaction with Windows 2000 event logs.
    ///    </para>
    /// </devdoc>
    internal class EventLogInternal : IDisposable, ISupportInitialize {
        // a collection over all our entries. Since the class holds no state, we
        // can just hand the same instance out every time.
        private EventLogEntryCollection entriesCollection;
        // the name of the log we're reading from or writing to
        internal string logName;
        // used in monitoring for event postings.
        private int lastSeenCount;
        // holds the machine we're on, or null if it's the local machine
        internal readonly string machineName;

        // the delegate to call when an event arrives
        internal EntryWrittenEventHandler onEntryWrittenHandler;
        // holds onto the handle for reading
        private SafeEventLogReadHandle  readHandle;
        // the source name - used only when writing
        internal readonly string sourceName;
        // holds onto the handle for writing
        private SafeEventLogWriteHandle writeHandle;

        private string logDisplayName;

        // cache system state variables
        // the initial size of the buffer (it can be made larger if necessary)
        private const int BUF_SIZE = 40000;
        // the number of bytes in the cache that belong to entries (not necessarily
        // the same as BUF_SIZE, because the cache only holds whole entries)
        private int bytesCached;
        // the actual cache buffer
        private byte[] cache;
        // the number of the entry at the beginning of the cache
        private int firstCachedEntry = -1;
        // the number of the entry that we got out of the cache most recently
        private int lastSeenEntry;
        // where that entry was
        private int lastSeenPos;
        //support for threadpool based deferred execution
        private ISynchronizeInvoke synchronizingObject;
        // the EventLog object that publicly exposes this instance.
        private readonly EventLog parent;

        private const string EventLogKey = "SYSTEM\\CurrentControlSet\\Services\\EventLog";
        internal const string DllName = "EventLogMessages.dll";
        private const string eventLogMutexName = "netfxeventlog.1.0";
        private const int SecondsPerDay = 60 * 60 * 24;
        private const int DefaultMaxSize = 512*1024;
        private const int DefaultRetention = 7*SecondsPerDay;
        
        private const int Flag_notifying     = 0x1;           // keeps track of whether we're notifying our listeners - to prevent double notifications
        private const int Flag_forwards      = 0x2;     // whether the cache contains entries in forwards order (true) or backwards (false)
        private const int Flag_initializing  = 0x4;
        internal const int Flag_monitoring    = 0x8;
        private const int Flag_registeredAsListener  = 0x10;
        private const int Flag_writeGranted     = 0x20;
        private const int Flag_disposed      = 0x100;
        private const int Flag_sourceVerified= 0x200;
        
        private BitVector32 boolFlags = new BitVector32();
        
        private Hashtable messageLibraries;
        private readonly static Hashtable listenerInfos = new Hashtable(StringComparer.OrdinalIgnoreCase);

        private Object m_InstanceLockObject;
        private Object InstanceLockObject {
            get {
                if (m_InstanceLockObject == null) {
                    Object o = new Object();
                    Interlocked.CompareExchange(ref m_InstanceLockObject, o, null);
                }
                return m_InstanceLockObject;
            }
        }


        private static Object s_InternalSyncObject;
        private static Object InternalSyncObject {
            get {
                if (s_InternalSyncObject == null) {
                    Object o = new Object();
                    Interlocked.CompareExchange(ref s_InternalSyncObject, o, null);
                }
                return s_InternalSyncObject;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Diagnostics.EventLog'/>
        ///       class.
        ///    </para>
        /// </devdoc>
        public EventLogInternal() : this("", ".", "", null) {
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public EventLogInternal(string logName) : this(logName, ".", "", null) {
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public EventLogInternal(string logName, string machineName) : this(logName, machineName, "", null) {
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public EventLogInternal(string logName, string machineName, string source) : this(logName, machineName, source, null) {
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity", Justification = "Microsoft: Safe, oldLog.machineName doesn't change")]
        public EventLogInternal(string logName, string machineName, string source, EventLog parent) {
            //look out for invalid log names
            if (logName == null)
                throw new ArgumentNullException("logName");
            if (!ValidLogName(logName, true))
                throw new ArgumentException(SR.GetString(SR.BadLogName));

            if (!SyntaxCheck.CheckMachineName(machineName))
                throw new ArgumentException(SR.GetString(SR.InvalidParameter, "machineName", machineName));

            EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Write, machineName);
            permission.Demand();
            
            this.machineName = machineName;

            this.logName = logName;
            this.sourceName = source;
            readHandle = null;
            writeHandle = null;
            boolFlags[Flag_forwards] = true;
            this.parent = parent;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the contents of the event log.
        ///    </para>
        /// </devdoc>
        public EventLogEntryCollection Entries {
            get {
                string currentMachineName = this.machineName;

                EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Administer, currentMachineName);
                permission.Demand();

                if (entriesCollection == null)
                    entriesCollection = new EventLogEntryCollection(this);
                return entriesCollection;
            }
        }

        /// <devdoc>
        ///     Gets the number of entries in the log
        /// </devdoc>
        internal int EntryCount {
            get {
                if (!IsOpenForRead)
                    OpenForRead(this.machineName);
                int count;
                bool success = UnsafeNativeMethods.GetNumberOfEventLogRecords(readHandle, out count);
                if (!success)
                    throw SharedUtils.CreateSafeWin32Exception();
                return count;
            }
        }

        /// <devdoc>
        ///     Determines whether the event log is open in either read or write access
        /// </devdoc>
        private bool IsOpen {
            get {
                return readHandle != null || writeHandle != null;
            }
        }

        /// <devdoc>
        ///     Determines whether the event log is open with read access
        /// </devdoc>
        private bool IsOpenForRead {
            get {
                return readHandle != null;
            }
        }

        /// <devdoc>
        ///     Determines whether the event log is open with write access.
        /// </devdoc>
        private bool IsOpenForWrite {
            get {
                return writeHandle != null;
            }
        }

        /// <devdoc>
        ///    <para>
        ///    </para>
        /// </devdoc>
        public string LogDisplayName {
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
            get {

                if (logDisplayName != null)
                    return logDisplayName;

                string currentMachineName = this.machineName;
                if (GetLogName(currentMachineName) != null) {
                        
                    EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Administer, currentMachineName);
                    permission.Demand();

                    //Check environment before looking at the registry
                    SharedUtils.CheckEnvironment();

                    //SECREVIEW: Note that EventLogPermission is just demmanded above
                    PermissionSet permissionSet = EventLog._UnsafeGetAssertPermSet();
                    permissionSet.Assert();

                    RegistryKey logkey = null;

                    try {
                        // we figure out what logs are on the machine by looking in the registry.
                        logkey = GetLogRegKey(currentMachineName, false);
                        if (logkey == null)
                            throw new InvalidOperationException(SR.GetString(SR.MissingLog, GetLogName(currentMachineName), currentMachineName));

                        string resourceDll = (string)logkey.GetValue("DisplayNameFile");
                        if (resourceDll == null)
                            logDisplayName = GetLogName(currentMachineName);
                        else {
                            int resourceId = (int)logkey.GetValue("DisplayNameID");
                            logDisplayName = FormatMessageWrapper(resourceDll, (uint) resourceId, null);
                            if (logDisplayName == null)
                                logDisplayName = GetLogName(currentMachineName);
                        }
                    }
                    finally {
                        if (logkey != null) logkey.Close();

                        // Revert registry and environment permission asserts
                        CodeAccessPermission.RevertAssert();
                    }
                }

                return logDisplayName;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the name of the log to read from and write to.
        ///    </para>
        /// </devdoc>
        public string Log {
            get {
                string currentMachineName = this.machineName;
                if (logName == null || logName.Length == 0) {
                    EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Administer, currentMachineName);
                    permission.Demand();
                }

                return GetLogName(currentMachineName);
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity", Justification = "Microsoft: Safe, machineName doesn't change")]
        private string GetLogName(string currentMachineName)
        {
            if ((logName == null || logName.Length == 0) && sourceName != null && sourceName.Length!=0) {
                // they've told us a source, but they haven't told us a log name.
                // try to deduce the log name from the source name.
                EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Write, currentMachineName);
                permission.Demand();

                logName = EventLog._InternalLogNameFromSourceName(sourceName, currentMachineName);
            }
            return logName;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the name of the computer on which to read or write events.
        ///    </para>
        /// </devdoc>
        public string MachineName {
            get {
                string currentMachineName = this.machineName;

                EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Write, currentMachineName);
                permission.Demand();

                return currentMachineName;
            }
        }

        [ComVisible(false)]
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "Microsoft: MaximumKilobytes is the name of this property.")]
        public long MaximumKilobytes {
            get {
                string currentMachineName = this.machineName;

                EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Administer, currentMachineName);
                permission.Demand();

                object val = GetLogRegValue(currentMachineName, "MaxSize");
                if (val != null) {
                    int intval = (int) val;         // cast to an int first to unbox
                    return ((uint)intval) / 1024;   // then convert to kilobytes
                }

                // 512k is the default value
                return 0x200;
            }

            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
            set {
                string currentMachineName = this.machineName;
                
                EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Administer, currentMachineName);
                permission.Demand();

                // valid range is 64 KB to 4 GB
                if (value < 64 || value > 0x3FFFC0 || value % 64 != 0)
                    throw new ArgumentOutOfRangeException("MaximumKilobytes", SR.GetString(SR.MaximumKilobytesOutOfRange));

                PermissionSet permissionSet = EventLog._UnsafeGetAssertPermSet();
                permissionSet.Assert();

                long regvalue = value * 1024; // convert to bytes
                int i = unchecked((int)regvalue);

                using (RegistryKey logkey = GetLogRegKey(currentMachineName, true))
                    logkey.SetValue("MaxSize", i, RegistryValueKind.DWord);
            }
        }

        internal Hashtable MessageLibraries {
            get {
                if (messageLibraries == null)
                    messageLibraries = new Hashtable(StringComparer.OrdinalIgnoreCase);
                return messageLibraries;
            }
        }

        [ComVisible(false)]
        public OverflowAction OverflowAction {
            get {
                string currentMachineName = this.machineName;

                EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Administer, currentMachineName);
                permission.Demand();

                object retentionobj  = GetLogRegValue(currentMachineName, "Retention");
                if (retentionobj  != null) {
                    int retention = (int) retentionobj;
                    if (retention == 0)
                        return OverflowAction.OverwriteAsNeeded;
                    else if (retention == -1)
                        return OverflowAction.DoNotOverwrite;
                    else
                        return OverflowAction.OverwriteOlder;
                }

                // default value as listed in MSDN
                return OverflowAction.OverwriteOlder;
            }
        }

        [ComVisible(false)]
        public int MinimumRetentionDays {
            get {
                string currentMachineName = this.machineName;

                EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Administer, currentMachineName);
                permission.Demand();

                object retentionobj  = GetLogRegValue(currentMachineName, "Retention");
                if (retentionobj  != null) {
                    int retention = (int) retentionobj;
                    if (retention == 0 || retention == -1)
                        return retention;
                    else
                        return (int) (((double) retention) / SecondsPerDay);
                }
                return 7;
            }
        }

        /// <devdoc>
        /// </devdoc>
        public bool EnableRaisingEvents {
            get {
                string currentMachineName = this.machineName;

                EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Write, currentMachineName);
                permission.Demand();

                return boolFlags[Flag_monitoring];
            }
            set {
                string currentMachineName = this.machineName;

                EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Write, currentMachineName);
                permission.Demand();

                if (parent.ComponentDesignMode)
                    this.boolFlags[Flag_monitoring] = value;
                else {
                    if (value)
                        StartRaisingEvents(currentMachineName, GetLogName(currentMachineName));
                    else
                        StopRaisingEvents(/*currentMachineName,*/ GetLogName(currentMachineName));
                }
            }
        }

        private int OldestEntryNumber {
            get {
                if (!IsOpenForRead)
                    OpenForRead(this.machineName);
                int num;
                bool success = UnsafeNativeMethods.GetOldestEventLogRecord(readHandle, out num);
                if (!success)
                    throw SharedUtils.CreateSafeWin32Exception();

                // When the event log is empty, GetOldestEventLogRecord returns 0.
                // But then after an entry is written, it returns 1. We need to go from
                // the last num to the current.
                if (num == 0)
                    num = 1;

                return num;
            }
        }

        internal SafeEventLogReadHandle  ReadHandle {
            get {
                if (!IsOpenForRead)
                    OpenForRead(this.machineName);
                return readHandle;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Represents the object used to marshal the event handler
        ///       calls issued as a result of an <see cref='System.Diagnostics.EventLog'/>
        ///       change.
        ///    </para>
        /// </devdoc>
        public ISynchronizeInvoke SynchronizingObject {
        [HostProtection(Synchronization=true)]
            get {
                string currentMachineName = this.machineName;

                EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Write, currentMachineName);
                permission.Demand();
                if (this.synchronizingObject == null && parent.ComponentDesignMode) {
                    IDesignerHost host = (IDesignerHost) parent.ComponentGetService(typeof(IDesignerHost));
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

        /// <devdoc>
        ///    <para>
        ///       Gets or
        ///       sets the application name (source name) to register and use when writing to the event log.
        ///    </para>
        /// </devdoc>
        public string Source {
            get {
                string currentMachineName = this.machineName;

                EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Write, currentMachineName);
                permission.Demand();
                return sourceName;
            }
        }



        [HostProtection(Synchronization=true)]
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private static void AddListenerComponent(EventLogInternal component, string compMachineName, string compLogName) {
            lock (InternalSyncObject) {
                Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "EventLog::AddListenerComponent(" + compLogName + ")");

                LogListeningInfo info = (LogListeningInfo) listenerInfos[compLogName];
                if (info != null) {
                    Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "EventLog::AddListenerComponent: listener already active.");
                    info.listeningComponents.Add(component);
                    return;
                }

                info = new LogListeningInfo();
                info.listeningComponents.Add(component);

                info.handleOwner = new EventLogInternal(compLogName, compMachineName);

                // tell the event log system about it
                info.waitHandle = new AutoResetEvent(false); 
                bool success = UnsafeNativeMethods.NotifyChangeEventLog(info.handleOwner.ReadHandle, info.waitHandle.SafeWaitHandle);
                if (!success)
                    throw new InvalidOperationException(SR.GetString(SR.CantMonitorEventLog), SharedUtils.CreateSafeWin32Exception());

                info.registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(info.waitHandle, new WaitOrTimerCallback(StaticCompletionCallback), info, -1, false);

                listenerInfos[compLogName] = info;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Occurs when an entry is written to the event log.
        ///    </para>
        /// </devdoc>
        public event EntryWrittenEventHandler EntryWritten {
            add {
                string currentMachineName = this.machineName;

                EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Administer, currentMachineName);
                permission.Demand();

                onEntryWrittenHandler += value;
            }
            remove {
                string currentMachineName = this.machineName;

                EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Administer, currentMachineName);
                permission.Demand();

                onEntryWrittenHandler -= value;
            }
        }

        /// <devdoc>
        /// </devdoc>
        public void BeginInit() {
            string currentMachineName = this.machineName;

            EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Write, currentMachineName);
            permission.Demand();

            if (boolFlags[Flag_initializing]) throw new InvalidOperationException(SR.GetString(SR.InitTwice));
            boolFlags[Flag_initializing] = true;
            if (boolFlags[Flag_monitoring])
                StopListening(GetLogName(currentMachineName));
        }

        /// <devdoc>
        ///    <para>
        ///       Clears
        ///       the event log by removing all entries from it.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]  // Should anyone ever call this, other than an event log viewer?
        [ResourceConsumption(ResourceScope.Machine)]
        public void Clear() {
            string currentMachineName = this.machineName;

            EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Administer, currentMachineName);
            permission.Demand();

            if (!IsOpenForRead)
                OpenForRead(currentMachineName);
            bool success = UnsafeNativeMethods.ClearEventLog(readHandle, NativeMethods.NullHandleRef);
            if (!success) {
                // Ignore file not found errors.  ClearEventLog seems to try to delete the file where the event log is
                // stored.  If it can't find it, it gives an error. 
                int error = Marshal.GetLastWin32Error();
                if (error != NativeMethods.ERROR_FILE_NOT_FOUND)
                    throw SharedUtils.CreateSafeWin32Exception();
            }
            
            // now that we've cleared the event log, we need to re-open our handles, because
            // the internal state of the event log has changed.
            Reset(currentMachineName);
        }

        /// <devdoc>
        ///    <para>
        ///       Closes the event log and releases read and write handles.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        public void Close() {
            Close(this.machineName);
        }

        [SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity", Justification = "Microsoft: Safe, currentMachineName doesn't change")]
        private void Close(string currentMachineName) {
            EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Write, currentMachineName);
            permission.Demand();

            Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "EventLog::Close");
            //Trace("Close", "Closing the event log");
            if (readHandle != null) {
                try {
                    readHandle.Close();
                }
                catch (IOException) {
                    throw SharedUtils.CreateSafeWin32Exception();
                }
                readHandle = null;
                //Trace("Close", "Closed read handle");
                Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "EventLog::Close: closed read handle");
            }
            if (writeHandle != null) {
                try {
                    writeHandle.Close();
                }
                catch (IOException) {
                    throw SharedUtils.CreateSafeWin32Exception();
                }
                writeHandle = null;
                //Trace("Close", "Closed write handle");
                Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "EventLog::Close: closed write handle");
            }
            if (boolFlags[Flag_monitoring])
                StopRaisingEvents(/*currentMachineName,*/ GetLogName(currentMachineName));

            if (messageLibraries != null) {
                foreach (SafeLibraryHandle handle in messageLibraries.Values)
                    handle.Close();

                messageLibraries = null;
            }
            
            boolFlags[Flag_sourceVerified] = false;
        }


        /// <internalonly/>
        /// <devdoc>
        ///     Called when the threadpool is ready for us to handle a status change.
        /// </devdoc>
        private void CompletionCallback(object context)  {

            if (boolFlags[Flag_disposed]) {
                // This object has been disposed previously, ignore firing the event.
                return;
            }

            Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "EventLog::CompletionStatusChanged: starting at " + lastSeenCount.ToString(CultureInfo.InvariantCulture));
            lock (InstanceLockObject) {
                if (boolFlags[Flag_notifying]) {
                    // don't do double notifications.
                    Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "EventLog::CompletionStatusChanged: aborting because we're already notifying.");
                    return;
                }
                boolFlags[Flag_notifying] = true;
            }

            int i = lastSeenCount;

            try {
                int oldest = OldestEntryNumber;
                int count = EntryCount + oldest;

                // Ensure lastSeenCount is within bounds.  This deals with the case where the event log has been cleared between
                // notifications.
                if (lastSeenCount < oldest || lastSeenCount > count) {
                    lastSeenCount = oldest;
                    i = lastSeenCount;
                }

                // NOTE, Microsoft: We have a double loop here so that we access the
                // EntryCount property as infrequently as possible. (It may be expensive
                // to get the property.) Even though there are two loops, they will together
                // only execute as many times as (final value of EntryCount) - lastSeenCount.
                Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "EventLog::CompletionStatusChanged: OldestEntryNumber is " + OldestEntryNumber + ", EntryCount is " + EntryCount);
                while (i < count) {
                    while (i < count) {
                        EventLogEntry entry = GetEntryWithOldest(i);
                        if (this.SynchronizingObject != null && this.SynchronizingObject.InvokeRequired)
                            this.SynchronizingObject.BeginInvoke(this.onEntryWrittenHandler, new object[]{this, new EntryWrittenEventArgs(entry)});
                        else
                           onEntryWrittenHandler(this, new EntryWrittenEventArgs(entry));

                        i++;
                    }
                    oldest = OldestEntryNumber;
                    count = EntryCount + oldest;
                }
            }
            catch (Exception e) {
                Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "EventLog::CompletionStatusChanged: Caught exception notifying event handlers: " + e.ToString());
            }

            try {
                // if the user cleared the log while we were receiving events, the call to GetEntryWithOldest above could have 
                // thrown an exception and i could be too large.  Make sure we don't set lastSeenCount to something bogus.
                int newCount = EntryCount + OldestEntryNumber;
                if (i > newCount)
                    lastSeenCount = newCount;
                else
                    lastSeenCount = i;
                Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "EventLog::CompletionStatusChanged: finishing at " + lastSeenCount.ToString(CultureInfo.InvariantCulture));
            }
            catch (Win32Exception e) {
                Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "EventLog::CompletionStatusChanged: Caught exception updating last entry number: " + e.ToString());
            }

            lock (InstanceLockObject) {
                boolFlags[Flag_notifying] = false;
            }
       }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <devdoc>
        /// </devdoc>
        internal void Dispose(bool disposing) {
            try {
                if (disposing) {
                    //Dispose unmanaged and managed resources
                    if (IsOpen) {
                        Close();
                    }

                    // This is probably unnecessary
                    if (readHandle != null) {
                    readHandle.Close();
                        readHandle = null;
                    }

                    if (writeHandle != null) {
                    writeHandle.Close();
                        writeHandle = null;
                    }
                }
            }
            finally {
                messageLibraries = null;
                this.boolFlags[Flag_disposed] = true;
            }
        }

        /// <devdoc>
        /// </devdoc>
        public void EndInit() {
            string currentMachineName = this.machineName;

            EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Write, currentMachineName);
            permission.Demand();

            boolFlags[Flag_initializing] = false;
            if (boolFlags[Flag_monitoring])
                StartListening(currentMachineName, GetLogName(currentMachineName));
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        internal string FormatMessageWrapper(string dllNameList, uint messageNum, string[] insertionStrings) {
            if (dllNameList == null)
                return null;

            if (insertionStrings == null)
                insertionStrings = new string[0];

            string[] listDll = dllNameList.Split(';');

            // Find first mesage in DLL list
            foreach ( string dllName in  listDll) {
                if (dllName == null || dllName.Length == 0)
                    continue;

                SafeLibraryHandle hModule = null;

                // if the EventLog is open, then we want to cache the library in our hashtable.  Otherwise
                // we'll just load it and free it after we're done.
                if (IsOpen) {
                    hModule = MessageLibraries[dllName] as SafeLibraryHandle;

                    if (hModule == null || hModule.IsInvalid) {
                        hModule = SafeLibraryHandle.LoadLibraryEx(dllName, IntPtr.Zero, NativeMethods.LOAD_LIBRARY_AS_DATAFILE);
                        MessageLibraries[dllName] = hModule;
                    }
                }
                else {
                    hModule = SafeLibraryHandle.LoadLibraryEx(dllName, IntPtr.Zero, NativeMethods.LOAD_LIBRARY_AS_DATAFILE);
                }

                if (hModule.IsInvalid)
                    continue;

                string msg = null;
                try {
                    msg = EventLog.TryFormatMessage(hModule, messageNum, insertionStrings);
                }
                finally {
                    if (!IsOpen) {
                        hModule.Close();
                    }
                }

                if ( msg != null ) {
                    return msg;
                }

            }
            return null;
        }

        /// <devdoc>
        ///     Gets an array of EventLogEntry's, one for each entry in the log.
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        internal EventLogEntry[] GetAllEntries() {
            // we could just call getEntryAt() on all the entries, but it'll be faster
            // if we grab multiple entries at once.
            string currentMachineName = this.machineName;

            if (!IsOpenForRead)
                OpenForRead(currentMachineName);

            EventLogEntry[] entries = new EventLogEntry[EntryCount];
            int idx = 0;
            int oldestEntry = OldestEntryNumber;

            int bytesRead;
            int minBytesNeeded;
            int error = 0;
            while (idx < entries.Length) {
                byte[] buf = new byte[BUF_SIZE];
                bool success = UnsafeNativeMethods.ReadEventLog(readHandle, NativeMethods.FORWARDS_READ | NativeMethods.SEEK_READ,
                                                      oldestEntry+idx, buf, buf.Length, out bytesRead, out minBytesNeeded);
                if (!success) {
                    error = Marshal.GetLastWin32Error();
                    // NOTE, Microsoft: ERROR_PROC_NOT_FOUND used to get returned, but I think that
                    // was because I was calling GetLastError directly instead of GetLastWin32Error.
                    // Making the buffer bigger and trying again seemed to work. I've removed the check
                    // for ERROR_PROC_NOT_FOUND because I don't think it's necessary any more, but
                    // I can't prove it...
                    Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "Error from ReadEventLog is " + error.ToString(CultureInfo.InvariantCulture));
#if !RETRY_ON_ALL_ERRORS
                    if (error == NativeMethods.ERROR_INSUFFICIENT_BUFFER || error == NativeMethods.ERROR_EVENTLOG_FILE_CHANGED) {
#endif
                        if (error == NativeMethods.ERROR_EVENTLOG_FILE_CHANGED) {
                            // somewhere along the way the event log file changed - probably it
                            // got cleared while we were looping here. Reset the handle and
                            // try again.
                            Reset(currentMachineName);
                        }
                        // try again with a bigger buffer if necessary
                        else if (minBytesNeeded > buf.Length) {
                            Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "Increasing buffer size from " + buf.Length.ToString(CultureInfo.InvariantCulture) + " to " + minBytesNeeded.ToString(CultureInfo.InvariantCulture) + " bytes");
                            buf = new byte[minBytesNeeded];
                        }
                        success = UnsafeNativeMethods.ReadEventLog(readHandle, NativeMethods.FORWARDS_READ | NativeMethods.SEEK_READ,
                                                         oldestEntry+idx, buf, buf.Length, out bytesRead, out minBytesNeeded);
                        if (!success)
                            // we'll just stop right here.
                            break;
#if !RETRY_ON_ALL_ERRORS
                    }
                    else {
                        break;
                    }
#endif
                    error = 0;
                }
                entries[idx] = new EventLogEntry(buf, 0, this);
                int sum = IntFrom(buf, 0);
                idx++;
                while (sum < bytesRead && idx < entries.Length) {
                    entries[idx] = new EventLogEntry(buf, sum, this);
                    sum += IntFrom(buf, sum);
                    idx++;
                }
            }
            if (idx != entries.Length) {
                if (error != 0)
                    throw new InvalidOperationException(SR.GetString(SR.CantRetrieveEntries), SharedUtils.CreateSafeWin32Exception(error));
                else
                    throw new InvalidOperationException(SR.GetString(SR.CantRetrieveEntries));
            }
            return entries;
        }

        /// <devdoc>
        ///     Searches the cache for an entry with the given index
        /// </devdoc>
        private int GetCachedEntryPos(int entryIndex) {
            if (cache == null || (boolFlags[Flag_forwards] && entryIndex < firstCachedEntry) ||
                (!boolFlags[Flag_forwards] && entryIndex > firstCachedEntry) || firstCachedEntry == -1) {
                // the index falls before anything we have in the cache, or the cache
                // is not yet valid
                return -1;
            }
            // we only know where the beginning of the cache is, not the end, so even
            // if it's past the end of the cache, we'll have to search through the whole
            // cache to find out.

            // we're betting heavily that the one they want to see now is close
            // to the one they asked for last time. We start looking where we
            // stopped last time.

            // We have two loops, one to go forwards and one to go backwards. Only one
            // of them will ever be executed.
            while (lastSeenEntry < entryIndex) {
                lastSeenEntry++;
                if (boolFlags[Flag_forwards]) {
                    lastSeenPos = GetNextEntryPos(lastSeenPos);
                    if (lastSeenPos >= bytesCached)
                        break;
                }
                else {
                    lastSeenPos = GetPreviousEntryPos(lastSeenPos);
                    if (lastSeenPos < 0)
                        break;
                }
            }
            while (lastSeenEntry > entryIndex) {
                lastSeenEntry--;
                if (boolFlags[Flag_forwards]) {
                    lastSeenPos = GetPreviousEntryPos(lastSeenPos);
                    if (lastSeenPos < 0)
                        break;
                }
                else {
                    lastSeenPos = GetNextEntryPos(lastSeenPos);
                    if (lastSeenPos >= bytesCached)
                        break;
                }
            }
            if (lastSeenPos >= bytesCached) {
                // we ran past the end. move back to the last one and return -1
                lastSeenPos = GetPreviousEntryPos(lastSeenPos);
                if (boolFlags[Flag_forwards])
                    lastSeenEntry--;
                else
                    lastSeenEntry++;
                return -1;
            }
            else if (lastSeenPos < 0) {
                // we ran past the beginning. move back to the first one and return -1
                lastSeenPos = 0;
                if (boolFlags[Flag_forwards])
                    lastSeenEntry++;
                else
                    lastSeenEntry--;
                return -1;
            }
            else {
                // we found it.
                return lastSeenPos;
            }
        }

        /// <devdoc>
        ///     Gets the entry at the given index
        /// </devdoc>
        internal EventLogEntry GetEntryAt(int index) {
            EventLogEntry entry = GetEntryAtNoThrow(index);
            if (entry == null)
                throw new ArgumentException(SR.GetString(SR.IndexOutOfBounds, index.ToString(CultureInfo.CurrentCulture)));
            return entry;
        }

        internal EventLogEntry GetEntryAtNoThrow(int index) {
            if (!IsOpenForRead)
                OpenForRead(this.machineName);

            if (index < 0 || index >= EntryCount)
                return null;

            // 

            index += OldestEntryNumber;
            EventLogEntry entry = null;

            try {
                entry = GetEntryWithOldest(index);
            }
            catch (InvalidOperationException) {
                // This would be common in rapidly spinning EventLog (i.e. logs which are rapidly receiving 
                // new events while discarding old ones in a rolling fashion) or if the EventLog is cleared asynchronously.
                //
                // EventLogEntryCollection heuristics is little bit convoluted due to the inherent ----s. 
                // The enumerator predominantly operates on the index from the last “known” oldest entry 
                // (refreshing on every iteration is probalby not right here) and it has no notion of the 
                // collection size when it is created or while it is operating. It would keep on enumerating 
                // until the index become invalid.  
                //
                // Throwing InvalidOperationException to let you know that your enumerator has been invalidated 
                // because of changes underneath is probably not the most useful behavior.
            }
            return entry;
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private EventLogEntry GetEntryWithOldest(int index) {
            EventLogEntry entry = null;
            int entryPos = GetCachedEntryPos(index);
            if (entryPos >= 0) {
                entry = new EventLogEntry(cache, entryPos, this);
                return entry;
            }

            string currentMachineName = this.machineName;

            // if we haven't seen the one after this, we were probably going
            // forwards.
            int flags = 0;
            if (GetCachedEntryPos(index+1) < 0) {
                flags = NativeMethods.FORWARDS_READ | NativeMethods.SEEK_READ;
                boolFlags[Flag_forwards] = true;
            }
            else {
                flags = NativeMethods.BACKWARDS_READ | NativeMethods.SEEK_READ;
                boolFlags[Flag_forwards] = false;
            }

            cache = new byte[BUF_SIZE];
            int bytesRead;
            int minBytesNeeded;
            bool success = UnsafeNativeMethods.ReadEventLog(readHandle, flags, index,
                                                  cache, cache.Length, out bytesRead, out minBytesNeeded);
            if (!success) {
                int error = Marshal.GetLastWin32Error();
                // NOTE, Microsoft: ERROR_PROC_NOT_FOUND used to get returned, but I think that
                // was because I was calling GetLastError directly instead of GetLastWin32Error.
                // Making the buffer bigger and trying again seemed to work. I've removed the check
                // for ERROR_PROC_NOT_FOUND because I don't think it's necessary any more, but
                // I can't prove it...
                Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "Error from ReadEventLog is " + error.ToString(CultureInfo.InvariantCulture));
                if (error == NativeMethods.ERROR_INSUFFICIENT_BUFFER || error == NativeMethods.ERROR_EVENTLOG_FILE_CHANGED) {
                    if (error == NativeMethods.ERROR_EVENTLOG_FILE_CHANGED) {
                        // Reset() sets the cache null.  But since we're going to call ReadEventLog right after this,
                        // we need the cache to be something valid.  We'll reuse the old byte array rather
                        // than creating a new one.
                        byte[] tempcache = cache;
                        Reset(currentMachineName);
                        cache = tempcache;
                    } else {
                        // try again with a bigger buffer.
                        if (minBytesNeeded > cache.Length) {
                            cache = new byte[minBytesNeeded];
                        }
                    }
                    success = UnsafeNativeMethods.ReadEventLog(readHandle, NativeMethods.FORWARDS_READ | NativeMethods.SEEK_READ, index,
                                                     cache, cache.Length, out bytesRead, out minBytesNeeded);
                }

                if (!success) {
                    throw new InvalidOperationException(SR.GetString(SR.CantReadLogEntryAt, index.ToString(CultureInfo.CurrentCulture)), SharedUtils.CreateSafeWin32Exception());
                }
            }
            bytesCached = bytesRead;
            firstCachedEntry = index;
            lastSeenEntry = index;
            lastSeenPos = 0;
            return new EventLogEntry(cache, 0, this);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static RegistryKey GetEventLogRegKey(string machine, bool writable) {
            RegistryKey lmkey = null;
            
            try {
                if (machine.Equals(".")) {
                    lmkey = Registry.LocalMachine;
                }
                else {
                    lmkey = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, machine);

                }
                if (lmkey != null)
                    return lmkey.OpenSubKey(EventLogKey, writable);
            }
            finally {
                if (lmkey != null) lmkey.Close();
            }

            return null;
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private RegistryKey GetLogRegKey(string currentMachineName, bool writable) {
            string logname = GetLogName(currentMachineName);
            // we need to verify the logname here again because we might have tried to look it up
            // based on the source and failed.
            if (!ValidLogName(logname, false))
                throw new InvalidOperationException(SR.GetString(SR.BadLogName));

            RegistryKey eventkey = null;
            RegistryKey logkey = null;

            try {
                eventkey = GetEventLogRegKey(currentMachineName, false);
                if (eventkey == null)
                    throw new InvalidOperationException(SR.GetString(SR.RegKeyMissingShort, EventLogKey, currentMachineName));

                logkey = eventkey.OpenSubKey(logname, writable);
                if (logkey == null)
                    throw new InvalidOperationException(SR.GetString(SR.MissingLog, logname, currentMachineName));
            }
            finally {
                if (eventkey != null) eventkey.Close();
            }

            return logkey;
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private object GetLogRegValue(string currentMachineName, string valuename) {
            PermissionSet permissionSet = EventLog._UnsafeGetAssertPermSet();
            permissionSet.Assert();
            
            RegistryKey logkey = null;

            try {
                logkey = GetLogRegKey(currentMachineName, false);
                if (logkey == null)
                    throw new InvalidOperationException(SR.GetString(SR.MissingLog, GetLogName(currentMachineName), currentMachineName));

                object val = logkey.GetValue(valuename);
                return val;
            }
            finally {
                if (logkey != null) logkey.Close();

                // Revert registry and environment permission asserts
                CodeAccessPermission.RevertAssert();
            }
        }

        /// <devdoc>
        ///     Finds the index into the cache where the next entry starts
        /// </devdoc>
        private int GetNextEntryPos(int pos) {
            return pos + IntFrom(cache, pos);
        }

        /// <devdoc>
        ///     Finds the index into the cache where the previous entry starts
        /// </devdoc>
        private int GetPreviousEntryPos(int pos) {
            // the entries in our buffer come back like this:
            // <length 1> ... <data> ...  <length 1> <length 2> ... <data> ... <length 2> ...
            // In other words, the length for each entry is repeated at the beginning and
            // at the end. This makes it easy to navigate forwards and backwards through
            // the buffer.
            return pos - IntFrom(cache, pos - 4);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static string GetDllPath(string machineName) {
            return Path.Combine(SharedUtils.GetLatestBuildDllDirectory(machineName), DllName);
        }

        /// <devdoc>
        ///     Extracts a 32-bit integer from the ubyte buffer, beginning at the byte offset
        ///     specified in offset.
        /// </devdoc>
        private static int IntFrom(byte[] buf, int offset) {
            // assumes Little Endian byte order.
            return(unchecked((int)0xFF000000) & (buf[offset+3] << 24)) | (0xFF0000 & (buf[offset+2] << 16)) |
            (0xFF00 & (buf[offset+1] << 8)) | (0xFF & (buf[offset]));
        }

        [ComVisible(false)]
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public void ModifyOverflowPolicy(OverflowAction action, int retentionDays) {
            string currentMachineName = this.machineName;
            
            EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Administer, currentMachineName);
            permission.Demand();

            if (action < OverflowAction.DoNotOverwrite || action > OverflowAction.OverwriteOlder)
                throw new InvalidEnumArgumentException("action", (int)action, typeof(OverflowAction));

            // this is a long because in the if statement we may need to store values as
            // large as UInt32.MaxValue - 1.  This would overflow an int.
            long retentionvalue = (long) action;
            if (action == OverflowAction.OverwriteOlder) {
                if (retentionDays < 1 || retentionDays > 365)
                    throw new ArgumentOutOfRangeException(SR.GetString(SR.RentionDaysOutOfRange));

                retentionvalue = (long) retentionDays * SecondsPerDay;
            }

            PermissionSet permissionSet = EventLog._UnsafeGetAssertPermSet();
            permissionSet.Assert();
            
            using (RegistryKey logkey = GetLogRegKey(currentMachineName, true))
                logkey.SetValue("Retention", retentionvalue, RegistryValueKind.DWord);
        }


        /// <devdoc>
        ///     Opens the event log with read access
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity", Justification = "Microsoft: Safe, machineName doesn't change")]
        private void OpenForRead(string currentMachineName) {
            Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "EventLog::OpenForRead");

            EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Administer, currentMachineName);
            permission.Demand();

            //Cannot allocate the readHandle if the object has been disposed, since finalization has been suppressed.
            if (this.boolFlags[Flag_disposed])
                throw new ObjectDisposedException(GetType().Name);

            string logname = GetLogName(currentMachineName);

            if (logname == null || logname.Length==0)
                throw new ArgumentException(SR.GetString(SR.MissingLogProperty));

            if (!EventLog.Exists(logname, currentMachineName) )        // do not open non-existing Log [Microsoft]
                throw new InvalidOperationException( SR.GetString(SR.LogDoesNotExists, logname, currentMachineName) );
            //Check environment before calling api
            SharedUtils.CheckEnvironment();

            // Clean up cache variables.
            // [Microsoft] The initilizing code is put here to guarantee, that first read of events
            //           from log file will start by filling up the cache buffer.
            lastSeenEntry = 0;
            lastSeenPos = 0;
            bytesCached = 0;
            firstCachedEntry = -1;

            SafeEventLogReadHandle handle = SafeEventLogReadHandle.OpenEventLog(currentMachineName, logname);
            if (handle.IsInvalid) {
                Win32Exception e = null;
                if (Marshal.GetLastWin32Error() != 0) {
                    e = SharedUtils.CreateSafeWin32Exception();
                }

                throw new InvalidOperationException(SR.GetString(SR.CantOpenLog, logname.ToString(), currentMachineName), e);
            }

            readHandle = handle;
        }

        /// <devdoc>
        ///     Opens the event log with write access
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private void OpenForWrite(string currentMachineName) {
            //Cannot allocate the writeHandle if the object has been disposed, since finalization has been suppressed.
            if (this.boolFlags[Flag_disposed])
                throw new ObjectDisposedException(GetType().Name);

            Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "EventLog::OpenForWrite");
            if (sourceName == null || sourceName.Length==0)
                throw new ArgumentException(SR.GetString(SR.NeedSourceToOpen));

            //Check environment before calling api
            SharedUtils.CheckEnvironment();

            SafeEventLogWriteHandle handle = SafeEventLogWriteHandle.RegisterEventSource(currentMachineName, sourceName);
            if (handle.IsInvalid) {
                Win32Exception e = null;
                if (Marshal.GetLastWin32Error() != 0) {
                    e = SharedUtils.CreateSafeWin32Exception();
                }
                throw new InvalidOperationException(SR.GetString(SR.CantOpenLogAccess, sourceName), e);
            }

            writeHandle = handle;
        }

        [ComVisible(false)]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public void RegisterDisplayName(string resourceFile, long resourceId) {
            string currentMachineName = this.machineName;

            EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Administer, currentMachineName);
            permission.Demand();

            PermissionSet permissionSet = EventLog._UnsafeGetAssertPermSet();
            permissionSet.Assert();

            using (RegistryKey logkey = GetLogRegKey(currentMachineName, true)) {
                logkey.SetValue("DisplayNameFile", resourceFile, RegistryValueKind.ExpandString);
                logkey.SetValue("DisplayNameID", resourceId, RegistryValueKind.DWord);
            }
        }

        private void Reset(string currentMachineName) {
            Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "EventLog::Reset");
            // save the state we're in now
            bool openRead = IsOpenForRead;
            bool openWrite = IsOpenForWrite;
            bool isMonitoring = boolFlags[Flag_monitoring];
            bool isListening = boolFlags[Flag_registeredAsListener];

            // close everything down
            Close(currentMachineName);
            cache = null;

            // and get us back into the same state as before
            if (openRead)
                OpenForRead(currentMachineName);
            if (openWrite)
                OpenForWrite(currentMachineName);
            if (isListening)
                StartListening(currentMachineName, GetLogName(currentMachineName));
            
            boolFlags[Flag_monitoring] = isMonitoring;
        }

        [HostProtection(Synchronization=true)]
        private static void RemoveListenerComponent(EventLogInternal component, string compLogName) {
            lock (InternalSyncObject) {
                Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "EventLog::RemoveListenerComponent(" + compLogName + ")");

                LogListeningInfo info = (LogListeningInfo) listenerInfos[compLogName];
                Debug.Assert(info != null);

                // remove the requested component from the list.
                info.listeningComponents.Remove(component);
                if (info.listeningComponents.Count != 0)
                    return;

                // if that was the last interested compononent, destroy the handles and stop listening.

                info.handleOwner.Dispose();

                //Unregister the thread pool wait handle
                info.registeredWaitHandle.Unregister(info.waitHandle);
                // close the handle
                info.waitHandle.Close();

                listenerInfos[compLogName] = null;
            }
        }
      
        /// <devdoc>
        ///     Sets up the event monitoring mechanism.  We don't track event log changes
        ///     unless someone is interested, so we set this up on demand.
        /// </devdoc>
        [HostProtection(Synchronization=true, ExternalThreading=true)]
        private void StartListening(string currentMachineName, string currentLogName) {
            // make sure we don't fire events for entries that are already there
            Debug.Assert(!boolFlags[Flag_registeredAsListener], "StartListening called with boolFlags[Flag_registeredAsListener] true.");
            lastSeenCount = EntryCount + OldestEntryNumber;
            Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "EventLog::StartListening: lastSeenCount = " + lastSeenCount);
            AddListenerComponent(this, currentMachineName, currentLogName);
            boolFlags[Flag_registeredAsListener] = true;
        }

        private void StartRaisingEvents(string currentMachineName, string currentLogName) {
            if (!boolFlags[Flag_initializing] && !boolFlags[Flag_monitoring] && !parent.ComponentDesignMode) {
                StartListening(currentMachineName, currentLogName);
            }
            boolFlags[Flag_monitoring] = true;
        }

        private static void StaticCompletionCallback(object context, bool wasSignaled) {
            
            LogListeningInfo info = (LogListeningInfo) context;
            if (info == null)
                return;

            // get a snapshot of the components to fire the event on
            EventLogInternal[] interestedComponents;
            lock (InternalSyncObject) {
                interestedComponents = (EventLogInternal[])info.listeningComponents.ToArray(typeof(EventLogInternal));
            }

            Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "EventLog::StaticCompletionCallback: notifying " + interestedComponents.Length + " components.");

            for (int i = 0; i < interestedComponents.Length; i++) {
                try {
                    if (interestedComponents[i] != null) {
                        interestedComponents[i].CompletionCallback(null);
                    }
                } catch (ObjectDisposedException) {
                    // The EventLog that was registered to listen has been disposed.  Nothing much we can do here
                    // we don't want to propigate this error up as it will likely be unhandled and will cause the app
                    // to crash.
                    Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "EventLog::StaticCompletionCallback: ignored an ObjectDisposedException");
                }
            }
        }

        /// <devdoc>
        ///     Tears down the event listening mechanism.  This is called when the last
        ///     interested party removes their event handler.
        /// </devdoc>
        [HostProtection(Synchronization=true, ExternalThreading=true)]
        private void StopListening(/*string currentMachineName,*/ string currentLogName) {
            Debug.Assert(boolFlags[Flag_registeredAsListener], "StopListening called without StartListening.");
            RemoveListenerComponent(this, currentLogName);
            boolFlags[Flag_registeredAsListener] = false;
        }

        /// <devdoc>
        /// </devdoc>
        private void StopRaisingEvents(/*string currentMachineName,*/ string currentLogName) {
            if (!boolFlags[Flag_initializing] && boolFlags[Flag_monitoring] && !parent.ComponentDesignMode) {
                StopListening(currentLogName);
            }
            boolFlags[Flag_monitoring] = false;
        }

        // CharIsPrintable used to be Char.IsPrintable, but Jay removed it and
        // is forcing people to use the Unicode categories themselves.  Copied
        // the code here.  
        private static bool CharIsPrintable(char c) {
            UnicodeCategory uc = Char.GetUnicodeCategory(c);
            return (!(uc == UnicodeCategory.Control) || (uc == UnicodeCategory.Format) ||
                    (uc == UnicodeCategory.LineSeparator) || (uc == UnicodeCategory.ParagraphSeparator) ||
            (uc == UnicodeCategory.OtherNotAssigned));
        }

        // SECREVIEW: Make sure this method catches all the strange cases.
        internal static bool ValidLogName(string logName, bool ignoreEmpty) {
            // No need to trim here since the next check will verify that there are no spaces.
            // We need to ignore the empty string as an invalid log name sometimes because it can
            // be passed in from our default constructor.
            if (logName.Length == 0 && !ignoreEmpty)
                return false;

            //any space, backslash, asterisk, or question mark is bad
            //any non-printable characters are also bad
            foreach (char c in logName)
                if (!CharIsPrintable(c) || (c == '\\') || (c == '*') || (c == '?'))
                    return false;

            return true;
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity", Justification = "Microsoft: Safe, machineName doesn't change")]
        private void VerifyAndCreateSource(string sourceName, string currentMachineName) {
            if (boolFlags[Flag_sourceVerified]) 
                return;
            
            if (!EventLog.SourceExists(sourceName, currentMachineName, true)) {
                Mutex mutex = null;
                RuntimeHelpers.PrepareConstrainedRegions();
                try {
                    SharedUtils.EnterMutex(eventLogMutexName, ref mutex);
                    if (!EventLog.SourceExists(sourceName, currentMachineName, true)) {
                        if (GetLogName(currentMachineName) == null)
                            this.logName = "Application";
                        // we automatically add an entry in the registry if there's not already
                        // one there for this source
                        EventLog.CreateEventSource(new EventSourceCreationData(sourceName, GetLogName(currentMachineName), currentMachineName));
                        // The user may have set a custom log and tried to read it before trying to
                        // write. Due to a quirk in the event log API, we would have opened the Application
                        // log to read (because the custom log wasn't there). Now that we've created
                        // the custom log, we should close so that when we re-open, we get a read
                        // handle on the _new_ log instead of the Application log.
                        Reset(currentMachineName);
                    }
                    else {
                        string rightLogName = EventLog.LogNameFromSourceName(sourceName, currentMachineName);
                        string currentLogName = GetLogName(currentMachineName);
                        if (rightLogName != null && currentLogName != null && String.Compare(rightLogName, currentLogName, StringComparison.OrdinalIgnoreCase) != 0)
                            throw new ArgumentException(SR.GetString(SR.LogSourceMismatch, Source.ToString(), currentLogName, rightLogName));
                    }
                    
                }
                finally {
                    if (mutex != null) {
                        mutex.ReleaseMutex();
                        mutex.Close();
                    }
                }
            }
            else {
                EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Write, currentMachineName);
                permission.Demand();

                string rightLogName = EventLog._InternalLogNameFromSourceName(sourceName, currentMachineName);
                string currentLogName = GetLogName(currentMachineName);
                if (rightLogName != null && currentLogName != null && String.Compare(rightLogName, currentLogName, StringComparison.OrdinalIgnoreCase) != 0)
                    throw new ArgumentException(SR.GetString(SR.LogSourceMismatch, Source.ToString(), currentLogName, rightLogName));
            }
            
            boolFlags[Flag_sourceVerified] = true;
        }

        /// <devdoc>
        ///    <para>
        ///       Writes an information type entry with the given message text to the event log.
        ///    </para>
        /// </devdoc>
        public void WriteEntry(string message) {
            WriteEntry(message, EventLogEntryType.Information, (short) 0, 0, null);
        }

        /// <devdoc>
        ///    <para>
        ///       Writes an entry of the specified <see cref='System.Diagnostics.EventLogEntryType'/> to the event log. Valid types are
        ///    <see langword='Error'/>, <see langword='Warning'/>, <see langword='Information'/>,
        ///    <see langword='Success Audit'/>, and <see langword='Failure Audit'/>.
        ///    </para>
        /// </devdoc>
        public void WriteEntry(string message, EventLogEntryType type) {
            WriteEntry(message, type, (short) 0, 0, null);
        }

        /// <devdoc>
        ///    <para>
        ///       Writes an entry of the specified <see cref='System.Diagnostics.EventLogEntryType'/>
        ///       and with the
        ///       user-defined <paramref name="eventID"/>
        ///       to
        ///       the event log.
        ///    </para>
        /// </devdoc>
        public void WriteEntry(string message, EventLogEntryType type, int eventID) {
            WriteEntry(message, type, eventID, 0, null);
        }

        /// <devdoc>
        ///    <para>
        ///       Writes an entry of the specified type with the
        ///       user-defined <paramref name="eventID"/> and <paramref name="category"/>
        ///       to the event log. The <paramref name="category"/>
        ///       can be used by the event viewer to filter events in the log.
        ///    </para>
        /// </devdoc>
        public void WriteEntry(string message, EventLogEntryType type, int eventID, short category) {
            WriteEntry(message, type, eventID, category, null);
        }

        /// <devdoc>
        ///    <para>
        ///       Writes an entry of the specified type with the
        ///       user-defined <paramref name="eventID"/> and <paramref name="category"/> to the event log, and appends binary data to
        ///       the message. The Event Viewer does not interpret this data; it
        ///       displays raw data only in a combined hexadecimal and text format.
        ///    </para>
        /// </devdoc>
        public void WriteEntry(string message, EventLogEntryType type, int eventID, short category,
                               byte[] rawData) {

            if (eventID < 0 || eventID > ushort.MaxValue)
                throw new ArgumentException(SR.GetString(SR.EventID, eventID, 0, (int)ushort.MaxValue));

            if (Source.Length == 0)
                throw new ArgumentException(SR.GetString(SR.NeedSourceToWrite));

            if (!Enum.IsDefined(typeof(EventLogEntryType), type))
                throw new InvalidEnumArgumentException("type", (int)type, typeof(EventLogEntryType));

            string currentMachineName = machineName;
            if (!boolFlags[Flag_writeGranted]) {
                EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Write, currentMachineName);
                permission.Demand();
                boolFlags[Flag_writeGranted] = true;
            }

            VerifyAndCreateSource(sourceName, currentMachineName);

            // now that the source has been hooked up to our DLL, we can use "normal"
            // (message-file driven) logging techniques.
            // Our DLL has 64K different entries; all of them just display the first
            // insertion string.
            InternalWriteEvent((uint)eventID, (ushort)category, type, new string[] { message}, rawData, currentMachineName);
        }

        [ComVisible(false)]
        public void WriteEvent(EventInstance instance, params Object[] values) {
            WriteEvent(instance, null, values);
        }

        [ComVisible(false)]
        public void WriteEvent(EventInstance instance, byte[] data, params Object[] values) {
            if (instance == null)
                throw new ArgumentNullException("instance");
            if (Source.Length == 0)
                throw new ArgumentException(SR.GetString(SR.NeedSourceToWrite));

            string currentMachineName = machineName;
            if (!boolFlags[Flag_writeGranted]) {
                EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Write, currentMachineName);
                permission.Demand();
                boolFlags[Flag_writeGranted] = true;
            }

            VerifyAndCreateSource(Source, currentMachineName);

            string[] strings = null;
            
            if (values != null) {
                strings = new string[values.Length];
                for (int i=0; i<values.Length; i++) {
                    if (values[i] != null)
                        strings[i] = values[i].ToString();
                    else
                        strings[i] = String.Empty;
                }
            }
            
            InternalWriteEvent((uint) instance.InstanceId, (ushort) instance.CategoryId, instance.EntryType, strings, data, currentMachineName);
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private void InternalWriteEvent(uint eventID, ushort category, EventLogEntryType type, string[] strings,
                                byte[] rawData, string currentMachineName) {

            // check arguments
            if (strings == null)
                strings = new string[0];
            if (strings.Length >= 256)
                throw new ArgumentException(SR.GetString(SR.TooManyReplacementStrings));
            
            for (int i = 0; i < strings.Length; i++) {
                if (strings[i] == null)
                    strings[i] = String.Empty;

                // make sure the strings aren't too long.  MSDN says each string has a limit of 32k (32768) characters, but 
                // experimentation shows that it doesn't like anything larger than 32766
                if (strings[i].Length > 32766)
                    throw new ArgumentException(SR.GetString(SR.LogEntryTooLong));
            }
            if (rawData == null)
                rawData = new byte[0];

            if (Source.Length == 0)
                throw new ArgumentException(SR.GetString(SR.NeedSourceToWrite));

            if (!IsOpenForWrite)
                OpenForWrite(currentMachineName);

            // pin each of the strings in memory
            IntPtr[] stringRoots = new IntPtr[strings.Length];
            GCHandle[] stringHandles = new GCHandle[strings.Length];
            GCHandle stringsRootHandle = GCHandle.Alloc(stringRoots, GCHandleType.Pinned);
            try {
                for (int strIndex = 0; strIndex < strings.Length; strIndex++) {
                    stringHandles[strIndex] = GCHandle.Alloc(strings[strIndex], GCHandleType.Pinned);
                    stringRoots[strIndex] = stringHandles[strIndex].AddrOfPinnedObject();
                }

                byte[] sid = null;
                // actually report the event
                bool success = UnsafeNativeMethods.ReportEvent(writeHandle, (short) type, category, eventID,
                                                     sid, (short) strings.Length, rawData.Length, new HandleRef(this, stringsRootHandle.AddrOfPinnedObject()), rawData);
                if (!success) {
                    //Trace("WriteEvent", "Throwing Win32Exception");
                    throw SharedUtils.CreateSafeWin32Exception();
                }
            }
            finally {
                // now free the pinned strings
                for (int i = 0; i < strings.Length; i++) {
                    if (stringHandles[i].IsAllocated)
                        stringHandles[i].Free();
                }
                stringsRootHandle.Free();
            }
        }

        private class LogListeningInfo {
            public EventLogInternal handleOwner;
            public RegisteredWaitHandle registeredWaitHandle;
            public WaitHandle waitHandle;
            public ArrayList listeningComponents = new ArrayList();
        }

    }

}
