//------------------------------------------------------------------------------
// <copyright file="HelpKeywordType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel.Design {
    using System.Runtime.Remoting;
    using System.ComponentModel;

    using System.Diagnostics;
    using System;

    /// <devdoc>
    ///    <para>
    ///       Specifies identifiers that can be
    ///       used to indicate the type of a help keyword.
    ///    </para>
    /// </devdoc>
    public enum HelpKeywordType {
        /// <devdoc>
        ///    <para>
        ///       Indicates the keyword is a word F1 was pressed to request help regarding.
        ///    </para>
        /// </devdoc>
        F1Keyword,
        /// <devdoc>
        ///    <para>
        ///       Indicates the keyword is a general keyword.
        ///    </para>
        /// </devdoc>
        GeneralKeyword,
        /// <devdoc>
        ///    <para>
        ///       Indicates the keyword is a filter keyword.
        ///    </para>
        /// </devdoc>
        FilterKeyword
    }
}
