//------------------------------------------------------------------------------
// <copyright file="ValidatingReaderNodeData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner> 
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Xml.Schema;
using System.Diagnostics;
using System.Globalization;

namespace System.Xml {

    internal class ValidatingReaderNodeData {
        string                  localName;
        string                  namespaceUri;
        string                  prefix;
        string                  nameWPrefix;

        string                  rawValue;
        string                  originalStringValue;  // Original value
        int                     depth;
        AttributePSVIInfo       attributePSVIInfo;  //Used only for default attributes
        XmlNodeType             nodeType;
       
        int                     lineNo;
        int                     linePos;
        
        public ValidatingReaderNodeData() {
            Clear(XmlNodeType.None);
        }
        
        public ValidatingReaderNodeData(XmlNodeType nodeType) {
            Clear(nodeType);
        }

        public string LocalName {
            get {
                return localName;
            }
            set {
                localName = value;
            }
        }

        public string Namespace {
            get {
                return namespaceUri;
            }
            set {
                namespaceUri = value;
            }
        }

        public string Prefix {
            get {
                return prefix;
            }
            set {
                prefix = value;
            }
        }
        
        public string GetAtomizedNameWPrefix(XmlNameTable nameTable) {
            if (nameWPrefix == null) {
                if (prefix.Length == 0 ) {
                    nameWPrefix = localName;
                }
                else {
                    nameWPrefix = nameTable.Add ( string.Concat (prefix,":", localName));
                }
            }
            return nameWPrefix;
        }

        public int Depth {
            get {
                return depth;
            }
            set {
                depth = value;
            }
        }

        public string RawValue {
            get {
                return rawValue;
            }
            set {
                rawValue = value;
            }
        }

        public string OriginalStringValue {
            get {
                return originalStringValue;
            }
            set {
                originalStringValue = value;
            }
        }

        public XmlNodeType NodeType {
            get {
                return nodeType;
            }
            set {
                nodeType = value;
            }
        }
        
        public AttributePSVIInfo AttInfo {
            get {
                return attributePSVIInfo;
            }
            set {
                attributePSVIInfo = value;
            }
        }
        
        public int LineNumber {
            get {
                return lineNo;
            }
        }
        
        public int LinePosition {
            get {
                return linePos;
            }
        }

        internal void Clear( XmlNodeType nodeType ) {
            this.nodeType = nodeType;
            localName = string.Empty;
            prefix = string.Empty;
            namespaceUri = string.Empty;
            rawValue = string.Empty;
            if (attributePSVIInfo != null) {
                attributePSVIInfo.Reset();
            }
            nameWPrefix = null;
            lineNo = 0;
            linePos = 0;
        }

        internal void ClearName() {
            localName = string.Empty;
            prefix = string.Empty;
            namespaceUri = string.Empty;
        }
        
        internal void SetLineInfo( int lineNo, int linePos ) {
            this.lineNo =  lineNo;
            this.linePos = linePos;
        }

        internal void SetLineInfo( IXmlLineInfo lineInfo ) {
            if (lineInfo != null) {
                this.lineNo =  lineInfo.LineNumber;
                this.linePos = lineInfo.LinePosition;
            }
        }

        internal void SetItemData(string localName, string prefix, string ns, string value) {
            this.localName = localName;
            this.prefix = prefix;
            namespaceUri = ns;
            rawValue = value;
        }
        
        internal void SetItemData(string localName, string prefix, string ns, int depth) {
            this.localName = localName;
            this.prefix = prefix;
            namespaceUri = ns;
            this.depth = depth;
            rawValue = string.Empty;
        }

        internal void SetItemData(string value) {
            SetItemData(value, value);
        }

        internal void SetItemData(string value, string originalStringValue) {
            rawValue = value;
            this.originalStringValue = originalStringValue;
        }
    }
}
