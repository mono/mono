//------------------------------------------------------------------------------
// <copyright file="FileChangesMonitor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web {
    using System.Collections;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Util;
    using Microsoft.Win32;


    // Type of the callback to the subscriber of a file change event in FileChangesMonitor.StartMonitoringFile
    delegate void FileChangeEventHandler(Object sender, FileChangeEvent e); 

    // The type of file change that occurred.
    enum FileAction {
        Dispose = -2,
        Error = -1,
        Overwhelming = 0,
        Added = 1,
        Removed = 2,
        Modified = 3,
        RenamedOldName = 4,
        RenamedNewName = 5
    }

    // Event data for a file change notification
    sealed class FileChangeEvent : EventArgs {
        internal FileAction   Action;       // the action
        internal string       FileName;     // the file that caused the action

        internal FileChangeEvent(FileAction action, string fileName) {
            this.Action = action;
            this.FileName = fileName;
        }
    }

    // Contains information about the target of a file change notification
    sealed class FileMonitorTarget {
        internal readonly FileChangeEventHandler    Callback;   // the callback
        internal readonly string                    Alias;      // the filename used to name the file
        internal readonly DateTime                  UtcStartMonitoring;// time we started monitoring
        int                                         _refs;      // number of uses of callbacks

        internal FileMonitorTarget(FileChangeEventHandler callback, string alias) {
            Callback = callback;
            Alias = alias;
            UtcStartMonitoring = DateTime.UtcNow;
            _refs = 1;
        }

        internal int AddRef() {
            _refs++;
            return _refs;
        }

        internal int Release() {
            _refs--;
            return _refs;
        }

#if DBG
        internal string DebugDescription(string indent) {
            StringBuilder   sb = new StringBuilder(200);
            string          i2 = indent + "    ";

            sb.Append(indent + "FileMonitorTarget\n");
            sb.Append(i2 + "       Callback: " + Callback.Target + "(HC=" + Callback.Target.GetHashCode().ToString("x", NumberFormatInfo.InvariantInfo) + ")\n");
            sb.Append(i2 + "          Alias: " + Alias + "\n");
            sb.Append(i2 + "StartMonitoring: " + Debug.FormatUtcDate(UtcStartMonitoring) + "\n");
            sb.Append(i2 + "          _refs: " + _refs + "\n");

            return sb.ToString();
        }
#endif
    }

