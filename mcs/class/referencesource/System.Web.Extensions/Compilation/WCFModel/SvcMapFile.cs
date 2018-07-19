#region Copyright (c) Microsoft Corporation
/// <copyright company='Microsoft Corporation'>
///    Copyright (c) Microsoft Corporation. All Rights Reserved.
///    Information Contained Herein is Proprietary and Confidential.
/// </copyright>
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

#if WEB_EXTENSIONS_CODE
namespace System.Web.Compilation.WCFModel
#else
namespace Microsoft.VSDesigner.WCFModel
#endif
{
#if WEB_EXTENSIONS_CODE
    internal class SvcMapFile : MapFile
#else
    [CLSCompliant(true)]
    public class SvcMapFile : MapFile
#endif
    {
        private SvcMapFileImpl _impl;

        public SvcMapFileImpl Impl
        {
            get
            {
                return _impl;
            }
        }

        public SvcMapFile()
        {
            _impl = new SvcMapFileImpl();
        }

        public SvcMapFile(SvcMapFileImpl impl)
        {
            Debug.Assert(impl != null, "impl is null!");

            _impl = impl;
        }

        public override string ID
        {
            get
            {
                return _impl.ID;
            }
            set
            {
                _impl.ID = value;
            }
        }

        public override List<MetadataSource> MetadataSourceList
        {
            get
            {
                return _impl.MetadataSourceList;
            }
        }

        public override List<MetadataFile> MetadataList
        {
            get
            {
                return _impl.MetadataList;
            }
        }

        public override List<ExtensionFile> Extensions
        {
            get
            {
                return _impl.Extensions;
            }
        }

        public ClientOptions ClientOptions
        {
            get
            {
                return _impl.ClientOptions;
            }
        }
    }

    [XmlRoot(Namespace = SvcMapFileImpl.NamespaceUri, ElementName = "ReferenceGroup")]
#if WEB_EXTENSIONS_CODE
    internal class SvcMapFileImpl
#else
    [CLSCompliant(true)]
    public class SvcMapFileImpl
#endif
    {
        public const string NamespaceUri = "urn:schemas-microsoft-com:xml-wcfservicemap";

        private string _id;
        private ClientOptions _clientOptions;
        private List<MetadataSource> _metadataSourceList;
        private List<MetadataFile> _metadataList;
        private List<ExtensionFile> _extensionFileList;

        [XmlAttribute]
        public string ID
        {
            get
            {
                if (_id == null)
                {
                    _id = Guid.NewGuid().ToString();
                }
                return _id;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _id = value;
            }
        }

        [XmlElement(ElementName = "ClientOptions", Order = 0)]
        public ClientOptions ClientOptions
        {
            get
            {
                if (_clientOptions == null)
                {
                    _clientOptions = new ClientOptions();
                }
                return _clientOptions;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _clientOptions = value;
            }
        }

        [XmlArray(ElementName = "MetadataSources", Order = 1)]
        [XmlArrayItem("MetadataSource", typeof(MetadataSource))]
        public List<MetadataSource> MetadataSourceList
        {
            get
            {
                if (_metadataSourceList == null)
                {
                    _metadataSourceList = new List<MetadataSource>();
                }
                return _metadataSourceList;
            }
        }

        [XmlArray(ElementName = "Metadata", Order = 2)]
        [XmlArrayItem("MetadataFile", typeof(MetadataFile))]
        public List<MetadataFile> MetadataList
        {
            get
            {
                if (_metadataList == null)
                {
                    _metadataList = new List<MetadataFile>();
                }
                return _metadataList;
            }
        }

        [XmlArray(ElementName = "Extensions", Order = 3)]
        [XmlArrayItem("ExtensionFile", typeof(ExtensionFile))]
        public List<ExtensionFile> Extensions
        {
            get
            {
                if (_extensionFileList == null)
                {
                    _extensionFileList = new List<ExtensionFile>();
                }
                return _extensionFileList;
            }
        }
    }
}
