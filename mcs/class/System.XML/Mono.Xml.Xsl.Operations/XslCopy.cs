//
// XslCopy.cs
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
	public class XslCopy : XslCompiledElement {
		XslOperation children;
		XmlQualifiedName [] useAttributeSets;
		
		public XslCopy (Compiler c) : base (c) {}
		
		protected override void Compile (Compiler c)
		{
			useAttributeSets = c.ParseQNameListAttribute ("use-attribute-sets");
			
			if (!c.Input.MoveToFirstChild ()) return;
			children = c.CompileTemplateContent();
			c.Input.MoveToParent ();
		}

		public override void Evaluate (XslTransformProcessor p)
		{
			switch (p.CurrentNode.NodeType)
			{
			case XPathNodeType.Root:
				if (children != null) children.Evaluate (p);
				break;
			case XPathNodeType.Element:
				p.Out.WriteStartElement (p.CurrentNode.Prefix, p.CurrentNode.LocalName, p.CurrentNode.NamespaceURI);
				
				if (useAttributeSets != null)
					foreach (XmlQualifiedName s in useAttributeSets)
						p.ResolveAttributeSet (s).Evaluate (p);
			
				if (children != null) children.Evaluate (p);
				p.Out.WriteEndElement ();
				break;
			case XPathNodeType.Attribute:
				p.Out.WriteAttributeString (p.CurrentNode.Prefix, p.CurrentNode.LocalName, p.CurrentNode.NamespaceURI, p.CurrentNode.Value);
				break;
			
			case XPathNodeType.SignificantWhitespace:
			case XPathNodeType.Text:
			case XPathNodeType.Whitespace:
				p.Out.WriteString (p.CurrentNode.Value);
				break;
			
			case XPathNodeType.Comment:
				p.Out.WriteComment (p.CurrentNode.Value);
				break;
			
			case XPathNodeType.ProcessingInstruction:
				p.Out.WriteProcessingInstruction (p.CurrentNode.Name, p.CurrentNode.Value);
				break;
			
			default:
				Console.WriteLine ("unhandled node type {0}", p.CurrentNode.NodeType);
				break;
			}
		}
	}
}
