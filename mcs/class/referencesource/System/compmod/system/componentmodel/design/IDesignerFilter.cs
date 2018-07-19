//------------------------------------------------------------------------------
// <copyright file="IDesignerFilter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel.Design {
    using System.Diagnostics;
    using System;
    using System.Collections;
    using System.ComponentModel;

    /// <devdoc>
    ///    <para>Provides access to, and
    ///       an interface for filtering, the dictionaries that store the properties, attributes, or events of a component.</para>
    /// </devdoc>
    public interface IDesignerFilter {

        /// <devdoc>
        ///    <para> Allows a designer to filter the set of
        ///       attributes the component being designed will expose through the <see cref='System.ComponentModel.TypeDescriptor'/> object.</para>
        /// </devdoc>
        void PostFilterAttributes(IDictionary attributes);

        /// <devdoc>
        ///    <para> Allows a designer to filter the set of events
        ///       the component being designed will expose through the <see cref='System.ComponentModel.TypeDescriptor'/>
        ///       object.</para>
        /// </devdoc>
        void PostFilterEvents(IDictionary events);

        /// <devdoc>
        ///    <para> Allows a designer to filter the set of properties
        ///       the component being designed will expose through the <see cref='System.ComponentModel.TypeDescriptor'/>
        ///       object.</para>
        /// </devdoc>
        void PostFilterProperties(IDictionary properties);

        /// <devdoc>
        ///    <para> Allows a designer to filter the set of
        ///       attributes the component being designed will expose through the <see cref='System.ComponentModel.TypeDescriptor'/>
        ///       object.</para>
        /// </devdoc>
        void PreFilterAttributes(IDictionary attributes);

        /// <devdoc>
        ///    <para> Allows a designer to filter the set of events
        ///       the component being designed will expose through the <see cref='System.ComponentModel.TypeDescriptor'/>
        ///       object.</para>
        /// </devdoc>
        void PreFilterEvents(IDictionary events);

        /// <devdoc>
        ///    <para> Allows a designer to filter the set of properties
        ///       the component being designed will expose through the <see cref='System.ComponentModel.TypeDescriptor'/>
        ///       object.</para>
        /// </devdoc>
        void PreFilterProperties(IDictionary properties);
    }
}

