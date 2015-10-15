//---------------------------------------------------------------------
// <copyright file="MetadataArtifactLoaderXmlReaderWrapper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Security.Permissions;


namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// This class represents a wrapper around an XmlReader to be used to load metadata.
    /// Note that the XmlReader object isn't created here -- the wrapper simply stores
    /// a reference to it -- therefore we do not Close() the reader when we Dispose()
    /// the wrapper, i.e., Dispose() is a no-op.
    /// </summary>
    internal class MetadataArtifactLoaderXmlReaderWrapper : MetadataArtifactLoader, IComparable
    {
        private readonly XmlReader _reader = null;
        private readonly string _resourceUri = null;

        /// <summary>
        /// Constructor - saves off the XmlReader in a private data field
        /// </summary>
        /// <param name="xmlReader">The path to the resource to load</param>
        public MetadataArtifactLoaderXmlReaderWrapper(XmlReader xmlReader)
        {
            _reader = xmlReader;
            _resourceUri = xmlReader.BaseURI;

        }

        public override string Path
        {
            get
            {
                if (string.IsNullOrEmpty(this._resourceUri))
                {
                    return string.Empty;
                }
                else
                {
                    return this._resourceUri;
                }
            }
        }

        /// <summary>
        /// Implementation of IComparable.CompareTo()
        /// </summary>
        /// <param name="obj">The object to compare to</param>
        /// <returns>0 if the loaders are "equal" (i.e., have the same _path value)</returns>
        public int CompareTo(object obj)
        {
            MetadataArtifactLoaderXmlReaderWrapper loader = obj as MetadataArtifactLoaderXmlReaderWrapper;
            if (loader != null)
            {
                if (Object.ReferenceEquals(this._reader, loader._reader))
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }

            Debug.Assert(false, "object is not a MetadataArtifactLoaderXmlReaderWrapper");
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
            return _reader.GetHashCode();
        }

        public override void CollectFilePermissionPaths(List<string> paths, DataSpace spaceToGet)
        {
            // no op
        }

        /// <summary>
        /// Get paths to artifacts for a specific DataSpace.
        /// </summary>
        /// <param name="spaceToGet">The DataSpace for the artifacts of interest</param>
        /// <returns>A List of strings identifying paths to all artifacts for a specific DataSpace</returns>
        public override List<string> GetPaths(DataSpace spaceToGet)
        {
            List<string> list = new List<string>();
            if (MetadataArtifactLoader.IsArtifactOfDataSpace(Path, spaceToGet))
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
            return new List<string>(new string[] { Path });
        }

        /// <summary>
        /// Get XmlReaders for all resources
        /// </summary>
        /// <returns>A List of XmlReaders for all resources</returns>
        public override List<XmlReader> GetReaders(Dictionary<MetadataArtifactLoader, XmlReader> sourceDictionary)
        {
            List<XmlReader> list = new List<XmlReader>();

            list.Add(this._reader);
            if (sourceDictionary != null)
            {
                sourceDictionary.Add(this, _reader);
            }

            return list;
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

            if (MetadataArtifactLoader.IsArtifactOfDataSpace(Path, spaceToGet))
            {
                list.Add(_reader);
            }

            return list;
        }
    }
}
