//------------------------------------------------------------------------------
// <copyright file="CheckoutException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel.Design {
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;


    /// <devdoc>
    ///    <para>
    ///       The exception thrown when an attempt is made to edit a file that is checked into
    ///       a source control program.
    ///    </para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2112:SecuredTypesShouldNotExposeFields")] // ReadOnly field - already shipped.
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name="FullTrust")]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name="FullTrust")]
    [Serializable]
    public class CheckoutException : ExternalException {

        /// <devdoc>
        ///    <para>
        ///       Initializes a <see cref='System.ComponentModel.Design.CheckoutException'/> that specifies that the checkout
        ///       was
        ///       canceled. This field is read-only.
        ///    </para>
        /// </devdoc>
        public readonly static CheckoutException Canceled = new CheckoutException(SR.GetString(SR.CHECKOUTCanceled), NativeMethods.E_ABORT);

        /// <devdoc>
        ///    <para>
        ///       Initializes
        ///       a new instance of the <see cref='System.ComponentModel.Design.CheckoutException'/> class with no
        ///       associated message or
        ///       error code.
        ///    </para>
        /// </devdoc>
        public CheckoutException() {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.ComponentModel.Design.CheckoutException'/>
        ///       class with the specified message.
        ///    </para>
        /// </devdoc>
        public CheckoutException(string message)
            : base(message) {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.ComponentModel.Design.CheckoutException'/>
        ///       class with the specified message and error code.
        ///    </para>
        /// </devdoc>
        public CheckoutException(string message, int errorCode)
            : base(message, errorCode) {
        }

        /// <devdoc>
        ///     Need this constructor since Exception implements ISerializable. We don't have any fields,
        ///     so just forward this to base.
        /// </devdoc>
        protected CheckoutException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }

        /// <devdoc>
        ///     Initializes a new instance of the Exception class with a specified error message and a 
        ///     reference to the inner exception that is the cause of this exception.
        ///     FxCop CA1032: Multiple constructors are required to correctly implement a custom exception.
        /// </devdoc>
        public CheckoutException( string message, Exception innerException ) : base(message, innerException) {
        }
    }
}