#if !FEATURE_PAL // FEATURE_PAL does not enable access control
    sealed class FileSecurity {
        const int DACL_INFORMATION = 
                UnsafeNativeMethods.DACL_SECURITY_INFORMATION |
                UnsafeNativeMethods.GROUP_SECURITY_INFORMATION |
                UnsafeNativeMethods.OWNER_SECURITY_INFORMATION;

        static Hashtable    s_interned;
        static byte[]       s_nullDacl;

        class DaclComparer : IEqualityComparer {
            // Compares two objects. An implementation of this method must return a
            // value less than zero if x is less than y, zero if x is equal to y, or a
            // value greater than zero if x is greater than y.
            //

            private int Compare(byte[] a, byte[] b) {
                int result = a.Length - b.Length;
                for (int i = 0; result == 0 && i < a.Length ; i++) {
                    result = a[i] - b[i];
                }

                return result;
            }

            bool IEqualityComparer.Equals(Object x, Object y) {
                if (x == null && y == null) {
                    return true;
                }

                if (x == null || y == null) {
                    return false;
                }

                byte[] a = x as byte[];
                byte[] b = y as byte[];
                
                if (a == null || b == null) {
                    return false;
                }

                return Compare(a, b) == 0;
            }

            int IEqualityComparer.GetHashCode(Object obj) {
                byte[] a = (byte[]) obj;

                HashCodeCombiner combiner = new HashCodeCombiner();
                foreach (byte b in a) {
                    combiner.AddObject(b);
                }

                return combiner.CombinedHash32;
            }
        }

        static FileSecurity() {
            s_interned = new Hashtable(0, 1.0f, new DaclComparer());
            s_nullDacl = new byte[0];
         }

        [SuppressMessage("Microsoft.Interoperability", "CA1404:CallGetLastErrorImmediatelyAfterPInvoke",
                         Justification="Microsoft: Call to GetLastWin32Error() does follow P/Invoke call that is outside the if/else block.")]
        static internal byte[] GetDacl(string filename) {
            // DevDiv #322858 - allow skipping DACL step for perf gain
            if (HostingEnvironment.FcnSkipReadAndCacheDacls) {
                return s_nullDacl;
            }

            // DevDiv #246973
            // Perf: Start with initial buffer size to minimize File IO
            //
            int lengthNeeded = 512;
            byte[] dacl = new byte[lengthNeeded];
            int fOK = UnsafeNativeMethods.GetFileSecurity(filename, DACL_INFORMATION, dacl, dacl.Length, ref lengthNeeded);

            if (fOK != 0) {
                // If no size is needed, return a non-null marker
                if (lengthNeeded == 0) {
                   Debug.Trace("GetDacl", "Returning null dacl");
                   return s_nullDacl;
                }
                
                // Shrink the buffer to fit the whole data
                Array.Resize(ref dacl, lengthNeeded);
            }
            else {
                int hr = HttpException.HResultFromLastError(Marshal.GetLastWin32Error());
                
                // Check if need to redo the call with larger buffer
                if (hr != HResults.E_INSUFFICIENT_BUFFER) {
                    Debug.Trace("GetDacl", "Error in first call to GetFileSecurity: 0x" + hr.ToString("x", NumberFormatInfo.InvariantInfo));
                    return null;
                }

                // The buffer wasn't large enough.  Try again
                dacl = new byte[lengthNeeded];
                fOK = UnsafeNativeMethods.GetFileSecurity(filename, DACL_INFORMATION, dacl, dacl.Length, ref lengthNeeded);
                if (fOK == 0) {
#if DBG
                    hr = HttpException.HResultFromLastError(Marshal.GetLastWin32Error());
                    Debug.Trace("GetDacl", "Error in second to GetFileSecurity: 0x" + hr.ToString("x", NumberFormatInfo.InvariantInfo));
#endif

                    return null;
                }
            }

            byte[] interned = (byte[]) s_interned[dacl];
            if (interned == null) {
                lock (s_interned.SyncRoot) {
                    interned = (byte[]) s_interned[dacl];
                    if (interned == null) {
                        Debug.Trace("GetDacl", "Interning new dacl, length " + dacl.Length);
                        interned = dacl;
                        s_interned[interned] = interned;
                    }
                }
            }

            Debug.Trace("GetDacl", "Returning dacl, length " + dacl.Length);
            return interned;
        }
    }

    // holds information about a single file and the targets of change notification
    sealed class FileMonitor {
        internal readonly DirectoryMonitor  DirectoryMonitor;   // the parent
        internal readonly HybridDictionary  Aliases;            // aliases for this file
        string                              _fileNameLong;      // long file name - if null, represents any file in this directory
        string                              _fileNameShort;     // short file name, may be null
        HybridDictionary                    _targets;           // targets of notification
        bool                                _exists;            // does the file exist?
        FileAttributesData                  _fad;               // file attributes
        byte[]                              _dacl;              // dacl
        FileAction                          _lastAction;        // last action that ocurred on this file
        DateTime                            _utcLastCompletion; // date of the last RDCW completion

        internal FileMonitor(
                DirectoryMonitor dirMon, string fileNameLong, string fileNameShort, 
                bool exists, FileAttributesData fad, byte[] dacl) {

            DirectoryMonitor = dirMon;
            _fileNameLong = fileNameLong;
            _fileNameShort = fileNameShort;
            _exists = exists;
            _fad = fad;
            _dacl = dacl;
            _targets = new HybridDictionary();
            Aliases = new HybridDictionary(true);
        }

        internal string FileNameLong    {get {return _fileNameLong;}}
        internal string FileNameShort   {get {return _fileNameShort;}}
        internal bool   Exists          {get {return _exists;}}
        internal bool   IsDirectory     {get {return (FileNameLong == null);}}
        internal FileAction LastAction {
            get {return _lastAction;}
            set {_lastAction = value;}
        }

        internal DateTime UtcLastCompletion {
            get {return _utcLastCompletion;}
            set {_utcLastCompletion = value;}
        }

        // Returns the attributes of a file, updating them if the file has changed.
        internal FileAttributesData Attributes {
            get {return _fad;}
        }

        internal byte[] Dacl {
            get {return _dacl;}
        }

        internal void ResetCachedAttributes() {
            _fad = null;
            _dacl = null;
        }

        internal void UpdateCachedAttributes() {
            string path = Path.Combine(DirectoryMonitor.Directory, FileNameLong);
            FileAttributesData.GetFileAttributes(path, out _fad);
            _dacl = FileSecurity.GetDacl(path);
        }

        // Set new file information when a file comes into existence
        internal void MakeExist(FindFileData ffd, byte[] dacl) {
            _fileNameLong = ffd.FileNameLong;
            _fileNameShort = ffd.FileNameShort;
            _fad = ffd.FileAttributesData;
            _dacl = dacl;
            _exists = true;
        }

        // Remove a file from existence
        internal void MakeExtinct() {
            _fad = null;
            _dacl = null;
            _exists = false;
        }

        internal void RemoveFileNameShort() {
            _fileNameShort = null;
        }

        internal ICollection Targets {
            get {return _targets.Values;}
        }

         // Add delegate for this file.
        internal void AddTarget(FileChangeEventHandler callback, string alias, bool newAlias) {
            FileMonitorTarget target = (FileMonitorTarget)_targets[callback.Target];
            if (target != null) {
                target.AddRef();
            }
            else {
#if DBG
                // Needs the lock to sync with DebugDescription
                lock (_targets) {
#endif                
                    _targets.Add(callback.Target, new FileMonitorTarget(callback, alias));
#if DBG
                }
#endif
            }

            if (newAlias) {
                Aliases[alias] = alias;
            }
        }

        
        // Remove delegate for this file given the target object.
        internal int RemoveTarget(object callbackTarget) {
            FileMonitorTarget target = (FileMonitorTarget)_targets[callbackTarget];
#if DBG            
            if (FileChangesMonitor.s_enableRemoveTargetAssert) {
                Debug.Assert(target != null, "removing file monitor target that was never added or already been removed");
            }
#endif
            if (target != null && target.Release() == 0) {
#if DBG
                // Needs the lock to sync with DebugDescription
                lock (_targets) {
#endif                
                    _targets.Remove(callbackTarget);
#if DBG
                }
#endif
            }

            return _targets.Count;
        }

#if DBG
        internal string DebugDescription(string indent) {
            StringBuilder   sb = new StringBuilder(200);
            string          i2 = indent + "    ";
            string          i3 = i2 + "    ";
            DictionaryEntryTypeComparer detcomparer = new DictionaryEntryTypeComparer();

            sb.Append(indent + "System.Web.FileMonitor: ");
            if (FileNameLong != null) {
                sb.Append(FileNameLong);
                if (FileNameShort != null) {
                    sb.Append("; ShortFileName=" + FileNameShort);
                }

                sb.Append("; FileExists="); sb.Append(_exists);                
            }
            else {
                sb.Append("<ANY>");
            }
            sb.Append("\n");
            sb.Append(i2 + "LastAction="); sb.Append(_lastAction);
            sb.Append("; LastCompletion="); sb.Append(Debug.FormatUtcDate(_utcLastCompletion));
            sb.Append("\n");

            if (_fad != null) {
                sb.Append(_fad.DebugDescription(i2));
            }
            else {
                sb.Append(i2 + "FileAttributesData = <null>\n");
            }

            DictionaryEntry[] delegateEntries;

            lock (_targets) {
                sb.Append(i2 + _targets.Count + " delegates...\n");

                delegateEntries = new DictionaryEntry[_targets.Count];
                _targets.CopyTo(delegateEntries, 0);
            }
            
            Array.Sort(delegateEntries, detcomparer);
            
            foreach (DictionaryEntry d in delegateEntries) {
                sb.Append(i3 + "Delegate " + d.Key.GetType() + "(HC=" + d.Key.GetHashCode().ToString("x", NumberFormatInfo.InvariantInfo) + ")\n");
            }

            return sb.ToString();
        }
#endif

    }

    // Change notifications delegate from native code.
    delegate void NativeFileChangeNotification(FileAction action, [In, MarshalAs(UnmanagedType.LPWStr)] string fileName, long ticks);

    // 
    // Wraps N/Direct calls to native code that does completion port
    // based ReadDirectoryChangesW().
    // This needs to be a separate object so that a DirectoryMonitory
    // can start monitoring while the old _rootCallback has not been
    // disposed.
    //
    sealed class DirMonCompletion : IDisposable {
        static int _activeDirMonCompletions = 0;            // private counter used via reflection by FCN check-in suite

        DirectoryMonitor    _dirMon;                        // directory monitor
        IntPtr              _ndirMonCompletionPtr;          // pointer to native dir mon as int (used only to construct HandleRef)
        HandleRef           _ndirMonCompletionHandle;       // handleref of a pointer to native dir mon as int
        GCHandle            _rootCallback;                  // roots this callback to prevent collection
        int                 _disposed;                      // set to 1 when we call DirMonClose
        object              _ndirMonCompletionHandleLock;

        internal static int ActiveDirMonCompletions { get { return _activeDirMonCompletions; } }

        internal DirMonCompletion(DirectoryMonitor dirMon, string dir, bool watchSubtree, uint notifyFilter) {
            Debug.Trace("FileChangesMonitor", "DirMonCompletion::ctor " + dir + " " + watchSubtree.ToString() + " " + notifyFilter.ToString(NumberFormatInfo.InvariantInfo));

            int                             hr;
            NativeFileChangeNotification    myCallback;

            _dirMon = dirMon;
            myCallback = new NativeFileChangeNotification(this.OnFileChange);
            _ndirMonCompletionHandleLock = new object();
            try {
            }
            finally {
                // protected from ThreadAbortEx
                lock(_ndirMonCompletionHandleLock) {
                    // Dev10 927846: The managed DirMonCompletion.ctor calls DirMonOpen to create and initialize the native DirMonCompletion.
                    // If this succeeds, the managed DirMonCompletion.ctor creates a GCHandle to root itself so the target of the callback
                    // stays alive.  When the native DirMonCompletion is freed it invokes the managed callback with ACTION_DISPOSE to
                    // release the GCHandle.  In order for the native DirMonCOmpletion to be freed, either DirMonOpen must fail or
                    // the managed DirMonCompletion.Dispose must be called and it must invoke DirMonClose.  Waiting until the native
                    // DirMonCompletion.dtor is called to release the GCHandle ensures that the directory handle has been closed,
                    // the i/o completions have finished and there are no other threads calling the managed callback.  This is because
                    // we AddRef when we initiate i/o and we Release when the i/o completion finishes.
                    
                    // If I don't do this, myCallback will be collected by GC since its only reference is
                    // from the native code.
                    _rootCallback = GCHandle.Alloc(myCallback);

                    hr = UnsafeNativeMethods.DirMonOpen(dir, HttpRuntime.AppDomainAppId, watchSubtree, notifyFilter, dirMon.FcnMode, myCallback, out _ndirMonCompletionPtr);
                    if (hr != HResults.S_OK) {
                        _rootCallback.Free();
                        throw FileChangesMonitor.CreateFileMonitoringException(hr, dir);
                    }
                    
                    _ndirMonCompletionHandle = new HandleRef(this, _ndirMonCompletionPtr);
                    Interlocked.Increment(ref _activeDirMonCompletions);
                }
            }
        }

        ~DirMonCompletion() {
            Dispose(false);
        }

        void IDisposable.Dispose() {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            Debug.Trace("FileChangesMonitor", "DirMonCompletion::Dispose");
            // this is here because callbacks from native code can cause
            // this to be invoked from multiple threads concurrently, and
            // I don't want contention on _ndirMonCompletionHandleLock,
            // but also need to ensure that DirMonOpen returns and the
            // _ndirMonCompletionHandle is set before DirMonClose is called.
            if (Interlocked.Exchange(ref _disposed, 1) == 0) {

                // Dev10 927846: There is a small window during which the .ctor has
                // not returned from DirMonOpen yet but because we already started
                // monitoring, we might receive a change notification which could 
                // potentially Dispose the instance, so we need to block until 
                // DirMonOpen returns and _ndirMonCompletionHandler is set
                lock(_ndirMonCompletionHandleLock) {
                    // Dev11 - 364642: if Dispose is called while finalizing for AD unload then
                    // the native DirMonCompletion won't be able to call back into the appdomain.
                    // But it does not need to because _rootCallback is already reclaimed as part of AD unload
                    bool fNeedToSendFileActionDispose = !AppDomain.CurrentDomain.IsFinalizingForUnload();
                    HandleRef ndirMonCompletionHandle = _ndirMonCompletionHandle;
                    if (ndirMonCompletionHandle.Handle != IntPtr.Zero) {
                        _ndirMonCompletionHandle = new HandleRef(this, IntPtr.Zero);
                        UnsafeNativeMethods.DirMonClose(ndirMonCompletionHandle, fNeedToSendFileActionDispose);
                    }
                }
            }
        }

        void OnFileChange(FileAction action, string fileName, long ticks) {
            DateTime utcCompletion;
            if (ticks == 0) {
                utcCompletion = DateTime.MinValue;
            }
            else {
                utcCompletion = DateTimeUtil.FromFileTimeToUtc(ticks);
            }

#if DBG
            Debug.Trace("FileChangesMonitorOnFileChange", "Action=" + action + "; Dir=" + _dirMon.Directory + "; fileName=" +  Debug.ToStringMaybeNull(fileName) + "; completion=" + Debug.FormatUtcDate(utcCompletion) + ";_ndirMonCompletionPtr=0x" + _ndirMonCompletionPtr.ToString("x"));
#endif

            //
            // The native DirMonCompletion sends FileAction.Dispose
            // when there are no more outstanding calls on the 
            // delegate. Only then can _rootCallback be freed.
            //
            if (action == FileAction.Dispose) {
                if (_rootCallback.IsAllocated) {
                    _rootCallback.Free();
                }
                Interlocked.Decrement(ref _activeDirMonCompletions);
            }
            else {
                using (new ApplicationImpersonationContext()) {
                    _dirMon.OnFileChange(action, fileName, utcCompletion);
                }
            }
        }

#if DBG
        internal string DebugDescription(string indent) {
            int hc = ((Delegate)_rootCallback.Target).Target.GetHashCode();
            string description = indent + "_ndirMonCompletionPtr=0x" + _ndirMonCompletionPtr.ToString("x") + "; callback=0x" + hc.ToString("x", NumberFormatInfo.InvariantInfo) + "\n";
            return description;
        }
#endif
    }

    sealed class NotificationQueueItem {
        internal readonly FileChangeEventHandler Callback;
        internal readonly string                 Filename;
        internal readonly FileAction             Action;

        internal NotificationQueueItem(FileChangeEventHandler callback, FileAction action, string filename) {
            Callback = callback;
            Action = action;
            Filename = filename;
        }
    }

    //
    // Monitor changes in a single directory.
    //
    sealed class DirectoryMonitor : IDisposable {

        static Queue            s_notificationQueue = new Queue();
        static WorkItemCallback s_notificationCallback = new WorkItemCallback(FireNotifications);
        static int              s_inNotificationThread;    
        static int              s_notificationBufferSizeIncreased = 0;

        internal readonly string    Directory;                      // directory being monitored
        Hashtable                   _fileMons;                      // fileName -> FileMonitor
        int                         _cShortNames;                   // number of file monitors that are added with their short name
        FileMonitor                 _anyFileMon;                    // special file monitor to watch for any changes in directory
        bool                        _watchSubtree;                  // watch subtree?
        uint                        _notifyFilter;                  // the notify filter for the call to ReadDirectoryChangesW
        bool                        _ignoreSubdirChange;            // when a subdirectory is deleted or renamed, ignore the notification if we're not monitoring it
        DirMonCompletion            _dirMonCompletion;              // dirmon completion
        bool                        _isDirMonAppPathInternal;       // special dirmon that monitors all files and subdirectories beneath the vroot (enabled via FCNMode registry key)

        // FcnMode to pass to native code
        internal int FcnMode {
            get;
            set; 
        }

        // constructor for special dirmon that monitors all files and subdirectories beneath the vroot (enabled via FCNMode registry key)
        internal DirectoryMonitor(string appPathInternal, int fcnMode): this(appPathInternal, true, UnsafeNativeMethods.RDCW_FILTER_FILE_AND_DIR_CHANGES, fcnMode) {
            _isDirMonAppPathInternal = true;
        }
        
        internal DirectoryMonitor(string dir, bool watchSubtree, uint notifyFilter, int fcnMode): this(dir, watchSubtree, notifyFilter, false, fcnMode) {
        }

        internal DirectoryMonitor(string dir, bool watchSubtree, uint notifyFilter, bool ignoreSubdirChange, int fcnMode) {
            Directory = dir;
            _fileMons = new Hashtable(StringComparer.OrdinalIgnoreCase);
            _watchSubtree = watchSubtree;
            _notifyFilter = notifyFilter;
            _ignoreSubdirChange = ignoreSubdirChange;
            FcnMode = fcnMode;
        }

        void IDisposable.Dispose() {
            if (_dirMonCompletion != null) {
                ((IDisposable)_dirMonCompletion).Dispose();
                _dirMonCompletion = null;
            }

            //
            // Remove aliases to this object in FileChangesMonitor so that
            // it is not rooted.
            //
            if (_anyFileMon != null) {
                HttpRuntime.FileChangesMonitor.RemoveAliases(_anyFileMon);
                _anyFileMon = null;
            }

            foreach (DictionaryEntry e in _fileMons) {
                string key = (string) e.Key;
                FileMonitor fileMon = (FileMonitor) e.Value;
                if (fileMon.FileNameLong == key) {
                    HttpRuntime.FileChangesMonitor.RemoveAliases(fileMon);
                }
            }

            _fileMons.Clear();
            _cShortNames = 0;
        }

        internal bool IsMonitoring() {
            return GetFileMonitorsCount() > 0;
        }

        void StartMonitoring() {
            if (_dirMonCompletion == null) {
                _dirMonCompletion = new DirMonCompletion(this, Directory, _watchSubtree, _notifyFilter);
            }
        }

        internal void StopMonitoring() {
            lock (this) {
                ((IDisposable)this).Dispose();
            }    
        }

        FileMonitor FindFileMonitor(string file) {
            FileMonitor fileMon;

            if (file == null) {
                fileMon = _anyFileMon;
            }
            else {
                fileMon = (FileMonitor)_fileMons[file];
            }

            return fileMon;
        }

        FileMonitor AddFileMonitor(string file) {
            string path;
            FileMonitor fileMon;
            FindFileData ffd = null;
            int hr;

            if (String.IsNullOrEmpty(file)) {
                // add as the <ANY> file monitor
                fileMon = new FileMonitor(this, null, null, true, null, null);
                _anyFileMon = fileMon;
            }
            else {
                // Get the long and short name of the file
                path = Path.Combine(Directory, file);
                if (_isDirMonAppPathInternal) {
                    hr = FindFileData.FindFile(path, Directory, out ffd);
                }
                else {
                    hr = FindFileData.FindFile(path, out ffd);
                }
                if (hr == HResults.S_OK) {
                    // Unless this is FileChangesMonitor._dirMonAppPathInternal,
                    // don't monitor changes to a directory - this will not pickup changes to files in the directory.
                    if (!_isDirMonAppPathInternal
                        && (ffd.FileAttributesData.FileAttributes & FileAttributes.Directory) != 0) {
                        throw FileChangesMonitor.CreateFileMonitoringException(HResults.E_INVALIDARG, path);
                    }

                    byte[] dacl = FileSecurity.GetDacl(path);
                    fileMon = new FileMonitor(this, ffd.FileNameLong, ffd.FileNameShort, true, ffd.FileAttributesData, dacl);
                    _fileMons.Add(ffd.FileNameLong, fileMon);

                    // Update short name aliases to this file
                    UpdateFileNameShort(fileMon, null, ffd.FileNameShort);
                }
                else if (hr == HResults.E_PATHNOTFOUND || hr == HResults.E_FILENOTFOUND) {
                    // Don't allow possible short file names to be added as non-existant,
                    // because it is impossible to track them if they are indeed a short name since
                    // short file names may change.
                    
                    // FEATURE_PAL 



                    if (file.IndexOf('~') != -1) {
                        throw FileChangesMonitor.CreateFileMonitoringException(HResults.E_INVALIDARG, path);
                    }

                    // Add as non-existent file
                    fileMon = new FileMonitor(this, file, null, false, null, null);
                    _fileMons.Add(file, fileMon);
                }
                else {
                    throw FileChangesMonitor.CreateFileMonitoringException(hr, path);
                }
            }

            return fileMon;
        }

        //
        // Update short names of a file
        //
        void UpdateFileNameShort(FileMonitor fileMon, string oldFileNameShort, string newFileNameShort) {
            if (oldFileNameShort != null) {
                FileMonitor oldFileMonShort = (FileMonitor)_fileMons[oldFileNameShort];
                if (oldFileMonShort != null) {
                    // The old filemonitor no longer has this short file name.
                    // Update the monitor and _fileMons
                    if (oldFileMonShort != fileMon) {
                        oldFileMonShort.RemoveFileNameShort();
                    }

                    
                    _fileMons.Remove(oldFileNameShort);
                    _cShortNames--;
                }
            }

            if (newFileNameShort != null) {
                // Add the new short file name.
                _fileMons.Add(newFileNameShort, fileMon);
                _cShortNames++;
            }
        }

        void RemoveFileMonitor(FileMonitor fileMon) {
            if (fileMon == _anyFileMon) {
                _anyFileMon = null;
            }
            else {
                _fileMons.Remove(fileMon.FileNameLong);
                if (fileMon.FileNameShort != null) {
                    _fileMons.Remove(fileMon.FileNameShort);
                    _cShortNames--;
                }
            }

            HttpRuntime.FileChangesMonitor.RemoveAliases(fileMon);
        }

        int GetFileMonitorsCount() {
            int c = _fileMons.Count - _cShortNames;
            if (_anyFileMon != null) {
                c++;
            }

            return c;
        }

        // The 4.0 CAS changes made the AppDomain homogenous, so we need to assert
        // FileIOPermission.  Currently this is only exposed publicly via CacheDependency, which
        // already does a PathDiscover check for public callers.
        [FileIOPermission(SecurityAction.Assert, Unrestricted = true)]
        internal FileMonitor StartMonitoringFileWithAssert(string file, FileChangeEventHandler callback, string alias) {
            FileMonitor fileMon = null;
            bool firstFileMonAdded = false;

            lock (this) {
                // Find existing file monitor
                fileMon = FindFileMonitor(file);
                if (fileMon == null) {
                    // Add a new monitor
                    fileMon = AddFileMonitor(file);
                    if (GetFileMonitorsCount() == 1) {
                        firstFileMonAdded = true;
                    }
                }

                // Add callback to the file monitor
                fileMon.AddTarget(callback, alias, true);

                // Start directory monitoring when the first file gets added
                if (firstFileMonAdded) {
                    StartMonitoring();
                }
            }

            return fileMon;
        }

        //
        // Request to stop monitoring a file.
        //
        internal void StopMonitoringFile(string file, object target) {
            FileMonitor fileMon;
            int numTargets;

            lock (this) {
                // Find existing file monitor
                fileMon = FindFileMonitor(file);
                if (fileMon != null) {
                    numTargets = fileMon.RemoveTarget(target);
                    if (numTargets == 0) {
                        RemoveFileMonitor(fileMon);

                        // last target for the file monitor gone 
                        // -- remove the file monitor
                        if (GetFileMonitorsCount() == 0) {
                            ((IDisposable)this).Dispose();
                        }
                    }
                }
            }

#if DBG
            if (fileMon != null) {
                Debug.Dump("FileChangesMonitor", HttpRuntime.FileChangesMonitor);
            }
#endif
        }


        internal bool GetFileAttributes(string file, out FileAttributesData fad) {
            FileMonitor fileMon = null;
            fad = null;

            lock (this) {
                // Find existing file monitor
                fileMon = FindFileMonitor(file);
                if (fileMon != null) {
                    // Get the attributes
                    fad = fileMon.Attributes;
                    return true;
                }
            }

            return false;
        }

        //
        // Notes about file attributes:
        // 
        // CreationTime is the time a file entry is added to a directory. 
        //     If file q1 is copied to q2, q2's creation time is updated if it is new to the directory,
        //         else q2's old time is used.
        // 
        //     If a file is deleted, then added, its creation time is preserved from before the delete.
        //     
        // LastWriteTime is the time a file was last written.    
        //     If file q1 is copied to q2, q2's lastWrite time is the same as q1.
        //     Note that this implies that the LastWriteTime can be older than the LastCreationTime,
        //     and that a copy of a file can result in the LastWriteTime being earlier than
        //     its previous value.
        // 
        // LastAccessTime is the time a file was last accessed, such as opened or written to.
        //     Note that if the attributes of a file are changed, its LastAccessTime is not necessarily updated.
        //     
        // If the FileSize, CreationTime, or LastWriteTime have changed, then we know that the 
        //     file has changed in a significant way, and that the LastAccessTime will be greater than
        //     or equal to that time.
        //     
        // If the FileSize, CreationTime, or LastWriteTime have not changed, then the file's
        //     attributes may have changed without changing the LastAccessTime.
        //
        
        // Confirm that the changes occurred after we started monitoring,
        // to handle the case where:
        //
        //     1. User creates a file.
        //     2. User starts to monitor the file.
        //     3. Change notification is made of the original creation of the file.
        // 
        // Note that we can only approximate when the last change occurred by
        // examining the LastAccessTime. The LastAccessTime will change if the 
        // contents of a file (but not necessarily its attributes) change.
        // The drawback to using the LastAccessTime is that it will also be
        // updated when a file is read.
        //
        // Note that we cannot make this confirmation when only the file's attributes
        // or ACLs change, because changes to attributes and ACLs won't change the LastAccessTime.
        // 
        bool IsChangeAfterStartMonitoring(FileAttributesData fad, FileMonitorTarget target, DateTime utcCompletion) {
            // If the LastAccessTime is more than 60 seconds before we
            // started monitoring, then the change likely did not update
            // the LastAccessTime correctly.
            if (fad.UtcLastAccessTime.AddSeconds(60) < target.UtcStartMonitoring) {
#if DBG
               Debug.Trace("FileChangesMonitorIsChangeAfterStart", "LastAccessTime is more than 60 seconds before monitoring started.");
#endif
                return true;
            }

            // Check if the notification of the change came after
            // we started monitoring.
            if (utcCompletion > target.UtcStartMonitoring) {
#if DBG
               Debug.Trace("FileChangesMonitorIsChangeAfterStart", "Notification came after we started monitoring.");
#endif
                return true;
            }

            // Make sure that the LastAccessTime is valid.
            // It must be more recent than the LastWriteTime.
            if (fad.UtcLastAccessTime < fad.UtcLastWriteTime) {
#if DBG
               Debug.Trace("FileChangesMonitorIsChangeAfterStart", "UtcLastWriteTime is greater then UtcLastAccessTime.");
#endif
                return true;
            }

            // If the LastAccessTime occurs exactly at midnight,
            // then the system is FAT32 and LastAccessTime is unusable.
            if (fad.UtcLastAccessTime.TimeOfDay == TimeSpan.Zero) {
#if DBG
               Debug.Trace("FileChangesMonitorIsChangeAfterStart", "UtcLastAccessTime is midnight -- FAT32 likely.");
#endif
                 return true;
            }

            // Finally, compare LastAccessTime to the time we started monitoring.
            // If the time of the last access was before we started monitoring, then
            // we know a change did not occur to the file contents.
            if (fad.UtcLastAccessTime >= target.UtcStartMonitoring) {
#if DBG
               Debug.Trace("FileChangesMonitorIsChangeAfterStart", "UtcLastAccessTime is greater than UtcStartMonitoring.");
#endif
                return true;
            }

#if DBG
               Debug.Trace("FileChangesMonitorIsChangeAfterStart", "Change is before start of monitoring.  Data:\n FileAttributesData: \nUtcCreationTime: "
               + fad.UtcCreationTime + " UtcLastAccessTime: " + fad.UtcLastAccessTime + " UtcLastWriteTime: " + fad.UtcLastWriteTime + "\n FileMonitorTarget:\n UtcStartMonitoring: "
               + target.UtcStartMonitoring + "\nUtcCompletion: " + utcCompletion);
#endif
            return false;
         }

        // If this is a special dirmon that monitors all files and subdirectories 
        // beneath the vroot (enabled via FCNMode registry key), then
        // we need to special case how we lookup the FileMonitor.  For example, nobody has called
        // StartMonitorFile for specific files in the App_LocalResources directory,
        // so we need to see if fileName is in App_LocalResources and then get the FileMonitor for
        // the directory.
        private bool GetFileMonitorForSpecialDirectory(string fileName, ref FileMonitor fileMon) {

            // fileName should not be in short form (8.3 format)...it was converted to long form in
            // DirMonCompletion::ProcessOneFileNotification
            
            // first search for match within s_dirsToMonitor
            for (int i = 0; i < FileChangesMonitor.s_dirsToMonitor.Length; i++) {
                if (StringUtil.StringStartsWithIgnoreCase(fileName, FileChangesMonitor.s_dirsToMonitor[i])) {
                    fileMon = (FileMonitor)_fileMons[FileChangesMonitor.s_dirsToMonitor[i]];
                    return fileMon != null;
                }
            }

            // if we did not find a match in s_dirsToMonitor, look for LocalResourcesDirectoryName anywhere within fileName
            int indexStart = fileName.IndexOf(HttpRuntime.LocalResourcesDirectoryName, StringComparison.OrdinalIgnoreCase);
            if (indexStart > -1) {
                int dirNameLength = indexStart + HttpRuntime.LocalResourcesDirectoryName.Length;

                // fileName should either end with LocalResourcesDirectoryName or include a trailing slash and more characters
                if (fileName.Length == dirNameLength || fileName[dirNameLength] == Path.DirectorySeparatorChar) {
                    string dirName = fileName.Substring(0, dirNameLength);
                    fileMon = (FileMonitor)_fileMons[dirName];
                    return fileMon != null;
                }
            }

            return false;
        }


        //
        // Delegate callback from native code.
        //
        internal void OnFileChange(FileAction action, string fileName, DateTime utcCompletion) {
            //
            // Use try/catch to prevent runtime exceptions from propagating 
            // into native code.
            //
            try {
                FileMonitor             fileMon = null;
                ArrayList               targets = null;
                int                     i, n;
                FileMonitorTarget       target;
                ICollection             col;
                string                  key;
                FileAttributesData      fadOld = null;
                FileAttributesData      fadNew = null;
                byte[]                  daclOld = null;
                byte[]                  daclNew = null;
                FileAction              lastAction = FileAction.Error;
                DateTime                utcLastCompletion = DateTime.MinValue;
                bool                    isSpecialDirectoryChange = false;

#if DBG
                string                  reasonIgnore = string.Empty;
                string                  reasonFire = string.Empty;
#endif

                // We've already stopped monitoring, but a change completion was
                // posted afterwards. Ignore it.
                if (_dirMonCompletion == null) {
                    return;
                }

                lock (this) {
                    if (_fileMons.Count > 0) {
                        if (action == FileAction.Error || action == FileAction.Overwhelming) {
                            // Overwhelming change -- notify all file monitors
                            Debug.Assert(fileName == null, "fileName == null");
                            Debug.Assert(action != FileAction.Overwhelming, "action != FileAction.Overwhelming");

                            if (action == FileAction.Overwhelming) {
                                //increase file notification buffer size, but only once per app instance
                                if (Interlocked.Increment(ref s_notificationBufferSizeIncreased) == 1) {
                                    UnsafeNativeMethods.GrowFileNotificationBuffer( HttpRuntime.AppDomainAppId, _watchSubtree );
                                }
                            }

                            // Get targets for all files
                            targets = new ArrayList();    
                            foreach (DictionaryEntry d in _fileMons) {
                                key = (string) d.Key;
                                fileMon = (FileMonitor) d.Value;
                                if (fileMon.FileNameLong == key) {
                                    fileMon.ResetCachedAttributes();
                                    fileMon.LastAction = action;
                                    fileMon.UtcLastCompletion = utcCompletion;
                                    col = fileMon.Targets;
                                    targets.AddRange(col);
                                }
                            }

                            fileMon = null;
                        }
                        else {
                            Debug.Assert((int) action >= 1 && fileName != null && fileName.Length > 0,
                                        "(int) action >= 1 && fileName != null && fileName.Length > 0");

                            // Find the file monitor
                            fileMon = (FileMonitor)_fileMons[fileName];

                            if (_isDirMonAppPathInternal && fileMon == null) {
                                isSpecialDirectoryChange = GetFileMonitorForSpecialDirectory(fileName, ref fileMon);
                            }
                            
                            if (fileMon != null) {
                                // Get the targets
                                col = fileMon.Targets;
                                targets = new ArrayList(col);

                                fadOld = fileMon.Attributes;
                                daclOld = fileMon.Dacl;
                                lastAction = fileMon.LastAction;
                                utcLastCompletion = fileMon.UtcLastCompletion;
                                fileMon.LastAction = action;
                                fileMon.UtcLastCompletion = utcCompletion;

                                if (action == FileAction.Removed || action == FileAction.RenamedOldName) {
                                    // File not longer exists.
                                    fileMon.MakeExtinct();
                                }
                                else if (fileMon.Exists) {
                                    // We only need to update the attributes if this is 
                                    // a different completion, as we retreive the attributes
                                    // after the completion is received.
                                    if (utcLastCompletion != utcCompletion) {
                                        fileMon.UpdateCachedAttributes();
                                    }
                                }
                                else {
                                    // File now exists - update short name and attributes.
                                    FindFileData ffd = null;
                                    string path = Path.Combine(Directory, fileMon.FileNameLong);
                                    int hr;
                                    if (_isDirMonAppPathInternal) {
                                        hr = FindFileData.FindFile(path, Directory, out ffd);
                                    }
                                    else {
                                        hr = FindFileData.FindFile(path, out ffd);
                                    }
                                    if (hr == HResults.S_OK) {
                                        Debug.Assert(StringUtil.EqualsIgnoreCase(fileMon.FileNameLong, ffd.FileNameLong),
                                                    "StringUtil.EqualsIgnoreCase(fileMon.FileNameLong, ffd.FileNameLong)");

                                        string oldFileNameShort = fileMon.FileNameShort;
                                        byte[] dacl = FileSecurity.GetDacl(path);
                                        fileMon.MakeExist(ffd, dacl);
                                        UpdateFileNameShort(fileMon, oldFileNameShort, ffd.FileNameShort);
                                    }
                                }

                                fadNew = fileMon.Attributes;
                                daclNew = fileMon.Dacl;
                            }
                        }
                    }

                    // Notify the delegate waiting for any changes
                    if (_anyFileMon != null) {
                        col = _anyFileMon.Targets;
                        if (targets != null) {
                            targets.AddRange(col);
                        }
                        else {
                            targets = new ArrayList(col);
                        }
                    }

                    if (action == FileAction.Error) {
                        // Stop monitoring.
                        ((IDisposable)this).Dispose();
                    }
                }

                // Ignore Modified action for directories (VSWhidbey 295597)
                bool ignoreThisChangeNotification = false;

                if (!isSpecialDirectoryChange && fileName != null && action == FileAction.Modified) {
                    // check if the file is a directory (reuse attributes if already obtained)
                    FileAttributesData fad = fadNew;

                    if (fad == null) {
                        string path = Path.Combine(Directory, fileName);
                        FileAttributesData.GetFileAttributes(path, out fad);
                    }

                    if (fad != null && ((fad.FileAttributes & FileAttributes.Directory) != 0)) {
                        // ignore if directory
                        ignoreThisChangeNotification = true;
                    }
                }

                // Dev10 440497: Don't unload AppDomain when a folder is deleted or renamed, unless we're monitoring files in it
                if (_ignoreSubdirChange && (action == FileAction.Removed || action == FileAction.RenamedOldName) && fileName != null) {
                    string fullPath = Path.Combine(Directory, fileName);
                    if (!HttpRuntime.FileChangesMonitor.IsDirNameMonitored(fullPath, fileName)) {
#if DBG
                        Debug.Trace("FileChangesMonitorIgnoreSubdirChange", 
                                    "*** Ignoring SubDirChange " + DateTime.Now.ToString("hh:mm:ss.fff", CultureInfo.InvariantCulture) 
                                    + ": fullPath=" + fullPath + ", action=" + action.ToString());
#endif
                        ignoreThisChangeNotification = true;
                    }
#if DBG
                    else {
                        Debug.Trace("FileChangesMonitorIgnoreSubdirChange", 
                                    "*** SubDirChange " + DateTime.Now.ToString("hh:mm:ss.fff", CultureInfo.InvariantCulture) 
                                    + ": fullPath=" + fullPath + ", action=" + action.ToString());
                    }
#endif
                }

                // Fire the event
                if (targets != null && !ignoreThisChangeNotification) {
                    Debug.Dump("FileChangesMonitor", HttpRuntime.FileChangesMonitor);

                    lock (s_notificationQueue.SyncRoot) {
                        for (i = 0, n = targets.Count; i < n; i++) {
                            //
                            // Determine whether the change is significant, and if so, add it 
                            // to the notification queue.
                            //
                            // - A change is significant if action is other than Added or Modified
                            // - A change is significant if the action is Added and it occurred after
                            //   the target started monitoring.
                            // - If the action is Modified:
                            // -- A change is significant if the file contents were modified
                            //    and it occurred after the target started monitoring.
                            // -- A change is significant if the DACL changed. We cannot check if
                            //    the change was made after the target started monitoring in this case,
                            //    as the LastAccess time may not be updated.
                            //
                            target = (FileMonitorTarget)targets[i];
                            bool isSignificantChange;
                            if ((action != FileAction.Added && action != FileAction.Modified) || fadNew == null) {

                                // Any change other than Added or Modified is significant.
                                // If we have no attributes to examine, the change is significant.
                                isSignificantChange = true;

#if DBG
                                reasonFire = "(action != FileAction.Added && action != FileAction.Modified) || fadNew == null";
#endif
                            }
                            else if (action == FileAction.Added) {
                                // Added actions are significant if they occur after we started monitoring.
                                isSignificantChange = IsChangeAfterStartMonitoring(fadNew, target, utcCompletion);

#if DBG
                                reasonIgnore = "change occurred before started monitoring";
                                reasonFire = "file added after start of monitoring";
#endif

                            }
                            else {
                                Debug.Assert(action == FileAction.Modified, "action == FileAction.Modified");
                                if (utcCompletion == utcLastCompletion) {
                                    // File attributes and ACLs will not have changed if the completion is the same
                                    // as the last, since we get the attributes after all changes in the completion
                                    // have occurred. Therefore if the previous change was Modified, there
                                    // is no change that we can detect.
                                    //
                                    // Notepad fires such spurious notifications when a file is saved.
                                    // 
                                    isSignificantChange = (lastAction != FileAction.Modified);

#if DBG
                                    reasonIgnore = "spurious FileAction.Modified";
                                    reasonFire = "spurious completion where action != modified";
#endif

                                }
                                else if (fadOld == null) {
                                    // There were no attributes before this notification, 
                                    // so assume the change is significant. We cannot check for
                                    // whether the change was after the start of monitoring,
                                    // because we don't know if the content changed, or just
                                    // DACL, in which case the LastAccessTime will not be updated.
                                    isSignificantChange = true;

#if DBG
                                    reasonFire = "no attributes before this notification";
#endif
                                }
                                else if (daclOld == null || daclOld != daclNew) {
                                    // The change is significant if the DACL changed. 
                                    // We cannot check if the change is after the start of monitoring,
                                    // as a change in the DACL does not necessarily update the
                                    // LastAccessTime of a file.
                                    // If we cannot access the DACL, then we must assume
                                    // that it is what has changed.
                                    isSignificantChange = true;

#if DBG
                                    if (daclOld == null) {
                                        reasonFire = "unable to access ACL";
                                    }
                                    else {
                                        reasonFire = "ACL changed";
                                    }
#endif

                                }
                                else {
                                    // The file content was modified. We cannot guarantee that the
                                    // LastWriteTime or FileSize changed when the file changed, as 
                                    // copying a file preserves the LastWriteTime, and the "touch"
                                    // command can reset the LastWriteTime of many files to the same
                                    // time.
                                    //
                                    // If the file content is modified, we can determine if the file
                                    // was not changed after the start of monitoring by looking at 
                                    // the LastAccess time.
                                    isSignificantChange = IsChangeAfterStartMonitoring(fadNew, target, utcCompletion);

#if DBG
                                    reasonIgnore = "change occurred before started monitoring";
                                    reasonFire = "file content modified after start of monitoring";
#endif

                                }
                            }

                            if (isSignificantChange) {
#if DBG
                                Debug.Trace("FileChangesMonitorCallback", "Firing change event, reason=" + reasonFire + 
                                    "\n\tArgs: Action=" + action +     ";     Completion=" + Debug.FormatUtcDate(utcCompletion) + "; fileName=" + fileName + 
                                    "\n\t  LastAction=" + lastAction + "; LastCompletion=" + Debug.FormatUtcDate(utcLastCompletion) + 
                                    "\nfadOld=" + ((fadOld != null) ? fadOld.DebugDescription("\t") : "<null>") +
                                    "\nfileMon=" + ((fileMon != null) ? fileMon.DebugDescription("\t") : "<null>") + 
                                    "\n" + target.DebugDescription("\t"));
#endif

                                s_notificationQueue.Enqueue(new NotificationQueueItem(target.Callback, action, target.Alias));
                            }
#if DBG
                            else {
                                Debug.Trace("FileChangesMonitorCallback", "Ignoring change event, reason=" + reasonIgnore +
                                    "\n\tArgs: Action=" + action +     ";     Completion=" + Debug.FormatUtcDate(utcCompletion) + "; fileName=" + fileName + 
                                    "\n\t  LastAction=" + lastAction + "; LastCompletion=" + Debug.FormatUtcDate(utcLastCompletion) + 
                                    "\nfadOld=" + ((fadOld != null) ? fadOld.DebugDescription("\t") : "<null>") +
                                    "\nfileMon=" + ((fileMon != null) ? fileMon.DebugDescription("\t") : "<null>") + 
                                    "\n" + target.DebugDescription("\t"));

                            }
#endif
                        }
                    }

                    if (s_notificationQueue.Count > 0 && s_inNotificationThread == 0 && Interlocked.Exchange(ref s_inNotificationThread, 1) == 0) {
                        WorkItem.PostInternal(s_notificationCallback);
                    }
                }
            }
            catch (Exception ex) {
                Debug.Trace(Debug.TAG_INTERNAL, 
                            "Exception thrown processing file change notification" +
                            " action=" + action.ToString() +
                            " fileName" + fileName);

                Debug.TraceException(Debug.TAG_INTERNAL, ex);
            }
        }

        // Fire notifications on a separate thread from that which received the notifications,
        // so that we don't block notification collection.
        static void FireNotifications() {
            try {
                // Outer loop: test whether we need to fire notifications and grab the lock
                for (;;) {
                    // Inner loop: fire notifications until the queue is emptied
                    for (;;) {
                        // Remove an item from the queue.
                        NotificationQueueItem nqi = null;
                        lock (s_notificationQueue.SyncRoot) {
                            if (s_notificationQueue.Count > 0) {
                                nqi = (NotificationQueueItem) s_notificationQueue.Dequeue();
                            }
                        }

                        if (nqi == null)
                            break;

                        try {
                            Debug.Trace("FileChangesMonitorFireNotification", "Firing change event" + 
                                "\n\tArgs: Action=" + nqi.Action + "; fileName=" + nqi.Filename + "; Target=" + nqi.Callback.Target + "(HC=" + nqi.Callback.Target.GetHashCode().ToString("x", NumberFormatInfo.InvariantInfo) + ")");

                            // Call the callback
                            nqi.Callback(null, new FileChangeEvent(nqi.Action, nqi.Filename)); 
                        }
                        catch (Exception ex) {
                            Debug.Trace(Debug.TAG_INTERNAL, 
                                        "Exception thrown in file change callback" +
                                        " action=" + nqi.Action.ToString() +
                                        " fileName" + nqi.Filename);

                            Debug.TraceException(Debug.TAG_INTERNAL, ex);
                        }
                    }

                    // Release the lock
                    Interlocked.Exchange(ref s_inNotificationThread, 0);

                    // We need to test again to avoid ---- where a thread that receives notifications adds to the
                    // queue, but does not spawn a thread because s_inNotificationThread = 1
                    if (s_notificationQueue.Count == 0 || Interlocked.Exchange(ref s_inNotificationThread, 1) != 0)
                        break;
                }
            }
            catch {
                Interlocked.Exchange(ref s_inNotificationThread, 0);
            }
        }

#if DBG
        internal string DebugDescription(string indent) {
            StringBuilder   sb = new StringBuilder(200);
            string          i2 = indent + "    ";
            DictionaryEntryCaseInsensitiveComparer  decomparer = new DictionaryEntryCaseInsensitiveComparer();
            
            lock (this) {
                DictionaryEntry[] fileEntries = new DictionaryEntry[_fileMons.Count];
                _fileMons.CopyTo(fileEntries, 0);
                Array.Sort(fileEntries, decomparer);
                
                sb.Append(indent + "System.Web.DirectoryMonitor: " + Directory + "\n");
                if (_dirMonCompletion != null) {
                    sb.Append(i2 + "_dirMonCompletion " + _dirMonCompletion.DebugDescription(String.Empty));
                }
                else {
                    sb.Append(i2 + "_dirMonCompletion = <null>\n");
                }

                sb.Append(i2 + GetFileMonitorsCount() + " file monitors...\n");
                if (_anyFileMon != null) {
                    sb.Append(_anyFileMon.DebugDescription(i2));
                }

                foreach (DictionaryEntry d in fileEntries) {
                    FileMonitor fileMon = (FileMonitor)d.Value;
                    if (fileMon.FileNameShort == (string)d.Key)
                        continue;

                    sb.Append(fileMon.DebugDescription(i2));
                }
            }

            return sb.ToString();
        }
#endif
    }
