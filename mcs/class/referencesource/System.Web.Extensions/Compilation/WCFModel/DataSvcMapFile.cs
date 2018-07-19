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
    internal class DataSvcMapFile : MapFile
#else
    [CLSCompliant(true)]
    public class DataSvcMapFile : MapFile
#endif
    {
        private DataSvcMapFileImpl _impl;

        public DataSvcMapFileImpl Impl
        {
            get
            {
                return _impl;
            }
        }

        public DataSvcMapFile()
        {
            _impl = new DataSvcMapFileImpl();
        }

        public DataSvcMapFile(DataSvcMapFileImpl impl)
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

        public List<Parameter> Parameters
        {
            get
            {
                return _impl.Parameters;
            }
        }
    }

    [XmlRoot(Namespace = DataSvcMapFileImpl.NamespaceUri, ElementName = "ReferenceGroup")]
#if WEB_EXTENSIONS_CODE
    internal class DataSvcMapFileImpl
#else
    [CLSCompliant(true)]
    public class DataSvcMapFileImpl
#endif
    {
        public const string NamespaceUri = "urn:schemas-microsoft-com:xml-dataservicemap";

        private string _id;
        private List<MetadataSource> _metadataSourceList;
        private List<MetadataFile> _metadataList;
        private List<ExtensionFile> _extensionFileList;
        private List<Parameter> _parameters;

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

        [XmlArray(ElementName = "MetadataSources", Order = 0)]
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

        [XmlArray(ElementName = "Metadata", Order = 1)]
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

        [XmlArray(ElementName = "Extensions", Order = 2)]
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

        [XmlArray(ElementName = "Parameters", Order = 3)]
        [XmlArrayItem("Parameter", typeof(Parameter))]
        public List<Parameter> Parameters
        {
            get
            {
                if (_parameters == null)
                {
                    _parameters = new List<Parameter>();
                }
                return _parameters;
            }
        }
    }
}
