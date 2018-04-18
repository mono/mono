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
    public partial class Win32Exception : ExternalException, ISerializable {
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
#if MONO_FEATURE_CAS
            IntSecurity.UnmanagedCode.Demand();
#endif
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

#if !MONO
        private static bool TryGetErrorMessage(int error, StringBuilder sb, out string errorMsg)
        {
            errorMsg = "";
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
            else if (Marshal.GetLastWin32Error() == SafeNativeMethods.ERROR_INSUFFICIENT_BUFFER) {
                return false;
            }
            else {
                errorMsg ="Unknown error (0x" + Convert.ToString(error, 16) + ")";
            }

            return true;
        }

        // Windows API FormatMessage lets you format a message string given an errocode.
        // Unlike other APIs this API does not support a way to query it for the total message size.
        //
        // So the API can only be used in one of these two ways.
        // a. You pass a buffer of appropriate size and get the resource.
        // b. Windows creates a buffer and passes the address back and the onus of releasing the bugffer lies on the caller.
        //
        // Since the error code is coming from the user, it is not possible to know the size in advance.
        // Unfortunately we can't use option b. since the buffer can only be freed using LocalFree and it is a private API on onecore.
        // Also, using option b is ugly for the manged code and could cause memory leak in situations where freeing is unsuccessful.
        // 
        // As a result we use the following approach.
        // We initially call the API with a buffer size of 256 and then gradually increase the size in case of failure until we reach the max allowed size of 65K bytes.

        private const int MaxAllowedBufferSize = 65 * 1024;

        private static string GetErrorMessage(int error) {
            string errorMsg;

            StringBuilder sb = new StringBuilder(256);
            do {
                if (TryGetErrorMessage(error, sb, out errorMsg))
                    return errorMsg;
                else {
                    // increase the capacity of the StringBuilder by 4 times.
                    sb.Capacity *= 4;
                }
            }
            while (sb.Capacity < MaxAllowedBufferSize);

            // If you come here then a size as large as 65K is also not sufficient and so we give the generic errorMsg.
            return "Unknown error (0x" + Convert.ToString(error, 16) + ")";
        }
#endif
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
