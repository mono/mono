//------------------------------------------------------------------------------
// <copyright file="TraceContextEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System.Collections;

    /// <devdoc>
    /// </devdoc>
    public sealed class TraceContextEventArgs : EventArgs {
        private ICollection _records;


        public TraceContextEventArgs(ICollection records) {
            _records = records;
        }


        /// <devdoc>
        /// Gets the trace records for this event
        /// </devdoc>
        public ICollection TraceRecords {
            get {
                return _records;
            }
        }
    }

}



