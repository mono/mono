//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    /// <summary>
    /// A filter containing an XPath 1.0 expression
    /// </summary>
    [XmlSchemaProvider("StaticGetSchema")]
    [XmlRoot(ElementName = "XPathMessageFilter", Namespace = XPathMessageFilter.RootNamespace)]
    public class XPathMessageFilter : MessageFilter, IXmlSerializable
    {
        internal const string NodeQuotaAttr = "NodeQuota";
        const string DialectAttr = "Dialect";
        const string OuterTypeName = "XPathMessageFilter";
        const string InnerElem = "XPath";
        const string XmlP = "xml";
        const string XmlnsP = "xmlns";
        const string WSEventingNamespace = "http://schemas.xmlsoap.org/ws/2004/06/eventing";

        internal const string XPathDialect = "http://www.w3.org/TR/1999/REC-xpath-19991116";
        //const string Namespace = NamingHelper.DefaultNamespace;
        const string RootNamespace = "http://schemas.microsoft.com/serviceModel/2004/05/xpathfilter";
        const string Namespace = RootNamespace + "/";

        static XPathQueryMatcher dummyMatcher = new XPathQueryMatcher(true);

        XPathQueryMatcher matcher;             // by default, set to a dummy matcher

        internal XmlNamespaceManager namespaces;

        string xpath;

        /*
        const string SchemaString = 
        "<xsd:schema xmlns:xsd='" + XmlSchema.Namespace + "'>" +
        "    <xsd:complexType name='XPathMessageFilter'>" + 
        "      <xsd:sequence>" + 
        "        <xsd:element name='" + InnerElem + "' >" + 
        "          <xsd:complexType>" + 
        "            <xsd:simpleContent>" +
        "              <xsd:extension base='xsd:string'>" +
        "                <xsd:attribute name='" + DialectAttr + "' type='xsd:string' use='optional'/>" + 
        "              </xsd:extension>" +
        "            </xsd:simpleContent>" +
        "          </xsd:complexType>" + 
        "        </xsd:element>" + 
        "      </xsd:sequence>" + 
        "      <xsd:attribute name='" + NodeQuotaAttr + "' type='xsd:int' use='optional'/>" + 
        "    </xsd:complexType>" + 
        "</xsd:schema>";
        
        static XPathMessageFilter()
        {
            XPathMessageFilter.schema = XmlSchema.Read(new StringReader(SchemaString), null);
        }
        */

        static XmlSchemaComplexType CreateOuterType()
        {
            // Dialect attribute
            XmlSchemaAttribute dAttr = new XmlSchemaAttribute();
            dAttr.Name = DialectAttr;
            dAttr.SchemaTypeName = new XmlQualifiedName("string", XmlSchema.Namespace);
            dAttr.Use = XmlSchemaUse.Optional;

            // Inner extension
            XmlSchemaSimpleContentExtension innerExt = new XmlSchemaSimpleContentExtension();
            innerExt.BaseTypeName = new XmlQualifiedName("string", XmlSchema.Namespace);
            innerExt.Attributes.Add(dAttr);

            // Inner content
            XmlSchemaSimpleContent innerContent = new XmlSchemaSimpleContent();
            innerContent.Content = innerExt;

            // Inner complexType
            XmlSchemaComplexType innerType = new XmlSchemaComplexType();
            innerType.ContentModel = innerContent;

            // Inner element
            XmlSchemaElement element = new XmlSchemaElement();
            element.Name = InnerElem;
            element.SchemaType = innerType;

            // Seq around innner elem
            XmlSchemaSequence sequence = new XmlSchemaSequence();
            sequence.Items.Add(element);

            // NodeQuota attribute
            XmlSchemaAttribute nqAttr = new XmlSchemaAttribute();
            nqAttr.Name = NodeQuotaAttr;
            nqAttr.SchemaTypeName = new XmlQualifiedName("int", XmlSchema.Namespace);
            nqAttr.Use = XmlSchemaUse.Optional;

            // anyAttribute on outer type
            // any namespace is the default
            XmlSchemaAnyAttribute anyAttr = new XmlSchemaAnyAttribute();

            // Outer type
            XmlSchemaComplexType outerType = new XmlSchemaComplexType();
            outerType.Name = OuterTypeName;
            outerType.Particle = sequence;
            outerType.Attributes.Add(nqAttr);
            outerType.AnyAttribute = anyAttr;

            return outerType;
        }

        public static XmlSchemaType StaticGetSchema(XmlSchemaSet schemas)
        {
            if (schemas == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("schemas");

            XmlSchemaComplexType outerType = CreateOuterType();

            if (schemas.Contains(XPathMessageFilter.Namespace))
            {
                IEnumerator en = schemas.Schemas(XPathMessageFilter.Namespace).GetEnumerator();
                en.MoveNext();
                ((XmlSchema)en.Current).Items.Add(outerType);
            }
            else
            {
                XmlSchema schema = new XmlSchema();
                schema.Items.Add(outerType);
                schema.TargetNamespace = XPathMessageFilter.Namespace;

                schemas.Add(schema);
            }

            return outerType;
        }

        /// <summary>
        /// Initializes a new instance of XPath filter with an empty XPath expression.
        /// An empty XPath expression ALWAYS MATCHES
        /// </summary>
        public XPathMessageFilter()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes an XPath expression 
        /// </summary>
        public XPathMessageFilter(string xpath)
            : this(xpath, new XPathMessageContext())
        {
        }

        /// <summary>
        /// Initializes an XPath that uses the given namespace manager to resolve prefixes
        /// </summary>
        public XPathMessageFilter(string xpath, XmlNamespaceManager namespaces)
        {
            if (null == xpath)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xpath");
            }
            this.Init(xpath, namespaces);
        }

        /// <summary>
        /// Initializes an XPath that uses the given XsltContext to resolve prefixes, functions, and variables
        /// Also associated the given tag with the filter
        /// You can pass in null for namespaces.
        /// </summary>
        public XPathMessageFilter(string xpath, XsltContext context)
        {
            if (null == xpath)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xpath");
            }

            this.Init(xpath, context);
        }

        /// <summary>
        /// Initializes a new instance of the class by reading in a streamed XPath with a specified Xml reader.
        /// The reader that is to read in the streamed XPath expression. 
        /// It is assumed that the reader is parked at the element containing the XPath, with ReaderState == StartElement. The
        /// content of the element will simply be consumed and no specific tag will be looked for. This allows the user to stash 
        /// their XPaths in whatever tag they want. The entire tag will have been consumed when the method returns.
        /// The constructor will also automatically initialize the namespace manager by resolving all prefixes in the xpath.
        /// </summary>
        public XPathMessageFilter(XmlReader reader)
            : this(reader, new XPathMessageContext())
        {
        }

        /// <summary>
        /// Initializes a new instance of the class by reading in a streamed XPath with a specified Xml reader.
        /// The reader that is to read in the streamed XPath expression. 
        /// It is assumed that the reader is parked at the element containing the XPath, with ReaderState == StartElement. The
        /// content of the element will simply be consumed and no specific tag will be looked for. This allows the user to stash 
        /// their XPaths in whatever tag they want. The entire tag will have been consumed when the method returns.
        /// The constructor will also automatically initialize the namespace manager by resolving all prefixes in the xpath.
        /// </summary>
        public XPathMessageFilter(XmlReader reader, XmlNamespaceManager namespaces)
            : base()
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            this.ReadFrom(reader, namespaces);
        }

        public XPathMessageFilter(XmlReader reader, XsltContext context)
            : base()
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            this.ReadFrom(reader, context);
        }

