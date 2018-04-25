#region Copyright (c) Microsoft Corporation
/// <copyright company='Microsoft Corporation'>
///    Copyright (c) Microsoft Corporation. All Rights Reserved.
///    Information Contained Herein is Proprietary and Confidential.
/// </copyright>
#endregion

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml.Schema;
using System.Xml.Serialization;

#if WEB_EXTENSIONS_CODE
namespace System.Web.Compilation.WCFModel
#else
namespace Microsoft.VSDesigner.WCFModel
#endif
{
#if WEB_EXTENSIONS_CODE
    internal class DataSvcMapFileLoader : MapFileLoader
#else
    public class DataSvcMapFileLoader : MapFileLoader
#endif
    {
        private string _mapFilePath;
        private XmlSchemaSet _mapFileSchemaSet;
        private XmlSerializer _mapFileSerializer;

        public DataSvcMapFileLoader(string mapFilePath)
        {
            Debug.Assert(!string.IsNullOrEmpty(mapFilePath), "mapFilePath is null!");

            _mapFilePath = mapFilePath;
        }

        #region protected overrides methods

        protected override string MapFileName
        {
            get { return _mapFilePath; }
        }

        protected override MapFile Wrap(object mapFileImpl)
        {
            return mapFileImpl is DataSvcMapFileImpl ? new DataSvcMapFile((DataSvcMapFileImpl)mapFileImpl) : null;
        }

        protected override object Unwrap(MapFile mapFile)
        {
            return mapFile is DataSvcMapFile ? ((DataSvcMapFile)mapFile).Impl : null;
        }

        [SuppressMessage("Microsoft.Security.Xml", "CA3060:UseXmlReaderForSchemaRead", Justification = "asp.net controls this .xsd file")]
        protected override XmlSchemaSet GetMapFileSchemaSet()
        {
            if (_mapFileSchemaSet == null)
            {
                _mapFileSchemaSet = new XmlSchemaSet();

                using (var stream = typeof(DataSvcMapFileImpl).Assembly.GetManifestResourceStream(typeof(DataSvcMapFileImpl), @"Schema.DataServiceMapSchema.xsd"))
                {
                    _mapFileSchemaSet.Add(XmlSchema.Read(stream, null));
                }
            }

            return _mapFileSchemaSet;
        }

        protected override XmlSerializer GetMapFileSerializer()
        {
            if (_mapFileSerializer == null)
            {
#if WEB_EXTENSIONS_CODE
                _mapFileSerializer = new System.Web.Compilation.WCFModel.DataSvcMapFileXmlSerializer.DataSvcMapFileImplSerializer();
#else
                _mapFileSerializer = new XmlSerializer(typeof(DataSvcMapFileImpl), DataSvcMapFileImpl.NamespaceUri);
#endif
            }

            return _mapFileSerializer;
        }

        protected override TextReader GetMapFileReader()
        {
            return File.OpenText(_mapFilePath);
        }

        protected override byte[] ReadMetadataFile(string name)
        {
            return File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(_mapFilePath), name));
        }

        protected override byte[] ReadExtensionFile(string name)
        {
            return File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(_mapFilePath), name));
        }

        #endregion protected overrides methods
    }
}
