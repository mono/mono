//
// XslNotSupportedOperation.cs
//
// Authors:
//	Atsushi Enomoto <atsushi@ximian.com>
//	
// (C)2005 Novell Inc,
//

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
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl.Operations
{
	internal class XslNotSupportedOperation : XslCompiledElement
	{
		string name;
		ArrayList fallbacks;

		public XslNotSupportedOperation (Compiler c)
			: base (c)
		{
		}

		protected override void Compile (Compiler c)
		{
			if (c.Debugger != null)
				c.Debugger.DebugCompile (this.DebugInput);

			name = c.Input.LocalName;
			if (c.Input.MoveToFirstChild ()) {
				do {
					if (c.Input.NodeType != XPathNodeType.Element ||
						c.Input.LocalName != "fallback" ||
						c.Input.NamespaceURI != XslStylesheet.XsltNamespace)
						continue;
					if (fallbacks == null)
						fallbacks = new ArrayList ();
					fallbacks.Add (new XslFallback (c));
				} while (c.Input.MoveToNext ());
				c.Input.MoveToParent ();
			}
		}

		public override void Evaluate (XslTransformProcessor p)
		{
			if (p.Debugger != null)
				p.Debugger.DebugExecute (p, this.DebugInput);

			if (fallbacks != null) {
				foreach (XslFallback f in fallbacks)
					f.Evaluate (p);
			}
			else
				throw new XsltException (String.Format ("'{0}' element is not supported as a template content in XSLT 1.0.", name), null);
		}
	}
}
