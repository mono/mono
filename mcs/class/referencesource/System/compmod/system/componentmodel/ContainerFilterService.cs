//------------------------------------------------------------------------------
// <copyright file="ContainerFilterService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {

    using System;
    using System.ComponentModel;
    using System.Security.Permissions;
    
    /// <devdoc>
    ///     The Container and NestedContainer classes will call GetService for ContainerFilterService
    ///     each time they need to construct a Components collection for return to a caller.  
    ///     ContainerFilterService may return an updated collection of components.  This allows
    ///     an external service to modify the view of components that are returned from a container.
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public abstract class ContainerFilterService
    {

        /// <devdoc>
        /// </devdoc>
        protected ContainerFilterService() {
        }

        /// <devdoc>
        ///     Filters the components collection by optionally returning a new, modified collection. 
        ///     The default implementation returns the input collection, thereby performing no filtering.
        /// </devdoc>
        public virtual ComponentCollection FilterComponents(ComponentCollection components) {
            return components;
        }
    }
}
