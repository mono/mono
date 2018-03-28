//------------------------------------------------------------------------------
// <copyright file="XsltInput.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

//#define XSLT2

using System.Diagnostics;
using System.Text;
using System.Xml.XPath;
using System.Collections.Generic;

namespace System.Xml.Xsl.Xslt {
    using Res = System.Xml.Utils.Res;
    using StringConcat = System.Xml.Xsl.Runtime.StringConcat;
    //         a) Forward only, one pass.
    //         b) You should call MoveToFirstChildren on nonempty element node. (or may be skip)

    internal class XsltInput : IErrorHelper {
    #if DEBUG
        const int InitRecordsSize = 1;
    #else
        const int InitRecordsSize = 1 + 21;
    #endif

        private XmlReader           reader;
        private IXmlLineInfo        readerLineInfo;
        private bool                topLevelReader;
        private CompilerScopeManager<VarPar> scopeManager;
        private KeywordsTable       atoms;
        private Compiler            compiler;
        private bool                reatomize;

        // Cached properties. MoveTo* functions set them.
        private XmlNodeType         nodeType;
        private Record[]            records = new Record[InitRecordsSize];
        private int                 currentRecord;
        private bool                isEmptyElement;
        private int                 lastTextNode;
        private int                 numAttributes;
        private ContextInfo         ctxInfo;
        private bool                attributesRead;

        public XsltInput(XmlReader reader, Compiler compiler, KeywordsTable atoms) {
            Debug.Assert(reader != null);
            Debug.Assert(atoms != null);
            EnsureExpandEntities(reader);
            IXmlLineInfo xmlLineInfo = reader as IXmlLineInfo;

            this.atoms          = atoms;
            this.reader         = reader;
            this.reatomize      = reader.NameTable != atoms.NameTable;
            this.readerLineInfo = (xmlLineInfo != null && xmlLineInfo.HasLineInfo()) ? xmlLineInfo : null;
            this.topLevelReader = reader.ReadState == ReadState.Initial;
            this.scopeManager   = new CompilerScopeManager<VarPar>(atoms);
            this.compiler       = compiler;
            this.nodeType       = XmlNodeType.Document;
        }

        // Cached properties
        public XmlNodeType   NodeType       { get { return nodeType == XmlNodeType.Element && 0 < currentRecord ? XmlNodeType.Attribute : nodeType; } }
        public string        LocalName      { get { return records[currentRecord].localName      ;} }
        public string        NamespaceUri   { get { return records[currentRecord].nsUri          ;} }
        public string        Prefix         { get { return records[currentRecord].prefix         ;} }
        public string        Value          { get { return records[currentRecord].value          ;} }
        public string        BaseUri        { get { return records[currentRecord].baseUri        ;} }
        public string        QualifiedName  { get { return records[currentRecord].QualifiedName  ;} }
        public bool          IsEmptyElement { get { return isEmptyElement; } }

        public string        Uri            { get { return records[currentRecord].baseUri        ; } }
        public Location      Start          { get { return records[currentRecord].start          ; } }
        public Location      End            { get { return records[currentRecord].end            ; } }

        private static void EnsureExpandEntities(XmlReader reader) {
            XmlTextReader tr = reader as XmlTextReader;
            if (tr != null && tr.EntityHandling != EntityHandling.ExpandEntities) {
                Debug.Assert(tr.Settings == null, "XmlReader created with XmlReader.Create should always expand entities.");
                tr.EntityHandling = EntityHandling.ExpandEntities;
            }
        }

        private void ExtendRecordBuffer(int position) {
            if (records.Length <= position) {
                int newSize = records.Length * 2;
                if (newSize <= position) {
                    newSize = position + 1;
                }
                Record[] tmp = new Record[newSize];
                Array.Copy(records, tmp, records.Length);
                records = tmp;
            }
        }

        public bool FindStylesheetElement() {
            if (! topLevelReader) {
                if (reader.ReadState != ReadState.Interactive) {
                    return false;
                }
            }

            // The stylesheet may be an embedded stylesheet. If this is the case the reader will be in Interactive state and should be 
            // positioned on xsl:stylesheet element (or any preceding whitespace) but there also can be namespaces defined on one 
            // of the ancestor nodes. These namespace definitions have to be copied to the xsl:stylesheet element scope. Otherwise it 
            // will not be possible to resolve them later and loading the stylesheet will end up with throwing an exception. 
            IDictionary<string, string> namespacesInScope = null;
            if (reader.ReadState == ReadState.Interactive) {
                // This may be an embedded stylesheet - store namespaces in scope
                IXmlNamespaceResolver nsResolver = reader as IXmlNamespaceResolver;
                if (nsResolver != null) {
                    namespacesInScope = nsResolver.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);
                }
            }
           
            while (MoveToNextSibling() && nodeType == XmlNodeType.Whitespace) ;

