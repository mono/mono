// System.Xml.Xsl.XsltContext
//
// Author: Tim Coleman <tim@timcoleman.com>
// (C) Copyright 2002 Tim Coleman

using System;
using System.Xml;
using System.Xml.XPath;

namespace System.Xml.Xsl
{
	public abstract class XsltContext : XmlNamespaceManager
	{
		#region Constructors

		// should this really be calling new NameTable() in the
		// base() call?
		public XsltContext ()
			: base (new NameTable ())
		{
		}

		public XsltContext (NameTable table)
			: base (table)
		{
		}

		#endregion

		#region Properties

		public abstract bool Whitespace { get; }
		public abstract bool PreserveWhitespace (XPathNavigator nav);

		#endregion

		#region Methods

		public abstract int CompareDocument (string baseUri, string nextbaseUri);
		public abstract IXsltContextFunction ResolveFunction (string prefix, string name, XPathResultType [] ArgTypes);
		public abstract IXsltContextVariable ResolveVariable (string prefix, string name);

		#endregion
	}
}
