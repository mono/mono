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

using System;
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Mono.Xml.Xsl.Operations {
	public class XslChoose : XslCompiledElement {
		XslOperation defaultChoice = null;
		ArrayList conditions = new ArrayList ();
		
		public XslChoose (Compiler c) : base (c) {}
		
		protected override void Compile (Compiler c)
		{
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
					if (c.Input.MoveToFirstChild ()) {
						defaultChoice = c.CompileTemplateContent ();
						c.Input.MoveToParent ();
					}
					break;

				default:
					if (c.CurrentStylesheet.Version == "1.0")
						throw new XsltCompileException ("XSLT choose element accepts only when and otherwise elements.", null, c.Input);
					break;
				}
			} while (c.Input.MoveToNext ());
			
			c.Input.MoveToParent ();
			
			if (conditions.Count == 0)
				throw new XsltCompileException ("Choose must have 1 or ore when elements", null, c.Input);
		}

		public override void Evaluate (XslTransformProcessor p)
		{
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
