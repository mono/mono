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
			if (!c.Input.MoveToFirstChild ()) throw new Exception ("Expecting non-empty element");
			
			do {
				if (c.Input.NodeType != XPathNodeType.Element) continue;
				if (c.Input.NamespaceURI != XsltNamespace) continue;
				
				if (defaultChoice != null)
					throw new Exception ("otherwise attribute must be last");

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
					break; // TODO: forwards compat
				}
			} while (c.Input.MoveToNext ());
			
			c.Input.MoveToParent ();
			
			if (conditions.Count == 0)
				throw new Exception ("Choose must have 1 or ore when elements");
		}

		public override void Evaluate (XslTransformProcessor p)
		{
			foreach (XslIf test in conditions) {
				if (test.EvaluateIfTrue (p))
					return;
			}
			if (defaultChoice != null) 	
				defaultChoice.Evaluate (p);
		}

	}
}
