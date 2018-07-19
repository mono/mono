//------------------------------------------------------------------------------
// <copyright file="BorderStyle.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    
    using System;


    /// <devdoc>
    ///    <para>
    ///       Specifies the basic border style of a control.
    ///    </para>
    /// </devdoc>
    public enum BorderStyle {


        /// <devdoc>
        ///    <para>
        ///       No border style set.
        ///    </para>
        /// </devdoc>
        NotSet = 0,
        

        /// <devdoc>
        ///    <para>
        ///       No border on the control.
        ///    </para>
        /// </devdoc>
        None = 1,


        /// <devdoc>
        ///    <para>
        ///       A dotted line border.
        ///    </para>
        /// </devdoc>
        Dotted = 2,


        /// <devdoc>
        ///    <para>
        ///       A dashed line border.
        ///    </para>
        /// </devdoc>
        Dashed = 3,


        /// <devdoc>
        ///    <para>
        ///       A solid line border.
        ///    </para>
        /// </devdoc>
        Solid = 4,


        /// <devdoc>
        ///    <para>
        ///       A double line border.
        ///    </para>
        /// </devdoc>
        Double = 5,


        /// <devdoc>
        ///    <para>
        ///       A grooved line border.
        ///    </para>
        /// </devdoc>
        Groove = 6,


        /// <devdoc>
        ///    <para>
        ///       A ridge line border.
        ///    </para>
        /// </devdoc>
        Ridge = 7,


        /// <devdoc>
        ///    <para>
        ///       An inset line border.
        ///    </para>
        /// </devdoc>
        Inset = 8,


        /// <devdoc>
        ///    <para>
        ///       An outset line border.
        ///    </para>
        /// </devdoc>
        Outset = 9
    }
}
