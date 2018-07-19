//------------------------------------------------------------------------------
// <copyright file="TreeNodeSelectAction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.UI.WebControls {
    using System;


    /// <devdoc>
    ///    Specifies the action the TreeView takes when a node is selected
    /// </devdoc>
    public enum TreeNodeSelectAction {

        /// <devdoc>
        ///    Select the node
        /// </devdoc>
        Select = 0,


        /// <devdoc>
        ///    Expand the node
        /// </devdoc>
        Expand = 1,


        /// <devdoc>
        ///    Select and expand the node
        /// </devdoc>
        SelectExpand = 2,


        /// <devdoc>
        ///    Do nothing when clicking on a node
        /// </devdoc>
        None = 3
    }
}
