//
// XslDecimalFormat.cs
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	
// (C) 2003 Ben Maurer
//

using System;
using System.Collections;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl {
	public class XslDecimalFormat {
		
		NumberFormatInfo info = new NumberFormatInfo ();
		char digit = '#', zeroDigit = '0', patternSeparator = ';';

		public static readonly XslDecimalFormat Default = new XslDecimalFormat ();
		
		XslDecimalFormat () {} // Default ctor for default info.
		public XslDecimalFormat (Compiler c)
		{
			XPathNavigator n = c.Input;
			if (n.MoveToFirstAttribute ()) {
				do {
					if (n.NamespaceURI != Compiler.XsltNamespace)
						continue;
					
					switch (n.LocalName) {
						case "name": break; // already handled
						case "decimal-separator":
							info.NumberDecimalSeparator = n.Value;
							break;
						
						case "grouping-separator":
							info.NumberGroupSeparator = n.Value;
							break;
						
						case "infinity":
							info.PositiveInfinitySymbol = n.Value;
							break;
						case "minus-sign":
							info.NegativeSign = n.Value;
							break;
						case "NaN":
							info.NaNSymbol = n.Value;
							break;
						case "percent":
							info.PercentSymbol = n.Value;
							break;
						case "per-mille":
							info.PerMilleSymbol = n.Value;
							break;
						case "zero-digit":
							digit = n.Value [0];
							break;
						case "digit":
							zeroDigit = n.Value [0];
							break;
						case "pattern-separator":
							patternSeparator = n.Value [0];
							break;
					}
				} while (n.MoveToNextAttribute ());
				n.MoveToParent ();
				
				info.NegativeInfinitySymbol = info.NegativeSign + info.PositiveInfinitySymbol;
			}
		}
		
		// check that the data in c is the same as this one, as we
		// must do, per the spec.
		public void CheckSameAs (Compiler c)
		{
			throw new NotImplementedException ();
		}
		
		public string FormatNumber (double number, string pattern)
		{
			throw new NotImplementedException ();
		}
	}
}