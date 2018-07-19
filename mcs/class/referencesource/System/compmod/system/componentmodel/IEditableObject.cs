//------------------------------------------------------------------------------
// <copyright file="IEditableObject.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * An object that can rollback edits.
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */
namespace System.ComponentModel {

    using System.Diagnostics;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public interface IEditableObject {
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        void BeginEdit();
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        void EndEdit();
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        void CancelEdit();
    }
}
