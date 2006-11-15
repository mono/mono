//
// XslDecimalFormat.cs
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	
// (C) 2003 Ben Maurer
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
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl {
	internal class XslDecimalFormat {
		
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
							throw new XsltCompileException ("XSLT decimal-separator value must be exact one character", null, n);
						info.NumberDecimalSeparator = n.Value;
						break;
						
					case "grouping-separator":
						if (n.Value.Length != 1)
							throw new XsltCompileException ("XSLT grouping-separator value must be exact one character", null, n);
						info.NumberGroupSeparator = n.Value;
						break;
						
					case "infinity":
						info.PositiveInfinitySymbol = n.Value;
						break;
					case "minus-sign":
						if (n.Value.Length != 1)
							throw new XsltCompileException ("XSLT minus-sign value must be exact one character", null, n);
						info.NegativeSign = n.Value;
						break;
					case "NaN":
						info.NaNSymbol = n.Value;
						break;
					case "percent":
						if (n.Value.Length != 1)
							throw new XsltCompileException ("XSLT percent value must be exact one character", null, n);
						info.PercentSymbol = n.Value;
						break;
					case "per-mille":
						if (n.Value.Length != 1)
							throw new XsltCompileException ("XSLT per-mille value must be exact one character", null, n);
						info.PerMilleSymbol = n.Value;
						break;
					case "digit":
						if (n.Value.Length != 1)
							throw new XsltCompileException ("XSLT digit value must be exact one character", null, n);
						digit = n.Value [0];
						break;
					case "zero-digit":
						if (n.Value.Length != 1)
							throw new XsltCompileException ("XSLT zero-digit value must be exact one character", null, n);
						zeroDigit = n.Value [0];
						break;
					case "pattern-separator":
						if (n.Value.Length != 1)
							throw new XsltCompileException ("XSLT pattern-separator value must be exact one character", null, n);
						patternSeparator = n.Value [0];
						break;
					}
				} while (n.MoveToNextAttribute ());
				n.MoveToParent ();
				
				info.NegativeInfinitySymbol = info.NegativeSign + info.PositiveInfinitySymbol;
			}
		}

		public char Digit { get { return digit; } }
		public char ZeroDigit { get { return zeroDigit; } }
		public NumberFormatInfo Info { get { return info; } }
		public char PatternSeparator { get { return patternSeparator; } }

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

		public string FormatNumber (double number, string pattern)
		{
			return ParsePatternSet (pattern).FormatNumber (number);
		}

		private DecimalFormatPatternSet ParsePatternSet (string pattern)
		{
			return new DecimalFormatPatternSet (pattern, this);
		}
	}

	// set of positive pattern and negative pattern
	internal class DecimalFormatPatternSet
	{
		DecimalFormatPattern positivePattern;
		DecimalFormatPattern negativePattern;
//		XslDecimalFormat decimalFormat;

		public DecimalFormatPatternSet (string pattern, XslDecimalFormat decimalFormat)
		{
			Parse (pattern, decimalFormat);
		}

		private void Parse (string pattern, XslDecimalFormat format)
		{
			if (pattern.Length == 0)
				throw new ArgumentException ("Invalid number format pattern string.");

			positivePattern = new DecimalFormatPattern ();
			negativePattern = positivePattern;

			int pos = positivePattern.ParsePattern (0, pattern, format);
			if (pos < pattern.Length) {
				if (pattern [pos] != format.PatternSeparator)
					// Expecting caught and wrapped by caller,
					// since it cannot provide XPathNavigator.
//					throw new ArgumentException ("Invalid number format pattern string.");
					return;
				pos++;
				negativePattern = new DecimalFormatPattern ();
				pos = negativePattern.ParsePattern (pos, pattern, format);
				if (pos < pattern.Length)
					throw new ArgumentException ("Number format pattern string ends with extraneous part.");
			}
		}

		public string FormatNumber (double number)
		{
			if (number >= 0)
				return positivePattern.FormatNumber (number);
			else
				return negativePattern.FormatNumber (number);
		}
	}

	internal class DecimalFormatPattern
	{
		public string Prefix = String.Empty;
		public string Suffix = String.Empty;
		public string NumberPart;
		NumberFormatInfo info;

		StringBuilder builder = new StringBuilder ();

		internal int ParsePattern (int start, string pattern, XslDecimalFormat format)
		{
			if (start == 0) // positive pattern
				this.info = format.Info;
			else {
				this.info = format.Info.Clone () as NumberFormatInfo;
				info.NegativeSign = String.Empty; // should be specified in Prefix
			}

			// prefix
			int pos = start;
			while (pos < pattern.Length) {
				if (pattern [pos] == format.ZeroDigit || pattern [pos] == format.Digit || pattern [pos] == format.Info.CurrencySymbol [0])
					break;
				else
					pos++;
			}

			Prefix = pattern.Substring (start, pos - start);
			if (pos == pattern.Length) {
				// Invalid number pattern.
//				throw new ArgumentException ("Invalid number format pattern."); 
				return pos;
			}

			// number
			pos = ParseNumber (pos, pattern, format);
			int suffixStart = pos;

			// suffix
			while (pos < pattern.Length) {
				if (pattern [pos] == format.ZeroDigit || pattern [pos] == format.Digit || pattern [pos] == format.PatternSeparator || pattern [pos] == format.Info.CurrencySymbol [0])
					break;
				else
					pos++;
			}

			Suffix = pattern.Substring (suffixStart, pos - suffixStart);

			return pos;
		}

		// FIXME: Collect grouping digits
		private int ParseNumber (int start, string pattern, XslDecimalFormat format)
		{
			int pos = start;
			// process non-minint part.
			for (; pos < pattern.Length; pos++) {
				if (pattern [pos] == format.Digit)
					builder.Append ('#');
				else if (pattern [pos] == format.Info.NumberGroupSeparator [0])
					builder.Append (',');
				else
					break;
			}
			
			// minint part.
			for (; pos < pattern.Length; pos++) {
				if (pattern [pos] == format.ZeroDigit)
					builder.Append ('0');
				else if (pattern [pos] == format.Info.NumberGroupSeparator [0])
					builder.Append (',');
				else
					break;
			}

			// optional fraction part
			if (pos < pattern.Length) {
				if (pattern [pos] == format.Info.NumberDecimalSeparator [0]) {
					builder.Append ('.');
					pos++;
				}
				while (pos < pattern.Length) {
					if (pattern [pos] == format.ZeroDigit) {
						pos++;
						builder.Append ('0');
					}
					else
						break;
				}
				while (pos < pattern.Length) {
					if (pattern [pos] == format.Digit) {
						pos++;
						builder.Append ('#');
					}
					else
						break;
				}
			}
			// optional exponent part
			if (pos + 1 < pattern.Length && pattern [pos] == 'E' && pattern [pos + 1] == format.ZeroDigit) {
				pos += 2;
				builder.Append ("E0");
				while (pos < pattern.Length) {
					if (pattern [pos] == format.ZeroDigit) {
						pos++;
						builder.Append ('0');
					}
					else
						break;
				}
			}
			// misc special characters
			if (pos < pattern.Length) {
				if (pattern [pos] == this.info.PercentSymbol [0])
					builder.Append ('%');
				else if (pattern [pos] == this.info.PerMilleSymbol [0])
					builder.Append ('\u2030');
				else if (pattern [pos] == this.info.CurrencySymbol [0])
					throw new ArgumentException ("Currency symbol is not supported for number format pattern string.");
				else
					pos--;
				pos++;
			}

			NumberPart = builder.ToString ();
			return pos;
		}

		public string FormatNumber (double number)
		{
			builder.Length = 0;
			builder.Append (Prefix);
			builder.Append (number.ToString (NumberPart, info));
			builder.Append (Suffix);
			return builder.ToString ();
		}
	}
}