//------------------------------------------------------------------------------
// <copyright file="ValidationDataType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {
        

    /// <devdoc>
    ///    <para>Specifies the validation data types to be used by the 
    ///    <see cref='System.Web.UI.WebControls.CompareValidator'/> and <see cref='System.Web.UI.WebControls.RangeValidator'/> 
    ///    controls.</para>
    /// </devdoc>
    public enum ValidationDataType {

        /// <devdoc>
        ///    <para>The data type is String.</para>
        /// </devdoc>
        String = 0,

        /// <devdoc>
        ///    <para>The data type is Integer.</para>
        /// </devdoc>
        Integer = 1,

        /// <devdoc>
        ///    <para>The data type is Double.</para>
        /// </devdoc>
        Double = 2,

        /// <devdoc>
        ///    <para>The data type is DataTime.</para>
        /// </devdoc>
        Date = 3,

        /// <devdoc>
        ///    <para>The data type is Currency.</para>
        /// </devdoc>
        Currency = 4
    }
}

