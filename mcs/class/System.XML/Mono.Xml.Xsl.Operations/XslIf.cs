//
// XslIf.cs
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
	// also applicable to xsl:when
	public class XslIf : XslCompiledElement {
		XPathExpression test;
		XslOperation children;
		
		public XslIf (Compiler c) : base (c) {}
		
		protected override void Compile (Compiler c)
		{
			c.AssertAttribute ("test");
			test = c.CompileExpression (c.GetAttribute ("test"));

			if (!c.Input.MoveToFirstChild ()) return;
			children = c.CompileTemplateContent ();
			c.Input.MoveToParent ();
		}	
		
		public bool EvaluateIfTrue (XslTransformProcessor p)
		{
			if (p.EvaluateBoolean (test)) {
				children.Evaluate (p);
				return true;
			}
			return false;
		}

		public override void Evaluate (XslTransformProcessor p)
		{
			EvaluateIfTrue (p);
		}
	}
}
