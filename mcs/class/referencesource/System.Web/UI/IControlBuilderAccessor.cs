//------------------------------------------------------------------------------
// <copyright file="IControlBuilderAccessor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    /// <internalonly />
    /// <devdoc>
    /// Allows the ControlSerializer to get to the ControlBuilder for a Control.
    /// </devdoc>
    public interface IControlBuilderAccessor {

        ControlBuilder ControlBuilder {
            get;
        }
    }
}

