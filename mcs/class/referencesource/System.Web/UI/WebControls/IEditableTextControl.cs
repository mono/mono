//------------------------------------------------------------------------------
// <copyright file="IStaticTextControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Web.UI {

    /// <devdoc>
    ///    <para>Represents a control used to render text that can be changed by the browser user.</para>
    /// </devdoc>
    public interface IEditableTextControl : ITextControl {
        /// <devdoc>
        ///     Raised when the text changes.
        /// </devdoc>
        event EventHandler TextChanged;
    }
}
