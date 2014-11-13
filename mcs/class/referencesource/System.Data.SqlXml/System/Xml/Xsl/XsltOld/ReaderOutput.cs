//------------------------------------------------------------------------------
// <copyright file="ReaderOutput.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.XsltOld {
    using Res = System.Xml.Utils.Res;
    using System;
    using System.Globalization;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.XPath;
    using System.Collections;

    internal class ReaderOutput : XmlReader, RecordOutput {
        private Processor       processor;
        private XmlNameTable    nameTable;

        // Main node + Fields Collection
        private RecordBuilder   builder;
        private BuilderInfo     mainNode;
        private ArrayList       attributeList;
        private int             attributeCount;
        private BuilderInfo     attributeValue;

        // OutputScopeManager
        private OutputScopeManager  manager;

        // Current position in the list
        private int             currentIndex;
        private BuilderInfo     currentInfo;

        // Reader state
        private ReadState       state = ReadState.Initial;
        private bool            haveRecord;

        // Static default record
        static BuilderInfo      s_DefaultInfo = new BuilderInfo();

        XmlEncoder  encoder = new XmlEncoder();
        XmlCharType xmlCharType = XmlCharType.Instance;

        internal ReaderOutput(Processor processor) {
            Debug.Assert(processor != null);
            Debug.Assert(processor.NameTable != null);

            this.processor = processor;
            this.nameTable = processor.NameTable;

            Reset();
        }

        // XmlReader abstract methods implementation
        public override XmlNodeType NodeType {
            [System.Runtime.TargetedPatchingOptOutAttribute("Performance critical to inline across NGen image boundaries")]
            get {
                CheckCurrentInfo();
                return this.currentInfo.NodeType;
            }
        }

        public override string Name {
            get {
                CheckCurrentInfo();
                string prefix    = Prefix;
                string localName = LocalName;

                if (prefix != null && prefix.Length > 0) {
                    if (localName.Length > 0) {
                        return nameTable.Add(prefix + ":" + localName);
                    }
                    else {
                        return prefix;
                    }
                }
                else {
                    return localName;
                }
            }
        }

        public override string LocalName {
            [System.Runtime.TargetedPatchingOptOutAttribute("Performance critical to inline across NGen image boundaries")]
            get {
                CheckCurrentInfo();
                return this.currentInfo.LocalName;
            }
        }

        public override string NamespaceURI {
            [System.Runtime.TargetedPatchingOptOutAttribute("Performance critical to inline across NGen image boundaries")]
            get {
                CheckCurrentInfo();
                return this.currentInfo.NamespaceURI;
            }
        }

        public override string Prefix {
            [System.Runtime.TargetedPatchingOptOutAttribute("Performance critical to inline across NGen image boundaries")]
            get {
                CheckCurrentInfo();
                return this.currentInfo.Prefix;
            }
        }

        public override bool HasValue {
            get {
                return XmlReader.HasValueInternal(NodeType);
            }
        }

        public override string Value {
            [System.Runtime.TargetedPatchingOptOutAttribute("Performance critical to inline across NGen image boundaries")]
            get {
                CheckCurrentInfo();
                return this.currentInfo.Value;
            }
        }

        public override int Depth {
            [System.Runtime.TargetedPatchingOptOutAttribute("Performance critical to inline across NGen image boundaries")]
            get {
                CheckCurrentInfo();
                return this.currentInfo.Depth;
            }
        }

        public override string BaseURI {
            get {
                return string.Empty;
            }
        }

        public override bool IsEmptyElement {
            [System.Runtime.TargetedPatchingOptOutAttribute("Performance critical to inline across NGen image boundaries")]
            get {
                CheckCurrentInfo();
                return this.currentInfo.IsEmptyTag;
            }
        }

        public override char QuoteChar {
            get { return encoder.QuoteChar; }
        }

        public override bool IsDefault {
            get { return false; }
        }

        public override XmlSpace XmlSpace {
            get { return this.manager != null ? this.manager.XmlSpace : XmlSpace.None; }
        }

        public override string XmlLang {
            get { return this.manager != null ? this.manager.XmlLang : string.Empty; }
        }

        // Attribute Accessors

        public override int AttributeCount {
            get { return this.attributeCount; }
        }

        public override string GetAttribute(string name) {
            int ordinal;
            if (FindAttribute(name, out ordinal)) {
                Debug.Assert(ordinal >= 0);
                return((BuilderInfo)this.attributeList[ordinal]).Value;
            }
            else {
                Debug.Assert(ordinal == -1);
                return null;
            }            
        }

        public override string GetAttribute(string localName, string namespaceURI) {
            int ordinal;
            if (FindAttribute(localName, namespaceURI, out ordinal)) {
                Debug.Assert(ordinal >= 0);
                return((BuilderInfo)this.attributeList[ordinal]).Value;
            }
            else {
                Debug.Assert(ordinal == -1);
                return null;
            }
        }

        public override string GetAttribute(int i) {
            BuilderInfo attribute = GetBuilderInfo(i);
            return attribute.Value;
        }

        public override string this [int i] {
            get { return GetAttribute(i); }
        }

        public override string this [string name] {
            get { return GetAttribute(name); }
        }

        public override string this [string name, string namespaceURI] {
            get { return GetAttribute(name, namespaceURI); }
        }

        public override bool MoveToAttribute(string name) {
            int ordinal;
            if (FindAttribute(name, out ordinal)) {
                Debug.Assert(ordinal >= 0);
                SetAttribute(ordinal);
                return true;
            }
            else {
                Debug.Assert(ordinal == -1);
                return false;
            }
        }

        public override bool MoveToAttribute(string localName, string namespaceURI) {
            int ordinal;
            if (FindAttribute(localName, namespaceURI, out ordinal)) {
                Debug.Assert(ordinal >= 0);
                SetAttribute(ordinal);
                return true;
            }
            else {
                Debug.Assert(ordinal == -1);
                return false;
            }
        }

        public override void MoveToAttribute(int i) {
            if (i < 0 || this.attributeCount <= i) {
                throw new ArgumentOutOfRangeException("i");
            }
            SetAttribute(i);
        }

        [System.Runtime.TargetedPatchingOptOutAttribute("Performance critical to inline across NGen image boundaries")]
        public override bool MoveToFirstAttribute() {
            if (this.attributeCount <= 0) {
                Debug.Assert(this.attributeCount == 0);
                return false;
            }
            else {
                SetAttribute(0);
                return true;
            }
        }

        public override bool MoveToNextAttribute() {
            if (this.currentIndex + 1 < this.attributeCount) {
                SetAttribute(this.currentIndex + 1);
                return true;
            }
            return false;
        }

        public override bool MoveToElement() {
            if (NodeType == XmlNodeType.Attribute || this.currentInfo == this.attributeValue) {
                SetMainNode();
                return true;
            }
            return false;
        }

        // Moving through the Stream

        public override bool Read() {
            Debug.Assert(this.processor != null || this.state == ReadState.Closed);

            if (this.state != ReadState.Interactive) {
                if (this.state == ReadState.Initial) {
                    state = ReadState.Interactive;
                }
                else {
                    return false;
                }
            }

            while (true) { // while -- to ignor empty whitespace nodes.
                if (this.haveRecord) {
                    this.processor.ResetOutput();
                    this.haveRecord = false;
                }

                this.processor.Execute();

                if (this.haveRecord) {
                    CheckCurrentInfo();
                    // check text nodes on whitespaces;
                    switch (this.NodeType) {
                    case XmlNodeType.Text :
                        if (xmlCharType.IsOnlyWhitespace(this.Value)) {
                            this.currentInfo.NodeType = XmlNodeType.Whitespace;
                            goto case XmlNodeType.Whitespace;
                        }
                        Debug.Assert(this.Value.Length != 0, "It whould be Whitespace in this case");
                        break;
                    case XmlNodeType.Whitespace :
                        if(this.Value.Length == 0) {
                            continue;                          // ignoring emty text nodes
                        }
                        if (this.XmlSpace == XmlSpace.Preserve) {
                            this.currentInfo.NodeType = XmlNodeType.SignificantWhitespace;
                        }
                        break;
                    }                
                }
                else {
                    Debug.Assert(this.processor.ExecutionDone);
                    this.state = ReadState.EndOfFile;
                    Reset();
                }

                return this.haveRecord;
            }
        }

        public override bool EOF {
            get { return this.state == ReadState.EndOfFile; }
        }

        public override void Close() {
            this.processor = null;
            this.state     = ReadState.Closed;
            Reset();
        }

        public override ReadState ReadState {
            get { return this.state; }
        }

        // Whole Content Read Methods
        public override string ReadString() {
            string result = string.Empty;

            if (NodeType == XmlNodeType.Element || NodeType == XmlNodeType.Attribute || this.currentInfo == this.attributeValue) {
                if(this.mainNode.IsEmptyTag) {
                    return result;
                }
                if (! Read()) {
                    throw new InvalidOperationException(Res.GetString(Res.Xml_InvalidOperation));
                }
            }

            StringBuilder   sb    = null;
            bool            first = true;

            while(true) {
                switch (NodeType) {
                case XmlNodeType.Text:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
//              case XmlNodeType.CharacterEntity:
                    if (first) {
                        result = this.Value;
                        first = false;
                    } else {
                        if (sb == null) {
                            sb = new StringBuilder(result);
                        }
                        sb.Append(this.Value);
                    }
                    if (! Read())
                        throw new InvalidOperationException(Res.GetString(Res.Xml_InvalidOperation));
    		        break;
                default:
                    return (sb == null) ? result : sb.ToString();
                }
            }
        }

        public override string ReadInnerXml() {
            if (ReadState == ReadState.Interactive) {
                if (NodeType == XmlNodeType.Element && ! IsEmptyElement) {
                    StringOutput output = new StringOutput(this.processor);
                    output.OmitXmlDecl();
                    int depth = Depth;

                    Read();                 // skeep  begin Element
                    while (depth < Depth) { // process content
                        Debug.Assert(this.builder != null);
                        output.RecordDone(this.builder);
                        Read();
                    }
                    Debug.Assert(NodeType == XmlNodeType.EndElement);
                    Read();                 // skeep end element

                    output.TheEnd();
                    return output.Result;
                }
                else if(NodeType == XmlNodeType.Attribute) {
                    return encoder.AtributeInnerXml(Value);
                }
                else {
                    Read();
                }
            }
            return string.Empty;
        }

        public override string ReadOuterXml() {
            if (ReadState == ReadState.Interactive) {
                if (NodeType == XmlNodeType.Element) {
                    StringOutput output = new StringOutput(this.processor);
                    output.OmitXmlDecl();
                    bool emptyElement = IsEmptyElement;
                    int  depth        = Depth;
                    // process current record
                    output.RecordDone(this.builder); 
                    Read();                          
                    // process internal elements & text nodes
                    while(depth < Depth) {                      
                        Debug.Assert(this.builder != null);
                        output.RecordDone(this.builder);
                        Read();
                    }
                    // process end element
                    if (! emptyElement) {
                        output.RecordDone(this.builder);            
                        Read();
                    }

                    output.TheEnd();
                    return output.Result; 
                }
                else if(NodeType == XmlNodeType.Attribute) {
                    return encoder.AtributeOuterXml(Name, Value);
                }
                else {
                    Read();
                }
            }
            return string.Empty;
        }

        //
        // Nametable and Namespace Helpers
        //

        public override XmlNameTable NameTable {
            get {
                Debug.Assert(this.nameTable != null);
                return this.nameTable;
            }
        }

        public override string LookupNamespace(string prefix) {
            prefix = this.nameTable.Get(prefix);

            if (this.manager != null && prefix != null) {
                return this.manager.ResolveNamespace(prefix);
            }
            return null;
        }

        public override void ResolveEntity() {
            Debug.Assert(NodeType != XmlNodeType.EntityReference);

            if (NodeType != XmlNodeType.EntityReference) {
                throw new InvalidOperationException(Res.GetString(Res.Xml_InvalidOperation));
            }
        }

        public override bool ReadAttributeValue() {
            if (ReadState != ReadState.Interactive || NodeType != XmlNodeType.Attribute) {
                return false;
            }

            if (this.attributeValue == null) {
                this.attributeValue = new BuilderInfo();
                this.attributeValue.NodeType = XmlNodeType.Text;
            }
            if (this.currentInfo == this.attributeValue) {
                return false;
            }

            this.attributeValue.Value = this.currentInfo.Value;
            this.attributeValue.Depth = this.currentInfo.Depth + 1;
            this.currentInfo          = this.attributeValue;

            return true;
        }

        //
        // RecordOutput interface method implementation
        //

        public Processor.OutputResult RecordDone(RecordBuilder record) {
            this.builder        = record;
            this.mainNode       = record.MainNode;
            this.attributeList  = record.AttributeList;
            this.attributeCount = record.AttributeCount;
            this.manager        = record.Manager;

            this.haveRecord     = true;
            SetMainNode();

            return Processor.OutputResult.Interrupt;
        }

        [System.Runtime.TargetedPatchingOptOutAttribute("Performance critical to inline across NGen image boundaries")]
        public void TheEnd() {
            // nothing here, was taken care of by RecordBuilder
        }

        //
        // Implementation internals
        //

        private void SetMainNode() {
            this.currentIndex   = -1;
            this.currentInfo    = this.mainNode;
        }

        [System.Runtime.TargetedPatchingOptOutAttribute("Performance critical to inline across NGen image boundaries")]
        private void SetAttribute(int attrib) {
            Debug.Assert(0 <= attrib && attrib < this.attributeCount);
            Debug.Assert(0 <= attrib && attrib < this.attributeList.Count);
            Debug.Assert(this.attributeList[attrib] is BuilderInfo);

            this.currentIndex = attrib;
            this.currentInfo  = (BuilderInfo) this.attributeList[attrib];
        }

        private BuilderInfo GetBuilderInfo(int attrib) {
            if (attrib < 0 || this.attributeCount <= attrib) {
                throw new ArgumentOutOfRangeException("attrib");
            }

            Debug.Assert(this.attributeList[attrib] is BuilderInfo);

            return(BuilderInfo) this.attributeList[attrib];
        }

        private bool FindAttribute(String localName, String namespaceURI, out int attrIndex) {
            if (namespaceURI == null) {
                namespaceURI = string.Empty;
            }
            if (localName == null) {
                localName    = string.Empty;
            }

            for (int index = 0; index < this.attributeCount; index ++) {
                Debug.Assert(this.attributeList[index] is BuilderInfo);

                BuilderInfo attribute = (BuilderInfo) this.attributeList[index];
                if (attribute.NamespaceURI == namespaceURI && attribute.LocalName == localName) {
                    attrIndex = index;
                    return true;
                }
            }

            attrIndex = -1;
            return false;
        }

        private bool FindAttribute(String name, out int attrIndex) {
            if (name == null) {
                name  = string.Empty;
            }

            for (int index = 0; index < this.attributeCount; index ++) {
                Debug.Assert(this.attributeList[index] is BuilderInfo);

                BuilderInfo attribute = (BuilderInfo) this.attributeList[index];
                if (attribute.Name == name) {
                    attrIndex = index;
                    return true;
                }
            }

            attrIndex = -1;
            return false;
        }

        private void Reset() {
            this.currentIndex = -1;
            this.currentInfo  = s_DefaultInfo;
            this.mainNode     = s_DefaultInfo;
            this.manager      = null;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void CheckCurrentInfo() {
            Debug.Assert(this.currentInfo   != null);
            Debug.Assert(this.attributeCount == 0 || this.attributeList != null);
            Debug.Assert((this.currentIndex == -1) == (this.currentInfo == this.mainNode));
            Debug.Assert((this.currentIndex == -1) || (this.currentInfo == this.attributeValue || this.attributeList[this.currentIndex] is BuilderInfo && this.attributeList[this.currentIndex] == this.currentInfo));
        }

        private class XmlEncoder {
            private StringBuilder  buffer  = null;
            private XmlTextEncoder encoder = null;

            private void Init() {
                buffer  = new StringBuilder();
                encoder = new XmlTextEncoder(new StringWriter(buffer, CultureInfo.InvariantCulture));
            }

            public string AtributeInnerXml(string value) {
                if(encoder == null) Init();
                buffer .Length = 0;       // clean buffer
                encoder.StartAttribute(/*save:*/false);
                encoder.Write(value);
                encoder.EndAttribute();
                return buffer.ToString();
            }

            public string AtributeOuterXml(string name, string value) {
                if(encoder == null) Init();
                buffer .Length = 0;       // clean buffer
                buffer .Append(name);
                buffer .Append('=');
                buffer .Append(QuoteChar);
                encoder.StartAttribute(/*save:*/false);
                encoder.Write(value);
                encoder.EndAttribute();
                buffer .Append(QuoteChar);
                return buffer.ToString();
            }

            public char QuoteChar {
                get { return '"'; }
            }
        }
    }
}
