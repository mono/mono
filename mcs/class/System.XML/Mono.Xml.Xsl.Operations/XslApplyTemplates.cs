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
					switch (c.Input.NodeType) {
					case XPathNodeType.Comment:
					case XPathNodeType.ProcessingInstruction:
					case XPathNodeType.Whitespace:
						continue;
					case XPathNodeType.Element:
						if (c.Input.NamespaceURI != XsltNamespace)
							throw new Exception ("unexptected element"); // TODO: fwd compat
						
						switch (c.Input.LocalName)
						{
							case "with-param":
								withParams.Add (new XslVariableInformation (c));
								break;
								
							case "sort":
								if (select == null)
									select = c.CompileExpression ("*");
								c.AddSort (select, new Sort (c));
								break;
							default:
								throw new Exception ("unexptected element"); // todo forwards compat
						}
						break;
					default:
						throw new Exception ("unexpected node type " + c.Input.NodeType);	// todo forwards compat
					}
				} while (c.Input.MoveToNext ());
				c.Input.MoveToParent ();
			}
		}
		
		public override void Evaluate (XslTransformProcessor p)
		{
			Hashtable passedParams = null;
			
			if (withParams.Count > 0) {
				passedParams = new Hashtable ();
				foreach (XslVariableInformation param in withParams)
					passedParams [param.Name] = param.Evaluate (p);
			}
			
			if (select == null)	
				p.ApplyTemplates (p.CurrentNode.SelectChildren (XPathNodeType.All), mode, passedParams);
			else
				p.ApplyTemplates (p.Select (select), mode, passedParams);
		}
	}
}
