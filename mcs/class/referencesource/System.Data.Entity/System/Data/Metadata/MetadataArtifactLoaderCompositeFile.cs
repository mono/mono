//---------------------------------------------------------------------
// <copyright file="MetadataArtifactLoaderCompositeFile.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Data.Mapping;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Collections.ObjectModel;
using System.Runtime.Versioning;


namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// This class represents a collection of artifact files to be loaded from one
    /// filesystem folder.
    /// </summary>
    internal class MetadataArtifactLoaderCompositeFile : MetadataArtifactLoader
    {
        private ReadOnlyCollection<MetadataArtifactLoaderFile> _csdlChildren;
        private ReadOnlyCollection<MetadataArtifactLoaderFile> _ssdlChildren;
        private ReadOnlyCollection<MetadataArtifactLoaderFile> _mslChildren;


        private readonly string _path;
        private readonly ICollection<string> _uriRegistry;

        /// <summary>
        /// Constructor - loads all resources into the _children collection
        /// </summary>
        /// <param name="path">The path to the (collection of) resources</param>
        /// <param name="uriRegistry">The global registry of URIs</param>
        [ResourceExposure(ResourceScope.Machine)] //Exposes the file path which is a Machine resource
        public MetadataArtifactLoaderCompositeFile(string path, ICollection<string> uriRegistry)
        {
            _path = path;
            _uriRegistry = uriRegistry;
        }

        public override string Path
        {
            get { return _path; }
        }

        [ResourceExposure(ResourceScope.Machine)] //Exposes the file paths which are a Machine resource
        public override void CollectFilePermissionPaths(List<string> paths, DataSpace spaceToGet)
        {
           IList<MetadataArtifactLoaderFile> files;
            if(TryGetListForSpace(spaceToGet, out files))
            {
                foreach(var loader in files)
                {
                    loader.CollectFilePermissionPaths(paths, spaceToGet);
                }
            }
        }

        public override bool IsComposite
        {
            get
            {
                return true;
            }
        }

        internal ReadOnlyCollection<MetadataArtifactLoaderFile> CsdlChildren
        {
            get 
            {
                LoadCollections();
                return _csdlChildren;
            }
        }
        internal ReadOnlyCollection<MetadataArtifactLoaderFile> SsdlChildren
        {
            get 
            {
                LoadCollections();
                return _ssdlChildren;
            }
        }
        internal ReadOnlyCollection<MetadataArtifactLoaderFile> MslChildren
        {
            get 
            {
                LoadCollections();
                return _mslChildren;
            }
        }

        /// <summary>
        /// Load all the collections at once so we have a "fairly" matched in time set of files
        /// otherwise we may end up loading the csdl files, and then not loading the ssdl, and msl 
        /// files for sometime later.
        /// </summary>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)] //For GetArtifactsInDirectory method call. We pick the paths from class variable. 
                                                                            //so this method does not expose any resource.
        void LoadCollections()
        {
            if (_csdlChildren == null)
            {
                ReadOnlyCollection<MetadataArtifactLoaderFile> csdlChildren = GetArtifactsInDirectory(_path, XmlConstants.CSpaceSchemaExtension, _uriRegistry).AsReadOnly();
                Interlocked.CompareExchange(ref _csdlChildren, csdlChildren, null);
            }
            if (_ssdlChildren == null)
            {
                ReadOnlyCollection<MetadataArtifactLoaderFile> ssdlChildren = GetArtifactsInDirectory(_path, XmlConstants.SSpaceSchemaExtension, _uriRegistry).AsReadOnly();
                Interlocked.CompareExchange(ref _ssdlChildren, ssdlChildren, null);
            }
            if (_mslChildren == null)
            {
                ReadOnlyCollection<MetadataArtifactLoaderFile> mslChildren = GetArtifactsInDirectory(_path, XmlConstants.CSSpaceSchemaExtension, _uriRegistry).AsReadOnly();
                Interlocked.CompareExchange(ref _mslChildren, mslChildren, null);
            }
        }

        /// <summary>
        /// Get paths to artifacts for a specific DataSpace, in the original, unexpanded 
        /// form.
        /// </summary>
        /// <remarks>A filesystem folder can contain any kind of artifact, so we simply
        /// ignore the parameter and return the original path to the folder.</remarks>
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
            IList<MetadataArtifactLoaderFile> files;

            if (!TryGetListForSpace(spaceToGet, out files))
            {
                return list;
            }

            foreach (MetadataArtifactLoaderFile file in files)
            {
                list.AddRange(file.GetPaths(spaceToGet));
            }

            return list;
        }

        private bool TryGetListForSpace(DataSpace spaceToGet, out IList<MetadataArtifactLoaderFile> files)
        {
            switch (spaceToGet)
            {
                case DataSpace.CSpace:
                    files = CsdlChildren;
                    return true;
                case DataSpace.SSpace:
                    files = SsdlChildren;
                    return true;
                case DataSpace.CSSpace:
                    files = MslChildren;
                    return true;
                default:
                    Debug.Assert(false, "Invalid DataSpace value.");
                    files = null;
                    return false;
            }
        }

        /// <summary>
        /// Get paths to all artifacts
        /// </summary>
        /// <returns>A List of strings identifying paths to all resources</returns>
        public override List<string> GetPaths()
        {
            List<string> list = new List<string>();

            foreach (MetadataArtifactLoaderFile resource in CsdlChildren)
            {
                list.AddRange(resource.GetPaths());
            }
            foreach (MetadataArtifactLoaderFile resource in SsdlChildren)
            {
                list.AddRange(resource.GetPaths());
            }
            foreach (MetadataArtifactLoaderFile resource in MslChildren)
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

            foreach (MetadataArtifactLoaderFile resource in CsdlChildren)
            {
                list.AddRange(resource.GetReaders(sourceDictionary));
            }
            foreach (MetadataArtifactLoaderFile resource in SsdlChildren)
            {
                list.AddRange(resource.GetReaders(sourceDictionary));
            }
            foreach (MetadataArtifactLoaderFile resource in MslChildren)
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
            IList<MetadataArtifactLoaderFile> files;

            if (!TryGetListForSpace(spaceToGet, out files))
            {
                return list;
            }

            foreach (MetadataArtifactLoaderFile file in files)
            {
                list.AddRange(file.CreateReaders(spaceToGet));
            }

            return list;
        }

        [ResourceExposure(ResourceScope.Machine)] //Exposes the directory name which is a Machine resource
        [ResourceConsumption(ResourceScope.Machine)] //For Directory.GetFiles method call but we do not create the directory name in this method 
        private static List<MetadataArtifactLoaderFile> GetArtifactsInDirectory(string directory, string extension, ICollection<string> uriRegistry)
        {
            List<MetadataArtifactLoaderFile> loaders = new List<MetadataArtifactLoaderFile>();

            string[] fileNames = Directory.GetFiles(
                                            directory,
                                            MetadataArtifactLoader.wildcard + extension,
                                            SearchOption.TopDirectoryOnly
                                        );


            foreach (string fileName in fileNames)
            {
                string fullPath = System.IO.Path.Combine(directory, fileName);

                if (uriRegistry.Contains(fullPath))
                    continue;

                // We need a second filter on the file names verifying the right extension because
                // a file name with an extension longer than 3 characters might still match the 
                // given extension. For example, if we look for *.msl, abc.msl_something would match 
                // because the 8.3 name format matches it.
                if (fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    loaders.Add(new MetadataArtifactLoaderFile(fullPath, uriRegistry));
                    // the file is added to the registry in the MetadataArtifactLoaderFile ctor
                }
            }

            return loaders;
        }
    }
}
