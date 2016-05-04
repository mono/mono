//------------------------------------------------------------------------------
// <copyright file="BaseComponentEditor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {
    using System;
    using System.Diagnostics;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para> Provides the base class for a custom component 
    ///       editor.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public abstract class ComponentEditor
    {
    
        /// <devdoc>
        ///    <para>Gets a value indicating whether the component was modified.</para>
        /// </devdoc>
        public bool EditComponent(object component) {
            return EditComponent(null, component);
        }
    
        /// <devdoc>
        ///    <para>Gets a value indicating whether the component was modified.</para>
        /// </devdoc>
        public abstract bool EditComponent(ITypeDescriptorContext context, object component);
    }
}
