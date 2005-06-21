using System;
using System.IO;
using System.Globalization;

namespace Mono.Globalization.Unicode
{
	// Internal sort key storage that is reused during GetSortKey.
	internal class SortKeyBuffer
	{
		// l4s = small kana sensitivity, l4t = mark type,
		// l4k = katakana flag, l4w = kana width sensitivity
		int l1, l2, l3, l4s, l4t, l4k, l4w, l5;
		byte [] l1b, l2b, l3b, l4sb, l4tb, l4kb, l4wb, l5b;
		int level5LastPos;

		string source;
		bool processLevel2;
		bool frenchSort;
		bool frenchSorted;
		int lcid;
		CompareOptions options;

		public SortKeyBuffer (int lcid)
		{
		}

		public void Reset ()
		{
			l1 = l2 = l3 = l4s = l4t = l4k = l4w = l5 = 0;
			level5LastPos = 0;
		}

		// It is used for CultureInfo.ClearCachedData().
		internal void ClearBuffer ()
		{
			l1b = l2b = l3b = l4sb = l4tb = l4kb = l4wb = l5b = null;
		}

		internal void Initialize (CompareOptions options, string s, bool frenchSort)
		{
			this.options = options;
			int len = s.Length;
			processLevel2 = (options & CompareOptions.IgnoreNonSpace) == 0;
			this.frenchSort = frenchSort;

			// For Korean text it is likely to be much bigger (for
			// Jamo), but even in ko-KR most of the compared
			// strings won't be Hangul.
			if (l1b == null || l1b.Length < len)
				l1b = new byte [len * 2 + 10];

			if (processLevel2 && (l2b == null || l2b.Length < len))
				l2b = new byte [len + 10];
			if (l3b == null || l3b.Length < len)
				l3b = new byte [len + 10];

			// This weight is used only in Japanese text.
			// We could expand the initial length as well as
			// primary length (actually x3), but even in ja-JP
			// most of the compared strings won't be Japanese.
			if (l4sb == null)
				l4sb = new byte [10];
			if (l4tb == null)
				l4tb = new byte [10];
			if (l4kb == null)
				l4kb = new byte [10];
			if (l4wb == null)
				l4wb = new byte [10];

			if (l5b == null)
				l5b = new byte [10];
		}

		internal void AppendCJKExtension (byte lv1msb, byte lv1lsb)
		{
			AppendBufferPrimitive (0xFE, ref l1b, ref l1);
			AppendBufferPrimitive (0xFF, ref l1b, ref l1);
			AppendBufferPrimitive (lv1msb, ref l1b, ref l1);
			AppendBufferPrimitive (lv1lsb, ref l1b, ref l1);
			if (processLevel2)
				AppendBufferPrimitive (2, ref l2b, ref l2);
			AppendBufferPrimitive (2, ref l3b, ref l3);
		}

		// LAMESPEC: Windows handles some of Hangul Jamo as to have
		// more than two primary weight values. However this causes
		// incorrect zero-termination. So I just ignore them and
		// treat it as usual character.
		/*
		internal void AppendJamo (byte category, byte lv1msb, byte lv1lsb)
		{
			AppendNormal (category, lv1msb, 0, 0);
			AppendBufferPrimitive (0xFF, ref l1b, ref l1);
			AppendBufferPrimitive (lv1lsb, ref l1b, ref l1);
			AppendBufferPrimitive (0xFF, ref l1b, ref l1);
			// FIXME: those values looks extraneous but might be
			// some advanced use. Worthy of digging into it.
			AppendBufferPrimitive (0, ref l1b, ref l1);
			AppendBufferPrimitive (0xFF, ref l1b, ref l1);
			AppendBufferPrimitive (0, ref l1b, ref l1);
		}
		*/

		// Append sort key value from table normally.
		internal void AppendKana (byte category, byte lv1, byte lv2, byte lv3, bool isSmallKana, byte markType, bool isKatakana, bool isHalfWidth)
		{
			AppendNormal (category, lv1, lv2, lv3);

			AppendBufferPrimitive ((byte) (isSmallKana ? 0xC4 : 0xE4), ref l4sb, ref l4s);
			AppendBufferPrimitive (markType, ref l4tb, ref l4t);
			AppendBufferPrimitive ((byte) (isKatakana ? 0xC4 : 0xE4), ref l4kb, ref l4k);
			AppendBufferPrimitive ((byte) (isHalfWidth ? 0xC4 : 0xE4), ref l4wb, ref l4w);
		}

