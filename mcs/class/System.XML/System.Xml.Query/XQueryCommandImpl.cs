//
// XQueryCommandImpl.cs - core XQueryCommand implementation in System.Xml.dll
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if NET_2_0
using System;
using System.Collections;
using System.IO;
using System.Security.Policy;
using System.Xml;
using System.Xml.Query;
using System.Xml.XPath;
using Mono.Xml.XPath2;
using Mono.Xml.XQuery.Parser;

namespace Mono.Xml.XQuery
{
	public class XQueryCommandImpl
	{
		XQueryStaticContext staticContext;

		public XQueryCommandImpl ()
		{
		}

		public void Compile (TextReader input, Evidence evidence)
		{
			staticContext = XQueryASTCompiler.Compile (XQueryParser.Parse (input), null);
			// FIXME: generate executable assembly, and load it with evidence.
		}

		public void Execute (XPathNavigator input, XmlResolver resolver, XmlArgumentList args, XmlWriter writer)
		{
			XQueryContext ctx = new XQueryContext (new XQueryContextManager (staticContext, input, writer, resolver, args));

			XPathSequence iter = new SingleItemIterator (input, ctx);

			foreach (ExprSingle expr in staticContext.QueryBody)
				expr.Serialize (iter);
		}
	}

}

#endif
