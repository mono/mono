//
// XslAttributeSet.cs
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//	
// (C) 2003 Ben Maurer
// (C) 2003 Atsushi Enomoto
//

using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;
using Mono.Xml.Xsl.Operations;

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl {
	public class XslAttributeSet : XslCompiledElement {
		QName name;
		// [QName]=>XslAttributeSet
		ArrayList usedAttributeSets = new ArrayList ();
		
		// [QName]=>XslAttribute
		ArrayList attributes = new ArrayList ();
		
		public XslAttributeSet (Compiler c) : base (c) {}
		
		public QName Name {
			get { return name; }
		}

		protected override void Compile (Compiler c)
		{
			this.name = c.ParseQNameAttribute ("name");
			
			QName [] attrSets = c.ParseQNameListAttribute ("use-attribute-sets");
			if (attrSets != null)
				foreach (QName q in attrSets)
					usedAttributeSets.Add (q);

			
			if (!c.Input.MoveToFirstChild ()) return;
				
			do {
				switch (c.Input.NodeType) {
				case XPathNodeType.Element:
					break;
				case XPathNodeType.Whitespace:
					continue;
				default:
					if (c.CurrentStylesheet.Version == "1.0")
						throw new XsltCompileException ("Content " + c.Input.NodeType + " is not allowed in XSLT attribute-set element.", null, c.Input);
					break;
				}
					
				if (c.Input.NamespaceURI != XsltNamespace || c.Input.LocalName != "attribute")
					throw new Exception ("Invalid attr set content");
				attributes.Add (new XslAttribute (c));
			} while (c.Input.MoveToNext ());
			
			c.Input.MoveToParent ();
			
		}
		
		public void Merge (XslAttributeSet s)
		{
			attributes.AddRange (s.attributes);
			
			foreach (QName q in s.usedAttributeSets)
				if (!usedAttributeSets.Contains (q))
					usedAttributeSets.Add (q);
		}
		
		public override void Evaluate (XslTransformProcessor p) {
			p.SetBusy (this);
			
			if (usedAttributeSets != null) {
				foreach (QName set in usedAttributeSets)
				{
					XslAttributeSet s = p.ResolveAttributeSet (set);
					if (s == null)
						throw new XsltException ("Could not resolve attribute set", null, p.CurrentNode);
					
					if (p.IsBusy (s))
						throw new XsltException ("circular dependency", null, p.CurrentNode);
					
					s.Evaluate (p);
				}
			}
						
			foreach (Operations.XslAttribute a in attributes)
				a.Evaluate (p);
			
			p.SetFree (this);
		}
	}
}
