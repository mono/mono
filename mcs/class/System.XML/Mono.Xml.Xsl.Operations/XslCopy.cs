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
				if (p.Out.CanProcessAttributes && useAttributeSets != null)
					foreach (XmlQualifiedName s in useAttributeSets)
						p.ResolveAttributeSet (s).Evaluate (p);

				if (children != null) children.Evaluate (p);
				break;
			case XPathNodeType.Element:
				bool cdataStarted = false;
				if (!p.InsideCDataElement && p.PushCDataState (p.CurrentNode.LocalName, p.CurrentNode.NamespaceURI))
					cdataStarted = true;
				p.Out.WriteStartElement (p.CurrentNode.Prefix, p.CurrentNode.LocalName, p.CurrentNode.NamespaceURI);
				
				p.TryStylesheetNamespaceOutput (null);
				if (useAttributeSets != null)
					foreach (XmlQualifiedName s in useAttributeSets)
						p.ResolveAttributeSet (s).Evaluate (p);

				if (p.CurrentNode.MoveToFirstNamespace (XPathNamespaceScope.Local)) {
					do {
						p.Out.WriteNamespaceDecl (p.CurrentNode.LocalName, p.CurrentNode.Value);
					} while (p.CurrentNode.MoveToNextNamespace (XPathNamespaceScope.Local));
					p.CurrentNode.MoveToParent ();
				}
			
				if (children != null) children.Evaluate (p);

				p.Out.WriteFullEndElement ();
				if (cdataStarted)
					p.PopCDataState ();
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
