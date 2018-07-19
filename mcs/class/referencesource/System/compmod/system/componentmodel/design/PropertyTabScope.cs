//------------------------------------------------------------------------------
// <copyright file="PropertyTabScope.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {

    /// <devdoc>
    ///    <para>
    ///       Specifies the function scope of
    ///       a tab in the properties window.
    ///    </para>
    /// </devdoc>
    public enum PropertyTabScope{
            /// <devdoc>
            ///    <para>
            ///       This tab will be added to the properties window and can never be
            ///       removed.
            ///    </para>
            /// </devdoc>
            Static = 0,
            
            /// <devdoc>
            ///    <para>
            ///       This tab will be added to the properties window and can
            ///       only be explictly removed by a component outside the properties window.
            ///    </para>
            /// </devdoc>
            Global = 1,
            
            /// <devdoc>
            ///    <para>
            ///       This tab will be added to the properties window
            ///       and will be removed when the currently selected document changes. This tab is relevant to
            ///       items on the current document.
            ///    </para>
            /// </devdoc>
            Document = 2,
            
            /// <devdoc>
            ///    <para>
            ///       This tab will be added to the properties window for the current component only, and is
            ///       removed when the component is no longer selected.
            ///    </para>
            /// </devdoc>
            Component = 3,
    }
}
