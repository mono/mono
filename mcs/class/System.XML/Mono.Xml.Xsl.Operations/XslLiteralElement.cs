//
// XslLiteralElement.cs
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
	public class XslLiteralElement : XslCompiledElement {
		XslOperation children;
		string localname, prefix, nsUri;
		ArrayList attrs = new ArrayList ();
		XmlQualifiedName [] useAttributeSets;
		
		public XslLiteralElement (Compiler c) : base (c) {}
			
		class XslLiteralAttribute {
			string localname, prefix, nsUri;
			XslAvt val;
			
			public XslLiteralAttribute (Compiler c)
			{
				this.prefix = c.Input.Prefix;
				this.nsUri = c.Input.NamespaceURI;
				this.localname = c.Input.LocalName;
				this.val = new XslAvt (c.Input.Value, c);
			}
			
			public void Evaluate (XslTransformProcessor p)
			{
				p.Out.WriteAttributeString (prefix, localname, nsUri, val.Evaluate (p));
			}
		}
		
		protected override void Compile (Compiler c)
		{
			
			this.prefix = c.Input.Prefix;
			this.nsUri = c.Input.NamespaceURI;
			this.localname = c.Input.LocalName;
			this.useAttributeSets = c.ParseQNameListAttribute ("use-attribute-sets", XsltNamespace);
			
			if (c.Input.MoveToFirstAttribute ())
			{
				do {
					if (c.Input.NamespaceURI == XsltNamespace)
						continue; //already handled
					attrs.Add (new XslLiteralAttribute (c));
				} while (c.Input.MoveToNextAttribute());
				c.Input.MoveToParent ();
			}
			
			if (!c.Input.MoveToFirstChild ()) return;
			children = c.CompileTemplateContent ();
			c.Input.MoveToParent ();
		}
		
		public override void Evaluate (XslTransformProcessor p)
		{
			p.Out.WriteStartElement (prefix, localname, nsUri);

			foreach (XslLiteralAttribute a in attrs)
				a.Evaluate (p);
			
			if (useAttributeSets != null)
				foreach (XmlQualifiedName s in useAttributeSets)
					p.ResolveAttributeSet (s).Evaluate (p);
						
			if (children != null) children.Evaluate (p);
			p.Out.WriteEndElement ();
		}
	}
}
