//------------------------------------------------------------------------------
// <copyright file="ICompositeControlDesignerAccessor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    
    // TODO, nikhilko: Change namespace to System.Web.UI


    /// <devdoc>
    /// Allows the designer of a composite control to recreate the composite control's child controls.
    ///
    /// ****************************************************************************
    /// THIS IS AN INTERIM SOLUTION UNTIL FRIEND ASSEMBLY FUNCTIONALITY COMES ONLINE
    /// ****************************************************************************
    ///
    /// </devdoc>
    public interface ICompositeControlDesignerAccessor {


        /// <devdoc>
        /// Recreates the child controls.
        /// </devdoc>
        void RecreateChildControls();
    }

}
