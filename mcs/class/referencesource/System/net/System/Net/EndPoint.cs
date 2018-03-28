//------------------------------------------------------------------------------
// <copyright file="EndPoint.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using System.Net.Sockets;

namespace System.Net {

    // Generic abstraction to identify network addresses

    /// <devdoc>
    ///    <para>
    ///       Identifies a network address.
    ///    </para>
    /// </devdoc>
    [Serializable]
    public abstract class EndPoint {
        /// <devdoc>
        ///    <para>
        ///       Returns the Address Family to which the EndPoint belongs.
        ///    </para>
        /// </devdoc>

        public virtual AddressFamily AddressFamily {
            get {
                throw ExceptionHelper.PropertyNotImplementedException;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Serializes EndPoint information into a SocketAddress structure.
        ///    </para>
        /// </devdoc>
        public virtual SocketAddress Serialize() {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        /// <devdoc>
        ///    <para>
        ///       Creates an EndPoint instance from a SocketAddress structure.
        ///    </para>
        /// </devdoc>
        public virtual EndPoint Create(SocketAddress socketAddress) {
            throw ExceptionHelper.MethodNotImplementedException;
        }

    }; // abstract class EndPoint


} // namespace System.Net

