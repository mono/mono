//------------------------------------------------------------------------------
// <copyright file="RenamedEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------


namespace System.IO {

    using System.Diagnostics;
    using System.Security.Permissions;
    using System;
    using System.Runtime.Versioning;


    /// <devdoc>
    /// <para>Provides data for the <see cref='System.IO.FileSystemWatcher.Renamed'/> event.</para>
    /// </devdoc>
    public class RenamedEventArgs : FileSystemEventArgs {
        private string oldName;
        private string oldFullPath;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.IO.RenamedEventArgs'/>
        ///       class.
        ///    </para>
        /// </devdoc>
        public RenamedEventArgs(WatcherChangeTypes changeType, string directory, string name, string oldName)
            : base(changeType, directory, name) {

            // Ensure that the directory name ends with a "\"
            if (!directory.EndsWith("\\", StringComparison.Ordinal)) {
                directory = directory + "\\";
            }
            
            this.oldName = oldName;
            this.oldFullPath = directory + oldName;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets
        ///       the previous fully qualified path of the affected file or directory.
        ///    </para>
        /// </devdoc>
        public string OldFullPath {
            [ResourceExposure(ResourceScope.Machine)]
            [ResourceConsumption(ResourceScope.Machine)]
            get {
                new FileIOPermission(FileIOPermissionAccess.Read, Path.GetPathRoot(oldFullPath)).Demand();
                return oldFullPath;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets
        ///       the old name of the affected file or directory.
        ///    </para>
        /// </devdoc>
        public string OldName {
            get {
                return oldName;
            }
        }
    }


}
