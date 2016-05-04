#region Copyright (c) Microsoft Corporation
/// <copyright company='Microsoft Corporation'>
///    Copyright (c) Microsoft Corporation. All Rights Reserved.
///    Information Contained Herein is Proprietary and Confidential.
/// </copyright>
#endregion

using System.Diagnostics;
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
    internal class SvcMapFileLoader : MapFileLoader
#else
    public class SvcMapFileLoader : MapFileLoader
#endif
    {
        private string _mapFilePath;
        private XmlSchemaSet _mapFileSchemaSet;
        private XmlSerializer _mapFileSerializer;

        public SvcMapFileLoader(string mapFilePath)
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
            return mapFileImpl is SvcMapFileImpl ? new SvcMapFile((SvcMapFileImpl)mapFileImpl) : null;
        }

        protected override object Unwrap(MapFile mapFile)
        {
            return mapFile is SvcMapFile ? ((SvcMapFile)mapFile).Impl : null;
        }

        protected override XmlSchemaSet GetMapFileSchemaSet()
        {
            if (_mapFileSchemaSet == null)
            {
                _mapFileSchemaSet = new XmlSchemaSet();

                using (var stream = typeof(SvcMapFileImpl).Assembly.GetManifestResourceStream(typeof(SvcMapFileImpl), @"Schema.ServiceMapSchema.xsd"))
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
                _mapFileSerializer = new System.Web.Compilation.WCFModel.SvcMapFileXmlSerializer.SvcMapFileImplSerializer();
#else
                _mapFileSerializer = new XmlSerializer(typeof(SvcMapFileImpl), SvcMapFileImpl.NamespaceUri);
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
