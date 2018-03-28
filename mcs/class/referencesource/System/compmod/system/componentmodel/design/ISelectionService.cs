//------------------------------------------------------------------------------
// <copyright file="ISelectionService.cs" company="Microsoft">
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
    using Microsoft.Win32;

    /// <devdoc>
    ///    <para>
    ///       Provides an interface for a designer to select components.
    ///    </para>
    /// </devdoc>
    [System.Runtime.InteropServices.ComVisible(true)]
    public interface ISelectionService {

        /// <devdoc>
        ///    <para>
        ///       Gets the object that is currently the primary selection.
        ///    </para>
        /// </devdoc>
        object PrimarySelection { get; }
        
        /// <devdoc>
        ///    <para>
        ///       Gets the count of selected objects.
        ///    </para>
        /// </devdoc>
        int SelectionCount { get; }

        /// <devdoc>
        ///    <para>
        ///       Adds a <see cref='System.ComponentModel.Design.ISelectionService.SelectionChanged'/> event handler to the selection service.
        ///    </para>
        /// </devdoc>
        event EventHandler SelectionChanged;

        /// <devdoc>
        ///    <para>
        ///       Adds an event handler to the selection service.
        ///    </para>
        /// </devdoc>
        event EventHandler SelectionChanging;

        /// <devdoc>
        ///    <para>Gets a value indicating whether the component is currently selected.</para>
        /// </devdoc>

        bool GetComponentSelected(object component);

        /// <devdoc>
        ///    <para>
        ///       Gets a collection of components that are currently part of the user's selection.
        ///    </para>
        /// </devdoc>
        ICollection GetSelectedComponents();

        /// <devdoc>
        ///    <para>
        ///       Sets the currently selected set of components.
        ///    </para>
        /// </devdoc>
        void SetSelectedComponents(ICollection components);

        /// <devdoc>
        ///    <para>
        ///       Sets the currently selected set of components to those with the specified selection type within the specified array of components.
        ///    </para>
        /// </devdoc>
        void SetSelectedComponents(ICollection components, SelectionTypes selectionType);
    }
}

