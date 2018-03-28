//------------------------------------------------------------------------------
// <copyright file="ITypeDiscoveryService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel.Design {
    using System;
    using System.Collections;

    /// <devdoc>
    /// The type discovery service is used to discover available types at design time,
    /// when the consumer doesn't know the names of existing types or referenced assemblies.
    /// </devdoc>
    public interface ITypeDiscoveryService {

        /// <devdoc>
        ///     Retrieves the list of available types. If baseType is null, all
        ///     types are returned. Otherwise, only types deriving from the
        ///     specified base type are returned. If bool excludeGlobalTypes is false, 
        ///     types from all referenced assemblies are checked. Otherwise,
        ///     only types from non-GAC referenced assemblies are checked. 
        /// </devdoc>
        ICollection GetTypes(Type baseType, bool excludeGlobalTypes);
    }
}
