//------------------------------------------------------------------------------
// <copyright file="IBindableControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System.Collections.Specialized;

    public interface IBindableControl {

        /// <devdoc>
        /// Retrives the values of all control properties with two-way bindings.
        /// </devdoc>
        void ExtractValues(IOrderedDictionary dictionary);

    }
}

