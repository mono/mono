//
// XslForEach.cs
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
	public class XslForEach : XslCompiledElement {
		XPathExpression select;
		XslOperation children;
		
		public XslForEach (Compiler c) : base (c) {}
		
		protected override void Compile (Compiler c)
		{
			c.AssertAttribute ("select");
			select = c.CompileExpression (c.GetAttribute ("select"));
			
			if (c.Input.MoveToFirstChild ()) {
				bool alldone = true;
				do {
					if (c.Input.NodeType == XPathNodeType.Text)
						{ alldone = false; break; }
					
					if (c.Input.NodeType != XPathNodeType.Element)
						continue;
					if (c.Input.NamespaceURI != Compiler.XsltNamespace)
						{ alldone = false; break; }
					if (c.Input.LocalName != "sort")
						{ alldone = false; break; }
						
					c.AddSort (select, new Sort (c));
					
				} while (c.Input.MoveToNext ());
				if (!alldone)
					children = c.CompileTemplateContent ();
				c.Input.MoveToParent ();
			}
		}
		
		public override void Evaluate (XslTransformProcessor p)
		{
			p.PushNodeset (p.Select (select));
			p.PushForEachContext ();
			
			while (p.NodesetMoveNext ())
				children.Evaluate (p);
			p.PopForEachContext();
			p.PopNodeset ();
		}
	}
}
