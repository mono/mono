//------------------------------------------------------------------------------
// <copyright file="IBindableTemplate.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;
    using System.Collections.Specialized;

    /// <devdoc>
    /// </devdoc>
    public interface IBindableTemplate : ITemplate {

        /// <devdoc>
        /// Retrives the values of all control properties with two-way bindings.
        /// </devdoc>
        IOrderedDictionary ExtractValues(Control container);
    }
}

