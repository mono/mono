//------------------------------------------------------------------------------
// <copyright file="WaitForChangedResult.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.IO {

    using System.Diagnostics;

    using System;

    /// <devdoc>
    ///    <para>Waits for a change in the specified path.</para>
    /// </devdoc>
    public struct WaitForChangedResult {
        private WatcherChangeTypes changeType;
        private string name;
        private string oldName;
        private bool timedOut;

        /// <devdoc>
        ///    <para>
        ///       Specifies that the call has timed out.
        ///    </para>
        /// </devdoc>
        internal static readonly WaitForChangedResult TimedOutResult = new WaitForChangedResult(0, null, true);
        
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.IO.WaitForChangedResult'/> class, given the
        ///       type of change to watch for, the folder to watch, and whether the call has
        ///       timed out.
        ///    </para>
        /// </devdoc>
        internal WaitForChangedResult(WatcherChangeTypes changeType, string name, bool timedOut)
            : this(changeType, name, null, timedOut){
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.IO.WaitForChangedResult'/> class. This constructor is called when you are waiting
        ///       for a change in a file or directory name.
        ///    </para>
        /// </devdoc>
        internal WaitForChangedResult(WatcherChangeTypes changeType, string name, string oldName, bool timedOut) {
            this.changeType = changeType;
            this.name = name;
            this.oldName = oldName;
            this.timedOut = timedOut;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the type of change to watch for.
        ///    </para>
        /// </devdoc>
        public WatcherChangeTypes ChangeType {
            get {
                return changeType;
            }
            set {
                changeType = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the name of the file or subdirectory that has changed.
        ///    </para>
        /// </devdoc>
        public string Name {
            get {
                return name;
            }
            set {
                name = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the original name of the file or subdirectory that has been
        ///       renamed.
        ///    </para>
        /// </devdoc>
        public string OldName {
            get {
                return oldName;
            }
            set {
                oldName = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value indicating whether the process has timed out.
        ///    </para>
        /// </devdoc>
        public bool TimedOut {
            get {
                return timedOut;
            }
            set {
                timedOut = value;
            }
        }
    }
}
