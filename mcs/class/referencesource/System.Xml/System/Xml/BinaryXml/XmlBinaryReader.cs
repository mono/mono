//------------------------------------------------------------------------------
// <copyright file="XmlBinaryWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Schema;

namespace System.Xml {

    internal sealed partial class XmlSqlBinaryReader : XmlReader, IXmlNamespaceResolver {
        internal static readonly Type TypeOfObject = typeof(System.Object);
        internal static readonly Type TypeOfString = typeof(System.String);

        static volatile Type[] TokenTypeMap = null;

        static byte[] XsdKatmaiTimeScaleToValueLengthMap = new byte[8] {
        // length scale
            3, // 0
            3, // 1
            3, // 2
            4, // 3
            4, // 4
            5, // 5
            5, // 6
            5, // 7
        };

        enum ScanState {
            Doc = 0,
            XmlText = 1,
            Attr = 2,
            AttrVal = 3,
            AttrValPseudoValue = 4,
            Init = 5,
            Error = 6,
            EOF = 7,
            Closed = 8
        }

        static ReadState[] ScanState2ReadState = {
            ReadState.Interactive,
            ReadState.Interactive,
            ReadState.Interactive,
            ReadState.Interactive,
            ReadState.Interactive,
            ReadState.Initial,
            ReadState.Error,
            ReadState.EndOfFile,
            ReadState.Closed
        };

        // Note: also used by XmlBinaryWriter
        internal struct QName {
            public string prefix;
            public string localname;
            public string namespaceUri;

            public QName(string prefix, string lname, string nsUri) {
                this.prefix = prefix; this.localname = lname; this.namespaceUri = nsUri;
            }
            public void Set(string prefix, string lname, string nsUri) {
                this.prefix = prefix; this.localname = lname; this.namespaceUri = nsUri;
            }

            public void Clear() {
                this.prefix = this.localname = this.namespaceUri = String.Empty;
            }

            public bool MatchNs(string lname, string nsUri) {
                return lname == this.localname && nsUri == this.namespaceUri;
            }
            public bool MatchPrefix(string prefix, string lname) {
                return lname == this.localname && prefix == this.prefix;
            }

            public void CheckPrefixNS(string prefix, string namespaceUri) {
                if (this.prefix == prefix && this.namespaceUri != namespaceUri)
                    throw new XmlException(Res.XmlBinary_NoRemapPrefix, new String[] { prefix, this.namespaceUri, namespaceUri });
            }

            public override int GetHashCode() {
                return this.prefix.GetHashCode() ^ this.localname.GetHashCode();
            }

            public int GetNSHashCode(SecureStringHasher hasher) {
                return hasher.GetHashCode(this.namespaceUri) ^ hasher.GetHashCode(this.localname);
            }


            public override bool Equals(object other) {
                if (other is QName) {
                    QName that = (QName)other;
                    return this == that;
                }
                return false;
            }

            public override string ToString() {
                if (prefix.Length == 0)
                    return this.localname;
                else
                    return this.prefix + ":" + this.localname;
            }

            public static bool operator ==(QName a, QName b) {
                return ((a.prefix == b.prefix)
                    && (a.localname == b.localname)
                    && (a.namespaceUri == b.namespaceUri));
            }

            public static bool operator !=(QName a, QName b) {
                return !(a == b);
            }
        };

        struct ElemInfo {
            public QName name;
            public string xmlLang;
            public XmlSpace xmlSpace;
            public bool xmlspacePreserve;
            public NamespaceDecl nsdecls;

            public void Set(QName name, bool xmlspacePreserve) {
                this.name = name;
                this.xmlLang = null;
                this.xmlSpace = XmlSpace.None;
                this.xmlspacePreserve = xmlspacePreserve;
            }
            public NamespaceDecl Clear() {
                NamespaceDecl nsdecls = this.nsdecls;
                this.nsdecls = null;
                return nsdecls;
            }
        };

        struct AttrInfo {
            public QName name;
            public string val;
            public int contentPos;
            public int hashCode;
            public int prevHash;

            public void Set(QName n, string v) {
                this.name = n;
                this.val = v;
                this.contentPos = 0;
                this.hashCode = 0;
                this.prevHash = 0;
            }
            public void Set(QName n, int pos) {
                this.name = n;
                this.val = null;
                this.contentPos = pos;
                this.hashCode = 0;
                this.prevHash = 0;
            }

            public void GetLocalnameAndNamespaceUri(out string localname, out string namespaceUri) {
                localname = this.name.localname;
                namespaceUri = this.name.namespaceUri;
            }

            public int GetLocalnameAndNamespaceUriAndHash(SecureStringHasher hasher, out string localname, out string namespaceUri) {
                localname = this.name.localname;
                namespaceUri = this.name.namespaceUri;
                return this.hashCode = this.name.GetNSHashCode(hasher);
            }

            public bool MatchNS(string localname, string namespaceUri) {
                return this.name.MatchNs(localname, namespaceUri);
            }

            public bool MatchHashNS(int hash, string localname, string namespaceUri) {
                return this.hashCode == hash && this.name.MatchNs(localname, namespaceUri);
            }

            public void AdjustPosition(int adj) {
                if (this.contentPos != 0)
                    this.contentPos += adj;
            }
        }

        class NamespaceDecl {
            public string prefix;
            public string uri;
            public NamespaceDecl scopeLink;
            public NamespaceDecl prevLink;
            public int scope;
            public bool implied;

            public NamespaceDecl(string prefix, string nsuri,
                                NamespaceDecl nextInScope, NamespaceDecl prevDecl,
                                int scope, bool implied) {
                this.prefix = prefix;
                this.uri = nsuri;
                this.scopeLink = nextInScope;
                this.prevLink = prevDecl;
                this.scope = scope;
                this.implied = implied;
            }
        }

        // symbol and qname tables
        struct SymbolTables {
            public string[] symtable;
            public int symCount;
            public QName[] qnametable;
            public int qnameCount;
            public void Init() {
                this.symtable = new string[64];
                this.qnametable = new QName[16];
                this.symtable[0] = String.Empty;
                this.symCount = 1;
                this.qnameCount = 1;
            }
        }
        class NestedBinXml {
            public SymbolTables symbolTables;
            public int docState;
            public NestedBinXml next;
            public NestedBinXml(SymbolTables symbolTables, int docState, NestedBinXml next) {
                this.symbolTables = symbolTables;
                this.docState = docState;
                this.next = next;
            }
        }

        // input data
        Stream inStrm;
        byte[] data;
        int pos;
        int mark;
        int end;
        long offset; // how much read and shift out of buffer
        bool eof;
        bool sniffed;
        bool isEmpty; // short-tag element start tag

        int docState; // 0=>auto, 1=>doc/pre-dtd, 2=>doc/pre-elem, 3=>doc/instance -1=>doc/post-elem, 9=>frag

        // symbol and qname tables
        SymbolTables symbolTables;

        XmlNameTable xnt;
        bool xntFromSettings;
        string xml;
        string xmlns;
        string nsxmlns;

        // base uri...
        string baseUri;

        // current parse state
        ScanState state;
        XmlNodeType nodetype;
        BinXmlToken token;
        // current attribute
        int attrIndex;
        // index of current qname
        QName qnameOther;
        // saved qname of element (for MoveToElement)
        QName qnameElement;
        XmlNodeType parentNodeType; // use for MoveToElement()
        // stack of current open element tags
        ElemInfo[] elementStack;
        int elemDepth;
        // current attributes
        AttrInfo[] attributes;
        int[] attrHashTbl;
        int attrCount;
        int posAfterAttrs;
        // xml:space
        bool xmlspacePreserve;
        // position/parse info for current typed token
        int tokLen;
        int tokDataPos;
        bool hasTypedValue;
        System.Type valueType;
        // if it is a simple string value, we cache it
        string stringValue;
        // hashtable of current namespaces
        Dictionary<String, NamespaceDecl> namespaces;
        //Hashtable namespaces;
        // linked list of pushed nametables (to support nested binary-xml documents)
        NestedBinXml prevNameInfo;
        // XmlTextReader to handle embeded text blocks
        XmlReader textXmlReader;
        // close input flag
        bool closeInput;

        bool checkCharacters;
        bool ignoreWhitespace;
        bool ignorePIs;
        bool ignoreComments;
        DtdProcessing dtdProcessing;

        SecureStringHasher hasher;
        XmlCharType xmlCharType;
        Encoding unicode;

        // current version of the protocol
        byte version;

        public XmlSqlBinaryReader(System.IO.Stream stream, byte[] data, int len, string baseUri, bool closeInput, XmlReaderSettings settings) {
            unicode = System.Text.Encoding.Unicode;
            xmlCharType = XmlCharType.Instance;

            this.xnt = settings.NameTable;
            if (this.xnt == null) {
                this.xnt = new NameTable();
                this.xntFromSettings = false;
            }
            else {
                this.xntFromSettings = true;
            }
            this.xml = this.xnt.Add("xml");
            this.xmlns = this.xnt.Add("xmlns");
            this.nsxmlns = this.xnt.Add(XmlReservedNs.NsXmlNs);
            this.baseUri = baseUri;
            this.state = ScanState.Init;
            this.nodetype = XmlNodeType.None;
            this.token = BinXmlToken.Error;
            this.elementStack = new ElemInfo[16];
            //this.elemDepth = 0;
            this.attributes = new AttrInfo[8];
            this.attrHashTbl = new int[8];
            //this.attrCount = 0;
            //this.attrIndex = 0;
            this.symbolTables.Init();
            this.qnameOther.Clear();
            this.qnameElement.Clear();
            this.xmlspacePreserve = false;
            this.hasher = new SecureStringHasher();
            this.namespaces = new Dictionary<String, NamespaceDecl>(hasher);
            AddInitNamespace(String.Empty, String.Empty);
            AddInitNamespace(this.xml, this.xnt.Add(XmlReservedNs.NsXml));
            AddInitNamespace(this.xmlns, this.nsxmlns);
            this.valueType = TypeOfString;
            // init buffer position, etc
            this.inStrm = stream;
            if (data != null) {
                Debug.Assert(len >= 2 && (data[0] == 0xdf && data[1] == 0xff));
                this.data = data;
                this.end = len;
                this.pos = 2;
                this.sniffed = true;
            }
            else {
                this.data = new byte[XmlReader.DefaultBufferSize];
                this.end = stream.Read(this.data, 0, XmlReader.DefaultBufferSize);
                this.pos = 0;
                this.sniffed = false;
            }

            this.mark = -1;
            this.eof = (0 == this.end);
            this.offset = 0;
            this.closeInput = closeInput;
            switch (settings.ConformanceLevel) {
                case ConformanceLevel.Auto:
                    this.docState = 0; break;
                case ConformanceLevel.Fragment:
                    this.docState = 9; break;
                case ConformanceLevel.Document:
                    this.docState = 1; break;
            }
            this.checkCharacters = settings.CheckCharacters;
            this.dtdProcessing = settings.DtdProcessing;
            this.ignoreWhitespace = settings.IgnoreWhitespace;
            this.ignorePIs = settings.IgnoreProcessingInstructions;
            this.ignoreComments = settings.IgnoreComments;

            if (TokenTypeMap == null)
                GenerateTokenTypeMap();
        }

        public override XmlReaderSettings Settings { 
            get {
                XmlReaderSettings settings = new XmlReaderSettings();
                if (xntFromSettings) {
                    settings.NameTable = xnt;
                }
                // 0=>auto, 1=>doc/pre-dtd, 2=>doc/pre-elem, 3=>doc/instance -1=>doc/post-elem, 9=>frag
                switch (this.docState) {
                    case 0:
                        settings.ConformanceLevel = ConformanceLevel.Auto; break;
                    case 9:
                        settings.ConformanceLevel = ConformanceLevel.Fragment; break;
                    default:
                        settings.ConformanceLevel = ConformanceLevel.Document; break;
                }
                settings.CheckCharacters = this.checkCharacters;
                settings.IgnoreWhitespace = this.ignoreWhitespace;
                settings.IgnoreProcessingInstructions = this.ignorePIs;
                settings.IgnoreComments = this.ignoreComments;
                settings.DtdProcessing = this.dtdProcessing;
                settings.CloseInput = this.closeInput;

                settings.ReadOnly = true;
                return settings;
            }
        }

        public override XmlNodeType NodeType {
            get {
                return this.nodetype;
            }
        }

        public override string LocalName {
            get {
                return this.qnameOther.localname;
            }
        }

        public override string NamespaceURI {
            get {
                return this.qnameOther.namespaceUri;
            }
        }

        public override string Prefix {
            get {
                return this.qnameOther.prefix;
            }
        }

        public override bool HasValue {
            get {
                if (ScanState.XmlText == this.state)
                    return this.textXmlReader.HasValue;
                else
                    return XmlReader.HasValueInternal(this.nodetype);
            }
        }

        public override string Value {
            get {
                if (null != this.stringValue)
                    return stringValue;
                switch (this.state) {
                    case ScanState.Doc:
                        switch (this.nodetype) {
                            case XmlNodeType.DocumentType:
                            case XmlNodeType.ProcessingInstruction:
                            case XmlNodeType.Comment:
                                return this.stringValue = GetString(this.tokDataPos, this.tokLen);

                            case XmlNodeType.CDATA:
                                return this.stringValue = CDATAValue();

                            case XmlNodeType.XmlDeclaration:
                                return this.stringValue = XmlDeclValue();

                            case XmlNodeType.Text:
                            case XmlNodeType.Whitespace:
                            case XmlNodeType.SignificantWhitespace:
                                return this.stringValue = ValueAsString(this.token);
                        }
                        break;

                    case ScanState.XmlText:
                        return this.textXmlReader.Value;

                    case ScanState.Attr:
                    case ScanState.AttrValPseudoValue:
                        return this.stringValue = GetAttributeText(this.attrIndex - 1);

                    case ScanState.AttrVal:
                        return this.stringValue = ValueAsString(this.token);
                }
                return String.Empty;
            }
        }

        public override int Depth {
            get {
                int adj = 0;
                switch (this.state) {
                    case ScanState.Doc:
                        if (this.nodetype == XmlNodeType.Element
                            || this.nodetype == XmlNodeType.EndElement)
                            adj = -1;
                        break;

                    case ScanState.XmlText:
                        adj = this.textXmlReader.Depth;
                        break;

                    case ScanState.Attr:
                        if (this.parentNodeType != XmlNodeType.Element)
                            adj = 1;
                        break;
                    case ScanState.AttrVal:
                    case ScanState.AttrValPseudoValue:
                        if (this.parentNodeType != XmlNodeType.Element)
                            adj = 1;
                        adj += 1;
                        break;
                    default:
                        return 0;
                }
                return this.elemDepth + adj;
            }
        }

        public override string BaseURI {
            get {
                return this.baseUri;
            }
        }

        public override bool IsEmptyElement {
            get {
                switch (this.state) {
                    case ScanState.Doc:
                    case ScanState.XmlText:
                        return this.isEmpty;
                    default:
                        return false;
                }
            }
        }

        public override XmlSpace XmlSpace {
            get {
                if (ScanState.XmlText != this.state) {
                    for (int i = this.elemDepth; i >= 0; i--) {
                        XmlSpace xs = this.elementStack[i].xmlSpace;
                        if (xs != XmlSpace.None)
                            return xs;
                    }
                    return XmlSpace.None;
                }
                else {
                    return this.textXmlReader.XmlSpace;
                }
            }
        }

        public override string XmlLang {
            get {
                if (ScanState.XmlText != this.state) {
                    for (int i = this.elemDepth; i >= 0; i--) {
                        string xl = this.elementStack[i].xmlLang;
                        if (null != xl)
                            return xl;
                    }
                    return string.Empty;
                }
                else {
                    return this.textXmlReader.XmlLang;
                }
            }
        }

        public override System.Type ValueType {
            get {
                return this.valueType;
            }
        }

        public override int AttributeCount {
            get {
                switch (this.state) {
                    case ScanState.Doc:
                    // for compatibility with XmlTextReader
                    //  we return the attribute count for the element
                    //  when positioned on an attribute under that
                    //  element...
                    case ScanState.Attr:
                    case ScanState.AttrVal:
                    case ScanState.AttrValPseudoValue:
                        return this.attrCount;
                    case ScanState.XmlText:
                        return this.textXmlReader.AttributeCount;
                    default:
                        return 0;
                }
            }
        }

        public override string GetAttribute(string name, string ns) {
            if (ScanState.XmlText == this.state) {
                return this.textXmlReader.GetAttribute(name, ns);
            }
            else {
                if (null == name)
                    throw new ArgumentNullException("name");
                if (null == ns)
                    ns = String.Empty;
                int index = LocateAttribute(name, ns);
                if (-1 == index)
                    return null;
                return GetAttribute(index);
            }
        }

        public override string GetAttribute(string name) {
            if (ScanState.XmlText == this.state) {
                return this.textXmlReader.GetAttribute(name);
            }
            else {
                int index = LocateAttribute(name);
                if (-1 == index)
                    return null;
                return GetAttribute(index);
            }
        }

        public override string GetAttribute(int i) {
            if (ScanState.XmlText == this.state) {
                return this.textXmlReader.GetAttribute(i);
            }
            else {
                if (i < 0 || i >= this.attrCount)
                    throw new ArgumentOutOfRangeException("i");
                return GetAttributeText(i);
            }
        }

