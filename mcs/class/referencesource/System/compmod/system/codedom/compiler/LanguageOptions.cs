//------------------------------------------------------------------------------
// <copyright file="LanguageOptions.cs" company="Microsoft">
// 
// <OWNER>petes</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.CodeDom.Compiler {
    
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [
        Flags,
        Serializable,
    ]
    public enum LanguageOptions {
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        None = 0x0,
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        CaseInsensitive = 0x1,
    }
}
