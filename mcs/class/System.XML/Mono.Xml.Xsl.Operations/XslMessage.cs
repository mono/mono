//
// XslMessage.cs
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

namespace Mono.Xml.Xsl.Operations {
	public class XslMessage : XslCompiledElement {
		bool terminate;
		XslOperation children;
		
		public XslMessage (Compiler c) : base (c) {}
		
		protected override void Compile (Compiler c)
		{
			terminate = c.ParseYesNoAttribute ("terminate", false);
			
			if (!c.Input.MoveToFirstChild ()) return;
			children = c.CompileTemplateContent ();
			c.Input.MoveToParent ();
		}
		
		public override void Evaluate (XslTransformProcessor p)
		{
			if (children != null) {
				p.PushOutput (new XmlTextWriter (Console.Error));
				children.Evaluate (p);
				p.PopOutput ();
			}
			
			if (terminate)
				throw new Exception ("XSLT TERMINATION");
		}
	}
}
