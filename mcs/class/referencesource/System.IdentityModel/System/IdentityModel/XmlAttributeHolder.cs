//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.Xml;

    struct XmlAttributeHolder
    {
        string prefix;
        string ns;
        string localName;
        string value;

        public static XmlAttributeHolder[] emptyArray = new XmlAttributeHolder[0];

        public XmlAttributeHolder(string prefix, string localName, string ns, string value)
        {
            this.prefix = prefix;
            this.localName = localName;
            this.ns = ns;
            this.value = value;
        }

        public string Prefix
        {
            get { return prefix; }
        }

        public string NamespaceUri
        {
            get { return ns; }
        }

        public string LocalName
        {
            get { return localName; }
        }

        public string Value
        {
            get { return value; }
        }

        public void WriteTo(XmlWriter writer)
        {
            writer.WriteStartAttribute(prefix, localName, ns);
            writer.WriteString(value);
            writer.WriteEndAttribute();
        }

        public static void WriteAttributes(XmlAttributeHolder[] attributes, XmlWriter writer)
        {
            for (int i = 0; i < attributes.Length; i++)
                attributes[i].WriteTo(writer);
        }

        public static XmlAttributeHolder[] ReadAttributes(XmlDictionaryReader reader)
        {
            int maxSizeOfHeaders = int.MaxValue;
            return ReadAttributes(reader, ref maxSizeOfHeaders);
        }

        public static XmlAttributeHolder[] ReadAttributes(XmlDictionaryReader reader, ref int maxSizeOfHeaders)
        {
            if (reader.AttributeCount == 0)
                return emptyArray;
            XmlAttributeHolder[] attributes = new XmlAttributeHolder[reader.AttributeCount];
            reader.MoveToFirstAttribute();
            for (int i = 0; i < attributes.Length; i++)
            {
                string ns = reader.NamespaceURI;
                string localName = reader.LocalName;
                string prefix = reader.Prefix;
                string value = string.Empty;
                while (reader.ReadAttributeValue())
                {
                    if (value.Length == 0)
                        value = reader.Value;
                    else
                        value += reader.Value;
                }
                Deduct(prefix, ref maxSizeOfHeaders);
                Deduct(localName, ref maxSizeOfHeaders);
                Deduct(ns, ref maxSizeOfHeaders);
                Deduct(value, ref maxSizeOfHeaders);
                attributes[i] = new XmlAttributeHolder(prefix, localName, ns, value);
                reader.MoveToNextAttribute();
            }
            reader.MoveToElement();
            return attributes;
        }

        static void Deduct(string s, ref int maxSizeOfHeaders)
        {
            int byteCount = s.Length * sizeof(char);
            if (byteCount > maxSizeOfHeaders)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlBufferQuotaExceeded)));
            }
            maxSizeOfHeaders -= byteCount;
        }

        public static string GetAttribute(XmlAttributeHolder[] attributes, string localName, string ns)
        {
            for (int i = 0; i < attributes.Length; i++)
                if (attributes[i].LocalName == localName && attributes[i].NamespaceUri == ns)
                    return attributes[i].Value;
            return null;
        }
    }
}
