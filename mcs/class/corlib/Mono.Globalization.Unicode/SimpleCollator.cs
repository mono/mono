//
// SimpleCollator.cs
//
// This class will demonstrate CompareInfo functionality that will just work.
//

using System;
using System.Globalization;

using Uni = Mono.Globalization.Unicode.MSCompatUnicodeTable;

namespace Mono.Globalization.Unicode
{
	internal class SimpleCollator
	{
		SortKeyBuffer buf = new SortKeyBuffer ();
		// CompareOptions expanded.
		bool ignoreWidth;
		bool ignoreCase;
		bool ignoreKanaType;
		TextInfo textInfo; // for ToLower().

		#region GetSortKey()

		public SimpleCollator (CultureInfo culture)
		{
			textInfo = culture.TextInfo;
		}

		public SortKey GetSortKey (string s)
		{
			return GetSortKey (s, CompareOptions.None);
		}

		public SortKey GetSortKey (string s, CompareOptions options)
		{
			return GetSortKey (s, 0, s.Length, options);
		}

		void SetOptions (CompareOptions options)
		{
			this.ignoreWidth = (options & CompareOptions.IgnoreWidth) != 0;
			this.ignoreCase = (options & CompareOptions.IgnoreCase) != 0;
			this.ignoreKanaType = (options & CompareOptions.IgnoreKanaType) != 0;
		}

		int FilterOptions (int i)
		{
			if (ignoreWidth)
				i = Uni.ToWidthCompat (i);
			if (ignoreCase)
				i = textInfo.ToLower ((char) i);
			if (ignoreKanaType)
				i = Uni.ToKanaTypeInsensitive (i);
			return i;
		}

		SortKey GetSortKey (string s, int start, int length, CompareOptions options)
		{
			SetOptions (options);

			int end = start + length;
			buf.Initialize (options, s);
			for (int n = start; n < end; n++) {
				int i = s [n];
				if (Uni.IsIgnorable (i))
					continue;
				i = FilterOptions (i);

				string expansion = Uni.GetExpansion ((char) i);
				if (expansion != null) {
					foreach (char e in expansion)
						FillSortKeyRaw (e);
				}
				else
					FillSortKeyRaw (i);
			}
			return buf.GetResultAndReset ();
		}

		void FillSortKeyRaw (int i)
		{
			if (0x3400 <= i && i <= 0x4DB5) {
				FillCJKExtensionSortKeyRaw (i);
				return;
			}

			UnicodeCategory uc = char.GetUnicodeCategory ((char) i);
			switch (uc) {
			case UnicodeCategory.PrivateUse:
				FillPrivateUseSortKeyRaw (i);
				return;
			case UnicodeCategory.Surrogate:
				FillSurrogateSortKeyRaw (i);
				return;
			}

			if (Uni.HasSpecialWeight ((char) i))
				buf.AppendKana (
					Uni.Categories [i],
					Uni.Level1 [i],
					Uni.Level2 [i],
					Uni.Level3 [i],
					Uni.IsJapaneseSmallLetter ((char) i),
					Uni.GetJapaneseDashType ((char) i),
					!Uni.IsHiragana ((char) i),
					!ignoreWidth && Uni.IsHalfWidthKana ((char) i)
					);
			else
				buf.AppendNormal (
					Uni.Categories [i],
					Uni.Level1 [i],
					Uni.Level2 [i],
					Uni.Level3 [i]);
		}

		void FillCJKExtensionSortKeyRaw (int i)
		{
			int diff = i - 0x3400;

			buf.AppendCJKExtension (
				(byte) (0x10 + diff / 254),
				(byte) (diff % 254 + 2));
		}

		void FillPrivateUseSortKeyRaw (int i)
		{
			int diff = i - 0xE000;
			buf.AppendNormal (
				(byte) (0xE5 + diff / 254),
				(byte) (diff % 254 + 2),
				0,
				0);
		}

		void FillSurrogateSortKeyRaw (int i)
		{
			int diffbase = 0;
			int segment = 0;
			byte lower = 0;

			if (i < 0xD840) {
				diffbase = 0xD800;
				segment = 0x41;
				lower = (byte) ((i == 0xD800) ? 0x3E : 0x3F);
			} else if (0xD840 <= i && i < 0xD880) {
				diffbase = 0xD840;
				segment = 0xF2;
				lower = 0x3E;
			} else if (0xDB80 <= i && i < 0xDC00) {
				diffbase = 0xDB80 - 0x40;
				segment = 0xFE;
				lower = 0x3E;
			} else {
				diffbase = 0xDC00 - 0xF8 + 2;
				segment = 0x41;
				lower = 0x3F;
			}
			int diff = i - diffbase;

			buf.AppendNormal (
				(byte) (segment + diff / 254),
				(byte) (diff % 254 + 2),
				lower,
				lower);
		}

		#endregion

		#region Compare()

		public int Compare (string s1, string s2)
		{
			return Compare (s1, s2, CompareOptions.None);
		}

		public int Compare (string s1, string s2, CompareOptions options)
		{
			return Compare (s1, 0, s1.Length, s2, 0, s2.Length, options);
		}

		public int Compare (string s1, int idx1, int len1,
			string s2, int idx2, int len2, CompareOptions options)
		{
			SortKey sk1 = GetSortKey (s1, idx1, len1, options);
			SortKey sk2 = GetSortKey (s2, idx2, len2, options);
			byte [] d1 = sk1.KeyData;
			byte [] d2 = sk2.KeyData;
			int len = d1.Length > d2.Length ? d2.Length : d1.Length;
			for (int i = 0; i < len; i++)
				if (d1 [i] != d2 [i])
					return d1 [i] < d2 [i] ? -1 : 1;
			return d1.Length == d2.Length ? 0 : d1.Length < d2.Length ? -1 : 1;
		}

		#endregion
	}
}
