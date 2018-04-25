//------------------------------------------------------------------------------
// <copyright file="ITypedList.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {

    using System;
    using System.ComponentModel;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public interface ITypedList {
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        string GetListName(PropertyDescriptor[] listAccessors);
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors);
    }
}
