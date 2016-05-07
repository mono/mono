//------------------------------------------------------------------------------
// <copyright file="EventLogEntryType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {

    using System.Diagnostics;
    
    // cpr: make this class an enum
    using System;

    /// <devdoc>
    ///    <para>
    ///       Specifies the event type of the event log entry.
    ///       
    ///    </para>
    /// </devdoc>
    public enum EventLogEntryType {
        /// <devdoc>
        ///    <para>
        ///       An
        ///       error event. This indicates a significant problem the
        ///       user should know about; usually a loss of
        ///       functionality or data.
        ///       
        ///    </para>
        /// </devdoc>
        Error = 1,
        /// <devdoc>
        ///    <para>
        ///       A warning event. This indicates a problem that is not
        ///       immediately significant, but that may signify conditions that could
        ///       cause future problems.
        ///       
        ///    </para>
        /// </devdoc>
        Warning = 2,
        /// <devdoc>
        ///    <para>
        ///       An information event. This indicates a significant successful
        ///       operation.
        ///    </para>
        /// </devdoc>
        Information = 4,
        /// <devdoc>
        ///    <para>
        ///       A success audit event. This indicates a security event
        ///       that occurs when an audited access attempt is successful; for
        ///       example, a successful logon.
        ///       
        ///    </para>
        /// </devdoc>
        SuccessAudit = 8,
        /// <devdoc>
        ///    <para>
        ///       A failure audit event. This indicates a security event
        ///       that occurs when an audited access attempt fails; for example, a failed attempt
        ///       to open a file.
        ///       
        ///    </para>
        /// </devdoc>
        FailureAudit = 16,
    }
}
