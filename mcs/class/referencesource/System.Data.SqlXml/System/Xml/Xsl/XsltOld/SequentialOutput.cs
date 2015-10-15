//------------------------------------------------------------------------------
// <copyright file="SequentialOutput.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.XsltOld {
    using Res = System.Xml.Utils.Res;
    using System;
    using System.Diagnostics;
    using System.Xml;
    using System.Text;
    using System.Collections;
    using System.Globalization;
    
    internal abstract class SequentialOutput : RecordOutput {
        private const char   s_Colon                = ':';
        private const char   s_GreaterThan          = '>';
        private const char   s_LessThan             = '<';
        private const char   s_Space                = ' ';
        private const char   s_Quote                = '\"';
        private const char   s_Semicolon            = ';';
        private const char   s_NewLine              = '\n';
        private const char   s_Return               = '\r';
        private const char   s_Ampersand            = '&';
        private const string s_LessThanQuestion     = "<?";
        private const string s_QuestionGreaterThan  = "?>";
        private const string s_LessThanSlash        = "</";
        private const string s_SlashGreaterThan     = " />";
        private const string s_EqualQuote           = "=\"";
        private const string s_DocType              = "<!DOCTYPE ";
        private const string s_CommentBegin         = "<!--";
        private const string s_CommentEnd           = "-->";
        private const string s_CDataBegin           = "<![CDATA[";
        private const string s_CDataEnd             = "]]>";
        private const string s_VersionAll           = " version=\"1.0\"";
        private const string s_Standalone           = " standalone=\"";
        private const string s_EncodingStart        = " encoding=\"";
        private const string s_Public               = "PUBLIC ";
        private const string s_System               = "SYSTEM ";
        private const string s_Html                 = "html";
        private const string s_QuoteSpace           = "\" ";
        private const string s_CDataSplit           = "]]]]><![CDATA[>";

        private const string s_EnLessThan           = "&lt;";
        private const string s_EnGreaterThan        = "&gt;";
        private const string s_EnAmpersand          = "&amp;";
        private const string s_EnQuote              = "&quot;";
        private const string s_EnNewLine            = "&#xA;";
        private const string s_EnReturn             = "&#xD;";

        private const string s_EndOfLine            = "\r\n";

        static char[]   s_TextValueFind    = new char[]   {s_Ampersand, s_GreaterThan, s_LessThan};
        static string[] s_TextValueReplace = new string[] {s_EnAmpersand , s_EnGreaterThan , s_EnLessThan };

        static char[]   s_XmlAttributeValueFind    = new char[]   {s_Ampersand, s_GreaterThan, s_LessThan, s_Quote, s_NewLine, s_Return};
        static string[] s_XmlAttributeValueReplace = new string[] {s_EnAmpersand , s_EnGreaterThan , s_EnLessThan , s_EnQuote , s_EnNewLine , s_EnReturn };

        // Instance members
        private     Processor processor;
        protected   Encoding  encoding;
        private     ArrayList outputCache;
        private     bool      firstLine          = true;
        private     bool      secondRoot;

        // Cached Output propertes:
        private XsltOutput output;
        private bool       isHtmlOutput;
        private bool       isXmlOutput;
        private Hashtable  cdataElements;
        private bool       indentOutput;
        private bool       outputDoctype;
        private bool       outputXmlDecl;
        private bool       omitXmlDeclCalled;

        // Uri Escaping:
        private byte[]     byteBuffer;
        private Encoding   utf8Encoding;

        XmlCharType xmlCharType = XmlCharType.Instance;

        private void CacheOuptutProps(XsltOutput output) {
            this.output        = output;
            this.isXmlOutput   = this.output.Method == XsltOutput.OutputMethod.Xml;
            this.isHtmlOutput  = this.output.Method == XsltOutput.OutputMethod.Html;
            this.cdataElements = this.output.CDataElements;
            this.indentOutput  = this.output.Indent;
            this.outputDoctype = this.output.DoctypeSystem != null || (this.isHtmlOutput && this.output.DoctypePublic != null);
            this.outputXmlDecl = this.isXmlOutput && ! this.output.OmitXmlDeclaration && ! this.omitXmlDeclCalled;
        }

        //
        // Constructor
        //
        internal SequentialOutput(Processor processor) {
            this.processor = processor;
            CacheOuptutProps(processor.Output);
        }

        public void OmitXmlDecl() {
            this.omitXmlDeclCalled = true;
            this.outputXmlDecl = false;
        }

        //
        // Particular outputs
        //
        void WriteStartElement(RecordBuilder record) {
            Debug.Assert(record.MainNode.NodeType == XmlNodeType.Element);
            BuilderInfo mainNode = record.MainNode;
            HtmlElementProps htmlProps = null;
            if (this.isHtmlOutput) {
                if (mainNode.Prefix.Length == 0) {
                    htmlProps = mainNode.htmlProps;
                    if (htmlProps == null && mainNode.search) {
                        htmlProps = HtmlElementProps.GetProps(mainNode.LocalName);
                    }
                    record.Manager.CurrentElementScope.HtmlElementProps = htmlProps;
                    mainNode.IsEmptyTag = false;
                }
            }
            else if (this.isXmlOutput) {
                if (mainNode.Depth == 0) {
                    if(
                        secondRoot && (
                            output.DoctypeSystem != null ||
                            output.Standalone
                        )
                    ) {
                        throw XsltException.Create(Res.Xslt_MultipleRoots);
                    }
                    secondRoot = true;
                }                
            }

            if (this.outputDoctype) {
                WriteDoctype(mainNode);
                this.outputDoctype = false;
            }

            if (this.cdataElements != null && this.cdataElements.Contains(new XmlQualifiedName(mainNode.LocalName, mainNode.NamespaceURI)) && this.isXmlOutput) {
                record.Manager.CurrentElementScope.ToCData = true;
            }

            Indent(record);
            Write(s_LessThan);
            WriteName(mainNode.Prefix, mainNode.LocalName);

            WriteAttributes(record.AttributeList, record.AttributeCount, htmlProps);


            if (mainNode.IsEmptyTag) {
                Debug.Assert(! this.isHtmlOutput || mainNode.Prefix != null, "Html can't have abreviated elements");
                Write(s_SlashGreaterThan);
            }
            else {
                Write(s_GreaterThan);
            }

            if(htmlProps != null && htmlProps.Head) {
                mainNode.Depth ++;
                Indent(record);
                mainNode.Depth --;
                Write("<META http-equiv=\"Content-Type\" content=\"");
                Write(this.output.MediaType);
                Write("; charset=");
                Write(this.encoding.WebName);
                Write("\">");
            }
        }

        void WriteTextNode(RecordBuilder record) {
            BuilderInfo mainNode = record.MainNode;
            OutputScope scope = record.Manager.CurrentElementScope;

            scope.Mixed = true;

            if(scope.HtmlElementProps != null && scope.HtmlElementProps.NoEntities) {
                // script or stile
                Write(mainNode.Value);
            }
            else if (scope.ToCData) {
                WriteCDataSection(mainNode.Value);
            }
            else {
                WriteTextNode(mainNode);
            }
        }

        void WriteTextNode(BuilderInfo node) {
            for (int i = 0; i < node.TextInfoCount; i ++) {
                string text = node.TextInfo[i];
                if (text == null) { // disableEscaping marker
                    i++;
                    Debug.Assert(i < node.TextInfoCount, "disableEscaping marker can't be last TextInfo record");
                    Write(node.TextInfo[i]);
                } else {
                    WriteWithReplace(text, s_TextValueFind, s_TextValueReplace);
                }
            }
        }

        void WriteCDataSection(string value) {
            Write(s_CDataBegin);
            WriteCData(value);
            Write(s_CDataEnd);
        }

        void WriteDoctype(BuilderInfo mainNode) {
            Debug.Assert(this.outputDoctype == true, "It supposed to check this condition before actual call");
            Debug.Assert(this.output.DoctypeSystem != null || (this.isHtmlOutput && this.output.DoctypePublic != null), "We set outputDoctype == true only if");
            Indent(0);
            Write(s_DocType);
            if (this.isXmlOutput) {
                WriteName(mainNode.Prefix, mainNode.LocalName);
            }
            else {
                WriteName(string.Empty, "html");
            }
            Write(s_Space);
            if (output.DoctypePublic != null) {
                Write(s_Public);
                Write(s_Quote);
                Write(output.DoctypePublic);
                Write(s_QuoteSpace);
            }
            else {
                Write(s_System);
            }
            if (output.DoctypeSystem != null) {
                Write(s_Quote);
                Write(output.DoctypeSystem);
                Write(s_Quote);
            }
            Write(s_GreaterThan);
        }

        void WriteXmlDeclaration() {
            Debug.Assert(this.outputXmlDecl == true, "It supposed to check this condition before actual call");
            Debug.Assert(this.isXmlOutput && ! this.output.OmitXmlDeclaration, "We set outputXmlDecl == true only if");
            this.outputXmlDecl = false;

            Indent(0);
            Write(s_LessThanQuestion);
            WriteName(string.Empty, "xml");
            Write(s_VersionAll);
            if (this.encoding != null) {
                Write(s_EncodingStart);
                Write(this.encoding.WebName);
                Write(s_Quote);
            }
            if (output.HasStandalone) {
                Write(s_Standalone);
                Write(output.Standalone ? "yes" : "no");
                Write(s_Quote);
            }
            Write(s_QuestionGreaterThan);
        }

        void WriteProcessingInstruction(RecordBuilder record) {
            Indent(record);
            WriteProcessingInstruction(record.MainNode);
        }

        void WriteProcessingInstruction(BuilderInfo node) {
            Write(s_LessThanQuestion);
            WriteName(node.Prefix, node.LocalName);
            Write(s_Space);
            Write(node.Value);

            if(this.isHtmlOutput) {
                Write(s_GreaterThan);
            }
            else {
                Write(s_QuestionGreaterThan);
            }
        }

        void WriteEndElement(RecordBuilder record) {
            BuilderInfo node = record.MainNode;
            HtmlElementProps htmlProps = record.Manager.CurrentElementScope.HtmlElementProps;

            if(htmlProps != null && htmlProps.Empty) {
                return;
            }

            Indent(record);
            Write(s_LessThanSlash);
            WriteName(record.MainNode.Prefix, record.MainNode.LocalName);
            Write(s_GreaterThan);
        }

        //
        // RecordOutput interface method implementation
        //

        public Processor.OutputResult RecordDone(RecordBuilder record) {
            if (output.Method == XsltOutput.OutputMethod.Unknown) {
                if (! DecideDefaultOutput(record.MainNode)) {
                    CacheRecord(record);
                }
                else {
                    OutputCachedRecords();
                    OutputRecord(record);    
                }
            }
            else {
                OutputRecord(record);
            }

            record.Reset();
            return Processor.OutputResult.Continue;
        }

        public void TheEnd() {
            OutputCachedRecords();
            Close();
        }

        private bool DecideDefaultOutput(BuilderInfo node) {
            XsltOutput.OutputMethod method = XsltOutput.OutputMethod.Xml;
            switch (node.NodeType) {
            case XmlNodeType.Element:
                if (node.NamespaceURI.Length == 0 && String.Compare("html", node.LocalName, StringComparison.OrdinalIgnoreCase) == 0) {
                    method = XsltOutput.OutputMethod.Html;
                }
                break;
            case XmlNodeType.Text:
            case XmlNodeType.Whitespace:
            case XmlNodeType.SignificantWhitespace:
                if (xmlCharType.IsOnlyWhitespace(node.Value)) {
                    return false;
                }
                method = XsltOutput.OutputMethod.Xml;
                break;
            default :
                return false;
            }
            if(this.processor.SetDefaultOutput(method)) {
                CacheOuptutProps(processor.Output);                
            }
            return true;
        }

        private void CacheRecord(RecordBuilder record) {
            if (this.outputCache == null) {
                this.outputCache = new ArrayList();
            }

            this.outputCache.Add(record.MainNode.Clone());
        }

        private void OutputCachedRecords() {
            if (this.outputCache == null) {
                return;
            }

            for(int record = 0; record < this.outputCache.Count; record ++) {
                Debug.Assert(this.outputCache[record] is BuilderInfo);
                BuilderInfo info = (BuilderInfo) this.outputCache[record];

                OutputRecord(info);
            }

            this.outputCache = null;
        }

        private void OutputRecord(RecordBuilder record) {
            BuilderInfo mainNode = record.MainNode;

            if(this.outputXmlDecl) {
                WriteXmlDeclaration();
            }

            switch (mainNode.NodeType) {
            case XmlNodeType.Element:
                WriteStartElement(record);
                break;
            case XmlNodeType.Text:
            case XmlNodeType.Whitespace:
            case XmlNodeType.SignificantWhitespace:
                WriteTextNode(record);
                break;
            case XmlNodeType.CDATA:
                Debug.Fail("Should never get here");
                break;
            case XmlNodeType.EntityReference:
                Write(s_Ampersand);
                WriteName(mainNode.Prefix, mainNode.LocalName);
                Write(s_Semicolon);
                break;
            case XmlNodeType.ProcessingInstruction:
                WriteProcessingInstruction(record);
                break;
            case XmlNodeType.Comment:
                Indent(record);
                Write(s_CommentBegin);
                Write(mainNode.Value);
                Write(s_CommentEnd);
                break;
            case XmlNodeType.Document:
                break;
            case XmlNodeType.DocumentType:
                Write(mainNode.Value);
                break;
            case XmlNodeType.EndElement:
                WriteEndElement(record);
                break;
            default:
                break;
            }
        }

        private void OutputRecord(BuilderInfo node) {
            if(this.outputXmlDecl) {
                WriteXmlDeclaration();
            }

            Indent(0); // we can have only top level stuff here

            switch (node.NodeType) {
            case XmlNodeType.Element:
                Debug.Fail("Should never get here");
                break;
            case XmlNodeType.Text:
            case XmlNodeType.Whitespace:
            case XmlNodeType.SignificantWhitespace:
                WriteTextNode(node);
                break;
            case XmlNodeType.CDATA:
                Debug.Fail("Should never get here");
                break;
            case XmlNodeType.EntityReference:
                Write(s_Ampersand);
                WriteName(node.Prefix, node.LocalName);
                Write(s_Semicolon);
                break;
            case XmlNodeType.ProcessingInstruction:
                WriteProcessingInstruction(node);
                break;
            case XmlNodeType.Comment:
                Write(s_CommentBegin);
                Write(node.Value);
                Write(s_CommentEnd);
                break;
            case XmlNodeType.Document:
                break;
            case XmlNodeType.DocumentType:
                Write(node.Value);
                break;
            case XmlNodeType.EndElement:
                Debug.Fail("Should never get here");
                break;
            default:
                break;
            }
        }

        //
        // Internal helpers
        //

        private void WriteName(string prefix, string name) {
            if (prefix != null && prefix.Length > 0) {
                Write(prefix);
                if (name != null && name.Length > 0) {
                    Write(s_Colon);
                }
                else {
                    return;
                }
            }
            Write(name);
        }

        private void WriteXmlAttributeValue(string value) {
            Debug.Assert(value != null);
            WriteWithReplace(value, s_XmlAttributeValueFind, s_XmlAttributeValueReplace);
        }

        private void WriteHtmlAttributeValue(string value) {
            Debug.Assert(value != null);

            int length = value.Length;
            int i = 0;
            while(i < length) {
                char ch = value[i];
                i ++;
                switch (ch) {
                case '&':
                    if(i != length && value[i] == '{') { // &{ hasn't to be encoded in HTML output.
                        Write(ch);
                    }
                    else {
                        Write(s_EnAmpersand);
                    }
                    break;
                case '"':
                    Write(s_EnQuote);
                    break;
                default:
                    Write(ch);
                    break;
                }
            }
        }

        private void WriteHtmlUri(string value) {
            Debug.Assert(value != null);
            Debug.Assert(this.isHtmlOutput);

            int length = value.Length;
            int i = 0;
            while(i < length) {
                char ch = value[i];
                i ++;
                switch (ch) {
                case '&':
                    if(i != length && value[i] == '{') { // &{ hasn't to be encoded in HTML output.
                        Write(ch);
                    }
                    else {
                        Write(s_EnAmpersand);
                    }
                    break;
                case '"':
                    Write(s_EnQuote);
                    break;
                case '\n':
                    Write(s_EnNewLine);
                    break;
                case '\r':
                    Write(s_EnReturn);
                    break;
                default:
                    if(127 < ch) {
                        if (this.utf8Encoding == null) {
                            this.utf8Encoding = Encoding.UTF8;
                            this.byteBuffer   = new byte[utf8Encoding.GetMaxByteCount(1)];
                        }
                        int bytes = this.utf8Encoding.GetBytes(value, i - 1, 1, this.byteBuffer, 0);
                        for(int j = 0; j < bytes; j ++) {
                            Write("%");
                            Write(((uint)this.byteBuffer[j]).ToString("X2", CultureInfo.InvariantCulture));
                        }
                    }
                    else {
                        Write(ch);
                    }
                    break;
                }
            }
        }

        private void WriteWithReplace(string value, char[] find, string[] replace) {
            Debug.Assert(value != null);
            Debug.Assert(find.Length == replace.Length);

            int length = value.Length;
            int pos = 0;

            while(pos < length) {
                int newPos = value.IndexOfAny(find, pos);
                if (newPos == -1) {
                    break; // not found;
                }
                // output clean leading part of the string
                while (pos < newPos) {
                    Write(value[pos]);
                    pos ++;
                }
                // output replacement
                char badChar = value[pos];
                int i;
                for(i = find.Length - 1; 0 <= i; i --) {
                    if(find[i] == badChar) {
                        Write(replace[i]);
                        break;
                    }
                }
                Debug.Assert(0 <= i, "find char wasn't realy find");
                pos ++;
            }

            // output rest of the string
            if(pos == 0) {
                Write(value);
            }
            else {
                while(pos < length) {
                    Write(value[pos]);
                    pos ++;
                }
            }
        }

        private void WriteCData(string value) {
            Debug.Assert(value != null);
            Write(value.Replace(s_CDataEnd, s_CDataSplit));
        }

        private void WriteAttributes(ArrayList list, int count, HtmlElementProps htmlElementsProps) {
            Debug.Assert(count <= list.Count);
            for (int attrib = 0; attrib < count; attrib ++) {
                Debug.Assert(list[attrib] is BuilderInfo);
                BuilderInfo attribute = (BuilderInfo) list[attrib];
                string attrValue = attribute.Value;
                bool abr = false, uri = false; {
                    if(htmlElementsProps != null && attribute.Prefix.Length == 0) {
                        HtmlAttributeProps htmlAttrProps = attribute.htmlAttrProps;
                        if (htmlAttrProps == null && attribute.search) {
                            htmlAttrProps = HtmlAttributeProps.GetProps(attribute.LocalName);
                        }
                        if(htmlAttrProps != null) {
                            abr = htmlElementsProps.AbrParent  && htmlAttrProps.Abr;
                            uri = htmlElementsProps.UriParent  && ( htmlAttrProps.Uri ||
								  htmlElementsProps.NameParent && htmlAttrProps.Name
							);
                        }
                    }
                }
                Write(s_Space);
                WriteName(attribute.Prefix, attribute.LocalName);
                if(abr && 0 == string.Compare(attribute.LocalName, attrValue, StringComparison.OrdinalIgnoreCase) ) {
                    // Since the name of the attribute = the value of the attribute, 
                    // this is a boolean attribute whose value should be suppressed
                    continue; 
                }
                Write(s_EqualQuote);
                if(uri) {
                    WriteHtmlUri(attrValue);
                }
                else if(this.isHtmlOutput) {
                    WriteHtmlAttributeValue(attrValue);
                }
                else {
                    WriteXmlAttributeValue(attrValue);
                }
                Write(s_Quote);
            }
        }

        void Indent(RecordBuilder record) {
            if (! record.Manager.CurrentElementScope.Mixed) {
                Indent(record.MainNode.Depth);
            }
        }

        void Indent(int depth) {
            if(this.firstLine) {
                if (this.indentOutput) {
                    this.firstLine = false;
                }
                return;    // preven leading CRLF
            }
            Write(s_EndOfLine);
            for (int i = 2 * depth; 0 < i; i--) {
                Write(" ");
            }
        }

        //
        // Abstract methods
        internal abstract void Write(char outputChar);
        internal abstract void Write(string outputText);
        internal abstract void Close();
    }
}