#if NO
        internal XPathFilterMatcher Matcher
        {
            get
            {
                return this.matcher;
            }
        }
#endif
        /// <summary>
        /// XPaths contain qnames with prefixes. The namespace maps maps those prefixes to actual namespaces. 
        /// </summary>
        /// <value></value>
        public XmlNamespaceManager Namespaces
        {
            get
            {
                return this.namespaces;
            }
        }

        /// <summary>
        /// This mitigates a security Threat.
        /// Some filters could be extremely expensive to evaluate or are very long running. Alternatively, a 
        /// filter could have a very large number of relatively simple filters that taken as a whole would be
        /// long running. 
        /// Since filters operate on Xml infosets, a natural and simple way to set computational limits
        /// is to specify the maximum # of nodes that should be looked at during filter evaluation. 
        /// </summary>
        public int NodeQuota
        {
            get
            {
                return this.matcher.NodeQuota;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("NodeQuota", value, SR.GetString(SR.FilterQuotaRange)));
                }

                this.EnsureMatcher();
                this.matcher.NodeQuota = value;
            }
        }

        public string XPath
        {
            get
            {
                return this.xpath;
            }
        }

        /// <summary>
        /// If the xpath is an empty string, there is nothing to compile and the filter always matches
        /// If not, try to compile the filter for execution within the filter engine's own query processor
        /// If that query processor cannot accept the filter (it doesn't fall within the class of xpaths it can handle),
        /// then revert to the fall-back solution - the slower Fx engine
        /// </summary>
        void Compile()
        {
            if (!this.matcher.IsCompiled)
            {
                this.EnsureMatcher();
                this.matcher.Compile(this.xpath, this.namespaces);
            }
        }

        /// <summary>
        /// Used for testing.. forcibly compile an XPath using the internal/external engine
        /// </summary>
        internal void Compile(bool internalEngine)
        {
            this.EnsureMatcher();
            if (internalEngine)
            {
                this.matcher.CompileForInternal(this.xpath, this.namespaces);
            }
            else
            {
                this.matcher.CompileForExternal(this.xpath, this.namespaces);
            }
        }

        protected internal override IMessageFilterTable<FilterData> CreateFilterTable<FilterData>()
        {
            XPathMessageFilterTable<FilterData> ft = new XPathMessageFilterTable<FilterData>();
            ft.NodeQuota = this.NodeQuota;
            return ft;
        }

        void EnsureMatcher()
        {
            if (this.matcher == XPathMessageFilter.dummyMatcher)
            {
                this.matcher = new XPathQueryMatcher(true);
            }
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return OnGetSchema();
        }

        protected virtual XmlSchema OnGetSchema()
        {
            XmlSchemaComplexType outerType = CreateOuterType();

            XmlSchema schema = new XmlSchema();
            schema.Items.Add(outerType);
            schema.TargetNamespace = XPathMessageFilter.Namespace;

            return schema;
        }

        void Init(string xpath, XmlNamespaceManager namespaces)
        {
            this.xpath = xpath;
            this.namespaces = namespaces;
            this.matcher = XPathMessageFilter.dummyMatcher;
            Compile();
        }

        /// <summary>
        /// Restricts filters to testing only the Xml contained within:
        ///     <envelope><header></header></envelope>
        /// Note: since this method never probes the message body, it should NOT close the message
        /// If the filter probes the message body, then the filter should THROW an Exception. The filter should not return false
        /// This is deliberate - we don't want to produce false positives. 
        /// Example of such a filter that probes the body: /env:Envelope/env:Body = ....
        /// </summary>
        public override bool Match(Message message)
        {
            if (null == message)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            return this.ProcessResult(this.matcher.Match(message, false));
        }

        /// <summary>
        /// Calls messageBuffer.CreateMessage(), then tests whether the message satisfies the criteria of the filter.
        /// MessageFilters will probe
        /// Always make the user choose whether they want to match on the body or not - and be very aware that they made that choice. 
        ///
        /// We deliberately did not use a default of Match(message), with matchOnBody = false
        /// We don't want situations where a developer who used a body filter would wonder why the filter didn't work!
        /// </summary>                
        public override bool Match(MessageBuffer messageBuffer)
        {
            if (null == messageBuffer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }

            return this.ProcessResult(this.matcher.Match(messageBuffer));
        }

        /// <summary>
        /// Evaluates the filter over the given navigator. Returns true if matched
        /// </summary>
        public bool Match(XPathNavigator navigator)
        {
            if (null == navigator)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("navigator");
            }

            return this.ProcessResult(this.matcher.Match(navigator));
        }

        /// <summary>
        /// Evaluates the filter over infosets surfaced via the given seekable navigator
        /// </summary>
        public bool Match(SeekableXPathNavigator navigator)
        {
            if (null == navigator)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("navigator");
            }

            return this.ProcessResult(this.matcher.Match(navigator));
        }

        bool ProcessResult(FilterResult result)
        {
            bool retVal = result.Result;
            this.matcher.ReleaseResult(result);
            return retVal;
        }

        // Assumes that the reader is current parked at the filter's start tag
        void ReadFrom(XmlReader reader, XmlNamespaceManager namespaces)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (!reader.IsStartElement())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("reader", SR.GetString(SR.FilterReaderNotStartElem));
            }

            bool found = false;
            string dialect = null;
            while (reader.MoveToNextAttribute())
            {
                if (QueryDataModel.IsAttribute(reader.NamespaceURI))
                {
                    if (found || reader.LocalName != DialectAttr || reader.NamespaceURI != WSEventingNamespace)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.FilterInvalidAttribute)));
                    }
                    dialect = reader.Value;
                    found = true;
                }
            }
            if (reader.NodeType == XmlNodeType.Attribute)
            {
                reader.MoveToElement();
            }

            if (dialect != null && dialect != XPathDialect)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.FilterInvalidDialect, XPathDialect)));
            }

            bool wasEmpty = reader.IsEmptyElement;
            reader.ReadStartElement();

            if (wasEmpty)
            {
                this.Init(string.Empty, namespaces);
            }
            else
            {
                ReadXPath(reader, namespaces);
                reader.ReadEndElement();
            }
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            OnReadXml(reader);
        }

        protected virtual void OnReadXml(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (!reader.IsStartElement())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("reader", SR.GetString(SR.FilterReaderNotStartElem));
            }

            if (reader.IsEmptyElement)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("reader", SR.GetString(SR.FilterInvalidInner, InnerElem));
            }

            // Pull in the node quota
            string quotaString = null;
            //bool found = false;
            while (reader.MoveToNextAttribute())
            {
                if (QueryDataModel.IsAttribute(reader.NamespaceURI))
                {
                    /*
                    if(found || reader.LocalName != NodeQuotaAttr || reader.NamespaceURI != string.Empty)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.FilterInvalidAttribute)));
                    }
                    */
                    if (reader.LocalName == NodeQuotaAttr && reader.NamespaceURI.Length == 0)
                    {
                        quotaString = reader.Value;
                        //found = true;
                        break;
                    }
                }
            }
            if (reader.NodeType == XmlNodeType.Attribute)
            {
                reader.MoveToElement();
            }
            int quota = quotaString == null ? int.MaxValue : int.Parse(quotaString, NumberFormatInfo.InvariantInfo);
            reader.ReadStartElement();

            reader.MoveToContent();

            if (reader.LocalName != InnerElem)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("reader", SR.GetString(SR.FilterInvalidInner, InnerElem));
            }
            ReadFrom(reader, new XPathMessageContext());

            reader.MoveToContent();
            reader.ReadEndElement();

            this.NodeQuota = quota;
        }

        protected void ReadXPath(XmlReader reader, XmlNamespaceManager namespaces)
        {
            // Pull in the string value
            // Trim should allow an all whitespace xpath to be perceived as a MatchAll.
            string xpath = reader.ReadString().Trim();

            // MatchAll XPathMessageFilter is allowed
            if (xpath.Length != 0)
            {
                // Lex the xpath to find all prefixes used
                XPathLexer lexer = new XPathLexer(xpath, false);

                while (lexer.MoveNext())
                {
                    string prefix = lexer.Token.Prefix;

                    if (prefix.Length > 0)
                    {
                        // Resolve the prefix. If the ns is not found, we'll let the Compiler throw the
                        // proper exception
                        string ns = null;

                        if (null != namespaces)
                        {
                            ns = namespaces.LookupNamespace(prefix);
                        }

                        if (null != ns && ns.Length > 0)
                        {
                            continue;
                        }

                        ns = reader.LookupNamespace(prefix);
                        if (null != ns && ns.Length > 0)
                        {
                            if (null == namespaces)
                            {
                                namespaces = new XPathMessageContext();
                            }

                            namespaces.AddNamespace(prefix, ns);
                        }
                    }
                }
            }

            this.Init(xpath, namespaces);
        }

        /// <summary>        
        /// Purge any cached buffers - reduces working set
        /// </summary>
        public void TrimToSize()
        {
            this.matcher.Trim();
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            OnWriteXml(writer);
        }

        protected virtual void OnWriteXml(XmlWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            // Write the NodeQuota
            writer.WriteAttributeString(NodeQuotaAttr, this.NodeQuota.ToString(NumberFormatInfo.InvariantInfo));

            WriteXPathTo(writer, null, InnerElem, null, true);
        }

        protected void WriteXPath(XmlWriter writer, IXmlNamespaceResolver resolver)
        {
            // Lex the xpath to find all prefixes used
            int startChar = 0;
            int tmp = 0;
            string newXPath = "";
            XPathLexer lexer = new XPathLexer(xpath, false);
            Dictionary<string, string> prefixMap = new Dictionary<string, string>();
            List<string> prefixes = new List<string>();
            while (lexer.MoveNext())
            {
                string nsPrefix = lexer.Token.Prefix;
                string nsNS = resolver.LookupNamespace(nsPrefix);

                // Check if we need to write the namespace
                if (nsPrefix.Length > 0 && (nsNS == null || (nsNS != null && nsNS != this.namespaces.LookupNamespace(nsPrefix))))
                {
                    // Write the previous xpath segment
                    if (this.xpath[tmp] == '$')
                    {
                        newXPath += this.xpath.Substring(startChar, tmp - startChar + 1);
                        startChar = tmp + 1;
                    }
                    else
                    {
                        newXPath += this.xpath.Substring(startChar, tmp - startChar);
                        startChar = tmp;
                    }

                    // Check if we need a new prefix
                    if (!prefixMap.ContainsKey(nsPrefix))
                    {
                        prefixes.Add(nsPrefix);
                        if (nsNS != null)
                        {
                            string newPrefix = nsPrefix;
                            int i = 0;
                            while (resolver.LookupNamespace(newPrefix) != null || this.namespaces.LookupNamespace(newPrefix) != null)
                            {
                                newPrefix = newPrefix + i.ToString(NumberFormatInfo.InvariantInfo);
                                ++i;
                            }
                            prefixMap.Add(nsPrefix, newPrefix);
                        }
                        else
                        {
                            prefixMap.Add(nsPrefix, nsPrefix);
                        }
                    }

                    // Write the new prefix
                    newXPath += prefixMap[nsPrefix];

                    // Update the xpath marker
                    startChar += nsPrefix.Length;
                }
                tmp = lexer.FirstTokenChar;
            }
            newXPath += this.xpath.Substring(startChar);    // Consume the remainder of the xpath

            // Write the namespaces
            for (int i = 0; i < prefixes.Count; ++i)
            {
                string prefix = prefixes[i];
                writer.WriteAttributeString("xmlns", prefixMap[prefix], null, this.namespaces.LookupNamespace(prefix));
            }

            // Write the XPath
            writer.WriteString(newXPath);
        }

        /// <summary>
        ///  Writes the Xml XPath element with a specified Xml writer.
        /// </summary>
        /// <param name='writer'>The writer used to write the filter.</param>
        /// <param name='prefix'>The namespace prefix of the XPath Xml element.</param>
        /// <param name='localName'><para> The local name of the XPath Xml element.</para></param>
        /// <param name='ns'>The namespace URI to associate with the Xml element.</param>
        /// <param name='writeNamespaces'>if namespaces should be serialized out separately as attributes;  if not.</param>
        public void WriteXPathTo(XmlWriter writer, string prefix, string localName, string ns, bool writeNamespaces)
        {
            if (null == writer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (null == localName)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            }

            if (localName.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("localName", SR.GetString(SR.FilterEmptyString));
            }

            if (null == prefix)
            {
                prefix = string.Empty;
            }

            if (null == ns)
            {
                ns = string.Empty;
            }

            // Write out the xpath tag
            writer.WriteStartElement(prefix, localName, ns);
            XmlNamespaceManager resolver = new XmlNamespaceManager(new NameTable());
            if (!writeNamespaces)
            {
                foreach (string pre in this.namespaces)
                {
                    if (pre != XmlP && pre != XmlnsP)
                    {
                        resolver.AddNamespace(pre, this.namespaces.LookupNamespace(pre));
                    }
                }
            }
            resolver.AddNamespace(prefix, ns);
            WriteXPath(writer, resolver);
            /*
            if(writeNamespaces)
            {
                if(this.namespaces.LookupNamespace(prefix) != ns)
                {
                    XmlNamespaceManager resolver = new XmlNamespaceManager(new NameTable());
                    foreach(string pre in this.namespaces)
                    {
                        if(pre != XmlP && pre != XmlnsP)
                        {
                            //resolver.AddNamespace(pre, this.namespaces.LookupNamespace(pre));
                        }
                    }
                    
                    resolver.AddNamespace(prefix, ns);
                    WriteXPath(writer, resolver);
                }
                else
                {
                    WriteXPath(writer, this.namespaces);
                }
            }
            else
            {
                writer.WriteAttributeString(NodeQuotaAttr, this.NodeQuota.ToString());
                writer.WriteString(this.xpath);
            }
            */
            /*
            if (writeNamespaces && null != this.namespaces)
            {
                // Lex the xpath to find all prefixes used
                System.Collections.Generic.List<string> prefixes = new System.Collections.Generic.List<string>();
                XPathLexer lexer = new XPathLexer(xpath, false);
                while (lexer.MoveNext())
                {
                    string nsPrefix = lexer.Token.Prefix;

                    if (nsPrefix.Length > 0 && !prefixes.Contains(nsPrefix))
                    {
                        prefixes.Add(nsPrefix);
                    }
                }
                
                // Write all the used prefixes
                for(int i = 0; i < prefixes.Count; ++i)
                {
                    string nsPrefix = prefixes[i];
                    string nsScope = this.namespaces.LookupNamespace(nsPrefix);
                    if (null != nsScope && nsScope.Length > 0 && !nsPrefix.StartsWith("xml"))
                    {
                        writer.WriteAttributeString("xmlns", nsPrefix, null, nsScope);
                    }
                }
            }

            // Write the NodeQuota
            writer.WriteAttributeString(NodeQuotaAttr, this.NodeQuota.ToString());

            writer.WriteString(this.xpath);
            */

            // Finish up!
            writer.WriteEndElement();
        }
    }
}
