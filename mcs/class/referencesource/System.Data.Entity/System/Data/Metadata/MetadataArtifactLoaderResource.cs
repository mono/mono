//---------------------------------------------------------------------
// <copyright file="MetadataArtifactLoaderResource.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.EntityModel.SchemaObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Xml;

    /// <summary>
    /// This class represents one resource item to be loaded from an assembly.
    /// </summary>
    internal class MetadataArtifactLoaderResource : MetadataArtifactLoader, IComparable
    {
        private readonly bool _alreadyLoaded = false;
        private readonly Assembly _assembly;
        private readonly string _resourceName;

        /// <summary>
        /// Constructor - loads the resource stream
        /// </summary>
        /// <param name="path">The path to the resource to load</param>
        /// <param name="uriRegistry">The global registry of URIs</param>
        internal MetadataArtifactLoaderResource(Assembly assembly, string resourceName, ICollection<string> uriRegistry)
        {
            Debug.Assert(assembly != null);
            Debug.Assert(resourceName != null);

            _assembly = assembly;
            _resourceName = resourceName;

            string tempPath = MetadataArtifactLoaderCompositeResource.CreateResPath(_assembly, _resourceName);
            _alreadyLoaded = uriRegistry.Contains(tempPath);
            if (!_alreadyLoaded)
            {
                uriRegistry.Add(tempPath);

                // '_alreadyLoaded' is not set because while we would like to prevent
                // other instances of MetadataArtifactLoaderFile that wrap the same
                // _path from being added to the list of paths/readers, we do want to
                // include this particular instance.
            }
        }

        public override string Path
        {
            get 
            {
                return MetadataArtifactLoaderCompositeResource.CreateResPath(_assembly, _resourceName);
            }
        }

        /// <summary>
        /// Implementation of IComparable.CompareTo()
        /// </summary>
        /// <param name="obj">The object to compare to</param>
        /// <returns>0 if the loaders are "equal" (i.e., have the same _path value)</returns>
        public int CompareTo(object obj)
        {
            MetadataArtifactLoaderResource loader = obj as MetadataArtifactLoaderResource;
            if (loader != null)
            {
                return string.Compare(Path, loader.Path, StringComparison.OrdinalIgnoreCase);
            }

            Debug.Assert(false, "object is not a MetadataArtifactLoaderResource");
            return -1;
        }

        /// <summary>
        /// Equals() returns true if the objects have the same _path value
        /// </summary>
        /// <param name="obj">The object to compare to</param>
        /// <returns>true if the objects have the same _path value</returns>
        public override bool Equals(object obj)
        {
            return this.CompareTo(obj) == 0;
        }

        /// <summary>
        /// GetHashCode override that defers the result to the _path member variable.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public override void CollectFilePermissionPaths(List<string> paths, DataSpace spaceToGet)
        {
            // does not apply
        }

        /// <summary>
        /// Get paths to artifacts for a specific DataSpace.
        /// </summary>
        /// <param name="spaceToGet">The DataSpace for the artifacts of interest</param>
        /// <returns>A List of strings identifying paths to all artifacts for a specific DataSpace</returns>
        public override List<string> GetPaths(DataSpace spaceToGet)
        {
            List<string> list = new List<string>();
            if (!_alreadyLoaded && MetadataArtifactLoader.IsArtifactOfDataSpace(Path, spaceToGet))
            {
                list.Add(Path);
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
            if (!_alreadyLoaded)
            {
                list.Add(Path);
            }
            return list;
        }

        /// <summary>
        /// Create and return an XmlReader around the resource represented by this instance.
        /// </summary>
        /// <returns>A List of XmlReaders for all resources</returns>
        public override List<XmlReader> GetReaders(Dictionary<MetadataArtifactLoader, XmlReader> sourceDictionary)
        {
            List<XmlReader> list = new List<XmlReader>();
            if (!_alreadyLoaded)
            {

                XmlReader reader = CreateReader();
                list.Add(reader);

                if (sourceDictionary != null)
                {
                    sourceDictionary.Add(this, reader);
                }
            }
            return list;
        }

        private XmlReader CreateReader()
        {

            Stream stream = LoadResource();

            XmlReaderSettings readerSettings = Schema.CreateEdmStandardXmlReaderSettings();
            // close the stream when the xmlreader is closed
            // now the reader owns the stream
            readerSettings.CloseInput = true;

            // we know that we aren't reading a fragment
            readerSettings.ConformanceLevel = ConformanceLevel.Document;
            XmlReader reader = XmlReader.Create(stream, readerSettings);
            // cannot set the base URI because res:// URIs cause the schema parser
            // to choke

            return reader;
        }

        /// <summary>
        /// Create and return an XmlReader around the resource represented by this instance
        /// if it is of the requested DataSpace type.
        /// </summary>
        /// <param name="spaceToGet">The DataSpace corresponding to the requested artifacts</param>
        /// <returns>A List of XmlReader objects</returns>
        public override List<XmlReader> CreateReaders(DataSpace spaceToGet)
        {
            List<XmlReader> list = new List<XmlReader>();
            if (!_alreadyLoaded)
            {
                if (MetadataArtifactLoader.IsArtifactOfDataSpace(Path, spaceToGet))
                {
                    XmlReader reader = CreateReader();
                    list.Add(reader);
                }
            }
            return list;
        }

        /// <summary>
        /// This method parses the path to the resource and attempts to load it.
        /// The method also accounts for the wildcard assembly name.
        /// </summary>
        private Stream LoadResource()
        {
            Stream resourceStream;
            if (TryCreateResourceStream(out resourceStream))
            {
                return resourceStream;
            }
            throw EntityUtil.Metadata(System.Data.Entity.Strings.UnableToLoadResource);
        }

        private bool TryCreateResourceStream(out Stream resourceStream)
        {
            resourceStream = _assembly.GetManifestResourceStream(_resourceName);
            return resourceStream != null;
        }
    }
}
