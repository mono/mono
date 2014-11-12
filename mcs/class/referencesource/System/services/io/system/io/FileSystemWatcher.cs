//------------------------------------------------------------------------------
// <copyright file="FileSystemWatcher.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.IO {
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System.Security.Permissions;
    using System.Security;
    using System.Globalization;
    using System.Runtime.Versioning;

    /// <devdoc>
    ///    <para>Listens to the system directory change notifications and
    ///       raises events when a directory or file within a directory changes.</para>
    /// </devdoc>
    [
    DefaultEvent("Changed"),
    // Disabling partial trust scenarios
    PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"),
    PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"),
    IODescription(SR.FileSystemWatcherDesc)
    ]
    public class FileSystemWatcher : Component, ISupportInitialize {
        /// <devdoc>
        ///     Private instance variables
        /// </devdoc>
        // Directory being monitored
        private string directory;

        // Filter for name matching
        private string filter;

        // Unmanaged handle to monitored directory
        private SafeFileHandle directoryHandle;

        // The watch filter for the API call.
        private const NotifyFilters defaultNotifyFilters = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
        private NotifyFilters notifyFilters = defaultNotifyFilters;

        // Flag to watch subtree of this directory
        private bool includeSubdirectories = false;

        // Flag to note whether we are attached to the thread pool and responding to changes
        private bool enabled = false;

        // Are we in init?
        private bool initializing = false;

        // Buffer size
        private int internalBufferSize = 8192;
                
        // Used for synchronization
        private WaitForChangedResult changedResult;
        private bool isChanged = false;
        private ISynchronizeInvoke synchronizingObject;
        private bool readGranted;
        private bool disposed;
        // Current "session" ID to ignore old events whenever we stop then 
        // restart.
        private int currentSession;

        // Event handlers
        private FileSystemEventHandler onChangedHandler = null;
        private FileSystemEventHandler onCreatedHandler = null;
        private FileSystemEventHandler onDeletedHandler = null;
        private RenamedEventHandler onRenamedHandler = null;
        private ErrorEventHandler onErrorHandler = null;

        // Thread gate holder and constats
        private bool stopListening = false;        

        // Used for async method
        private bool runOnce = false;

        // To validate the input for "path"
        private static readonly char[] wildcards = new char[] { '?', '*' };

        private static int notifyFiltersValidMask;        

        // Additional state information to pass to callback.  Note that we
        // never return this object to users, but we do pass state in it.
        private sealed class FSWAsyncResult : IAsyncResult
        {
            internal int session;
            internal byte[] buffer;

            public bool IsCompleted                { get { throw new NotImplementedException(); } }
            public WaitHandle AsyncWaitHandle    { get { throw new NotImplementedException(); } }            
            public Object AsyncState            { get { throw new NotImplementedException(); } }                
            public bool CompletedSynchronously    { get { throw new NotImplementedException(); } }                
        }

        static FileSystemWatcher() {
            notifyFiltersValidMask = 0;
            foreach (int enumValue in Enum.GetValues(typeof(NotifyFilters)))
                notifyFiltersValidMask |= enumValue;
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.IO.FileSystemWatcher'/> class.</para>
        /// </devdoc>
        public FileSystemWatcher() {
            this.directory = String.Empty;
            this.filter = "*.*";
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.IO.FileSystemWatcher'/> class,
        ///       given the specified directory to monitor.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public FileSystemWatcher(string path) : this(path, "*.*") {
        }


        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.IO.FileSystemWatcher'/> class,
        ///       given the specified directory and type of files to monitor.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public FileSystemWatcher(string path, string filter) {
            if (path == null)
                throw new ArgumentNullException("path");

            if (filter == null)
                throw new ArgumentNullException("filter");
            
            // Early check for directory parameter so that an exception can be thrown as early as possible.
            if (path.Length == 0 || !Directory.Exists(path))
                throw new ArgumentException(SR.GetString(SR.InvalidDirName, path));            

            this.directory = path;
            this.filter = filter;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the type of changes to watch for.
        ///    </para>
        /// </devdoc>
        [
        DefaultValue(defaultNotifyFilters),
        IODescription(SR.FSW_ChangedFilter)
        ]
        public NotifyFilters NotifyFilter {
            get {
                return notifyFilters;
            }
            set {
                if (((int) value & ~notifyFiltersValidMask) != 0)
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(NotifyFilters));                                                                                

                if (notifyFilters != value) {
                    notifyFilters = value;

                    Restart();
                }
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets a value indicating whether the component is enabled.</para>
        /// </devdoc>
        [
        DefaultValue(false),
        IODescription(SR.FSW_Enabled)
        ]
        public bool EnableRaisingEvents {
            get {
                return enabled;
            }
            set {

                if (enabled == value) {
                    return;
                }

                enabled = value;

                if (!IsSuspended()) {
                    if (enabled) {
                        StartRaisingEvents();
                    }
                    else {
                        StopRaisingEvents();
                    }
                }
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the filter string, used to determine what files are monitored in a directory.</para>
        /// </devdoc>
        [
        DefaultValue("*.*"),
        IODescription(SR.FSW_Filter),
        TypeConverter("System.Diagnostics.Design.StringValueConverter, " + AssemblyRef.SystemDesign),
        SettingsBindable(true),        
        ]
        public string Filter {
            get {
                return filter;
            }
            set {                
                if (String.IsNullOrEmpty(value)) {
                    value = "*.*";
                }
                if (String.Compare(filter, value, StringComparison.OrdinalIgnoreCase) != 0) {
                    filter = value;
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a
        ///       value indicating whether subdirectories within the specified path should be monitored.
        ///    </para>
        /// </devdoc>
        [
        DefaultValue(false),
        IODescription(SR.FSW_IncludeSubdirectories)
        ]
        public bool IncludeSubdirectories {
            get {
                return includeSubdirectories;
            }
            set {
                if (includeSubdirectories != value) {
                    includeSubdirectories = value;

                    Restart();
                }
            }
        }

        /// <devdoc>
        ///    <para>Gets or
        ///       sets the size of the internal buffer.</para>
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(8192)
        ]
        public int InternalBufferSize {
            get {
                return internalBufferSize;
            }
            set {
                if (internalBufferSize != value) {
                    if (value < 4096) {
                        value = 4096;
                    }

                    internalBufferSize = value;

                    Restart();
                }
            }
        }

        private bool IsHandleInvalid {
            get {
                return (directoryHandle == null || directoryHandle.IsInvalid);
            }
        }
        
        /// <devdoc>
        ///    <para>Gets or sets the path of the directory to watch.</para>
        /// </devdoc>
        [
        DefaultValue(""),
        IODescription(SR.FSW_Path),
        Editor("System.Diagnostics.Design.FSWPathEditor, " + AssemblyRef.SystemDesign, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),        
        TypeConverter("System.Diagnostics.Design.StringValueConverter, " + AssemblyRef.SystemDesign),
        SettingsBindable(true)
        ]
        public string Path {
            [ResourceExposure(ResourceScope.Machine)]
            [ResourceConsumption(ResourceScope.Machine)]
            get {
                return directory;
            }
            [ResourceExposure(ResourceScope.Machine)]
            [ResourceConsumption(ResourceScope.Machine)]
            set {
                value = (value == null) ? string.Empty : value;
                if (String.Compare(directory, value, StringComparison.OrdinalIgnoreCase) != 0) {
                    if (DesignMode) {
                        // Don't check the path if in design mode, try to do simple syntax check                 
                        if (value.IndexOfAny(FileSystemWatcher.wildcards) != -1 || value.IndexOfAny(System.IO.Path.GetInvalidPathChars()) != -1) {
                            throw new ArgumentException(SR.GetString(SR.InvalidDirName, value));
                        }
                    }
                    else {
                        if (!Directory.Exists(value))                             
                            throw new ArgumentException(SR.GetString(SR.InvalidDirName, value));                        
                    }
                    directory = value;
                    readGranted = false;
                    Restart();
                }
            }
        }

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        [Browsable(false)]
        public override ISite Site {
            get {
                return base.Site;
            }
            set {
                base.Site = value;

                // set EnableRaisingEvents to true at design time so the user
                // doesn't have to manually. We can't do this in
                // the constructor because in code it should
                // default to false.
                if (Site != null && Site.DesignMode)
                    EnableRaisingEvents = true;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the object used to marshal the event handler calls issued as a
        ///       result of a directory change.
        ///    </para>
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null), 
        IODescription(SR.FSW_SynchronizingObject)
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

        /// <devdoc>
        ///    <para>
        ///       Occurs when a file or directory in the specified <see cref='System.IO.FileSystemWatcher.Path'/>
        ///       is changed.
        ///    </para>
        /// </devdoc>
        [IODescription(SR.FSW_Changed)]
        public event FileSystemEventHandler Changed {
            add {
                onChangedHandler += value;
            }
            remove {                            
                onChangedHandler -= value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Occurs when a file or directory in the specified <see cref='System.IO.FileSystemWatcher.Path'/>
        ///       is created.
        ///    </para>
        /// </devdoc>
        [IODescription(SR.FSW_Created)]
        public event FileSystemEventHandler Created {
            add {
                onCreatedHandler += value;
            }
            remove {
                onCreatedHandler -= value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Occurs when a file or directory in the specified <see cref='System.IO.FileSystemWatcher.Path'/>
        ///       is deleted.
        ///    </para>
        /// </devdoc>
        [IODescription(SR.FSW_Deleted)]
        public event FileSystemEventHandler Deleted {
            add{
                onDeletedHandler += value;
            }
            remove {
                onDeletedHandler -= value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Occurs when the internal buffer overflows.
        ///    </para>
        /// </devdoc>
        [Browsable(false)]
        public event ErrorEventHandler Error {
            add {
                onErrorHandler += value;
            }
            remove {
                onErrorHandler -= value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Occurs when a file or directory in the specified <see cref='System.IO.FileSystemWatcher.Path'/>
        ///       is renamed.
        ///    </para>
        /// </devdoc>
        [IODescription(SR.FSW_Renamed)]
        public event RenamedEventHandler Renamed {
            add {
                onRenamedHandler += value;
            }
            remove {
                onRenamedHandler -= value;
            }
        }

        /// <devdoc>
        ///    <para>Notifies the object that initialization is beginning and tells it to standby.</para>
        /// </devdoc>
        public void BeginInit() {
            bool oldEnabled = enabled;
            StopRaisingEvents();
            enabled = oldEnabled;
            initializing = true;
        }

        /// <devdoc>
        ///     Callback from thread pool.
        /// </devdoc>
        /// <internalonly/>
        private unsafe void CompletionStatusChanged(uint errorCode, uint numBytes, NativeOverlapped * overlappedPointer) {

            Overlapped overlapped = Overlapped.Unpack(overlappedPointer);
            FSWAsyncResult asyncResult = (FSWAsyncResult) overlapped.AsyncResult;

            try {                

                if (stopListening) {
                    return;
                }

                lock (this) {

                    if (errorCode != 0) {
                        if (errorCode == 995 /* ERROR_OPERATION_ABORTED */) {
                            //Win2000 inside a service the first completion status is false
                            //cannot return without monitoring again.
                            //Because this return statement is inside a try/finally block,
                            //the finally block will execute. It does restart the monitoring.
                            return;
                        }
                        else {
                            OnError(new ErrorEventArgs(new Win32Exception((int)errorCode)));
                            EnableRaisingEvents = false;
                            return;
                        }
                    }

                    // Ignore any events that occurred before this "session",
                    // so we don't get changed or error events after we 
                    // told FSW to stop.
                    if (asyncResult.session != currentSession)                    
                        return;


                    if (numBytes == 0) {
                        NotifyInternalBufferOverflowEvent();
                    }
                    else {  // Else, parse each of them and notify appropriate delegates
    
                        /******
                            Format for the buffer is the following C struct:
    
                            typedef struct _FILE_NOTIFY_INFORMATION {
                               DWORD NextEntryOffset;
                               DWORD Action;
                               DWORD FileNameLength;
                               WCHAR FileName[1];
                            } FILE_NOTIFY_INFORMATION;
    
                            NOTE1: FileNameLength is length in bytes.
                            NOTE2: The Filename is a Unicode string that's NOT NULL terminated.
                            NOTE3: A NextEntryOffset of zero means that it's the last entry
                        *******/
    
                        // Parse the file notify buffer:
                        int offset = 0;
                        int nextOffset, action, nameLength;
                        string oldName = null;
                        string name = null;
    
                        do {

                            fixed (byte * buffPtr = asyncResult.buffer) {

                                // Get next offset:
                                nextOffset = *( (int *) (buffPtr + offset) );

                                // Get change flag:
                                action = *( (int *) (buffPtr + offset + 4) );

                                // Get filename length (in bytes):
                                nameLength = *( (int *) (buffPtr + offset + 8) );                                                                
                                name = new String( (char *) (buffPtr + offset + 12), 0, nameLength / 2);
                            }


                            /* A slightly convoluted piece of code follows.  Here's what's happening:
    
                               We wish to collapse the poorly done rename notifications from the
                               ReadDirectoryChangesW API into a nice rename event. So to do that,
                               it's assumed that a FILE_ACTION_RENAMED_OLD_NAME will be followed
                               immediately by a FILE_ACTION_RENAMED_NEW_NAME in the buffer, which is
                               all that the following code is doing.
    
                               On a FILE_ACTION_RENAMED_OLD_NAME, it asserts that no previous one existed
                               and saves its name.  If there are no more events in the buffer, it'll
                               assert and fire a RenameEventArgs with the Name field null.
    
                               If a NEW_NAME action comes in with no previous OLD_NAME, we assert and fire
                               a rename event with the OldName field null.
    
                               If the OLD_NAME and NEW_NAME actions are indeed there one after the other,
                               we'll fire the RenamedEventArgs normally and clear oldName.
    
                               If the OLD_NAME is followed by another action, we assert and then fire the
                               rename event with the Name field null and then fire the next action.
    
                               In case it's not a OLD_NAME or NEW_NAME action, we just fire the event normally.
    
                               (Phew!)
                             */
    
                            // If the action is RENAMED_FROM, save the name of the file
                            if (action == Direct.FILE_ACTION_RENAMED_OLD_NAME) {
                                Debug.Assert(oldName == null, "FileSystemWatcher: Two FILE_ACTION_RENAMED_OLD_NAME " +
                                                              "in a row!  [" + oldName + "], [ " + name + "]");
    
                                oldName = name;
                            }
                            else if (action == Direct.FILE_ACTION_RENAMED_NEW_NAME) {
                                if (oldName != null) {
                                    NotifyRenameEventArgs(WatcherChangeTypes.Renamed, name, oldName);
                                    oldName = null;
                                }
                                else {
                                    Debug.Assert(false, "FileSystemWatcher: FILE_ACTION_RENAMED_NEW_NAME with no" +
                                                                  "old name! [ " + name + "]");
    
                                    NotifyRenameEventArgs(WatcherChangeTypes.Renamed, name, oldName);
                                    oldName = null;
                                }
                            }
                            else {
                                if (oldName != null) {
                                    Debug.Assert(false, "FileSystemWatcher: FILE_ACTION_RENAMED_OLD_NAME with no" +
                                                                  "new name!  [" + oldName + "]");
    
                                    NotifyRenameEventArgs(WatcherChangeTypes.Renamed, null, oldName);
                                    oldName = null;
                                }
    
                                // Notify each file of change
                                NotifyFileSystemEventArgs(action, name);
    
                            }
    
                            offset += nextOffset;
                        } while (nextOffset != 0);
    
                        if (oldName != null) {
                            Debug.Assert(false, "FileSystemWatcher: FILE_ACTION_RENAMED_OLD_NAME with no" +
                                                          "new name!  [" + oldName + "]");
    
                            NotifyRenameEventArgs(WatcherChangeTypes.Renamed, null, oldName);
                            oldName = null;
                        }
                    }                                                                        
                }
            }
            finally {
                Overlapped.Free(overlappedPointer);
                if (!stopListening && !runOnce) {
                    Monitor(asyncResult.buffer);
                } 
            }                                                    
        }                            

        /// <devdoc>
        /// </devdoc>
        protected override void Dispose(bool disposing) {
            try {
                if (disposing) {
                    
                    //Stop raising events cleans up managed and
                    //unmanaged resources.                    
                    StopRaisingEvents();

                    // Clean up managed resources
                    onChangedHandler = null;
                    onCreatedHandler = null;
                    onDeletedHandler = null;
                    onRenamedHandler = null;
                    onErrorHandler = null;
                    readGranted = false;
                
                } else {
                    stopListening = true;
                             
                    // Clean up unmanaged resources
                    if (!IsHandleInvalid) {
                        directoryHandle.Close();
                    }                                                          
                }     
           
            } finally {
                this.disposed = true;
                base.Dispose(disposing);
            }
        }
                             
        /// <devdoc>
        ///    <para>
        ///       Notifies the object that initialization is complete.
        ///    </para>
        /// </devdoc>
        public void EndInit() {
            initializing = false;
            // Unless user told us NOT to start after initialization, we'll start listening
            // to events
            if (directory.Length != 0 && enabled == true)
                StartRaisingEvents();            
        }        

        
        /// <devdoc>
        ///     Returns true if the component is either in a Begin/End Init block or in design mode.
        /// </devdoc>
        // <internalonly/>
        //
        private bool IsSuspended() {
            return initializing || DesignMode;
        }

        /// <devdoc>
        ///     Sees if the name given matches the name filter we have.
        /// </devdoc>
        /// <internalonly/>
        private bool MatchPattern(string relativePath) {            
            string name = System.IO.Path.GetFileName(relativePath);            
            if (name != null)
                return PatternMatcher.StrictMatchPattern(filter.ToUpper(CultureInfo.InvariantCulture), name.ToUpper(CultureInfo.InvariantCulture));
            else
                return false;                
        }

        /// <devdoc>
        ///     Calls native API and sets up handle with the directory change API.
        /// </devdoc>
        /// <internalonly/>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private unsafe void Monitor(byte[] buffer) {
            if (!enabled || IsHandleInvalid) {
                return;
            }

            Overlapped overlapped = new Overlapped();            
            if (buffer == null) {
                try {
                    buffer = new byte[internalBufferSize];
                }
                catch (OutOfMemoryException) {
                        throw new OutOfMemoryException(SR.GetString(SR.BufferSizeTooLarge, internalBufferSize.ToString(CultureInfo.CurrentCulture)));
                }
            }
                        
            // Pass "session" counter to callback:
            FSWAsyncResult asyncResult = new FSWAsyncResult();
            asyncResult.session = currentSession;
            asyncResult.buffer = buffer;

            // Pack overlapped. The buffer will be pinned by Overlapped:
            overlapped.AsyncResult = asyncResult;
            NativeOverlapped* overlappedPointer = overlapped.Pack(new IOCompletionCallback(this.CompletionStatusChanged), buffer);

            // Can now call OS:
            int size;
            bool ok = false;

            try {
                // There could be a ---- in user code between calling StopRaisingEvents (where we close the handle) 
                // and when we get here from CompletionStatusChanged. 
                // We might need to take a lock to prevent ---- absolutely, instead just catch 
                // ObjectDisposedException from SafeHandle in case it is disposed
                if (!IsHandleInvalid) {
                    // An interrupt is possible here
                    fixed (byte * buffPtr = buffer) {
                        ok = UnsafeNativeMethods.ReadDirectoryChangesW(directoryHandle,
                                                           new HandleRef(this, (IntPtr) buffPtr),
                                                           internalBufferSize,
                                                           includeSubdirectories ? 1 : 0,
                                                           (int) notifyFilters,
                                                           out size,
                                                           overlappedPointer,
                                                           NativeMethods.NullHandleRef);
                    }
                }
            } catch (ObjectDisposedException ) { //Ignore
                Debug.Assert(IsHandleInvalid, "ObjectDisposedException from something other than SafeHandle?");
            } catch (ArgumentNullException ) { //Ignore
                Debug.Assert(IsHandleInvalid, "ArgumentNullException from something other than SafeHandle?");
            } finally {
                if (! ok) {
                    Overlapped.Free(overlappedPointer);

                    // If the handle was for some reason changed or closed during this call, then don't throw an
                    // exception.  Else, it's a valid error.
                    if (!IsHandleInvalid) {
                        OnError(new ErrorEventArgs(new Win32Exception()));
                    }
                }
            }
        }                            
        
        /// <devdoc>
        ///     Raises the event to each handler in the list.
        /// </devdoc>
        /// <internalonly/>
        private void NotifyFileSystemEventArgs(int action, string name) {
            if (!MatchPattern(name)) {
                return;
            }

            switch (action) {
                case Direct.FILE_ACTION_ADDED:
                    OnCreated(new FileSystemEventArgs(WatcherChangeTypes.Created, directory, name));
                    break;
                case Direct.FILE_ACTION_REMOVED:
                    OnDeleted(new FileSystemEventArgs(WatcherChangeTypes.Deleted, directory, name));
                    break;
                case Direct.FILE_ACTION_MODIFIED:
                    OnChanged(new FileSystemEventArgs(WatcherChangeTypes.Changed, directory, name));
                    break;

                default:
                    Debug.Fail("Unknown FileSystemEvent action type!  Value: "+action);
                    break;
            }
        }

        /// <devdoc>
        ///     Raises the event to each handler in the list.
        /// </devdoc>
        /// <internalonly/>
        private void NotifyInternalBufferOverflowEvent() {
            InternalBufferOverflowException ex = new InternalBufferOverflowException(SR.GetString(SR.FSW_BufferOverflow, directory));

            ErrorEventArgs errevent = new ErrorEventArgs(ex);

            OnError(errevent);
        }

        /// <devdoc>
        ///     Raises the event to each handler in the list.
        /// </devdoc>
        /// <internalonly/>
        private void NotifyRenameEventArgs(WatcherChangeTypes action, string name, string oldName) {
            //filter if neither new name or old name are a match a specified pattern
            if (!MatchPattern(name) && !MatchPattern(oldName)) {
                return;
            }

            RenamedEventArgs renevent = new RenamedEventArgs(action, directory, name, oldName);
            OnRenamed(renevent);
        }

        /// <devdoc>
        ///    <para>
        ///       Raises the <see cref='System.IO.FileSystemWatcher.Changed'/> event.
        ///    </para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security","CA2109:ReviewVisibleEventHandlers", MessageId="0#", Justification="Changing from protected to private would be a breaking change")]
        protected void OnChanged(FileSystemEventArgs e) {
            // To avoid ---- between remove handler and raising the event
            FileSystemEventHandler changedHandler = onChangedHandler;
            
            if (changedHandler != null) {
                if (this.SynchronizingObject != null && this.SynchronizingObject.InvokeRequired)
                    this.SynchronizingObject.BeginInvoke(changedHandler, new object[]{this, e});
                else                        
                   changedHandler(this, e);                
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Raises the <see cref='System.IO.FileSystemWatcher.Created'/> event.
        ///    </para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security","CA2109:ReviewVisibleEventHandlers", MessageId="0#", Justification="Changing from protected to private would be a breaking change")]
        protected void OnCreated(FileSystemEventArgs e) {
            // To avoid ---- between remove handler and raising the event
            FileSystemEventHandler createdHandler = onCreatedHandler;
            if (createdHandler != null) {
                if (this.SynchronizingObject != null && this.SynchronizingObject.InvokeRequired)
                    this.SynchronizingObject.BeginInvoke(createdHandler, new object[]{this, e});
                else                        
                   createdHandler(this, e);                
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Raises the <see cref='System.IO.FileSystemWatcher.Deleted'/> event.
        ///    </para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#", Justification = "Changing from protected to private would be a breaking change")]
        protected void OnDeleted(FileSystemEventArgs e) {
            // To avoid ---- between remove handler and raising the event
            FileSystemEventHandler deletedHandler = onDeletedHandler;
            if (deletedHandler != null) {
                if (this.SynchronizingObject != null && this.SynchronizingObject.InvokeRequired)
                    this.SynchronizingObject.BeginInvoke(deletedHandler, new object[]{this, e});
                else                        
                   deletedHandler(this, e);                
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Raises the <see cref='System.IO.FileSystemWatcher.Error'/> event.
        ///    </para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#", Justification = "Changing from protected to private would be a breaking change")]
        protected void OnError(ErrorEventArgs e) {
            // To avoid ---- between remove handler and raising the event
            ErrorEventHandler errorHandler = onErrorHandler;
            if (errorHandler != null) {
                if (this.SynchronizingObject != null && this.SynchronizingObject.InvokeRequired)
                    this.SynchronizingObject.BeginInvoke(errorHandler, new object[]{this, e});
                else                        
                   errorHandler(this, e);                
            }
        }

        /// <devdoc>
        ///     Internal method used for synchronous notification.
        /// </devdoc>
        /// <internalonly/>
        private void OnInternalFileSystemEventArgs(object sender, FileSystemEventArgs e) {
            lock (this) {
                // Only change the state of the changed result if it doesn't contain a previous one.
                if (isChanged != true) {
                    changedResult = new WaitForChangedResult(e.ChangeType, e.Name, false);
                    isChanged = true;
                    System.Threading.Monitor.Pulse(this);
                }
            }
        }

        /// <devdoc>
        ///     Internal method used for synchronous notification.
        /// </devdoc>
        /// <internalonly/>
        private void OnInternalRenameEventArgs(object sender, RenamedEventArgs e) {
            lock (this) {
                // Only change the state of the changed result if it doesn't contain a previous one.
                if (isChanged != true) {
                    changedResult = new WaitForChangedResult(e.ChangeType, e.Name, e.OldName, false);
                    isChanged = true;
                    System.Threading.Monitor.Pulse(this);
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Raises the <see cref='System.IO.FileSystemWatcher.Renamed'/> event.
        ///    </para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#", Justification = "Changing from protected to private would be a breaking change")]
        protected void OnRenamed(RenamedEventArgs e) {
            RenamedEventHandler renamedHandler = onRenamedHandler;
            if (renamedHandler != null) {
                if (this.SynchronizingObject != null && this.SynchronizingObject.InvokeRequired)
                    this.SynchronizingObject.BeginInvoke(renamedHandler, new object[]{this, e});
                else                        
                   renamedHandler(this, e);                
            }
        }

        /// <devdoc>
        ///     Stops and starts this object.
        /// </devdoc>
        /// <internalonly/>
        private void Restart() {
            if ((!IsSuspended()) && enabled) {
                StopRaisingEvents();
                StartRaisingEvents();
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Starts monitoring the specified directory.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private void StartRaisingEvents() {
            //Cannot allocate the directoryHandle and the readBuffer if the object has been disposed; finalization has been suppressed.
            if (this.disposed)
                throw new ObjectDisposedException(GetType().Name);
                
            try {
                new EnvironmentPermission(PermissionState.Unrestricted).Assert();
                if (Environment.OSVersion.Platform != PlatformID.Win32NT) {
                    throw new PlatformNotSupportedException(SR.GetString(SR.WinNTRequired));
                }
            }
            finally {
                CodeAccessPermission.RevertAssert();
            }

            // If we're called when "Initializing" is true, set enabled to true
            if (IsSuspended()) {
                enabled = true;
                return;
            }
        
            if (!readGranted) {
                string fullPath;
                // Consider asserting path discovery permission here.
                fullPath = System.IO.Path.GetFullPath(directory);

                FileIOPermission permission = new FileIOPermission(FileIOPermissionAccess.Read, fullPath);
                permission.Demand();                
                readGranted = true;                    
            }
            
            
            // If we're attached, don't do anything.
            if (!IsHandleInvalid) {
                return;
            }

            // Create handle to directory being monitored
            directoryHandle = NativeMethods.CreateFile(directory,            // Directory name
                                UnsafeNativeMethods.FILE_LIST_DIRECTORY,           // access (read-write) mode
                                UnsafeNativeMethods.FILE_SHARE_READ |
                                    UnsafeNativeMethods.FILE_SHARE_DELETE |
                                    UnsafeNativeMethods.FILE_SHARE_WRITE,          // share mode
                                null,                                              // security descriptor
                                UnsafeNativeMethods.OPEN_EXISTING,                 // how to create
                                UnsafeNativeMethods.FILE_FLAG_BACKUP_SEMANTICS |
                                    UnsafeNativeMethods.FILE_FLAG_OVERLAPPED,      // file attributes
                                new SafeFileHandle(IntPtr.Zero, false)             // file with attributes to copy
                            );

            if (IsHandleInvalid) {
                throw new FileNotFoundException(SR.GetString(SR.FSW_IOError, directory));
            }
            
            stopListening = false;
            // Start ignoring all events that were initiated before this.
            Interlocked.Increment(ref currentSession);

            // Attach handle to thread pool
            
            //SECREVIEW: At this point at least FileIOPermission has already been demanded.
            SecurityPermission secPermission = new SecurityPermission(PermissionState.Unrestricted);
            secPermission.Assert();
            try {
                ThreadPool.BindHandle(directoryHandle);
            }
            finally {
                SecurityPermission.RevertAssert();
            }                                                   
            enabled = true;

            // Setup IO completion port
            Monitor(null);
        }

        /// <devdoc>
        ///    <para>
        ///       Stops monitoring the specified directory.
        ///    </para>
        /// </devdoc>
        private void StopRaisingEvents() {
            if (IsSuspended()) {
                enabled = false;
                return;
            }

            // If we're not attached, do nothing.
            if (IsHandleInvalid) {
                return;
            }

            // Close directory handle 
            // This operation doesn't need to be atomic because the API will deal with a closed
            // handle appropriately.
            // Ensure that the directoryHandle is set to INVALID_HANDLE before closing it, so that
            // the Monitor() can shutdown appropriately.
            // If we get here while asynchronously waiting on a change notification, closing the
            // directory handle should cause CompletionStatusChanged be be called
            // thus freeing the pinned buffer.
            stopListening = true;
            directoryHandle.Close();
            directoryHandle = null;


            // Start ignoring all events occurring after this.
            Interlocked.Increment(ref currentSession);
            
            // Set enabled to false
            enabled = false;
        }

        /// <devdoc>
        ///    <para>
        ///       A synchronous method that returns a structure that
        ///       contains specific information on the change that occurred, given the type
        ///       of change that you wish to monitor.
        ///    </para>
        /// </devdoc>
        public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType) {
            return WaitForChanged(changeType, -1);
        }

        /// <devdoc>
        ///    <para>
        ///       A synchronous
        ///       method that returns a structure that contains specific information on the change that occurred, given the
        ///       type of change that you wish to monitor and the time (in milliseconds) to wait before timing out.
        ///    </para>
        /// </devdoc>
        public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout) {
            FileSystemEventHandler dirHandler = new FileSystemEventHandler(this.OnInternalFileSystemEventArgs);
            RenamedEventHandler renameHandler = new RenamedEventHandler(this.OnInternalRenameEventArgs);

            this.isChanged = false;
            this.changedResult = WaitForChangedResult.TimedOutResult;

            // Register the internal event handler from the given change types.
            if ((changeType & WatcherChangeTypes.Created) != 0) {
                this.Created += dirHandler;
            }
            if ((changeType & WatcherChangeTypes.Deleted) != 0) {
                this.Deleted += dirHandler;
            }
            if ((changeType & WatcherChangeTypes.Changed) != 0) {
                this.Changed += dirHandler;
            }
            if ((changeType & WatcherChangeTypes.Renamed) != 0) {
                this.Renamed += renameHandler;
            }

            // Save the Enabled state of this component to revert back to it later (if needed).
            bool savedEnabled = EnableRaisingEvents;
            if (savedEnabled == false) {
                runOnce = true;
                EnableRaisingEvents = true;
            }

            // For each thread entering this wait loop, addref it and wait.  When the last one
            // exits, reset the waiterObject.
            WaitForChangedResult retVal = WaitForChangedResult.TimedOutResult;
            lock (this) {
                if (timeout == -1) {
                    while (!isChanged) {
                        System.Threading.Monitor.Wait(this);
                    }
                }
                else {
                    System.Threading.Monitor.Wait(this, timeout, true);
                }

                retVal = changedResult;
            }

            // Revert the Enabled flag to its previous state.
            EnableRaisingEvents = savedEnabled;
            runOnce = false;

            // Decouple the event handlers added above.
            if ((changeType & WatcherChangeTypes.Created) != 0) {
                this.Created -= dirHandler;
            }
            if ((changeType & WatcherChangeTypes.Deleted) != 0) {
                this.Deleted -= dirHandler;
            }
            if ((changeType & WatcherChangeTypes.Changed) != 0) {
                this.Changed -= dirHandler;
            }
            if ((changeType & WatcherChangeTypes.Renamed) != 0) {
                this.Renamed -= renameHandler;
            }

            // Return the struct.
            return retVal;
        }
    }

    /// <devdoc>
    ///    Helper class to hold to N/Direct call declaration and flags.
    /// </devdoc>
    [
        System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]    
    internal static class Direct {
        // All possible action flags
        public const int FILE_ACTION_ADDED            = 1;
        public const int FILE_ACTION_REMOVED          = 2;
        public const int FILE_ACTION_MODIFIED         = 3;
        public const int FILE_ACTION_RENAMED_OLD_NAME = 4;
        public const int FILE_ACTION_RENAMED_NEW_NAME = 5;


        // All possible notifications flags
        public const int FILE_NOTIFY_CHANGE_FILE_NAME    = 0x00000001;
        public const int FILE_NOTIFY_CHANGE_DIR_NAME     = 0x00000002;
        public const int FILE_NOTIFY_CHANGE_NAME         = 0x00000003;
        public const int FILE_NOTIFY_CHANGE_ATTRIBUTES   = 0x00000004;
        public const int FILE_NOTIFY_CHANGE_SIZE         = 0x00000008;
        public const int FILE_NOTIFY_CHANGE_LAST_WRITE   = 0x00000010;
        public const int FILE_NOTIFY_CHANGE_LAST_ACCESS  = 0x00000020;
        public const int FILE_NOTIFY_CHANGE_CREATION     = 0x00000040;
        public const int FILE_NOTIFY_CHANGE_SECURITY     = 0x00000100;
    }
}


