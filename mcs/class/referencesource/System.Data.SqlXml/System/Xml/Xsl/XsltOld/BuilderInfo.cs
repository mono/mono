//------------------------------------------------------------------------------
// <copyright file="BuilderInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.XsltOld {
    using Res = System.Xml.Utils.Res;
    using System;
    using System.Diagnostics; 
    using System.Text;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.XPath;

    internal class BuilderInfo {
        private     string              name;
        private     string              localName;
        private     string              namespaceURI;
        private     string              prefix;
        
        private     XmlNodeType         nodeType;
        private     int                 depth;
        private     bool                isEmptyTag;
                
        internal    string[]            TextInfo = new string[4];
        internal    int                 TextInfoCount = 0;        

        internal    bool                search;
        internal    HtmlElementProps    htmlProps;
        internal    HtmlAttributeProps  htmlAttrProps;

        internal BuilderInfo() {
            Initialize(string.Empty, string.Empty, string.Empty);
        }

        internal void Initialize(string prefix, string name, string nspace) {
            this.prefix         = prefix;
            this.localName      = name;
            this.namespaceURI   = nspace;
            this.name           = null;
            this.htmlProps      = null;
            this.htmlAttrProps  = null;
            this.TextInfoCount  = 0;
        }

        internal void Initialize(BuilderInfo src) {
            this.prefix        = src.Prefix;
            this.localName     = src.LocalName;
            this.namespaceURI  = src.NamespaceURI;
            this.name          = null;
            this.depth         = src.Depth;
            this.nodeType      = src.NodeType;
            this.htmlProps     = src.htmlProps;
            this.htmlAttrProps = src.htmlAttrProps;

            this.TextInfoCount = 0;
            EnsureTextInfoSize(src.TextInfoCount);
            src.TextInfo.CopyTo(this.TextInfo, 0);
            this.TextInfoCount = src.TextInfoCount;
        }
        
        void EnsureTextInfoSize(int newSize) {
            if (this.TextInfo.Length < newSize) {
                string[] newArr = new string[newSize * 2];
                Array.Copy(this.TextInfo, newArr, this.TextInfoCount);
                this.TextInfo = newArr;
            }
        }
        
        internal BuilderInfo Clone() {
            BuilderInfo info = new BuilderInfo();
            info.Initialize(this);
            Debug.Assert(info.NodeType != XmlNodeType.Text || XmlCharType.Instance.IsOnlyWhitespace(info.Value));
            return info;
        }
       
        internal string Name {
            get {
                if (this.name == null) {
                    string prefix    = Prefix;
                    string localName = LocalName;

                    if (prefix != null && 0 < prefix.Length) {
                        if (localName.Length > 0) {
                            this.name = prefix + ":" + localName;
                        }
                        else {
                            this.name = prefix;
                        }
                    }
                    else {
                        this.name = localName;
                    }
                }
                return this.name;
            }
        }

        internal string LocalName {
            get { return this.localName; }
            set { this.localName = value; }
        }
        internal string NamespaceURI {
            get { return this.namespaceURI; }
            set { this.namespaceURI = value; }
        }
        internal string Prefix {
            get { return this.prefix; }
            set { this.prefix = value; }
        }

        // The true value of this object is a list of TextInfo
        // Value.get merges them together but discards each node's escape info
        // Value.set clears this object, and appends the new, single string
        internal string Value {
            get {
                switch  (this.TextInfoCount) {
                case 0: return string.Empty;
                case 1: return this.TextInfo[0];
                default :
                    int size = 0;
                    for (int i = 0; i < this.TextInfoCount; i++) {
                        string ti = this.TextInfo[i];
                        if (ti == null) continue; // ignore disableEscaping
                        size += ti.Length;
                    }
                    StringBuilder sb = new StringBuilder(size);
                    for (int i = 0; i < this.TextInfoCount; i++) {
                        string ti = this.TextInfo[i];
                        if (ti == null) continue; // ignore disableEscaping
                        sb.Append(ti);
                    }
                    return sb.ToString();
                }
            }
            set {
                this.TextInfoCount = 0;
                ValueAppend(value, /*disableEscaping:*/false);
            }
        }

        internal void ValueAppend(string s, bool disableEscaping) {
            if (s == null || s.Length == 0) {
                return;
            }
            EnsureTextInfoSize(this.TextInfoCount + (disableEscaping ? 2 : 1));
            if (disableEscaping) {
                this.TextInfo[this.TextInfoCount ++] = null;
            }
            this.TextInfo[this.TextInfoCount++] = s;
        }

        internal XmlNodeType NodeType {
            get { return this.nodeType; }
            set { this.nodeType = value; }
        }
        internal int Depth {
            get { return this.depth; }
            set { this.depth = value; }
        }
        internal bool IsEmptyTag {
            get { return this.isEmptyTag; }
            set { this.isEmptyTag = value; }
        }
    }
}
