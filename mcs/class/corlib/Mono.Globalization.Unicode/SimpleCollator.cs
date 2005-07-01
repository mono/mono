//
// SimpleCollator.cs
//
// This class will demonstrate CompareInfo functionality that will just work.
//

//
// Here's a summary for supporting contractions, expansions and diacritical 
// remappings.
//
// Diacritical mapping is a simple tailoring rule that "remaps" diacritical
// weight value from one to another. For now it applies to all range of
// characters, but at some stage we might need to limit the remapping ranges.
//
// A Contraction consists of a string (actually char[]) and a sortkey entry
// (i.e. byte[]). It indicates that a sequence of characters is interpreted
// as a single character that has the mapped sortkey value. There is no
// character which goes across "special rules". When the collator encountered
// such a sequence of characters, it just returns the sortkey value without
// further replacement.
//
// Since it is still likely to happen that a contraction sequence matches
// other character than the identical sequence (depending on CompareOptions
// and of course, defined sortkey value itself), comparison cannot be done
// at source char[] level.
//
// (to be continued.)
//

//
// In IndexOf(), the first character in the target string or the target char
// itself is turned into sortkey bytes. If the character has a contraction and
// that is sortkey map, then it is used instead. If the contraction exists and
// that is replacement map, then the first character of the replacement string
// is searched instead. IndexOf() always searches only for the top character,
// and if it returned negative value, then it returns -1. Otherwise, it then
// tries IsPrefix() from that location. If it returns true, then it returns
// the index.
//

// LAMESPEC: IndexOf() is lame as a whole API. It never matches in the middle
// of expansion and there is no proper way to return such indexes within
// a single int return value.
//
// For example, try below in .NET:
//	IndexOf("\u00E6", "a")
//	IndexOf("\u00E6", "e")
//


using System;
using System.Collections;
using System.Globalization;

using Uni = Mono.Globalization.Unicode.MSCompatUnicodeTable;
using UUtil = Mono.Globalization.Unicode.MSCompatUnicodeTableUtil;

namespace Mono.Globalization.Unicode
{
	internal class SimpleCollator
	{
		static SimpleCollator invariant =
			new SimpleCollator (CultureInfo.InvariantCulture);

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
		readonly int lcid;
		readonly Contraction [] contractions;
		readonly Level2Map [] level2Maps;

		// temporary sortkey buffer for index search/comparison
		byte [] charSortKey = new byte [4];
		// temporary expansion store for IsPrefix/Suffix
		int escapedSourceIndex;

		#region Tailoring support classes
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

		#endregion

		#region .ctor() and split functions

