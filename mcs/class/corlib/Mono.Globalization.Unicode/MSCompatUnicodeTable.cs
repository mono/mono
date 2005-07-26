#define USE_MANAGED_RESOURCE
using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

using UUtil = Mono.Globalization.Unicode.MSCompatUnicodeTableUtil;

namespace Mono.Globalization.Unicode
{
	internal class TailoringInfo
	{
		public readonly int LCID;
		public readonly int TailoringIndex;
		public readonly int TailoringCount;
		public readonly bool FrenchSort;

		public TailoringInfo (int lcid, int tailoringIndex, int tailoringCount, bool frenchSort)
		{
			LCID = lcid;
			TailoringIndex = tailoringIndex;
			TailoringCount = tailoringCount;
			FrenchSort = frenchSort;
		}
	}

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

	unsafe internal class MSCompatUnicodeTable
	{
		public static TailoringInfo GetTailoringInfo (int lcid)
		{
			for (int i = 0; i < tailoringInfos.Length; i++)
				if (tailoringInfos [i].LCID == lcid)
					return tailoringInfos [i];
			return null;
		}

		public static void BuildTailoringTables (CultureInfo culture,
			TailoringInfo t,
			ref Contraction [] contractions,
			ref Level2Map [] diacriticals)
		{
			// collect tailoring entries.
			ArrayList cmaps = new ArrayList ();
			ArrayList dmaps = new ArrayList ();
			char [] tarr = tailorings;
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

		static void SetCJKReferences (string name,
			ref CodePointIndexer cjkIndexer,
			ref byte* catTable, ref byte* lv1Table,
			ref CodePointIndexer lv2Indexer, ref byte* lv2Table)
		{
			// as a part of mscorlib.dll, this invocation is
			// somewhat extraneous (pointers were already assigned).

			switch (name) {
			case "zh-CHS":
				catTable = cjkCHScategory;
				lv1Table = cjkCHSlv1;
				cjkIndexer = UUtil.CjkCHS;
				break;
			case "zh-CHT":
				catTable = cjkCHTcategory;
				lv1Table = cjkCHTlv1;
				cjkIndexer = UUtil.Cjk;
				break;
			case "ja":
				catTable = cjkJAcategory;
				lv1Table = cjkJAlv1;
				cjkIndexer = UUtil.Cjk;
				break;
			case "ko":
				catTable = cjkKOcategory;
				lv1Table = cjkKOlv1;
				lv2Table = cjkKOlv2;
				cjkIndexer = UUtil.Cjk;
				lv2Indexer = UUtil.Cjk;
				break;
			}
		}

		public static byte Category (int cp)
		{
			return categories [UUtil.Category.ToIndex (cp)];
		}

		public static byte Level1 (int cp)
		{
			return level1 [UUtil.Level1.ToIndex (cp)];
		}

		public static byte Level2 (int cp)
		{
			return level2 [UUtil.Level2.ToIndex (cp)];
		}

		public static byte Level3 (int cp)
		{
			return level3 [UUtil.Level3.ToIndex (cp)];
		}

		public static bool IsIgnorable (int cp)
		{
			UnicodeCategory uc = Char.GetUnicodeCategory ((char) cp);
			// This check eliminates some extraneous code areas
			if (uc == UnicodeCategory.OtherNotAssigned)
				return true;
			// Some characters in Surrogate area are ignored.
			if (0xD880 <= cp && cp < 0xDB80)
				return true;
			int i = UUtil.Ignorable.ToIndex (cp);
			return i >= 0 && ignorableFlags [i] == 7;
		}
		// Verifier:
		// for (int i = 0; i <= char.MaxValue; i++)
		//	if (Char.GetUnicodeCategory ((char) i)
		//		== UnicodeCategory.OtherNotAssigned 
		//		&& ignorableFlags [i] != 7)
		//		Console.WriteLine ("{0:X04}", i);

		public static bool IsIgnorableSymbol (int cp)
		{
			int i = UUtil.Ignorable.ToIndex (cp);
			return i >= 0 && (ignorableFlags [i] & 0x2) != 0;
		}

		public static bool IsIgnorableNonSpacing (int cp)
		{
			int i = UUtil.Ignorable.ToIndex (cp);
			return i >= 0 && (ignorableFlags [i] & 0x4) != 0;
			// It could be implemented this way, but the above
			// is faster.
//			return categories [UUtil.Category.ToIndex (cp)] == 1;
		}

		public static int ToKanaTypeInsensitive (int i)
		{
			// Note that IgnoreKanaType does not treat half-width
			// katakana as equivalent to full-width ones.

			// Thus, it is so simple ;-)
			return (0x3041 <= i && i <= 0x3094) ? i + 0x60 : i;
		}

		// Note that currently indexer optimizes this table a lot,
		// which might have resulted in bugs.
		public static int ToWidthCompat (int i)
		{
			if (i < 0x2190)
				return i;
			if (i > 0xFF00) {
				if (i <= 0xFF5E)
					return i - 0xFF00 + 0x20;
				switch (i) {
				case 0xFFE0:
					return 0xA2;
				case 0xFFE1:
					return 0xA3;
				case 0xFFE2:
					return 0xAC;
				case 0xFFE3:
					return 0xAF;
				case 0xFFE4:
					return 0xA6;
				case 0xFFE5:
					return 0xA5;
				case 0xFFE6:
					return 0x20A9;
				}
			}
			if (i > 0x32FE)
				return i;

			if (i <= 0x2193)
				return 0xFFE9 - 0x2190 + i;
			if (i < 0x2502)
				return i;
			if (i <= 0x25CB) {
				switch (i) {
				case 0x2502:
					return 0xFFE8;
				case 0x25A0:
					return 0xFFED;
				case 0x25CB:
					return 0xFFEE;
				default:
					return i;
				}
			}
			if (i < 0x3000)
				return i;
			if (i < 0x3131) {
				switch (i) {
				case 0x3000:
					return 0x20;
				case 0x3001:
					return 0xFF64;
				case 0x3002:
					return 0xFF61;
				case 0x300C:
					return 0xFF62;
				case 0x300D:
					return 0xFF63;
				case 0x30FB:
					return 0xFF65;
				// Other Kana compat characters' width
				// compatibility is considered in special weight.
				default:
					return i;
				}
			}
			if (i < 0x3164) { // Hangul compat
				return i - 0x3130 + 0xFFA0;
			}
			if (i == 0x3164)
				return 0xFFA0;
			// 0x32D0-0x32FE are Kana compat characters, whose
			// width compatibility is considered in special weight.
			return i;
		}

		#region Level 4 properties (Kana)

		public static bool HasSpecialWeight (char c)
		{
			if (c < '\u3041')
				return false;
			else if ('\uFF66' <= c && c < '\uFF9E')
				return true;
			else if ('\u3300' <= c)
				return false;
			else if (c < '\u309D')
				return (c < '\u3099');
			else if (c < '\u3100')
				return c != '\u30FB';
			else if (c < '\u32D0')
				return false;
			else if (c < '\u32FF')
				return true;
			return false;
		}

		// FIXME: it should be removed at some stage
		// (will become unused).
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
			return 3;
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
			if ('\uFF67' <= c && c <= '\uFF6F')
				return true;
			if ('\u3040' < c && c < '\u30FA') {
				switch (c) {
				case '\u3041':
				case '\u3043':
				case '\u3045':
				case '\u3047':
				case '\u3049':
				case '\u3063':
				case '\u3083':
				case '\u3085':
				case '\u3087':
				case '\u308E':
				case '\u30A1':
				case '\u30A3':
				case '\u30A5':
				case '\u30A7':
				case '\u30A9':
				case '\u30C3':
				case '\u30E3':
				case '\u30E5':
				case '\u30E7':
				case '\u30EE':
				case '\u30F5':
				case '\u30F6':
					return true;
				}
			}
			return false;
		}

