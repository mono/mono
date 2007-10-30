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
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Text;
using Mono.Xml.XPath;

namespace Mono.Xml.Xsl.Operations {
	internal class XslNumber : XslCompiledElement {
		
		// <xsl:number
		//   level = "single" | "multiple" | "any"
		XslNumberingLevel level;
		//   count = pattern
		Pattern count;
		//   from = pattern
		Pattern from;
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

		// This behaves differently from Math.Round. For n + 0.5,
		// Math.Round() truncates, while XSLT expects ceiling.
		public static double Round (double n)
		{
			double f = System.Math.Floor (n);
			return (n - f >= 0.5) ? f + 1.0 : f;
		}

		protected override void Compile (Compiler c)
		{
			if (c.Debugger != null)
				c.Debugger.DebugCompile (this.DebugInput);

			c.CheckExtraAttributes ("number", "level", "count", "from", "value", "format", "lang", "letter-value", "grouping-separator", "grouping-size");

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
			
			count = c.CompilePattern (c.GetAttribute ("count"), c.Input);
			from = c.CompilePattern (c.GetAttribute ("from"), c.Input);
			value = c.CompileExpression (c.GetAttribute ("value"));
			
			format = c.ParseAvtAttribute ("format");
			lang = c.ParseAvtAttribute ("lang");
			letterValue = c.ParseAvtAttribute ("letter-value");
			groupingSeparator = c.ParseAvtAttribute ("grouping-separator");
			groupingSize = c.ParseAvtAttribute ("grouping-size");
		}

		public override void Evaluate (XslTransformProcessor p)
		{
			if (p.Debugger != null)
				p.Debugger.DebugExecute (p, this.DebugInput);

			string formatted = GetFormat (p);
			if (formatted != String.Empty)
				p.Out.WriteString (formatted);
		}
		
		XslNumberFormatter GetNumberFormatter (XslTransformProcessor p)
		{
			string formatStr = "1";
			string lang = null;
			string letterValue = null;
			char groupingSeparatorChar = '\0';
			decimal groupingSize = 0;
			
			if (this.format != null)
				formatStr = this.format.Evaluate (p);
			
			if (this.lang != null)
				lang = this.lang.Evaluate (p);
			
			if (this.letterValue != null)
				letterValue = this.letterValue.Evaluate (p);
			
			if (groupingSeparator != null)
				groupingSeparatorChar = this.groupingSeparator.Evaluate (p) [0];
			
			if (this.groupingSize != null)
				groupingSize = decimal.Parse (this.groupingSize.Evaluate (p), CultureInfo.InvariantCulture);
			
			//FIXME: Negative test compliency: .NET throws exception on negative grouping-size
			if (groupingSize > Int32.MaxValue || groupingSize < 1)
				groupingSize = 0;

			return new XslNumberFormatter (formatStr, lang, letterValue, groupingSeparatorChar, (int)groupingSize);
		}
		
		string GetFormat (XslTransformProcessor p)
		{
			XslNumberFormatter nf = GetNumberFormatter (p);
			
			if (this.value != null) {
				double result = p.EvaluateNumber (this.value);
				//Do we need to round the result here???
				//result = (int) ((result - (int) result >= 0.5) ? result + 1 : result); 
				return nf.Format (result);
			}
			
			switch (this.level) {
			case XslNumberingLevel.Single:
				int hit = NumberSingle (p);
				return nf.Format (hit, hit != 0);
			case XslNumberingLevel.Multiple:
				return nf.Format (NumberMultiple (p));
			case XslNumberingLevel.Any:
				hit = NumberAny (p);
				return nf.Format (hit, hit != 0);
			default:
				throw new XsltException ("Should not get here", null, p.CurrentNode);
			}
		}
		
		int [] NumberMultiple (XslTransformProcessor p)
		{
			ArrayList nums = new ArrayList ();
			XPathNavigator n = p.CurrentNode.Clone ();
			
			bool foundFrom = false;
			
			do {
				if (MatchesFrom (n, p)) {
					foundFrom = true;
					break;
				}
				
				if (MatchesCount (n, p)) {
					int i = 1;
					while (n.MoveToPrevious ()) {
						if (MatchesCount (n, p)) i++;
					}
					nums.Add (i);
				}
			} while (n.MoveToParent ());
			
			if (!foundFrom) return new int [0];
				
			int [] ret = new int [nums.Count];
			int pos = nums.Count;
			for (int i = 0; i < nums.Count; i++)
				ret [--pos] = (int) nums [i];
			
			return ret;
		}

