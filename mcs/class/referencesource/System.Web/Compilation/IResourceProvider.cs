//------------------------------------------------------------------------------
// <copyright file="IResourceProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Compilation {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.IO;
    using System.Security.Permissions;
    using System.CodeDom;
    using System.Globalization;
    using System.Resources;
    using System.Web.Compilation;
    using System.Web.Util;
    using System.Web.UI;


    /*
     * Basic interface to access and enumerate resources
     */
    public interface IResourceProvider {

        /*
         * Retrieve a resource object for the passed in key and culture
         */
        object GetObject(string resourceKey, CultureInfo culture);

        /*
         * Returns a reader that enumerates all the neutral resources for this provider
         */
        IResourceReader ResourceReader { get; }
    }

    /*
     * Base class for resource providers based on a standard ResourceManager
     */
    internal abstract class BaseResXResourceProvider: IResourceProvider {

        private ResourceManager _resourceManager;

        ///// IResourceProvider implementation

        public virtual object GetObject(string resourceKey, CultureInfo culture) {

            // Attempt to get the resource manager
            EnsureResourceManager();

            // If we couldn't get a resource manager, return null
            if (_resourceManager == null)
                return null;

            if (culture == null)
                culture = CultureInfo.CurrentUICulture;

            return _resourceManager.GetObject(resourceKey, culture);
        }

        public virtual IResourceReader ResourceReader { get { return null; } }

        ///// End of IResourceProvider implementation


        protected abstract ResourceManager CreateResourceManager();

        private void EnsureResourceManager() {
            if (_resourceManager != null)
                return;

            _resourceManager = CreateResourceManager();
        }
    }

    /*
     * ResourceManager based provider for application resources.
     */
    internal class GlobalResXResourceProvider : BaseResXResourceProvider {

        private string _classKey;

        internal GlobalResXResourceProvider(string classKey) {
            _classKey = classKey;
        }

        protected override ResourceManager CreateResourceManager() {

            string fullClassName = BaseResourcesBuildProvider.DefaultResourcesNamespace +
                "." + _classKey;

            // If there is no app resource assembly, return null
            if (BuildManager.AppResourcesAssembly == null)
                return null;

            ResourceManager resourceManager = new ResourceManager(fullClassName,
                BuildManager.AppResourcesAssembly);
            resourceManager.IgnoreCase = true;

            return resourceManager;
        }

        public override IResourceReader ResourceReader {
            get {
                // App resources don't support implicit resources, so the IResourceReader
                // should never be needed
                throw new NotSupportedException();
            }
        }
    }

    /*
     * ResourceManager based provider for page (local) resources.
     */
    internal class LocalResXResourceProvider : BaseResXResourceProvider {

        private VirtualPath _virtualPath;

        internal LocalResXResourceProvider(VirtualPath virtualPath) {
            _virtualPath = virtualPath;
        }

        protected override ResourceManager CreateResourceManager() {

            ResourceManager resourceManager = null;

            Assembly pageResAssembly = GetLocalResourceAssembly();

            if (pageResAssembly != null) {
                string fileName = _virtualPath.FileName;

                resourceManager = new ResourceManager(fileName, pageResAssembly);
                resourceManager.IgnoreCase = true;
            }
            else {
                throw new InvalidOperationException(SR.GetString(SR.ResourceExpresionBuilder_PageResourceNotFound));
            }

            return resourceManager;
        }

        public override IResourceReader ResourceReader {
            get {
                // Get the local resource assembly for this page
                Assembly pageResAssembly = GetLocalResourceAssembly();

                if (pageResAssembly == null)
                    return null;

                // Get the name of the embedded .resource file for this page
                string resourceFileName = _virtualPath.FileName + ".resources";

                // Make it lower case, since GetManifestResourceStream is case sensitive
                resourceFileName = resourceFileName.ToLower(CultureInfo.InvariantCulture);

                // Get the resource stream from the resource assembly
                Stream resourceStream = pageResAssembly.GetManifestResourceStream(resourceFileName);

                // If this page has no resources, return null
                if (resourceStream == null)
                    return null;

                return new ResourceReader(resourceStream);
            }
        }

        // Need to Assert here in order to access the codegen dir (VSWhidbey 387312)
        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private Assembly GetLocalResourceAssembly() {
            // Remove the page file name to get its directory
            VirtualPath virtualDir = _virtualPath.Parent;

            // Get the name of the local resource assembly
            string cacheKey = BuildManager.GetLocalResourcesAssemblyName(virtualDir);
            BuildResult result = BuildManager.GetBuildResultFromCache(cacheKey);

            if (result != null) {
                return ((BuildResultCompiledAssembly)result).ResultAssembly;
            }

            return null;
        }
    }

}