		#endregion


		static readonly byte* ignorableFlags;
		static readonly byte* categories;
		static readonly byte* level1;
		static readonly byte* level2;
		static readonly byte* level3;
//		static readonly ushort* widthCompat;
		static byte* cjkCHScategory;
		static byte* cjkCHTcategory;
		static byte* cjkJAcategory;
		static byte* cjkKOcategory;
		static byte* cjkCHSlv1;
		static byte* cjkCHTlv1;
		static byte* cjkJAlv1;
		static byte* cjkKOlv1;
		static byte* cjkKOlv2;

#if GENERATE_TABLE

		public static readonly bool IsReady = true; // always

		static MSCompatUnicodeTable ()
		{
			fixed (byte* tmp = ignorableFlagsArr) {
				ignorableFlags = tmp;
			}
			fixed (byte* tmp = categoriesArr) {
				categories = tmp;
			}
			fixed (byte* tmp = level1Arr) {
				level1 = tmp;
			}
			fixed (byte* tmp = level2Arr) {
				level2 = tmp;
			}
			fixed (byte* tmp = level3Arr) {
				level3 = tmp;
			}
//			fixed (ushort* tmp = widthCompatArr) {
//				widthCompat = tmp;
//			}
			fixed (byte* tmp = cjkCHSArr) {
				cjkCHScategory = tmp;
				cjkCHSlv1 = tmp + cjkCHSArrLength;
			}
			fixed (byte* tmp = cjkCHTArr) {
				cjkCHTcategory = tmp;
				cjkCHTlv1 = tmp + cjkCHTArrLength;
			}
			fixed (byte* tmp = cjkJAArr) {
				cjkJAcategory = tmp;
				cjkJAlv1 = tmp + cjkJAArrLength;
			}
			fixed (byte* tmp = cjkKOArr) {
				cjkKOcategory = tmp;
				cjkKOlv1 = tmp + cjkKOArrLength;
			}
			fixed (byte* tmp = cjkKOlv2Arr) {
				cjkKOlv2 = tmp;
			}
		}

