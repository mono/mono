//
// XslProcessingInstruction.cs
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
using System.Globalization;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.IO;

namespace Mono.Xml.Xsl.Operations {
	internal class XslProcessingInstruction : XslCompiledElement {
		XslAvt name;
		XslOperation value;
		
		public XslProcessingInstruction (Compiler c) : base (c) {}
		
		protected override void Compile (Compiler c)
		{
			name = c.ParseAvtAttribute ("name");

			if (c.Input.MoveToFirstAttribute ()) {
				do {
					if (c.Input.NamespaceURI == String.Empty && c.Input.LocalName != "name")
						throw new XsltCompileException ("Invalid attribute \"" + c.Input.Name + "\"", null, c.Input);
				} while (c.Input.MoveToNextAttribute ());
				c.Input.MoveToParent ();
			}

			if (!c.Input.MoveToFirstChild ()) return;
			
			value = c.CompileTemplateContent (XPathNodeType.ProcessingInstruction);
			c.Input.MoveToParent ();
		}

		public override void Evaluate (XslTransformProcessor p)
		{
			StringWriter s = new StringWriter ();
			if (value != null) {
				Outputter outputter = new TextOutputter(s, true);
				p.PushOutput (outputter);
				value.Evaluate (p);
				p.PopOutput ();
			}
			
			string actualName = name.Evaluate (p);
			if (String.Compare (actualName, "xml", true, CultureInfo.InvariantCulture) == 0)
				throw new XsltException ("Processing instruction name was evaluated to \"xml\"", null, p.CurrentNode);
			if (actualName.IndexOf (':') >= 0)
				return; //MS.NET ignores such processing instructions

			p.Out.WriteProcessingInstruction (actualName, s.ToString ());
		}
	}
}
