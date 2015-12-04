//------------------------------------------------------------------------------
// <copyright file="EventLog.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

//#define RETRY_ON_ALL_ERRORS

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
    [
    DefaultEvent("EntryWritten"),
    InstallerType("System.Diagnostics.EventLogInstaller, " + AssemblyRef.SystemConfigurationInstall),
    MonitoringDescription(SR.EventLogDesc)
    ]
    public class EventLog : Component, ISupportInitialize {

        private const string EventLogKey = "SYSTEM\\CurrentControlSet\\Services\\EventLog";
        internal const string DllName = "EventLogMessages.dll";
        private const string eventLogMutexName = "netfxeventlog.1.0";

        private const int DefaultMaxSize = 512 * 1024;
        private const int DefaultRetention = 7 * SecondsPerDay;
        private const int SecondsPerDay = 60 * 60 * 24;

        private EventLogInternal m_underlyingEventLog;
        
        // Whether we need backward compatible OS patch work or not
        private static volatile bool s_CheckedOsVersion; 
        private static volatile bool s_SkipRegPatch;

        private static bool SkipRegPatch {
            get {
                if (!s_CheckedOsVersion) {
                    OperatingSystem os = Environment.OSVersion;
                    s_SkipRegPatch = (os.Platform == PlatformID.Win32NT) && (os.Version.Major > 5);
                    s_CheckedOsVersion = true;
                }
                return s_SkipRegPatch;
            }
        }

        internal static PermissionSet _UnsafeGetAssertPermSet() {
            // SEC_NOTE: All callers should already be guarded by EventLogPermission demand.
            PermissionSet permissionSet = new PermissionSet(PermissionState.None);

            // We need RegistryPermission 
            RegistryPermission registryPermission = new RegistryPermission(PermissionState.Unrestricted);
            permissionSet.AddPermission(registryPermission);

            // It is not enough to just assert RegistryPermission, for some regkeys
            // we need to assert EnvironmentPermission too
            EnvironmentPermission environmentPermission = new EnvironmentPermission(PermissionState.Unrestricted);
            permissionSet.AddPermission(environmentPermission);

            // For remote machine registry access UnmanagdCodePermission is required.
            SecurityPermission securityPermission = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
            permissionSet.AddPermission(securityPermission);

            return permissionSet;
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Diagnostics.EventLog'/>
        ///       class.
        ///    </para>
        /// </devdoc>
        public EventLog() : this("", ".", "") {
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public EventLog(string logName) : this(logName, ".", "") {
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public EventLog(string logName, string machineName) : this(logName, machineName, "") {
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public EventLog(string logName, string machineName, string source) {
            m_underlyingEventLog = new EventLogInternal(logName, machineName, source, this);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the contents of the event log.
        ///    </para>
        /// </devdoc>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription(SR.LogEntries)]
        public EventLogEntryCollection Entries {
            get {
                return m_underlyingEventLog.Entries;
            }
        }

        /// <devdoc>
        ///    <para>
        ///    </para>
        /// </devdoc>
        [Browsable(false)]
        public string LogDisplayName {
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
            get {
                return m_underlyingEventLog.LogDisplayName;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the name of the log to read from and write to.
        ///    </para>
        /// </devdoc>
        [TypeConverter("System.Diagnostics.Design.LogConverter, " + AssemblyRef.SystemDesign)]
        [ReadOnly(true)]
        [MonitoringDescription(SR.LogLog)]
        [DefaultValue("")]
        [SettingsBindable(true)]
        [SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity", Justification = "[....]: Safe, oldLog.machineName doesn't change")]
        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification = "[....]: By design, see justification above assert")]
        public string Log {
            get {
                return m_underlyingEventLog.Log;
            }
            set {
                EventLogInternal newLog = new EventLogInternal(value, m_underlyingEventLog.MachineName, m_underlyingEventLog.Source, this);
                EventLogInternal oldLog = m_underlyingEventLog;

                // EnableRaisingEvents and Close demand Write permission but that permission might be removed upstack
                // previously we didn't call Close() since we were reusing the same object.  We assert the permission here.
                new EventLogPermission(EventLogPermissionAccess.Write, oldLog.machineName).Assert();
                if (oldLog.EnableRaisingEvents) {
                    newLog.onEntryWrittenHandler = oldLog.onEntryWrittenHandler;
                    newLog.EnableRaisingEvents = true;
                }
                m_underlyingEventLog = newLog;
                oldLog.Close();
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the name of the computer on which to read or write events.
        ///    </para>
        /// </devdoc>
        [ReadOnly(true)]
        [MonitoringDescription(SR.LogMachineName)]
        [DefaultValue(".")]
        [SettingsBindable(true)]
        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification = "[....]: By design, see justification above assert")]
        public string MachineName {
            get {
                return m_underlyingEventLog.MachineName;
            }
            set {
                EventLogInternal newLog = new EventLogInternal(m_underlyingEventLog.logName, value, m_underlyingEventLog.sourceName, this);
                EventLogInternal oldLog = m_underlyingEventLog;

                // EnableRaisingEvents and Close demand Write permission but that permission might be removed upstack
                // previously we didn't call Close() since we were reusing the same object.  We assert the permission here.
                new EventLogPermission(EventLogPermissionAccess.Write, oldLog.machineName).Assert();
                if (oldLog.EnableRaisingEvents) {
                    newLog.onEntryWrittenHandler = oldLog.onEntryWrittenHandler;
                    newLog.EnableRaisingEvents = true;
                }
                m_underlyingEventLog = newLog;
                oldLog.Close();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [ComVisible(false)]
        public long MaximumKilobytes {
            get {
                return m_underlyingEventLog.MaximumKilobytes;
            }

            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
            set {
                m_underlyingEventLog.MaximumKilobytes = value;
            }
        }

        [Browsable(false)]
        [ComVisible(false)]
        public OverflowAction OverflowAction {
            get {
                return m_underlyingEventLog.OverflowAction;
            }
        }

        [Browsable(false)]
        [ComVisible(false)]
        public int MinimumRetentionDays {
            get {
                return m_underlyingEventLog.MinimumRetentionDays;
            }
        }

        // EventLogInternal needs to know if the component is in design mode but
        // the DesignMode property is protected.
        internal bool ComponentDesignMode {
            get {
                return this.DesignMode;
            }
        }

        // Expose for EventLogInternal
        internal object ComponentGetService(Type service) {
            return GetService(service);
        }

        /// <devdoc>
        /// </devdoc>
        [Browsable(false)]
        [MonitoringDescription(SR.LogMonitoring)]
        [DefaultValue(false)]
        public bool EnableRaisingEvents {
            get {
                return m_underlyingEventLog.EnableRaisingEvents;
            }
            set {
                m_underlyingEventLog.EnableRaisingEvents = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Represents the object used to marshal the event handler
        ///       calls issued as a result of an <see cref='System.Diagnostics.EventLog'/>
        ///       change.
        ///    </para>
        /// </devdoc>
        [Browsable(false)]
        [DefaultValue(null)]
        [MonitoringDescription(SR.LogSynchronizingObject)]
        public ISynchronizeInvoke SynchronizingObject {
        [HostProtection(Synchronization=true)]
            get {
                return m_underlyingEventLog.SynchronizingObject;
            }

            set {
                m_underlyingEventLog.SynchronizingObject = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or
        ///       sets the application name (source name) to register and use when writing to the event log.
        ///    </para>
        /// </devdoc>
        [ReadOnly(true)]
        [TypeConverter("System.Diagnostics.Design.StringValueConverter, " + AssemblyRef.SystemDesign)]
        [MonitoringDescription(SR.LogSource)]
        [DefaultValue("")]
        [SettingsBindable(true)]
        [SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity", Justification = "[....]: Safe, oldLog.machineName doesn't change")]
        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification = "[....]: By design, see justification above assert")]
        public string Source {
            get {
                return m_underlyingEventLog.Source;
            }
            set {
                EventLogInternal newLog = new EventLogInternal(m_underlyingEventLog.Log, m_underlyingEventLog.MachineName, CheckAndNormalizeSourceName(value), this);
                EventLogInternal oldLog = m_underlyingEventLog;

                // EnableRaisingEvents and Close demand Write permission but that permission might be removed upstack
                // previously we didn't call Close() since we were reusing the same object.  We assert the permission here.
                new EventLogPermission(EventLogPermissionAccess.Write, oldLog.machineName).Assert();
                if (oldLog.EnableRaisingEvents) {
                    newLog.onEntryWrittenHandler = oldLog.onEntryWrittenHandler;
                    newLog.EnableRaisingEvents = true;
                }
                m_underlyingEventLog = newLog;
                oldLog.Close();
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Occurs when an entry is written to the event log.
        ///    </para>
        /// </devdoc>
        [MonitoringDescription(SR.LogEntryWritten)]
        public event EntryWrittenEventHandler EntryWritten {
            add {
                m_underlyingEventLog.EntryWritten += value;
            }
            remove {
                m_underlyingEventLog.EntryWritten -= value;
            }
        }

        /// <devdoc>
        /// </devdoc>
        public void BeginInit() {
            m_underlyingEventLog.BeginInit();
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
            m_underlyingEventLog.Clear();
        }

        /// <devdoc>
        ///    <para>
        ///       Closes the event log and releases read and write handles.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        public void Close() {
            m_underlyingEventLog.Close();
        }

        /// <devdoc>
        ///    <para> Establishes an application, using the
        ///       specified <see cref='System.Diagnostics.EventLog.Source'/> , as a valid event source for
        ///       writing entries
        ///       to a log on the local computer. This method
        ///       can also be used to create
        ///       a new custom log on the local computer.</para>
        /// </devdoc>
        public static void CreateEventSource(string source, string logName) {
            CreateEventSource(new EventSourceCreationData(source, logName, "."));
        }

        /// <devdoc>
        ///    <para>Establishes an application, using the specified
        ///    <see cref='System.Diagnostics.EventLog.Source'/> as a valid event source for writing
        ///       entries to a log on the computer
        ///       specified by <paramref name="machineName"/>. This method can also be used to create a new
        ///       custom log on the given computer.</para>
        /// </devdoc>
        [Obsolete("This method has been deprecated.  Please use System.Diagnostics.EventLog.CreateEventSource(EventSourceCreationData sourceData) instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        public static void CreateEventSource(string source, string logName, string machineName) {
            CreateEventSource(new EventSourceCreationData(source, logName, machineName));
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public static void CreateEventSource(EventSourceCreationData sourceData) {
            if (sourceData == null)
                throw new ArgumentNullException("sourceData");

            string logName = sourceData.LogName;
            string source = sourceData.Source;
            string machineName = sourceData.MachineName;

            // verify parameters
            Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "CreateEventSource: Checking arguments");
            if (!SyntaxCheck.CheckMachineName(machineName)) {
                throw new ArgumentException(SR.GetString(SR.InvalidParameter, "machineName", machineName));
            }
            if (logName == null || logName.Length==0)
                logName = "Application";
            if (!ValidLogName(logName, false))
                throw new ArgumentException(SR.GetString(SR.BadLogName));
            if (source == null || source.Length==0)
                throw new ArgumentException(SR.GetString(SR.MissingParameter, "source"));
            if (source.Length + EventLogKey.Length > 254)
                throw new ArgumentException(SR.GetString(SR.ParameterTooLong, "source", 254 - EventLogKey.Length));

            EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Administer, machineName);
            permission.Demand();

            Mutex mutex = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                SharedUtils.EnterMutex(eventLogMutexName, ref mutex);
                Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "CreateEventSource: Calling SourceExists");
                if (SourceExists(source, machineName, true)) {
                    Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "CreateEventSource: SourceExists returned true");
                    // don't let them register a source if it already exists
                    // this makes more sense than just doing it anyway, because the source might
                    // be registered under a different log name, and we don't want to create
                    // duplicates.
                    if (".".Equals(machineName))
                        throw new ArgumentException(SR.GetString(SR.LocalSourceAlreadyExists, source));
                    else
                        throw new ArgumentException(SR.GetString(SR.SourceAlreadyExists, source, machineName));
                }

                Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "CreateEventSource: Getting DllPath");

                //SECREVIEW: Note that EventLog permission is demanded above.
                PermissionSet permissionSet = _UnsafeGetAssertPermSet();
                permissionSet.Assert();
                
                RegistryKey baseKey = null;
                RegistryKey eventKey = null;
                RegistryKey logKey = null;
                RegistryKey sourceLogKey = null;
                RegistryKey sourceKey = null;
                try {
                    Debug.WriteLineIf(CompModSwitches.EventLog.TraceVerbose, "CreateEventSource: Getting local machine regkey");
                    if (machineName == ".")
                        baseKey = Registry.LocalMachine;
                    else
                        baseKey = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, machineName);

                    eventKey = baseKey.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\EventLog", true);
                    if (eventKey == null) {
                        if (!".".Equals(machineName))
                            throw new InvalidOperationException(SR.GetString(SR.RegKeyMissing, "SYSTEM\\CurrentControlSet\\Services\\EventLog", logName, source, machineName));
                        else
                            throw new InvalidOperationException(SR.GetString(SR.LocalRegKeyMissing, "SYSTEM\\CurrentControlSet\\Services\\EventLog", logName, source));
                    }

                    // The event log system only treats the first 8 characters of the log name as
                    // significant. If they're creating a new log, but that new log has the same
                    // first 8 characters as another log, the system will think they're the same.
                    // Throw an exception to let them know.
                    logKey = eventKey.OpenSubKey(logName, true);
                    if (logKey == null && logName.Length >= 8) {

                        // check for Windows embedded logs file names
                        string logNameFirst8 = logName.Substring(0,8);
                        if ( string.Compare(logNameFirst8,"AppEvent",StringComparison.OrdinalIgnoreCase) ==0  ||
                             string.Compare(logNameFirst8,"SecEvent",StringComparison.OrdinalIgnoreCase) ==0  ||
                             string.Compare(logNameFirst8,"SysEvent",StringComparison.OrdinalIgnoreCase) ==0 )
                            throw new ArgumentException(SR.GetString(SR.InvalidCustomerLogName, logName));

                        string sameLogName = FindSame8FirstCharsLog(eventKey, logName);
                        if ( sameLogName != null )
                            throw new ArgumentException(SR.GetString(SR.DuplicateLogName, logName, sameLogName));
                    }

                    bool createLogKey = (logKey == null);
                    if (createLogKey) {
                        if (SourceExists(logName, machineName, true)) {
                            // don't let them register a log name that already
                            // exists as source name, a source with the same
                            // name as the log will have to be created by default
                            if (".".Equals(machineName))
                                throw new ArgumentException(SR.GetString(SR.LocalLogAlreadyExistsAsSource, logName));
                            else
                                throw new ArgumentException(SR.GetString(SR.LogAlreadyExistsAsSource, logName, machineName));
                        }

                        logKey = eventKey.CreateSubKey(logName);
                                                                        
                        // NOTE: We shouldn't set "Sources" explicitly, the OS will automatically set it.
                        // The EventLog service doesn't use it for anything it is just an helping hand for event viewer filters.
                        // Writing this value explicitly might confuse the service as it might perceive it as a change and 
                        // start initializing again

                        if (!SkipRegPatch) 
                            logKey.SetValue("Sources", new string[] {logName, source}, RegistryValueKind.MultiString);

                        SetSpecialLogRegValues(logKey, logName);

                        // A source with the same name as the log has to be created
                        // by default. It is the behavior expected by EventLog API.
                        sourceLogKey = logKey.CreateSubKey(logName);
                        SetSpecialSourceRegValues(sourceLogKey, sourceData);
                    }

                    if (logName != source) {
                        if (!createLogKey) {
                            SetSpecialLogRegValues(logKey, logName);
                                                        
                            if (!SkipRegPatch) {
                                string[] sources = logKey.GetValue("Sources") as string[];
                                if (sources == null)
                                    logKey.SetValue("Sources", new string[] {logName, source}, RegistryValueKind.MultiString);
                                else {
                                    // We have a ---- with OS EventLog here.
                                    // OS might update Sources as well. We should avoid writing the 
                                    // source name if OS beats us.
                                    if( Array.IndexOf(sources, source) == -1) {
                                        string[] newsources = new string[sources.Length + 1];
                                        Array.Copy(sources, newsources, sources.Length);
                                        newsources[sources.Length] = source;
                                        logKey.SetValue("Sources", newsources, RegistryValueKind.MultiString);
                                    }
                                }
                            }
                        }

                        sourceKey = logKey.CreateSubKey(source);
                        SetSpecialSourceRegValues(sourceKey, sourceData);
                    }
                }
                finally {
                    if (baseKey != null) 
                        baseKey.Close();

                    if (eventKey != null)
                        eventKey.Close();

                    if (logKey != null) {
                        logKey.Flush();
                        logKey.Close();
                    }

                    if (sourceLogKey != null) {
                        sourceLogKey.Flush();
                        sourceLogKey.Close();
                    }

                    if (sourceKey != null) {
                        sourceKey.Flush();
                        sourceKey.Close();
                    }

                    // Revert registry and environment permission asserts
                    CodeAccessPermission.RevertAssert();
                }
            }
            finally {
                if (mutex != null) {
                    mutex.ReleaseMutex();
                    mutex.Close();
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Removes
        ///       an event
        ///       log from the local computer.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]   // See why someone would delete an event log
        [ResourceConsumption(ResourceScope.Machine)]
        public static void Delete(string logName) {
            Delete(logName, ".");
        }

        /// <devdoc>
        ///    <para>
        ///       Removes
        ///       an
        ///       event
        ///       log from the specified computer.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]   // See why someone would delete an event log
        [ResourceConsumption(ResourceScope.Machine)]
        public static void Delete(string logName, string machineName) {

            if (!SyntaxCheck.CheckMachineName(machineName))
                throw new ArgumentException(SR.GetString(SR.InvalidParameterFormat, "machineName"));
            if (logName == null || logName.Length==0)
                throw new ArgumentException(SR.GetString(SR.NoLogName));
            if (!ValidLogName(logName, false))
                throw new InvalidOperationException(SR.GetString(SR.BadLogName));

            EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Administer, machineName);
            permission.Demand();

            //Check environment before even trying to play with the registry
            SharedUtils.CheckEnvironment();

            //SECREVIEW: Note that EventLog permission is demanded above.
            PermissionSet permissionSet = _UnsafeGetAssertPermSet();
            permissionSet.Assert();
            
            RegistryKey eventlogkey = null;

            Mutex mutex = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                SharedUtils.EnterMutex(eventLogMutexName, ref mutex);

                try {
                    eventlogkey  = GetEventLogRegKey(machineName, true);
                    if (eventlogkey  == null) {
                        // there's not even an event log service on the machine.
                        // or, more likely, we don't have the access to read the registry.
                        throw new InvalidOperationException(SR.GetString(SR.RegKeyNoAccess, "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\EventLog", machineName));
                    }

                    using (RegistryKey logKey = eventlogkey.OpenSubKey(logName)) {
                        if (logKey == null)
                            throw new InvalidOperationException(SR.GetString(SR.MissingLog, logName, machineName));

                        //clear out log before trying to delete it
                        //that way, if we can't delete the log file, no entries will persist because it has been cleared
                        EventLog logToClear = new EventLog(logName, machineName);
                        try {
                            logToClear.Clear();
                        }
                        finally {
                            logToClear.Close();
                        }

                        // 


                        string filename = null;
                        try {
                            //most of the time, the "File" key does not exist, but we'll still give it a whirl
                            filename = (string) logKey.GetValue("File");
                        }
                        catch { }
                        if (filename != null) {
                            try {
                                File.Delete(filename);
                            }
                            catch { }
                        }
                    }

                    // now delete the registry entry
                    eventlogkey.DeleteSubKeyTree(logName);
                }
                finally {
                    if (eventlogkey != null) eventlogkey.Close();
                
                    // Revert registry and environment permission asserts
                    CodeAccessPermission.RevertAssert();
                }
            }
            finally {
                if (mutex != null) mutex.ReleaseMutex();
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Removes the event source
        ///       registration from the event log of the local computer.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public static void DeleteEventSource(string source) {
            DeleteEventSource(source, ".");
        }

        /// <devdoc>
        ///    <para>
        ///       Removes
        ///       the application's event source registration from the specified computer.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static void DeleteEventSource(string source, string machineName) {
            if (!SyntaxCheck.CheckMachineName(machineName)) {
                throw new ArgumentException(SR.GetString(SR.InvalidParameter, "machineName", machineName));
            }

            EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Administer, machineName);
            permission.Demand();

            //Check environment before looking at the registry
            SharedUtils.CheckEnvironment();

            //SECREVIEW: Note that EventLog permission is demanded above.
            PermissionSet permissionSet = _UnsafeGetAssertPermSet();
            permissionSet.Assert();
            
            Mutex mutex = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                SharedUtils.EnterMutex(eventLogMutexName, ref mutex);
                RegistryKey key = null;

                // First open the key read only so we can do some checks.  This is important so we get the same 
                // exceptions even if we don't have write access to the reg key. 
                using (key = FindSourceRegistration(source, machineName, true)) {
                    if (key == null) {
                        if (machineName == null)
                            throw new ArgumentException(SR.GetString(SR.LocalSourceNotRegistered, source));
                        else
                            throw new ArgumentException(SR.GetString(SR.SourceNotRegistered, source, machineName, "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\EventLog"));
                    }
            
                    // Check parent registry key (Event Log Name) and if it's equal to source, then throw an exception.
                    // The reason: each log registry key must always contain subkey (i.e. source) with the same name.
                    string keyname = key.Name;
                    int index = keyname.LastIndexOf('\\');
                    if ( string.Compare(keyname, index+1, source, 0, keyname.Length - index, StringComparison.Ordinal) == 0 )
                        throw new InvalidOperationException(SR.GetString(SR.CannotDeleteEqualSource, source));
                }

                try {
                    // now open it read/write to try to do the actual delete
                    key = FindSourceRegistration(source, machineName, false);
                    key.DeleteSubKeyTree(source);
                                        
                    if (!SkipRegPatch) { 
                        string[] sources = (string[]) key.GetValue("Sources");
                        ArrayList newsources = new ArrayList(sources.Length - 1);

                        for (int i=0; i<sources.Length; i++) {
                            if (sources[i] != source) {
                                newsources.Add(sources[i]);
                            }
                        }
                        string[] newsourcesArray = new string[newsources.Count];
                        newsources.CopyTo(newsourcesArray);

                        key.SetValue("Sources", newsourcesArray, RegistryValueKind.MultiString);
                    }
                }
                finally {
                    if (key != null) {
                        key.Flush();
                        key.Close();
                    }
                
                    // Revert registry and environment permission asserts
                    CodeAccessPermission.RevertAssert();
                }
            }
            finally {
                if (mutex != null)
                    mutex.ReleaseMutex();
            }
        }

        /// <devdoc>
        /// </devdoc>
        protected override void Dispose(bool disposing) {
            if(m_underlyingEventLog != null) {
                m_underlyingEventLog.Dispose(disposing);
            }

            base.Dispose(disposing);
        }

        /// <devdoc>
        /// </devdoc>
        public void EndInit() {
            m_underlyingEventLog.EndInit();
        }

        /// <devdoc>
        ///    <para>
        ///       Determines whether the log
        ///       exists on the local computer.
        ///    </para>
        /// </devdoc>
        public static bool Exists(string logName) {
            return Exists(logName, ".");
        }

        /// <devdoc>
        ///    <para>
        ///       Determines whether the
        ///       log exists on the specified computer.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public static bool Exists(string logName, string machineName) {
            if (!SyntaxCheck.CheckMachineName(machineName))
                throw new ArgumentException(SR.GetString(SR.InvalidParameterFormat, "machineName"));

            EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Administer, machineName);
            permission.Demand();

            if (logName == null || logName.Length==0)
                return false;

            //Check environment before looking at the registry
            SharedUtils.CheckEnvironment();

            //SECREVIEW: Note that EventLog permission is demanded above.
            PermissionSet permissionSet = _UnsafeGetAssertPermSet();
            permissionSet.Assert();
            
            RegistryKey eventkey = null;
            RegistryKey logKey = null;

            try {
                eventkey = GetEventLogRegKey(machineName, false);
                if (eventkey == null)
                    return false;

                logKey = eventkey.OpenSubKey(logName, false);         // try to find log file key immediately.
                return (logKey != null );
            }
            finally {
                if (eventkey != null) eventkey.Close();
                if (logKey != null) logKey.Close();
                
                // Revert registry and environment permission asserts
                CodeAccessPermission.RevertAssert();
            }
        }


        // Try to find log file name with the same 8 first characters.
        // Returns 'null' if no "same first 8 chars" log is found.   logName.Length must be > 7
        private static string FindSame8FirstCharsLog(RegistryKey keyParent, string logName) {

            string logNameFirst8 = logName.Substring(0, 8);
            string[] logNames = keyParent.GetSubKeyNames();

            for (int i = 0; i < logNames.Length; i++) {
                string currentLogName = logNames[i];
                if ( currentLogName.Length >= 8  &&
                     string.Compare(currentLogName.Substring(0, 8), logNameFirst8, StringComparison.OrdinalIgnoreCase) == 0)
                    return currentLogName;
            }

            return null;   // not found
        }

        /// <devdoc>
        ///     Gets a RegistryKey that points to the LogName entry in the registry that is
        ///     the parent of the given source on the given machine, or null if none is found.
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private static RegistryKey FindSourceRegistration(string source, string machineName, bool readOnly) {
            return FindSourceRegistration(source, machineName, readOnly, false);
        }

        /// <devdoc>
        ///     Gets a RegistryKey that points to the LogName entry in the registry that is
        ///     the parent of the given source on the given machine, or null if none is found.
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private static RegistryKey FindSourceRegistration(string source, string machineName, bool readOnly, bool wantToCreate) {
            if (source != null && source.Length != 0) {

                //Check environment before looking at the registry
                SharedUtils.CheckEnvironment();

                //SECREVIEW: Any call to this function must have demmanded
                //                         EventLogPermission before.
                PermissionSet permissionSet = _UnsafeGetAssertPermSet();
                permissionSet.Assert();
                
                RegistryKey eventkey = null;
                try {
                    eventkey = GetEventLogRegKey(machineName, !readOnly);
                    if (eventkey == null) {
                        // there's not even an event log service on the machine.
                        // or, more likely, we don't have the access to read the registry.
                        return null;
                    }

                    StringBuilder inaccessibleLogs = null;
                    
                    // Most machines will return only { "Application", "System", "Security" },
                    // but you can create your own if you want.
                    string[] logNames = eventkey.GetSubKeyNames();
                    for (int i = 0; i < logNames.Length; i++) {
                        // see if the source is registered in this log.
                        // NOTE: A source name must be unique across ALL LOGS!
                        RegistryKey sourceKey = null;
                        try {
                            RegistryKey logKey = eventkey.OpenSubKey(logNames[i], /*writable*/!readOnly);
                            if (logKey != null) {
                                sourceKey = logKey.OpenSubKey(source, /*writable*/!readOnly);
                                if (sourceKey != null) {
                                    // found it
                                    return logKey;
                                } else {
                                    logKey.Close();
                                }
                            }
                            // else logKey is null, so we don't need to Close it
                        }
                        catch (UnauthorizedAccessException) {
                            if (inaccessibleLogs == null) {
                                inaccessibleLogs = new StringBuilder(logNames[i]);
                            }
                            else {
                                inaccessibleLogs.Append(", ");
                                inaccessibleLogs.Append(logNames[i]);
                            }
                        }
                        catch (SecurityException) {
                            if (inaccessibleLogs == null) {
                                inaccessibleLogs = new StringBuilder(logNames[i]);
                            }
                            else {
                                inaccessibleLogs.Append(", ");
                                inaccessibleLogs.Append(logNames[i]);
                            }
                        }
                        finally {
                            if (sourceKey != null) sourceKey.Close();
                        }
                    }

                    if (inaccessibleLogs != null)
                        throw new SecurityException(SR.GetString(wantToCreate ? SR.SomeLogsInaccessibleToCreate : SR.SomeLogsInaccessible, inaccessibleLogs.ToString()));
                    
                }
                finally {
                    if (eventkey != null) eventkey.Close();
                    
                    // Revert registry and environment permission asserts
                    CodeAccessPermission.RevertAssert();
                }
                // didn't see it anywhere
            }

            return null;
        }

        /// <devdoc>
        ///    <para>
        ///       Searches for all event logs on the local computer and
        ///       creates an array of <see cref='System.Diagnostics.EventLog'/>
        ///       objects to contain the
        ///       list.
        ///    </para>
        /// </devdoc>
        public static EventLog[] GetEventLogs() {
            return GetEventLogs(".");
        }

        /// <devdoc>
        ///    <para>
        ///       Searches for all event logs on the given computer and
        ///       creates an array of <see cref='System.Diagnostics.EventLog'/>
        ///       objects to contain the
        ///       list.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public static EventLog[] GetEventLogs(string machineName) {
            if (!SyntaxCheck.CheckMachineName(machineName)) {
                throw new ArgumentException(SR.GetString(SR.InvalidParameter, "machineName", machineName));
            }

            EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Administer, machineName);
            permission.Demand();

            //Check environment before looking at the registry
            SharedUtils.CheckEnvironment();

            string[] logNames = new string[0];
            //SECREVIEW: Note that EventLogPermission is just demmanded above
            PermissionSet permissionSet = _UnsafeGetAssertPermSet();
            permissionSet.Assert();
            
            RegistryKey eventkey = null;
            try {
                // we figure out what logs are on the machine by looking in the registry.
                eventkey = GetEventLogRegKey(machineName, false);
                if (eventkey == null)
                    // there's not even an event log service on the machine.
                    // or, more likely, we don't have the access to read the registry.
                    throw new InvalidOperationException(SR.GetString(SR.RegKeyMissingShort, EventLogKey, machineName));
                // Most machines will return only { "Application", "System", "Security" },
                // but you can create your own if you want.
                logNames = eventkey.GetSubKeyNames();
            }
            finally {
                if (eventkey != null) eventkey.Close();
                // Revert registry and environment permission asserts
                CodeAccessPermission.RevertAssert();
            }

            // now create EventLog objects that point to those logs
            EventLog[] logs = new EventLog[logNames.Length];
            for (int i = 0; i < logNames.Length; i++) {
                EventLog log = new EventLog(logNames[i], machineName);
                logs[i] = log;
            }

            return logs;
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
        internal static string GetDllPath(string machineName) {
            return Path.Combine(SharedUtils.GetLatestBuildDllDirectory(machineName), DllName);
        }

        /// <devdoc>
        ///    <para>
        ///       Determines whether an event source is registered on the local computer.
        ///    </para>
        /// </devdoc>
        public static bool SourceExists(string source) {
            return SourceExists(source, ".");
        }

        /// <devdoc>
        ///    <para>
        ///       Determines whether an event
        ///       source is registered on a specified computer.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public static bool SourceExists(string source, string machineName) {
            return SourceExists(source, machineName, false);
        }

        /// <devdoc>
        ///    <para>
        ///       Determines whether an event
        ///       source is registered on a specified computer.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity", Justification = "[....]: Safe, machineName doesn't change")]
        internal static bool SourceExists(string source, string machineName, bool wantToCreate) {
            if (!SyntaxCheck.CheckMachineName(machineName)) {
                throw new ArgumentException(SR.GetString(SR.InvalidParameter, "machineName", machineName));
            }

            EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Write, machineName);
            permission.Demand();

            using (RegistryKey keyFound = FindSourceRegistration(source, machineName, true, wantToCreate)) {
                return (keyFound != null);
            }
        }

        /// <devdoc>
        ///     Gets the name of the log that the given source name is registered in.
        /// </devdoc>
        public static string LogNameFromSourceName(string source, string machineName) {
            EventLogPermission permission = new EventLogPermission(EventLogPermissionAccess.Administer, machineName);
            permission.Demand();

            return _InternalLogNameFromSourceName(source, machineName);
        }

        // No permission check, use with care!
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        internal static string _InternalLogNameFromSourceName(string source, string machineName) {
            using (RegistryKey key = FindSourceRegistration(source, machineName, true)) {
                if (key == null)
                    return "";
                else {
                    string name = key.Name;
                    int whackPos = name.LastIndexOf('\\');
                    // this will work even if whackPos is -1
                    return name.Substring(whackPos+1);
                }
            }
        }


        [ComVisible(false)]
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public void ModifyOverflowPolicy(OverflowAction action, int retentionDays) {
            m_underlyingEventLog.ModifyOverflowPolicy(action, retentionDays);
        }

        [ComVisible(false)]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public void RegisterDisplayName(string resourceFile, long resourceId) {
            m_underlyingEventLog.RegisterDisplayName(resourceFile, resourceId);
        }

        // The reasoning behind filling these values is historical. WS03 RTM had a ----
        // between registry changes and EventLog service, which made the service wait 2 secs
        // before retrying to see whether all regkey values are present. To avoid this 
        // potential lag (worst case up to n*2 secs where n is the number of required regkeys) 
        // between creation and being able to write events, we started filling some of these
        // values explicitly but for XP and latter OS releases like WS03 SP1 and Vista this 
        // is not necessary and in some cases like the "File" key it's plain wrong to write. 
        private static void SetSpecialLogRegValues(RegistryKey logKey, string logName) {
            // Set all the default values for this log.  AutoBackupLogfiles only makes sense in 
            // Win2000 SP4, WinXP SP1, and Win2003, but it should alright elsewhere. 

            // Since we use this method on the existing system logs as well as our own,
            // we need to make sure we don't overwrite any existing values. 
            if (logKey.GetValue("MaxSize") == null)
                logKey.SetValue("MaxSize", DefaultMaxSize, RegistryValueKind.DWord);
            if (logKey.GetValue("AutoBackupLogFiles") == null)
                logKey.SetValue("AutoBackupLogFiles", 0, RegistryValueKind.DWord);

            if (!SkipRegPatch) { 
                // In Vista, "retention of events for 'n' days" concept is removed
                if (logKey.GetValue("Retention") == null)
                    logKey.SetValue("Retention", DefaultRetention, RegistryValueKind.DWord);
                
                if (logKey.GetValue("File") == null) { 
                    string filename;
                    if (logName.Length > 8)
                        filename = @"%SystemRoot%\System32\config\" + logName.Substring(0,8) + ".evt";
                    else
                        filename = @"%SystemRoot%\System32\config\" + logName + ".evt";

                    logKey.SetValue("File", filename, RegistryValueKind.ExpandString);
                }
            }
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private static void SetSpecialSourceRegValues(RegistryKey sourceLogKey, EventSourceCreationData sourceData) {
            if (String.IsNullOrEmpty(sourceData.MessageResourceFile))
                sourceLogKey.SetValue("EventMessageFile", GetDllPath(sourceData.MachineName), RegistryValueKind.ExpandString);
            else 
                sourceLogKey.SetValue("EventMessageFile", FixupPath(sourceData.MessageResourceFile), RegistryValueKind.ExpandString);

            if (!String.IsNullOrEmpty(sourceData.ParameterResourceFile))
                sourceLogKey.SetValue("ParameterMessageFile", FixupPath(sourceData.ParameterResourceFile), RegistryValueKind.ExpandString);

            if (!String.IsNullOrEmpty(sourceData.CategoryResourceFile)) {
                sourceLogKey.SetValue("CategoryMessageFile", FixupPath(sourceData.CategoryResourceFile), RegistryValueKind.ExpandString);
                sourceLogKey.SetValue("CategoryCount", sourceData.CategoryCount, RegistryValueKind.DWord);
            }
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private static string FixupPath(string path) {
            if (path[0] == '%')
                return path;
            else
                return Path.GetFullPath(path);
        }
        
        // Format message in specific DLL. Return <null> on failure.
        internal static string TryFormatMessage(SafeLibraryHandle hModule, uint messageNum, string[] insertionStrings) {

            if (insertionStrings.Length == 0) {
                // UnsafeTryFromatMessage will set FORMAT_MESSAGE_IGNORE_INSERTS when calling into the OS 
                // when there are no insertion strings, in this case we don't have to guard against insertionStrings
                // not having enough data since it is unused when FORMAT_MESSAGE_IGNORE_INSERTS is specified
                return UnsafeTryFormatMessage(hModule, messageNum, insertionStrings);
            }

            // If you pass in an empty array UnsafeTryFormatMessage will just pull out the message.
            string formatString = UnsafeTryFormatMessage(hModule, messageNum, new string[0]);

            if (formatString == null) {
                return null;
            }

            int largestNumber = 0;

            for (int i = 0; i < formatString.Length; i++) {
                if (formatString[i] == '%') {
                    // See if a number follows this, if so, grab the number.
                    if(formatString.Length > i + 1) {
                        StringBuilder sb = new StringBuilder();
                        while (i + 1 < formatString.Length && Char.IsDigit(formatString[i + 1])) {
                            sb.Append(formatString[i + 1]);
                            i++;
                        }

                        // move over the non number character that broke us out of the loop
                        i++;

                        if (sb.Length > 0) {
                            int num = -1;
                            if (Int32.TryParse(sb.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out num)) {
                                largestNumber = Math.Max(largestNumber, num);
                            }
                        }
                    }
                }
            }

            // Replacement strings are 1 indexed.
            if (largestNumber > insertionStrings.Length) {
                string[] newStrings = new string[largestNumber];
                Array.Copy(insertionStrings, newStrings, insertionStrings.Length);
                for (int i = insertionStrings.Length; i < newStrings.Length; i++) {
                    newStrings[i] = "%" + (i + 1);
                }

                insertionStrings = newStrings;
            }

            return UnsafeTryFormatMessage(hModule, messageNum, insertionStrings);
        }

        // FormatMessageW will AV if you don't pass in enough format strings.  If you call TryFormatMessage we ensure insertionStrings
        // is long enough.  You don't want to call this directly unless you're sure insertionStrings is long enough!
        internal static string UnsafeTryFormatMessage(SafeLibraryHandle hModule, uint messageNum, string[] insertionStrings) {
            string msg = null;

            int msgLen = 0;
            StringBuilder buf = new StringBuilder(1024);
            int flags = NativeMethods.FORMAT_MESSAGE_FROM_HMODULE | NativeMethods.FORMAT_MESSAGE_ARGUMENT_ARRAY;

            IntPtr[] addresses = new IntPtr[insertionStrings.Length];
            GCHandle[] handles = new GCHandle[insertionStrings.Length];
            GCHandle stringsRoot = GCHandle.Alloc(addresses, GCHandleType.Pinned);

            // Make sure that we don't try to pass in a zero length array of addresses.  If there are no insertion strings, 
            // we'll use the FORMAT_MESSAGE_IGNORE_INSERTS flag . 
            // If you change this behavior, make sure you look at TryFormatMessage which depends on this behavior!
            if (insertionStrings.Length == 0) {
                flags |= NativeMethods.FORMAT_MESSAGE_IGNORE_INSERTS;
            }
            
            try {
                for (int i=0; i<handles.Length; i++) {
                    handles[i] = GCHandle.Alloc(insertionStrings[i], GCHandleType.Pinned);
                    addresses[i] = handles[i].AddrOfPinnedObject();
                }
                int lastError = NativeMethods.ERROR_INSUFFICIENT_BUFFER;
                while (msgLen == 0 && lastError == NativeMethods.ERROR_INSUFFICIENT_BUFFER) {
                    msgLen = SafeNativeMethods.FormatMessage(
                        flags,
                        hModule,
                        messageNum,
                        0,
                        buf,
                        buf.Capacity,
                        addresses);

                    if (msgLen == 0) {
                        lastError = Marshal.GetLastWin32Error();
                        if (lastError == NativeMethods.ERROR_INSUFFICIENT_BUFFER)
                            buf.Capacity = buf.Capacity * 2;
                    }
                }
            }
            catch {
                msgLen = 0;              // return empty on failure
            }
            finally  {
                for (int i=0; i<handles.Length; i++) {
                    if (handles[i].IsAllocated) handles[i].Free();
                }
                stringsRoot.Free();
            }
            
            if (msgLen > 0) {
                msg = buf.ToString();
                // chop off a single CR/LF pair from the end if there is one. FormatMessage always appends one extra.
                if (msg.Length > 1 && msg[msg.Length-1] == '\n')
                    msg = msg.Substring(0, msg.Length-2);
            }

            return msg;
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

        /// <devdoc>
        ///    <para>
        ///       Writes an information type entry with the given message text to the event log.
        ///    </para>
        /// </devdoc>
        public void WriteEntry(string message) {
            WriteEntry(message, EventLogEntryType.Information, (short) 0, 0, null);
        }

        /// <devdoc>
        /// </devdoc>
        public static void WriteEntry(string source, string message) {
            WriteEntry(source, message, EventLogEntryType.Information, (short) 0, 0, null);
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
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static void WriteEntry(string source, string message, EventLogEntryType type) {
            WriteEntry(source, message, type, (short) 0, 0, null);
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
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static void WriteEntry(string source, string message, EventLogEntryType type, int eventID) {
            WriteEntry(source, message, type, eventID, 0, null);
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
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static void WriteEntry(string source, string message, EventLogEntryType type, int eventID, short category) {
            WriteEntry(source, message, type, eventID, category, null);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static void WriteEntry(string source, string message, EventLogEntryType type, int eventID, short category,
                               byte[] rawData) {
            using (EventLogInternal log = new EventLogInternal("", ".", CheckAndNormalizeSourceName(source))) {
                log.WriteEntry(message, type, eventID, category, rawData);
            }
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

            m_underlyingEventLog.WriteEntry(message, type, eventID, category, rawData);
        }

        [ComVisible(false)]
        public void WriteEvent(EventInstance instance, params Object[] values) {
            WriteEvent(instance, null, values);
        }

        [ComVisible(false)]
        public void WriteEvent(EventInstance instance, byte[] data, params Object[] values) {
            m_underlyingEventLog.WriteEvent(instance, data, values);
        }

        public static void WriteEvent(string source, EventInstance instance, params Object[] values) {
            using (EventLogInternal log = new EventLogInternal("", ".", CheckAndNormalizeSourceName(source))) {
                log.WriteEvent(instance, null, values);
            }
        }

        public static void WriteEvent(string source, EventInstance instance, byte[] data, params Object[] values) {
            using (EventLogInternal log = new EventLogInternal("", ".", CheckAndNormalizeSourceName(source))) {
                log.WriteEvent(instance, data, values);
            }
        }

        // The EventLog.set_Source used to do some normalization and throw some exceptions.  We mimic that behavior here.
        private static string CheckAndNormalizeSourceName(string source) {
            if (source == null)
                source = string.Empty;

            // this 254 limit is the max length of a registry key.
            if (source.Length + EventLogKey.Length > 254)
                throw new ArgumentException(SR.GetString(SR.ParameterTooLong, "source", 254 - EventLogKey.Length));

            return source;
        }
    }

}
