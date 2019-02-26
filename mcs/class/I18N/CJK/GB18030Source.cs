//
// GB18030Encoding.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;
#if BUILD_GENERATOR
using System.IO;
using System.Xml;
#endif

namespace I18N.CJK
{
	internal unsafe class GB18030Source
	{
		class GB18030Map
		{
			public readonly int UStart;
			public readonly int UEnd;
			public readonly long GStart;
			public readonly long GEnd;
			public readonly bool Dummy; // This range is actually not usable.

			public GB18030Map (
				int ustart, int uend, long gstart, long gend, bool dummy)
			{
				this.UStart = ustart;
				this.UEnd = uend;
				this.GStart = gstart;
				this.GEnd = gend;
				this.Dummy = dummy;
			}
		}

		private GB18030Source ()
		{
		}

		static readonly byte *gbx2uni;
		static readonly byte *uni2gbx;
		static readonly int gbx2uniSize, uni2gbxSize;

		static GB18030Source ()
		{
			MethodInfo mi = typeof (Assembly).GetMethod (
				"GetManifestResourceInternal",
				BindingFlags.NonPublic | BindingFlags.Instance);

			int size = 0;
			Module mod = null;
			IntPtr ret = IntPtr.Zero;

			if (mi != null)
			{
				ret = (IntPtr)mi.Invoke(
				 Assembly.GetExecutingAssembly(),
				 new object[] { "gb18030.table", size, mod });
			}
			else
			{
				// DotNet's way ;)
				using (var ms = Assembly.GetExecutingAssembly()
					.GetManifestResourceStream("gb18030.table"))
				{
					var len = (int)ms.Length;
					byte* buf = (byte*)Marshal.AllocHGlobal(sizeof(byte) * len);

					for (int i = 0; i < len; i++)
						buf[i] = (byte)ms.ReadByte();
					
					ret = (IntPtr)buf;
				}
			}

			if (ret != IntPtr.Zero) {
				gbx2uni = (byte*) ((void*) ret);
				gbx2uniSize =
					(gbx2uni [0] << 24) + (gbx2uni [1] << 16) +
					(gbx2uni [2] << 8) + (gbx2uni [3]);
				gbx2uni += 4;
				uni2gbx = gbx2uni + gbx2uniSize;
				uni2gbxSize =
					(uni2gbx [0] << 24) + (uni2gbx [1] << 16) +
					(uni2gbx [2] << 8) + (uni2gbx [3]);
				uni2gbx += 4;
			}
		}

		static readonly long gbxBase =
			FromGBXRaw (0x81, 0x30, 0x81, 0x30, false);
		static readonly long gbxSuppBase =
			FromGBXRaw (0x90, 0x30, 0x81, 0x30, false);

		// See http://icu.sourceforge.net/docs/papers/gb18030.html
		// and referenced XML mapping table.
		static readonly GB18030Map [] ranges = new GB18030Map [] {
			// rawmap: 0x0080-0x0451
			new GB18030Map (0x0452, 0x200F, FromGBXRaw (0x81, 0x30, 0xD3, 0x30, false), FromGBXRaw (0x81, 0x36, 0xA5, 0x31, false), false),
			// rawmap: 0x2010-0x2642
			new GB18030Map (0x2643, 0x2E80, FromGBXRaw (0x81, 0x37, 0xA8, 0x39, false), FromGBXRaw (0x81, 0x38, 0xFD, 0x38, false), false),
			// rawmap: 0x2E81-0x361A
			new GB18030Map (0x361B, 0x3917, FromGBXRaw (0x82, 0x30, 0xA6, 0x33, false), FromGBXRaw (0x82, 0x30, 0xF2, 0x37, false), false),
			// rawmap: 0x3918-0x3CE0
			new GB18030Map (0x3CE1, 0x4055, FromGBXRaw (0x82, 0x31, 0xD4, 0x38, false), FromGBXRaw (0x82, 0x32, 0xAF, 0x32, false), false),
			// rawmap: 0x4056-0x415F
			new GB18030Map (0x4160, 0x4336, FromGBXRaw (0x82, 0x32, 0xC9, 0x37, false), FromGBXRaw (0x82, 0x32, 0xF8, 0x37, false), false),
			// rawmap: 4337-0x44D6
			new GB18030Map (0x44D7, 0x464B, FromGBXRaw (0x82, 0x33, 0xA3, 0x39, false), FromGBXRaw (0x82, 0x33, 0xC9, 0x31, false), false),
			// rawmap: 0x464C-0x478D
			new GB18030Map (0x478E, 0x4946, FromGBXRaw (0x82, 0x33, 0xE8, 0x38, false), FromGBXRaw (0x82, 0x34, 0x96, 0x38, false), false),
			// rawmap: 0x4947-0x49B7
			new GB18030Map (0x49B8, 0x4C76, FromGBXRaw (0x82, 0x34, 0xA1, 0x31, false), FromGBXRaw (0x82, 0x34, 0xE7, 0x33, false), false),
			// rawmap: 0x4C77-0x4DFF

			// 4E00-9FA5 are all mapped in GB2312
			new GB18030Map (0x4E00, 0x9FA5, 0, 0, true),

			new GB18030Map (0x9FA6, 0xD7FF, FromGBXRaw (0x82, 0x35, 0x8F, 0x33, false), FromGBXRaw (0x83, 0x36, 0xC7, 0x38, false), false),

			// D800-DFFF are ignored (surrogate)
			// E000-E76B are all mapped in GB2312.
			new GB18030Map (0xD800, 0xE76B, 0, 0, true),

			// rawmap: 0xE76C-E884
			new GB18030Map (0xE865, 0xF92B, FromGBXRaw (0x83, 0x36, 0xD0, 0x30, false), FromGBXRaw (0x84, 0x30, 0x85, 0x34, false), false),
			// rawmap: 0xF92C-FA29
			new GB18030Map (0xFA2A, 0xFE2F, FromGBXRaw (0x84, 0x30, 0x9C, 0x38, false), FromGBXRaw (0x84, 0x31, 0x85, 0x37, false), false),
			// rawmap: 0xFE30-FFE5
			new GB18030Map (0xFFE6, 0xFFFF, FromGBXRaw (0x84, 0x31, 0xA2, 0x34, false), FromGBXRaw (0x84, 0x31, 0xA4, 0x39, false), false),
			};

