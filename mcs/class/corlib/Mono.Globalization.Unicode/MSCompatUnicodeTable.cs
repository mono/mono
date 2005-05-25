using System;
using System.Globalization;

namespace Mono.Globalization.Unicode
{
	internal class MSCompatUnicodeTable
	{
		#region IsIgnorable
		public static bool IsIgnorable (int i)
		{
			switch (i) {
			case 0:
			// No idea why they are ignored.
			case 0x2df: case 0x387:
			case 0x3d7: case 0x3d8: case 0x3d9:
			case 0x3f3: case 0x3f4: case 0x3f5: case 0x3f6:
			case 0x400: case 0x40d: case 0x450: case 0x45d:
			case 0x587: case 0x58a: case 0x5c4: case 0x640:
			case 0x653: case 0x654: case 0x655: case 0x66d:
			case 0xb56:
			case 0x1e9b: case 0x202f: case 0x20ad:
			case 0x20ae: case 0x20af:
			case 0x20e2: case 0x20e3:
			case 0x2139: case 0x213a: case 0x2183:
			case 0x2425: case 0x2426: case 0x2619:
			case 0x2670: case 0x2671: case 0x3007:
			case 0x3190: case 0x3191:
			case 0xfffc: case 0xfffd:
				return true;
			// exceptional characters filtered by the 
			// following conditions. Originally those exceptional
			// ranges are incorrect (they should not be ignored)
			// and most of those characters are unfortunately in
			// those ranges.
			case 0x4d8: case 0x4d9:
			case 0x4e8: case 0x4e9:
			case 0x70f:
			case 0x3036: case 0x303f:
			case 0x337b: case 0xfb1e:
				return false;
			}

			if (
				// The whole Sinhala characters.
				0x0D82 <= i && i <= 0x0DF4
				// The whole Tibetan characters.
				|| 0x0F00 <= i && i <= 0x0FD1
				// The whole Myanmar characters.
				|| 0x1000 <= i && i <= 0x1059
				// The whole Etiopic, Cherokee, 
				// Canadian Syllablic, Ogham, Runic,
				// Tagalog, Hanunoo, Philippine,
				// Buhid, Tagbanwa, Khmer and Mongorian
				// characters.
				|| 0x1200 <= i && i <= 0x1DFF
				// Greek extension characters.
				|| 0x1F00 <= i && i <= 0x1FFF
				// The whole Braille characters.
				|| 0x2800 <= i && i <= 0x28FF
				// CJK radical characters.
				|| 0x2E80 <= i && i <= 0x2EF3
				// Kangxi radical characters.
				|| 0x2F00 <= i && i <= 0x2FD5
				// Ideographic description characters.
				|| 0x2FF0 <= i && i <= 0x2FFB
				// Bopomofo letter and final
				|| 0x31A0 <= i && i <= 0x31B7
				// White square with quadrant characters.
				|| 0x25F0 <= i && i <= 0x25F7
				// Ideographic telegraph symbols.
				|| 0x32C0 <= i && i <= 0x32CB
				|| 0x3358 <= i && i <= 0x3370
				|| 0x33E0 <= i && i <= 0x33FF
				// The whole YI characters.
				|| 0xA000 <= i && i <= 0xA48C
				|| 0xA490 <= i && i <= 0xA4C6
				// American small ligatures
				|| 0xFB13 <= i && i <= 0xFB17
				// hebrew, arabic, variation selector.
				|| 0xFB1D <= i && i <= 0xFE2F
				// Arabic ligatures.
				|| 0xFEF5 <= i && i <= 0xFEFC
				// FIXME: why are they excluded?
				|| 0x01F6 <= i && i <= 0x01F9
				|| 0x0218 <= i && i <= 0x0233
				|| 0x02A9 <= i && i <= 0x02AD
				|| 0x02EA <= i && i <= 0x02EE
				|| 0x0349 <= i && i <= 0x036F
				|| 0x0488 <= i && i <= 0x048F
				|| 0x04D0 <= i && i <= 0x04FF
				|| 0x0500 <= i && i <= 0x050F // actually it matters only for 2.0
				|| 0x06D6 <= i && i <= 0x06ED
				|| 0x06FA <= i && i <= 0x06FE
				|| 0x2048 <= i && i <= 0x204D
				|| 0x20e4 <= i && i <= 0x20ea
				|| 0x213C <= i && i <= 0x214B
				|| 0x21EB <= i && i <= 0x21FF
				|| 0x22F2 <= i && i <= 0x22FF
				|| 0x237B <= i && i <= 0x239A
				|| 0x239B <= i && i <= 0x23CF
				|| 0x24EB <= i && i <= 0x24FF
				|| 0x2596 <= i && i <= 0x259F
				|| 0x25F8 <= i && i <= 0x25FF
				|| 0x2672 <= i && i <= 0x2689
				|| 0x2768 <= i && i <= 0x2775
				|| 0x27d0 <= i && i <= 0x27ff
				|| 0x2900 <= i && i <= 0x2aff
				|| 0x3033 <= i && i <= 0x303F
				|| 0x31F0 <= i && i <= 0x31FF
				|| 0x3250 <= i && i <= 0x325F
				|| 0x32B1 <= i && i <= 0x32BF
				|| 0x3371 <= i && i <= 0x337B
				|| 0xFA30 <= i && i <= 0xFA6A
			)
				return true;

			UnicodeCategory uc = Char.GetUnicodeCategory ((char) i);
			switch (uc) {
			// ignored by nature
			case UnicodeCategory.PrivateUse:
			case UnicodeCategory.Surrogate:
				return false;
			case UnicodeCategory.Format:
			case UnicodeCategory.OtherNotAssigned:
				return true;
			default:
				return false;
			}
		}

