//------------------------------------------------------------------------------
// <copyright file="XmlContent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">amirhmy</owner>
// <owner current="true" primary="false">markash</owner>
// <owner current="false" primary="false">jasonzhu</owner>
//------------------------------------------------------------------------------
 
namespace System.Data {

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    internal enum XmlContent {
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        None = 0,
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Empty = 1,
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        TextOnly = 2,
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        EltOnly = 3,
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Mixed = 4,
    }
}
