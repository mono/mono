//------------------------------------------------------------------------------
// <copyright file="ConnectionModes.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Net {

// Used to indicate the mode of how we use a transport
// Not sure whether this is the right way to define an enum
/// <devdoc>
///    <para>
///       Specifies the mode used to establish a connection with a server.
///    </para>
/// </devdoc>
    internal enum ConnectionModes {
        /// <devdoc>
        ///    <para>
        ///       Non-persistent, one request per connection.
        ///    </para>
        /// </devdoc>
        Single      ,       // Non-Persistent, one request per connection
        /// <devdoc>
        ///    <para>
        ///       Persistent connection, one request/response at a time.
        ///    </para>
        /// </devdoc>
        Persistent,         // Persistant, one request/response at a time
        /// <devdoc>
        ///    <para>
        ///       Persistent connection, many requests/responses in order.
        ///    </para>
        /// </devdoc>
        Pipeline ,          // Persistant, many requests/responses in order
        /// <devdoc>
        ///    <para>
        ///       Persistent connection, many requests/responses out of order.
        ///    </para>
        /// </devdoc>
        Mux                 // Persistant, many requests/responses out of order

    } // enum ConnectionModes


} // namespace System.Net
