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
			
			if (value != null && value.ReturnType != XPathResultType.Number && value.ReturnType != XPathResultType.Any)
				throw new Exception ("The expression for attribute 'value' must return a number");
			
			format = c.ParseAvtAttribute ("format");
			lang = c.ParseAvtAttribute ("lang");
			letterValue = c.ParseAvtAttribute ("letter-value");
			groupingSeparator = c.ParseAvtAttribute ("grouping-separator");
			groupingSize = c.ParseAvtAttribute ("grouping-size");
		}

		public override void Evaluate (XslTransformProcessor p)
		{
			p.Out.WriteString (GetFormat (p));
		}
		
		XslNumberFormatter GetNumberFormatter (XslTransformProcessor p)
		{
			string format = "1. ";
			string lang = null;
			string letterValue = null;
			char groupingSeparator = '\0';
			int groupingSize = 0;
			
			if (this.format != null)
				format = this.format.Evaluate (p);
			
			if (this.lang != null)
				lang = this.lang.Evaluate (p);
			
			if (this.letterValue != null)
				letterValue = this.letterValue.Evaluate (p);
			
			if (this.groupingSeparator != null)
				groupingSeparator = this.groupingSeparator.Evaluate (p) [0];
			
			if (this.groupingSize != null)
				groupingSize = int.Parse (this.groupingSize.Evaluate (p));
			
			return new XslNumberFormatter (format, lang, letterValue, groupingSeparator, groupingSize);
		}
		
		string GetFormat (XslTransformProcessor p)
		{
			XslNumberFormatter nf = GetNumberFormatter (p);
			
			if (this.value != null)
				return nf.Format ((int)p.EvaluateNumber (this.value)); // TODO: Correct rounding
			
			switch (this.level) {
				case XslNumberingLevel.Single:
					throw new NotImplementedException ();
				case XslNumberingLevel.Multiple:
					throw new NotImplementedException ();
				case XslNumberingLevel.Any:
					throw new NotImplementedException ();
				default:
					throw new Exception ("Should not get here");
			}
		}
		
		class XslNumberFormatter {
			public XslNumberFormatter (string format, string lang, string letterValue, char groupingSeparator, int groupingSize)
			{
				throw new NotImplementedException ();
			}
			
			public int NumbersNeeded {
				get { throw new NotImplementedException (); }
			}
			
			// return the format for a single value, ie, if using Single or Any
			public string Format (int value)
			{
				throw new NotImplementedException ();
			}
			
			// format for an array of numbers.
			public string Format (int [] values)
			{
				throw new NotImplementedException ();
			}
		}
	}
	
	public enum XslNumberingLevel
	{
		Single,
		Multiple,
		Any
	}
}
