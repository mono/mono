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
	[MonoTODO]
	public class XPathMessageContext : XsltContext
	{
		public XPathMessageContext ()
			: base ()
		{
		}

		public XPathMessageContext (NameTable nameTable)
			: base (nameTable)
		{
		}

		[MonoTODO]
		public override bool Whitespace {
			get { throw new NotImplementedException (); }
		}

		public override int CompareDocument (string uri1, string uri2)
		{
			return String.CompareOrdinal (uri1, uri2);
		}

		[MonoTODO]
		public override bool PreserveWhitespace (XPathNavigator node)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IXsltContextFunction ResolveFunction (
			string prefix, string name, XPathResultType [] argTypes)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IXsltContextVariable ResolveVariable (
			string prefix, string name)
		{
			throw new NotImplementedException ();
		}
	}
}