		public static void Unlinear (byte [] bytes, int start, long gbx)
		{
			fixed (byte* bptr = bytes) {
				Unlinear (bptr + start, gbx);
			}
		}

		public static unsafe void Unlinear (byte* bytes, long gbx)
		{
			bytes [3] = (byte) (gbx % 10 + 0x30);
			gbx /= 10;
			bytes [2] = (byte) (gbx % 126 + 0x81);
			gbx /= 126;
			bytes [1] = (byte) (gbx % 10 + 0x30);
			gbx /= 10;
			bytes [0] = (byte) (gbx + 0x81);
		}

		// negative (invalid) or positive (valid)
		public static long FromGBX (byte [] bytes, int start)
		{
			byte b1 = bytes [start];
			byte b2 = bytes [start + 1];
			byte b3 = bytes [start + 2];
			byte b4 = bytes [start + 3];
			if (b1 < 0x81 || b1 == 0xFF)
				return -1;
			if (b2 < 0x30 || b2 > 0x39)
				return -2;
			if (b3 < 0x81 || b3 == 0xFF)
				return -3;
			if (b4 < 0x30 || b4 > 0x39)
				return -4;
			if (b1 >= 0x90)
				return FromGBXRaw (b1, b2, b3, b4, true);
			long linear = FromGBXRaw (b1, b2, b3, b4, false);

			long rawOffset = 0;
			long startIgnore = 0;

			for (int i = 0; i < ranges.Length; i++) {
				GB18030Map m = ranges [i];
				if (linear < m.GStart)
					return ToUcsRaw ((int) (linear
						- startIgnore + rawOffset));
				if (linear <= m.GEnd)
					return linear - gbxBase - m.GStart
						+ m.UStart;
				if (m.GStart != 0) {
					rawOffset += m.GStart - startIgnore;
					startIgnore = m.GEnd + 1;
				}
			}

			// All 4 bytes look valid but we didn't find any appropriate range.
			// So just return negative result for it.
			return -4;
		}

		public static long FromUCSSurrogate (int cp)
		{
			return cp + gbxSuppBase;
		}

		public static long FromUCS (int cp)
		{
			long rawOffset = 0;
			long startIgnore = 0x80;
			for (int i = 0; i < ranges.Length; i++) {
				GB18030Map m = ranges [i];
				if (cp < m.UStart)
					return ToGbxRaw ((int) (cp
						- startIgnore + rawOffset));
				if (cp <= m.UEnd)
					return cp - m.UStart + m.GStart;
				if (m.GStart != 0) {
					rawOffset += m.UStart - startIgnore;
					startIgnore = m.UEnd + 1;
				}
			}

			// Consider it as invalid character
			return -1;
		}

		static long FromGBXRaw (
			byte b1, byte b2, byte b3, byte b4, bool supp)
		{
			// 126 = 0xFE - 0x80
			return (((b1 - (supp ? 0x90 : 0x81)) * 10 +
				(b2 - 0x30)) * 126 +
				(b3 - 0x81)) * 10 +
				b4 - 0x30 + (supp ? 0x10000 : 0);
		}

		static int ToUcsRaw (int idx)
		{
			return gbx2uni [idx * 2] * 0x100 +
				gbx2uni [idx * 2 + 1];
		}

