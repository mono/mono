using System;
using System.Globalization;
using System.Text;
using System.Runtime.CompilerServices;

using Util = Mono.Globalization.Unicode.NormalizationTableUtil;

namespace Mono.Globalization.Unicode
{
	internal enum NormalizationCheck {
		Yes,
		No,
		Maybe
	}

	internal unsafe class Normalization
	{
		public const int NoNfd = 1;
		public const int NoNfkd = 2;
		public const int NoNfc = 4;
		public const int MaybeNfc = 8;
		public const int NoNfkc = 16;
		public const int MaybeNfkc = 32;
		public const int FullCompositionExclusion = 64;
		public const int IsSafe = 128;
//		public const int ExpandOnNfd = 256;
//		public const int ExpandOnNfc = 512;
//		public const int ExpandOnNfkd = 1024;
//		public const int ExpandOnNfkc = 2048;

		static uint PropValue (int cp)
		{
			return props [Util.PropIdx (cp)];
		}

		static int CharMapIdx (int cp)
		{
			return charMapIndex [Util.MapIdx (cp)];
		}

		static int GetComposedStringLength (int mapIdx)
		{
			int i = mapIdx;
			while (mappedChars [i] != 0)
				i++;
			return i - mapIdx;
		}

		static byte GetCombiningClass (int c)
		{
			return combiningClass [Util.Combining.ToIndex (c)];
		}

		static int GetPrimaryCompositeFromMapIndex (int src)
		{
			return mapIdxToComposite [Util.MapIndexes.ToIndex (src)];
		}

		static short GetPrimaryCompositeHelperIndex (int cp)
		{
			return helperIndex [Util.Helper.ToIndex (cp)];
		}

		static int GetPrimaryCompositeCharIndex (object chars, int start, int charsLength)
		{
			string s = chars as string;
			StringBuilder sb = chars as StringBuilder;
			char startCh = s != null ? s [start] : sb [start];

			int idx = GetPrimaryCompositeHelperIndex ((int) startCh);
			if (idx == 0)
				return 0;
			while (mappedChars [idx] == startCh) {
				for (int i = 1; ; i++) {
					if (mappedChars [idx + i] == 0)
						// match
						return idx;
					if (start + i < charsLength)
						return 0; // no match
					char curCh = s != null ?
						s [start + i] : sb [start + i];
					if (mappedChars [idx + i] == curCh)
						continue;
					if (mappedChars [idx + i] > curCh)
						return 0; // no match
					// otherwise move idx to next item
					while (mappedChars [i] != 0)
						i++;
					idx = i + 1;
					break;
				}
			}
			// reached to end of entries
			return 0;
		}

		private static string Compose (string source, int checkType)
		{
			StringBuilder sb = null;
			Decompose (source, ref sb, checkType);
			if (sb == null)
				sb = Combine (source, 0, checkType);
			else
				Combine (sb, 0, checkType);

			return sb != null ? sb.ToString () : source;
		}

		private static StringBuilder Combine (string source, int start, int checkType)
		{
			for (int i = 0; i < source.Length; i++) {
				if (QuickCheck (source [i], checkType) == NormalizationCheck.Yes)
					continue;
				StringBuilder sb = new StringBuilder (source.Length);
				sb.Append (source);
				Combine (sb, i, checkType);
				return sb;
			}
			return null;
		}

		private static void Combine (StringBuilder sb, int start, int checkType)
		{
			for (int i = start; i < sb.Length; i++) {
				switch (QuickCheck (sb [i], checkType)) {
				case NormalizationCheck.Yes:
					continue;
				case NormalizationCheck.No:
					break;
				case NormalizationCheck.Maybe:
					if (i == 0)
						continue;
					else
						break;
				}

				int cur = i;
				// FIXME: It should use IsUnsafe flag.
				// FIXME: It should check "blocked" too
				for (;i >= 0; i--)
					if (QuickCheck (sb [i], checkType) == NormalizationCheck.Yes)
						break;
				i++;

				// Now i is the "starter"

				int ch = 0;
				int idx = 0;
				for (; i < cur; i++) {
					idx = GetPrimaryComposite (sb, (int) sb [i], sb.Length, i, ref ch);
					if (idx > 0)
						break;
				}
				if (idx == 0)
					continue;
				sb.Remove (i, GetComposedStringLength (idx));
				sb.Insert (i, (char) ch);
				i--; // apply recursively
			}
		}

