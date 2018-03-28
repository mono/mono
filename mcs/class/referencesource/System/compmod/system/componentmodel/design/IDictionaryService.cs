//------------------------------------------------------------------------------
// <copyright file="IDictionaryService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel.Design {
    using System.ComponentModel;

    using System.Diagnostics;
    
    using System;

    /// <devdoc>
    ///    <para>Provides a generic dictionary service that a designer can use
    ///       to store user-defined data on the site.</para>
    /// </devdoc>
    public interface IDictionaryService {
    
        /// <devdoc>
        ///    <para>
        ///       Gets the key corresponding to the specified value.
        ///    </para>
        /// </devdoc>
        object GetKey(object value);
        
        /// <devdoc>
        ///    <para>
        ///       Gets the value corresponding to the specified key.
        ///    </para>
        /// </devdoc>
        object GetValue(object key);
    
        /// <devdoc>
        ///    <para> 
        ///       Sets the specified key-value pair.</para>
        /// </devdoc>
        void SetValue(object key, object value);
    }
}
