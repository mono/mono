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
		XPathNavigator source;
		string baseUri;
		int lineNumber;
		int linePosition;

		public static readonly XslDecimalFormat Default = new XslDecimalFormat ();
		
		XslDecimalFormat () {} // Default ctor for default info.
		public XslDecimalFormat (Compiler c)
		{
			XPathNavigator n = c.Input;

			IXmlLineInfo li = n as IXmlLineInfo;
			if (li != null) {
				lineNumber = li.LineNumber;
				linePosition = li.LinePosition;
			}
			baseUri = n.BaseURI;

			if (n.MoveToFirstAttribute ()) {
				do {
					if (n.NamespaceURI != String.Empty)
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
						case "digit":
							digit = n.Value [0];
							break;
						case "zero-digit":
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

		// TODO: complete comparison for XSLT spec. 12.3.
		public void CheckSameAs (XslDecimalFormat other)
		{
			if (this.digit != other.digit ||
				this.patternSeparator != other.patternSeparator ||
				this.zeroDigit != other.zeroDigit)
				throw new XsltCompileException (null, other.baseUri, other.lineNumber, other.linePosition);
		}
		
		// TODO: format pattern check.
		public string FormatNumber (double number, string pattern)
		{
			return number.ToString (pattern, info);
		}
	}
}