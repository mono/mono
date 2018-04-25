//------------------------------------------------------------------------------
// <copyright file="DayNameFormat.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    

    /// <devdoc>
    ///    <para>
    ///       Specifies the format for the name of days.
    ///    </para>
    /// </devdoc>
    public enum DayNameFormat {

        /// <devdoc>
        ///    <para>
        ///       The day name displayed in full.
        ///    </para>
        /// </devdoc>
        Full = 0,

        /// <devdoc>
        ///    <para>
        ///       The day name displayed in short format.
        ///    </para>
        /// </devdoc>
        Short = 1,

        /// <devdoc>
        ///    <para>
        ///       The day name displayed with just the first letter.
        ///    </para>
        /// </devdoc>
        FirstLetter = 2,

        /// <devdoc>
        ///    <para>
        ///       The day name displayed with just the first two letters.
        ///    </para>
        /// </devdoc>
        FirstTwoLetters = 3,
        Shortest = 4
    }
}
