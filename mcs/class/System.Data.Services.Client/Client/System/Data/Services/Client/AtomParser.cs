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
    #region Namespaces.

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Linq;
    using System.Text;

    #endregion Namespaces.

    [DebuggerDisplay("AtomParser {kind} {reader}")]
    internal class AtomParser
    {
        #region Private fields.

        private readonly Func<XmlReader, KeyValuePair<XmlReader, object>> entryCallback;

        private readonly Stack<XmlReader> readers;

        private readonly string typeScheme;

        private AtomEntry entry;

        private AtomFeed feed;

        private AtomDataKind kind;

        private XmlReader reader;

        private string currentDataNamespace;

        #endregion Private fields.

        #region Constructors.

        internal AtomParser(XmlReader reader, Func<XmlReader, KeyValuePair<XmlReader, object>> entryCallback, string typeScheme, string currentDataNamespace)
        {
            Debug.Assert(reader != null, "reader != null");
            Debug.Assert(typeScheme != null, "typeScheme != null");
            Debug.Assert(entryCallback != null, "entryCallback != null");
            Debug.Assert(!String.IsNullOrEmpty(currentDataNamespace), "currentDataNamespace is empty or null");

            this.reader = reader;
            this.readers = new Stack<XmlReader>();
            this.entryCallback = entryCallback;
            this.typeScheme = typeScheme;
            this.currentDataNamespace = currentDataNamespace;
            
            Debug.Assert(this.kind == AtomDataKind.None, "this.kind == AtomDataKind.None -- otherwise not initialized correctly");
        }

        #endregion Constructors.

        #region Internal properties.

        internal AtomEntry CurrentEntry
        {
            get
            {
                return this.entry;
            }
        }

        internal AtomFeed CurrentFeed
        {
            get
            {
                return this.feed;
            }
        }

        internal AtomDataKind DataKind
        {
            get
            {
                return this.kind;
            }
        }

        internal bool IsDataWebElement
        {
            get { return this.reader.NamespaceURI == this.currentDataNamespace; }
        }

        #endregion Internal properties.

        #region Internal methods.

        internal static KeyValuePair<XmlReader, object> XElementBuilderCallback(XmlReader reader)
        {
            Debug.Assert(reader != null, "reader != null");
            Debug.Assert(reader is Xml.XmlWrappingReader, "reader must be a instance of XmlWrappingReader");
            
            string readerBaseUri = reader.BaseURI;
            XElement element = XElement.Load(reader.ReadSubtree(), LoadOptions.None);
            return new KeyValuePair<XmlReader, object>(Xml.XmlWrappingReader.CreateReader(readerBaseUri, element.CreateReader()), element);
        }

        #endregion Internal methods.

        #region Internal methods.

        internal bool Read()
        {
            if (this.DataKind == AtomDataKind.Finished)
            {
                return false;
            }

            while (this.reader.Read())
            {
                if (ShouldIgnoreNode(this.reader))
                {
                    continue;
                }

                Debug.Assert(
                    this.reader.NodeType == XmlNodeType.Element || this.reader.NodeType == XmlNodeType.EndElement,
                    "this.reader.NodeType == XmlNodeType.Element || this.reader.NodeType == XmlNodeType.EndElement -- otherwise we should have ignored or thrown");

                AtomDataKind readerData = ParseStateForReader(this.reader);

                if (this.reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }

                switch (readerData)
                {
                    case AtomDataKind.Custom:
                        if (this.DataKind == AtomDataKind.None)
                        {
                            this.kind = AtomDataKind.Custom;
                            return true;
                        }
                        else
                        {
                            MaterializeAtom.SkipToEnd(this.reader);
                            continue;
                        }

                    case AtomDataKind.Entry:
                        this.kind = AtomDataKind.Entry;
                        this.ParseCurrentEntry(out this.entry);
                        return true;

                    case AtomDataKind.Feed:
                        if (this.DataKind == AtomDataKind.None)
                        {
                            this.feed = new AtomFeed();
                            this.kind = AtomDataKind.Feed;
                            return true;
                        }

                        throw new InvalidOperationException(Strings.AtomParser_FeedUnexpected);

                    case AtomDataKind.FeedCount:
                        this.ParseCurrentFeedCount();
                        break;

                    case AtomDataKind.PagingLinks:
                        if (this.feed == null)
                        {
                            throw new InvalidOperationException(Strings.AtomParser_PagingLinkOutsideOfFeed);
                        }

                        this.kind = AtomDataKind.PagingLinks;
                        this.ParseCurrentFeedPagingLinks();
                        return true;

                    default:
                        Debug.Assert(false, "Atom Parser is in a wrong state...Did you add a new AtomDataKind?");
                        break;
                }
            }

            this.kind = AtomDataKind.Finished;
            this.entry = null;            
            return false;
        }

        internal AtomContentProperty ReadCurrentPropertyValue()
        {
            Debug.Assert(
                this.kind == AtomDataKind.Custom,
                "this.kind == AtomDataKind.Custom -- otherwise caller shouldn't invoke ReadCurrentPropertyValue");
            return this.ReadPropertyValue();
        }

        internal string ReadCustomElementString()
        {
            Debug.Assert(
                this.kind == AtomDataKind.Custom,
                "this.kind == AtomDataKind.Custom -- otherwise caller shouldn't invoke ReadCustomElementString");
            return MaterializeAtom.ReadElementString(this.reader, true);
        }

        internal void ReplaceReader(XmlReader newReader)
        {
            Debug.Assert(newReader != null, "newReader != null");
            this.reader = newReader;
        }

        #endregion Internal methods.

        #region Private methods.

        private static AtomDataKind ParseStateForReader(XmlReader reader)
        {
            Debug.Assert(reader != null, "reader != null");
            Debug.Assert(
                reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.EndElement,
                "reader.NodeType == XmlNodeType.Element || EndElement -- otherwise can't determine");

            AtomDataKind result = AtomDataKind.Custom;
            string elementName = reader.LocalName;
            string namespaceURI = reader.NamespaceURI;
            if (Util.AreSame(XmlConstants.AtomNamespace, namespaceURI))
            {
                if (Util.AreSame(XmlConstants.AtomEntryElementName, elementName))
                {
                    result = AtomDataKind.Entry;
                }
                else if (Util.AreSame(XmlConstants.AtomFeedElementName, elementName))
                {
                    result = AtomDataKind.Feed;
                }
                else if (Util.AreSame(XmlConstants.AtomLinkElementName, elementName) &&
                    Util.AreSame(XmlConstants.AtomLinkNextAttributeString, reader.GetAttribute(XmlConstants.AtomLinkRelationAttributeName)))
                {
                    result = AtomDataKind.PagingLinks;
                }
            }
            else if (Util.AreSame(XmlConstants.DataWebMetadataNamespace, namespaceURI))
            {
                if (Util.AreSame(XmlConstants.RowCountElement, elementName))
                {
                    result = AtomDataKind.FeedCount;
                }
            }

            return result;
        }

        private static bool ReadChildElement(XmlReader reader, string localName, string namespaceUri)
        {
            Debug.Assert(localName != null, "localName != null");
            Debug.Assert(namespaceUri != null, "namespaceUri != null");
            Debug.Assert(!reader.IsEmptyElement, "!reader.IsEmptyElement");
            Debug.Assert(reader.NodeType != XmlNodeType.EndElement, "reader.NodeType != XmlNodeType.EndElement");

            return reader.Read() && reader.IsStartElement(localName, namespaceUri);
        }

        private static void SkipToEndAtDepth(XmlReader reader, int depth)
        {
            Debug.Assert(reader != null, "reader != null");
            Debug.Assert(reader.Depth >= depth, "reader.Depth >= depth");

            while (!(reader.Depth == depth && 
                     (reader.NodeType == XmlNodeType.EndElement ||
                      (reader.NodeType == XmlNodeType.Element && reader.IsEmptyElement))))
            {
                reader.Read();
            }
        }

        private static string ReadElementStringForText(XmlReader reader)
        {
            Debug.Assert(reader != null, "reader != null");
            if (reader.IsEmptyElement)
            {
                return String.Empty;
            }

            StringBuilder result = new StringBuilder();
            int depth = reader.Depth;
            while (reader.Read())
            {
                if (reader.Depth == depth)
                {
                    Debug.Assert(
                        reader.NodeType == XmlNodeType.EndElement, 
                        "reader.NodeType == XmlNodeType.EndElement -- otherwise XmlReader is acting odd");
                    break;
                }

                if (reader.NodeType == XmlNodeType.SignificantWhitespace ||
                    reader.NodeType == XmlNodeType.Text)
                {
                    result.Append(reader.Value);
                }
            }

            return result.ToString();
        }

        private static bool ShouldIgnoreNode(XmlReader reader)
        {
            Debug.Assert(reader != null, "reader != null");

            switch (reader.NodeType)
            {
                case XmlNodeType.CDATA:
                case XmlNodeType.EntityReference:
                case XmlNodeType.EndEntity:
                    Error.ThrowInternalError(InternalError.UnexpectedXmlNodeTypeWhenReading);
                    break;
                case XmlNodeType.Text:
                case XmlNodeType.SignificantWhitespace:
                    Error.ThrowInternalError(InternalError.UnexpectedXmlNodeTypeWhenReading);
                    break;
                case XmlNodeType.Element:
                case XmlNodeType.EndElement:
                    return false;
                default:
                    break;
            }

            return true;
        }

        private static bool IsAllowedContentType(string contentType)
        {
            return (String.Equals(XmlConstants.MimeApplicationXml, contentType, StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(XmlConstants.MimeApplicationAtom, contentType, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsAllowedLinkType(string linkType, out bool isFeed)
        {
            isFeed = String.Equals(XmlConstants.LinkMimeTypeFeed, linkType, StringComparison.OrdinalIgnoreCase);
            return isFeed ? true : String.Equals(XmlConstants.LinkMimeTypeEntry, linkType, StringComparison.OrdinalIgnoreCase);
        }

        private void ParseCurrentContent(AtomEntry targetEntry)
        {
            Debug.Assert(targetEntry != null, "targetEntry != null");
            Debug.Assert(this.reader.NodeType == XmlNodeType.Element, "this.reader.NodeType == XmlNodeType.Element");
            
            string propertyValue = this.reader.GetAttributeEx(XmlConstants.AtomContentSrcAttributeName, XmlConstants.AtomNamespace);
            if (propertyValue != null)
            {
                if (!this.reader.IsEmptyElement)
                {
                    throw Error.InvalidOperation(Strings.Deserialize_ExpectedEmptyMediaLinkEntryContent);
                }

                targetEntry.MediaLinkEntry = true;
                targetEntry.MediaContentUri = new Uri(propertyValue, UriKind.RelativeOrAbsolute);
            }
            else
            {
                if (targetEntry.MediaLinkEntry.HasValue && targetEntry.MediaLinkEntry.Value)
                {
                    throw Error.InvalidOperation(Strings.Deserialize_ContentPlusPropertiesNotAllowed);
                }

                targetEntry.MediaLinkEntry = false;

                propertyValue = this.reader.GetAttributeEx(XmlConstants.AtomTypeAttributeName, XmlConstants.AtomNamespace);
                if (AtomParser.IsAllowedContentType(propertyValue))
                {
                    if (this.reader.IsEmptyElement)
                    {
                        return;
                    }

                    if (ReadChildElement(this.reader, XmlConstants.AtomPropertiesElementName, XmlConstants.DataWebMetadataNamespace))
                    {
                        this.ReadCurrentProperties(targetEntry.DataValues);
                    }
                    else
                    if (this.reader.NodeType != XmlNodeType.EndElement)
                    {
                        throw Error.InvalidOperation(Strings.Deserialize_NotApplicationXml);
                    }
                }
            }
        }

        private void ParseCurrentLink(AtomEntry targetEntry)
        {
            Debug.Assert(targetEntry != null, "targetEntry != null");
            Debug.Assert(
                this.reader.NodeType == XmlNodeType.Element, 
                "this.reader.NodeType == XmlNodeType.Element -- otherwise we shouldn't try to parse a link");
            Debug.Assert(
                this.reader.LocalName == "link",
                "this.reader.LocalName == 'link' -- otherwise we shouldn't try to parse a link");

            string relation = this.reader.GetAttribute(XmlConstants.AtomLinkRelationAttributeName);
            if (relation == null)
            {
                return;
            }

            if (relation == XmlConstants.AtomEditRelationAttributeValue && targetEntry.EditLink == null)
            {
                string href = this.reader.GetAttribute(XmlConstants.AtomHRefAttributeName);
                if (String.IsNullOrEmpty(href))
                {
                    throw Error.InvalidOperation(Strings.Context_MissingEditLinkInResponseBody);
                }

                targetEntry.EditLink = this.ConvertHRefAttributeValueIntoURI(href);
            }
            else if (relation == XmlConstants.AtomSelfRelationAttributeValue && targetEntry.QueryLink == null)
            {
                string href = this.reader.GetAttribute(XmlConstants.AtomHRefAttributeName);
                if (String.IsNullOrEmpty(href))
                {
                    throw Error.InvalidOperation(Strings.Context_MissingSelfLinkInResponseBody);
                }

                targetEntry.QueryLink = this.ConvertHRefAttributeValueIntoURI(href);
            }
            else if (relation == XmlConstants.AtomEditMediaRelationAttributeValue && targetEntry.MediaEditUri == null)
            {
                string href = this.reader.GetAttribute(XmlConstants.AtomHRefAttributeName);
                if (String.IsNullOrEmpty(href))
                {
                    throw Error.InvalidOperation(Strings.Context_MissingEditMediaLinkInResponseBody);
                }

                targetEntry.MediaEditUri = this.ConvertHRefAttributeValueIntoURI(href);
                targetEntry.StreamETagText = this.reader.GetAttribute(XmlConstants.AtomETagAttributeName, XmlConstants.DataWebMetadataNamespace);
            }

            if (!this.reader.IsEmptyElement)
            {
                string propertyName = UriUtil.GetNameFromAtomLinkRelationAttribute(relation);
                if (propertyName == null)
                {
                    return;
                }

                string propertyValueText = this.reader.GetAttribute(XmlConstants.AtomTypeAttributeName);
                bool isFeed;

                if (!IsAllowedLinkType(propertyValueText, out isFeed))
                {
                    return;
                }

                if (!ReadChildElement(this.reader, XmlConstants.AtomInlineElementName, XmlConstants.DataWebMetadataNamespace))
                {
                    return;
                }

                bool emptyInlineCollection = this.reader.IsEmptyElement;
                object propertyValue = null;

                if (!emptyInlineCollection)
                {
                    AtomFeed nestedFeed = null;
                    AtomEntry nestedEntry = null;
                    List<AtomEntry> feedEntries = null;

                    Debug.Assert(this.reader is Xml.XmlWrappingReader, "reader must be a instance of XmlWrappingReader");
                    string readerBaseUri = this.reader.BaseURI;
                    XmlReader nestedReader = Xml.XmlWrappingReader.CreateReader(readerBaseUri, this.reader.ReadSubtree());
                    nestedReader.Read();
                    Debug.Assert(nestedReader.LocalName == "inline", "nestedReader.LocalName == 'inline'");

                    AtomParser nested = new AtomParser(nestedReader, this.entryCallback, this.typeScheme, this.currentDataNamespace);
                    while (nested.Read())
                    {
                        switch (nested.DataKind)
                        {
                            case AtomDataKind.Feed:
                                feedEntries = new List<AtomEntry>();
                                nestedFeed = nested.CurrentFeed;
                                propertyValue = nestedFeed;
                                break;
                            case AtomDataKind.Entry:
                                nestedEntry = nested.CurrentEntry;
                                if (feedEntries != null)
                                {
                                    feedEntries.Add(nestedEntry);
                                }
                                else
                                {
                                    propertyValue = nestedEntry;
                                }

                                break;
                            case AtomDataKind.PagingLinks:
                                break;
                            default:
                                throw new InvalidOperationException(Strings.AtomParser_UnexpectedContentUnderExpandedLink);
                        }
                    }

                    if (nestedFeed != null)
                    {
                        Debug.Assert(
                            nestedFeed.Entries == null,
                            "nestedFeed.Entries == null -- otherwise someone initialized this for us");
                        nestedFeed.Entries = feedEntries;
                    }
                }

                AtomContentProperty property = new AtomContentProperty();
                property.Name = propertyName;

                if (emptyInlineCollection || propertyValue == null)
                {
                    property.IsNull = true;
                    if (isFeed)
                    {
                        property.Feed = new AtomFeed();
                        property.Feed.Entries = Enumerable.Empty<AtomEntry>();
                    }
                    else
                    {
                        property.Entry = new AtomEntry();
                        property.Entry.IsNull = true;
                    }
                }
                else
                {
                    property.Feed = propertyValue as AtomFeed;
                    property.Entry = propertyValue as AtomEntry;
                }

                targetEntry.DataValues.Add(property);
            }
        }
        
        private void ReadPropertyValueIntoResult(AtomContentProperty property)
        {
            Debug.Assert(this.reader != null, "reader != null");
            Debug.Assert(property != null, "property != null");

            switch (this.reader.NodeType)
            {
                case XmlNodeType.CDATA:
                case XmlNodeType.SignificantWhitespace:
                case XmlNodeType.Text:
                    if (!String.IsNullOrEmpty(property.Text))
                    {
                        throw Error.InvalidOperation(Strings.Deserialize_MixedTextWithComment);
                    }

                    property.Text = this.reader.Value;
                    break;

                case XmlNodeType.Comment:
                case XmlNodeType.Whitespace:
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.EndElement:
                    break;

                case XmlNodeType.Element:
                    if (!String.IsNullOrEmpty(property.Text))
                    {
                        throw Error.InvalidOperation(Strings.Deserialize_ExpectingSimpleValue);
                    }

                    property.EnsureProperties();
                    AtomContentProperty prop = this.ReadPropertyValue();

                    if (prop != null)
                    {
                        property.Properties.Add(prop);
                    }

                    break;

                default:
                    throw Error.InvalidOperation(Strings.Deserialize_ExpectingSimpleValue);
            }
        }

        private AtomContentProperty ReadPropertyValue()
        {
            Debug.Assert(this.reader != null, "reader != null");
            Debug.Assert(
                this.reader.NodeType == XmlNodeType.Element,
                "reader.NodeType == XmlNodeType.Element -- otherwise caller is confused as to where the reader is");

            if (!this.IsDataWebElement)
            {
                SkipToEndAtDepth(this.reader, this.reader.Depth);
                return null;
            }

            AtomContentProperty result = new AtomContentProperty();
            result.Name = this.reader.LocalName;
            result.TypeName = this.reader.GetAttributeEx(XmlConstants.AtomTypeAttributeName, XmlConstants.DataWebMetadataNamespace);
            result.IsNull = Util.DoesNullAttributeSayTrue(this.reader);
            result.Text = result.IsNull ? null : String.Empty;

            if (!this.reader.IsEmptyElement)
            {
                int depth = this.reader.Depth;
                while (this.reader.Read())
                {
                    this.ReadPropertyValueIntoResult(result);
                    if (this.reader.Depth == depth)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        private void ReadCurrentProperties(List<AtomContentProperty> values)
        {
            Debug.Assert(values != null, "values != null");
            Debug.Assert(this.reader.NodeType == XmlNodeType.Element, "this.reader.NodeType == XmlNodeType.Element");

            while (this.reader.Read())
            {
                if (ShouldIgnoreNode(this.reader))
                {
                    continue;
                }

                if (this.reader.NodeType == XmlNodeType.EndElement)
                {
                    return;
                }

                if (this.reader.NodeType == XmlNodeType.Element)
                {
                    AtomContentProperty prop = this.ReadPropertyValue();

                    if (prop != null)
                    {
                        values.Add(prop);
                    }
                }
            }
        }

        private void ParseCurrentEntry(out AtomEntry targetEntry)
        {
            Debug.Assert(this.reader.NodeType == XmlNodeType.Element, "this.reader.NodeType == XmlNodeType.Element");

            var callbackResult = this.entryCallback(this.reader);
            Debug.Assert(callbackResult.Key != null, "callbackResult.Key != null");
            this.readers.Push(this.reader);
            this.reader = callbackResult.Key;

            this.reader.Read();
            Debug.Assert(this.reader.LocalName == "entry", "this.reader.LocalName == 'entry' - otherwise we're not reading the subtree");

            bool hasContent = false;
            targetEntry = new AtomEntry();
            targetEntry.DataValues = new List<AtomContentProperty>();
            targetEntry.Tag = callbackResult.Value;
            targetEntry.ETagText = this.reader.GetAttribute(XmlConstants.AtomETagAttributeName, XmlConstants.DataWebMetadataNamespace);

            while (this.reader.Read())
            {
                if (ShouldIgnoreNode(this.reader))
                {
                    continue;
                }

                if (this.reader.NodeType == XmlNodeType.Element)
                {
                    int depth = this.reader.Depth;
                    string elementName = this.reader.LocalName;
                    string namespaceURI = this.reader.NamespaceURI;
                    if (namespaceURI == XmlConstants.AtomNamespace)
                    {
                        if (elementName == XmlConstants.AtomCategoryElementName && targetEntry.TypeName == null)
                        {
                            string text = this.reader.GetAttributeEx(XmlConstants.AtomCategorySchemeAttributeName, XmlConstants.AtomNamespace);
                            if (text == this.typeScheme)
                            {
                                targetEntry.TypeName = this.reader.GetAttributeEx(XmlConstants.AtomCategoryTermAttributeName, XmlConstants.AtomNamespace);
                            }
                        }
                        else if (elementName == XmlConstants.AtomContentElementName)
                        {
                            hasContent = true;
                            this.ParseCurrentContent(targetEntry);
                        }
                        else if (elementName == XmlConstants.AtomIdElementName && targetEntry.Identity == null)
                        {
                            string idText = ReadElementStringForText(this.reader);
                            idText = Util.ReferenceIdentity(idText);
                            
                            Uri idUri = Util.CreateUri(idText, UriKind.RelativeOrAbsolute);
                            if (!idUri.IsAbsoluteUri)
                            {
                                throw Error.InvalidOperation(Strings.Context_TrackingExpectsAbsoluteUri);
                            }

                            targetEntry.Identity = idText;
                        }
                        else if (elementName == XmlConstants.AtomLinkElementName)
                        {
                            this.ParseCurrentLink(targetEntry);
                        }
                    }
                    else if (namespaceURI == XmlConstants.DataWebMetadataNamespace)
                    {
                        if (elementName == XmlConstants.AtomPropertiesElementName)
                        {
                            if (targetEntry.MediaLinkEntry.HasValue && !targetEntry.MediaLinkEntry.Value)
                            {
                                throw Error.InvalidOperation(Strings.Deserialize_ContentPlusPropertiesNotAllowed);
                            }

                            targetEntry.MediaLinkEntry = true;

                            if (!this.reader.IsEmptyElement)
                            {
                                this.ReadCurrentProperties(targetEntry.DataValues);
                            }
                        }
                    }

                    SkipToEndAtDepth(this.reader, depth);
                }
            }

            if (targetEntry.Identity == null)
            {
                throw Error.InvalidOperation(Strings.Deserialize_MissingIdElement);
            }

            if (!hasContent)
            {
                throw Error.BatchStreamContentExpected(BatchStreamState.GetResponse);
            }

            this.reader = this.readers.Pop();
        }

        private void ParseCurrentFeedCount()
        {
            if (this.feed == null)
            {
                throw new InvalidOperationException(Strings.AtomParser_FeedCountNotUnderFeed);
            }

            if (this.feed.Count.HasValue)
            {
                throw new InvalidOperationException(Strings.AtomParser_ManyFeedCounts);
            }

            long countValue;
            if (!long.TryParse(MaterializeAtom.ReadElementString(this.reader, true), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out countValue))
            {
                throw new FormatException(Strings.MaterializeFromAtom_CountFormatError);
            }

            if (countValue < 0)
            {
                throw new FormatException(Strings.MaterializeFromAtom_CountFormatError);
            }

            this.feed.Count = countValue;
        }

        private void ParseCurrentFeedPagingLinks()
        {
            Debug.Assert(this.feed != null, "Trying to parser paging links but feed is null.");

            if (this.feed.NextLink != null)
            {
                throw new InvalidOperationException(Strings.AtomMaterializer_DuplicatedNextLink);
            }

            string nextLink = this.reader.GetAttribute(XmlConstants.AtomHRefAttributeName);

            if (nextLink == null)
            {
                throw new InvalidOperationException(Strings.AtomMaterializer_LinksMissingHref);
            }
            else
            {
                this.feed.NextLink = this.ConvertHRefAttributeValueIntoURI(nextLink);
            }
        }

        private Uri ConvertHRefAttributeValueIntoURI(string href)
        {
            Uri uri = Util.CreateUri(href, UriKind.RelativeOrAbsolute);
            if (!uri.IsAbsoluteUri && !String.IsNullOrEmpty(this.reader.BaseURI))
            {
                Uri baseUri = Util.CreateUri(this.reader.BaseURI, UriKind.RelativeOrAbsolute);

                uri = new Uri(baseUri, uri);
            }

            return uri;
        }

        #endregion Private methods.
    }
}
