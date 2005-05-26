//
//
// There are two kind of sort keys : which are computed and which are laid out
// as an indexed array. Computed sort keys are:
//
//	- CJK, which largely vary depending on LCID (namely kr,jp,zh-CHS,zh-TW)
//	- Surrogate
//	- PrivateUse
//
// Also, for composite characters it should prepare different index table.
//
// Except for them, it should use precomputed index array.
//

//
// * sortkey getter signature
//
//	int GetSortKey (string s, int index, byte [] buf)
//	Stores sort key for corresponding character element into buf and
//	returns the length of the consumed _source_ character element in s.
//
// * character length to consume; default implementation
//
//	If there is a diacritic after the base character, they are consumed
//	and they are considered as a part of the character element.
//

using System;
using System.Collections;
using System.Globalization;

namespace Mono.Globalization.Unicode
{
	internal class MSCompatSortKeyTableGenerator
	{
		public static void Main ()
		{
			new MSCompatSortKeyTableGenerator ().Run ();
		}

		byte [] fillIndex = new byte [255]; // by category
		CharMapEntry [] map = new CharMapEntry [char.MaxValue + 1];

		char [] specialIgnore = new char [] {
			'\u3099', '\u309A', '\u309B', '\u309C', '\u0BCD',
			'\u0E47', '\u0E4C', '\uFF9E', '\uFF9F'
			};

		// FIXME: need more love (as always)
		char [] alphabets = new char [] {'A', 'B', 'C', 'D', 'E', 'F',
			'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q',
			'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
			'\u0292', '\u01BE', '\u0298'};
		byte [] alphaWeights = new byte [] {2, 9, 0xA, 0x1A, 0x21,
			0x23, 0x25, 0x2C, 0x32, 0x35, 0x36, 0x48, 0x51, 0x70,
			0x7C, 0x7E, 0x89, 0x8A, 0x91, 0x99, 0x9F, 0xA2, 0xA4,
			0xA6, 0xA9, 0xAA, 0xB3, 0xB4};


