//------------------------------------------------------------------------------
// <copyright file="ImageAlign.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {
    
    using System;


    /// <devdoc>
    ///    <para>
    ///       Specifies the alignment of
    ///       images within the text flow on the page.
    ///    </para>
    /// </devdoc>
    public enum ImageAlign {


        /// <devdoc>
        ///    <para>
        ///       The alignment is not set.
        ///    </para>
        /// </devdoc>
        NotSet = 0,
        

        /// <devdoc>
        ///    <para>The image is aligned to the left with
        ///       text wrapping on the right.</para>
        /// </devdoc>
        Left = 1,


        /// <devdoc>
        ///    <para>The image is aligned to the right with
        ///       text wrapping on the left.</para>
        /// </devdoc>
        Right = 2,


        /// <devdoc>
        ///    <para>The bottom of the image is aligned with the bottom of the first line of wrapping
        ///       text.</para>
        /// </devdoc>
        Baseline = 3,


        /// <devdoc>
        ///    <para>The image is aligned with the top of the the highest element on the same line.</para>
        /// </devdoc>
        Top = 4,


        /// <devdoc>
        ///    <para>The middle of the image is aligned with the bottom of the first 
        ///       line of wrapping text.</para>
        /// </devdoc>
        Middle = 5,


        /// <devdoc>
        ///    <para>The bottom of the image is aligned with the bottom of the first line of wrapping text.</para>
        /// </devdoc>
        Bottom = 6,


        /// <devdoc>
        ///    <para>The bottom of the image is aligned with the bottom 
        ///       of the largest element on the same line.</para>
        /// </devdoc>
        AbsBottom = 7,


        /// <devdoc>
        ///    <para> The middle of the image is aligned with the middle of the largest element on the 
        ///       same line.</para>
        /// </devdoc>
        AbsMiddle = 8,


        /// <devdoc>
        ///    <para>The image is aligned with the top of the the highest text on the same 
        ///       line.</para>
        /// </devdoc>
        TextTop = 9
    }
}

