//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Runtime;
    using System.Xml;

    static class XmlUtil
    {
        public const string XmlNs = "http://www.w3.org/XML/1998/namespace";
        public const string XmlNsNs = "http://www.w3.org/2000/xmlns/";

        public static string GetXmlLangAttribute(XmlReader reader)
        {
            string xmlLang = null;
            if (reader.MoveToAttribute("lang", XmlNs))
            {
                xmlLang = reader.Value;
                reader.MoveToElement();
            }

            if (xmlLang == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.XmlLangAttributeMissing)));

            return xmlLang;
        }

        // FIX for 7455
        public static bool IsTrue(string booleanValue)
        {
            if (string.IsNullOrEmpty(booleanValue))
            {
                return false;
            }

            return XmlConvert.ToBoolean(booleanValue);
        }

        public static void ReadContentAsQName(XmlReader reader, out string localName, out string ns)
        {
            ParseQName(reader, reader.ReadContentAsString(), out localName, out ns);
        }

        public static bool IsWhitespace(char ch)
        {
            return (ch == ' ' || ch == '\t' || ch == '\r' || ch == '\n');
        }

        public static string TrimEnd(string s)
        {
            int i;
            for (i = s.Length; i > 0 && IsWhitespace(s[i - 1]); i--);

            if (i != s.Length)
            {
                return s.Substring(0, i);
            }

            return s;
        }

        public static string TrimStart(string s)
        {
            int i;
            for (i = 0; i < s.Length && IsWhitespace(s[i]); i++);

            if (i != 0)
            {
                return s.Substring(i);
            }

            return s;
        }

        public static string Trim(string s)
        {
            int i;
            for (i = 0; i < s.Length && IsWhitespace(s[i]); i++);

            if (i >= s.Length)
            {
                return string.Empty;
            }

            int j;
            for (j = s.Length; j > 0 && IsWhitespace(s[j - 1]); j--);

            Fx.Assert(j > i, "Logic error in XmlUtil.Trim().");

            if (i != 0 || j != s.Length)
            {
                return s.Substring(i, j - i);
            }
            return s;
        }

        public static void ParseQName(XmlReader reader, string qname, out string localName, out string ns)
        {
            int index = qname.IndexOf(':');
            string prefix;
            if (index < 0)
            {
                prefix = "";
                localName = TrimStart(TrimEnd(qname));
            }
            else
            {
                if (index == qname.Length - 1)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.InvalidXmlQualifiedName, qname)));
                prefix = TrimStart(qname.Substring(0, index));
                localName = TrimEnd(qname.Substring(index + 1));
            }
            ns = reader.LookupNamespace(prefix);
            if (ns == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.UnboundPrefixInQName, qname)));
        }
    }
}
