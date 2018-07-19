//------------------------------------------------------------------------------
// <copyright file="ValidationCompareOperator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {
    

    /// <devdoc>
    ///    <para>Specifies the validation comparison operators to be used by 
    ///       the <see cref='System.Web.UI.WebControls.CompareValidator'/>
    ///       control.</para>
    /// </devdoc>
    public enum ValidationCompareOperator {

        /// <devdoc>
        ///    <para>The Equal comparison operator.</para>
        /// </devdoc>
        Equal = 0,

        /// <devdoc>
        ///    <para>The NotEqual comparison operator.</para>
        /// </devdoc>
        NotEqual = 1,

        /// <devdoc>
        ///    <para>The GreaterThan comparison operator.</para>
        /// </devdoc>
        GreaterThan = 2,

        /// <devdoc>
        ///    <para>The GreaterThanEqual comparison operator.</para>
        /// </devdoc>
        GreaterThanEqual = 3,

        /// <devdoc>
        ///    <para>The LessThan comparison operator.</para>
        /// </devdoc>
        LessThan = 4,

        /// <devdoc>
        ///    <para>The LessThanEqual comparison operator.</para>
        /// </devdoc>
        LessThanEqual = 5,

        /// <devdoc>
        ///    <para>The DataTypeCheck comparison operator. Specifies that only data 
        ///       type validity is to be performed.</para>
        /// </devdoc>
        DataTypeCheck = 6,
    }    
}

