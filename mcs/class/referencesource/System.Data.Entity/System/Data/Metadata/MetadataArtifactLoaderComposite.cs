//---------------------------------------------------------------------
// <copyright file="MetadataArtifactLoaderComposite.cs" company="Microsoft">
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
using System.Collections.ObjectModel;


namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// This class represents a super-collection (a collection of collections) 
    /// of artifact resources. Typically, this "meta-collection" would contain
    /// artifacts represented as individual files, directories (which are in
    /// turn collections of files), and embedded resources.
    /// </summary>
    /// <remarks>This is the root class for access to all loader objects.</remarks>
    internal class MetadataArtifactLoaderComposite : MetadataArtifactLoader, IEnumerable<MetadataArtifactLoader>
    {
        /// <summary>
        /// The list of loaders aggregated by the composite.
        /// </summary>
        private readonly ReadOnlyCollection<MetadataArtifactLoader> _children;

        /// <summary>
        /// Constructor - loads all resources into the _children collection
        /// </summary>
        /// <param name="children">A list of collections to aggregate</param>
        public MetadataArtifactLoaderComposite(List<MetadataArtifactLoader> children)
        {
            Debug.Assert(children != null);
            _children = new List<MetadataArtifactLoader>(children).AsReadOnly();
        }

        public override string Path
        {
            get { return string.Empty; }
        }

        public override void CollectFilePermissionPaths(List<string> paths, DataSpace spaceToGet)
        {
            foreach (MetadataArtifactLoader loader in _children)
            {
                loader.CollectFilePermissionPaths(paths, spaceToGet);
            }
        }

        public override bool IsComposite
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Get the list of paths to all artifacts in the original, unexpanded form
        /// </summary>
        /// <returns>A List of strings identifying paths to all resources</returns>
        public override List<string> GetOriginalPaths()
        {
            List<string> list = new List<string>();

            foreach (MetadataArtifactLoader loader in _children)
            {
                list.AddRange(loader.GetOriginalPaths());
            }

            return list;
        }

        /// <summary>
        /// Get paths to artifacts for a specific DataSpace, in the original, unexpanded 
        /// form
        /// </summary>
        /// <param name="spaceToGet">The DataSpace for the artifacts of interest</param>
        /// <returns>A List of strings identifying paths to all artifacts for a specific DataSpace</returns>
        public override List<string> GetOriginalPaths(DataSpace spaceToGet)
        {
            List<string> list = new List<string>();

            foreach (MetadataArtifactLoader loader in _children)
            {
                list.AddRange(loader.GetOriginalPaths(spaceToGet));
            }

            return list;
        }

        /// <summary>
        /// Get paths to artifacts for a specific DataSpace.
        /// </summary>
        /// <param name="spaceToGet">The DataSpace for the artifacts of interest</param>
        /// <returns>A List of strings identifying paths to all artifacts for a specific DataSpace</returns>
        public override List<string> GetPaths(DataSpace spaceToGet)
        {
            List<string> list = new List<string>();

            foreach (MetadataArtifactLoader loader in _children)
            {
                list.AddRange(loader.GetPaths(spaceToGet));
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

            foreach (MetadataArtifactLoader resource in _children)
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

            foreach (MetadataArtifactLoader resource in _children)
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

            foreach (MetadataArtifactLoader resource in _children)
            {
                list.AddRange(resource.CreateReaders(spaceToGet));
            }

            return list;
        }

        #region IEnumerable<MetadataArtifactLoader> Members

        public IEnumerator<MetadataArtifactLoader> GetEnumerator()
        {
            return this._children.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._children.GetEnumerator();
        }

        #endregion
    }
}
