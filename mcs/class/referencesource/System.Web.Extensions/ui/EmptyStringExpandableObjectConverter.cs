//------------------------------------------------------------------------------
// <copyright file="EmptyStringExpandableObjectConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.ComponentModel;
    using System.Globalization;

    // Used by objects that are subproperties of Controls.  Improves the UI in the property grid by displaying
    // nothing as the property value, instead of the fully qualified type name.
    internal sealed class EmptyStringExpandableObjectConverter : ExpandableObjectConverter {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
                                         Type destinationType) {
            if (destinationType == typeof(string)) {
                return String.Empty;
            }
            else {
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}
