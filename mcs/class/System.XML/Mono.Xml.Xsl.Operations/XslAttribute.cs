//
// XslAttribute.cs
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
	public class XslAttribute : XslCompiledElement {
		XslAvt name;
		XslAvt ns;
		XslOperation value;
		public XslAttribute (Compiler c) : base (c) {}
		
		protected override void Compile (Compiler c)
		{
			name = c.ParseAvtAttribute ("name");
			ns = c.ParseAvtAttribute ("namespace");
			if (c.Input.MoveToFirstChild ()) {
				value = c.CompileTemplateContent ();
				c.Input.MoveToParent ();
			}
		}

		public override void Evaluate (XslTransformProcessor p)
		{
			
			string nm = name.Evaluate (p);
			if (ns == null)
				p.Out.WriteStartAttribute (nm, "");
			else
				p.Out.WriteStartAttribute (nm, ns.Evaluate (p));
			
			if (value != null) value.Evaluate (p);
			p.Out.WriteEndAttribute ();
		}
	}
}
