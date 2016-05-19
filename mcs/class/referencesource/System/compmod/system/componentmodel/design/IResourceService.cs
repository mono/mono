//------------------------------------------------------------------------------
// <copyright file="IResourceService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel.Design {

    using System.Globalization;
    using System.Resources;

    /// <devdoc>
    ///    <para> 
    ///       Provides designers a way to
    ///       access a resource for the current design-time
    ///       object.</para>
    /// </devdoc>
    public interface IResourceService {
    
        /// <devdoc>
        ///    <para> 
        ///       Locates the resource reader for the specified culture and
        ///       returns it.</para>
        /// </devdoc>
        IResourceReader GetResourceReader(CultureInfo info);
    
        /// <devdoc>
        ///    <para>Locates the resource writer for the specified culture
        ///       and returns it. This will create a new resource for
        ///       the specified culture and destroy any existing resource,
        ///       should it exist.</para>
        /// </devdoc>
        IResourceWriter GetResourceWriter(CultureInfo info);
    }
}

