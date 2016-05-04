//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.IdentityModel;

    static class XmlHelper
    {
        internal static string GetWhiteSpace(XmlReader reader)
        {
            string s = null;
            StringBuilder sb = null;
            while (reader.NodeType == XmlNodeType.Whitespace || reader.NodeType == XmlNodeType.SignificantWhitespace)
            {
                if (sb != null)
                {
                    sb.Append(reader.Value);
                }
                else if (s != null)
                {
                    sb = new StringBuilder(s);
                    sb.Append(reader.Value);
                    s = null;
                }
                else
                {
                    s = reader.Value;
                }
                if (!reader.Read())
                {
                    break;
                }
            }
            return sb != null ? sb.ToString() : s;
        }

        internal static void OnRequiredAttributeMissing(string attrName, string elementName)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.RequiredAttributeMissing, attrName, elementName)));
        }

        internal static string ReadEmptyElementAndRequiredAttribute(XmlDictionaryReader reader,
            XmlDictionaryString name, XmlDictionaryString namespaceUri, XmlDictionaryString attributeName,
            out string prefix)
        {
            reader.MoveToStartElement(name, namespaceUri);
            prefix = reader.Prefix;
            bool isEmptyElement = reader.IsEmptyElement;
            string value = reader.GetAttribute(attributeName, null);
            if (value == null)
            {
                OnRequiredAttributeMissing(attributeName.Value, null);
            }
            reader.Read();

            if (!isEmptyElement)
            {
                reader.ReadEndElement();
            }
            return value;
        }

        internal static string ReadTextElementAsTrimmedString(XmlElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            XmlReader reader = new XmlNodeReader(element);
            reader.MoveToContent();
            return XmlUtil.Trim(reader.ReadElementContentAsString());
        }

        internal static void OnRequiredElementMissing(string elementName, string elementNamespace)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ExpectedElementMissing, elementName, elementNamespace)));
        }

        internal static void OnUnexpectedChildNodeError(string parentName, XmlReader r)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.UnexpectedXmlChildNode, r.Name, r.NodeType, parentName)));
        }

        internal static void OnUnexpectedChildNodeError(XmlElement parent, XmlNode n)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.UnexpectedXmlChildNode, n.Name, n.NodeType, parent.Name)));
        }

        internal static System.Xml.UniqueId GetAttributeAsUniqueId(XmlDictionaryReader reader, XmlDictionaryString localName, XmlDictionaryString ns)
        {
            return GetAttributeAsUniqueId(reader, localName.Value, (ns != null ? ns.Value : null));
        }

        static System.Xml.UniqueId GetAttributeAsUniqueId(XmlDictionaryReader reader, string name, string ns)
        {
            if (!reader.MoveToAttribute(name, ns))
            {
                return null;
            }

            System.Xml.UniqueId id = reader.ReadContentAsUniqueId();
            reader.MoveToElement();

            return id;
        }

        static public void WriteAttributeStringAsUniqueId(XmlDictionaryWriter writer, string prefix, XmlDictionaryString localName, XmlDictionaryString ns, System.Xml.UniqueId id)
        {
            writer.WriteStartAttribute(prefix, localName, ns);
            writer.WriteValue(id);
            writer.WriteEndAttribute();
        }

        static public Int64 ReadElementContentAsInt64(XmlDictionaryReader reader)
        {
            reader.ReadFullStartElement();
            Int64 i = reader.ReadContentAsLong();
            reader.ReadEndElement();
            return i;
        }

    }

}
