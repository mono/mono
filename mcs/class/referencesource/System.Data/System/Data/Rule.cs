
//------------------------------------------------------------------------------
// <copyright file="Rule.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">amirhmy</owner>
// <owner current="true" primary="false">markash</owner>
// <owner current="false" primary="false">jasonzhu</owner>
//------------------------------------------------------------------------------

namespace System.Data {

    /// <devdoc>
    /// <para>Indicates the action that occurs when a <see cref='System.Data.ForeignKeyConstraint'/>
    /// is enforced.</para>
    /// </devdoc>
    public enum Rule {
    
        /// <devdoc>
        ///    <para>
        ///       No action occurs.
        ///    </para>
        /// </devdoc>
        None = 0,
        /// <devdoc>
        ///    <para>
        ///       Changes are cascaded through the relationship.
        ///    </para>
        /// </devdoc>
        Cascade = 1,
        /// <devdoc>
        ///    <para>
        ///       Null values are set in the rows affected by the deletion.
        ///    </para>
        /// </devdoc>
        SetNull = 2,
        /// <devdoc>
        ///    <para>
        ///       Default values are set in the rows affected by the deletion.
        ///    </para>
        /// </devdoc>
        SetDefault = 3
    }
}