        public override bool MoveToAttribute(string name, string ns) {
            if (ScanState.XmlText == this.state) {
                return UpdateFromTextReader(this.textXmlReader.MoveToAttribute(name, ns));
            }
            else {
                if (null == name)
                    throw new ArgumentNullException("name");
                if (null == ns)
                    ns = String.Empty;
                int index = LocateAttribute(name, ns);
                if ((-1 != index) && (this.state < ScanState.Init)) {
                    PositionOnAttribute(index + 1);
                    return true;
                }
                return false;
            }
        }

        public override bool MoveToAttribute(string name) {
            if (ScanState.XmlText == this.state) {
                return UpdateFromTextReader(this.textXmlReader.MoveToAttribute(name));
            }
            else {
                int index = LocateAttribute(name);
                if ((-1 != index) && (this.state < ScanState.Init)) {
                    PositionOnAttribute(index + 1);
                    return true;
                }
                return false;
            }
        }

        public override void MoveToAttribute(int i) {
            if (ScanState.XmlText == this.state) {
                this.textXmlReader.MoveToAttribute(i);
                UpdateFromTextReader(true);
            }
            else {
                if (i < 0 || i >= this.attrCount) {
                    throw new ArgumentOutOfRangeException("i");
                }
                PositionOnAttribute(i + 1);
            }
        }

        public override bool MoveToFirstAttribute() {
            if (ScanState.XmlText == this.state) {
                return UpdateFromTextReader(this.textXmlReader.MoveToFirstAttribute());
            }
            else {
                if (this.attrCount == 0)
                    return false;
                // set up for walking attributes
                PositionOnAttribute(1);
                return true;
            }
        }

        public override bool MoveToNextAttribute() {
            switch (this.state) {
                case ScanState.Doc:
                case ScanState.Attr:
                case ScanState.AttrVal:
                case ScanState.AttrValPseudoValue:
                    if (this.attrIndex >= this.attrCount)
                        return false;
                    PositionOnAttribute(++this.attrIndex);
                    return true;

                case ScanState.XmlText:
                    return UpdateFromTextReader(this.textXmlReader.MoveToNextAttribute());

                default:
                    return false;
            }
        }

        public override bool MoveToElement() {
            switch (this.state) {
                case ScanState.Attr:
                case ScanState.AttrVal:
                case ScanState.AttrValPseudoValue:
                    this.attrIndex = 0;
                    this.qnameOther = this.qnameElement;
                    if (XmlNodeType.Element == this.parentNodeType)
                        this.token = BinXmlToken.Element;
                    else if (XmlNodeType.XmlDeclaration == this.parentNodeType)
                        this.token = BinXmlToken.XmlDecl;
                    else if (XmlNodeType.DocumentType == this.parentNodeType)
                        this.token = BinXmlToken.DocType;
                    else
                        Debug.Fail("Unexpected parent NodeType");
                    this.nodetype = this.parentNodeType;
                    this.state = ScanState.Doc;
                    this.pos = this.posAfterAttrs;
                    this.stringValue = null;
                    return true;

                case ScanState.XmlText:
                    return UpdateFromTextReader(this.textXmlReader.MoveToElement());

                default:
                    return false;
            }
        }

        public override bool EOF {
            get {
                return this.state == ScanState.EOF;
            }
        }

        public override bool ReadAttributeValue() {
            this.stringValue = null;
            switch (this.state) {
                case ScanState.Attr:
                    if (null == this.attributes[this.attrIndex - 1].val) {
                        this.pos = this.attributes[this.attrIndex - 1].contentPos;
                        BinXmlToken tok = RescanNextToken();
                        if (BinXmlToken.Attr == tok || BinXmlToken.EndAttrs == tok) {
                            return false;
                        }
                        this.token = tok;
                        ReScanOverValue(tok);
                        this.valueType = GetValueType(tok);
                        this.state = ScanState.AttrVal;
                    }
                    else {
                        this.token = BinXmlToken.Error;
                        this.valueType = TypeOfString;
                        this.state = ScanState.AttrValPseudoValue;
                    }
                    this.qnameOther.Clear();
                    this.nodetype = XmlNodeType.Text;
                    return true;

                case ScanState.AttrVal:
                    return false;

                case ScanState.XmlText:
                    return UpdateFromTextReader(this.textXmlReader.ReadAttributeValue());

                default:
                    return false;
            }
        }

        public override void Close() {
            this.state = ScanState.Closed;
            this.nodetype = XmlNodeType.None;
            this.token = BinXmlToken.Error;
            this.stringValue = null;
            if (null != this.textXmlReader) {
                this.textXmlReader.Close();
                this.textXmlReader = null;
            }
            if (null != this.inStrm && closeInput)
                this.inStrm.Close();
            this.inStrm = null;
            this.pos = this.end = 0;
        }

        public override XmlNameTable NameTable {
            get {
                return this.xnt;
            }
        }

        public override string LookupNamespace(string prefix) {
            if (ScanState.XmlText == this.state)
                return this.textXmlReader.LookupNamespace(prefix);
            NamespaceDecl decl;
            if (prefix != null && this.namespaces.TryGetValue(prefix, out decl)) {
                Debug.Assert(decl != null);
                return decl.uri;
            }
            return null;
        }

        public override void ResolveEntity() {
            throw new NotSupportedException();
        }

        public override ReadState ReadState {
            get {
                return ScanState2ReadState[(int)this.state];
            }
        }

        public override bool Read() {
            try {
                switch (this.state) {
                    case ScanState.Init:
                        return ReadInit(false);

                    case ScanState.Doc:
                        return ReadDoc();

                    case ScanState.XmlText:
                        if (this.textXmlReader.Read()) {
                            return UpdateFromTextReader(true);
                        }
                        this.state = ScanState.Doc;
                        this.nodetype = XmlNodeType.None;
                        this.isEmpty = false;
                        goto case ScanState.Doc;

                    case ScanState.Attr:
                    case ScanState.AttrVal:
                    case ScanState.AttrValPseudoValue:
                        // clean up attribute stuff...
                        MoveToElement();
                        goto case ScanState.Doc;

                    default:
                        return false;
                }
            }
            catch (OverflowException e) {
                this.state = ScanState.Error;
                throw new XmlException(e.Message, e);
            }
            catch {
                this.state = ScanState.Error;
                throw;
            }
        }

        // Use default implementation of and ReadContentAsString and ReadElementContentAsString
        // (there is no benefit to providing a custom version)
        // public override bool ReadElementContentAsString( string localName, string namespaceURI )
        // public override bool ReadElementContentAsString()
        // public override bool ReadContentAsString()

        // Do setup work for ReadContentAsXXX methods
        // If ready for a typed value read, returns true, otherwise returns
        //  false to indicate caller should ball back to XmlReader.ReadContentAsXXX
        // Special-Case: returns true and positioned on Element or EndElem to force parse of empty-string
        private bool SetupContentAsXXX(string name) {
            if (!CanReadContentAs(this.NodeType)) {
                throw CreateReadContentAsException(name);
            }
            switch (this.state) {
                case ScanState.Doc:
                    if (this.NodeType == XmlNodeType.EndElement)
                        return true;
                    if (this.NodeType == XmlNodeType.ProcessingInstruction || this.NodeType == XmlNodeType.Comment) {
                        while (Read() && (this.NodeType == XmlNodeType.ProcessingInstruction || this.NodeType == XmlNodeType.Comment))
                            ;
                        if (this.NodeType == XmlNodeType.EndElement)
                            return true;
                    }
                    if (this.hasTypedValue) {
                        return true;
                    }
                    break;
                case ScanState.Attr:
                    this.pos = this.attributes[this.attrIndex - 1].contentPos;
                    BinXmlToken token = RescanNextToken();
                    if (BinXmlToken.Attr == token || BinXmlToken.EndAttrs == token)
                        break;
                    this.token = token;
                    ReScanOverValue(token);
                    return true;
                case ScanState.AttrVal:
                    return true;
                default:
                    break;
            }
            return false;
        }

        private int FinishContentAsXXX(int origPos) {
            if (this.state == ScanState.Doc) {
                // if we are already on a tag, then don't move
                if (this.NodeType != XmlNodeType.Element && this.NodeType != XmlNodeType.EndElement) {
                // advance over PIs and Comments
                Loop:
                    if (Read()) {
                        switch (this.NodeType) {
                            case XmlNodeType.ProcessingInstruction:
                            case XmlNodeType.Comment:
                                goto Loop;

                            case XmlNodeType.Element:
                            case XmlNodeType.EndElement:
                                break;

                            default:
                                throw ThrowNotSupported(Res.XmlBinary_ListsOfValuesNotSupported);
                        }
                    }
                }
                return this.pos;
            }
            return origPos;
        }

