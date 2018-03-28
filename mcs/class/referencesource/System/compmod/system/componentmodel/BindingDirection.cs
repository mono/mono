//------------------------------------------------------------------------------
// <copyright file="BindingDirection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.ComponentModel {

    /// <devdoc>
    /// <para>Specifies whether the template can be bound one-way or two-way.</para>
    /// </devdoc>
    public enum BindingDirection {
        
        /// <devdoc>
        /// <para>The template can only accept property values.  Used with a generic ITemplate.</para>
        /// </devdoc>
        OneWay = 0,
        
        /// <devdoc>
        /// <para>The template can accept and expose property values.  Used with an IBindableTemplate.</para>
        /// </devdoc>
        TwoWay = 1
    }
}
