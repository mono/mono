using System;
using System.Globalization;
using System.Text;
using System.Runtime.CompilerServices;

using NUtil = Mono.Globalization.Unicode.NormalizationTableUtil;

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
		public const int IsUnsafe = 128;
//		public const int ExpandOnNfd = 256;
//		public const int ExpandOnNfc = 512;
//		public const int ExpandOnNfkd = 1024;
//		public const int ExpandOnNfkc = 2048;

		static uint PropValue (int cp)
		{
			return props [NUtil.PropIdx (cp)];
		}

		static int CharMapIdx (int cp)
		{
			return charMapIndex [NUtil.MapIdx (cp)];
		}

		static byte GetCombiningClass (int c)
		{
			return combiningClass [NUtil.Combining.ToIndex (c)];
		}

		static int GetPrimaryCompositeFromMapIndex (int src)
		{
			return mapIdxToComposite [NUtil.Composite.ToIndex (src)];
		}

		static int GetPrimaryCompositeHelperIndex (int cp)
		{
			return helperIndex [NUtil.Helper.ToIndex (cp)];
		}

		private static string Compose (string source, int checkType)
		{
			StringBuilder sb = null;
			// Decompose to NFD or NKFD depending on our target
			Decompose (source, ref sb, checkType == 2 ? 3 : 1);
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
				StringBuilder sb = new StringBuilder (source.Length + source.Length / 10);
				sb.Append (source);
				Combine (sb, i, checkType);
				return sb;
			}
			return null;
		}