		static int GetPrimaryComposite (object o, int cur, int length, int bufferPos, ref int ch)
		{
			if ((PropValue (cur) & FullCompositionExclusion) != 0)
				return 0;
			if (GetCombiningClass (cur) != 0)
				return 0; // not a starter
			int idx = GetPrimaryCompositeCharIndex (o, bufferPos, length);
			if (idx == 0)
				return 0;
			return GetPrimaryCompositeFromMapIndex (idx);
		}

		static string Decompose (string source, int checkType)
		{
			StringBuilder sb = null;
			Decompose (source, ref sb, checkType);
			return sb != null ? sb.ToString () : source;
		}

		static void Decompose (string source,
			ref StringBuilder sb, int checkType)
		{
			int [] buf = null;
			int start = 0;
			for (int i = 0; i < source.Length; i++)
				if (QuickCheck (source [i], checkType) == NormalizationCheck.No)
					DecomposeChar (ref sb, ref buf, source,
						i, ref start);
			if (sb != null)
				sb.Append (source, start, source.Length - start);
//			ReorderCanonical (source, ref sb, 1);
		}

		static void ReorderCanonical (string src, ref StringBuilder sb, int start)
		{
			if (sb == null) {
				// check only with src.
				for (int i = 1; i < src.Length; i++) {
					int level = GetCombiningClass (src [i]);
					if (level == 0)
						continue;
					if (GetCombiningClass (src [i - 1]) > level) {
						sb = new StringBuilder (src.Length);
						sb.Append (src, 0, i - 1);
						ReorderCanonical (src, ref sb, i);
						return;
					}
				}
				return;
			}
			// check only with sb
			for (int i = start; i < sb.Length; i++) {
				int level = GetCombiningClass (sb [i]);
				if (level == 0)
					continue;
				if (GetCombiningClass (sb [i - 1]) > level) {
					char c = sb [i - 1];
					sb [i - 1] = sb [i];
					sb [i] = c;
					i--; // apply recursively
				}
			}
		}

		static void DecomposeChar (ref StringBuilder sb,
			ref int [] buf, string s, int i, ref int start)
		{
			if (sb == null)
				sb = new StringBuilder (s.Length + 100);
			sb.Append (s, start, i - start);
			if (buf == null)
				buf = new int [5];
			GetCanonical (s [i], buf, 0);
			for (int x = 0; ; x++) {
				if (buf [x] == 0)
					break;
				if (buf [x] < char.MaxValue)
					sb.Append ((char) buf [x]);
				else { // surrogate
					sb.Append ((char) (buf [x] >> 10 + 0xD800));
					sb.Append ((char) ((buf [x] & 0x0FFF) + 0xDC00));
				}
			}
			start = i + 1;
		}

		public static NormalizationCheck QuickCheck (char c, int type)
		{
			uint v;
			switch (type) {
			default: // NFC
				v = PropValue ((int) c);
				return (v & NoNfc) == 0 ?
					(v & MaybeNfc) == 0 ?
					NormalizationCheck.Yes :
					NormalizationCheck.Maybe :
					NormalizationCheck.No;
			case 1: // NFD
				if ('\uAC00' <= c && c <= '\uD7A3')
					return NormalizationCheck.No;
				return (PropValue ((int) c) & NoNfd) != 0 ?
					NormalizationCheck.No : NormalizationCheck.Yes;
			case 2: // NFKC
				v = PropValue ((int) c);
				return (v & NoNfkc) != 0 ? NormalizationCheck.No :
					(v & MaybeNfkc) != 0 ?
					NormalizationCheck.Maybe :
					NormalizationCheck.Yes;
			case 3: // NFKD
				if ('\uAC00' <= c && c <= '\uD7A3')
					return NormalizationCheck.No;
				return (PropValue ((int) c) & NoNfkd) != 0 ?
					NormalizationCheck.No : NormalizationCheck.Yes;
			}
		}

