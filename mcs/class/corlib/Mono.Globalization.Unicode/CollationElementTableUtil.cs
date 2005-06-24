using System;
using System.Globalization;
using System.Text;

namespace Mono.Globalization.Unicode
{
	internal struct SortKeyValue {
		public readonly bool Alt;
		public readonly ushort Primary;
		public readonly byte Secondary;
		public readonly byte Thirtiary;
		public readonly int Quarternary;

		public SortKeyValue (bool alt, ushort v1, byte v2, byte v3, int v4)
		{
			Alt = alt;
			Primary = v1;
			Secondary = v2;
			Thirtiary = v3;
			Quarternary = v4;
		}
	}

	internal /*static*/ class CollationElementTableUtil
	{
		public static readonly CodePointIndexer Indexer;

		static CollationElementTableUtil ()
		{
			int [] starts = new int [] {
				0, 0x4dc0, 0xa000, 0xf900, 0xfb00,
				0x1d000, 0x2f800, 0xe0000, 0x110000};
			int [] ends = new int [] {
				0x3410, 0x4e10, 0xa4d0, 0xfa70, 0x10840,
				0x1d800, 0x2fa20, 0xe0200, 0x110000};
			Indexer = new CodePointIndexer (starts, ends, 0, 0);
		}
	}
}