		public void Run ()
		{
			UnicodeCategory uc;

			#region Specially ignored // 01
			// This will raise "Defined" flag up.
			foreach (char c in specialIgnore)
				map [(int) c] = new CharMapEntry (0, 0, 0);
			#endregion


			#region Variable weights
			// Controls : 06 03 - 06 3D
			fillIndex [6] = 3;
			for (int i = 0; i < 65536; i++) {
				char c = (char) i;
				uc = Char.GetUnicodeCategory (c);
				if (uc == UnicodeCategory.Control &&
					!Char.IsWhiteSpace (c))
					AddCharMap (c, 6, true);
			}

			// Apostrophe 06 80
			map ['\''] = new CharMapEntry (6, 80, 1);
			map ['\uFF63'] = new CharMapEntry (6, 80, 1); // full

			// Hyphen/Dash : 06 81 - 06 90
			fillIndex [6] = 0x81;
			for (int i = 0; i < 65536; i++) {
				if (Char.GetUnicodeCategory ((char) i)
					== UnicodeCategory.DashPunctuation)
					AddCharMapGroup ((char) i, 6, true, true);
			}

			// Arabic variable weight chars 06 A0 -
			fillIndex [6] = 0xA0;
			// vowels
			for (int i = 0x64B; i <= 0x650; i++)
				AddCharMapGroup ((char) i, 6, true, true);
			// sukun
			AddCharMapGroup ('\u0652', 6, false, true);
			// shadda
			AddCharMapGroup ('\u0651', 6, false, true);
			#endregion


			#region Nonspacing marks // 01
			// FIXME: 01 03 - 01 B6 ... annoyance :(

			// Combining diacritical marks: 01 DC -

			// LAMESPEC: It should not stop at '\u20E1'. There are
			// a few more characters (that however results in 
			// overflow of level 2 unless we start before 0xDD).
			fillIndex [1] = 0xDC;
			for (int i = 0x20d0; i <= 0x20e1; i++)
				AddCharMap ((char) i, 1, true);
			#endregion


			#region Whitespaces // 07 03 -
			fillIndex [7] = 0x3;
			AddCharMapGroup (' ', 7, false, true);
			AddCharMap ('\u00A0', 7, true);
			for (int i = 9; i <= 0xD; i++)
				AddCharMap ((char) i, 7, true);
			for (int i = 0x2000; i <= 0x200B; i++)
				AddCharMap ((char) i, 7, true);
			AddCharMapGroup ('\u2028', 7, false, true);
			AddCharMapGroup ('\u2029', 7, false, true);

			// LAMESPEC: Windows developers seem to have thought 
			// that those characters are kind of whitespaces,
			// while they aren't.
			AddCharMapGroup ('\u2422', 7, false, true); // blank symbol
			AddCharMapGroup ('\u2423', 7, false, true); // open box
			#endregion


			#region ASCII non-alphanumeric // 07
			// non-alphanumeric ASCII except for: + - < = > '
			for (int i = 0x21; i < 0x7F; i++) {
				if (Char.IsLetterOrDigit ((char) i)
					|| "+-<=>'".IndexOf ((char) i) >= 0)
					continue; // they are not added here.
				AddCharMapGroup ((char) i, 7, false, true);
			}
			#endregion


			// FIXME: for 07 xx we need more love.


			#region Numbers // 0C 02 - 0C E1
			fillIndex [9] = 2;

			// 9F8 : Bengali "one less than the denominator"
			AddCharMap ('\u09F8', 9, true);

			ArrayList numbers = new ArrayList ();
			for (int i = 0; i < 65536; i++)
				if (Char.IsNumber ((char) i))
					numbers.Add (i);

			ArrayList numberValues = new ArrayList ();
			foreach (int i in numbers)
				numberValues.Add (new DictionaryEntry (i, CharUnicodeInfo.GetDecimalValue ((char) i)));
			numberValues.Sort (DictionaryValueComparer.Instance);
			decimal prevValue = -1;
			foreach (DictionaryEntry de in numberValues) {
				decimal currValue = (decimal) de.Value;
				if (prevValue < currValue) {
					prevValue = currValue;
					fillIndex [9] += 1;
				}
				AddCharMap ((char) ((int) de.Key), 9, false);
			}

			// 221E: infinity
			fillIndex [9] = 0xFF;
			AddCharMap ('\u221E', 9, true);
			#endregion


			#region Latin alphabets
			for (int i = 0; i < alphabets.Length; i++) {
				AddAlphaMap (alphabets [i], 0xE, alphaWeights [i]);
			}
			#endregion

			#region Letters

			// Greek and Coptic
			fillIndex [0xF] = 02;
			for (int i = 0x0380; i < 0x03CF; i++)
				if (Char.IsLetter ((char) i))
					AddLetterMap ((char) i, 0xF, true);
			fillIndex [0xF] = 0x40;
			for (int i = 0x03D0; i < 0x0400; i++)
				if (Char.IsLetter ((char) i))
					AddLetterMap ((char) i, 0xF, true);

			// Cyrillic - UCA order w/ some modification
			fillIndex [0x10] = 0x3;
			// FIXME: For \u0400-\u045F we need "ordered Cyrillic"
			// table which is moslty from UCA DUCET.
			for (int i = 0; i < orderedCyrillic.Length; i++) {
				char c = orderedCyrillic [i];
				if (Char.IsLetter (c)) {
					AddLetterMap (c, 0x10, false);
					fillIndex [0x10] += 3;
				}
			}
			for (int i = 0x0460; i < 0x0481; i++) {
				if (Char.IsLetter ((char) i)) {
					AddLetterMap ((char) i, 0x10, false);
					fillIndex [0x10] += 3;
				}
			}

			// Armenian
			fillIndex [0x11] = 0x3;
			for (int i = 0x0531; i < 0x0586; i++)
				if (Char.IsLetter ((char) i))
					AddLetterMap ((char) i, 0x11, true);

			// Hebrew
			fillIndex [0x12] = 0x3;
			for (int i = 0x05D0; i < 0x05FF; i++)
				if (Char.IsLetter ((char) i))
					AddLetterMap ((char) i, 0x12, true);

			// Arabic
			fillIndex [0x13] = 0x3;
			/*
			FIXME: I still need more love on presentation form B
			*/
			fillIndex [0x13] = 0x84;
			for (int i = 0x0674; i < 0x06D6; i++)
				if (Char.IsLetter ((char) i))
					AddLetterMap ((char) i, 0x13, true);

			// Devanagari
			for (int i = 0x0901; i < 0x0905; i++) {
				if (Char.IsLetter ((char) i)) {
					AddLetterMap ((char) i, 0x14, false);
					fillIndex [0x14] += 2;
				}
			}
			for (int i = 0x0905; i < 0x093A; i++) {
				if (Char.IsLetter ((char) i)) {
					AddLetterMap ((char) i, 0x14, false);
					fillIndex [0x14] += 4;
				}
			}
			for (int i = 0x093E; i < 0x094F; i++) {
				if (Char.IsLetter ((char) i)) {
					AddLetterMap ((char) i, 0x14, false);
					fillIndex [0x14] += 2;
				}
			}

			// Bengali
			fillIndex [0x15] = 02;
			for (int i = 0x0980; i < 0x9FF; i++) {
				if (i == 0x09E0)
					fillIndex [0x15] = 0x3B;
				switch (Char.GetUnicodeCategory ((char) i)) {
				case NonSpacingMark:
				case DecimalDigitNumber:
				case OtherNumber:
					continue;
				}
				AddLetterMap ((char) i, 0x15, true);
			}

			// Gurmukhi
			fillIndex [0x16] = 02;
			// FIXME: orderedGurmukhi needed from UCA
			for (int i = 0; i < orderedGurmukhi.Length; i++) {
				char c = orderedGurmukhi [i];
				if (c == '\u0A3C' || c == '\u0A4D' ||
					'\u0A66' <= c && c <= '\u0A71')
					continue;
				AddLetterMap (c, 0x16, false);
				fillIndex [0x16] += 4;
			}

			// Gujarati
			fillIndex [0x17] = 02;
			// FIXME: orderedGujarati needed from UCA
			for (int i = 0; i < orderedGujarati.Length; i++) {
				char c = orderedGujarati [i];
				AddLetterMap (c, 0x17, false);
				fillIndex [0x17] += 4;
			}

			// Oriya
			fillIndex [0x18] = 02;
			for (int i = 0x0B00; i < 0x0B7F; i++) {
				switch (Char.GetUnicodeCategory ((char) i)) {
				case NonSpacingMark:
				case DecimalDigitNumber:
					continue;
				}
				AddLetterMap ((char) i, 0x18, true);
			}

			// Tamil
			fillIndex [0x19] = 2;
			AddCharMap ('\u0BD7', 0x19, false);
			fillIndex [0x19] = 0xA;
			// vowels
			for (int i = 0x0BD7; i < 0x0B94; i++) {
				if (Char.IsLetter ((char) i) {
					AddCharMap ((char) i, 0x19, false);
					fillIndex [0x19] += 2;
				}
			}
			// special vowel
			fillIndex [0x19] = 0x24;
			AddCharMap ('\u0B94', 0x19, false);
			fillIndex [0x19] = 0x26;
			// FIXME: we need to have constant array for Tamil
			// consonants. Windows have almost similar sequence
			// to TAM from tamilnet but a bit different in Grantha
			for (int i = 0; i < orderedTamil.Length; i++) {
				char c = orderedGujarati [i];
				AddLetterMap (c, 0x19, false);
				fillIndex [0x19] += 4;
			}

			// Telugu
			fillIndex [0x1A] = 0x4;
			for (int i = 0x0C00; i < 0x0C62; i++) {
				if (i == 0x0C55 || i == 0x0C56)
					continue; // skip
				AddCharMap ((char) i, 0x1A, false);
				fillIndex [0x1A] += 3;
				char supp = (i == 0x0C0B) ? '\u0C60':
					i == 0x0C0C ? '\u0C61' : char.MinValue;
				if (supp == char.MinValue)
					continue;
				AddCharMap (supp, 0x1A, false);
				fillIndex [0x1A] += 3;
			}

			// Kannada
			fillIndex [0x1B] = 4;
			for (int i = 0x0C80; i < 0x0CE5; i++) {
				if (i == 0x0CD5 || i == 0x0CD6)
					continue; // ignore
				AddCharMap ((char) i, 0x1B, false);
				fillIndex [0x1B] += 3;
			}
			
			// Malayalam
			fillIndex [0x1C] = 2;
			for (int i = 0x0D02; i < 0x0D61; i++)
				if (!IsIgnorable ((char) i))
					AddCharMap ((char) i, 0x1C, true);

			// Thai ... note that it breaks 0x1E wall after E2B!
			// Also, all Thai characters have level 2 value 3.
			fillIndex [0x1E] = 2;
			for (int i = 0xE44; i < 0xE48; i++)
				AddThaiCharMap ((char) i, 0x1E, true);
			for (int i = 0xE01; i < 0xE2B; i++) {
				AddThaiCharMap ((char) i, 0x1E, false);
				fillIndex [0x1E] += 6;
			}
			fillIndex [0x1F] = 5;
			for (int i = 0xE2B; i < 0xE30; i++) {
				AddThaiCharMap ((char) i, 0x1F, false);
				fillIndex [0x1F] += 6;
			}
			for (int i = 0xE30; i < 0xE3B; i++)
				AddThaiCharMap ((char) i, 0x1F, true);
			// some Thai characters remains.
			char [] specialThai = new char [] {'\u0E45', '\u0E46',
				'\u0E4E', '\u0E4F', '\u0E5A', '\u0E5B'};
			foreach (char c in specialThai)
				AddThaiCharMap (c, 0x1F, true);

			// Lao
			fillIndex [0x1F] = 2;
			for (int i = 0xE80; i < 0xEDF; i++)
				if (Char.IsLetter ((char) i))
					AddCharMap ((char) i, 0x1F, true);

			// Georgian
			// FIXME: we need an array in UCA order.
			fillIndex [0x21] = 5;
			for (int i = 0; i < orderedGeorgian.Length; i++) {
				char c = orderedGeorgian [i];
				AddLetterMap (c, 0x21, false);
				fillIndex [0x21] += 5;
			}

			#endregion
		}

		private void AddAlphaMap (char c, byte category, byte alphaWeight)
		{
			throw new NotImplementedException ();
		}

		class DictionaryValueComparer : IComparer
		{
			public static readonly DictionaryValueComparer Instance
				= new DictionaryValueComparer ();

			private DictionaryValueComparer ()
			{
			}

			public /*static*/ int Compare (object o1, object o2)
			{
				DictionaryEntry e1 = (DictionaryEntry) o1;
				DictionaryEntry e2 = (DictionaryEntry) o2;
				// FIXME: in case of 0, compare decomposition categories
				return Decimal.Compare ((decimal) e1.Value, (decimal) e2.Value);
			}
		}

		private void AddCharMapGroup (char c, byte category, bool tail, bool updateIndexForSelf)
		{
			// <small> update index
			char c2 = tail ?
				MSCompatGenerated.ToSmallFormTail (c) :
				MSCompatGenerated.ToSmallForm (c);
			if (c2 > char.MinValue)
				AddCharMap (c2, category, true);
			// itself
			AddCharMap (c, category, updateIndexForSelf);
			// <full>
			c2 = tail ?
				MSCompatGenerated.ToFullWidthTail (c) :
				MSCompatGenerated.ToFullWidth (c);
			if (c2 > char.MinValue)
				AddCharMapGroup (c2, category, tail, false);
		}

		private void AddCharMap (char c, byte category, bool increment)
		{
			map [(int) c] = new CharMapEntry (category,
				category == 1 ? (byte) 1 : fillIndex [category],
				category != 1 ? fillIndex [category] : (byte) 1);
			if (increment)
				fillIndex [category] += 1;
		}

		#region Level 3 properties (Case/Width)

		public static byte GetLevel3WeightRaw (char c) // add 2 for sortkey value
		{
			// Korean
			if ('\u1100' <= c && c <= '\u11F9)
				return 2;
			if ('\uFFA0' <= c && c <= '\uFFDC)
				return 4;
			if ('\u3130' <= c && c <= '\u3164)
				return 5;
			// numbers
			if ('\u2776' <= c && c <= '\u277F')
				return 4;
			if ('\u2780' <= c && c <= '\u2789')
				return 8;
			if ('\u2776' <= c && c <= '\u2793')
				return 0xC;
			if ('\u2160' <= c && c <= '\u216F')
				return 0x18;
			if ('\u2181' <= c && c <= '\u2182')
				return 0x18;
			// Arabic
			if ('\u2135' <= c && c <= '\u2138')
				return 4;
			if ('\uFE80' <= c && c <= '\uFE8E')
				return MSCompatGenerated.GetArabicFormInPresentationB (c);

			// actually I dunno the reason why they have weights.
			switch (c) {
			case '\u01BC':
				return 0x10;
			case '\u06A9':
				return 0x20;
			case '\u06AA':
				return 0x28;
			}

			byte ret = 0;
			switch (c) {
			case '\u03C2':
			case '\u2104':
			case '\u212B':
				ret |= 8;
				break;
			case '\uFE42':
				ret |= 0xC;
				break;
			}

			// misc
			switch (MSCompatGenerated.GetNormalizationType (c)) {
			case 1: // <full>
				ret |= 1;
				break;
			case 2: // <sub>
				ret |= 2;
				break;
			case 3: // <super>
				ret |= 0xE;
				break;
			}
			if (MSCompatGenerated.IsSmallCapital (c)) // grep "SMALL CAPITAL"
				ret |= 8;
			if (MSCompatGenerated.IsUppercase (c)) // DerivedCoreProperties
				ret |= 0x10;

			return ret;
		}

		// TODO: implement GetArabicFormInRepresentationD(),
		// GetNormalizationType(), IsSmallCapital() and IsUppercase().
		// (They can be easily to be generated.)

		#endregion

	}

	internal struct CharMapEntry
	{
		public readonly byte Category;
		public readonly byte Level1;
		public readonly byte Level2; // It is always single byte.
		public readonly bool Defined;

		public CharMapEntry (byte category, byte level1, byte level2)
		{
			Category = category;
			Level1 = level1;
			Level2 = level2;
			Defined = true;
		}
	}
}
