//
// SimpleCollator.cs
//
// This class will demonstrate CompareInfo functionality that will just work.
//

using System;
using System.Collections;
using System.Globalization;

using Uni = Mono.Globalization.Unicode.MSCompatUnicodeTable;
using Util = Mono.Globalization.Unicode.MSCompatUnicodeTableUtil;

namespace Mono.Globalization.Unicode
{
	internal class SimpleCollator
	{
		SortKeyBuffer buf;
		// CompareOptions expanded.
		bool ignoreNonSpace; // used in IndexOf()
		bool ignoreSymbols;
		bool ignoreWidth;
		bool ignoreCase;
		bool ignoreKanaType;
		TextInfo textInfo; // for ToLower().
		bool frenchSort;
		readonly ushort [] cjkTable;
		readonly CodePointIndexer cjkIndexer;
		readonly byte [] cjkLv2Table;
		readonly CodePointIndexer cjkLv2Indexer;

		#region Tailoring supports
		// Possible mapping types are:
		//
		//	- string to string (ReplacementMap)
		//	- string to SortKey (SortKeyMap)
		//	- diacritical byte to byte (DiacriticalMap)
		//
		// There could be mapping from string to sortkeys, but
		// for now there is none as such.
		//
		internal class Contraction
		{
			public readonly char [] Source;
			// only either of them is used.
			public readonly string Replacement;
			public readonly byte [] SortKey;

			public Contraction (char [] source,
				string replacement, byte [] sortkey)
			{
				Source = source;
				Replacement = replacement;
				SortKey = sortkey;
			}
		}

		internal class ContractionComparer : IComparer
		{
			public static readonly ContractionComparer Instance =
				new ContractionComparer ();

			public int Compare (object o1, object o2)
			{
				Contraction c1 = (Contraction) o1;
				Contraction c2 = (Contraction) o2;
				char [] a1 = c1.Source;
				char [] a2 = c2.Source;
				int min = a1.Length > a2.Length ?
					a2.Length : a1.Length;
				for (int i = 0; i < min; i++)
					if (a1 [i] != a2 [i])
						return a1 [i] - a2 [i];
				return a1.Length - a2.Length;
			}
		}

		internal class Level2Map
		{
			public byte Source;
			public byte Replace;

			public Level2Map (byte source, byte replace)
			{
				Source = source;
				Replace = replace;
			}
		}

		internal class Level2MapComparer : IComparer
		{
			public static readonly Level2MapComparer Instance =
				new Level2MapComparer ();

			public int Compare (object o1, object o2)
			{
				Level2Map m1 = (Level2Map) o1;
				Level2Map m2 = (Level2Map) o2;
				return (m1.Source - m2.Source);
			}
		}

		readonly Contraction [] contractions;
		readonly Level2Map [] level2Maps;

		#endregion

		#region .ctor() and split functions

		public SimpleCollator (CultureInfo culture)
		{
			textInfo = culture.TextInfo;
			buf = new SortKeyBuffer (culture.LCID);

			SetCJKTable (culture, ref cjkTable, ref cjkIndexer,
				ref cjkLv2Table, ref cjkLv2Indexer);

			// Get tailoring info
			TailoringInfo t = null;
			for (CultureInfo ci = culture; ci.LCID != 127; ci = ci.Parent) {
				t = Uni.GetTailoringInfo (ci.LCID);
				if (t != null)
					break;
			}
			if (t == null) // then use invariant
				t = Uni.GetTailoringInfo (127);

			frenchSort = t.FrenchSort;
			BuildTailoringTables (culture, t, ref contractions,
				ref level2Maps);
/*
Console.WriteLine ("******** building table for {0} : c - {1} d - {2}",
culture.LCID, contractions.Length, level2Maps.Length);
foreach (Contraction c in contractions) {
foreach (char cc in c.Source)
Console.Write ("{0:X4} ", (int) cc);
Console.WriteLine (" -> {0}", c.Replacement);
}
*/
		}

