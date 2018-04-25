//------------------------------------------------------------------------------
// <copyright file="INestedSite.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {
    
    using System;

    /// <devdoc>
    ///     Nested containers site objects using INestedSite.  A nested
    ///     site is simply a site with an additional property that can
    ///     retrieve the full nested name of a component.
    /// </devdoc>
    public interface INestedSite : ISite {

        /// <devdoc>
        ///     Returns the full name of the component in this site in the format
        ///     of <owner>.<component>.  If this component's site has a null
        ///     name, FullName also returns null.
        /// </devdoc>
        string FullName { get; }
    }
}
