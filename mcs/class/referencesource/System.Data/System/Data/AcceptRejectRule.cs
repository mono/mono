//------------------------------------------------------------------------------
// <copyright file="AcceptRejectRule.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {

    public enum AcceptRejectRule {    
        /// <devdoc>
        ///    <para>
        ///       No action occurs.
        ///    </para>
        /// </devdoc>
        None = 0,
        /// <devdoc>
        ///    <para>
        ///       Changes are cascaded across the relationship.
        ///    </para>
        /// </devdoc>
        Cascade = 1
    }
}
