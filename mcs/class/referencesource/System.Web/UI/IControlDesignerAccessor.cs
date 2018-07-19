//------------------------------------------------------------------------------
// <copyright file="IControlDesignerAccessor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System.Collections;

    /// <devdoc>
    /// Allows the designer of to both push and pull data from a control.
    ///
    /// ****************************************************************************
    /// THIS IS AN INTERIM SOLUTION UNTIL FRIEND ASSEMBLY FUNCTIONALITY COMES ONLINE
    /// ****************************************************************************
    ///
    /// </devdoc>
    public interface IControlDesignerAccessor {


        /// <devdoc>
        /// </devdoc>
        IDictionary UserData {
            get;
        }


        /// <devdoc>
        /// Gets design mode state from the control.
        /// </devdoc>
        IDictionary GetDesignModeState();


        /// <devdoc>
        /// Sets design mode state for the control before rendering at design-time.
        /// </devdoc>
        void SetDesignModeState(IDictionary data);

        void SetOwnerControl(Control owner);
    }

}
