//------------------------------------------------------------------------------
// <copyright file="IResourceUrlGenerator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    public interface IResourceUrlGenerator {


        /// <devdoc>
        /// </devdoc>
        string GetResourceUrl(Type type, string resourceName);
    }
}