            // An Element node was reached. Potentially this is xsl:stylesheet instruction. 
            if (nodeType == XmlNodeType.Element) {
                // If namespacesInScope is not null then the stylesheet being read is an embedded stylesheet that can have namespaces 
                // defined outside of xsl:stylesheet instruction. In this case the namespace definitions collected above have to be added
                // to the element scope.
                if (namespacesInScope != null) {
                    foreach (KeyValuePair<string, string> prefixNamespacePair in namespacesInScope) {
                        // The namespace could be redefined on the element we just read. If this is the case scopeManager already has
                        // namespace definition for this prefix and the old definition must not be added to the scope. 
                        if (scopeManager.LookupNamespace(prefixNamespacePair.Key) == null) {
                            string nsAtomizedValue = atoms.NameTable.Add(prefixNamespacePair.Value);
                            scopeManager.AddNsDeclaration(prefixNamespacePair.Key, nsAtomizedValue);
                            ctxInfo.AddNamespace(prefixNamespacePair.Key, nsAtomizedValue);
                        }
                    }
                }

                // return true to indicate that we reached XmlNodeType.Element node - potentially xsl:stylesheet element.
                return true;
            }

            // return false to indicate that we did not reach XmlNodeType.Element node so it is not a valid stylesheet.
            return false;
        }

        public void Finish() {
            scopeManager.CheckEmpty();

            if (topLevelReader) {
                while (reader.ReadState == ReadState.Interactive) {
                    reader.Skip();
                }
            }
        }

        private void FillupRecord(ref Record rec) {
            rec.localName       = reader.LocalName;
            rec.nsUri           = reader.NamespaceURI;
            rec.prefix          = reader.Prefix;
            rec.value           = reader.Value;
            rec.baseUri         = reader.BaseURI;

            if (reatomize) {
                rec.localName = atoms.NameTable.Add(rec.localName);
                rec.nsUri     = atoms.NameTable.Add(rec.nsUri    );
                rec.prefix    = atoms.NameTable.Add(rec.prefix   );
            }

            if (readerLineInfo != null) {
                rec.start       = new Location(readerLineInfo.LineNumber, readerLineInfo.LinePosition - PositionAdjustment(reader.NodeType));
            }
        }

        private void SetRecordEnd(ref Record rec) {
            if (readerLineInfo != null) {
                rec.end = new Location(readerLineInfo.LineNumber, readerLineInfo.LinePosition - PositionAdjustment(reader.NodeType));
                if (reader.BaseURI != rec.baseUri || rec.end.LessOrEqual(rec.start)) {
                    rec.end = new Location(rec.start.Line, int.MaxValue);
                }
            }
        }

        private void FillupTextRecord(ref Record rec) {
            Debug.Assert(
                reader.NodeType == XmlNodeType.Whitespace || reader.NodeType == XmlNodeType.SignificantWhitespace ||
                reader.NodeType == XmlNodeType.Text       || reader.NodeType == XmlNodeType.CDATA
            );
            rec.localName       = string.Empty;
            rec.nsUri           = string.Empty;
            rec.prefix          = string.Empty;
            rec.value           = reader.Value;
            rec.baseUri         = reader.BaseURI;

            if (readerLineInfo != null) {
                bool isCDATA = (reader.NodeType == XmlNodeType.CDATA);
                int line = readerLineInfo.LineNumber;
                int pos  = readerLineInfo.LinePosition;
                rec.start = new Location(line, pos - (isCDATA ? 9 : 0));
                char prevChar = ' ';
                foreach (char ch in rec.value) {
                    switch (ch) {
                    case '\n':
                        if (prevChar != '\r') {
                            goto case '\r';
                        }
                        break;
                    case '\r':
                        line ++;
                        pos = 1;
                        break;
                    default :
                        pos ++;
                        break;
                    }
                    prevChar = ch;
                }
                rec.end = new Location(line, pos + (isCDATA ? 3 : 0));
            }
        }

        private void FillupCharacterEntityRecord(ref Record rec) {
            Debug.Assert(reader.NodeType == XmlNodeType.EntityReference);
            string local = reader.LocalName;
            Debug.Assert(local[0] == '#' || local == "lt" || local == "gt" || local == "quot" || local == "apos");
            rec.localName       = string.Empty;
            rec.nsUri           = string.Empty;
            rec.prefix          = string.Empty;
            rec.baseUri         = reader.BaseURI;

            if (readerLineInfo != null) {
                rec.start = new Location(readerLineInfo.LineNumber, readerLineInfo.LinePosition - 1);
            }
            reader.ResolveEntity();
            reader.Read();
            Debug.Assert(reader.NodeType == XmlNodeType.Text || reader.NodeType == XmlNodeType.Whitespace || reader.NodeType == XmlNodeType.SignificantWhitespace);
            rec.value = reader.Value;
            reader.Read();
            Debug.Assert(reader.NodeType == XmlNodeType.EndEntity);
            if (readerLineInfo != null) {
                int line = readerLineInfo.LineNumber;
                int pos = readerLineInfo.LinePosition;
                rec.end = new Location(readerLineInfo.LineNumber, readerLineInfo.LinePosition + 1);
            }
        }

        StringConcat strConcat = new StringConcat();

