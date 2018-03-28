//------------------------------------------------------------------------------
// <copyright file="VerticalAlign.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {
    
    using System;
    using System.ComponentModel;
    

    /// <devdoc>
    ///    <para>
    ///       Specifies the vertical alignment of an object or text within a control.
    ///    </para>
    /// </devdoc>
    [ TypeConverterAttribute(typeof(VerticalAlignConverter)) ]
    public enum VerticalAlign {


        /// <devdoc>
        ///    <para>
        ///       Vertical
        ///       alignment property is not set.
        ///    </para>
        /// </devdoc>
        NotSet = 0,
        

        /// <devdoc>
        ///    <para>
        ///       The object or text is aligned with the top of the
        ///       enclosing control.
        ///    </para>
        /// </devdoc>
        Top = 1,


        /// <devdoc>
        ///    <para>
        ///       The object or text is placed
        ///       across the vertical center of the enclosing control.
        ///    </para>
        /// </devdoc>
        Middle = 2,


        /// <devdoc>
        ///    <para>
        ///       The object or text is aligned with the bottom of the enclosing
        ///       control.
        ///    </para>
        /// </devdoc>
        Bottom = 3
    }
}
