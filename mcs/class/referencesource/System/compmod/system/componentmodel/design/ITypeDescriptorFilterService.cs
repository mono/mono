//------------------------------------------------------------------------------
// <copyright file="ITypeDescriptorFilterService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel.Design {
    using System.Runtime.InteropServices;
    using System.Diagnostics;
    using System;
    using System.Collections;
    using System.ComponentModel;

    /// <devdoc>
    ///    <para>
    ///       Modifies the set of type descriptors that a component
    ///       provides.
    ///    </para>
    /// </devdoc>
    public interface ITypeDescriptorFilterService {

        /// <devdoc>
        ///    <para>
        ///       Provides a way to filter the attributes from a component that are displayed to the user.
        ///    </para>
        /// </devdoc>
        bool FilterAttributes(IComponent component, IDictionary attributes);

        /// <devdoc>
        ///    <para>
        ///       Provides a way to filter the events from a component that are displayed to the user.
        ///    </para>
        /// </devdoc>
        bool FilterEvents(IComponent component, IDictionary events);

        /// <devdoc>
        ///    <para>
        ///       Provides a way to filter the properties from a component that are displayed to the user.
        ///    </para>
        /// </devdoc>
        bool FilterProperties(IComponent component, IDictionary properties);
    }
}

