//---------------------------------------------------------------------
// <copyright file="MetadataArtifactLoaderCompositeResource.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Xml;

    /// <summary>
    /// This class represents a collection of resources to be loaded from one
    /// or more assemblies.
    /// </summary>
    internal class MetadataArtifactLoaderCompositeResource : MetadataArtifactLoader
    {
        /// <summary>
        /// The list of metadata artifacts encapsulated by the composite.
        /// </summary>
        private readonly ReadOnlyCollection<MetadataArtifactLoaderResource> _children;
        private readonly string _originalPath;
        
        /// <summary>
        /// This constructor expects to get the paths that have potential to turn into multiple 
        /// artifacts like
        /// 
        /// res://*/foo.csdl   -- could be multiple assemblies
        /// res://MyAssembly/  -- could be multiple artifacts in the one assembly
        /// 
        /// </summary>
        /// <param name="path">The path to the (collection of) resources</param>
        /// <param name="uriRegistry">The global registry of URIs</param>
        /// <param name="resolveAssembly"></param>
        internal MetadataArtifactLoaderCompositeResource(string originalPath, string assemblyName, string resourceName, ICollection<string> uriRegistry, MetadataArtifactAssemblyResolver resolver)
        {
            Debug.Assert(resolver != null);

            _originalPath = originalPath;
            _children = LoadResources(assemblyName, resourceName, uriRegistry, resolver).AsReadOnly();
        }

        public override string Path
        {
            get { return _originalPath; }
        }

        public override bool IsComposite
        {
            get
            {
                return true;
            }
        }

        public override void CollectFilePermissionPaths(List<string> paths, DataSpace spaceToGet)
        {
            foreach (var loader in _children)
            {
                loader.CollectFilePermissionPaths(paths, spaceToGet);
            }
        }

        /// <summary>
        /// Get paths to artifacts for a specific DataSpace, in the original, unexpanded 
        /// form.
        /// </summary>
        /// <remarks>An assembly can embed any kind of artifact as a resource, so we simply
        /// ignore the parameter and return the original assembly name in the URI.</remarks>
        /// <param name="spaceToGet">The DataSpace for the artifacts of interest</param>
        /// <returns>A List of strings identifying paths to all artifacts for a specific DataSpace</returns>
        public override List<string> GetOriginalPaths(DataSpace spaceToGet)
        {
            return GetOriginalPaths();
        }

        /// <summary>
        /// Get paths to artifacts for a specific DataSpace.
        /// </summary>
        /// <param name="spaceToGet">The DataSpace for the artifacts of interest</param>
        /// <returns>A List of strings identifying paths to all artifacts for a specific DataSpace</returns>
        public override List<string> GetPaths(DataSpace spaceToGet)
        {
            List<string> list = new List<string>();

            foreach (MetadataArtifactLoaderResource resource in _children)
            {
                list.AddRange(resource.GetPaths(spaceToGet));
            }

            return list;
        }

        /// <summary>
        /// Get paths to all artifacts
        /// </summary>
        /// <returns>A List of strings identifying paths to all resources</returns>
        public override List<string> GetPaths()
        {
            List<string> list = new List<string>();

            foreach (MetadataArtifactLoaderResource resource in _children)
            {
                list.AddRange(resource.GetPaths());
            }

            return list;
        }

        /// <summary>
        /// Aggregates all resource streams from the _children collection
        /// </summary>
        /// <returns>A List of XmlReader objects; cannot be null</returns>
        public override List<XmlReader> GetReaders(Dictionary<MetadataArtifactLoader, XmlReader> sourceDictionary)
        {
            List<XmlReader> list = new List<XmlReader>();

            foreach (MetadataArtifactLoaderResource resource in _children)
            {
                list.AddRange(resource.GetReaders(sourceDictionary));
            }

            return list;
        }

        /// <summary>
        /// Get XmlReaders for a specific DataSpace.
        /// </summary>
        /// <param name="spaceToGet">The DataSpace corresponding to the requested artifacts</param>
        /// <returns>A List of XmlReader objects</returns>
        public override List<XmlReader> CreateReaders(DataSpace spaceToGet)
        {
            List<XmlReader> list = new List<XmlReader>();

            foreach (MetadataArtifactLoaderResource resource in _children)
            {
                list.AddRange(resource.CreateReaders(spaceToGet));
            }

            return list;
        }

        /// <summary>
        /// Load all resources from the assembly/assemblies identified in the resource path.
        /// </summary>
        /// <param name="uriRegistry">The global registry of URIs</param>
        /// <param name="resolveAssembly"></param>
        /// <returns></returns>
        private static List<MetadataArtifactLoaderResource> LoadResources(string assemblyName, string resourceName, ICollection<string> uriRegistry, MetadataArtifactAssemblyResolver resolver)
        {
            Debug.Assert(resolver != null);

            List<MetadataArtifactLoaderResource> loaders = new List<MetadataArtifactLoaderResource>();
            Debug.Assert(!string.IsNullOrEmpty(assemblyName));

            if (assemblyName == MetadataArtifactLoader.wildcard)
            {
                foreach (Assembly assembly in resolver.GetWildcardAssemblies())
                {
                    if (AssemblyContainsResource(assembly, ref resourceName))
                    {
                        LoadResourcesFromAssembly(assembly, resourceName, uriRegistry, loaders, resolver);
                    }
                }

            }
            else
            {
                Assembly assembly = ResolveAssemblyName(assemblyName, resolver);
                LoadResourcesFromAssembly(assembly, resourceName, uriRegistry, loaders, resolver);
            }

            if (resourceName != null && loaders.Count == 0)
            {
                // they were asking for a specific resource name, and we didn't find it
                throw EntityUtil.Metadata(System.Data.Entity.Strings.UnableToLoadResource);
            }

            return loaders;
        }

        private static bool AssemblyContainsResource(Assembly assembly, ref string resourceName)
        {
            if (resourceName == null)
            {
                return true;
            }

            string[] allresources = GetManifestResourceNamesForAssembly(assembly);
            foreach (string current in allresources)
            {
                if (string.Equals(resourceName, current, StringComparison.OrdinalIgnoreCase))
                {
                    resourceName = current;
                    return true;
                }
            }

            return false;

        }

        private static void LoadResourcesFromAssembly(Assembly assembly, string resourceName, ICollection<string> uriRegistry, List<MetadataArtifactLoaderResource> loaders, MetadataArtifactAssemblyResolver resolver)
        {
            if (resourceName == null)
            {
                LoadAllResourcesFromAssembly(assembly, uriRegistry, loaders, resolver);
            }
            else if (AssemblyContainsResource(assembly, ref resourceName))
            {
                CreateAndAddSingleResourceLoader(assembly, resourceName, uriRegistry, loaders);
            }
            else
            {
                throw EntityUtil.Metadata(System.Data.Entity.Strings.UnableToLoadResource);
            }
        }

        private static void LoadAllResourcesFromAssembly(Assembly assembly, ICollection<string> uriRegistry, List<MetadataArtifactLoaderResource> loaders, MetadataArtifactAssemblyResolver resolver)
        {
            Debug.Assert(assembly != null);
            string[] allresources = GetManifestResourceNamesForAssembly(assembly);

            foreach (string resourceName in allresources)
            {
                CreateAndAddSingleResourceLoader(assembly, resourceName, uriRegistry, loaders);
            }
        }

        private static void CreateAndAddSingleResourceLoader(Assembly assembly, string resourceName, ICollection<string> uriRegistry, List<MetadataArtifactLoaderResource> loaders)
        {
            Debug.Assert(resourceName != null);
            Debug.Assert(assembly != null);

            string resourceUri = CreateResPath(assembly, resourceName);
            if (!uriRegistry.Contains(resourceUri))
            {
                loaders.Add(new MetadataArtifactLoaderResource(assembly, resourceName, uriRegistry));
            }
        }

        internal static string CreateResPath(Assembly assembly, string resourceName)
        {
            string resourceUri = string.Format(CultureInfo.InvariantCulture,
                                               "{0}{1}{2}{3}",
                                               resPathPrefix,
                                               assembly.FullName,
                                               resPathSeparator,
                                               resourceName);

            return resourceUri;
        }

        internal static string[] GetManifestResourceNamesForAssembly(Assembly assembly)
        {
            Debug.Assert(assembly != null);

            return !assembly.IsDynamic ? assembly.GetManifestResourceNames() : new string[0];
        }

        /// <summary>
        /// Load all resources from a specific assembly.
        /// </summary>
        /// <param name="fullName">The full name identifying the assembly to
        /// load resources from</param>
        /// <param name="uriRegistry">The global registry of URIs</param>
        /// <param name="resolveAssembly">delegate for resolve the assembly</param>
        private static Assembly ResolveAssemblyName(string assemblyName, MetadataArtifactAssemblyResolver resolver)
        {
            Debug.Assert(resolver != null);

            AssemblyName referenceName = new AssemblyName(assemblyName);
            Assembly assembly;
            if(!resolver.TryResolveAssemblyReference(referenceName, out assembly))
            {
                throw new FileNotFoundException(Strings.UnableToResolveAssembly(assemblyName));
            }

            return assembly;
        }


        internal static MetadataArtifactLoader CreateResourceLoader(string path, ExtensionCheck extensionCheck, string validExtension, ICollection<string> uriRegistry, MetadataArtifactAssemblyResolver resolver)
        {
            Debug.Assert(path != null);
            Debug.Assert(MetadataArtifactLoader.PathStartsWithResPrefix(path));

            // if the supplied path ends with a separator, or contains only one
            // segment (i.e., the name of an assembly, or the wildcard character),
            // create a composite loader that can extract resources from one or 
            // more assemblies
            //
            bool createCompositeResLoader = false;
            string assemblyName = null;
            string resourceName = null;
            ParseResourcePath(path, out assemblyName, out resourceName);
            createCompositeResLoader = (assemblyName != null) && (resourceName == null || assemblyName.Trim() == wildcard);

            ValidateExtension(extensionCheck, validExtension, resourceName);

            if (createCompositeResLoader)
            {
                return new MetadataArtifactLoaderCompositeResource(path, assemblyName, resourceName, uriRegistry, resolver);
            }

            Debug.Assert(!string.IsNullOrEmpty(resourceName), "we should not get here is the resourceName is null");
            Assembly assembly = ResolveAssemblyName(assemblyName, resolver);
            return new MetadataArtifactLoaderResource(assembly, resourceName, uriRegistry);
        }

        private static void ValidateExtension(ExtensionCheck extensionCheck, string validExtension, string resourceName)
        {
            if (resourceName == null)
            {
                return;
            }

            // the supplied path represents a single resource
            //
            switch (extensionCheck)
            {
                case ExtensionCheck.Specific:
                    MetadataArtifactLoader.CheckArtifactExtension(resourceName, validExtension);
                    break;

                case ExtensionCheck.All:
                    if (!MetadataArtifactLoader.IsValidArtifact(resourceName))
                    {
                        throw EntityUtil.Metadata(Strings.InvalidMetadataPath);
                    }
                    break;
            }
        }

        /// <summary>
        /// Splits the supplied path into the assembly portion and the resource
        /// part (if any)
        /// </summary>
        /// <param name="path">The resource path to parse</param>
        /// <returns>An array of (two) strings containing the assembly name
        /// and the resource name</returns>
        private static void ParseResourcePath(string path, out string assemblyName, out string resourceName)
        {
            // Extract the components from the path
            int prefixLength = MetadataArtifactLoader.resPathPrefix.Length;

            string[] result = path.Substring(prefixLength).Split(
                                                new string[] { 
                                                    MetadataArtifactLoader.resPathSeparator,
                                                    MetadataArtifactLoader.altPathSeparator 
                                                },
                                                StringSplitOptions.RemoveEmptyEntries
                                            );

            if (result.Length == 0 || result.Length > 2)
            {
                throw EntityUtil.Metadata(Strings.InvalidMetadataPath);
            }

            if (result.Length >= 1)
            {
                assemblyName = result[0];
            }
            else
            {
                assemblyName = null;
            }

            if (result.Length == 2)
            {
                resourceName = result[1];
            }
            else
            {
                resourceName = null;
            }

        }
    }
}
