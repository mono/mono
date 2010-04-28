//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.


namespace System.Data.Services.Client
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Xml;

    internal static partial class UriUtil
    {

        internal static string GetNameFromAtomLinkRelationAttribute(string value)
        {
            string name = null;
            if (!String.IsNullOrEmpty(value))
            {
                Uri uri = null;
                try
                {
                    uri = new Uri(value, UriKind.RelativeOrAbsolute);
                }
                catch (UriFormatException)
                {                }

                if ((null != uri) && uri.IsAbsoluteUri)
                {
                    string unescaped = uri.GetComponents(UriComponents.AbsoluteUri, UriFormat.SafeUnescaped);
                    if (unescaped.StartsWith(XmlConstants.DataWebRelatedNamespace, StringComparison.Ordinal))
                    {
                        name = unescaped.Substring(XmlConstants.DataWebRelatedNamespace.Length);
                    }
                }
            }

            return name;
        }

    }

    internal static partial class XmlUtil
    {
        private static NameTable CreateAtomNameTable()
        {
            NameTable table = new NameTable();
            table.Add(XmlConstants.AtomNamespace);
            table.Add(XmlConstants.DataWebNamespace);
            table.Add(XmlConstants.DataWebMetadataNamespace);

            table.Add(XmlConstants.AtomContentElementName);
            table.Add(XmlConstants.AtomContentSrcAttributeName);
            table.Add(XmlConstants.AtomEntryElementName);
            table.Add(XmlConstants.AtomETagAttributeName);
            table.Add(XmlConstants.AtomFeedElementName);

            table.Add(XmlConstants.AtomIdElementName);

            table.Add(XmlConstants.AtomInlineElementName);
            table.Add(XmlConstants.AtomLinkElementName);
            table.Add(XmlConstants.AtomLinkRelationAttributeName);
            table.Add(XmlConstants.AtomNullAttributeName);
            table.Add(XmlConstants.AtomPropertiesElementName);
            table.Add(XmlConstants.AtomTitleElementName);
            table.Add(XmlConstants.AtomTypeAttributeName);

            table.Add(XmlConstants.XmlErrorCodeElementName);
            table.Add(XmlConstants.XmlErrorElementName);
            table.Add(XmlConstants.XmlErrorInnerElementName);
            table.Add(XmlConstants.XmlErrorMessageElementName);
            table.Add(XmlConstants.XmlErrorTypeElementName);
            return table;
        }

        internal static XmlReader CreateXmlReader(Stream stream, Encoding encoding)
        {
            Debug.Assert(null != stream, "null stream");

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.CheckCharacters = false;
            settings.CloseInput = true;
            settings.IgnoreWhitespace = true;
            settings.NameTable = XmlUtil.CreateAtomNameTable();

            if (null == encoding)
            {                return XmlReader.Create(stream, settings);
            }

            return XmlReader.Create(new StreamReader(stream, encoding), settings);
        }

        internal static XmlWriterSettings CreateXmlWriterSettings(Encoding encoding)
        {
            Debug.Assert(null != encoding, "null != encoding");

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.CheckCharacters = false;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.Encoding = encoding;
            settings.Indent = true;
            settings.NewLineHandling = NewLineHandling.Entitize;

            Debug.Assert(!settings.CloseOutput, "!settings.CloseOutput -- otherwise default changed?");

            return settings;
        }

        internal static XmlWriter CreateXmlWriterAndWriteProcessingInstruction(Stream stream, Encoding encoding)
        {
            Debug.Assert(null != stream, "null != stream");
            Debug.Assert(null != encoding, "null != encoding");

            XmlWriterSettings settings = CreateXmlWriterSettings(encoding);
            XmlWriter writer = XmlWriter.Create(stream, settings);
            writer.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"" + encoding.WebName + "\" standalone=\"yes\"");
            return writer;
        }

        internal static string GetAttributeEx(this XmlReader reader, string attributeName, string namespaceUri)
        {
            return reader.GetAttribute(attributeName, namespaceUri) ?? reader.GetAttribute(attributeName);
        }

        internal static void RemoveDuplicateNamespaceAttributes(System.Xml.Linq.XElement element)
        {
            Debug.Assert(element != null, "element != null");

            HashSet<string> names = new HashSet<string>(EqualityComparer<string>.Default);
            foreach (System.Xml.Linq.XElement e in element.DescendantsAndSelf())
            {
                bool attributesFound = false;
                foreach (var attribute in e.Attributes())
                {
                    if (!attributesFound)
                    {
                        attributesFound = true;
                        names.Clear();
                    }

                    if (attribute.IsNamespaceDeclaration)
                    {
                        string localName = attribute.Name.LocalName;
                        bool alreadyPresent = names.Add(localName) == false;
                        if (alreadyPresent)
                        {
                            attribute.Remove();
                        }
                    }
                }
            }
        }
    }
}
