//------------------------------------------------------------------------------
// <copyright file="InvalidEnumArgumentException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {
    using Microsoft.Win32;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    
    /// <devdoc>
    ///    <para>The exception that is thrown when using invalid arguments that are enumerators.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [Serializable]
    public class InvalidEnumArgumentException : ArgumentException {

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.InvalidEnumArgumentException'/> class without a message.</para>
        /// </devdoc>
        public InvalidEnumArgumentException() : this(null) {
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.InvalidEnumArgumentException'/> class with 
        ///    the specified message.</para>
        /// </devdoc>
        public InvalidEnumArgumentException(string message)
            : base(message) {
        }

        /// <devdoc>
        ///     Initializes a new instance of the Exception class with a specified error message and a 
        ///     reference to the inner exception that is the cause of this exception.
        ///     FxCop CA1032: Multiple constructors are required to correctly implement a custom exception.
        /// </devdoc>
        public InvalidEnumArgumentException( string message, Exception innerException ) 
            : base(message, innerException) {
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.InvalidEnumArgumentException'/> class with a 
        ///    message generated from the argument, invalid value, and enumeration
        ///    class.</para>
        /// </devdoc>
        public InvalidEnumArgumentException(string argumentName, int invalidValue, Type enumClass)
            : base(SR.GetString(SR.InvalidEnumArgument,
                                argumentName,
                                invalidValue.ToString(CultureInfo.CurrentCulture),
                                enumClass.Name), argumentName) {
        }

        /// <devdoc>
        ///     Need this constructor since Exception implements ISerializable. We don't have any fields,
        ///     so just forward this to base.
        /// </devdoc>
        protected InvalidEnumArgumentException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}
