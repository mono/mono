//---------------------------------------------------------------------
// <copyright file="EntityDesignerUtils.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.IO;
using System.Text;
using System.Xml;
using System.Data.Metadata.Edm;
using System.Data.Mapping;

namespace System.Data.Entity.Design
{
    internal static class EntityDesignerUtils
    {
        internal const string EdmxRootElementName = "Edmx";
        internal const string EdmxNamespaceUriV1 = "http://schemas.microsoft.com/ado/2007/06/edmx";
        internal const string EdmxNamespaceUriV2 = "http://schemas.microsoft.com/ado/2008/10/edmx";
        internal const string EdmxNamespaceUriV3 = "http://schemas.microsoft.com/ado/2009/11/edmx";

        private static readonly EFNamespaceSet v1Namespaces = new EFNamespaceSet
                                        { 
                                            Edmx = EdmxNamespaceUriV1,
                                            Csdl = XmlConstants.ModelNamespace_1,
                                            Msl =  StorageMslConstructs.NamespaceUriV1,
                                            Ssdl = XmlConstants.TargetNamespace_1,
                                        };
        private static readonly EFNamespaceSet v2Namespaces = new EFNamespaceSet
                                        { 
                                            Edmx = EdmxNamespaceUriV2,
                                            Csdl = XmlConstants.ModelNamespace_2,
                                            Msl =  StorageMslConstructs.NamespaceUriV2,
                                            Ssdl = XmlConstants.TargetNamespace_2,
                                        };
        private static readonly EFNamespaceSet v3Namespaces = new EFNamespaceSet
                                        {
                                            Edmx = EdmxNamespaceUriV3,
                                            Csdl = XmlConstants.ModelNamespace_3,
                                            Msl = StorageMslConstructs.NamespaceUriV3,
                                            Ssdl = XmlConstants.TargetNamespace_3,
                                        };
        internal static readonly string _edmxFileExtension = ".edmx";

        /// <summary>
        /// Extract the Conceptual, Mapping and Storage nodes from an EDMX input streams, and extract the value of the metadataArtifactProcessing property.
        /// </summary>
        /// <param name="edmxInputStream"></param>
        /// <param name="conceptualSchemaNode"></param>
        /// <param name="mappingNode"></param>
        /// <param name="storageSchemaNode"></param>
        ///
        internal static void ExtractConceptualMappingAndStorageNodes(StreamReader edmxInputStream,
            out XmlElement conceptualSchemaNode, out XmlElement mappingNode, out XmlElement storageSchemaNode, out string metadataArtifactProcessingValue)
        {
            // load up an XML document representing the edmx file
            XmlDocument xmlDocument = new XmlDocument();
            using (var reader = XmlReader.Create(edmxInputStream))
            {
                xmlDocument.Load(reader);
            }

            EFNamespaceSet set = v3Namespaces;
            if (xmlDocument.DocumentElement.NamespaceURI == v2Namespaces.Edmx)
            {
                set = v2Namespaces;
            }
            else if (xmlDocument.DocumentElement.NamespaceURI == v1Namespaces.Edmx)
            {
                set = v1Namespaces;
            }

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(xmlDocument.NameTable);
            nsMgr.AddNamespace("edmx", set.Edmx);
            nsMgr.AddNamespace("edm", set.Csdl);
            nsMgr.AddNamespace("ssdl", set.Ssdl);
            nsMgr.AddNamespace("map", set.Msl);

            // find the ConceptualModel Schema node
            conceptualSchemaNode = (XmlElement)xmlDocument.SelectSingleNode(
                "/edmx:Edmx/edmx:Runtime/edmx:ConceptualModels/edm:Schema", nsMgr);

            // find the StorageModel Schema node
            storageSchemaNode = (XmlElement)xmlDocument.SelectSingleNode(
                "/edmx:Edmx/edmx:Runtime/edmx:StorageModels/ssdl:Schema", nsMgr);

            // find the Mapping node
            mappingNode = (XmlElement)xmlDocument.SelectSingleNode(
                "/edmx:Edmx/edmx:Runtime/edmx:Mappings/map:Mapping", nsMgr);

            // find the Connection node

            metadataArtifactProcessingValue = String.Empty;
            XmlNodeList connectionProperties = xmlDocument.SelectNodes(
                "/edmx:Edmx/edmx:Designer/edmx:Connection/edmx:DesignerInfoPropertySet/edmx:DesignerProperty", nsMgr);
            if (connectionProperties != null)
            {
                foreach (XmlNode propertyNode in connectionProperties)
                {
                    foreach (XmlAttribute a in propertyNode.Attributes)
                    {
                        // treat attribute names case-sensitive (since it is xml), but attribute value case-insensitive to be accommodating .
                        if (a.Name.Equals("Name", StringComparison.Ordinal) && a.Value.Equals("MetadataArtifactProcessing", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (XmlAttribute a2 in propertyNode.Attributes)
                            {
                                if (a2.Name.Equals("Value", StringComparison.Ordinal))
                                {
                                    metadataArtifactProcessingValue = a2.Value;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        // utility method to ensure an XmlElement (containing the C, M or S element
        // from the Edmx file) is sent out to a stream in the same format
        internal static void OutputXmlElementToStream(XmlElement xmlElement, Stream stream)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            settings.Indent = true;

            // set up output document
            XmlDocument outputXmlDoc = new XmlDocument();
            XmlNode importedElement = outputXmlDoc.ImportNode(xmlElement, true);
            outputXmlDoc.AppendChild(importedElement);

            // write out XmlDocument
            XmlWriter writer = null;
            try
            {
                writer = XmlWriter.Create(stream, settings);
                outputXmlDoc.WriteTo(writer);
            }
            finally
            {
                if (writer != null) { writer.Close(); }
            }
        }

        private struct EFNamespaceSet
        {
            public string Edmx;
            public string Csdl;
            public string Msl;
            public string Ssdl;
        }
    }
}
