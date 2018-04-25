//------------------------------------------------------------------------------
// <copyright file="RefreshProperties.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {

    /// <devdoc>
    ///    <para>
    ///       Specifies identifiers that indicate the nature of
    ///       the refresh, for use in refreshing the design time view.
    ///    </para>
    /// </devdoc>
    public enum RefreshProperties {
        /// <devdoc>
        ///    <para>Indicates to use the no refresh mode.</para>
        /// </devdoc>
        None,
        /// <devdoc>
        ///    <para>Indicates to use the refresh all refresh mode.</para>
        /// </devdoc>
        All,
        /// <devdoc>
        ///    <para>Indicates to use the repaint refresh mode.</para>
        /// </devdoc>
        Repaint,
    }
}

