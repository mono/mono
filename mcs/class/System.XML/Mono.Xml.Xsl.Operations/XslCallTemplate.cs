//
// XslCallTemplate.cs
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
	public class XslCallTemplate : XslCompiledElement {
		XmlQualifiedName name;
		ArrayList withParams;
		public XslCallTemplate (Compiler c) : base (c) {}
		
		protected override void Compile (Compiler c)
		{
			c.AssertAttribute ("name");
			name = c.ParseQNameAttribute ("name");
			
			if (c.Input.MoveToFirstChild ()) {
				do {
					switch (c.Input.NodeType) {
					case XPathNodeType.Comment:
					case XPathNodeType.ProcessingInstruction:
					case XPathNodeType.Whitespace:
						continue;
					case XPathNodeType.Element:
						if (c.Input.NamespaceURI != XsltNamespace)
							throw new XsltCompileException ("unexptected element", null, c.Input); // TODO: fwd compat
						
						switch (c.Input.LocalName)
						{
							case "with-param":
								if (withParams == null) withParams = new ArrayList ();
								withParams.Add (new XslVariableInformation (c));
								break;
							default:
								throw new XsltCompileException ("unexptected element", null, c.Input); // todo forwards compat
						}
						break;
					default:
						throw new XsltCompileException ("unexptected node type " + c.Input.NodeType, null, c.Input); // TODO: fwd compat
					}
				} while (c.Input.MoveToNext ());
				c.Input.MoveToParent ();
			}
		}
		
		public override void Evaluate (XslTransformProcessor p)
		{			
			p.CallTemplate (name, withParams);
		}
	}
}
