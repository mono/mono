//------------------------------------------------------------------------------
// <copyright file="TraceInternal.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {
    using System.Threading;
    using System.IO;
    using System.Security.Permissions;
    using System.Collections;

    internal static class TraceInternal {
        private static volatile string appName = null;
        static volatile TraceListenerCollection listeners;
        static volatile bool autoFlush;
        static volatile bool useGlobalLock;
        [ThreadStatic]
        static int indentLevel;
        static volatile int indentSize;
        static volatile bool settingsInitialized;
        static volatile bool defaultInitialized;


        // this is internal so TraceSource can use it.  We want to lock on the same object because both TraceInternal and 
        // TraceSource could be writing to the same listeners at the same time. 
        internal static readonly object critSec = new object();

        public static TraceListenerCollection Listeners { 
            get {
                InitializeSettings();
                if (listeners == null) {
                    lock (critSec) {
                        if (listeners == null) {
                            // We only need to check that the main section exists.  Everything else will get 
                            // created for us if it doesn't exist already. 
                            SystemDiagnosticsSection configSectionSav = DiagnosticsConfiguration.SystemDiagnosticsSection;
                            if (configSectionSav != null) {
                                listeners = configSectionSav.Trace.Listeners.GetRuntimeObject();
                            }
                            else {
                                // If machine.config was deleted the code will get to here
                                // supply at least something to prevent the world from coming to
                                // an abrupt end. 
                                listeners = new TraceListenerCollection();
                                TraceListener defaultListener = new DefaultTraceListener();
                                defaultListener.IndentLevel = indentLevel;
                                defaultListener.IndentSize = indentSize;
                                listeners.Add(defaultListener);
                            }
                        }
                    }
                }
                return listeners;
            }
        }

        internal static string AppName {
            get {
                if (appName == null) {
                    new EnvironmentPermission(EnvironmentPermissionAccess.Read, "Path").Assert();
                    appName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
                }
                return appName;
            }
        }
        
        public static bool AutoFlush { 
            get { 
                InitializeSettings();
                return autoFlush; 
            }

            set {
                InitializeSettings();    
                autoFlush = value;
            }
        }

        public static bool UseGlobalLock { 
            get { 
                InitializeSettings();
                return useGlobalLock; 
            }

            set {
                InitializeSettings();    
                useGlobalLock = value;
            }
        }
        
        public static int IndentLevel {
            get { return indentLevel; }

            set {
                // Use global lock
                lock (critSec) {
                    // We don't want to throw here -- it is very bad form to have debug or trace
                    // code throw exceptions!
                    if (value < 0) {
                        value = 0;
                    }
                    indentLevel = value;
                    
                    if (listeners != null) {
                        foreach (TraceListener listener in Listeners) {
                            listener.IndentLevel = indentLevel;
                        }
                    }
                }
            }
        }

        public static int IndentSize {
            get { 
                InitializeSettings();
                return indentSize; 
            }
            
            set {
                InitializeSettings();    
                SetIndentSize(value);
            }
        }

        static void SetIndentSize(int value) {
            // Use global lock
            lock (critSec) {                
                // We don't want to throw here -- it is very bad form to have debug or trace
                // code throw exceptions!            
                if (value < 0) {
                    value = 0;
                }

                indentSize = value;
                
                if (listeners != null) {
                    foreach (TraceListener listener in Listeners) {
                        listener.IndentSize = indentSize;
                    }
                } 
            }
        }

        public static void Indent() {
            // Use global lock
            lock (critSec) {
                InitializeSettings();
                if (indentLevel < Int32.MaxValue) {
                    indentLevel++;
                }
                foreach (TraceListener listener in Listeners) {
                    listener.IndentLevel = indentLevel;
                }
            }
        }

        public static void Unindent() {
            // Use global lock
            lock (critSec) {
                InitializeSettings();
                if (indentLevel > 0) {
                    indentLevel--;
                }
                foreach (TraceListener listener in Listeners) {
                    listener.IndentLevel = indentLevel;
                }
            }
        }

        public static void Flush() {
            if (listeners != null) {
                if (UseGlobalLock) {
                    lock (critSec) {
                        foreach (TraceListener listener in Listeners) {
                            listener.Flush();
                        }            
                    }
                }
                else {
                    foreach (TraceListener listener in Listeners) {
                        if (!listener.IsThreadSafe) {
                            lock (listener) {
                                listener.Flush();
                            }
                        }
                        else {
                            listener.Flush();
                        }
                    }            
                }
            }
        }

        public static void Close() {
            if (listeners != null) {
                // Use global lock
                lock (critSec) {
                    foreach (TraceListener listener in Listeners) {
                        listener.Close();
                    }            
                }
            }
        }

        public static void Assert(bool condition) {
            if (condition) return;
            Fail(string.Empty);
        }

        public static void Assert(bool condition, string message) {
            if (condition) return;
            Fail(message);
        }

        public static void Assert(bool condition, string message, string detailMessage) {
            if (condition) return;
            Fail(message, detailMessage);
        }

        public static void Fail(string message) {
            if (UseGlobalLock) {
                lock (critSec) {
                    foreach (TraceListener listener in Listeners) {
                        listener.Fail(message);
                        if (AutoFlush) listener.Flush();
                    }            
                }
            }
            else {
                foreach (TraceListener listener in Listeners) {
                    if (!listener.IsThreadSafe) {
                        lock (listener) {
                            listener.Fail(message);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                    else {
                        listener.Fail(message);
                        if (AutoFlush) listener.Flush();
                    }
                }            
            }
        }        

        public static void Fail(string message, string detailMessage) {
            if (UseGlobalLock) {
                lock (critSec) {
                    foreach (TraceListener listener in Listeners) {
                        listener.Fail(message, detailMessage);
                        if (AutoFlush) listener.Flush();
                    }            
                }
            }
            else {
                foreach (TraceListener listener in Listeners) {
                    if (!listener.IsThreadSafe) {
                        lock (listener) {
                            listener.Fail(message, detailMessage);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                    else {
                        listener.Fail(message, detailMessage);
                        if (AutoFlush) listener.Flush();
                    }
                }            
            }
        }        

        private static void InitializeSettings() {
            // we want to redo this logic exactly once if the last time we entered the config
            // system was still initializing.  (ASURT 111941, VSWhidbey 149552)
            if (!settingsInitialized || (defaultInitialized && DiagnosticsConfiguration.IsInitialized())) {
                // we should avoid 2 threads altering the state concurrently for predictable behavior
                // though it may not be strictly necessary at present
                lock(critSec) {
                    if (!settingsInitialized || (defaultInitialized && DiagnosticsConfiguration.IsInitialized())) {
                        defaultInitialized = DiagnosticsConfiguration.IsInitializing();
    
                        // Getting IndentSize and AutoFlush will load config on demand.
                        // If we load config and there are trace listeners added, we'll
                        // end up recursing, but that recursion will be stopped in
                        // DiagnosticsConfiguration.Initialize()           
                        SetIndentSize(DiagnosticsConfiguration.IndentSize);
                        autoFlush = DiagnosticsConfiguration.AutoFlush;
                        useGlobalLock = DiagnosticsConfiguration.UseGlobalLock;
                        settingsInitialized = true;
                    }
                }
            }
        }

        // This method refreshes all the data from the configuration file, so that updated to the configuration file are mirrored
        // in the System.Diagnostics.Trace class
        static internal void Refresh() {
            lock (critSec) {
                settingsInitialized = false;
                listeners = null;
            }
            InitializeSettings();
        }

    	public static void TraceEvent(TraceEventType eventType, int id, string format, params object[] args) {

            TraceEventCache EventCache = new TraceEventCache();

            if (UseGlobalLock) {
                lock (critSec) {
                    if (args == null) {
                        foreach (TraceListener listener in Listeners) {
                            listener.TraceEvent(EventCache, AppName, eventType, id, format);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                    else {
                        foreach (TraceListener listener in Listeners) {
                            listener.TraceEvent(EventCache, AppName, eventType, id, format, args);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                }
            }
            else {
                if (args == null) {
                    foreach (TraceListener listener in Listeners) {
                        if (!listener.IsThreadSafe) {
                            lock (listener) {
                                listener.TraceEvent(EventCache, AppName, eventType, id, format);
                                if (AutoFlush) listener.Flush();
                            }
                        }
                        else {
                            listener.TraceEvent(EventCache, AppName, eventType, id, format);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                }
                else {
                    foreach (TraceListener listener in Listeners) {
                        if (!listener.IsThreadSafe) {
                            lock (listener) {
                                listener.TraceEvent(EventCache, AppName, eventType, id, format, args);
                                if (AutoFlush) listener.Flush();
                            }
                        }
                        else {
                            listener.TraceEvent(EventCache, AppName, eventType, id, format, args);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                }
            }
    	}
    	

        public static void Write(string message) {
            if (UseGlobalLock) {
                lock (critSec) {
                    foreach (TraceListener listener in Listeners) {
                        listener.Write(message);
                        if (AutoFlush) listener.Flush();
                    }                        
                }
            }
            else {
                foreach (TraceListener listener in Listeners) {
                    if (!listener.IsThreadSafe) {
                        lock (listener) {
                            listener.Write(message);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                    else {
                        listener.Write(message);
                        if (AutoFlush) listener.Flush();
                    }
                }                        
            }
        }

        public static void Write(object value) {
            if (UseGlobalLock) {
                lock (critSec) {
                    foreach (TraceListener listener in Listeners) {
                        listener.Write(value);
                        if (AutoFlush) listener.Flush();
                    }                        
                }
            }
            else {
                foreach (TraceListener listener in Listeners) {
                    if (!listener.IsThreadSafe) {
                        lock (listener) {
                            listener.Write(value);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                    else {
                        listener.Write(value);
                        if (AutoFlush) listener.Flush();
                    }
                }                        
            }
        }

        public static void Write(string message, string category) {
            if (UseGlobalLock) {
                lock (critSec) {
                    foreach (TraceListener listener in Listeners) {
                        listener.Write(message, category);
                        if (AutoFlush) listener.Flush();
                    }                        
                }
            }
            else {
                foreach (TraceListener listener in Listeners) {
                    if (!listener.IsThreadSafe) {
                        lock (listener) {
                            listener.Write(message, category);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                    else {
                        listener.Write(message, category);
                        if (AutoFlush) listener.Flush();
                    }
                }                        
            }
        }

        public static void Write(object value, string category) {
            if (UseGlobalLock) {
                lock (critSec) {
                    foreach (TraceListener listener in Listeners) {
                        listener.Write(value, category);
                        if (AutoFlush) listener.Flush();
                    }                        
                }
            }
            else {
                foreach (TraceListener listener in Listeners) {
                    if (!listener.IsThreadSafe) {
                        lock (listener) {
                            listener.Write(value, category);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                    else {
                        listener.Write(value, category);
                        if (AutoFlush) listener.Flush();
                    }
                }                        
            }
        }

        public static void WriteLine(string message) {
            if (UseGlobalLock) {
                lock (critSec) {
                    foreach (TraceListener listener in Listeners) {
                        listener.WriteLine(message);
                        if (AutoFlush) listener.Flush();
                    }                        
                }
            }
            else {
                foreach (TraceListener listener in Listeners) {
                    if (!listener.IsThreadSafe) {
                        lock (listener) {
                            listener.WriteLine(message);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                    else {
                        listener.WriteLine(message);
                        if (AutoFlush) listener.Flush();
                    }
                }                        
            }
        }

        public static void WriteLine(object value) {
            if (UseGlobalLock) {
                lock (critSec) {
                    foreach (TraceListener listener in Listeners) {
                        listener.WriteLine(value);
                        if (AutoFlush) listener.Flush();
                    }                        
                }
            }
            else {
                foreach (TraceListener listener in Listeners) {
                    if (!listener.IsThreadSafe) {
                        lock (listener) {
                            listener.WriteLine(value);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                    else {
                        listener.WriteLine(value);
                        if (AutoFlush) listener.Flush();
                    }
                }                        
            }
        }

        public static void WriteLine(string message, string category) {
            if (UseGlobalLock) {
                lock (critSec) {
                    foreach (TraceListener listener in Listeners) {
                        listener.WriteLine(message, category);
                        if (AutoFlush) listener.Flush();
                    }                        
                }
            }
            else {
                foreach (TraceListener listener in Listeners) {
                    if (!listener.IsThreadSafe) {
                        lock (listener) {
                            listener.WriteLine(message, category);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                    else {
                        listener.WriteLine(message, category);
                        if (AutoFlush) listener.Flush();
                    }
                }                        
            }
        }

        public static void WriteLine(object value, string category) {
            if (UseGlobalLock) {
                lock (critSec) {
                    foreach (TraceListener listener in Listeners) {
                        listener.WriteLine(value, category);
                        if (AutoFlush) listener.Flush();
                    }                        
                }
            }
            else {
                foreach (TraceListener listener in Listeners) {
                    if (!listener.IsThreadSafe) {
                        lock (listener) {
                            listener.WriteLine(value, category);
                            if (AutoFlush) listener.Flush();
                        }
                    }
                    else {
                        listener.WriteLine(value, category);
                        if (AutoFlush) listener.Flush();
                    }
                }                        
            }
        }

        public static void WriteIf(bool condition, string message) {
            if (condition)
                Write(message);
        }

        public static void WriteIf(bool condition, object value) {
            if (condition)
                Write(value);
        }

        public static void WriteIf(bool condition, string message, string category) {
            if (condition)
                Write(message, category);
        }

        public static void WriteIf(bool condition, object value, string category) {
            if (condition)
                Write(value, category);
        }

        public static void WriteLineIf(bool condition, string message) {
            if (condition)
                WriteLine(message);
        }

        public static void WriteLineIf(bool condition, object value) {
            if (condition)
                WriteLine(value);
        }

        public static void WriteLineIf(bool condition, string message, string category) {
            if (condition)
                WriteLine(message, category);
        }

        public static void WriteLineIf(bool condition, object value, string category) {
            if (condition)
                WriteLine(value, category);
        }
    }
}
