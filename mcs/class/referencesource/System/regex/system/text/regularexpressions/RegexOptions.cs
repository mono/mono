//------------------------------------------------------------------------------
// <copyright file="RegexOptions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------


namespace System.Text.RegularExpressions {

using System;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [Flags]
    public enum RegexOptions {
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        None =                     0x0000,  

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        IgnoreCase =               0x0001,      // "i"

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Multiline =                0x0002,      // "m"
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        ExplicitCapture =          0x0004,      // "n"
#if !SILVERLIGHT || FEATURE_LEGACYNETCF
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Compiled =                 0x0008,      // "c"
#endif
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Singleline =               0x0010,      // "s"
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        IgnorePatternWhitespace =  0x0020,      // "x"
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        RightToLeft =              0x0040,      // "r"

#if DBG
        /// <internalonly/>
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Debug=                     0x0080,      // "d"
#endif

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        ECMAScript =                  0x0100,      // "e"

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        CultureInvariant =                  0x0200,
    }


}

