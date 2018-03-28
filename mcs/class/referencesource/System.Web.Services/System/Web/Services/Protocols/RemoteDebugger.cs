namespace System.Web.Services.Protocols {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Text;
    using System.Runtime.InteropServices;
    using System.Web.Services.Interop;
    using System.Reflection;
    using System.Threading;
    using System.Security.Permissions;
    using System.Net;
    using System.ComponentModel; // for CompModSwitches
    using System.Web.Services.Diagnostics;

    internal class RemoteDebugger : INotifySource2 {
        private static INotifyConnection2 connection;
        private static bool getConnection = true;
        private INotifySink2 notifySink;
        private NotifyFilter notifyFilter;
        private UserThread userThread;

        private const int INPROC_SERVER = 1;
        private static Guid IID_NotifyConnectionClassGuid = new Guid("12A5B9F0-7A1C-4fcb-8163-160A30F519B5");
        private static Guid IID_NotifyConnection2Guid = new Guid("1AF04045-6659-4aaa-9F4B-2741AC56224B");
        private static string debuggerHeader = "VsDebuggerCausalityData";

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

        [DebuggerStepThrough]
        [DebuggerHidden]
        internal RemoteDebugger() {
        }

        ~RemoteDebugger() {
            Close();
        }


        internal static bool IsClientCallOutEnabled() {
            bool enabled = false;

            try {
                enabled = !CompModSwitches.DisableRemoteDebugging.Enabled && Debugger.IsAttached && Connection != null;
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, typeof(RemoteDebugger), "IsClientCallOutEnabled", e);
            }
            return enabled;
        }

        internal static bool IsServerCallInEnabled(ServerProtocol protocol, out string stringBuffer) {
            stringBuffer = null;
            bool enabled = false;
            try {
                if (CompModSwitches.DisableRemoteDebugging.Enabled)
                    return false;

                enabled = protocol.Context.IsDebuggingEnabled && Connection != null;
                if (enabled) {
                    stringBuffer = protocol.Request.Headers[debuggerHeader];
                    enabled = (stringBuffer != null && stringBuffer.Length > 0);
                }
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, typeof(RemoteDebugger), "IsServerCallInEnabled", e);
                enabled = false;
            }
            return enabled;
        }

        private static INotifyConnection2 Connection {
            get {
                if (connection == null && getConnection) {
                    lock (InternalSyncObject) {
                        if (connection == null) {
                            AppDomain.CurrentDomain.DomainUnload += new EventHandler(OnAppDomainUnload);
                            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

                            TraceMethod method = Tracing.On ? new TraceMethod(typeof(RemoteDebugger), "get_Connection") : null;
                            if (Tracing.On) Tracing.Enter("RemoteDebugger", method);

                            object unk;
                            int result = UnsafeNativeMethods.CoCreateInstance(ref IID_NotifyConnectionClassGuid,
                                                                               null,
                                                                               INPROC_SERVER,
                                                                               ref IID_NotifyConnection2Guid,
                                                                               out unk);

                            if (Tracing.On) Tracing.Exit("RemoteDebugger", method);

                            if (result >= 0) // success
                                connection = (INotifyConnection2)unk;
                            else
                                connection = null;
                        }
                        getConnection = false;
                    }
                }
                return connection;
            }
        }


        private INotifySink2 NotifySink {
            get {
                if (this.notifySink == null && Connection != null) {
                    TraceMethod method = Tracing.On ? new TraceMethod(this, "get_NotifySink") : null;
                    if (Tracing.On) Tracing.Enter("RemoteDebugger", method);

                    this.notifySink = UnsafeNativeMethods.RegisterNotifySource(Connection, this);

                    if (Tracing.On) Tracing.Exit("RemoteDebugger", method);
                }
                return this.notifySink;
            }
        }

        private static void CloseSharedResources() {
            if (connection != null) {
                lock (InternalSyncObject) {
                    if (connection != null) {
                        TraceMethod method = Tracing.On ? new TraceMethod(typeof(RemoteDebugger), "CloseSharedResources") : null;
                        if (Tracing.On) Tracing.Enter("RemoteDebugger", method);

                        try {
                            Marshal.ReleaseComObject(connection);
                        }
                        catch (Exception e) {
                            if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                                throw;
                            }
                            if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, typeof(RemoteDebugger), "CloseSharedResources", e);
                        }
                        if (Tracing.On) Tracing.Exit("RemoteDebugger", method);
                        connection = null;
                    }
                }
            }
        }

        private void Close() {
            if (this.notifySink != null && connection != null) {
                lock (InternalSyncObject) {
                    if (this.notifySink != null && connection != null) {
                        TraceMethod method = Tracing.On ? new TraceMethod(this, "Close") : null;
                        if (Tracing.On) Tracing.Enter("RemoteDebugger", method);

                        try {
                            UnsafeNativeMethods.UnregisterNotifySource(connection, this);
                        }
                        catch (Exception e) {
                            if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                                throw;
                            }
                            if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, method, e);
                        }
                        if (Tracing.On) Tracing.Exit("RemoteDebugger", method);
                        this.notifySink = null;
                    }
                }
            }
        }

        [DebuggerStepThrough]
        [DebuggerHidden]
        internal void NotifyClientCallOut(WebRequest request) {
            try {
                if (NotifySink == null) return;

                IntPtr bufferPtr;
                int bufferSize = 0;
                CallId callId = new CallId(null, 0, (IntPtr)0, 0, null, request.RequestUri.Host);

                TraceMethod method = Tracing.On ? new TraceMethod(this, "NotifyClientCallOut") : null;
                if (Tracing.On) Tracing.Enter("RemoteDebugger", method);

                UnsafeNativeMethods.OnSyncCallOut(NotifySink, callId, out bufferPtr, ref bufferSize);

                if (Tracing.On) Tracing.Exit("RemoteDebugger", method);

                if (bufferPtr == IntPtr.Zero) return;
                byte[] buffer = null;
                try {
                    buffer = new byte[bufferSize];
                    Marshal.Copy(bufferPtr, buffer, 0, bufferSize);
                }
                finally {
                    Marshal.FreeCoTaskMem(bufferPtr);
                }
                string bufferString = Convert.ToBase64String(buffer);
                request.Headers.Add(debuggerHeader, bufferString);
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, typeof(RemoteDebugger), "NotifyClientCallOut", e);
            }
        }

        [DebuggerStepThrough]
        [DebuggerHidden]
        internal void NotifyClientCallReturn(WebResponse response) {
            try {
                if (NotifySink == null) return;

                byte[] buffer = new byte[0];
                if (response != null) {
                    string bufferString = response.Headers[debuggerHeader];
                    if (bufferString != null && bufferString.Length != 0)
                        buffer = Convert.FromBase64String(bufferString);
                }
                CallId callId = new CallId(null, 0, (IntPtr)0, 0, null, null);

                TraceMethod method = Tracing.On ? new TraceMethod(this, "NotifyClientCallReturn") : null;
                if (Tracing.On) Tracing.Enter("RemoteDebugger", method);

                UnsafeNativeMethods.OnSyncCallReturn(NotifySink, callId, buffer, buffer.Length);

                if (Tracing.On) Tracing.Exit("RemoteDebugger", method);
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, typeof(RemoteDebugger), "NotifyClientCallReturn", e);
            }

            this.Close();
        }

        [DebuggerStepThrough]
        [DebuggerHidden]
        internal void NotifyServerCallEnter(ServerProtocol protocol, string stringBuffer) {
            try {
                if (NotifySink == null) return;
                StringBuilder methodBuilder = new StringBuilder();
                methodBuilder.Append(protocol.Type.FullName);
                methodBuilder.Append('.');
                methodBuilder.Append(protocol.MethodInfo.Name);
                methodBuilder.Append('(');
                ParameterInfo[] parameterInfos = protocol.MethodInfo.Parameters;
                for (int i = 0; i < parameterInfos.Length; ++i) {
                    if (i != 0)
                        methodBuilder.Append(',');

                    methodBuilder.Append(parameterInfos[i].ParameterType.FullName);
                }
                methodBuilder.Append(')');

                byte[] buffer = Convert.FromBase64String(stringBuffer);
                CallId callId = new CallId(null, 0, (IntPtr)0, 0, methodBuilder.ToString(), null);

                TraceMethod method = Tracing.On ? new TraceMethod(this, "NotifyServerCallEnter") : null;
                if (Tracing.On) Tracing.Enter("RemoteDebugger", method);

                UnsafeNativeMethods.OnSyncCallEnter(NotifySink, callId, buffer, buffer.Length);

                if (Tracing.On) Tracing.Exit("RemoteDebugger", method);
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, typeof(RemoteDebugger), "NotifyServerCallEnter", e);
            }
        }

        [DebuggerStepThrough]
        [DebuggerHidden]
        internal void NotifyServerCallExit(HttpResponse response) {
            try {
                if (NotifySink == null) return;

                IntPtr bufferPtr;
                int bufferSize = 0;
                CallId callId = new CallId(null, 0, (IntPtr)0, 0, null, null);

                TraceMethod method = Tracing.On ? new TraceMethod(this, "NotifyServerCallExit") : null;
                if (Tracing.On) Tracing.Enter("RemoteDebugger", method);

                UnsafeNativeMethods.OnSyncCallExit(NotifySink, callId, out bufferPtr, ref bufferSize);

                if (Tracing.On) Tracing.Exit("RemoteDebugger", method);

                if (bufferPtr == IntPtr.Zero) return;
                byte[] buffer = null;
                try {
                    buffer = new byte[bufferSize];
                    Marshal.Copy(bufferPtr, buffer, 0, bufferSize);
                }
                finally {
                    Marshal.FreeCoTaskMem(bufferPtr);
                }
                string stringBuffer = Convert.ToBase64String(buffer);
                response.AddHeader(debuggerHeader, stringBuffer);
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, typeof(RemoteDebugger), "NotifyServerCallExit", e);
            }
            this.Close();
        }


        private static void OnAppDomainUnload(object sender, EventArgs args) {
            CloseSharedResources();
        }

        private static void OnProcessExit(object sender, EventArgs args) {
            CloseSharedResources();
        }

        void INotifySource2.SetNotifyFilter(NotifyFilter in_NotifyFilter, UserThread in_pUserThreadFilter) {
            notifyFilter = in_NotifyFilter;
            userThread = in_pUserThreadFilter;
        }
    }
}
