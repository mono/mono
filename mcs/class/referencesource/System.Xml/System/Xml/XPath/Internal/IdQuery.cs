//------------------------------------------------------------------------------
// <copyright file="IDQuery.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace MS.Internal.Xml.XPath {
    using System;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;
    using System.Diagnostics;
    using System.Collections.Generic;

    internal sealed class IDQuery : CacheOutputQuery {

        public IDQuery(Query arg) : base(arg) {}
        private IDQuery(IDQuery other) : base(other) { }

        public override object Evaluate(XPathNodeIterator context) {
            object argVal = base.Evaluate(context);
            XPathNavigator contextNode = context.Current.Clone();

            switch (GetXPathType(argVal)) {
            case XPathResultType.NodeSet:
                XPathNavigator temp;
                while ((temp = input.Advance()) != null) {
                    ProcessIds(contextNode, temp.Value);
                }
                break;
            case XPathResultType.String:
                ProcessIds(contextNode, (string)argVal);
                break;
            case XPathResultType.Number:
                ProcessIds(contextNode, StringFunctions.toString((double)argVal));
                break;
            case XPathResultType.Boolean:
                ProcessIds(contextNode, StringFunctions.toString((bool)argVal));
                break;
            case XPathResultType_Navigator:
                ProcessIds(contextNode, ((XPathNavigator)argVal).Value);
                break;
            }
            return this;
        }

        void ProcessIds(XPathNavigator contextNode, string val) {
            string[] ids = XmlConvert.SplitString(val);
            for (int idx = 0; idx < ids.Length; idx++) {
                if (contextNode.MoveToId(ids[idx])) {
                    Insert(outputBuffer, contextNode);
                }
            }
        }

        public override XPathNavigator MatchNode(XPathNavigator context) {
            Evaluate(new XPathSingletonIterator(context, /*moved:*/true));
            XPathNavigator result;
            while ((result = Advance()) != null) {
                if (result.IsSamePosition(context)) {
                    return context;
                }
            }
            return null;
        }

        public override XPathNodeIterator Clone() { return new IDQuery(this); }
    }
}