		public SimpleCollator (CultureInfo culture)
		{
			lcid = culture.LCID;
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
			// FIXME: Since tailorings are mostly for latin
			// (and in some cases Cyrillic) characters, it would
			// be much better for performance to store "start 
			// indexes" for > 370 (culture-specific letters).

/*
// dump tailoring table
Console.WriteLine ("******** building table for {0} : c - {1} d - {2}",
culture.LCID, contractions.Length, level2Maps.Length);
foreach (Contraction c in contractions) {
foreach (char cc in c.Source)
Console.Write ("{0:X4} ", (int) cc);
Console.WriteLine (" -> '{0}'", c.Replacement);
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
					byte [] sortkey = new byte [4];
					for (int i = 0; i < 4; i++)
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
					ss++;
					int l = ss;
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
				cjkIndexer = UUtil.CjkCHS;
				break;
			case "zh-CHT":
				cjkTable = Uni.CjkCHT;
				cjkIndexer = UUtil.Cjk;
				break;
			case "ja":
				cjkTable = Uni.CjkJA;
				cjkIndexer = UUtil.Cjk;
				break;
			case "ko":
				cjkTable = Uni.CjkKO;
				cjkLv2Table = Uni.CjkKOLv2;
				cjkIndexer = UUtil.Cjk;
				cjkLv2Indexer = UUtil.Cjk;
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

		Contraction GetContraction (string s, int start, int end)
		{
			Contraction c = GetContraction (s, start, end, contractions);
			if (c != null || lcid == 127)
				return c;
			return GetContraction (s, start, end, invariant.contractions);
		}

		Contraction GetContraction (string s, int start, int end, Contraction [] clist)
		{
			for (int i = 0; i < clist.Length; i++) {
				Contraction ct = clist [i];
				if (ct.Source [0] > s [start])
					return null; // it's already sorted
				char [] chars = ct.Source;
				if (end - start < chars.Length)
					continue;
				bool match = true;
				for (int n = 0; n < chars.Length; n++)
					if (s [start + n] != chars [n]) {
						match = false;
						break;
					}
				if (match)
					return ct;
			}
			return null;
		}

		Contraction GetTailContraction (string s, int start, int end)
		{
			Contraction c = GetTailContraction (s, start, end, contractions);
			if (c != null || lcid == 127)
				return c;
			return GetTailContraction (s, start, end, invariant.contractions);
		}

		Contraction GetTailContraction (string s, int start, int end, Contraction [] clist)
		{
			for (int i = 0; i < clist.Length; i++) {
				Contraction ct = clist [i];
				if (ct.Source [0] > s [end])
					return null; // it's already sorted
				char [] chars = ct.Source;
				if (start - end + 1 < chars.Length)
					continue;
				bool match = true;
				int offset = start - chars.Length + 1;
				for (int n = 0; n < chars.Length; n++)
					if (s [offset + n] != chars [n]) {
						match = false;
						break;
					}
				if (match)
					return ct;
			}
			return null;
		}

		Contraction GetContraction (char c)
		{
			Contraction ct = GetContraction (c, contractions);
			if (ct != null || lcid == 127)
				return ct;
			return GetContraction (c, invariant.contractions);
		}

		Contraction GetContraction (char c, Contraction [] clist)
		{
			for (int i = 0; i < clist.Length; i++) {
				Contraction ct = clist [i];
				if (ct.Source [0] > c)
					return null; // it's already sorted
				if (ct.Source [0] == c && ct.Source.Length == 1)
					return ct;
			}
			return null;
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

			buf.Initialize (options, s, frenchSort);
			int end = start + length;
			GetSortKey (s, start, end);
			return buf.GetResultAndReset ();
		}

		void GetSortKey (string s, int start, int end)
		{
			for (int n = start; n < end; n++) {
				int i = s [n];
				if (IsIgnorable (i))
					continue;
				i = FilterOptions (i);

				Contraction ct = GetContraction (s, n, end);
				if (ct != null) {
					if (ct.Replacement != null)
						GetSortKey (ct.Replacement, 0, ct.Replacement.Length);
					else {
						byte [] b = ct.SortKey;
						buf.AppendNormal (
							b [0],
							b [1],
							b [2] != 1 ? b [2] : Level2 (i),
							b [3] != 1 ? b [3] : Uni.Level3 (i));
					}
					n += ct.Source.Length - 1;
				}
				else
					FillSortKeyRaw (i);
			}
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

		#region IsPrefix() and IsSuffix()

		public bool IsPrefix (string src, string target, CompareOptions opt)
		{
			return IsPrefix (src, target, 0, src.Length, opt);
		}

		public bool IsPrefix (string s, string target, int start, int length, CompareOptions opt)
		{
			SetOptions (opt);
			return IsPrefix (s, target, start, length);
		}

		// returns the consumed length in positive number, or -1 if
		// target was not a prefix.
		bool IsPrefix (string s, string target, int start, int length)
		{
			// quick check : simple codepoint comparison
			if (s.Length >= target.Length) {
				int si = start;
				for (int i = 0; i < target.Length; i++, si++)
					if (s [si] != target [i])
						break;
				if (si == start + target.Length)
					return true;
			}

			escapedSourceIndex = -1;
			return IsPrefixInternal (s, target, start, length);
		}

		bool IsPrefixInternal (string s, string target, int start, int length)
		{
			int si = start;
			int end = start + length;
			int ti = 0;
			string source = s;

			while (ti < target.Length) {
				if (IsIgnorable (target [ti])) {
					ti++;
					continue;
				}
				if (si >= end) {
					if (s == source)
						break;
					s = source;
					si = escapedSourceIndex;
					end = start + length;
					escapedSourceIndex = -1;
					continue;
				}
				if (IsIgnorable (s [si])) {
					si++;
					continue;
				}

				// Check contraction for target.
				Contraction ctt = GetContraction (target, ti, target.Length);
				if (ctt != null) {
					ti += ctt.Source.Length;
					if (ctt.SortKey != null) {
						int ret = GetMatchLength (ref s, ref si, ref end, -1, ctt.SortKey, true);
						if (ret < 0)
							return false;
						si += ret;
					} else {
						string r = ctt.Replacement;
						int i = 0;
						while (i < r.Length && si < end) {
							int ret = GetMatchLength (ref s, ref si, ref end, r [i]);
							if (ret < 0)
								return false;
							si += ret;
							i++;
						}
						if (i < r.Length && si >= end)
							return false;
					}
				}
				else {
					int ret = GetMatchLength (ref s, ref si, ref end, target [ti]);
					if (ret < 0)
						return false;
					si += ret;
					ti++;
				}
			}
			if (si == end) {
				// All codepoints in the compared range
				// matches. In that case, what matters 
				// is whether the remaining part of 
				// "target" is ignorable or not.
				while (ti < target.Length)
					if (!IsIgnorable (target [ti++]))
						return false;
				return true;
			}
			return true;
		}

		// WARNING: Don't invoke it outside IsPrefix().
		int GetMatchLength (ref string s, ref int idx, ref int end, char target)
		{
			int it = FilterOptions ((int) target);
			charSortKey [0] = Category (it);
			charSortKey [1] = Level1 (it);
			if (!ignoreNonSpace)
				charSortKey [2] = Level2 (it);
			charSortKey [3] = Uni.Level3 (it);

			return GetMatchLength (ref s, ref idx, ref end, it, charSortKey, !Uni.HasSpecialWeight ((char) it));
		}

		// WARNING: Don't invoke it outside IsPrefix().
		// returns consumed source length (mostly 1, source length in case of contraction)
		int GetMatchLength (ref string s, ref int idx, ref int end, int it, byte [] sortkey, bool noLv4)
		{
			Contraction ct = null;
			// If there is already expansion, then it should not
			// process further expansions.
			if (escapedSourceIndex < 0)
				ct = GetContraction (s, idx, end);
			if (ct != null) {
				if (ct.SortKey != null) {
					if (!noLv4)
						return -1;
					for (int i = 0; i < ct.SortKey.Length; i++)
						if (sortkey [i] != ct.SortKey [i])
							return -1;
					return ct.Source.Length;
				} else {
					escapedSourceIndex = idx + ct.Source.Length;
					s = ct.Replacement;
					idx = 0;
					end = s.Length;
					return GetMatchLength (ref s, ref idx, ref end, it, sortkey, noLv4);
				}
			} else {
				// primitive comparison
				if (Compare (s [idx], it, sortkey) != 0)
					return -1;
				return 1;
			}
		}

		// IsSuffix()

		public bool IsSuffix (string src, string target, CompareOptions opt)
		{
			return IsSuffix (src, target, src.Length - 1, src.Length, opt);
		}

		public bool IsSuffix (string s, string target, int start, int length, CompareOptions opt)
		{
			SetOptions (opt);

			// quick check : simple codepoint comparison
			if (s.Length >= target.Length) {
				int si = start;
				for (int i = target.Length - 1; i >= 0; i--, si--)
					if (s [si] != target [i])
						break;
				if (si == start + target.Length)
					return true;
			}

			escapedSourceIndex = -1;
			return IsSuffix (s, target, start, length);
		}

		bool IsSuffix (string s, string target, int start, int length)
		{
			int si = start;
			int ti = target.Length - 1;
			string source = s;
			int end = start - length + 1;

			while (ti >= 0) {
				if (IsIgnorable (target [ti])) {
					ti--;
					continue;
				}
				if (si < 0) {
					if (s == source)
						break;
					s = source;
					si = escapedSourceIndex;
					escapedSourceIndex = -1;
					continue;
				}
				if (IsIgnorable (s [si])) {
					si--;
					continue;
				}

				// Check contraction for target.
				Contraction ctt = GetTailContraction (target, ti, 0);
				if (ctt != null) {
					ti -= ctt.Source.Length;
					if (ctt.SortKey != null) {
						int ret = GetMatchLengthBack (ref s, ref si, ref end, -1, ctt.SortKey, true);
						if (ret < 0)
							return false;
						si -= ret;
					} else {
						string r = ctt.Replacement;
						int i = r.Length - 1;
						while (i >= 0 && si >= end) {
							int ret = GetMatchLengthBack (ref s, ref si, ref end, r [i]);
							if (ret < 0)
								return false;
							si -= ret;
							i--;
						}
						if (i >= 0 && si < end)
							return false;
					}
				}
				else {
					int ret = GetMatchLengthBack (ref s, ref si, ref end, target [ti]);
					if (ret < 0)
						return false;
					si -= ret;
					ti--;
				}
			}
			if (si < end) {
				// All codepoints in the compared range
				// matches. In that case, what matters 
				// is whether the remaining part of 
				// "target" is ignorable or not.
				while (ti >= 0)
					if (!IsIgnorable (target [ti--]))
						return false;
				return true;
			}
			return true;
		}

		// WARNING: Don't invoke it outside IsSuffix().
		int GetMatchLengthBack (ref string s, ref int idx, ref int end, char target)
		{
			int it = FilterOptions ((int) target);
			charSortKey [0] = Category (it);
			charSortKey [1] = Level1 (it);
			if (!ignoreNonSpace)
				charSortKey [2] = Level2 (it);
			charSortKey [3] = Uni.Level3 (it);

			return GetMatchLengthBack (ref s, ref idx, ref end, it, charSortKey, !Uni.HasSpecialWeight ((char) it));
		}

		// WARNING: Don't invoke it outside IsSuffix().
		// returns consumed source length (mostly 1, source length in case of contraction)
		int GetMatchLengthBack (ref string s, ref int idx, ref int end, int it, byte [] sortkey, bool noLv4)
		{
			Contraction ct = null;
			// If there is already expansion, then it should not
			// process further expansions.
			if (escapedSourceIndex < 0)
				ct = GetTailContraction (s, idx, end);
			if (ct != null) {
				if (ct.SortKey != null) {
					if (!noLv4)
						return -1;
					for (int i = 0; i < ct.SortKey.Length; i++)
						if (sortkey [i] != ct.SortKey [i])
							return -1;
					return ct.Source.Length;
				} else {
					escapedSourceIndex = idx - ct.Source.Length;
					s = ct.Replacement;
					idx = s.Length - 1;
					end = 0;
					return GetMatchLength (ref s, ref idx, ref end, it, sortkey, noLv4);
				}
			} else {
				// primitive comparison
				if (Compare (s [idx], it, sortkey) != 0)
					return -1;
				return 1;
			}
		}

		// Common use methods 

		// returns comparison result.
		private int Compare (char src, int ct, byte [] sortkey)
		{
			// char-by-char comparison.
			int cs = FilterOptions (src);
			if (cs == ct)
				return 0;
			// lv.1 to 3
			int ret = Category (cs) - Category (ct);
			if (ret != 0)
				return ret;
			ret = Level1 (cs) - Level1 (ct);
			if (ret != 0)
				return ret;
			if (!ignoreNonSpace) {
				ret = Level2 (cs) - Level2 (ct);
				if (ret != 0)
					return ret;
			}
			ret = Uni.Level3 (cs) - Uni.Level3 (ct);
			if (ret != 0)
				return ret;
			// lv.4 (only when required). No need to check cj coz
			// there is no pair of characters that has the same
			// primary level and differs here.
			if (!Uni.HasSpecialWeight (src))
				return 0;
			char target = (char) ct;
			ret = CompareFlagPair (
				Uni.IsJapaneseSmallLetter (src),
				Uni.IsJapaneseSmallLetter (target));
			if (ret != 0)
				return ret;
			ret = Uni.GetJapaneseDashType (src) -
				Uni.GetJapaneseDashType (target);
			if (ret != 0)
				return ret;
			ret = CompareFlagPair (Uni.IsHiragana (src),
				Uni.IsHiragana (target));
			if (ret != 0)
				return ret;
			ret = CompareFlagPair (Uni.IsHalfWidthKana (src),
				Uni.IsHalfWidthKana (target));
			return ret;
		}

		int CompareFlagPair (bool b1, bool b2)
		{
			return b1 == b2 ? 0 : b1 ? 1 : -1;
		}

		#endregion

		#region IndexOf() / LastIndexOf()

		// IndexOf (string, string, CompareOptions)
		// IndexOf (string, string, int, int, CompareOptions)
		// IndexOf (string, char, int, int, CompareOptions)
		// IndexOfPrimitiveChar (string, int, int, char)
		// IndexOfSortKey (string, int, int, byte[], char, int, bool)
		// IndexOf (string, string, int, int)

		public int IndexOf (string s, string target, CompareOptions opt)
		{
			return IndexOf (s, target, 0, s.Length, opt);
		}

		public int IndexOf (string s, string target, int start, int length, CompareOptions opt)
		{
			SetOptions (opt);
			return IndexOf (s, target, start, length);
		}

		public int IndexOf (string s, char target, CompareOptions opt)
		{
			return IndexOf (s, target, 0, s.Length, opt);
		}

		public int IndexOf (string s, char target, int start, int length, CompareOptions opt)
		{
			SetOptions (opt);

			// If target is contraction, then use string search.
			Contraction ct = GetContraction (target);
			if (ct != null) {
				if (ct.Replacement != null)
					return IndexOfPrimitiveChar (s, start, length, ct.Replacement [0]);
				else
					return IndexOfSortKey (s, start, length, ct.SortKey, char.MinValue, -1, true);
			}
			else
				return IndexOfPrimitiveChar (s, start, length, target);
		}

		// Searches target char w/o checking contractions
		int IndexOfPrimitiveChar (string s, int start, int length, char target)
		{
			int ti = FilterOptions ((int) target);
			charSortKey [0] = Category (ti);
			charSortKey [1] = Level1 (ti);
			if (!ignoreNonSpace)
				charSortKey [2] = Level2 (ti);
			charSortKey [3] = Uni.Level3 (ti);
			return IndexOfSortKey (s, start, length, charSortKey, target, ti, !Uni.HasSpecialWeight ((char) ti));
		}

		// Searches target byte[] keydata
		int IndexOfSortKey (string s, int start, int length, byte [] sortkey, char target, int ti, bool noLv4)
		{
			int end = start + length;
			for (int idx = start; idx < end; idx++) {
				int cur = idx;
				if (Matches (s, ref idx, end, ti, target, sortkey, noLv4, false))
					return cur;
			}
			return -1;
		}

		// Searches string. Search head character (or keydata when
		// the head is contraction sortkey) and try IsPrefix().
		int IndexOf (string s, string target, int start, int length)
		{
			Contraction ct = GetContraction (target, 0, target.Length);
			byte [] sortkey = ct != null ? ct.SortKey : null;
			string replace = ct != null ? ct.Replacement : null;
			do {
				int idx = 0;
				if (sortkey != null)
					idx = IndexOfSortKey (s, start, length, ct.SortKey, char.MinValue, -1, true);
				else if (replace != null)
					idx = IndexOf (ct.Replacement, target, 0, ct.Replacement.Length);
				else
					idx = IndexOfPrimitiveChar (s, start, length, target [0]);
				if (idx < 0)
					return -1;
				if (IsPrefix (s, target, idx, length - (idx - start)))
					return idx;
				start++;
				length--;
			} while (length > 0);
			return -1;
		}

		//
		// There are the same number of IndexOf() related methods,
		// with the same functionalities for each.
		//

		public int LastIndexOf (string s, string target, CompareOptions opt)
		{
			return LastIndexOf (s, target, s.Length - 1, s.Length, opt);
		}

		public int LastIndexOf (string s, string target, int start, int length, CompareOptions opt)
		{
			SetOptions (opt);
			return LastIndexOf (s, target, start, length);
		}

		public int LastIndexOf (string s, char target, CompareOptions opt)
		{
			return LastIndexOf (s, target, s.Length - 1, s.Length, opt);
		}

		public int LastIndexOf (string s, char target, int start, int length, CompareOptions opt)
		{
			SetOptions (opt);

			// If target is contraction, then use string search.
			Contraction ct = GetContraction (target);
			if (ct != null) {
				if (ct.Replacement != null)
					return LastIndexOfPrimitiveChar (s, start, length, ct.Replacement [0]);
				else
					return LastIndexOfSortKey (s, start, length, ct.SortKey, char.MinValue, -1, true);
			}
			else
				return LastIndexOfPrimitiveChar (s, start, length, target);
		}

		// Searches target char w/o checking contractions
		int LastIndexOfPrimitiveChar (string s, int start, int length, char target)
		{
			int ti = FilterOptions ((int) target);
			charSortKey [0] = Category (ti);
			charSortKey [1] = Level1 (ti);
			if (!ignoreNonSpace)
				charSortKey [2] = Level2 (ti);
			charSortKey [3] = Uni.Level3 (ti);
			return LastIndexOfSortKey (s, start, length, charSortKey, target, ti, !Uni.HasSpecialWeight ((char) ti));
		}

		// Searches target byte[] keydata
		int LastIndexOfSortKey (string s, int start, int length, byte [] sortkey, char target, int ti, bool noLv4)
		{
			int end = start - length;

			for (int idx = start; idx > end; idx--) {
				int cur = idx;
				if (Matches (s, ref idx, end, ti, target, sortkey, noLv4, true))
					return cur;
			}
			return -1;
		}

		// Searches string. Search head character (or keydata when
		// the head is contraction sortkey) and try IsPrefix().
		int LastIndexOf (string s, string target, int start, int length)
		{
			int orgStart = start;

			do {
				int idx = 0;
				Contraction ct = GetContraction (s, start, length);
				if (ct != null) {
					if (ct.SortKey != null)
						idx = LastIndexOfSortKey (s, start, length, ct.SortKey, char.MinValue, -1, true);
					else
						idx = LastIndexOf (ct.Replacement, target, ct.Replacement.Length - 1, ct.Replacement.Length);
				}
				else
					idx = LastIndexOfPrimitiveChar (s, start, length, target [0]);

				if (idx < 0)
					return -1;
				if (IsPrefix (s, target, idx, orgStart - idx + 1))
					return idx;
				length--;
				start--;
			} while (length > 0);
			return -1;
		}

		private bool Matches (string s, ref int idx, int end, int ti, char target, byte [] sortkey, bool noLv4, bool lastIndexOf)
		{
			switch (char.GetUnicodeCategory (s [idx])) {
			case UnicodeCategory.PrivateUse:
			case UnicodeCategory.Surrogate:
				if (s [idx] != target)
					return false;
				return true;
			}

			char sc = char.MinValue;
			Contraction ct = GetContraction (s, idx, end);
			// if lv4 exists, it never matches contraction
			if (ct != null && noLv4) {
				if (lastIndexOf)
					idx -= ct.Source.Length - 1;
				else
					idx += ct.Source.Length - 1;
				if (ct.SortKey != null) {
					for (int i = 0; i < sortkey.Length; i++)
						if (ct.SortKey [i] != sortkey [i])
							return false;
					return true;
				}
				// Here is the core of LAMESPEC
				// described at the top of the source.
				sc = ct.Replacement [0];
			}
			else
				sc = s [idx];

			if (sc == target)
				return true;
			int si = FilterOptions ((int) sc);
			if (Category (si) != sortkey [0] ||
				Level1 (si) != sortkey [1] ||
				!ignoreNonSpace && Level2 (si) != sortkey [2] ||
				Uni.Level3 (si) != sortkey [3])
				return false;
			if (noLv4 && !Uni.HasSpecialWeight ((char) si))
				return true;
			else if (noLv4)
				return false;
			if (Uni.IsJapaneseSmallLetter ((char) si) !=
				Uni.IsJapaneseSmallLetter ((char) ti) ||
				Uni.GetJapaneseDashType ((char) si) !=
				Uni.GetJapaneseDashType ((char) ti) ||
				!Uni.IsHiragana ((char) si) !=
				!Uni.IsHiragana ((char) ti) ||
				Uni.IsHalfWidthKana ((char) si) !=
				Uni.IsHalfWidthKana ((char) ti))
				return false;
			return true;
		}

		#endregion
	}
}
