//------------------------------------------------------------------------------
// <copyright file="XsltContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

using System.Collections;
using System.Xml.XPath;

namespace System.Xml.Xsl {
    public interface IXsltContextFunction {
        int               Minargs    { get; }
        int               Maxargs    { get; }
        XPathResultType   ReturnType { get; }
        XPathResultType[] ArgTypes   { get; }
        object            Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext);
    }

    public interface IXsltContextVariable {
        bool            IsLocal { get; }
        bool            IsParam { get; }
        XPathResultType VariableType { get; }
        object          Evaluate(XsltContext xsltContext);
    }

    public abstract class XsltContext : XmlNamespaceManager {
        protected XsltContext(NameTable table) : base(table) {}
        protected XsltContext() : base(new NameTable()) {}
            // This dummy XsltContext that doesn't actualy initialize XmlNamespaceManager
            // is used by XsltCompileContext
        internal  XsltContext(bool dummy) : base() {} 
        public abstract IXsltContextVariable ResolveVariable(string prefix, string name);
        public abstract IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] ArgTypes);
        public abstract bool Whitespace { get; }
        public abstract bool PreserveWhitespace(XPathNavigator node);
        public abstract int CompareDocument (string baseUri, string nextbaseUri);
    }
}
