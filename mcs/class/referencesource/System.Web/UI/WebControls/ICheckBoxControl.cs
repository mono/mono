//------------------------------------------------------------------------------
// <copyright file="ICheckBoxControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.UI {

    /// <devdoc>
    ///    <para>Represents a control used to render a check box that can be changed by the browser user.</para>
    /// </devdoc>
    public interface ICheckBoxControl {

        /// <devdoc>
        ///     The checked state of the control.
        /// </devdoc>
        bool Checked { get; set; }

        /// <devdoc>
        ///     Raised when the checked state changes.
        /// </devdoc>
        event EventHandler CheckedChanged;
    }
}