		// To check IsIgnorable sanity, try the driver below under MS.NET.

		/*
		public static void Main ()
		{
			for (int i = 0; i <= char.MaxValue; i++)
				Dump (i, IsIgnorable (i));
		}

		static void Dump (int i, bool ignore)
		{
			switch (Char.GetUnicodeCategory ((char) i)) {
			case UnicodeCategory.PrivateUse:
			case UnicodeCategory.Surrogate:
				return; // check nothing
			}

			string s1 = "";
			string s2 = new string ((char) i, 10);
			int ret = CultureInfo.InvariantCulture.CompareInfo.Compare (s1, s2, CompareOptions.IgnoreCase);
			if ((ret == 0) == ignore)
				return;
			Console.WriteLine ("{0} : {1:x} {2}", ignore ? "o" : "x", i, Char.GetUnicodeCategory ((char) i));
		}
		*/
		#endregion // IsIgnorable

		#region IsIgnorableSymbol
		public static bool IsIgnorableSymbol (int i)
		{
			if (IsIgnorable (i))
				return true;

			switch (i) {
			// *Letter
			case 0x00b5: case 0x01C0: case 0x01C1:
			case 0x01C2: case 0x01C3: case 0x01F6:
			case 0x01F7: case 0x01F8: case 0x01F9:
			case 0x02D0: case 0x02EE: case 0x037A:
			case 0x03D7: case 0x03F3:
			case 0x0400: case 0x040d:
			case 0x0450: case 0x045d:
			case 0x048C: case 0x048D:
			case 0x048E: case 0x048F:
			case 0x0587: case 0x0640: case 0x06E5:
			case 0x06E6: case 0x06FA: case 0x06FB:
			case 0x06FC: case 0x093D: case 0x0950:
			case 0x1E9B: case 0x2139: case 0x3006:
			case 0x3033: case 0x3034: case 0x3035:
			case 0xFE7E: case 0xFE7F:
			// OtherNumber
			case 0x16EE: case 0x16EF: case 0x16F0:
			// LetterNumber
			case 0x2183: // ROMAN NUMERAL REVERSED ONE HUNDRED
			case 0x3007: // IDEOGRAPHIC NUMBER ZERO
			case 0x3038: // HANGZHOU NUMERAL TEN
			case 0x3039: // HANGZHOU NUMERAL TWENTY
			case 0x303a: // HANGZHOU NUMERAL THIRTY
			// OtherSymbol
			case 0x2117:
			case 0x327F:
				return true;
			// ModifierSymbol
			case 0x02B9: case 0x02BA: case 0x02C2:
			case 0x02C3: case 0x02C4: case 0x02C5:
			case 0x02C8: case 0x02CC: case 0x02CD:
			case 0x02CE: case 0x02CF: case 0x02D2:
			case 0x02D3: case 0x02D4: case 0x02D5:
			case 0x02D6: case 0x02D7: case 0x02DE:
			case 0x02E5: case 0x02E6: case 0x02E7:
			case 0x02E8: case 0x02E9:
			case 0x309B: case 0x309C:
			// OtherPunctuation
			case 0x055A: // American Apos
			case 0x05C0: // Hebrew Punct
			case 0x0E4F: // Thai FONGMAN
			case 0x0E5A: // Thai ANGKHANKHU
			case 0x0E5B: // Thai KHOMUT
			// CurencySymbol
			case 0x09F2: // Bengali Rupee Mark
			case 0x09F3: // Bengali Rupee Sign
			// MathSymbol
			case 0x221e: // INF.
			// OtherSymbol
			case 0x0482:
			case 0x09FA:
			case 0x0B70:
				return false;
			}

			// *Letter
			if (0xFE70 <= i && i < 0xFE7C // ARABIC LIGATURES B
#if NET_2_0
				|| 0x0501 <= i && i <= 0x0510 // CYRILLIC KOMI
				|| 0xFA30 <= i && i < 0xFA70 // CJK COMPAT
#endif
			)
				return true;

			UnicodeCategory uc = Char.GetUnicodeCategory ((char) i);
			switch (uc) {
			case UnicodeCategory.Surrogate:
				return false; // inconsistent

			case UnicodeCategory.SpacingCombiningMark:
			case UnicodeCategory.EnclosingMark:
			case UnicodeCategory.NonSpacingMark:
			case UnicodeCategory.PrivateUse:
				// NonSpacingMark
				if (0x064B <= i && i <= 0x0652) // Arabic
					return true;
				return false;

			case UnicodeCategory.Format:
			case UnicodeCategory.OtherNotAssigned:
				return true;

			default:
				bool use = false;
				// OtherSymbols
				if (
					// latin in a circle
					0x249A <= i && i <= 0x24E9
					|| 0x2100 <= i && i <= 0x2132
					// Japanese
					|| 0x3196 <= i && i <= 0x31A0
					// Korean
					|| 0x3200 <= i && i <= 0x321C
					// Chinese/Japanese
					|| 0x322A <= i && i <= 0x3243
					// CJK
					|| 0x3260 <= i && i <= 0x32B0
					|| 0x32D0 <= i && i <= 0x3357
					|| 0x337B <= i && i <= 0x33DD
				)
					use = !Char.IsLetterOrDigit ((char) i);
				if (use)
					return false;

				// This "Digit" rule is mystery.
				// It filters some symbols out.
				if (Char.IsLetterOrDigit ((char) i))
					return false;
				if (Char.IsNumber ((char) i))
					return false;
				if (Char.IsControl ((char) i)
					|| Char.IsSeparator ((char) i)
					|| Char.IsPunctuation ((char) i))
					return true;
				if (Char.IsSymbol ((char) i))
					return true;

				// FIXME: should check more
				return false;
			}
		}