		public static void FillCJK (string name,
			ref CodePointIndexer cjkIndexer,
			ref byte* catTable, ref byte* lv1Table,
			ref CodePointIndexer cjkLv2Indexer,
			ref byte* lv2Table)
		{
			SetCJKReferences (name, ref cjkIndexer,
				ref catTable, ref lv1Table,
				ref cjkLv2Indexer, ref lv2Table);
		}
#else

		static readonly char [] tailorings;
		static readonly TailoringInfo [] tailoringInfos;
		static object forLock = new object ();
		public static readonly bool isReady;

		public static bool IsReady {
			get { return isReady; }
		}

#if USE_MANAGED_RESOURCE
		static IntPtr GetResource (string name)
		{
			int size;
			Module module;
			return Assembly.GetExecutingAssembly ().GetManifestResourceInternal (name, out size, out module);
		}
#else
		static readonly string corlibPath = Assembly.GetExecutingAssembly ().Location;

		const int CollationResourceCore = 0;
		const int CollationResourceCJKCHS = 1;
		const int CollationResourceCJKCHT = 2;
		const int CollationResourceCJKJA = 3;
		const int CollationResourceCJKKO = 4;
		const int CollationResourceCJKKOlv2 = 5;
		const int CollationResourceTailoring = 6;

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void load_collation_resource (string path, int resource_index, byte** data, int* size);
#endif

		const int ResourceVersionSize = 1;

		static uint UInt32FromBytePtr (byte* raw, uint idx)
		{
			return (uint) (raw [idx] + (raw [idx + 1] << 8)
				+ (raw [idx + 2] << 16) + (raw [idx + 3] << 24));
		}

		static MSCompatUnicodeTable ()
		{
			byte* raw;
			byte* tailor;
			uint size;
			uint idx = 0;

#if USE_MANAGED_RESOURCE
			IntPtr ptr = GetResource ("collation.core.bin");
			if (ptr == IntPtr.Zero)
				return;
			raw = (byte*) ((void*) ptr);
			ptr = GetResource ("collation.tailoring.bin");
			if (ptr == IntPtr.Zero)
				return;
			tailor = (byte*) ((void*) ptr);
#else
			int rawsize;
			int trawsize;

			lock (forLock) {
				load_collation_resource (corlibPath, CollationResourceCore, &raw, &rawsize);
				load_collation_resource (corlibPath, CollationResourceTailoring, &tailor, &trawsize);
			}
#endif

			if (raw == null || tailor == null)
				return;
			// check resource version
			if (raw [0] != UUtil.ResourceVersion ||
				tailor [0] != UUtil.ResourceVersion)
				return;

			idx = 1;
			size = UInt32FromBytePtr (raw, idx);
			idx += 4;
			ignorableFlags = raw + idx;
			idx += size;

			size = UInt32FromBytePtr (raw, idx);
			idx += 4;
			categories = raw + idx;
			idx += size;

			size = UInt32FromBytePtr (raw, idx);
			idx += 4;
			level1 = raw + idx;
			idx += size;

			size = UInt32FromBytePtr (raw, idx);
			idx += 4;
			level2 = raw + idx;
			idx += size;

			size = UInt32FromBytePtr (raw, idx);
			idx += 4;
			level3 = raw + idx;
			idx += size;

//			size = UInt32FromBytePtr (raw, idx);
//			idx += 4;
//			widthCompat = (ushort*) (raw + idx);
//			idx += size * 2;

			idx = 1;
			uint count = UInt32FromBytePtr (tailor, idx);
			idx += 4;
			tailoringInfos = new TailoringInfo [count];
			for (int i = 0; i < count; i++) {
				int i1 = (int) UInt32FromBytePtr (tailor, idx);
				idx += 4;
				int i2 = (int) UInt32FromBytePtr (tailor, idx);
				idx += 4;
				int i3 = (int) UInt32FromBytePtr (tailor, idx);
				idx += 4;
				TailoringInfo ti = new TailoringInfo (
					i1, i2, i3, tailor [idx++] != 0);
				tailoringInfos [i] = ti;
			}
			idx += 2; // dummy
			// tailorings
			count = UInt32FromBytePtr (tailor, idx);
			idx += 4;
			tailorings = new char [count];
			for (int i = 0; i < count; i++, idx += 2)
				tailorings [i] = (char) (tailor [idx] + (tailor [idx + 1] << 8));
			isReady = true;
		}

