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
		
		#region XSLT Internal Calls
		
		internal virtual IXsltContextVariable ResolveVariable (XmlQualifiedName name)
		{
			throw new InvalidOperationException ("somehow you got into the internals of xslt!?");
		}
		
		internal virtual IXsltContextFunction ResolveFunction (XmlQualifiedName name, XPathResultType [] ArgTypes)
		{
			throw new InvalidOperationException ("somehow you got into the internals of xslt!?");
		}
		#endregion
	}
	
	// The static XSLT context, will try to simplify what it can
	internal interface IStaticXsltContext
	{
		Expression TryGetVariable (string nm);
		Expression TryGetFunction (XmlQualifiedName nm, FunctionArguments args);
		XmlQualifiedName LookupQName (string s);
		XmlNamespaceManager GetNsm ();
	}
}
