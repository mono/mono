//------------------------------------------------------------------------------
// <copyright file="Root.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace MS.Internal.Xml.XPath {
    using System;
    using System.Xml;
    using System.Xml.XPath;

    internal class Root : AstNode {
        public Root() {}

        public override AstType         Type       { get { return AstType.Root;            } }
        public override XPathResultType ReturnType { get { return XPathResultType.NodeSet; } }
    }
}
