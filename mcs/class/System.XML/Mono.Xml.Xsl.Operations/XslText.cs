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
		bool disableOutputEscaping = false;
		string text = "";
		
		public XslText (Compiler c) : base (c) {}

		protected override void Compile (Compiler c)
		{
			this.text = c.Input.Value;
			
			if (c.Input.NodeType == XPathNodeType.Element)
				this.disableOutputEscaping = c.ParseYesNoAttribute ("disable-output-escaping", false);
		}
		

		public override void Evaluate (XslTransformProcessor p)
		{
			if (!disableOutputEscaping)
				p.Out.WriteString (text);
			else
				p.Out.WriteRaw (text);
		}
	}
}