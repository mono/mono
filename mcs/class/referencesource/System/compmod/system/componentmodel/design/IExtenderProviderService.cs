//------------------------------------------------------------------------------
// <copyright file="IExtenderProviderService.cs" company="Microsoft">
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
    ///    <para>Provides an interface to add and remove extender providers.</para>
    /// </devdoc>
    public interface IExtenderProviderService {

        /// <devdoc>
        ///    <para>
        ///       Adds an extender provider.
        ///    </para>
        /// </devdoc>
        void AddExtenderProvider(IExtenderProvider provider);

        /// <devdoc>
        ///    <para>
        ///       Removes
        ///       an extender provider.
        ///    </para>
        /// </devdoc>
        void RemoveExtenderProvider(IExtenderProvider provider);
    }
}

