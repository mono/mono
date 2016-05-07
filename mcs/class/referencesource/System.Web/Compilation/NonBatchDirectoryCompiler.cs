//------------------------------------------------------------------------------
// <copyright file="NonBatchDirectoryCompiler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------



namespace System.Web.Compilation {

    using System;
    using System.IO;
    using System.Collections;
    using System.Reflection;
    using System.Configuration;
    using System.Web.Hosting;
    using System.Web.Util;
    using System.Web.Caching;
    using System.Web.UI;
    using System.Web.Configuration;

    /*
    * This class makes sure that every file in a directory is compiled individually (i.e. it
    * doesn't use batching.  Also, the assemblies get stable names.
    */
    internal class NonBatchDirectoryCompiler {

        // The <compilation> config section for the set of build providers that we handle
        private CompilationSection _compConfig;

        private VirtualDirectory _vdir;

        internal NonBatchDirectoryCompiler(VirtualDirectory vdir) {
            _vdir = vdir;
            _compConfig = MTConfigUtil.GetCompilationConfig(_vdir.VirtualPath);
        }

        internal void Process() {

            foreach (VirtualFile vfile in _vdir.Files) {

                string extension = UrlPath.GetExtension(vfile.VirtualPath);

                // Skip any file for which we can't get a BuildProvider type, as it is not
                // compilable.
                Type buildProviderType = CompilationUtil.GetBuildProviderTypeFromExtension(_compConfig,
                    extension, BuildProviderAppliesTo.Web, false /*failIfUnknown*/);
                if (buildProviderType == null)
                    continue;

                // If it's a source file, skip it.  We need to do this for v1 compatibility,
                // since v1 VS projects contain many source files which have already been
                // precompiled into bin, and that should not be compiled dynamically
                if (buildProviderType == typeof(SourceFileBuildProvider))
                    continue;

                // For the same reason, skip resources
                if (buildProviderType == typeof(ResXBuildProvider))
                    continue;

                // Call GetVPathBuildResult to cause the file to be compiled.  We ignore the
                // return value.
                BuildManager.GetVPathBuildResult(vfile.VirtualPathObject);
            }
        }
    }

}
