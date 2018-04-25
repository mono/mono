//------------------------------------------------------------------------------
// <copyright file="XPathDescendantIterator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace MS.Internal.Xml.XPath {
    using System.Xml.XPath;

    internal class XPathDescendantIterator: XPathAxisIterator {
        private int level = 0;

        public XPathDescendantIterator(XPathNavigator nav, XPathNodeType type, bool matchSelf) : base(nav, type, matchSelf) {}
        public XPathDescendantIterator(XPathNavigator nav, string name, string namespaceURI, bool matchSelf) : base(nav, name, namespaceURI, matchSelf) {}

        public XPathDescendantIterator(XPathDescendantIterator it) : base(it) {
            this.level = it.level;
        }

        public override XPathNodeIterator Clone() {
            return new XPathDescendantIterator(this);
        }

        public override bool MoveNext() {
            if (first) {
                first = false;
                if (matchSelf && Matches) {
                    position = 1;
                    return true;
                }
            }

            while (true) {
                if (nav.MoveToFirstChild()) {
                    level++;
                } else {
                    while (true) {
                        if (level == 0) {
                            return false;
                        }
                        if (nav.MoveToNext()) {
                            break;
                        }
                        nav.MoveToParent();
                        level--;
                    }
                }

                if (Matches) {
                    position++;
                    return true;
                }
            }
        }
    }
}