		static long ToGbxRaw (int idx)
		{
			if (idx < 0 || idx * 2 + 1 >= uni2gbxSize)
				return -1;
			return gbxBase + uni2gbx [idx * 2] * 0x100 + uni2gbx [idx * 2 + 1];
		}


#if BUILD_GENERATOR
		public static void Main ()
		{
			new GB18030Source ().Run ();
		}

		byte [] uni2gbxMap;
		byte [] gbx2uniMap;

		void Run ()
		{
			int ustart = 0x80;
			long gstart = 0;
			int ucount = 0;
			long gcount = 0;
			bool skip = false;
			for (int i = 0; i < ranges.Length; i++) {
				GB18030Map m = ranges [i];
				if (!skip) {
//Console.WriteLine ("---- adding {0:X04} umap. {1:X04} gmap, skip range between {2:X04} and {3:X04}", m.UStart - ustart, m.GStart != 0 ? m.GStart - gstart : 0, m.UStart, m.UEnd);
					ucount += m.UStart - ustart;
				}
				if (m.GStart != 0)
					gcount += m.GStart - gstart;
				skip = m.GStart == 0;
				ustart = m.UEnd + 1;
				if (m.GStart != 0)
					gstart = m.GEnd + 1;
			}

Console.Error.WriteLine ("Total UCS codepoints: {0} ({1:X04})", ucount, ucount);
Console.Error.WriteLine ("Total GBX codepoints: {0} ({1:X04})", gcount, gcount);

			uni2gbxMap = new byte [ucount * 2];
			gbx2uniMap = new byte [gcount * 2];

			XmlDocument doc = new XmlDocument ();
			doc.XmlResolver = null;
			doc.Load ("gb-18030-2000.xml");
			foreach (XmlElement e in doc.SelectNodes (
				"/characterMapping/assignments/a"))
				AddMap (e);

			using (FileStream fs = File.Create ("gb18030.table")) {
				byte [] size = new byte [4];
				for (int i = 0, len = gbx2uniMap.Length;
					i < 4; i++, len >>= 8)
					size [3 - i] = (byte) (len % 0x100);
				fs.Write (size, 0, 4);
				fs.Write (gbx2uniMap, 0, gbx2uniMap.Length);
				fs.Write (uni2gbxMap, 0, uni2gbxMap.Length);
			}
Console.WriteLine ("done.");
		}

		void AddMap (XmlElement e)
		{
			int u = int.Parse (e.GetAttribute ("u"),
				NumberStyles.HexNumber);
			byte [] b = new byte [4];
			int idx = 0;
			foreach (string s in e.GetAttribute ("b").Split (' '))
				b [idx++] =
					byte.Parse (s, NumberStyles.HexNumber);
			if (idx != 4)
				return;

			AddMap (u, b);
		}

		void AddMap (int u, byte [] b)
		{
			int gbx = (int) (FromGBXRaw (
				b [0], b [1], b [2], b [3], false) - gbxBase);
			if (u > 0x10000 || gbx > 0x10000)
				throw new Exception (String.Format (
					"should not happen: {0:X04} {1:X04}",
					u, gbx));

			int uidx = IndexForUcs (u);
//Console.WriteLine ("U: {0:x04} for {1:x04} [{2:x02} {3:x02}]", uidx, u, (byte) (gbx / 0x100), (byte) (gbx % 0x100));
			uni2gbxMap [uidx * 2] = (byte) (gbx / 0x100);
			uni2gbxMap [uidx * 2 + 1] = (byte) (gbx % 0x100);

			int gidx = IndexForGbx (gbx);
//Console.WriteLine ("G: {0:x04} for {1:x04} ({2:x02} {3:x02} {4:x02} {5:x02})", gidx, gbx, b [0], b [1], b [2], b [3]);
			gbx2uniMap [gidx * 2] = (byte) (u / 0x100);
			gbx2uniMap [gidx * 2 + 1] = (byte) (u % 0x100);
		}

		static int IndexForUcs (int ucs)
		{
			int start = 0x80;
			int count = 0;
			bool skip = false;
			for (int i = 0; i < ranges.Length; i++) {
				GB18030Map m = ranges [i];
				if (!skip) {
					if (ucs < m.UStart)
						return count + ucs - start;
					count += m.UStart - start;
				}
				skip = m.GStart == 0;
				start = m.UEnd + 1;
			}
			return -1;
		}

		static int IndexForGbx (int gbx)
		{
			long start = 0;
			long count = 0;
			for (int i = 0; i < ranges.Length; i++) {
				GB18030Map m = ranges [i];
				if (m.GStart == 0)
					continue;
				if (gbx < m.GStart)
					return (int) (count + gbx - start);
				count += m.GStart - start;
				start = m.GEnd + 1;
			}
			return -1;
		}

#endif


	}

}
