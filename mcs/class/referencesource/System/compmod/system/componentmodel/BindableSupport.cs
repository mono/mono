//------------------------------------------------------------------------------
// <copyright file="BindableSupport.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {
    

    using System.Diagnostics;
    using System;

    /// <devdoc>
    ///    <para>Specifies which values to say if property or event value can be bound to a data
    ///          element or another property or event's value.</para>
    /// </devdoc>
    public enum BindableSupport {
        /// <devdoc>
        ///    <para>
        ///       The property or event is bindable.
        ///    </para>
        /// </devdoc>
        No        = 0x00,
        /// <devdoc>
        ///    <para>
        ///       The property or event is not bindable.
        ///    </para>
        /// </devdoc>
        Yes = 0x01,
        /// <devdoc>
        ///    <para>
        ///       The property or event is the default.
        ///    </para>
        /// </devdoc>
        Default        = 0x02,
    }
}
