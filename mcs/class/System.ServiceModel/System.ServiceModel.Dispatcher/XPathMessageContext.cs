//
// XPathMessageContext.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Xml.Xsl;
using System.ServiceModel;

namespace System.ServiceModel.Dispatcher
{
	public class XPathMessageContext : XsltContext
	{
		public XPathMessageContext ()
			: this (new NameTable ())
		{
		}

		public XPathMessageContext (NameTable table)
			: base (table)
		{
			AddNamespace ("s11", Constants.Soap11);
			AddNamespace ("s12", Constants.Soap12);
		}

		public override bool Whitespace {
			get { return false; } // as documented.
		}

		public override int CompareDocument (string baseUri, string nextBaseUri)
		{
			return String.CompareOrdinal (baseUri, nextBaseUri);
		}

		public override bool PreserveWhitespace (XPathNavigator node)
		{
			return false; // as documented.
		}

		public override IXsltContextFunction ResolveFunction (
			string prefix, string name, XPathResultType [] argTypes)
		{
			return null;
		}

		public override IXsltContextVariable ResolveVariable (
			string prefix, string name)
		{
			return null;
		}
	}
}