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
		bool ignoreNonSpace; // used in IndexOf()
		bool ignoreSymbols;
		bool ignoreWidth;
		bool ignoreCase;
		bool ignoreKanaType;
		TextInfo textInfo; // for ToLower().
		bool frenchSort;

		public SimpleCollator (CultureInfo culture)
		{
			textInfo = culture.TextInfo;
			// FIXME: fill frenchSort from CultureInfo.
		}

		void SetOptions (CompareOptions options)
		{
			this.ignoreNonSpace = (options & CompareOptions.IgnoreNonSpace) != 0;
			this.ignoreSymbols = (options & CompareOptions.IgnoreSymbols) != 0;
			this.ignoreWidth = (options & CompareOptions.IgnoreWidth) != 0;
			this.ignoreCase = (options & CompareOptions.IgnoreCase) != 0;
			this.ignoreKanaType = (options & CompareOptions.IgnoreKanaType) != 0;
		}

		string GetExpansion (int i)
		{
			// FIXME: handle tailorings
			return Uni.GetExpansion ((char) i);
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

		#region GetSortKey()

		public SortKey GetSortKey (string s)
		{
			return GetSortKey (s, CompareOptions.None);
		}

		public SortKey GetSortKey (string s, CompareOptions options)
		{
			return GetSortKey (s, 0, s.Length, options);
		}

		public SortKey GetSortKey (string s, int start, int length, CompareOptions options)
		{
			SetOptions (options);

			int end = start + length;
			buf.Initialize (options, s, frenchSort);
			for (int n = start; n < end; n++) {
				int i = s [n];
				if (IsIgnorable (i))
					continue;
				i = FilterOptions (i);

				string expansion = GetExpansion (i);
				if (expansion != null) {
					foreach (char e in expansion)
						FillSortKeyRaw (e);
				}
				else
					FillSortKeyRaw (i);
			}
			return buf.GetResultAndReset ();
		}

		bool IsIgnorable (int i)
		{
			return Uni.IsIgnorable (i) ||
				ignoreSymbols && Uni.IsIgnorableSymbol (i);
		}

		void FillSortKeyRaw (int i)
		{
			if (0x3400 <= i && i <= 0x4DB5) {
				int diff = i - 0x3400;
				buf.AppendCJKExtension (
					(byte) (0x10 + diff / 254),
					(byte) (diff % 254 + 2));
				return;
			}

			UnicodeCategory uc = char.GetUnicodeCategory ((char) i);
			switch (uc) {
			case UnicodeCategory.PrivateUse:
				int diff = i - 0xE000;
				buf.AppendNormal (
					(byte) (0xE5 + diff / 254),
					(byte) (diff % 254 + 2),
					0,
					0);
				return;
			case UnicodeCategory.Surrogate:
				FillSurrogateSortKeyRaw (i);
				return;
			}

			if (Uni.HasSpecialWeight ((char) i))
				buf.AppendKana (
					Uni.Categories (i),
					Uni.Level1 (i),
					Uni.Level2 (i),
					Uni.Level3 (i),
					Uni.IsJapaneseSmallLetter ((char) i),
					Uni.GetJapaneseDashType ((char) i),
					!Uni.IsHiragana ((char) i),
					Uni.IsHalfWidthKana ((char) i)
					);
			else
				buf.AppendNormal (
					Uni.Categories (i),
					Uni.Level1 (i),
					Uni.Level2 (i),
					Uni.Level3 (i));
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

		#region IsPrefix()

		public bool IsPrefix (string src, string target, CompareOptions opt)
		{
			return IsPrefix (src, target, 0, src.Length, opt);
		}

		public bool IsPrefix (string s, string target, int start, int length, CompareOptions opt)
		{
			SetOptions (opt);

			int min = length > target.Length ? target.Length : length;
			int si = start;

			// FIXME: this is not enough to handle tailorings.
			for (int j = 0; j < min; j++, si++) {
				int ci = FilterOptions (s [si]);
				int cj = FilterOptions (target [j]);
				if (ci == cj)
					continue;
				if (IsIgnorable (s [si])) {
					if (!IsIgnorable (target [j]))
						j--;
					continue;
				}
				else if (IsIgnorable (target [j])) {
					si--;
					continue;
				}

				// FIXME: should handle expansions (and it 
				// should be before codepoint comparison).
				string expansion = GetExpansion (s [si]);
				if (expansion != null)
					return false;
				expansion = GetExpansion (target [j]);
				if (expansion != null)
					return false;

				if (Uni.Categories (ci) != Uni.Categories (cj) ||
					Uni.Level1 (ci) != Uni.Level1 (cj) ||
					!ignoreNonSpace && Uni.Level2 (ci) != Uni.Level2 (cj) ||
					Uni.Level3 (ci) != Uni.Level3 (cj))
					return false;
				if (!Uni.HasSpecialWeight ((char) ci))
					continue;
				if (Uni.IsJapaneseSmallLetter ((char) ci) !=
					Uni.IsJapaneseSmallLetter ((char) cj) ||
					Uni.GetJapaneseDashType ((char) ci) !=
					Uni.GetJapaneseDashType ((char) cj) ||
					!Uni.IsHiragana ((char) ci) !=
					!Uni.IsHiragana ((char) cj) ||
					Uni.IsHalfWidthKana ((char) ci) !=
					Uni.IsHalfWidthKana ((char) cj))
					return false;
			}
			if (si == min) {
				// All codepoints in the compared range
				// matches. In that case, what matters 
				// is whether the remaining part of 
				// "target" is ignorable or not.
				for (int i = min; i < target.Length; i++)
					if (!IsIgnorable (target [i]))
						return false;
				return true;
			}
			return true;
		}

		#endregion

		#region IsSuffix()

		public bool IsSuffix (string src, string target, CompareOptions opt)
		{
			return IsSuffix (src, target, 0, src.Length, opt);
		}

		// It is mostly copy of IsPrefix().
		public bool IsSuffix (string s, string target, int start, int length, CompareOptions opt)
		{
			SetOptions (opt);

			int min = length > target.Length ? target.Length : length;
			int si = start + length - 1;

			// FIXME: this is not enough to handle tailorings.
			for (int j = min - 1; j >= 0; j--, si--) {
				int ci = FilterOptions (s [si]);
				int cj = FilterOptions (target [j]);
				if (ci == cj)
					continue;
				if (IsIgnorable (s [si])) {
					if (!IsIgnorable (target [j]))
						j++;
					continue;
				}
				else if (IsIgnorable (target [j])) {
					si++;
					continue;
				}

				// FIXME: should handle expansions (and it 
				// should be before codepoint comparison).
				string expansion = GetExpansion (s [si]);
				if (expansion != null)
					return false;
				expansion = GetExpansion (target [j]);
				if (expansion != null)
					return false;

				if (Uni.Categories (ci) != Uni.Categories (cj) ||
					Uni.Level1 (ci) != Uni.Level1 (cj) ||
					!ignoreNonSpace && Uni.Level2 (ci) != Uni.Level2 (cj) ||
					Uni.Level3 (ci) != Uni.Level3 (cj))
					return false;
				if (!Uni.HasSpecialWeight ((char) ci))
					continue;
				if (Uni.IsJapaneseSmallLetter ((char) ci) !=
					Uni.IsJapaneseSmallLetter ((char) cj) ||
					Uni.GetJapaneseDashType ((char) ci) !=
					Uni.GetJapaneseDashType ((char) cj) ||
					!Uni.IsHiragana ((char) ci) !=
					!Uni.IsHiragana ((char) cj) ||
					Uni.IsHalfWidthKana ((char) ci) !=
					Uni.IsHalfWidthKana ((char) cj))
					return false;
			}
			if (si == min) {
				// All codepoints in the compared range
				// matches. In that case, what matters 
				// is whether the remaining part of 
				// "target" is ignorable or not.
				for (int i = target.Length - min - 1; i >= 0; i--)
					if (!IsIgnorable (target [i]))
						return false;
				return true;
			}
			return true;
		}

		#endregion

		#region IndexOf()

		public int IndexOf (string s, char target)
		{
			return IndexOf (s, target, 0, s.Length, CompareOptions.None);
		}

		public int IndexOf (string s, char target, CompareOptions opt)
		{
			return IndexOf (s, target, 0, s.Length, opt);
		}

		public int IndexOf (string s, char target, int start, int length, CompareOptions opt)
		{
			// If target has an expansion, then use string search.
			string expansion = GetExpansion (target);
			if (expansion != null)
				return IndexOf (s, expansion, start, length, opt);

			SetOptions (opt);

			int ti = FilterOptions ((int) target);
			for (int idx = start; idx < length; idx++) {
				switch (char.GetUnicodeCategory (s [idx])) {
				case UnicodeCategory.PrivateUse:
				case UnicodeCategory.Surrogate:
					if (s [idx] != target)
						continue;
					return idx;
				}

				expansion = GetExpansion (s [idx]);
				if (expansion != null)
					continue; // since target cannot be expansion as conditioned above.
				if (s [idx] == target)
					return idx;
				int si = FilterOptions ((int) s [idx]);
				if (Uni.Categories (si) != Uni.Categories (ti) ||
					Uni.Level1 (si) != Uni.Level1 (ti) ||
					!ignoreNonSpace && Uni.Level2 (si) != Uni.Level2 (ti) ||
					Uni.Level3 (si) != Uni.Level3 (ti))
					continue;
				if (!Uni.HasSpecialWeight ((char) si))
					return idx;
				if (Uni.IsJapaneseSmallLetter ((char) si) !=
					Uni.IsJapaneseSmallLetter ((char) ti) ||
					Uni.GetJapaneseDashType ((char) si) !=
					Uni.GetJapaneseDashType ((char) ti) ||
					!Uni.IsHiragana ((char) si) !=
					!Uni.IsHiragana ((char) ti) ||
					Uni.IsHalfWidthKana ((char) si) !=
					Uni.IsHalfWidthKana ((char) ti))
					continue;
			}
			return -1;
		}

		public int IndexOf (string s, string target, CompareOptions opt)
		{
			return IndexOf (s, target, 0, s.Length, opt);
		}

		public int IndexOf (string s, string target, int start, int length, CompareOptions opt)
		{
			SetOptions (opt);
			do {
				// FIXME: this should be modified to handle
				// expansions
				int idx = IndexOf (s, target [0], start, length, opt);
				if (idx < 0)
					return -1;
				if (IsPrefix (s, target, idx, length - (idx - start), opt))
					return idx;
				start++;
				length--;
			} while (length > 0);
			return -1;
		}

		#endregion

		#region LastIndexOf()

		public int LastIndexOf (string s, char target)
		{
			return LastIndexOf (s, target, 0, s.Length, CompareOptions.None);
		}

		public int LastIndexOf (string s, char target, CompareOptions opt)
		{
			return LastIndexOf (s, target, 0, s.Length, opt);
		}

		public int LastIndexOf (string s, char target, int start, int length, CompareOptions opt)
		{
			// If target has an expansion, then use string search.
			string expansion = GetExpansion (target);
			if (expansion != null)
				return LastIndexOf (s, expansion, start, length, opt);

			SetOptions (opt);

			int ti = FilterOptions ((int) target);
			for (int idx = start + length - 1; idx >= start; idx--) {
				switch (char.GetUnicodeCategory (s [idx])) {
				case UnicodeCategory.PrivateUse:
				case UnicodeCategory.Surrogate:
					if (s [idx] != target)
						continue;
					return idx;
				}

				expansion = GetExpansion (s [idx]);
				if (expansion != null)
					continue; // since target cannot be expansion as conditioned above.
				if (s [idx] == target)
					return idx;
				int si = FilterOptions ((int) s [idx]);
				if (Uni.Categories (si) != Uni.Categories (ti) ||
					Uni.Level1 (si) != Uni.Level1 (ti) ||
					!ignoreNonSpace && Uni.Level2 (si) != Uni.Level2 (ti) ||
					Uni.Level3 (si) != Uni.Level3 (ti))
					continue;
				if (!Uni.HasSpecialWeight ((char) si))
					return idx;
				if (Uni.IsJapaneseSmallLetter ((char) si) !=
					Uni.IsJapaneseSmallLetter ((char) ti) ||
					Uni.GetJapaneseDashType ((char) si) !=
					Uni.GetJapaneseDashType ((char) ti) ||
					!Uni.IsHiragana ((char) si) !=
					!Uni.IsHiragana ((char) ti) ||
					Uni.IsHalfWidthKana ((char) si) !=
					Uni.IsHalfWidthKana ((char) ti))
					continue;
			}
			return -1;
		}

		public int LastIndexOf (string s, string target, CompareOptions opt)
		{
			return LastIndexOf (s, target, 0, s.Length, opt);
		}

		public int LastIndexOf (string s, string target, int start, int length, CompareOptions opt)
		{
			SetOptions (opt);

			do {
				// FIXME: this should be modified to handle
				// expansions
				int idx = LastIndexOf (s, target [0], start, length, opt);
				if (idx < 0)
					return -1;

				if (IsPrefix (s, target, idx, length - (idx - start), opt))
					return idx;
				length = idx - start - 1;
			} while (length > 0);
			return -1;
		}

		#endregion
	}
}