        public override bool ReadContentAsBoolean() {
            int origPos = this.pos;
            bool value = false;
            try {
                if (SetupContentAsXXX("ReadContentAsBoolean")) {
                    try {
                        switch (this.token) {
                            case BinXmlToken.XSD_BOOLEAN:
                                value = 0 != this.data[this.tokDataPos];
                                break;

                            case BinXmlToken.SQL_BIT:
                            case BinXmlToken.SQL_TINYINT:
                            case BinXmlToken.SQL_SMALLINT:
                            case BinXmlToken.SQL_INT:
                            case BinXmlToken.SQL_BIGINT:
                            case BinXmlToken.SQL_REAL:
                            case BinXmlToken.SQL_FLOAT:
                            case BinXmlToken.SQL_MONEY:
                            case BinXmlToken.SQL_SMALLMONEY:
                            case BinXmlToken.SQL_DATETIME:
                            case BinXmlToken.SQL_SMALLDATETIME:
                            case BinXmlToken.SQL_DECIMAL:
                            case BinXmlToken.SQL_NUMERIC:
                            case BinXmlToken.XSD_DECIMAL:
                            case BinXmlToken.SQL_UUID:
                            case BinXmlToken.SQL_VARBINARY:
                            case BinXmlToken.SQL_BINARY:
                            case BinXmlToken.SQL_IMAGE:
                            case BinXmlToken.SQL_UDT:
                            case BinXmlToken.XSD_KATMAI_DATE:
                            case BinXmlToken.XSD_KATMAI_DATETIME:
                            case BinXmlToken.XSD_KATMAI_TIME:
                            case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                            case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                            case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                            case BinXmlToken.XSD_BINHEX:
                            case BinXmlToken.XSD_BASE64:
                            case BinXmlToken.XSD_TIME:
                            case BinXmlToken.XSD_DATETIME:
                            case BinXmlToken.XSD_DATE:
                            case BinXmlToken.XSD_BYTE:
                            case BinXmlToken.XSD_UNSIGNEDSHORT:
                            case BinXmlToken.XSD_UNSIGNEDINT:
                            case BinXmlToken.XSD_UNSIGNEDLONG:
                            case BinXmlToken.XSD_QNAME:
                                throw new InvalidCastException(Res.GetString(Res.XmlBinary_CastNotSupported, this.token, "Boolean"));

                            case BinXmlToken.SQL_CHAR:
                            case BinXmlToken.SQL_VARCHAR:
                            case BinXmlToken.SQL_TEXT:
                            case BinXmlToken.SQL_NCHAR:
                            case BinXmlToken.SQL_NVARCHAR:
                            case BinXmlToken.SQL_NTEXT:
                                goto Fallback;

                            case BinXmlToken.Element:
                            case BinXmlToken.EndElem:
                                return XmlConvert.ToBoolean(String.Empty);

                            default:
                                Debug.Fail("should never happen");
                                goto Fallback;
                        }
                    }
                    catch (InvalidCastException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, "Boolean", e, null);
                    }
                    catch (FormatException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, "Boolean", e, null);
                    }
                    origPos = FinishContentAsXXX(origPos);
                    return value;
                }
            }
            finally {
                this.pos = origPos;
            }
        Fallback:
            return base.ReadContentAsBoolean();
        }

        public override DateTime ReadContentAsDateTime() {
            int origPos = this.pos;
            DateTime value;
            try {
                if (SetupContentAsXXX("ReadContentAsDateTime")) {
                    try {
                        switch (this.token) {
                            case BinXmlToken.SQL_DATETIME:
                            case BinXmlToken.SQL_SMALLDATETIME:
                            case BinXmlToken.XSD_TIME:
                            case BinXmlToken.XSD_DATETIME:
                            case BinXmlToken.XSD_DATE:
                            case BinXmlToken.XSD_KATMAI_DATE:
                            case BinXmlToken.XSD_KATMAI_DATETIME:
                            case BinXmlToken.XSD_KATMAI_TIME:
                            case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                            case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                            case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                                value = ValueAsDateTime();
                                break;

                            case BinXmlToken.SQL_BIT:
                            case BinXmlToken.SQL_TINYINT:
                            case BinXmlToken.SQL_SMALLINT:
                            case BinXmlToken.SQL_INT:
                            case BinXmlToken.SQL_BIGINT:
                            case BinXmlToken.SQL_REAL:
                            case BinXmlToken.SQL_FLOAT:
                            case BinXmlToken.SQL_MONEY:
                            case BinXmlToken.SQL_SMALLMONEY:
                            case BinXmlToken.SQL_DECIMAL:
                            case BinXmlToken.SQL_NUMERIC:
                            case BinXmlToken.XSD_DECIMAL:
                            case BinXmlToken.SQL_UUID:
                            case BinXmlToken.SQL_VARBINARY:
                            case BinXmlToken.SQL_BINARY:
                            case BinXmlToken.SQL_IMAGE:
                            case BinXmlToken.SQL_UDT:
                            case BinXmlToken.XSD_BINHEX:
                            case BinXmlToken.XSD_BASE64:
                            case BinXmlToken.XSD_BOOLEAN:
                            case BinXmlToken.XSD_BYTE:
                            case BinXmlToken.XSD_UNSIGNEDSHORT:
                            case BinXmlToken.XSD_UNSIGNEDINT:
                            case BinXmlToken.XSD_UNSIGNEDLONG:
                            case BinXmlToken.XSD_QNAME:
                                throw new InvalidCastException(Res.GetString(Res.XmlBinary_CastNotSupported, this.token, "DateTime"));

                            case BinXmlToken.SQL_CHAR:
                            case BinXmlToken.SQL_VARCHAR:
                            case BinXmlToken.SQL_TEXT:
                            case BinXmlToken.SQL_NCHAR:
                            case BinXmlToken.SQL_NVARCHAR:
                            case BinXmlToken.SQL_NTEXT:
                                goto Fallback;

                            case BinXmlToken.Element:
                            case BinXmlToken.EndElem:
                                return XmlConvert.ToDateTime(String.Empty, XmlDateTimeSerializationMode.RoundtripKind);

                            default:
                                Debug.Fail("should never happen");
                                goto Fallback;
                        }
                    }
                    catch (InvalidCastException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, "DateTime", e, null);
                    }
                    catch (FormatException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, "DateTime", e, null);
                    }
                    catch (OverflowException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, "DateTime", e, null);
                    }

                    origPos = FinishContentAsXXX(origPos);
                    return value;
                }
            }
            finally {
                this.pos = origPos;
            }
        Fallback:
            return base.ReadContentAsDateTime();
        }

        public override Double ReadContentAsDouble() {
            int origPos = this.pos;
            Double value;
            try {
                if (SetupContentAsXXX("ReadContentAsDouble")) {
                    try {
                        switch (this.token) {
                            case BinXmlToken.SQL_REAL:
                            case BinXmlToken.SQL_FLOAT:
                                value = ValueAsDouble();
                                break;

                            case BinXmlToken.SQL_BIT:
                            case BinXmlToken.SQL_TINYINT:
                            case BinXmlToken.SQL_SMALLINT:
                            case BinXmlToken.SQL_INT:
                            case BinXmlToken.SQL_BIGINT:
                            case BinXmlToken.SQL_MONEY:
                            case BinXmlToken.SQL_SMALLMONEY:
                            case BinXmlToken.SQL_DATETIME:
                            case BinXmlToken.SQL_SMALLDATETIME:
                            case BinXmlToken.SQL_DECIMAL:
                            case BinXmlToken.SQL_NUMERIC:
                            case BinXmlToken.XSD_DECIMAL:
                            case BinXmlToken.SQL_UUID:
                            case BinXmlToken.SQL_VARBINARY:
                            case BinXmlToken.SQL_BINARY:
                            case BinXmlToken.SQL_IMAGE:
                            case BinXmlToken.SQL_UDT:
                            case BinXmlToken.XSD_KATMAI_DATE:
                            case BinXmlToken.XSD_KATMAI_DATETIME:
                            case BinXmlToken.XSD_KATMAI_TIME:
                            case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                            case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                            case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                            case BinXmlToken.XSD_BINHEX:
                            case BinXmlToken.XSD_BASE64:
                            case BinXmlToken.XSD_BOOLEAN:
                            case BinXmlToken.XSD_TIME:
                            case BinXmlToken.XSD_DATETIME:
                            case BinXmlToken.XSD_DATE:
                            case BinXmlToken.XSD_BYTE:
                            case BinXmlToken.XSD_UNSIGNEDSHORT:
                            case BinXmlToken.XSD_UNSIGNEDINT:
                            case BinXmlToken.XSD_UNSIGNEDLONG:
                            case BinXmlToken.XSD_QNAME:
                                throw new InvalidCastException(Res.GetString(Res.XmlBinary_CastNotSupported, this.token, "Double"));

                            case BinXmlToken.SQL_CHAR:
                            case BinXmlToken.SQL_VARCHAR:
                            case BinXmlToken.SQL_TEXT:
                            case BinXmlToken.SQL_NCHAR:
                            case BinXmlToken.SQL_NVARCHAR:
                            case BinXmlToken.SQL_NTEXT:
                                goto Fallback;

                            case BinXmlToken.Element:
                            case BinXmlToken.EndElem:
                                return XmlConvert.ToDouble(String.Empty);

                            default:
                                Debug.Fail("should never happen");
                                goto Fallback;
                        }
                    }
                    catch (InvalidCastException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, "Double", e, null);
                    }
                    catch (FormatException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, "Double", e, null);
                    }
                    catch (OverflowException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, "Double", e, null);
                    }

                    origPos = FinishContentAsXXX(origPos);
                    return value;
                }
            }
            finally {
                this.pos = origPos;
            }
        Fallback:
            return base.ReadContentAsDouble();
        }

        public override float ReadContentAsFloat() {
            int origPos = this.pos;
            float value;
            try {
                if (SetupContentAsXXX("ReadContentAsFloat")) {
                    try {
                        switch (this.token) {
                            case BinXmlToken.SQL_REAL:
                            case BinXmlToken.SQL_FLOAT:
                                value = checked (((float)ValueAsDouble()));
                                break;

                            case BinXmlToken.SQL_BIT:
                            case BinXmlToken.SQL_TINYINT:
                            case BinXmlToken.SQL_SMALLINT:
                            case BinXmlToken.SQL_INT:
                            case BinXmlToken.SQL_BIGINT:
                            case BinXmlToken.SQL_MONEY:
                            case BinXmlToken.SQL_SMALLMONEY:
                            case BinXmlToken.SQL_DATETIME:
                            case BinXmlToken.SQL_SMALLDATETIME:
                            case BinXmlToken.SQL_DECIMAL:
                            case BinXmlToken.SQL_NUMERIC:
                            case BinXmlToken.XSD_DECIMAL:
                            case BinXmlToken.SQL_UUID:
                            case BinXmlToken.SQL_VARBINARY:
                            case BinXmlToken.SQL_BINARY:
                            case BinXmlToken.SQL_IMAGE:
                            case BinXmlToken.SQL_UDT:
                            case BinXmlToken.XSD_KATMAI_DATE:
                            case BinXmlToken.XSD_KATMAI_DATETIME:
                            case BinXmlToken.XSD_KATMAI_TIME:
                            case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                            case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                            case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                            case BinXmlToken.XSD_BINHEX:
                            case BinXmlToken.XSD_BASE64:
                            case BinXmlToken.XSD_BOOLEAN:
                            case BinXmlToken.XSD_TIME:
                            case BinXmlToken.XSD_DATETIME:
                            case BinXmlToken.XSD_DATE:
                            case BinXmlToken.XSD_BYTE:
                            case BinXmlToken.XSD_UNSIGNEDSHORT:
                            case BinXmlToken.XSD_UNSIGNEDINT:
                            case BinXmlToken.XSD_UNSIGNEDLONG:
                            case BinXmlToken.XSD_QNAME:
                                throw new InvalidCastException(Res.GetString(Res.XmlBinary_CastNotSupported, this.token, "Float"));

                            case BinXmlToken.SQL_CHAR:
                            case BinXmlToken.SQL_VARCHAR:
                            case BinXmlToken.SQL_TEXT:
                            case BinXmlToken.SQL_NCHAR:
                            case BinXmlToken.SQL_NVARCHAR:
                            case BinXmlToken.SQL_NTEXT:
                                goto Fallback;

                            case BinXmlToken.Element:
                            case BinXmlToken.EndElem:
                                return XmlConvert.ToSingle(String.Empty);

                            default:
                                Debug.Fail("should never happen");
                                goto Fallback;
                        }
                    }
                    catch (InvalidCastException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, "Float", e, null);
                    }
                    catch (FormatException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, "Float", e, null);
                    }
                    catch (OverflowException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, "Float", e, null);
                    }

                    origPos = FinishContentAsXXX(origPos);
                    return value;
                }
            }
            finally {
                this.pos = origPos;
            }
        Fallback:
            return base.ReadContentAsFloat();
        }

        public override decimal ReadContentAsDecimal() {
            int origPos = this.pos;
            decimal value;
            try {
                if (SetupContentAsXXX("ReadContentAsDecimal")) {
                    try {
                        switch (this.token) {
                            case BinXmlToken.SQL_BIT:
                            case BinXmlToken.SQL_TINYINT:
                            case BinXmlToken.SQL_SMALLINT:
                            case BinXmlToken.SQL_INT:
                            case BinXmlToken.SQL_BIGINT:
                            case BinXmlToken.SQL_MONEY:
                            case BinXmlToken.SQL_SMALLMONEY:
                            case BinXmlToken.SQL_DECIMAL:
                            case BinXmlToken.SQL_NUMERIC:
                            case BinXmlToken.XSD_DECIMAL:
                            case BinXmlToken.XSD_BYTE:
                            case BinXmlToken.XSD_UNSIGNEDSHORT:
                            case BinXmlToken.XSD_UNSIGNEDINT:
                            case BinXmlToken.XSD_UNSIGNEDLONG:
                                value = ValueAsDecimal();
                                break;

                            case BinXmlToken.SQL_REAL:
                            case BinXmlToken.SQL_FLOAT:
                            case BinXmlToken.SQL_DATETIME:
                            case BinXmlToken.SQL_SMALLDATETIME:
                            case BinXmlToken.SQL_UUID:
                            case BinXmlToken.SQL_VARBINARY:
                            case BinXmlToken.SQL_BINARY:
                            case BinXmlToken.SQL_IMAGE:
                            case BinXmlToken.SQL_UDT:
                            case BinXmlToken.XSD_KATMAI_DATE:
                            case BinXmlToken.XSD_KATMAI_DATETIME:
                            case BinXmlToken.XSD_KATMAI_TIME:
                            case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                            case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                            case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                            case BinXmlToken.XSD_BINHEX:
                            case BinXmlToken.XSD_BASE64:
                            case BinXmlToken.XSD_BOOLEAN:
                            case BinXmlToken.XSD_TIME:
                            case BinXmlToken.XSD_DATETIME:
                            case BinXmlToken.XSD_DATE:
                            case BinXmlToken.XSD_QNAME:
                                throw new InvalidCastException(Res.GetString(Res.XmlBinary_CastNotSupported, this.token, "Decimal"));

                            case BinXmlToken.SQL_CHAR:
                            case BinXmlToken.SQL_VARCHAR:
                            case BinXmlToken.SQL_TEXT:
                            case BinXmlToken.SQL_NCHAR:
                            case BinXmlToken.SQL_NVARCHAR:
                            case BinXmlToken.SQL_NTEXT:
                                goto Fallback;

                            case BinXmlToken.Element:
                            case BinXmlToken.EndElem:
                                return XmlConvert.ToDecimal(String.Empty);

                            default:
                                Debug.Fail("should never happen");
                                goto Fallback;
                        }
                    }
                    catch (InvalidCastException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, "Decimal", e, null);
                    }
                    catch (FormatException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, "Decimal", e, null);
                    }
                    catch (OverflowException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, "Decimal", e, null);
                    }

                    origPos = FinishContentAsXXX(origPos);
                    return value;
                }
            }
            finally {
                this.pos = origPos;
            }
        Fallback:
            return base.ReadContentAsDecimal();
        }

        public override int ReadContentAsInt() {
            int origPos = this.pos;
            int value;
            try {
                if (SetupContentAsXXX("ReadContentAsInt")) {
                    try {
                        switch (this.token) {
                            case BinXmlToken.SQL_BIT:
                            case BinXmlToken.SQL_TINYINT:
                            case BinXmlToken.SQL_SMALLINT:
                            case BinXmlToken.SQL_INT:
                            case BinXmlToken.SQL_BIGINT:
                            case BinXmlToken.SQL_MONEY:
                            case BinXmlToken.SQL_SMALLMONEY:
                            case BinXmlToken.SQL_DECIMAL:
                            case BinXmlToken.SQL_NUMERIC:
                            case BinXmlToken.XSD_DECIMAL:
                            case BinXmlToken.XSD_BYTE:
                            case BinXmlToken.XSD_UNSIGNEDSHORT:
                            case BinXmlToken.XSD_UNSIGNEDINT:
                            case BinXmlToken.XSD_UNSIGNEDLONG:
                                value = checked((int)ValueAsLong());
                                break;

                            case BinXmlToken.SQL_REAL:
                            case BinXmlToken.SQL_FLOAT:
                            case BinXmlToken.SQL_DATETIME:
                            case BinXmlToken.SQL_SMALLDATETIME:
                            case BinXmlToken.SQL_UUID:
                            case BinXmlToken.SQL_VARBINARY:
                            case BinXmlToken.SQL_BINARY:
                            case BinXmlToken.SQL_IMAGE:
                            case BinXmlToken.SQL_UDT:
                            case BinXmlToken.XSD_KATMAI_DATE:
                            case BinXmlToken.XSD_KATMAI_DATETIME:
                            case BinXmlToken.XSD_KATMAI_TIME:
                            case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                            case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                            case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                            case BinXmlToken.XSD_BINHEX:
                            case BinXmlToken.XSD_BASE64:
                            case BinXmlToken.XSD_BOOLEAN:
                            case BinXmlToken.XSD_TIME:
                            case BinXmlToken.XSD_DATETIME:
                            case BinXmlToken.XSD_DATE:
                            case BinXmlToken.XSD_QNAME:
                                throw new InvalidCastException(Res.GetString(Res.XmlBinary_CastNotSupported, this.token, "Int32"));

                            case BinXmlToken.SQL_CHAR:
                            case BinXmlToken.SQL_VARCHAR:
                            case BinXmlToken.SQL_TEXT:
                            case BinXmlToken.SQL_NCHAR:
                            case BinXmlToken.SQL_NVARCHAR:
                            case BinXmlToken.SQL_NTEXT:
                                goto Fallback;

                            case BinXmlToken.Element:
                            case BinXmlToken.EndElem:
                                return XmlConvert.ToInt32(String.Empty);

                            default:
                                Debug.Fail("should never happen");
                                goto Fallback;
                        }
                    }
                    catch (InvalidCastException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, "Int32", e, null);
                    }
                    catch (FormatException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, "Int32", e, null);
                    }
                    catch (OverflowException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, "Int32", e, null);
                    }

                    origPos = FinishContentAsXXX(origPos);
                    return value;
                }
            }
            finally {
                this.pos = origPos;
            }
        Fallback:
            return base.ReadContentAsInt();
        }

        public override long ReadContentAsLong() {
            int origPos = this.pos;
            long value;
            try {
                if (SetupContentAsXXX("ReadContentAsLong")) {
                    try {
                        switch (this.token) {
                            case BinXmlToken.SQL_BIT:
                            case BinXmlToken.SQL_TINYINT:
                            case BinXmlToken.SQL_SMALLINT:
                            case BinXmlToken.SQL_INT:
                            case BinXmlToken.SQL_BIGINT:
                            case BinXmlToken.SQL_MONEY:
                            case BinXmlToken.SQL_SMALLMONEY:
                            case BinXmlToken.SQL_DECIMAL:
                            case BinXmlToken.SQL_NUMERIC:
                            case BinXmlToken.XSD_DECIMAL:
                            case BinXmlToken.XSD_BYTE:
                            case BinXmlToken.XSD_UNSIGNEDSHORT:
                            case BinXmlToken.XSD_UNSIGNEDINT:
                            case BinXmlToken.XSD_UNSIGNEDLONG:
                                value = ValueAsLong();
                                break;

                            case BinXmlToken.SQL_REAL:
                            case BinXmlToken.SQL_FLOAT:
                            case BinXmlToken.SQL_DATETIME:
                            case BinXmlToken.SQL_SMALLDATETIME:
                            case BinXmlToken.SQL_UUID:
                            case BinXmlToken.SQL_VARBINARY:
                            case BinXmlToken.SQL_BINARY:
                            case BinXmlToken.SQL_IMAGE:
                            case BinXmlToken.SQL_UDT:
                            case BinXmlToken.XSD_KATMAI_DATE:
                            case BinXmlToken.XSD_KATMAI_DATETIME:
                            case BinXmlToken.XSD_KATMAI_TIME:
                            case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                            case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                            case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                            case BinXmlToken.XSD_BINHEX:
                            case BinXmlToken.XSD_BASE64:
                            case BinXmlToken.XSD_BOOLEAN:
                            case BinXmlToken.XSD_TIME:
                            case BinXmlToken.XSD_DATETIME:
                            case BinXmlToken.XSD_DATE:
                            case BinXmlToken.XSD_QNAME:
                                throw new InvalidCastException(Res.GetString(Res.XmlBinary_CastNotSupported, this.token, "Int64"));

                            case BinXmlToken.SQL_CHAR:
                            case BinXmlToken.SQL_VARCHAR:
                            case BinXmlToken.SQL_TEXT:
                            case BinXmlToken.SQL_NCHAR:
                            case BinXmlToken.SQL_NVARCHAR:
                            case BinXmlToken.SQL_NTEXT:
                                goto Fallback;

                            case BinXmlToken.Element:
                            case BinXmlToken.EndElem:
                                return XmlConvert.ToInt64(String.Empty);

                            default:
                                Debug.Fail("should never happen");
                                goto Fallback;
                        }
                    }
                    catch (InvalidCastException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, "Int64", e, null);
                    }
                    catch (FormatException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, "Int64", e, null);
                    }
                    catch (OverflowException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, "Int64", e, null);
                    }

                    origPos = FinishContentAsXXX(origPos);
                    return value;
                }
            }
            finally {
                this.pos = origPos;
            }
        Fallback:
            return base.ReadContentAsLong();
        }

        public override object ReadContentAsObject() {
            int origPos = this.pos;
            try {
                if (SetupContentAsXXX("ReadContentAsObject")) {
                    object value;
                    try {
                        if (this.NodeType == XmlNodeType.Element || this.NodeType == XmlNodeType.EndElement)
                            value = String.Empty;
                        else
                            value = this.ValueAsObject(this.token, false);
                    }
                    catch (InvalidCastException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, "Object", e, null);
                    }
                    catch (FormatException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, "Object", e, null);
                    }
                    catch (OverflowException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, "Object", e, null);
                    }
                    origPos = FinishContentAsXXX(origPos);
                    return value;
                }
            }
            finally {
                this.pos = origPos;
            }
            //Fallback:
            return base.ReadContentAsObject();
        }

        public override object ReadContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver) {
            int origPos = this.pos;
            try {
                if (SetupContentAsXXX("ReadContentAs")) {
                    object value;
                    try {
                        if (this.NodeType == XmlNodeType.Element || this.NodeType == XmlNodeType.EndElement) {
                            value = String.Empty;
                        }
                        else if (returnType == this.ValueType || returnType == typeof(object)) {
                            value = this.ValueAsObject(this.token, false);
                        }
                        else {
                            value = this.ValueAs(this.token, returnType, namespaceResolver);
                        }
                    }
                    catch (InvalidCastException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, returnType.ToString(), e, null);
                    }
                    catch (FormatException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, returnType.ToString(), e, null);
                    }
                    catch (OverflowException e) {
                        throw new XmlException(Res.Xml_ReadContentAsFormatException, returnType.ToString(), e, null);
                    }
                    origPos = FinishContentAsXXX(origPos);
                    return value;
                }
            }
            finally {
                this.pos = origPos;
            }
            return base.ReadContentAs(returnType, namespaceResolver);
        }

        //////////
        // IXmlNamespaceResolver

        System.Collections.Generic.IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope(XmlNamespaceScope scope) {
            if (ScanState.XmlText == this.state) {
                IXmlNamespaceResolver resolver = (IXmlNamespaceResolver)this.textXmlReader;
                return resolver.GetNamespacesInScope(scope);
            }
            else {
                Dictionary<String, String> nstable = new Dictionary<String, String>();
                if (XmlNamespaceScope.Local == scope) {
                    // are we even inside an element? (depth==0 is where we have xml, and xmlns declared...)
                    if (this.elemDepth > 0) {
                        NamespaceDecl nsdecl = this.elementStack[this.elemDepth].nsdecls;
                        while (null != nsdecl) {
                            nstable.Add(nsdecl.prefix, nsdecl.uri);
                            nsdecl = nsdecl.scopeLink;
                        }
                    }
                }
                else {
                    foreach (NamespaceDecl nsdecl in this.namespaces.Values) {
                        // don't add predefined decls unless scope == all, then only add 'xml'                       
                        if (nsdecl.scope != -1 || (XmlNamespaceScope.All == scope && "xml" == nsdecl.prefix)) {
                            // xmlns="" only ever reported via scope==local
                            if (nsdecl.prefix.Length > 0 || nsdecl.uri.Length > 0)
                                nstable.Add(nsdecl.prefix, nsdecl.uri);
                        }
                    }
                }
                return nstable;
            }
        }

        string IXmlNamespaceResolver.LookupPrefix(string namespaceName) {
            if (ScanState.XmlText == this.state) {
                IXmlNamespaceResolver resolver = (IXmlNamespaceResolver)this.textXmlReader;
                return resolver.LookupPrefix(namespaceName);
            }
            else {
                if (null == namespaceName)
                    return null;

                namespaceName = this.xnt.Get(namespaceName);
                if (null == namespaceName)
                    return null;

                for (int i = this.elemDepth; i >= 0; i--) {
                    NamespaceDecl nsdecl = this.elementStack[i].nsdecls;
                    while (null != nsdecl) {
                        if ((object)nsdecl.uri == (object)namespaceName)
                            return nsdecl.prefix;
                        nsdecl = nsdecl.scopeLink;
                    }
                }
                return null;
            }
        }

        //////////
        // Internal implementation methods

        void VerifyVersion(int requiredVersion, BinXmlToken token) {
            if (version < requiredVersion) {
                throw ThrowUnexpectedToken(token);
            }
        }

        void AddInitNamespace(string prefix, string uri) {
            NamespaceDecl nsdecl = new NamespaceDecl(prefix, uri, this.elementStack[0].nsdecls, null, -1, true);
            this.elementStack[0].nsdecls = nsdecl;
            this.namespaces.Add(prefix, nsdecl);
        }

        void AddName() {
            string txt = ParseText();
            int symNum = this.symbolTables.symCount++;
            string[] symtable = this.symbolTables.symtable;
            if (symNum == symtable.Length) {
                string[] n = new string[checked(symNum * 2)];
                System.Array.Copy(symtable, 0, n, 0, symNum);
                this.symbolTables.symtable = symtable = n;
            }
            symtable[symNum] = xnt.Add(txt);
        }

        void AddQName() {
            int nsUri = ReadNameRef();
            int prefix = ReadNameRef();
            int lname = ReadNameRef();
            int qnameNum = this.symbolTables.qnameCount++;
            QName[] qnametable = this.symbolTables.qnametable;
            if (qnameNum == qnametable.Length) {
                QName[] n = new QName[checked(qnameNum * 2)];
                System.Array.Copy(qnametable, 0, n, 0, qnameNum);
                this.symbolTables.qnametable = qnametable = n;
            }
            string[] symtable = this.symbolTables.symtable;
            string prefixStr = symtable[prefix];
            string lnameStr;
            string nsUriStr;
            // xmlns attributes are encodes differently...
            if (lname == 0) { // xmlns attribute
                // for some reason, sqlserver sometimes generates these...
                if (prefix == 0 && nsUri == 0)
                    return;
                // it is a real namespace decl, make sure it looks valid
                if (!prefixStr.StartsWith("xmlns", StringComparison.Ordinal))
                    goto BadDecl;
                if (5 < prefixStr.Length) {
                    if (6 == prefixStr.Length || ':' != prefixStr[5])
                        goto BadDecl;
                    lnameStr = this.xnt.Add(prefixStr.Substring(6));
                    prefixStr = this.xmlns;
                }
                else {
                    lnameStr = prefixStr;
                    prefixStr = String.Empty;
                }
                nsUriStr = this.nsxmlns;
            }
            else {
                lnameStr = symtable[lname];
                nsUriStr = symtable[nsUri];
            }
            qnametable[qnameNum].Set(prefixStr, lnameStr, nsUriStr);
            return;
        BadDecl:
            throw new XmlException(Res.Xml_BadNamespaceDecl, (string[])null);
        }

        private void NameFlush() {
            this.symbolTables.symCount = this.symbolTables.qnameCount = 1;
            Array.Clear(this.symbolTables.symtable, 1, this.symbolTables.symtable.Length - 1);
            Array.Clear(this.symbolTables.qnametable, 0, this.symbolTables.qnametable.Length);
        }

        private void SkipExtn() {
            int cb = ParseMB32();
            checked{this.pos += cb;} 
            Fill(-1);
        }

        private int ReadQNameRef() {
            int nameNum = ParseMB32();
            if (nameNum < 0 || nameNum >= this.symbolTables.qnameCount)
                throw new XmlException(Res.XmlBin_InvalidQNameID, String.Empty);
            return nameNum;
        }

        private int ReadNameRef() {
            int nameNum = ParseMB32();
            if (nameNum < 0 || nameNum >= this.symbolTables.symCount)
                throw new XmlException(Res.XmlBin_InvalidQNameID, String.Empty);
            return nameNum;
        }

        // pull more data from input stream
        private bool FillAllowEOF() {
            if (this.eof)
                return false;
            byte[] data = this.data;
            int pos = this.pos;
            int mark = this.mark;
            int end = this.end;
            if (mark == -1) {
                mark = pos;
            }
            if (mark >= 0 && mark < end) {
                Debug.Assert(this.mark <= this.end, "Mark should never be past End");
                Debug.Assert(this.mark <= this.pos, "Mark should never be after Pos");
                int cbKeep = end - mark;
                if (cbKeep > 7 * (data.Length / 8)) {
                    // grow buffer
                    byte[] newdata = new byte[checked(data.Length * 2)];
                    System.Array.Copy(data, mark, newdata, 0, cbKeep);
                    this.data = data = newdata;
                }
                else {
                    System.Array.Copy(data, mark, data, 0, cbKeep);
                }
                pos -= mark;
                end -= mark;
                this.tokDataPos -= mark;
                for (int i = 0; i < this.attrCount; i++) {
                    this.attributes[i].AdjustPosition(-mark);
                    // make sure it is still a valid range
                    Debug.Assert((this.attributes[i].contentPos >= 0) && (this.attributes[i].contentPos <= (end)));
                }
                this.pos = pos;
                this.mark = 0;
                this.offset += mark;
            }
            else {
                Debug.Assert(this.attrCount == 0);
                this.pos -= end;
                this.mark -= end;
                this.offset += end;
                this.tokDataPos -= end;
                end = 0;
            }
            int cbFill = data.Length - end;
            int cbRead = this.inStrm.Read(data, end, cbFill);
            this.end = end + cbRead;
            this.eof = !(cbRead > 0);
            return (cbRead > 0);
        }

        // require must be < 1/8 buffer, or else Fill might not actually 
        // grab that much data
        void Fill_(int require) {
            Debug.Assert((this.pos + require) >= this.end);
            while (FillAllowEOF() && ((this.pos + require) >= this.end))
                ;
            if ((this.pos + require) >= this.end)
                throw ThrowXmlException(Res.Xml_UnexpectedEOF1);
        }

        // inline the common case
        void Fill(int require) {
            if ((this.pos + require) >= this.end)
                Fill_(require);
        }

        byte ReadByte() {
            Fill(0);
            return this.data[this.pos++];
        }
        ushort ReadUShort() {
            Fill(1);
            int pos = this.pos; byte[] data = this.data;
            ushort val = (ushort)(data[pos] + (data[pos + 1] << 8));
            this.pos += 2;
            return val;
        }

        int ParseMB32() {
            byte b = ReadByte();
            if (b > 127)
                return ParseMB32_(b);
            return b;
        }

        int ParseMB32_(byte b) {
            uint u, t;
            u = (uint)b & (uint)0x7F;
            Debug.Assert(0 != (b & 0x80));
            b = ReadByte();
            t = (uint)b & (uint)0x7F;
            u = u + (t << 7);
            if (b > 127) {
                b = ReadByte();
                t = (uint)b & (uint)0x7F;
                u = u + (t << 14);
                if (b > 127) {
                    b = ReadByte();
                    t = (uint)b & (uint)0x7F;
                    u = u + (t << 21);
                    if (b > 127) {
                        b = ReadByte();
                        // bottom 4 bits are all that are needed, 
                        // but we are mapping to 'int', which only
                        // actually has space for 3 more bits.
                        t = (uint)b & (uint)0x07;
                        if (b > 7)
                            throw ThrowXmlException(Res.XmlBinary_ValueTooBig);
                        u = u + (t << 28);
                    }
                }
            }
            return (int)u;
        }

        // this assumes that we have already ensured that all
        // necessary bytes are loaded in to the buffer
        int ParseMB32(int pos) {
            uint u, t;
            byte[] data = this.data;
            byte b = data[pos++];
            u = (uint)b & (uint)0x7F;
            if (b > 127) {
                b = data[pos++];
                t = (uint)b & (uint)0x7F;
                u = u + (t << 7);
                if (b > 127) {
                    b = data[pos++];
                    t = (uint)b & (uint)0x7F;
                    u = u + (t << 14);
                    if (b > 127) {
                        b = data[pos++];
                        t = (uint)b & (uint)0x7F;
                        u = u + (t << 21);
                        if (b > 127) {
                            b = data[pos++];
                            // last byte only has 4 significant digits
                            t = (uint)b & (uint)0x07;
                            if (b > 7)
                                throw ThrowXmlException(Res.XmlBinary_ValueTooBig);
                            u = u + (t << 28);
                        }
                    }
                }
            }
            return (int)u;
        }

        // we don't actually support MB64, since we use int for 
        // all our math anyway...
        int ParseMB64() {
            byte b = ReadByte();
            if (b > 127)
                return ParseMB32_(b);
            return b;
        }

        BinXmlToken PeekToken() {
            while ((this.pos >= this.end) && FillAllowEOF())
                ;
            if (this.pos >= this.end)
                return BinXmlToken.EOF;
            return (BinXmlToken)this.data[this.pos];
        }

        BinXmlToken ReadToken() {
            while ((this.pos >= this.end) && FillAllowEOF())
                ;
            if (this.pos >= this.end)
                return BinXmlToken.EOF;
            return (BinXmlToken)this.data[this.pos++];
        }

        BinXmlToken NextToken2(BinXmlToken token) {
            while (true) {
                switch (token) {
                    case BinXmlToken.Name:
                        AddName();
                        break;
                    case BinXmlToken.QName:
                        AddQName();
                        break;
                    case BinXmlToken.NmFlush:
                        NameFlush();
                        break;
                    case BinXmlToken.Extn:
                        SkipExtn();
                        break;
                    default:
                        return token;
                }
                token = ReadToken();
            }
        }

        BinXmlToken NextToken1() {
            BinXmlToken token;
            int pos = this.pos;
            if (pos >= this.end)
                token = ReadToken();
            else {
                token = (BinXmlToken)this.data[pos];
                this.pos = pos + 1;
            }
            // BinXmlToken.Name = 0xF0
            // BinXmlToken.QName = 0xEF
            // BinXmlToken.Extn = 0xEA,
            // BinXmlToken.NmFlush = 0xE9,
            if (token >= BinXmlToken.NmFlush
                && token <= BinXmlToken.Name)
                return NextToken2(token);
            return token;
        }

        BinXmlToken NextToken() {
            int pos = this.pos;
            if (pos < this.end) {
                BinXmlToken t = (BinXmlToken)this.data[pos];
                if (!(t >= BinXmlToken.NmFlush && t <= BinXmlToken.Name)) {
                    this.pos = pos + 1;
                    return t;
                }
            }
            return NextToken1();
        }

        // peek next non-meta token
        BinXmlToken PeekNextToken() {
            BinXmlToken token = NextToken();
            if (BinXmlToken.EOF != token)
                this.pos--;
            return token;
        }

        // like NextToken() but meta-tokens are skipped (not reinterpreted)
        BinXmlToken RescanNextToken() {
            BinXmlToken token;
            while (true) {
                token = ReadToken();
                switch (token) {
                    case BinXmlToken.Name: {
                            int cb = ParseMB32();
                            checked{this.pos += 2 * cb;}
                            break;
                        }
                    case BinXmlToken.QName:
                        ParseMB32();
                        ParseMB32();
                        ParseMB32();
                        break;
                    case BinXmlToken.Extn: {
                            int cb = ParseMB32();
                            checked{this.pos += cb;}
                            break;
                        }
                    case BinXmlToken.NmFlush:
                        break;
                    default:
                        return token;
                }
            }
        }

        string ParseText() {
            int oldmark = this.mark;
            try {
                if (oldmark < 0)
                    this.mark = this.pos;
                int cch, pos;
                cch = ScanText(out pos);
                return GetString(pos, cch);
            }
            finally {
                if (oldmark < 0)
                    this.mark = -1;
            }
        }

        int ScanText(out int start) {
            int cch = ParseMB32();
            int oldmark = this.mark;
            int begin = this.pos;
            checked{this.pos += cch * 2;} // cch = num utf-16 chars
            if (this.pos > this.end)
                Fill(-1);
            // Fill call might have moved buffer
            start = begin - (oldmark - this.mark);
            return cch;
        }

        string GetString(int pos, int cch) {
            Debug.Assert(pos >= 0 && cch >= 0);
            if (checked(pos + (cch * 2)) > this.end)
                throw new XmlException(Res.Xml_UnexpectedEOF1, (string[])null);
            if (cch == 0)
                return String.Empty;
            // GetStringUnaligned is _significantly_ faster than unicode.GetString()
            // but since IA64 doesn't support unaligned reads, we can't do it if
            // the address is not aligned properly.  Since the byte[] will be aligned,
            // we can detect address alignment my just looking at the offset
            if ((pos & 1) == 0)
                return GetStringAligned(this.data, pos, cch);
            else
                return unicode.GetString(this.data, pos, checked(cch * 2));
        }

        unsafe String GetStringAligned(byte[] data, int offset, int cch) {
            Debug.Assert((offset & 1) == 0);
            fixed (byte* pb = data) {
                char* p = (char*)(pb + offset);
                return new String(p, 0, cch);
            }
        }

        private string GetAttributeText(int i) {
            string val = this.attributes[i].val;

            if (null != val)
                return val;
            else {
                int origPos = this.pos;
                try {
                    this.pos = this.attributes[i].contentPos;
                    BinXmlToken token = RescanNextToken();
                    if (BinXmlToken.Attr == token || BinXmlToken.EndAttrs == token) {
                        return "";
                    }
                    this.token = token;
                    ReScanOverValue(token);
                    return ValueAsString(token);
                }
                finally {
                    this.pos = origPos;
                }
            }
        }

        private int LocateAttribute(string name, string ns) {
            for (int i = 0; i < this.attrCount; i++) {
                if (this.attributes[i].name.MatchNs(name, ns))
                    return i;
            }

            return -1;
        }

        private int LocateAttribute(string name) {
            string prefix, lname;
            ValidateNames.SplitQName(name, out prefix, out lname);

            for (int i = 0; i < this.attrCount; i++) {
                if (this.attributes[i].name.MatchPrefix(prefix, lname))
                    return i;
            }

            return -1;
        }

        private void PositionOnAttribute(int i) {
            // save element's qname
            this.attrIndex = i;
            this.qnameOther = this.attributes[i - 1].name;
            if (this.state == ScanState.Doc) {
                this.parentNodeType = this.nodetype;
            }
            this.token = BinXmlToken.Attr;
            this.nodetype = XmlNodeType.Attribute;
            this.state = ScanState.Attr;
            this.valueType = TypeOfObject;
            this.stringValue = null;
        }

        void GrowElements() {
            int newcount = this.elementStack.Length * 2;
            ElemInfo[] n = new ElemInfo[newcount];

            System.Array.Copy(this.elementStack, 0, n, 0, this.elementStack.Length);
            this.elementStack = n;
        }

        void GrowAttributes() {
            int newcount = this.attributes.Length * 2;
            AttrInfo[] n = new AttrInfo[newcount];

            System.Array.Copy(this.attributes, 0, n, 0, this.attrCount);
            this.attributes = n;
        }

        void ClearAttributes() {
            if (this.attrCount != 0)
                this.attrCount = 0;
        }

        void PushNamespace(string prefix, string ns, bool implied) {
            if (prefix == "xml")
                return;
            int elemDepth = this.elemDepth;
            NamespaceDecl curDecl;
            this.namespaces.TryGetValue(prefix, out curDecl);
            if (null != curDecl) {
                if (curDecl.uri == ns) {
                    // if we see the nsdecl after we saw the first reference in this scope
                    // fix up 'implied' flag
                    if (!implied && curDecl.implied
                        && (curDecl.scope == elemDepth)) {
                        curDecl.implied = false;
                    }
                    return;
                }
                // check that this doesn't conflict
                this.qnameElement.CheckPrefixNS(prefix, ns);
                if (prefix.Length != 0) {
                    for (int i = 0; i < this.attrCount; i++) {
                        if (this.attributes[i].name.prefix.Length != 0)
                            this.attributes[i].name.CheckPrefixNS(prefix, ns);
                    }
                }
            }
            // actually add ns decl
            NamespaceDecl decl = new NamespaceDecl(prefix, ns,
                this.elementStack[elemDepth].nsdecls,
                curDecl, elemDepth, implied);
            this.elementStack[elemDepth].nsdecls = decl;
            this.namespaces[prefix] = decl;
        }

        void PopNamespaces(NamespaceDecl firstInScopeChain) {
            NamespaceDecl decl = firstInScopeChain;
            while (null != decl) {
                if (null == decl.prevLink)
                    this.namespaces.Remove(decl.prefix);
                else
                    this.namespaces[decl.prefix] = decl.prevLink;
                NamespaceDecl next = decl.scopeLink;
                // unlink chains for better gc behaviour 
                decl.prevLink = null;
                decl.scopeLink = null;
                decl = next;
            }
        }

        void GenerateImpliedXmlnsAttrs() {
            QName name;
            NamespaceDecl decl = this.elementStack[this.elemDepth].nsdecls;
            while (null != decl) {
                if (decl.implied) {
                    if (this.attrCount == this.attributes.Length)
                        GrowAttributes();
                    if (decl.prefix.Length == 0)
                        name = new QName(string.Empty, this.xmlns, this.nsxmlns);
                    else
                        name = new QName(this.xmlns, xnt.Add(decl.prefix), this.nsxmlns);
                    this.attributes[this.attrCount].Set(name, decl.uri);
                    this.attrCount++;
                }
                decl = decl.scopeLink;
            }
        }

        bool ReadInit(bool skipXmlDecl) {
            string err = null;
            if (!sniffed) {
                // check magic header
                ushort magic = ReadUShort();
                if (magic != 0xFFDF) {
                    err = Res.XmlBinary_InvalidSignature;
                    goto Error;
                }
            }

            // check protocol version
            this.version = ReadByte();
            if (version != 0x1 && version != 0x2) {
                err = Res.XmlBinary_InvalidProtocolVersion;
                goto Error;
            }

            // check encoding marker, 1200 == utf16
            if (1200 != ReadUShort()) {
                err = Res.XmlBinary_UnsupportedCodePage;
                goto Error;
            }

            this.state = ScanState.Doc;
            if (BinXmlToken.XmlDecl == PeekToken()) {
                this.pos++;
                this.attributes[0].Set(new QName(string.Empty, this.xnt.Add("version"), string.Empty), ParseText());
                this.attrCount = 1;
                if (BinXmlToken.Encoding == PeekToken()) {
                    this.pos++;
                    this.attributes[1].Set(new QName(string.Empty, this.xnt.Add("encoding"), string.Empty), ParseText());
                    this.attrCount++;
                }

                byte standalone = ReadByte();
                switch (standalone) {
                    case 0:
                        break;
                    case 1:
                    case 2:
                        this.attributes[this.attrCount].Set(new QName(string.Empty, this.xnt.Add("standalone"), string.Empty), (standalone == 1) ? "yes" : "no");
                        this.attrCount++;
                        break;
                    default:
                        err = Res.XmlBinary_InvalidStandalone;
                        goto Error;
                }
                if (!skipXmlDecl) {
                    QName xmlDeclQName = new QName(String.Empty, this.xnt.Add("xml"), String.Empty);
                    this.qnameOther = this.qnameElement = xmlDeclQName;
                    this.nodetype = XmlNodeType.XmlDeclaration;
                    this.posAfterAttrs = this.pos;
                    return true;
                }
                // else ReadDoc will clear the attributes for us
            }
            return ReadDoc();

        Error:
            this.state = ScanState.Error;
            throw new XmlException(err, (string[])null);
        }

        void ScanAttributes() {
            BinXmlToken token;
            int xmlspace = -1;
            int xmllang = -1;

            this.mark = this.pos;
            string curDeclPrefix = null;
            bool lastWasValue = false;

            while (BinXmlToken.EndAttrs != (token = NextToken())) {
                if (BinXmlToken.Attr == token) {
                    // watch out for nsdecl with no actual content
                    if (null != curDeclPrefix) {
                        PushNamespace(curDeclPrefix, string.Empty, false);
                        curDeclPrefix = null;
                    }
                    // do we need to grow the array?
                    if (this.attrCount == this.attributes.Length)
                        GrowAttributes();
                    // note: ParseMB32 _must_ happen _before_ we grab this.pos...
                    QName n = this.symbolTables.qnametable[ReadQNameRef()];
                    this.attributes[this.attrCount].Set(n, (int)this.pos);
                    if (n.prefix == "xml") {
                        if (n.localname == "lang") {
                            xmllang = this.attrCount;
                        }
                        else if (n.localname == "space") {
                            xmlspace = this.attrCount;
                        }
                    }
                    else if (Ref.Equal(n.namespaceUri, this.nsxmlns)) {
                        // push namespace when we get the value
                        curDeclPrefix = n.localname;
                        if (curDeclPrefix == "xmlns")
                            curDeclPrefix = string.Empty;
                    }
                    else if (n.prefix.Length != 0) {
                        if (n.namespaceUri.Length == 0)
                            throw new XmlException(Res.Xml_PrefixForEmptyNs, String.Empty);
                        this.PushNamespace(n.prefix, n.namespaceUri, true);
                    }
                    else if (n.namespaceUri.Length != 0) {
                        throw ThrowXmlException(Res.XmlBinary_AttrWithNsNoPrefix, n.localname, n.namespaceUri);
                    }
                    this.attrCount++;
                    lastWasValue = false;
                }
                else {
                    // first scan over token to make sure it is a value token
                    ScanOverValue(token, true, true);
                    // don't allow lists of values
                    if (lastWasValue) {
                        throw ThrowNotSupported(Res.XmlBinary_ListsOfValuesNotSupported);
                    }

                    // if char checking is on, we need to scan text values to
                    // validate that they don't use invalid CharData, so we
                    // might as well store the saved string for quick attr value access
                    string val = this.stringValue;
                    if (null != val) {
                        this.attributes[this.attrCount - 1].val = val;
                        this.stringValue = null;
                    }
                    // namespace decls can only have text values, and should only
                    // have a single value, so we just grab it here...
                    if (null != curDeclPrefix) {
                        string nsuri = this.xnt.Add(ValueAsString(token));
                        PushNamespace(curDeclPrefix, nsuri, false);
                        curDeclPrefix = null;
                    }
                    lastWasValue = true;
                }
            }

            if (xmlspace != -1) {
                string val = GetAttributeText(xmlspace);
                XmlSpace xs = XmlSpace.None;
                if (val == "preserve")
                    xs = XmlSpace.Preserve;
                else if (val == "default")
                    xs = XmlSpace.Default;
                this.elementStack[this.elemDepth].xmlSpace = xs;
                this.xmlspacePreserve = (XmlSpace.Preserve == xs);
            }
            if (xmllang != -1) {
                this.elementStack[this.elemDepth].xmlLang = GetAttributeText(xmllang);
            }

            if (this.attrCount < 200)
                SimpleCheckForDuplicateAttributes();
            else
                HashCheckForDuplicateAttributes();
        }

        void SimpleCheckForDuplicateAttributes() {
            for (int i = 0; i < this.attrCount; i++) {
                string localname, namespaceUri;
                this.attributes[i].GetLocalnameAndNamespaceUri(out localname, out namespaceUri);
                for (int j = i + 1; j < this.attrCount; j++) {
                    if (this.attributes[j].MatchNS(localname, namespaceUri))
                        throw new XmlException(Res.Xml_DupAttributeName, this.attributes[i].name.ToString());
                }
            }
        }

        void HashCheckForDuplicateAttributes() {
            int tblSize = 256;
            while (tblSize < this.attrCount)
                tblSize = checked(tblSize * 2);
            if (this.attrHashTbl.Length < tblSize)
                this.attrHashTbl = new int[tblSize];
            for (int i = 0; i < this.attrCount; i++) {
                string localname, namespaceUri;
                int hash = this.attributes[i].GetLocalnameAndNamespaceUriAndHash(hasher, out localname, out namespaceUri);
                int index = hash & (tblSize - 1);
                int next = this.attrHashTbl[index];
                this.attrHashTbl[index] = i + 1;
                this.attributes[i].prevHash = next;
                while (next != 0) {
                    next--;
                    if (this.attributes[next].MatchHashNS(hash, localname, namespaceUri)) {
                        throw new XmlException(Res.Xml_DupAttributeName, this.attributes[i].name.ToString());
                    }
                    next = this.attributes[next].prevHash;
                }
            }
            Array.Clear(this.attrHashTbl, 0, tblSize);
        }

        string XmlDeclValue() {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < this.attrCount; i++) {
                if (i > 0)
                    sb.Append(' ');
                sb.Append(this.attributes[i].name.localname);
                sb.Append("=\"");
                sb.Append(this.attributes[i].val);
                sb.Append('"');
            }
            return sb.ToString();
        }

        string CDATAValue() {
            Debug.Assert(this.stringValue == null, "this.stringValue == null");
            Debug.Assert(this.token == BinXmlToken.CData, "this.token == BinXmlToken.CData");
            String value = GetString(this.tokDataPos, this.tokLen);
            StringBuilder sb = null;
            while (PeekToken() == BinXmlToken.CData) {
                this.pos++; // skip over token byte
                if (sb == null) {
                    sb = new StringBuilder(value.Length + value.Length / 2);
                    sb.Append(value);
                }
                sb.Append(ParseText());
            }
            if (sb != null)
                value = sb.ToString();
            this.stringValue = value;
            return value;
        }

        void FinishCDATA() {
            for (; ; ) {
                switch (PeekToken()) {
                    case BinXmlToken.CData:
                        // skip
                        this.pos++;
                        int pos;
                        ScanText(out pos);
                        // try again
                        break;
                    case BinXmlToken.EndCData:
                        // done... on to next token...
                        this.pos++;
                        return;
                    default:
                        throw new XmlException(Res.XmlBin_MissingEndCDATA);
                }
            }
        }

        void FinishEndElement() {
            NamespaceDecl nsdecls = this.elementStack[this.elemDepth].Clear();
            this.PopNamespaces(nsdecls);
            this.elemDepth--;
        }

        bool ReadDoc() {
            switch (this.nodetype) {
                case XmlNodeType.CDATA:
                    FinishCDATA();
                    break;
                case XmlNodeType.EndElement:
                    FinishEndElement();
                    break;
                case XmlNodeType.Element:
                    if (this.isEmpty) {
                        FinishEndElement();
                        this.isEmpty = false;
                    }
                    break;
            }

        Read:
            // clear existing state
            this.nodetype = XmlNodeType.None;
            this.mark = -1;
            if (this.qnameOther.localname.Length != 0)
                this.qnameOther.Clear();

            ClearAttributes();
            this.attrCount = 0;
            this.valueType = TypeOfString;
            this.stringValue = null;
            this.hasTypedValue = false;

            this.token = NextToken();
            switch (this.token) {
                case BinXmlToken.EOF:
                    if (this.elemDepth > 0)
                        throw new XmlException(Res.Xml_UnexpectedEOF1, (string[])null);
                    this.state = ScanState.EOF;
                    return false;

                case BinXmlToken.Element:
                    ImplReadElement();
                    break;

                case BinXmlToken.EndElem:
                    ImplReadEndElement();
                    break;

                case BinXmlToken.DocType:
                    ImplReadDoctype();
                    if (this.dtdProcessing == DtdProcessing.Ignore)
                        goto Read;
                    // nested, don't report doctype
                    if (prevNameInfo != null)
                        goto Read;
                    break;

                case BinXmlToken.PI:
                    ImplReadPI();
                    if (this.ignorePIs)
                        goto Read;
                    break;

                case BinXmlToken.Comment:
                    ImplReadComment();
                    if (this.ignoreComments)
                        goto Read;
                    break;

                case BinXmlToken.CData:
                    ImplReadCDATA();
                    break;

                case BinXmlToken.Nest:
                    ImplReadNest();
                    // parse first token in nested document
                    sniffed = false;
                    return ReadInit(true);

                case BinXmlToken.EndNest:
                    if (null == this.prevNameInfo)
                        goto default;
                    ImplReadEndNest();
                    return ReadDoc();

                case BinXmlToken.XmlText:
                    ImplReadXmlText();
                    break;

                // text values
                case BinXmlToken.SQL_BIT:
                case BinXmlToken.SQL_TINYINT:
                case BinXmlToken.SQL_SMALLINT:
                case BinXmlToken.SQL_INT:
                case BinXmlToken.SQL_BIGINT:
                case BinXmlToken.SQL_REAL:
                case BinXmlToken.SQL_FLOAT:
                case BinXmlToken.SQL_MONEY:
                case BinXmlToken.SQL_SMALLMONEY:
                case BinXmlToken.SQL_DATETIME:
                case BinXmlToken.SQL_SMALLDATETIME:
                case BinXmlToken.SQL_DECIMAL:
                case BinXmlToken.SQL_NUMERIC:
                case BinXmlToken.XSD_DECIMAL:
                case BinXmlToken.SQL_UUID:
                case BinXmlToken.SQL_VARBINARY:
                case BinXmlToken.SQL_BINARY:
                case BinXmlToken.SQL_IMAGE:
                case BinXmlToken.SQL_UDT:
                case BinXmlToken.XSD_KATMAI_DATE:
                case BinXmlToken.XSD_KATMAI_DATETIME:
                case BinXmlToken.XSD_KATMAI_TIME:
                case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                case BinXmlToken.XSD_BINHEX:
                case BinXmlToken.XSD_BASE64:
                case BinXmlToken.SQL_CHAR:
                case BinXmlToken.SQL_VARCHAR:
                case BinXmlToken.SQL_TEXT:
                case BinXmlToken.SQL_NCHAR:
                case BinXmlToken.SQL_NVARCHAR:
                case BinXmlToken.SQL_NTEXT:
                case BinXmlToken.XSD_BOOLEAN:
                case BinXmlToken.XSD_TIME:
                case BinXmlToken.XSD_DATETIME:
                case BinXmlToken.XSD_DATE:
                case BinXmlToken.XSD_BYTE:
                case BinXmlToken.XSD_UNSIGNEDSHORT:
                case BinXmlToken.XSD_UNSIGNEDINT:
                case BinXmlToken.XSD_UNSIGNEDLONG:
                case BinXmlToken.XSD_QNAME:
                    ImplReadData(this.token);
                    if (XmlNodeType.Text == this.nodetype)
                        CheckAllowContent();
                    else if (this.ignoreWhitespace && !this.xmlspacePreserve)
                        goto Read; // skip to next token
                    return true;

                default:
                    throw ThrowUnexpectedToken(token);
            }

            return true;
        }

        void ImplReadData(BinXmlToken tokenType) {
            Debug.Assert(this.mark < 0);
            this.mark = this.pos;

            switch (tokenType) {
                case BinXmlToken.SQL_CHAR:
                case BinXmlToken.SQL_VARCHAR:
                case BinXmlToken.SQL_TEXT:
                case BinXmlToken.SQL_NCHAR:
                case BinXmlToken.SQL_NVARCHAR:
                case BinXmlToken.SQL_NTEXT:
                    this.valueType = TypeOfString;
                    this.hasTypedValue = false;
                    break;
                default:
                    this.valueType = GetValueType(this.token);
                    this.hasTypedValue = true;
                    break;
            }

            this.nodetype = ScanOverValue(this.token, false, true);

            // we don't support lists of values
            BinXmlToken tNext = PeekNextToken();
            switch (tNext) {
                case BinXmlToken.SQL_BIT:
                case BinXmlToken.SQL_TINYINT:
                case BinXmlToken.SQL_SMALLINT:
                case BinXmlToken.SQL_INT:
                case BinXmlToken.SQL_BIGINT:
                case BinXmlToken.SQL_REAL:
                case BinXmlToken.SQL_FLOAT:
                case BinXmlToken.SQL_MONEY:
                case BinXmlToken.SQL_SMALLMONEY:
                case BinXmlToken.SQL_DATETIME:
                case BinXmlToken.SQL_SMALLDATETIME:
                case BinXmlToken.SQL_DECIMAL:
                case BinXmlToken.SQL_NUMERIC:
                case BinXmlToken.XSD_DECIMAL:
                case BinXmlToken.SQL_UUID:
                case BinXmlToken.SQL_VARBINARY:
                case BinXmlToken.SQL_BINARY:
                case BinXmlToken.SQL_IMAGE:
                case BinXmlToken.SQL_UDT:
                case BinXmlToken.XSD_KATMAI_DATE:
                case BinXmlToken.XSD_KATMAI_DATETIME:
                case BinXmlToken.XSD_KATMAI_TIME:
                case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                case BinXmlToken.XSD_BINHEX:
                case BinXmlToken.XSD_BASE64:
                case BinXmlToken.SQL_CHAR:
                case BinXmlToken.SQL_VARCHAR:
                case BinXmlToken.SQL_TEXT:
                case BinXmlToken.SQL_NCHAR:
                case BinXmlToken.SQL_NVARCHAR:
                case BinXmlToken.SQL_NTEXT:
                case BinXmlToken.XSD_BOOLEAN:
                case BinXmlToken.XSD_TIME:
                case BinXmlToken.XSD_DATETIME:
                case BinXmlToken.XSD_DATE:
                case BinXmlToken.XSD_BYTE:
                case BinXmlToken.XSD_UNSIGNEDSHORT:
                case BinXmlToken.XSD_UNSIGNEDINT:
                case BinXmlToken.XSD_UNSIGNEDLONG:
                case BinXmlToken.XSD_QNAME:
                    throw ThrowNotSupported(Res.XmlBinary_ListsOfValuesNotSupported);
                default:
                    break;
            }
        }

        void ImplReadElement() {
            if (3 != this.docState || 9 != this.docState) {
                switch (this.docState) {
                    case 0:
                        this.docState = 9;
                        break;
                    case 1:
                    case 2:
                        this.docState = 3;
                        break;
                    case -1:
                        throw ThrowUnexpectedToken(this.token);
                    default:
                        break;
                }
            }
            this.elemDepth++;
            if (this.elemDepth == this.elementStack.Length)
                GrowElements();
            QName qname = this.symbolTables.qnametable[ReadQNameRef()];
            this.qnameOther = this.qnameElement = qname;
            this.elementStack[this.elemDepth].Set(qname, this.xmlspacePreserve);
            this.PushNamespace(qname.prefix, qname.namespaceUri, true);
            BinXmlToken t = PeekNextToken();
            if (BinXmlToken.Attr == t) {
                ScanAttributes();
                t = PeekNextToken();
            }
            GenerateImpliedXmlnsAttrs();
            if (BinXmlToken.EndElem == t) {
                NextToken(); // move over token...
                this.isEmpty = true;
            }
            else if (BinXmlToken.SQL_NVARCHAR == t) {
                if (this.mark < 0)
                    this.mark = this.pos;
                // skip over token byte
                this.pos++;
                // is this a zero-length string?  if yes, skip it.  
                // (It just indicates that this is _not_ an empty element)
                // Also make sure that the following token is an EndElem
                if (0 == ReadByte()) {
                    if (BinXmlToken.EndElem != (BinXmlToken)ReadByte()) {
                        Debug.Assert(this.pos >= 3);
                        this.pos -= 3; // jump back to start of NVarChar token
                    }
                    else {
                        Debug.Assert(this.pos >= 1);
                        this.pos -= 1; // jump back to EndElem token
                    }
                }
                else {
                    Debug.Assert(this.pos >= 2);
                    this.pos -= 2; // jump back to start of NVarChar token
                }
            }
            this.nodetype = XmlNodeType.Element;
            this.valueType = TypeOfObject;
            this.posAfterAttrs = this.pos;
        }

        void ImplReadEndElement() {
            if (this.elemDepth == 0)
                throw ThrowXmlException(Res.Xml_UnexpectedEndTag);
            int index = this.elemDepth;
            if (1 == index && 3 == this.docState)
                this.docState = -1;
            this.qnameOther = this.elementStack[index].name;
            this.xmlspacePreserve = this.elementStack[index].xmlspacePreserve;
            this.nodetype = XmlNodeType.EndElement;
        }

        void ImplReadDoctype() {
            if (this.dtdProcessing == DtdProcessing.Prohibit)
                throw ThrowXmlException(Res.Xml_DtdIsProhibited);
            // 0=>auto, 1=>doc/pre-dtd, 2=>doc/pre-elem, 3=>doc/instance -1=>doc/post-elem, 9=>frag
            switch (this.docState) {
                case 0: // 0=>auto
                case 1: // 1=>doc/pre-dtd
                    break;
                case 9: // 9=>frag
                    throw ThrowXmlException(Res.Xml_DtdNotAllowedInFragment);
                default: // 2=>doc/pre-elem, 3=>doc/instance -1=>doc/post-elem
                    throw ThrowXmlException(Res.Xml_BadDTDLocation);
            }
            this.docState = 2;
            this.qnameOther.localname = ParseText();
            if (BinXmlToken.System == PeekToken()) {
                this.pos++;
                this.attributes[this.attrCount++].Set(new QName(string.Empty, this.xnt.Add("SYSTEM"), string.Empty), ParseText());
            }
            if (BinXmlToken.Public == PeekToken()) {
                this.pos++;
                this.attributes[this.attrCount++].Set(new QName(string.Empty, this.xnt.Add("PUBLIC"), string.Empty), ParseText());
            }
            if (BinXmlToken.Subset == PeekToken()) {
                this.pos++;
                this.mark = this.pos;
                this.tokLen = ScanText(out this.tokDataPos);
            }
            else {
                this.tokLen = this.tokDataPos = 0;
            }
            this.nodetype = XmlNodeType.DocumentType;
            this.posAfterAttrs = this.pos;
        }

        void ImplReadPI() {
            this.qnameOther.localname = this.symbolTables.symtable[ReadNameRef()];
            this.mark = this.pos;
            this.tokLen = ScanText(out this.tokDataPos);
            this.nodetype = XmlNodeType.ProcessingInstruction;
        }

        void ImplReadComment() {
            this.nodetype = XmlNodeType.Comment;
            this.mark = this.pos;
            this.tokLen = ScanText(out this.tokDataPos);
        }

        void ImplReadCDATA() {
            CheckAllowContent();
            this.nodetype = XmlNodeType.CDATA;
            this.mark = this.pos;
            this.tokLen = ScanText(out this.tokDataPos);
        }

        void ImplReadNest() {
            CheckAllowContent();
            // push current nametables
            this.prevNameInfo = new NestedBinXml(this.symbolTables, this.docState, this.prevNameInfo);
            this.symbolTables.Init();
            this.docState = 0; // auto
        }

        void ImplReadEndNest() {
            NestedBinXml nested = this.prevNameInfo;
            this.symbolTables = nested.symbolTables;
            this.docState = nested.docState;
            this.prevNameInfo = nested.next;
        }

        void ImplReadXmlText() {
            CheckAllowContent();
            string xmltext = ParseText();
            XmlNamespaceManager xnm = new XmlNamespaceManager(this.xnt);
            foreach (NamespaceDecl decl in this.namespaces.Values) {
                if (decl.scope > 0) {
#if DEBUG
                    if ((object)decl.prefix != (object)this.xnt.Get(decl.prefix))
                        throw new Exception("Prefix not interned: \'" + decl.prefix + "\'");
                    if ((object)decl.uri != (object)this.xnt.Get(decl.uri))
                        throw new Exception("Uri not interned: \'" + decl.uri + "\'");
#endif
                    xnm.AddNamespace(decl.prefix, decl.uri);
                }
            }
            XmlReaderSettings settings = this.Settings;
            settings.ReadOnly = false;
            settings.NameTable = this.xnt;
            settings.DtdProcessing = DtdProcessing.Prohibit;
            if (0 != this.elemDepth) {
                settings.ConformanceLevel = ConformanceLevel.Fragment;
            }
            settings.ReadOnly = true;
            XmlParserContext xpc = new XmlParserContext(this.xnt, xnm, this.XmlLang, this.XmlSpace);
            this.textXmlReader = new XmlTextReaderImpl(xmltext, xpc, settings);
            if (!this.textXmlReader.Read()
                || ((this.textXmlReader.NodeType == XmlNodeType.XmlDeclaration)
                    && !this.textXmlReader.Read())) {
                this.state = ScanState.Doc;
                ReadDoc();
            }
            else {
                this.state = ScanState.XmlText;
                UpdateFromTextReader();
            }
        }

        void UpdateFromTextReader() {
            XmlReader r = this.textXmlReader;
            this.nodetype = r.NodeType;
            this.qnameOther.prefix = r.Prefix;
            this.qnameOther.localname = r.LocalName;
            this.qnameOther.namespaceUri = r.NamespaceURI;
            this.valueType = r.ValueType;
            this.isEmpty = r.IsEmptyElement;
        }

        bool UpdateFromTextReader(bool needUpdate) {
            if (needUpdate)
                UpdateFromTextReader();
            return needUpdate;
        }

        void CheckAllowContent() {
            switch (this.docState) {
                case 0: // auto
                    this.docState = 9;
                    break;
                case 9: // conformance = fragment
                case 3:
                    break;
                default:
                    throw ThrowXmlException(Res.Xml_InvalidRootData);
            }
        }

        private void GenerateTokenTypeMap() {
            Type[] map = new Type[256];
            map[(int)BinXmlToken.XSD_BOOLEAN] = typeof(System.Boolean);
            map[(int)BinXmlToken.SQL_TINYINT] = typeof(System.Byte);
            map[(int)BinXmlToken.XSD_BYTE] = typeof(System.SByte);
            map[(int)BinXmlToken.SQL_SMALLINT] = typeof(Int16);
            map[(int)BinXmlToken.XSD_UNSIGNEDSHORT] = typeof(UInt16);
            map[(int)BinXmlToken.XSD_UNSIGNEDINT] = typeof(UInt32);
            map[(int)BinXmlToken.SQL_REAL] = typeof(Single);
            map[(int)BinXmlToken.SQL_FLOAT] = typeof(Double);
            map[(int)BinXmlToken.SQL_BIGINT] = typeof(Int64);
            map[(int)BinXmlToken.XSD_UNSIGNEDLONG] = typeof(UInt64);
            map[(int)BinXmlToken.XSD_QNAME] = typeof(XmlQualifiedName);
            Type TypeOfInt32 = typeof(System.Int32);
            map[(int)BinXmlToken.SQL_BIT] = TypeOfInt32;
            map[(int)BinXmlToken.SQL_INT] = TypeOfInt32;
            Type TypeOfDecimal = typeof(System.Decimal);
            map[(int)BinXmlToken.SQL_SMALLMONEY] = TypeOfDecimal;
            map[(int)BinXmlToken.SQL_MONEY] = TypeOfDecimal;
            map[(int)BinXmlToken.SQL_DECIMAL] = TypeOfDecimal;
            map[(int)BinXmlToken.SQL_NUMERIC] = TypeOfDecimal;
            map[(int)BinXmlToken.XSD_DECIMAL] = TypeOfDecimal;
            Type TypeOfDateTime = typeof(System.DateTime);
            map[(int)BinXmlToken.SQL_SMALLDATETIME] = TypeOfDateTime;
            map[(int)BinXmlToken.SQL_DATETIME] = TypeOfDateTime;
            map[(int)BinXmlToken.XSD_TIME] = TypeOfDateTime;
            map[(int)BinXmlToken.XSD_DATETIME] = TypeOfDateTime;
            map[(int)BinXmlToken.XSD_DATE] = TypeOfDateTime;
            map[(int)BinXmlToken.XSD_KATMAI_DATE] = TypeOfDateTime;
            map[(int)BinXmlToken.XSD_KATMAI_DATETIME] = TypeOfDateTime;
            map[(int)BinXmlToken.XSD_KATMAI_TIME] = TypeOfDateTime;
            Type TypeOfDateTimeOffset = typeof( System.DateTimeOffset );
            map[(int)BinXmlToken.XSD_KATMAI_DATEOFFSET] = TypeOfDateTimeOffset;
            map[(int)BinXmlToken.XSD_KATMAI_DATETIMEOFFSET] = TypeOfDateTimeOffset;
            map[(int)BinXmlToken.XSD_KATMAI_TIMEOFFSET] = TypeOfDateTimeOffset;
            Type TypeOfByteArray = typeof( System.Byte[] );
            map[(int)BinXmlToken.SQL_VARBINARY] = TypeOfByteArray;
            map[(int)BinXmlToken.SQL_BINARY] = TypeOfByteArray;
            map[(int)BinXmlToken.SQL_IMAGE] = TypeOfByteArray;
            map[(int)BinXmlToken.SQL_UDT] = TypeOfByteArray;
            map[(int)BinXmlToken.XSD_BINHEX] = TypeOfByteArray;
            map[(int)BinXmlToken.XSD_BASE64] = TypeOfByteArray;
            map[(int)BinXmlToken.SQL_CHAR] = TypeOfString;
            map[(int)BinXmlToken.SQL_VARCHAR] = TypeOfString;
            map[(int)BinXmlToken.SQL_TEXT] = TypeOfString;
            map[(int)BinXmlToken.SQL_NCHAR] = TypeOfString;
            map[(int)BinXmlToken.SQL_NVARCHAR] = TypeOfString;
            map[(int)BinXmlToken.SQL_NTEXT] = TypeOfString;
            map[(int)BinXmlToken.SQL_UUID] = TypeOfString;
            if (TokenTypeMap == null)
                TokenTypeMap = map;
        }

        System.Type GetValueType(BinXmlToken token) {
            Type t = TokenTypeMap[(int)token];
            if (t == null)
                throw ThrowUnexpectedToken(token);
            return t;
        }

        // helper method...
        void ReScanOverValue(BinXmlToken token) {
            ScanOverValue(token, true, false);
        }

        XmlNodeType ScanOverValue(BinXmlToken token, bool attr, bool checkChars) {
            if (token == BinXmlToken.SQL_NVARCHAR) {
                if (this.mark < 0)
                    this.mark = this.pos;
                this.tokLen = ParseMB32();
                this.tokDataPos = this.pos;
                checked{this.pos += this.tokLen * 2;}
                Fill(-1);
                // check chars (if this is the first pass and settings.CheckCharacters was set)
                if (checkChars && this.checkCharacters) {
                    // check for invalid chardata
                    return CheckText(attr);
                }
                else if (!attr) { // attribute values are always reported as Text
                    // check for whitespace-only text
                    return CheckTextIsWS();
                }
                else {
                    return XmlNodeType.Text;
                }
            }
            else {
                return ScanOverAnyValue(token, attr, checkChars);
            }
        }

        XmlNodeType ScanOverAnyValue(BinXmlToken token, bool attr, bool checkChars) {
            if (this.mark < 0)
                this.mark = this.pos;
            checked {
                switch (token) {
                    case BinXmlToken.SQL_BIT:
                    case BinXmlToken.SQL_TINYINT:
                    case BinXmlToken.XSD_BOOLEAN:
                    case BinXmlToken.XSD_BYTE:
                        this.tokDataPos = this.pos;
                        this.tokLen = 1;
                        this.pos += 1;
                        break;

                    case BinXmlToken.SQL_SMALLINT:
                    case BinXmlToken.XSD_UNSIGNEDSHORT:
                        this.tokDataPos = this.pos;
                        this.tokLen = 2;
                        this.pos += 2;
                        break;

                    case BinXmlToken.SQL_INT:
                    case BinXmlToken.XSD_UNSIGNEDINT:
                    case BinXmlToken.SQL_REAL:
                    case BinXmlToken.SQL_SMALLMONEY:
                    case BinXmlToken.SQL_SMALLDATETIME:
                        this.tokDataPos = this.pos;
                        this.tokLen = 4;
                        this.pos += 4;
                        break;

                    case BinXmlToken.SQL_BIGINT:
                    case BinXmlToken.XSD_UNSIGNEDLONG:
                    case BinXmlToken.SQL_FLOAT:
                    case BinXmlToken.SQL_MONEY:
                    case BinXmlToken.SQL_DATETIME:
                    case BinXmlToken.XSD_TIME:
                    case BinXmlToken.XSD_DATETIME:
                    case BinXmlToken.XSD_DATE:
                        this.tokDataPos = this.pos;
                        this.tokLen = 8;
                        this.pos += 8;
                        break;

                    case BinXmlToken.SQL_UUID:
                        this.tokDataPos = this.pos;
                        this.tokLen = 16;
                        this.pos += 16;
                        break;

                    case BinXmlToken.SQL_DECIMAL:
                    case BinXmlToken.SQL_NUMERIC:
                    case BinXmlToken.XSD_DECIMAL:
                        this.tokDataPos = this.pos;
                        this.tokLen = ParseMB64();
                        this.pos += this.tokLen;
                        break;

                    case BinXmlToken.SQL_VARBINARY:
                    case BinXmlToken.SQL_BINARY:
                    case BinXmlToken.SQL_IMAGE:
                    case BinXmlToken.SQL_UDT:
                    case BinXmlToken.XSD_BINHEX:
                    case BinXmlToken.XSD_BASE64:
                        this.tokLen = ParseMB64();
                        this.tokDataPos = this.pos;
                        this.pos += this.tokLen;
                        break;

                    case BinXmlToken.SQL_CHAR:
                    case BinXmlToken.SQL_VARCHAR:
                    case BinXmlToken.SQL_TEXT:
                        this.tokLen = ParseMB64();
                        this.tokDataPos = this.pos;
                        this.pos += this.tokLen;
                        if (checkChars && this.checkCharacters) {
                            // check for invalid chardata
                            Fill(-1);
                            string val = ValueAsString(token);
                            XmlConvert.VerifyCharData(val, ExceptionType.ArgumentException, ExceptionType.XmlException);
                            this.stringValue = val;
                        }
                        break;

                    case BinXmlToken.SQL_NVARCHAR:
                    case BinXmlToken.SQL_NCHAR:
                    case BinXmlToken.SQL_NTEXT:
                        return ScanOverValue(BinXmlToken.SQL_NVARCHAR, attr, checkChars);

                    case BinXmlToken.XSD_QNAME:
                        this.tokDataPos = this.pos;
                        ParseMB32();
                        break;

                    case BinXmlToken.XSD_KATMAI_DATE:
                    case BinXmlToken.XSD_KATMAI_DATETIME:
                    case BinXmlToken.XSD_KATMAI_TIME:
                    case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                    case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                    case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                        VerifyVersion(2, token);
                        this.tokDataPos = this.pos;
                        this.tokLen = GetXsdKatmaiTokenLength(token);
                        this.pos += tokLen;
                        break;

                    default:
                        throw ThrowUnexpectedToken(token);
                }
            }
            Fill(-1);
            return XmlNodeType.Text;
        }

        unsafe XmlNodeType CheckText(bool attr) {
            Debug.Assert(this.checkCharacters, "this.checkCharacters");
            // assert that size is an even number
            Debug.Assert(0 == ((this.pos - this.tokDataPos) & 1), "Data size should not be odd");
            // grab local copy (perf)
            XmlCharType xmlCharType = this.xmlCharType;

            fixed (byte* pb = this.data) {
                int end = this.pos;
                int pos = this.tokDataPos;

                if (!attr) {
                    // scan if this is whitespace
                    for (; ; ) {
                        int posNext = pos + 2;
                        if (posNext > end)
                            return this.xmlspacePreserve ? XmlNodeType.SignificantWhitespace : XmlNodeType.Whitespace;
                        if (pb[pos + 1] != 0 || (xmlCharType.charProperties[pb[pos]] & XmlCharType.fWhitespace) == 0)
                            break;
                        pos = posNext;
                    }
                }

                for (; ; ) {
                    char ch;
                    for (; ; ) {
                        int posNext = pos + 2;
                        if (posNext > end)
                            return XmlNodeType.Text;
                        ch = (char)(pb[pos] | ((int)(pb[pos + 1]) << 8));
                        if ((xmlCharType.charProperties[ch] & XmlCharType.fCharData) == 0)
                            break;
                        pos = posNext;
                    }

                    if (!XmlCharType.IsHighSurrogate(ch)) {
                        throw XmlConvert.CreateInvalidCharException(ch, '\0', ExceptionType.XmlException);
                    }
                    else {
                        if ((pos + 4) > end) {
                            throw ThrowXmlException(Res.Xml_InvalidSurrogateMissingLowChar);
                        }
                        char chNext = (char)(pb[pos + 2] | ((int)(pb[pos + 3]) << 8));
                        if (!XmlCharType.IsLowSurrogate(chNext)) {
                            throw XmlConvert.CreateInvalidSurrogatePairException(ch, chNext);
                        }
                    }
                    pos += 4;
                }
            }
        }

        XmlNodeType CheckTextIsWS() {
            Debug.Assert(!this.checkCharacters, "!this.checkCharacters");
            byte[] data = this.data;
            // assert that size is an even number
            Debug.Assert(0 == ((this.pos - this.tokDataPos) & 1), "Data size should not be odd");
            for (int pos = this.tokDataPos; pos < this.pos; pos += 2) {
                if (0 != data[pos + 1])
                    goto NonWSText;
                switch (data[pos]) {
                    case 0x09: // tab
                    case 0x0A: // nl
                    case 0x0D: // cr
                    case 0x20: // space
                        break;
                    default:
                        goto NonWSText;
                }
            }
            if (this.xmlspacePreserve)
                return XmlNodeType.SignificantWhitespace;
            return XmlNodeType.Whitespace;
        NonWSText:
            return XmlNodeType.Text;
        }

        void CheckValueTokenBounds() {
            if ((this.end - this.tokDataPos) < this.tokLen)
                throw ThrowXmlException(Res.Xml_UnexpectedEOF1);
        }

        int GetXsdKatmaiTokenLength(BinXmlToken token) {
            byte scale;
            switch (token) {
                case BinXmlToken.XSD_KATMAI_DATE:
                    // SQL Katmai type DATE = date(3b)
                    return 3;                                           
                case BinXmlToken.XSD_KATMAI_TIME:
                case BinXmlToken.XSD_KATMAI_DATETIME:
                    // SQL Katmai type DATETIME2 = scale(1b) + time(3-5b) + date(3b)
                    Fill(0);
                    scale = this.data[this.pos];
                    return 4 + XsdKatmaiTimeScaleToValueLength(scale);  
                case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                    // SQL Katmai type DATETIMEOFFSET = scale(1b) + time(3-5b) + date(3b) + zone(2b)
                    Fill(0);
                    scale = this.data[this.pos];
                    return 6 + XsdKatmaiTimeScaleToValueLength(scale);
                default:
                    throw ThrowUnexpectedToken(this.token);
            }
        }

        int XsdKatmaiTimeScaleToValueLength(byte scale) {
            if (scale > 7) {
                throw new XmlException(Res.SqlTypes_ArithOverflow, (string)null);
            }
            return XsdKatmaiTimeScaleToValueLengthMap[scale];
        }

        long ValueAsLong() {
            CheckValueTokenBounds();
            switch (this.token) {
                case BinXmlToken.SQL_BIT:
                case BinXmlToken.SQL_TINYINT: {
                        byte v = this.data[this.tokDataPos];
                        return v;
                    }

                case BinXmlToken.XSD_BYTE: {
                        sbyte v = unchecked((sbyte)this.data[this.tokDataPos]);
                        return v;
                    }

                case BinXmlToken.SQL_SMALLINT:
                    return GetInt16(this.tokDataPos);

                case BinXmlToken.SQL_INT:
                    return GetInt32(this.tokDataPos);

                case BinXmlToken.SQL_BIGINT:
                    return GetInt64(this.tokDataPos);

                case BinXmlToken.XSD_UNSIGNEDSHORT:
                    return GetUInt16(this.tokDataPos);

                case BinXmlToken.XSD_UNSIGNEDINT:
                    return GetUInt32(this.tokDataPos);

                case BinXmlToken.XSD_UNSIGNEDLONG: {
                        ulong v = GetUInt64(this.tokDataPos);
                        return checked((long)v);
                    }

                case BinXmlToken.SQL_REAL:
                case BinXmlToken.SQL_FLOAT: {
                        double v = ValueAsDouble();
                        return (long)v;
                    }

                case BinXmlToken.SQL_MONEY:
                case BinXmlToken.SQL_SMALLMONEY:
                case BinXmlToken.SQL_DECIMAL:
                case BinXmlToken.SQL_NUMERIC:
                case BinXmlToken.XSD_DECIMAL: {
                        Decimal v = ValueAsDecimal();
                        return (long)v;
                    }

                default:
                    throw ThrowUnexpectedToken(this.token);
            }
        }

        ulong ValueAsULong() {
            if (BinXmlToken.XSD_UNSIGNEDLONG == this.token) {
                CheckValueTokenBounds();
                return GetUInt64(this.tokDataPos);
            }
            else {
                throw ThrowUnexpectedToken(this.token);
            }
        }

        Decimal ValueAsDecimal() {
            CheckValueTokenBounds();
            switch (token) {
                case BinXmlToken.SQL_BIT:
                case BinXmlToken.SQL_TINYINT:
                case BinXmlToken.SQL_SMALLINT:
                case BinXmlToken.SQL_INT:
                case BinXmlToken.SQL_BIGINT:
                case BinXmlToken.XSD_BYTE:
                case BinXmlToken.XSD_UNSIGNEDSHORT:
                case BinXmlToken.XSD_UNSIGNEDINT:
                    return new Decimal(ValueAsLong());

                case BinXmlToken.XSD_UNSIGNEDLONG:
                    return new Decimal(ValueAsULong());

                case BinXmlToken.SQL_REAL:
                    return new Decimal(GetSingle(this.tokDataPos));

                case BinXmlToken.SQL_FLOAT:
                    return new Decimal(GetDouble(this.tokDataPos));

                case BinXmlToken.SQL_SMALLMONEY: {
                        BinXmlSqlMoney v = new BinXmlSqlMoney(GetInt32(this.tokDataPos));
                        return v.ToDecimal();
                    }
                case BinXmlToken.SQL_MONEY: {
                        BinXmlSqlMoney v = new BinXmlSqlMoney(GetInt64(this.tokDataPos));
                        return v.ToDecimal();
                    }

                case BinXmlToken.XSD_DECIMAL:
                case BinXmlToken.SQL_DECIMAL:
                case BinXmlToken.SQL_NUMERIC: {
                        BinXmlSqlDecimal v = new BinXmlSqlDecimal(this.data, this.tokDataPos, token == BinXmlToken.XSD_DECIMAL);
                        return v.ToDecimal();
                    }

                default:
                    throw ThrowUnexpectedToken(this.token);
            }
        }

        double ValueAsDouble() {
            CheckValueTokenBounds();
            switch (token) {
                case BinXmlToken.SQL_BIT:
                case BinXmlToken.SQL_TINYINT:
                case BinXmlToken.SQL_SMALLINT:
                case BinXmlToken.SQL_INT:
                case BinXmlToken.SQL_BIGINT:
                case BinXmlToken.XSD_BYTE:
                case BinXmlToken.XSD_UNSIGNEDSHORT:
                case BinXmlToken.XSD_UNSIGNEDINT:
                    return (double)ValueAsLong();

                case BinXmlToken.XSD_UNSIGNEDLONG:
                    return (double)ValueAsULong();

                case BinXmlToken.SQL_REAL:
                    return GetSingle(this.tokDataPos);

                case BinXmlToken.SQL_FLOAT:
                    return GetDouble(this.tokDataPos);

                case BinXmlToken.SQL_SMALLMONEY:
                case BinXmlToken.SQL_MONEY:
                case BinXmlToken.XSD_DECIMAL:
                case BinXmlToken.SQL_DECIMAL:
                case BinXmlToken.SQL_NUMERIC:
                    return (double)ValueAsDecimal();

                default:
                    throw ThrowUnexpectedToken(this.token);
            }
        }

        DateTime ValueAsDateTime() {
            CheckValueTokenBounds();
            switch (token) {
                case BinXmlToken.SQL_DATETIME: {
                        int pos = this.tokDataPos;
                        int dateticks; uint timeticks;
                        dateticks = GetInt32(pos);
                        timeticks = GetUInt32(pos + 4);
                        return BinXmlDateTime.SqlDateTimeToDateTime(dateticks, timeticks);
                    }

                case BinXmlToken.SQL_SMALLDATETIME: {
                        int pos = this.tokDataPos;
                        short dateticks; ushort timeticks;
                        dateticks = GetInt16(pos);
                        timeticks = GetUInt16(pos + 2);
                        return BinXmlDateTime.SqlSmallDateTimeToDateTime(dateticks, timeticks);
                    }

                case BinXmlToken.XSD_TIME: {
                        long time = GetInt64(this.tokDataPos);
                        return BinXmlDateTime.XsdTimeToDateTime(time);
                    }

                case BinXmlToken.XSD_DATE: {
                        long time = GetInt64(this.tokDataPos);
                        return BinXmlDateTime.XsdDateToDateTime(time);
                    }

                case BinXmlToken.XSD_DATETIME: {
                        long time = GetInt64(this.tokDataPos);
                        return BinXmlDateTime.XsdDateTimeToDateTime(time);
                    }

                case BinXmlToken.XSD_KATMAI_DATE:
                    return BinXmlDateTime.XsdKatmaiDateToDateTime(this.data, this.tokDataPos);

                case BinXmlToken.XSD_KATMAI_DATETIME:
                    return BinXmlDateTime.XsdKatmaiDateTimeToDateTime(this.data, this.tokDataPos);

                case BinXmlToken.XSD_KATMAI_TIME:
                    return BinXmlDateTime.XsdKatmaiTimeToDateTime(this.data, this.tokDataPos);

                case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                    return BinXmlDateTime.XsdKatmaiDateOffsetToDateTime(this.data, this.tokDataPos);

                case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                    return BinXmlDateTime.XsdKatmaiDateTimeOffsetToDateTime(this.data, this.tokDataPos);

                case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                    return BinXmlDateTime.XsdKatmaiTimeOffsetToDateTime(this.data, this.tokDataPos);

                default:
                    throw ThrowUnexpectedToken(this.token);
            }
        }

        DateTimeOffset ValueAsDateTimeOffset() {
            CheckValueTokenBounds();
            switch (token) {
                case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                    return BinXmlDateTime.XsdKatmaiDateOffsetToDateTimeOffset(this.data, this.tokDataPos);

                case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                    return BinXmlDateTime.XsdKatmaiDateTimeOffsetToDateTimeOffset(this.data, this.tokDataPos);

                case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                    return BinXmlDateTime.XsdKatmaiTimeOffsetToDateTimeOffset(this.data, this.tokDataPos);

                default:
                    throw ThrowUnexpectedToken(this.token);
            }
        }


        string ValueAsDateTimeString() {
            CheckValueTokenBounds();
            switch (token) {
                case BinXmlToken.SQL_DATETIME: {
                        int pos = this.tokDataPos;
                        int dateticks; uint timeticks;
                        dateticks = GetInt32(pos);
                        timeticks = GetUInt32(pos + 4);
                        return BinXmlDateTime.SqlDateTimeToString(dateticks, timeticks);
                    }

                case BinXmlToken.SQL_SMALLDATETIME: {
                        int pos = this.tokDataPos;
                        short dateticks; ushort timeticks;
                        dateticks = GetInt16(pos);
                        timeticks = GetUInt16(pos + 2);
                        return BinXmlDateTime.SqlSmallDateTimeToString(dateticks, timeticks);
                    }

                case BinXmlToken.XSD_TIME: {
                        long time = GetInt64(this.tokDataPos);
                        return BinXmlDateTime.XsdTimeToString(time);
                    }

                case BinXmlToken.XSD_DATE: {
                        long time = GetInt64(this.tokDataPos);
                        return BinXmlDateTime.XsdDateToString(time);
                    }

                case BinXmlToken.XSD_DATETIME: {
                        long time = GetInt64(this.tokDataPos);
                        return BinXmlDateTime.XsdDateTimeToString(time);
                    }

                case BinXmlToken.XSD_KATMAI_DATE:
                    return BinXmlDateTime.XsdKatmaiDateToString(this.data, this.tokDataPos);

                case BinXmlToken.XSD_KATMAI_DATETIME:
                    return BinXmlDateTime.XsdKatmaiDateTimeToString(this.data, this.tokDataPos);

                case BinXmlToken.XSD_KATMAI_TIME:
                    return BinXmlDateTime.XsdKatmaiTimeToString(this.data, this.tokDataPos);

                case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                    return BinXmlDateTime.XsdKatmaiDateOffsetToString(this.data, this.tokDataPos);

                case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                    return BinXmlDateTime.XsdKatmaiDateTimeOffsetToString(this.data, this.tokDataPos);

                case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                    return BinXmlDateTime.XsdKatmaiTimeOffsetToString(this.data, this.tokDataPos);

                default:
                    throw ThrowUnexpectedToken(this.token);
            }
        }

        string ValueAsString(BinXmlToken token) {
            try {
                CheckValueTokenBounds();
                switch ( token ) {
                    case BinXmlToken.SQL_NCHAR:
                    case BinXmlToken.SQL_NVARCHAR:
                    case BinXmlToken.SQL_NTEXT:
                        return GetString( this.tokDataPos, this.tokLen );

                    case BinXmlToken.XSD_BOOLEAN: {
                            if ( 0 == this.data[this.tokDataPos] )
                                return "false";
                            else
                                return "true";
                        }

                    case BinXmlToken.SQL_BIT:
                    case BinXmlToken.SQL_TINYINT:
                    case BinXmlToken.SQL_SMALLINT:
                    case BinXmlToken.SQL_INT:
                    case BinXmlToken.SQL_BIGINT:
                    case BinXmlToken.XSD_BYTE:
                    case BinXmlToken.XSD_UNSIGNEDSHORT:
                    case BinXmlToken.XSD_UNSIGNEDINT:
                        return ValueAsLong().ToString( CultureInfo.InvariantCulture );

                    case BinXmlToken.XSD_UNSIGNEDLONG:
                        return ValueAsULong().ToString( CultureInfo.InvariantCulture );

                    case BinXmlToken.SQL_REAL:
                        return XmlConvert.ToString( GetSingle( this.tokDataPos ) );

                    case BinXmlToken.SQL_FLOAT:
                        return XmlConvert.ToString( GetDouble( this.tokDataPos ) );

                    case BinXmlToken.SQL_UUID: {
                            int a; short b, c;
                            int pos = this.tokDataPos;
                            a = GetInt32( pos );
                            b = GetInt16( pos + 4 );
                            c = GetInt16( pos + 6 );
                            Guid v = new Guid( a, b, c, data[pos + 8], data[pos + 9], data[pos + 10], data[pos + 11], data[pos + 12], data[pos + 13], data[pos + 14], data[pos + 15] );
                            return v.ToString();
                        }

                    case BinXmlToken.SQL_SMALLMONEY: {
                            BinXmlSqlMoney v = new BinXmlSqlMoney( GetInt32( this.tokDataPos ) );
                            return v.ToString();
                        }
                    case BinXmlToken.SQL_MONEY: {
                            BinXmlSqlMoney v = new BinXmlSqlMoney( GetInt64( this.tokDataPos ) );
                            return v.ToString();
                        }

                    case BinXmlToken.XSD_DECIMAL:
                    case BinXmlToken.SQL_DECIMAL:
                    case BinXmlToken.SQL_NUMERIC: {
                            BinXmlSqlDecimal v = new BinXmlSqlDecimal( this.data, this.tokDataPos, token == BinXmlToken.XSD_DECIMAL );
                            return v.ToString();
                        }

                    case BinXmlToken.SQL_CHAR:
                    case BinXmlToken.SQL_VARCHAR:
                    case BinXmlToken.SQL_TEXT: {
                            int pos = this.tokDataPos;
                            int codepage = GetInt32( pos );
                            Encoding enc = System.Text.Encoding.GetEncoding( codepage );
                            return enc.GetString( this.data, pos + 4, this.tokLen - 4 );
                        }

                    case BinXmlToken.SQL_VARBINARY:
                    case BinXmlToken.SQL_BINARY:
                    case BinXmlToken.SQL_IMAGE:
                    case BinXmlToken.SQL_UDT:
                    case BinXmlToken.XSD_BASE64: {
                            return Convert.ToBase64String( this.data, this.tokDataPos, this.tokLen );
                        }

                    case BinXmlToken.XSD_BINHEX:
                        return BinHexEncoder.Encode( this.data, this.tokDataPos, this.tokLen );

                    case BinXmlToken.SQL_DATETIME:
                    case BinXmlToken.SQL_SMALLDATETIME:
                    case BinXmlToken.XSD_TIME:
                    case BinXmlToken.XSD_DATE:
                    case BinXmlToken.XSD_DATETIME:
                    case BinXmlToken.XSD_KATMAI_DATE:
                    case BinXmlToken.XSD_KATMAI_DATETIME:
                    case BinXmlToken.XSD_KATMAI_TIME:
                    case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                    case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                    case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                        return ValueAsDateTimeString();

                    case BinXmlToken.XSD_QNAME: {
                            int nameNum = ParseMB32( this.tokDataPos );
                            if ( nameNum < 0 || nameNum >= this.symbolTables.qnameCount )
                                throw new XmlException( Res.XmlBin_InvalidQNameID, String.Empty );
                            QName qname = this.symbolTables.qnametable[nameNum];
                            if ( qname.prefix.Length == 0 )
                                return qname.localname;
                            else
                                return String.Concat( qname.prefix, ":", qname.localname );
                        }

                    default:
                        throw ThrowUnexpectedToken( this.token );
                }
            }
            catch {
                this.state = ScanState.Error;
                throw;
            }
        }

        object ValueAsObject(BinXmlToken token, bool returnInternalTypes) {
            CheckValueTokenBounds();
            switch (token) {
                case BinXmlToken.SQL_NCHAR:
                case BinXmlToken.SQL_NVARCHAR:
                case BinXmlToken.SQL_NTEXT:
                    return GetString(this.tokDataPos, this.tokLen);

                case BinXmlToken.XSD_BOOLEAN:
                    return (0 != this.data[this.tokDataPos]);

                case BinXmlToken.SQL_BIT:
                    return (Int32)this.data[this.tokDataPos];

                case BinXmlToken.SQL_TINYINT:
                    return this.data[this.tokDataPos];

                case BinXmlToken.SQL_SMALLINT:
                    return GetInt16(this.tokDataPos);

                case BinXmlToken.SQL_INT:
                    return GetInt32(this.tokDataPos);

                case BinXmlToken.SQL_BIGINT:
                    return GetInt64(this.tokDataPos);

                case BinXmlToken.XSD_BYTE: {
                        sbyte v = unchecked((sbyte)this.data[this.tokDataPos]);
                        return v;
                    }

                case BinXmlToken.XSD_UNSIGNEDSHORT:
                    return GetUInt16(this.tokDataPos);

                case BinXmlToken.XSD_UNSIGNEDINT:
                    return GetUInt32(this.tokDataPos);

                case BinXmlToken.XSD_UNSIGNEDLONG:
                    return GetUInt64(this.tokDataPos);

                case BinXmlToken.SQL_REAL:
                    return GetSingle(this.tokDataPos);

                case BinXmlToken.SQL_FLOAT:
                    return GetDouble(this.tokDataPos);

                case BinXmlToken.SQL_UUID: {
                        int a; short b, c;
                        int pos = this.tokDataPos;
                        a = GetInt32(pos);
                        b = GetInt16(pos + 4);
                        c = GetInt16(pos + 6);
                        Guid v = new Guid(a, b, c, data[pos + 8], data[pos + 9], data[pos + 10], data[pos + 11], data[pos + 12], data[pos + 13], data[pos + 14], data[pos + 15]);
                        return v.ToString();
                    }

                case BinXmlToken.SQL_SMALLMONEY: {
                        BinXmlSqlMoney v = new BinXmlSqlMoney(GetInt32(this.tokDataPos));
                        if (returnInternalTypes)
                            return v;
                        else
                            return v.ToDecimal();
                    }

                case BinXmlToken.SQL_MONEY: {
                        BinXmlSqlMoney v = new BinXmlSqlMoney(GetInt64(this.tokDataPos));
                        if (returnInternalTypes)
                            return v;
                        else
                            return v.ToDecimal();
                    }

                case BinXmlToken.XSD_DECIMAL:
                case BinXmlToken.SQL_DECIMAL:
                case BinXmlToken.SQL_NUMERIC: {
                        BinXmlSqlDecimal v = new BinXmlSqlDecimal(this.data, this.tokDataPos, token == BinXmlToken.XSD_DECIMAL);
                        if (returnInternalTypes)
                            return v;
                        else
                            return v.ToDecimal();
                    }

                case BinXmlToken.SQL_CHAR:
                case BinXmlToken.SQL_VARCHAR:
                case BinXmlToken.SQL_TEXT: {
                        int pos = this.tokDataPos;
                        int codepage = GetInt32(pos);
                        Encoding enc = System.Text.Encoding.GetEncoding(codepage);
                        return enc.GetString(this.data, pos + 4, this.tokLen - 4);
                    }

                case BinXmlToken.SQL_VARBINARY:
                case BinXmlToken.SQL_BINARY:
                case BinXmlToken.SQL_IMAGE:
                case BinXmlToken.SQL_UDT:
                case BinXmlToken.XSD_BASE64:
                case BinXmlToken.XSD_BINHEX: {
                        byte[] data = new byte[this.tokLen];
                        Array.Copy(this.data, this.tokDataPos, data, 0, this.tokLen);
                        return data;
                    }

                case BinXmlToken.SQL_DATETIME:
                case BinXmlToken.SQL_SMALLDATETIME:
                case BinXmlToken.XSD_TIME:
                case BinXmlToken.XSD_DATE:
                case BinXmlToken.XSD_DATETIME:
                case BinXmlToken.XSD_KATMAI_DATE:
                case BinXmlToken.XSD_KATMAI_DATETIME:
                case BinXmlToken.XSD_KATMAI_TIME:
                    return ValueAsDateTime();

                case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                    return ValueAsDateTimeOffset();

                case BinXmlToken.XSD_QNAME: {
                        int nameNum = ParseMB32(this.tokDataPos);
                        if (nameNum < 0 || nameNum >= this.symbolTables.qnameCount)
                            throw new XmlException(Res.XmlBin_InvalidQNameID, String.Empty);
                        QName qname = this.symbolTables.qnametable[nameNum];
                        return new XmlQualifiedName(qname.localname, qname.namespaceUri);
                    }

                default:
                    throw ThrowUnexpectedToken(this.token);
            }
        }

        XmlValueConverter GetValueConverter(XmlTypeCode typeCode) {
            XmlSchemaSimpleType xsst = DatatypeImplementation.GetSimpleTypeFromTypeCode(typeCode);
            return xsst.ValueConverter;
        }

        object ValueAs(BinXmlToken token, Type returnType, IXmlNamespaceResolver namespaceResolver) {
            object value;
            CheckValueTokenBounds();
            switch (token) {
                case BinXmlToken.SQL_NCHAR:
                case BinXmlToken.SQL_NVARCHAR:
                case BinXmlToken.SQL_NTEXT:
                    value = GetValueConverter(XmlTypeCode.UntypedAtomic).ChangeType(
                        GetString(this.tokDataPos, this.tokLen),
                        returnType, namespaceResolver);
                    break;

                case BinXmlToken.XSD_BOOLEAN:
                    value = GetValueConverter(XmlTypeCode.Boolean).ChangeType(
                        (0 != this.data[this.tokDataPos]),
                        returnType, namespaceResolver);
                    break;

                case BinXmlToken.SQL_BIT:
                    value = GetValueConverter(XmlTypeCode.NonNegativeInteger).ChangeType(
                        (Int32)this.data[this.tokDataPos],
                        returnType, namespaceResolver);
                    break;

                case BinXmlToken.SQL_TINYINT:
                    value = GetValueConverter(XmlTypeCode.UnsignedByte).ChangeType(
                        this.data[this.tokDataPos],
                        returnType, namespaceResolver);
                    break;

                case BinXmlToken.SQL_SMALLINT: {
                        int v = GetInt16(this.tokDataPos);
                        value = GetValueConverter(XmlTypeCode.Short).ChangeType(
                            v, returnType, namespaceResolver);
                        break;
                    }
                case BinXmlToken.SQL_INT: {
                        int v = GetInt32(this.tokDataPos);
                        value = GetValueConverter(XmlTypeCode.Int).ChangeType(
                            v, returnType, namespaceResolver);
                        break;
                    }
                case BinXmlToken.SQL_BIGINT: {
                        long v = GetInt64(this.tokDataPos);
                        value = GetValueConverter(XmlTypeCode.Long).ChangeType(
                            v, returnType, namespaceResolver);
                        break;
                    }
                case BinXmlToken.XSD_BYTE: {
                        value = GetValueConverter(XmlTypeCode.Byte).ChangeType(
                            (int)unchecked((sbyte)this.data[this.tokDataPos]),
                            returnType, namespaceResolver);
                        break;
                    }
                case BinXmlToken.XSD_UNSIGNEDSHORT: {
                        int v = GetUInt16(this.tokDataPos);
                        value = GetValueConverter(XmlTypeCode.UnsignedShort).ChangeType(
                            v, returnType, namespaceResolver);
                        break;
                    }
                case BinXmlToken.XSD_UNSIGNEDINT: {
                        long v = GetUInt32(this.tokDataPos);
                        value = GetValueConverter(XmlTypeCode.UnsignedInt).ChangeType(
                            v, returnType, namespaceResolver);
                        break;
                    }
                case BinXmlToken.XSD_UNSIGNEDLONG: {
                        Decimal v = (Decimal)GetUInt64(this.tokDataPos);
                        value = GetValueConverter(XmlTypeCode.UnsignedLong).ChangeType(
                            v, returnType, namespaceResolver);
                        break;
                    }
                case BinXmlToken.SQL_REAL: {
                        Single v = GetSingle(this.tokDataPos);
                        value = GetValueConverter(XmlTypeCode.Float).ChangeType(
                            v, returnType, namespaceResolver);
                        break;
                    }
                case BinXmlToken.SQL_FLOAT: {
                        Double v = GetDouble(this.tokDataPos);
                        value = GetValueConverter(XmlTypeCode.Double).ChangeType(
                            v, returnType, namespaceResolver);
                        break;
                    }
                case BinXmlToken.SQL_UUID:
                    value = GetValueConverter(XmlTypeCode.String).ChangeType(
                        this.ValueAsString(token), returnType, namespaceResolver);
                    break;

                case BinXmlToken.SQL_SMALLMONEY:
                    value = GetValueConverter(XmlTypeCode.Decimal).ChangeType(
                        (new BinXmlSqlMoney(GetInt32(this.tokDataPos))).ToDecimal(),
                        returnType, namespaceResolver);
                    break;

                case BinXmlToken.SQL_MONEY:
                    value = GetValueConverter(XmlTypeCode.Decimal).ChangeType(
                        (new BinXmlSqlMoney(GetInt64(this.tokDataPos))).ToDecimal(),
                        returnType, namespaceResolver);
                    break;

                case BinXmlToken.XSD_DECIMAL:
                case BinXmlToken.SQL_DECIMAL:
                case BinXmlToken.SQL_NUMERIC:
                    value = GetValueConverter(XmlTypeCode.Decimal).ChangeType(
                        (new BinXmlSqlDecimal(this.data, this.tokDataPos, token == BinXmlToken.XSD_DECIMAL)).ToDecimal(),
                        returnType, namespaceResolver);
                    break;

                case BinXmlToken.SQL_CHAR:
                case BinXmlToken.SQL_VARCHAR:
                case BinXmlToken.SQL_TEXT: {
                        int pos = this.tokDataPos;
                        int codepage = GetInt32(pos);
                        Encoding enc = System.Text.Encoding.GetEncoding(codepage);
                        value = GetValueConverter(XmlTypeCode.UntypedAtomic).ChangeType(
                            enc.GetString(this.data, pos + 4, this.tokLen - 4),
                            returnType, namespaceResolver);
                        break;
                    }

                case BinXmlToken.SQL_VARBINARY:
                case BinXmlToken.SQL_BINARY:
                case BinXmlToken.SQL_IMAGE:
                case BinXmlToken.SQL_UDT:
                case BinXmlToken.XSD_BASE64:
                case BinXmlToken.XSD_BINHEX: {
                        byte[] data = new byte[this.tokLen];
                        Array.Copy(this.data, this.tokDataPos, data, 0, this.tokLen);
                        value = GetValueConverter(token == BinXmlToken.XSD_BINHEX ? XmlTypeCode.HexBinary : XmlTypeCode.Base64Binary).ChangeType(
                            data, returnType, namespaceResolver);
                        break;
                    }

                case BinXmlToken.SQL_DATETIME:
                case BinXmlToken.SQL_SMALLDATETIME:
                case BinXmlToken.XSD_DATETIME:
                case BinXmlToken.XSD_KATMAI_DATE:
                case BinXmlToken.XSD_KATMAI_DATETIME:
                case BinXmlToken.XSD_KATMAI_TIME:
                    value = GetValueConverter(XmlTypeCode.DateTime).ChangeType(
                        ValueAsDateTime(),
                        returnType, namespaceResolver);
                    break;

                case BinXmlToken.XSD_KATMAI_DATEOFFSET:
                case BinXmlToken.XSD_KATMAI_DATETIMEOFFSET:
                case BinXmlToken.XSD_KATMAI_TIMEOFFSET:
                    value = GetValueConverter(XmlTypeCode.DateTime).ChangeType(
                        ValueAsDateTimeOffset(),
                        returnType, namespaceResolver);
                    break;

                case BinXmlToken.XSD_TIME:
                    value = GetValueConverter(XmlTypeCode.Time).ChangeType(
                        ValueAsDateTime(),
                        returnType, namespaceResolver);
                    break;

                case BinXmlToken.XSD_DATE:
                    value = GetValueConverter(XmlTypeCode.Date).ChangeType(
                        ValueAsDateTime(),
                        returnType, namespaceResolver);
                    break;

                case BinXmlToken.XSD_QNAME: {
                        int nameNum = ParseMB32(this.tokDataPos);
                        if (nameNum < 0 || nameNum >= this.symbolTables.qnameCount)
                            throw new XmlException(Res.XmlBin_InvalidQNameID, String.Empty);
                        QName qname = this.symbolTables.qnametable[nameNum];
                        value = GetValueConverter(XmlTypeCode.QName).ChangeType(
                            new XmlQualifiedName(qname.localname, qname.namespaceUri),
                            returnType, namespaceResolver);
                        break;
                    }

                default:
                    throw ThrowUnexpectedToken(this.token);
            }
            return value;
        }

        Int16 GetInt16(int pos) {
            byte[] data = this.data;
            return (Int16)(data[pos] | data[pos + 1] << 8);
        }

        UInt16 GetUInt16(int pos) {
            byte[] data = this.data;
            return (UInt16)(data[pos] | data[pos + 1] << 8);
        }

        Int32 GetInt32(int pos) {
            byte[] data = this.data;
            return (Int32)(data[pos] | data[pos + 1] << 8 | data[pos + 2] << 16 | data[pos + 3] << 24);
        }

        UInt32 GetUInt32(int pos) {
            byte[] data = this.data;
            return (UInt32)(data[pos] | data[pos + 1] << 8 | data[pos + 2] << 16 | data[pos + 3] << 24);
        }

        Int64 GetInt64(int pos) {
            byte[] data = this.data;
            uint lo = (uint)(data[pos] | data[pos + 1] << 8 | data[pos + 2] << 16 | data[pos + 3] << 24);
            uint hi = (uint)(data[pos + 4] | data[pos + 5] << 8 | data[pos + 6] << 16 | data[pos + 7] << 24);
            return (Int64)((ulong)hi) << 32 | lo;
        }

        UInt64 GetUInt64(int pos) {
            byte[] data = this.data;
            uint lo = (uint)(data[pos] | data[pos + 1] << 8 | data[pos + 2] << 16 | data[pos + 3] << 24);
            uint hi = (uint)(data[pos + 4] | data[pos + 5] << 8 | data[pos + 6] << 16 | data[pos + 7] << 24);
            return (UInt64)((ulong)hi) << 32 | lo;
        }

        Single GetSingle(int offset) {
            byte[] data = this.data;
            uint tmp = (uint)(data[offset]
                            | data[offset + 1] << 8
                            | data[offset + 2] << 16
                            | data[offset + 3] << 24);
            unsafe {
                return *((float*)&tmp);
            }
        }

        Double GetDouble(int offset) {
            uint lo = (uint)(data[offset + 0]
                            | data[offset + 1] << 8
                            | data[offset + 2] << 16
                            | data[offset + 3] << 24);
            uint hi = (uint)(data[offset + 4]
                            | data[offset + 5] << 8
                            | data[offset + 6] << 16
                            | data[offset + 7] << 24);
            ulong tmp = ((ulong)hi) << 32 | lo;
            unsafe {
                return *((double*)&tmp);
            }
        }

        Exception ThrowUnexpectedToken(BinXmlToken token) {
            System.Diagnostics.Debug.WriteLine("Unhandled token: " + token.ToString());
            return ThrowXmlException(Res.XmlBinary_UnexpectedToken);
        }

        Exception ThrowXmlException(string res) {
            this.state = ScanState.Error;
            return new XmlException(res, (string[])null);
        }

        // not currently used...
        //Exception ThrowXmlException(string res, string arg1) {
        //    this.state = ScanState.Error;
        //    return new XmlException(res, new string[] {arg1} );
        //}

        Exception ThrowXmlException(string res, string arg1, string arg2) {
            this.state = ScanState.Error;
            return new XmlException(res, new string[] { arg1, arg2 });
        }

        Exception ThrowNotSupported(string res) {
            this.state = ScanState.Error;
            return new NotSupportedException(Res.GetString(res));
        }
    }
}