#endif // !FEATURE_PAL

    //
    // Manager for directory monitors.                       
    // Provides file change notification services in ASP.NET 
    //
    sealed class FileChangesMonitor {
#if !FEATURE_PAL // FEATURE_PAL does not enable file change notification
        internal static string[] s_dirsToMonitor = new string[] {
            HttpRuntime.BinDirectoryName,
            HttpRuntime.ResourcesDirectoryName,
            HttpRuntime.CodeDirectoryName,
            HttpRuntime.WebRefDirectoryName,
            HttpRuntime.BrowsersDirectoryName
        };

        internal const int MAX_PATH = 260;

        #pragma warning disable 0649
        ReadWriteSpinLock       _lockDispose;                       // spinlock for coordinating dispose
        #pragma warning restore 0649

        bool                    _disposed;                          // have we disposed?
        Hashtable               _aliases;                           // alias -> FileMonitor
        Hashtable               _dirs;                              // dir -> DirectoryMonitor
        DirectoryMonitor        _dirMonSubdirs;                     // subdirs monitor for renames
        Hashtable               _subDirDirMons;                     // Hashtable of DirectoryMonitor used in ListenToSubdirectoryChanges
        ArrayList               _dirMonSpecialDirs;                 // top level dirs we monitor
        FileChangeEventHandler  _callbackRenameOrCriticaldirChange; // event handler for renames and bindir
        int                     _activeCallbackCount;               // number of callbacks currently executing
        DirectoryMonitor        _dirMonAppPathInternal;             // watches all files and subdirectories (at any level) beneath HttpRuntime.AppDomainAppPathInternal
        String                  _appPathInternal;                   // HttpRuntime.AppDomainAppPathInternal
        int                     _FCNMode;                           // from registry, controls how we monitor directories

#if DBG
        internal static bool    s_enableRemoveTargetAssert;
#endif

        // Dev10 927283: We were appending to HttpRuntime._shutdownMessage in DirectoryMonitor.OnFileChange when
        // we received overwhelming changes and errors, but not all overwhelming file change notifications result
        // in a shutdown.  The fix is to only append to _shutdownMessage when the domain is being shutdown.
        internal static string GenerateErrorMessage(FileAction action, String fileName = null) {
            string message = null;
            if (action == FileAction.Overwhelming) {
                message = "Overwhelming Change Notification in ";
            }
            else if (action == FileAction.Error) {
                message = "File Change Notification Error in ";
            }
            else {
                return null;
            }
            return (fileName != null) ? message + Path.GetDirectoryName(fileName) : message;
        }

        internal static HttpException CreateFileMonitoringException(int hr, string path) {
            string  message;
            bool    logEvent = false;

            switch (hr) {
                case HResults.E_FILENOTFOUND:
                case HResults.E_PATHNOTFOUND:
                    message = SR.Directory_does_not_exist_for_monitoring;
                    break;

                case HResults.E_ACCESSDENIED:
                    message = SR.Access_denied_for_monitoring;
                    logEvent = true;
                    break;

                case HResults.E_INVALIDARG:
                    message = SR.Invalid_file_name_for_monitoring;
                    break;

                case HResults.ERROR_TOO_MANY_CMDS:
                    message = SR.NetBios_command_limit_reached;
                    logEvent = true;
                    break;

                default:
                    message = SR.Failed_to_start_monitoring;
                    break;
            }


            if (logEvent) {
                // Need to raise an eventlog too.
                UnsafeNativeMethods.RaiseFileMonitoringEventlogEvent(
                    SR.GetString(message, HttpRuntime.GetSafePath(path)) + 
                    "\n\r" + 
                    SR.GetString(SR.App_Virtual_Path, HttpRuntime.AppDomainAppVirtualPath),
                    path, HttpRuntime.AppDomainAppVirtualPath, hr);
            }
            
            return new HttpException(SR.GetString(message, HttpRuntime.GetSafePath(path)), hr);
        }

        internal static string GetFullPath(string alias) {
            // Assert PathDiscovery before call to Path.GetFullPath
            try {
                new FileIOPermission(FileIOPermissionAccess.PathDiscovery, alias).Assert();
            }
            catch {
                throw CreateFileMonitoringException(HResults.E_INVALIDARG, alias);
            }

            string path = Path.GetFullPath(alias);
            path = FileUtil.RemoveTrailingDirectoryBackSlash(path);

            return path;
        }

        private bool IsBeneathAppPathInternal(string fullPathName) {
            if (_appPathInternal != null
                && fullPathName.Length > _appPathInternal.Length+1
                && fullPathName.IndexOf(_appPathInternal, StringComparison.OrdinalIgnoreCase) > -1 
                && fullPathName[_appPathInternal.Length] == Path.DirectorySeparatorChar) {
                return true;
            }
            return false;
        }

        private bool IsFCNDisabled { get { return _FCNMode == 1; } }

        internal FileChangesMonitor(FcnMode mode) {
            // Possible values for DWORD FCNMode:
            //       does not exist == default behavior (create DirectoryMonitor for each subdir)
            //              0 or >2 == default behavior (create DirectoryMonitor for each subdir)
            //                    1 == disable File Change Notifications (FCN)
            //                    2 == create 1 DirectoryMonitor for AppPathInternal and watch subtrees
            switch (mode) {
                case FcnMode.NotSet:
                    // If the mode is not set, we use the registry key's value
                    UnsafeNativeMethods.GetDirMonConfiguration(out _FCNMode);
                    break;
                case FcnMode.Disabled:
                    _FCNMode = 1;
                    break;
                case FcnMode.Single:
                    _FCNMode = 2;
                    break;
                case FcnMode.Default:
                default:
                    _FCNMode = 0;
                    break;
            }

            if (IsFCNDisabled) {
                return;
            }

            _aliases = Hashtable.Synchronized(new Hashtable(StringComparer.OrdinalIgnoreCase));
            _dirs    = new Hashtable(StringComparer.OrdinalIgnoreCase);
            _subDirDirMons = new Hashtable(StringComparer.OrdinalIgnoreCase);

            if (_FCNMode == 2 && HttpRuntime.AppDomainAppPathInternal != null) {
                _appPathInternal = GetFullPath(HttpRuntime.AppDomainAppPathInternal);
                _dirMonAppPathInternal = new DirectoryMonitor(_appPathInternal, _FCNMode);
            }

#if DBG
            if ((int)Misc.GetAspNetRegValue(null /*subKey*/, "FCMRemoveTargetAssert", 0) > 0) {
                s_enableRemoveTargetAssert = true;
            }
#endif

        }

        internal bool IsDirNameMonitored(string fullPath, string dirName) {
            // is it one of the not-so-special directories we're monitoring?
            if (_dirs.ContainsKey(fullPath)) {
                return true;
            }
            // is it one of the special directories (bin, App_Code, etc) or a subfolder?
            foreach (string specialDirName in s_dirsToMonitor) {
                if (StringUtil.StringStartsWithIgnoreCase(dirName, specialDirName)) {
                    // a special directory?
                    if (dirName.Length == specialDirName.Length) {
                        return true;
                    }
                    // a subfolder?
                    else if (dirName.Length > specialDirName.Length && dirName[specialDirName.Length] == Path.DirectorySeparatorChar) {
                        return true;
                    }
                }
            }
            // Dev10 
            if (dirName.IndexOf(HttpRuntime.LocalResourcesDirectoryName, StringComparison.OrdinalIgnoreCase) > -1) {
                return true;
            }
            // we're not monitoring it
            return false;
        }

        //
        // Find the directory monitor. If not found, maybe add it.
        // If the directory is not actively monitoring, ensure that
        // it still represents an accessible directory.
        //
        DirectoryMonitor FindDirectoryMonitor(string dir, bool addIfNotFound, bool throwOnError) {
            DirectoryMonitor dirMon;
            FileAttributesData fad = null;
            int hr;

            dirMon = (DirectoryMonitor)_dirs[dir];
            if (dirMon != null) {
                if (!dirMon.IsMonitoring()) {
                    hr = FileAttributesData.GetFileAttributes(dir, out fad);
                    if (hr != HResults.S_OK || (fad.FileAttributes & FileAttributes.Directory) == 0) {
                        dirMon = null;
                    }
                }
            }

            if (dirMon != null || !addIfNotFound) {
                return dirMon;
            }

            lock (_dirs.SyncRoot) {
                // Check again, this time under synchronization.
                dirMon = (DirectoryMonitor)_dirs[dir];
                if (dirMon != null) {
                    if (!dirMon.IsMonitoring()) {
                        // Fail if it's not a directory or inaccessible.
                        hr = FileAttributesData.GetFileAttributes(dir, out fad);
                        if (hr == HResults.S_OK && (fad.FileAttributes & FileAttributes.Directory) == 0) {
                            // Fail if it's not a directory.
                            hr = HResults.E_INVALIDARG;
                        }

                        if (hr != HResults.S_OK) {
                            // Not accessible or a dir, so stop monitoring and remove.
                            _dirs.Remove(dir);
                            dirMon.StopMonitoring();
                            if (addIfNotFound && throwOnError) {
                                throw FileChangesMonitor.CreateFileMonitoringException(hr, dir);
                            }

                            return null;
                        }
                    }
                }
                else if (addIfNotFound) {
                    // Fail if it's not a directory or inaccessible.
                    hr = FileAttributesData.GetFileAttributes(dir, out fad);
                    if (hr == HResults.S_OK && (fad.FileAttributes & FileAttributes.Directory) == 0) {
                        hr = HResults.E_INVALIDARG;
                    }

                    if (hr == HResults.S_OK) {
                        // Add a new directory monitor.
                        dirMon = new DirectoryMonitor(dir, false, UnsafeNativeMethods.RDCW_FILTER_FILE_AND_DIR_CHANGES, _FCNMode);
                        _dirs.Add(dir, dirMon);
                    }
                    else if (throwOnError) {
                        throw FileChangesMonitor.CreateFileMonitoringException(hr, dir);
                    }
                }
            }

            return dirMon;
        }

        // Remove the aliases of a file monitor.
        internal void RemoveAliases(FileMonitor fileMon) {
            if (IsFCNDisabled) {
                return;
            }
             
            foreach (DictionaryEntry entry in fileMon.Aliases) {
                if (_aliases[entry.Key] == fileMon) {
                    _aliases.Remove(entry.Key);
                }
            }
        }

        //
        // Request to monitor a file, which may or may not exist.
        //
        internal DateTime StartMonitoringFile(string alias, FileChangeEventHandler callback) {
            Debug.Trace("FileChangesMonitor", "StartMonitoringFile\n" + "\tArgs: File=" + alias + "; Callback=" + callback.Target + "(HC=" + callback.Target.GetHashCode().ToString("x", NumberFormatInfo.InvariantInfo) + ")");

            FileMonitor         fileMon;
            DirectoryMonitor    dirMon;
            string              fullPathName, dir, file;
            bool                addAlias = false;

            if (alias == null) {
                throw CreateFileMonitoringException(HResults.E_INVALIDARG, alias);
            }

            if (IsFCNDisabled) {
                fullPathName = GetFullPath(alias);
                FindFileData ffd = null;
                int hr = FindFileData.FindFile(fullPathName, out ffd);
                if (hr == HResults.S_OK) {
                    return ffd.FileAttributesData.UtcLastWriteTime;
                }
                else {
                    return DateTime.MinValue;
                }
            }

            using (new ApplicationImpersonationContext()) {
                _lockDispose.AcquireReaderLock();
                try{
                    // Don't start monitoring if disposed.
                    if (_disposed) {
                        return DateTime.MinValue;
                    }

                    fileMon = (FileMonitor)_aliases[alias];
                    if (fileMon != null) {
                        // Used the cached directory monitor and file name.
                        dirMon = fileMon.DirectoryMonitor;
                        file = fileMon.FileNameLong;
                    }
                    else {
                        addAlias = true;

                        if (alias.Length == 0 || !UrlPath.IsAbsolutePhysicalPath(alias)) {
                            throw CreateFileMonitoringException(HResults.E_INVALIDARG, alias);
                        }

                        //
                        // Get the directory and file name, and lookup 
                        // the directory monitor.
                        //
                        fullPathName = GetFullPath(alias);
                        
                        if (IsBeneathAppPathInternal(fullPathName)) {
                            dirMon = _dirMonAppPathInternal;
                            file = fullPathName.Substring(_appPathInternal.Length+1);
                        }
                        else {
                            dir = UrlPath.GetDirectoryOrRootName(fullPathName);
                            file = Path.GetFileName(fullPathName);
                            if (String.IsNullOrEmpty(file)) {
                                // not a file
                                throw CreateFileMonitoringException(HResults.E_INVALIDARG, alias);
                            }
                            dirMon = FindDirectoryMonitor(dir, true /*addIfNotFound*/, true /*throwOnError*/);
                        }
                    }

                    fileMon = dirMon.StartMonitoringFileWithAssert(file, callback, alias);
                    if (addAlias) {
                        _aliases[alias] = fileMon;
                    }
                }
                finally {
                    _lockDispose.ReleaseReaderLock();
                }

                FileAttributesData fad;
                fileMon.DirectoryMonitor.GetFileAttributes(file, out fad);

                Debug.Dump("FileChangesMonitor", this);

                if (fad != null) {
                    return fad.UtcLastWriteTime;
                }
                else {
                    return DateTime.MinValue;
                }
            }
        }

        //
        // Request to monitor a path, which may be file, directory, or non-existent
        // file.
        //
        internal DateTime StartMonitoringPath(string alias, FileChangeEventHandler callback, out FileAttributesData fad) {
            Debug.Trace("FileChangesMonitor", "StartMonitoringPath\n" + "\tArgs: File=" + alias + "; Callback=" + callback.Target + "(HC=" + callback.Target.GetHashCode().ToString("x", NumberFormatInfo.InvariantInfo) + ")");

            FileMonitor         fileMon = null;
            DirectoryMonitor    dirMon = null;
            string              fullPathName, dir, file = null;
            bool                addAlias = false;

            fad = null;

            if (alias == null) {
                throw new HttpException(SR.GetString(SR.Invalid_file_name_for_monitoring, String.Empty));
            }
            
            if (IsFCNDisabled) {
                fullPathName = GetFullPath(alias);
                FindFileData ffd = null;
                int hr = FindFileData.FindFile(fullPathName, out ffd);
                if (hr == HResults.S_OK) {
                    fad = ffd.FileAttributesData;
                    return ffd.FileAttributesData.UtcLastWriteTime;
                }
                else {
                    return DateTime.MinValue;
                }
            }

            using (new ApplicationImpersonationContext()) {
                _lockDispose.AcquireReaderLock();
                try{
                    if (_disposed) {
                        return DateTime.MinValue;
                    }

                    // do/while loop once to make breaking out easy
                    do {
                        fileMon = (FileMonitor)_aliases[alias];
                        if (fileMon != null) {
                            // Used the cached directory monitor and file name.
                            file = fileMon.FileNameLong;
                            fileMon = fileMon.DirectoryMonitor.StartMonitoringFileWithAssert(file, callback, alias);
                            continue;
                        }

                        addAlias = true;

                        if (alias.Length == 0 || !UrlPath.IsAbsolutePhysicalPath(alias)) {
                            throw new HttpException(SR.GetString(SR.Invalid_file_name_for_monitoring, HttpRuntime.GetSafePath(alias)));
                        }

                        fullPathName = GetFullPath(alias);

                        // see if the path is beneath HttpRuntime.AppDomainAppPathInternal
                        if (IsBeneathAppPathInternal(fullPathName)) {
                            dirMon = _dirMonAppPathInternal;
                            file = fullPathName.Substring(_appPathInternal.Length+1);
                            fileMon = dirMon.StartMonitoringFileWithAssert(file, callback, alias);
                            continue;
                        }

                        // try treating the path as a directory
                        dirMon = FindDirectoryMonitor(fullPathName, false, false);
                        if (dirMon != null) {
                            fileMon = dirMon.StartMonitoringFileWithAssert(null, callback, alias);
                            continue;
                        }

                        // try treaing the path as a file
                        dir = UrlPath.GetDirectoryOrRootName(fullPathName);
                        file = Path.GetFileName(fullPathName);
                        if (!String.IsNullOrEmpty(file)) {
                            dirMon = FindDirectoryMonitor(dir, false, false);
                            if (dirMon != null) {
                                // try to add it - a file is the common case,
                                // and we avoid hitting the disk twice
                                try {
                                    fileMon = dirMon.StartMonitoringFileWithAssert(file, callback, alias);
                                }
                                catch {
                                }

                                if (fileMon != null) {
                                    continue;
                                }
                            }
                        }

                        // We aren't monitoring this path or its parent directory yet. 
                        // Hit the disk to determine if it's a directory or file.
                        dirMon = FindDirectoryMonitor(fullPathName, true, false);
                        if (dirMon != null) {
                            // It's a directory, so monitor all changes in it
                            file = null;
                        }
                        else {
                            // It's not a directory, so treat as file
                            if (String.IsNullOrEmpty(file)) {
                                throw CreateFileMonitoringException(HResults.E_INVALIDARG, alias);
                            }

                            dirMon = FindDirectoryMonitor(dir, true, true);
                        }

                        fileMon = dirMon.StartMonitoringFileWithAssert(file, callback, alias);
                    } while (false);

                    if (!fileMon.IsDirectory) {
                        fileMon.DirectoryMonitor.GetFileAttributes(file, out fad);
                    }

                    if (addAlias) {
                        _aliases[alias] = fileMon;
                    }
                }
                finally {
                    _lockDispose.ReleaseReaderLock();
                }

                Debug.Dump("FileChangesMonitor", this);

                if (fad != null) {
                    return fad.UtcLastWriteTime;
                }
                else {
                    return DateTime.MinValue;
                }
            }
        }

        //
        // Request to monitor the bin directory and directory renames anywhere under app
        //

        internal void StartMonitoringDirectoryRenamesAndBinDirectory(string dir, FileChangeEventHandler callback) {
            Debug.Trace("FileChangesMonitor", "StartMonitoringDirectoryRenamesAndBinDirectory\n" + "\tArgs: File=" + dir + "; Callback=" + callback.Target + "(HC=" + callback.Target.GetHashCode().ToString("x", NumberFormatInfo.InvariantInfo) + ")");

            if (String.IsNullOrEmpty(dir)) {
                throw new HttpException(SR.GetString(SR.Invalid_file_name_for_monitoring, String.Empty));
            }

            if (IsFCNDisabled) {
                return;
            }

#if DBG
            Debug.Assert(_dirs.Count == 0, "This function must be called before monitoring other directories, otherwise monitoring of UNC directories will be unreliable on Windows2000 Server.");
#endif
            using (new ApplicationImpersonationContext()) {
                _lockDispose.AcquireReaderLock();
                try {
                    if (_disposed) {
                        return;
                    }

                    _callbackRenameOrCriticaldirChange = callback;

                    string dirRoot = GetFullPath(dir);

                    // Monitor bin directory and app directory (for renames only) separately
                    // to avoid overwhelming changes when the user writes to a subdirectory
                    // of the app directory.

                    _dirMonSubdirs = new DirectoryMonitor(dirRoot, true, UnsafeNativeMethods.RDCW_FILTER_DIR_RENAMES, true, _FCNMode);
                    try {
                        _dirMonSubdirs.StartMonitoringFileWithAssert(null, new FileChangeEventHandler(this.OnSubdirChange), dirRoot);
                    }
                    catch {
                        ((IDisposable)_dirMonSubdirs).Dispose();
                        _dirMonSubdirs = null;
                        throw;
                    }

                    _dirMonSpecialDirs = new ArrayList();
                    for (int i=0; i<s_dirsToMonitor.Length; i++) {
                        _dirMonSpecialDirs.Add(ListenToSubdirectoryChanges(dirRoot, s_dirsToMonitor[i]));
                    }
                }
                finally {
                    _lockDispose.ReleaseReaderLock();
                }
            }
        }

        //
        // Monitor a directory that causes an appdomain shutdown when it changes
        //
        internal void StartListeningToLocalResourcesDirectory(VirtualPath virtualDir) {
            Debug.Trace("FileChangesMonitor", "StartListeningToVirtualSubdirectory\n" + "\tArgs: virtualDir=" + virtualDir);

            if (IsFCNDisabled) {
                return;
            }

            // In some situation (not well understood yet), we get here with either
            // _callbackRenameOrCriticaldirChange or _dirMonSpecialDirs being null (VSWhidbey #215040).
            // When that happens, just return.
            //Debug.Assert(_callbackRenameOrCriticaldirChange != null);
            //Debug.Assert(_dirMonSpecialDirs != null);
            if (_callbackRenameOrCriticaldirChange == null || _dirMonSpecialDirs == null)
                return;

            using (new ApplicationImpersonationContext()) {
                _lockDispose.AcquireReaderLock();
                try {
                    if (_disposed) {
                        return;
                    }

                    // Get the physical path, and split it into the parent dir and the dir name
                    string dir = virtualDir.MapPath();
                    dir = FileUtil.RemoveTrailingDirectoryBackSlash(dir);
                    string name = Path.GetFileName(dir);
                    dir = Path.GetDirectoryName(dir);

                    // If the physical parent directory doesn't exist, don't do anything.
                    // This could happen when using a non-file system based VirtualPathProvider
                    if (!Directory.Exists(dir))
                        return;

                    _dirMonSpecialDirs.Add(ListenToSubdirectoryChanges(dir, name));
                }
                finally {
                    _lockDispose.ReleaseReaderLock();
                }
            }
        }

        DirectoryMonitor ListenToSubdirectoryChanges(string dirRoot, string dirToListenTo) {

            string dirRootSubDir;
            DirectoryMonitor dirMonSubDir;

            if (StringUtil.StringEndsWith(dirRoot, '\\')) {
                dirRootSubDir = dirRoot + dirToListenTo;
            }
            else {
                dirRootSubDir = dirRoot + "\\" + dirToListenTo;
            }

            if (IsBeneathAppPathInternal(dirRootSubDir)) {
                dirMonSubDir = _dirMonAppPathInternal;

                dirToListenTo = dirRootSubDir.Substring(_appPathInternal.Length+1);
                Debug.Trace("ListenToSubDir", dirRoot + " " + dirToListenTo);
                dirMonSubDir.StartMonitoringFileWithAssert(dirToListenTo, new FileChangeEventHandler(this.OnCriticaldirChange), dirRootSubDir);
            }
            else if (Directory.Exists(dirRootSubDir)) {
                dirMonSubDir = new DirectoryMonitor(dirRootSubDir, true, UnsafeNativeMethods.RDCW_FILTER_FILE_CHANGES, _FCNMode);
                try {
                    dirMonSubDir.StartMonitoringFileWithAssert(null, new FileChangeEventHandler(this.OnCriticaldirChange), dirRootSubDir);
                }
                catch {
                    ((IDisposable)dirMonSubDir).Dispose();
                    dirMonSubDir = null;
                    throw;
                }
            }
            else {
                dirMonSubDir = (DirectoryMonitor)_subDirDirMons[dirRoot];
                if (dirMonSubDir == null) {
                    dirMonSubDir = new DirectoryMonitor(dirRoot, false, UnsafeNativeMethods.RDCW_FILTER_FILE_AND_DIR_CHANGES, _FCNMode);
                    _subDirDirMons[dirRoot] = dirMonSubDir;
                }

                try {
                    dirMonSubDir.StartMonitoringFileWithAssert(dirToListenTo, new FileChangeEventHandler(this.OnCriticaldirChange), dirRootSubDir);
                }
                catch {
                    ((IDisposable)dirMonSubDir).Dispose();
                    dirMonSubDir = null;
                    throw;
                }
            }

            return dirMonSubDir;
        }

        void OnSubdirChange(Object sender, FileChangeEvent e) {
            try {
                Interlocked.Increment(ref _activeCallbackCount);

                if (_disposed) {
                    return;
                }

                Debug.Trace("FileChangesMonitor", "OnSubdirChange\n" + "\tArgs: Action=" + e.Action + "; fileName=" + e.FileName);
                FileChangeEventHandler handler = _callbackRenameOrCriticaldirChange;
                if (    handler != null &&
                        (e.Action == FileAction.Error || e.Action == FileAction.Overwhelming || e.Action == FileAction.RenamedOldName || e.Action == FileAction.Removed)) {
                    Debug.Trace("FileChangesMonitor", "Firing subdir change event\n" + "\tArgs: Action=" + e.Action + "; fileName=" + e.FileName + "; Target=" + handler.Target + "(HC=" + handler.Target.GetHashCode().ToString("x", NumberFormatInfo.InvariantInfo) + ")");
                    
                    HttpRuntime.SetShutdownMessage(
                        SR.GetString(SR.Directory_rename_notification, e.FileName));
                    
                    handler(this, e);
                }
            }
            finally {
                Interlocked.Decrement(ref _activeCallbackCount);
            }
        }

        void OnCriticaldirChange(Object sender, FileChangeEvent e) {
            try {
                Interlocked.Increment(ref _activeCallbackCount);

                if (_disposed) {
                    return;
                }

                Debug.Trace("FileChangesMonitor", "OnCriticaldirChange\n" + "\tArgs: Action=" + e.Action + "; fileName=" + e.FileName);
                HttpRuntime.SetShutdownMessage(SR.GetString(SR.Change_notification_critical_dir));
                FileChangeEventHandler handler = _callbackRenameOrCriticaldirChange;
                if (handler != null) {
                    handler(this, e);
                }
            }
            finally {
                Interlocked.Decrement(ref _activeCallbackCount);
            }
        }

        //
        // Request to stop monitoring a file.
        //
        internal void StopMonitoringFile(string alias, object target) {
            Debug.Trace("FileChangesMonitor", "StopMonitoringFile\n" + "File=" + alias + "; Callback=" + target);

            if (IsFCNDisabled) {
                return;
            }

            FileMonitor         fileMon;
            DirectoryMonitor    dirMon = null;
            string              fullPathName, file = null, dir;

            if (alias == null) {
                throw new HttpException(SR.GetString(SR.Invalid_file_name_for_monitoring, String.Empty));
            }

            using (new ApplicationImpersonationContext()) {
                _lockDispose.AcquireReaderLock();
                try {
                    if (_disposed) {
                        return;
                    }

                    fileMon = (FileMonitor)_aliases[alias];
                    if (fileMon != null && !fileMon.IsDirectory) {
                        // Used the cached directory monitor and file name
                        dirMon = fileMon.DirectoryMonitor;
                        file = fileMon.FileNameLong;
                    }
                    else {
                        if (alias.Length == 0 || !UrlPath.IsAbsolutePhysicalPath(alias)) {
                            throw new HttpException(SR.GetString(SR.Invalid_file_name_for_monitoring, HttpRuntime.GetSafePath(alias)));
                        }

                        // Lookup the directory monitor
                        fullPathName = GetFullPath(alias);
                        dir = UrlPath.GetDirectoryOrRootName(fullPathName);
                        file = Path.GetFileName(fullPathName);
                        if (String.IsNullOrEmpty(file)) {
                            // not a file
                            throw new HttpException(SR.GetString(SR.Invalid_file_name_for_monitoring, HttpRuntime.GetSafePath(alias)));
                        }

                        dirMon = FindDirectoryMonitor(dir, false, false);
                    }

                    if (dirMon != null) {
                        dirMon.StopMonitoringFile(file, target);
                    }
                }
                finally {
                    _lockDispose.ReleaseReaderLock();
                }
            }
        }

        //
        // Request to stop monitoring a file.
        // 
        internal void StopMonitoringPath(String alias, object target) {
            Debug.Trace("FileChangesMonitor", "StopMonitoringFile\n" + "File=" + alias + "; Callback=" + target);

            if (IsFCNDisabled) {
                return;
            }

            FileMonitor         fileMon;
            DirectoryMonitor    dirMon = null;
            string              fullPathName, file = null, dir;

            if (alias == null) {
                throw new HttpException(SR.GetString(SR.Invalid_file_name_for_monitoring, String.Empty));
            }

            using (new ApplicationImpersonationContext()) {
                _lockDispose.AcquireReaderLock();
                try {
                    if (_disposed) {
                        return;
                    }

                    fileMon = (FileMonitor)_aliases[alias];
                    if (fileMon != null) {
                        // Used the cached directory monitor and file name.
                        dirMon = fileMon.DirectoryMonitor;
                        file = fileMon.FileNameLong;
                    }
                    else {
                        if (alias.Length == 0 || !UrlPath.IsAbsolutePhysicalPath(alias)) {
                            throw new HttpException(SR.GetString(SR.Invalid_file_name_for_monitoring, HttpRuntime.GetSafePath(alias)));
                        }

                        // try treating the path as a directory
                        fullPathName = GetFullPath(alias);
                        dirMon = FindDirectoryMonitor(fullPathName, false, false);
                        if (dirMon == null) {
                            // try treaing the path as a file
                            dir = UrlPath.GetDirectoryOrRootName(fullPathName);
                            file = Path.GetFileName(fullPathName);
                            if (!String.IsNullOrEmpty(file)) {
                                dirMon = FindDirectoryMonitor(dir, false, false);
                            }
                        }
                    }

                    if (dirMon != null) {
                        dirMon.StopMonitoringFile(file, target);
                    }
                }
                finally {
                    _lockDispose.ReleaseReaderLock();
                }
            }
        }

         //
         // Returns the last modified time of the file. If the 
         // file does not exist, returns DateTime.MinValue.
         //
         internal FileAttributesData GetFileAttributes(string alias) {
             FileMonitor        fileMon;
             DirectoryMonitor   dirMon = null;
             string             fullPathName, file = null, dir;
             FileAttributesData fad = null;

             if (alias == null) {
                 throw FileChangesMonitor.CreateFileMonitoringException(HResults.E_INVALIDARG, alias);
             }

             if (IsFCNDisabled) {
                 if (alias.Length == 0 || !UrlPath.IsAbsolutePhysicalPath(alias)) {
                     throw FileChangesMonitor.CreateFileMonitoringException(HResults.E_INVALIDARG, alias);
                 }

                 fullPathName = GetFullPath(alias);
                 FindFileData ffd = null;
                 int hr = FindFileData.FindFile(fullPathName, out ffd);
                 if (hr == HResults.S_OK) {
                     return ffd.FileAttributesData;
                 }
                 else {
                     return null;
                 }   
             }

             using (new ApplicationImpersonationContext()) {
                 _lockDispose.AcquireReaderLock();
                try {
                    if (!_disposed) {
                        fileMon = (FileMonitor)_aliases[alias];
                        if (fileMon != null && !fileMon.IsDirectory) {
                            // Used the cached directory monitor and file name.
                            dirMon = fileMon.DirectoryMonitor;
                            file = fileMon.FileNameLong;
                        }
                        else {
                            if (alias.Length == 0 || !UrlPath.IsAbsolutePhysicalPath(alias)) {
                                throw FileChangesMonitor.CreateFileMonitoringException(HResults.E_INVALIDARG, alias);
                            }

                            // Lookup the directory monitor
                            fullPathName = GetFullPath(alias);
                            dir = UrlPath.GetDirectoryOrRootName(fullPathName);
                            file = Path.GetFileName(fullPathName);
                            if (!String.IsNullOrEmpty(file)) {
                                dirMon = FindDirectoryMonitor(dir, false, false);
                            }
                        }
                    }
                 }
                 finally {
                     _lockDispose.ReleaseReaderLock();
                 }

                 // If we're not monitoring the file, get the attributes.
                 if (dirMon == null || !dirMon.GetFileAttributes(file, out fad)) {
                     FileAttributesData.GetFileAttributes(alias, out fad);
                 }

                 return fad;
             }
        }

        //
        // Request to stop monitoring everything -- release all native resources
        //
        internal void Stop() {
            Debug.Trace("FileChangesMonitor", "Stop!");

             if (IsFCNDisabled) {
                 return;
             }

            using (new ApplicationImpersonationContext()) {
                _lockDispose.AcquireWriterLock();
                try {
                    _disposed = true;
                }
                finally {
                    _lockDispose.ReleaseWriterLock();
                }

                // wait for executing callbacks to complete
                while(_activeCallbackCount != 0) {
                    Thread.Sleep(250);
                }

                if (_dirMonSubdirs != null) {
                    _dirMonSubdirs.StopMonitoring();
                    _dirMonSubdirs = null;
                }

                if (_dirMonSpecialDirs != null) {
                    foreach (DirectoryMonitor dirMon in _dirMonSpecialDirs) {
                        if (dirMon != null) {
                            dirMon.StopMonitoring();
                        }
                    }

                    _dirMonSpecialDirs = null;
                }

                _callbackRenameOrCriticaldirChange = null;

                if (_dirs != null) {
                    IDictionaryEnumerator e = _dirs.GetEnumerator();
                    while (e.MoveNext()) {
                        DirectoryMonitor dirMon = (DirectoryMonitor)e.Value;
                        dirMon.StopMonitoring();
                    }
                }

                _dirs.Clear();
                _aliases.Clear();

                // Don't allow the AppDomain to unload while we have
                // active DirMonCompletions
                while (DirMonCompletion.ActiveDirMonCompletions != 0) {
                    Thread.Sleep(10);
                }
            }

            Debug.Dump("FileChangesMonitor", this);
        }

#if DBG
        internal string DebugDescription(string indent) {
            StringBuilder   sb = new StringBuilder(200);
            string          i2 = indent + "    ";
            DictionaryEntryCaseInsensitiveComparer  decomparer = new DictionaryEntryCaseInsensitiveComparer();

            sb.Append(indent + "System.Web.FileChangesMonitor\n");
            if (_dirMonSubdirs != null) {
                sb.Append(indent + "_dirMonSubdirs\n");
                sb.Append(_dirMonSubdirs.DebugDescription(i2));
            }

            if (_dirMonSpecialDirs != null) {
                for (int i=0; i<s_dirsToMonitor.Length; i++) {
                    if (_dirMonSpecialDirs[i] != null) {
                        sb.Append(indent + "_dirMon" + s_dirsToMonitor[i] + "\n");
                        sb.Append(((DirectoryMonitor)_dirMonSpecialDirs[i]).DebugDescription(i2));
                    }
                }
            }

            sb.Append(indent + "_dirs " + _dirs.Count + " directory monitors...\n");

            DictionaryEntry[] dirEntries = new DictionaryEntry[_dirs.Count];
            _dirs.CopyTo(dirEntries, 0);
            Array.Sort(dirEntries, decomparer);
            
            foreach (DictionaryEntry d in dirEntries) {
                DirectoryMonitor dirMon = (DirectoryMonitor)d.Value;
                sb.Append(dirMon.DebugDescription(i2));
            }

            return sb.ToString();
        }
#endif

#else // !FEATURE_PAL stubbing

        internal static string[] s_dirsToMonitor = new string[] {
        };

        internal DateTime StartMonitoringFile(string alias, FileChangeEventHandler callback)
        {
            return DateTime.Now;
        }
        
        internal DateTime StartMonitoringPath(string alias, FileChangeEventHandler callback)
        {
            return DateTime.Now;
        }

        internal void StopMonitoringPath(String alias, object target) 
        {
        }

        internal void StartMonitoringDirectoryRenamesAndBinDirectory(string dir, FileChangeEventHandler callback) 
        {
        }
        
        internal void Stop() 
        {
        }                

#endif // !FEATURE_PAL
    }

