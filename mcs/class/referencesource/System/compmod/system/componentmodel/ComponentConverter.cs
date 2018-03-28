//------------------------------------------------------------------------------
// <copyright file="ComponentConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {
    using Microsoft.Win32;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Runtime.Serialization.Formatters;
    using System.Runtime.Remoting;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>Provides a type converter to convert component objects to and
    ///       from various other representations.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public class ComponentConverter : ReferenceConverter {
    
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.ComponentModel.ComponentConverter'/> class.
        ///    </para>
        /// </devdoc>
        public ComponentConverter(Type type) : base(type) {
        }

        /// <internalonly/>
        /// <devdoc>
        ///    <para>Gets a collection of properties for the type of component
        ///       specified by the value
        ///       parameter.</para>
        /// </devdoc>
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes) {
            return TypeDescriptor.GetProperties(value, attributes);
        }
        
        /// <internalonly/>
        /// <devdoc>
        ///    <para>Gets a value indicating whether this object supports properties using the
        ///       specified context.</para>
        /// </devdoc>
        public override bool GetPropertiesSupported(ITypeDescriptorContext context) {
            return true;
        }
    }
}

