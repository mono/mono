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
		
		// <xsl:number
		//   level = "single" | "multiple" | "any"
		XslNumberingLevel level;
		//   count = pattern
		XPathExpression count;
		//   from = pattern
		XPathExpression from;
		//   value = number-expression
		XPathExpression value;
		//   format = { string }
		XslAvt format;
		//   lang = { nmtoken }
		XslAvt lang;
		//   letter-value = { "alphabetic" | "traditional" }
		XslAvt letterValue;
		//   grouping-separator = { char }
		XslAvt groupingSeparator;
		//   grouping-size = { number } />
		XslAvt groupingSize;
		
		public XslNumber (Compiler c) : base (c) {}
		
		protected override void Compile (Compiler c)
		{
			switch (c.GetAttribute ("level"))
			{
			case "single":
				level = XslNumberingLevel.Single;
				break;
			case "multiple":
				level = XslNumberingLevel.Multiple;
				break;
			case "any":
				level = XslNumberingLevel.Any;
				break;
			case null:
			case "":
			default:
				level = XslNumberingLevel.Single; // single == default
				break;
			}
			
			count = c.CompilePattern (c.GetAttribute ("count"));
			from = c.CompilePattern (c.GetAttribute ("from"));
			value = c.CompileExpression (c.GetAttribute ("value"));
			
			if (value.ReturnType != XPathResultType.Number && value.ReturnType != XPathResultType.Any)
				throw new Exception ("The expression for attribute 'value' must return a number");
			
			format = c.ParseAvtAttribute ("format");
			lang = c.ParseAvtAttribute ("lang");
			letterValue = c.ParseAvtAttribute ("letter-value");
			groupingSeparator = c.ParseAvtAttribute ("grouping-separator");
			groupingSize = c.ParseAvtAttribute ("grouping-size");
		}

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
