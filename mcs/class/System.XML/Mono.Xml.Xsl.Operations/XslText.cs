//
// XslText.cs
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
	public class XslText : XslCompiledElement {
		bool disableOutputEscaping;
		string text;
		
		public XslText (Compiler c) : base (c) {}

		protected override void Compile (Compiler c)
		{
			if (c.Input.NodeType == XPathNodeType.Text || c.Input.NodeType == XPathNodeType.SignificantWhitespace)
				this.text = c.Input.Value;
			else if (c.Input.MoveToFirstChild ()) {
				this.text = "";
				do {
					switch (c.Input.NodeType) {
					case XPathNodeType.Text:
					case XPathNodeType.Whitespace:
					case XPathNodeType.SignificantWhitespace:
						this.text += c.Input.Value;
						break;
					case XPathNodeType.Comment:
					case XPathNodeType.ProcessingInstruction:
						break;
					default:
						throw new Exception ("unexpected value");
					}
				} while (c.Input.MoveToNext ());
				c.Input.MoveToParent ();
			} else {
				Debug.WriteLine ("IN XslText, what do i do");
			}
		}
		

		public override void Evaluate (XslTransformProcessor p)
		{
			if (text != null) p.Out.WriteString (text);
		}
	}
}