//------------------------------------------------------------------------------
// <copyright file="TraceModeEnum.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * The different styles of trace output.
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web {

using System;


    /// <devdoc>
    ///    <para>Specifies how trace messages are emitted into the HTML output of a page.</para>
    /// </devdoc>
    public enum TraceMode {


        /// <devdoc>
        ///    <para>Specifies that trace messages should be emitted in the order they were 
        ///       processed.</para>
        /// </devdoc>
        SortByTime = 0,


        /// <devdoc>
        ///    <para>Specifies that trace messages should be emitted 
        ///       alphabetically by category. </para>
        /// </devdoc>
        SortByCategory = 1,


        /// <devdoc>
        /// <para>Specifies the default value, which is <see langword='SortByTime'/>.</para>
        /// </devdoc>
        Default = 2,
    }


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    internal enum TraceEnable {


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Default,


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Enable,


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Disable,

    }

}

