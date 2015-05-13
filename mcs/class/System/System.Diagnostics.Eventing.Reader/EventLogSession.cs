namespace System.Diagnostics.Eventing.Reader
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class EventLogSession : IDisposable
    {
        private string domain;
        private static EventLogSession globalSession = new EventLogSession();
        private EventLogHandle handle;
        private SessionAuthentication logOnType;
        internal EventLogHandle renderContextHandleSystem;
        internal EventLogHandle renderContextHandleUser;
        private string server;
        private object syncObject;
        private string user;

        [SecurityCritical]
        public EventLogSession()
        {
            this.renderContextHandleSystem = EventLogHandle.Zero;
            this.renderContextHandleUser = EventLogHandle.Zero;
            this.handle = EventLogHandle.Zero;
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            this.syncObject = new object();
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventLogSession(string server) : this(server, null, null, null, SessionAuthentication.Default)
        {
        }

        [SecurityCritical]
        public EventLogSession(string server, string domain, string user, SecureString password, SessionAuthentication logOnType)
        {
            this.renderContextHandleSystem = EventLogHandle.Zero;
            this.renderContextHandleUser = EventLogHandle.Zero;
            this.handle = EventLogHandle.Zero;
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            if (server == null)
            {
                server = "localhost";
            }
            this.syncObject = new object();
            this.server = server;
            this.domain = domain;
            this.user = user;
            this.logOnType = logOnType;
            Microsoft.Win32.UnsafeNativeMethods.EvtRpcLogin login = new Microsoft.Win32.UnsafeNativeMethods.EvtRpcLogin {
                Server = this.server,
                User = this.user,
                Domain = this.domain,
                Flags = (int) this.logOnType,
                Password = CoTaskMemUnicodeSafeHandle.Zero
            };
            try
            {
                if (password != null)
                {
                    login.Password.SetMemory(Marshal.SecureStringToCoTaskMemUnicode(password));
                }
                this.handle = NativeWrapper.EvtOpenSession(Microsoft.Win32.UnsafeNativeMethods.EvtLoginClass.EvtRpcLogin, ref login, 0, 0);
            }
            finally
            {
                login.Password.Close();
            }
        }

        public void CancelCurrentOperations()
        {
            NativeWrapper.EvtCancel(this.handle);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void ClearLog(string logName)
        {
            this.ClearLog(logName, null);
        }

        public void ClearLog(string logName, string backupPath)
        {
            if (logName == null)
            {
                throw new ArgumentNullException("logName");
            }
            NativeWrapper.EvtClearLog(this.Handle, logName, backupPath, 0);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SecuritySafeCritical]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this == globalSession)
                {
                    throw new InvalidOperationException();
                }
                EventLogPermissionHolder.GetEventLogPermission().Demand();
            }
            if ((this.renderContextHandleSystem != null) && !this.renderContextHandleSystem.IsInvalid)
            {
                this.renderContextHandleSystem.Dispose();
            }
            if ((this.renderContextHandleUser != null) && !this.renderContextHandleUser.IsInvalid)
            {
                this.renderContextHandleUser.Dispose();
            }
            if ((this.handle != null) && !this.handle.IsInvalid)
            {
                this.handle.Dispose();
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void ExportLog(string path, PathType pathType, string query, string targetFilePath)
        {
            this.ExportLog(path, pathType, query, targetFilePath, false);
        }

        public void ExportLog(string path, PathType pathType, string query, string targetFilePath, bool tolerateQueryErrors)
        {
            Microsoft.Win32.UnsafeNativeMethods.EvtExportLogFlags evtExportLogChannelPath;
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (targetFilePath == null)
            {
                throw new ArgumentNullException("targetFilePath");
            }
            switch (pathType)
            {
                case PathType.LogName:
                    evtExportLogChannelPath = Microsoft.Win32.UnsafeNativeMethods.EvtExportLogFlags.EvtExportLogChannelPath;
                    break;

                case PathType.FilePath:
                    evtExportLogChannelPath = Microsoft.Win32.UnsafeNativeMethods.EvtExportLogFlags.EvtExportLogFilePath;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("pathType");
            }
            if (!tolerateQueryErrors)
            {
                NativeWrapper.EvtExportLog(this.Handle, path, query, targetFilePath, (int) evtExportLogChannelPath);
            }
            else
            {
                NativeWrapper.EvtExportLog(this.Handle, path, query, targetFilePath, ((int) evtExportLogChannelPath) | 0x1000);
            }
        }

        public void ExportLogAndMessages(string path, PathType pathType, string query, string targetFilePath)
        {
            this.ExportLogAndMessages(path, pathType, query, targetFilePath, false, CultureInfo.CurrentCulture);
        }

        public void ExportLogAndMessages(string path, PathType pathType, string query, string targetFilePath, bool tolerateQueryErrors, CultureInfo targetCultureInfo)
        {
            if (targetCultureInfo == null)
            {
                targetCultureInfo = CultureInfo.CurrentCulture;
            }
            this.ExportLog(path, pathType, query, targetFilePath, tolerateQueryErrors);
            NativeWrapper.EvtArchiveExportedLog(this.Handle, targetFilePath, targetCultureInfo.LCID, 0);
        }

        public EventLogInformation GetLogInformation(string logName, PathType pathType)
        {
            if (logName == null)
            {
                throw new ArgumentNullException("logName");
            }
            return new EventLogInformation(this, logName, pathType);
        }

        [SecurityCritical]
        public IEnumerable<string> GetLogNames()
        {
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            List<string> list = new List<string>(100);
            using (EventLogHandle handle = NativeWrapper.EvtOpenChannelEnum(this.Handle, 0))
            {
                bool finish = false;
                do
                {
                    string item = NativeWrapper.EvtNextChannelPath(handle, ref finish);
                    if (!finish)
                    {
                        list.Add(item);
                    }
                }
                while (!finish);
                return list;
            }
        }

        [SecurityCritical]
        public IEnumerable<string> GetProviderNames()
        {
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            List<string> list = new List<string>(100);
            using (EventLogHandle handle = NativeWrapper.EvtOpenProviderEnum(this.Handle, 0))
            {
                bool finish = false;
                do
                {
                    string item = NativeWrapper.EvtNextPublisherId(handle, ref finish);
                    if (!finish)
                    {
                        list.Add(item);
                    }
                }
                while (!finish);
                return list;
            }
        }

        [SecuritySafeCritical]
        internal void SetupSystemContext()
        {
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            if (this.renderContextHandleSystem.IsInvalid)
            {
                lock (this.syncObject)
                {
                    if (this.renderContextHandleSystem.IsInvalid)
                    {
                        this.renderContextHandleSystem = NativeWrapper.EvtCreateRenderContext(0, null, Microsoft.Win32.UnsafeNativeMethods.EvtRenderContextFlags.EvtRenderContextSystem);
                    }
                }
            }
        }

        [SecuritySafeCritical]
        internal void SetupUserContext()
        {
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            lock (this.syncObject)
            {
                if (this.renderContextHandleUser.IsInvalid)
                {
                    this.renderContextHandleUser = NativeWrapper.EvtCreateRenderContext(0, null, Microsoft.Win32.UnsafeNativeMethods.EvtRenderContextFlags.EvtRenderContextUser);
                }
            }
        }

        public static EventLogSession GlobalSession
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return globalSession;
            }
        }

        internal EventLogHandle Handle
        {
            get
            {
                return this.handle;
            }
        }
    }
}

