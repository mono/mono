//------------------------------------------------------------------------------
// <copyright file="IExtenderListService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel.Design {
    using System.Diagnostics;
    using System;
    using System.ComponentModel;
    using Microsoft.Win32;

    /// <devdoc>
    ///    <para>Provides an interface to list extender providers.</para>
    /// </devdoc>
    public interface IExtenderListService {

        /// <devdoc>
        ///    <para>Gets the set of extender providers for the component.</para>
        /// </devdoc>
        IExtenderProvider[] GetExtenderProviders();
    }

}