		/* for now we don't use FC_NFKC closure
		public static bool IsMultiForm (char c)
		{
			return (PropValue ((int) c) & 0xF0000000) != 0;
		}

		public static char SingleForm (char c)
		{
			uint v = PropValue ((int) c);
			int idx = (int) ((v & 0x7FFF0000) >> 16);
			return (char) singleNorm [idx];
		}

		public static void MultiForm (char c, char [] buf, int index)
		{
			// FIXME: handle surrogate
			uint v = PropValue ((int) c);
			int midx = (int) ((v & 0x7FFF0000) >> 16);
			buf [index] = (char) multiNorm [midx];
			buf [index + 1] = (char) multiNorm [midx + 1];
			buf [index + 2] = (char) multiNorm [midx + 2];
			buf [index + 3] = (char) multiNorm [midx + 3];
			if (buf [index + 3] != 0)
				buf [index + 4] = (char) 0; // zero termination
		}
		*/

		public static void GetCanonical (int c, int [] buf, int bufIdx)
		{
			for (int i = CharMapIdx (c); mappedChars [i] != 0; i++)
				buf [bufIdx++] = mappedChars [i];
			buf [bufIdx] = (char) 0;
		}

		public static bool IsNormalized (string source, int type)
		{
			int prevCC = -1;
			for (int i = 0; i < source.Length; i++) {
				int cc = GetCombiningClass (source [i]);
				if (cc != 0 && cc < prevCC)
					return false;
				prevCC = cc;
				switch (QuickCheck (source [i], type)) {
				case NormalizationCheck.Yes:
					break;
				case NormalizationCheck.No:
					return false;
				case NormalizationCheck.Maybe:
					int ch = 0;
					if (GetPrimaryComposite (source,
						source [i], source.Length,
						i, ref ch) != 0)
						return false;
					break;
				}
			}
			return true;
		}

		public static string Normalize (string source, int type)
		{
			switch (type) {
			default:
			case 2:
				return Compose (source, type);
			case 1:
				return Decompose (source, type);
			}
		}

		static byte* props;
		static int* mappedChars;
		static short* charMapIndex;
		static short* helperIndex;
		static ushort* mapIdxToComposite;
		static byte* combiningClass;

#if GENERATE_TABLE

		public static readonly bool IsReady = true; // always

		static Normalization ()
		{
			fixed (byte* tmp = propsArr) {
				props = tmp;
			}
			fixed (int* tmp = mappedCharsArr) {
				mappedChars = tmp;
			}
			fixed (short* tmp = charMapIndexArr) {
				charMapIndex = tmp;
			}
			fixed (short* tmp = helperIndexArr) {
				helperIndex = tmp;
			}
			fixed (ushort* tmp = mapIdxToCompositeArr) {
				mapIdxToComposite = tmp;
			}
			fixed (byte* tmp = combiningClassArr) {
				combiningClass = tmp;
			}
		}
#else

		static object forLock = new object ();
		public static readonly bool isReady;

		public static bool IsReady {
			get { return isReady; }
		}

		[MethodImpl (MethodImplOptions.InternalCall)]
		static extern void load_normalization_resource (
			byte** props, byte** mappedChars, byte** charMapIndex,
			byte** helperIndex, byte** mapIdxToComposite,
			byte** combiningClass);

		static Normalization ()
		{
			lock (forLock) {
			fixed (byte** addrProps = &props) {
			fixed (int** addrMappedChars = &mappedChars) {
			fixed (short** addrCharMapIndex = &charMapIndex) {
			fixed (short** addrHelperIndex = &helperIndex) {
			fixed (ushort** addrMapIdxToComposite = &mapIdxToComposite) {
			fixed (byte** addrCombiningClass = &combiningClass) {
				load_normalization_resource (addrProps,
				(byte**) addrMappedChars,
				(byte**) addrCharMapIndex,
				(byte**) addrHelperIndex,
				(byte**) addrMapIdxToComposite,
				(byte**) addrCombiningClass);
			} } } } } }
			}

			isReady = true;
		}
	}
}
#endif

		//
		// autogenerated code or icall to fill array runs here
		//

