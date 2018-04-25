//------------------------------------------------------------------------------
// <copyright file="IIntellisenseBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.ComponentModel {

    /// <devdoc>
    /// 
    /// </devdoc>
    public interface IIntellisenseBuilder {

        /// <devdoc>
        /// Return a localized name.
        /// </devdoc>
        string Name { get; }

        /// <devdoc>
        /// Show the builder and return a boolean indicating whether value should be replaced with newValue
        /// - false if the user cancels for example
        ///
        /// language - indicates which language service is calling the builder
        /// value - expression being edited
        /// newValue - return the new value
        /// </devdoc> 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        bool Show(string language, string value, ref string newValue);       
    }
}