		public static void FillCJK (string culture,
			ref CodePointIndexer cjkIndexer,
			ref byte* catTable,
			ref byte* lv1Table,
			ref CodePointIndexer lv2Indexer,
			ref byte* lv2Table)
		{
			lock (forLock) {
				FillCJKCore (culture, ref cjkIndexer,
					ref catTable, ref lv1Table,
					ref lv2Indexer, ref lv2Table);
				SetCJKReferences (culture, ref cjkIndexer,
					ref catTable, ref lv1Table,
					ref lv2Indexer, ref lv2Table);
			}
		}

		static void FillCJKCore (string culture,
			ref CodePointIndexer cjkIndexer,
			ref byte* catTable, ref byte* lv1Table,
			ref CodePointIndexer cjkLv2Indexer, ref byte* lv2Table)
		{
			if (!IsReady)
				return;

			string name = null;
			switch (culture) {
			case "zh-CHS":
				name = "cjkCHS";
				catTable = cjkCHScategory;
				lv1Table = cjkCHSlv1;
				break;
			case "zh-CHT":
				name = "cjkCHT";
				catTable = cjkCHTcategory;
				lv1Table = cjkCHTlv1;
				break;
			case "ja":
				name = "cjkJA";
				catTable = cjkJAcategory;
				lv1Table = cjkJAlv1;
				break;
			case "ko":
				name = "cjkKO";
				catTable = cjkKOcategory;
				lv1Table = cjkKOlv1;
				break;
			}

			if (name == null || lv1Table != null)
				return;

			byte* raw;
#if USE_MANAGED_RESOURCE
			string filename =
				String.Format ("collation.{0}.bin", name);
			IntPtr ptr = GetResource (filename);
			if (ptr == IntPtr.Zero)
				return;
			raw = (byte*) ((void*) ptr);
#else
			int size;
			int residx = -1;
			switch (culture) {
			case "zh-CHS": residx = CollationResourceCJKCHS; break;
			case "zh-CHT": residx = CollationResourceCJKCHT; break;
			case "ja": residx = CollationResourceCJKJA; break;
			case "ko": residx = CollationResourceCJKKO; break;
			}
			if (residx < 0)
				return;
			load_collation_resource (corlibPath, residx, &raw, &size);
#endif
			uint count = UInt32FromBytePtr (raw, ResourceVersionSize);
			catTable = (byte*) raw + ResourceVersionSize + 4;
			lv1Table = (byte*) raw + ResourceVersionSize + 4 + count;

			switch (culture) {
			case "zh-CHS":
				cjkCHScategory = catTable;
				cjkCHSlv1 = lv1Table;
				break;
			case "zh-CHT":
				cjkCHTcategory = catTable;
				cjkCHTlv1 = lv1Table;
				break;
			case "ja":
				cjkJAcategory = catTable;
				cjkJAlv1 = lv1Table;
				break;
			case "ko":
				cjkKOcategory = catTable;
				cjkKOlv1 = lv1Table;
				break;
			}

			if (name != "cjkKO")
				return;
#if USE_MANAGED_RESOURCE
			ptr = GetResource ("collation.cjkKOlv2.bin");
			if (ptr == IntPtr.Zero)
				return;
			raw = (byte*) ((void*) ptr);
#else
			load_collation_resource (corlibPath, CollationResourceCJKKOlv2, &raw, &size);
#endif
			cjkKOlv2 = raw + ResourceVersionSize + 4;
			lv2Table = cjkKOlv2;
		}
	}
}
#endif


		// For "categories", 0 means no primary weight. 6 means 
		// variable weight
		// For expanded character the value is never filled (i.e. 0).
		// Those arrays will be split into blocks (<3400 and >F800)
		// level 4 is computed.

		// public static bool HasSpecialWeight (char c)
		// { return level1 [(int) c] == 6; }

		//
		// autogenerated code or icall to fill array runs here
		//

