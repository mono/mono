//------------------------------------------------------------------------------
// <copyright file="SocketShutdown.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Net.Sockets {
    using System;

    /// <devdoc>
    ///    <para>
    ///       Defines constants used by the <see cref='System.Net.Sockets.Socket.Shutdown'/> method.
    ///    </para>
    /// </devdoc>
    public enum SocketShutdown {
        /// <devdoc>
        ///    <para>
        ///       Shutdown sockets for receive.
        ///    </para>
        /// </devdoc>
        Receive   = 0x00,
        /// <devdoc>
        ///    <para>
        ///       Shutdown socket for send.
        ///    </para>
        /// </devdoc>
        Send      = 0x01,
        /// <devdoc>
        ///    <para>
        ///       Shutdown socket for both send and receive.
        ///    </para>
        /// </devdoc>
        Both      = 0x02,

    }; // enum SocketShutdown


} // namespace System.Net.Sockets
