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
			
		void CopyNode (XmlWriter w, XPathNavigator nav)
		{
			switch (nav.NodeType) {
			case XPathNodeType.Root:
				XPathNodeIterator itr = nav.SelectChildren (XPathNodeType.All);
				while (itr.MoveNext ())
					CopyNode (w, itr.Current);
				break;
				
			case XPathNodeType.Element:
				w.WriteStartElement (nav.Prefix, nav.LocalName, nav.NamespaceURI);
				
				if (nav.MoveToFirstNamespace (XPathNamespaceScope.Local))
				{
					do {
						if (nav.Name != "")
							w.WriteAttributeString ("xmlns", nav.Name, null, nav.Value);
						else 
							w.WriteAttributeString ("xmlns", nav.Value);
					} while (nav.MoveToNextNamespace (XPathNamespaceScope.Local));
					nav.MoveToParent ();
				}
				
				if (nav.MoveToFirstAttribute())
				{
					do {
						w.WriteStartAttribute (nav.Prefix, nav.LocalName, nav.NamespaceURI);
						w.WriteString (nav.Value);
						w.WriteEndAttribute (); 
					} while (nav.MoveToNextAttribute ());
					nav.MoveToParent();
				}
				
				if (nav.MoveToFirstChild ()) {
					do {
						CopyNode (w, nav);
					} while (nav.MoveToNext ());
					nav.MoveToParent ();
				}
				
				w.WriteEndElement ();
				break;
			case XPathNodeType.Whitespace:                                                     

			case XPathNodeType.SignificantWhitespace:
			case XPathNodeType.Text:
				w.WriteString (nav.Value);
				break;
			case XPathNodeType.ProcessingInstruction:
				w.WriteProcessingInstruction (nav.Name, nav.Value);
				break;
			case XPathNodeType.Comment:
				w.WriteComment (nav.Value);
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
					CopyNode (p.Out, itr.Current);
			} else {
				p.Out.WriteString (XPathFunctions.ToString (o));
			}

		}
	}
}
