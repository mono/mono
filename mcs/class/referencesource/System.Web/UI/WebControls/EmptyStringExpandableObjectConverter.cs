//------------------------------------------------------------------------------
// <copyright file="EmptyStringExpandableObjectConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.ComponentModel;
    using System.Globalization;
    using System.Security.Permissions;


    /// <devdoc>
    /// Converts an object to String.Empty so it looks better in the designer property grid.
    /// </devdoc>
    internal sealed class EmptyStringExpandableObjectConverter : ExpandableObjectConverter {

        /// <devdoc>
        /// Returns String.Empty so the object looks better in the designer property grid.
        /// </devdoc>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType == typeof(string)) {
                return String.Empty;
            }
            throw GetConvertToException(value, destinationType);
        }
    }
}