#if DBG
    internal sealed class DictionaryEntryCaseInsensitiveComparer : IComparer {
        IComparer _cicomparer = StringComparer.OrdinalIgnoreCase;

        internal DictionaryEntryCaseInsensitiveComparer() {}
        
        int IComparer.Compare(object x, object y) {
            string a = (string) ((DictionaryEntry) x).Key;
            string b = (string) ((DictionaryEntry) y).Key;

            if (a != null && b != null) {
                return _cicomparer.Compare(a, b);
            }
            else {
                return InvariantComparer.Default.Compare(a, b);            
            }
        }
    }
#endif

#if DBG
    internal sealed class DictionaryEntryTypeComparer : IComparer {
        IComparer _cicomparer = StringComparer.OrdinalIgnoreCase;

        internal DictionaryEntryTypeComparer() {}

        int IComparer.Compare(object x, object y) {
            object a = ((DictionaryEntry) x).Key;
            object b = ((DictionaryEntry) y).Key;

            string i = null, j = null;
            if (a != null) {
                i = a.GetType().ToString();
            }

            if (b != null) {
                j = b.GetType().ToString();
            }

            if (i != null && j != null) {
                return _cicomparer.Compare(i, j);
            }
            else {
                return InvariantComparer.Default.Compare(i, j);            
            }
        }
    }
#endif
}