		int NumberAny (XslTransformProcessor p)
		{
			int i = 0;
			XPathNavigator n = p.CurrentNode.Clone ();
			n.MoveToRoot ();
			bool countable = (from == null);
			do {
				if (from != null && MatchesFrom (n, p)) {
					countable = true;
					i = 0;
				}
				// Here this *else* is important
				else if (countable && MatchesCount (n, p))
					i++;
				if (n.IsSamePosition (p.CurrentNode))
					return i;

				if (!n.MoveToFirstChild ()) {
					while (!n.MoveToNext ()) {
						if (!n.MoveToParent ()) // returned to Root
							return 0;
					};
				}
			} while (true);
		}

		int NumberSingle (XslTransformProcessor p)
		{
			XPathNavigator n = p.CurrentNode.Clone ();
		
			while (!MatchesCount (n, p)) {
				if (from != null && MatchesFrom (n, p))
					return 0;
				
				if (!n.MoveToParent ())
					return 0;
			}
			
			if (from != null) {
				XPathNavigator tmp = n.Clone ();
				if (MatchesFrom (tmp, p))
					// Was not desc of closest matches from
					return 0;
				
				bool found = false;
				while (tmp.MoveToParent ())
					if (MatchesFrom (tmp, p)) {
						found = true; break;
					}
				if (!found)
					// not desc of matches from
					return 0;
			}
			
			int i = 1;
				
			while (n.MoveToPrevious ()) {
				if (MatchesCount (n, p)) i++;
			}
				
			return i;
		}
		
		bool MatchesCount (XPathNavigator item, XslTransformProcessor p)
		{
			if (count == null)
				return item.NodeType == p.CurrentNode.NodeType &&
					item.LocalName == p.CurrentNode.LocalName &&
					item.NamespaceURI == p.CurrentNode.NamespaceURI;
			else
				return p.Matches (count, item);
		}
		
		bool MatchesFrom (XPathNavigator item, XslTransformProcessor p)
		{
			if (from == null)
				return item.NodeType == XPathNodeType.Root;
			else
				return p.Matches (from, item);
		}
		
		class XslNumberFormatter {
			string firstSep, lastSep;
			ArrayList fmtList = new ArrayList ();
			
			public XslNumberFormatter (string format, string lang, string letterValue, char groupingSeparator, int groupingSize)
			{
				// We dont do any i18n now, so we ignore lang and letterValue.
				if (format == null || format == "")
					fmtList.Add (FormatItem.GetItem (null, "1", groupingSeparator, groupingSize));
				else {
					NumberFormatterScanner s = new NumberFormatterScanner (format);
					
					string itm;
					string sep = ".";
					
					firstSep = s.Advance (false);
					itm = s.Advance (true);
					
					if (itm == null) { // Only separator is specified
						sep = firstSep;
						firstSep = null;
						fmtList.Add (FormatItem.GetItem (sep, "1", groupingSeparator, groupingSize));
					} else {
						// The first format item.
						fmtList.Add (FormatItem.GetItem (".", itm, groupingSeparator, groupingSize));
						do {
							sep = s.Advance (false);
							itm = s.Advance (true);
							if (itm == null) {
								lastSep = sep;
								break;
							}
							fmtList.Add (FormatItem.GetItem (sep, itm, groupingSeparator, groupingSize));
						} while (itm != null);
					}
				}
			}
			
			// return the format for a single value, ie, if using Single or Any
			public string Format (double value)
			{
				return Format (value, true);
			}

			public string Format (double value, bool formatContent)
			{
				StringBuilder b = new StringBuilder ();
				if (firstSep != null) b.Append (firstSep);
				if (formatContent)
					((FormatItem)fmtList [0]).Format (b, value);
				if (lastSep != null) b.Append (lastSep);
				return b.ToString ();
			}
			
			// format for an array of numbers.
			public string Format (int [] values)
			{
				StringBuilder b = new StringBuilder ();
				if (firstSep != null) b.Append (firstSep);
				
				int formatIndex = 0;
				int formatMax  = fmtList.Count - 1;
				if (values.Length > 0) {
					if (fmtList.Count > 0) {
						FormatItem itm = (FormatItem)fmtList [formatIndex];
						itm.Format (b, values [0]);
					}
					if (formatIndex < formatMax)
						formatIndex++;
				}
				for (int i = 1; i < values.Length; i++) {
					FormatItem itm = (FormatItem)fmtList [formatIndex];
					b.Append (itm.sep);
					int v = values [i];
					itm.Format (b, v);
					if (formatIndex < formatMax)
						formatIndex++;
				}
				
				if (lastSep != null) b.Append (lastSep);
				
				return b.ToString ();
			}
			
