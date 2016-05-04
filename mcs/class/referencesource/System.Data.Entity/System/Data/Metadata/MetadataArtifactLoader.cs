//---------------------------------------------------------------------
// <copyright file="MetadataArtifactLoader.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Versioning;
    using System.Xml;

    /// <summary>
    /// This is the base class for the resource metadata artifact loader; derived
    /// classes enacpsulate a single resource as well as collections of resources,
    /// along the lines of the Composite pattern.
    /// </summary>
    internal abstract class MetadataArtifactLoader
    {
        protected readonly static string resPathPrefix    = @"res://";
        protected readonly static string resPathSeparator = @"/";
        protected readonly static string altPathSeparator = @"\";
        protected readonly static string wildcard         = @"*";

        /// <summary>
        /// Read-only access to the resource/file path
        /// </summary>
        public abstract string Path{ get; }
        public abstract void CollectFilePermissionPaths(List<string> paths, DataSpace spaceToGet);

        /// <summary>
        /// This enum is used to indicate the level of extension check to be perfoemed 
        /// on a metadata URI.
        /// </summary>
        public enum ExtensionCheck
        {
            /// <summary>
            /// Do not perform any extension check
            /// </summary>
            None = 0,

            /// <summary>
            /// Check the extension against a specific value
            /// </summary>
            Specific,

            /// <summary>
            /// Check the extension against the set of acceptable extensions
            /// </summary>
            All
        }

        [ResourceExposure(ResourceScope.Machine)] //Exposes the file name which is a Machine resource
        [ResourceConsumption(ResourceScope.Machine)] //For Create method call. But the path is not created in this method.
        public static MetadataArtifactLoader Create(string path,
                                                    ExtensionCheck extensionCheck,
                                                    string validExtension,
                                                    ICollection<string> uriRegistry)
        {
            return Create(path, extensionCheck, validExtension, uriRegistry, new DefaultAssemblyResolver());
        }
        /// <summary>
        /// Factory method to create an artifact loader. This is where an appropriate
        /// subclass of MetadataArtifactLoader is created, depending on the kind of
        /// resource it will encapsulate.
        /// </summary>
        /// <param name="path">The path to the resource(s) to be loaded</param>
        /// <param name="extensionCheck">Any URI extension checks to perform</param>
        /// <param name="validExtension">A specific extension for an artifact resource</param>
        /// <param name="uriRegistry">The global registry of URIs</param>
        /// <param name="resolveAssembly"></param>
        /// <returns>A concrete instance of an artifact loader.</returns>
        [ResourceExposure(ResourceScope.Machine)] //Exposes the file name which is a Machine resource
        [ResourceConsumption(ResourceScope.Machine)] //For CheckArtifactExtension method call. But the path is not created in this method.
        internal static MetadataArtifactLoader Create(string path, 
                                                    ExtensionCheck extensionCheck,
                                                    string validExtension,
                                                    ICollection<string> uriRegistry, 
                                                    MetadataArtifactAssemblyResolver resolver)
        {
            Debug.Assert(path != null);
            Debug.Assert(resolver != null);

            // res:// -based artifacts
            //
            if (MetadataArtifactLoader.PathStartsWithResPrefix(path))
            {
                return MetadataArtifactLoaderCompositeResource.CreateResourceLoader(path, extensionCheck, validExtension, uriRegistry, resolver);
            }

            // Files and Folders
            //
            string normalizedPath = MetadataArtifactLoader.NormalizeFilePaths(path);
            if (Directory.Exists(normalizedPath))
            {
                return new MetadataArtifactLoaderCompositeFile(normalizedPath, uriRegistry);
            }
            else if (File.Exists(normalizedPath))
            {
                switch (extensionCheck)
                {
                    case ExtensionCheck.Specific:
                        CheckArtifactExtension(normalizedPath, validExtension);
                        break;

                    case ExtensionCheck.All:
                        if (!MetadataArtifactLoader.IsValidArtifact(normalizedPath))
                        {
                            throw EntityUtil.Metadata(Strings.InvalidMetadataPath);
                        } 
                        break;
                }

                return new MetadataArtifactLoaderFile(normalizedPath, uriRegistry);
            }

            throw EntityUtil.Metadata(Strings.InvalidMetadataPath);
        }

        /// <summary>
        /// Factory method to create an aggregating artifact loader, one that encapsulates
        /// multiple collections.
        /// </summary>
        /// <param name="allCollections">The list of collections to be aggregated</param>
        /// <returns>A concrete instance of an artifact loader.</returns>
        public static MetadataArtifactLoader Create(List<MetadataArtifactLoader> allCollections)
        {
            return new MetadataArtifactLoaderComposite(allCollections);
        }

        /// <summary>
        /// Helper method that wraps a list of file paths in MetadataArtifactLoader instances.
        /// </summary>
        /// <param name="filePaths">The list of file paths to wrap</param>
        /// <param name="validExtension">An acceptable extension for the file</param>
        /// <returns>An instance of MetadataArtifactLoader</returns>
        [ResourceExposure(ResourceScope.Machine)] //Exposes the file names which are a Machine resource
        [ResourceConsumption(ResourceScope.Machine)] //For CreateCompositeFromFilePaths method call. But the path is not created in this method.
        public static MetadataArtifactLoader CreateCompositeFromFilePaths(IEnumerable<string> filePaths, string validExtension)
        {
            Debug.Assert(!string.IsNullOrEmpty(validExtension));

            return CreateCompositeFromFilePaths(filePaths, validExtension, new DefaultAssemblyResolver());
        }

        [ResourceExposure(ResourceScope.Machine)] //Exposes the file names which are a Machine resource
        [ResourceConsumption(ResourceScope.Machine)] //For Create method call. But the paths are not created in this method.
        internal static MetadataArtifactLoader CreateCompositeFromFilePaths(IEnumerable<string> filePaths, string validExtension, MetadataArtifactAssemblyResolver resolver)
        {
            ExtensionCheck extensionCheck;
            if (string.IsNullOrEmpty(validExtension))
            {
                extensionCheck = ExtensionCheck.All;
            }
            else
            {
                extensionCheck = ExtensionCheck.Specific;
            }
            
            List<MetadataArtifactLoader> loaders = new List<MetadataArtifactLoader>();

            // The following set is used to remove duplicate paths from the incoming array
            HashSet<string> uriRegistry = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach(string path in filePaths)
            {
                if (string.IsNullOrEmpty(path))
                {
                    throw EntityUtil.Metadata(System.Data.Entity.Strings.NotValidInputPath,
                                              EntityUtil.CollectionParameterElementIsNullOrEmpty("filePaths"));
                }

                string trimedPath = path.Trim();
                if (trimedPath.Length > 0)
                {
                    loaders.Add(MetadataArtifactLoader.Create(
                                            trimedPath,
                                            extensionCheck,
                                            validExtension,
                                            uriRegistry,
                                            resolver)
                                        );
                }
            }

            return MetadataArtifactLoader.Create(loaders);
        }

        /// <summary>
        /// Helper method that wraps a collection of XmlReader objects in MetadataArtifactLoader
        /// instances.
        /// </summary>
        /// <param name="filePaths">The collection of XmlReader objects to wrap</param>
        /// <returns>An instance of MetadataArtifactLoader</returns>
        public static MetadataArtifactLoader CreateCompositeFromXmlReaders(IEnumerable<XmlReader> xmlReaders)
        {
            List<MetadataArtifactLoader> loaders = new List<MetadataArtifactLoader>();

            foreach (XmlReader reader in xmlReaders)
            {
                if (reader == null)
                {
                    throw EntityUtil.CollectionParameterElementIsNull("xmlReaders");
                }

                loaders.Add(new MetadataArtifactLoaderXmlReaderWrapper(reader));
            }

            return MetadataArtifactLoader.Create(loaders);
        }

        /// <summary>
        /// If the path doesn't have the right extension, throw
        /// </summary>
        /// <param name="path">The path to the resource</param>
        /// <param name="validExtension"></param>
        internal static void CheckArtifactExtension(string path, string validExtension)
        {
            Debug.Assert(!string.IsNullOrEmpty(path));
            Debug.Assert(!string.IsNullOrEmpty(validExtension));

            string extension = GetExtension(path);
            if (!extension.Equals(validExtension, StringComparison.OrdinalIgnoreCase))
            {
                throw EntityUtil.Metadata(Strings.InvalidFileExtension(path, extension, validExtension));
            }
        }

        /// <summary>
        /// Get paths to all artifacts, in the original, unexpanded form
        /// </summary>
        /// <returns>A List of strings identifying paths to all resources</returns>
        public virtual List<string> GetOriginalPaths()
        {
            return new List<string>(new string[] { Path });
        }

        /// <summary>
        /// Get paths to artifacts for a specific DataSpace, in the original, unexpanded
        /// form
        /// </summary>
        /// <param name="spaceToGet">The DataSpace for the artifacts of interest</param>
        /// <returns>A List of strings identifying paths to all artifacts for a specific DataSpace</returns>
        public virtual List<string> GetOriginalPaths(DataSpace spaceToGet)
        {
            List<string> list = new List<string>();
            if (MetadataArtifactLoader.IsArtifactOfDataSpace(Path, spaceToGet))
            {
                list.Add(Path);
            }
            return list;
        }

        public virtual bool IsComposite 
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// Get paths to all artifacts
        /// </summary>
        /// <returns>A List of strings identifying paths to all resources</returns>
        public abstract List<string> GetPaths();

        /// <summary>
        /// Get paths to artifacts for a specific DataSpace.
        /// </summary>
        /// <param name="spaceToGet">The DataSpace for the artifacts of interest</param>
        /// <returns>A List of strings identifying paths to all artifacts for a specific DataSpace</returns>
        public abstract List<string> GetPaths(DataSpace spaceToGet);

        public List<XmlReader> GetReaders()
        {
            return GetReaders(null);
        }
        /// <summary>
        /// Get XmlReaders for all resources
        /// </summary>
        /// <returns>A List of XmlReaders for all resources</returns>
        public abstract List<XmlReader> GetReaders(Dictionary<MetadataArtifactLoader, XmlReader> sourceDictionary);

        /// <summary>
        /// Get XmlReaders for a specific DataSpace.
        /// </summary>
        /// <param name="spaceToGet">The DataSpace for the artifacts of interest</param>
        /// <returns>A List of XmlReader object</returns>
        public abstract List<XmlReader> CreateReaders(DataSpace spaceToGet);

        /// <summary>
        /// Helper method to determine whether a given path to a resource
        /// starts with the "res://" prefix.
        /// </summary>
        /// <param name="path">The resource path to test.</param>
        /// <returns>true if the path represents a resource location</returns>
        internal static bool PathStartsWithResPrefix(string path)
        {
            return path.StartsWith(MetadataArtifactLoader.resPathPrefix, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Helper method to determine whether a resource identifies a C-Space
        /// artifact.
        /// </summary>
        /// <param name="resource">The resource path</param>
        /// <returns>true if the resource identifies a C-Space artifact</returns>
        protected static bool IsCSpaceArtifact(string resource)
        {
            Debug.Assert(!string.IsNullOrEmpty(resource));

            string extn = GetExtension(resource);
            if (!string.IsNullOrEmpty(extn))
            {
                return string.Compare(extn, XmlConstants.CSpaceSchemaExtension, StringComparison.OrdinalIgnoreCase) == 0;
            }
            return false;
        }

        /// <summary>
        /// Helper method to determine whether a resource identifies an S-Space
        /// artifact.
        /// </summary>
        /// <param name="resource">The resource path</param>
        /// <returns>true if the resource identifies an S-Space artifact</returns>
        protected static bool IsSSpaceArtifact(string resource)
        {
            Debug.Assert(!string.IsNullOrEmpty(resource));

            string extn = GetExtension(resource);
            if (!string.IsNullOrEmpty(extn))
            {
                return string.Compare(extn, XmlConstants.SSpaceSchemaExtension, StringComparison.OrdinalIgnoreCase) == 0;
            }
            return false;
        }

        /// <summary>
        /// Helper method to determine whether a resource identifies a CS-Space
        /// artifact.
        /// </summary>
        /// <param name="resource">The resource path</param>
        /// <returns>true if the resource identifies a CS-Space artifact</returns>
        protected static bool IsCSSpaceArtifact(string resource)
        {
            Debug.Assert(!string.IsNullOrEmpty(resource));

            string extn = GetExtension(resource);
            if (!string.IsNullOrEmpty(extn))
            {
                return string.Compare(extn, XmlConstants.CSSpaceSchemaExtension, StringComparison.OrdinalIgnoreCase) == 0;
            }
            return false;
        }

        // don't use Path.GetExtension because it is ok for the resource
        // name to have characters in it that would be illegal in a path (ie '<' is illegal in a path)
        // and when they do, Path.GetExtension throws and ArgumentException
        private static string GetExtension(string resource)
        {
            if(String.IsNullOrEmpty(resource))
                return string.Empty;

            int pos = resource.LastIndexOf('.');
            if (pos < 0)
                return string.Empty;

            return resource.Substring(pos);
        }


        /// <summary>
        /// Helper method to determine whether a resource identifies a valid artifact.
        /// </summary>
        /// <param name="resource">The resource path</param>
        /// <returns>true if the resource identifies a valid artifact</returns>
        internal static bool IsValidArtifact(string resource)
        {
            Debug.Assert(!string.IsNullOrEmpty(resource));

            string extn = GetExtension(resource);
            if (!string.IsNullOrEmpty(extn))
            {
                return (
                    string.Compare(extn, XmlConstants.CSpaceSchemaExtension, StringComparison.OrdinalIgnoreCase) == 0 ||
                    string.Compare(extn, XmlConstants.SSpaceSchemaExtension, StringComparison.OrdinalIgnoreCase) == 0 ||
                    string.Compare(extn, XmlConstants.CSSpaceSchemaExtension, StringComparison.OrdinalIgnoreCase) == 0
                );
            }
            return false;
        }

        /// <summary>
        /// This helper method accepts a resource URI and a value from the DataSpace enum
        /// and determines whether the resource identifies an artifact of that DataSpace.
        /// </summary>
        /// <param name="resource">A URI to an artifact resource</param>
        /// <param name="dataSpace">A DataSpace enum value</param>
        /// <returns>true if the resource identifies an artifact of the specified DataSpace</returns>
        protected static bool IsArtifactOfDataSpace(string resource, DataSpace dataSpace)
        {
            if (dataSpace == DataSpace.CSpace)
                return MetadataArtifactLoader.IsCSpaceArtifact(resource);

            if (dataSpace == DataSpace.SSpace)
                return MetadataArtifactLoader.IsSSpaceArtifact(resource);

            if (dataSpace == DataSpace.CSSpace)
                return MetadataArtifactLoader.IsCSSpaceArtifact(resource);

            Debug.Assert(false, "Invalid DataSpace specified.");
            return false;
        }

        /// <summary>
        /// Normalize a file path:
        ///     1. Add backslashes if given a drive letter.
        ///     2. Resolve the '~' macro in a Web/ASP.NET environment.
        ///     3. Expand the |DataDirectory| macro, if found in the argument.
        ///     4. Convert relative paths into absolute paths.
        /// </summary>
        /// <param name="path">the path to normalize</param>
        /// <returns>The normalized file path</returns>
        [ResourceExposure(ResourceScope.Machine)] //Exposes the file name which is a Machine resource
        [ResourceConsumption(ResourceScope.Machine)] //For Path.GetFullPath method call. But the path is not created in this method.
        static internal string NormalizeFilePaths(string path)
        {
            bool getFullPath = true;    // used to determine whether we need to invoke GetFullPath()

            if (!String.IsNullOrEmpty(path))
            {
                path = path.Trim();

                // If the path starts with a '~' character, try to resolve it as a Web/ASP.NET
                // application path.
                //
                if (path.StartsWith(EdmConstants.WebHomeSymbol, StringComparison.Ordinal))
                {
                    AspProxy aspProxy = new AspProxy();
                    path = aspProxy.MapWebPath(path);
                    getFullPath = false;
                }

                if (path.Length == 2 && path[1] == System.IO.Path.VolumeSeparatorChar)
                {
                    path = path + System.IO.Path.DirectorySeparatorChar;
                }
                else
                {
                    // See if the path contains the |DataDirectory| macro that we need to
                    // expand. Note that ExpandDataDirectory() won't process the path unless
                    // it begins with the macro.
                    //
                    string fullPath = System.Data.EntityClient.DbConnectionOptions.ExpandDataDirectory(
                            System.Data.EntityClient.EntityConnectionStringBuilder.MetadataParameterName,   // keyword ("Metadata")
                            path                                                                            // value
                        );

                    // ExpandDataDirectory() returns null if it doesn't find the macro in its
                    // argument.
                    //
                    if (fullPath != null)
                    {
                        path = fullPath;
                        getFullPath = false;
                    }
                }
            }
            try
            {
                if (getFullPath)
                {
                    path = System.IO.Path.GetFullPath(path);
                }
            }
            catch (ArgumentException e)
            {
                throw EntityUtil.Metadata(System.Data.Entity.Strings.NotValidInputPath, e);
            }
            catch (NotSupportedException e)
            {
                throw EntityUtil.Metadata(System.Data.Entity.Strings.NotValidInputPath, e);
            }
            catch (PathTooLongException)
            {
                throw EntityUtil.Metadata(System.Data.Entity.Strings.NotValidInputPath);
            }

            return path;
        }


    }
}
