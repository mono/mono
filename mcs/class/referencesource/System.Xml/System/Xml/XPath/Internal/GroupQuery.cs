//------------------------------------------------------------------------------
// <copyright file="GroupQuery.cs" company="Microsoft">
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

    internal sealed class GroupQuery : BaseAxisQuery {

        public GroupQuery(Query qy): base(qy) {}
        private GroupQuery(GroupQuery other) : base(other) { }

        public override XPathNavigator Advance() {
            currentNode = qyInput.Advance();
            if (currentNode != null) {
                position++;
            }
            return currentNode;
        }

        public override object Evaluate(XPathNodeIterator nodeIterator) {
            return qyInput.Evaluate(nodeIterator);
        }

        public override XPathNodeIterator Clone() { return new GroupQuery(this); }
        public override XPathResultType StaticType { get { return qyInput.StaticType; } }
        public override QueryProps Properties { get { return QueryProps.Position; } } // Doesn't have QueryProps.Merge
    }
}
