//------------------------------------------------------------------------------
// <copyright file="XmlReadMode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public enum XmlReadMode {    
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Auto = 0,
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        ReadSchema = 1,
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        IgnoreSchema = 2,
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        InferSchema = 3,
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        DiffGram = 4,
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Fragment = 5,
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>        
        InferTypedSchema = 6
    }
}
