//------------------------------------------------------------------------------
// <copyright file="SocketType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Net.Sockets {

    /// <devdoc>
    ///    <para>
    ///       Specifies the type of socket an instance of the <see cref='System.Net.Sockets.Socket'/> class represents.
    ///    </para>
    /// </devdoc>
    public enum SocketType {

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Stream      = 1,    // stream socket
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Dgram       = 2,    // datagram socket
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Raw         = 3,    // raw-protocolinterface
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Rdm         = 4,    // reliably-delivered message
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Seqpacket   = 5,    // sequenced packet stream
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Unknown     = -1,   // Unknown socket type

    } // enum SocketType

} // namespace System.Net.Sockets
