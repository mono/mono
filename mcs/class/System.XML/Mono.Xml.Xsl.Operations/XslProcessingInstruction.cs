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

using System;
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.IO;

namespace Mono.Xml.Xsl.Operations {
	public class XslProcessingInstruction : XslCompiledElement {
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
			Outputter outputter = new TextOutputter(s, true);
			p.PushOutput (outputter);
			value.Evaluate (p);
			p.PopOutput ();
			
			string actualName = name.Evaluate (p);
			if (actualName.ToLower () == "xml")
				throw new XsltException ("Processing instruction name was evaluated to \"xml\"", null, p.CurrentNode);
			p.Out.WriteProcessingInstruction (actualName, s.ToString ());
		}
	}
}
