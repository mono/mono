//
// XslValueOf.cs
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//	
// (C) 2003 Ben Maurer
// (C) 2003 Atsushi Enomoto
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
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Mono.Xml.Xsl.Operations {
	internal class XslValueOf : XslCompiledElement {
		XPathExpression select;
		bool disableOutputEscaping;

		public XslValueOf (Compiler c) : base (c) {}

		protected override void Compile (Compiler c)
		{
			if (c.Debugger != null)
				c.Debugger.DebugCompile (this.DebugInput);

			c.CheckExtraAttributes ("value-of", "select", "disable-output-escaping");

			c.AssertAttribute ("select");
			select = c.CompileExpression (c.GetAttribute ("select"));
			disableOutputEscaping = c.ParseYesNoAttribute ("disable-output-escaping", false);
			if (c.Input.MoveToFirstChild ()) {
				do {
					switch (c.Input.NodeType) {
					case XPathNodeType.Element:
						if (c.Input.NamespaceURI == XsltNamespace)
							goto case XPathNodeType.SignificantWhitespace;
						// otherwise element in external namespace -> ignore
						break;
					case XPathNodeType.Text:
					case XPathNodeType.SignificantWhitespace:
						throw new XsltCompileException ("XSLT value-of element cannot contain any child.", null, c.Input);
					}
				} while (c.Input.MoveToNext ());
			}
		}
		
		public override void Evaluate (XslTransformProcessor p)
		{
			if (p.Debugger != null)
				p.Debugger.DebugExecute (p, this.DebugInput);

			if (!disableOutputEscaping)
				p.Out.WriteString (p.EvaluateString (select));
			else
				p.Out.WriteRaw (p.EvaluateString (select));
		}
	}
}
