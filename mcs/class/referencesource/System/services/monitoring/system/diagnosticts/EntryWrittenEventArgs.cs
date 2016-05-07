//------------------------------------------------------------------------------
// <copyright file="EntryWrittenEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {

    using System.Diagnostics;

    using System;
    using System.ComponentModel;

    /// <devdoc>
    ///    <para>
    ///       Provides data for the <see cref='System.Diagnostics.EventLog.EntryWritten'/> event.
    ///    </para>
    /// </devdoc>
    public class EntryWrittenEventArgs : EventArgs {
        private EventLogEntry entry;

        /// <devdoc>
        ///    <para>
        ///       The default constructor, which
        ///       initializes a new instance of the <see cref='System.Diagnostics.EntryWrittenEventArgs'/> class without
        ///       specifying a value for <see cref='System.Diagnostics.EntryWrittenEventArgs.Entry'/>.
        ///       
        ///    </para>
        /// </devdoc>
        public EntryWrittenEventArgs() {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Diagnostics.EntryWrittenEventArgs'/> class with the
        ///       specified event log entry.
        ///    </para>
        /// </devdoc>
        public EntryWrittenEventArgs(EventLogEntry entry) {
            this.entry = entry;
        }
        
        /// <devdoc>
        ///    <para>
        ///       Represents
        ///       an event log entry.
        ///       
        ///    </para>
        /// </devdoc>
        public EventLogEntry Entry {
            get {
                return this.entry;
            }
        }
    }
}