		private void BuildTailoringTables (CultureInfo culture,
			TailoringInfo t,
			ref Contraction [] contractions,
			ref Level2Map [] diacriticals)
		{
			// collect tailoring entries.
			ArrayList cmaps = new ArrayList ();
			ArrayList dmaps = new ArrayList ();
			char [] tarr = Uni.TailoringValues;
			int idx = t.TailoringIndex;
			int end = idx + t.TailoringCount;
			while (idx < end) {
				int ss = idx + 1;
				char [] src = null;
				switch (tarr [idx]) {
				case '\x1': // SortKeyMap
					idx++;
					while (tarr [ss] != 0)
						ss++;
					src = new char [ss - idx];
					Array.Copy (tarr, idx, src, 0, ss - idx);
					byte [] sortkey = new byte [5];
					for (int i = 0; i < 5; i++)
						sortkey [i] = (byte) tarr [ss + 1 + i];
					cmaps.Add (new Contraction (
						src, null, sortkey));
					// it ends with 0
					idx = ss + 6;
					break;
				case '\x2': // DiacriticalMap
					dmaps.Add (new Level2Map (
						(byte) tarr [idx + 1],
						(byte) tarr [idx + 2]));
					idx += 3;
					break;
				case '\x3': // ReplacementMap
					idx++;
					while (tarr [ss] != 0)
						ss++;
					src = new char [ss - idx];
					Array.Copy (tarr, idx, src, 0, ss - idx);
					int l = ss + 1;
					while (tarr [l] != 0)
						l++;
					string r = new string (tarr, ss, l - ss);
					cmaps.Add (new Contraction (
						src, r, null));
					idx = l + 1;
					break;
				default:
					throw new NotImplementedException (String.Format ("Mono INTERNAL ERROR (Should not happen): Collation tailoring table is broken for culture {0} ({1}) at 0x{2:X}", culture.LCID, culture.Name, idx));
				}
			}
			cmaps.Sort (ContractionComparer.Instance);
			dmaps.Sort (Level2MapComparer.Instance);
			contractions = cmaps.ToArray (typeof (Contraction))
				as Contraction [];
			diacriticals = dmaps.ToArray (typeof (Level2Map))
				as Level2Map [];
		}

		private void SetCJKTable (CultureInfo culture,
			ref ushort [] cjkTable, ref CodePointIndexer cjkIndexer,
			ref byte [] cjkLv2Table, ref CodePointIndexer cjkLv2Indexer)
		{
			// custom CJK table support.
			switch (GetNeutralCulture (culture).Name) {
			case "zh-CHS":
				cjkTable = Uni.CjkCHS;
				cjkIndexer = Util.CjkCHS;
				break;
			case "zh-CHT":
				cjkTable = Uni.CjkCHT;
				cjkIndexer = Util.Cjk;
				break;
			case "ja":
				cjkTable = Uni.CjkJA;
				cjkIndexer = Util.Cjk;
				break;
			case "ko":
				cjkTable = Uni.CjkKO;
				cjkLv2Table = Uni.CjkKOLv2;
				cjkIndexer = Util.Cjk;
				cjkLv2Indexer = Util.Cjk;
				break;
			}
		}

		static CultureInfo GetNeutralCulture (CultureInfo info)
		{
			CultureInfo ret = info;
			while (ret.Parent != null && ret.Parent.LCID != 127)
				ret = ret.Parent;
			return ret;
		}

		#endregion

		byte Category (int cp)
		{
			if (cp < 0x3000 || cjkTable == null)
				return Uni.Categories (cp);
			ushort cjk = cjkTable [cjkIndexer.ToIndex (cp)];
			return cjk != 0 ? (byte) ((cjk & 0xFF00) >> 8) :
				Uni.Categories (cp);
		}

		byte Level1 (int cp)
		{
			if (cp < 0x3000 || cjkTable == null)
				return Uni.Level1 (cp);
			ushort cjk = cjkTable [cjkIndexer.ToIndex (cp)];
			return cjk != 0 ? (byte) (cjk & 0xFF) : Uni.Level1 (cp);
		}

