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
			if (!c.Input.MoveToFirstChild ()) return;
			
			value = c.CompileTemplateContent ();
			c.Input.MoveToParent ();
		}

		public override void Evaluate (XslTransformProcessor p)
		{
			StringWriter s = new StringWriter ();
			//TODO: only text nodes allowed here, other nodes must be ignored along with any content,
			//so TextOutputter is not really suitable here
			Outputter outputter = new TextOutputter(s);
			p.PushOutput (outputter);
			value.Evaluate (p);
			p.PopOutput ();
			
			p.Out.WriteProcessingInstruction (name.Evaluate (p), s.ToString ());
		}
	}
}