        // returns false if attribute is actualy namespace
        private bool ReadAttribute(ref Record rec) {
            Debug.Assert(reader.NodeType == XmlNodeType.Attribute, "reader.NodeType == XmlNodeType.Attribute");
            FillupRecord(ref rec);
            if (Ref.Equal(rec.prefix, atoms.Xmlns)) {                                      // xmlns:foo="NS_FOO"
                string atomizedValue = atoms.NameTable.Add(reader.Value);
                if (!Ref.Equal(rec.localName, atoms.Xml)) {
                    scopeManager.AddNsDeclaration(rec.localName, atomizedValue);
                    ctxInfo.AddNamespace(rec.localName, atomizedValue);
                }
                return false;
            } else if (rec.prefix.Length == 0 && Ref.Equal(rec.localName, atoms.Xmlns)) {  // xmlns="NS_FOO"
                string atomizedValue = atoms.NameTable.Add(reader.Value);
                scopeManager.AddNsDeclaration(string.Empty, atomizedValue);
                ctxInfo.AddNamespace(string.Empty, atomizedValue);
                return false;
            }
            /* Read Attribute Value */ {
                if (!reader.ReadAttributeValue()) {
                    // XmlTextReader never returns false from first call to ReadAttributeValue()
                    rec.value = string.Empty;
                    SetRecordEnd(ref rec);
                    return true;
                }
                if (readerLineInfo != null) {
                    int correction = (reader.NodeType == XmlNodeType.EntityReference) ? -2 : -1;
                    rec.valueStart = new Location(readerLineInfo.LineNumber, readerLineInfo.LinePosition + correction);
                    if (reader.BaseURI != rec.baseUri || rec.valueStart.LessOrEqual(rec.start)) {
                        int nameLength = ((rec.prefix.Length != 0) ? rec.prefix.Length + 1 : 0) + rec.localName.Length;
                        rec.end = new Location(rec.start.Line, rec.start.Pos + nameLength + 1);
                    }
                }
                string lastText = string.Empty;
                strConcat.Clear();
                do {
                    switch (reader.NodeType) {
                    case XmlNodeType.EntityReference:
                        reader.ResolveEntity();
                        break;
                    case XmlNodeType.EndEntity:
                        break;
                    default:
                        Debug.Assert(reader.NodeType == XmlNodeType.Text, "Unexpected node type inside attribute value");
                        lastText = reader.Value;
                        strConcat.Concat(lastText);
                        break;
                    }
                } while (reader.ReadAttributeValue());
                rec.value = strConcat.GetResult();
                if (readerLineInfo != null) {
                    Debug.Assert(reader.NodeType != XmlNodeType.EntityReference);
                    int correction = ((reader.NodeType == XmlNodeType.EndEntity) ? 1 : lastText.Length) + 1;
                    rec.end = new Location(readerLineInfo.LineNumber, readerLineInfo.LinePosition + correction);
                    if (reader.BaseURI != rec.baseUri || rec.end.LessOrEqual(rec.valueStart)) {
                        rec.end = new Location(rec.start.Line, int.MaxValue);
                    }
                }
            }
            return true;
        }

        // --------------------

        public bool MoveToFirstChild() {
            Debug.Assert(nodeType == XmlNodeType.Element, "To call MoveToFirstChild() XsltI---- should be positioned on an Element.");
            if (IsEmptyElement) {
                return false;
            }
            return ReadNextSibling();
        }

        public bool MoveToNextSibling() {
            Debug.Assert(nodeType != XmlNodeType.Element || IsEmptyElement, "On non-empty elements we should call MoveToFirstChild()");
            if (nodeType == XmlNodeType.Element || nodeType == XmlNodeType.EndElement) {
                scopeManager.ExitScope();
            }
            return ReadNextSibling();
        }

        public void SkipNode() {
            if (nodeType == XmlNodeType.Element && MoveToFirstChild()) {
                do {
                    SkipNode();
                } while (MoveToNextSibling());
            }
        }

        private int ReadTextNodes() {
            bool textPreserveWS = reader.XmlSpace == XmlSpace.Preserve;
            bool textIsWhite = true;
            int curTextNode = 0;
            do {
                switch (reader.NodeType) {
                case XmlNodeType.Text:
                    // XLinq reports WS nodes as Text so we need to analyze them here
                case XmlNodeType.CDATA:
                    if (textIsWhite && ! XmlCharType.Instance.IsOnlyWhitespace(reader.Value)) {
                        textIsWhite = false;
                    }
                    goto case XmlNodeType.SignificantWhitespace;
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    ExtendRecordBuffer(curTextNode);
                    FillupTextRecord(ref records[curTextNode]);
                    reader.Read();
                    curTextNode++;
                    break;
                case XmlNodeType.EntityReference:
                    string local = reader.LocalName;
                    if (local.Length > 0 && (
                        local[0] == '#' ||
                        local == "lt" || local == "gt" || local == "quot" || local == "apos"
                    )) {
                        // Special treatment for character and built-in entities
                        ExtendRecordBuffer(curTextNode);
                        FillupCharacterEntityRecord(ref records[curTextNode]);
                        if (textIsWhite && !XmlCharType.Instance.IsOnlyWhitespace(records[curTextNode].value)) {
                            textIsWhite = false;
                        }
                        curTextNode++;
                    } else {
                        reader.ResolveEntity();
                        reader.Read();
                    }
                    break;
                case XmlNodeType.EndEntity:
                    reader.Read();
                    break;
                default:
                    this.nodeType = (
                        ! textIsWhite  ? XmlNodeType.Text :
                        textPreserveWS ? XmlNodeType.SignificantWhitespace :
                        /*default:    */ XmlNodeType.Whitespace
                    );
                    return curTextNode;
                }
            } while (true);
        }

