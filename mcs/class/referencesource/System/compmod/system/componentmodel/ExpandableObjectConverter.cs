//------------------------------------------------------------------------------
// <copyright file="ExpandableObjectConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {
    using Microsoft.Win32;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>Provides
    ///       a type converter to convert expandable objects to and from various
    ///       other representations.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public class ExpandableObjectConverter : TypeConverter {
    
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the System.ComponentModel.ExpandableObjectConverter class.
        ///    </para>
        /// </devdoc>
        public ExpandableObjectConverter() {
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Gets a collection of properties for the type of object
        ///       specified by the value
        ///       parameter.</para>
        /// </devdoc>
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes) {
            return TypeDescriptor.GetProperties(value, attributes);
        }
        
        /// <internalonly/>
        /// <devdoc>
        ///    <para>Gets a value indicating
        ///       whether this object supports properties using the
        ///       specified context.</para>
        /// </devdoc>
        public override bool GetPropertiesSupported(ITypeDescriptorContext context) {
            return true;
        }
    }
}

