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
using System.Text;
using Mono.Xml.XPath;

namespace Mono.Xml.Xsl.Operations {
	public class XslNumber : XslCompiledElement {
		
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
			
			// This may result in NodeSet.
//			if (value != null && value.ReturnType != XPathResultType.Number && value.ReturnType != XPathResultType.Any)
//				throw new Exception ("The expression for attribute 'value' must return a number");
			
			format = c.ParseAvtAttribute ("format");
			lang = c.ParseAvtAttribute ("lang");
			letterValue = c.ParseAvtAttribute ("letter-value");
			groupingSeparator = c.ParseAvtAttribute ("grouping-separator");
			groupingSize = c.ParseAvtAttribute ("grouping-size");
		}

		public override void Evaluate (XslTransformProcessor p)
		{
			string formatted = GetFormat (p);
			if (formatted != String.Empty)
				p.Out.WriteString (formatted);
		}
		
		XslNumberFormatter GetNumberFormatter (XslTransformProcessor p)
		{
			string format = "1";
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
				int hit = NumberSingle (p);
				return nf.Format (hit, hit != 0);
			case XslNumberingLevel.Multiple:
				return nf.Format (NumberMultiple (p));
			case XslNumberingLevel.Any:
				hit = NumberAny (p);
				return nf.Format (hit, hit != 0);
			default:
				throw new Exception ("Should not get here");
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
			foreach (int num in nums)
				ret [--pos] = num;
			
			return ret;
		}

		int NumberAny (XslTransformProcessor p)
		{
			int i = 0;
			XPathNavigator n = p.CurrentNode.Clone ();
			do {
				do {
					if (MatchesCount (n, p))
						i++;
					if (MatchesFrom (n, p) && n.IsDescendant (p.CurrentNode))
						return i;
				} while (n.MoveToPrevious ());
			} while (n.MoveToParent ());
			return 0;
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
			string firstSep = "", lastSep = "";
			ArrayList fmtList = new ArrayList ();
			FormatItem defaultFormat;
			
			public XslNumberFormatter (string format, string lang, string letterValue, char groupingSeparator, int groupingSize)
			{
				// We dont do any i18n now, so we ignore lang and letterValue.
				if (format == null || format == "")
					fmtList.Add (FormatItem.GetItem (null, "1", groupingSeparator, groupingSize));
				else {
					NumberFormatterScanner s = new NumberFormatterScanner (format);
					
					string sep, itm;
					
					sep = s.Advance (false);
					itm = s.Advance (true);
					
					if (itm == null) {
						lastSep = sep;
						fmtList.Add (FormatItem.GetItem (null, "1", groupingSeparator, groupingSize));
					} else {
						firstSep = sep;
						sep = null;
					
						while (itm != null) {
							fmtList.Add (FormatItem.GetItem (sep, itm, groupingSeparator, groupingSize));
							sep = s.Advance (false);
							itm = s.Advance (true);
							if (defaultFormat == null)
								defaultFormat = FormatItem.GetItem (sep, "1", groupingSeparator, groupingSize);
						}
						
						lastSep = sep;
					}
				}
			}
			
			// return the format for a single value, ie, if using Single or Any
			public string Format (int value)
			{
				return Format (value, true);
			}

			public string Format (int value, bool formatContent)
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
				
				int i = 0;
				foreach (int v in values) {
					FormatItem itm = (FormatItem)fmtList [i];
					if (i > 0) b.Append (itm.sep);
					itm.Format (b, v);
					
					if (++i == fmtList.Count)
						i--;
//					++i;
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
				
				public abstract void Format (StringBuilder b, int num);
					
				public static FormatItem GetItem (string sep, string item, char gpSep, int gpSize)
				{
					switch (item [0])
					{
						default: // See XSLT 1.0 spec 7.7.1.
						case '0': case '1':
							return new DigitItem (sep, item.Length, gpSep, gpSize);
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
				
				public override void Format (StringBuilder b, int num)
				{
					alphaSeq (b, num, uc ? ucl : lcl);
				}
				
				static void alphaSeq (StringBuilder b, int n, char [] alphabet) {
					if (n > alphabet.Length)
						alphaSeq (b, (n-1) / alphabet.Length, alphabet);
					b.Append (alphabet [(n-1) % alphabet.Length]); 
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
				
				public override void Format (StringBuilder b, int num)
				{
					if (num < 1 || num > 4999) {
						b.Append (num);
						return;
					}
					
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
				System.Globalization.NumberFormatInfo nfi;
				int decimalSectionLength;
				string format;
				
				public DigitItem (string sep, int len, char gpSep, int gpSize) : base (sep)
				{
					nfi = new System.Globalization.NumberFormatInfo  ();
					nfi.NumberDecimalDigits = 0;
					nfi.NumberGroupSizes = new int [] {gpSize};
					nfi.NumberGroupSeparator = gpSep.ToString ();

					/*
					FIXME: This washes other format specifications away ;-(
					decimalSectionLength = len;
					StringBuilder sb = new StringBuilder ();
					if (len > 0) {
						sb.Append ("D");
						sb.Append ('0');
						sb.Append (len);
						format = sb.ToString ();
					}
					*/
				}
				
				public override void Format (StringBuilder b, int num)
				{
//					b.Append (num.ToString (format, nfi));
					b.Append (num.ToString ("N", nfi));
				}
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
