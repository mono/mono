//------------------------------------------------------------------------------
// <copyright file="InheritanceLevel.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {
    
    /// <devdoc>
    ///    <para>
    ///       Specifies
    ///       numeric IDs for different inheritance levels.
    ///    </para>
    /// </devdoc>
    public enum InheritanceLevel {
    
        /// <devdoc>
        ///      Indicates that the object is inherited.
        /// </devdoc>
        Inherited = 1,
        
        /// <devdoc>
        ///    <para>
        ///       Indicates that the object is inherited, but has read-only access.
        ///    </para>
        /// </devdoc>
        InheritedReadOnly = 2,
        
        /// <devdoc>
        ///      Indicates that the object is not inherited.
        /// </devdoc>
        NotInherited = 3,
    }
}

