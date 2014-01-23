using System;
using System.Globalization;
using System.Text;

namespace Mono.Globalization.Unicode
{
	internal class NormalizationTableUtil
	{
		public static readonly CodePointIndexer Prop;
		public static readonly CodePointIndexer Map;
		public static readonly CodePointIndexer Combining;
		public static readonly CodePointIndexer Composite;
		public static readonly CodePointIndexer Helper;

		static NormalizationTableUtil ()
		{
			int [] propStarts = new int [] {
				0, 0x0910, 0x1B00, 0x2460, 0x2980,
				0x2C70, 0x2D60, 0x2E90, 0xA770, 0xA7F0, 0xF900,
//				0x1D100, 0x2f800, 0x2fa10
				};
			int [] propEnds = new int [] {
				0x06E0, 0x1200, 0x2330, 0x2600, 0x2AE0,
				0x2C80, 0x2D70, 0x3400, 0xA780, 0xA800, 0x10000,
//				0x1D800, 0x2f810, 0x2fa20
				};
			int [] mapStarts = new int [] {
				0x90, 0x0920, 0x1D20, 0x2460, 0x24A0, 0x2A00,
				0x2D60, 0x2E90, 0xF900,
//				0x1d150, 0x2f800
				};
			int [] mapEnds = new int [] {
				0x06E0, 0x1100, 0x2330, 0x24A0, 0x24F0, 0x2AE0,
				0x2D70, 0x3400, 0x10000,
//				0x1d800, 0x2fb00
				};
			int [] combiningStarts = new int [] {
				0x02F0, 0x0480, 0x0590, 0x0930, 0x09B0,
				0x0A30, 0x0AB0, 0x0B30, 0x0BC0, 0x0C40,
				0x0CB0, 0x0D40, 0x0DC0, 0x0E30, 0x0EB0,
				0x0F00, 0x1030, 0x1350, 0x1710, 0x17D0,
				0x18A0, 0x1930, 0x1A10, 0x1DC0, 0x20D0,
				0x3020, 0x3090, 0xA800, 0xFB10, 0xFE20,
//				0x10A00, 0x1D160, 0x1D240
				};
			int [] combiningEnds = new int [] {
				0x0360, 0x0490, 0x0750, 0x0960, 0x09D0,
				0x0A50, 0x0AD0, 0x0B50, 0x0BD0, 0x0C60,
				0x0CD0, 0x0D50, 0x0DD0, 0x0E50, 0x0ED0,
				0x0FD0, 0x1040, 0x1360, 0x1740, 0x17E0,
				0x18B0, 0x1940, 0x1A20, 0x1DD0, 0x20F0,
				0x3030, 0x30A0, 0xA810, 0xFB20, 0xFE30,
//				0x10A40, 0x1D1B0, 0x1D250
				};
			// since mapToCompositeIndex only holds canonical
			// mappings, those indexes could be still shorten.
			int [] compositeStarts = new int [] {
				0x480, 0x1410, 0x1670
				};
			int [] compositeEnds = new int [] {
				0x1080, 0x1580, 0x21B0
				};
			int [] helperStarts = new int [] {
				0, 0x900, 0x1D00, 0x2500, 0x3000, 0x3B90,
				0x4010, 0x4E00, 0xFB40,
//				0x1D150, 0x20100, 0x20510,
//				0x20630, 0x20800, 0x20A20, 0x20B60, 0x214E0,
				};
			int [] helperEnds = new int [] {
				0x700, 0x1200, 0x2300, 0x2600, 0x3160, 0x3BA0, 
				0x4030, 0xA000, 0xFB50,
//				0x1D1C0, 0x20130, 0x20550,
//				0x20640, 0x208E0, 0x20A30, 0x20B70, 0x214F0,
				};

			Prop = new CodePointIndexer (propStarts, propEnds, 0, 0);
			Map = new CodePointIndexer (mapStarts, mapEnds, 0, 0);
			Combining = new CodePointIndexer (combiningStarts,
				combiningEnds, 0, 0);
			Composite = new CodePointIndexer (compositeStarts,
				compositeEnds, 0, 0);
			Helper = new CodePointIndexer (helperStarts, helperEnds,
				0, 0);
		}

		public static int PropIdx (int cp)
		{
			return Prop.ToIndex (cp);
		}

		public static int PropCP (int index)
		{
			return Prop.ToCodePoint (index);
		}

		public static int PropCount { get { return Prop.TotalCount; } }

		public static int MapIdx (int cp)
		{
			return Map.ToIndex (cp);
		}

		public static int MapCP (int index)
		{
			return Map.ToCodePoint (index);
		}

		public static int CbIdx (int cp)
		{
			return Combining.ToIndex (cp);
		}

		public static int CbCP (int index)
		{
			return Combining.ToCodePoint (index);
		}

		public static int MapCount { get { return Map.TotalCount; } }
	}
}
