//------------------------------------------------------------------------------
// <copyright file="XPathAncestorIterator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace MS.Internal.Xml.XPath {
    using System;
    using System.Xml;
    using System.Xml.XPath;
    using System.Diagnostics;
    using System.Globalization;

    internal class XPathAncestorIterator: XPathAxisIterator {
        public XPathAncestorIterator(XPathNavigator nav, XPathNodeType type, bool matchSelf) : base(nav, type, matchSelf) {}
        public XPathAncestorIterator(XPathNavigator nav, string name, string namespaceURI, bool matchSelf) : base(nav, name, namespaceURI, matchSelf) {}
        public XPathAncestorIterator(XPathAncestorIterator other) : base(other) { }

        public override bool MoveNext() {
            if (first) {
                first = false;
                if(matchSelf && Matches) {
                    position = 1;
                    return true;
                }
            }

            while (nav.MoveToParent()) {
                if (Matches) {
                    position ++;
                    return true;
                }
            }
            return false;
        }

        public override XPathNodeIterator Clone() { return new XPathAncestorIterator(this); }
    }    
}

