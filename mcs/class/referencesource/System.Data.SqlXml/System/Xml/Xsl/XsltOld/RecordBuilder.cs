//------------------------------------------------------------------------------
// <copyright file="RecordBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.XsltOld {
    using Res = System.Xml.Utils.Res;
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Xml;
    using System.Xml.XPath;
    using System.Collections;

    internal sealed class RecordBuilder {
        private int             outputState;
        private RecordBuilder   next;

        RecordOutput            output;

        // Atomization:
        private XmlNameTable    nameTable;
        private OutKeywords     atoms;

        // Namespace manager for output
        private OutputScopeManager  scopeManager;

        // Main node + Fields Collection
        private BuilderInfo     mainNode           = new BuilderInfo();
        private ArrayList       attributeList      = new ArrayList();
        private int             attributeCount;
        private ArrayList       namespaceList      = new ArrayList();
        private int             namespaceCount;
        private BuilderInfo     dummy = new BuilderInfo();

        // Current position in the list
        private BuilderInfo     currentInfo;
        // Builder state
        private bool            popScope;
        private int             recordState;
        private int             recordDepth;

        private const int       NoRecord    = 0;      // No part of a new record was generated (old record was cleared out)
        private const int       SomeRecord  = 1;      // Record was generated partially        (can be eventually record)
        private const int       HaveRecord  = 2;      // Record was fully generated

        private const char      s_Minus         = '-';
        private const string    s_Space         = " ";
        private const string    s_SpaceMinus    = " -";
        private const char      s_Question      = '?';
        private const char      s_Greater       = '>';
        private const string    s_SpaceGreater  = " >";

        private const string    PrefixFormat    = "xp_{0}";

        internal RecordBuilder(RecordOutput output, XmlNameTable nameTable) {
            Debug.Assert(output != null);
            this.output    = output;
            this.nameTable = nameTable != null ? nameTable : new NameTable();
            this.atoms     = new OutKeywords(this.nameTable);
            this.scopeManager   = new OutputScopeManager(this.nameTable, this.atoms);
        }

        //
        // Internal properties
        //

        internal int OutputState {
            get { return this.outputState; }
            set { this.outputState = value; }
        }

        internal RecordBuilder Next {
            get { return this.next; }
            set { this.next = value; }
        }

        internal RecordOutput Output {
            get { return this.output; }
        }

        internal BuilderInfo MainNode {
            get { return this.mainNode; }
        }

        internal ArrayList AttributeList {
            get { return this.attributeList; }
        }

        internal int AttributeCount {
            get { return this.attributeCount; }
        }

        internal OutputScopeManager Manager {
            get { return this.scopeManager; }
        }

        private void ValueAppend(string s, bool disableOutputEscaping) {
            this.currentInfo.ValueAppend(s, disableOutputEscaping);
        }

        private bool CanOutput(int state) {
            Debug.Assert(this.recordState != HaveRecord);

            // If we have no record cached or the next event doesn't start new record, we are OK

            if (this.recordState == NoRecord || (state & StateMachine.BeginRecord) == 0) {
                return true;
            }
            else {
                this.recordState = HaveRecord;
                FinalizeRecord();
                SetEmptyFlag(state);
                return this.output.RecordDone(this) == Processor.OutputResult.Continue;
            }
        }

        internal Processor.OutputResult BeginEvent(int state, XPathNodeType nodeType, string prefix, string name, string nspace, bool empty, Object htmlProps, bool search) {
            if (! CanOutput(state)) {
                return Processor.OutputResult.Overflow;
            }

            Debug.Assert(this.recordState == NoRecord || (state & StateMachine.BeginRecord) == 0);

            AdjustDepth(state);
            ResetRecord(state);
            PopElementScope();

            prefix = (prefix != null) ? this.nameTable.Add(prefix) : this.atoms.Empty;
            name   = (name   != null) ? this.nameTable.Add(name)   : this.atoms.Empty;
            nspace = (nspace != null) ? this.nameTable.Add(nspace) : this.atoms.Empty;

            switch (nodeType) {
            case XPathNodeType.Element:
                this.mainNode.htmlProps = htmlProps as HtmlElementProps;
                this.mainNode.search = search;
                BeginElement(prefix, name, nspace, empty);
                break;
            case XPathNodeType.Attribute:
                BeginAttribute(prefix, name, nspace, htmlProps, search);
                break;
            case XPathNodeType.Namespace:
                BeginNamespace(name, nspace);
                break;
            case XPathNodeType.Text:
                break;
            case XPathNodeType.ProcessingInstruction:
                if (BeginProcessingInstruction(prefix, name, nspace) == false) {
                    return Processor.OutputResult.Error;
                }
                break;
            case XPathNodeType.Comment:
                BeginComment();
                break;
            case XPathNodeType.Root:
                break;
            case XPathNodeType.Whitespace:
            case XPathNodeType.SignificantWhitespace:
            case XPathNodeType.All:
                break;
            }

            return CheckRecordBegin(state);
        }

        internal Processor.OutputResult TextEvent(int state, string text, bool disableOutputEscaping) {
            if (! CanOutput(state)) {
                return Processor.OutputResult.Overflow;
            }

            Debug.Assert(this.recordState == NoRecord || (state & StateMachine.BeginRecord) == 0);

            AdjustDepth(state);
            ResetRecord(state);
            PopElementScope();

            if ((state & StateMachine.BeginRecord) != 0) {
                this.currentInfo.Depth      = this.recordDepth;
                this.currentInfo.NodeType   = XmlNodeType.Text;
            }

            ValueAppend(text, disableOutputEscaping);

            return CheckRecordBegin(state);
        }

        internal Processor.OutputResult EndEvent(int state, XPathNodeType nodeType) {
            if (! CanOutput(state)) {
                return Processor.OutputResult.Overflow;
            }

            AdjustDepth(state);
            PopElementScope();
            this.popScope = (state & StateMachine.PopScope) != 0;

            if ((state & StateMachine.EmptyTag) != 0 && this.mainNode.IsEmptyTag == true) {
                return Processor.OutputResult.Continue;
            }

            ResetRecord(state);

            if ((state & StateMachine.BeginRecord) != 0) {
                if(nodeType == XPathNodeType.Element) {
                    EndElement();
                }
            }

            return CheckRecordEnd(state);
        }

        internal void Reset() {
            if (this.recordState == HaveRecord) {
                this.recordState = NoRecord;
            }
        }

        internal void TheEnd() {
            if (this.recordState == SomeRecord) {
                this.recordState = HaveRecord;
                FinalizeRecord();
                this.output.RecordDone(this);
            }
            this.output.TheEnd();
        }

        //
        // Utility implementation methods
        //

        private int FindAttribute(string name, string nspace, ref string prefix) {
            Debug.Assert(this.attributeCount <= this.attributeList.Count);

            for (int attrib = 0; attrib < this.attributeCount; attrib ++) {
                Debug.Assert(this.attributeList[attrib] != null && this.attributeList[attrib] is BuilderInfo);

                BuilderInfo attribute = (BuilderInfo) this.attributeList[attrib];

                if (Ref.Equal(attribute.LocalName, name)) {
                    if (Ref.Equal(attribute.NamespaceURI, nspace)) {
                        return attrib;
                    }
                    if (Ref.Equal(attribute.Prefix, prefix)) {
                        // prefix conflict. Should be renamed.
                        prefix = string.Empty;
                    }
                }

            }

            return -1;
        }

        private void BeginElement(string prefix, string name, string nspace, bool empty) {
            Debug.Assert(this.attributeCount == 0);

            this.currentInfo.NodeType     = XmlNodeType.Element;
            this.currentInfo.Prefix       = prefix;
            this.currentInfo.LocalName    = name;
            this.currentInfo.NamespaceURI = nspace;
            this.currentInfo.Depth        = this.recordDepth;
            this.currentInfo.IsEmptyTag   = empty;

            this.scopeManager.PushScope(name, nspace, prefix);
        }

        private void EndElement() {
            Debug.Assert(this.attributeCount == 0);
            OutputScope elementScope = this.scopeManager.CurrentElementScope;

            this.currentInfo.NodeType     = XmlNodeType.EndElement;
            this.currentInfo.Prefix       = elementScope.Prefix;
            this.currentInfo.LocalName    = elementScope.Name;
            this.currentInfo.NamespaceURI = elementScope.Namespace;
            this.currentInfo.Depth        = this.recordDepth;
        }

        private int NewAttribute() {
            if (this.attributeCount >= this.attributeList.Count) {
                Debug.Assert(this.attributeCount == this.attributeList.Count);
                this.attributeList.Add(new BuilderInfo());
            }
            return this.attributeCount ++;
        }

        private void BeginAttribute(string prefix, string name, string nspace, Object htmlAttrProps, bool search) {
            int attrib = FindAttribute(name, nspace, ref prefix);

            if (attrib == -1) {
                attrib = NewAttribute();
            }

            Debug.Assert(this.attributeList[attrib] != null && this.attributeList[attrib] is BuilderInfo);

            BuilderInfo attribute = (BuilderInfo) this.attributeList[attrib];
            attribute.Initialize(prefix, name, nspace);
            attribute.Depth = this.recordDepth;
            attribute.NodeType = XmlNodeType.Attribute;
            attribute.htmlAttrProps = htmlAttrProps as HtmlAttributeProps;
            attribute.search = search;
            this.currentInfo  = attribute;
        }

        private void BeginNamespace(string name, string nspace) {
            bool thisScope = false;
            if (Ref.Equal(name, this.atoms.Empty)) {
                if (Ref.Equal(nspace, this.scopeManager.DefaultNamespace)) {
                    // Main Node is OK
                }
                else if (Ref.Equal(this.mainNode.NamespaceURI, this.atoms.Empty)) {
                    // http://www.w3.org/1999/11/REC-xslt-19991116-errata/ E25 
                    // Should throw an error but ingnoring it in Everett. 
                    // Would be a breaking change
                }
                else {
                    DeclareNamespace(nspace, name);
                }
            }
            else {
                string nspaceDeclared = this.scopeManager.ResolveNamespace(name, out thisScope);
                if (nspaceDeclared != null) {
                    if (! Ref.Equal(nspace, nspaceDeclared)) {
                        if(!thisScope) {
                            DeclareNamespace(nspace, name);
                        }
                    }
                }
                else {
                     DeclareNamespace(nspace, name);
                }
            }
            this.currentInfo = dummy;
            currentInfo.NodeType = XmlNodeType.Attribute;
        }

        private bool BeginProcessingInstruction(string prefix, string name, string nspace) {
            this.currentInfo.NodeType     = XmlNodeType.ProcessingInstruction;
            this.currentInfo.Prefix       = prefix;
            this.currentInfo.LocalName    = name;
            this.currentInfo.NamespaceURI = nspace;
            this.currentInfo.Depth  = this.recordDepth;
            return true;
        }

        private void BeginComment() {
            this.currentInfo.NodeType   = XmlNodeType.Comment;
            this.currentInfo.Depth = this.recordDepth;
        }

        private void AdjustDepth(int state) {
            switch (state & StateMachine.DepthMask) {
            case StateMachine.DepthUp:
                this.recordDepth ++;
                break;
            case StateMachine.DepthDown:
                this.recordDepth --;
                break;
            default:
                break;
            }
        }

        private void ResetRecord(int state) {
            Debug.Assert(this.recordState == NoRecord || this.recordState == SomeRecord);

            if ((state & StateMachine.BeginRecord) != 0) {
                this.attributeCount     = 0;
                this.namespaceCount     = 0;
                this.currentInfo        = this.mainNode;

                this.currentInfo.Initialize(this.atoms.Empty, this.atoms.Empty, this.atoms.Empty);
                this.currentInfo.NodeType      = XmlNodeType.None;
                this.currentInfo.IsEmptyTag    = false;
                this.currentInfo.htmlProps     = null;
                this.currentInfo.htmlAttrProps = null;
            }
        }

        private void PopElementScope() {
            if (this.popScope) {
                this.scopeManager.PopScope();
                this.popScope = false;
            }
        }

        private Processor.OutputResult CheckRecordBegin(int state) {
            Debug.Assert(this.recordState == NoRecord || this.recordState == SomeRecord);

            if ((state & StateMachine.EndRecord) != 0) {
                this.recordState = HaveRecord;
                FinalizeRecord();
                SetEmptyFlag(state);
                return this.output.RecordDone(this);
            }
            else {
                this.recordState = SomeRecord;
                return Processor.OutputResult.Continue;
            }
        }

        private Processor.OutputResult CheckRecordEnd(int state) {
            Debug.Assert(this.recordState == NoRecord || this.recordState == SomeRecord);

            if ((state & StateMachine.EndRecord) != 0) {
                this.recordState = HaveRecord;
                FinalizeRecord();
                SetEmptyFlag(state);
                return this.output.RecordDone(this);
            }
            else {
                // For end event, if there is no end token, don't force token
                return Processor.OutputResult.Continue;
            }
        }

        private void SetEmptyFlag(int state) {
            Debug.Assert(this.mainNode != null);

            if ((state & StateMachine.BeginChild) != 0) {
                this.mainNode.IsEmptyTag = false;
            }
        }


        private void AnalyzeSpaceLang() {
            Debug.Assert(this.mainNode.NodeType == XmlNodeType.Element);

            for (int attr = 0; attr < this.attributeCount; attr ++) {
                Debug.Assert(this.attributeList[attr] is BuilderInfo);
                BuilderInfo info = (BuilderInfo) this.attributeList[attr];

                if (Ref.Equal(info.Prefix, this.atoms.Xml)) {
                    OutputScope scope = this.scopeManager.CurrentElementScope;

                    if (Ref.Equal(info.LocalName, this.atoms.Lang)) {
                        scope.Lang  = info.Value;
                    }
                    else if (Ref.Equal(info.LocalName, this.atoms.Space)) {
                        scope.Space = TranslateXmlSpace(info.Value);
                    }
                }
            }
        }

        private void FixupElement() {
            Debug.Assert(this.mainNode.NodeType == XmlNodeType.Element);

            if (Ref.Equal(this.mainNode.NamespaceURI, this.atoms.Empty)) {
                this.mainNode.Prefix = this.atoms.Empty;
            }

            if (Ref.Equal(this.mainNode.Prefix, this.atoms.Empty)) {
                if (Ref.Equal(this.mainNode.NamespaceURI, this.scopeManager.DefaultNamespace)) {
                    // Main Node is OK
                }
                else {
                    DeclareNamespace(this.mainNode.NamespaceURI, this.mainNode.Prefix);
                }
            }
            else {
                bool   thisScope = false;
                string nspace = this.scopeManager.ResolveNamespace(this.mainNode.Prefix, out thisScope);
                if (nspace != null) {
                    if (! Ref.Equal(this.mainNode.NamespaceURI, nspace)) {
                        if (thisScope) {    // Prefix conflict
                            this.mainNode.Prefix = GetPrefixForNamespace(this.mainNode.NamespaceURI);
                        }
                        else {
                            DeclareNamespace(this.mainNode.NamespaceURI, this.mainNode.Prefix);
                        }
                    }
                }
                else {
                    DeclareNamespace(this.mainNode.NamespaceURI, this.mainNode.Prefix);
                }
            }

            OutputScope elementScope = this.scopeManager.CurrentElementScope;
            elementScope.Prefix      = this.mainNode.Prefix;
        }

        private void FixupAttributes(int attributeCount) {
            for (int attr = 0; attr < attributeCount; attr ++) {
                Debug.Assert(this.attributeList[attr] is BuilderInfo);
                BuilderInfo info = (BuilderInfo) this.attributeList[attr];


                if (Ref.Equal(info.NamespaceURI, this.atoms.Empty)) {
                    info.Prefix = this.atoms.Empty;
                }
                else {
                    if (Ref.Equal(info.Prefix, this.atoms.Empty)) {
                        info.Prefix = GetPrefixForNamespace(info.NamespaceURI);
                    }
                    else {
                        bool thisScope = false;
                        string nspace = this.scopeManager.ResolveNamespace(info.Prefix, out thisScope);
                        if (nspace != null) {
                            if (! Ref.Equal(info.NamespaceURI, nspace)) {
                                if(thisScope) { // prefix conflict
                                    info.Prefix = GetPrefixForNamespace(info.NamespaceURI);
                                }
                                else {
                                    DeclareNamespace(info.NamespaceURI, info.Prefix);
                                }
                            }
                        }
                        else {
                            DeclareNamespace(info.NamespaceURI, info.Prefix);
                        }
                    }
                }
            }
        }

        private void AppendNamespaces() {
            for (int i = this.namespaceCount - 1; i >= 0; i --) {
                BuilderInfo attribute = (BuilderInfo) this.attributeList[NewAttribute()];
                attribute.Initialize((BuilderInfo)this.namespaceList[i]);
            }
        }

        private void AnalyzeComment() {
            Debug.Assert(this.mainNode.NodeType == XmlNodeType.Comment);
            Debug.Assert((object) this.currentInfo == (object) this.mainNode);

            StringBuilder newComment = null;
            string        comment    = this.mainNode.Value;
            bool          minus      = false;
            int index = 0, begin = 0;

            for (; index < comment.Length; index ++) {
                switch (comment[index]) {
                    case s_Minus:
                        if (minus) {
                            if (newComment == null)
                                newComment = new StringBuilder(comment, begin, index, 2 * comment.Length);
                            else
                                newComment.Append(comment, begin, index - begin);

                            newComment.Append(s_SpaceMinus);
                            begin = index + 1;
                        }
                        minus = true;
                        break;
                    default:
                        minus = false;
                        break;
                }
            }

            if (newComment != null) {
                if (begin < comment.Length)
                    newComment.Append(comment, begin, comment.Length - begin);

                if (minus)
                    newComment.Append(s_Space);

                this.mainNode.Value = newComment.ToString();
            }
            else if (minus) {
                this.mainNode.ValueAppend(s_Space, false);
            }
        }

        private void AnalyzeProcessingInstruction() {
            Debug.Assert(this.mainNode.NodeType == XmlNodeType.ProcessingInstruction || this.mainNode.NodeType == XmlNodeType.XmlDeclaration);
            //Debug.Assert((object) this.currentInfo == (object) this.mainNode);

            StringBuilder newPI    = null;
            string        pi       = this.mainNode.Value;
            bool          question = false;
            int index = 0, begin = 0;

            for (; index < pi.Length; index ++) {
                switch (pi[index]) {
                case s_Question:
                    question = true;
                    break;
                case s_Greater:
                    if (question) {
                        if (newPI == null) {
                            newPI = new StringBuilder(pi, begin, index, 2 * pi.Length);
                        }
                        else {
                            newPI.Append(pi, begin, index - begin);
                        }
                        newPI.Append(s_SpaceGreater);
                        begin = index + 1;
                    }
                    question = false;
                    break;
                default:
                    question = false;
                    break;
                }
            }

            if (newPI != null) {
                if (begin < pi.Length) {
                    newPI.Append(pi, begin, pi.Length - begin);
                }
                this.mainNode.Value = newPI.ToString();
            }
        }

        private void FinalizeRecord() {
            switch (this.mainNode.NodeType) {
            case XmlNodeType.Element:
                // Save count since FixupElement can add attribute...
                int attributeCount = this.attributeCount;
                
                FixupElement();
                FixupAttributes(attributeCount);
                AnalyzeSpaceLang();
                AppendNamespaces();
                break;
            case XmlNodeType.Comment:
                AnalyzeComment();
                break;
            case XmlNodeType.ProcessingInstruction:
                AnalyzeProcessingInstruction();
                break;
            }
        }

        private int NewNamespace() {
            if (this.namespaceCount >= this.namespaceList.Count) {
                Debug.Assert(this.namespaceCount == this.namespaceList.Count);
                this.namespaceList.Add(new BuilderInfo());
            }
            return this.namespaceCount ++;
        }

        private void DeclareNamespace(string nspace, string prefix) {
            int index = NewNamespace();

            Debug.Assert(this.namespaceList[index] != null && this.namespaceList[index] is BuilderInfo);

            BuilderInfo ns = (BuilderInfo) this.namespaceList[index];
            if (prefix == this.atoms.Empty) {
                ns.Initialize(this.atoms.Empty, this.atoms.Xmlns, this.atoms.XmlnsNamespace);
            }
            else {
                ns.Initialize(this.atoms.Xmlns, prefix, this.atoms.XmlnsNamespace);
            }
            ns.Depth = this.recordDepth;
            ns.NodeType = XmlNodeType.Attribute;
            ns.Value = nspace;

            this.scopeManager.PushNamespace(prefix, nspace);
        }

        private string DeclareNewNamespace(string nspace) {
            string prefix = this.scopeManager.GeneratePrefix(PrefixFormat);
            DeclareNamespace(nspace, prefix);
            return prefix;
        }

        internal string GetPrefixForNamespace(string nspace) {
            string prefix = null;

            if (this.scopeManager.FindPrefix(nspace, out prefix)) {
                Debug.Assert(prefix != null && prefix.Length > 0);
                return prefix;
            }
            else {
                return DeclareNewNamespace(nspace);
            }
        }

        private static XmlSpace TranslateXmlSpace(string space) {
            if (space == "default") {
                return XmlSpace.Default;
            }
            else if (space == "preserve") {
                return XmlSpace.Preserve;
            }
            else {
                return XmlSpace.None;
            }
        }
    }
}
