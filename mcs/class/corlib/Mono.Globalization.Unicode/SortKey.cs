using System;
using System.IO;
using System.Globalization;

namespace System.Globalization
{
	public class SortKey
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
		readonly CompareOptions options;
		readonly byte [] key;
		readonly int lcid;

		// for legacy unmanaged one
		internal SortKey (int lcid, string source, CompareOptions opt)
		{
			this.lcid = lcid;
			this.source = source;
			this.options = opt;
		}

		internal SortKey (int lcid, string source, byte [] buffer, CompareOptions opt,
			int lv1Length, int lv2Length, int lv3Length,
			int kanaSmallLength, int markTypeLength,
			int katakanaLength, int kanaWidthLength,
			int identLength)
		{
			this.lcid = lcid;
			this.source = source;
			this.key = buffer;
			this.options = opt;
		}

		public string OriginalString {
			get { return source; }
		}

		public byte [] KeyData {
			get { return key; }
		}

		// copy from original SortKey.cs
		public override bool Equals (object value)
		{
			SortKey other = (value as SortKey);
			if(other!=null) {
				if((this.lcid==other.lcid) &&
				   (this.options==other.options) &&
				   (Compare (this, other)==0)) {
					return(true);
				}
			}

			return(false);
		}

		public override int GetHashCode ()
		{
			if (key.Length == 0)
				return 0; // should not happen though.
			int val = key [0];
			for (int i = 1; i < key.Length; i++)
				val ^= (int) key [i] << (i & 3);
			return (int) val;
		}

		// copy from original SortKey.cs
		public override string ToString()
		{
			return("SortKey - "+lcid+", "+options+", "+source);
		}
	}
}
