//------------------------------------------------------------------------------
// <copyright file="BulletMode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
   

    /// <devdoc>
    ///    <para> Specifies the style of the bulleted list.</para>
    /// </devdoc>
    public enum BulletStyle {

        NotSet = 0,
        /// <devdoc>
        ///    <para>The choices for an ordered list.</para>
        /// </devdoc>

        Numbered = 1,

        LowerAlpha = 2,

        UpperAlpha = 3,

        LowerRoman = 4,

        UpperRoman = 5,
        
        /// <devdoc>
        ///    <para>The choices for an unordered list.</para>
        /// </devdoc>

        Disc = 6,

        Circle = 7,

        Square = 8,
                
        /// <devdoc>
        ///    <para>The style that matches the Image type.</para>
        /// </devdoc>

        CustomImage = 9
    }


    /// <devdoc>
    ///    <para> Specifies the mode of the bulleted list.</para>
    /// </devdoc>
    public enum BulletedListDisplayMode 
    {

      Text = 0,

      HyperLink = 1,

      LinkButton = 2
    }

}
