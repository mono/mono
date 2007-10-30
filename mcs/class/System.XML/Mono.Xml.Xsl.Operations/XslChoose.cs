//
// XslChoose.cs
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
	internal class XslChoose : XslCompiledElement {
		XslOperation defaultChoice = null;
		ArrayList conditions = new ArrayList ();
		
		public XslChoose (Compiler c) : base (c) {}
		
		protected override void Compile (Compiler c)
		{
			if (c.Debugger != null)
				c.Debugger.DebugCompile (c.Input);

			c.CheckExtraAttributes ("choose");

			if (!c.Input.MoveToFirstChild ())
				throw new XsltCompileException ("Expecting non-empty element", null, c.Input);
			
			do {
				if (c.Input.NodeType != XPathNodeType.Element) continue;
				if (c.Input.NamespaceURI != XsltNamespace) continue;
				
				if (defaultChoice != null)
					throw new XsltCompileException ("otherwise attribute must be last", null, c.Input);

				switch (c.Input.LocalName) {
				case "when":
					conditions.Add (new XslIf (c));
					break;
					
				case "otherwise":
					c.CheckExtraAttributes ("otherwise");
					if (c.Input.MoveToFirstChild ()) {
						defaultChoice = c.CompileTemplateContent ();
						c.Input.MoveToParent ();
					}
					break;

				default:
					if (c.CurrentStylesheet.Version == "1.0")
						throw new XsltCompileException ("XSLT choose element accepts only when and otherwise elements", null, c.Input);
					break;
				}
			} while (c.Input.MoveToNext ());
			
			c.Input.MoveToParent ();
			
			if (conditions.Count == 0)
				throw new XsltCompileException ("Choose must have 1 or more when elements", null, c.Input);
		}

		public override void Evaluate (XslTransformProcessor p)
		{
			if (p.Debugger != null)
				p.Debugger.DebugExecute (p, this.DebugInput);

			int len = conditions.Count;
			for (int i = 0; i < len; i++) {
				if (((XslIf)conditions [i]).EvaluateIfTrue (p))
					return;
			}
			
			if (defaultChoice != null) 	
				defaultChoice.Evaluate (p);
		}

	}
}