/*
		private static bool CanBePrimaryComposite (int i)
		{
			if (i >= 0x3400 && i <= 0x9FBB)
				return GetPrimaryCompositeHelperIndex (i) != 0;
			return (PropValue (i) & IsUnsafe) != 0;
		}
*/
		private static void Combine (StringBuilder sb, int i, int checkType)
		{
			// Back off one character as we may be looking at a V or T jamo.
			CombineHangul (sb, null, i > 0 ? i - 1 : i);

			while (i < sb.Length) {
				if (QuickCheck (sb [i], checkType) == NormalizationCheck.Yes) {
					i++;
					continue;
				}

				i = TryComposeWithPreviousStarter (sb, null, i);
			}
		}

		private static int CombineHangul (StringBuilder sb, string s, int current)
		{
			int length = sb != null ? sb.Length : s.Length;
			int last = Fetch (sb, s, current);

			for (int i = current + 1; i < length; ++i) {
				int ch = Fetch (sb, s, i);

				// 1. check to see if two current characters are L and V

				int LIndex = last - HangulLBase;
				if (0 <= LIndex && LIndex < HangulLCount) {
					int VIndex = ch - HangulVBase;
					if (0 <= VIndex && VIndex < HangulVCount) {
						if (sb == null)
							return -1;

						// make syllable of form LV

						last = HangulSBase + (LIndex * HangulVCount + VIndex) * HangulTCount;

						sb [i - 1] = (char) last; // reset last
						sb.Remove (i, 1);
						i--; length--;
						continue; // discard ch
					}
				}


				// 2. check to see if two current characters are LV and T

				int SIndex = last - HangulSBase;
				if (0 <= SIndex && SIndex < HangulSCount && (SIndex % HangulTCount) == 0) {
					int TIndex = ch - HangulTBase;
					if (0 < TIndex && TIndex < HangulTCount) {
						if (sb == null)
							return -1;

						// make syllable of form LVT

						last += TIndex;

						sb [i - 1] = (char) last; // reset last
						sb.Remove (i, 1);
						i--; length--;
						continue; // discard ch
					}
				}
				// if neither case was true, just add the character
				last = ch;
			}

			return length;
		}

		static int Fetch (StringBuilder sb, string s, int i)
		{
			return (int) (sb != null ? sb [i] : s [i]);
		}

		// Cf. figure 7, section 1.3 of http://unicode.org/reports/tr15/.
		static int TryComposeWithPreviousStarter (StringBuilder sb, string s, int current)
		{
			// Backtrack to previous starter.
			int i = current - 1;
			if (GetCombiningClass (Fetch (sb, s, current)) == 0) {
				if (i < 0 || GetCombiningClass (Fetch (sb, s, i)) != 0)
					return current + 1;
			} else {
				while (i >= 0 && GetCombiningClass (Fetch (sb, s, i)) != 0)
					i--;
				if (i < 0)
					return current + 1;
			}

			int starter = Fetch (sb, s, i);

			// The various decompositions involving starter follow this index.
			int comp_idx = GetPrimaryCompositeHelperIndex (starter);
			if (comp_idx == 0)
				return current + 1;

			int length = (sb != null ? sb.Length : s.Length);
			int prevCombiningClass = -1;
			for (int j = i + 1; j < length; j++) {
				int candidate = Fetch (sb, s, j);

				int combiningClass = GetCombiningClass (candidate);
				if (combiningClass == prevCombiningClass)
					// We skipped over a guy with the same class, without
					// combining.  Skip this one, too.
					continue;

				int composed = TryCompose (comp_idx, starter, candidate);
				if (composed != 0) {
					if (sb == null)
						// Not normalized, and we are only checking.
						return -1;

					// Full Unicode warning: This will break when the underlying
					// tables are extended.
					sb [i] = (char) composed;
					sb.Remove (j, 1);

					return current;
				}

				// Gray box.  We're done.
				if (combiningClass == 0)
					return j + 1;

				prevCombiningClass = combiningClass;
			}

			return length;
		}

		static int TryCompose (int i, int starter, int candidate)
		{
			while (mappedChars [i] == starter) {
				if (mappedChars [i + 1] == candidate &&
				    mappedChars [i + 2] == 0) {
					int composed = GetPrimaryCompositeFromMapIndex (i);

					if ((PropValue (composed) & FullCompositionExclusion) == 0)
						return composed;
				}

				// Skip this entry.
				while (mappedChars [i] != 0)
					i++;
				i++;
			}

			return 0;
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
						i, checkType, ref start);
			if (sb != null)
				sb.Append (source, start, source.Length - start);
			ReorderCanonical (source, ref sb, 1);
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
						sb.Append (src, 0, src.Length);
						ReorderCanonical (src, ref sb, i);
						return;
					}
				}
				return;
			}
			// check only with sb
			for (int i = start; i < sb.Length; ) {
				int level = GetCombiningClass (sb [i]);
				if (level == 0 || GetCombiningClass (sb [i - 1]) <= level) {
					i++;
					continue;
				}

				char c = sb [i - 1];
				sb [i - 1] = sb [i];
				sb [i] = c;
				// Apply recursively.
				if (i > 1)
					i--;
			}
		}

		static void DecomposeChar (ref StringBuilder sb,
			ref int [] buf, string s, int i, int checkType, ref int start)
		{
			if (sb == null)
				sb = new StringBuilder (s.Length + 100);
			sb.Append (s, start, i - start);
			if (buf == null)
				buf = new int [19];
			int n = GetCanonical (s [i], buf, 0, checkType);
			for (int x = 0; x < n; x++) {
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

		const int HangulSBase = 0xAC00, HangulLBase = 0x1100,
				  HangulVBase = 0x1161, HangulTBase = 0x11A7,
				  HangulLCount = 19, HangulVCount = 21, HangulTCount = 28,
				  HangulNCount = HangulVCount * HangulTCount,   // 588
				  HangulSCount = HangulLCount * HangulNCount;   // 11172

		private static int GetCanonicalHangul (int s, int [] buf, int bufIdx)
		{
			int idx = s - HangulSBase;
			if (idx < 0 || idx >= HangulSCount) {
				return bufIdx;
			}

			int L = HangulLBase + idx / HangulNCount;
			int V = HangulVBase + (idx % HangulNCount) / HangulTCount;
			int T = HangulTBase + idx % HangulTCount;

			buf [bufIdx++] = L;
			buf [bufIdx++] = V;
			if (T != HangulTBase) {
				buf [bufIdx++] = T;
			}
			buf [bufIdx] = (char) 0;
			return bufIdx;
		}

		static int GetCanonical (int c, int [] buf, int bufIdx, int checkType)
		{
			int newBufIdx = GetCanonicalHangul (c, buf, bufIdx);
			if (newBufIdx > bufIdx)
				return newBufIdx;
 
			int i = CharMapIdx (c);
			if (i == 0 || mappedChars [i] == c)
				buf [bufIdx++] = c;
			else {
				// Character c maps to one or more decomposed chars.
				for (; mappedChars [i] != 0; i++) {
					int nth = mappedChars [i];

					// http://www.unicode.org/reports/tr15/tr15-31.html, 1.3:
					// Full decomposition involves recursive application of the
					// Decomposition_Mapping values.  Note that QuickCheck does
					// not currently support astral plane codepoints.
					if (nth <= 0xffff && QuickCheck ((char)nth, checkType) == NormalizationCheck.Yes)
						buf [bufIdx++] = nth;
					else
						bufIdx = GetCanonical (nth, buf, bufIdx, checkType);
				}
			}

			return bufIdx;
		}

		public static bool IsNormalized (string source, int type)
		{
			int prevCC = -1;
			for (int i = 0; i < source.Length; ) {
				int cc = GetCombiningClass (source [i]);
				if (cc != 0 && cc < prevCC)
					return false;
				prevCC = cc;

				switch (QuickCheck (source [i], type)) {
				case NormalizationCheck.Yes:
					i++;
					break;
				case NormalizationCheck.No:
					return false;
				case NormalizationCheck.Maybe:
					// for those forms with composition, it cannot be checked here
					switch (type) {
					case 0: // NFC
					case 2: // NFKC
						return source == Normalize (source, type);
					}
					// go on...

					i = CombineHangul (null, source, i > 0 ? i - 1 : i);
					if (i < 0)
						return false;

					i = TryComposeWithPreviousStarter (null, source, i);
					if (i < 0)
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
			case 3:
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
			out IntPtr props, out IntPtr mappedChars,
			out IntPtr charMapIndex, out IntPtr helperIndex,
			out IntPtr mapIdxToComposite, out IntPtr combiningClass);

		static Normalization ()
		{
			IntPtr p1, p2, p3, p4, p5, p6;
			lock (forLock) {
				load_normalization_resource (out p1, out p2, out p3, out p4, out p5, out p6);
				props = (byte*) p1;
				mappedChars = (int*) p2;
				charMapIndex = (short*) p3;
				helperIndex = (short*) p4;
				mapIdxToComposite = (ushort*) p5;
				combiningClass = (byte*) p6;
			}

			isReady = true;
		}
	}
}
#endif

		//
		// autogenerated code or icall to fill array runs here
		//

