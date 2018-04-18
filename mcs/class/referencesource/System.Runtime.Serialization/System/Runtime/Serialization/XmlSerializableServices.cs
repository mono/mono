//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime.Serialization
{
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Schema;

    public static class XmlSerializableServices
    {
        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        internal static readonly string ReadNodesMethodName = "ReadNodes";
        public static XmlNode[] ReadNodes(XmlReader xmlReader)
        {
            if (xmlReader == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xmlReader");
            XmlDocument doc = new XmlDocument();
            List<XmlNode> nodeList = new List<XmlNode>();
            if (xmlReader.MoveToFirstAttribute())
            {
                do
                {
                    if (IsValidAttribute(xmlReader))
                    {
                        XmlNode node = doc.ReadNode(xmlReader);
                        if (node == null)
                            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.UnexpectedEndOfFile)));
                        nodeList.Add(node);
                    }
                } while (xmlReader.MoveToNextAttribute());
            }
            xmlReader.MoveToElement();
            if (!xmlReader.IsEmptyElement)
            {
                int startDepth = xmlReader.Depth;
                xmlReader.Read();
                while (xmlReader.Depth > startDepth && xmlReader.NodeType != XmlNodeType.EndElement)
                {
                    XmlNode node = doc.ReadNode(xmlReader);
                    if (node == null)
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.UnexpectedEndOfFile)));
                    nodeList.Add(node);
                }
            }
            return nodeList.ToArray();
        }

        private static bool IsValidAttribute(XmlReader xmlReader)
        {
            return xmlReader.NamespaceURI != Globals.SerializationNamespace &&
                                   xmlReader.NamespaceURI != Globals.SchemaInstanceNamespace &&
                                   xmlReader.Prefix != "xmlns" &&
                                   xmlReader.LocalName != "xmlns";
        }

        internal static string WriteNodesMethodName = "WriteNodes";
        public static void WriteNodes(XmlWriter xmlWriter, XmlNode[] nodes)
        {
            if (xmlWriter == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xmlWriter");
            if (nodes != null)
                for (int i = 0; i < nodes.Length; i++)
                    if (nodes[i] != null)
                        nodes[i].WriteTo(xmlWriter);
        }

        internal static string AddDefaultSchemaMethodName = "AddDefaultSchema";
        public static void AddDefaultSchema(XmlSchemaSet schemas, XmlQualifiedName typeQName)
        {
            if (schemas == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("schemas");
            if (typeQName == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("typeQName");
            SchemaExporter.AddDefaultXmlType(schemas, typeQName.Name, typeQName.Namespace);
        }
    }
}
