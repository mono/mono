//------------------------------------------------------------------------------
// <copyright file="AbsoluteQuery.cs" company="Microsoft">
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

    internal sealed class AbsoluteQuery : ContextQuery {
        public  AbsoluteQuery()                    : base() {}
        private AbsoluteQuery(AbsoluteQuery other) : base(other) {}

        public override object Evaluate(XPathNodeIterator context) {
            base.contextNode = context.Current.Clone();
            base.contextNode.MoveToRoot();
            count = 0;
            return this; 
        }

        public override XPathNavigator MatchNode(XPathNavigator context) {
            if (context != null && context.NodeType == XPathNodeType.Root) {
                return context;
            }
            return null;
        }

        public override XPathNodeIterator Clone() { return new AbsoluteQuery(this); }
    }
}