			class NumberFormatterScanner {
				int pos = 0, len;
				string fmt;
				
				public NumberFormatterScanner (string fmt) {
					this.fmt = fmt;
					len = fmt.Length;
				}
				
				public string Advance (bool alphaNum)
				{
					int start = pos;
					while ((pos < len) && (char.IsLetterOrDigit (fmt, pos) == alphaNum))
						pos++;
					
					if (pos == start)
						return null;
					else
						return fmt.Substring (start, pos - start);
				}
			}
			
			abstract class FormatItem {
				public readonly string sep;
				public FormatItem (string sep)
				{
					this.sep = sep;
				}
				
				public abstract void Format (StringBuilder b, double num);
					
				public static FormatItem GetItem (string sep, string item, char gpSep, int gpSize)
				{
					switch (item [0])
					{
						default: // See XSLT 1.0 spec 7.7.1.
							return new DigitItem (sep, 1, gpSep, gpSize);
						case '0': case '1':
							int len = 1;
							for (; len < item.Length; len++)
								if (!Char.IsDigit (item, len))
									break;
							return new DigitItem (sep, len, gpSep, gpSize);
						case 'a':
							return new AlphaItem (sep, false);
						case 'A':
							return new AlphaItem (sep, true);
						case 'i':
							return new RomanItem (sep, false);
						case 'I':
							return new RomanItem (sep, true);
					}
				}
			}
			
			class AlphaItem : FormatItem {
				bool uc;
				static readonly char [] ucl = {'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'};
				static readonly char [] lcl = {'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'};
				
				public AlphaItem (string sep, bool uc) : base (sep)
				{
					this.uc = uc;
				}
				
				public override void Format (StringBuilder b, double num)
				{
					alphaSeq (b, num, uc ? ucl : lcl);
				}
				
				static void alphaSeq (StringBuilder b, double n, char [] alphabet) {
					n = XslNumber.Round (n);
					if (n == 0)
						return;
					if (n > alphabet.Length)
						alphaSeq (b, System.Math.Floor ((n - 1) / alphabet.Length), alphabet);
					b.Append (alphabet [((int) n - 1) % alphabet.Length]); 
				}
			}
			
			class RomanItem : FormatItem {
				bool uc;
				public RomanItem (string sep, bool uc) : base (sep)
				{
					this.uc = uc;
				}
				static readonly string [] ucrDigits =
				{ "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
				static readonly string [] lcrDigits =
				{ "m", "cm", "d", "cd", "c", "xc", "l", "xl", "x", "ix", "v", "iv", "i" };
				static readonly int [] decValues =
				{1000, 900 , 500, 400 , 100, 90  , 50 , 40  , 10 , 9   , 5  , 4   , 1   };
				
				public override void Format (StringBuilder b, double num)
				{
					if (num < 1 || num > 4999) {
						b.Append (num);
						return;
					}
					num = XslNumber.Round (num);
					for (int i = 0; i < decValues.Length; i++) {
						while (decValues [i] <= num) {
							if (uc)
								b.Append (ucrDigits [i]);
							else
								b.Append (lcrDigits [i]);
							
							num -= decValues [i];
						}
						if (num == 0) break;
					}
				}
			}
			
			class DigitItem : FormatItem {
				NumberFormatInfo nfi;
				int decimalSectionLength;
				StringBuilder numberBuilder;
				
				public DigitItem (string sep, int len, char gpSep, int gpSize) : base (sep)
				{
					nfi = new NumberFormatInfo  ();
					nfi.NumberDecimalDigits = 0;
					nfi.NumberGroupSizes = new int [] {0};
					if (gpSep != '\0' && gpSize > 0) {
						// ignored if either of them doesn't exist.
						nfi.NumberGroupSeparator = gpSep.ToString ();
						nfi.NumberGroupSizes = new int [] {gpSize};
					}
					decimalSectionLength = len;
				}
				
				public override void Format (StringBuilder b, double num)
				{
					string number = num.ToString ("N", nfi);
					int len = decimalSectionLength;
					if (len > 1) {
						if (numberBuilder == null)
							numberBuilder = new StringBuilder ();
						for (int i = len; i > number.Length; i--)
							numberBuilder.Append ('0');
						numberBuilder.Append (number.Length <= len ? number : number.Substring (number.Length - len, len));
						number = numberBuilder.ToString ();
						numberBuilder.Length = 0;
					}
					b.Append (number);
				}
			}
		}
	}
	
	internal enum XslNumberingLevel
	{
		Single,
		Multiple,
		Any
	}
}
