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
		ArrayList withParams = new ArrayList ();
		public XslCallTemplate (Compiler c) : base (c) {}
		
		protected override void Compile (Compiler c)
		{
			c.AssertAttribute ("name");
			name = c.ParseQNameAttribute ("name");
			
			if (c.Input.MoveToFirstChild ()) {
				do {
					if (c.Input.NamespaceURI != XsltNamespace)
						throw new Exception ("unexptected element"); // TODO: fwd compat
					
					switch (c.Input.LocalName)
					{
						case "with-param":
							withParams.Add (new XslVariableInformation (c));
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
			Hashtable passedParams = null;
			
			if (withParams.Count > 0) {
				passedParams = new Hashtable ();
				foreach (XslVariableInformation param in withParams)
					passedParams [param.Name] = param.Evaluate (p);
			}
			
			p.CallTemplate (name, passedParams);
		}
	}
}
