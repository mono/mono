//
// XslComment.cs
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
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Mono.Xml.Xsl.Operations {
	public class XslComment : XslCompiledElement {
		bool disableOutputEscaping = false;
		XslOperation value;
		XPathNavigator nav;
		
		public XslComment (Compiler c) : base (c) {}

		protected override void Compile (Compiler c)
		{
			this.nav = c.Input.Clone ();

			if (c.Input.MoveToFirstChild ()) {
				value = c.CompileTemplateContent ();
				c.Input.MoveToParent ();
			}
		}
		
		public override void Evaluate (XslTransformProcessor p)
		{
			StringWriter s = new StringWriter ();
			XmlWriter w = new XmlTextWriter (s);
			
			p.PushOutput (w);
			value.Evaluate (p);
			p.PopOutput ();
			
			p.Out.WriteComment (s.ToString ());
		}
	}
}
