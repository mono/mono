//------------------------------------------------------------------------------
// <copyright file="SelectionTypes.cs" company="Microsoft">
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
    ///     Specifies identifiers
    ///     that indicate the type
    ///     of selection for a component or group of components that are selected.
    /// </devdoc>
    [
        Flags,
        System.Runtime.InteropServices.ComVisible(true)
    ]
    public enum SelectionTypes {

        /// <devdoc>
        ///    A Normal selection. With this type of selection, the selection service responds
        ///    to the control and shift keys to support appending or toggling components into the
        ///    selection as needed.
        /// </devdoc>
        Auto    = 0x0001,

        /// <devdoc>
        ///    A Normal selection. With this type of selection, the selection service responds
        ///    to the control and shift keys to support appending or toggling components into the
        ///    selection as needed.
        /// </devdoc>
        [Obsolete("This value has been deprecated. Use SelectionTypes.Auto instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        Normal    = 0x0001,

        /// <devdoc>
        ///    A Replace selection. This causes the selection service to always replace the
        ///    current selection with the replacement.
        /// </devdoc>
        Replace   = 0x0002,

        /// <devdoc>
        ///     A MouseDown selection. Happens when the user presses down on
        ///     the mouse button when the pointer is over a control (or component). If a
        ///     component in the selection list is already selected, it does not remove the
        ///     existing selection, but promotes that component to be the primary selection.
        /// </devdoc>
        [Obsolete("This value has been deprecated.  It is no longer supported. http://go.microsoft.com/fwlink/?linkid=14202")]
        MouseDown = 0x0004,

        /// <devdoc>
        ///     A MouseUp selection. Happens when the user releases the
        ///     mouse button when a control (or component) has been selected. If a component
        ///     in the selection list is already selected, it does not remove the
        ///     existing selection, but promotes that component to be the primary selection.
        /// </devdoc>
        [Obsolete("This value has been deprecated.  It is no longer supported. http://go.microsoft.com/fwlink/?linkid=14202")]
        MouseUp  = 0x0008,

        /// <devdoc>
        ///     A Click selection.
        ///     Happens when a user clicks on a component. If a component in the selection list is already
        ///     selected, it does not remove the existing selection, but promotes that component to be the
        ///     primary selection.
        /// </devdoc>
        [Obsolete("This value has been deprecated. Use SelectionTypes.Primary instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        Click   = 0x0010,

        /// <devdoc>
        ///     A Primary selection.
        ///     Happens when a user clicks on a component. If a component in the selection list is already
        ///     selected, it does not remove the existing selection, but promotes that component to be the
        ///     primary selection.
        /// </devdoc>
        Primary   = 0x0010,

        /// <devdoc>
        ///     A toggle selection.
        ///     This selection toggles the current selection with the provided selection.  So, if 
        ///     a component is already selected and is passed into SetSelectedComponents with a
        ///     selection type of Toggle, it will be unselected.
        /// </devdoc>
        Toggle  = 0x0020,

        /// <devdoc>
        ///     An Add selection.
        ///     This selection adds the selected components to the current selection, 
        ///     maintaining the current set of selected components.
        /// </devdoc>
        Add     = 0x0040,

        /// <devdoc>
        ///     A Remove selection.
        ///     This selection removes the selected components from the current selection, 
        ///     maintaining the current set of selected components.
        /// </devdoc>
        Remove  = 0x0080,

        /// <devdoc>
        ///     Limits valid selection types to Normal, Replace, MouseDown, MouseUp,
        ///     Click, Toggle or Add.
        /// </devdoc>
        [Obsolete("This value has been deprecated. Use Enum class methods to determine valid values, or use a type converter. http://go.microsoft.com/fwlink/?linkid=14202")]
        Valid   = 0x1F,
    }
}