		byte Level2 (int cp)
		{
			if (cp < 0x3000 || cjkLv2Table == null)
				return Uni.Level2 (cp);
			byte ret = cjkLv2Table [cjkLv2Indexer.ToIndex (cp)];
			if (ret != 0)
				return ret;
			ret = Uni.Level2 (cp);
			if (level2Maps.Length == 0)
				return ret;
			for (int i = 0; i < level2Maps.Length; i++) {
				if (level2Maps [i].Source == ret)
					return level2Maps [i].Replace;
				else if (level2Maps [i].Source > ret)
					break;
			}
			return ret;
		}

		void SetOptions (CompareOptions options)
		{
			this.ignoreNonSpace = (options & CompareOptions.IgnoreNonSpace) != 0;
			this.ignoreSymbols = (options & CompareOptions.IgnoreSymbols) != 0;
			this.ignoreWidth = (options & CompareOptions.IgnoreWidth) != 0;
			this.ignoreCase = (options & CompareOptions.IgnoreCase) != 0;
			this.ignoreKanaType = (options & CompareOptions.IgnoreKanaType) != 0;
		}

		// FIXME: It should not be used, since it disregards both
		// sortkey maps and replacement map from two or more chars.
		string GetExpansion (int i)
		{
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
					Category (i),
					Level1 (i),
					Level2 (i),
					Uni.Level3 (i),
					Uni.IsJapaneseSmallLetter ((char) i),
					Uni.GetJapaneseDashType ((char) i),
					!Uni.IsHiragana ((char) i),
					Uni.IsHalfWidthKana ((char) i)
					);
			else
				buf.AppendNormal (
					Category (i),
					Level1 (i),
					Level2 (i),
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

				if (Category (ci) != Category (cj) ||
					Level1 (ci) != Level1 (cj) ||
					!ignoreNonSpace && Level2 (ci) != Level2 (cj) ||
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
			if (length == min) {
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
			return IsSuffix (src, target, src.Length - 1, src.Length, opt);
		}

		public bool IsSuffix (string s, string target, int start, int length, CompareOptions opt)
		{
			SetOptions (opt);

			int min = length > target.Length ? target.Length : length;
			int si = start;

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

				if (Category (ci) != Category (cj) ||
					Level1 (ci) != Level1 (cj) ||
					!ignoreNonSpace && Level2 (ci) != Level2 (cj) ||
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
			int end = start + length;
			for (int idx = start; idx < end; idx++) {
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
				if (Category (si) != Category (ti) ||
					Level1 (si) != Level1 (ti) ||
					!ignoreNonSpace && Level2 (si) != Level2 (ti) ||
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
			return LastIndexOf (s, target, s.Length - 1, s.Length, CompareOptions.None);
		}

		public int LastIndexOf (string s, char target, CompareOptions opt)
		{
			return LastIndexOf (s, target, s.Length - 1, s.Length, opt);
		}

		public int LastIndexOf (string s, char target, int start, int length, CompareOptions opt)
		{
			// If target has an expansion, then use string search.
			string expansion = GetExpansion (target);
			if (expansion != null)
				return LastIndexOf (s, expansion, start, length, opt);

			SetOptions (opt);

			int end = start - length;

			int ti = FilterOptions ((int) target);
			for (int idx = start; idx > end; idx--) {
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
				if (Category (si) != Category (ti) ||
					Level1 (si) != Level1 (ti) ||
					!ignoreNonSpace && Level2 (si) != Level2 (ti) ||
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
				return idx;
			}
			return -1;
		}

		public int LastIndexOf (string s, string target, CompareOptions opt)
		{
			return LastIndexOf (s, target, s.Length - 1, s.Length, opt);
		}

		public int LastIndexOf (string s, string target, int start, int length, CompareOptions opt)
		{
			SetOptions (opt);

			int orgStart = start;

			do {
				// FIXME: this should be modified to handle
				// expansions
				int idx = LastIndexOf (s, target [0], start, length, opt);
				if (idx < 0)
					return -1;
				if (IsPrefix (s, target, idx, orgStart - idx + 1, opt))
					return idx;
				length--;
				start--;
			} while (length > 0);
			return -1;
		}

		#endregion
	}
}
