//
// XslCopyOf.cs
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
	public class XslCopyOf : XslCompiledElement {
		XPathExpression select;
		public XslCopyOf (Compiler c) : base (c) {}
		protected override void Compile (Compiler c)
		{
			c.AssertAttribute ("select");
			select = c.CompileExpression (c.GetAttribute ("select"));
		}
			
		void CopyNode (XslTransformProcessor p, XPathNavigator nav)
		{
			Outputter outputter = p.Out;
			switch (nav.NodeType) {
			case XPathNodeType.Root:
				XPathNodeIterator itr = nav.SelectChildren (XPathNodeType.All);
				while (itr.MoveNext ())
					CopyNode (p, itr.Current);
				break;
				
			case XPathNodeType.Element:
				bool isCData = p.InsideCDataElement;
				p.PushElementState (nav.LocalName, nav.NamespaceURI, false);
				outputter.WriteStartElement (nav.Prefix, nav.LocalName, nav.NamespaceURI);
				
				if (nav.MoveToFirstNamespace (XPathNamespaceScope.Local))
				{
					do {
						outputter.WriteNamespaceDecl (nav.Name, nav.Value);
					} while (nav.MoveToNextNamespace (XPathNamespaceScope.Local));
					nav.MoveToParent ();
				}
				
				if (nav.MoveToFirstAttribute())
				{
					do {
						outputter.WriteAttributeString (nav.Prefix, nav.LocalName, nav.NamespaceURI, nav.Value);
					} while (nav.MoveToNextAttribute ());
					nav.MoveToParent();
				}
				
				if (nav.MoveToFirstChild ()) {
					do {
						CopyNode (p, nav);
					} while (nav.MoveToNext ());
					nav.MoveToParent ();
				}

				if (nav.IsEmptyElement)
					outputter.WriteEndElement ();
				else
					outputter.WriteFullEndElement ();

				p.PopCDataState (isCData);
				break;
				
			case XPathNodeType.Namespace:
				outputter.WriteNamespaceDecl (nav.Name, nav.Value);
				break;
			case XPathNodeType.Attribute:
				outputter.WriteAttributeString (nav.Prefix, nav.LocalName, nav.NamespaceURI, nav.Value);
				break;
			case XPathNodeType.Whitespace:
			case XPathNodeType.SignificantWhitespace:
				bool cdata = outputter.InsideCDataSection;
				outputter.InsideCDataSection = false;
				outputter.WriteString (nav.Value);
				outputter.InsideCDataSection = cdata;
				break;
			case XPathNodeType.Text:
				outputter.WriteString (nav.Value);
				break;
			case XPathNodeType.ProcessingInstruction:
				outputter.WriteProcessingInstruction (nav.Name, nav.Value);
				break;
			case XPathNodeType.Comment:
				outputter.WriteComment (nav.Value);
				break;
			}			
		}
	
		public override void Evaluate (XslTransformProcessor p)
		{
			object o = p.Evaluate (select);
			if (o is XPathNodeIterator)
			{
				XPathNodeIterator itr = (XPathNodeIterator)o;
				while (itr.MoveNext ())
					CopyNode (p, itr.Current);
			} else {
				p.Out.WriteString (XPathFunctions.ToString (o));
			}

		}
	}
}
