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
