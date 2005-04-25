using System;
using System.Globalization;
using System.Text;

namespace Mono.Globalization.Unicode
{
	internal /*static*/ class NormalizationTableUtil
	{
		public static readonly CodePointIndexer prop;
		public static readonly CodePointIndexer map;

		static NormalizationTableUtil ()
		{
			int [] propStarts = new int [] {
				0, 0xAC00, 0xF900, 0x1D100,
				0x2f800, 0x2fa10
				};
			int [] propEnds = new int [] {
				0x3400, 0xD7AF, 0x10000, 0x1D800,
				0x2f810, 0x2fa20
				};
			int [] mapStarts = new int [] {
				0, 0xF900, 0x1d150, 0x2f800
				};
			int [] mapEnds = new int [] {
				0x3400, 0x10000, 0x1d800, 0x2fb00
				};

			prop = new CodePointIndexer (propStarts, propEnds);
			map = new CodePointIndexer (mapStarts, mapEnds);
		}

		public static int PropIdx (int cp)
		{
			return prop.GetIndexForCodePoint (cp);
		}

		public static int PropCP (int index)
		{
			return prop.GetCodePointForIndex (index);
		}

		public static int PropCount { get { return prop.TotalCount; } }

		public static int MapIdx (int cp)
		{
			return map.GetIndexForCodePoint (cp);
		}

		public static int MapCP (int index)
		{
			return map.GetCodePointForIndex (index);
		}

		public static int MapCount { get { return map.TotalCount; } }
	}
}
