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

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl.Operations {	
	public class XslElement : XslCompiledElement {
		XslAvt name, ns;
		string calcName, calcNs;
		
		XslOperation value;
		XmlQualifiedName [] useAttributeSets;
		
		public XslElement (Compiler c) : base (c) {}
		protected override void Compile (Compiler c)
		{
			name = c.ParseAvtAttribute ("name");
			ns = c.ParseAvtAttribute ("namespace");
			
			calcName = XslAvt.AttemptPreCalc (ref name);
			
			if (calcName != null && ns == null) {
				QName q = XslNameUtil.FromString (calcName, c.Input);
				calcName = q.Name;
				calcNs = q.Namespace;	
			} else if (ns != null)
				calcNs = XslAvt.AttemptPreCalc (ref ns);
			
			
			useAttributeSets = c.ParseQNameListAttribute ("use-attribute-sets");
			
			if (c.Input.MoveToFirstChild ()) {
				value = c.CompileTemplateContent ();
				c.Input.MoveToParent ();
			}
		}
		
		public override void Evaluate (XslTransformProcessor p)
		{
			string nm, nmsp;
			
			nm = calcName != null ? calcName : name.Evaluate (p);
			nmsp = calcNs != null ? calcNs : ns != null ? ns.Evaluate (p) : null;
			
			if (nmsp == null)
				throw new NotImplementedException ();
			
			p.Out.WriteStartElement (nm, nmsp);
			
			if (useAttributeSets != null)
				foreach (XmlQualifiedName s in useAttributeSets)
					p.ResolveAttributeSet (s).Evaluate (p);
			
			if (value != null) value.Evaluate (p);
			p.Out.WriteEndElement ();
		}
	}
}
