//
// XslNumber.cs
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
	public class XslNumber : XslCompiledElement {
		XslNumberingLevel level;
		XPathExpression count;
		XPathExpression from;
		XPathExpression value;
		string format;
		string XmlLang;
		string letterValue;
		XPathExpression groupingSeparator;
		XPathExpression groupingSize;
		
		public XslNumber (Compiler c) : base (c) {}
		
		protected override void Compile (Compiler c)
		{
			throw new NotImplementedException ();
		}
		// TODO: parse this
		public override void Evaluate (XslTransformProcessor p)
		{
			throw new NotImplementedException ();
		}
	}
	
	public enum XslNumberingLevel
	{
		Single,
		Multiple,
		Any
	}
}
