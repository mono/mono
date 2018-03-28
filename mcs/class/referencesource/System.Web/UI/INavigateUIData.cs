//------------------------------------------------------------------------------
// <copyright file="INavigateDataUI.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    /// <devdoc>
    ///     Provides a strongly-typed way to get navigation UI-related properties
    /// </devdoc>
    public interface INavigateUIData {

        string Description {
            get;
        }
        

        /// <devdoc>
        /// </devdoc>
        string Name {
            get;
        }


        /// <devdoc>
        /// </devdoc>
        string NavigateUrl {
            get;
        }


        /// <devdoc>
        /// </devdoc>
        string Value {
            get;
        }

    }
}