        private bool ReadNextSibling() {
            if (currentRecord < lastTextNode) {
                Debug.Assert(nodeType == XmlNodeType.Text || nodeType == XmlNodeType.Whitespace || nodeType == XmlNodeType.SignificantWhitespace);
                currentRecord++;
                if (currentRecord == lastTextNode) {
                    lastTextNode = 0;  // we are done with text nodes. Reset this counter
                }
                return true;
            }
            currentRecord = 0;
            while (! reader.EOF) {
                switch (reader.NodeType) {
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                case XmlNodeType.EntityReference:
                    int numTextNodes = ReadTextNodes();
                    if (numTextNodes == 0) {
                        // Most likely this was Entity that starts from non-text node
                        continue;
                    }
                    lastTextNode = numTextNodes - 1;
                    return true;
                case XmlNodeType.Element:
                    scopeManager.EnterScope();
                    numAttributes = ReadElement();
                    return true;
                case XmlNodeType.EndElement:
                    nodeType = XmlNodeType.EndElement;
                    isEmptyElement = false;
                    FillupRecord(ref records[0]);
                    reader.Read();
                    SetRecordEnd(ref records[0]);
                    return false;
                default:
                    reader.Read();
                    break;
                }
            }
            return false;
        }

        private int ReadElement() {
            Debug.Assert(reader.NodeType == XmlNodeType.Element);

            attributesRead = false;
            FillupRecord(ref records[0]);
            nodeType = XmlNodeType.Element;
            isEmptyElement = reader.IsEmptyElement;
            ctxInfo = new ContextInfo(this);

            int record = 1;
            if (reader.MoveToFirstAttribute()) {
                do {
                    ExtendRecordBuffer(record);
                    if (ReadAttribute(ref records[record])) {
                        record++;
                    }
                } while (reader.MoveToNextAttribute());
                reader.MoveToElement();
            }
            reader.Read();
            SetRecordEnd(ref records[0]);
            ctxInfo.lineInfo = BuildLineInfo();
            attributes = null;
            return record - 1;
        }

        public void MoveToElement() {
            Debug.Assert(nodeType == XmlNodeType.Element, "For MoveToElement() we should be positioned on Element or Attribute");
            currentRecord = 0;
        }

        private bool MoveToAttributeBase(int attNum) {
            Debug.Assert(nodeType == XmlNodeType.Element, "For MoveToLiteralAttribute() we should be positioned on Element or Attribute");
            if (0 < attNum && attNum <= numAttributes) {
                currentRecord = attNum;
                return true;
            } else {
                currentRecord = 0;
                return false;
            }
        }

        public bool MoveToLiteralAttribute(int attNum) {
            Debug.Assert(nodeType == XmlNodeType.Element, "For MoveToLiteralAttribute() we should be positioned on Element or Attribute");
            if (0 < attNum && attNum <= numAttributes) {
                currentRecord = attNum;
                return true;
            } else {
                currentRecord = 0;
                return false;
            }
        }

        public bool MoveToXsltAttribute(int attNum, string attName) {
            Debug.Assert(attributes != null && attributes[attNum].name == attName, "Attribute numbering error.");
            this.currentRecord = xsltAttributeNumber[attNum];
            return this.currentRecord != 0;
        }

        public bool IsRequiredAttribute(int attNum) {
            return (attributes[attNum].flags & (compiler.Version == 2 ? XsltLoader.V2Req : XsltLoader.V1Req)) != 0;
        }

        public bool AttributeExists(int attNum, string attName) {
            Debug.Assert(attributes != null && attributes[attNum].name == attName, "Attribute numbering error.");
            return xsltAttributeNumber[attNum] != 0;
        }

        public struct DelayedQName {
            string prefix   ;
            string localName;
            public DelayedQName(ref Record rec) {
                this.prefix    = rec.prefix;
                this.localName = rec.localName;
            }
            public static implicit operator string(DelayedQName qn) {
                return qn.prefix.Length == 0 ? qn.localName : (qn.prefix + ':' + qn.localName);
            }
        }

        public DelayedQName ElementName {
            get {
                Debug.Assert(nodeType == XmlNodeType.Element || nodeType == XmlNodeType.EndElement, "Input is positioned on element or attribute");
                return new DelayedQName(ref records[0]);
            }
        }

        // -------------------- Keywords testing --------------------

        public bool IsNs(string ns)             { return Ref.Equal(ns, NamespaceUri); }
        public bool IsKeyword(string kwd)       { return Ref.Equal(kwd, LocalName);  }
        public bool IsXsltNamespace()           { return IsNs(atoms.UriXsl); }
        public bool IsNullNamespace()           { return IsNs(string.Empty); }
        public bool IsXsltAttribute(string kwd) { return IsKeyword(kwd) && IsNullNamespace(); }
        public bool IsXsltKeyword(  string kwd) { return IsKeyword(kwd) && IsXsltNamespace(); }

