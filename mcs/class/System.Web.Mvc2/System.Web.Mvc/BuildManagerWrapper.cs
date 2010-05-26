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
    using System.IO;
    using System.Web.Compilation;

    internal sealed class BuildManagerWrapper : IBuildManager {
        private static readonly Func<string, Stream> _readCachedFileDelegate =
            TypeHelpers.CreateDelegate<Func<string, Stream>>(typeof(BuildManager), "ReadCachedFile", null /* thisParameter */);
        private static readonly Func<string, Stream> _createCachedFileDelegate =
            TypeHelpers.CreateDelegate<Func<string, Stream>>(typeof(BuildManager), "CreateCachedFile", null /* thisParameter */);

        #region IBuildManager Members
        object IBuildManager.CreateInstanceFromVirtualPath(string virtualPath, Type requiredBaseType) {
            return BuildManager.CreateInstanceFromVirtualPath(virtualPath, requiredBaseType);
        }

        ICollection IBuildManager.GetReferencedAssemblies() {
            return BuildManager.GetReferencedAssemblies();
        }

        // ASP.NET 4 methods
        Stream IBuildManager.ReadCachedFile(string fileName) {
            return (_readCachedFileDelegate != null) ? _readCachedFileDelegate(fileName) : null;
        }

        Stream IBuildManager.CreateCachedFile(string fileName) {
            return (_createCachedFileDelegate != null) ? _createCachedFileDelegate(fileName) : null;
        }
        #endregion
    }
}
