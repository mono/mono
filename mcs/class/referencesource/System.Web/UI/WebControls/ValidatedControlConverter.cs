//------------------------------------------------------------------------------
// <copyright file="ValidatedControlConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;

    /// <devdoc>
    ///    <para> Filters and retrieves several types of values from validated controls.</para>
    /// </devdoc>
    public class ValidatedControlConverter: ControlIDConverter {        

        /// <devdoc>
        ///    <para>Determines whether a given control should have its id added to the StandardValuesCollection.</para>
        /// </devdoc>
        protected override bool FilterControl(Control control) {
            ValidationPropertyAttribute valProp = (ValidationPropertyAttribute)TypeDescriptor.GetAttributes(control)[typeof(ValidationPropertyAttribute)];
            return valProp != null && valProp.Name != null;
        }
    }    
}
