//------------------------------------------------------------------------------
// <copyright file="DataErrorsChangedEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.ComponentModel {

    using System;
    using System.Diagnostics;
    using System.Security.Permissions;

    /// <devdoc>
    /// <para>Provides data for the <see langword='ErrorsChanged'/>
    /// event.</para>
    /// </devdoc>
#if !FEATURE_NETCORE
    [HostProtection(SharedState = true)]
#endif
    public class DataErrorsChangedEventArgs : EventArgs {
        private readonly string propertyName;

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.DataErrorsChangedEventArgs'/>
        /// class.</para>
        /// </devdoc>
        public DataErrorsChangedEventArgs(string propertyName) {
            this.propertyName = propertyName;
        }

        /// <devdoc>
        ///    <para>Indicates the name of the property whose errors changed.</para>
        /// </devdoc>
        public virtual string PropertyName {
            get {
                return propertyName;
            }
        }
    }
}