        // -------------------- Scope Management --------------------
        // See private class InputScopeManager bellow.
        // InputScopeManager handles some flags and values with respect of scope level where they as defined.
        // To parse XSLT style sheet we need the folloing values:
        //  BackwardCompatibility -- this flag is set when compiler.version==2 && xsl:version<2.
        //  ForwardCompatibility  -- this flag is set when compiler.version==2 && xsl:version>1 or compiler.version==1 && xsl:version!=1
        //  CanHaveApplyImports  -- we allow xsl:apply-templates instruction to apear in any template with match!=null, but not inside xsl:for-each
        //                          so it can't be inside global variable and has initial value = false
        //  ExtentionNamespace   -- is defined by extension-element-prefixes attribute on LRE or xsl:stylesheet

        public bool CanHaveApplyImports {
            get { return scopeManager.CanHaveApplyImports;  }
            set { scopeManager.CanHaveApplyImports = value; }
        }

        public bool IsExtensionNamespace(string uri) {
            Debug.Assert(nodeType != XmlNodeType.Element || attributesRead, "Should first read attributes");
            return scopeManager.IsExNamespace(uri);
        }

        public bool ForwardCompatibility {
            get {
                Debug.Assert(nodeType != XmlNodeType.Element || attributesRead, "Should first read attributes");
                return scopeManager.ForwardCompatibility;
            }
        }

        public bool BackwardCompatibility {
            get {
                Debug.Assert(nodeType != XmlNodeType.Element || attributesRead, "Should first read attributes");
                return scopeManager.BackwardCompatibility;
            }
        }

        public XslVersion XslVersion {
            get { return scopeManager.ForwardCompatibility ? XslVersion.ForwardsCompatible : XslVersion.Current; }
        }

        private void SetVersion(int attVersion) {
            MoveToLiteralAttribute(attVersion);
            Debug.Assert(IsKeyword(atoms.Version));
            double version = XPathConvert.StringToDouble(Value);
            if (double.IsNaN(version)) {
                ReportError(/*[XT0110]*/Res.Xslt_InvalidAttrValue, atoms.Version, Value);
#if XSLT2
                version = 2.0;
#else
                version = 1.0;
#endif
            }
            SetVersion(version);
        }
        private void SetVersion(double version) {
            if (compiler.Version == 0) {
#if XSLT2
                compiler.Version = version < 2.0 ? 1 : 2;
#else
                compiler.Version = 1;
#endif
            }

            if (compiler.Version == 1) {
                scopeManager.BackwardCompatibility = false;
                scopeManager.ForwardCompatibility = (version != 1.0);
            } else {
                scopeManager.BackwardCompatibility = version < 2;
                scopeManager.ForwardCompatibility = 2 < version;
            }
        }

        // --------------- GetAtributes(...) -------------------------
        // All Xslt Instructions allows fixed set of attributes in null-ns, no in XSLT-ns and any in other ns.
        // In ForwardCompatibility mode we should ignore any of this problems.
        // We not use these functions for parseing LiteralResultElement and xsl:stylesheet

        public struct XsltAttribute {
            public string name;
            public int    flags;
            public XsltAttribute(string name, int flags) {
                this.name  = name;
                this.flags = flags;
            }
        }

        private XsltAttribute[] attributes = null;
        // Mapping of attribute names as they ordered in 'attributes' array
        // to there's numbers in actual stylesheet as they ordered in 'records' array
        private int[] xsltAttributeNumber = new int[21];

        static private XsltAttribute[] noAttributes = new XsltAttribute[]{};
        public ContextInfo GetAttributes() {
            return GetAttributes(noAttributes);
        }

