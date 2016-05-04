//------------------------------------------------------------------------------
// <copyright file="UnitType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <devdoc>
    ///    <para> Specifies the unit types.</para>
    /// </devdoc>
    public enum UnitType {

        // NOTE: There is no enumeration value with '0' for a reason.
        //    Unit is a value class, and so when a Unit is created it
        //    is all zero'd out. We don't want that to imply 0px and
        //    we also use a 0 for type to imply its equal to Unit.Empty.
        // NotSet = 0,


        /// <devdoc>
        ///    A pixel.
        /// </devdoc>
        Pixel = 1,


        /// <devdoc>
        ///    A point.
        /// </devdoc>
        Point = 2,


        /// <devdoc>
        ///    A pica.
        /// </devdoc>
        Pica = 3,


        /// <devdoc>
        ///    An inch.
        /// </devdoc>
        Inch = 4,


        /// <devdoc>
        ///    A millimeter.
        /// </devdoc>
        Mm = 5,


        /// <devdoc>
        ///    <para>A centimeter.</para>
        /// </devdoc>
        Cm = 6,


        /// <devdoc>
        ///    A percentage.
        /// </devdoc>
        Percentage = 7,


        /// <devdoc>
        ///    <para> 
        ///       A unit of font width relative to its parent element's font.</para>
        ///    <para>For example, if the font size of a phrase is 2em and it is within a paragraph 
        ///       whose font size is 10px, then the font size of the phrase is 20px.</para>
        ///    <para>Refer to the World Wide Web Consortium Website for more information. </para>
        /// </devdoc>
        Em = 8,


        /// <devdoc>
        ///    <para>A unit of font height relative to its parent 
        ///       element's font.</para>
        ///    <para>Refer to the World Wide Web Consortium Website for more 
        ///       information. </para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix",
            Justification = "This is the correct name for a unit of measurement.")]
        Ex = 9
    }
}
