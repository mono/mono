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

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Mono.Xml.Xsl.Operations {
	internal class XslCopyOf : XslCompiledElement {
		XPathExpression select;
		public XslCopyOf (Compiler c) : base (c) {}
		protected override void Compile (Compiler c)
		{
			if (c.Debugger != null)
				c.Debugger.DebugCompile (c.Input);

			c.CheckExtraAttributes ("copy-of", "select");

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
				string prefix = nav.Prefix;
				string ns = nav.NamespaceURI;
				p.PushElementState (prefix, nav.LocalName, ns, false);
				outputter.WriteStartElement (prefix, nav.LocalName, ns);
				
				if (nav.MoveToFirstNamespace (XPathNamespaceScope.ExcludeXml))
				{
					do {
						if (prefix == nav.Name)
							continue;
						if (nav.Name.Length == 0 && ns.Length == 0)
							continue;
						outputter.WriteNamespaceDecl (nav.Name, nav.Value);
					} while (nav.MoveToNextNamespace (XPathNamespaceScope.ExcludeXml));
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
				if (nav.Name != p.XPathContext.ElementPrefix &&
					(p.XPathContext.ElementNamespace.Length > 0 || nav.Name.Length > 0))
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
			if (p.Debugger != null)
				p.Debugger.DebugExecute (p, this.DebugInput);

			object o = p.Evaluate (select);
			XPathNodeIterator itr = o as XPathNodeIterator;
			if (itr != null) {
				while (itr.MoveNext ())
					CopyNode (p, itr.Current);
			} else {
				XPathNavigator nav = o as XPathNavigator; // RTF
				if (nav != null)
					CopyNode (p, nav);
				else
					p.Out.WriteString (XPathFunctions.ToString (o));
			}
		}
	}
}
