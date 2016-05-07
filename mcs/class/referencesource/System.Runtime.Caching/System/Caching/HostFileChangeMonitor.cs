// <copyright file="HostFileChangeMonitor.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>
using System;
using System.Runtime.Caching.Hosting;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Caching.Resources;
using System.Globalization;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;

namespace System.Runtime.Caching {
    public sealed class HostFileChangeMonitor : FileChangeMonitor {
        private const int MAX_CHAR_COUNT_OF_LONG_CONVERTED_TO_HEXADECIMAL_STRING = 16;
        private static IFileChangeNotificationSystem s_fcn;
        private readonly ReadOnlyCollection<String> _filePaths;
        private String _uniqueId;
        private Object _fcnState;
        private DateTimeOffset _lastModified;

        private HostFileChangeMonitor() { } // hide default .ctor

        private void InitDisposableMembers() {
            bool dispose = true;
            try {
                string uniqueId = null;
                if (_filePaths.Count == 1) {
                    string path = _filePaths[0];
                    DateTimeOffset lastWrite;
                    long fileSize;
                    s_fcn.StartMonitoring(path, new OnChangedCallback(OnChanged), out _fcnState, out lastWrite, out fileSize);
                    uniqueId = path + lastWrite.UtcDateTime.Ticks.ToString("X", CultureInfo.InvariantCulture) + fileSize.ToString("X", CultureInfo.InvariantCulture);
                    _lastModified = lastWrite;
                }
                else {
                    int capacity = 0;
                    foreach (string path in _filePaths) {
                        capacity += path.Length + (2 * MAX_CHAR_COUNT_OF_LONG_CONVERTED_TO_HEXADECIMAL_STRING);
                    }
                    Hashtable fcnState = new Hashtable(_filePaths.Count);
                    _fcnState = fcnState;
                    StringBuilder sb = new StringBuilder(capacity);
                    foreach (string path in _filePaths) {
                        if (fcnState.Contains(path)) {
                            continue;
                        }
                        DateTimeOffset lastWrite;
                        long fileSize;
                        object state;
                        s_fcn.StartMonitoring(path, new OnChangedCallback(OnChanged), out state, out lastWrite, out fileSize);
                        fcnState[path] = state;
                        sb.Append(path);
                        sb.Append(lastWrite.UtcDateTime.Ticks.ToString("X", CultureInfo.InvariantCulture));
                        sb.Append(fileSize.ToString("X", CultureInfo.InvariantCulture));
                        if (lastWrite > _lastModified) {
                            _lastModified = lastWrite;
                        }
                    }
                    uniqueId = sb.ToString();
                }
                _uniqueId = uniqueId;
                dispose = false;
            }
            finally {
                InitializationComplete();
                if (dispose) {
                    Dispose();
                }
            }
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification = "Grandfathered suppression from original caching code checkin")]
        private static void InitFCN() {
            if (s_fcn == null) {
                IFileChangeNotificationSystem fcn = null;
                IServiceProvider host = ObjectCache.Host;
                if (host != null) {
                    fcn = host.GetService(typeof(IFileChangeNotificationSystem)) as IFileChangeNotificationSystem;
                }
                if (fcn == null) {
                    fcn = new FileChangeNotificationSystem();
                }
                Interlocked.CompareExchange(ref s_fcn, fcn, null);
            }
        }

        //
        // protected members
        //

        protected override void Dispose(bool disposing) {
            if (disposing && s_fcn != null) {
                if (_filePaths != null && _fcnState != null) {
                    if (_filePaths.Count > 1) {
                        Hashtable fcnState = _fcnState as Hashtable;
                        foreach (string path in _filePaths) {
                            if (path != null) {
                                object state = fcnState[path];
                                if (state != null) {
                                    s_fcn.StopMonitoring(path, state);
                                }
                            }
                        }
                    }
                    else {
                        string path = _filePaths[0];
                        if (path != null && _fcnState != null) {
                            s_fcn.StopMonitoring(path, _fcnState);
                        }
                    }
                }
            }
        }


        //
        // public and internal members
        //

        public override ReadOnlyCollection<String> FilePaths { get { return _filePaths; } }
        public override String UniqueId { get { return _uniqueId; } }
        public override DateTimeOffset LastModified { get { return _lastModified; } }

        public HostFileChangeMonitor(IList<String> filePaths) {
            if (filePaths == null) {
                throw new ArgumentNullException("filePaths");
            }
            if (filePaths.Count == 0) {
                throw new ArgumentException(RH.Format(R.Empty_collection, "filePaths"));
            }

            // *SECURITY* - filePaths is untrusted and should not be consumed outside of the sanitization method.
            _filePaths = SanitizeFilePathsList(filePaths);

            InitFCN();
            InitDisposableMembers();
        }

        [SecuritySafeCritical]
        private static ReadOnlyCollection<string> SanitizeFilePathsList(IList<string> filePaths) {
            List<string> newList = new List<string>(filePaths.Count);

            foreach (string path in filePaths) {
                if (String.IsNullOrEmpty(path)) {
                    throw new ArgumentException(RH.Format(R.Collection_contains_null_or_empty_string, "filePaths"));
                }
                else {
                    // DevDiv #269534: When we use a user-provided string in the constructor to this FileIOPermission and demand the permission,
                    // we need to be certain that we're adding *exactly that string* to the new list we're generating. The original code tried to
                    // optimize by checking all of the permissions upfront then doing a List<T>.AddRange at the end, but this opened the method
                    // to a TOCTOU attack since a malicious user could have modified the original IList<T> between the two calls.
                    new FileIOPermission(FileIOPermissionAccess.PathDiscovery, path).Demand();
                    newList.Add(path);
                }
            }

            return newList.AsReadOnly();
        }

    }
}
