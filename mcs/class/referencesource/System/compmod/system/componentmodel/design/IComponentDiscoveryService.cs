//------------------------------------------------------------------------------
// <copyright file="IComponentDiscoveryService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel.Design {
    using System;
    using System.Collections;
    using System.ComponentModel.Design;

    /// <devdoc>
    /// This service allows design-time enumeration of components across the toolbox
    /// and other available types at design-time.
    /// </devdoc>
    public interface IComponentDiscoveryService {

        /// <devdoc>
        ///     Retrieves the list of available component types, i.e. types implementing
        ///     IComponent. If baseType is null, all components are retrieved; otherwise
        ///     only component types derived from the specified baseType are returned.
        /// </devdoc>    
        ICollection GetComponentTypes(IDesignerHost designerHost, Type baseType);
    }
}