		// To check IsIgnorableSymbol sanity, try the driver below under MS.NET.
/*
		public static void Main ()
		{
			CompareInfo ci = CultureInfo.InvariantCulture.CompareInfo;
			for (int i = 0; i <= char.MaxValue; i++) {
				UnicodeCategory uc = Char.GetUnicodeCategory ((char) i);
				if (uc == UnicodeCategory.Surrogate)
					continue;

				bool ret = IsIgnorableSymbol (i);

				string s1 = "TEST ";
				string s2 = "TEST " + (char) i;

				int result = ci.Compare (s1, s2, CompareOptions.IgnoreSymbols);

				if (ret != (result == 0))
					Console.WriteLine ("{0} : {1:x}[{2}]({3})",
						ret ? "should not ignore" :
							"should ignore",
						i,(char) i, uc);
			}
		}
*/
		#endregion

		#region NonSpacing
		public static bool IsIgnorableNonSpacing (int i)
		{
			if (Mono.Globalization.Unicode.MSCompatUnicodeTable.IsIgnorable (i))
				return true;

			switch (i) {
			case 0x02C8: case 0x02DE: case 0x0559: case 0x055A:
			case 0x05C0: case 0x0ABD: case 0x0CD5: case 0x0CD6:
			case 0x309B: case 0x309C: case 0xFF9E: case 0xFF9F:
				return true;
			case 0x02D0: case 0x0670: case 0x0901: case 0x0902:
			case 0x094D: case 0x0962: case 0x0963: case 0x0A41:
			case 0x0A42: case 0x0A47: case 0x0A48: case 0x0A4B:
			case 0x0A4C: case 0x0A81: case 0x0A82: case 0x0B82:
			case 0x0BC0: case 0x0CBF: case 0x0CC6: case 0x0CCC:
			case 0x0CCD: case 0x0E4E:
				return false;
			}

			if (0x02b9 <= i && i <= 0x02c5
				|| 0x02cc <= i && i <= 0x02d7
				|| 0x02e4 <= i && i <= 0x02ef
				|| 0x20DD <= i && i <= 0x20E0
			)
				return true;

			if (0x064B <= i && i <= 0x00652
				|| 0x0941 <= i && i <= 0x0948
				|| 0x0AC1 <= i && i <= 0x0ACD
				|| 0x0C3E <= i && i <= 0x0C4F
				|| 0x0E31 <= i && i <= 0x0E3F
			)
				return false;

			return Char.GetUnicodeCategory ((char) i) ==
				UnicodeCategory.NonSpacingMark;
		}

