using System;
using System.Globalization;
using System.IO;
using System.Reflection;

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

	internal class MSCompatUnicodeTable
	{
		public static char [] TailoringValues {
			get { return tailorings; }
		}

		public static ushort [] CjkCHS {
			get { return cjkCHS; }
		}

		public static ushort [] CjkCHT {
			get { return cjkCHT; }
		}

		public static ushort [] CjkJA {
			get { return cjkJA; }
		}

		public static ushort [] CjkKO {
			get { return cjkKO; }
		}

		public static byte [] CjkKOLv2 {
			get { return cjkKOlv2; }
		}

		public static TailoringInfo GetTailoringInfo (int lcid)
		{
			for (int i = 0; i < tailoringInfos.Length; i++)
				if (tailoringInfos [i].LCID == lcid)
					return tailoringInfos [i];
			return null;
		}

		public static byte Categories (int cp)
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
		public static int ToWidthCompat (int cp)
		{
			int i = UUtil.WidthCompat.ToIndex (cp);
			int v = i >= 0 ? (int) widthCompat [i] : 0;
			return v != 0 ? v : cp;
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

#if GENERATE_TABLE

		public static readonly bool IsReady = true; // always

		public static void FillCJK (string name) {}
#else

		static readonly char [] tailorings;
		static readonly TailoringInfo [] tailoringInfos;
		internal static readonly byte [] ignorableFlags;
		internal static readonly byte [] categories;
		internal static readonly byte [] level1;
		internal static readonly byte [] level2;
		internal static readonly byte [] level3;
		internal static readonly ushort [] widthCompat;
		static ushort [] cjkCHS;
		static ushort [] cjkCHT;
		static ushort [] cjkJA;
		static ushort [] cjkKO;
		static byte [] cjkKOlv2;
		static object forLock = new object ();

		public static readonly bool IsReady = false;

		static Stream GetResource (string name)
		{
			return Assembly.GetExecutingAssembly ()
				.GetManifestResourceStream (name);
		}

		static MSCompatUnicodeTable ()
		{
			using (Stream s = GetResource ("collation.core.bin")) {
				// FIXME: remove those lines later.
				// actually this line should not be required,
				// but when we switch from the corlib that
				// does not have resources to the corlib that
				// do have, it tries to read resource from
				// the corlib that runtime kicked and returns
				// null (because old one does not have it).
				// In such cases managed collation won't work.
				if (s == null)
					return;

				BinaryReader reader = new BinaryReader (s);
				FillTable (reader, ref ignorableFlags);
				FillTable (reader, ref categories);
				FillTable (reader, ref level1);
				FillTable (reader, ref level2);
				FillTable (reader, ref level3);

				int size = reader.ReadInt32 ();
				widthCompat = new ushort [size];
				for (int i = 0; i < size; i++)
					widthCompat [i] = reader.ReadUInt16 ();
			}

			using (Stream s = GetResource ("collation.tailoring.bin")) {
				if (s == null) // see FIXME above.
					return;

				BinaryReader reader = new BinaryReader (s);
				// tailoringInfos
				int count = reader.ReadInt32 ();
				HasSpecialWeight ((char) count); // dummy
				tailoringInfos = new TailoringInfo [count];
				for (int i = 0; i < count; i++) {
					TailoringInfo ti = new TailoringInfo (
						reader.ReadInt32 (),
						reader.ReadInt32 (),
						reader.ReadInt32 (),
						reader.ReadBoolean ());
					tailoringInfos [i] = ti;
				}
				reader.ReadByte (); // dummy
				IsHiragana ((char) reader.ReadByte ()); // dummy
				// tailorings
				count = reader.ReadInt32 ();
				tailorings = new char [count];
				for (int i = 0; i < count; i++)
					tailorings [i] = (char) reader.ReadUInt16 ();
			}

			IsReady = true;
		}

		static void FillTable (BinaryReader reader, ref byte [] bytes)
		{
			int size = reader.ReadInt32 ();
			bytes = new byte [size];
			reader.Read (bytes, 0, size);
		}

		public static void FillCJK (string culture)
		{
			lock (forLock) {
				FillCJKCore (culture);
			}
		}

		static void FillCJKCore (string culture)
		{
			if (!IsReady)
				return;

			string name = null;
			ushort [] arr = null;
			switch (culture) {
			case "zh-CHS":
				name = "cjkCHS";
				arr = cjkCHS;
				break;
			case "zh-CHT":
				name = "cjkCHT";
				arr = cjkCHT;
				break;
			case "ja":
				name = "cjkJA";
				arr = cjkJA;
				break;
			case "ko":
				name = "cjkKO";
				arr = cjkKO;
				break;
			}

			if (name == null || arr != null)
				return;

			using (Stream s = GetResource (String.Format ("collation.{0}.bin", name))) {
				BinaryReader reader = new BinaryReader (s);
				int size = reader.ReadInt32 ();
				arr = new ushort [size];
				for (int i = 0; i < size; i++)
					arr [i] = reader.ReadUInt16 ();
			}

			switch (culture) {
			case "zh-CHS":
				cjkCHS = arr;
				break;
			case "zh-CHT":
				cjkCHT = arr;
				break;
			case "ja":
				cjkJA = arr;
				break;
			case "ko":
				cjkKO = arr;
				break;
			}

			if (name != "cjkKO")
				return;

			using (Stream s = GetResource ("collation.cjkKOlv2.bin")) {
				BinaryReader reader = new BinaryReader (s);
				FillTable (reader, ref cjkKOlv2);
			}
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

