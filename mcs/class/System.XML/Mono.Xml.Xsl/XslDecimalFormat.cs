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
						if (n.Value.Length != 1)
							throw new XsltCompileException ("XSLT decimal-separator value must be exact one character.", null, n);
						info.NumberDecimalSeparator = n.Value;
						break;
						
					case "grouping-separator":
						if (n.Value.Length != 1)
							throw new XsltCompileException ("XSLT grouping-separator value must be exact one character.", null, n);
						info.NumberGroupSeparator = n.Value;
						break;
						
					case "infinity":
						info.PositiveInfinitySymbol = n.Value;
						break;
					case "minus-sign":
						if (n.Value.Length != 1)
							throw new XsltCompileException ("XSLT minus-sign value must be exact one character.", null, n);
						info.NegativeSign = n.Value;
						break;
					case "NaN":
						info.NaNSymbol = n.Value;
						break;
					case "percent":
						if (n.Value.Length != 1)
							throw new XsltCompileException ("XSLT percent value must be exact one character.", null, n);
						info.PercentSymbol = n.Value;
						break;
					case "per-mille":
						if (n.Value.Length != 1)
							throw new XsltCompileException ("XSLT per-mille value must be exact one character.", null, n);
						info.PerMilleSymbol = n.Value;
						break;
					case "digit":
						if (n.Value.Length != 1)
							throw new XsltCompileException ("XSLT digit value must be exact one character.", null, n);
						digit = n.Value [0];
						break;
					case "zero-digit":
						if (n.Value.Length != 1)
							throw new XsltCompileException ("XSLT zero-digit value must be exact one character.", null, n);
						zeroDigit = n.Value [0];
						break;
					case "pattern-separator":
						if (n.Value.Length != 1)
							throw new XsltCompileException ("XSLT pattern-separator value must be exact one character.", null, n);
						patternSeparator = n.Value [0];
						break;
					}
				} while (n.MoveToNextAttribute ());
				n.MoveToParent ();
				
				info.NegativeInfinitySymbol = info.NegativeSign + info.PositiveInfinitySymbol;
			}
		}

		public void CheckSameAs (XslDecimalFormat other)
		{
			if (this.digit != other.digit ||
				this.patternSeparator != other.patternSeparator ||
				this.zeroDigit != other.zeroDigit ||
				this.info.NumberDecimalSeparator != other.info.NumberDecimalSeparator ||
				this.info.NumberGroupSeparator != other.info.NumberGroupSeparator ||
				this.info.PositiveInfinitySymbol != other.info.PositiveInfinitySymbol ||
				this.info.NegativeSign != other.info.NegativeSign ||
				this.info.NaNSymbol != other.info.NaNSymbol ||
				this.info.PercentSymbol != other.info.PercentSymbol ||
				this.info.PerMilleSymbol != other.info.PerMilleSymbol)
				throw new XsltCompileException (null, other.baseUri, other.lineNumber, other.linePosition);
		}
		
		// TODO: format pattern check.
		public string FormatNumber (double number, string pattern)
		{
			return number.ToString (pattern, info);
		}
	}
}