//------------------------------------------------------------------------------
// <copyright file="PersistenceMode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;


    /// <devdoc>
    ///    <para>Specifies whether properties and events are presistable 
    ///       in an HTML tag.</para>
    /// </devdoc>
    public enum PersistenceMode {


        /// <devdoc>
        ///    <para>The property or event is persistable in the HTML tag as an attribute.</para>
        /// </devdoc>
        Attribute = 0,


        /// <devdoc>
        ///    <para>The property or event is persistable within the HTML tag.</para>
        /// </devdoc>
        InnerProperty = 1,


        /// <devdoc>
        ///    <para>The property or event is persistable within the HTML tag as a child. Only
        ///    a single property can be marked as InnerDefaultProperty.</para>
        /// </devdoc>
        InnerDefaultProperty = 2,


        /// <devdoc>
        ///    <para>The property or event is persistable within the HTML tag as a child. Only
        ///    a single property can be marked as InnerDefaultProperty. Furthermode, this
        ///    persistence mode can only be applied to string properties.</para>
        /// </devdoc>
        EncodedInnerDefaultProperty = 3
    }
}