        public ContextInfo GetAttributes(XsltAttribute[] attributes) {
            Debug.Assert(NodeType == XmlNodeType.Element);
            Debug.Assert(attributes.Length <= xsltAttributeNumber.Length);
            this.attributes = attributes;
            // temp hack to fix value? = new AttValue(records[values[?]].value);
            records[0].value = null;

            // Standard Attributes:
            int attExtension = 0;
            int attExclude   = 0;
            int attNamespace = 0;
            int attCollation = 0;
            int attUseWhen   = 0;

            bool isXslOutput = IsXsltNamespace() && IsKeyword(atoms.Output);
            bool SS = IsXsltNamespace() && (IsKeyword(atoms.Stylesheet) || IsKeyword(atoms.Transform));
            bool V2 = compiler.Version == 2;

            for (int i = 0; i < attributes.Length; i++) {
                xsltAttributeNumber[i] = 0;
            }

            compiler.EnterForwardsCompatible();
            if (SS || V2 && !isXslOutput) {
                for (int i = 1; MoveToAttributeBase(i); i++) {
                    if (IsNullNamespace() && IsKeyword(atoms.Version)) {
                        SetVersion(i);
                        break;
                    }
                }
            }
            if (compiler.Version == 0) {
                Debug.Assert(SS, "First we parse xsl:stylesheet element");
#if XSLT2
                SetVersion(2.0);
#else
                SetVersion(1.0);
#endif
            }
            V2 = compiler.Version == 2;
            int OptOrReq = V2 ? XsltLoader.V2Opt | XsltLoader.V2Req : XsltLoader.V1Opt | XsltLoader.V1Req;

            for (int attNum = 1; MoveToAttributeBase(attNum); attNum++) {
                if (IsNullNamespace()) {
                    string localName = LocalName;
                    int kwd;
                    for (kwd = 0; kwd < attributes.Length; kwd++) {
                        if (Ref.Equal(localName, attributes[kwd].name) && (attributes[kwd].flags & OptOrReq) != 0) {
                            xsltAttributeNumber[kwd] = attNum;
                            break;
                        }
                    }

                    if (kwd == attributes.Length) {
                        if (Ref.Equal(localName, atoms.ExcludeResultPrefixes   ) && (SS || V2)) {attExclude   = attNum; } else
                        if (Ref.Equal(localName, atoms.ExtensionElementPrefixes) && (SS || V2)) {attExtension = attNum; } else
                        if (Ref.Equal(localName, atoms.XPathDefaultNamespace   ) && (      V2)) {attNamespace = attNum; } else
                        if (Ref.Equal(localName, atoms.DefaultCollation        ) && (      V2)) {attCollation = attNum; } else
                        if (Ref.Equal(localName, atoms.UseWhen                 ) && (      V2)) {attUseWhen   = attNum; } else {
                            ReportError(/*[XT0090]*/Res.Xslt_InvalidAttribute, QualifiedName, records[0].QualifiedName);
                        }
                    }
                } else if (IsXsltNamespace()) {
                    ReportError(/*[XT0090]*/Res.Xslt_InvalidAttribute, QualifiedName, records[0].QualifiedName);
                } else {
                    // Ignore the attribute.
                    // An element from the XSLT namespace may have any attribute not from the XSLT namespace,
                    // provided that the expanded-name of the attribute has a non-null namespace URI.
                    // For example, it may be 'xml:space'.
                }
            }

            attributesRead = true;

            // Ignore invalid attributes if forwards-compatible behavior is enabled. Note that invalid
            // attributes may encounter before ForwardCompatibility flag is set to true. For example,
            // <xsl:stylesheet unknown="foo" version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"/>
            compiler.ExitForwardsCompatible(ForwardCompatibility);

            InsertExNamespaces(attExtension, ctxInfo, /*extensions:*/ true );
            InsertExNamespaces(attExclude  , ctxInfo, /*extensions:*/ false);
            SetXPathDefaultNamespace(attNamespace);
            SetDefaultCollation(attCollation);
            if (attUseWhen != 0) {
                ReportNYI(atoms.UseWhen);
            }

            MoveToElement();
            // Report missing mandatory attributes
            for (int i = 0; i < attributes.Length; i ++) {
                if (xsltAttributeNumber[i] == 0) {
                    int flags = attributes[i].flags;
                    if (
                        compiler.Version == 2 && (flags & XsltLoader.V2Req) != 0 ||
                        compiler.Version == 1 && (flags & XsltLoader.V1Req) != 0 && (!ForwardCompatibility || (flags & XsltLoader.V2Req) != 0)
                    ) {
                        ReportError(/*[XT_001]*/Res.Xslt_MissingAttribute, attributes[i].name);
                    }
                }
            }

            return ctxInfo;
        }

        public ContextInfo GetLiteralAttributes(bool asStylesheet) {
            Debug.Assert(NodeType == XmlNodeType.Element);

            // Standard Attributes:
            int attVersion   = 0;
            int attExtension = 0;
            int attExclude   = 0;
            int attNamespace = 0;
            int attCollation = 0;
            int attUseWhen   = 0;

            for (int i = 1; MoveToLiteralAttribute(i); i++) {
                if (IsXsltNamespace()) {
                    string localName = LocalName;
                    if (Ref.Equal(localName, atoms.Version                 )) {attVersion   = i; } else
                    if (Ref.Equal(localName, atoms.ExtensionElementPrefixes)) {attExtension = i; } else
                    if (Ref.Equal(localName, atoms.ExcludeResultPrefixes   )) {attExclude   = i; } else
                    if (Ref.Equal(localName, atoms.XPathDefaultNamespace   )) {attNamespace = i; } else
                    if (Ref.Equal(localName, atoms.DefaultCollation        )) {attCollation = i; } else
                    if (Ref.Equal(localName, atoms.UseWhen                 )) {attUseWhen   = i; }
                }
            }

            attributesRead = true;
            this.MoveToElement();

            if (attVersion != 0) {
                // Enable forwards-compatible behavior if version attribute is not "1.0"
                SetVersion(attVersion);
            } else {
                if (asStylesheet) {
                    ReportError(Ref.Equal(NamespaceUri, atoms.UriWdXsl) && Ref.Equal(LocalName, atoms.Stylesheet) ?
                        /*[XT_025]*/Res.Xslt_WdXslNamespace : /*[XT0150]*/Res.Xslt_WrongStylesheetElement
                    );
#if XSLT2
                    SetVersion(2.0);
#else
                    SetVersion(1.0);
#endif
                }
            }

            // Parse xsl:extension-element-prefixes attribute (now that forwards-compatible mode is known)
            InsertExNamespaces(attExtension, ctxInfo, /*extensions:*/true);

            if (! IsExtensionNamespace(records[0].nsUri)) {
                // Parse other attributes (now that it's known this is a literal result element)
                if (compiler.Version == 2) {
                    SetXPathDefaultNamespace(attNamespace);
                    SetDefaultCollation(attCollation);
                    if (attUseWhen != 0) {
                        ReportNYI(atoms.UseWhen);
                    }
                }

                InsertExNamespaces(attExclude, ctxInfo, /*extensions:*/false);
            }

            return ctxInfo;
        }

