//------------------------------------------------------------------------------
// <copyright file="SocketFlags.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Net.Sockets {
    using System;

    /// <devdoc>
    ///    <para>
    ///       Provides constant values for socket messages.
    ///    </para>
    /// </devdoc>
    //UEUE

    [Flags] 
    public enum TransmitFileOptions
    {
        /// <devdoc>
        ///    <para>
        ///       Use no flags for this call.
        ///    </para>
        /// </devdoc>

        UseDefaultWorkerThread=  0x00,
        /// <devdoc>
        ///    <para>
        ///       Use no flags for this call.
        ///    </para>
        /// </devdoc>
        Disconnect =       0x01,
        /// <devdoc>
        ///    <para>
        ///       Use no flags for this call.
        ///    </para>
        /// </devdoc>
        ReuseSocket=     0x02,
        /// <devdoc>
        ///    <para>
        ///       Use no flags for this call.
        ///    </para>
        /// </devdoc>
        WriteBehind=     0x04,
        /// <devdoc>
        ///    <para>
        ///       Use no flags for this call.
        ///    </para>
        /// </devdoc>
        UseSystemThread=  0x10,
        /// <devdoc>
        ///    <para>
        ///       Use no flags for this call.
        ///    </para>
        /// </devdoc>
        UseKernelApc=     0x20,
    };
} // namespace System.Net.Sockets
