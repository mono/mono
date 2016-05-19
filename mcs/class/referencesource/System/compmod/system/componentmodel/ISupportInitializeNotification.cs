//------------------------------------------------------------------------------
// <copyright file="ISupportInitializeNotification.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {
    

    using System.Diagnostics;

    using System;

    /// <include file='doc\ISupportInitializeNotification.uex' path='docs/doc[@for="ISupportInitializeNotification"]/*' />
    /// <devdoc>
    ///    <para>
    ///         Extends ISupportInitialize to allow dependent components to be notified when initialization is complete.
    ///    </para>
    /// </devdoc>
    public interface ISupportInitializeNotification : ISupportInitialize {
        /// <include file='doc\ISupportInitializeNotification.uex' path='docs/doc[@for="ISupportInitializeNotification.IsInitialized"]/*' />
        /// <devdoc>
        ///    <para>
        ///         Indicates whether initialization is complete yet.
        ///    </para>
        /// </devdoc>
        bool IsInitialized { get; }

        /// <include file='doc\ISupportInitializeNotification.uex' path='docs/doc[@for="ISupportInitializeNotification.Initialized"]/*' />
        /// <devdoc>
        ///    <para>
        ///         Sent when initialization is complete.
        ///    </para>
        /// </devdoc>
        event EventHandler Initialized;
    }
}
