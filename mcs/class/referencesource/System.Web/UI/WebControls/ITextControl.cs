//------------------------------------------------------------------------------
// <copyright file="IStaticTextControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Web.UI {
    
    /// <devdoc>
    ///    <para>Represents a control used for text rendering, with text that cannot be changed by the browser user.</para>
    /// </devdoc>
    public interface ITextControl {

        /// <devdoc>
        ///     The text of the control.
        /// </devdoc>
        string Text{get;set;}
    }
}
