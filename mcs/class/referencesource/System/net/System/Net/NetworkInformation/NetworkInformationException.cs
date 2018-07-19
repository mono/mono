//------------------------------------------------------------------------------
// <copyright file="NetworkInformationException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Net.NetworkInformation {
    using System;
    using System.ComponentModel;
    using System.Net.Sockets;
    using System.Runtime.Serialization;
    using System.Runtime.InteropServices;

    /// <devdoc>
    ///    <para>
    ///       Provides NetworkInformation exceptions to the application.
    ///    </para>
    /// </devdoc>
    [Serializable]
    public class NetworkInformationException : Win32Exception {
        /// <devdoc>
        ///    <para>
        ///       Creates a new instance of the <see cref='System.Net.NetworkInformation.NetworkInformationException'/> class with the default error code.
        ///    </para>
        /// </devdoc>
        public NetworkInformationException() : base(Marshal.GetLastWin32Error()) {
        }

        /// <devdoc>
        ///    <para>
        ///       Creates a new instance of the <see cref='System.Net.NetworkInformation.NetworkInformationException'/> class with the specified error code.
        ///    </para>
        /// </devdoc>
        public NetworkInformationException(int errorCode) : base(errorCode) {
        }


        internal NetworkInformationException(SocketError socketError) : base((int)socketError) {
        }

        protected NetworkInformationException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext) {
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override int ErrorCode {
            //
            // the base class returns the HResult with this property
            // we need the Win32 Error Code, hence the override.
            //
            get {
                return NativeErrorCode;
            }
        }

    }; // class NetworkInformationException
    

} // namespace System.Net.NetworkInformation
