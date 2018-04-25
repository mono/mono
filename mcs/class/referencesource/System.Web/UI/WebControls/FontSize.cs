//------------------------------------------------------------------------------
// <copyright file="FontSize.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

// FontSize.cs
//

namespace System.Web.UI.WebControls {
    
    using System;


    /// <devdoc>
    ///    <para>
    ///       Specifies the font size.
    ///    </para>
    /// </devdoc>
    public enum FontSize {


        /// <devdoc>
        ///    <para>
        ///       The font size is not set.
        ///    </para>
        /// </devdoc>
        NotSet = 0,
        

        /// <devdoc>
        ///    <para>The font size is specified as point values.</para>
        /// </devdoc>
        AsUnit = 1,
        

        /// <devdoc>
        ///    <para>
        ///       The font size is smaller.
        ///    </para>
        /// </devdoc>
        Smaller = 2,


        /// <devdoc>
        ///    <para>
        ///       The font size is larger.
        ///    </para>
        /// </devdoc>
        Larger = 3,


        /// <devdoc>
        ///    <para>
        ///       The font size is extra extra small.
        ///    </para>
        /// </devdoc>
        XXSmall = 4,


        /// <devdoc>
        ///    <para>
        ///       The font size is extra small.
        ///    </para>
        /// </devdoc>
        XSmall = 5,


        /// <devdoc>
        ///    <para> The font size is small.</para>
        /// </devdoc>
        Small = 6,


        /// <devdoc>
        ///    <para>
        ///       The font size is medium.
        ///    </para>
        /// </devdoc>
        Medium = 7,


        /// <devdoc>
        ///    <para>
        ///       The font size is large.
        ///    </para>
        /// </devdoc>
        Large = 8,


        /// <devdoc>
        ///    <para>
        ///       The font size is extra large.
        ///    </para>
        /// </devdoc>
        XLarge = 9,


        /// <devdoc>
        ///    <para>
        ///       The font size is extra extra large.
        ///    </para>
        /// </devdoc>
        XXLarge = 10
    }
}
