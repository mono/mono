/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System.Collections;

    // REVIEW: Should we make this public?
    internal interface IBuildManager {
        object CreateInstanceFromVirtualPath(string virtualPath, Type requiredBaseType);
        ICollection GetReferencedAssemblies();
    }
}
