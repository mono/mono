//------------------------------------------------------------------------------
// <copyright file="TransportType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System;


    /// <devdoc>
    ///    <para>
    ///       Defines the transport type allowed for the socket.
    ///    </para>
    /// </devdoc>
    public  enum TransportType {
        /// <devdoc>
        ///    <para>
        ///       Udp connections are allowed.
        ///    </para>
        /// </devdoc>
        Udp     = 0x1,
        Connectionless = 1,
        /// <devdoc>
        ///    <para>
        ///       TCP connections are allowed.
        ///    </para>
        /// </devdoc>
        Tcp     = 0x2,
        ConnectionOriented = 2,
        /// <devdoc>
        ///    <para>
        ///       Any connection is allowed.
        ///    </para>
        /// </devdoc>
        All     = 0x3

    } // enum TransportType

} // namespace System.Net
