//
// XslElement.cs
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
	public class XslElement : XslCompiledElement {
		XslAvt name;
		XslAvt ns;
		XslOperation value;
		XmlQualifiedName [] useAttributeSets;
		
		public XslElement (Compiler c) : base (c) {}
		protected override void Compile (Compiler c)
		{
			name = c.ParseAvtAttribute ("name");
			ns = c.ParseAvtAttribute ("namespace");
			useAttributeSets = c.ParseQNameListAttribute ("use-attribute-sets");
			
			if (c.Input.MoveToFirstChild ()) {
				value = c.CompileTemplateContent ();
				c.Input.MoveToParent ();
			}
		}
		
		public override void Evaluate (XslTransformProcessor p)
		{
			string nm = name.Evaluate (p);
			string nsUri = ns == null ? null : ns.Evaluate (p);
			
			p.Out.WriteStartElement (nm, nsUri);
			if (useAttributeSets != null)
				foreach (XmlQualifiedName s in useAttributeSets)
					p.ResolveAttributeSet (s).Evaluate (p);
			
			if (value != null) value.Evaluate (p);
			p.Out.WriteEndElement ();
		}
	}
}