		// Append sort key value from table normally.
		internal void AppendNormal (byte category, byte lv1, byte lv2, byte lv3)
		{
			if (lv2 == 0)
				lv2 = 2;
			if (lv3 == 0)
				lv3 = 2;

			// non-primary diacritical weight is added to that of
			// the previous character (and does not reset level 3
			// weight).
			if (processLevel2 &&category == 1 && l1 > 0) {
				lv2 = (byte) (lv2 + l2b [--l2]);
				lv3 = l3b [--l3];
			}

			if (category != 1) {
				AppendBufferPrimitive (category, ref l1b, ref l1);
				AppendBufferPrimitive (lv1, ref l1b, ref l1);
			}
			if (processLevel2)
				AppendBufferPrimitive (lv2, ref l2b, ref l2);
			AppendBufferPrimitive (lv3, ref l3b, ref l3);
		}

		// Append variable-weight character.
		internal void AppendLevel5 (byte [] table, int idx, int currentIndex)
		{
			// offset
			int offsetValue = currentIndex - level5LastPos;
			for (; offsetValue > 8064; offsetValue -= 8064)
				AppendBufferPrimitive (0xFF, ref l5b, ref l5);
			if (offsetValue > 63)
				AppendBufferPrimitive ((byte) (offsetValue - 63 / 4 + 0x80), ref l5b, ref l5);
			AppendBufferPrimitive ((byte) (offsetValue % 63), ref l5b, ref l5);

			level5LastPos = currentIndex;

			// sortkey value
			idx++; // skip the "variable" mark: 01
			while (table [idx] != 0)
				AppendBufferPrimitive (table [idx++], ref l5b, ref l5);
		}

		private void AppendBufferPrimitive (byte value, ref byte [] buf, ref int bidx)
		{
			buf [bidx++] = value;
			if (bidx == buf.Length) {
				byte [] tmp = new byte [bidx * 2];
				Array.Copy (buf, tmp, buf.Length);
				buf = tmp;
			}
		}

		public SortKey GetResultAndReset ()
		{
			SortKey ret = GetResult ();
			Reset ();
			return ret;
		}

		// For level2-5, 02 is the default and could be cut (implied).
		// 02 02 02 -> 0
		// 02 03 02 -> 2
		// 03 04 05 -> 3
		private int GetOptimizedLength (byte [] data, int len, byte defaultValue)
		{
			int cur = -1;
			for (int i = 0; i < len; i++)
				if (data [i] != defaultValue)
					cur = i;
			return cur + 1;
		}

		public SortKey GetResult ()
		{
			if (frenchSort && !frenchSorted) {
				Array.Reverse (l2b);
				frenchSorted = true;
			}

			l2 = GetOptimizedLength (l2b, l2, 2);
			l3 = GetOptimizedLength (l3b, l3, 2);
			bool hasJapaneseWeight = (l4s > 0);
			l4s = GetOptimizedLength (l4sb, l4s, 0xE4);
			l4t = GetOptimizedLength (l4tb, l4t, 3);
			l4k = GetOptimizedLength (l4kb, l4k, 0xE4);
			l4w = GetOptimizedLength (l4wb, l4w, 0xE4);
			l5 = GetOptimizedLength (l5b, l5, 2);

			int length = l1 + l2 + l3 + l5 + 5;
			int jpLength = l4s + l4t + l4k + l4w;
			if (hasJapaneseWeight)
				length += jpLength + 4;

			byte [] ret = new byte [length];
			Array.Copy (l1b, ret, l1);
			ret [l1] = 1; // end-of-level mark
			int cur = l1 + 1;
			if (l2 > 0)
				Array.Copy (l2b, 0, ret, cur, l2);
			cur += l2;
			ret [cur++] = 1; // end-of-level mark
			if (l3 > 0)
				Array.Copy (l3b, 0, ret, cur, l3);
			cur += l3;
			ret [cur++] = 1; // end-of-level mark
			if (hasJapaneseWeight) {
				Array.Copy (l4sb, 0, ret, cur, l4s);
				cur += l4s;
				ret [cur++] = 0xFF; // end-of-jp-subsection
				Array.Copy (l4tb, 0, ret, cur, l4t);
				cur += l4t;
				ret [cur++] = 2; // end-of-jp-middle-subsection
				Array.Copy (l4kb, 0, ret, cur, l4k);
				cur += l4k;
				ret [cur++] = 0xFF; // end-of-jp-subsection
				Array.Copy (l4wb, 0, ret, cur, l4w);
				cur += l4w;
				ret [cur++] = 0xFF; // end-of-jp-subsection
			}
			ret [cur++] = 1; // end-of-level mark
			if (l5 > 0)
				Array.Copy (l5b, 0, ret, cur, l5);
			cur += l5;
			ret [cur++] = 0; // end-of-data mark
			return new SortKey (lcid, source, ret, options, l1, l2, l3, l4s, l4t, l4k, l4w, l5);
		}
	}
}
