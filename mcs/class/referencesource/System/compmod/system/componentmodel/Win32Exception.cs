//------------------------------------------------------------------------------
// <copyright file="Win32Exception.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.ComponentModel {
    using Microsoft.Win32;
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    /// <devdoc>
    ///    <para>The exception that is thrown for a Win32 error code.</para>
    /// </devdoc>
    // Code already shipped - safe to place link demand on derived class constructor when base doesn't have it - Suppress message.
    [HostProtection(SharedState = true)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
    [Serializable]
    [SuppressUnmanagedCodeSecurity]
    public class Win32Exception : ExternalException, ISerializable {
        /// <devdoc>
        ///    <para>Represents the Win32 error code associated with this exception. This 
        ///       field is read-only.</para>
        /// </devdoc>
        private readonly int nativeErrorCode;

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.Win32Exception'/> class with the last Win32 error 
        ///    that occured.</para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public Win32Exception() : this(Marshal.GetLastWin32Error()) {
        }
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.Win32Exception'/> class with the specified error.</para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public Win32Exception(int error) : this(error, GetErrorMessage(error)) {
        }
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.Win32Exception'/> class with the specified error and the 
        ///    specified detailed description.</para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public Win32Exception(int error, string message)
        : base(message) {
            nativeErrorCode = error;
        }

        /// <devdoc>
        ///     Initializes a new instance of the Exception class with a specified error message.
        ///     FxCop CA1032: Multiple constructors are required to correctly implement a custom exception.
        /// </devdoc>
        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public Win32Exception( string message ) : this(Marshal.GetLastWin32Error(), message) {
        }

        /// <devdoc>
        ///     Initializes a new instance of the Exception class with a specified error message and a 
        ///     reference to the inner exception that is the cause of this exception.
        ///     FxCop CA1032: Multiple constructors are required to correctly implement a custom exception.
        /// </devdoc>
        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public Win32Exception( string message, Exception innerException ) : base(message, innerException) {
            nativeErrorCode = Marshal.GetLastWin32Error();
        }

        protected Win32Exception(SerializationInfo info, StreamingContext context) : base (info, context) {
            IntSecurity.UnmanagedCode.Demand();
            nativeErrorCode = info.GetInt32("NativeErrorCode");
        }

        /// <devdoc>
        ///    <para>Represents the Win32 error code associated with this exception. This 
        ///       field is read-only.</para>
        /// </devdoc>
        public int NativeErrorCode {
            get {
                return nativeErrorCode;
            }
        }

        private static string GetErrorMessage(int error) {
            //get the system error message...
            string errorMsg = "";

            StringBuilder sb = new StringBuilder(256);
            int result = SafeNativeMethods.FormatMessage(
                                        SafeNativeMethods.FORMAT_MESSAGE_IGNORE_INSERTS |
                                        SafeNativeMethods.FORMAT_MESSAGE_FROM_SYSTEM |
                                        SafeNativeMethods.FORMAT_MESSAGE_ARGUMENT_ARRAY,
                                        IntPtr.Zero, (uint) error, 0, sb, sb.Capacity + 1,
                                        null);
            if (result != 0) {
                int i = sb.Length;
                while (i > 0) {
                    char ch = sb[i - 1];
                    if (ch > 32 && ch != '.') break;
                    i--;
                }
                errorMsg = sb.ToString(0, i);
            }
            else {
                errorMsg ="Unknown error (0x" + Convert.ToString(error, 16) + ")";
            }

            return errorMsg;
        }

        // Even though all we're exposing is the nativeErrorCode (which is also available via public property)
        // it's not a bad idea to have this in place.  Later, if more fields are added to this exception, 
        // we won't need to worry about accidentaly exposing them through this interface.
        [SecurityPermissionAttribute(SecurityAction.Demand,SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            if (info==null) {
                throw new ArgumentNullException("info");
            }
            info.AddValue("NativeErrorCode", nativeErrorCode);
            base.GetObjectData(info, context);
        }
    }
}