		// We can reuse IsIgnorableSymbol testcode 
		// for IsIgnorableNonSpacing.
		#endregion

		public static int ToKanatypeInsensitive (int i)
		{
			// Note that IgnoreKanaType does not treat half-width
			// katakana as equivalent to full-width ones.

			// Thus, it is so simple ;-)
			return (0x3041 <= i && i <= 0x3094) ? i + 0x60 : i;
		}

		public static int ToWidthInsensitive (int i)
		{
			return Normalization.ToWidthInsensitive (i);
		}

		#region Utilities

		public static void GetPrimaryWeight (char c, bool variable,
			out byte category, out byte value)
		{
		}

		public static string GetExpansion (char c)
		{
			switch (c) {
			case '\u00C6':
				return "AE";
			case '\u00DE':
				return "TH";
			case '\u00DF':
				return "ss";
			case '\u00E6':
				return "ae";
			case '\u00FE':
				return "th";
			case '\u0132':
				return "IJ";
			case '\u0133':
				return "ij";
			case '\u0152':
				return "OE";
			case '\u0153':
				return "oe";
			case '\u01C4':
				return "DZ\u030C"; // surprisingly Windows works fine here
			case '\u01C5':
				return "Dz\u030C";
			case '\u01C6':
				return "dz\u030C";
			case '\u01C7':
				return "LJ";
			case '\u01C8':
				return "Lj";
			case '\u01C9':
				return "lj";
			case '\u01CA':
				return "NJ";
			case '\u01CB':
				return "Nj";
			case '\u01CC':
				return "nj";
			case '\u01E2':
				return "A\u0304E\u0304"; // LAMESPEC: should be \u00C6\u0304
			case '\u01E3':
				return "a\u0304e\u0304"; // LAMESPEC: should be \u00E6\u0304
			case '\u01F1':
				return "DZ";
			case '\u01F2':
				return "Dz";
			case '\u01F3':
				return "dz";
			case '\u01FC':
				return "A\u0301E\u0301"; // LAMESPEC: should be \u00C6\u0301
			case '\u01FD':
				return "a\u0301e\u0301"; // LAMESPEC: should be \u00C6\u0301
			case '\u05F0':
				return "\u05D5\u05D5";
			case '\u05F1':
				return "\u05D5\u05D9";
			case '\u05F2':
				return "\u05D9\u05D9";
			case '\uFB00':
				return "ff";
			case '\uFB01':
				return "fi";
			case '\uFB02':
				return "fl";
			}
//			if ('\u1113' <= c && c <= '\u115F') Korean Jamo
//				return true;
			return null;
		}
		#endregion


		#region Level 4 properties (Kana)

		public static bool HasSpecialWeight (char c)
		{
			if (c < '\u3041')
				return false;
			else if (c < '\u3100')
				return true;
			else if (c < '\uFF60')
				return false;
			else if (c < '\uFF9F')
				return true;
			return true;
		}

		public static byte GetJapaneseDashType (char c)
		{
			switch (c) {
			case '\u309D':
			case '\u309E':
			case '\u30FD':
			case '\u30FE':
			case '\uFF70':
				return 4;
			case '\u30FC':
				return 5;
			}
			return 0;
		}

		public static bool IsHalfWidthKana (char c)
		{
			return '\uFF66' <= c && c <= '\uFF9D';
		}

		public static bool IsHiragana (char c)
		{
			return '\u3041' <= c && c <= '\u3094';
		}

		public static bool IsJapaneseSmallLetter (char c)
		{
			if ('\uFF67' <= c && c <= '\FF6F')
				return true;
			if ('\u3040' < c && c < '\u30FA') {
				switch (c) {
				case '\u3041':
				case '\u3043':
				case '\u3045':
				case '\u3047':
				case '\u3049':
				case '\u3083':
				case '\u3085':
				case '\u3087':
				case '\u308E':
				case '\u30A1':
				case '\u30A3':
				case '\u30A5':
				case '\u30A7':
				case '\u30A9':
				case '\u30E3':
				case '\u30E5':
				case '\u30E7':
				case '\u30EE':
					return true;
				}
			}
			return false;
		}

		#endregion


		// 0 means no primary weight. 6 means variable weight
		// For expanded character the value is 0.
		// Those arrays will be split into blocks (<3400 and >F800)
		byte [] categories;
		byte [] level1;
		byte [] level2;
		byte [] level3;
		// level 4 is computed.

		// public static bool HasSpecialWeight (char c)
		// { return level1 [(int) c] == 6; }

		//
		// Maybe autogenerated code or icall to fill array runs here
		//
	}
}


