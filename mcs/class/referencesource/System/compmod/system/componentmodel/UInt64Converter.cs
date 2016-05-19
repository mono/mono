


//------------------------------------------------------------------------------
// <copyright file="UInt64Converter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {
    using Microsoft.Win32;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Serialization.Formatters;
    using System.Security.Permissions;


    /// <devdoc>
    ///    <para>Provides a type converter to convert 64-bit unsigned integer objects to and
    ///       from various other representations.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public class UInt64Converter : BaseNumberConverter {

        /// <devdoc>
        /// The Type this converter is targeting (e.g. Int16, UInt64, etc.)
        /// </devdoc>
        internal override Type TargetType {
                get {
                    return typeof(UInt64);
                }
        }

        /// <devdoc>
        /// Convert the given value to a string using the given radix
        /// </devdoc>
        internal override object FromString(string value, int radix) {
                return Convert.ToUInt64(value, radix);
        }
        
        /// <devdoc>
        /// Convert the given value to a string using the given formatInfo
        /// </devdoc>
        internal override object FromString(string value, NumberFormatInfo formatInfo) {
                return UInt64.Parse(value, NumberStyles.Integer, formatInfo);
        }
        
        
        /// <devdoc>
        /// Convert the given value to a string using the given CultureInfo
        /// </devdoc>
        internal override object FromString(string value, CultureInfo culture){
                 return UInt64.Parse(value, culture);
        }
        
        /// <devdoc>
        /// Convert the given value from a string using the given formatInfo
        /// </devdoc>
        internal override string ToString(object value, NumberFormatInfo formatInfo) {
                return ((UInt64)value).ToString("G", formatInfo);
        }

    }
}

