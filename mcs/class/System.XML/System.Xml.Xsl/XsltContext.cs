// System.Xml.Xsl.XsltContext
//
// Author: Tim Coleman <tim@timcoleman.com>
// (C) Copyright 2002 Tim Coleman

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Xml;
using System.Xml.XPath;

namespace System.Xml.Xsl
{
	public abstract class XsltContext : XmlNamespaceManager
	{
		#region Constructors
		protected 	XsltContext ()
			: base (new NameTable ())
		{
		}

		protected XsltContext (NameTable table)
			: base (table)
		{
		}

		#endregion

		#region Properties

		public abstract bool Whitespace { get; }
		public abstract bool PreserveWhitespace (XPathNavigator node);

		#endregion

		#region Methods

		public abstract int CompareDocument (string baseUri, string nextbaseUri);
		public abstract IXsltContextFunction ResolveFunction (string prefix, string name, XPathResultType [] ArgTypes);
		public abstract IXsltContextVariable ResolveVariable (string prefix, string name);

		#endregion
		
		#region XSLT Internal Calls
		
		internal virtual IXsltContextVariable ResolveVariable (XmlQualifiedName name)
		{
			return ResolveVariable (LookupPrefix (name.Namespace), name.Name);
		}
		
		internal virtual IXsltContextFunction ResolveFunction (XmlQualifiedName name, XPathResultType [] argTypes)
		{
			return ResolveFunction (name.Name, name.Namespace, argTypes);
		}
		#endregion
	}
	
	// The static XSLT context, will try to simplify what it can
	internal interface IStaticXsltContext
	{
		Expression TryGetVariable (string nm);
		Expression TryGetFunction (XmlQualifiedName nm, FunctionArguments args);
		XmlQualifiedName LookupQName (string s);
		string LookupNamespace (string prefix);
	}
}
