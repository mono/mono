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

		internal static readonly byte [] ignorableFlags =
			Uni.ignorableFlags;
		internal static readonly byte [] categories =
			Uni.categories;
		internal static readonly byte [] level1 =
			Uni.level1;
		internal static readonly byte [] level2 =
			Uni.level2;
		internal static readonly byte [] level3 =
			Uni.level3;
		internal static readonly ushort [] widthCompat =
			Uni.widthCompat;
		internal static readonly CodePointIndexer categoryIndexer =
			UUtil.Category;
		internal static readonly CodePointIndexer lv1Indexer =
			UUtil.Level1;
		internal static readonly CodePointIndexer lv2Indexer =
			UUtil.Level2;
		internal static readonly CodePointIndexer lv3Indexer =
			UUtil.Level3;
		internal static readonly CodePointIndexer widthIndexer =
			UUtil.WidthCompat;


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
		byte [] charSortKey2 = new byte [4];
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
			string name = GetNeutralCulture (culture).Name;

			Uni.FillCJK (name);

			// custom CJK table support.
			switch (name) {
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
				return categories [categoryIndexer.ToIndex (cp)];
			int idx = cjkIndexer.ToIndex (cp);
			ushort cjk = idx < 0 ? (ushort) 0 : cjkTable [idx];
			return cjk != 0 ? (byte) ((cjk & 0xFF00) >> 8) :
				categories [categoryIndexer.ToIndex (cp)];
		}

		byte Level1 (int cp)
		{
			if (cp < 0x3000 || cjkTable == null)
				return level1 [lv1Indexer.ToIndex (cp)];
			int idx = cjkIndexer.ToIndex (cp);
			ushort cjk = idx < 0 ? (ushort) 0 : cjkTable [idx];
			return cjk != 0 ? (byte) (cjk & 0xFF) :
				level1 [lv1Indexer.ToIndex (cp)];
		}

		byte Level2 (int cp, ExtenderType ext)
		{
			if (ext == ExtenderType.Buggy)
				return 5;
			else if (ext == ExtenderType.Conditional)
				return 0;

			if (cp < 0x3000 || cjkLv2Table == null)
				return level2 [lv2Indexer.ToIndex (cp)];
			int idx = cjkLv2Indexer.ToIndex (cp);
			byte ret = idx < 0 ? (byte) 0 : cjkLv2Table [idx];
			if (ret != 0)
				return ret;
			ret = level2 [lv2Indexer.ToIndex (cp)];
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

		bool IsHalfKana (int cp)
		{
			return ignoreWidth || Uni.IsHalfWidthKana ((char) cp);
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
				int diff = ct.Source [0] - s [start];
				if (diff > 0)
					return null; // it's already sorted
				else if (diff < 0)
					continue;
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
				int diff = ct.Source [0] - s [end];
				if (diff > 0)
					return null; // it's already sorted
				else if (diff < 0)
					continue;
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
			if (ignoreWidth) {
				int x = widthCompat [widthIndexer.ToIndex (i)];
				if (x != 0)
					i = x;
			}
			if (ignoreCase)
				i = textInfo.ToLower ((char) i);
			if (ignoreKanaType)
				i = Uni.ToKanaTypeInsensitive (i);
			return i;
		}

		int previousChar = -1;
		byte [] previousSortKey = null;
		int previousChar2 = -1;
		byte [] previousSortKey2 = null;

		enum ExtenderType {
			None,
			Simple,
			Voiced,
			Conditional,
			Buggy,
		}

		ExtenderType GetExtenderType (int i)
		{
			// LAMESPEC: Windows expects true for U+3005, but 
			// sometimes it does not represent to repeat just
			// one character.
			// Windows also expects true for U+3031 and U+3032,
			// but they should *never* repeat one character.

			if (i < 0x3005 || i > 0xFF70)
				return ExtenderType.None;
			if (i == 0xFE7C || i == 0xFE7D)
				return ExtenderType.Simple;
			if (i == 0xFF70)
				return ExtenderType.Conditional;
			if (i > 0x30FE)
				return ExtenderType.None;
			switch (i) {
			case 0x3005: // LAMESPEC: see above.
				return ExtenderType.Buggy;
			case 0x3031: // LAMESPEC: see above.
			case 0x3032: // LAMESPEC: see above.
			case 0x309D:
			case 0x30FD:
				return ExtenderType.Simple;
			case 0x309E:
			case 0x30FE:
				return ExtenderType.Voiced;
			case 0x30FC:
				return ExtenderType.Conditional;
			default:
				return ExtenderType.None;
			}
		}

		byte ToDashTypeValue (ExtenderType ext)
		{
			if (ignoreNonSpace) // LAMESPEC: huh, why?
				return 3;
			switch (ext) {
			case ExtenderType.None:
				return 3;
			case ExtenderType.Conditional:
				return 5;
			default:
				return 4;
			}
		}

		bool IsIgnorable (int i)
		{
			return Uni.IsIgnorable (i) ||
				ignoreSymbols && Uni.IsIgnorableSymbol (i) ||
				ignoreNonSpace && Uni.IsIgnorableNonSpacing (i);
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

			buf.Initialize (options, lcid, s, frenchSort);
			int end = start + length;
			previousChar = -1;
			GetSortKey (s, start, end);
			return buf.GetResultAndReset ();
		}

		void GetSortKey (string s, int start, int end)
		{
			for (int n = start; n < end; n++) {
				int i = s [n];

				ExtenderType ext = GetExtenderType (i);
				if (ext != ExtenderType.None) {
					i = previousChar;
					if (i >= 0)
						FillSortKeyRaw (i, ext);
					else if (previousSortKey != null) {
						byte [] b = previousSortKey;
						buf.AppendNormal (
							b [0],
							b [1],
							b [2] != 1 ? b [2] : Level2 (i, ext),
							b [3] != 1 ? b [3] : level3 [lv3Indexer.ToIndex (i)]);
					}
					// otherwise do nothing.
					// (if the extender is the first char
					// in the string, then just ignore.)
					continue;
				}

				if (IsIgnorable (i))
					continue;
				i = FilterOptions (i);

				Contraction ct = GetContraction (s, n, end);
				if (ct != null) {
					if (ct.Replacement != null) {
						GetSortKey (ct.Replacement, 0, ct.Replacement.Length);
					} else {
						byte [] b = ct.SortKey;
						buf.AppendNormal (
							b [0],
							b [1],
							b [2] != 1 ? b [2] : Level2 (i, ext),
							b [3] != 1 ? b [3] : level3 [lv3Indexer.ToIndex (i)]);
						previousSortKey = b;
						previousChar = -1;
					}
					n += ct.Source.Length - 1;
				}
				else {
					if (!Uni.IsIgnorableNonSpacing (i))
						previousChar = i;
					FillSortKeyRaw (i, ExtenderType.None);
				}
			}
		}

		void FillSortKeyRaw (int i, ExtenderType ext)
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

			byte level2 = Level2 (i, ext);
			if (Uni.HasSpecialWeight ((char) i)) {
				byte level1 = Level1 (i);
				if (ext == ExtenderType.Conditional)
					level1 = (byte) ((level1 & 0xF) % 8);
				buf.AppendKana (
					Category (i),
					level1,
					level2,
					Uni.Level3 (i),
					Uni.IsJapaneseSmallLetter ((char) i),
					ToDashTypeValue (ext),
					!Uni.IsHiragana ((char) i),
					IsHalfKana ((char) i)
					);
				if (!ignoreNonSpace && ext == ExtenderType.Voiced)
					// Append voice weight
					buf.AppendNormal (1, 1, 1, 0);
			}
			else
				buf.AppendNormal (
					Category (i),
					Level1 (i),
					level2,
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

		class Escape
		{
			public string Source;
			public int Index;
			public int Start;
			public int End;
		}

		// Those instances are reused not to invoke instantiation
		// during Compare().
		Escape escape1 = new Escape ();
		Escape escape2 = new Escape ();

		private int CompareOrdinal (string s1, int idx1, int len1,
			string s2, int idx2, int len2)
		{
			int min = len1 < len2 ? len1 : len2;
			int end1 = idx1 + min;
			int end2 = idx2 + min;
			for (int i1 = idx1, i2 = idx2;
				i1 < end1 && i2 < end2; i1++, i2++)
				if (s1 [i1] != s2 [i2])
					return s1 [i1] - s2 [i2];
			return len1 == len2 ? 0 :
				len1 == min ? - 1 : 1;
		}

		public int Compare (string s1, int idx1, int len1,
			string s2, int idx2, int len2, CompareOptions options)
		{
			// quick equality check
			if (idx1 == idx2 && len1 == len2 &&
				Object.ReferenceEquals (s1, s2))
				return 0;
			// FIXME: this should be done inside Compare() at
			// any time.
//			int ord = CompareOrdinal (s1, idx1, len1, s2, idx2, len2);
//			if (ord == 0)
//				return 0;
			if (options == CompareOptions.Ordinal)
				return CompareOrdinal (s1, idx1, len1, s2, idx2, len2);

#if false // stable easy version, depends on GetSortKey().
			SortKey sk1 = GetSortKey (s1, idx1, len1, options);
			SortKey sk2 = GetSortKey (s2, idx2, len2, options);
			byte [] d1 = sk1.KeyData;
			byte [] d2 = sk2.KeyData;
			int len = d1.Length > d2.Length ? d2.Length : d1.Length;
			for (int i = 0; i < len; i++)
				if (d1 [i] != d2 [i])
					return d1 [i] < d2 [i] ? -1 : 1;
			return d1.Length == d2.Length ? 0 : d1.Length < d2.Length ? -1 : 1;
#else
			SetOptions (options);
			escape1.Source = null;
			escape2.Source = null;
			previousSortKey= previousSortKey2 = null;
			previousChar = previousChar2 = -1;
			int ret = Compare (s1, idx1, len1, s2, idx2, len2, (options & CompareOptions.StringSort) != 0);
			return ret == 0 ? 0 : ret < 0 ? -1 : 1;
#endif
		}

		int Compare (string s1, int idx1, int len1, string s2,
			int idx2, int len2, bool stringSort)
		{
			int start1 = idx1;
			int start2 = idx2;
			int end1 = idx1 + len1;
			int end2 = idx2 + len2;

			// It holds final result that comes from the comparison
			// at level 2 or lower. Even if Compare() found the
			// difference at level 2 or lower, it still has to
			// continue level 1 comparison. FinalResult is used
			// when there was no further differences.
			int finalResult = 0;
			// It holds the comparison level to do. It starts from
			// 5, and becomes 1 when we do primary-only comparison.
			int currentLevel = 5;

			int lv5At1 = -1;
			int lv5At2 = -1;
			int lv5Value1 = 0;
			int lv5Value2 = 0;

			// Skip heading extenders
			for (; idx1 < end1; idx1++)
				if (GetExtenderType (s1 [idx1]) == ExtenderType.None)
					break;
			for (; idx2 < end2; idx2++)
				if (GetExtenderType (s2 [idx2]) == ExtenderType.None)
					break;

			ExtenderType ext1 = ExtenderType.None;
			ExtenderType ext2 = ExtenderType.None;

			while (true) {
				for (; idx1 < end1; idx1++)
					if (!IsIgnorable (s1 [idx1]))
						break;
				for (; idx2 < end2; idx2++)
					if (!IsIgnorable (s2 [idx2]))
						break;

				if (idx1 >= end1) {
					if (escape1.Source == null)
						break;
					s1 = escape1.Source;
					start1 = escape1.Start;
					idx1 = escape1.Index;
					end1 = escape1.End;
					escape1.Source = null;
				}
				if (idx2 >= end2) {
					if (escape2.Source == null)
						break;
					s2 = escape2.Source;
					start2 = escape2.Start;
					idx2 = escape2.Index;
					end2 = escape2.End;
					escape2.Source = null;
				}
#if false
// FIXME: optimization could be done here.

				if (s1 [idx1] == s2 [idx2]) {
					idx1++;
					idx2++;
					continue;
				}
//				while (idx1 >= start1 && !IsSafe ((int) s [idx1]))
//					idx1--;
//				while (idx2 >= start2 && !IsSafe ((int) s [idx2]))
//					idx2--;
#endif

				int cur1 = idx1;
				int cur2 = idx2;
				byte [] sk1 = null;
				byte [] sk2 = null;
				int i1 = FilterOptions (s1 [idx1]);
				int i2 = FilterOptions (s2 [idx2]);
				bool special1 = false;
				bool special2 = false;

				// If current character is an expander, then
				// repeat the previous character.
				ext1 = GetExtenderType (i1);
				if (ext1 != ExtenderType.None) {
					if (previousChar < 0) {
						if (previousSortKey == null) {
							// nothing to extend
							idx1++;
							continue;
						}
						sk1 = previousSortKey;
					}
					else
						i1 = previousChar;
				}
				ext2 = GetExtenderType (i2);
				if (ext2 != ExtenderType.None) {
					if (previousChar2 < 0) {
						if (previousSortKey2 == null) {
							// nothing to extend
							idx2++;
							continue;
						}
						sk2 = previousSortKey2;
					}
					else
						i2 = previousChar2;
				}

				byte cat1 = Category (i1);
				byte cat2 = Category (i2);

				// Handle special weight characters
				if (!stringSort && currentLevel > 4) {
					if (cat1 == 6) {
						lv5At1 = escape1.Source != null ?
							escape1.Index - escape1.Start :
							cur1 - start1;
						// here Windows has a bug that it does
						// not consider thirtiary weight.
						lv5Value1 = Level1 (i1) << 8 + Uni.Level3 (i1);
						previousChar = i1;
						idx1++;
					}
					if (cat2 == 6) {
						lv5At2 = escape2.Source != null ?
							escape2.Index - escape2.Start :
							cur2 - start2;
						// here Windows has a bug that it does
						// not consider thirtiary weight.
						lv5Value2 = Level1 (i2) << 8 + Uni.Level3 (i2);
						previousChar2 = i2;
						idx2++;
					}
					if (cat1 == 6 || cat2 == 6) {
						currentLevel = 4;
						continue;
					}
				}

				Contraction ct1 = null;
				if (ext1 == ExtenderType.None)
					ct1 = GetContraction (s1, idx1, end1);

				int offset1 = 1;
				if (sk1 != null)
					offset1 = 1;
				else if (ct1 != null) {
					offset1 = ct1.Source.Length;
					if (ct1.SortKey != null) {
						sk1 = charSortKey;
						for (int i = 0; i < ct1.SortKey.Length; i++)
							sk1 [i] = ct1.SortKey [i];
						previousChar = -1;
						previousSortKey = sk1;
					}
					else if (escape1.Source == null) {
						escape1.Source = s1;
						escape1.Start = start1;
						escape1.Index = cur1 + ct1.Source.Length;
						escape1.End = end1;
						s1 = ct1.Replacement;
						idx1 = 0;
						start1 = 0;
						end1 = s1.Length;
						continue;
					}
				}
				else {
					sk1 = charSortKey;
					sk1 [0] = cat1;
					sk1 [1] = Level1 (i1);
					if (!ignoreNonSpace && currentLevel > 1)
						sk1 [2] = Level2 (i1, ext1);
					if (currentLevel > 2)
						sk1 [3] = Uni.Level3 (i1);
					if (currentLevel > 3)
						special1 = Uni.HasSpecialWeight ((char) i1);
					if (cat1 > 1)
						previousChar = i1;
				}

				Contraction ct2 = null;
				if (ext2 == ExtenderType.None)
					ct2 = GetContraction (s2, idx2, end2);

				if (sk2 != null)
					idx2++;
				else if (ct2 != null) {
					idx2 += ct2.Source.Length;
					if (ct2.SortKey != null) {
						sk2 = charSortKey2;
						for (int i = 0; i < ct2.SortKey.Length; i++)
							sk2 [i] = ct2.SortKey [i];
						previousChar2 = -1;
						previousSortKey2 = sk2;
					}
					else if (escape2.Source == null) {
						escape2.Source = s2;
						escape2.Start = start2;
						escape2.Index = cur2 + ct2.Source.Length;
						escape2.End = end2;
						s2 = ct2.Replacement;
						idx2 = 0;
						start2 = 0;
						end2 = s2.Length;
						continue;
					}
				}
				else {
					sk2 = charSortKey2;
					sk2 [0] = cat2;
					sk2 [1] = Level1 (i2);
					if (!ignoreNonSpace && currentLevel > 1)
						sk2 [2] = Level2 (i2, ext2);
					if (currentLevel > 2)
						sk2 [3] = Uni.Level3 (i2);
					if (currentLevel > 3)
						special2 = Uni.HasSpecialWeight ((char) i2);
					if (cat2 > 1)
						previousChar = i2;
					idx2++;
				}

				// Add offset here so that it does not skip
				// idx1 while s2 has a replacement.
				idx1 += offset1;

				// add diacritical marks in s1 here
				if (!ignoreNonSpace) {
					while (idx1 < end1) {
						if (Category (s1 [idx1]) != 1)
							break;
						if (sk1 [2] == 0)
							sk1 [2] = 2;
						sk1 [2] = (byte) (sk1 [2] + 
							Level2 (s1 [idx1], ExtenderType.None));
						idx1++;
					}

					// add diacritical marks in s2 here
					while (idx2 < end2) {
						if (Category (s2 [idx2]) != 1)
							break;
						if (sk2 [2] == 0)
							sk2 [2] = 2;
						sk2 [2] = (byte) (sk2 [2] + 
							Level2 (s2 [idx2], ExtenderType.None));
						idx2++;
					}
				}

				int ret = sk1 [0] - sk2 [0];
				ret = ret != 0 ? ret : sk1 [1] - sk2 [1];
				if (ret != 0)
					return ret;
				if (currentLevel == 1)
					continue;
				if (!ignoreNonSpace) {
					ret = sk1 [2] - sk2 [2];
					if (ret != 0) {
						finalResult = ret;
						currentLevel = frenchSort ? 2 : 1;
						continue;
					}
				}
				if (currentLevel == 2)
					continue;
				ret = sk1 [3] - sk2 [3];
				if (ret != 0) {
					finalResult = ret;
					currentLevel = 2;
					continue;
				}
				if (currentLevel == 3)
					continue;
				if (special1 != special2) {
					finalResult = special1 ? 1 : -1;
					currentLevel = 3;
					continue;
				}
				if (special1) {
					ret = CompareFlagPair (
						!Uni.IsJapaneseSmallLetter ((char) i1),
						!Uni.IsJapaneseSmallLetter ((char) i2));
					ret = ret != 0 ? ret :
						ToDashTypeValue (ext1) -
						ToDashTypeValue (ext2);
					ret = ret != 0 ? ret : CompareFlagPair (
						Uni.IsHiragana ((char) i1),
						Uni.IsHiragana ((char) i2));
					ret = ret != 0 ? ret : CompareFlagPair (
						!IsHalfKana ((char) i1),
						!IsHalfKana ((char) i2));
					if (ret != 0) {
						finalResult = ret;
						currentLevel = 3;
						continue;
					}
				}
			}

			// If there were only level 3 or lower differences,
			// then we still have to find diacritical differences
			// if any.
			if (!ignoreNonSpace &&
				finalResult != 0 && currentLevel > 2) {
				while (idx1 < end1 && idx2 < end2) {
					if (!Uni.IsIgnorableNonSpacing (s1 [idx1]))
						break;
					if (!Uni.IsIgnorableNonSpacing (s2 [idx2]))
						break;
					finalResult = Level2 (FilterOptions ((s1 [idx1])), ext1) - Level2 (FilterOptions (s2 [idx2]), ext2);
					if (finalResult != 0)
						break;
					idx1++;
					idx2++;
					// they should work only for the first character
					ext1 = ExtenderType.None;
					ext2 = ExtenderType.None;
				}
			}
			// we still have to handle level 5
			if (finalResult == 0) {
				finalResult = lv5At1 - lv5At2;
				if (finalResult == 0)
					finalResult = lv5Value1 - lv5Value2;
			}
			return idx1 != end1 ? 1 : idx2 == end2 ? finalResult : -1;
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
				for (int i = 0; si < length && i < target.Length; i++, si++)
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
				// FIXME: pass ExtenderType
				charSortKey [2] = Level2 (it, ExtenderType.None);
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
				int se = start - length;
				for (int i = target.Length - 1; si >= se && i >= 0; i--, si--)
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
				// FIXME: pass extender type
				charSortKey [2] = Level2 (it, ExtenderType.None);
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
				// FIXME: pass ExtenderType
				ret = Level2 (cs, ExtenderType.None) - Level2 (ct, ExtenderType.None);
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
				!Uni.IsJapaneseSmallLetter (src),
				!Uni.IsJapaneseSmallLetter (target));
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
			ret = CompareFlagPair (!IsHalfKana (src),
				!IsHalfKana (target));
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
				// FIXME: pass ExtenderType
				charSortKey [2] = Level2 (ti, ExtenderType.None);
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
			int tidx = 0;
			for (; tidx < target.Length; tidx++)
				if (!IsIgnorable (target [tidx]))
					break;
			if (tidx == target.Length)
				return start;
			Contraction ct = GetContraction (target, tidx, target.Length - tidx);
			byte [] sortkey = ct != null ? ct.SortKey : null;
			string replace = ct != null ? ct.Replacement : null;
			do {
				int idx = 0;
				if (sortkey != null)
					idx = IndexOfSortKey (s, start, length, ct.SortKey, char.MinValue, -1, true);
				else if (replace != null)
					idx = IndexOf (s, replace, start, length);
				else
					idx = IndexOfPrimitiveChar (s, start, length, target [tidx]);
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
				// FIXME: pass ExtenderType
				charSortKey [2] = Level2 (ti, ExtenderType.None);
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
			int tidx = 0;
			for (; tidx < target.Length; tidx++)
				if (!IsIgnorable (target [tidx]))
					break;
			if (tidx == target.Length)
				return start;
			Contraction ct = GetContraction (target, tidx, target.Length - tidx);
			byte [] sortkey = ct != null ? ct.SortKey : null;
			string replace = ct != null ? ct.Replacement : null;

			do {
				int idx = 0;
				if (sortkey != null)
					idx = LastIndexOfSortKey (s, start, length, ct.SortKey, char.MinValue, -1, true);
				else if (replace != null)
					idx = LastIndexOf (s, replace, start, length);
				else
					idx = LastIndexOfPrimitiveChar (s, start, length, target [tidx]);

				if (idx < 0)
					return -1;
				if (IsPrefix (s, target, idx, orgStart - idx + 1)) {
					for (;idx < orgStart; idx++)
						if (!IsIgnorable (s [idx]))
							break;
					return idx;
				}
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
				// FIXME: pass ExtenderType
				!ignoreNonSpace && Level2 (si, ExtenderType.None) != sortkey [2] ||
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
				IsHalfKana ((char) si) !=
				IsHalfKana ((char) ti))
				return false;
			return true;
		}

		#endregion
	}
}
