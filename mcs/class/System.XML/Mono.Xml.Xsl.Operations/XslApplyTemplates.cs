//
// XslApplyTemplates.cs
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

	public class XslApplyTemplates : XslCompiledElement {
		XPathExpression select;
		XmlQualifiedName mode;
		ArrayList withParams = new ArrayList ();
		
		public XslApplyTemplates (Compiler c) : base (c) {}
		
		protected override void Compile (Compiler c)
		{
			select = c.CompileExpression (c.GetAttribute ("select"));
			mode = c.ParseQNameAttribute ("mode");
			
			if (c.Input.MoveToFirstChild ()) {
				do {
					if (c.Input.NamespaceURI != XsltNamespace)
						throw new Exception ("unexptected element"); // TODO: fwd compat
					
					switch (c.Input.LocalName)
					{
						case "with-param":
							withParams.Add (new XslWithParam (c));
							break;
							
						case "sort":
							if (select == null)
								select = c.CompileExpression ("*");
							c.AddSort (select, new Sort (c));
							break;
						default:
							throw new Exception ("unexptected element"); // todo forwards compat
					}
				} while (c.Input.MoveToNext ());
				c.Input.MoveToParent ();
			}
		}
		
		public override void Evaluate (XslTransformProcessor p)
		{
			foreach (XslWithParam param in withParams)
				param.Evaluate (p);
			
			if (select == null)	
				p.ApplyTemplates (p.CurrentNode.SelectChildren (XPathNodeType.All), mode, withParams);
			else
				p.ApplyTemplates (p.Select (select), mode, withParams);
						
			foreach (XslWithParam param in withParams)
				param.Clear ();
		}
	}
}
