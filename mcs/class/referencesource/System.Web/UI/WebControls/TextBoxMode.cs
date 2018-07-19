//------------------------------------------------------------------------------
// <copyright file="TextBoxMode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    
    using System;

    /// <devdoc>
    ///    <para> Specifies the behavior mode of
    ///       the text box.</para>
    /// </devdoc>
    public enum TextBoxMode {
        SingleLine = 0,
        MultiLine = 1,
        Password = 2,
        // The following values (Color through Week) are taken from the HTML5 specification.
        Color = 3,
        Date = 4,
        DateTime = 5,
        DateTimeLocal = 6,
        Email = 7,
        Month = 8,
        Number = 9,
        Range = 10,
        Search = 11,
        Phone = 12,
        Time = 13,
        Url = 14,
        Week = 15
    }
}
