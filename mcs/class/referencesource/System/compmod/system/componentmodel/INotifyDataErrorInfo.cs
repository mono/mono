//------------------------------------------------------------------------------
// <copyright file="INotifyDataErrorInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.ComponentModel {

    using System;
    using System.Collections;

    /// <devdoc>
    /// </devdoc>
    public interface INotifyDataErrorInfo {

        /// <devdoc>
        /// </devdoc>
        bool HasErrors { get; }

        /// <devdoc>
        /// </devdoc>
        IEnumerable GetErrors(string propertyName);

        /// <devdoc>
        /// </devdoc>
        event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
    }
}
