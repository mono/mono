//
// XslValueOf.cs
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
	public class XslValueOf : XslCompiledElement {
		XPathExpression select;
		bool disableOutputEscaping;

		public XslValueOf (Compiler c) : base (c) {}

		protected override void Compile (Compiler c)
		{
			c.AssertAttribute ("select");
			select = c.CompileExpression (c.GetAttribute ("select"));
			disableOutputEscaping = c.ParseYesNoAttribute ("disable-output-escaping", false);
			if (c.CurrentStylesheet.Version == "1.0" && c.Input.MoveToFirstChild ())
				throw new XsltCompileException ("XSLT value-of element cannot contain any child.", null, c.Input);
		}
		
		public override void Evaluate (XslTransformProcessor p)
		{
			if (!disableOutputEscaping)
				p.Out.WriteString (p.EvaluateString (select));
			else
				p.Out.WriteRaw (p.EvaluateString (select));
		}
	}
}