        // Get just the 'version' attribute of an unknown XSLT instruction. All other attributes
        // are ignored since we do not want to report an error on each of them.
        public void GetVersionAttribute() {
            Debug.Assert(NodeType == XmlNodeType.Element && IsXsltNamespace());
            bool V2 = compiler.Version == 2;

            if (V2) {
                for (int i = 1; MoveToAttributeBase(i); i++) {
                    if (IsNullNamespace() && IsKeyword(atoms.Version)) {
                        SetVersion(i);
                        break;
                    }
                }
            }
            attributesRead = true;
        }

        private void InsertExNamespaces(int attExPrefixes, ContextInfo ctxInfo, bool extensions) {
            // List of Extension namespaces are maintaned by XsltInput's ScopeManager and is used by IsExtensionNamespace() in XsltLoader.LoadLiteralResultElement()
            // Both Extension and Exclusion namespaces will not be coppied by LiteralResultElement. Logic of copping namespaces are in QilGenerator.CompileLiteralElement().
            // At this time we will have different scope manager and need preserve all required information from load time to compile time.
            // Each XslNode contains list of NsDecls (nsList) wich stores prefix+namespaces pairs for each namespace decls as well as exclusion namespaces.
            // In addition it also contains Exclusion namespace. They are represented as (null+namespace). Special case is Exlusion "#all" represented as (null+null).
            //and Exclusion namespace
            if (MoveToLiteralAttribute(attExPrefixes)) {
                Debug.Assert(extensions ? IsKeyword(atoms.ExtensionElementPrefixes) : IsKeyword(atoms.ExcludeResultPrefixes));
                string value = Value;
                if (value.Length != 0) {
                    if (!extensions && compiler.Version != 1 && value == "#all") {
                        ctxInfo.nsList = new NsDecl(ctxInfo.nsList, /*prefix:*/null, /*nsUri:*/null);    // null, null means Exlusion #all
                    } else {
                        compiler.EnterForwardsCompatible();
                        string[] list = XmlConvert.SplitString(value);
                        for (int idx = 0; idx < list.Length; idx++) {
                            if (list[idx] == "#default") {
                                list[idx] = this.LookupXmlNamespace(string.Empty);
                                if (list[idx].Length == 0 && compiler.Version != 1 && !BackwardCompatibility) {
                                    ReportError(/*[XTSE0809]*/Res.Xslt_ExcludeDefault);
                                }
                            } else {
                                list[idx] = this.LookupXmlNamespace(list[idx]);
                            }
                        }
                        if (!compiler.ExitForwardsCompatible(this.ForwardCompatibility)) {
                            // There were errors in the list, ignore the whole list
                            return;
                        }

                        for (int idx = 0; idx < list.Length; idx++) {
                            if (list[idx] != null) {
                                ctxInfo.nsList = new NsDecl(ctxInfo.nsList, /*prefix:*/null, list[idx]); // null means that this Exlusion NS
                                if (extensions) {
                                    this.scopeManager.AddExNamespace(list[idx]);                         // At Load time we need to know Extencion namespaces to ignore such literal elements.
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SetXPathDefaultNamespace(int attNamespace) {
            if (MoveToLiteralAttribute(attNamespace)) {
                Debug.Assert(IsKeyword(atoms.XPathDefaultNamespace));
                if (Value.Length != 0) {
                    ReportNYI(atoms.XPathDefaultNamespace);
                }
            }
        }

        private void SetDefaultCollation(int attCollation) {
            if (MoveToLiteralAttribute(attCollation)) {
                Debug.Assert(IsKeyword(atoms.DefaultCollation));
                string[] list = XmlConvert.SplitString(Value);
                int col;
                for (col = 0; col < list.Length; col++) {
                    if (System.Xml.Xsl.Runtime.XmlCollation.Create(list[col], /*throw:*/false) != null) {
                        break;
                    }
                }
                if (col == list.Length) {
                    ReportErrorFC(/*[XTSE0125]*/Res.Xslt_CollationSyntax);
                } else {
                    if (list[col] != XmlReservedNs.NsCollCodePoint) {
                        ReportNYI(atoms.DefaultCollation);
                    }
                }
            }
        }

        // ----------------------- ISourceLineInfo -----------------------

        private static int PositionAdjustment(XmlNodeType nt) {
            switch (nt) {
            case XmlNodeType.Element:
                return 1;   // "<"
            case XmlNodeType.CDATA:
                return 9;   // "<![CDATA["
            case XmlNodeType.ProcessingInstruction:
                return 2;   // "<?"
            case XmlNodeType.Comment:
                return 4;   // "<!--"
            case XmlNodeType.EndElement:
                return 2;   // "</"
            case XmlNodeType.EntityReference:
                return 1;   // "&"
            default:
                return 0;
            }
        }

        public ISourceLineInfo BuildLineInfo() {
            return new SourceLineInfo(Uri, Start, End);
        }

        public ISourceLineInfo BuildNameLineInfo() {
            if (readerLineInfo == null) {
                return BuildLineInfo();
            }

            // LocalName is checked against null since it is used to calculate QualifiedName used in turn to 
            // calculate end position. 
            // LocalName (and other cached properties) can be null only if nothing has been read from the reader. 
            // This happens for instance when a reader which has already been closed or a reader positioned
            // on the very last node of the document is passed to the ctor. 
            if(LocalName == null) {
                // Fill up the current record to set all the properties used below.
                FillupRecord(ref records[currentRecord]);
            }

            Location start = Start;
            int line = start.Line;
            int pos  = start.Pos + PositionAdjustment(NodeType);
            return new SourceLineInfo(Uri, new Location(line, pos), new Location(line, pos + QualifiedName.Length));
        }

        public ISourceLineInfo BuildReaderLineInfo() {
            Location loc;

            if (readerLineInfo != null)
                loc = new Location(readerLineInfo.LineNumber, readerLineInfo.LinePosition);
            else
                loc = new Location(0, 0);

            return new SourceLineInfo(reader.BaseURI, loc, loc);
        }

        // Resolve prefix, return null and report an error if not found
        public string LookupXmlNamespace(string prefix) {
            Debug.Assert(prefix != null);
            string nsUri = scopeManager.LookupNamespace(prefix);
            if (nsUri != null) {
                Debug.Assert(Ref.Equal(atoms.NameTable.Get(nsUri), nsUri), "Namespaces must be atomized");
                return nsUri;
            }
            if (prefix.Length == 0) {
                return string.Empty;
            }
            ReportError(/*[XT0280]*/Res.Xslt_InvalidPrefix, prefix);
            return null;
        }

        // ---------------------- Error Handling ----------------------

        public void ReportError(string res, params string[] args) {
            compiler.ReportError(BuildNameLineInfo(), res, args);
        }

        public void ReportErrorFC(string res, params string[] args) {
            if (!ForwardCompatibility) {
                compiler.ReportError(BuildNameLineInfo(), res, args);
            }
        }

        public void ReportWarning(string res, params string[] args) {
            compiler.ReportWarning(BuildNameLineInfo(), res, args);
        }

        private void ReportNYI(string arg) {
            ReportErrorFC(Res.Xslt_NotYetImplemented, arg);
        }

        // -------------------------------- ContextInfo ------------------------------------

        internal class ContextInfo {
            public NsDecl           nsList;
            public ISourceLineInfo  lineInfo;       // Line info for whole start tag
            public ISourceLineInfo  elemNameLi;     // Line info for element name
            public ISourceLineInfo  endTagLi;       // Line info for end tag or '/>'
            private int             elemNameLength;

            // Create ContextInfo based on existing line info (used during AST rewriting)
            internal ContextInfo(ISourceLineInfo lineinfo) {
                this.elemNameLi = lineinfo;
                this.endTagLi = lineinfo;
                this.lineInfo = lineinfo;
            }

            public ContextInfo(XsltInput input) {
                elemNameLength = input.QualifiedName.Length;
            }

            public void AddNamespace(string prefix, string nsUri) {
                nsList = new NsDecl(nsList, prefix, nsUri);
            }

            public void SaveExtendedLineInfo(XsltInput input) {
                if (lineInfo.Start.Line == 0) {
                    elemNameLi = endTagLi = null;
                    return;
                }

                elemNameLi = new SourceLineInfo(
                    lineInfo.Uri,
                    lineInfo.Start.Line, lineInfo.Start.Pos + 1,  // "<"
                    lineInfo.Start.Line, lineInfo.Start.Pos + 1 + elemNameLength
                );

                if (!input.IsEmptyElement) {
                    Debug.Assert(input.NodeType == XmlNodeType.EndElement);
                    endTagLi = input.BuildLineInfo();
                } else {
                    Debug.Assert(input.NodeType == XmlNodeType.Element || input.NodeType == XmlNodeType.Attribute);
                    endTagLi = new EmptyElementEndTag(lineInfo);
                }
            }

            // We need this wrapper class because elementTagLi is not yet calculated
            internal class EmptyElementEndTag : ISourceLineInfo {
                private ISourceLineInfo elementTagLi;

                public EmptyElementEndTag(ISourceLineInfo elementTagLi) {
                    this.elementTagLi = elementTagLi;
                }

                public string Uri       { get { return elementTagLi.Uri;        } }
                public bool IsNoSource  { get { return elementTagLi.IsNoSource; } }
                public Location Start   { get { return new Location(elementTagLi.End.Line, elementTagLi.End.Pos - 2); } }
                public Location End     { get { return elementTagLi.End ; } }
            }
        }
        internal struct Record {
            public string       localName ;
            public string       nsUri     ;
            public string       prefix    ;
            public string       value     ;
            public string       baseUri   ;
            public Location     start     ;
            public Location     valueStart;
            public Location     end       ;
            public string       QualifiedName  { get { return prefix.Length == 0 ? localName : string.Concat(prefix, ":", localName); } }
        }
    }
}
