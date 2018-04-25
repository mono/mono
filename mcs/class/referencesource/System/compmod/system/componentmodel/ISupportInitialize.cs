//------------------------------------------------------------------------------
// <copyright file="ISupportInitialize.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {
    

    using System.Diagnostics;

    using System;

    /// <devdoc>
    ///    <para>Specifies that this object supports
    ///       a simple,
    ///       transacted notification for batch initialization.</para>
    /// </devdoc>
    [SRDescription(SR.ISupportInitializeDescr)]
    public interface ISupportInitialize {
        /// <devdoc>
        ///    <para>
        ///       Signals
        ///       the object that initialization is starting.
        ///    </para>
        /// </devdoc>
        void BeginInit();

        /// <devdoc>
        ///    <para>
        ///       Signals the object that initialization is
        ///       complete.
        ///    </para>
        /// </devdoc>
        void EndInit();
    }
}
