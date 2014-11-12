//------------------------------------------------------------------------------
// <copyright file="PropertyChangingEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {
    
    using System;
    using System.Diagnostics;
    using System.Security.Permissions;

    /// <devdoc>
    /// <para>Provides data for the <see langword='PropertyChanging'/>
    /// event.</para>
    /// </devdoc>
#if !SILVERLIGHT
    [HostProtection(SharedState = true)]
#endif
    public class PropertyChangingEventArgs : EventArgs {
        private readonly string propertyName;

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.PropertyChangingEventArgs'/>
        /// class.</para>
        /// </devdoc>
        public PropertyChangingEventArgs(string propertyName) {
            this.propertyName = propertyName;
        }

        /// <devdoc>
        ///    <para>Indicates the name of the property that is changing.</para>
        /// </devdoc>
        public virtual string PropertyName {
            get {
                return propertyName;
            }
        }
    }
}
