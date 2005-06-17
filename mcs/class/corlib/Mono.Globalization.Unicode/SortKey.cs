using System;
using System.IO;
using System.Globalization;

namespace Mono.Globalization.Unicode
{
	internal class SortKey
	{
		#region Static members
		public static int Compare (SortKey sk1, SortKey sk2)
		{
			if (Object.ReferenceEquals (sk1, sk2)
				|| Object.ReferenceEquals (sk1.OriginalString,
				sk2.OriginalString))
				return 0;

			byte [] d1 = sk1.KeyData;
			byte [] d2 = sk2.KeyData;

			int len = d1.Length > d2.Length ? d2.Length : d1.Length;
			for (int i = 0; i < len; i++)
				if (d1 [i] != d2 [i])
					return d1 [i] < d2 [i] ? -1 : 1;
			return d1.Length == d2.Length ? 0 : d1.Length < d2.Length ? -1 : 1;
		}
		#endregion

		readonly string source;
		readonly byte [] data;
		readonly int lv1Length;
		readonly int lv2Length;
		readonly int lv3Length;
		readonly int kanaSmallLength;
		readonly int markTypeLength;
		readonly int katakanaLength;
		readonly int kanaWidthLength;
		readonly int identLength;

		public SortKey (string source, byte [] buffer,
			int lv1Length, int lv2Length, int lv3Length,
			int kanaSmallLength, int markTypeLength,
			int katakanaLength, int kanaWidthLength,
			int identLength)
		{
			this.source = source;
			this.data = buffer;
			this.lv1Length = lv1Length;
			this.lv2Length = lv2Length;
			this.lv3Length = lv3Length;
			this.kanaSmallLength = kanaSmallLength;
			this.markTypeLength = markTypeLength;
			this.katakanaLength = katakanaLength;
			this.kanaWidthLength = kanaWidthLength;
			this.identLength = identLength;
		}

		public string OriginalString {
			get { return source; }
		}

		public byte [] KeyData {
			get { return data; }
		}

		internal int Level1Length {
			get { return lv1Length; }
		}

		internal int Level2Index {
			get { return lv1Length + 1; }
		}

		internal int Level2Length {
			get { return lv2Length; }
		}

		internal int Level3Index {
			get { return lv1Length + lv2Length + 2; }
		}

		internal int Level3Length {
			get { return lv3Length; }
		}

		internal int Level4Index {
			get { return lv1Length + lv2Length + lv3Length + 3; }
		}

		internal int MarkTypeLength {
			get { return markTypeLength; }
		}

		internal int KatakanaLength {
			get { return katakanaLength; }
		}

		internal int KanaWidthLength {
			get { return kanaWidthLength; }
		}

		internal int IdenticalIndex {
			get { return data.Length - identLength - 1; }
		}

		internal int IdenticalLength {
			get { return identLength; }
		}
	}
}
